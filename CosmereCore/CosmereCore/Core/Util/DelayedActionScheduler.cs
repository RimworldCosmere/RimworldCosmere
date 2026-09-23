using System;
using Verse;

namespace Cosmere.Core.Util;

[StaticConstructorOnStartup]
public class DelayedActionScheduler {
    private static readonly List<ScheduledAction> Scheduled = [];

    static DelayedActionScheduler() { }

    public static void Schedule(Action action, int delayTicks) {
        Scheduled.Add(
            new ScheduledAction {
                ticksLeft = delayTicks,
                action = action,
            }
        );
    }

    public static void Tick() {
        for (int i = Scheduled.Count - 1; i >= 0; i--) {
            ScheduledAction? item = Scheduled[i];
            item.ticksLeft--;
            if (item.ticksLeft > 0) continue;

            try {
                item.action?.Invoke();
            } catch (Exception ex) {
                Log.Error($"DelayedActionScheduler: scheduled action threw: {ex}");
            } finally {
                Scheduled.RemoveAt(i);
            }
        }
    }

    private class ScheduledAction {
        public Action? action;
        public int ticksLeft;
    }

    private class GameComponentDelayedActionScheduler : GameComponent {
        public GameComponentDelayedActionScheduler() { }

        public GameComponentDelayedActionScheduler(Game game) { }

        public override void GameComponentTick() {
            Tick();
        }
    }
}
