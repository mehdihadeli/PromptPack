# CLI Reference

## Configuration and input

PromptPack reads `promptpack.config.json`, `promptpack.config.yaml`, or `promptpack.config.yml`. Configuration is merged in this order: global user config, repository and ancestor configs, then `--config` and command-line values. On Windows, global files live under `%APPDATA%/PromptPack`; on Linux and macOS, use `$XDG_CONFIG_HOME/PromptPack` or `~/.config/PromptPack`.

Useful options:

- `-g, --config PATH`: use an explicit config file.
- `-q, --stdin`: read one repository-relative or absolute file path per stdin line.
- `-k, --security-scan`: report common private-key, cloud-key, token, and secret-assignment patterns.
- `-h, --header-text TEXT` and `-p, --instruction-file-path PATH`: add prompt-specific context to output.
- `-a, --include-full-directory-structure`: include the ignored-by-selection but not ignored-by-repository full tree separately from included files.
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

Run commands with the published `promptpack` .NET tool.

## Usage

```text
promptpack [directory] [options]
```

The directory defaults to `.`.

## Output options

| Option                                     | Purpose                                                                                               |
| ------------------------------------------ | ----------------------------------------------------------------------------------------------------- |
| `-o, --output <path>`                      | Write output to a file. Use `-` for stdout. Clipboard is the default when no destination is selected. |
| `-y, --style <xml\|markdown\|json\|plain>` | Select the output format. Defaults to `markdown`.                                                     |
| `-s, --stdout`                             | Write the generated content to stdout.                                                                |
| `-c, --copy`                               | Copy output to the system clipboard in addition to another destination.                               |

Examples:

```bash
promptpack -y xml -o context.xml
promptpack -s
promptpack -y json -o context.json -c
```

## File selection options

| Option                               | Purpose                                                                  |
| ------------------------------------ | ------------------------------------------------------------------------ |
| `-i, --include <patterns>`           | Include only files matching comma-separated glob patterns.               |
| `-e, --exclude, --ignore <patterns>` | Add comma-separated patterns to the default and `.gitignore` exclusions. |

```bash
promptpack src --include "**/*.cs,**/*.csproj"
promptpack -e "**/*.generated.cs,docs/**"
```

## Processing options

| Option                           | Purpose                                                                                  |
| -------------------------------- | ---------------------------------------------------------------------------------------- |
| `-x, --compress`                 | Keep a compact structural view of code and replace implementation blocks with `{ ... }`. |
| `-r, --remove-comments`          | Remove supported line and block comments.                                                |
| `-l, --remove-empty-lines`       | Remove blank lines from file contents.                                                   |
| `-n, --output-show-line-numbers` | Prefix generated file contents with line numbers.                                        |

## Context options

| Option                                   | Purpose                                                                |
| ---------------------------------------- | ---------------------------------------------------------------------- |
| `-m, --no-file-summary`                  | Omit generated metadata such as timestamp, file count, and total size. |
| `-d, --no-directory-structure`           | Omit the generated directory tree.                                     |
| `-f, --no-files`                         | Omit file contents while retaining other selected output sections.     |
| `-g, --config <path>`                    | Load an explicit JSON or YAML configuration file.                      |
| `-q, --stdin`                            | Read one file path per stdin line.                                     |
| `-k, --security-scan`                    | Report common credential and private-key patterns.                     |
| `-h, --header-text <text>`               | Add custom header text to generated output.                            |
| `-p, --instruction-file-path`            | Add instructions read from a file.                                     |
| `-a, --include-full-directory-structure` | Include a separate full tree section.                                  |
| `-z, --split-output <size>`              | Split file output by bytes, KB, or MB.                                 |

Configuration files can be named `promptpack.config.json`, `promptpack.config.yaml`, or `promptpack.config.yml`. Precedence is global user config, ancestor/repository config, explicit `--config`, then command-line values. Global files live under `%APPDATA%/PromptPack` on Windows or `$XDG_CONFIG_HOME/PromptPack` / `~/.config/PromptPack` elsewhere.

Example:

```bash
printf 'src/Program.cs\nsrc/Commands/PackCommand.cs\n' | promptpack -q -s
promptpack -y json -k -z 2MB -o context.json
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
promptpack -y markdown -x -r -l -t
```

Metadata-only JSON:

```bash
promptpack -y json -f -d -o metadata.json
```

Line-numbered XML for a targeted directory:

```bash
promptpack src -y xml -o src-context.xml -n
```
