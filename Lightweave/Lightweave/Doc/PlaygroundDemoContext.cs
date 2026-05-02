using System;

namespace Cosmere.Lightweave.Doc;

public sealed class PlaygroundDemoContext {
    public bool ForceDisabled { get; init; }

    private static readonly PlaygroundDemoContext defaultContext = new PlaygroundDemoContext();

    [ThreadStatic]
    private static PlaygroundDemoContext? current;

    public static PlaygroundDemoContext Current => current ?? defaultContext;

    internal static IDisposable Push(PlaygroundDemoContext ctx) {
        return new Scope(ctx);
    }

    private sealed class Scope : IDisposable {
        private readonly PlaygroundDemoContext? previous;

        internal Scope(PlaygroundDemoContext next) {
            previous = current;
            current = next;
        }

        public void Dispose() {
            current = previous;
        }
    }
}
