using Cosmere.System.Scadrial.Feruchemy.Comp.Thing;
using Cosmere.System.Scadrial.Feruchemy.Memory;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Feruchemy.UI;

public sealed class Dialog_StoreMemory : Window {
    private static readonly Color PositiveMoodColor = new Color(0.45f, 0.85f, 0.45f);
    private static readonly Color NegativeMoodColor = new Color(0.9f, 0.45f, 0.45f);
    private static readonly Color SelectedRowColor = new Color(0.9f, 0.75f, 0.35f, 0.22f);
    private static readonly Color HoverRowColor = new Color(1f, 1f, 1f, 0.06f);
    private static readonly Color SecondaryTextColor = new Color(0.7f, 0.7f, 0.7f);
    private static readonly Color DisabledTextColor = new Color(0.55f, 0.55f, 0.55f);
    private readonly List<Metalmind> copperminds;
    private readonly List<Thought_Memory> memories;

    private readonly Pawn pawn;
    private Vector2 coppermindsScroll;
    private Vector2 memoriesScroll;
    private int selectedCoppermindIndex = -1;

    private int selectedMemoryIndex = -1;

    public Dialog_StoreMemory(Pawn pawn, List<Thought_Memory> memories, List<Metalmind> copperminds) {
        this.pawn = pawn;
        this.memories = memories;
        this.copperminds = copperminds;
        doCloseX = true;
        closeOnClickedOutside = false;
        absorbInputAroundWindow = true;
        forcePause = true;
    }

    public override Vector2 InitialSize => new Vector2(720f, 520f);

    public override void DoWindowContents(Rect inRect) {
        float y = inRect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(
                new Rect(inRect.x, y, inRect.width, 32f),
                "CC_Codex_Feruchemy_StoreMemory_Title".Translate(pawn.LabelShortCap.Named("PAWN"))
            );
        }

        y += 36f;

        float bottomBarHeight = 40f;
        float columnsHeight = inRect.height - (y - inRect.y) - bottomBarHeight - 12f;
        float columnWidth = (inRect.width - 12f) * 0.5f;

        Rect memoriesColumn = new Rect(inRect.x, y, columnWidth, columnsHeight);
        Rect coppermindsColumn = new Rect(memoriesColumn.xMax + 12f, y, columnWidth, columnsHeight);

        DrawMemoriesColumn(memoriesColumn);
        DrawCoppermindsColumn(coppermindsColumn);

