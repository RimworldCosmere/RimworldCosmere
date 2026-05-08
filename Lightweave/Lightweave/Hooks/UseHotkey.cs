using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
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
        [DocParam("Key that triggers the binding when pressed.")]
        KeyCode code,
        [DocParam("Callback invoked when the chord fires. Runs once per key press.")]
        Action handler,
        [DocParam("Modifier mask required alongside the key. Combine flags for chords like Ctrl+Shift+S.")]
        KeyModifiers modifiers = KeyModifiers.None,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        RenderContext.Current.RegisterHotkey(new HotkeyBinding(code, modifiers, handler));
    }
}