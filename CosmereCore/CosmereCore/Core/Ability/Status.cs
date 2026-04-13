using System;
using Verse;

namespace Cosmere.Core.Ability;

public record struct Status(Active active, int power) : IExposable {
    public Active active = active;
    public int power = power;

    public static Status PowerOne => new Status(Active.On, 1);
    public static Status PowerTwo => new Status(Active.On, 2);
    public static Status PowerTen => new Status(Active.On, 10);

    public bool isActive => active.Equals(Active.On);
    public bool isPoweredUp => power > 1;

    public void ExposeData() {
        Scribe_Values.Look(ref active, "active", Active.On);
        Scribe_Values.Look(ref power, "power");
    }

    public static implicit operator Status(Active active) {
        return active.Equals(Active.Off) ? new Status(active, 0) : new Status(active, 1);
    }

    public static explicit operator Status(bool active) {
        return active ? Active.On : Active.Off;
    }

    public static explicit operator Status(int power) {
        return power > 0 ? new Status(Active.On, power) : Active.Off;
    }

    public static explicit operator bool(Status status) {
        return status.active == Active.On;
    }

    public override int GetHashCode() {
        return HashCode.Combine((int)active, power);
    }
}