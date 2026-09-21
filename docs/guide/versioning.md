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
./release-version.sh prepare-preview 1.0.0 1
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

PR 2: version.json -> 1.0.0-preview.2
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
nbgv get-version
```

A topic branch that is not listed in `publicReleaseRefSpec` may include a commit ID. That keeps packages from separate pull requests unique before they merge into `main`.

## Publish a release candidate

When the code is ready for stabilization, create a pull request that changes the release intent:

```bash
./release-version.sh prepare-rc 1.0.0 1
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
./release-version.sh prepare-rc 1.0.0 2
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
./release-version.sh prepare-preview 1.1.0 1
git add version.json
git commit -m "Start 1.1 preview"
```

## Release Drafter and stable release notes

Release Drafter updates one rolling draft after every merge to `main`. Preview
merges should build and test normally; they also update that same draft with the
new version and merged pull requests. This does not publish a GitHub Release.

When the version is ready for RC or stable publication, create and push the tag
locally with NBGV. The tag-triggered build then publishes the matching draft
after its build, test, package, and asset steps succeed.

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
tag-triggered job in `.github/workflows/build-and-publish.yml` publishes that
matching draft and uploads the release assets; it does not create a second
GitHub Release.

Do not use a second release-creation action for the tag. NBGV creates the tag,
Release Drafter publishes the matching draft, and the build workflow uploads
assets to that release.

## GitHub Actions requirements

NBGV requires the repository's Git history and `.git` directory. GitHub Actions must use a full checkout:

```yaml
- uses: actions/checkout@v4
  with:
    fetch-depth: 0
```

Keep this setting in every workflow that builds, packs, calculates, or publishes a version. Do not rewrite `version.json` or remove prerelease suffixes inside CI. Version intent belongs in reviewed source-control changes.

The repository's tag publishing workflow should run only after a version tag is pushed. The tag workflow can publish both RC and stable packages if that is the project policy; otherwise, distinguish stable tags from prerelease tags in the job condition.

## Release checklist

### Preview

1. Set the next preview number with `./release-version.sh prepare-preview`.
2. Open and merge the version change as a pull request.
3. Merge normal feature, fix, test, and documentation pull requests.
4. Use `nbgv get-version` to inspect the calculated version.
5. Publish preview packages only when the release policy requires them.

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
