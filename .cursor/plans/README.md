# Architecture Plans

Before implementing complex OpenRA logic (new traits, economy systems, campaign
scripts, major YAML compositions), draft a step-by-step plan here and wait for
human approval.

## Naming

```
.cursor/plans/<feature-slug>.md
```

Examples: `celestial-alignment-provider.md`, `resonance-node-economy.md`.

## Required sections

1. **Goal** — what ships when this is done
2. **Traits / YAML surface** — new `*Info` types, actor keys, conditions
3. **Canon check** — Acoustic Paradigm + hard lines (Raphael = Tobit only, etc.)
4. **Build order** — files to touch, worktree/branch if major
5. **Verify** — `make`, `./utility.sh cydonian --check-yaml`, `make test`
6. **Risks** — desync, allocations in Tick, lore violations

Do not execute implementation until the plan is explicitly approved.
