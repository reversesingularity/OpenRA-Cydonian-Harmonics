#!/usr/bin/env bash
# Loop Engineering grind — compile + MiniYaml lint until clean, or surface errors.
# Invoke explicitly or via .cursor/hooks.json. Never claim green without observing output.
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT"

MAX_ROUNDS="${GRIND_MAX_ROUNDS:-3}"
LOG_DIR="${ROOT}/.cursor/hooks/logs"
mkdir -p "$LOG_DIR"
STAMP="$(date +%Y%m%d-%H%M%S)"
LOG="${LOG_DIR}/grind-${STAMP}.log"

run_step() {
  local label="$1"
  shift
  echo "" | tee -a "$LOG"
  echo "=== [${label}] $* ===" | tee -a "$LOG"
  if "$@"; then
    echo "=== [${label}] OK ===" | tee -a "$LOG"
    return 0
  else
    local code=$?
    echo "=== [${label}] FAILED (exit ${code}) ===" | tee -a "$LOG"
    return "$code"
  fi
}

MAKE_CMD=(make)
if [[ ! -x "$(command -v make)" ]] && [[ -f ./make.cmd ]]; then
  MAKE_CMD=(cmd.exe //c make.cmd)
fi

UTILITY=()
if [[ -f ./utility.sh ]]; then
  UTILITY=(bash ./utility.sh cydonian --check-yaml)
elif [[ -f ./utility.cmd ]]; then
  UTILITY=(cmd.exe //c utility.cmd cydonian --check-yaml)
else
  echo "ERROR: neither utility.sh nor utility.cmd found" | tee -a "$LOG"
  exit 2
fi

echo "Cydonian Harmonics grind — root=${ROOT}" | tee "$LOG"
echo "Max rounds: ${MAX_ROUNDS}" | tee -a "$LOG"

round=1
while (( round <= MAX_ROUNDS )); do
  echo "" | tee -a "$LOG"
  echo "######## ROUND ${round}/${MAX_ROUNDS} ########" | tee -a "$LOG"
  build_ok=0
  yaml_ok=0

  if run_step "make" "${MAKE_CMD[@]}"; then
    build_ok=1
  fi

  if run_step "check-yaml" "${UTILITY[@]}"; then
    yaml_ok=1
  fi

  if (( build_ok && yaml_ok )); then
    echo "" | tee -a "$LOG"
    echo "GRIND CLEAN — make and check-yaml both passed (round ${round})." | tee -a "$LOG"
    echo "Log: ${LOG}"
    exit 0
  fi

  echo "" | tee -a "$LOG"
  echo "GRIND DIRTY — feed the failures above back into the agent and re-run." | tee -a "$LOG"
  round=$((round + 1))
done

echo "" | tee -a "$LOG"
echo "GRIND STOPPED — still dirty after ${MAX_ROUNDS} rounds. Log: ${LOG}" | tee -a "$LOG"
exit 1
