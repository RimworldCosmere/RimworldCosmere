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

    private static readonly Color ReadyColor = new Color(0.45f, 0.72f, 0.42f);
    private static readonly Color ShortColor = new Color(0.85f, 0.35f, 0.3f);
    private static readonly Color StatColor = new Color(0.62f, 0.6f, 0.56f);

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
        Rect countRect = new Rect(inRect.x, y, inRect.width, 24f);
        bool enoughSelected = selected.Count >= MinPawns;
        using (new TextBlock(GameFont.Small, null, enoughSelected ? ReadyColor : ShortColor)) {
            Widgets.Label(
                countRect,
                "CRO_Gemheart_Selected".Translate(selected.Count.Named("COUNT"), MaxPawns.Named("MAX"))
            );
        }

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

            using (new TextBlock(GameFont.Tiny, null, StatColor)) {
                Widgets.Label(statsRect, SkillLine(pawn));
            }
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
