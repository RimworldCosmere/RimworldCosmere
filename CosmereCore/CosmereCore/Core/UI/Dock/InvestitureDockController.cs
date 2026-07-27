using Verse;

namespace Cosmere.Core.UI.Dock;

public static class InvestitureDockController {
    private static InvestitureDockWindow? window;

    public static void UpdateVisibility() {
        bool shouldShow = InvestitureDockWindow.ShouldShow();

        if (shouldShow && (window == null || !Find.WindowStack.IsOpen(window))) {
            window = new InvestitureDockWindow();
            Find.WindowStack.Add(window);
            return;
        }

        if (!shouldShow && window != null && Find.WindowStack.IsOpen(window)) {
            window.Close(false);
            window = null;
        }
    }
}
