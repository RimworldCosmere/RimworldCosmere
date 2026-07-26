using Cosmere.System.Roshar.Comp.Map;
using Cosmere.System.Roshar.Dialog;
using Verse;

namespace Cosmere.System.Roshar.Incident;

public class ChoiceLetter_GemheartHunt : ChoiceLetter {
    public Map map = null!;

    public override bool CanDismissWithRightClick => false;

    public override IEnumerable<DiaOption> Choices {
        get {
            if (ArchivedOnly) {
                yield return Option_Close;
                yield break;
            }

            DiaOption sendExpedition = new DiaOption("Send Expedition") {
                action = delegate {
                    Find.WindowStack.Add(new Dialog_GemheartExpedition(map));
                    Find.LetterStack.RemoveLetter(this);
                },
                resolveTree = true,
            };

            if (map.mapPawns.FreeColonistsCount < 3) {
                sendExpedition.Disable("Not enough colonists (minimum 3)");
            }

            GemheartExpeditionManager? manager = map.GetComponent<GemheartExpeditionManager>();
            if (manager is { ExpeditionActive: true }) {
                sendExpedition.Disable("An expedition is already in progress");
            }

            yield return sendExpedition;

            yield return new DiaOption("Ignore") {
                action = delegate { Find.LetterStack.RemoveLetter(this); },
                resolveTree = true,
            };
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref map, "map");
    }
}