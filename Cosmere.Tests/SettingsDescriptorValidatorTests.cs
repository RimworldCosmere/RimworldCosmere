using Cosmere.Core.Settings.Model;
using global::System.Collections;
using global::System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class SettingsDescriptorValidatorTests {
    [TestMethod]
    public void DuplicateSectionAndSettingKeysAreReportedWithFullPaths() {
        bool value = false;
        CheckboxControl control = new CheckboxControl(() => value, updated => value = updated, false);
        SettingDescriptor first = new SettingDescriptor("enabled", "Label.One", null, control);
        SettingDescriptor duplicate = new SettingDescriptor("enabled", "Label.Two", null, control);
        SettingSection section = new SettingSection("general", "Section.General", [first, duplicate]);

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate("Core", [section, section]);

        CollectionAssert.Contains((global::System.Collections.ICollection)errors, "Core: duplicate section key 'general'");
        CollectionAssert.Contains((global::System.Collections.ICollection)errors, "Core/general: duplicate setting key 'enabled'");
    }

    [TestMethod]
    public void SliderDefaultOutsideBoundsIsReported() {
        float value = 0f;
        SliderControl control = new SliderControl(() => value, updated => value = updated, 101f, 0f, 100f, null, Format);
        SettingDescriptor descriptor = new SettingDescriptor("value", "Label.Value", null, control);
        SettingSection section = new SettingSection("general", "Section.General", [descriptor]);

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate("Core", [section]);

        CollectionAssert.Contains((global::System.Collections.ICollection)errors, "Core/general/value: slider default 101 is outside [0, 100]");
    }

    [TestMethod]
    public void NonfiniteSliderDefaultIsReported() {
        float value = 0f;
        SliderControl control = new SliderControl(() => value, updated => value = updated, float.NaN, 0f, 100f, null, Format);
        SettingDescriptor descriptor = new SettingDescriptor("value", "Label.Value", null, control);
        SettingSection section = new SettingSection("general", "Section.General", [descriptor]);

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate("Core", [section]);

        CollectionAssert.Contains((global::System.Collections.ICollection)errors, "Core/general/value: slider default must be finite");
    }

    [TestMethod]
    public void TickDefaultBelowMinimumIsReported() {
        float value = 500f;
        TicksControl control = new TicksControl(() => value, updated => value = updated, 499f, 500f, 1000f, TickUnit.Hours);
        SettingDescriptor descriptor = new SettingDescriptor("interval", "Label.Interval", null, control);
        SettingSection section = new SettingSection("general", "Section.General", [descriptor]);

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate("Core", [section]);

        CollectionAssert.Contains((global::System.Collections.ICollection)errors, "Core/general/interval: ticks default 499 is outside [500, 1000]");
    }

    [TestMethod]
    public void IntegerRangeDefaultWithMinimumAboveMaximumIsReported() {
        int minimum = 1;
        int maximum = 30;
        IntRangeControl control = new IntRangeControl(
            () => minimum,
            updated => minimum = updated,
            () => maximum,
            updated => maximum = updated,
            20,
            10,
            1,
            30
        );
        SettingDescriptor descriptor = new SettingDescriptor("range", "Label.Range", null, control);
        SettingSection section = new SettingSection("general", "Section.General", [descriptor]);

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate("Core", [section]);

        CollectionAssert.Contains((global::System.Collections.ICollection)errors, "Core/general/range: range default minimum 20 exceeds maximum 10");
    }

    [TestMethod]
    public void ChoiceDefaultAbsentFromOptionsIsReportedWhenNoneIsNotAllowed() {
        string? value = "first";
        ChoiceControl control = new ChoiceControl(
            () => value,
            updated => value = updated,
            "missing",
            () => [new Choice("first", "Choice.First")],
            false
        );
        SettingDescriptor descriptor = new SettingDescriptor("choice", "Label.Choice", null, control);
        SettingSection section = new SettingSection("general", "Section.General", [descriptor]);

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate("Core", [section]);

        CollectionAssert.Contains((global::System.Collections.ICollection)errors, "Core/general/choice: choice default 'missing' is not an option");
    }

    [TestMethod]
    public void DuplicateChoiceValuesAreReported() {
        string? value = "first";
        ChoiceControl control = new ChoiceControl(
            () => value,
            updated => value = updated,
            "first",
            () => [new Choice("first", "Choice.First"), new Choice("first", "Choice.Second")],
            false
        );
        SettingDescriptor descriptor = new SettingDescriptor("choice", "Label.Choice", null, control);
        SettingSection section = new SettingSection("general", "Section.General", [descriptor]);

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate("Core", [section]);

        CollectionAssert.Contains((global::System.Collections.ICollection)errors, "Core/general/choice: duplicate choice value 'first'");
    }

    private static string Format(float value) {
        return value.ToString();
    }
}
