using UnityEngine;

namespace Cosmere.Core.Window;

public sealed class SettingsNavigationState {
    public const float FlashDuration = 0.3f;

    private readonly Dictionary<string, Vector2> scrollBySystem = [];
    private FlashState? flash;
    private bool reduceMotionFrameRendered;

    public string? SelectedSystemKey { get; private set; }

    public string? ScrollTargetSectionKey { get; private set; }

    public void SelectSystem(string systemKey) {
        SelectedSystemKey = systemKey;
        ScrollTargetSectionKey = null;
        flash = null;
        reduceMotionFrameRendered = false;
    }

    public Vector2 GetScroll(string systemKey) {
        return scrollBySystem.TryGetValue(systemKey, out Vector2 position) ? position : Vector2.zero;
    }

    public void SetScroll(string systemKey, Vector2 position) {
        scrollBySystem[systemKey] = position;
    }

    public void FlashSection(string systemKey, string sectionKey) {
        ScrollTargetSectionKey = sectionKey;
        flash = new FlashState(systemKey, sectionKey, null, Time.realtimeSinceStartup);
        reduceMotionFrameRendered = false;
    }

    public void FlashSetting(string systemKey, string sectionKey, string settingKey) {
        ScrollTargetSectionKey = sectionKey;
        flash = new FlashState(systemKey, sectionKey, settingKey, Time.realtimeSinceStartup);
        reduceMotionFrameRendered = false;
    }

    public bool TryGetFlash(bool reduceMotion, out FlashState activeFlash, out float alpha) {
        if (flash == null) {
            activeFlash = default;
            alpha = 0f;
            return false;
        }

        if (reduceMotion) {
            if (reduceMotionFrameRendered) {
                flash = null;
                activeFlash = default;
                alpha = 0f;
                return false;
            }

            reduceMotionFrameRendered = true;
            activeFlash = flash.Value;
            alpha = 1f;
            return true;
        }

        float elapsed = Time.realtimeSinceStartup - flash.Value.StartedAt;
        if (elapsed >= FlashDuration) {
            flash = null;
            activeFlash = default;
            alpha = 0f;
            return false;
        }

        activeFlash = flash.Value;
        alpha = 1f - elapsed / FlashDuration;
        return true;
    }

    public readonly record struct FlashState(
        string SystemKey,
        string? SectionKey,
        string? SettingKey,
        float StartedAt
    );
}
