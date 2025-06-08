using System;
using System.Collections.Generic;
using Verse;

namespace CosmereFramework.Utils {
    [StaticConstructorOnStartup]
    public class DelayedActionScheduler {
        private static readonly List<ScheduledAction> scheduled = new List<ScheduledAction>();

        static DelayedActionScheduler() { }

        public static void Schedule(Action action, int delayTicks) {
            scheduled.Add(new ScheduledAction {
                ticksLeft = delayTicks,
                action = action,
            });
        }

        public static void Tick() {
            for (var i = scheduled.Count - 1; i >= 0; i--) {
                var item = scheduled[i];
                item.ticksLeft--;
                if (item.ticksLeft <= 0) {
                    item.action();
                    scheduled.RemoveAt(i);
                }
            }
        }

        private class ScheduledAction {
            public Action action;
            public int ticksLeft;
        }

        private class GameComponent_DelayedActionScheduler : GameComponent {
            public GameComponent_DelayedActionScheduler() { }
            public GameComponent_DelayedActionScheduler(Game game) { }

            public override void GameComponentTick() {
                Tick();
            }
        }
    }
}