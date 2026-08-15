using Cosmere.Core;
using Cosmere.Core.Investiture;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Game;

/// <summary>
///     One Allomancer holding one koloss, and when they took it.
/// </summary>
/// <remarks>
///     The tick is what makes "drop the most recent" mean something. When a holder runs out of
///     metal, the koloss they seized a minute ago goes before the one they have marched across
///     three maps.
/// </remarks>
public class KolossBond : IExposable {
    public Pawn? holder;
    public Pawn? koloss;
    public int boundAtTick;

    /// <summary>Which metal took it, because that one running out ends the hold.</summary>
    public string? metal;

    /// <summary>The push that took it, which is also what the upkeep is measured against.</summary>
    public string? ability;

    public KolossBond() { }

    public KolossBond(Pawn holder, Pawn koloss, int tick, string? metal, string? ability) {
        this.holder = holder;
        this.koloss = koloss;
        boundAtTick = tick;
        this.metal = metal;
        this.ability = ability;
    }

    public bool Intact => holder is { Dead: false } && koloss is { Dead: false };

    public void ExposeData() {
        Scribe_References.Look(ref holder, "holder");
        Scribe_References.Look(ref koloss, "koloss");
        Scribe_Values.Look(ref boundAtTick, "boundAtTick");
        Scribe_Values.Look(ref metal, "metal");
        Scribe_Values.Look(ref ability, "ability");
    }
}

/// <summary>
///     Who is holding which koloss, and what it costs them to keep doing it.
/// </summary>
/// <remarks>
///     A hold is a roster entry, not a radius. Distance never enters into it - the Lord Ruler's
///     koloss stayed his across the whole empire, and an earlier design that leashed a koloss to
///     twelve tiles around its holder had to disable hauling to stop the thing snapping on its
///     first job.
///     <para>
///         What limits an army is capacity and metal. Slots come off AllomanticPower, and every
///         koloss on the roster bills its holder every interval. A holder who cannot pay loses the
///         most recently bound one first, which is the reading that keeps a long-standing army
///         stable while a greedy seizure is the thing that fails.
///     </para>
///     <para>
///         Flat list rather than a dictionary of lists because it scribes without a rebuild step,
///         it keeps its order so "most recent" is a real question, and both directions are one
///         scan of something that never gets long.
///     </para>
/// </remarks>
public class KolossRoster : GameComponent {
    /// <summary>
    ///     What holding costs against what seizing cost, per koloss.
    /// </summary>
    /// <remarks>
    ///     Taking hold is the hard part and keeping hold is not. A tenth means an Allomancer can
    ///     seize far fewer than they can carry, which is the shape the whole mechanic wants.
    /// </remarks>
    public const float HoldFraction = 0.1f;

    /// <summary>How often the roster re-reads itself. The metal is charged by the gene.</summary>
    public const int BillingInterval = 250;

    /// <summary>Slots the weakest Allomancer gets. Everyone who can Soothe at all can hold one.</summary>
    private const int BaseSlots = 1;

    /// <summary>Allomantic Power that buys one more slot.</summary>
    private const float PowerPerSlot = 1.5f;

    private List<KolossBond> bonds = [];

    public KolossRoster(Verse.Game game) { }

    public static KolossRoster? Current => Verse.Current.Game?.GetComponent<KolossRoster>();

    public override void ExposeData() {
        Scribe_Collections.Look(ref bonds, "bonds", LookMode.Deep);
        bonds ??= [];
    }

    /// <summary>How many koloss this Allomancer can keep hold of at once.</summary>
    public static int CapacityOf(Pawn? holder) {
        if (holder == null) return 0;

        float power = holder.GetStatValue(StatDefOf.Cosmere_Scadrial_Stat_AllomanticPower);

        return BaseSlots + Mathf.FloorToInt(Mathf.Max(0f, power) / PowerPerSlot);
    }

    public int UsedBy(Pawn? holder) {
        if (holder == null) return 0;

        int used = 0;
        for (int i = 0; i < bonds.Count; i++) {
            if (bonds[i].holder == holder && bonds[i].Intact) used++;
        }

        return used;
    }

    /// <summary>The Allomancer holding this koloss, or nobody.</summary>
    public Pawn? HolderOf(Pawn? koloss) {
        if (koloss == null) return null;

        for (int i = 0; i < bonds.Count; i++) {
            if (bonds[i].koloss == koloss && bonds[i].Intact) return bonds[i].holder;
        }

        return null;
    }

