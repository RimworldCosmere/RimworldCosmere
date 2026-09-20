using System.Collections.Generic;
using Cosmere.Core.Listing;
using Cosmere.Core.UI;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     The body a kandra actually is, beside the ones it has eaten. The true body is not a peer
///     of the borrowed shapes, so it takes the rail rather than a card in the grid.
/// </summary>
[StaticConstructorOnStartup]
public partial class Dialog_KandraForms : Core.Window.BaseWindow {
    private static readonly Texture2D MaleTrueBody =
        ContentFinder<Texture2D>.Get("Things/Pawn/Humanlike/Bodies/Kandra_Male_south");

    private static readonly Texture2D FemaleTrueBody =
        ContentFinder<Texture2D>.Get("Things/Pawn/Humanlike/Bodies/Kandra_Female_south");

    private static readonly Texture2D MaleTrueHead =
        ContentFinder<Texture2D>.Get("Things/Pawn/Humanlike/Heads/Kandra_Male_south");

    private static readonly Texture2D FemaleTrueHead =
        ContentFinder<Texture2D>.Get("Things/Pawn/Humanlike/Heads/Kandra_Female_south");

    /// <summary>The preview owns its own paths, the way it already does for the body and the head.</summary>
    private const string IrisDir = "Things/Pawn/Humanlike/HeadAttachments/KandraEyes/";

    private static readonly Texture2D MaleIrisLeft =
        ContentFinder<Texture2D>.Get(IrisDir + "Kandra_Eyes_Male_Standard_Left_south");

    private static readonly Texture2D MaleIrisRight =
        ContentFinder<Texture2D>.Get(IrisDir + "Kandra_Eyes_Male_Standard_Right_south");

    private static readonly Texture2D FemaleIrisLeft =
        ContentFinder<Texture2D>.Get(IrisDir + "Kandra_Eyes_Female_Standard_Left_south");

    private static readonly Texture2D FemaleIrisRight =
        ContentFinder<Texture2D>.Get(IrisDir + "Kandra_Eyes_Female_Standard_Right_south");

    /// <summary>Every iris the table has art for, loaded once so no draw call reaches ContentFinder.</summary>
    private static readonly Dictionary<string, Texture2D> IrisArt = BuildIrisArt();

    /// <summary>The humanlike body mesh is 1.5 world units, which is what headOffset is measured in.</summary>
    private const float BodyMeshUnits = 1.5f;

    private const float RailWidth = 232f;
    private const float CardWidth = 118f;
    private const float CardHeight = 132f;
    private const float CardGap = 12f;

    /// <summary>The engine's own bar width. A narrower guess leaves content too wide and adds a
    ///     horizontal scrollbar nobody asked for.</summary>
    private static readonly float ScrollbarWidth = GenUI.ScrollBarWidth;

    private const float ButtonHeight = 32f;
    private const float RailPad = 12f;
    private const float WellHeight = 150f;

    private const float PreviewPlateHeight = 224f;
    private const float GenderButtonHeight = 40f;
    private const float SwatchRowHeight = 24f;

    private const float SummaryNameHeight = 24f;
    private const float SummaryLineHeight = 18f;
    private const float SummarySwatchSize = 10f;

    /// <summary>Two chip columns, not three. The palette is half as wide as it used to be.</summary>
    private const int SwatchColumns = 2;

    /// <summary>How much of a tab's pane the left column takes. A grid needs room a list does not.</summary>
    private const float BodyColumnShare = 0.5f;

    private const float HairColumnShare = 0.655f;
    private const float EyeColumnShare = 0.535f;

    private const int HairColumns = 4;
    private const float HairCellHeight = 92f;
    private const float HairThumbSize = 62f;
    private const float SectionHeaderHeight = 20f;

    private static readonly Vector2 PreviewSize = new Vector2(180f, 168f);
    private static readonly Vector2 CardPortrait = new Vector2(108f, 84f);
    private static readonly Vector2 RailPortrait = new Vector2(200f, WellHeight);

    private readonly global::System.Action<int> choose;
    private readonly CompKandraForms forms;

    private KandraForm? confirming;
    private bool picking;
    private Vector2 scroll;

    /// <summary>The third rail state, beside picking and confirming. Set by the edit button.</summary>
    private bool reshaping;

    private string? designMaterial;
    private Gender designGender;
    private HairDef? designHair;
    private string? designHairColour;
    private string? designEyeColour;
    private string? designEyeColourTwo;
    private string? designIrisSize;
    private string? designEyeLight;

    private Vector2 materialScroll;
    private Vector2 hairScroll;
    private Vector2 dyeScroll;
    private Vector2 eyeScroll;

    /// <summary>Every loaded hair. Abstracts never reach the database, so the list needs no filter.</summary>
    private List<HairDef>? hairs;

    public Dialog_KandraForms(CompKandraForms forms, global::System.Action<int> choose) {
        this.forms = forms;
        this.choose = choose;

        doCloseX = false;
        forcePause = true;
        absorbInputAroundWindow = true;
    }

    protected override Vector2 initialWindowSize => new Vector2(Spacing.Get(65), Spacing.Get(39));

    protected override float headerHeight => Spacing.Get(3.5);

    private Pawn? Pawn => forms.parent as Pawn;

    private List<HairDef> Hairs => hairs ??= DefDatabase<HairDef>.AllDefsListForReading;

    protected override TaggedString GetTitle() {
        if (reshaping) return "CS_Kandra_ReshapeTitle".Translate();

        return (picking ? "CS_Kandra_ChooseTrue" : "CS_Kandra_PickForm").Translate();
    }

    protected override TaggedString? GetSubtitle() {
        return reshaping ? "CS_Kandra_ReshapeSub".Translate() : CountLine();
    }

