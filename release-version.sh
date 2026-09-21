#!/usr/bin/env bash

set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./release-version.sh prepare-preview <major.minor.patch> <number>
  ./release-version.sh prepare-rc <major.minor.patch> <number>
  ./release-version.sh prepare-stable <major.minor.patch>
  ./release-version.sh tag

Examples:
  ./release-version.sh prepare-preview 1.0.0 2
  ./release-version.sh prepare-rc 1.0.0 1
  ./release-version.sh prepare-stable 1.0.0
  ./release-version.sh tag
EOF
}

if [[ $# -lt 1 ]]; then
  usage
  exit 1
fi

if ! command -v nbgv >/dev/null 2>&1; then
  echo "nbgv is required. Install it with: dotnet tool install --global nbgv" >&2
  exit 1
fi

release_version() {
  local base_version="$1"
  local suffix="$2"

  nbgv set-version "${base_version}-${suffix}"
  echo "Updated version.json to ${base_version}-${suffix}. Open a pull request with this change."
}

case "$1" in
  prepare-preview)
    [[ $# -eq 3 ]] || { usage; exit 1; }
    release_version "$2" "preview.$3"
    ;;
  prepare-rc)
    [[ $# -eq 3 ]] || { usage; exit 1; }
    release_version "$2" "rc.$3"
    ;;
  prepare-stable)
    [[ $# -eq 2 ]] || { usage; exit 1; }
    nbgv set-version "$2"
    echo "Updated version.json to $2. Open a pull request with this change."
    ;;
  tag)
    nbgv get-version
    nbgv tag
    ;;
  *)
    usage
    exit 1
    ;;
esac