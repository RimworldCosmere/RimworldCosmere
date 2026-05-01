using System;
using Cosmere.Lightweave.Hooks;
using UnityEngine;

namespace Cosmere.Lightweave.Runtime;

public sealed class HotkeyBinding {
    public HotkeyBinding(KeyCode code, KeyModifiers modifiers, Action handler) {
        Code = code;
        Modifiers = modifiers;
        Handler = handler;
    }

    public KeyCode Code { get; }
    public KeyModifiers Modifiers { get; }
    public Action Handler { get; }
}