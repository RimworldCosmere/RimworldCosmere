using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     One skill as it stood before the spikes came out.
/// </summary>
public class KandraSkillMemory : IExposable {
    public SkillDef? skill;
    public int level;
    public float xp;
    public Passion passion;

    public KandraSkillMemory() { }

    public static KandraSkillMemory From(SkillRecord record) {
        return new KandraSkillMemory {
            skill = record.def,
            level = record.Level,
            xp = record.xpSinceLastLevel,
            passion = record.passion,
        };
    }

    public void ExposeData() {
        Scribe_Defs.Look(ref skill, "skill");
        Scribe_Values.Look(ref level, "level");
        Scribe_Values.Look(ref xp, "xp");
        Scribe_Values.Look(ref passion, "passion");
    }
}

/// <summary>
///     Everything a kandra loses when its spikes come out, kept so it can be handed back.
/// </summary>
/// <remarks>
///     The spikes hold the mind in place; they do not contain it. Pull them and what the kandra
///     knew stops being reachable rather than stops existing, which is why driving a fresh pair
///     in gives the same person back rather than a blank one. Mechanically that means a snapshot
///     taken on the way down and replayed on the way up.
/// </remarks>
public class KandraMind : IExposable {
    private List<KandraSkillMemory> skills = [];
    private List<Thought_Memory> memories = [];
    private bool held;

    public bool Held => held;

    /// <summary>
    ///     Takes the snapshot, once. A kandra that loses its second spike after already losing
    ///     the first must not overwrite the good copy with the half-wrecked one.
    /// </summary>
    public void Store(Pawn pawn) {
        if (held) return;

        skills = [];
        if (pawn.skills?.skills != null) {
            for (int i = 0; i < pawn.skills.skills.Count; i++) {
                skills.Add(KandraSkillMemory.From(pawn.skills.skills[i]));
            }
        }

        memories = [];
        List<Thought_Memory>? current = pawn.needs?.mood?.thoughts?.memories?.Memories;
        if (current != null) memories.AddRange(current);

        held = true;
    }

    /// <summary>Empties the kandra out. Everything taken is already in the snapshot.</summary>
    public static void Clear(Pawn pawn, bool alsoSkills) {
        pawn.needs?.mood?.thoughts?.memories?.Memories?.Clear();

        if (!alsoSkills || pawn.skills?.skills == null) return;

        for (int i = 0; i < pawn.skills.skills.Count; i++) {
            pawn.skills.skills[i].Level = 0;
            pawn.skills.skills[i].xpSinceLastLevel = 0f;
            pawn.skills.skills[i].passion = Passion.None;
        }
    }

    /// <summary>Hands it all back and forgets the snapshot.</summary>
    public void Restore(Pawn pawn) {
        if (!held) return;

        if (pawn.skills?.skills != null) {
            for (int i = 0; i < skills.Count; i++) {
                if (skills[i].skill == null) continue;

                SkillRecord? record = pawn.skills.GetSkill(skills[i].skill);
                if (record == null) continue;

                record.Level = skills[i].level;
                record.xpSinceLastLevel = skills[i].xp;
                record.passion = skills[i].passion;
            }
        }

        MemoryThoughtHandler? handler = pawn.needs?.mood?.thoughts?.memories;
        if (handler != null) {
            for (int i = 0; i < memories.Count; i++) {
                if (memories[i] == null) continue;

                handler.TryGainMemory(memories[i]);
            }
        }

        skills = [];
        memories = [];
        held = false;
    }

    public void ExposeData() {
        Scribe_Values.Look(ref held, "held");
        Scribe_Collections.Look(ref skills, "skills", LookMode.Deep);
        Scribe_Collections.Look(ref memories, "memories", LookMode.Deep);
        skills ??= [];
        memories ??= [];
    }
}
