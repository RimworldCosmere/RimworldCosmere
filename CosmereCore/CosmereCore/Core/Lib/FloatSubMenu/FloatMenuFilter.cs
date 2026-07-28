using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Lib.FloatSubMenu;

internal class FloatMenuFilter {
    private static readonly FieldInfo OptionsField =
        typeof(FloatMenu).GetField("options", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private (Func<FloatMenuOption, bool>? predicate, bool reset, bool recursive) delayed;
    private List<FloatMenuOption> filtered = null!;
    private FloatMenu? initialized;
    private List<FloatMenuOption> options = null!;
    private FloatMenuSizeMode sizeMode = FloatMenuSizeMode.Undefined;
    private bool updateSize;

    public IEnumerable<FloatMenuOption> Unfiltered => options;

    public IEnumerable<FloatMenuOption> Filtered => filtered;

    public int Count => filtered.Count;

    public void Filter(
        Func<FloatMenuOption, bool> predicate,
        bool reset = false,
        bool recursive = false
    ) {
        if (initialized == null) {
            delayed = (predicate, reset, recursive);
            return;
        }

        filtered.Clear();
        foreach (FloatMenuOption option in options) {
            FloatSubMenu? sub = recursive ? option as FloatSubMenu : null;
            bool match = reset || predicate(option);
            if (match || (sub?.AnyMatches(predicate, recursive) ?? false)) {
                filtered.Add(option);
                sub?.FilterSubMenu(predicate, match, recursive);
            }
        }

        updateSize = true;
    }

    public void Update(FloatMenu floatMenu, Action? onInit = null, Action? onResize = null) {
        if (initialized != floatMenu) Init(floatMenu, onInit);
        if (updateSize) UpdateSize(floatMenu, onResize);
    }

    protected void Init(FloatMenu floatMenu, Action? action) {
        options = (List<FloatMenuOption>)OptionsField.GetValue(floatMenu);
        OptionsField.SetValue(floatMenu, filtered = options.ToList());
        initialized = floatMenu;
        action?.Invoke();
        if (delayed.predicate != null) {
            Filter(delayed.predicate, delayed.reset, delayed.recursive);
            delayed.predicate = null;
        }
    }

    protected void UpdateSize(FloatMenu floatMenu, Action? action) {
        FloatMenuSizeMode mode = floatMenu.SizeMode;
        if (sizeMode != mode) {
            options.ForEach(x => x.SetSizeMode(mode));
            sizeMode = mode;
        }

        floatMenu.windowRect.size = floatMenu.InitialSize;
        floatMenu.windowRect.xMax = Mathf.Min(floatMenu.windowRect.xMax, Verse.UI.screenWidth);
        floatMenu.windowRect.yMax = Mathf.Min(floatMenu.windowRect.yMax, Verse.UI.screenHeight);

        updateSize = false;
        action?.Invoke();
    }
}
