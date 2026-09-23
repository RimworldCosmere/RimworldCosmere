namespace Cosmere.Core.UI.Radial;

public sealed class RadialState {
    public int HoveredIndex = -1;
    public RadialStateKind Kind = RadialStateKind.Closed;
    public int SelectedSubsectionIndex = -1;
    public int SelectedSystemIndex = -1;

    public void Reset() {
        Kind = RadialStateKind.Closed;
        SelectedSystemIndex = -1;
        SelectedSubsectionIndex = -1;
        HoveredIndex = -1;
    }

    public void Back() {
        switch (Kind) {
            case RadialStateKind.AbilityTier:
                Kind = RadialStateKind.SubsectionTier;
                SelectedSubsectionIndex = -1;
                HoveredIndex = -1;
                break;
            case RadialStateKind.SubsectionTier:
                Kind = RadialStateKind.SystemTier;
                SelectedSystemIndex = -1;
                HoveredIndex = -1;
                break;
            case RadialStateKind.SystemTier:
            case RadialStateKind.Closed:
                Reset();
                break;
        }
    }

    public RadialLeaf? ResolveHoveredLeaf(RadialSnapshot snapshot) {
        if (Kind != RadialStateKind.AbilityTier) return null;
        if (SelectedSystemIndex < 0 || SelectedSystemIndex >= snapshot.Systems.Count) return null;
        RadialSystem sys = snapshot.Systems[SelectedSystemIndex];
        if (SelectedSubsectionIndex < 0 || SelectedSubsectionIndex >= sys.Subsections.Count) return null;
        RadialSubsection sub = sys.Subsections[SelectedSubsectionIndex];
        if (HoveredIndex < 0 || HoveredIndex >= sub.Leaves.Count) return null;
        return sub.Leaves[HoveredIndex];
    }
}
