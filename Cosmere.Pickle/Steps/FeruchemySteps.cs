using System;
using Cosmere.Core.Def;
using Cosmere.Pickle.Lookup;
using Cosmere.System.Scadrial.Feruchemy;
using RimWorks.Pickle;
using RimWorld;
using Verse;
using ImplantData = Cosmere.System.Scadrial.Feruchemy.Hediff.ImplantedMetalmindData;
using Implants = Cosmere.System.Scadrial.Feruchemy.Hediff.ImplantedMetalminds;
using Metalmind = Cosmere.System.Scadrial.Feruchemy.Comp.Thing.Metalmind;
using StoredMemory = Cosmere.System.Scadrial.Feruchemy.Memory.StoredMemory;

namespace Cosmere.Pickle.Steps;

/// <summary>Metalminds and what a feruchemist does with them: filling one, drawing it back,
/// burning it away, and the bounds the charge ledger puts on all three.</summary>
/// <remarks>Every move goes through <see cref="MetalmindDistribution.Transfer" />, the same
/// call the Feruchemist gene makes, so a step can never move charge a dial could not.</remarks>
[PickleSteps]
public class FeruchemySteps {
    private const string ImplantHediffDefName = "Cosmere_Scadrial_Hediff_ImplantedMetalminds";

    private const string CarriedScope = "carried";

    private const string ImplantedScope = "implanted";

    /// <summary>Puts a normal-quality metalmind of a metal into a pawn's inventory.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn who carries it.</param>
    /// <param name="metal">The metal it is made of, by MetalDef name.</param>
    /// <param name="thingDefName">The metalmind ThingDef.</param>
    [Given("{string} carries a {string} {string}")]
    public void CarriesMetalmind(PickleContext ctx, string nickname, string metal, string thingDefName) {
        CarriesMetalmindOfQuality(ctx, nickname, metal, thingDefName, "Normal");
    }

    /// <summary>Puts a metalmind of a named quality into a pawn's inventory. Quality scales
    /// capacity, so a scenario that asserts a figure has to pin it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn who carries it.</param>
    /// <param name="metal">The metal it is made of, by MetalDef name.</param>
    /// <param name="thingDefName">The metalmind ThingDef.</param>
    /// <param name="quality">The QualityCategory name.</param>
    [Given("{string} carries a {string} {string} of {word} quality")]
    public void CarriesMetalmindOfQuality(
        PickleContext ctx,
        string nickname,
        string metal,
        string thingDefName,
        string quality
    ) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ctx.Require(pawn.inventory != null, $"pawn '{nickname}' has no inventory to carry a metalmind in");

        // one of this metal already on them splits every transfer, silently.
        ctx.Require(
            CarriedOf(pawn, metal).Count == 0,
            $"pawn '{nickname}' already carries a {metal} metalmind; {Describe(pawn)}");

