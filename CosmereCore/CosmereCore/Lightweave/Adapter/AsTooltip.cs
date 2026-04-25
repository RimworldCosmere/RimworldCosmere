using System;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Adapter;

/// <summary>
///     Adapter that renders a rich Lightweave widget tree as a hover tooltip over a given <see cref="Rect" />.
///     The tooltip is enqueued as a pending overlay and painted after the main Lightweave pass completes.
///     <para>
///         <b>Requirement:</b> must be called from within a Lightweave Paint body where
///         <see cref="RenderContext.CurrentOrNull" /> is non-null. If called outside a Lightweave pass
///         (i.e. <c>RenderContext.CurrentOrNull == null</c>), this method no-ops and no tooltip appears.
///         In that case, callers should use vanilla <see cref="TooltipHandler.TipRegion" /> instead.
///     </para>
///     <para>
///         Bypassed affordances vs vanilla <see cref="TooltipHandler.TipRegion" />:
///         <list type="bullet">
///             <item>
///                 No fade-in delay - the tooltip appears immediately on the frame the pointer enters
///                 <paramref name="rect" />.
///             </item>
///             <item>No <c>TipSignal</c> hash caching - the <paramref name="build" /> closure is invoked fresh each frame.</item>
///             <item>
///                 No built-in string translation or formatting - the Lightweave tree is responsible for all text
///                 rendering.
///             </item>
///             <item>
///                 No integration with vanilla tooltip stacking priority - this overlay always renders on top of the
///                 current Lightweave pass.
///             </item>
///         </list>
///     </para>
/// </summary>
public static class AsTooltip {
    /// <summary>
    ///     Attaches a rich Lightweave tooltip to <paramref name="rect" />.
    ///     No-ops if called outside a Lightweave pass or if the pointer is not over <paramref name="rect" />.
    /// </summary>
    /// <param name="rect">The screen rect the tooltip is anchored to.</param>
    /// <param name="build">
    ///     Factory invoked each frame the pointer is over <paramref name="rect" /> to produce the tooltip
    ///     content node.
    /// </param>
    /// <param name="preferredSize">Explicit tooltip size. Defaults to 15rem x 6rem when null.</param>
    public static void Attach(Rect rect, Func<LightweaveNode> build, Vector2? preferredSize = null) {
        RenderContext? ctx = RenderContext.CurrentOrNull;
        if (ctx == null) {
            return;
        }

        if (!Mouse.IsOver(rect)) {
            return;
        }

        Vector2 anchor = Event.current.mousePosition + new Vector2(16f, 16f);
        ctx.PendingOverlays.Enqueue(() => {
                Vector2 size = preferredSize ?? new Vector2(new Rem(15f).ToPixels(), new Rem(6f).ToPixels());
                Rect tooltipRect = new Rect(anchor.x, anchor.y, size.x, size.y);

                Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
                if (tooltipRect.xMax > screen.xMax) {
                    tooltipRect.x = screen.xMax - tooltipRect.width;
                }

                if (tooltipRect.yMax > screen.yMax) {
                    tooltipRect.y = screen.yMax - tooltipRect.height;
                }

                if (tooltipRect.x < 0f) {
                    tooltipRect.x = 0f;
                }

                if (tooltipRect.y < 0f) {
                    tooltipRect.y = 0f;
                }

                BackgroundSpec bg = new BackgroundSpec.Solid(ThemeSlot.SurfaceRaised);
                BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
                RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));
                PaintBox.Draw(tooltipRect, bg, border, radius);

                LightweaveNode node = build();
                LightweaveRoot.PaintSubtree(node, tooltipRect);
            }
        );
    }
}