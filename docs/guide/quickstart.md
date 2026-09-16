# Quickstart

PromptPack packages a local directory into AI-friendly context. Install the published .NET tool, then run it with `promptpack`.

## Prerequisites

- .NET SDK 10
- A repository or directory to pack
- Clipboard support when using the default output destination

## Pack the current directory

```bash
promptpack
```

The command uses Markdown output and copies the result to the system clipboard. It does not create an output file unless you provide `-o` or `--output`.

## Choose an output destination

Write a file:

```bash
promptpack -o promptpack-output.md
```

Write Markdown to stdout:

```bash
promptpack -s
```

Write JSON and copy the same content to the clipboard:

```bash
promptpack -o context.json -y json -c
```

Use `-o -` as another way to write to stdout.

## Narrow the files

Pack only C# files and the root README:

```bash
promptpack -i "**/*.cs,README.md"
```

Exclude generated files and documentation (`--ignore` remains supported):

```bash
promptpack -e "**/*.generated.cs,docs/**"
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
git ls-files '*.cs' | promptpack -q -s
```

Enable the conservative security scan to report likely private keys, AWS access keys, GitHub tokens, and generic secret assignments:

```bash
promptpack -k -y json -o security-review.json
```

The scan reports matches in output; it does not upload files or replace a dedicated secret scanner.

## Add prompt context and split output

```bash
promptpack \
  -h "Review this as a production change." \
  -p review-instructions.md \
  -a \
  -z 2MB \
  -o context.md
```

Split files are written beside the requested output as `context.part001.md`, `context.part002.md`, and so on.

## Make a compact handoff

```bash
promptpack \
  -y markdown \
  -x \
  -r \
  -l \
  -t
```

For a metadata-only JSON package:

```bash
promptpack \
  -y json \
  -f \
  -d \
  -o metadata.json
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
