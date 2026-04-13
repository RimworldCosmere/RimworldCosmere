using System.Reflection;
using System.Text;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding;
using LudeonTK;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Dev;

public static class IdealDebugCommands {
    [DebugAction(
        "Cosmere/Roshar/Ideals",
        "Set Ideal Level",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SetIdealLevel(Pawn pawn) {
        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            Messages.Message("Not a Surgebinder", MessageTypeDefOf.RejectInput);
            return;
        }

        List<DebugMenuOption> options = [];
        for (int i = 0; i <= 4; i++) {
            int level = i;
            options.Add(new DebugMenuOption($"Ideal {level + 1} (index {level})", DebugMenuOptionMode.Action, () => {
                surgebinder.currentIdeal = level;
                Messages.Message($"Set {pawn.NameShortColored} to Ideal {level + 1}", MessageTypeDefOf.SilentInput);
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    [DebugAction(
        "Cosmere/Roshar/Ideals",
        "Set Record Value",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SetRecordValue(Pawn pawn) {
        List<DebugMenuOption> recordOptions = [];
        FieldInfo[] fields = typeof(RecordDefOf).GetFields(BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < fields.Length; i++) {
            RecordDef? recordDef = fields[i].GetValue(null) as RecordDef;
            if (recordDef == null) continue;
            RecordDef capturedDef = recordDef;
            recordOptions.Add(new DebugMenuOption(recordDef.defName, DebugMenuOptionMode.Action, () => {
                ShowValuePicker(pawn, capturedDef);
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(recordOptions));
    }

    private static void ShowValuePicker(Pawn pawn, RecordDef recordDef) {
        int[] values = [0, 1, 3, 5, 10, 15, 20, 25, 30, 50, 100];
        List<DebugMenuOption> valueOptions = [];
        for (int i = 0; i < values.Length; i++) {
            int value = values[i];
            valueOptions.Add(new DebugMenuOption($"{value}", DebugMenuOptionMode.Action, () => {
                float current = pawn.records.GetValue(recordDef);
                float delta = value - current;
                pawn.records.AddTo(recordDef, delta);
                Messages.Message($"Set {recordDef.defName} to {value} on {pawn.NameShortColored}", MessageTypeDefOf.SilentInput);
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(valueOptions));
    }

    [DebugAction(
        "Cosmere/Roshar/Ideals",
        "Trigger Oath Notification",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void TriggerOathNotification(Pawn pawn) {
        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            Messages.Message("Not a Surgebinder", MessageTypeDefOf.RejectInput);
            return;
        }
        surgebinder.DebugTriggerOath();
    }

    [DebugAction(
        "Cosmere/Roshar/Ideals",
        "Speak Oath Now",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SpeakOathNow(Pawn pawn) {
        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            Messages.Message("Not a Surgebinder", MessageTypeDefOf.RejectInput);
            return;
        }
        surgebinder.DebugSpeakOathNow();
    }

    [DebugAction(
        "Cosmere/Roshar/Ideals",
        "Add Bond Strain",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void AddBondStrain(Pawn pawn) {
        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            Messages.Message("Not a Surgebinder", MessageTypeDefOf.RejectInput);
            return;
        }

        List<DebugMenuOption> options = [
            new DebugMenuOption("Minor (+0.1)", DebugMenuOptionMode.Action, () => ViolationUtility.ApplyViolation(pawn, 0.1f, "debug command")),
            new DebugMenuOption("Major (+0.3)", DebugMenuOptionMode.Action, () => ViolationUtility.ApplyViolation(pawn, 0.3f, "debug command")),
            new DebugMenuOption("Catastrophic (+0.6)", DebugMenuOptionMode.Action, () => ViolationUtility.ApplyViolation(pawn, 0.6f, "debug command")),
        ];
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    [DebugAction(
        "Cosmere/Roshar/Ideals",
        "Set Fury Level",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void SetFuryLevel(Pawn pawn) {
        Need.Fury? fury = pawn.needs?.TryGetNeed(NeedDefOf.Cosmere_Roshar_Need_Fury) as Need.Fury;
        if (fury == null) {
            Messages.Message("Not a Dustbringer or no Fury need", MessageTypeDefOf.RejectInput);
            return;
        }

        float[] levels = [0f, 0.3f, 0.5f, 0.7f, 0.85f, 0.95f, 1.0f];
        List<DebugMenuOption> options = [];
        for (int i = 0; i < levels.Length; i++) {
            float level = levels[i];
            options.Add(new DebugMenuOption($"{level:F2}", DebugMenuOptionMode.Action, () => {
                fury.CurLevel = level;
                Messages.Message($"Set Fury to {level:F2} on {pawn.NameShortColored}", MessageTypeDefOf.SilentInput);
                fury.CheckFuryBreak();
            }));
        }
        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    [DebugAction(
        "Cosmere/Roshar/Ideals",
        "Show Ideal Info",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void ShowIdealInfo(Pawn pawn) {
        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            Messages.Message("Not a Surgebinder", MessageTypeDefOf.RejectInput);
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine($"=== Ideal Info: {pawn.NameShortColored} ===");
        sb.AppendLine($"Order: {surgebinder.radiantOrderDef.LabelCap}");
        sb.AppendLine($"Current Ideal: {surgebinder.currentIdealDisplay} (index {surgebinder.currentIdeal})");
        sb.AppendLine($"Pending Oath: {surgebinder.PendingOath}");
        sb.AppendLine($"Surgebinding Skill: {pawn.skills.GetSkill(SkillDefOf.Cosmere_Roshar_Skill_SurgebindingPower).Level}");
        sb.AppendLine($"Last Ideal Change Tick: {surgebinder.LastIdealChangeTick}");

        if (surgebinder.currentIdeal < 4) {
            int nextLevel = surgebinder.currentIdeal + 1;
            bool satisfied = surgebinder.radiantOrderDef.idealChecker.IsSatisfied(pawn, surgebinder, nextLevel);
            sb.AppendLine($"IsSatisfied for level {nextLevel}: {satisfied}");
        }

        sb.AppendLine("--- Records ---");
        FieldInfo[] fields = typeof(RecordDefOf).GetFields(BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < fields.Length; i++) {
            RecordDef? recordDef = fields[i].GetValue(null) as RecordDef;
            if (recordDef == null) continue;
            float value = pawn.records.GetValue(recordDef);
            if (value > 0) sb.AppendLine($"  {recordDef.defName}: {value}");
        }

        Verse.Hediff? stainedBond = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Roshar_Hediff_StrainedBond);
        if (stainedBond != null) sb.AppendLine($"Bond Strain Severity: {stainedBond.Severity:F2}");

        Need.Fury? fury = pawn.needs?.TryGetNeed(NeedDefOf.Cosmere_Roshar_Need_Fury) as Need.Fury;
        if (fury != null) sb.AppendLine($"Fury Level: {fury.CurLevel:F2}");

        Logger.Verbose(sb.ToString());
    }

    [DebugAction(
        "Cosmere/Roshar/Ideals",
        "Reset Cooldown",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void ResetCooldown(Pawn pawn) {
        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            Messages.Message("Not a Surgebinder", MessageTypeDefOf.RejectInput);
            return;
        }
        surgebinder.DebugResetCooldown();
        Messages.Message($"Reset cooldown for {pawn.NameShortColored}", MessageTypeDefOf.SilentInput);
    }
}
