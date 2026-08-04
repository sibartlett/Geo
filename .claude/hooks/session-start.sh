#!/bin/bash
#
# SessionStart hook for Claude Code on the web.
#
# The cloud container ships with an older .NET SDK (8.x) or none at all, and
# `build.sh` otherwise tries to fetch the version pinned in global.json from
# builds.dotnet.microsoft.com / dotnetcli.azureedge.net, which the default
# network policy blocks. That CDN is NOT needed: the .NET 10 SDK is available
# from the allowed Ubuntu archive, so we install it here. Once a 10.x SDK is on
# PATH, build.sh uses it and skips the CDN.
#
# The archive's SDK is a 10.0.1xx build, which is why global.json pins
# 10.0.100 with "rollForward": "latestFeature" rather than an exact patch.
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

# The repo targets net10.0, so a preinstalled 8.x SDK is not good enough: probe
# for a 10.x SDK specifically rather than for the `dotnet` muxer. `--list-sdks`
# is used because `dotnet --version` fails outright when global.json cannot be
# satisfied by any installed SDK.
installed_dotnet_10() {
  command -v dotnet >/dev/null 2>&1 &&
    dotnet --list-sdks 2>/dev/null | grep -q '^10\.'
}

if installed_dotnet_10; then
  log "A .NET 10 SDK is already present; skipping SDK install."
else
  log "Installing .NET 10 SDK from the Ubuntu archive..."
  SUDO=""
  if [ "$(id -u)" -ne 0 ]; then SUDO="sudo"; fi
  export DEBIAN_FRONTEND=noninteractive
  # '|| true' on update so a single unrelated PPA 403 doesn't abort the install.
  $SUDO apt-get update -qq >&2 || true
  if ! $SUDO apt-get install -y -qq dotnet-sdk-10.0 >&2; then
    log "ERROR: 'apt-get install dotnet-sdk-10.0' failed."
    echo "SessionStart hook: FAILED to install the .NET 10 SDK. Builds/tests via ./build.sh will not work until dotnet is installed manually."
    exit 1
  fi
fi

# Hard gate: if a 10.x SDK still isn't callable, report it on stdout
# (agent-visible) rather than exiting 0 and letting the agent hit a broken
# toolchain later.
if ! installed_dotnet_10; then
  log "ERROR: no .NET 10 SDK on PATH after install."
  echo "SessionStart hook: no .NET 10 SDK is on PATH. Builds/tests via ./build.sh will not work."
  exit 1
fi

# Resolved against global.json; fall back to the newest 10.x the muxer lists.
DOTNET_VERSION="$(cd "$PROJECT_DIR" && dotnet --version 2>/dev/null)"
if [ -z "$DOTNET_VERSION" ]; then
  DOTNET_VERSION="$(dotnet --list-sdks | grep '^10\.' | tail -n 1 | cut -d' ' -f1)"
  log "warning: 'dotnet --version' failed in $PROJECT_DIR; global.json may pin an SDK that is not installed."
fi

# Restore the pinned local tools (CSharpier + Husky) so the linter / format gate
# is runnable. NuGet (api.nuget.org) is on the default allowlist.
TOOLS_STATUS="ok"
if [ -f "$PROJECT_DIR/.config/dotnet-tools.json" ]; then
  log "Restoring local dotnet tools (csharpier, husky)..."
  if (cd "$PROJECT_DIR" && HUSKY=0 dotnet tool restore >&2); then
    log "Local tools restored."
    # HUSKY=0 suppresses the MSBuild target that would normally do this, so
    # wire up the git hooks here; without it 'dotnet husky run --group verify'
    # (the CheckForUncommittedChanges target) fails with "Could not find Husky path".
    if ! (cd "$PROJECT_DIR" && dotnet husky install >&2); then
      log "warning: 'dotnet husky install' failed; './build.sh CheckForUncommittedChanges' may fail."
    fi
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
