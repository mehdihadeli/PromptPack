# CLI Reference

## Configuration and input

PromptPack reads `promptpack.config.json`, `promptpack.config.yaml`, or `promptpack.config.yml`. Configuration is merged in this order: global user config, repository and ancestor configs, then `--config` and command-line values. On Windows, global files live under `%APPDATA%/PromptPack`; on Linux and macOS, use `$XDG_CONFIG_HOME/PromptPack` or `~/.config/PromptPack`.

Example config:

```json
{
  "include": "src/**,tests/**",
  "exclude": "**/*.generated.cs",
  "securityScan": true,
  "securityScanner": "built-in",
  "headerText": "Review this repository for correctness.",
  "splitOutput": "2MB"
}
```

Run commands with the published `promptpack` .NET tool.

PromptPack is a local CLI. It does not fetch remote repositories or upload generated context. Its normal interactive workflow copies Markdown to the clipboard; use `--output` for a file and `--stdout` for a pipeline.

## Usage

```text
promptpack [directory] [options]
```

The directory defaults to `.`.

## Positional arguments

| Argument      | Required | Default | Description        | Example          |
| ------------- | -------- | ------- | ------------------ | ---------------- |
| `[directory]` | No       | `.`     | Directory to pack. | `promptpack src` |

### Output formats

| Format     | Typical use                                              | Example                                |
| ---------- | -------------------------------------------------------- | -------------------------------------- |
| `markdown` | Reviewable context and direct AI prompts; default format | `promptpack -y markdown -o context.md` |
| `xml`      | Explicit sections for structured consumers               | `promptpack -y xml -o context.xml`     |
| `json`     | Programmatic inspection and automation                   | `promptpack -y json -o context.json`   |
| `plain`    | Text-only shell pipelines                                | `promptpack -y plain -s`               |

When no destination is supplied, PromptPack copies the generated output to the clipboard. `--copy` is additive: it copies output even when `--output` or `--stdout` is also supplied.

## Command options

<!-- markdownlint-disable MD033 -->
<details open>
<summary><strong>Output</strong></summary>

<table><thead><tr><th>Option</th><th>Purpose</th></tr></thead><tbody>
<tr><td><code>-o, --output &lt;path&gt;</code></td><td>Write output to a file; clipboard is the default destination.</td></tr>
<tr><td><code>-y, --style &lt;format&gt;</code></td><td>Select <code>markdown</code>, <code>xml</code>, <code>json</code>, or <code>plain</code>.</td></tr>
<tr><td><code>-s, --stdout</code></td><td>Write generated content to stdout.</td></tr>
<tr><td><code>-c, --copy</code></td><td>Copy output to the clipboard in addition to another destination.</td></tr>
<tr><td><code>-z, --split-output &lt;size&gt;</code></td><td>Split output into numbered parts.</td></tr>
</tbody></table>
</details>

<details open>
<summary><strong>Selection, input, and repository</strong></summary>

<table><thead><tr><th>Option</th><th>Config property</th><th>Purpose</th></tr></thead><tbody>
<tr><td><code>-i, --include &lt;patterns&gt;</code></td><td><code>include</code></td><td>Include only matching files.</td></tr>
<tr><td><code>-e, --exclude, --ignore &lt;patterns&gt;</code></td><td><code>exclude</code></td><td>Add exclusion patterns.</td></tr>
<tr><td><code>--no-gitignore</code></td><td><code>useGitignore</code></td><td>Disable <code>.gitignore</code> and <code>.ignore</code>.</td></tr>
<tr><td><code>--no-default-patterns</code></td><td><code>useDefaultPatterns</code></td><td>Disable built-in exclusions.</td></tr>
<tr><td><code>-q, --stdin</code></td><td></td><td>Read one file path per stdin line.</td></tr>
<tr><td><code>--init</code></td><td></td><td>Create a starter configuration file.</td></tr>
<tr><td><code>-g, --config &lt;path&gt;</code></td><td></td><td>Load an explicit JSON or YAML configuration file.</td></tr>
<tr><td><code>--remote &lt;url&gt;</code></td><td></td><td>Clone and pack a Git repository temporarily.</td></tr>
<tr><td><code>--remote-branch &lt;name&gt;</code></td><td></td><td>Select a branch for <code>--remote</code>.</td></tr>
</tbody></table>
</details>

<details open>
<summary><strong>Processing and context</strong></summary>

