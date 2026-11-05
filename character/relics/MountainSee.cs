#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Momizi.character.Mini;
using Momizi.character.power;
using Momizi.character.Scry;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.relics;
[RegisterRelic(typeof(MomiziRelicPool))]
[RegisterCharacterStarterRelic(typeof(Momizi))]
public class MountainSee: ModRelicTemplate,IModRightClickableRelic
{
    // 稀有度
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://images/relics/{GetType().Name}.png"
    );
    // 跨战斗持久化字段
    private int _baseMaxHp1 = 10;
    private int _baseMaxHp2 = 10;
    // 白狼基础力量（跨战斗持久化：天狗三档奖励叠加，召唤白狼时注入）
    private int _baseStrength1 = 0;
    private int _baseStrength2 = 0;
    [SavedProperty]
    public int baseMaxHp1 { get => _baseMaxHp1; private set { AssertMutable(); if (_baseMaxHp1 != value) { _baseMaxHp1 = value; DynamicVars.MaxHp.BaseValue = value; UpdateDisplay(); } } }
    [SavedProperty]
    public int baseMaxHp2 { get => _baseMaxHp2; private set { AssertMutable(); if (_baseMaxHp2 != value) { _baseMaxHp2 = value; DynamicVars["Max"].BaseValue = value; UpdateDisplay(); } } }
    [SavedProperty]
    public int baseStrength1 { get => _baseStrength1; private set { AssertMutable(); if (_baseStrength1 != value) { _baseStrength1 = value; DynamicVars["Power1"].BaseValue = value; UpdateDisplay(); } } }
    [SavedProperty]
    public int baseStrength2 { get => _baseStrength2; private set { AssertMutable(); if (_baseStrength2 != value) { _baseStrength2 = value; DynamicVars["Power2"].BaseValue = value; UpdateDisplay(); } } }
    private void UpdateDisplay() => InvokeDisplayAmountChanged();
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Scry",3 ),
        new MaxHpVar(baseMaxHp1),
        ModCardVars.Int("Max",baseMaxHp2),
        new DamageVar(4m,ValueProp.Move),
        ModCardVars.Int("Power1",baseStrength1),
        ModCardVars.Int("Power2",baseStrength2),
    ];
    public override async Task BeforeCombatStart()
    {
        DynamicVars.MaxHp.BaseValue = baseMaxHp1;
        DynamicVars["Max"].BaseValue = baseMaxHp2;
        DynamicVars["Power1"].BaseValue = baseStrength1;
        DynamicVars["Power2"].BaseValue = baseStrength2;
        await Task.CompletedTask;
    }
    // 战斗结束时，将当前 Dynamcivars 值回写持久字段（战斗外也保留改动）
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        baseMaxHp1 = DynamicVars.MaxHp.IntValue;
        baseMaxHp2 = DynamicVars["Max"].IntValue;
        baseStrength1 = DynamicVars["Power1"].IntValue;
        baseStrength2 = DynamicVars["Power2"].IntValue;
        await Task.CompletedTask;
    }
    // 持久化修改白狼基础力量（召唤时注入，不随战斗清零，跨战斗保留）
    public void PersistStrength1(int value)
    {
        baseStrength1 = value;
        DynamicVars["Power1"].BaseValue = value;
    }
    // 持久化修改白狼基础力量
    public void PersistStrength2(int value)
    {
        baseStrength2 = value;
        DynamicVars["Power2"].BaseValue = value;
    }
    // 持久化修改白狼生命上限，并即时同步场上存活白狼的最大生命（上限与当前血量等量增加，不回满）
    public async Task PersistMaxHp1(int value)
    {
        baseMaxHp1 = value;
        DynamicVars.MaxHp.BaseValue = value;
        // 同步到场上 1 号狼；生物已拆除时不改，避免 SetCurrentHp 空引用
        if (WolfOne.Current?.Creature is { } wolf1)
        {
            decimal delta = value - wolf1.MaxHp;
            await CreatureCmd.SetMaxHp(wolf1, value);
            // 当前血量只在存活时同步，避免对已拆除生物结算
            if (wolf1.IsAlive)
            {
                await CreatureCmd.SetCurrentHp(wolf1, wolf1.CurrentHp + delta);
            }
        }
    }
    // 持久化修改天狗伤害
    public async Task PersistMaxHp2(int value)
    {
        baseMaxHp2 = value;
        DynamicVars["Max"].BaseValue = value;
        if (WolfTwo.Current?.Creature is { } wolf2)
        {
            decimal delta = value - wolf2.MaxHp;
            await CreatureCmd.SetMaxHp(wolf2, value);
            // 当前血量只在存活时同步，避免对已拆除生物结算
            if (wolf2.IsAlive)
            {
                await CreatureCmd.SetCurrentHp(wolf2, wolf2.CurrentHp + delta);
            }
        }
    }
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            ScryCmd.ResetSeenCards(Owner);
            Flash(); // 触发遗物图标闪烁
            if (WolfOne.OwnedBy(Owner))
            {
                DynamicVars["Scry"].BaseValue = 4;
            }
            else
            {
                DynamicVars["Scry"].BaseValue = 3;
            }
            await ScryCmd.Execute(new BlockingPlayerChoiceContext(), Owner.Creature.Player, DynamicVars["Scry"].IntValue);
        }
    }
    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        return Owner.Creature.GetPowerAmount<Alert>() >= 5;
    }

    public bool CanExecuteRightClick(ModRightClickExecutionContext context)
    {
        return Owner.Creature.GetPowerAmount<Alert>() >= 5;
    }

    // 右键执行（多人下会在所有客户端同步执行）
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (Owner.Creature.GetPowerAmount<Alert>() < 5)
        {
            return;
        }
        Flash();
        if (WolfOne.OwnedBy(Owner))
        {
            DynamicVars["Scry"].BaseValue = 4;
        }
        else
        {
            DynamicVars["Scry"].BaseValue = 3;
        }
        await PowerCmd.Apply<Alert>(new BlockingPlayerChoiceContext(), Owner.Creature, -5, Owner.Creature, null);
        await ScryCmd.Lock(new BlockingPlayerChoiceContext(), Owner.Creature.Player, DynamicVars["Scry"].IntValue);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromKeyword(MomiziKeyword.Scry),
        HoverTipFactory.FromKeyword(MomiziKeyword.Lock),
    ];
}
