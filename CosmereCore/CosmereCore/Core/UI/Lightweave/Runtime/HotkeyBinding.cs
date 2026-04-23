using System;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Hooks;

namespace Cosmere.Core.UI.Lightweave.Runtime;

public sealed class HotkeyBinding
{
    public KeyCode Code { get; }
    public KeyModifiers Modifiers { get; }
    public Action Handler { get; }

    public HotkeyBinding(KeyCode code, KeyModifiers modifiers, Action handler)
    {
        Code = code;
        Modifiers = modifiers;
        Handler = handler;
    }
}
