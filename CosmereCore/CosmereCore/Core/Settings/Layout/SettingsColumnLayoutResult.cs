using global::System.Collections.Generic;

namespace Cosmere.Core.Settings.Layout;

public sealed record SettingsColumnLayoutResult(IReadOnlyList<SettingSectionPlacement> Placements, float ContentHeight);
