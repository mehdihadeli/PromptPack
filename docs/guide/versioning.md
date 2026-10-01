# Versioning with NBGV

PromptPack uses [Nerdbank.GitVersioning (NBGV)](https://dotnet.github.io/Nerdbank.GitVersioning/) to derive package and assembly versions from `version.json` and Git history.

The release contract is:

- `main` is the long-lived development branch.
- Preview versions use commit height: `MAJOR.MINOR.PATCH-preview.{height}`.
- RC versions use commit height: `MAJOR.MINOR.PATCH-rc.{height}`.
- Stable versions use `MAJOR.MINOR.PATCH`.
- Preview and RC CI artifacts append UTC day-of-year and GitHub Actions run number.
- Tags identify the exact RC or stable commit that was published.

## Configure versioning

`version.json` contains the release line and public refs:

```json
{
  "$schema": "https://raw.githubusercontent.com/dotnet/Nerdbank.GitVersioning/main/src/NerdBank.GitVersioning/version.schema.json",
  "version": "1.1.0-preview.{height}",
  "publicReleaseRefSpec": [
    "^refs/heads/main$",
    "^refs/tags/v\\d+\\.\\d+\\.\\d+(?:-rc\\.\\d+)?$"
  ]
}
```

The local tool manifest pins NBGV 3.10.94. Restore it before local commands:

```bash
dotnet tool restore
dotnet nbgv get-version -v SemVer2
```

`publicReleaseRefSpec` keeps `main` and valid stable or RC tags free of the
commit suffix that appears on ordinary topic-branch builds.

## Version shapes

The workflow produces these effective artifact versions:

| Source ref | Effective version | Environment | Publication |
| --- | --- | --- | --- |
| `main`, preview height `0` | `1.1.0-preview.0` | none | Validation only |
| `main`, preview height `1+` | `1.1.0-preview.N.YYDDD.RUN_NUMBER` | dev | NuGet package and archives |
| `v1.1.0-rc.1` | `1.1.0-rc.1.YYDDD.RUN_NUMBER` | staging | NuGet package and archives |
| `v1.1.0` | `1.1.0` | production | NuGet package and archives |

`YYDDD` is the UTC two-digit year and day of year. `RUN_NUMBER` is the
monotonically increasing GitHub Actions workflow run number. These suffixes
make preview and RC artifacts from separate runs distinguishable; they do not
replace the NBGV preview or RC height.

## Release commands

Start a preview train:

```bash
./release-version.sh prepare-train 1.2.0
git add version.json
git commit -m "chore: start 1.2.0 preview train"
```

This writes `1.2.0-preview.{height}`. Ordinary changes do not edit
`version.json`; the next accepted merge advances the preview height.
The helper also removes any bootstrap height-offset properties left by an
earlier release train.

Prepare RC or stable release intent through a reviewed pull request:

```bash
./release-version.sh prepare-rc 1.2.0
./release-version.sh prepare-stable 1.2.0
```

Commit and merge the selected change before tagging. Create the tag from the
approved merged commit:

```bash
./release-version.sh tag
git push origin <tag-created-by-nbgv>
```

`nbgv tag` creates the tag for the version calculated from the current commit;
it does not edit `version.json`.

## CI version calculation

`calculate-version.sh` is the single CI calculation path. It emits these
GitHub Actions outputs:

- `version`
- `assembly_version`
- `informational_version`
- `environment`
- `docker_tag`
- `commit`

For `main`, `preview.0` is validation-only. Later previews become
`1.2.0-preview.N.YYDDD.RUN_NUMBER` and publish to the `dev` environment.
RC tags become `1.2.0-rc.N.YYDDD.RUN_NUMBER` and target `staging`. Stable
tags keep `1.2.0` and target `production`.

The script rejects unsupported refs and fails when a release tag does not
match the NBGV version calculated for its commit. This prevents publishing a
tag such as `v1.2.0-rc.2` from a commit calculated as `1.2.0-rc.1`.

## GitHub Actions flow

[`.github/workflows/build-and-publish.yml`](../../.github/workflows/build-and-publish.yml)
restores, builds, and tests every pull request and push. Version calculation
runs only for `main` and `v*` tags. Publication runs for non-bootstrap previews
from `main` and for all valid release tags.

The workflow then packs the `PromptPack` global tool, publishes it to NuGet.org,
creates self-contained Linux and macOS archives, updates the matching Release
Drafter release, and uploads the archives. Release Drafter runs with
`if: always()` so release notes remain current when an artifact step fails;
the workflow still remains failed until the artifact problem is fixed.

Before pushing a release tag, run:

```bash
dotnet tool restore
dotnet nbgv get-version -v SemVer2
dotnet test --solution PromptPack.slnx --configuration Release
```

The release helper does not commit or push changes automatically.
