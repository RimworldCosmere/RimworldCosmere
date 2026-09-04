using System;
using UnityEngine.Networking;

namespace Cosmere.Core.BetaHub;

/// <summary>
///     Runs UnityWebRequests without a MonoBehaviour. RimWorld has no coroutine host a mod can
///     borrow, so requests are started here and polled once per frame from Root.Update.
/// </summary>
public static class BetaHubRequestPump {
    private static readonly List<Pending> pending = [];

    public static int InFlight => pending.Count;

    public static void Send(UnityWebRequest request, Action<UnityWebRequest> onDone) {
        request.SendWebRequest();
        pending.Add(new Pending { Request = request, OnDone = onDone });
    }

    public static void Pump() {
        if (pending.Count == 0) return;

        for (int i = pending.Count - 1; i >= 0; i--) {
            Pending entry = pending[i];
            if (!entry.Request.isDone) continue;

            pending.RemoveAt(i);

            try {
                entry.OnDone(entry.Request);
            } catch (Exception ex) {
                Log.Error($"BetaHub request callback threw: {ex}");
            } finally {
                entry.Request.Dispose();
            }
        }
    }

    private sealed class Pending {
        public UnityWebRequest Request = null!;
        public Action<UnityWebRequest> OnDone = null!;
    }
}
