using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Keeps a koloss looking its age: build, face and split skin are all readings of the growth
///     hediff, stored as hidden cosmetic genes so vanilla does the drawing.
/// </summary>
public static class KolossAppearance {
    /// <summary>
    ///     Five years of the default eight. Fractions of a life, not years: the growth-years
    ///     setting rewrites how fast severity climbs, not the 0-to-1 range it climbs through.
    /// </summary>
    public const float MatureAt = 0.625f;

    /// <summary>The skin stops keeping up at the same point the body changes shape.</summary>
    public const float ScarsLowAt = 0.625f;

    public const float ScarsMediumAt = 0.75f;
    public const float ScarsHeavyAt = 0.875f;

    private static readonly List<GeneDef> BuildGenes = [];
    private static readonly List<GeneDef> ScarGenes = [];

    /// <summary>
    ///     Puts the cosmetic genes and head type where growth says they belong. Anything without a
    ///     growth clock is left alone, which is what excludes the koloss-blooded.
    /// </summary>
    public static void Refresh(Pawn? pawn) {
        if (pawn?.genes == null || pawn.story == null) return;

        Verse.Hediff? growth = pawn.health?.hediffSet?.GetFirstHediffOfDef(
            HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth
        );
        if (growth == null) return;

        EnsureGroups();

        bool mature = growth.Severity >= MatureAt;
        GeneDef build = mature
            ? GeneDefOf.Cosmere_Scadrial_Gene_KolossBuild_Mature
            : GeneDefOf.Cosmere_Scadrial_Gene_KolossBuild_Young;

        bool changed = KeepOnly(pawn, BuildGenes, build);
        changed |= KeepOnly(pawn, ScarGenes, ScarsFor(growth.Severity));
        changed |= MoveHead(pawn, mature);
        changed |= Bare(pawn);

        if (changed) pawn.Drawer?.renderer?.SetAllGraphicsDirty();
    }

    /// <summary>
    ///     The body texture this pawn's build gene calls for, or null when it is not a koloss.
    /// </summary>
    public static string? BodyGraphicPathFor(Pawn? pawn) {
        if (pawn?.genes == null) return null;
        if (pawn.genes.HasActiveGene(GeneDefOf.Cosmere_Scadrial_Gene_KolossBuild_Mature)) {
            return "Things/Pawn/Humanlike/Bodies/Koloss_Mature";
        }

        return pawn.genes.HasActiveGene(GeneDefOf.Cosmere_Scadrial_Gene_KolossBuild_Young)
            ? "Things/Pawn/Humanlike/Bodies/Koloss_Young"
            : null;
    }

    private static GeneDef? ScarsFor(float severity) {
        if (severity >= ScarsHeavyAt) return GeneDefOf.Cosmere_Scadrial_Gene_KolossScars_Heavy;
        if (severity >= ScarsMediumAt) return GeneDefOf.Cosmere_Scadrial_Gene_KolossScars_Medium;

        return severity >= ScarsLowAt ? GeneDefOf.Cosmere_Scadrial_Gene_KolossScars_Low : null;
    }

    /// <summary>Leaves exactly <paramref name="wanted" /> of a group on the pawn.</summary>
    private static bool KeepOnly(Pawn pawn, List<GeneDef> group, GeneDef? wanted) {
        bool changed = false;

        foreach (GeneDef def in group) {
            Verse.Gene? had = pawn.genes.GetGene(def);
            if (def == wanted) {
                if (had != null) continue;

                // Endogene: a koloss did not have this implanted, it is what it grew into.
                pawn.genes.AddGene(def, false);
                changed = true;
            } else if (had != null) {
                pawn.genes.RemoveGene(had);
                changed = true;
            }
        }

        return changed;
    }

    /// <summary>
    ///     Swaps to the matching face when the build changes under it, keeping whichever of the two
    ///     this koloss started with - the choice rides thingIDNumber, so it survives a reload.
    /// </summary>
    private static bool MoveHead(Pawn pawn, bool mature) {
        bool second = pawn.thingIDNumber % 2 != 0;
        HeadTypeDef want = mature
            ? second
                ? HeadTypeDefOf.Cosmere_Scadrial_KolossHead_Mature_B
                : HeadTypeDefOf.Cosmere_Scadrial_KolossHead_Mature_A
            : second
                ? HeadTypeDefOf.Cosmere_Scadrial_KolossHead_Young_B
                : HeadTypeDefOf.Cosmere_Scadrial_KolossHead_Young_A;

        if (want == null || pawn.story.headType == want) return false;

        pawn.story.headType = want;

        return true;
    }

    /// <summary>
    ///     Strips hair, beard and ink. Whatever this thing used to be, it kept none of the habits
    ///     of a person, and its ideoligion is not something it can still act on.
    /// </summary>
    private static bool Bare(Pawn pawn) {
        bool changed = false;

        if (pawn.story.hairDef != HairDefOf.Bald) {
            pawn.story.hairDef = HairDefOf.Bald;
            changed = true;
        }

        if (pawn.style == null) return changed;

        if (pawn.style.beardDef != BeardDefOf.NoBeard) {
            pawn.style.beardDef = BeardDefOf.NoBeard;
            changed = true;
        }

        // Null when Ideology is off, and null already means no tattoo, so there is nothing to do.
        if (TattooDefOf.NoTattoo_Body != null && pawn.style.BodyTattoo != TattooDefOf.NoTattoo_Body) {
            pawn.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
            changed = true;
        }

        if (TattooDefOf.NoTattoo_Face != null && pawn.style.FaceTattoo != TattooDefOf.NoTattoo_Face) {
            pawn.style.FaceTattoo = TattooDefOf.NoTattoo_Face;
            changed = true;
        }

        return changed;
    }

    private static void EnsureGroups() {
        if (BuildGenes.Count > 0) return;

        BuildGenes.Add(GeneDefOf.Cosmere_Scadrial_Gene_KolossBuild_Young);
        BuildGenes.Add(GeneDefOf.Cosmere_Scadrial_Gene_KolossBuild_Mature);
        ScarGenes.Add(GeneDefOf.Cosmere_Scadrial_Gene_KolossScars_Low);
        ScarGenes.Add(GeneDefOf.Cosmere_Scadrial_Gene_KolossScars_Medium);
        ScarGenes.Add(GeneDefOf.Cosmere_Scadrial_Gene_KolossScars_Heavy);
    }
}
