using Concord;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.IdealTracking;

[Patch]
public abstract class ArtCreationTrackingPatch : CompArt {
    [Inject(At.Return, nameof(JustCreatedBy))]
    private void AfterJustCreatedBy(Pawn pawn) {
        if (pawn == null) return;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        CompQuality compQuality = parent.TryGetComp<CompQuality>();
        QualityCategory quality = compQuality != null ? compQuality.Quality : QualityCategory.Normal;

        surgebinder.OnArtCreated(quality);
        surgebinder.NotifyCreativeOutput();
    }
}
