using Cosmere.Lightweave.Tokens;
using UnityEngine;

namespace Cosmere.Lightweave.Theme;

public interface IThemeOverlay {
    Color? GetColor(ThemeSlot slot);
    Font? GetFont(FontRole role);
}
