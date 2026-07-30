using System.Collections.Generic;
using Cosmere.Core.Quest.Objective;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.Core.Quest;

/// <summary>
///     The only place that constructs a RimWorld.Quest from a CosmereQuestDef. Builds the
///     quest fully in a local variable and never touches Find.QuestManager - the caller
///     commits it to the game only when TryBuild returns true, so a half-built quest can
///     never leak into play.
/// </summary>
public static class CosmereQuestBuilder {
    /// <summary>
    ///     Builds def into a ready-to-accept quest. Returns false and sets quest to null if
    ///     any stage's objective cannot resolve. The caller must not add quest to
    ///     Find.QuestManager unless this returns true.
    /// </summary>
    public static bool TryBuild(CosmereQuestDef def, Verse.Map map, Pawn? subject, out RimWorld.Quest? quest) {
        RimWorld.Quest built = RimWorld.Quest.MakeRaw();
        built.root = DefDatabase<QuestScriptDef>.GetNamed("Cosmere_Quest_Root");
        if (built.root == null) {
            Logger.Warning($"{def.defName}: quest build aborted - Cosmere_Quest_Root marker def is missing");
            quest = null;
            return false;
        }

        built.name = def.LabelCap;
        built.description = def.description;
        built.challengeRating = def.challengeRating;
        long expireOffsetTicks = global::System.Math.Min((long)def.expireAfterDays * GenDate.TicksPerDay, int.MaxValue - Find.TickManager.TicksGame);
        built.acceptanceExpireTick = Find.TickManager.TicksGame + (int)expireOffsetTicks;

        try {
            BuildStages(built, def, map, subject);
        } catch (QuestBuildFailure failure) {
            Logger.Warning($"{def.defName}: quest build aborted - {failure.Message}");
            quest = null;
            return false;
        }

        built.SetNotYetAccepted();
        quest = built;
        return true;
    }

    private static void BuildStages(RimWorld.Quest quest, CosmereQuestDef def, Verse.Map map, Pawn? subject) {
        List<CosmereQuestStage>? stages = def.stages;
        if (stages == null || stages.Count == 0) {
            throw new QuestBuildFailure("no stages to build");
        }

        QuestBuildContext ctx = new QuestBuildContext {
            def = def,
            map = map,
            quest = quest,
            subject = subject,
            rewardSeed = Rand.Int,
        };

        string previousOutSignal = quest.InitiateSignal;
        for (int i = 0; i < stages.Count; i++) {
            CosmereQuestStage stage = stages[i];
            QuestObjective? objective = stage.objective;
            if (objective == null) {
                throw new QuestBuildFailure($"stage '{stage.key}' has no objective");
            }

            string outSignal = SignalFor(quest, i);
            objective.AddParts(quest, previousOutSignal, outSignal, ctx);
            previousOutSignal = outSignal;
        }

        string successSignal = previousOutSignal;
        string failSignal = $"{quest.GetUniqueLoadID()}.failed";

        // Objectives build their own QuestPart_CosmereActivable pollers during the loop above,
        // so failSignal can only be assigned after the fact by walking what they added.
        List<QuestPart> parts = quest.PartsListForReading;
        for (int i = 0; i < parts.Count; i++) {
            if (parts[i] is QuestPart_CosmereActivable activable) {
                activable.failSignal = failSignal;
            }
        }

        // The reward part must be added before the success QuestPart_QuestEnd: Quest.
        // Notify_SignalReceived evaluates each part's signalListenMode against the quest's
        // *current* State as it walks the parts list in order, and QuestPart_QuestEnd.
        // Notify_QuestSignalReceived calls Quest.End() synchronously, which flips State away
        // from Ongoing. A part added after QuestPart_QuestEnd would find the quest already
        // ended and never fire.
        quest.AddPart(new QuestPart_CosmereReward {
            quest = quest,
            inSignal = successSignal,
            def = def,
            map = map,
            subject = subject,
            rewardSeed = ctx.rewardSeed,
        });

        QuestPart_QuestEnd questEnd = new QuestPart_QuestEnd {
            quest = quest,
            inSignal = successSignal,
            outcome = QuestEndOutcome.Success,
            sendLetter = true,
            playSound = true,
        };
        quest.AddPart(questEnd);

        // Same ordering rule applies to the failure branch: the outcome resolver has to run
        // before the failure QuestPart_QuestEnd ends the quest.
        quest.AddPart(new QuestPart_CosmereOutcome {
            quest = quest,
            inSignal = failSignal,
            def = def,
            map = map,
            subject = subject,
            rewardSeed = ctx.rewardSeed,
        });

        QuestPart_QuestEnd failEnd = new QuestPart_QuestEnd {
            quest = quest,
            inSignal = failSignal,
            outcome = QuestEndOutcome.Fail,
            sendLetter = true,
            playSound = true,
        };
        quest.AddPart(failEnd);
    }

    /// <summary>
    ///     A signal name unique to this quest instance and this stage index. Quest.id is
    ///     assigned once in MakeRaw and never reused, so no two live quests can collide.
    /// </summary>
    private static string SignalFor(RimWorld.Quest quest, int stageIndex) {
        return $"{quest.GetUniqueLoadID()}.stage{stageIndex}";
    }
}
