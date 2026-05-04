namespace Cosmere.Lightweave.Runtime;

public sealed class HookStore {
    private readonly Dictionary<HookKey, HookSlot> slots = new Dictionary<HookKey, HookSlot>();

    public HookSlot Acquire(HookKey key) {
        if (!slots.TryGetValue(key, out HookSlot slot)) {
            slot = new HookSlot();
            slots[key] = slot;
        }

        slot.TouchedThisFrame = true;
        return slot;
    }

    public void RetireUntouched() {
        List<HookKey>? toRemove = null;
        foreach (KeyValuePair<HookKey, HookSlot> kv in slots) {
            if (!kv.Value.TouchedThisFrame) {
                kv.Value.Cleanup?.Invoke();
                (toRemove ??= new List<HookKey>()).Add(kv.Key);
            }
            else {
                kv.Value.TouchedThisFrame = false;
            }
        }

        if (toRemove != null) {
            foreach (HookKey k in toRemove) {
                slots.Remove(k);
            }
        }
    }

    public void ReleaseAll() {
        foreach (HookSlot s in slots.Values) {
            s.Cleanup?.Invoke();
        }

        slots.Clear();
    }
}