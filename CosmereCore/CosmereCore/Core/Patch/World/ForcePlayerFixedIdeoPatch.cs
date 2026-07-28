using Concord;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class ForcePlayerFixedIdeoPatch : Page_ChooseIdeoPreset {
    [InjectField("classicIdeo")]
    private Ideo classicIdeo = null!;

    [Inject(At.Return, nameof(PostOpen))]
    private void AfterPostOpen() {
        Faction? player = Faction.OfPlayer;
        FactionDef? def = player?.def;
        if (def == null || !def.fixedIdeo) return;

        IdeoGenerationParms parms = new IdeoGenerationParms(
            def,
            false,
            null,
            null,
            name: def.ideoName,
            styles: def.styles,
            deities: def.deityPresets,
            hidden: def.hiddenIdeo,
            description: def.ideoDescription,
            forcedMemes: def.forcedMemes,
            classicExtra: false,
            forceNoWeaponPreference: false,
            forNewFluidIdeo: false,
            fixedIdeo: true,
            requiredPreceptsOnly: def.requiredPreceptsOnly
        );

        player!.ideos.ChooseOrGenerateIdeo(parms);
        Ideo? forcedIdeo = player.ideos.PrimaryIdeo;
        if (forcedIdeo == null) return;

        classicIdeo = forcedIdeo;
        Find.IdeoManager.RemoveUnusedStartingIdeos();
        Logger.Info(
            $"ForcePlayerFixedIdeoPatch: forced player ideo to '{forcedIdeo.name}' from fixedIdeo on {def.defName}"
        );
    }
}
