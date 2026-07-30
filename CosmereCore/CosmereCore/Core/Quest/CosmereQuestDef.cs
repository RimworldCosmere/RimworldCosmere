using System.Collections.Generic;
using Cosmere.Core.Quest.Outcome;
using Cosmere.Core.Quest.Prereq;
using Cosmere.Core.Quest.Reward;
using RimWorld;

namespace Cosmere.Core.Quest;

/// <summary>
///     A quest described as data. Shaped after ScenarioProgressionDef so the two read alike:
///     polymorphic Class= lists for prerequisites, objectives, rewards and outcomes.
///     CosmereQuestBuilder turns one of these into a real RimWorld.Quest.
/// </summary>
public class CosmereQuestDef : Verse.Def {
    public int challengeRating = 1;
    public int cooldownDays;
    public ScadrialEra era = ScadrialEra.Any;
    public int expireAfterDays = 10;
    public FactionDef? giverFaction;
    public QuestKind kind = QuestKind.Repeatable;
    public int minColonists;
    public int minDaysElapsed;
    public QuestOutcome? onFailure;
    public List<QuestPrereq>? prerequisites;
    public string? requiredCapstone;
    public List<string>? requiredFlags;
    public List<string>? requiredShards;
    public List<QuestReward>? rewards;
    public float selectionWeight = 1f;
    public List<CosmereQuestStage>? stages;
    public QuestSubject subject = QuestSubject.Colony;
    public FactionDef? targetFaction;

    /// <summary>Projects this def onto the plain shape CosmereQuestEligibility reads.</summary>
    public QuestCandidate ToCandidate() {
        return new QuestCandidate {
            defName = defName,
            kind = kind,
            era = era,
            requiredShards = requiredShards,
            requiredFlags = requiredFlags,
            requiredCapstone = requiredCapstone,
            minColonists = minColonists,
            minDaysElapsed = minDaysElapsed,
            cooldownDays = cooldownDays,
            selectionWeight = selectionWeight,
        };
    }

    public override IEnumerable<string> ConfigErrors() {
        foreach (string error in base.ConfigErrors()) {
            yield return error;
        }

        if (stages == null || stages.Count == 0) {
            yield return $"{defName}: has no stages.";
        }

        if (rewards == null || rewards.Count == 0) {
            yield return $"{defName}: has no rewards.";
        }

        if (kind == QuestKind.Capstone && onFailure == null) {
            yield return $"{defName}: is a Capstone but declares no onFailure. Use " +
                         "Cosmere.Core.Quest.Outcome.NeverBurns if it genuinely cannot fail.";
        }

        if (expireAfterDays <= 0 && kind == QuestKind.Repeatable) {
            yield return $"{defName}: repeatable quests need a positive expireAfterDays.";
        }

        if (stages != null) {
            HashSet<string> seenKeys = new HashSet<string>();
            for (int i = 0; i < stages.Count; i++) {
                CosmereQuestStage stage = stages[i];
                string? key = stage.key;
                if (key == null || key.Length == 0) {
                    yield return $"{defName}: stage {i} has no key.";
                } else if (!seenKeys.Add(key)) {
                    yield return $"{defName}: duplicate stage key '{key}'.";
                }

                if (stage.objective == null) {
                    yield return $"{defName}: stage '{stage.key}' has no objective.";
                    continue;
                }

                string? objectiveError = stage.objective.ConfigError();
                if (objectiveError != null) {
                    yield return $"{defName}: stage '{stage.key}': {objectiveError}";
                }
            }
        }

        if (prerequisites != null) {
            for (int i = 0; i < prerequisites.Count; i++) {
                string? error = prerequisites[i].ConfigError();
                if (error != null) yield return $"{defName}: prerequisite {i}: {error}";
            }
        }

        if (rewards != null) {
            for (int i = 0; i < rewards.Count; i++) {
                string? error = rewards[i].ConfigError();
                if (error != null) yield return $"{defName}: reward {i}: {error}";
            }
        }

        if (onFailure != null) {
            string? error = onFailure.ConfigError();
            if (error != null) yield return $"{defName}: onFailure: {error}";
        }
    }
}
