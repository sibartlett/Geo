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
#
# Two output channels are used deliberately:
#   * stderr -> progress logs; shown in session/hook logs but NOT added to the
#               agent's context.
#   * stdout -> a short readiness summary. Claude Code injects a SessionStart
#               hook's stdout into the agent context, so this is how the agent
#               learns the toolchain is ready (or that setup failed).

set -euo pipefail

# Only act in Claude Code on the web; local machines already have their own SDK.
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

log() { echo "[session-start] $*" >&2; }

# Resolve the repo root. $CLAUDE_PROJECT_DIR is set when the harness runs the
# hook; fall back to this script's own location so direct invocation also works.
PROJECT_DIR="${CLAUDE_PROJECT_DIR:-}"
if [ -z "$PROJECT_DIR" ]; then
  PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
fi

# Persist env vars for the whole session. A plain `export` here dies with this
# subshell; $CLAUDE_ENV_FILE is sourced into every subsequent session shell.
persist_env() {
  if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
    printf 'export %s\n' "$1" >> "$CLAUDE_ENV_FILE"
  fi
  export "${1?}"
}
persist_env "DOTNET_CLI_TELEMETRY_OPTOUT=1"
persist_env "DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1"
persist_env "DOTNET_NOLOGO=1"
# So build.sh's husky MSBuild target never reaches for the network-blocked CDN.
persist_env "HUSKY=0"

if command -v dotnet >/dev/null 2>&1; then
  log "dotnet already present ($(dotnet --version 2>/dev/null || echo unknown)); skipping SDK install."
else
  log "Installing .NET 8 SDK from the Ubuntu archive..."
  SUDO=""
  if [ "$(id -u)" -ne 0 ]; then SUDO="sudo"; fi
  export DEBIAN_FRONTEND=noninteractive
  # '|| true' on update so a single unrelated PPA 403 doesn't abort the install.
  $SUDO apt-get update -qq >&2 || true
  if ! $SUDO apt-get install -y -qq dotnet-sdk-8.0 >&2; then
    log "ERROR: 'apt-get install dotnet-sdk-8.0' failed."
    echo "SessionStart hook: FAILED to install the .NET 8 SDK. Builds/tests via ./build.sh will not work until dotnet is installed manually."
    exit 1
  fi
  log "Installed dotnet $(dotnet --version)."
fi

# Hard gate: if dotnet still isn't callable, report it on stdout (agent-visible)
# rather than exiting 0 and letting the agent hit a broken toolchain later.
if ! command -v dotnet >/dev/null 2>&1; then
  log "ERROR: dotnet is not on PATH after install."
  echo "SessionStart hook: .NET SDK is NOT on PATH. Builds/tests via ./build.sh will not work."
  exit 1
fi

DOTNET_VERSION="$(dotnet --version 2>/dev/null || echo unknown)"

# Restore the pinned local tools (CSharpier + Husky) so the linter / format gate
# is runnable. NuGet (api.nuget.org) is on the default allowlist.
TOOLS_STATUS="ok"
if [ -f "$PROJECT_DIR/.config/dotnet-tools.json" ]; then
  log "Restoring local dotnet tools (csharpier, husky)..."
  if (cd "$PROJECT_DIR" && HUSKY=0 dotnet tool restore >&2); then
    log "Local tools restored."
  else
    TOOLS_STATUS="failed"
    log "warning: 'dotnet tool restore' failed; the CSharpier/Husky lint gate may be unavailable."
  fi
fi

log "Ready. Build/test with: ./build.sh Test"

# Agent-visible readiness summary (stdout -> injected into context).
if [ "$TOOLS_STATUS" = "ok" ]; then
  echo "SessionStart hook: .NET SDK $DOTNET_VERSION ready and local tools (csharpier, husky) restored. Build/test with './build.sh Test'; format with 'dotnet csharpier format .'."
else
  echo "SessionStart hook: .NET SDK $DOTNET_VERSION ready, but 'dotnet tool restore' failed so the CSharpier/Husky lint gate may be unavailable. Build/test with './build.sh Test'."
fi
