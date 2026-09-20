# 📦 PromptPack

Turn a local codebase into focused, reviewable context for AI tools.

PromptPack is a local .NET global tool for preparing repository context before a code review, refactoring task, documentation pass, or architecture question. It selects the files that matter, preserves their paths and repository structure, optionally removes comments or compresses implementation detail, checks for likely secrets, estimates token usage, and produces a handoff in Markdown, XML, JSON, or plain text.

It is designed to make AI requests more focused without hiding the source material. You choose the directory, file patterns, processing level, output format, and destination. PromptPack writes only to local files, standard output, or the system clipboard; it does not upload repository contents or retrieve external context.

> [!IMPORTANT]
> Security scanning is enabled by default through the built-in scanner, which requires no external installation. To use the optional Secretlint scanner, install Node.js/npm and run `--security-scanner secretlint`; PromptPack invokes `npx @secretlint/quick-start`.

## 🔄 How It Works

PromptPack follows a local context-preparation pipeline:

1. **Select** files from a directory, stdin, or configured include patterns while applying `.gitignore`, `.ignore`, and built-in build-directory exclusions.
2. **Shape** the selected content by keeping it exact, removing comments or empty lines, or creating a structural compressed view.
3. **Review** the package with repository metadata, directory structure, token estimates, budget checks, and security findings for common credential patterns.
4. **Hand off** the result as Markdown, XML, JSON, or plain text to a file, stdout, clipboard, or numbered split files.

## 🚀 Quick Start

Install the tool:

```bash
dotnet tool install --global PromptPack
```

From any project directory, run:

```bash
promptpack
```

The default workflow packs the current directory as Markdown and copies the result to the system clipboard. Paste it into your AI assistant, editor, or review workflow.

> [!NOTE]
> The default command does not create a file. Use `-o` or `--output` when you want to save the generated context.

## 📤 Output Workflows

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

## 🧰 Common Usage

Pack a directory other than the current one:

```bash
promptpack src
```

Include only selected files or add exclusions:

```bash
promptpack -i "src/**/*.cs,README.md"
promptpack -e "**/*.generated.cs,docs/**"
```

PromptPack also uses common build exclusions and patterns from `.gitignore`.

> [!IMPORTANT]
> Generated folders and repository-ignored paths are excluded automatically. Use `--no-gitignore` or `--no-default-patterns` only when you need to broaden collection.

Read file paths from another command:

```bash
git ls-files '*.cs' '*.csproj' | promptpack -q -s
```

Create compact context for a code review:

```bash
promptpack -x -r -l -t -o review-context.md
```

Include the full repository tree when the model needs paths outside the selected files:

> [!TIP]
> `--full-tree` adds a separate tree of non-ignored paths without adding every path's contents to the selected file set. The older `--include-full-directory-structure` name remains supported.

```bash
promptpack --full-tree -o context.md
```

Create metadata-only JSON:

```bash
promptpack -y json -f -d -o metadata.json
```

## ✨ Features

- ✅ AI-friendly repository structure with file summaries and language-aware code blocks.
- ✅ Include and exclude files with comma-separated glob patterns.
- ✅ Pack a local directory or temporarily clone a remote Git repository and select its branch.
- ✅ Read newline-delimited file paths from stdin for shell and Git pipelines.
- ✅ Create starter configuration with `--init` and merge JSON/YAML settings across repository ancestors.
- ✅ Override configuration with command-line options and explicit `--config` files.
- ✅ Git-aware selection using `.gitignore` and `.ignore`, with options to disable Git and default ignore patterns.
- ✅ Add a separate full tree of non-ignored paths with `--full-tree`.
- ✅ Markdown, XML, JSON, and plain-text output.
- ✅ Clipboard, file, stdout, and split-file destinations.
- ✅ Optional compression, comment removal, empty-line removal, and line numbers.
- ✅ Choose whether generated output includes file summaries, directory structure, and file contents.
- ✅ Token estimates and token-budget checks for context limits and CI.
- ✅ Built-in security scanning enabled by default for common keys, tokens, and private-key patterns, with optional Secretlint support.
- ✅ Custom headers and instruction files for repository-specific context.
- ✅ Verbose processing and configuration diagnostics for troubleshooting.

