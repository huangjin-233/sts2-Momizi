#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Commands;
using Momizi.character.power;
using Momizi.character.single;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 心神合一：X费/攻击/罕见 —— 对全体敌人造成 X×当前伤害值 点伤害；白狼对血量最低敌人攻击 X 次
[RegisterCard(typeof(MomiziCardPool))]
public class MindUnity : ModCardTemplate
{
    private const int energyCost = 0; // 基础费用写 0，通过 HasEnergyCostX 声明为 X 费
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllAllies; // 无需手动指定目标
    private const bool shouldShowInCardLibrary = true;
    

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/rare/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6m, ValueProp.Move)
    ];

    public MindUnity() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    // 声明为 X 费卡：自动消耗全部剩余能量，效果按消耗能量数 X 倍结算
    protected override bool HasEnergyCostX => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // X = 本次消耗的能量
        int x = ResolveEnergyXValue();

        // 对全体敌人造成 X × 伤害值 点伤害（分 X 次，每次 6/8 点，作用于全部存活敌人）
        for (int i = 0; i < x; i++)
        {
            foreach (var e in Owner.Creature.CombatState?.HittableEnemies ?? [])
            {
                if (!e.IsAlive)
                {
                    continue;
                }
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this,cardPlay)
                    .Targeting(e)
                    .Execute(choiceContext);
            }
        }

        // 白狼对血量最低敌人攻击 X 次
        foreach (var pet in Owner.Creature.Pets)
        {
            if (!pet.IsAlive || pet.GetPowerAmount<WolfVanguard>() <= 0)
            {
                continue;
            }
            for (int i = 0; i < x; i++)
            {
                Creature? target = Owner.Creature.CombatState?.HittableEnemies
                    .Where(c => c.IsAlive)
                    .OrderBy(c => c.CurrentHp)
                    .FirstOrDefault();
                if (!target.IsAlive)
                {
                    break;
                }
                await MinionAnimCmd.PlayBumpAttackAsync(pet, target);
                await CreatureCmd.Damage(choiceContext, target,
                    WolfSingle.amount + Owner.Creature.GetPowerAmount<StrengthPower>(), ValueProp.Move, pet);
                await PowerCmd.Remove<VigorPower>(pet);
                Coordination.firstattack = true;
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2); // 6 → 8
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MomiziKeyword.Coordinations)
    ];
}