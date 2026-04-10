using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public abstract class ProgressionAction {
    public abstract void Execute(GameComponent_ScenarioProgression comp);
}

public class SendLetterAction : ProgressionAction {
    public string title = "";
    public string text = "";
    public string? letterDef;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        LetterDef def = LetterDefOf.NeutralEvent;
        if (letterDef != null) {
            LetterDef? resolved = DefDatabase<LetterDef>.GetNamedSilentFail(letterDef);
            if (resolved != null) def = resolved;
        }

        Find.LetterStack.ReceiveLetter(title, text, def);
    }
}

public class AddGeneAction : ProgressionAction {
    public string pawnName = "";
    public string gene = "";
    public bool xenogene = true;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for AddGene");
            return;
        }

        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(gene);
        if (geneDef == null) {
            Logger.Warning($"ScenarioProgression: Gene '{gene}' not found");
            return;
        }

        if (pawn.genes != null && !pawn.genes.HasActiveGene(geneDef)) {
            pawn.genes.AddGene(geneDef, xenogene);
        }
    }
}

public class RemoveGeneAction : ProgressionAction {
    public string pawnName = "";
    public string gene = "";

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for RemoveGene");
            return;
        }

        GeneDef? geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(gene);
        if (geneDef == null) {
            Logger.Warning($"ScenarioProgression: Gene '{gene}' not found");
            return;
        }

        if (pawn.genes == null) return;

        Verse.Gene? activeGene = pawn.genes.GetGene(geneDef);
        if (activeGene != null) {
            pawn.genes.RemoveGene(activeGene);
        }
    }
}

public class AdvanceIdealAction : ProgressionAction {
    public string pawnName = "";
    public int targetIdeal = -1;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for AdvanceIdeal");
            return;
        }

        Cosmere.System.Roshar.Gene.Surgebinder? surgebinder =
            pawn.genes?.GetFirstGeneOfType<Cosmere.System.Roshar.Gene.Surgebinder>();
        if (surgebinder == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' has no Surgebinder gene");
            return;
        }

        int target = targetIdeal >= 0 ? targetIdeal : surgebinder.currentIdeal + 1;
        if (target > 4) target = 4;

        surgebinder.currentIdeal = target;
    }
}

public class SetSkillAction : ProgressionAction {
    public string pawnName = "";
    public string skill = "";
    public int level = -1;
    public string? passion;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for SetSkill");
            return;
        }

        SkillDef? skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skill);
        if (skillDef == null) {
            Logger.Warning($"ScenarioProgression: Skill '{skill}' not found");
            return;
        }

        SkillRecord? record = pawn.skills?.GetSkill(skillDef);
        if (record == null) return;

        if (level >= 0) record.Level = level;
        if (passion != null) {
            record.passion = (Passion)ParseHelper.FromString(passion, typeof(Passion));
        }
    }
}

public class AddTraitAction : ProgressionAction {
    public string pawnName = "";
    public string trait = "";
    public int degree;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for AddTrait");
            return;
        }

        TraitDef? traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(trait);
        if (traitDef == null) {
            Logger.Warning($"ScenarioProgression: Trait '{trait}' not found");
            return;
        }

        pawn.story?.traits?.GainTrait(new Trait(traitDef, degree));
    }
}

public class RemovePawnAction : ProgressionAction {
    public string pawnName = "";
    public string method = "vanish";
    public string? letterTitle;
    public string? letterText;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for RemovePawn");
            return;
        }

        if (letterTitle != null && letterText != null) {
            LetterDef letterDef = method == "death" ? LetterDefOf.Death : LetterDefOf.NegativeEvent;
            Find.LetterStack.ReceiveLetter(letterTitle, letterText, letterDef, pawn);
        }

        switch (method) {
            case "death":
                pawn.Kill(null);
                break;
            case "leave":
                pawn.DeSpawn();
                Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.Decide);
                break;
            default:
                pawn.DeSpawn();
                Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.Discard);
                break;
        }
    }
}

public class SpawnItemAction : ProgressionAction {
    public string thing = "";
    public string? stuff;
    public int count = 1;
    public string? letterTitle;
    public string? letterText;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        ThingDef? thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(thing);
        if (thingDef == null) {
            Logger.Warning($"ScenarioProgression: Thing '{thing}' not found for SpawnItem");
            return;
        }

        ThingDef? stuffDef = null;
        if (stuff != null) {
            stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(stuff);
        }

        Map? map = Find.CurrentMap;
        if (map == null) return;

        Verse.Thing item = ThingMaker.MakeThing(thingDef, stuffDef);
        item.stackCount = count;

        IntVec3 dropSpot = DropCellFinder.TradeDropSpot(map);
        DropPodUtility.DropThingsNear(dropSpot, map, [item], 110, false, true);

        if (letterTitle != null && letterText != null) {
            Find.LetterStack.ReceiveLetter(letterTitle, letterText, LetterDefOf.PositiveEvent,
                new TargetInfo(dropSpot, map));
        }
    }
}

public class SetGameConditionAction : ProgressionAction {
    public string gameCondition = "";
    public int durationDays = 1;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        GameConditionDef? def = DefDatabase<GameConditionDef>.GetNamedSilentFail(gameCondition);
        if (def == null) {
            Logger.Warning($"ScenarioProgression: GameCondition '{gameCondition}' not found");
            return;
        }

        Map? map = Find.CurrentMap;
        if (map == null) return;

        GameCondition condition = GameConditionMaker.MakeCondition(def, durationDays * GenDate.TicksPerDay);
        map.gameConditionManager.RegisterCondition(condition);
    }
}

public class TriggerIncidentAction : ProgressionAction {
    public string incident = "";

    public override void Execute(GameComponent_ScenarioProgression comp) {
        IncidentDef? def = DefDatabase<IncidentDef>.GetNamedSilentFail(incident);
        if (def == null) {
            Logger.Warning($"ScenarioProgression: Incident '{incident}' not found");
            return;
        }

        Map? map = Find.CurrentMap;
        if (map == null) return;

        IncidentParms parms = StorytellerUtility.DefaultParmsNow(def.category, map);
        def.Worker.TryExecute(parms);
    }
}
