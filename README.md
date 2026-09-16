# PromptPack

Pack a local codebase into one AI-ready document.

PromptPack collects the files that matter, preserves repository structure, estimates token usage, and produces Markdown, XML, JSON, or plain text. It is a .NET global tool designed for code reviews, refactoring, documentation, and other repository-level AI workflows.

## Quick Start

Install the tool:

```bash
dotnet tool install --global PromptPack
```

From any project directory, run:

```bash
promptpack
```

The default workflow packs the current directory as Markdown and copies the result to the system clipboard. Paste it into your AI assistant, editor, or review workflow.

## Output Workflows

| Goal                                   | Command                              |
| -------------------------------------- | ------------------------------------ |
| Copy the current directory as Markdown | `promptpack`                         |
| Write Markdown to standard output      | `promptpack -s`                      |
| Copy explicitly to the clipboard       | `promptpack -c`                      |
| Write output to a file                 | `promptpack -o context.md`           |
| Write a file and copy it               | `promptpack -o context.md -c`        |
| Choose another format                  | `promptpack -y json -o context.json` |

Supported styles are `markdown` (default), `xml`, `json`, and `plain`.

For help:

```bash
promptpack --help
```

## Common Usage

Pack a directory other than the current one:

```bash
promptpack src
```

Include only selected files or add exclusions:

```bash
promptpack -i "**/*.cs,README.md"
promptpack -e "**/*.generated.cs,docs/**"
```

PromptPack also uses common build exclusions and patterns from `.gitignore`.

Read file paths from another command:

```bash
git ls-files '*.cs' '*.csproj' | promptpack -q -s
```

Create compact context for a code review:

```bash
promptpack -x -r -l -t -o review-context.md
```

Create metadata-only JSON:

```bash
promptpack -y json -f -d -o metadata.json
```

## Features

- AI-friendly repository structure with file summaries and language-aware code blocks.
- Include and exclude files with comma-separated glob patterns.
- Markdown, XML, JSON, and plain-text output.
- Clipboard, file, stdout, and split-file destinations.
- Optional compression, comment removal, empty-line removal, and line numbers.
- Token estimates and token-budget checks for context limits and CI.
- Security scanning for common keys, tokens, and private-key patterns.
- Custom headers and instruction files for repository-specific context.
- JSON and YAML configuration with layered precedence.

## What PromptPack Produces

Every generated package is organized for both people and AI tools:

1. An optional custom header and instruction section.
2. A file summary with generation time, file count, and total size.
3. The selected repository structure.
4. Optional security findings and a full non-selection directory tree.
5. The selected file contents, with language labels and optional line numbers.

Choose the format based on where the result is going:

| Format     | Best for                                                 | Command                                |
| ---------- | -------------------------------------------------------- | -------------------------------------- |
| Markdown   | Human-readable review and direct AI prompts              | `promptpack -y markdown -o context.md` |
| XML        | Consumers that benefit from explicit structural sections | `promptpack -y xml -o context.xml`     |
| JSON       | Scripts and tools that need structured data              | `promptpack -y json -o context.json`   |
| Plain text | Simple pipelines and tools without Markdown parsing      | `promptpack -y plain -s`               |

PromptPack does not upload repository data. It writes to local files, standard output, or the system clipboard.

## Installation

Update an existing global installation:

```bash
dotnet tool update --global PromptPack
```

To run the version from a cloned checkout:

```bash
dotnet pack src/PromptPack.Cli -c Release
dotnet tool install --global --add-source ./src/PromptPack.Cli/bin/Release PromptPack
```

After rebuilding, update that local installation:

```bash
dotnet pack src/PromptPack.Cli -c Release
dotnet tool update --global --add-source ./src/PromptPack.Cli/bin/Release PromptPack
```

## Configuration

PromptPack discovers `promptpack.config.json`, `promptpack.config.yaml`, and `promptpack.config.yml`. Configuration is layered from global user settings, repository and ancestor directories, an explicit `--config` file, and command-line values.

Global configuration locations:

- Windows: `%APPDATA%/PromptPack`
- Linux and macOS: `$XDG_CONFIG_HOME/PromptPack` or `~/.config/PromptPack`

Example `promptpack.config.yaml`:

```yaml
include: src/**,tests/**,README.md
exclude: "**/*.generated.cs"
securityScan: true
includeFullDirectoryStructure: true
headerText: Review this repository for correctness.
splitOutput: 2MB
```

Use `-g, --config` to load a specific configuration file. See the [configuration guide](docs/guide/workflow.md) for more examples.

## Command and Argument Reference

PromptPack uses this command shape:

```text
promptpack [DIRECTORY] [OPTIONS]
```

`DIRECTORY` is optional and defaults to the current directory (`.`). Options can be combined in one command. The table below pairs every supported argument with a practical scenario.

### Input and output

| Argument                    | What it does                                                                                | Example scenario                                                                                               |
| --------------------------- | ------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| `[DIRECTORY]`               | Pack this directory instead of the current directory.                                       | `promptpack src` packs only the `src` directory.                                                               |
| `-o, --output <path>`       | Write generated content to a file. Without `-o` or `-s`, output is copied to the clipboard. | `promptpack -o context.md` saves Markdown to `context.md`.                                                     |
| `-y, --style <format>`      | Select `markdown`, `xml`, `json`, or `plain`. Defaults to `markdown`.                       | `promptpack -y json -o context.json` creates structured JSON.                                                  |
| `-s, --stdout`              | Write generated content to standard output.                                                 | `promptpack -s > context.md` sends output into a shell file or pipe.                                           |
| `-c, --copy`                | Copy output to the system clipboard in addition to another destination.                     | `promptpack -o context.md -c` saves and copies the same context.                                               |
| `-z, --split-output <size>` | Split file output into numbered parts. Accepts bytes, `KB`, or `MB`.                        | `promptpack -y json -z 2MB -o context.json` creates `context.part001.json`, `context.part002.json`, and so on. |

