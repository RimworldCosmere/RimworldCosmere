using System;
using Cosmere.Core.Extension;
using Cosmere.Pickle.Lookup;
using Cosmere.System.Roshar.DefModExtension;
using Cosmere.System.Roshar.Extension;
using Cosmere.System.Roshar.Gene;
using RimWorks.Pickle;
using RimWorld;
using UnityEngine;
using Verse;
using Invested = Cosmere.Core.Gene.Invested;

namespace Cosmere.Pickle.Steps;

/// <summary>Genes, which is how every Cosmere magic system reaches a pawn: granting one,
/// taking it back, and reading the Investiture reserve an <c>Invested</c> gene carries.</summary>
[PickleSteps]
public class GeneSteps {
    /// <summary>Reserves are floats, so an equality assert would fail at random.</summary>
    private const float ReserveTolerance = 0.01f;

    // every path in the tree that grants one of these itself passes xenogene: true
    private const bool AsXenogene = true;

    /// <summary>Grants a gene the way the game grants it. A Radiant order goes through
    /// <c>TryAddRadiantOrder</c>, so the Honor gate, the rebond check and the spren all run.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="geneDefName">The gene def to grant.</param>
    [When("I give {string} the gene {string}")]
    public void GiveGene(PickleContext ctx, string nickname, string geneDefName) {
        Pawn pawn = RequireGeneCarrier(ctx, nickname);
        GeneDef geneDef = CosmereLookup.RequireDef<GeneDef>(geneDefName);

        if (geneDef.GetModExtension<RadiantOrder>()?.order != null) {
            Surgebinder? bond = pawn.genes!.TryAddRadiantOrder(geneDef, xenogene: AsXenogene);
            ctx.Require(
                bond != null,
                $"the game refused to bond '{geneDefName}' to '{nickname}'. the Honor shard has to be " +
                "enabled, and RadiantTracker has to allow the bond");
        } else {
            pawn.genes!.EnsureGene(geneDef, AsXenogene);
        }

        CosmereLookup.AssertThat(
            ctx,
            pawn.genes!.HasActiveGene(geneDef),
            $"'{nickname}' should carry the gene '{geneDefName}' once it is granted",
            () => DescribeGenes(pawn));
    }

    /// <summary>Takes a gene off a pawn. Removing one it never had is not an error.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="geneDefName">The gene def to remove.</param>
    /// <param name="nickname">The pawn's short name.</param>
    [When("I take the gene {string} from {string}")]
    public void TakeGene(PickleContext ctx, string geneDefName, string nickname) {
        Pawn pawn = RequireGeneCarrier(ctx, nickname);
        GeneDef geneDef = CosmereLookup.RequireDef<GeneDef>(geneDefName);

        pawn.genes!.RemoveGene(geneDef);

        CosmereLookup.AssertThat(
            ctx,
            !pawn.genes!.HasActiveGene(geneDef),
            $"'{nickname}' should no longer carry the gene '{geneDefName}'",
            () => DescribeGenes(pawn));
    }

    /// <summary>Asserts a pawn carries an active gene.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="geneDefName">The gene def expected.</param>
    [Then("{string} has gene {string}")]
    public void AssertHasGene(PickleContext ctx, string nickname, string geneDefName) {
        Pawn pawn = RequireGeneCarrier(ctx, nickname);
        GeneDef geneDef = CosmereLookup.RequireDef<GeneDef>(geneDefName);

        CosmereLookup.AssertThat(
            ctx,
            pawn.genes!.HasActiveGene(geneDef),
            $"'{nickname}' should have the gene '{geneDefName}'",
            () => DescribeGenes(pawn));
    }

    /// <summary>Asserts a pawn does not carry an active gene.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="geneDefName">The gene def that must be absent.</param>
    [Then("{string} has no gene {string}")]
    public void AssertLacksGene(PickleContext ctx, string nickname, string geneDefName) {
        Pawn pawn = RequireGeneCarrier(ctx, nickname);
        GeneDef geneDef = CosmereLookup.RequireDef<GeneDef>(geneDefName);

        CosmereLookup.AssertThat(
            ctx,
            !pawn.genes!.HasActiveGene(geneDef),
            $"'{nickname}' should not have the gene '{geneDefName}'",
            () => DescribeGenes(pawn));
    }

