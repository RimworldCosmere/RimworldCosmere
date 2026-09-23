using Concord;
using Cosmere.Core.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Patch.InnerStorage;

/// <summary>
///     Draws an Investiture band where the hit points slider would go, for containers that ask for
///     one.
/// </summary>
/// <remarks>
///     Vanilla lays the hit points and quality sliders out itself, inside the filter column's
///     scroll view and above the category tree, so the band has to be drawn from the same place to
///     sit with them. Every other container still gets its hit points slider.
/// </remarks>
[Patch(typeof(ThingFilterUI))]
public static class InvestitureBandFilterPatch {
    // vanilla's own numbers from DrawHitPointsFilterConfig, so the band lines up with the quality slider beneath it.
    private const float Inset = 20f;
    private const float SliderHeight = 32f;
    private const float RowHeight = 24f;
    private const float RowGap = 5f;

    [Inject(At.Head, "DrawHitPointsFilterConfig")]
    private static Control BeforeDrawHitPointsFilterConfig(ref float y, float width, ThingFilter filter) {
        Comp.Thing.InnerStorage? storage = Comp.Thing.InnerStorage.BandOwnerOf(filter);
        if (storage == null) return Control.Continue;

        DrawBand(ref y, width, storage);
        return Control.Cancel;
    }

    private static void DrawBand(ref float y, float width, Comp.Thing.InnerStorage storage) {
        Rect slider = new Rect(Inset, y, width - Inset, SliderHeight);
        FloatRange band = storage.allowedInvestiture;
        Widgets.FloatRange(
            slider,
            storage.parent.thingIDNumber,
            ref band,
            0f,
            1f,
            storage.props.thresholdLabel,
            ToStringStyle.PercentZero,
            0f,
            GameFont.Small,
            null,
            0.01f
        );
        TooltipHandler.TipRegion(slider, "CC_Storage_InvestitureBand_Tip".Translate());
        MouseoverSounds.DoRegion(slider);
        storage.allowedInvestiture = band;
        y += SliderHeight + RowGap;

        Rect eject = new Rect(Inset, y, width - Inset, RowHeight);
        bool wasEjecting = storage.ejectOutOfRange;
        bool ejecting = wasEjecting;
        Widgets.CheckboxLabeled(eject, "CC_Storage_InvestitureBand_Eject".Translate(), ref ejecting);
        Widgets.DrawHighlightIfMouseover(eject);
        TooltipHandler.TipRegion(eject, "CC_Storage_InvestitureBand_Eject_Tip".Translate());
        MouseoverSounds.DoRegion(eject);

        if (ejecting != wasEjecting) {
            storage.ejectOutOfRange = ejecting;
            (ejecting ? RimWorld.SoundDefOf.Checkbox_TurnedOn : RimWorld.SoundDefOf.Checkbox_TurnedOff).PlayOneShotOnCamera();
        }

        y += RowHeight + RowGap;

        // FloatRange leaves the font where it likes it; vanilla puts it back here too.
        Text.Font = GameFont.Small;
    }
}
