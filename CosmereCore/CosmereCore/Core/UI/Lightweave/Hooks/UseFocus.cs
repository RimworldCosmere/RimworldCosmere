using System.Runtime.CompilerServices;
using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;

namespace Cosmere.Core.UI.Lightweave.Hooks;

public static class UseFocus
{
    internal sealed class FocusState
    {
        public bool PendingRequest;
        public bool PendingClear;
    }

    public sealed class FocusHandle
    {
        private readonly string name;
        private readonly FocusState state;

        internal FocusHandle(string name, FocusState state)
        {
            this.name = name;
            this.state = state;
        }

        public string Name => name;

        public bool IsFocused => GUI.GetNameOfFocusedControl() == name;

        public void Request() { state.PendingRequest = true; state.PendingClear = false; }
        public void Clear() { state.PendingClear = true; state.PendingRequest = false; }
    }

    public static FocusHandle Use(
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = "")
    {
        int callSiteId = unchecked(file.GetHashCode() * 31 + line);
        int parentHash = RenderContext.Current.ParentPathHash;
        string focusName = $"lw_focus_{parentHash:X}_{callSiteId:X}";

        Hooks.RefHandle<FocusState> stateRef = Hooks.UseRef<FocusState>(null!, line, file);
        if (stateRef.Current == null)
        {
            stateRef.Current = new FocusState();
        }

        FocusState focusState = stateRef.Current;

        if (focusState.PendingRequest)
        {
            GUI.FocusControl(focusName);
            focusState.PendingRequest = false;
        }
        else if (focusState.PendingClear)
        {
            GUI.FocusControl(null);
            focusState.PendingClear = false;
        }

        return new FocusHandle(focusName, focusState);
    }
}
