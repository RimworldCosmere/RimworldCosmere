namespace Cosmere.Core.Ability;

public record struct Status(Active active, int power) {
    public static Status PowerOne => new Status(Active.On, 1);
    public static Status PowerTwo => new Status(Active.On, 2);
    public static Status PowerTen => new Status(Active.On, 10);

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