using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Runtime;
using UnityEngine;

namespace Cosmere.Lightweave.Hooks;

[Flags]
public enum KeyModifiers {
    None = 0,
    Control = 1,
    Shift = 2,
    Alt = 4,
}

public static class UseHotkey {
    public static void Use(
        KeyCode code,
        Action handler,
        KeyModifiers modifiers = KeyModifiers.None,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        RenderContext.Current.RegisterHotkey(new HotkeyBinding(code, modifiers, handler));
    }
}