    /// <summary>Everything this Allomancer is holding, oldest bond first.</summary>
    public List<Pawn> HeldBy(Pawn? holder) {
        List<Pawn> held = [];
        if (holder == null) return held;

        for (int i = 0; i < bonds.Count; i++) {
            if (bonds[i].holder == holder && bonds[i].Intact && bonds[i].koloss != null) {
                held.Add(bonds[i].koloss!);
            }
        }

        return held;
    }

    /// <summary>
    ///     Takes hold, and hands the thing over to whoever took it.
    /// </summary>
    /// <remarks>
    ///     Drafting gates on IsColonistPlayerControlled, which needs the pawn in the player
    ///     faction. Without the transfer a held koloss reported a holder and still could not be
    ///     given a single order, which is the whole point of holding one.
    /// </remarks>
    /// <summary>Everything this Allomancer holds on one particular metal.</summary>
    public List<Pawn> HeldOnMetal(Pawn? holder, Cosmere.Core.Def.MetalDef metal) {
        List<Pawn> held = [];
        if (holder == null) return held;

        for (int i = 0; i < bonds.Count; i++) {
            KolossBond bond = bonds[i];
            if (bond.holder != holder || !bond.Intact || bond.koloss == null) continue;
            if (bond.metal != metal.defName) continue;

            held.Add(bond.koloss);
        }

        return held;
    }

    public void Bind(Pawn holder, Pawn koloss, Cosmere.Core.Def.MetalDef? metal = null, Cosmere.Core.Def.AbilityDef? through = null) {
        Release(koloss);
        bonds.Add(new KolossBond(
            holder,
            koloss,
            Find.TickManager?.TicksGame ?? 0,
            metal?.defName,
            through?.defName
        ));

        if (holder.Faction != null && koloss.Faction != holder.Faction) {
            koloss.SetFaction(holder.Faction);
        }

        Enslave(koloss, holder.Faction);
    }

    /// <summary>
    ///     A held koloss works for the colony and is not one of it.
    /// </summary>
    /// <remarks>
    ///     Slave rather than colonist: it takes orders because somebody is standing on its mind,
    ///     which is what the status already means. Ideology owns slavery, so a colony without it
    ///     gets a plain faction member and the hold still works.
    /// </remarks>
    private static void Enslave(Pawn koloss, Faction? faction) {
        if (!ModsConfig.IdeologyActive || faction == null || koloss.guest == null) return;
        if (koloss.IsSlaveOfColony) return;

        koloss.guest.SetGuestStatus(faction, GuestStatus.Slave);
    }

    /// <summary>True if a bond was actually there to break.</summary>
    public bool Release(Pawn? koloss) {
        if (koloss == null) return false;

        // Back to belonging to nobody. A koloss that keeps the colony's colours while rampaging
        // through it reads as a bug rather than a loss of control.
        if (ModsConfig.IdeologyActive && koloss.guest != null && koloss.IsSlaveOfColony) {
            koloss.guest.SetGuestStatus(null);
        }

        if (koloss.Faction != null) koloss.SetFaction(null);

        bool broke = false;
        for (int i = bonds.Count - 1; i >= 0; i--) {
            if (bonds[i].koloss != koloss) continue;

            bonds.RemoveAt(i);
            broke = true;
        }

        return broke;
    }

    public override void GameComponentTick() {
        base.GameComponentTick();

        int now = Find.TickManager?.TicksGame ?? 0;
        if (now % BillingInterval != 0) return;

        DropBrokenBonds();
        Bill();
    }

    /// <summary>
    ///     A bond whose holder or koloss is dead or gone is not a hold anybody is maintaining.
    /// </summary>
    private void DropBrokenBonds() {
        for (int i = bonds.Count - 1; i >= 0; i--) {
            KolossBond bond = bonds[i];
            if (bond.Intact && bond.holder!.Spawned && bond.koloss!.Spawned) continue;

            bonds.RemoveAt(i);
        }
    }