        Verse.Thing thing = MakeMetalmind(ctx, quality, metal, thingDefName);
        ctx.Require(
            pawn.inventory!.innerContainer.TryAdd(thing, false),
            $"pawn '{nickname}' would not take the {metal} {thingDefName} into their inventory");
    }

    /// <summary>Files a metalmind under a pawn's implant hediff, the way the surgery recipe does.
    /// Only an implanted metalmind can be compounded into.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to implant it in.</param>
    /// <param name="metal">The metal it is made of, by MetalDef name.</param>
    /// <param name="thingDefName">The metalmind ThingDef.</param>
    [Given("{string} has a {string} {string} implanted")]
    public void HasMetalmindImplanted(PickleContext ctx, string nickname, string metal, string thingDefName) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        ctx.Require(
            !ImplantsOf(pawn).Any(d => IsMetal(d.Metal, metal)),
            $"pawn '{nickname}' already has a {metal} metalmind implanted; {Describe(pawn)}");

        Verse.Thing thing = MakeMetalmind(ctx, "Normal", metal, thingDefName);
        Metalmind comp = thing.TryGetComp<Metalmind>()!;

        BodyPartRecord? torso = pawn.health.hediffSet.GetNotMissingParts()
            .FirstOrDefault(p => p.def == pawn.RaceProps.body.corePart.def);
        ctx.Require(torso != null, $"pawn '{nickname}' has no torso left to implant a metalmind in");

        ImplantData data = new ImplantData {
            metalDefName = comp.Metal?.defName ?? metal,
            metalmindType = thingDefName,
            MaxAmount = comp.MaxAmount,
        };

        Implants.Attach(pawn, data, torso!);
        thing.Destroy();
    }

    /// <summary>Points the following transfers at one group of metalminds: all, external or
    /// internal. Without this step a transfer reaches every metalmind of the metal.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn whose transfers this aims.</param>
    /// <param name="group">One of all, external or internal.</param>
    [Given("{string} aims feruchemy at {word} metalminds")]
    public void AimsFeruchemy(PickleContext ctx, string nickname, string group) {
        Pawn aimed = CosmereLookup.RequirePawn(ctx, nickname);

        string target = group.ToLowerInvariant() switch {
            "all" => MetalmindDistribution.TargetAll,
            "external" => MetalmindDistribution.TargetExternal,
            "internal" => MetalmindDistribution.TargetInternal,
            _ => throw new InvalidOperationException($"'{group}' is not a metalmind group; use all, external or internal"),
        };

        Aims(ctx).ByPawn[aimed.thingIDNumber] = target;
    }

    /// <summary>Fills the pawn's metalminds of a metal with an attribute.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The feruchemist.</param>
    /// <param name="amount">The charge offered.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    [When("{string} stores {float} into the {string} metalminds")]
    public void Stores(PickleContext ctx, string nickname, float amount, string metal) {
        Move(ctx, nickname, metal, MetalmindOperation.Store, amount, null);
    }

    /// <summary>Draws an attribute back out of the pawn's metalminds of a metal.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The feruchemist.</param>
    /// <param name="amount">The charge asked for.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    [When("{string} taps {float} from the {string} metalminds")]
    public void Taps(PickleContext ctx, string nickname, float amount, string metal) {
        Move(ctx, nickname, metal, MetalmindOperation.Tap, amount, null);
    }

    /// <summary>Fills the compounded pool, which only an implant will take.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The feruchemist.</param>
    /// <param name="amount">The charge offered.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    [When("{string} compounds {float} into the {string} metalminds")]
    public void Compounds(PickleContext ctx, string nickname, float amount, string metal) {
        Move(ctx, nickname, metal, MetalmindOperation.StoreCompounded, amount, null);
    }

    /// <summary>Burns charge out of the pawn's metalminds of a metal, spending the metal with it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The feruchemist.</param>
    /// <param name="amount">The charge asked for.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    [When("{string} burns {float} from the {string} metalminds")]
    public void Burns(PickleContext ctx, string nickname, float amount, string metal) {
        Move(ctx, nickname, metal, MetalmindOperation.TapCompounded, amount, null);
    }

    /// <summary>Fills a duralumind, filing the charge under one Connection ledger.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The feruchemist.</param>
    /// <param name="amount">The charge offered.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="ledger">The DuraluminLedger name.</param>
    [When("{string} stores {float} into the {string} metalminds under {word}")]
    public void StoresUnder(PickleContext ctx, string nickname, float amount, string metal, string ledger) {
        Move(ctx, nickname, metal, MetalmindOperation.Store, amount, RequireKey(ledger));
    }

    /// <summary>Draws a duralumind back down, reaching only charge filed under that ledger.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The feruchemist.</param>
    /// <param name="amount">The charge asked for.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="ledger">The DuraluminLedger name.</param>
    [When("{string} taps {float} from the {string} metalminds under {word}")]
    public void TapsUnder(PickleContext ctx, string nickname, float amount, string metal, string ledger) {
        Move(ctx, nickname, metal, MetalmindOperation.Tap, amount, RequireKey(ledger));
    }

    /// <summary>Files a memory of a mood weight in a carried metalmind. Memories and attribute
    /// charge share the same metal, so this is space the pawn can no longer store into.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn whose memory it is.</param>
    /// <param name="mood">The memory's mood offset; its magnitude is the space it costs.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    [When("{string} files a memory worth {float} mood in their {string} metalmind")]
    public void FilesMemory(PickleContext ctx, string nickname, float mood, string metal) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        Metalmind comp = RequireCarried(ctx, metal, nickname);
        comp.StoreMemory(new StoredMemory { moodOffset = mood, owner = pawn });
    }

    /// <summary>Hands a carried metalmind from one pawn to another. The metalmind keeps whoever
    /// first filled it as its owner.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="from">The pawn giving it up.</param>
    /// <param name="to">The pawn taking it.</param>
    [When("the {string} metalmind of {string} moves to {string}")]
    public void MovesTo(PickleContext ctx, string metal, string from, string to) {
        Pawn giver = CosmereLookup.RequirePawn(ctx, from);
        Pawn taker = CosmereLookup.RequirePawn(ctx, to);
        ctx.Require(taker.inventory != null, $"pawn '{to}' has no inventory to take a metalmind into");

        Verse.Thing thing = RequireCarried(ctx, metal, from).parent;
        giver.inventory!.innerContainer.Remove(thing);
        ctx.Require(
            taker.inventory!.innerContainer.TryAdd(thing, false),
            $"pawn '{to}' would not take the {metal} metalmind into their inventory");
    }

    /// <summary>Clears out anything the pawn holds that has been burned to nothing.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to sweep.</param>
    [When("I sweep burned out metalminds for {string}")]
    public void SweepBurnedOut(PickleContext ctx, string nickname) {
        MetalmindBurnout.Sweep(CosmereLookup.RequirePawn(ctx, nickname));
    }

    /// <summary>Asserts what the last transfer actually moved, which is what the game accounts
    /// for. A refused move reports nought however much was offered.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="expected">The charge expected to have moved.</param>
    [Then("the last transfer moved {float}")]
    public void AssertLastTransfer(PickleContext ctx, float expected) {
        float moved;
        try {
            moved = ctx.Get<Moved>().Amount;
        } catch (InvalidOperationException) {
            ctx.Require(false, "nothing has been stored, tapped, compounded or burned yet");
            return;
        }

        CosmereLookup.AssertThat(
            ctx,
            CosmereLookup.IsNear(moved, expected),
            $"the last transfer should have moved {expected:0.###}",
            () => $"it moved {moved:0.###}");
    }

    /// <summary>Asserts the ordinary charge a metalmind holds.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="scope">Either carried or implanted.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="nickname">The pawn holding it.</param>
    /// <param name="expected">The charge expected.</param>
    [Then("the {word} {string} metalmind of {string} holds {float}")]
    public void AssertHolds(PickleContext ctx, string scope, string metal, string nickname, float expected) {
        IMetalmindSource source = RequireSource(ctx, scope, metal, nickname);
        AssertFigure(ctx, source, scope, metal, nickname, source.StoredAmount, expected, "should hold");
    }

    /// <summary>Asserts the compounded charge a metalmind holds. It shares capacity with the
    /// ordinary pool but pays out far harder.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="scope">Either carried or implanted.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="nickname">The pawn holding it.</param>
    /// <param name="expected">The compounded charge expected.</param>
    [Then("the {word} {string} metalmind of {string} holds {float} compounded")]
    public void AssertHoldsCompounded(
        PickleContext ctx,
        string scope,
        string metal,
        string nickname,
        float expected
    ) {
        IMetalmindSource source = RequireSource(ctx, scope, metal, nickname);
        AssertFigure(ctx, source, scope, metal, nickname, source.CompoundedAmount, expected, "should hold compounded");
    }

    /// <summary>Asserts the charge a metalmind holds under one Connection ledger.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="scope">Either carried or implanted.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="nickname">The pawn holding it.</param>
    /// <param name="expected">The charge expected under that ledger.</param>
    /// <param name="ledger">The DuraluminLedger name.</param>
    [Then("the {word} {string} metalmind of {string} holds {float} under {word}")]
    public void AssertHoldsUnder(
        PickleContext ctx,
        string scope,
        string metal,
        string nickname,
        float expected,
        string ledger
    ) {
        IMetalmindSource source = RequireSource(ctx, scope, metal, nickname);
        float actual = source.StoredFor(RequireKey(ledger));
        AssertFigure(ctx, source, scope, metal, nickname, actual, expected, $"should hold under {ledger}");
    }

    /// <summary>Asserts how much a metalmind can hold at all. Burning charge out of one spends
    /// the metal, so this figure falls as it is compounded away.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="scope">Either carried or implanted.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="nickname">The pawn holding it.</param>
    /// <param name="expected">The capacity expected.</param>
    [Then("the {word} {string} metalmind of {string} capacity is {float}")]
    public void AssertCapacity(PickleContext ctx, string scope, string metal, string nickname, float expected) {
        IMetalmindSource source = RequireSource(ctx, scope, metal, nickname);
        AssertFigure(ctx, source, scope, metal, nickname, source.MaxAmount, expected, "capacity should be");
    }

    /// <summary>Asserts the room left in a metalmind, which memories eat into as well as charge.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="scope">Either carried or implanted.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="nickname">The pawn holding it.</param>
    /// <param name="expected">The free space expected.</param>
    [Then("the {word} {string} metalmind of {string} has {float} free space")]
    public void AssertFreeSpace(PickleContext ctx, string scope, string metal, string nickname, float expected) {
        IMetalmindSource source = RequireSource(ctx, scope, metal, nickname);
        AssertFigure(ctx, source, scope, metal, nickname, source.FreeSpace, expected, "free space should be");
    }

    /// <summary>Asserts a metalmind offers itself to be drawn on.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="scope">Either carried or implanted.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="nickname">The pawn holding it.</param>
    [Then("the {word} {string} metalmind of {string} can be tapped")]
    public void AssertCanBeTapped(PickleContext ctx, string scope, string metal, string nickname) {
        IMetalmindSource source = RequireSource(ctx, scope, metal, nickname);
        AssertState(ctx, source, scope, metal, nickname, source.CanTap, "should offer itself to be tapped");
    }

    /// <summary>Asserts a metalmind refuses to be drawn on. The gate has to agree with what a
    /// draw would really do: everything that decides whether to tap reads it and nothing else.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="scope">Either carried or implanted.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="nickname">The pawn holding it.</param>
    [Then("the {word} {string} metalmind of {string} cannot be tapped")]
    public void AssertCannotBeTapped(PickleContext ctx, string scope, string metal, string nickname) {
        IMetalmindSource source = RequireSource(ctx, scope, metal, nickname);
        AssertState(ctx, source, scope, metal, nickname, !source.CanTap, "should refuse to be tapped");
    }

    /// <summary>Asserts a metalmind has been spent down to nothing.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="scope">Either carried or implanted.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="nickname">The pawn holding it.</param>
    [Then("the {word} {string} metalmind of {string} is burned out")]
    public void AssertBurnedOut(PickleContext ctx, string scope, string metal, string nickname) {
        IMetalmindSource source = RequireSource(ctx, scope, metal, nickname);
        AssertState(ctx, source, scope, metal, nickname, source.IsBurnedOut, "should be burned out");
    }

    /// <summary>Asserts a metalmind still has metal left in it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="scope">Either carried or implanted.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="nickname">The pawn holding it.</param>
    [Then("the {word} {string} metalmind of {string} is not burned out")]
    public void AssertNotBurnedOut(PickleContext ctx, string scope, string metal, string nickname) {
        IMetalmindSource source = RequireSource(ctx, scope, metal, nickname);
        AssertState(ctx, source, scope, metal, nickname, !source.IsBurnedOut, "should not be burned out");
    }

    /// <summary>Asserts who a carried metalmind is keyed to, which is whoever first filled it.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    /// <param name="nickname">The pawn holding it now.</param>
    /// <param name="ownerNickname">The pawn it should be keyed to.</param>
    [Then("the {string} metalmind of {string} is owned by {string}")]
    public void AssertOwnedBy(PickleContext ctx, string metal, string nickname, string ownerNickname) {
        Metalmind comp = RequireCarried(ctx, metal, nickname);
        Pawn owner = CosmereLookup.RequirePawn(ctx, ownerNickname);

        CosmereLookup.AssertThat(
            ctx,
            comp.owner == owner,
            $"the {metal} metalmind carried by '{nickname}' should be owned by '{ownerNickname}'",
            () => $"it is owned by {comp.owner?.Name?.ToStringShort ?? "nobody"}");
    }

    /// <summary>Asserts a pawn carries no metalmind of a metal, which is what a burn-out sweep
    /// leaves behind.</summary>
    /// <param name="ctx">The scenario's context.</param>
    /// <param name="nickname">The pawn to check.</param>
    /// <param name="metal">The metal, by MetalDef name.</param>
    [Then("{string} carries no {string} metalmind")]
    public void AssertCarriesNone(PickleContext ctx, string nickname, string metal) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        List<Metalmind> carried = CarriedOf(pawn, metal);

        CosmereLookup.AssertThat(
            ctx,
            carried.Count == 0,
            $"pawn '{nickname}' should carry no {metal} metalmind",
            () => $"they carry {carried.Count}: {Describe(pawn)}");
    }

    private static void Move(
        PickleContext ctx,
        string nickname,
        string metal,
        MetalmindOperation operation,
        float amount,
        ConnectionKey? ledgerKey
    ) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        List<IMetalmindSource> sources = SourcesFor(pawn, metal);
        ctx.Require(sources.Count > 0, $"pawn '{nickname}' has no {metal} metalmind; {Describe(pawn)}");

        ctx.Set(new Moved(MetalmindDistribution.Transfer(sources, operation, AimFor(ctx, pawn), ledgerKey, amount)));
    }

    private static string AimFor(PickleContext ctx, Pawn pawn) {
        return Aims(ctx).ByPawn.TryGetValue(pawn.thingIDNumber, out string? target)
            ? target
            : MetalmindDistribution.TargetAll;
    }

    private static Aim Aims(PickleContext ctx) {
        try {
            return ctx.Get<Aim>();
        } catch (InvalidOperationException) {
            Aim fresh = new Aim();
            ctx.Set(fresh);
            return fresh;
        }
    }

    private static Verse.Thing MakeMetalmind(
        PickleContext ctx,
        string quality,
        string metal,
        string thingDefName
    ) {
        ThingDef def = CosmereLookup.RequireDef<ThingDef>(thingDefName);
        ThingDef stuff = CosmereLookup.RequireDef<ThingDef>(metal);
        ctx.Require(
            def.MadeFromStuff,
            $"{thingDefName} is not made from stuff, so it cannot be a {metal} metalmind");

        Verse.Thing thing = ThingMaker.MakeThing(def, stuff);
        ctx.Require(
            thing.TryGetComp<Metalmind>() != null,
            $"{thingDefName} has no Metalmind comp, so it cannot hold a feruchemical charge");

        CompQuality? qualityComp = thing.TryGetComp<CompQuality>();
        if (qualityComp != null) {
            ctx.Require(
                Enum.TryParse(quality, true, out QualityCategory parsed),
                $"'{quality}' is not a quality; use awful, poor, normal, good, excellent, masterwork or legendary");
            qualityComp.SetQuality(parsed, ArtGenerationContext.Colony);
        }

        return thing;
    }

    private static ConnectionKey RequireKey(string ledger) {
        if (!Enum.TryParse(ledger, true, out DuraluminLedger parsed)) {
            throw new InvalidOperationException(
                $"'{ledger}' is not a duralumin ledger; use {string.Join(", ", Enum.GetNames(typeof(DuraluminLedger)))}");
        }

        return ConnectionKey.For(parsed, null);
    }

    private static IMetalmindSource RequireSource(
        PickleContext ctx,
        string scope,
        string metal,
        string nickname
    ) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);

        if (string.Equals(scope, CarriedScope, StringComparison.OrdinalIgnoreCase)) {
            return RequireCarried(ctx, metal, nickname);
        }

        ctx.Require(
            string.Equals(scope, ImplantedScope, StringComparison.OrdinalIgnoreCase),
            $"'{scope}' is not a metalmind scope; use {CarriedScope} or {ImplantedScope}");

        ImplantData? data = ImplantsOf(pawn).FirstOrDefault(d => IsMetal(d.Metal, metal));
        ctx.Require(data != null, $"pawn '{nickname}' has no {metal} metalmind implanted; {Describe(pawn)}");

        return data!;
    }

    private static Metalmind RequireCarried(PickleContext ctx, string metal, string nickname) {
        Pawn pawn = CosmereLookup.RequirePawn(ctx, nickname);
        List<Metalmind> carried = CarriedOf(pawn, metal);
        ctx.Require(carried.Count > 0, $"pawn '{nickname}' carries no {metal} metalmind; {Describe(pawn)}");

        return carried[0];
    }

    /// <summary>The metalminds of one metal a transfer can reach, carried first and implanted
    /// after. The same order and the same filter the Feruchemist gene builds.</summary>
    private static List<IMetalmindSource> SourcesFor(Pawn pawn, string metal) {
        List<IMetalmindSource> sources = [.. CarriedOf(pawn, metal)];
        sources.AddRange(ImplantsOf(pawn).Where(d => IsMetal(d.Metal, metal)));

        return sources;
    }

    private static List<Metalmind> CarriedOf(Pawn pawn, string metal) {
        List<Metalmind> found = [];
        if (pawn.inventory == null) return found;

        List<Verse.Thing> items = pawn.inventory.innerContainer.InnerListForReading;
        for (int i = 0; i < items.Count; i++) {
            Metalmind? comp = items[i].TryGetComp<Metalmind>();
            if (comp != null && IsMetal(comp.Metal, metal)) found.Add(comp);
        }

        return found;
    }

    private static List<ImplantData> ImplantsOf(Pawn pawn) {
        HediffDef def = CosmereLookup.RequireDef<HediffDef>(ImplantHediffDefName);

        return pawn.health?.hediffSet?.GetFirstHediffOfDef(def) is Implants hediff ? hediff.metalminds : [];
    }

    private static bool IsMetal(MetalDef? metalDef, string metal) {
        return metalDef != null && string.Equals(metalDef.defName, metal, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertFigure(
        PickleContext ctx,
        IMetalmindSource source,
        string scope,
        string metal,
        string nickname,
        float actual,
        float expected,
        string wanted
    ) {
        CosmereLookup.AssertThat(
            ctx,
            CosmereLookup.IsNear(actual, expected),
            $"the {scope} {metal} metalmind of '{nickname}' {wanted} {expected:0.###}",
            () => $"it is {actual:0.###}. {Describe(source)}");
    }

    private static void AssertState(
        PickleContext ctx,
        IMetalmindSource source,
        string scope,
        string metal,
        string nickname,
        bool condition,
        string wanted
    ) {
        CosmereLookup.AssertThat(
            ctx,
            condition,
            $"the {scope} {metal} metalmind of '{nickname}' {wanted}",
            () => Describe(source));
    }

    private static string Describe(IMetalmindSource source) {
        return $"stored={source.StoredAmount:0.###} compounded={source.CompoundedAmount:0.###} " +
            $"max={source.MaxAmount:0.###} free={source.FreeSpace:0.###} " +
            $"canStore={source.CanStore} canTap={source.CanTap} burnedOut={source.IsBurnedOut}";
    }

    private static string Describe(Pawn pawn) {
        List<string> labels = [];
        List<Verse.Thing>? items = pawn.inventory?.innerContainer.InnerListForReading;
        for (int i = 0; items != null && i < items.Count; i++) {
            Metalmind? comp = items[i].TryGetComp<Metalmind>();
            if (comp != null) labels.Add("carried " + (comp.Metal?.defName ?? "unknown"));
        }

        labels.AddRange(ImplantsOf(pawn).Select(d => "implanted " + d.metalDefName));

        return "metalminds on them: " + (labels.Count == 0 ? "(none)" : string.Join(", ", labels));
    }

    /// <summary>Which metalminds each pawn's following transfers reach, keyed by thingIDNumber.</summary>
    private sealed class Aim {
        public Dictionary<int, string> ByPawn { get; } = [];
    }

    /// <summary>What the last transfer actually moved, as opposed to what it was offered.</summary>
    private sealed class Moved {
        public Moved(float amount) {
            Amount = amount;
        }

        public float Amount { get; }
    }
}
