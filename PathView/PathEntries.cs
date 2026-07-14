namespace PathView;

internal static class PathEntries
{
    public static string[] Split(string? rawPath) =>
        (rawPath ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
