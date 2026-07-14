namespace PathView;

internal sealed record PathEntry(
    int Index,
    string Value,
    PathOrigin Origin,
    bool IsMissing,
    IReadOnlyList<int> DuplicateOfIndexes);
