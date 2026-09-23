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

    private const float RowHeight = 40f;
    private const float CheckSize = 24f;
    private const float Gutter = 8f;

    private static readonly Color ReadyColor = new Color(0.45f, 0.72f, 0.42f);
    private static readonly Color ShortColor = new Color(0.85f, 0.35f, 0.3f);
    private static readonly Color StatColor = new Color(0.62f, 0.6f, 0.56f);
    private static readonly Color CostlyColor = new Color(0.82f, 0.7f, 0.35f);

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
        Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 36f);
        using (new TextBlock(GameFont.Medium)) {
            Widgets.Label(titleRect, "CRO_Gemheart_Title".Translate());
        }

        float y = titleRect.yMax + 10f;
        Rect descRect = new Rect(inRect.x, y, inRect.width, 48f);
        using (new TextBlock(GameFont.Small)) {
            Widgets.Label(
                descRect,
                "CRO_Gemheart_Blurb".Translate(MinPawns.Named("MIN"), MaxPawns.Named("MAX"))
            );
        }

        y = descRect.yMax + 10f;
        Rect statusRect = new Rect(inRect.x, y, inRect.width, 24f);
        bool enoughSelected = selected.Count >= MinPawns;

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, enoughSelected ? ReadyColor : ShortColor)) {
            Widgets.Label(
                statusRect.LeftHalf(),
                "CRO_Gemheart_Selected".Translate(selected.Count.Named("COUNT"), MaxPawns.Named("MAX"))
            );
        }

        DrawOdds(statusRect.RightHalf());

        y = statusRect.yMax + 6f;
        float listHeight = inRect.height - y - 50f;
        Rect listOuterRect = new Rect(inRect.x, y, inRect.width, listHeight);
        Rect listInnerRect = new Rect(0f, 0f, listOuterRect.width - 16f, available.Count * RowHeight);

        Widgets.BeginScrollView(listOuterRect, ref scrollPos, listInnerRect);
        for (int i = 0; i < available.Count; i++) {
            DrawPawnRow(new Rect(0f, i * RowHeight, listInnerRect.width, RowHeight), available[i], i % 2 == 1);
        }

        Widgets.EndScrollView();

        float buttonY = inRect.yMax - 40f;
        float buttonWidth = 140f;
        float spacing = 20f;

        Rect sendRect = new Rect(inRect.x + inRect.width / 2f - buttonWidth - spacing / 2f, buttonY, buttonWidth, 36f);
        Rect cancelRect = new Rect(inRect.x + inRect.width / 2f + spacing / 2f, buttonY, buttonWidth, 36f);

        if (!enoughSelected) {
            TooltipHandler.TipRegion(sendRect, "CRO_Gemheart_NeedMore".Translate(MinPawns.Named("MIN")));
        }

        if (Widgets.ButtonText(sendRect, "CRO_Gemheart_March".Translate(), active: enoughSelected)) {
            GemheartExpeditionManager? manager = map.GetComponent<GemheartExpeditionManager>();
            if (manager != null) {
                manager.StartExpedition(selected.ToList());
                SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
                Messages.Message(
                    "CRO_Gemheart_Marching".Translate(selected.Count.Named("COUNT")),
                    MessageTypeDefOf.PositiveEvent
                );
            }

            Close();
        }

        if (Widgets.ButtonText(cancelRect, "CRO_Gemheart_Cancel".Translate())) {
            Close();
        }
    }

    private void DrawPawnRow(Rect rowRect, Pawn pawn, bool striped) {
        if (striped) Widgets.DrawLightHighlight(rowRect);

        bool isSelected = selected.Contains(pawn);
        bool atCapacity = !isSelected && selected.Count >= MaxPawns;

        if (!atCapacity) {
            Widgets.DrawHighlightIfMouseover(rowRect);
            MouseoverSounds.DoRegion(rowRect);
        }

        TooltipHandler.TipRegion(
            rowRect,
            atCapacity
                ? "CRO_Gemheart_Row_Full".Translate(MaxPawns.Named("MAX"))
                : "CRO_Gemheart_Row_Tooltip".Translate(
                    pawn.LabelShortCap.Named("PAWN"),
                    GemheartExpeditionManager.PawnPower(pawn).ToString("F0").Named("POWER")
                )
        );

        float checkX = rowRect.x + 4f;
        Widgets.CheckboxDraw(checkX, rowRect.y + (rowRect.height - CheckSize) / 2f, isSelected, atCapacity, CheckSize);

        float textX = checkX + CheckSize + Gutter;
        float textWidth = rowRect.xMax - textX - Gutter;

        Rect nameRect = new Rect(textX, rowRect.y + 2f, textWidth, rowRect.height / 2f);
        Widgets.Label(nameRect, pawn.LabelShortCap);

        Rect statsRect = new Rect(textX, rowRect.y + rowRect.height / 2f, textWidth, rowRect.height / 2f);
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, StatColor)) {
            Widgets.Label(statsRect, SkillLine(pawn));
        }

        if (atCapacity || !Widgets.ButtonInvisible(rowRect)) return;

        if (isSelected) {
            selected.Remove(pawn);
            SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
        } else {
            selected.Add(pawn);
            SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
        }
    }

    private void DrawOdds(Rect rect) {
        if (selected.Count == 0) return;

        GemheartExpeditionManager? manager = map.GetComponent<GemheartExpeditionManager>();
        if (manager == null) return;

        float power = GemheartExpeditionManager.ExpeditionPower(selected.ToList());
        float difficulty = manager.CurrentDifficulty;
        GemheartOutcome outcome = GemheartOdds.Classify(GemheartOdds.Ratio(power, difficulty));

        TooltipHandler.TipRegion(
            rect,
            "CRO_Gemheart_Odds_Tooltip".Translate(
                power.ToString("F0").Named("POWER"),
                difficulty.ToString("F0").Named("DIFFICULTY")
            )
        );

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleRight, OutcomeColor(outcome))) {
            Widgets.Label(rect, "CRO_Gemheart_Odds".Translate(OutcomeLabel(outcome).Named("OUTCOME")));
        }
    }

    private static Color OutcomeColor(GemheartOutcome outcome) {
        return outcome switch {
            GemheartOutcome.Victory or GemheartOutcome.HardWon => ReadyColor,
            GemheartOutcome.Pyrrhic => CostlyColor,
            _ => ShortColor,
        };
    }

    private static TaggedString OutcomeLabel(GemheartOutcome outcome) {
        return outcome switch {
            GemheartOutcome.Victory => "CRO_Gemheart_Odds_Victory".Translate(),
            GemheartOutcome.HardWon => "CRO_Gemheart_Odds_HardWon".Translate(),
            GemheartOutcome.Pyrrhic => "CRO_Gemheart_Odds_Pyrrhic".Translate(),
            GemheartOutcome.Failure => "CRO_Gemheart_Odds_Failure".Translate(),
            _ => "CRO_Gemheart_Odds_Disaster".Translate(),
        };
    }

    private static TaggedString SkillLine(Pawn pawn) {
        NamedArgument melee = (pawn.skills?.GetSkill(RimWorld.SkillDefOf.Melee)?.Level ?? 0).Named("MELEE");
        NamedArgument shooting = (pawn.skills?.GetSkill(RimWorld.SkillDefOf.Shooting)?.Level ?? 0).Named("SHOOTING");

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder is { Active: true }) {
            return "CRO_Gemheart_SkillsRadiant".Translate(
                melee,
                shooting,
                surgebinder.CurrentIdealDisplay.Named("IDEAL")
            );
        }

        return "CRO_Gemheart_Skills".Translate(melee, shooting);
    }
}
