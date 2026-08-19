using Cosmere.Core.Settings.Model;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Skin;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Window;

public sealed class SettingsControlRenderer {
    private const float CheckboxSize = 24f;
    private const float StandardControlHeight = 28f;
    private const float ButtonHeight = 24f;
    private const float ButtonStatusHeight = 14f;
    private const float ReadbackWidth = 52f;
    private const float NumericFieldWidth = 74f;
    private const float RangeSeparatorWidth = 26f;

    private readonly Dictionary<string, string> numericBuffers = [];

    public float HeightFor(SettingControl control) {
        return control is ButtonControl ? ButtonHeight + ButtonStatusHeight + 4f : StandardControlHeight;
    }

    public void Draw(
        Rect rect,
        SettingControl control,
        string stateKey,
        ISystemSkin skin,
        bool enabled,
        string? disabledReasonKey
    ) {
        // controls stay centred at their declared height - given the full row, a dropdown would grow 3 lines tall.
        float controlHeight = Mathf.Min(HeightFor(control), rect.height);
        rect = new Rect(rect.x, rect.y + (rect.height - controlHeight) / 2f, rect.width, controlHeight);

        switch (control) {
            case CheckboxControl checkbox:
                DrawCheckbox(rect, checkbox, enabled);
                return;
            case SliderControl slider:
                DrawSlider(rect, slider, skin, enabled);
                return;
            case TicksControl ticks:
                DrawTicks(rect, ticks, stateKey, skin, enabled);
                return;
            case IntRangeControl range:
                DrawRange(rect, range, stateKey, skin, enabled);
                return;
            case ChoiceControl choice:
                DrawChoice(rect, choice, skin, enabled, disabledReasonKey);
                return;
            case ButtonControl button:
                DrawButton(rect, button, skin, enabled, disabledReasonKey);
                return;
            default:
                DrawUnavailable(rect, skin);
                return;
        }
    }

