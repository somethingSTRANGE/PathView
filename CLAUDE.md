# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

PathView is a Windows CLI tool that acts as a cleaner, human-readable alternative to the built-in `path` command. It enumerates `PATH` entries split by origin (Machine then User, in actual resolution order), flags duplicate entries and entries pointing at directories that no longer exist. The published executable is named `pathview.exe`. See `README.md` for the user-facing pitch and example output.

Implemented: the enumerated Machine/User listing with continuous numbering, duplicate detection (cross-referencing all prior indexes, e.g. `Duplicate of 4, 15, 26`), missing-directory detection, a diagnostics summary footer, and colorized glyph+badge output that auto-disables when stdout is redirected or `NO_COLOR` is set. Not yet implemented: CLI arguments of any kind (always runs the full enumerated view), and the launch-context detection described below (parent-process walking, matching the built-in `path` command's plain output for non-interactive callers). Check `Program.cs`, `PathScanner.cs`, and `Ansi.cs` for current behavior before assuming a feature exists.

## Commands

SDK version is pinned via `global.json` (10.0.301) — Rider and the CLI will use .NET 10.

```
dotnet build                                          # build the solution
dotnet run --project PathView                          # run pathview from source
dotnet test                                            # run the full NUnit suite
dotnet test --filter "FullyQualifiedName~PathScannerTests"   # run one test class
dotnet test --filter "Name=Scan_FlagsDuplicate_AcrossMachineAndUserSections" # run one test
```

Note: `dotnet run`/`dotnet build` invoked through a piped/non-TTY shell (including Claude Code's own Bash/PowerShell tools) will always show color/badges disabled — that's `Ansi.IsEnabled` correctly detecting `Console.IsOutputRedirected`, not a bug. Glyphs would render as literal `?` there too if `Program.Main` didn't explicitly set `Console.OutputEncoding = Encoding.UTF8` (the default Windows console codepage can't represent them). To see the real colored/glyph output, run `pathview.exe` directly in an interactive terminal (Windows Terminal).

Publishing a self-contained single-file exe (what the eventual GitHub Actions release workflow will do):

```
dotnet publish PathView -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

## Structure

- `PathView.slnx` — solution file (.NET 10's new XML solution format, not the legacy `.sln`).
- `PathView/` — the tool itself. `AssemblyName` is lowercase (`pathview`) so the build output is `pathview.dll`/`pathview.exe`; the C# namespace stays `PathView`.
- `PathView-Tests/` — NUnit test project (NUnit, not xUnit — this was an explicit choice). References `PathView` via `ProjectReference` and reaches its `internal` types through `InternalsVisibleTo` in `PathView.csproj`. Prefer `internal` + `InternalsVisibleTo` over making implementation types `public` when adding testable logic.
- `PathScanner.Scan` (`PathScanner.cs`) is the core model-building step — takes raw Machine/User `PATH` strings, returns an ordered `IReadOnlyList<PathEntry>` with missing/duplicate flags already computed. `Program.cs` only renders; it doesn't do any detection logic itself.
- `Ansi.cs` centralizes all terminal styling (dimming, bold/underline headers, colored tags) behind `Ansi.IsEnabled`, so `Program.cs` never checks redirect/`NO_COLOR` state directly.

## Current output design (implemented)

- **Ordering**: Machine PATH first, then User PATH, with one continuous index across both — this matches how Windows actually resolves `PATH` (Machine entries, then User entries appended), and lets duplicate cross-references (`Duplicate of 9`) point at a single unambiguous index space.
- **Duplicate detection**: normalized via `TrimEnd('\\')` + case-insensitive comparison (`PathScanner.NormalizeForComparison`), across the Machine/User boundary. Each repeat occurrence lists *all* prior indexes, not just the first (`Duplicate of 4, 15, 26`), so `DuplicateOfIndexes` accumulates as `PathScanner.Scan` walks the list.
- **Missing detection**: plain `Directory.Exists` check per entry.
- **Tags**: inline per-entry, shown as separate tags (not merged into one string) when an entry is both missing and duplicate, with `Missing` always ordered before `Duplicate`. Tags are placed immediately after the path text with a two-space separator — deliberately *not* padded into an aligned column (that was tried and reverted; keeping tags close to the entry made problem lines easier to visually track, even though the right edge is now ragged). Glyphs (`Glyphs.cs`) are currently the light Unicode variants — `✓` `✘` `⚠` — chosen over the heavy variants (`✔` `✘` U+2718 heavy/`⚠`) and emoji-presentation forms (`⚠️` etc.) after testing across Windows Terminal and Rider's run console: heavy checkmark rendered double-width as an emoji in Windows Terminal, and the light checkmark rendered as two characters in Rider's console. No single glyph set was perfectly consistent everywhere; the current light set was the closest. Commented-out alternatives are left inline in `Glyphs.cs` for reference — don't delete them without checking with the user first, they're an active experiment log, not dead code.
- **Color** (`Ansi.cs`): each tag type has its own icon color and (for `Missing`/`Duplicate` only) a background band color — `MissingIconColor`/`MissingBackColor`, `DuplicateIconColor`/`DuplicateBackColor`, `RegularIconColor` (used for the diagnostics "N entries" line; it deliberately has no background). `Ansi.Tag` builds the whole tag as one continuous SGR span (background set once, icon-colored glyph, then `LabelGray`-colored label, single reset at the end) rather than colorizing each piece independently, to avoid the background dropping out between the glyph and label. Index numbers are dimmed via SGR 2 (`\x1b[2m`)/reset via SGR 22, not an explicit color, so they stay faint relative to whatever foreground color the user's theme already resolves for path text. Section headers (`Ansi.Header`) use SGR 1 (bold) + SGR 4 (underline) instead of a `----` separator line. `Ansi.IsEnabled` auto-disables all escape codes when `Console.IsOutputRedirected` or `NO_COLOR` is set — in that case `Header` and `Tag` fall back to plain unadorned text.
- **Encoding**: `Program.Main` sets `Console.OutputEncoding = Encoding.UTF8` explicitly — without it, .NET falls back to the system codepage on Windows and silently replaces the glyphs above with `?`.

Color/glyph choices here came from live back-and-forth testing in the user's actual terminals, not just code review — if asked to change them again, prefer small, easily-reverted tweaks (the RGB tuples and glyph constants are simple to swap) over restructuring `Ansi.cs`, since the current shape (`Tag` building one continuous escape span) was arrived at after an earlier version that colorized pieces independently caused the background to visibly drop out mid-tag.

## Still open (not yet built)

- **Launch-context-aware output**: detecting *how* the tool was invoked (real console vs redirected/piped vs script) beyond the current `IsOutputRedirected` color toggle — specifically, walking the parent-process chain (e.g. `WindowsTerminal.exe → cmd.exe → pathview.exe` vs `cmd.exe /c script.bat → pathview.exe` vs `explorer.exe → pathview.exe`) so that when the tool is renamed locally to `path.exe` to shadow the built-in command, non-interactive callers (scripts, other tools) get output matching the real `path` command's format instead of the enumerated/tagged view.
- **CLI arguments**: leaning toward no-hyphen subcommand-style arguments (`pathview user machine`, in the style of `dotnet`'s CLI) over `--flags`, but this is explicitly not decided. No-argument invocation should always default to the most inclusive output (everything: both origins, duplicates, missing paths) rather than requiring flags to see everything.
