# Render Pipeline — Cydonian Harmonics

Constraints and procedures for getting art through the OpenRA renderer.
Enforced by the `sprite-design` agent; palette conformance via
`/process-palette-sheet`.

## Sheet & Frame Budget

| Constraint | Value | Notes |
|---|---|---|
| Max sprite sheet | **2048 x 2048 px** | Hard engine cap. Larger sheets must be split |
| Rotational facings | **8** (infantry) / **16** (vehicles, mechs) | Engine interpolates intermediate angles |
| Palette | 256-color global Cydonian palette, index 0 transparent | Single palette for ALL sprites |
| Faction remap | Guardians = teal-gold band; Collective = amber band | Remap indices, never baked colors |

## Sequence Splitting (large actors)

Nephilim mechs and Anakim-class walkers at 16 facings overflow a single 2048
sheet. Required approach:

1. Split by body part into separate sequences: `<actor>-body`,
   `<actor>-arms`, `<actor>-turret`, each on its own sheet within the cap.
2. Compose at runtime with `WithSpriteBody` plus additional `With*` render
   traits, sharing the actor's facing.
3. Keep per-part frame counts identical so facings stay synchronized.
4. Verify with `./utility.sh cydonian --check-sequence-sprites` — observed
   clean output is the done-condition.

## Dynamic PNG Metadata Baking

Sequence metadata (anchor offsets, per-frame origins, shadow offsets) is baked
into PNG ancillary chunks by the asset pipeline at import time:

- The bake step runs as part of `/process-palette-sheet` output; the engine
  loader reads the chunks instead of side-car offset files.
- NEVER hand-edit baked PNGs or their chunks; change the source art or the
  bake parameters and regenerate. Hand edits are overwritten on next bake and
  desync offsets from sequence YAML.
- After regeneration, re-run the sequence check — offsets are only "correct"
  when the utility says so, not when they look right in an image viewer.

## Import Procedure (raw art -> game)

1. Drop raw art in `assets/raw/`.
2. `/process-palette-sheet` — conform to the global palette (observe output).
3. Author the sequence YAML in `mods/cydonian/sequences/` (2-space indent,
   facings matching frame count).
4. `./utility.sh cydonian --check-sequence-sprites` and
   `./utility.sh cydonian --check-yaml` — both observed clean.
5. In-game smoke test per `CLAUDE.local.md` debug flags before sign-off.