<table><thead><tr><th>Option</th><th>Config property</th><th>Purpose</th></tr></thead><tbody>
<tr><td><code>-x, --compress</code></td><td><code>compress</code></td><td>Reduce implementation detail while retaining structure.</td></tr>
<tr><td><code>-r, --remove-comments</code></td><td><code>removeComments</code></td><td>Remove supported comments.</td></tr>
<tr><td><code>-l, --remove-empty-lines</code></td><td><code>removeEmptyLines</code></td><td>Remove blank lines.</td></tr>
<tr><td><code>-n, --output-show-line-numbers</code></td><td></td><td>Prefix generated source lines with line numbers.</td></tr>
<tr><td><code>-m, --no-file-summary</code></td><td></td><td>Omit generated file metadata.</td></tr>
<tr><td><code>-d, --no-directory-structure</code></td><td></td><td>Omit the generated directory tree.</td></tr>
<tr><td><code>-f, --no-files</code></td><td></td><td>Omit file contents while retaining metadata and structure.</td></tr>
<tr><td><code>-j, --header-text &lt;text&gt;</code></td><td><code>headerText</code></td><td>Add custom header text.</td></tr>
<tr><td><code>-p, --instruction-file-path &lt;path&gt;</code></td><td><code>instructionFilePath</code></td><td>Include instructions read from a file.</td></tr>
<tr><td><code>-a, --full-tree</code></td><td><code>includeFullDirectoryStructure</code></td><td>Include a separate full tree of non-ignored paths. The older <code>--include-full-directory-structure</code> name remains supported.</td></tr>
</tbody></table>
</details>

<details open>
<summary><strong>Analysis and general</strong></summary>

<table><thead><tr><th>Option</th><th>Config property</th><th>Purpose</th></tr></thead><tbody>
<tr><td><code>-t, --token-count</code></td><td></td><td>Display total and largest per-file estimates.</td></tr>
<tr><td><code>-b, --token-budget &lt;number&gt;</code></td><td></td><td>Return exit code <code>2</code> when output exceeds the estimate.</td></tr>
<tr><td><code>-k, --security-scan</code></td><td><code>securityScan</code></td><td>Report common credential and private-key patterns; enabled by default.</td></tr>
<tr><td><code>--security-scanner &lt;name&gt;</code></td><td><code>securityScanner</code></td><td>Choose <code>built-in</code> (default) or <code>secretlint</code> via Node.js/npm.</td></tr>
<tr><td><code>-w, --verbose</code></td><td></td><td>Enable detailed processing and configuration logging.</td></tr>
<tr><td><code>--help</code></td><td></td><td>Show command usage and option help.</td></tr>
<tr><td><code>-v, --version</code></td><td></td><td>Show the installed version.</td></tr>
</tbody></table>
</details>

<!-- markdownlint-enable MD033 -->

Examples:

```bash
promptpack -y xml -o context.xml
promptpack src --include "**/*.cs,**/*.csproj"
promptpack --remote https://github.com/org/repo.git --remote-branch main -o remote-context.md
```

Configuration files can be named `promptpack.config.json`, `promptpack.config.yaml`, or `promptpack.config.yml`. Precedence is global user config, ancestor/repository config, explicit `--config`, then command-line values. Global files live under `%APPDATA%/PromptPack` on Windows or `$XDG_CONFIG_HOME/PromptPack` / `~/.config/PromptPack` elsewhere. Remote packing requires `git` on `PATH`; the temporary clone is removed after completion.

Example:

```bash
printf 'src/Program.cs\nsrc/Commands/PackCommand.cs\n' | promptpack -q -s
promptpack -y json -k -z 2MB -o context.json
```

## Exit behavior

A successful pack returns exit code `0`. A token budget overflow returns exit code `2`. Unexpected failures are reported and return a nonzero exit code.

## Scenario recipes

Prepare a focused code-review package:

```bash
promptpack \
  -i "src/**/*.cs,tests/**/*.cs" \
  -e "**/*.generated.cs" \
  -x -r -l -t \
  -j "Review correctness and test coverage." \
  -o review.md
```

Prepare a bounded CI handoff:

```bash
promptpack -y json -b 50000 -k -o context.json
```

The command returns `2` when the estimated output exceeds 50,000 tokens. A regular command failure returns another nonzero code.

Prepare context from a tracked file list:

```bash
git ls-files 'src/**/*.cs' 'tests/**/*.cs' | promptpack -q -y plain -s > tracked.txt
```

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
