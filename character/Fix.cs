using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Momizi.character.relics;

namespace Momizi.character;

public class Fix:RestSiteOption
{
public override string OptionId => "Fix";

public Fix(Player owner)
    : base(owner)
{
}

public override async Task<bool> OnSelect()
{
    base.Owner.GetRelic<WoodenClogs>()?.Fix();
    return true;
}

public override Task DoLocalPostSelectVfx(CancellationToken ct = default(CancellationToken))
{
    NDebugAudioManager.Instance.Play("sts_sfx_shovel_v1.mp3", 1f, PitchVariance.Small);
    return Task.CompletedTask;
}

public override Task DoRemotePostSelectVfx()
{
    NDebugAudioManager.Instance?.Play("sts_sfx_shovel_v1.mp3", 0.5f, PitchVariance.Small);
    NRestSiteCharacter nRestSiteCharacter = NRestSiteRoom.Instance?.Characters.First((NRestSiteCharacter c) => c.Player == base.Owner);
    nRestSiteCharacter?.Shake();
    NRelicFlashVfx nRelicFlashVfx = NRelicFlashVfx.Create(ModelDb.Relic<Shovel>());
    if (nRelicFlashVfx == null)
    {
        return Task.CompletedTask;
    }
    nRestSiteCharacter?.AddChildSafely(nRelicFlashVfx);
    nRelicFlashVfx.Position = Vector2.Zero;
    return Task.CompletedTask;
}
}