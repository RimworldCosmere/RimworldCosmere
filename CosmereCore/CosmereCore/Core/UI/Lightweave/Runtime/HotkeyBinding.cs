using System;
using Cosmere.Core.UI.Lightweave.Hooks;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Runtime;

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