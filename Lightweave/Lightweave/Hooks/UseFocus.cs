using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Runtime;
using UnityEngine;

namespace Cosmere.Lightweave.Hooks;

public static class UseFocus {
    public static FocusHandle Use(
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        int callSiteId = unchecked(file.GetHashCode() * 31 + line);
        int parentHash = RenderContext.Current.ParentPathHash;
        string focusName = $"lw_focus_{parentHash:X}_{callSiteId:X}";

        Hooks.RefHandle<FocusState?> stateRef = Hooks.UseRef<FocusState?>(null, line, file);
        stateRef.Current ??= new FocusState();
        FocusState focusState = stateRef.Current;

        if (focusState.PendingRequest) {
            RenderContext.Current.PendingOverlays.Enqueue(() => { GUI.FocusControl(focusName); });
            focusState.PendingRequest = false;
        }
        else if (focusState.PendingClear) {
            RenderContext.Current.PendingOverlays.Enqueue(() => { GUI.FocusControl(null); });
            focusState.PendingClear = false;
        }

        return new FocusHandle(focusName, focusState);
    }

    internal sealed class FocusState {
        public bool PendingClear;
        public bool PendingRequest;
    }

    public sealed class FocusHandle {
        private readonly FocusState state;

        internal FocusHandle(string name, FocusState state) {
            this.Name = name;
            this.state = state;
        }

        public string Name { get; }

        public bool IsFocused => GUI.GetNameOfFocusedControl() == Name;

        public void Request() {
            state.PendingRequest = true;
            state.PendingClear = false;
        }

        public void Clear() {
            state.PendingClear = true;
            state.PendingRequest = false;
        }
    }
}