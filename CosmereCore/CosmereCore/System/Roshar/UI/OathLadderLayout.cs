namespace Cosmere.System.Roshar.UI;

public enum OathDotState {
    Sworn,
    Current,
    Unsworn,
}

/// <summary>
///     Rect maths for the oath ladder, kept free of Unity types so the test host can load it.
/// </summary>
public static class OathLadderLayout {
    public const float DotSize = 9f;
    public const float Connector = 2f;
    public const float Height = 14f;

    public static int DotCount(int ideals) {
        return ideals < 0 ? 0 : ideals;
    }

    public static OathDotState StateFor(int index, int currentIdeal) {
        if (index < currentIdeal) return OathDotState.Sworn;

        return index == currentIdeal ? OathDotState.Current : OathDotState.Unsworn;
    }

    public static float DotX(int index, int count, float x, float width) {
        if (count <= 1) return x;

        float span = width - DotSize;

        return x + span * index / (count - 1);
    }
}
