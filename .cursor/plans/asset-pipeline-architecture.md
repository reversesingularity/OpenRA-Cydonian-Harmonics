# Phase H: Asset Pipeline and Sprite Sequence Architecture

**Status:** IMPLEMENTED — `.\make.cmd all`, `--check-yaml`, `--check-missing-sprites`, `.\make.cmd test` green.  
**Predecessor:** Phase G `270edd2`.

## Shipped

| Artifact | Role |
|---|---|
| `assets/raw/`, `assets/raw/anakim/` | Raw art drop zone |
| `mods/cydonian/uibits/cydonian.pal` | Global 256-color JASC palette (index 0 transparent; remap 80–95) |
| `mods/cydonian/bits/anakim-*.png` | Indexed fixture sheets ≤2048 |
| `OpenRA.Mods.Cydonian/UtilityCommands/RemapPaletteCommand.cs` | `--remap-palette` |
| `sequences/visual.yaml` | Facing tiers T0–T3 (8/16; 32 banned) |
| `sequences/anakim.yaml` | Hull / turret / move split + `anakim` composite |
| `sequences/guardians.yaml`, `nephilim.yaml` | Roster placeholders |
| `ANAKIM.TITAN` | `Image: anakim` + `WithIdleOverlay@TURRET` |

## Import loop

```
.\utility.cmd cydonian --remap-palette assets/raw/<name>.png \
  --palette mods/cydonian/uibits/cydonian.pal \
  --output mods/cydonian/bits/<name>.png \
  --frame-size W,H --frame-amount N
.\utility.cmd cydonian --check-missing-sprites
.\utility.cmd cydonian --check-yaml
```

## Facing budget

- T0/T1 secondary: **8**
- T2/T3 core / Anakim parts: **16**
- **32 forbidden**
