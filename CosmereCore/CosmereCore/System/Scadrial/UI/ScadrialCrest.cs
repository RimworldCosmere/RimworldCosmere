using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

// The marks for a complete Allomancer and a complete Feruchemist are single drawn
// symbols rather than the metal set stacked up, so they are loaded by name. Nothing
// else reaches these paths - MetallicArtsMetalDef only resolves icons whose file
// name matches a metal defName.
[StaticConstructorOnStartup]
public static class ScadrialMarks {
    public static readonly Texture2D? Mistborn =
        Tintable("UI/Icons/Genes/Investiture/Allomancy/Mistborn");

    public static readonly Texture2D? FullFeruchemist =
        Tintable("UI/Icons/Genes/Investiture/Feruchemy/FullFeruchemist");

    // The source art is black on transparent, and GUI.color multiplies - black times
    // any tint is still black. Inverting to white first is what makes the mark take a
    // colour at all, which is why MetallicArtsMetalDef keeps an inverted copy of every
    // metal icon.
    private static Texture2D? Tintable(string path) {
        return ContentFinder<Texture2D>.Get(path, false)?.CloneTexture().InvertColors();
    }
}

// What the pawn is, above the table of what they can do. Both Metallic Arts want
// the same shape with different words in it, so the wording and the caching live
// here rather than twice over in the two sections.
public sealed class ScadrialCrest {
    public const float Gap = 6f;
    public const float RuleGap = 7f;

    // Identity moves rarely - a pawn does not become Mistborn twice a second - so it
    // is held for a second at a time. The activity line is not cached: a readout of
    // what is happening now has to be now.
    private const int RefreshInterval = 60;

    private static readonly Color TitleColor = new Color(0.867f, 0.831f, 0.757f);
    private static readonly Color IdleColor = new Color(0.545f, 0.514f, 0.451f);
    private static readonly Color LiveColor = new Color(0.851f, 0.643f, 0.255f);
    private static readonly Color WarnColor = new Color(0.753f, 0.478f, 0.416f);

    private readonly bool feruchemy;
    private readonly List<Texture2D?> marks = [];

    private int cachedPawnId = -1;
    private int cachedBucket = -1;
    private int cachedCellCount = -1;

    private Texture2D? glyph;
    private string title = string.Empty;
    private string rank = string.Empty;

    private string subtitle = string.Empty;
    private Color subtitleColor;

    public ScadrialCrest(bool feruchemy) {
        this.feruchemy = feruchemy;
    }

    public float Height =>
        Crest.HeightFor(true, marks.Count > 0, GameFont.Medium) + RuleGap + Gap;

    public void Refresh(Pawn pawn, InvestitureSnapshot snapshot) {
        RefreshIdentity(pawn, snapshot);
        RefreshActivity(pawn, snapshot);
    }

    public void Draw(Rect rect, ISystemSkin skin) {
        Crest.Draw(
            rect,
            glyph,
            title,
            subtitle,
            rank,
            new CrestPalette(TitleColor, subtitleColor, skin.AccentColor),
            GameFont.Medium,
            marks.Count > 0 ? marks : null
        );

        // A rule under the crest, because who the pawn is and what they can burn are
        // two different questions and the table below answers the second.
        Widgets.DrawBoxSolid(
            new Rect(rect.x, rect.yMax + RuleGap - 1f, rect.width, 1f),
            new Color(skin.AccentColor.r, skin.AccentColor.g, skin.AccentColor.b, 0.35f)
        );
    }

