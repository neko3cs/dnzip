# Agent Guidelines - DnZip

This document captures the current project-specific guidance for AI agents working on DnZip.

DnZip is a .NET CLI tool for creating ZIP archives with optional encryption and recursive directory support. It is intended as a practical alternative to the default Windows archiver.

## Build, run, format, and test commands

The main project is `src/DnZip/DnZip.csproj`.

### Build

```bash
dotnet build src/DnZip/DnZip.csproj
```

### Run

```bash
dotnet run --project src/DnZip/DnZip.csproj -- <archiveFilePath> <sourceDirectoryPath> [options]
```

Supported options map to `DnZipCommand.Compress` parameters:

- `--recurse` or `-r`: include subdirectories recursively
- `--encrypt` or `-e`: encrypt the archive and prompt for a password

Example:

```bash
dotnet run --project src/DnZip/DnZip.csproj -- output.zip ./data --recurse
```

### Format

Prefer formatting the solution, not only the main project:

```bash
dotnet format src/DnZip.slnx
```

### Tests

There is an xUnit test project at `src/DnZip.Tests/`.

Always run test-related commands from `src/DnZip.Tests/` so generated artifacts stay near the test project instead of creating `TestResults/` at the repository root.

Run all tests:

```bash
cd src/DnZip.Tests
dotnet test
```

Run coverage collection:

```bash
cd src/DnZip.Tests
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

Generate a coverage report if `reportgenerator` is available:

```bash
cd src/DnZip.Tests
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:"TextSummary;Html" -filefilters:"-*ConsoleAppFramework*"
```

Run mutation tests if `dotnet-stryker` is available:

```bash
cd src/DnZip.Tests
dotnet stryker --break-at 80
```

## Current code structure

- `src/DnZip/Program.cs`: composition root only; registers encoding and starts `ConsoleAppFramework`
- `src/DnZip/DnZipCommand.cs`: CLI command logic, validation, exit-code behavior, password confirmation
- `src/DnZip/ZipArchiveService.cs`: ZIP archive creation and recursive entry writing
- `src/DnZip/IPasswordPrompt.cs`: abstraction for password input
- `src/DnZip/SharpromptPasswordPrompt.cs`: production implementation using `Sharprompt`
- `src/DnZip/IArchiveService.cs`: abstraction for archive creation

### Test structure

Keep a strict **one file, one class** rule.

Examples:

- `src/DnZip.Tests/DnZipCommandTests.cs`
- `src/DnZip.Tests/ZipArchiveServiceTests.cs`
- `src/DnZip.Tests/FakePasswordPrompt.cs`
- `src/DnZip.Tests/FakeArchiveService.cs`
- `src/DnZip.Tests/TestWorkspace.cs`

Do not re-introduce a single large test file that mixes unrelated test classes or helper classes.

`Program` is intentionally thin and should generally not need direct tests.

## Code style and conventions

### General

- Target framework: `.NET 10`
- Indentation: **2 spaces**, no tabs
- Prefer LF line endings
- Use Allman braces
- Use block-scoped namespaces

### Naming

- Classes, interfaces, methods: `PascalCase`
- Parameters and locals: `camelCase`
- Private fields: `_camelCase`

### Using directives

- Put `using` directives at the top of the file
- Order groups as: `System.*`, third-party namespaces, then project namespaces
- Sort each group alphabetically when practical

### Types and language features

- Prefer modern C# features when they improve clarity
- Use `var` when the type is obvious from the right-hand side
- Use `string.IsNullOrEmpty()` for basic string validation

## Error handling and CLI behavior

- Successful CLI execution should return `0`
- Failures should return `1`
- Expected user-facing failures should print a clear message instead of crashing silently
- Unexpected failures at the command layer should be surfaced to the console and converted to exit code `1`

## Libraries in use

### ZIP handling: SharpZipLib

- ZIP creation uses `ICSharpCode.SharpZipLib`
- Use `Encoding.GetEncoding("Shift_JIS")` for archive entry compatibility with Windows tools
- Register code pages first with:

```csharp
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

- Compression level is set to best compression via `zipStream.SetLevel(9)`
- Use `Path.GetRelativePath` and `ZipEntry.CleanName` when building archive paths

### CLI framework: ConsoleAppFramework

- The entry point remains `ConsoleApp.RunAsync(args, command.Compress)`
- Command parameters become CLI arguments/options automatically

### Password prompting: Sharprompt

- Production password input goes through `IPasswordPrompt`
- `SharpromptPasswordPrompt` is the concrete implementation and should call `Prompt.Password(...)`
- For tests, prefer fake implementations over static hooks

## Agent rules and best practices

1. Preserve 2-space indentation.
2. Do not remove these existing HACK comments unless the corresponding feature is actually implemented:
   - `HACK: 複数ファイル指定に対応`
   - `HACK: --no-dir-entries(-D) に対応`
3. Keep `Program` thin; prefer extracting behavior into classes with explicit dependencies.
4. If a dependency needs to be faked in tests, introduce an interface and inject it rather than adding static test hooks.
5. Keep tests focused per class and per responsibility.
6. Run verification after changes. For test-related work, prefer running commands from `src/DnZip.Tests/`.
7. Do not run `git commit` or `git push` unless the user explicitly asks.
8. If CLI syntax changes, update `README.md` as part of the same change.

## Verification checklist

- [ ] Code follows 2-space indentation
- [ ] `dotnet build` succeeds
- [ ] No new compiler warnings are introduced
- [ ] Exit codes remain correct (`0` on success, `1` on failure)
- [ ] Password prompting stays behind `IPasswordPrompt`
- [ ] Tests remain one-file-per-class
