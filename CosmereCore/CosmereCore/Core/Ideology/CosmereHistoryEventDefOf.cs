using RimWorld;
using Verse;

namespace Cosmere.Core.Ideology;

[DefOf]
public static class CosmereHistoryEventDefOf {
    // Hemalurgy events
    public static HistoryEventDef? Cosmere_ReceivedHemalurgicSpike;
    public static HistoryEventDef? Cosmere_PerformedHemalurgy;

    // Oath events
    public static HistoryEventDef? Cosmere_SworeIdeal;
    public static HistoryEventDef? Cosmere_BrokeOath;

    // Spren bond events
    public static HistoryEventDef? Cosmere_BondedSpren;

    // Soulcasting events
    public static HistoryEventDef? Cosmere_PerformedSoulcasting;
    public static HistoryEventDef? Cosmere_SoulcastLivingBeing;

    // Stormlight events
    public static HistoryEventDef? Cosmere_SharedStormlight;
    public static HistoryEventDef? Cosmere_HoardedStormlight;

    // Investiture events
    public static HistoryEventDef? Cosmere_UsedInvestiture;

    // Knowledge events
    public static HistoryEventDef? Cosmere_CompletedResearch;
    public static HistoryEventDef? Cosmere_TaughtSkill;

    // Gemheart events
    public static HistoryEventDef? Cosmere_HarvestedGemheart;

    static CosmereHistoryEventDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(CosmereHistoryEventDefOf));
    }
}