    /// <summary>
    ///     Charges every holder for everything they are holding, and drops what they cannot afford.
    /// </summary>
    /// <remarks>
    ///     Newest first, so an army built up over a campaign survives a bad moment and the koloss
    ///     seized one second before the metal ran out is the one that goes.
    /// </remarks>
    /// <summary>
    ///     Publishes what every hold costs, and drops the ones nobody can pay for.
    /// </summary>
    /// <remarks>
    ///     The reserve is not touched here. Allomancer.BurnTickInterval already charges the sum of
    ///     its drain sources and wipes them all when it cannot, so taking the metal by hand as well
    ///     charged twice and never showed a rate - the gene read Idle at 0.00%/s while the reserve
    ///     quietly fell. Registering a source instead puts the hold on the same footing as any
    ///     other burn, and the player can see it.
    /// </remarks>
    private void Bill() {
        HashSet<Pawn> holders = [];
        for (int i = 0; i < bonds.Count; i++) {
            if (bonds[i].holder != null) holders.Add(bonds[i].holder!);
        }

        foreach (Pawn holder in holders) {
            // One source per push, summed, because DrainSource compares on its def alone and a
            // second entry for the same ability would replace the first rather than add to it.
            Dictionary<Cosmere.Core.Def.AbilityDef, float> owed = [];

            foreach (KolossBond bond in BondsOf(holder)) {
                Allomancer? gene = GeneFor(holder, bond);
                Cosmere.Core.Def.AbilityDef? through = AbilityFor(bond);

                if (gene == null || through == null || gene.Value <= gene.MinimumAmount) {
                    Drop(bond);
                    continue;
                }

                owed.TryGetValue(through, out float running);
                owed[through] = running + (through.beuPerTick * HoldFraction);
            }

            foreach ((Cosmere.Core.Def.AbilityDef through, float rate) in owed) {
                GeneFor(holder, through)?.UpdateDrainSource(new DrainSource(through, rate));
            }
        }
    }

    private static Cosmere.Core.Def.AbilityDef? AbilityFor(KolossBond bond) {
        return bond.ability == null ? null : DefDatabase<Cosmere.Core.Def.AbilityDef>.GetNamedSilentFail(bond.ability);
    }

    private static Allomancer? GeneFor(Pawn holder, Cosmere.Core.Def.AbilityDef through) {
        return through is AllomanticAbilityDef allomantic
            ? holder.genes?.GetAllomanticGeneForMetal(allomantic.metal)
            : null;
    }

    /// <summary>Newest first, so an army built over a campaign outlives a greedy seizure.</summary>
    private List<KolossBond> BondsOf(Pawn holder) {
        List<KolossBond> mine = [];
        for (int i = 0; i < bonds.Count; i++) {
            if (bonds[i].holder == holder && bonds[i].Intact) mine.Add(bonds[i]);
        }

        mine.SortByDescending(b => b.boundAtTick);

        return mine;
    }

    private static Allomancer? GeneFor(Pawn holder, KolossBond bond) {
        if (holder.genes == null) return null;

        Cosmere.Core.Def.MetalDef? metal = bond.metal == null
            ? null
            : DefDatabase<Cosmere.Core.Def.MetalDef>.GetNamedSilentFail(bond.metal);

        return metal != null ? holder.genes.GetAllomanticGeneForMetal(metal) : HoldingGene(holder);
    }

    private void Drop(KolossBond bond) {
        Pawn? lost = bond.koloss;
        Pawn? holder = bond.holder;

        // Stop billing for it before letting go, or the gene keeps paying upkeep on a hold that
        // no longer exists.
        Cosmere.Core.Def.AbilityDef? through = AbilityFor(bond);
        if (holder != null && through != null) {
            GeneFor(holder, through)?.UpdateDrainSource(new DrainSource(through, 0f));
        }

        Release(lost);
        if (lost == null || holder == null) return;

        Messages.Message(
            "CS_KolossHoldLapsed".Translate(holder.LabelShortCap.Named("HOLDER"), lost.LabelShortCap.Named("KOLOSS")),
            lost,
            MessageTypeDefOf.NegativeEvent
        );
    }

    /// <summary>
    ///     Zinc or brass, whichever this Allomancer has. Either one holds a koloss; the books do
    ///     not care which direction you push somebody's emotions to own them.
    /// </summary>
    public static Allomancer? HoldingGene(Pawn? holder) {
        if (holder?.genes == null) return null;

        // GetAllomanticGeneForMetal goes through Cosmere.Core.Def.MetalDef.GetMistingGene, which knows the real
        // names. Guessing at "Cosmere_Scadrial_Gene_Allomancy_Zinc" matched nothing - the gene is
        // called MistingZinc - so billing found no gene and dropped every bond on the next tick.
        // A hold lasted about four seconds.
        return holder.genes.GetAllomanticGeneForMetal(MetalDefOf.Zinc)
               ?? holder.genes.GetAllomanticGeneForMetal(MetalDefOf.Brass);
    }
}
