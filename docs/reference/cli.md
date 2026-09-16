# CLI Reference

## Configuration and input

PromptPack reads `promptpack.config.json`, `promptpack.config.yaml`, or `promptpack.config.yml`. Configuration is merged in this order: global user config, repository and ancestor configs, then `--config` and command-line values. On Windows, global files live under `%APPDATA%/PromptPack`; on Linux and macOS, use `$XDG_CONFIG_HOME/PromptPack` or `~/.config/PromptPack`.

Useful options:

- `-g, --config PATH`: use an explicit config file.
- `-q, --stdin`: read one repository-relative or absolute file path per stdin line.
- `-k, --security-scan`: report common private-key, cloud-key, token, and secret-assignment patterns.
- `-H, --header-text TEXT` and `-P, --instruction-file-path PATH`: add prompt-specific context to output.
- `-T, --include-full-directory-structure`: include the ignored-by-selection but not ignored-by-repository full tree separately from included files.
- `-z, --split-output SIZE`: split file output into `.part001`, `.part002`, and so on. Sizes accept bytes, `KB`, or `MB`.

Example config:

```json
{
  "include": "src/**,tests/**",
  "exclude": "**/*.generated.cs",
  "securityScan": true,
  "headerText": "Review this repository for correctness.",
  "splitOutput": "2MB"
}
```

Run commands from the repository root with `dotnet run -- <directory>`. After packaging the CLI as a .NET tool, the same options are available through the `promptpack` command.

## Usage

```text
dotnet run -- [directory] [options]
```

The directory defaults to `.`.

## Output options

| Option                                     | Purpose                                                                                               |
| ------------------------------------------ | ----------------------------------------------------------------------------------------------------- |
| `-o, --output <path>`                      | Write output to a file. Use `-` for stdout. Clipboard is the default when no destination is selected. |
| `-s, --style <xml\|markdown\|json\|plain>` | Select the output format. Defaults to `xml`.                                                          |
| `-S, --stdout`                             | Write the generated content to stdout.                                                                |
| `-c, --copy`                               | Copy output to the system clipboard in addition to another destination.                               |

Examples:

```bash
dotnet run -- . --style xml --output context.xml
dotnet run -- . --style markdown --stdout
dotnet run -- . --style json --output context.json --copy
```

## File selection options

| Option                               | Purpose                                                                  |
| ------------------------------------ | ------------------------------------------------------------------------ |
| `-i, --include <patterns>`           | Include only files matching comma-separated glob patterns.               |
| `-e, --exclude, --ignore <patterns>` | Add comma-separated patterns to the default and `.gitignore` exclusions. |

```bash
dotnet run -- src --include "**/*.cs,**/*.csproj"
dotnet run -- . --ignore "**/*.generated.cs,docs/**"
```

## Processing options

| Option                           | Purpose                                                                                  |
| -------------------------------- | ---------------------------------------------------------------------------------------- |
| `-C, --compress`                 | Keep a compact structural view of code and replace implementation blocks with `{ ... }`. |
| `-r, --remove-comments`          | Remove supported line and block comments.                                                |
| `-l, --remove-empty-lines`       | Remove blank lines from file contents.                                                   |
| `-n, --output-show-line-numbers` | Prefix generated file contents with line numbers.                                        |

## Context options

| Option                                   | Purpose                                                                |
| ---------------------------------------- | ---------------------------------------------------------------------- |
| `-F, --no-file-summary`                  | Omit generated metadata such as timestamp, file count, and total size. |
| `-D, --no-directory-structure`           | Omit the generated directory tree.                                     |
| `-f, --no-files`                         | Omit file contents while retaining other selected output sections.     |
| `-g, --config <path>`                    | Load an explicit JSON or YAML configuration file.                      |
| `-q, --stdin`                            | Read one file path per stdin line.                                     |
| `-k, --security-scan`                    | Report common credential and private-key patterns.                     |
| `-H, --header-text <text>`               | Add custom header text to generated output.                            |
| `-P, --instruction-file-path`            | Add instructions read from a file.                                     |
| `-T, --include-full-directory-structure` | Include a separate full tree section.                                  |
| `-z, --split-output <size>`              | Split file output by bytes, KB, or MB.                                 |

Configuration files can be named `promptpack.config.json`, `promptpack.config.yaml`, or `promptpack.config.yml`. Precedence is global user config, ancestor/repository config, explicit `--config`, then command-line values. Global files live under `%APPDATA%/PromptPack` on Windows or `$XDG_CONFIG_HOME/PromptPack` / `~/.config/PromptPack` elsewhere.

Example:

```bash
printf 'src/Program.cs\nsrc/Commands/PackCommand.cs\n' | dotnet run -- . --stdin --style markdown --stdout
dotnet run -- . --style json --security-scan --split-output 2MB --output context.json
```

## Token and logging options

| Option                        | Purpose                                                           |
| ----------------------------- | ----------------------------------------------------------------- |
| `-t, --token-count`           | Show the estimated total and the ten largest per-file estimates.  |
| `-b, --token-budget <number>` | Set exit code `2` when the generated output exceeds the estimate. |
| `-v, --verbose`               | Enable verbose logging.                                           |

## Exit behavior

A successful pack returns exit code `0`. A token budget overflow returns exit code `2`. Unexpected failures are reported and return a nonzero exit code.

## Common recipes

Compact Markdown for a code review:

```bash
dotnet run -- . --style markdown --compress --remove-comments --remove-empty-lines --token-count
```

Metadata-only JSON:

```bash
dotnet run -- . --style json --no-files --no-directory-structure --output metadata.json
```

Line-numbered XML for a targeted directory:

```bash
dotnet run -- src --style xml --output src-context.xml --output-show-line-numbers
```
