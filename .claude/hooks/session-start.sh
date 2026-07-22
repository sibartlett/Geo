#!/bin/bash
#
# SessionStart hook for Claude Code on the web.
#
# The cloud container ships without a .NET SDK, and `build.sh` otherwise tries to
# fetch one from builds.dotnet.microsoft.com / dotnetcli.azureedge.net, which the
# default network policy blocks. Those hosts are NOT needed: the .NET 8 SDK is
# available from the allowed Ubuntu archive (and packages.microsoft.com), so we
# install it here. Once `dotnet` is on PATH, build.sh uses it and skips the CDN.
#
# Runs only in remote (web) sessions. Idempotent and non-interactive.

set -euo pipefail

# Only act in Claude Code on the web; local machines already have their own SDK.
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

log() { echo "[session-start] $*" >&2; }

if command -v dotnet >/dev/null 2>&1; then
  log "dotnet already present ($(dotnet --version 2>/dev/null || echo unknown)); skipping SDK install."
else
  log "Installing .NET 8 SDK from the Ubuntu archive..."
  SUDO=""
  if [ "$(id -u)" -ne 0 ]; then SUDO="sudo"; fi
  export DEBIAN_FRONTEND=noninteractive
  # '|| true' on update so a single unrelated PPA 403 doesn't abort the install.
  $SUDO apt-get update -qq >&2 || true
  $SUDO apt-get install -y -qq dotnet-sdk-8.0 >&2
  log "Installed dotnet $(dotnet --version)."
fi

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Restore the pinned local tools (CSharpier + Husky) so the linter / format gate
# is runnable. NuGet (api.nuget.org) is on the default allowlist.
if [ -f "${CLAUDE_PROJECT_DIR:-.}/.config/dotnet-tools.json" ]; then
  log "Restoring local dotnet tools (csharpier, husky)..."
  (cd "${CLAUDE_PROJECT_DIR:-.}" && HUSKY=0 dotnet tool restore >&2) || \
    log "warning: 'dotnet tool restore' failed; the CSharpier/Husky lint gate may be unavailable."
fi

log "Ready. Build/test with: ./build.sh Test"