    /// <summary>Sets an invested gene's reserve to an absolute amount, then proves it landed.
    /// The gene clamps to its own floor and ceiling, so an over-ask fails here and names both.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="geneDefName">The invested gene def.</param>
    /// <param name="amount">The reserve to set.</param>
    [When("{string} gene {string} is set to {float}")]
    public void SetReserve(PickleContext ctx, string nickname, string geneDefName, float amount) {
        Invested gene = RequireInvested(ctx, nickname, geneDefName);
        gene.SetReserve(amount);

        CosmereLookup.AssertThat(
            ctx,
            Mathf.Abs(gene.Value - amount) <= ReserveTolerance,
            $"'{nickname}' gene '{geneDefName}' should hold {amount:0.###} once it is set",
            () => DescribeReserve(gene));
    }

    /// <summary>Sets an invested gene's reserve as a share of its ceiling, for a gene whose
    /// ceiling a feature file cannot know. A ceiling of zero fails rather than passing at 0.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="geneDefName">The invested gene def.</param>
    /// <param name="percent">The share of the ceiling, 0 to 100.</param>
    [When("{string} gene {string} is set to {int} percent")]
    public void SetReservePercent(PickleContext ctx, string nickname, string geneDefName, int percent) {
        ctx.Require(percent is >= 0 and <= 100, $"a reserve share must be 0 to 100; got {percent}");

        Invested gene = RequireInvested(ctx, nickname, geneDefName);
        float wanted = percent / 100f;
        gene.SetReserve(gene.Max * wanted);

        CosmereLookup.AssertThat(
            ctx,
            Mathf.Abs(gene.ValuePercent - wanted) <= ReserveTolerance,
            $"'{nickname}' gene '{geneDefName}' should sit at {percent} percent once it is set",
            () => DescribeReserve(gene));
    }

    /// <summary>Asserts an invested gene's reserve, within a tolerance.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="geneDefName">The invested gene def.</param>
    /// <param name="amount">The reserve expected.</param>
    [Then("{string} gene {string} is {float}")]
    public void AssertReserve(PickleContext ctx, string nickname, string geneDefName, float amount) {
        Invested gene = RequireInvested(ctx, nickname, geneDefName);

        CosmereLookup.AssertThat(
            ctx,
            Mathf.Abs(gene.Value - amount) <= ReserveTolerance,
            $"'{nickname}' gene '{geneDefName}' should hold {amount:0.###}, give or take {ReserveTolerance:0.###}",
            () => DescribeReserve(gene));
    }

    /// <summary>Asserts an invested gene's reserve is above a bound, which is how a scenario
    /// checks a gain without pinning a number the game's own rate decides.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="geneDefName">The invested gene def.</param>
    /// <param name="bound">The lower bound, exclusive.</param>
    [Then("{string} gene {string} is above {float}")]
    public void AssertReserveAbove(PickleContext ctx, string nickname, string geneDefName, float bound) {
        Invested gene = RequireInvested(ctx, nickname, geneDefName);

        CosmereLookup.AssertThat(
            ctx,
            gene.Value > bound,
            $"'{nickname}' gene '{geneDefName}' should hold more than {bound:0.###}",
            () => DescribeReserve(gene));
    }

    /// <summary>Asserts an invested gene's reserve is below a bound.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="geneDefName">The invested gene def.</param>
    /// <param name="bound">The upper bound, exclusive.</param>
    [Then("{string} gene {string} is below {float}")]
    public void AssertReserveBelow(PickleContext ctx, string nickname, string geneDefName, float bound) {
        Invested gene = RequireInvested(ctx, nickname, geneDefName);

        CosmereLookup.AssertThat(
            ctx,
            gene.Value < bound,
            $"'{nickname}' gene '{geneDefName}' should hold less than {bound:0.###}",
            () => DescribeReserve(gene));
    }

    /// <summary>Asserts a gene grants an ability right now. A Radiant order grants more of
    /// them as its ideal rises, so this answers "at this ideal", not "ever".</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="geneDefName">The gene def.</param>
    /// <param name="abilityDefName">The ability def expected.</param>
    [Then("{string} gene {string} grants ability {string}")]
    public void AssertGrantsAbility(PickleContext ctx, string nickname, string geneDefName, string abilityDefName) {
        Pawn pawn = RequireGeneCarrier(ctx, nickname);
        Verse.Gene gene = RequireGene(ctx, pawn, geneDefName);
        AbilityDef abilityDef = CosmereLookup.RequireDef<AbilityDef>(abilityDefName);

        CosmereLookup.AssertThat(
            ctx,
            GrantedAbilities(gene).Contains(abilityDef),
            $"'{nickname}' gene '{geneDefName}' should grant '{abilityDefName}'",
            () => DescribeAbilities(pawn, gene));
    }

