namespace Cosmere.Core.UI.Model;

public sealed record InvestitureSnapshot(
    string SystemId,
    string SystemLabel,
    ResourceBar? PrimaryBar,
    List<InvestitureCell> Cells
);
