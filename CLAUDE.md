# Cydonian Harmonics — OpenRA Total Conversion

Asymmetric RTS total conversion on the **OpenRA engine** (C#/.NET runtime, MiniYaml data layer).
Two factions: **Oathbound Guardians** (teal-gold creation harmonics) vs the
**Nephilim Collective** (amber corruption). All supernatural mechanics derive from the
**Acoustic Paradigm** — sound/frequency as the root physic.

## Core Architecture: the OpenRA ECS Paradigm

OpenRA is a **composition** engine, not an inheritance engine:

- An **Actor** is an empty shell. ALL behavior comes from **Traits** attached to it.
- Every trait is a C# pair: `FooInfo : TraitInfo` (immutable, YAML-bound config) +
  `Foo` (runtime instance implementing interfaces like `ITick`, `INotifyCreated`).
- **MiniYaml** (`mods/cydonian/rules/*.yaml`) composes traits onto actors declaratively.
  YAML is data; C# is behavior. Never hardcode balance numbers in C#.
- New behavior = new trait + YAML registration. Never subclass actors.
- Traits talk through **conditions** (`GrantConditionOn*` / `RequiresCondition`), the
  engine's message bus. Prefer conditions over direct cross-trait references.

## Build & Verify — Loop Engineering (MANDATORY)

Never guess whether code works. Every C# or MiniYaml edit is followed by an
observation step before the task may be declared complete. Hooks enforce this
automatically (`.claude/hooks/verify-post-tool.sh`), but run checks explicitly too:

| Action | Command |
|---|---|
| Full rebuild | `make clean && make` |
| YAML lint | `./utility.sh cydonian --check-yaml` |
| Sprite sequences | `./utility.sh cydonian --check-sequence-sprites` |
| Unit tests | `make test` |

Cycle: **Edit → Compile/Lint → Observe output → Fix → only then complete.**
A red build or lint error is never "probably fine."

## Directory Map

```
src/                      Custom C# trait assemblies (OpenRA.Mods.Cydonian)
mods/cydonian/            Mod manifest, MiniYaml rules, sequences, chrome
mods/cydonian/rules/      Actor/trait composition (*.yaml)
mods/cydonian/sequences/  Sprite sequence definitions
engine/                   Vendored OpenRA engine checkout (READ-ONLY reference)
assets/                   Raw art pre-palette-conformance
.claude/                  Workspace automation (settings, hooks, rules, agents, docs)
```

## Progressive Disclosure

Deep documentation lives in `.claude/docs/` — loaded via the imports below;
consult the relevant file before working in its domain:

- @.claude/docs/lore-bible.md — factions, Resonance economy, Oiketerion Principle, canon guardrails
- @.claude/docs/engine-traits.md — custom traits: `AcousticResonanceInfo`, `AnakimPhysicalPresenceScaler`
- @.claude/docs/render-pipeline.md — sprite sheet caps, PNG metadata baking, 8/16-frame facings

## Scoped Rules

Path-triggered rules in `.claude/rules/` (loaded when matching files are touched):

| Rule file | Scope | Enforces |
|---|---|---|
| `rules-csharp.md` | `src/**/*.cs` | No inline allocations in `ITick`; `INotifyOwnerCreated`; `TraitInfo` inheritance |
| `rules-yaml.md` | `mods/**/*.yaml` | Strict 2-space indent (no tabs); `^Templates`; `@` label separators |

## Subagents

| Agent | Delegate when |
|---|---|
| `master-lore` | Any lore-facing text: names, briefings, faction copy — canon validation |
| `sprite-design` | Sprite sheets, sequences, palette work — rendering constraint enforcement |
| `trait-engineer` | Performance-critical C# traits, tick-loop optimization, tests |

## Skills

| Skill | Purpose |
|---|---|
| `/scaffold-trait` | Boilerplate `TraitInfo` + trait pair (manual invocation only) |
| `/process-palette-sheet` | Conform raw art to the 256-color global Cydonian palette |
| `/lint-rules` | Manually run the OpenRA YAML validation utility |

## Canon Hard Lines (non-negotiable — see lore-bible.md)

1. **Binitarian** theology only (Father + Son). NO Trinitarian references, ever.
2. **Azazel is NEPHILIM** — son of Gadreel. He is NOT a Watcher.
3. All supernatural effects trace to the **Acoustic Paradigm**; Watchers act through
   knowledge, never innate power (**Oiketerion Principle**).
