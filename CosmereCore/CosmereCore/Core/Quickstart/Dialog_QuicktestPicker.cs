using System;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Quickstart;

/// <summary>
///     Replaces vanilla's straight-to-Crashlanded dev quicktest with a choice between that and
///     every quickstart the mods register.
/// </summary>
public class Dialog_QuicktestPicker : Verse.Window {
    private const float WindowWidth = 520f;
    private const float WindowHeight = 560f;
    private const float TitleHeight = 36f;
    private const float RowHeight = 58f;

    private static readonly Color RowColor = new Color(0.16f, 0.16f, 0.18f, 0.65f);
    private static readonly Color BlurbColor = new Color(0.72f, 0.72f, 0.7f);

    private readonly List<AbstractQuickstart> quickstarts = Build();
    private Vector2 scrollPos;

    public Dialog_QuicktestPicker() {
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        doCloseX = true;
        draggable = true;
    }

    public override Vector2 InitialSize => new Vector2(WindowWidth, WindowHeight);

    public override void DoWindowContents(Rect inRect) {
        using (new TextBlock(GameFont.Medium)) {
            Widgets.Label(inRect.TopPartPixels(TitleHeight), "CC_Quicktest_Title".Translate());
        }

        Rect scrollArea = new Rect(
            inRect.x,
            inRect.y + TitleHeight + Spacing.Get(0.5f),
            inRect.width,
            inRect.height - TitleHeight - Spacing.Get(0.5f)
        );
        float totalHeight = (quickstarts.Count + 1) * (RowHeight + Spacing.Get(0.25f));
        Rect viewRect = new Rect(0f, 0f, scrollArea.width - 16f, totalHeight);

        Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);
        float y = 0f;

        if (DrawRow(
                new Rect(0f, y, viewRect.width, RowHeight),
                "CC_Quicktest_Vanilla".Translate(),
                "CC_Quicktest_Vanilla_Blurb".Translate()
            )) {
            Close();
            Find.WindowStack.Add(new Dialog_QuicktestShards());
            Widgets.EndScrollView();
            return;
        }

        y += RowHeight + Spacing.Get(0.25f);

        for (int i = 0; i < quickstarts.Count; i++) {
            AbstractQuickstart quickstart = quickstarts[i];
            Rect row = new Rect(0f, y, viewRect.width, RowHeight);
            if (DrawRow(row, quickstart.GetType().Name, quickstart.description, quickstart.GetDescription())) {
                Close();
                Quickstarter.Launch(quickstart);
                Widgets.EndScrollView();
                return;
            }

            y += RowHeight + Spacing.Get(0.25f);
        }

        Widgets.EndScrollView();
    }

    private static bool DrawRow(Rect row, string label, string blurb, string? tooltip = null) {
        Widgets.DrawBoxSolid(row, RowColor);
        Widgets.DrawHighlightIfMouseover(row);
        TooltipHandler.TipRegion(row, tooltip ?? blurb);
        MouseoverSounds.DoRegion(row);

        Rect inner = row.ContractedBy(Spacing.Get(0.5f));
        Rect labelRect = inner.TopPartPixels(inner.height / 2f);
        Rect blurbRect = new Rect(inner.x, labelRect.yMax, inner.width, inner.height / 2f);

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(labelRect, label);
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, BlurbColor)) {
            Widgets.Label(blurbRect, blurb.Truncate(blurbRect.width));
        }

        return Widgets.ButtonInvisible(row, false);
    }

    private static List<AbstractQuickstart> Build() {
        List<AbstractQuickstart> built = [];
        foreach (Type type in typeof(AbstractQuickstart).AllSubclassesNonAbstract()) {
            try {
                built.Add((AbstractQuickstart)Activator.CreateInstance(type));
            } catch (Exception ex) {
                Logger.Error($"Could not instantiate quickstart {type.FullName}: {ex}");
            }
        }

        built.SortBy(q => q.GetType().Name);
        return built;
    }
}
