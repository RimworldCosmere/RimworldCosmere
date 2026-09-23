using Cosmere.Core.Comp.Map;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Allomancy.Comp.Hediff;
using Cosmere.System.Scadrial.Def;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Verb;

/// <summary>
///     Targeting verb for the external physical Allomancy abilities.
/// </summary>
/// <remarks>
///     Verb_CastAbility.ValidateTarget never consults Ability.CanApplyOn, so steelpush and ironpull
///     used to fire on stone and burn metal for nothing. IsApplicableTo is the hook that gates it.
/// </remarks>
public class PhysicalAllomancy : Verb_CastAbility {
    /// <summary>
    ///     Lines only for metal near the cursor. The ability reaches 18 tiles, and drawing every
    ///     line in that radius buries the one the player is aiming at.
    /// </summary>
    private const float HighlightRadius = 4f;

    /// <summary>
    ///     The blue an Allomancer sees metal through. Same value as PhysicalExternalAura's
    ///     lineColor, so the targeting preview and the burning aura read as one system.
    /// </summary>
    private static readonly Color LineColor = new Color(0.3f, 0.6f, 1f, 1f);

    private readonly List<Verse.Thing> metalNearCursor = [];
    private Material? cachedLineMaterial;
    private int lastScanFrame = -1;

    /// <summary>
    ///     Matches LineDrawingAuraProperties.lineMaterial. A different texture or shader here and
    ///     the preview stops looking like the aura it is previewing.
    /// </summary>
    private Material LineMaterial =>
        cachedLineMaterial ??= MaterialPool.MatFrom(
            GenDraw.OneSidedLineOpaqueTexPath,
            ShaderDatabase.TransparentPostLight,
            LineColor
        );

    public override bool IsApplicableTo(LocalTargetInfo target, bool showMessages = false) {
        if (ability == null || ability.CanApplyOn(target)) return true;

        if (showMessages && target.IsValid && CasterPawn is { Spawned: true }) {
            Messages.Message(
                "CS_NoMetalToGrasp".Translate(),
                new LookTargets(CasterPawn, target.ToTargetInfo(CasterPawn.Map)),
                MessageTypeDefOf.RejectInput,
                false
            );
        }

        return false;
    }

    public override void DrawHighlight(LocalTargetInfo target) {
        base.DrawHighlight(target);

        Pawn caster = CasterPawn;
        if (caster?.Map == null || !target.IsValid) return;

        bool pulling = ability is Ability.AllomancyAbility { def.metal.allomancy.polarity: AllomancyPolarity.Pulling };

        RefreshMetalNearCursor(caster, target.Cell);
        for (int i = 0; i < metalNearCursor.Count; i++) {
            LineToRender line = PhysicalExternalAura.MetalLine(
                caster,
                metalNearCursor[i],
                pulling,
                LineMaterial,
                HighlightRadius
            );
            GenDraw.DrawLineBetween(line.from.DrawPos, line.to.DrawPos, line.lineMaterial, line.thickness);
        }
    }

    /// <summary>
    ///     DrawHighlight runs every frame the targeter is open. One scan per frame is enough, and
    ///     the list is reused so the sweep allocates nothing.
    /// </summary>
    private void RefreshMetalNearCursor(Pawn caster, IntVec3 cursor) {
        if (Time.frameCount == lastScanFrame) return;
        lastScanFrame = Time.frameCount;

        metalNearCursor.Clear();
        int cells = GenRadial.NumCellsInRadius(HighlightRadius);
        for (int i = 0; i < cells; i++) {
            IntVec3 cell = cursor + GenRadial.RadialPattern[i];
            if (!cell.InBounds(caster.Map) || !CanHitTarget(cell)) continue;

            List<Verse.Thing> things = cell.GetThingList(caster.Map);
            for (int j = 0; j < things.Count; j++) {
                if (things[j] != caster && MetalDetector.HasMetal(things[j])) metalNearCursor.Add(things[j]);
            }
        }
    }
}
