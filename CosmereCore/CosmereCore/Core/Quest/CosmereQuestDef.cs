using System.Collections.Generic;
using Cosmere.Core.Def;
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
    public List<string>? eras;
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
            eras = eras,
            requiredShards = requiredShards,
            requiredFlags = requiredFlags,
            requiredCapstone = requiredCapstone,
            minColonists = minColonists,
            minDaysElapsed = minDaysElapsed,
            cooldownDays = cooldownDays,
            selectionWeight = selectionWeight,
            targetFaction = targetFaction?.defName,
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

        if (eras != null) {
            for (int i = 0; i < eras.Count; i++) {
                string? name = eras[i];
                if (name == null || name.Length == 0) {
                    yield return $"{defName}: eras[{i}] is empty.";
                    continue;
                }

                if (Verse.DefDatabase<EraDef>.GetNamedSilentFail(name) == null) {
                    yield return $"{defName}: era '{name}' does not resolve.";
                }
            }
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

        foreach (string error in BranchErrors()) {
            yield return error;
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

    /// <summary>
    ///     An afterChoice that names no real option silently never runs, which looks like a
    ///     dead stage or a reward that vanished. Catch it at load instead.
    /// </summary>
    private IEnumerable<string> BranchErrors() {
        HashSet<string> choiceKeys = new HashSet<string>();
        if (stages != null) {
            for (int i = 0; i < stages.Count; i++) {
                if (stages[i].objective is not Objective.ChoiceObjective choice) continue;

                List<Objective.QuestChoiceOption>? options = choice.options;
                if (options == null) continue;
                for (int j = 0; j < options.Count; j++) {
                    string? key = options[j].key;
                    if (key != null && key.Length > 0) choiceKeys.Add(key);
                }
            }
        }

        if (stages != null) {
            for (int i = 0; i < stages.Count; i++) {
                string? branch = stages[i].afterChoice;
                if (branch == null || branch.Length == 0) continue;
                if (!choiceKeys.Contains(branch)) {
                    yield return $"{defName}: stage '{stages[i].key}' is tagged afterChoice " +
                                 $"'{branch}', which no ChoiceObjective option declares.";
                }
            }
        }

        if (rewards != null) {
            for (int i = 0; i < rewards.Count; i++) {
                string? branch = rewards[i].afterChoice;
                if (branch == null || branch.Length == 0) continue;
                if (!choiceKeys.Contains(branch)) {
                    yield return $"{defName}: reward {i} is tagged afterChoice '{branch}', " +
                                 "which no ChoiceObjective option declares.";
                }
            }
        }
    }
}