    // A pawn born to every metal is named for that and wears one mark. One metal
    // names the metal. Anything between is simply an Allomancer, and wears a mark
    // for each metal they actually have.
    private void RefreshIdentity(Pawn pawn, InvestitureSnapshot snapshot) {
        int bucket = Find.TickManager.TicksGame / RefreshInterval;
        if (cachedPawnId == pawn.thingIDNumber
            && cachedBucket == bucket
            && cachedCellCount == snapshot.Cells.Count) {
            return;
        }

        cachedPawnId = pawn.thingIDNumber;
        cachedBucket = bucket;
        cachedCellCount = snapshot.Cells.Count;
        marks.Clear();

        List<InvestitureCell> cells = snapshot.Cells;
        rank = "CC_Dock_Crest_MetalCount".Translate(cells.Count.Named("COUNT")).Resolve();

        if (feruchemy ? pawn.IsFullFeruchemist() : pawn.IsMistborn()) {
            glyph = feruchemy ? ScadrialMarks.FullFeruchemist : ScadrialMarks.Mistborn;
            title = (feruchemy ? "CC_Dock_Crest_Feruchemist" : "CC_Dock_Crest_Mistborn").Translate().Resolve();
            return;
        }

        if (cells.Count == 1) {
            glyph = cells[0].Icon;
            title = (feruchemy ? "CC_Dock_Crest_Ferring" : "CC_Dock_Crest_Misting")
                .Translate(MetalLabel(cells[0].SubsystemId).Named("METAL"))
                .Resolve();
            return;
        }

        // No single mark is true here, so every metal gets its own.
        glyph = null;
        title = (feruchemy ? "CC_Dock_Crest_Feruchemist" : "CC_Dock_Crest_Allomancer").Translate().Resolve();
        for (int i = 0; i < cells.Count; i++) marks.Add(cells[i].Icon);
    }

    // The one thing seventeen tiles cannot say at a glance. Finding what is lit means
    // scanning the table; this says it in three words, and says something true even
    // when the answer is nothing.
    private void RefreshActivity(Pawn pawn, InvestitureSnapshot snapshot) {
        if (feruchemy) {
            RefreshFeruchemyActivity(pawn);
            return;
        }

        int burning = 0;
        int flaring = 0;
        List<InvestitureCell> cells = snapshot.Cells;
        for (int i = 0; i < cells.Count; i++) {
            if (cells[i].IsFlaring) flaring++;
            else if (cells[i].IsActive) burning++;
        }

        subtitleColor = burning + flaring > 0 ? LiveColor : IdleColor;
        subtitle = burning > 0 && flaring > 0
            ? "CC_Dock_Crest_BurningFlaring".Translate(burning.Named("BURNING"), flaring.Named("FLARING")).Resolve()
            : flaring > 0
                ? "CC_Dock_Crest_Flaring".Translate(flaring.Named("COUNT")).Resolve()
                : burning > 0
                    ? "CC_Dock_Crest_Burning".Translate(burning.Named("COUNT")).Resolve()
                    : "CC_Dock_Crest_NothingBurning".Translate().Resolve();
    }

    // Tapping and storing are opposite directions through the same gene, and the
    // cells cannot tell them apart, so this reads the genes. A Ferring with nothing
    // to draw on is the loudest case: they can do nothing at all.
    private void RefreshFeruchemyActivity(Pawn pawn) {
        int tapping = 0;
        int storing = 0;
        int metalminds = 0;

        List<Verse.Gene> all = pawn.genes?.GenesListForReading ?? [];
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Feruchemist f || f.Overridden) continue;

            metalminds += f.metalminds.Count;
            if (f.isTapping) tapping++;
            else if (f.isStoring) storing++;
        }

        if (metalminds == 0) {
            subtitleColor = WarnColor;
            subtitle = "CC_Dock_Crest_NoMetalminds".Translate().Resolve();
            return;
        }

        subtitleColor = tapping + storing > 0 ? LiveColor : IdleColor;
        subtitle = tapping > 0 && storing > 0
            ? "CC_Dock_Crest_TappingStoring".Translate(tapping.Named("TAPPING"), storing.Named("STORING")).Resolve()
            : tapping > 0
                ? "CC_Dock_Crest_Tapping".Translate(tapping.Named("COUNT")).Resolve()
                : storing > 0
                    ? "CC_Dock_Crest_Storing".Translate(storing.Named("COUNT")).Resolve()
                    : "CC_Dock_Crest_NothingMoving".Translate().Resolve();
    }

    private static string MetalLabel(string subsystemId) {
        return DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(subsystemId)?.LabelCap ?? subsystemId;
    }
}
