using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MinionLib.Minion;
using MinionLib.Powers;
using Momizi.character.power;

namespace Momizi.character.Mini;

public sealed class Wolf : MinionModel
{
    public static bool wolf =false;
    public override int MinInitialHp => 10;
    public override int MaxInitialHp => 14;
    protected override string VisualsPath =>
        "res://Example/MinionTest/scenes/creature_visuals/pettest_attackaka.tscn";

    public override async Task OnSummon(
        PlayerChoiceContext choiceContext,
        Player owner,
        MinionSummonOptions options)
    {
        var self = Creature;

        if (options.MaxHp is decimal maxHp)
            await CreatureCmd.SetMaxAndCurrentHp(self, maxHp);

        if (options.PrimaryStatAmount is decimal strength && strength > 0m)
            await PowerCmd.Apply<StrengthPower>(
                choiceContext, self, strength, owner.Creature, options.Source);

        // 护主：敌方攻击椛时，未格挡伤害 50% 概率改由白狼承受
        await PowerCmd.Apply<WolfVanguard>(
            choiceContext, self, 1, owner.Creature, options.Source);
    }

    // 白狼死亡：你获得 1 层自卑
    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature dead,
        bool wasDead,
        float damageSource)
    {
        if (dead.PetOwner is { } owner && owner.Creature != null)
        {
            await PowerCmd.Apply<Inferior>(choiceContext, owner.Creature, 1, owner.Creature, null);
        }
    }
}
