# PromptPack

PromptPack packages a local repository into AI-friendly context. It collects files, applies include/ignore rules, optionally removes comments or compresses code, estimates tokens, and generates XML, Markdown, JSON, or plain text.

## Quick Start

Run from the repository root without installing the tool:

```bash
dotnet run --project src/PromptPack.Cli -- .
```

By default, PromptPack copies the generated context to the system clipboard using [TextCopy](https://github.com/CopyText/TextCopy). It does not create an output file unless you request one.

Show command help:

```bash
dotnet run --project src/PromptPack.Cli -- --help
```

PromptPack uses [Spectre.Console.Cli](https://spectreconsole.net/) for typed command settings, help, aliases, and validation. The default command is packing, so these forms are equivalent:

```bash
promptpack .
promptpack pack .
promptpack p .
```

## Install As A .NET Tool

Build and install the local tool package:

```bash
dotnet pack src/PromptPack.Cli -c Release
dotnet tool install --global --add-source ./src/PromptPack.Cli/bin/Release PromptPack
promptpack --help
```

Update an existing local installation:

```bash
dotnet tool update --global --add-source ./src/PromptPack.Cli/bin/Release PromptPack
```

For a published package, install directly from NuGet:

```bash
dotnet tool install --global PromptPack
```

## Output Destinations

Copy to clipboard (default):

```bash
dotnet run --project src/PromptPack.Cli -- .
```

Write to a file:

```bash
dotnet run --project src/PromptPack.Cli -- . --output promptpack-output.xml
```

Write directly to stdout:

```bash
dotnet run --project src/PromptPack.Cli -- . --stdout --style markdown
```

Copy output in addition to writing it to a file or stdout:

```bash
dotnet run --project src/PromptPack.Cli -- . --output context.json --style json --copy
```

Supported styles are `xml`, `markdown`, `json`, and `plain`. XML is the default.

## File Selection

Pack a specific directory:

```bash
dotnet run --project src/PromptPack.Cli -- src
```

Include only matching comma-separated glob patterns:

```bash
dotnet run --project src/PromptPack.Cli -- . --include "**/*.cs,README.md"
```

Add exclude patterns (`--ignore` remains supported):

```bash
dotnet run --project src/PromptPack.Cli -- . --exclude "**/*.generated.cs,docs/**"
```

PromptPack also applies common build-directory exclusions and patterns from `.gitignore`.

Read file paths from standard input when another command determines the selection:

```bash
git ls-files '*.cs' '*.csproj' | dotnet run --project src/PromptPack.Cli -- . --stdin --style markdown --stdout
```

## Configuration

PromptPack discovers these configuration names:

- `promptpack.config.json`
- `promptpack.config.yaml` or `promptpack.config.yml`

Configuration is merged from global user configuration, repository ancestors, the target directory, and finally the explicit file supplied with `--config`. Global configuration is stored in `%APPDATA%/PromptPack` on Windows and `$XDG_CONFIG_HOME/PromptPack` or `~/.config/PromptPack` on other systems.

Supported configuration keys are `include`, `exclude`, `compress`, `removeComments`, `removeEmptyLines`, `includeFullDirectoryStructure`, `securityScan`, `headerText`, `instructionFilePath`, and `splitOutput`.

Example `promptpack.config.yaml`:

```yaml
include: src/**,tests/**,README.md
exclude: "**/*.generated.cs"
securityScan: true
includeFullDirectoryStructure: true
headerText: Review this repository for correctness.
splitOutput: 2MB
```

## Processing Options

| Option                           | Description                                                        |
| -------------------------------- | ------------------------------------------------------------------ |
| `-C, --compress`                 | Keep essential code structure while reducing implementation detail |
| `-r, --remove-comments`          | Remove code comments                                               |
| `-l, --remove-empty-lines`       | Remove blank lines from file contents                              |
| `-n, --output-show-line-numbers` | Prefix generated file contents with line numbers                   |
| `-F, --no-file-summary`          | Omit metadata summary                                              |
| `-D, --no-directory-structure`   | Omit the directory tree                                            |
| `-f, --no-files`                 | Generate metadata without file contents                            |
| `-t, --token-count`              | Display estimated total and per-file token counts                  |
| `-b, --token-budget <number>`    | Report overflow and exit nonzero when output exceeds the budget    |
| `-v, --verbose`                  | Enable verbose logging                                             |

Additional context and safety options:

| Option                                   | Description                                                                 |
| ---------------------------------------- | --------------------------------------------------------------------------- |
| `-g, --config <path>`                    | Load an explicit JSON or YAML configuration file.                           |
| `-q, --stdin`                            | Read one absolute or directory-relative file path per standard-input line.  |
| `-k, --security-scan`                    | Report common private-key, cloud-key, token, and secret-assignment matches. |
| `-H, --header-text <text>`               | Add custom header text to every generated format.                           |
| `-P, --instruction-file-path <path>`     | Add instructions read from a file.                                          |
| `-T, --include-full-directory-structure` | Include a separate tree of all non-ignored paths.                           |
| `-z, --split-output <size>`              | Split file output into numbered parts; supports bytes, `KB`, and `MB`.      |

Example compact context copied to the clipboard:

```bash
dotnet run --project src/PromptPack.Cli -- . --compress --remove-comments --remove-empty-lines --style markdown
```

Example metadata-only JSON written to a file:

```bash
dotnet run --project src/PromptPack.Cli -- . --style json --no-files --no-directory-structure --output metadata.json
```

Scan for likely secrets and keep the full repository tree alongside selected files:

```bash
dotnet run --project src/PromptPack.Cli -- . \
	--security-scan \
	--include-full-directory-structure \
	--style markdown \
	--output review-context.md
```

Split a large output into `context.part001.json`, `context.part002.json`, and subsequent files:

```bash
dotnet run --project src/PromptPack.Cli -- . \
	--style json \
	--split-output 2MB \
	--output context.json
```

## Documentation

The full user guide and CLI reference live in [`docs/`](docs/).

Run the documentation site locally:

```bash
cd docs
npm install
npm run dev
```

## Requirements

- .NET 10 SDK
- Clipboard support provided by the TextCopy package

## License

See the repository license information.
