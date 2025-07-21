namespace Cosmere.Core.Ability;

public record struct Status(Active active, int power) {
    public bool isActive => active.Equals(Active.On);
    public bool isPoweredUp => power > 1;

    public static implicit operator Status(Active active) {
        return active.Equals(Active.Off) ? new Status(active, 0) : new Status(active, 1);
    }

    public static implicit operator Status(bool active) {
        return active ? Active.On : Active.Off;
    }

    public static implicit operator Status(int power) {
        return power > 0 ? new Status(Active.On, power) : Active.Off;
    }

    public static implicit operator bool(Status status) {
        return status.active == Active.On;
    }

    public static implicit operator int(Status status) {
        return status.power;
    }
}