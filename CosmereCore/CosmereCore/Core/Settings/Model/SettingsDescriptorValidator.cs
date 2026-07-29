using global::System.Collections.Generic;

namespace Cosmere.Core.Settings.Model;

public static class SettingsDescriptorValidator {
    public static IReadOnlyList<string> Validate(string systemKey, IReadOnlyList<SettingSection> sections) {
        List<string> errors = [];
        HashSet<string> sectionKeys = new HashSet<string>(global::System.StringComparer.Ordinal);

        foreach (SettingSection section in sections) {
            string sectionPath = systemKey + "/" + section.Key;
            if (!sectionKeys.Add(section.Key)) errors.Add($"{systemKey}: duplicate section key '{section.Key}'");

            HashSet<string> settingKeys = new HashSet<string>(global::System.StringComparer.Ordinal);
            foreach (SettingDescriptor descriptor in section.Settings) {
                string descriptorPath = sectionPath + "/" + descriptor.Key;
                if (!settingKeys.Add(descriptor.Key)) errors.Add($"{sectionPath}: duplicate setting key '{descriptor.Key}'");
                ValidateControl(descriptorPath, descriptor.Control, errors);
            }
        }

        return errors;
    }

    private static void ValidateControl(string path, SettingControl control, List<string> errors) {
        try {
            switch (control) {
                case CheckboxControl checkbox:
                    _ = checkbox.Value;
                    return;
                case SliderControl slider:
                    ValidateSlider(path, slider, errors);
                    return;
                case TicksControl ticks:
                    ValidateTicks(path, ticks, errors);
                    return;
                case IntRangeControl range:
                    ValidateRange(path, range, errors);
                    return;
                case ChoiceControl choice:
                    ValidateChoice(path, choice, errors);
                    return;
                case ButtonControl button:
                    _ = button.StatusKey();
                    return;
            }
        } catch (global::System.Exception exception) {
            errors.Add($"{path}: failed to read descriptor state ({exception.GetType().Name})");
        }
    }

    private static void ValidateSlider(string path, SliderControl slider, List<string> errors) {
        if (!IsFinite(slider.Default)) {
            errors.Add($"{path}: slider default must be finite");
        } else if (slider.Default < slider.Min || slider.Default > slider.Max) {
            errors.Add($"{path}: slider default {slider.Default} is outside [{slider.Min}, {slider.Max}]");
        }

        _ = slider.Value;
    }

    private static void ValidateTicks(string path, TicksControl ticks, List<string> errors) {
        if (!IsFinite(ticks.Default)) {
            errors.Add($"{path}: ticks default must be finite");
        } else if (ticks.Default < ticks.MinTicks || ticks.Default > ticks.MaxTicks) {
            errors.Add($"{path}: ticks default {ticks.Default} is outside [{ticks.MinTicks}, {ticks.MaxTicks}]");
        }

        _ = ticks.Value;
    }

    private static void ValidateRange(string path, IntRangeControl range, List<string> errors) {
        if (range.DefaultMin > range.DefaultMax) {
            errors.Add($"{path}: range default minimum {range.DefaultMin} exceeds maximum {range.DefaultMax}");
        }

        _ = range.Minimum;
        _ = range.Maximum;
    }

    private static void ValidateChoice(string path, ChoiceControl choice, List<string> errors) {
        IReadOnlyList<Choice> options;
        try {
            options = choice.Options();
        } catch (global::System.Exception exception) {
            errors.Add($"{path}: failed to read choice options ({exception.GetType().Name})");
            return;
        }

        HashSet<string> optionValues = new HashSet<string>(global::System.StringComparer.Ordinal);
        foreach (Choice option in options) {
            if (!optionValues.Add(option.Value)) errors.Add($"{path}: duplicate choice value '{option.Value}'");
        }

        if (!choice.AllowNone && (choice.Default is null || !optionValues.Contains(choice.Default))) {
            string defaultValue = choice.Default ?? "null";
            errors.Add($"{path}: choice default '{defaultValue}' is not an option");
        }

        _ = choice.Value;
    }

    private static bool IsFinite(float value) {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
