# Cydonian Harmonics — Agent Context

OpenRA total conversion (C# / MiniYaml). Two asymmetric factions fight over
Resonance on Mars-Cydonia. All supernatural mechanics derive from the
**Acoustic Paradigm** — sound/frequency as the root physic. There is no mana,
magic, or psionics vocabulary.

## Factions

| Faction | Signature | Playstyle |
|---|---|---|
| **Oathbound Guardians** | Teal-gold creation harmonics | Defensive resilience, faith-sustained support frequencies, hero-led strikes |
| **Nephilim Collective** | Amber corruption | Swarm/tech pressure, detuned emitters, biomechanical force, monolith structures |

Watcher Remnants supply forbidden **knowledge**; Nephilim supply physical force.
**Azazel is Nephilim** (son of Gadreel) — never a Watcher.

## Resonance Economy (Acoustic Paradigm)

Resonance replaces ore/credits. Harmonic nodes are **captured/attuned**, not
mined out. Guardians attune (sustain-frequency collectors); the Collective
siphons (visible teal-gold → amber detune). Abilities must name their acoustic
cause (frequency, resonance, harmonic, waveform).

**Oiketerion Principle:** Watchers act through knowledge/tech only — never
innate power. Guardian effects are externally granted by support-structure
frequencies.

## Canon Hard Lines (non-negotiable)

1. **Binitarian** theology only (Father + Son). No Trinitarian references.
2. **Azazel = Nephilim**, not a Watcher.
3. All supernatural effects trace to the Acoustic Paradigm.
4. **Raphael / "Liaigh"** is never a standard combat unit — presence is the
   late-game **Tobit Protocol** support power only (Three Limitations apply).

Deep canon: `MASTER_LORE_BOOK.md`, `.claude/docs/lore-bible.md`.

## OpenRA Architecture (composition, not inheritance)

- An **Actor** is an empty shell. Behavior comes from **Traits**.
- Trait pair: `FooInfo : TraitInfo` (YAML-bound config) + `Foo` (runtime).
- MiniYaml in `mods/cydonian/rules/` composes traits. Custom C# lives in
  `OpenRA.Mods.Cydonian/`.
- Prefer **conditions** over direct cross-trait references.

## Loop Engineering (mandatory)

Never claim a green build without observing compiler/lint output:

| Action | Command |
|---|---|
| Full rebuild | `make clean && make` (or `.\make.cmd` on Windows) |
| YAML lint | `./utility.sh cydonian --check-yaml` |
| Unit tests | `make test` |

Continuous grind hook: `.cursor/hooks/grind.sh` (see `.cursor/hooks.json`).

## Cursor Workflow Mandate

1. **Plan Mode first** — before complex OpenRA logic, draft
   `.cursor/plans/<feature>.md`, wait for human approval, then execute.
2. **Parallel git worktrees** — major expansions (e.g. `CelestialAlignmentProvider`)
   run in isolated branches/worktrees to avoid conflicts.
3. **No auto-run YOLO** — halt and ask before destructive terminal commands
   or large-scale file wipes.

## Directory Map

```
OpenRA.Mods.Cydonian/   Custom C# trait assembly
mods/cydonian/          Mod manifest, MiniYaml rules, sequences, chrome
mods/cydonian/rules/    Actor/trait composition
engine/                 Vendored OpenRA (read-only reference)
assets/                 Raw art pre-palette-conformance (not indexed)
.cursor/rules/          Cursor modular rules (*.mdc)
.cursor/hooks/          Loop Engineering grind scripts
.cursor/plans/          Approved architecture plans before execution
.claude/docs/           Deep domain docs (lore, traits, render pipeline)
```