## 📦 What PromptPack Produces

Every generated package is organized for both people and AI tools:

1. An optional custom header and instruction section.
2. A file summary with generation time, file count, and total size.
3. The selected repository structure.
4. Optional security findings and a full repository tree of non-ignored paths when `--full-tree` is enabled.
5. The selected file contents, with language labels and optional line numbers.

Choose the format based on where the result is going:

| Format     | Best for                                                 | Command                                |
| ---------- | -------------------------------------------------------- | -------------------------------------- |
| Markdown   | Human-readable review and direct AI prompts              | `promptpack -y markdown -o context.md` |
| XML        | Consumers that benefit from explicit structural sections | `promptpack -y xml -o context.xml`     |
| JSON       | Scripts and tools that need structured data              | `promptpack -y json -o context.json`   |
| Plain text | Simple pipelines and tools without Markdown parsing      | `promptpack -y plain -s`               |

PromptPack does not upload repository data. It writes to local files, standard output, or the system clipboard.

> [!WARNING]
> Security scanning reports likely matches for review; it does not redact, remove, or upload matching files. Review findings before sharing generated context.

## 🏗️ Architecture

PromptPack has two production projects:

- `PromptPack.Core` contains repository collection, configuration, processing,
  security scanning, token estimation, and output generation.
- `PromptPack.Cli` contains Spectre commands, dependency injection, stdin,
  clipboard integration, terminal output, and exit-code handling.

The detailed project tree and dependency rules are documented in the
[architecture guide](docs/guide/architecture.md).

## 📥 Installation

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

## ⚙️ Configuration

PromptPack discovers `promptpack.config.json`, `promptpack.config.yaml`, and `promptpack.config.yml`. Configuration is layered from global user settings, repository and ancestor directories, an explicit `--config` file, and command-line values.

Global configuration locations:

- Windows: `%APPDATA%/PromptPack`
- Linux and macOS: `$XDG_CONFIG_HOME/PromptPack` or `~/.config/PromptPack`

Example `promptpack.config.yaml`:

```yaml
include: src/**,tests/**,README.md
exclude: "**/*.generated.cs"
securityScan: true
securityScanner: built-in
includeFullDirectoryStructure: true
headerText: Review this repository for correctness.
splitOutput: 2MB
```

Use `-g, --config` to load a specific configuration file. See the [configuration guide](docs/guide/workflow.md) for more examples.

## 📖 Command Guide

PromptPack uses one packing command. Options and the optional directory argument can be combined freely:

```text
promptpack [DIRECTORY] [OPTIONS]
```

`DIRECTORY` defaults to the current directory (`.`). Run `promptpack --help` for the generated terminal reference.

### Positional arguments

| Argument      | Required | Default | Description        | Example          |
| ------------- | -------- | ------- | ------------------ | ---------------- |
| `[DIRECTORY]` | No       | `.`     | Directory to pack. | `promptpack src` |

### Command options

<!-- markdownlint-disable MD033 -->
<details open>
<summary><strong>Output</strong></summary>

