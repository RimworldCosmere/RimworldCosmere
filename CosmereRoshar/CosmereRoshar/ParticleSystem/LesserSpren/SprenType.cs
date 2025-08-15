namespace Cosmere.Roshar.ParticleSystem.LesserSpren;

public enum SprenType {
    // Nature spren (static, terrain-based)
    Wavespren,
    Riverspren,
    Rockspren,
    Sandspren,
    Grassspren,
    Windspren,
    Rainspren,

    // Emotion spren (dynamic, pawn-based)
    Joyspren,
    Fearspren,
    Angerspren,
    Painspren,
    Exhaustionspren,
    Gloryspren,
    Shamespren,

    // Event spren (dynamic, event-based)
    Flamespren,
    Coldspren,
    Deathspren,
    Decayspren,
    Lifespren,
    Rotspren,
    Hungerspren,

    // Other
    Creationspren,
    Logicspren,
}

public static class SprenTypeExtensions {
    public static bool IsNatureSpren(this SprenType type) {
        return type switch {
            SprenType.Wavespren
                or SprenType.Riverspren
                or SprenType.Rockspren
                or SprenType.Sandspren
                or SprenType.Grassspren
                or SprenType.Windspren
                or SprenType.Rainspren => true,
            _ => false,
        };
    }

    public static bool IsEmotionSpren(this SprenType type) {
        return type switch {
            SprenType.Joyspren
                or SprenType.Fearspren
                or SprenType.Angerspren
                or SprenType.Painspren
                or SprenType.Exhaustionspren
                or SprenType.Gloryspren
                or SprenType.Shamespren => true,
            _ => false,
        };
    }

    public static bool IsEventSpren(this SprenType type) {
        return type switch {
            SprenType.Flamespren
                or SprenType.Coldspren
                or SprenType.Deathspren
                or SprenType.Decayspren
                or SprenType.Lifespren
                or SprenType.Rotspren
                or SprenType.Hungerspren => true,
            _ => false,
        };
    }
}