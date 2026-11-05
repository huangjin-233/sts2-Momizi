using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Momizi.character.relics;
using STS2RitsuLib.Scaffolding.Content;

namespace Momizi.character.rest;

public class InferiorRest:ModRestSiteOptionTemplate
{
    public override string OptionId => "InferiorRest";
    public override RestSiteOptionAssetProfile AssetProfile => new( "res://images/rest/InferiorRest.png");
    public override bool IsEnabled => Owner.GetRelic<Tengu>() is { } tengu && tengu.inferior;
    public InferiorRest(Player owner)
    : base(owner)
{
}
public override LocString Description
{
    get
    {
        if (!IsEnabled)
        {
            return new LocString("rest_site_ui", "OPTION_" + OptionId + ".descriptionX");
        }
        if (Owner.GetRelic<Tengu>() is { } tengu && tengu.Flag)
        {
            LocString locString = new LocString("rest_site_ui", "OPTION_" + OptionId + ".description");
            return locString;
        }
        return new LocString("rest_site_ui", "OPTION_" + OptionId + ".descriptionDisabled");
    }
}
public override async Task<bool> OnSelect()
{
    Owner.GetRelic<Tengu>()?.InferiorRest();
    return false;
}

public override Task DoLocalPostSelectVfx(CancellationToken ct = default(CancellationToken))
{
    NDebugAudioManager.Instance.Play("sts_sfx_shovel_v1.mp3", 1f, PitchVariance.Small);
    return Task.CompletedTask;
}

public override Task DoRemotePostSelectVfx()
{
    NDebugAudioManager.Instance?.Play("sts_sfx_shovel_v1.mp3", 0.5f, PitchVariance.Small);
    NRestSiteCharacter nRestSiteCharacter = NRestSiteRoom.Instance?.Characters.First(( c) => c.Player == Owner);
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