<table>
<thead><tr><th>Option</th><th>Purpose</th><th>Example</th></tr></thead>
<tbody>
<tr><td><code>-o, --output &lt;path&gt;</code></td><td>Write output to a file; clipboard is the default destination.</td><td><code>promptpack -o context.md</code></td></tr>
<tr><td><code>-y, --style &lt;format&gt;</code></td><td>Select <code>markdown</code>, <code>xml</code>, <code>json</code>, or <code>plain</code>.</td><td><code>promptpack -y json -o context.json</code></td></tr>
<tr><td><code>-s, --stdout</code></td><td>Write output to standard output.</td><td><code>promptpack -s &gt; context.md</code></td></tr>
<tr><td><code>-c, --copy</code></td><td>Copy output to the clipboard in addition to another destination.</td><td><code>promptpack -o context.md -c</code></td></tr>
<tr><td><code>-z, --split-output &lt;size&gt;</code></td><td>Split output into numbered parts.</td><td><code>promptpack -z 2MB -o context.json</code></td></tr>
</tbody></table>
</details>

<details open>
<summary><strong>Selection and input</strong></summary>

<table>
<thead><tr><th>Option</th><th>Config property</th><th>Purpose</th><th>Example</th></tr></thead>
<tbody>
<tr><td><code>-i, --include &lt;patterns&gt;</code></td><td><code>include</code></td><td>Include only matching files.</td><td><code>promptpack -i "src/**/*.cs,README.md"</code></td></tr>
<tr><td><code>-e, --exclude, --ignore &lt;patterns&gt;</code></td><td><code>exclude</code></td><td>Add exclusion patterns.</td><td><code>promptpack -e "**/*.generated.cs,docs/**"</code></td></tr>
<tr><td><code>--no-gitignore</code></td><td><code>useGitignore</code></td><td>Disable <code>.gitignore</code> and <code>.ignore</code>.</td><td><code>promptpack --no-gitignore</code></td></tr>
<tr><td><code>--no-default-patterns</code></td><td><code>useDefaultPatterns</code></td><td>Disable built-in exclusions.</td><td><code>promptpack --no-default-patterns</code></td></tr>
<tr><td><code>-q, --stdin</code></td><td></td><td>Read one file path per input line.</td><td><code>git ls-files '*.cs' | promptpack -q -s</code></td></tr>
</tbody></table>
</details>

<details open>
<summary><strong>Repository</strong></summary>

<table>
<thead><tr><th>Option</th><th>Purpose</th><th>Example</th></tr></thead>
<tbody>
<tr><td><code>--init</code></td><td>Create a starter <code>promptpack.config.json</code>.</td><td><code>promptpack --init</code></td></tr>
<tr><td><code>-g, --config &lt;path&gt;</code></td><td>Load an explicit JSON or YAML configuration file.</td><td><code>promptpack -g team.config.yaml</code></td></tr>
<tr><td><code>--remote &lt;url&gt;</code></td><td>Clone and pack a Git repository temporarily.</td><td><code>promptpack --remote https://github.com/org/repo.git -s</code></td></tr>
<tr><td><code>--remote-branch &lt;name&gt;</code></td><td>Select a branch for <code>--remote</code>.</td><td><code>promptpack --remote URL --remote-branch develop</code></td></tr>
</tbody></table>
</details>

<details open>
<summary><strong>Processing and context</strong></summary>

