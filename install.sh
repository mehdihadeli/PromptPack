#!/usr/bin/env bash
set -euo pipefail

repository="mehdihadeli/PromptPack"
install_dir="${PROMPTPACK_INSTALL_DIR:-$HOME/.local/bin}"

case "$(uname -s):$(uname -m)" in
    Linux:x86_64|Linux:amd64)
        runtime="linux-x64"
        ;;
    Darwin:x86_64|Darwin:amd64)
        runtime="osx-x64"
        ;;
    Darwin:arm64|Darwin:aarch64)
        runtime="osx-arm64"
        ;;
    *)
        printf '%s\n' "Unsupported platform. PromptPack installer supports Linux x64 and macOS x64/arm64." >&2
        exit 1
        ;;
esac

asset_name="promptpack-${runtime}.tar.gz"
download_url="https://github.com/${repository}/releases/latest/download/${asset_name}"
temporary_archive="$(mktemp)"
trap 'rm -f "$temporary_archive"' EXIT

mkdir -p "$install_dir"
curl --fail --location --silent --show-error "$download_url" --output "$temporary_archive"
tar --extract --gzip --file "$temporary_archive" --directory "$install_dir"
chmod +x "$install_dir/promptpack"

printf 'PromptPack installed to %s/promptpack\n' "$install_dir"
if [[ ":${PATH}:" != *":${install_dir}:"* ]]; then
    printf 'Add this directory to PATH if `promptpack` is not found:\n  %s\n' "$install_dir"
fi