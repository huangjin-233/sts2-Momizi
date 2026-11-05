using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.power;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.card.uncommon;

// 迎击：1费/攻击/罕见 消耗3层警戒：造成10点伤害；目标意图攻击时改为16点。升级：14 / 20
[RegisterCard(typeof(MomiziCardPool))]
public class Riposte : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://images/cards/uncommon/{GetType().Name}.png"
    );
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(11, ValueProp.Move),
        ModCardVars.Damage("Amplified", 9)
    ];
    protected override bool IsPlayable => base.Owner.Creature.GetPowerAmount<Alert>() >= 3;
    protected override bool ShouldGlowGoldInternal => Owner.Creature.GetPowerAmount<Alert>() >= 3;
    public Riposte() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var alert = Owner.Creature.GetPowerAmount<Alert>();
        await PowerCmd.Apply<Alert>(choiceContext, Owner.Creature, -3, Owner.Creature, this);
        if (cardPlay.Target != null)
        {
            if (Owner.Creature.CombatState != null)
            {
                var monsterModel = Owner.Creature.CombatState.Enemies;
                // 复制快照再迭代：攻击可能击杀敌人并将其从 Enemies 移除，直接 foreach 会触发集合修改异常
                foreach (var monster in monsterModel.ToList())
                {
                    if (monster.Monster != null && monster.Monster.IntendsToAttack)
                    {
                        await DamageCmd.Attack(DynamicVars["Amplified"].BaseValue)
                            .FromCard(this,cardPlay)
                            .Targeting(monster)
                            .Execute(choiceContext);
                    }
                }
            }
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this,cardPlay)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
        DynamicVars["Amplified"].UpgradeValueBy(2);
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Alert>()
    ];
}