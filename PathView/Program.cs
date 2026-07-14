namespace PathView;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var entries = PathScanner.Scan(
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine),
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User));

        Render(entries);
    }

    private static void Render(IReadOnlyList<PathEntry> entries)
    {
        Console.WriteLine();
        if (entries.Count == 0)
        {
            Console.WriteLine("PATH is empty.");
            return;
        }

        var indexWidth = Math.Max(2, entries.Count.ToString().Length);

        PrintSection(entries, PathOrigin.Machine, "Machine PATH", indexWidth);
        Console.WriteLine();
        PrintSection(entries, PathOrigin.User, "User PATH", indexWidth);
        Console.WriteLine();
        PrintDiagnostics(entries);
    }

    private static void PrintSection(
        IReadOnlyList<PathEntry> entries, PathOrigin origin, string title, int indexWidth)
    {
        Console.WriteLine(Ansi.Header(title));

        foreach (var entry in entries.Where(e => e.Origin == origin))
        {
            PrintEntry(entry, indexWidth);
        }
    }

    private static void PrintEntry(PathEntry entry, int indexWidth)
    {
        var indexText = Ansi.Dim($" {entry.Index.ToString().PadLeft(indexWidth)}.");

        var tags = new List<string>();
        if (entry.IsMissing)
        {
            tags.Add(Ansi.MissingTag("Missing"));
        }

        if (entry.DuplicateOfIndexes.Count > 0)
        {
            tags.Add(Ansi.DuplicateTag($"Duplicate of {string.Join(", ", entry.DuplicateOfIndexes)}"));
        }

        var line = $"{indexText} {entry.Value}";
        if (tags.Count > 0)
        {
            line += "  " + string.Join(' ', tags);
        }

        Console.WriteLine(line);
    }

    private static void PrintDiagnostics(IReadOnlyList<PathEntry> entries)
    {
        var duplicateCount = entries.Count(e => e.DuplicateOfIndexes.Count > 0);
        var missingCount = entries.Count(e => e.IsMissing);

        Console.WriteLine(Ansi.Header("Diagnostics"));
        Console.WriteLine($"{Ansi.RegularTag($"{entries.Count} entries")}");

        if (duplicateCount > 0)
        {
            Console.WriteLine(Ansi.DuplicateTag($"{duplicateCount} duplicate{(duplicateCount == 1 ? "" : "s")}"));
        }

        if (missingCount > 0)
        {
            Console.WriteLine(Ansi.MissingTag($"{missingCount} missing director{(missingCount == 1 ? "y" : "ies")}"));
        }
    }
}
