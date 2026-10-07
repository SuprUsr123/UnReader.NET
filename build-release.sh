#!/usr/bin/env bash
set -Eeuo pipefail

repo_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cd "$repo_root"

build_windows=true
build_linux=true
case "${1:---all}" in
  --all) ;;
  --windows) build_linux=false ;;
  --linux) build_windows=false ;;
  *) printf 'Usage: %s [--all|--windows|--linux]\n' "$0" >&2; exit 2 ;;
esac

python_cmd="$(command -v python3 || command -v python)"
build_config_dir="${XDG_CONFIG_HOME:-$HOME/.config}/unreader-net"
build_server_url_file="$build_config_dir/server-url"
saved_server_url=""
if [[ -r "$build_server_url_file" ]]; then
  IFS= read -r saved_server_url < "$build_server_url_file" || true
fi
server_url="${UNREADER_SERVER_URL:-}"
if [[ -z "$server_url" ]]; then
  if [[ "${CI:-}" == true ]]; then
    printf 'UNREADER_SERVER_URL is required in CI.\n' >&2
    exit 1
  fi
  server_url_default="${saved_server_url:-http://localhost:10000}"
  read -r -p "Server URL to configure into this build [$server_url_default]: " server_url
  server_url="${server_url:-$server_url_default}"
fi
server_url="${server_url%/}"

saved_jwt_secret=""
jwt_secret="${UNREADER_JWT_SECRET:-}"
if [[ -z "$jwt_secret" ]]; then
  if command -v secret-tool >/dev/null 2>&1; then
    saved_jwt_secret="$(secret-tool lookup application UnReader.NET purpose build-jwt 2>/dev/null || true)"
  fi
  if [[ "${CI:-}" == true ]]; then
    printf 'UNREADER_JWT_SECRET is required in CI.\n' >&2
    exit 1
  elif [[ -n "$saved_jwt_secret" ]]; then
    read -r -s -p "Server JWT secret (press Enter to use the saved keyring value): " jwt_secret
    if [[ -z "$jwt_secret" ]]; then jwt_secret="$saved_jwt_secret"; fi
  else
    read -r -s -p "Server JWT secret: " jwt_secret
  fi
fi
printf '\n'
if [[ -z "$jwt_secret" ]]; then
  printf 'A JWT secret is required.\n' >&2
  exit 1
fi

"$python_cmd" - "$server_url" <<'PY'
import sys
from urllib.parse import urlparse

url = urlparse(sys.argv[1])
if url.scheme not in ("http", "https") or not url.netloc:
    raise SystemExit("Enter a full server URL beginning with http:// or https://")
PY

temporary_dir="$(mktemp -d)"
files=(
  "UnReader.NET.Core/ServerReader.vb"
  "UnReader.NET.Gtk/Program.vb"
  "UnReader.NET.Linux/Program.vb"
)

restore_sources() {
  local index=0
  for file in "${files[@]}"; do
    if [[ -f "$temporary_dir/$index" ]]; then
      cp -p -- "$temporary_dir/$index" "$repo_root/$file"
    fi
    index=$((index + 1))
  done
  rm -rf -- "$temporary_dir"
}
trap restore_sources EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
trap 'exit 129' HUP

for index in "${!files[@]}"; do
  cp -p -- "${files[$index]}" "$temporary_dir/$index"
done

"$python_cmd" - "$server_url" "${files[@]}" <<'PY'
import pathlib
import sys

url, *names = sys.argv[1:]
vb_url = '"' + url.replace('"', '""') + '"'
replacements = {
    "UnReader.NET.Core/ServerReader.vb": (
        'Public Property ServerUrl As String = "http://localhost:10000"',
        f"Public Property ServerUrl As String = {vb_url}",
    ),
    "UnReader.NET.Gtk/Program.vb": (
        'AppSettings.Current.ServerUrl, envServerUrl',
        f'{vb_url}, envServerUrl',
    ),
    "UnReader.NET.Linux/Program.vb": (
        'Environment.GetEnvironmentVariable("UNREADER_SERVER_URL"), "http://localhost:10000"',
        f'Environment.GetEnvironmentVariable("UNREADER_SERVER_URL"), {vb_url}',
    ),
}

for name in names:
    path = pathlib.Path(name)
    text = path.read_text(encoding="utf-8-sig")
    old, new = replacements[name]
    if text.count(old) != 1:
        raise SystemExit(f"Expected one default server URL in {name}; source was not changed")
    path.write_text(text.replace(old, new), encoding="utf-8")
PY

printf '\nPublishing with the configured server URL...\n'
publish_failed=0
runtime_identifier="${UNREADER_RUNTIME_IDENTIFIER:-}"
self_contained="${UNREADER_SELF_CONTAINED:-false}"
version="${UNREADER_VERSION:-}"
if [[ -z "$runtime_identifier" && "$self_contained" == true ]]; then
  printf 'UNREADER_RUNTIME_IDENTIFIER is required for a self-contained publish.\n' >&2
  exit 1
fi

publish_project() {
  local project="$1" output="$2"
  local publish_args=()
  if [[ -n "$runtime_identifier" ]]; then
    publish_args+=(--runtime "$runtime_identifier" --self-contained "$self_contained")
  fi
  if [[ -n "$version" ]]; then publish_args+=("-p:Version=$version"); fi
  UNREADER_JWT_SECRET="$jwt_secret" dotnet publish "$project" -c Release "${publish_args[@]}" -o "$output"
}

if [[ "$build_windows" == true ]]; then
  if publish_project ./UnReader.NET/UnReader.NET.vbproj ./release/windows; then
    printf 'Windows WinForms publish succeeded.\n'
  else
    printf 'Windows WinForms publish failed.\n' >&2
    publish_failed=1
  fi
fi

if [[ "$build_linux" == true ]]; then
  if publish_project ./UnReader.NET.Gtk/UnReader.NET.Gtk.vbproj ./release/linux; then
    printf 'Linux GTK publish succeeded.\n'
  else
    printf 'Linux GTK publish failed.\n' >&2
    publish_failed=1
  fi
  if publish_project ./UnReader.NET.Linux/UnReader.NET.Linux.vbproj ./release/linux-runner; then
    printf 'Linux console publish succeeded.\n'
  else
    printf 'Linux console publish failed.\n' >&2
    publish_failed=1
  fi
fi
if (( publish_failed )); then
  printf '\nOne or more publishes failed. Successful outputs are in ./release.\n' >&2
  exit 1
fi

if [[ "${CI:-}" == true ]]; then
  save_build_credentials="N"
else
  read -r -p "Save the server URL and JWT secret for the next build? [y/N]: " save_build_credentials
fi
if [[ "$save_build_credentials" =~ ^[Yy]$ ]]; then
  mkdir -p -- "$build_config_dir"
  chmod 700 -- "$build_config_dir"
  (umask 077; printf '%s\n' "$server_url" > "$build_server_url_file")
  chmod 600 -- "$build_server_url_file"
  if command -v secret-tool >/dev/null 2>&1; then
    if printf '%s' "$jwt_secret" | secret-tool store --label="UnReader.NET build JWT" application UnReader.NET purpose build-jwt; then
      printf 'Build URL and JWT secret saved for this user.\n'
    else
      printf 'Saved the server URL, but the JWT secret could not be stored in the system keyring.\n' >&2
    fi
  else
    printf 'Saved the server URL. secret-tool is unavailable, so the JWT secret was not saved.\n' >&2
  fi
fi
unset jwt_secret saved_jwt_secret

printf '\nBuild complete. Source defaults are being restored; published files are in ./release.\n'
