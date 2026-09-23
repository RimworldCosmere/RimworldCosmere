namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     What a transfer does to every metalmind it reaches. One token picks the eligibility test,
///     the apply, the room measure and the direction, so no caller can pair them wrongly.
/// </summary>
public enum MetalmindOperation {
    Store,
    StoreCompounded,
    Tap,
    TapCompounded,
}
