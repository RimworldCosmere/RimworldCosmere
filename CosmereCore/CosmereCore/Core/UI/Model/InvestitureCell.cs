using UnityEngine;

namespace Cosmere.Core.UI.Model;

public sealed record InvestitureCell(
    string SubsystemId,
    string Label,
    Texture2D? Icon,
    ResourceBar Bar,
    bool IsActive,
    bool IsFlaring
);