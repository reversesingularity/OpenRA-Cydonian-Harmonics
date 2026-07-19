#!/usr/bin/env bash
# Cursor `stop` hook entry — opt-in only (No Auto-Run YOLO).
# Direct grind: bash .cursor/hooks/grind.sh
# Stop-hook grind: GRIND=1 (agent sets this when instructed)
set -uo pipefail

if [[ "${GRIND:-0}" != "1" ]]; then
  # Fail-open: allow the agent to stop without compiling the world.
  exit 0
fi

exec "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/grind.sh"
