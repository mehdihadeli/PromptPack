# Architecture

PromptPack separates repository discovery, content shaping, and output generation into a small local pipeline.

```mermaid
flowchart LR
  A[Directory] --> B[FileCollector]
  L[ConfigurationLoader] --> B
  S[stdin paths] --> B
  B --> P[ContentProcessor]
  P --> D[RepositoryContext]
  D --> E[OutputGenerator]
  E --> F[Clipboard, file, or stdout]
  D --> G[TokenEstimator]
  G --> H[Budget and reporting]
  B --> I[SecurityScanner]
  I --> D
```

## Collection phase

`FileCollector` walks the selected directory and converts paths to repository-relative paths. It starts with common build-directory exclusions, adds patterns from `.gitignore`, then applies custom `--include` and `--exclude` patterns. The older `--ignore` name remains supported.

`ConfigurationLoader` merges global and repository configuration files before collection. `FileCollector` can also consume newline-delimited stdin paths or return a separate full tree for `--include-full-directory-structure`.

An include pattern must match before a file can enter the pipeline. An ignore match removes it afterward. Patterns are comma-separated and use the CLI's glob matching rules.

## Processing phase

`ContentProcessor` reads each selected file and creates a `ProcessedFile` containing:

- Absolute and relative paths
- Processed content
- Content size
- A language label inferred from the extension

Processing is ordered and opt-in: comment removal, empty-line removal, and compression can be combined.

Compression is intentionally structural rather than semantic. It keeps declarations and statement-shaped lines while replacing implementation blocks with `{ ... }`. Review compact output before relying on it for detailed behavior.

## Context and generation

`RepositoryContext` holds the processed files and controls whether generated output includes:

- File summary metadata
- Directory structure
- File contents
- Line numbers
- Custom header and instructions
- A separate full directory structure
- Security findings from the optional scanner

`SecurityScanner` uses conservative local pattern matching for common private keys, AWS access keys, GitHub tokens, and generic secret assignments. It reports findings for review and does not transmit repository data.

Four generators implement the same context contract:

| Style      | Best for                      | Shape                                                   |
| ---------- | ----------------------------- | ------------------------------------------------------- |
| `xml`      | Structured model input        | `<repository>` with metadata, structure, and file CDATA |
| `markdown` | Human review and chat prompts | Headings, tree, and fenced file contents                |
| `json`     | Programmatic consumers        | Metadata and files keyed by relative path               |
| `plain`    | Simple text pipelines         | Delimited sections without markup                       |

## Token estimates

`TokenEstimator` reports a heuristic estimate based on words and code-like tokens. It is useful for comparing outputs and enforcing a limit, but it is not a tokenizer for a specific model.

With `--token-budget`, PromptPack sets exit code `2` when the generated output exceeds the requested estimate. With `--token-count`, it prints the total and the ten largest per-file estimates.

When `--split-output` is supplied with a file destination, the generated text is divided into numbered sibling files using the requested byte, KB, or MB size.

## Data boundaries

PromptPack is a local packaging tool. It does not upload repository contents or retrieve live external context. Clipboard behavior depends on the host environment and is handled by the `TextCopy` package.