    public void DrawUnavailable(Rect rect, ISystemSkin skin) {
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleRight, DisabledTextColor(skin))) {
            Widgets.Label(rect, "CC_Settings_Unavailable".Translate());
        }
    }

    private static void DrawCheckbox(Rect rect, CheckboxControl control, bool enabled) {
        Rect checkboxRect = new Rect(rect.xMax - CheckboxSize, rect.y + (rect.height - CheckboxSize) / 2f, CheckboxSize, CheckboxSize);
        Widgets.CheckboxDraw(checkboxRect.x, checkboxRect.y, control.Value, !enabled, CheckboxSize);

        if (!enabled) return;

        Widgets.DrawHighlightIfMouseover(rect);
        MouseoverSounds.DoRegion(rect);
        if (Widgets.ButtonInvisible(rect)) control.Value = !control.Value;
    }

    private static void DrawSlider(Rect rect, SliderControl control, ISystemSkin skin, bool enabled) {
        float value = control.Value;
        Rect sliderRect = new Rect(rect.x, rect.y + 4f, rect.width - ReadbackWidth - 8f, rect.height - 8f);
        Rect readbackRect = new Rect(sliderRect.xMax + 8f, rect.y, ReadbackWidth, rect.height);

        if (enabled) {
            Widgets.DrawHighlightIfMouseover(sliderRect);
            MouseoverSounds.DoRegion(sliderRect);
            float updated = Widgets.HorizontalSlider(sliderRect, value, control.Min, control.Max, false, null, null, null, -1f);
            if (control.Step.HasValue) {
                updated = control.Min + Mathf.Round((updated - control.Min) / control.Step.Value) * control.Step.Value;
            }

            if (!Mathf.Approximately(updated, value)) control.Value = updated;
        } else {
            Widgets.DrawBoxSolid(sliderRect, new Color(0.08f, 0.08f, 0.09f, 0.45f));
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleRight, enabled ? skin.HeaderTextColor : DisabledTextColor(skin))) {
            Widgets.Label(readbackRect, control.Format(control.Value));
        }
    }

    private void DrawTicks(Rect rect, TicksControl control, string stateKey, ISystemSkin skin, bool enabled) {
        float ticksPerUnit = control.EditUnit == TickUnit.Hours ? GenDate.TicksPerHour : GenDate.TicksPerDay;
        float valueInUnits = control.Value / ticksPerUnit;
        float minimum = control.MinTicks / ticksPerUnit;
        float maximum = control.MaxTicks / ticksPerUnit;
        Rect fieldRect = new Rect(rect.x, rect.y, NumericFieldWidth, rect.height);
        Rect displayRect = new Rect(fieldRect.xMax + 8f, rect.y, rect.xMax - fieldRect.xMax - 8f, rect.height);

        if (enabled) {
            TooltipHandler.TipRegion(fieldRect, control.DisplayText);
            Widgets.DrawHighlightIfMouseover(fieldRect);
            MouseoverSounds.DoRegion(fieldRect);
            string buffer = BufferFor(stateKey, valueInUnits, "0.##");
            GUI.SetNextControlName(stateKey);
            Widgets.TextFieldNumeric(fieldRect, ref valueInUnits, ref buffer, minimum, maximum);
            numericBuffers[stateKey] = buffer;
            float ticks = valueInUnits * ticksPerUnit;
            if (!Mathf.Approximately(ticks, control.Value)) control.Value = ticks;
        } else {
            DrawDisabledNumber(fieldRect, valueInUnits.ToString("0.##"), skin);
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, enabled ? skin.HeaderTextColor : DisabledTextColor(skin))) {
            Widgets.Label(displayRect, control.DisplayText);
        }
    }

    private void DrawRange(Rect rect, IntRangeControl control, string stateKey, ISystemSkin skin, bool enabled) {
        float fieldWidth = (rect.width - RangeSeparatorWidth - 8f) / 2f;
        Rect minimumRect = new Rect(rect.x, rect.y, fieldWidth, rect.height);
        Rect separatorRect = new Rect(minimumRect.xMax + 4f, rect.y, RangeSeparatorWidth, rect.height);
        Rect maximumRect = new Rect(separatorRect.xMax + 4f, rect.y, fieldWidth, rect.height);
        int minimum = control.Minimum;
        int maximum = control.Maximum;

        if (enabled) {
            TooltipHandler.TipRegion(minimumRect, "CC_Settings_Range_Minimum".Translate());
            Widgets.DrawHighlightIfMouseover(minimumRect);
            MouseoverSounds.DoRegion(minimumRect);
            string minimumKey = stateKey + "/minimum";
            string minimumBuffer = BufferFor(minimumKey, minimum, "0");
            GUI.SetNextControlName(minimumKey);
            Widgets.TextFieldNumeric(minimumRect, ref minimum, ref minimumBuffer, control.HardMin, control.HardMax);
            numericBuffers[minimumKey] = minimumBuffer;
            if (minimum != control.Minimum) control.Minimum = minimum;

            TooltipHandler.TipRegion(maximumRect, "CC_Settings_Range_Maximum".Translate());
            Widgets.DrawHighlightIfMouseover(maximumRect);
            MouseoverSounds.DoRegion(maximumRect);
            string maximumKey = stateKey + "/maximum";
            string maximumBuffer = BufferFor(maximumKey, maximum, "0");
            GUI.SetNextControlName(maximumKey);
            Widgets.TextFieldNumeric(maximumRect, ref maximum, ref maximumBuffer, control.HardMin, control.HardMax);
            numericBuffers[maximumKey] = maximumBuffer;
            if (maximum != control.Maximum) control.Maximum = maximum;
        } else {
            DrawDisabledNumber(minimumRect, minimum.ToString(), skin);
            DrawDisabledNumber(maximumRect, maximum.ToString(), skin);
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, enabled ? skin.HeaderTextColor : DisabledTextColor(skin))) {
            Widgets.Label(separatorRect, "CC_Settings_Range_To".Translate());
        }
    }

    private static void DrawChoice(
        Rect rect,
        ChoiceControl control,
        ISystemSkin skin,
        bool enabled,
        string? disabledReasonKey
    ) {
        IReadOnlyList<Choice> options = control.Options();
        bool hasOptions = options.Count > 0;
        bool interactive = enabled && hasOptions;
        string label = ChoiceLabel(control.Value, options, control.AllowNone);
        Color textColor = interactive ? skin.HeaderTextColor : DisabledTextColor(skin);

        // dropdowns share the button tan - on plain panel colour they'd read as static, uneditable text.
        Widgets.DrawBoxSolid(rect, new Color(skin.AccentColor.r, skin.AccentColor.g, skin.AccentColor.b, interactive ? 0.32f : 0.14f));
        UIText.EllipsisLabel(rect.ContractedBy(6f, 0f), label, GameFont.Small, TextAnchor.MiddleLeft, textColor);

        if (!interactive) {
            TooltipHandler.TipRegion(rect, DisabledReason(disabledReasonKey));
            return;
        }

        TooltipHandler.TipRegion(rect, label);
        Widgets.DrawHighlightIfMouseover(rect);
        MouseoverSounds.DoRegion(rect);
        if (!Widgets.ButtonInvisible(rect)) return;

        List<FloatMenuOption> menuOptions = new List<FloatMenuOption>(options.Count + (control.AllowNone ? 1 : 0));
        if (control.AllowNone) {
            menuOptions.Add(new FloatMenuOption("CC_Settings_Choice_None".Translate(), () => control.Value = null));
        }

        for (int i = 0; i < options.Count; i++) {
            Choice option = options[i];
            string optionLabel = LabelFor(option);
            menuOptions.Add(new FloatMenuOption(optionLabel, () => control.Value = option.Value));
        }

        Find.WindowStack.Add(new FloatMenu(menuOptions));
    }

    private static void DrawButton(
        Rect rect,
        ButtonControl control,
        ISystemSkin skin,
        bool enabled,
        string? disabledReasonKey
    ) {
        Rect buttonRect = new Rect(rect.x, rect.y, rect.width, ButtonHeight);
        string label = (string)control.LabelKey.Translate();
        string? statusKey = control.StatusKey();

        Widgets.DrawBoxSolid(buttonRect, new Color(skin.AccentColor.r, skin.AccentColor.g, skin.AccentColor.b, enabled ? 0.32f : 0.14f));
        UIText.EllipsisLabel(
            buttonRect.ContractedBy(6f, 0f),
            label,
            GameFont.Small,
            TextAnchor.MiddleCenter,
            enabled ? skin.HeaderTextColor : DisabledTextColor(skin)
        );

        if (enabled) {
            TooltipHandler.TipRegion(buttonRect, label);
            Widgets.DrawHighlightIfMouseover(buttonRect);
            MouseoverSounds.DoRegion(buttonRect);
            if (Widgets.ButtonInvisible(buttonRect)) control.OnClick();
        } else {
            TooltipHandler.TipRegion(buttonRect, DisabledReason(disabledReasonKey));
        }

        if (statusKey.NullOrEmpty()) return;

        Rect statusRect = new Rect(rect.x, buttonRect.yMax + 2f, rect.width, ButtonStatusHeight);
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, enabled ? skin.HeaderTextColor : DisabledTextColor(skin))) {
            Widgets.Label(statusRect, statusKey!.Translate());
        }
    }

    private string BufferFor(string key, float value, string format) {
        bool focused = GUI.GetNameOfFocusedControl() == key;
        if (!focused || !numericBuffers.TryGetValue(key, out string? buffer)) {
            buffer = value.ToString(format);
        }

        return buffer;
    }

    private static string ChoiceLabel(string? selectedValue, IReadOnlyList<Choice> options, bool allowNone) {
        if (selectedValue == null && allowNone) return "CC_Settings_Choice_None".Translate();

        for (int i = 0; i < options.Count; i++) {
            Choice option = options[i];
            if (option.Value == selectedValue) return LabelFor(option);
        }

        return "CC_Settings_Unavailable".Translate();
    }

    private static string LabelFor(Choice choice) {
        return choice.IsLiteral
            ? choice.LiteralDisplayText ?? choice.Value
            : (string)choice.LabelKey!.Translate();
    }

    private static void DrawDisabledNumber(Rect rect, string value, ISystemSkin skin) {
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, DisabledTextColor(skin))) {
            Widgets.Label(rect, value);
        }
    }

    private static string DisabledReason(string? disabledReasonKey) {
        return disabledReasonKey.NullOrEmpty()
            ? "CC_Settings_Unavailable".Translate()
            : disabledReasonKey.Translate();
    }

    private static Color DisabledTextColor(ISystemSkin skin) {
        return new Color(skin.HeaderTextColor.r, skin.HeaderTextColor.g, skin.HeaderTextColor.b, 0.42f);
    }
}
