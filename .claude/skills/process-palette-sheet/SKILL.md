---
name: process-palette-sheet
description: Conforms raw sprite sheets to the 256-color global Cydonian palette using the cydonia --remap-palette utility. Use when importing art from assets/ into mods/cydonian/, or when a sheet renders with wrong/washed-out colors.
---

# Process Palette Sheet

Conform raw art in `assets/` to the global 256-color Cydonian palette so the
engine can index it and apply faction remaps.

## Procedure

1. Identify the input sheet(s): `assets/raw/<name>.png`.
2. Run the remap utility:

   ```
   ./utility.sh cydonia --remap-palette assets/raw/<name>.png \
       --palette mods/cydonian/uibits/cydonian.pal \
       --output mods/cydonian/bits/<name>.png
   ```

3. **Observe** the utility output. Any "color out of gamut" warnings mean the
   source art uses colors outside the global palette — report the offending
   index ranges; do not silently accept nearest-neighbor drift on faction
   remap bands.
4. Verify the output is indexed and within engine caps:

   ```
   python -c "from PIL import Image; im = Image.open(r'mods/cydonian/bits/<name>.png'); print(im.mode, im.size)"
   ```

   Expected: mode `P`, size <= 2048x2048. If larger, hand off to the
   `sprite-design` agent for a sequence split plan.

## Palette Contract

- Single global palette: 256 colors, index 0 transparent.
- Faction remap bands: Oathbound Guardians = teal-gold band; Nephilim
  Collective = amber band. Never paint faction colors directly into sprites —
  use the remap indices.

## Done Criteria

Utility ran clean + verification printed `P` mode within caps. Paste both
outputs before declaring the asset processed.
