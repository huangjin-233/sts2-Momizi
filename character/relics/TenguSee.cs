#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
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
public class TenguSee: ModRelicTemplate,IModRightClickableRelic
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
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Scry",2 ),
    ];
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner)
        {
            Flash(); // 触发遗物图标闪烁
            if (Wolf.wolf)
            {
                DynamicVars["Scry"].BaseValue = 3;
            }
            else
            {
                DynamicVars["Scry"].BaseValue = 2;
            }
            await ScryCmd.Execute(new BlockingPlayerChoiceContext(), Owner.Creature.Player, DynamicVars["Scry"].IntValue);
        }
    }
    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        return Owner.Creature.GetPowerAmount<Alert>() >= 5;
    }

    // 右键执行（多人下会在所有客户端同步执行）
    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        Flash();
        if (Wolf.wolf)
        {
            DynamicVars["Scry"].BaseValue = 3;
        }
        else
        {
            DynamicVars["Scry"].BaseValue = 2;
        }
        await ScryCmd.Lock(new BlockingPlayerChoiceContext(), Owner.Creature.Player, DynamicVars["Scry"].IntValue);
        await PowerCmd.Apply<Alert>(new BlockingPlayerChoiceContext(), Owner.Creature, -5, Owner.Creature, null);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips  => [
        HoverTipFactory.FromKeyword(MomiziKeyword.Scry),
        HoverTipFactory.FromKeyword(MomiziKeyword.Lock),
    ];
}