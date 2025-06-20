using System;
using System.Collections.Generic;
using Verse;

namespace CosmereFramework.Utils;

[StaticConstructorOnStartup]
public class DelayedActionScheduler {
    private static readonly List<ScheduledAction> Scheduled = new List<ScheduledAction>();

    static DelayedActionScheduler() { }

    public static void Schedule(Action action, int delayTicks) {
        Scheduled.Add(new ScheduledAction {
            ticksLeft = delayTicks,
            action = action,
        });
    }

    public static void Tick() {
        for (int i = Scheduled.Count - 1; i >= 0; i--) {
            ScheduledAction? item = Scheduled[i];
            item.ticksLeft--;
            if (item.ticksLeft > 0) continue;

            item.action?.Invoke();
            Scheduled.RemoveAt(i);
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