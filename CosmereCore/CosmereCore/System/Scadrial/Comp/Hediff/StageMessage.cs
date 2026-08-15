using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Comp.Hediff;

public class StageMessageProperties : HediffCompProperties {
    /// <summary>A language key, not a sentence.</summary>
    public string? message;

    public MessageTypeDef? messageType;

    public StageMessageProperties() {
        compClass = typeof(StageMessage);
    }
}

/// <summary>
///     Says something when a hediff moves up a stage.
/// </summary>
/// <remarks>
///     Vanilla's HediffComp_MessageStageIncreased does <c>Props.message.Formatted(Pawn)</c> and
///     never translates, so its one use in the game puts a literal English sentence in the def.
///     Handing it a key prints the key on screen. This does the same job through the language
///     files, which is the rule everywhere else in the mod.
///     <para>
///         Not a subclass of vanilla's, because the stage it last spoke about is private there and
///         the whole body would have to be duplicated to reach it anyway.
///     </para>
/// </remarks>
public class StageMessage : HediffComp {
    private int lastSpokenOf = -1;

    private StageMessageProperties Props => (StageMessageProperties)props;

    public override void CompPostTickInterval(ref float severityAdjustment, int delta) {
        base.CompPostTickInterval(ref severityAdjustment, delta);

        if (Props.message == null || parent.CurStageIndex <= lastSpokenOf) return;

        // Set before the message, so a stage that somehow fails to announce is not announced
        // twice a tick later for the rest of the game.
        lastSpokenOf = parent.CurStageIndex;

        // Nothing to say the first time. Every koloss is born into its first stage, and a message
        // about that is a message about existing.
        if (parent.CurStageIndex == 0) return;

        Messages.Message(
            Props.message.Translate(
                Pawn.LabelShortCap.Named("PAWN"),
                parent.CurStage?.label?.CapitalizeFirst().Named("STAGE") ?? string.Empty.Named("STAGE")
            ),
            Pawn,
            Props.messageType ?? MessageTypeDefOf.NeutralEvent
        );
    }

    public override void CompExposeData() {
        base.CompExposeData();
        Scribe_Values.Look(ref lastSpokenOf, "lastSpokenOf", -1);
    }
}
