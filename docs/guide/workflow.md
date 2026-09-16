# Workflow

Use PromptPack as a repeatable context preparation step before asking an AI tool to inspect code.

The useful loop is: select the smallest relevant scope, choose the least destructive processing level, check size and safety, then choose the handoff format.

## 1. Start broad

Create a baseline package so you can see what the collector includes:

```bash
promptpack -y markdown -o baseline.md
```

Inspect the directory structure and file summary before adding filters.

## 2. Focus the selection

Use `--include` when the task has a clear scope:

```bash
promptpack \
  -i "src/**/*.cs,tests/**/*.cs,README.md" \
  -y markdown \
  -o implementation.md
```

Use `--exclude` for known noise such as generated files, snapshots, or local documentation. The older `--ignore` option remains supported.

For repeatable selections, put the same values in `promptpack.config.yaml`. Use nested configuration files when a subdirectory needs a narrower local policy, and use `--config` for a one-off final override.

## 3. Pick the right processing level

- Keep default content for debugging exact implementation behavior.
- Use `--remove-comments` when comments consume attention but are not part of the task.
- Use `--remove-empty-lines` for denser prompts.
- Use `--compress` for a structural overview, not for a line-by-line code review.
- Use `--no-files` when only repository metadata and structure are relevant.

## 4. Bound the handoff

Estimate the result before sending it to a model:

```bash
promptpack -t -b 12000 -y markdown
```

A budget overflow reports the estimate and exits with code `2`. This makes the command usable in scripts that need to stop before producing an oversized handoff.

## 5. Choose a destination

Clipboard is the default for interactive work. Use a file for review or archival, and stdout for shell pipelines:

```bash
promptpack -y json -o context.json
promptpack -y plain -s > context.txt
```

Add `-c` when you want a file or stdout result and a clipboard copy as well.

For shell pipelines, pass a newline-delimited file list through stdin:

```bash
git ls-files 'src/**/*.cs' | promptpack -q -y plain -s
```

For larger handoffs, use `--split-output 2MB` with a file destination. PromptPack creates numbered sibling files. Use `--include-full-directory-structure` when the model needs to understand repository layout beyond the selected file contents.

Before sharing a package, use `--security-scan` to surface common credential patterns and add task-specific framing with `--header-text` or `--instruction-file-path`.

## 6. Give the package a concrete task

The generated package is context, not a question. Pair it with a focused request:

```text
Review the selected code for correctness and security risks.
Use file paths in every finding and separate confirmed bugs from suggestions.
```

For architecture work:

```text
Explain the request flow through this repository.
Identify the entry point, core services, configuration boundary, and output boundary.
```

For testing work:

```text
Design focused tests for the selected behavior.
Include normal cases, empty input, invalid configuration, and boundary conditions.
```

Keep the generated package local until you have reviewed its file selection and security findings.

## Repeatable checklist

1. Confirm the target directory.
2. Narrow with include and ignore patterns.
3. Select processing options deliberately.
4. Check the token estimate.
5. Generate the format your consumer expects.
