using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using Momizi.character.power;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.relics;

[RegisterRelic(typeof(MomiziRelicPool))]
[RegisterCharacterStarterRelic(typeof(Momizi))]
public class WoodenClogs : ModRelicTemplate
{
    private int _RemainingUses = 0;
    private bool _isActivating;
    // 稀有度
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool ShowCounter => true;

    // 遗物的数值。这里会替换本地化中的{Cards}。
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(10)];

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://Test/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://Test/images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://Test/images/relics/{GetType().Name}.png"
    );

    public static Dictionary<Player, int> dodge = new Dictionary<Player, int>();

    public static void SetValue(Player player, int value)
    {
        dodge[player] = value;
    }

    public static int GetValue(Player player)
    {
        return dodge.TryGetValue(player, out int value) ? value : 0;
    }

    public static void IncrementValue(Player player, int value)
    {
        dodge[player] = value;
    }
    public override int DisplayAmount
    {
        get
        {

            return RemainingUses;
        }
    }

    private bool IsActivating
    {
        get
        {
            return _isActivating;
        }
        set
        {
            AssertMutable();
            _isActivating = value;
            UpdateDisplay();
        }
    }
    

    [SavedProperty]
    public int RemainingUses
    {
        get
        {
            return _RemainingUses;
        }
        private set
        {
            AssertMutable();
            if (_RemainingUses != value)
            {
                _RemainingUses = value;
                UpdateDisplay();
            }
        }
    }

    private int Threshold => DynamicVars.Cards.IntValue;

    private void UpdateDisplay()
    {
        if (IsActivating)
        {
            Status = RelicStatus.Active;  // 激活时显示高亮
        }
        else
        {
            Status = ((RemainingUses == Threshold) ? RelicStatus.Active : RelicStatus.Normal);
        }
        InvokeDisplayAmountChanged();
    }

    public void NotifySkillPlayed()
    {if (RemainingUses == 0)
        {
            return;
        }
        RemainingUses++;
    }
    private async Task DoActivateVisuals()
    {
        IsActivating = true;
        Flash();
        await Cmd.Wait(1f);
        IsActivating = false;
    }
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Dodge.Active)
        {
            RemainingUses++;
            Dodge.Active=false;
        }
        if (RemainingUses == 0)
        {
        }
        else
        {
            Flash(); // 触发遗物图标闪烁
            SetValue(Owner, 25 + Owner.Creature.GetPowerAmount<JiZhong>() * 5 - Owner.Creature.GetPowerAmount<Inferior>() * 10);
            if (side == Owner.Creature.Side)
            {
                foreach (var creature in Owner.Creature.CombatState.HittableEnemies)
                {
                    if (creature.Monster.IntendsToAttack)
                    {
                        int dodge = Owner.RunState.Rng.CombatCardGeneration.NextInt(0, 100);
                        if (dodge <= WoodenClogs.dodge[Owner])
                        {
                            await PowerCmd.Apply<Dodge>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
                        }
                    }
                }
            }
        }
    }
    public override bool IsAllowed(IRunState runState)
    {
        return IsBeforeAct3TreasureChest(runState);
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
        {
            return false;
        }
        options.Add(new Fix(player));
        return true;
    }

    public void Fix()
    {
        RemainingUses = 0;
    }
}