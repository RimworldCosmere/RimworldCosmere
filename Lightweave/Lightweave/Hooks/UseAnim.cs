using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Runtime;
using UnityEngine;

namespace Cosmere.Lightweave.Hooks;

public static class UseAnim {
    public static float Animate(
        [DocParam("Target value to animate toward. Changing it retargets from the current value with the same easing curve.")]
        float target,
        [DocParam("Duration of the transition in seconds. Zero or less snaps instantly to the target.")]
        float duration,
        [DocParam("Optional easing curve mapping linear progress (0..1) to eased progress. Null is linear.")]
        Func<float, float>? easing = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.RefHandle<AnimState?> stateRef = Hooks.UseRef<AnimState?>(null, line, file);
        AnimState state = stateRef.Current ??= new AnimState {
            From = target,
            To = target,
            StartTime = Time.unscaledTime,
        };

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

    public static float Wave(
        [DocParam("Full period of one oscillation in seconds (0 -> 1 -> 0).")]
        float period,
        [DocParam("Optional easing applied to the [0,1] wave each frame.")]
        Func<float, float>? easing = null
    ) {
        if (period <= 0f) {
            return 0f;
        }

        float phase = Time.unscaledTime / period;
        float raw = (1f - Mathf.Cos(phase * Mathf.PI * 2f)) * 0.5f;

        RenderContext? ctx = RenderContext.CurrentOrNull;
        if (ctx != null) {
            AnimationClock.RegisterActive(ctx.RootId);
        }

        return ApplyEasing(easing, raw);
    }

    private static float Lerp(float from, float to, float t) {
        return from + (to - from) * t;
    }

    private static float ApplyEasing(Func<float, float>? easing, float t) {
        return easing != null ? easing(t) : t;
    }

    private sealed class AnimState {
        public float From;
        public float StartTime;
        public float To;
    }
}