<table>
<thead><tr><th>Option</th><th>Config property</th><th>Purpose</th><th>Example</th></tr></thead>
<tbody>
<tr><td><code>-x, --compress</code></td><td><code>compress</code></td><td>Reduce implementation detail while retaining structure.</td><td><code>promptpack -x -o compact.md</code></td></tr>
<tr><td><code>-r, --remove-comments</code></td><td><code>removeComments</code></td><td>Remove supported comments.</td><td><code>promptpack -r -o review.md</code></td></tr>
<tr><td><code>-l, --remove-empty-lines</code></td><td><code>removeEmptyLines</code></td><td>Remove blank lines.</td><td><code>promptpack -l -o compact.md</code></td></tr>
<tr><td><code>-n, --output-show-line-numbers</code></td><td></td><td>Prefix generated source lines with line numbers.</td><td><code>promptpack -n -o debugging.md</code></td></tr>
<tr><td><code>-m, --no-file-summary</code></td><td></td><td>Omit generated file metadata.</td><td><code>promptpack -m -o files-only.md</code></td></tr>
<tr><td><code>-d, --no-directory-structure</code></td><td></td><td>Omit the generated directory tree.</td><td><code>promptpack -d -o contents.md</code></td></tr>
<tr><td><code>-f, --no-files</code></td><td></td><td>Omit file contents while retaining metadata and structure.</td><td><code>promptpack -f -y json -o inventory.json</code></td></tr>
<tr><td><code>-j, --header-text &lt;text&gt;</code></td><td><code>headerText</code></td><td>Add custom header text.</td><td><code>promptpack -j "Review authentication" -o review.md</code></td></tr>
<tr><td><code>-p, --instruction-file-path &lt;path&gt;</code></td><td><code>instructionFilePath</code></td><td>Include instructions read from a file.</td><td><code>promptpack -p ai-instructions.md -o context.md</code></td></tr>
<tr><td><code>-a, --full-tree</code></td><td><code>includeFullDirectoryStructure</code></td><td>Include a separate full tree of non-ignored paths. The older <code>--include-full-directory-structure</code> name remains supported.</td><td><code>promptpack -a -o context.md</code></td></tr>
</tbody></table>
</details>

<details open>
<summary><strong>Analysis and general</strong></summary>

<table>
<thead><tr><th>Option</th><th>Config property</th><th>Purpose</th><th>Example</th></tr></thead>
<tbody>
<tr><td><code>-t, --token-count</code></td><td></td><td>Display total and largest per-file estimates.</td><td><code>promptpack -t -o context.md</code></td></tr>
<tr><td><code>-b, --token-budget &lt;number&gt;</code></td><td></td><td>Return exit code <code>2</code> when output exceeds the estimate.</td><td><code>promptpack -b 100000 -o context.md</code></td></tr>
<tr><td><code>-k, --security-scan</code></td><td><code>securityScan</code></td><td>Report common credential and private-key patterns; enabled by default.</td><td><code>promptpack -k -o context.md</code></td></tr>
<tr><td><code>--security-scanner &lt;name&gt;</code></td><td><code>securityScanner</code></td><td>Choose <code>built-in</code> (default) or <code>secretlint</code> via Node.js/npm.</td><td><code>promptpack --security-scanner secretlint -o context.md</code></td></tr>
<tr><td><code>-w, --verbose</code></td><td></td><td>Enable detailed processing and configuration logging.</td><td><code>promptpack -w -o context.md</code></td></tr>
<tr><td><code>--help</code></td><td></td><td>Show command usage and option help.</td><td><code>promptpack --help</code></td></tr>
<tr><td><code>-v, --version</code></td><td></td><td>Show the installed version.</td><td><code>promptpack --version</code></td></tr>
</tbody></table>
</details>

<!-- markdownlint-enable MD033 -->

PromptPack applies common build exclusions plus `.gitignore` and `.ignore` patterns unless disabled.

Use `promptpack --init` to create a starter JSON file. Configuration is layered from global settings, repository and ancestor files, explicit `--config`, then command-line values. Remote packing requires `git` on `PATH`; its temporary clone is removed after completion.

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

## 📚 Documentation

See the [documentation site](docs) for installation, workflows, architecture, and command details:

- [Quickstart](docs/guide/quickstart.md): install the tool and run common scenarios.
- [Workflow guide](docs/guide/workflow.md): narrow, process, estimate, and hand off context.
- [Architecture](docs/guide/architecture.md): understand collection, processing, scanning, and generation.
- [CLI reference](docs/reference/cli.md): complete option descriptions and recipes.

## 💬 Prompt Examples

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

## 🤝 Contributing

Build and test changes locally:

```bash
dotnet build PromptPack.slnx
dotnet test --solution PromptPack.slnx
```

Documentation uses VitePress. Run it from `docs/` with `npm install` and `npm run dev`.

## License

See the repository license information.