    /// <summary>Asserts a gene does not grant an ability right now.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn's short name.</param>
    /// <param name="geneDefName">The gene def.</param>
    /// <param name="abilityDefName">The ability def that must be absent.</param>
    [Then("{string} gene {string} does not grant ability {string}")]
    public void AssertLacksAbility(PickleContext ctx, string nickname, string geneDefName, string abilityDefName) {
        Pawn pawn = RequireGeneCarrier(ctx, nickname);
        Verse.Gene gene = RequireGene(ctx, pawn, geneDefName);
        AbilityDef abilityDef = CosmereLookup.RequireDef<AbilityDef>(abilityDefName);

        CosmereLookup.AssertThat(
            ctx,
            !GrantedAbilities(gene).Contains(abilityDef),
            $"'{nickname}' gene '{geneDefName}' should not grant '{abilityDefName}'",
            () => DescribeAbilities(pawn, gene));
    }

    private static Pawn RequireGeneCarrier(PickleContext ctx, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ctx.Require(
            pawn.genes != null,
            $"'{nickname}' has no gene tracker, so no gene step can touch it. Biotech has to be active and " +
            "the pawn has to be a race that carries genes");

        return pawn;
    }

    private static Verse.Gene RequireGene(PickleContext ctx, Pawn pawn, string geneDefName) {
        GeneDef geneDef = CosmereLookup.RequireDef<GeneDef>(geneDefName);
        Verse.Gene? gene = pawn.genes!.GetGene(geneDef);

        ctx.Require(
            gene != null,
            $"'{pawn.Name?.ToStringShort}' does not carry the gene '{geneDefName}'. {DescribeGenes(pawn)}");

        return gene!;
    }

    private static Invested RequireInvested(PickleContext ctx, string nickname, string geneDefName) {
        Pawn pawn = RequireGeneCarrier(ctx, nickname);
        Verse.Gene gene = RequireGene(ctx, pawn, geneDefName);

        ctx.Require(
            gene is Invested,
            $"gene '{geneDefName}' on '{nickname}' is a {gene.GetType().Name}, not an invested gene, so it " +
            "carries no reserve to read or set");

        return (Invested)gene;
    }

    private static List<AbilityDef> GrantedAbilities(Verse.Gene gene) {
        List<AbilityDef>? abilities = gene is Invested invested ? invested.Abilities : gene.def.abilities;

        return abilities ?? [];
    }

    private static string DescribeGenes(Pawn pawn) {
        List<Verse.Gene> genes = pawn.genes!.GenesListForReading;
        if (genes.Count == 0) return "the pawn carries no genes";

        List<string> described = [.. genes
            .Select(g => g.Active ? g.def.defName : $"{g.def.defName} (overridden)")
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)];

        return $"the pawn carries: {string.Join(", ", described)}";
    }

    // Max and MinimumAmount are the clamp SetReserve applies, so a set that did not land is explained here
    private static string DescribeReserve(Invested gene) {
        return $"value={gene.Value:0.###} of max={gene.Max:0.###} ({gene.ValuePercent:P0}), " +
            $"floor={gene.MinimumAmount:0.###}, investiture ceiling={gene.MaxInvestitureLevel:0.###}, " +
            $"drain={gene.DrainPerSecond:0.###}/s";
    }

    // the gene's list and the pawn's tracker can disagree, and that gap is the defect worth seeing
    private static string DescribeAbilities(Pawn pawn, Verse.Gene gene) {
        List<AbilityDef> granted = GrantedAbilities(gene);
        string grantedNames = granted.Count == 0
            ? "(none)"
            : string.Join(", ", granted.Select(a => a.defName).OrderBy(n => n, StringComparer.OrdinalIgnoreCase));

        List<Ability>? held = pawn.abilities?.AllAbilitiesForReading;
        string heldNames = held == null || held.Count == 0
            ? "(none)"
            : string.Join(", ", held.Select(a => a.def.defName).OrderBy(n => n, StringComparer.OrdinalIgnoreCase));

        return $"the gene grants: {grantedNames}; the pawn holds: {heldNames}";
    }
}
