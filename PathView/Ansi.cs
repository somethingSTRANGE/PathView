namespace PathView;

internal static class Ansi
{
    private const char Esc = '\u001b';
    private static readonly string Reset = $"{Esc}[0m";
    private static readonly string Faint = $"{Esc}[2m";
    private static readonly string FaintReset = $"{Esc}[22m";
    private static readonly string Bold = $"{Esc}[1m";
    private static readonly string Underline = $"{Esc}[4m";

    private static readonly (int R, int G, int B) DuplicateBackColor = (104, 104, 0);
    private static readonly (int R, int G, int B) DuplicateIconColor = (255, 196, 0);
    private static readonly (int R, int G, int B) MissingBackColor = (104, 0, 0);
    private static readonly (int R, int G, int B) MissingIconColor = (255, 82, 82);
    private static readonly (int R, int G, int B) RegularIconColor = (76, 217, 100);
    private static readonly (int R, int G, int B) LabelGray = (176, 176, 176);

    public static bool IsEnabled { get; } =
        !Console.IsOutputRedirected && Environment.GetEnvironmentVariable("NO_COLOR") is null;

    public static string Dim(string text) => IsEnabled ? $"{Faint}{text}{FaintReset}" : text;

    public static string Header(string text) => IsEnabled ? $"{Bold}{Underline}{text}{Reset}" : text;

    public static string MissingTag(string label) => Tag(Glyphs.Missing, MissingIconColor, label, MissingBackColor);

    public static string DuplicateTag(string label) => Tag(Glyphs.Duplicate, DuplicateIconColor, label, DuplicateBackColor);

    public static string RegularTag(string label) => Tag(Glyphs.Ok, RegularIconColor, label);

    private static string Tag(string glyph, (int R, int G, int B) iconColor, string label, (int R, int G, int B)? backColor = null)
    {
        if (!IsEnabled)
        {
            return $"{glyph} {label}";
        }

        var back = backColor.HasValue ? $"{Esc}[48;2;{backColor.Value.R};{backColor.Value.G};{backColor.Value.B}m" : string.Empty;
        var iconFg = $"{Esc}[38;2;{iconColor.R};{iconColor.G};{iconColor.B}m";
        var labelFg = $"{Esc}[38;2;{LabelGray.R};{LabelGray.G};{LabelGray.B}m";

        return $"{back}{iconFg} {glyph} {labelFg}{label} {Reset}";
    }
}
