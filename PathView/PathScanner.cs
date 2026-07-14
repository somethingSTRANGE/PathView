namespace PathView;

internal static class PathScanner
{
    public static IReadOnlyList<PathEntry> Scan(string? machineRaw, string? userRaw)
    {
        var seen = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
        var result = new List<PathEntry>();
        var index = 0;

        void AddSection(string? raw, PathOrigin origin)
        {
            foreach (var value in PathEntries.Split(raw))
            {
                index++;

                var key = NormalizeForComparison(value);
                if (!seen.TryGetValue(key, out var priorIndexes))
                {
                    priorIndexes = [];
                    seen[key] = priorIndexes;
                }

                var duplicateOf = priorIndexes.Count > 0 ? priorIndexes.ToArray() : Array.Empty<int>();
                priorIndexes.Add(index);

                result.Add(new PathEntry(index, value, origin, !Directory.Exists(value), duplicateOf));
            }
        }

        AddSection(machineRaw, PathOrigin.Machine);
        AddSection(userRaw, PathOrigin.User);

        return result;
    }

    private static string NormalizeForComparison(string value) => value.TrimEnd('\\').ToLowerInvariant();
}
