# PromptPack

PromptPack packages a local repository into AI-friendly context. It collects files, applies include/ignore rules, optionally removes comments or compresses code, estimates tokens, and generates XML, Markdown, JSON, or plain text.

## Quick Start

Run the installed tool from any directory:

```bash
promptpack
```

By default, PromptPack copies the generated context to the system clipboard using [TextCopy](https://github.com/CopyText/TextCopy). It does not create an output file unless you request one.

Show command help:

```bash
promptpack --help
```

PromptPack uses [Spectre.Console.Cli](https://spectreconsole.net/) for typed command settings, help, aliases, and validation. Running `promptpack` packs the current directory.

## Install From NuGet (Default)

Install the published tool globally:

```bash
dotnet tool install --global PromptPack
promptpack --help
```

Update an existing installation:

```bash
dotnet tool update --global PromptPack
```

### Install Locally After Cloning

To install the version from a cloned repository, create the tool package and install it from the local package directory:

```bash
dotnet pack src/PromptPack.Cli -c Release
dotnet tool install --global --add-source ./src/PromptPack.Cli/bin/Release PromptPack
promptpack --help
```

For later local rebuilds, update the installed tool:

```bash
dotnet pack src/PromptPack.Cli -c Release
dotnet tool update --global --add-source ./src/PromptPack.Cli/bin/Release PromptPack
```

## Output Destinations

Copy to clipboard (default):

```bash
promptpack
```

Write to a file:

```bash
promptpack -o promptpack-output.md
```

Write directly to stdout:

```bash
promptpack -s
```

Copy output in addition to writing it to a file or stdout:

```bash
promptpack -o context.json -y json -c
```

Supported styles are `xml`, `markdown`, `json`, and `plain`. Markdown is the default.

## File Selection

Pack a specific directory:

```bash
promptpack src
```

Include only matching comma-separated glob patterns:

```bash
promptpack -i "**/*.cs,README.md"
```

Add exclude patterns (`--ignore` remains supported):

```bash
promptpack -e "**/*.generated.cs,docs/**"
```

PromptPack also applies common build-directory exclusions and patterns from `.gitignore`.

Read file paths from standard input when another command determines the selection:

```bash
git ls-files '*.cs' '*.csproj' | promptpack -q -s
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
| `-x, --compress`                 | Keep essential code structure while reducing implementation detail |
| `-r, --remove-comments`          | Remove code comments                                               |
| `-l, --remove-empty-lines`       | Remove blank lines from file contents                              |
| `-n, --output-show-line-numbers` | Prefix generated file contents with line numbers                   |
| `-m, --no-file-summary`          | Omit metadata summary                                              |
| `-d, --no-directory-structure`   | Omit the directory tree                                            |
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
| `-h, --header-text <text>`               | Add custom header text to every generated format.                           |
| `-p, --instruction-file-path <path>`     | Add instructions read from a file.                                          |
| `-a, --include-full-directory-structure` | Include a separate tree of all non-ignored paths.                           |
| `-z, --split-output <size>`              | Split file output into numbered parts; supports bytes, `KB`, and `MB`.      |

Example compact context copied to the clipboard:

```bash
promptpack -x -r -l -y markdown
```

Example metadata-only JSON written to a file:

```bash
promptpack -y json -f -d -o metadata.json
```

Scan for likely secrets and keep the full repository tree alongside selected files:

```bash
promptpack \
	-k \
	-a \
	-y markdown \
	-o review-context.md
```

Split a large output into `context.part001.json`, `context.part002.json`, and subsequent files:

```bash
promptpack \
	-y json \
	-z 2MB \
	-o context.json
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
