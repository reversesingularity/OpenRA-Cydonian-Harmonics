---
name: process-palette-sheet
description: Conforms raw sprite sheets to the 256-color global Cydonian palette using the cydonian --remap-palette utility. Use when importing art from assets/ into mods/cydonian/, or when a sheet renders with wrong/washed-out colors.
---

# Process Palette Sheet

Conform raw art in `assets/` to the global 256-color Cydonian palette so the
engine can index it and apply faction remaps.

## Procedure

1. Identify the input sheet(s): `assets/raw/<name>.png`.
2. Run the remap utility:

   ```
   ./utility.sh cydonian --remap-palette assets/raw/<name>.png \
       --palette mods/cydonian/uibits/cydonian.pal \
       --output mods/cydonian/bits/<name>.png
   ```

   On Windows:

   ```
   .\utility.cmd cydonian --remap-palette assets/raw/<name>.png --palette mods/cydonian/uibits/cydonian.pal --output mods/cydonian/bits/<name>.png
   ```

   Optional metadata bake (PNG tEXt chunks read by PngSheetLoader):

   ```
   --frame-size W,H --frame-amount N [--offset X,Y]
   ```

3. **Observe** the utility output. Any "color out of gamut" warnings mean the
   source art uses colors outside the global palette — report the offending
   index ranges; do not silently accept nearest-neighbor drift on faction
   remap bands (indices 80–95).
4. Verify the output is indexed and within engine caps:

   ```
   python -c "from PIL import Image; im = Image.open(r'mods/cydonian/bits/<name>.png'); print(im.mode, im.size)"
   ```

   Expected: mode `P`, size <= 2048x2048. If larger, hand off to the
   `sprite-design` agent for a sequence split plan (hull / turret / move).

## Palette Contract

- Single global palette: `mods/cydonian/uibits/cydonian.pal` (256 colors, index 0 transparent).
- Faction remap bands: indices 80–95 via `PlayerColorPalette` (Guardians teal-gold /
  Collective amber at runtime). Never paint faction colors directly into sprites —
  use the remap indices.
- Fixed accent ranges (non-remap): teal 96–111, gold 112–127, amber 128–143.

## Facing Budget (do not exceed)

| Tier | Class | Facings |
|---|---|---|
| T0/T1 | Infantry, secondary vehicles | 8 |
| T2/T3 | Core vehicles, Anakim parts | 16 |
| Banned | Anything | 32 |

## Done Criteria

Utility ran clean + verification printed `P` mode within caps. Then observe:

```
./utility.sh cydonian --check-missing-sprites
./utility.sh cydonian --check-yaml
```

Paste remap + Pillow + check outputs before declaring the asset processed.
