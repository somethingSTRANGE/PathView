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

## Replacing the built-in `path` command

If you're used to typing `path` out of habit, you can make Command Prompt run PathView instead of its built-in `PATH` command.

This isn't as simple as putting `pathview.exe` (even renamed to `path.exe`) on your `PATH` — `cmd.exe` always checks its built-in commands (`PATH`, `DIR`, `CD`, etc.) before it ever searches `PATH` for an external program, and there's no setting to disable a specific built-in. The one thing that *does* take priority over built-in commands is a [DOSKEY](https://learn.microsoft.com/windows-server/administration/windows-commands/doskey) macro, so that's the mechanism to use:

1. Save a small script next to `pathview.exe` that defines the macro — e.g. `path.cmd` in the same folder:

   ```bat
   @echo off
   doskey path=%~dp0pathview.exe $*
   ```

   `%~dp0` resolves to the folder the script itself lives in, so this keeps working if you move the folder later.

2. Check whether you already have an `AutoRun` command set (some prompt customizers like Clink or ConEmu use it):

   ```
   reg query "HKCU\Software\Microsoft\Command Processor" /v AutoRun
   ```

   If that returns nothing, set it to run the script above on every new Command Prompt session:

   ```
   reg add "HKCU\Software\Microsoft\Command Processor" /v AutoRun /t REG_SZ /d "C:\path\to\path.cmd" /f
   ```

   Replace `C:\path\to\path.cmd` with wherever you saved the script. If a value already exists, don't overwrite it — instead append your script to it, e.g. `existing-command & "C:\path\to\path.cmd"`.

3. Open a **new** Command Prompt window (existing ones won't pick up the change) and confirm it worked:

   ```
   > where path
   C:\path\to\path.cmd
   ```

A couple of things worth knowing:

- This only affects `cmd.exe`. PowerShell has no built-in `path` command, so `pathview.exe` on your `PATH` (under whatever name) already works there with none of the above.
- DOSKEY macros only expand at an interactive prompt — they don't apply inside batch files or `cmd /c "..."` invocations, so any script that calls `path` will still see the real built-in command untouched.

### Removing the override

1. Remove the `AutoRun` value, or your addition to it:

   ```
   reg delete "HKCU\Software\Microsoft\Command Processor" /v AutoRun /f
   ```

   If you'd appended the script to a pre-existing `AutoRun` value (step 2 above) rather than setting it fresh, edit the value back down to what it was instead of deleting it outright, so you don't lose your other customization.

2. Delete `path.cmd` (or wherever you saved the macro script).

3. Open a **new** Command Prompt window — `path` will go back to the built-in command.

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

## License

[MIT](LICENSE)
