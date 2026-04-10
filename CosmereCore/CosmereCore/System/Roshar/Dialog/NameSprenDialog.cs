using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Dialog;

public class NameSprenDialog : Window {
    private readonly Verse.Pawn spren;
    private string curName;

    public override Vector2 InitialSize => new Vector2(400f, 200f);

    public NameSprenDialog(Verse.Pawn spren) {
        this.spren = spren;
        curName = spren.Name is NameSingle single ? single.Name : spren.LabelShort;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;
        closeOnAccept = false;
    }

    public override void DoWindowContents(Rect inRect) {
        bool enterPressed = Event.current.type == EventType.KeyDown
            && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

        if (enterPressed) {
            Event.current.Use();
        }

        float y = inRect.y;

        using (new TextBlock(GameFont.Medium)) {
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 30f), "CRO_NameSpren_Title".Translate());
            y += 35f;
        }

        using (new TextBlock(GameFont.Small)) {
            string desc = spren.kindDef.LabelCap;
            Widgets.Label(new Rect(inRect.x, y, inRect.width, 24f), desc);
            y += 30f;

            Rect nameRect = new Rect(inRect.x, y, inRect.width, 30f);
            GUI.SetNextControlName("SprenName");
            curName = Widgets.TextField(nameRect, curName, 16);
            y += 40f;

            float btnWidth = (inRect.width - 10f) / 2f;
            if (Widgets.ButtonText(new Rect(inRect.x, y, btnWidth, 35f), "Cancel".Translate()) ) {
                Close();
            }
            if (Widgets.ButtonText(new Rect(inRect.x + btnWidth + 10f, y, btnWidth, 35f), "Accept".Translate()) || enterPressed) {
                if (curName.NullOrEmpty()) {
                    Messages.Message("NameInvalid".Translate(), spren, MessageTypeDefOf.NeutralEvent, false);
                } else {
                    spren.Name = new NameSingle(curName.Trim());
                    Messages.Message(
                        "CRO_NameSpren_Named".Translate(curName.Trim().Named("NAME")),
                        spren,
                        MessageTypeDefOf.PositiveEvent,
                        false
                    );
                    Close();
                }
            }
        }

        if (Event.current.type == EventType.Layout) {
            GUI.FocusControl("SprenName");
        }
    }
}
