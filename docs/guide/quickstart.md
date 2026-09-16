# Quickstart

PromptPack packages a local directory into AI-friendly context. It targets .NET 10 and runs from the repository root with `dotnet run`.

## Prerequisites

- .NET SDK 10
- A repository or directory to pack
- Clipboard support when using the default output destination

## Pack the current directory

```bash
dotnet run -- .
```

The command uses XML output and copies the result to the system clipboard. It does not create an output file unless you provide `--output`.

## Choose an output destination

Write a file:

```bash
dotnet run -- . --output promptpack-output.xml
```

Write Markdown to stdout:

```bash
dotnet run -- . --stdout --style markdown
```

Write JSON and copy the same content to the clipboard:

```bash
dotnet run -- . --output context.json --style json --copy
```

Use `--output -` as another way to write to stdout.

## Narrow the files

Pack only C# files and the root README:

```bash
dotnet run -- . --include "**/*.cs,README.md"
```

Exclude generated files and documentation (`--ignore` remains supported):

```bash
dotnet run -- . --exclude "**/*.generated.cs,docs/**"
```

PromptPack also excludes common build and tool directories, including `.git`, `bin`, `obj`, `node_modules`, `dist`, `build`, `.vs`, and `.vscode`. Patterns from the target directory's `.gitignore` are applied too.

## Use configuration files

PromptPack discovers `promptpack.config.json`, `promptpack.config.yaml`, and `promptpack.config.yml`. Configurations merge from global user settings through repository ancestors to the target directory. Pass `--config <path>` to apply one additional explicit file last.

Example:

```yaml
include: src/**,tests/**
exclude: "**/*.generated.cs"
securityScan: true
headerText: Focus on correctness and API compatibility.
```

The global location is `%APPDATA%/PromptPack` on Windows and `$XDG_CONFIG_HOME/PromptPack` or `~/.config/PromptPack` on Linux and macOS.

## Use stdin and safety scanning

Provide one absolute or target-relative path per line. This works well with `git ls-files`:

```bash
git ls-files '*.cs' | dotnet run -- . --stdin --style markdown --stdout
```

Enable the conservative security scan to report likely private keys, AWS access keys, GitHub tokens, and generic secret assignments:

```bash
dotnet run -- . --security-scan --style json --output security-review.json
```

The scan reports matches in output; it does not upload files or replace a dedicated secret scanner.

## Add prompt context and split output

```bash
dotnet run -- . \
  --header-text "Review this as a production change." \
  --instruction-file-path review-instructions.md \
  --include-full-directory-structure \
  --split-output 2MB \
  --output context.md
```

Split files are written beside the requested output as `context.part001.md`, `context.part002.md`, and so on.

## Make a compact handoff

```bash
dotnet run -- . \
  --style markdown \
  --compress \
  --remove-comments \
  --remove-empty-lines \
  --token-count
```

For a metadata-only JSON package:

```bash
dotnet run -- . \
  --style json \
  --no-files \
  --no-directory-structure \
  --output metadata.json
```

## Build and test

Build the solution:

```bash
dotnet build PromptPack.slnx
```

Run tests:

```bash
dotnet test --solution PromptPack.slnx
```

See the complete option list in the [CLI reference](/reference/cli).
