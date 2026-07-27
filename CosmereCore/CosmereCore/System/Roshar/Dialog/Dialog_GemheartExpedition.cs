using Cosmere.System.Roshar.Comp.Map;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.System.Roshar.Dialog;

public class Dialog_GemheartExpedition : Window {
    private const int MinPawns = 3;
    private const int MaxPawns = 6;
    private readonly List<Pawn> available = [];

    private readonly Map map;
    private readonly HashSet<Pawn> selected = [];
    private Vector2 scrollPos;

    public Dialog_GemheartExpedition(Map map) {
        this.map = map;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = false;

        List<Pawn> colonists = map.mapPawns.FreeColonists;
        for (int i = 0; i < colonists.Count; i++) {
            Pawn pawn = colonists[i];
            if (!pawn.Downed && !pawn.InMentalState) {
                available.Add(pawn);
            }
        }
    }

    public override Vector2 InitialSize => new Vector2(500f, 600f);

    public override void DoWindowContents(Rect inRect) {
        Text.Font = GameFont.Medium;
        Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 36f);
        Widgets.Label(titleRect, "Gemheart Expedition");

        Text.Font = GameFont.Small;
        float y = titleRect.yMax + 10f;
        Rect descRect = new Rect(inRect.x, y, inRect.width, 48f);
        Widgets.Label(
            descRect,
            $"Select {MinPawns}-{MaxPawns} colonists for the plateau run. Stronger fighters and Radiants improve the odds."
        );

        y = descRect.yMax + 10f;
        Rect countRect = new Rect(inRect.x, y, inRect.width, 24f);
        string countText = $"Selected: {selected.Count}/{MaxPawns}";
        Color countColor = selected.Count >= MinPawns ? Color.green : Color.red;
        GUI.color = countColor;
        Widgets.Label(countRect, countText);
        GUI.color = Color.white;

        y = countRect.yMax + 6f;
        float listHeight = inRect.height - y - 50f;
        Rect listOuterRect = new Rect(inRect.x, y, inRect.width, listHeight);
        float entryHeight = 40f;
        Rect listInnerRect = new Rect(0f, 0f, listOuterRect.width - 16f, available.Count * entryHeight);

        Widgets.BeginScrollView(listOuterRect, ref scrollPos, listInnerRect);
        for (int i = 0; i < available.Count; i++) {
            Pawn pawn = available[i];
            Rect entryRect = new Rect(0f, i * entryHeight, listInnerRect.width, entryHeight);

            if (i % 2 == 1) {
                Widgets.DrawLightHighlight(entryRect);
            }

            bool isSelected = selected.Contains(pawn);
            Rect checkRect = new Rect(entryRect.x + 4f, entryRect.y + 8f, 24f, 24f);
            bool wasSelected = isSelected;
            Widgets.Checkbox(checkRect.position, ref isSelected, 24f, !(!isSelected && selected.Count >= MaxPawns));

            if (isSelected != wasSelected) {
                if (isSelected) {
                    selected.Add(pawn);
                } else {
                    selected.Remove(pawn);
                }
            }

            Rect nameRect = new Rect(checkRect.xMax + 8f, entryRect.y + 2f, 180f, entryHeight / 2f);
            Widgets.Label(nameRect, pawn.LabelShortCap);

            Rect statsRect = new Rect(
                checkRect.xMax + 8f,
                entryRect.y + entryHeight / 2f,
                entryRect.width - 40f,
                entryHeight / 2f
            );
            Text.Font = GameFont.Tiny;
            int melee = pawn.skills?.GetSkill(RimWorld.SkillDefOf.Melee)?.Level ?? 0;
            int shooting = pawn.skills?.GetSkill(RimWorld.SkillDefOf.Shooting)?.Level ?? 0;
            string stats = $"Melee: {melee}  Shooting: {shooting}";

            if (pawn.genes != null) {
                Surgebinder? surgebinder = pawn.genes.GetFirstGeneOfType<Surgebinder>();
                if (surgebinder is { Active: true }) {
                    stats += $"  Radiant (Ideal {surgebinder.CurrentIdealDisplay})";
                }
            }

            GUI.color = Color.gray;
            Widgets.Label(statsRect, stats);
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
        }

        Widgets.EndScrollView();

        float buttonY = inRect.yMax - 40f;
        float buttonWidth = 140f;
        float spacing = 20f;

        Rect sendRect = new Rect(inRect.x + inRect.width / 2f - buttonWidth - spacing / 2f, buttonY, buttonWidth, 36f);
        Rect cancelRect = new Rect(inRect.x + inRect.width / 2f + spacing / 2f, buttonY, buttonWidth, 36f);

        if (selected.Count >= MinPawns) {
            if (Widgets.ButtonText(sendRect, "March")) {
                GemheartExpeditionManager? manager = map.GetComponent<GemheartExpeditionManager>();
                if (manager != null) {
                    manager.StartExpedition(selected.ToList());
                    SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
                    Messages.Message(
                        $"{selected.Count} colonists march toward the Shattered Plains.",
                        MessageTypeDefOf.PositiveEvent
                    );
                }

                Close();
            }
        } else {
            GUI.color = Color.gray;
            Widgets.ButtonText(sendRect, "March");
            GUI.color = Color.white;
        }

        if (Widgets.ButtonText(cancelRect, "Cancel")) {
            Close();
        }
    }
}
