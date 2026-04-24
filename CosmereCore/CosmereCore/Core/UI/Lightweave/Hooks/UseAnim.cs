using System;
using System.Runtime.CompilerServices;
using Cosmere.Core.UI.Lightweave.Runtime;
using UnityEngine;

namespace Cosmere.Core.UI.Lightweave.Hooks;

public static class UseAnim {
    public static float Animate(
        float target,
        float duration,
        Func<float, float>? easing = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.RefHandle<AnimState> stateRef = Hooks.UseRef<AnimState>(null!, line, file);
        if (stateRef.Current == null) {
            stateRef.Current = new AnimState {
                From = target,
                To = target,
                StartTime = Time.unscaledTime,
                Initialized = true,
            };
        }

        AnimState state = stateRef.Current;

        if (!Mathf.Approximately(state.To, target)) {
            float elapsed = Time.unscaledTime - state.StartTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float currentValue = Lerp(state.From, state.To, ApplyEasing(easing, t));
            state.From = currentValue;
            state.To = target;
            state.StartTime = Time.unscaledTime;
        }

        float progress = duration > 0f
            ? Mathf.Clamp01((Time.unscaledTime - state.StartTime) / duration)
            : 1f;
        float easedProgress = ApplyEasing(easing, progress);
        float result = Lerp(state.From, state.To, easedProgress);

        if (progress < 1f) {
            RenderContext? ctx = RenderContext.CurrentOrNull;
            if (ctx != null) {
                AnimationClock.RegisterActive(ctx.RootId);
            }
        }

        return result;
    }

    private static float Lerp(float from, float to, float t) {
        return from + (to - from) * t;
    }

    private static float ApplyEasing(Func<float, float>? easing, float t) {
        return easing != null ? easing(t) : t;
    }

    private sealed class AnimState {
        public float From;
        public bool Initialized;
        public float StartTime;
        public float To;
    }
}