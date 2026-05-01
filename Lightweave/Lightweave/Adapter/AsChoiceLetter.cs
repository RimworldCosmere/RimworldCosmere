using Cosmere.Lightweave.Runtime;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Adapter;

/// <summary>
///     Phase 3 adapter that opens a Lightweave-rendered window in place of vanilla ChoiceLetter's
///     default dialog. The adapter overrides <c>OpenLetter</c> through the vanilla virtual slot
///     (verified: Verse.DeathLetter overrides the same slot), so the letter stack's automatic
///     dispatch reaches this adapter even through base-typed references. Bypassed affordances:
///     the default letter window chrome (title bar, scrollable body text, and DiaOption button
///     row rendered by <c>Dialog_NodeTreeWithFactionInfo</c>); automatic <c>Choices</c> rendering
///     (the DiaOption list is not passed to any node tree - consumers must query <c>this.Choices</c>
///     inside their <c>Build()</c> and render the buttons themselves); and <c>CanDismissWithRightClick</c>
///     (right-click dismissal must be handled inside the Lightweave tree if desired). All other
///     <c>ChoiceLetter</c> affordances - save/load via <c>ExposeData</c>, <c>lookTargets</c>,
///     <c>relatedFaction</c>, stack management, and the <c>Choices</c> helper properties - remain
///     available to subclasses.
/// </summary>
public abstract class AsChoiceLetter : ChoiceLetter {
    protected abstract Vector2 LetterSize { get; }
    protected abstract LightweaveNode Build();

    public override void OpenLetter() {
        Find.WindowStack.Add(new LetterWindow(this));
    }

    private sealed class LetterWindow : LightweaveWindow {
        private readonly AsChoiceLetter owner;

        public LetterWindow(AsChoiceLetter owner) {
            this.owner = owner;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => owner.LetterSize;

        protected override LightweaveNode Build() {
            return owner.Build();
        }
    }
}