        Rect bottomBar = new Rect(inRect.x, inRect.yMax - bottomBarHeight, inRect.width, bottomBarHeight);
        DrawBottomBar(bottomBar);
    }

    private void DrawMemoriesColumn(Rect rect) {
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(
                new Rect(rect.x, rect.y, rect.width, 24f),
                "CC_Codex_Feruchemy_StoreMemory_PawnMemories".Translate()
            );
        }

        Rect body = new Rect(rect.x, rect.y + 26f, rect.width, rect.height - 26f);
        Widgets.DrawBoxSolid(body, new Color(0f, 0f, 0f, 0.2f));
        Rect inner = body.ContractedBy(2f);

        float rowHeight = 28f;
        Rect viewRect = new Rect(0f, 0f, inner.width - 16f, memories.Count * rowHeight);
        Widgets.BeginScrollView(inner, ref memoriesScroll, viewRect);

        for (int i = 0; i < memories.Count; i++) {
            Thought_Memory memory = memories[i];
            Rect row = new Rect(0f, i * rowHeight, viewRect.width, rowHeight);
            if (selectedMemoryIndex == i) {
                Widgets.DrawBoxSolid(row, SelectedRowColor);
            }
            else if (Mouse.IsOver(row)) Widgets.DrawBoxSolid(row, HoverRowColor);

            float offset = memory.MoodOffset();
            Color color = offset >= 0f ? PositiveMoodColor : NegativeMoodColor;

            Rect label = new Rect(row.x + 6f, row.y, row.width * 0.7f - 6f, row.height);
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, color)) {
                Widgets.Label(
                    label,
                    memory.otherPawn != null
                        ? $"{memory.def.LabelCap} ({memory.otherPawn.LabelShortCap})"
                        : memory.def.LabelCap.ToString()
                );
            }

            Rect sizeRect = new Rect(label.xMax, row.y, row.width - label.width - 6f, row.height);
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleRight, SecondaryTextColor)) {
                Widgets.Label(
                    sizeRect,
                    "CC_Codex_Feruchemy_StoreMemory_Size".Translate(Mathf.Abs(offset).ToString("F1").Named("SIZE"))
                );
            }

            if (Widgets.ButtonInvisible(row)) {
                selectedMemoryIndex = i;
                if (selectedCoppermindIndex >= 0 && !CanFitSelected(selectedCoppermindIndex)) {
                    selectedCoppermindIndex = -1;
                }
            }
        }

        Widgets.EndScrollView();
    }

    private void DrawCoppermindsColumn(Rect rect) {
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(
                new Rect(rect.x, rect.y, rect.width, 24f),
                "CC_Codex_Feruchemy_StoreMemory_Copperminds".Translate()
            );
        }

        Rect body = new Rect(rect.x, rect.y + 26f, rect.width, rect.height - 26f);
        Widgets.DrawBoxSolid(body, new Color(0f, 0f, 0f, 0.2f));
        Rect inner = body.ContractedBy(2f);

        float rowHeight = 34f;
        Rect viewRect = new Rect(0f, 0f, inner.width - 16f, copperminds.Count * rowHeight);
        Widgets.BeginScrollView(inner, ref coppermindsScroll, viewRect);

        for (int i = 0; i < copperminds.Count; i++) {
            Metalmind mind = copperminds[i];
            Rect row = new Rect(0f, i * rowHeight, viewRect.width, rowHeight);

            bool fits = CanFitSelected(i);
            bool isSelected = selectedCoppermindIndex == i;

            if (isSelected) {
                Widgets.DrawBoxSolid(row, SelectedRowColor);
            }
            else if (fits && Mouse.IsOver(row)) Widgets.DrawBoxSolid(row, HoverRowColor);

            Color labelColor = fits ? Color.white : DisabledTextColor;
            Rect label = new Rect(row.x + 6f, row.y + 2f, row.width - 12f, 18f);
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, labelColor))
                Widgets.Label(label, mind.parent.LabelCap);

            Rect detail = new Rect(row.x + 6f, row.y + 18f, row.width - 12f, 14f);
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, SecondaryTextColor)) {
                Widgets.Label(
                    detail,
                    "CC_Codex_Feruchemy_StoreMemory_CoppermindDetail".Translate(
                        mind.StoredMemories.Count.Named("COUNT"),
                        mind.UsedMemorySpace.ToString("F1").Named("USED"),
                        mind.MaxAmount.ToString("F0").Named("MAX")
                    )
                );
            }

            if (fits && Widgets.ButtonInvisible(row)) {
                selectedCoppermindIndex = i;
            }
        }

        Widgets.EndScrollView();
    }

    private void DrawBottomBar(Rect rect) {
        string status;
        if (selectedMemoryIndex < 0) {
            status = (string)"CC_Codex_Feruchemy_StoreMemory_Hint_SelectMemory".Translate();
        }
        else if (selectedCoppermindIndex < 0) {
            status = (string)"CC_Codex_Feruchemy_StoreMemory_Hint_SelectCoppermind".Translate();
        }
        else {
            status = string.Empty;
        }

        Rect statusRect = new Rect(rect.x, rect.y, rect.width - 260f, rect.height);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, SecondaryTextColor))
            Widgets.Label(statusRect, status);

        Rect cancelRect = new Rect(rect.xMax - 250f, rect.y + 6f, 120f, rect.height - 12f);
        if (Widgets.ButtonText(cancelRect, "CC_Codex_Feruchemy_StoreMemory_Cancel".Translate())) {
            Close();
        }

        Rect confirmRect = new Rect(rect.xMax - 124f, rect.y + 6f, 124f, rect.height - 12f);
        bool canConfirm = selectedMemoryIndex >= 0 && selectedCoppermindIndex >= 0;
        GUI.enabled = canConfirm;
        if (Widgets.ButtonText(confirmRect, "CC_Codex_Feruchemy_StoreMemory_Confirm".Translate())) {
            PerformStore();
            Close();
        }

        GUI.enabled = true;
    }

    private bool CanFitSelected(int coppermindIndex) {
        if (selectedMemoryIndex < 0) return true;
        if (coppermindIndex < 0 || coppermindIndex >= copperminds.Count) return false;
        float magnitude = Mathf.Abs(memories[selectedMemoryIndex].MoodOffset());
        return copperminds[coppermindIndex].CanFitMemory(magnitude);
    }

    private void PerformStore() {
        if (selectedMemoryIndex < 0 || selectedMemoryIndex >= memories.Count) return;
        if (selectedCoppermindIndex < 0 || selectedCoppermindIndex >= copperminds.Count) return;

        Thought_Memory thought = memories[selectedMemoryIndex];
        Metalmind mind = copperminds[selectedCoppermindIndex];

        float magnitude = Mathf.Abs(thought.MoodOffset());
        if (!mind.CanFitMemory(magnitude)) return;

        StoredMemory stored = new StoredMemory(thought, pawn);
        mind.StoreMemory(stored);
        pawn.needs?.mood?.thoughts?.memories?.RemoveMemory(thought);
        mind.SyncInjectedThoughts(pawn);
    }
}