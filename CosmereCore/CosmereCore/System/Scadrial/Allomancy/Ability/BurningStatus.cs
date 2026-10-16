using Cosmere.Core.Ability;

namespace Cosmere.System.Scadrial.Allomancy.Ability;

public static class BurningStatus {
    public static readonly Status Off = new Status(Active.Off, 0);
    public static readonly Status Burning = new Status(Active.On, 1);
    public static readonly Status Flaring = new Status(Active.On, 2);
    public static readonly Status Duralumin = new Status(Active.On, 10);
}
