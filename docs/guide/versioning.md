# Versioning with NBGV

PromptPack uses [Nerdbank.GitVersioning (NBGV)](https://dotnet.github.io/Nerdbank.GitVersioning/) to calculate package and assembly versions from committed version intent and Git history.

This guide describes the single-branch GitHub Flow policy:

- `main` is the only long-lived development branch.
- Preview, RC, and stable changes to release intent are reviewed pull requests.
- Tags identify the exact RC or stable commit that was published.
- CI builds from a full Git clone so NBGV can calculate version height.

## Version lifecycle

A release moves through these states:

```text
1.0.0-preview.N -> 1.0.0-rc.N -> 1.0.0
```

The suffix is committed in `version.json`. A tag does not convert a preview into an RC or an RC into a stable version.

| State                | GitHub Flow action                              | Example                        |
| -------------------- | ----------------------------------------------- | ------------------------------ |
| Start development    | Open and merge a PR that changes `version.json` | `1.0.0-preview.1`              |
| Continue development | Merge ordinary PRs                              | Same preview version and draft |
| Publish next preview | Open and merge a version PR                     | `1.0.0-preview.2`              |
| Start stabilization  | Open and merge a version PR                     | `1.0.0-rc.1`                   |
| Publish RC           | Tag the approved RC commit                      | `v1.0.0-rc.1`                  |
| Declare stable       | Open and merge a version PR                     | `1.0.0`                        |
| Publish stable       | Tag the approved stable commit                  | `v1.0.0`                       |

NBGV still uses Git history for assembly and informational metadata, but the
public preview number is explicitly committed in `version.json`. It is not a
pull-request counter.

## Configure `version.json`

Use an explicit three-part prerelease version while the release is in development:

```json
{
  "$schema": "https://raw.githubusercontent.com/dotnet/Nerdbank.GitVersioning/main/src/NerdBank.GitVersioning/version.schema.json",
  "version": "1.0.0-preview.1",
  "publicReleaseRefSpec": [
    "^refs/heads/main$",
    "^refs/tags/v\\d+\\.\\d+\\.\\d+(?:-[0-9A-Za-z.-]+)?$"
  ],
  "release": {
    "tagName": "v{version}"
  },
  "cloudBuild": {
    "buildNumber": {
      "enabled": true
    }
  }
}
```

`publicReleaseRefSpec` makes builds from `main` and version tags public release builds. The tag expression includes both stable tags, such as `v1.0.0`, and prerelease tags, such as `v1.0.0-rc.1`.

A public release ref does not remove or add a prerelease suffix. It controls whether NBGV includes the Git commit ID in the calculated public package version.

## Start a preview release

Choose the next release line and change the committed version through a pull request:

```bash
./release-version.sh prepare-preview 1.0.0
git add version.json
git commit -m "Start 1.0 preview"
```

After this change reaches `main`, ordinary pull requests do not modify
`version.json`. They are included in the same rolling Release Drafter draft.
When you want to publish another preview package, create a new version PR:

```text
PR 1: version.json -> 1.0.0-preview.1
  feature A, bug fix, and documentation merge
  draft remains v1.0.0-preview.1

PR 2: run `./release-version.sh prepare-preview 1.0.0`
  version.json -> 1.0.0-preview.2
  draft becomes v1.0.0-preview.2
```

The preview number is an intentional release number, not a pull-request
counter. This Microsoft-style format requires a version change when publishing
the next preview. Ordinary PRs do not create new public package versions.

NBGV stamps the exact committed version and adds Git commit information to
assembly metadata. Topic-branch builds can still include a commit ID because
those branches are not public release refs.

Do not create a version-changing commit for every ordinary PR. Create one when
you intentionally publish the next preview package.

Check the calculated version at any point with:

```bash
nbgv get-version -v SemVer2
```

Use `SemVer2` as the canonical release version everywhere. It preserves the
Microsoft-style prerelease form, for example `1.0.0-preview.1`. Do not use
`NuGetPackageVersion` for Release Drafter metadata because NBGV may normalize
that value to `1.0.0-preview-0001`.

A topic branch that is not listed in `publicReleaseRefSpec` may include a commit ID. That keeps packages from separate pull requests unique before they merge into `main`.

## Publish a release candidate

When the code is ready for stabilization, create a pull request that changes the release intent:

```bash
./release-version.sh prepare-rc 1.0.0
git add version.json
git commit -m "Begin 1.0 release candidate"
```

After the pull request merges, build and test the resulting `main` commit. If that exact commit is approved for RC publication, create its tag:

```bash
nbgv tag
git push origin <tag-created-by-nbgv>
```

`nbgv tag` uses the version calculated for the current commit. It does not change `version.json`.

If another RC is required, merge the required fixes and create a new version PR:

```bash
./release-version.sh prepare-rc 1.0.0
```

## Publish a stable release

When the RC is accepted, create a second pull request that removes the prerelease designation:

```bash
./release-version.sh prepare-stable 1.0.0
git add version.json
git commit -m "Declare 1.0 stable"
```

After the pull request merges, validate that exact commit and create the stable tag:

```bash
nbgv get-version
nbgv tag
git push origin <tag-created-by-nbgv>
```

The resulting tag should be `v1.0.0`. The tag records which commit was released and triggers the tag-based publishing workflow.

After stable publication, advance `main` to the next development line before accepting work for it:

```bash
./release-version.sh prepare-preview 1.1.0
git add version.json
git commit -m "Start 1.1 preview"
```

## Release Drafter and stable release notes

Release Drafter updates one rolling draft after every merge to `main`. The
unified publish job also calculates the NBGV `SemVer2` version and runs for a
`main` merge only when that version contains `-preview.`. A merged preview
version therefore publishes the NuGet package, binaries, and a GitHub
prerelease automatically.

RC and stable merges to `main` still build and test, but do not publish. Their
publication starts only after the approved `v*` tag is pushed with NBGV.

Set this in `.github/release-drafter.yml`:

```yaml
include-pre-releases: false
```

With this setting, Release Drafter ignores published prerelease releases when
choosing the previous release for comparison. This keeps the previous stable
release as the baseline.

For example:

```text
v10.0.0                 previous stable release
  |-- Preview PR A
  |-- Preview PR B
  |-- Preview PR C
v1.0.0-rc.1              published release candidate
  |-- RC fix PR
v1.0.0                  stable release
```

The RC draft includes Preview PR A, B, and C because they are changes since
`v10.0.0`. When the stable draft is prepared with
`include-pre-releases: false`, it also uses `v10.0.0` as its baseline, so the
stable notes include Preview PR A, B, Preview PR C, and RC fix PR.

The RC release itself is not a changelog item. It is a published version of the
same release line. If the project instead wants stable notes to contain only
changes made after the RC, set `include-pre-releases: true`; that is not the
PromptPack policy.

The workflow in `.github/workflows/release-drafter.yml` runs on pushes to
`main`, so it keeps one draft current. NBGV supplies the calculated version,
while Release Drafter collects merged pull requests and formats the notes. The
single publish job in `.github/workflows/build-and-publish.yml` uses that same
calculated version. It publishes previews from `main` and publishes RC/stable
releases from `v*` tags.

Do not use a second release-creation action for the tag. NBGV creates the tag,
Release Drafter publishes the matching draft, and the build workflow uploads
assets to that release.

## End-to-end GitHub Flow

### First preview

Start the first release line from a working branch:

```bash
./release-version.sh prepare-preview 1.0.0
git add version.json
git commit -m "Start 1.0.0-preview.1"
git push origin <branch>
```

Open and merge the pull request. The merge causes the following actions:

1. NBGV calculates `1.0.0-preview.1` from the committed `version.json`.
2. The Release Drafter workflow creates or updates one draft named `v1.0.0-preview.1`.
3. The build workflow builds and tests the merge.
4. The publish job publishes the calculated NuGet package and creates or
   updates the matching GitHub prerelease with its binary assets.

Ordinary feature, bug-fix, test, and documentation pull requests update the
same draft. They do not change the version. To publish another preview, run
`prepare-preview` and merge its version pull request:

```bash
./release-version.sh prepare-preview 1.0.0
```

This changes the version to `1.0.0-preview.2` because the helper reads the
current preview number from `version.json` and increments it.

### First stable release

When preview validation is complete, prepare the stable version:

```bash
./release-version.sh prepare-stable 1.0.0
git add version.json
git commit -m "Prepare 1.0.0 stable release"
git push origin <branch>
```

Merge the pull request and verify the exact commit:

```bash
git checkout main
git pull --ff-only
nbgv get-version -v SemVer2
```

The result must be:

```text
1.0.0
```

Release Drafter updates the rolling draft to `v1.0.0`. On the first release,
it may display a warning that no previous published release exists. This is
expected: `v1.0.0` is the first comparison baseline.

After build and test validation, create and push the tag locally:

```bash
./release-version.sh tag
git push origin v1.0.0
```

The tag triggers the publish job. That job publishes the package, publishes the
matching Release Drafter draft, and uploads release assets. Do not create a
second GitHub Release manually.

### Continue after stable

After `v1.0.0` is published, begin the next release line:

```bash
./release-version.sh prepare-preview 1.1.0
git add version.json
git commit -m "Start 1.1.0-preview.1"
git push origin <branch>
```

After merging the version pull request, the next rolling draft is
`v1.1.0-preview.1`. Stable release notes use `v1.0.0` as their baseline because
`include-pre-releases: false` ignores published preview and RC releases when
choosing the comparison release.

### RC and stable promotion

When `1.1.0` is ready for stabilization:

```bash
./release-version.sh prepare-rc 1.1.0
```

Merge that version pull request, validate the commit, then run:

```bash
./release-version.sh tag
git push origin v1.1.0-rc.1
```

If another RC is required, merge fixes and run `prepare-rc 1.1.0` again. The
helper changes the version to `1.1.0-rc.2`. After the RC is accepted, use
`prepare-stable 1.1.0`, merge it, and tag `v1.1.0`.

Every tag must be created from the exact approved `main` commit. NBGV creates
the tag; Release Drafter does not create tags.

## Release Drafter alongside NBGV

Use NBGV and Release Drafter for different parts of the same release process:

| Activity                 | NBGV or helper                                 | Release Drafter                                     |
| ------------------------ | ---------------------------------------------- | --------------------------------------------------- |
| Choose `1.1.0-preview.1` | `prepare-preview 1.1.0` changes `version.json` | Uses the calculated `SemVer2` value for the draft   |
| Merge ordinary PR        | Keeps the committed version unchanged          | Adds the merged PR to the one rolling draft         |
| Choose `1.1.0-preview.2` | `prepare-preview 1.1.0` increments the version | Updates the same draft to `v1.1.0-preview.2`        |
| Choose `1.1.0-rc.1`      | `prepare-rc 1.1.0` changes `version.json`      | Updates the same draft to `v1.1.0-rc.1`             |
| Approve RC               | `nbgv tag` creates `v1.1.0-rc.1`               | Tag-triggered workflow publishes the matching draft |
| Choose `1.1.0`           | `prepare-stable 1.1.0` changes `version.json`  | Updates the draft to `v1.1.0`                       |
| Approve stable           | `nbgv tag` creates `v1.1.0`                    | Tag-triggered workflow publishes the matching draft |

### During preview development

The push to `main` triggers `.github/workflows/release-drafter.yml`. It runs
NBGV with:

```bash
nbgv get-version -v SemVer2
```

That value is passed to Release Drafter as the version, name, and tag. For
example, after a version PR sets `1.1.0-preview.1`, the draft is named
`v1.1.0-preview.1`. Later ordinary PRs update the same draft and add their
merged pull requests to its notes. They do not create separate drafts.

When the next preview is intentionally prepared, the version PR changes
`version.json` to `1.1.0-preview.2`. The next push to `main` updates the rolling
draft and automatically publishes the calculated preview package and
prerelease. No tag is required for previews.

### During RC or stable publication

After the version PR is merged and the exact commit is approved, run NBGV
locally:

```bash
./release-version.sh tag
git push origin <tag-created-by-nbgv>
```

The tag starts `.github/workflows/build-and-publish.yml`. That workflow uses
`SemVer2` again, packages with the calculated version, publishes to NuGet, and
passes the pushed tag to Release Drafter with `publish: true`. It then uploads
the binary assets. This is the same publish path used for previews; only the
trigger differs.

Do not run Release Drafter manually after pushing the tag, and do not create a
tag from Release Drafter. If a preview publish or tag publish fails, fix the
workflow or release problem and rerun it; NuGet's `--skip-duplicate` makes a
retry of an already published package harmless.

### Stable release-note baseline

The configuration contains:

```yaml
include-pre-releases: false
```

This tells Release Drafter to use the previous stable release as the comparison
baseline. Therefore, stable notes include the preview and RC pull requests for
the release line, while the published RC itself is not treated as a separate
baseline. The first stable release has no previous baseline, so its warning is
normal and disappears for later release lines.

## GitHub Actions requirements

NBGV requires the repository's Git history and `.git` directory. GitHub Actions must use a full checkout:

```yaml
- uses: actions/checkout@v4
  with:
    fetch-depth: 0
```

Keep this setting in every workflow that builds, packs, calculates, or publishes a version. Do not rewrite `version.json` or remove prerelease suffixes inside CI. Version intent belongs in reviewed source-control changes.

The publish job runs after successful builds and tests for either a preview
version on `main` or an approved `v*` tag. RC and stable version changes merged
to `main` do not satisfy the preview condition, so they do not publish until a
tag is pushed.

## Release checklist

### Preview

1. Set the next preview number with `./release-version.sh prepare-preview`.
2. Open and merge the version change as a pull request.
3. Merge normal feature, fix, test, and documentation pull requests.
4. Use `nbgv get-version -v SemVer2` to inspect the calculated version.
5. Merge the version PR; CI automatically publishes the preview package and
   GitHub prerelease.

### RC

1. Set the RC version with `./release-version.sh prepare-rc`.
2. Open and merge that change as a pull request.
3. Test the exact `main` commit.
4. Run `nbgv tag` on the approved commit.
5. Push the tag to trigger publication.

### Stable

1. Set the stable version with `./release-version.sh prepare-stable`.
2. Open and merge that change as a pull request.
3. Test the exact `main` commit.
4. Run `nbgv tag` on the approved commit.
5. Push the tag to trigger publication.
6. Set the next preview version through a new pull request.

## Why use pull requests for version changes?

The version suffix represents a product decision: unstable preview, release candidate, or stable. Keeping that decision in a pull request gives the project a reviewable audit trail and makes the built commit deterministic.

Tags have a different job. They identify the exact commit that was approved and published. Using a tag alone to promote `1.0.0-rc.1` to `1.0.0` would not work because NBGV calculates versions from committed files and Git history.

## References

- [NBGV recommended versioning workflow](https://dotnet.github.io/Nerdbank.GitVersioning/docs/versioning-workflow.html)
- [NBGV simpler single-branch workflow](https://dotnet.github.io/Nerdbank.GitVersioning/docs/versioning-workflow.html#simpler-single-branch-workflow)
- [NBGV cloud-build requirements](https://dotnet.github.io/Nerdbank.GitVersioning/docs/cloudbuild.html#requirements)
- [NBGV CLI](https://dotnet.github.io/Nerdbank.GitVersioning/docs/nbgv-cli.html)
