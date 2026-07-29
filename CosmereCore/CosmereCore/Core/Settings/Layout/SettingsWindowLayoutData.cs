namespace Cosmere.Core.Settings.Layout;

public readonly record struct SettingsWindowLayoutData(
    float CrestX,
    float SectionRailX,
    float ContentViewportX,
    float ContentViewportY,
    float ContentViewportWidth,
    float ContentViewportHeight,
    float ContentX,
    float ContentY,
    float ContentWidth,
    float ContentHeight,
    float FooterX,
    float FooterY
);