    /// <summary>Nothing flows here, so the listing body stays empty and DrawBody lays out Rects.</summary>
    protected override void DrawBodyContent(FoundationListing listing) { }

    /// <summary>
    ///     Returns the rect's own height so the window's scroll view never engages. The grid runs
    ///     its own scroll, and a scroll view inside a scroll view eats the wheel.
    /// </summary>
    protected override float DrawBody(Rect rect) {
        Rect body = rect.ContractedBy(Spacing.Get(0.75f));
        Rect rail = new Rect(body.x, body.y, RailWidth, body.height);
        Rect right = new Rect(
            rail.xMax + Spacing.Get(),
            body.y,
            body.width - RailWidth - Spacing.Get(),
            body.height
        );

        if (reshaping) {
            DrawReshapeRail(rail);
            DrawDesigner(right);

            return rect.height;
        }

        if (confirming != null) {
            DrawConfirm(rail, confirming);
        } else {
            DrawRail(rail);
        }

        if (forms.Known.Count == 0) {
            DrawEmpty(right);

            return rect.height;
        }

        if (picking) right = DrawPickBar(right);

        DrawGrid(right);

        return rect.height;
    }

    private Rect DrawPickBar(Rect right) {
        Rect bar = right.TopPartPixels(Spacing.Get(1.75f));

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, BorderColor)) {
            Widgets.Label(bar, "CS_Kandra_PickTrueHint".Translate());
        }

        if (Widgets.ButtonText(bar.RightPartPixels(88f), "CS_Kandra_Cancel".Translate())) {
            picking = false;
            confirming = null;
        }

        return new Rect(right.x, bar.yMax + Spacing.Get(0.5f), right.width, right.height - Spacing.Get(2.25f));
    }

    private TaggedString CountLine() {
        if (picking) return "CS_Kandra_ToChooseFrom".Translate(forms.Known.Count.Named("COUNT"));

        string? worn = forms.WornName;

        return worn.NullOrEmpty()
            ? "CS_Kandra_FormCount".Translate(forms.Known.Count.Named("COUNT"))
            : "CS_Kandra_CountWearing".Translate(forms.Known.Count.Named("COUNT"), worn!.Named("FORM"));
    }

    /// <summary>
    ///     A vanilla menu section is what makes the true body read as its own region, so nothing
    ///     here needs a coloured frame or a fill this window invented.
    /// </summary>
    private void DrawRail(Rect rail) {
        Widgets.DrawMenuSection(rail);

        float x = rail.x + RailPad;
        float width = rail.width - (RailPad * 2f);
        float y = rail.y + RailPad;

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            Widgets.Label(new Rect(x, y, width, 20f), "CS_Kandra_TrueFormHeader".Translate());
        }

        y += 22f;
        DrawPortrait(new Rect(x, y, width, WellHeight), forms.TrueBody, RailPortrait, 1.12f);
        y += WellHeight + 8f;

        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, headerTextColor)) {
            Widgets.Label(new Rect(x, y, width, 22f), Pawn?.LabelShortCap ?? string.Empty);
        }

        y += 24f;
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            Widgets.Label(new Rect(x, y, width, 18f), KindLine());
            DrawMaterial(new Rect(x, y + 18f, width, 18f));
        }

        DrawRailButtons(new Rect(x, rail.yMax - RailPad - (ButtonHeight * 3f) - 16f, width, ButtonHeight));
    }

    private void DrawRailButtons(Rect first) {
        bool wearing = forms.IsWearingSomeoneElse;

        // Only a built body has a material to pick. One it ate is bone, and bone is not designed.
        bool crafted = forms.TrueBodyCrafted;

        if (Reasoned(
                first,
                "CS_Kandra_EditTrue".Translate(),
                crafted,
                crafted ? null : "CS_Kandra_EditTrue_NotCrafted".Translate()
            )) {
            BeginDesign();
        }

        // Allowed while wearing somebody. The body underneath is what changes, not the disguise.
        Rect change = new Rect(first.x, first.y + ButtonHeight + 8f, first.width, first.height);
        bool canChange = forms.Known.Count > 0;

        if (Reasoned(
                change,
                "CS_Kandra_ChangeTrue".Translate(),
                canChange,
                canChange ? null : "CS_Kandra_ChangeTrue_Nothing".Translate()
            )) {
            picking = true;
        }

        if (!wearing) return;

        // The one action in this state that changes what the colony sees. Nothing else is a CTA.
        Rect revert = new Rect(first.x, change.yMax + 8f, first.width, first.height);
        if (!CTAButtonText(revert, "CS_Kandra_Revert".Translate())) return;

        // Queued as a job rather than applied here. Rearranging a body takes ten seconds.
        choose(JobDriver.KandraChangeShape.RevertIndex);
        Close();
    }

    /// <summary>A button that says why it cannot be used, rather than one that is merely grey.</summary>
    private static bool Reasoned(Rect rect, string label, bool enabled, string? reason) {
        bool clicked = Widgets.ButtonText(rect, label, active: enabled);
        if (!enabled && !reason.NullOrEmpty()) TooltipHandler.TipRegion(rect, reason);

        return clicked;
    }

    private string KindLine() {
        if (forms.TrueBodyCrafted) return "CS_Kandra_TrueKind_Crafted".Translate();

        return "CS_Kandra_TrueKind_Adopted".Translate(
            (forms.TrueBody?.Label ?? string.Empty).Named("FORM")
        );
    }

    /// <summary>The material only exists on a crafted body. An adopted one is bone.</summary>
    private void DrawMaterial(Rect rect) {
        Pawn? pawn = Pawn;
        if (pawn == null) return;

        if (!forms.TrueBodyCrafted) {
            Widgets.Label(rect, "CS_Kandra_NoMaterial".Translate());

            return;
        }

        (string name, Color colour) = KandraAppearance.TrueBodyMaterialFor(pawn);

        Rect swatch = new Rect(rect.x, rect.y + 4f, 10f, 10f);
        Widgets.DrawBoxSolid(swatch, colour);
        Widgets.Label(new Rect(swatch.xMax + 6f, rect.y, rect.width - 16f, rect.height), MaterialLabel(name));
    }

    /// <summary>
    ///     What the player reads a material as. The table's name is the save key, so it never goes
    ///     on screen. A save naming a material the table dropped shows that name rather than a blank.
    /// </summary>
    private static string MaterialLabel(string name) {
        (string name, string hex, float weight, string labelKey, string group)? row =
            KandraAppearance.FindMaterial(name);

        return row == null ? name.CapitalizeFirst() : (string)row.Value.labelKey.Translate();
    }

    /// <summary>Opens the designer on what the kandra already is, so accepting changes nothing.</summary>
    private void BeginDesign() {
        Pawn? pawn = Pawn;

        KandraForm? body = forms.TrueBody;

        designMaterial = pawn == null ? null : KandraAppearance.TrueBodyMaterialFor(pawn).name;
        designGender = body?.gender ?? pawn?.gender ?? Gender.Male;
        designHair = body?.hair ?? pawn?.story?.hairDef;
        designHairColour = body?.hairColourName;
        designEyeColour = body?.eyeColourName;
        designEyeColourTwo = body?.eyeColourTwoName;
        designIrisSize = body?.irisSizeName;
        designEyeLight = body?.eyeLightName;
        reshaping = true;
        picking = false;
        confirming = null;
    }

    /// <summary>The before picture. The body as it stands, beside the one being designed.</summary>
    private void DrawReshapeRail(Rect rail) {
        Widgets.DrawMenuSection(rail);

        float x = rail.x + RailPad;
        float width = rail.width - (RailPad * 2f);
        float y = rail.y + RailPad;

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            Widgets.Label(new Rect(x, y, width, 20f), "CS_Kandra_ReshapeNow".Translate());
        }

        y += 22f;
        DrawPortrait(new Rect(x, y, width, WellHeight), forms.TrueBody, RailPortrait, 1.12f);
        y += WellHeight + 8f;

        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, headerTextColor)) {
            Widgets.Label(new Rect(x, y, width, 22f), Pawn?.LabelShortCap ?? string.Empty);
        }

        y += 24f;
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            DrawMaterial(new Rect(x, y, width, 18f));
        }

        Rect cancel = new Rect(x, rail.yMax - RailPad - ButtonHeight, width, ButtonHeight);
        if (Widgets.ButtonText(cancel, "CS_Kandra_Cancel".Translate())) reshaping = false;
    }

    /// <summary>
    ///     The preview across the top, the tab strip under it, and whichever pane is live below
    ///     that. The preview never moves while the player picks, because it is what is being judged.
    /// </summary>
    private void DrawDesigner(Rect region) {
        Rect plate = region.TopPartPixels(PreviewPlateHeight);
        DrawPreviewPlate(plate);

        Rect below = new Rect(
            region.x,
            plate.yMax + Spacing.Get(),
            region.width,
            region.height - PreviewPlateHeight - Spacing.Get()
        );

        Rect pane = DrawTabStrip(below);

        if (tab == DesignerTab.Hair) {
            (Rect grid, Rect dyes) = DrawColumnPlates(pane, HairColumnShare);
            DrawHairGrid(grid);
            DrawHairDyes(dyes);

            return;
        }

        if (tab == DesignerTab.Eyes) {
            (Rect stones, Rect options) = DrawColumnPlates(pane, EyeColumnShare);
            DrawEyeColours(stones);
            DrawEyeOptions(options);

            return;
        }

        (Rect build, Rect materials) = DrawColumnPlates(pane, BodyColumnShare);
        DrawBuildColumn(build);
        DrawPalette(materials);
    }

    /// <summary>What the kandra is built as, and the note saying none of it is permanent.</summary>
    private void DrawBuildColumn(Rect inner) {
        float y = inner.y;

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, BorderColor)) {
            Widgets.Label(new Rect(inner.x, y, inner.width, 18f), "CS_Kandra_BuildHeader".Translate());
        }

        y += SectionHeaderHeight;
        float half = (inner.width - Spacing.Get(0.5f)) / 2f;
        DrawGenderOption(new Rect(inner.x, y, half, GenderButtonHeight), Gender.Male);
        DrawGenderOption(new Rect(inner.xMax - half, y, half, GenderButtonHeight), Gender.Female);

        y += GenderButtonHeight + Spacing.Get(0.5f);
        TaggedString reads = "CS_Kandra_GenderReads".Translate(designGender.GetLabel().CapitalizeFirst().Named("GENDER"));

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            Rect line = new Rect(inner.x, y, inner.width, Text.CalcHeight(reads, inner.width));
            Widgets.Label(line, reads);
            y = line.yMax + Spacing.Get(0.5f);

            TaggedString note = "CS_Kandra_BuildNote".Translate();
            Widgets.Label(new Rect(inner.x, y, inner.width, Text.CalcHeight(note, inner.width)), note);
        }
    }

    /// <summary>The stones a kandra can cut its eyes from, as the same chip rows the materials use.</summary>
    private void DrawEyeColours(Rect inner) {
        float gap = Spacing.Get(0.5f);
        float width = ColumnContentWidth(inner);
        float column = (width - (gap * (SwatchColumns - 1))) / SwatchColumns;

        IReadOnlyList<(string name, string hex, string labelKey)> stones = KandraAppearance.AllEyeColours;
        float rows = Mathf.CeilToInt(stones.Count / (float)SwatchColumns) * SwatchRowHeight;
        bool odd = KandraAppearance.FindEyeColour(designEyeColourTwo) != null;
        float content = SectionHeaderHeight + rows + (odd ? gap + SectionHeaderHeight + rows : 0f);

        Widgets.BeginScrollView(inner, ref eyeScroll, new Rect(0f, 0f, width, content));

        // Named per eye only when they differ, so the common case is one plain list.
        DrawSectionHeader(0f, width, odd ? "CS_Kandra_LeftEyeHeader" : "CS_Kandra_EyeColourHeader");

        for (int i = 0; i < stones.Count; i++) {
            (string name, string hex, string labelKey) = stones[i];

            Rect row = new Rect(
                (i % SwatchColumns) * (column + gap),
                SectionHeaderHeight + ((i / SwatchColumns) * SwatchRowHeight),
                column,
                SwatchRowHeight - 2f
            );

            if (DrawSwatch(row, hex, labelKey.Translate(), name == designEyeColour)) designEyeColour = name;
        }

        if (odd) {
            float y = SectionHeaderHeight + rows + gap;
            y = DrawSectionHeader(y, width, "CS_Kandra_RightEyeHeader");

            for (int i = 0; i < stones.Count; i++) {
                (string name, string hex, string labelKey) = stones[i];

                // The left eye's own stone is not an odd eye, and picking it would say so in words.
                if (name == designEyeColour) continue;

                Rect row = new Rect(
                    (i % SwatchColumns) * (column + gap),
                    y + ((i / SwatchColumns) * SwatchRowHeight),
                    column,
                    SwatchRowHeight - 2f
                );

                if (DrawSwatch(row, hex, labelKey.Translate(), name == designEyeColourTwo)) {
                    designEyeColourTwo = name;
                }
            }
        }

        Widgets.EndScrollView();
    }

    private void DrawPreviewPlate(Rect plate) {
        Widgets.DrawMenuSection(plate);

        Rect inner = plate.ContractedBy(Spacing.Get());
        Rect preview = new Rect(inner.x, inner.y, PreviewSize.x, PreviewSize.y);

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, bodyTextColor)) {
            Widgets.Label(new Rect(preview.x, preview.yMax, preview.width, 18f), "CS_Kandra_ReshapeAfter".Translate());
        }

        // Held in a local because the summary beside it paints the same colour on its build line.
        Color material = DesignColour();

        DrawPortrait(
            preview,
            forms.TrueBody,
            PreviewSize,
            1.12f,
            material,
            designGender,
            designHair,
            designHairColour,
            designEyeColour,
            designEyeColourTwo,
            designIrisSize,
            designEyeLight
        );

        float x = preview.xMax + Spacing.Get();
        float width = inner.xMax - x;
        Rect cost = new Rect(x, inner.yMax - ButtonHeight - 18f, width, 18f);
        DrawSummary(new Rect(x, inner.y, width, cost.y - inner.y), material);

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            Widgets.Label(cost, "CS_Kandra_ReshapeCost".Translate());
        }

        Rect commit = new Rect(x, cost.yMax, width, ButtonHeight);
        if (!CTAButtonText(commit, "CS_Kandra_Reshape".Translate())) return;
        if (designMaterial == null) return;

        // Written to the comp now, applied when the ten seconds are up. Wearing a face is the same deal.
        forms.BeginReshape(
            designMaterial,
            designGender,
            designHair,
            designHairColour,
            designEyeColour,
            designEyeColourTwo,
            designIrisSize,
            designEyeLight
        );

        choose(JobDriver.KandraChangeShape.ReshapeIndex);
        Close();
    }

    /// <summary>
    ///     What the button is about to commit, in the space the gender toggle used to hold. Iris
    ///     size is not a line: the portrait beside it already shows the size, and saying it too
    ///     would double the eye lines the translators have to carry.
    /// </summary>
    private void DrawSummary(Rect region, Color material) {
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, headerTextColor)) {
            Widgets.Label(
                new Rect(region.x, region.y, region.width, SummaryNameHeight),
                "CS_Kandra_SummaryName".Translate((Pawn?.LabelShortCap ?? string.Empty).Named("PAWN"))
            );
        }

        float y = region.y + SummaryNameHeight;

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            y = SummaryLine(region, y, material, BuildSummary());

            TaggedString? hair = HairSummary();
            if (hair != null) y = SummaryLine(region, y, HairTint(), hair.Value);

            TaggedString? eyes = EyeSummary();
            if (eyes == null) return;

            SummaryLine(region, y, KandraAppearance.EyeDrawColorFor(designEyeColour, designEyeLight), eyes.Value);
        }
    }

    /// <summary>One summary row: the colour it commits, then what that colour reads as.</summary>
    private static float SummaryLine(Rect region, float y, Color colour, TaggedString text) {
        Rect swatch = new Rect(region.x, y + 4f, SummarySwatchSize, SummarySwatchSize);
        Widgets.DrawBoxSolid(swatch, colour);

        Widgets.Label(
            new Rect(swatch.xMax + 6f, y, region.width - SummarySwatchSize - 6f, SummaryLineHeight),
            text
        );

        return y + SummaryLineHeight;
    }

    /// <summary>What the body is shaped as, and what it is cut from.</summary>
    private TaggedString BuildSummary() {
        return "CS_Kandra_SummaryBuild".Translate(
            designGender.GetLabel().CapitalizeFirst().Named("GENDER"),
            MaterialLabel(designMaterial ?? string.Empty).Named("MATERIAL")
        );
    }

    /// <summary>The hair line: the cut alone while no dye is named, or nothing while there is no cut.</summary>
    private TaggedString? HairSummary() {
        if (designHair == null) return null;

        (string name, string hex, string labelKey)? dye = KandraAppearance.FindHairColour(designHairColour);
        if (dye == null) return designHair.LabelCap;

        return "CS_Kandra_SummaryHair".Translate(
            designHair.LabelCap.Named("HAIR"),
            dye.Value.labelKey.Translate().Named("DYE")
        );
    }

    /// <summary>
    ///     One of four lines, by whether the eyes are odd and whether they carry light. A name the
    ///     table no longer has, and both sentinels, all read as off.
    /// </summary>
    private TaggedString? EyeSummary() {
        (string name, string hex, string labelKey)? stone = KandraAppearance.FindEyeColour(designEyeColour);
        if (stone == null) return null;

        (string name, string hex, string labelKey)? second = KandraAppearance.FindEyeColour(designEyeColourTwo);
        (string name, float strength, string labelKey)? light = KandraAppearance.FindEyeLight(designEyeLight);
        NamedArgument colour = stone.Value.labelKey.Translate().Named("COLOUR");
        NamedArgument iris = IrisLabel().Named("IRIS");

        if (second == null) {
            return light == null
                ? "CS_Kandra_SummaryEyes".Translate(iris, colour)
                : "CS_Kandra_SummaryEyesLit".Translate(
                    iris,
                    colour,
                    light.Value.labelKey.Translate().Named("LIGHT")
                );
        }

        NamedArgument other = second.Value.labelKey.Translate().Named("COLOURTWO");

        return light == null
            ? "CS_Kandra_SummaryEyesOdd".Translate(iris, colour, other)
            : "CS_Kandra_SummaryEyesOddLit".Translate(
                iris,
                colour,
                other,
                light.Value.labelKey.Translate().Named("LIGHT")
            );
    }

    /// <summary>The iris size, falling back to the table's default when nothing is picked.</summary>
    private TaggedString IrisLabel() {
        (string name, float scale, string labelKey)? size =
            KandraAppearance.FindIrisSize(designIrisSize) ?? KandraAppearance.FindIrisSize(KandraAppearance.DefaultIris);

        return (size?.labelKey ?? KandraAppearance.AllIrisSizes[0].labelKey).Translate();
    }

    /// <summary>Says Male and Female because it really does set the pawn's gender.</summary>
    private void DrawGenderOption(Rect rect, Gender gender) {
        bool selected = designGender == gender;

        if (selected) {
            Widgets.DrawHighlightSelected(rect);
        } else {
            Widgets.DrawHighlightIfMouseover(rect);
        }

        Rect thumb = new Rect(rect.x + 8f, rect.y + ((rect.height - 28f) / 2f), 22f, 28f);
        Color previous = GUI.color;
        GUI.color = DesignColour();
        GUI.DrawTexture(thumb, gender == Gender.Female ? FemaleTrueBody : MaleTrueBody, ScaleMode.ScaleToFit);
        GUI.color = previous;

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, selected ? BorderColor : bodyTextColor)) {
            Widgets.Label(new Rect(thumb.xMax + 6f, rect.y, rect.width - 36f, rect.height), gender.GetLabel().CapitalizeFirst());
        }

        Verse.Sound.MouseoverSounds.DoRegion(rect);
        if (Widgets.ButtonInvisible(rect)) designGender = gender;
    }

    /// <summary>
    ///     Eighteen materials in three named groups. Grouping is what keeps the choice readable,
    ///     and every colour on offer is a row the gamut test has already cleared.
    /// </summary>
    private void DrawPalette(Rect inner) {
        float gap = Spacing.Get(0.5f);
        float width = ColumnContentWidth(inner);
        float column = (width - (gap * (SwatchColumns - 1))) / SwatchColumns;

        Widgets.BeginScrollView(inner, ref materialScroll, new Rect(0f, 0f, width, PaletteHeight(gap)));

        float y = 0f;

        foreach ((string group, string labelKey) in KandraAppearance.MaterialGroups) {
            y = DrawSectionHeader(y, width, labelKey);
            int shown = 0;

            foreach ((string name, string hex, float _, string label, string rowGroup) in KandraAppearance.AllMaterials) {
                if (rowGroup != group) continue;

                Rect row = new Rect(
                    (shown % SwatchColumns) * (column + gap),
                    y + ((shown / SwatchColumns) * SwatchRowHeight),
                    column,
                    SwatchRowHeight - 2f
                );

                TaggedString text = label.Translate();
                string tip = (string)"CS_Kandra_MaterialTip".Translate(text.Named("MATERIAL"));

                if (DrawSwatch(row, hex, text, name == designMaterial, tip)) designMaterial = name;

                shown++;
            }

            y += Mathf.CeilToInt(shown / (float)SwatchColumns) * SwatchRowHeight;
            y += gap;
        }

        Widgets.EndScrollView();
    }

    /// <summary>How tall the material palette is, so its own scroll view knows what it holds.</summary>
    private static float PaletteHeight(float gap) {
        float height = 0f;

        foreach ((string group, string _) in KandraAppearance.MaterialGroups) {
            int count = 0;

            foreach ((string _, string _, float _, string _, string rowGroup) in KandraAppearance.AllMaterials) {
                if (rowGroup == group) count++;
            }

            height += SectionHeaderHeight + (Mathf.CeilToInt(count / (float)SwatchColumns) * SwatchRowHeight) + gap;
        }

        return height;
    }

    /// <summary>Every loaded hair, drawn in whatever dye the column beside it has selected.</summary>
    private void DrawHairGrid(Rect inner) {
        float gap = Spacing.Get(0.5f);
        float width = ColumnContentWidth(inner);
        float cell = (width - (gap * (HairColumns - 1))) / HairColumns;

        List<HairDef> loaded = Hairs;
        float grid = Mathf.CeilToInt(loaded.Count / (float)HairColumns) * (HairCellHeight + gap);

        Widgets.BeginScrollView(inner, ref hairScroll, new Rect(0f, 0f, width, SectionHeaderHeight + grid));

        DrawSectionHeader(0f, width, "CS_Kandra_HairHeader");

        Color tint = HairTint();

        for (int i = 0; i < loaded.Count; i++) {
            Rect box = new Rect(
                (i % HairColumns) * (cell + gap),
                SectionHeaderHeight + ((i / HairColumns) * (HairCellHeight + gap)),
                cell,
                HairCellHeight
            );

            // Eighty hairs with expansions loaded, so the rows off screen are never drawn.
            if (box.yMax < hairScroll.y || box.y > hairScroll.y + inner.height) continue;

            DrawHairCell(box, loaded[i], tint);
        }

        Widgets.EndScrollView();
    }

    /// <summary>The dye the grid beside it is drawn in, as the same chip rows the materials use.</summary>
    private void DrawHairDyes(Rect inner) {
        float width = ColumnContentWidth(inner);
        IReadOnlyList<(string name, string hex, string labelKey)> dyes = KandraAppearance.AllHairColours;

        Widgets.BeginScrollView(
            inner,
            ref dyeScroll,
            new Rect(0f, 0f, width, SectionHeaderHeight + (dyes.Count * SwatchRowHeight))
        );

        DrawSectionHeader(0f, width, "CS_Kandra_HairColourHeader");

        for (int i = 0; i < dyes.Count; i++) {
            (string name, string hex, string labelKey) = dyes[i];
            Rect row = new Rect(0f, SectionHeaderHeight + (i * SwatchRowHeight), width, SwatchRowHeight - 2f);

            if (DrawSwatch(row, hex, labelKey.Translate(), name == designHairColour)) designHairColour = name;
        }

        Widgets.EndScrollView();
    }

    /// <summary>A hair whose sheet will not load draws as an empty cell rather than throwing.</summary>
    private void DrawHairCell(Rect cell, HairDef hair, Color tint) {
        bool selected = hair == designHair;

        if (selected) {
            Widgets.DrawHighlightSelected(cell);
        } else {
            Widgets.DrawHighlightIfMouseover(cell);
        }

        Texture? mane = HairTexture(hair);

        if (mane != null) {
            Rect thumb = new Rect(
                cell.x + ((cell.width - HairThumbSize) / 2f),
                cell.y + 2f,
                HairThumbSize,
                HairThumbSize
            );

            Color previous = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(thumb, mane, ScaleMode.ScaleToFit);
            GUI.color = previous;
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, selected ? BorderColor : bodyTextColor)) {
            Widgets.Label(
                new Rect(cell.x + 2f, cell.y + HairThumbSize + 4f, cell.width - 4f, cell.height - HairThumbSize - 6f),
                hair.LabelCap
            );
        }

        Verse.Sound.MouseoverSounds.DoRegion(cell);
        if (Widgets.ButtonInvisible(cell)) designHair = hair;
    }

    /// <summary>One chip row, shared by the material palette and the dye palette.</summary>
    private static bool DrawSwatch(Rect row, string hex, TaggedString label, bool selected, string? tip = null) {
        if (selected) {
            Widgets.DrawHighlightSelected(row);
        } else {
            Widgets.DrawHighlightIfMouseover(row);
        }

        Rect chip = new Rect(row.x + 4f, row.y + ((row.height - 14f) / 2f), 14f, 14f);
        Widgets.DrawBoxSolid(chip, Parse(hex));

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, selected ? BorderColor : BodyTextColor)) {
            Widgets.Label(new Rect(chip.xMax + 6f, row.y, row.width - 26f, row.height), label);
        }

        if (!tip.NullOrEmpty()) TooltipHandler.TipRegion(row, tip);
        Verse.Sound.MouseoverSounds.DoRegion(row);

        return Widgets.ButtonInvisible(row);
    }

    /// <summary>The dye the preview and the grid draw in, falling back to what the body already wears.</summary>
    private Color HairTint() {
        return designHairColour == null
            ? forms.TrueBody?.hairColour ?? Color.white
            : KandraAppearance.HairColorFor(designHairColour);
    }

    private Color DesignColour() {
        (string name, string hex, float weight, string labelKey, string group)? row =
            KandraAppearance.FindMaterial(designMaterial);

        return row == null ? Color.white : Parse(row.Value.hex);
    }

    /// <summary>Every colour in this window comes out of the material table. None is written here.</summary>
    private static Color Parse(string hex) {
        return ColorUtility.TryParseHtmlString("#" + hex, out Color colour) ? colour : Color.white;
    }

    private void DrawGrid(Rect region) {
        IReadOnlyList<KandraForm> known = forms.Known;
        float width = region.width - ScrollbarWidth;

        // The pane widens when the true body designer is open, so the grid counts its own columns.
        int columns = Mathf.Max(1, Mathf.FloorToInt((width + CardGap) / (CardWidth + CardGap)));
        int rows = Mathf.CeilToInt(known.Count / (float)columns);

        Rect view = new Rect(0f, 0f, width, rows * (CardHeight + CardGap));
        Widgets.BeginScrollView(region, ref scroll, view);

        for (int i = 0; i < known.Count; i++) {
            Rect card = new Rect(
                (i % columns) * (CardWidth + CardGap),
                (i / columns) * (CardHeight + CardGap),
                CardWidth,
                CardHeight
            );

            // Culling matters here. A kandra centuries old can have a great many faces.
            if (card.yMax < scroll.y || card.y > scroll.y + region.height) continue;

            DrawCard(card, known[i], i);
        }

        Widgets.EndScrollView();
    }

    private void DrawCard(Rect card, KandraForm form, int index) {
        Widgets.DrawMenuSection(card);
        DrawPortrait(new Rect(card.x + 5f, card.y + 6f, CardPortrait.x, CardPortrait.y), form, CardPortrait, 1.2f);

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, bodyTextColor)) {
            Widgets.Label(new Rect(card.x + 4f, card.y + 93f, card.width - 8f, 26f), form.Label);
        }

        DrawBadges(card, form);

        // Inert behind the confirmation, or a stray click wears a form and drops the question.
        if (confirming != null) return;

        Widgets.DrawHighlightIfMouseover(card);
        TooltipHandler.TipRegion(card, () => Tooltip(form), card.GetHashCode());
        Verse.Sound.MouseoverSounds.DoRegion(card);

        if (!Widgets.ButtonInvisible(card)) return;

        if (picking) {
            confirming = form;
            picking = false;

            return;
        }

        choose(index);
        Close();
    }

    /// <summary>
    ///     The body it gives up beside the one it takes. Drawn in the rail rather than over the
    ///     window, because a kandra dialog never stacks a second one.
    /// </summary>
    private void DrawConfirm(Rect rail, KandraForm form) {
        Widgets.DrawMenuSection(rail);

        float x = rail.x + RailPad;
        float width = rail.width - (RailPad * 2f);
        float y = rail.y + RailPad;

        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, headerTextColor)) {
            Rect title = new Rect(x, y, width, Text.CalcHeight("CS_Kandra_AdoptTitle".Translate(), width));
            Widgets.Label(title, "CS_Kandra_AdoptTitle".Translate());
            y = title.yMax + 8f;
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, bodyTextColor)) {
            Rect blurb = new Rect(x, y, width, Text.CalcHeight("CS_Kandra_AdoptBody".Translate(), width));
            Widgets.Label(blurb, "CS_Kandra_AdoptBody".Translate());
            y = blurb.yMax + 12f;
        }

        Rect was = new Rect(x, y, 98f, 96f);
        Rect now = new Rect(x + width - 98f, y, 98f, 96f);
        DrawPortrait(was, forms.TrueBody, new Vector2(98f, 96f), 1.05f);
        DrawPortrait(now, form, new Vector2(98f, 96f), 1.05f);

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, bodyTextColor)) {
            Widgets.Label(new Rect(was.x, was.yMax + 4f, was.width, 28f), CurrentTrueLabel());
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, BorderColor)) {
            Widgets.Label(new Rect(now.x, now.yMax + 4f, now.width, 28f), form.Label);
        }

        Rect keep = new Rect(x, rail.yMax - RailPad - (ButtonHeight * 2f) - 8f, width, ButtonHeight);
        if (Widgets.ButtonText(keep, "CS_Kandra_AdoptKeep".Translate())) confirming = null;

        Rect take = new Rect(x, keep.yMax + 8f, width, ButtonHeight);
        if (!CTAButtonText(take, "CS_Kandra_AdoptTake".Translate())) return;

        forms.AdoptTrueBody(form);
        confirming = null;
    }

    private string CurrentTrueLabel() {
        if (!forms.TrueBodyCrafted) return forms.TrueBody?.Label ?? string.Empty;

        Pawn? pawn = Pawn;

        return pawn == null ? string.Empty : MaterialLabel(KandraAppearance.TrueBodyMaterialFor(pawn).name);
    }

    private void DrawEmpty(Rect region) {
        using (new TextBlock(GameFont.Small, TextAnchor.UpperCenter, headerTextColor)) {
            Widgets.Label(new Rect(region.x, region.y + 150f, region.width, 24f), "CS_Kandra_NoFormsYet".Translate());
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperCenter, bodyTextColor)) {
            Widgets.Label(
                new Rect(region.x + 56f, region.y + 178f, region.width - 112f, 60f),
                "CS_Kandra_NoFormsYetHint".Translate()
            );
        }
    }

    private void DrawPortrait(
        Rect well,
        KandraForm? form,
        Vector2 size,
        float zoom,
        Color? tint = null,
        Gender? gender = null,
        HairDef? hair = null,
        string? hairColour = null,
        string? eyeColour = null,
        string? eyeColourTwo = null,
        string? irisSize = null,
        string? eyeLight = null
    ) {
        Widgets.DrawBoxSolid(well, Widgets.WindowBGFillColor);

        // A generated stand-in arrives dressed and human, which is the one thing this is not.
        if (form is { crafted: true }) {
            DrawTrueBody(well, form, size, tint, gender, hair, hairColour, eyeColour, eyeColourTwo, irisSize, eyeLight);

            return;
        }

        Pawn? pawn = form?.PortraitPawn;
        if (pawn == null) {
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, bodyTextColor)) {
                Widgets.Label(well, "CS_Kandra_NoPortrait".Translate());
            }

            return;
        }

        Rect inner = new Rect(
            well.x + ((well.width - size.x) / 2f),
            well.y + ((well.height - size.y) / 2f),
            size.x,
            size.y
        );
        GUI.DrawTexture(inner, PortraitsCache.Get(pawn, size, Rot4.South, default, zoom));
    }

    /// <summary>
    ///     The crafted body drawn from its own textures, in the material it is made of. Body and
    ///     head are separate files, and the head rides where this pawn's body type puts one.
    /// </summary>
    private void DrawTrueBody(
        Rect well,
        KandraForm form,
        Vector2 size,
        Color? tint,
        Gender? gender,
        HairDef? hair,
        string? hairColour,
        string? eyeColour,
        string? eyeColourTwo,
        string? irisSize,
        string? eyeLight
    ) {
        Pawn? pawn = Pawn;
        if (pawn == null) return;

        // A worn disguise puts its gender on the pawn, so only the form knows the real one.
        bool female = (gender ?? form.gender) == Gender.Female;
        float side = Mathf.Min(size.x, size.y);
        Rect body = new Rect(
            well.x + ((well.width - side) / 2f),
            well.y + ((well.height - side) / 2f),
            side,
            side
        );

        BodyTypeDef? shape = form.bodyType ?? pawn.story?.bodyType;
        float lift = shape == null ? 0f : side * (shape.headOffset.y / BodyMeshUnits);
        Rect head = new Rect(body.x, body.y - lift, side, side);

        Color previous = GUI.color;
        GUI.color = tint ?? KandraAppearance.TrueBodyColorFor(pawn);
        GUI.DrawTexture(body, female ? FemaleTrueBody : MaleTrueBody, ScaleMode.ScaleToFit);
        GUI.DrawTexture(head, female ? FemaleTrueHead : MaleTrueHead, ScaleMode.ScaleToFit);

        // Under the hair, because hair is what covers them. The iris rides the head's own quad.
        string? stone = eyeColour ?? form.eyeColourName;
        string? iris = irisSize ?? form.irisSizeName;
        string? light = eyeLight ?? form.eyeLightName;

        if (stone != null) {
            DrawIris(head, IrisLeft(female, iris), stone, light);

            // A second stone cuts the right eye alone, and none of one means the pair matches.
            string? second = eyeColourTwo ?? form.eyeColourTwoName;
            DrawIris(
                head,
                IrisRight(female, iris),
                second is null or KandraAppearance.EyeColourNone ? stone : second,
                light
            );
        }

        // A kandra grows its own hair, so it keeps its own colour rather than the body material.
        Texture? mane = HairTexture(hair ?? form.hair ?? pawn.story?.hairDef);
        if (mane != null) {
            GUI.color = hairColour == null ? form.hairColour : KandraAppearance.HairColorFor(hairColour);
            GUI.DrawTexture(head, mane, ScaleMode.ScaleToFit);
        }

        GUI.color = previous;
    }

    /// <summary>
    ///     The south-facing hair sheet, through GraphicDatabase so nothing hits ContentFinder on a
    ///     draw call. Null for a bald head or a path that will not load.
    /// </summary>
    private static Texture? HairTexture(HairDef? hair) {
        if (hair == null || hair.texPath.NullOrEmpty()) return null;

        try {
            return GraphicDatabase.Get<Graphic_Multi>(
                hair.texPath,
                ShaderDatabase.Cutout,
                Vector2.one,
                Color.white
            ).MatSouth.mainTexture;
        } catch (global::System.Exception) {
            return null;
        }
    }

    /// <summary>The left iris at a gender and size, falling back to the standard art.</summary>
    private static Texture2D IrisLeft(bool female, string? size) {
        return Iris(female, size, false) ?? (female ? FemaleIrisLeft : MaleIrisLeft);
    }

    /// <summary>The right iris. A second stone changes its colour, never its size.</summary>
    private static Texture2D IrisRight(bool female, string? size) {
        return Iris(female, size, true) ?? (female ? FemaleIrisRight : MaleIrisRight);
    }

    /// <summary>The art for one iris, or null when the size names none the table has.</summary>
    private static Texture2D? Iris(bool female, string? size, bool right) {
        if (size == null) return null;

        return IrisArt.TryGetValue(IrisKey(female, size, right), out Texture2D? found) ? found : null;
    }

    /// <summary>Every size the table carries, in both genders and both sides, read once at load.</summary>
    private static Dictionary<string, Texture2D> BuildIrisArt() {
        Dictionary<string, Texture2D> art = new Dictionary<string, Texture2D>();

        foreach ((string size, float _, string _) in KandraAppearance.AllIrisSizes) {
            foreach (bool female in new[] { false, true }) {
                foreach (bool right in new[] { false, true }) {
                    string key = IrisKey(female, size, right);
                    Texture2D? found = ContentFinder<Texture2D>.Get(IrisDir + "Kandra_Eyes_" + key + "_south", false);
                    if (found != null) art[key] = found;
                }
            }
        }

        return art;
    }

    /// <summary>The file name for one iris, which is also how the art above is keyed.</summary>
    private static string IrisKey(bool female, string size, bool right) {
        return (female ? "Female_" : "Male_") + size.CapitalizeFirst() + (right ? "_Right" : "_Left");
    }

    /// <summary>Faction on the left, ideoligion on the right, both along the bottom edge.</summary>
    private static void DrawBadges(Rect card, KandraForm form) {
        float y = card.yMax - 19f;
        Color previous = GUI.color;

        if (form.faction?.FactionIcon != null) {
            GUI.color = form.faction.DefaultColor;
            GUI.DrawTexture(new Rect(card.x + 5f, y, 14f, 14f), form.faction.FactionIcon);
        }

        if (form.ideo?.Icon != null) {
            GUI.color = form.ideo.Color;
            GUI.DrawTexture(new Rect(card.xMax - 19f, y, 14f, 14f), form.ideo.Icon);
        }

        GUI.color = previous;
    }

    /// <summary>
    ///     Four lines that all look alike are four lines nobody reads. Each carries its own colour
    ///     so the eye can go straight to the one it wants.
    /// </summary>
    private string Tooltip(KandraForm form) {
        global::System.Text.StringBuilder text = new global::System.Text.StringBuilder();

        text.AppendLine(Tinted(form.nameFull ?? form.Label, BorderColor));

        if (form.faction != null) text.AppendLine(Tinted(form.faction.LabelCap, form.faction.DefaultColor));
        if (form.ideo != null) text.AppendLine(Tinted(form.ideo.name, form.ideo.Color));
        if (form.xenotype != null) text.AppendLine(Tinted(form.xenotype.LabelCap, BodyTextColor));

        text.AppendLine();
        text.Append((picking ? "CS_Kandra_PickTrueTip" : "CS_Kandra_PickFormTip").Translate().Resolve());

        return text.ToString();
    }

    private static string Tinted(string body, Color colour) {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(colour) + ">" + body + "</color>";
    }
}
