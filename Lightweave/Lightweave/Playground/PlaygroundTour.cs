using System;
using UnityEngine;

namespace Cosmere.Lightweave.Playground;

public static class PlaygroundTour {
    public const float DefaultScrollSpeedPxPerSec = 200f;
    public const float TopPauseSeconds = 1f;
    public const float BottomPauseSeconds = 1f;

    public static bool IsActive { get; private set; }
    public static int FlatIndex { get; private set; }
    public static float PrimitiveStartTime { get; private set; }
    public static float ScrollSpeedPxPerSec = DefaultScrollSpeedPxPerSec;

    private static float currentScrollSeconds;
    private static List<string>? cachedOrder;

    public static IReadOnlyList<string> AllPrimitives() {
        if (cachedOrder != null) {
            return cachedOrder;
        }

        List<string> flat = new List<string>();
        for (int i = 0; i < LightweavePlayground.Categories.Count; i++) {
            IReadOnlyList<string> ids = LightweavePlayground.Categories[i].PrimitiveIds;
            List<string> sorted = new List<string>(ids);
            sorted.Sort(StringComparer.OrdinalIgnoreCase);
            for (int j = 0; j < sorted.Count; j++) {
                flat.Add(sorted[j]);
            }
        }

        cachedOrder = flat;
        return cachedOrder;
    }

    public static void Start() {
        IsActive = true;
        FlatIndex = 0;
        PrimitiveStartTime = Time.realtimeSinceStartup;
        currentScrollSeconds = 0f;
    }

    public static void Stop() {
        IsActive = false;
    }

    public static string? CurrentPrimitiveId() {
        if (!IsActive) {
            return null;
        }

        IReadOnlyList<string> all = AllPrimitives();
        if (FlatIndex < 0 || FlatIndex >= all.Count) {
            return null;
        }

        return all[FlatIndex];
    }

    public static void NotifyMetrics(float contentHeight, float viewportHeight) {
        float maxScroll = Mathf.Max(0f, contentHeight - viewportHeight);
        float speed = Mathf.Max(1f, ScrollSpeedPxPerSec);
        currentScrollSeconds = maxScroll / speed;
    }

    public static float CurrentDurationSeconds() {
        return TopPauseSeconds + currentScrollSeconds + BottomPauseSeconds;
    }

    public static bool TryAdvance() {
        if (!IsActive) {
            return false;
        }

        IReadOnlyList<string> all = AllPrimitives();
        float now = Time.realtimeSinceStartup;
        float elapsed = now - PrimitiveStartTime;
        if (elapsed < CurrentDurationSeconds()) {
            return false;
        }

        FlatIndex++;
        PrimitiveStartTime = now;
        currentScrollSeconds = 0f;
        if (FlatIndex >= all.Count) {
            IsActive = false;
            return true;
        }

        return true;
    }

    public static void NotifyPrimitiveSwitched() {
        PrimitiveStartTime = Time.realtimeSinceStartup;
        currentScrollSeconds = 0f;
    }

    public static float ScrollProgress() {
        if (!IsActive) {
            return 0f;
        }

        float elapsed = Time.realtimeSinceStartup - PrimitiveStartTime;
        if (elapsed <= TopPauseSeconds) {
            return 0f;
        }

        elapsed -= TopPauseSeconds;
        if (currentScrollSeconds <= 0f || elapsed >= currentScrollSeconds) {
            return 1f;
        }

        return Mathf.Clamp01(elapsed / currentScrollSeconds);
    }
}