### File selection

| Argument                   | What it does                                                               | Example scenario                                                                           |
| -------------------------- | -------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| `-i, --include <patterns>` | Include only files matching comma-separated glob patterns.                 | `promptpack -i "src/**/*.cs,README.md"` selects C# files under `src` and the README.       |
| `-e, --exclude <patterns>` | Add comma-separated exclusion patterns.                                    | `promptpack -e "**/*.generated.cs,docs/**"` excludes generated C# files and documentation. |
| `--ignore <patterns>`      | Alias for `--exclude`.                                                     | `promptpack --ignore "**/*.snap"` excludes snapshot files.                                 |
| `-q, --stdin`              | Read one absolute or directory-relative file path per standard-input line. | `git ls-files '*.cs' \| promptpack -q -s` packs only tracked C# files and prints them.     |

PromptPack also applies common build-directory exclusions and patterns from `.gitignore`.

### Content processing

| Argument                         | What it does                                                        | Example scenario                                                                 |
| -------------------------------- | ------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| `-x, --compress`                 | Keep essential code structure while reducing implementation detail. | `promptpack -x -o compact.md` prepares a smaller context for a large repository. |
| `-r, --remove-comments`          | Remove supported line and block comments before output.             | `promptpack -r -o review.md` removes comments for a focused review context.      |
| `-l, --remove-empty-lines`       | Remove blank lines from file contents.                              | `promptpack -l -o compact.md` reduces whitespace in generated content.           |
| `-n, --output-show-line-numbers` | Prefix each generated source line with its line number.             | `promptpack -n -o debugging.md` makes it easier to discuss exact source lines.   |

### Context and instructions

| Argument                                 | What it does                                                                                 | Example scenario                                                                                           |
| ---------------------------------------- | -------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `-m, --no-file-summary`                  | Omit generated metadata such as timestamp, file count, and total size.                       | `promptpack -m -o files-only.md` removes the summary from the output.                                      |
| `-d, --no-directory-structure`           | Omit the generated directory tree.                                                           | `promptpack -d -o contents.md` keeps file contents without the tree section.                               |
| `-f, --no-files`                         | Omit file contents while retaining available metadata and structure.                         | `promptpack -f -y json -o inventory.json` creates a repository inventory.                                  |
| `-h, --header-text <text>`               | Add custom header text to generated output.                                                  | `promptpack -h "Review authentication for security issues" -o review.md` adds review context.              |
| `-p, --instruction-file-path <path>`     | Read additional instructions from a file and include them in output.                         | `promptpack -p ai-instructions.md -o context.md` includes project-specific review rules.                   |
| `-a, --include-full-directory-structure` | Include a separate tree of all non-ignored paths, even when `--include` selects fewer files. | `promptpack -i "src/**/*.cs" -a -o context.md` shows the full project layout while packing only C# source. |
| `-g, --config <path>`                    | Load an explicit JSON or YAML configuration file.                                            | `promptpack -g review.config.yaml` runs with a named configuration.                                        |

### Analysis and diagnostics

| Argument                      | What it does                                                                 | Example scenario                                                                          |
| ----------------------------- | ---------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| `-t, --token-count`           | Display estimated total tokens and the ten largest per-file estimates.       | `promptpack -t -o context.md` checks context size before sending it to an AI tool.        |
| `-b, --token-budget <number>` | Set exit code `2` when generated output exceeds the estimated token budget.  | `promptpack -b 100000 -o context.md` fails a CI step when context exceeds 100,000 tokens. |
| `-k, --security-scan`         | Report common private-key, cloud-key, token, and secret-assignment patterns. | `promptpack -k -o context.md` checks for likely secrets before sharing output.            |
| `-v, --verbose`               | Enable detailed processing and configuration logging.                        | `promptpack -v -o context.md` helps investigate file selection or configuration behavior. |
| `--help`                      | Show command usage and option help.                                          | `promptpack --help` displays the installed CLI reference.                                 |

### Useful combinations

Compact Markdown for a code review:

```bash
promptpack -i "src/**,tests/**" -x -r -l -t -o review-context.md
```

Metadata-only JSON for an inventory check:

```bash
promptpack -y json -f -d -k -o metadata.json
```

Line-numbered XML for debugging a targeted directory:

```bash
promptpack src -y xml -n -o src-context.xml
```

The same options are also documented in [docs/reference/cli.md](docs/reference/cli.md).

## Documentation

See the [documentation site](docs) for installation, workflows, architecture, and command details:

- [Quickstart](docs/guide/quickstart.md): install the tool and run common scenarios.
- [Workflow guide](docs/guide/workflow.md): narrow, process, estimate, and hand off context.
- [Architecture](docs/guide/architecture.md): understand collection, processing, scanning, and generation.
- [CLI reference](docs/reference/cli.md): complete option descriptions and recipes.

## Prompt Examples

After generating `context.md`, give the AI tool a task-specific request instead of a generic summary request:

```text
This file contains the selected repository context. Review the authentication flow.
Identify correctness, security, and maintainability risks. Cite file paths and suggest focused fixes.
```

```text
Use this repository context to propose tests for the file-collection behavior.
Cover normal cases, empty selections, ignore patterns, stdin input, and configuration precedence.
```

```text
Based on this repository context, explain the architecture to a new contributor.
Start with the main execution path, then name the files they should read first.
```

## Contributing

Build and test changes locally:

```bash
dotnet build PromptPack.slnx
dotnet test --solution PromptPack.slnx
```

Documentation uses VitePress. Run it from `docs/` with `npm install` and `npm run dev`.

## License

See the repository license information.
