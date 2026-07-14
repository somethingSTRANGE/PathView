# PathView

A cleaner, human-readable alternative to Windows' built-in `path` command. `pathview` lists your `PATH` environment variable one entry per line, split by origin (Machine and User, in the order Windows actually resolves them), and calls out duplicate entries and directories that no longer exist.

## Example

```
Machine PATH
 1. C:\Windows\System32
 2. C:\Windows
 3. C:\Program Files\Git\cmd
 4. C:\Program Files\dotnet\

User PATH
 5. C:\Users\Michael\AppData\Local\Microsoft\WindowsApps
 6. C:\Tools
 7. C:\Program Files\Git\cmd  ⚠ Duplicate of 3
 8. C:\OldTools  ✘ Missing

Diagnostics
 ✓ 8 entries
 ⚠ 1 duplicate
 ✘ 1 missing directory
```

In a real terminal, the `⚠`/`✘` glyphs and their labels are colored to stand out; that's flattened to plain text here.

## Why

The built-in `path` command dumps the entire `PATH` as one long semicolon-separated line — hard to scan, and it doesn't distinguish Machine-level entries from User-level ones or flag anything that's wrong. `pathview` numbers each entry in actual resolution order (Machine first, then User appended) and highlights duplicates and missing directories inline.

## Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/) (the exact version is pinned in `global.json`).

```
dotnet build -c Release
dotnet publish PathView -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

`publish/pathview.exe` is a single self-contained executable — no separate .NET runtime install required on the target machine.

## Running tests

```
dotnet test
```

## Status

PathView is early and under active development. Implemented: the enumerated Machine/User listing, duplicate detection, missing-directory detection, and colorized output that respects `NO_COLOR` and disables itself automatically when output is redirected. Not yet implemented: command-line arguments/filtering, and detecting *how* the tool was launched so it can mimic the built-in `path` command's plain output when invoked non-interactively (e.g. from scripts) — this matters if you rename `pathview.exe` to `path.exe` locally to shadow the built-in command.

## License

[MIT](LICENSE)
