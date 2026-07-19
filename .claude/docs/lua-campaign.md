# Lua Campaign Architecture

Phase J wires OpenRA's secured **Lua 5.1** sandbox for single-player campaign maps
and the Book 3 Ch14 **Operational Silence** condition bus.

## World sandbox

[`mods/cydonian/rules/world.yaml`](../../mods/cydonian/rules/world.yaml) attaches:

```yaml
world:
  LuaScript:
    Scripts:
```

Empty global `Scripts:` enables the sandbox. Campaign maps override in map
`rules.yaml` (paths are relative to the map package):

```yaml
world:
  LuaScript:
    Scripts: campaign-bootstrap.lua, operational-silence.lua, map-events.lua, mission.lua
```

Lobby / shellmap maps keep empty scripts (no campaign logic).

## Operational Silence API

| Effect | Mechanism |
|---|---|
| Radar off | `ProvidesRadar` on `player` with `RequiresCondition: !operational-silence` |
| Tobit revoked | `ResonanceSupportPower@TOBIT` with `RequiresCondition: empyreal-link && !operational-silence` |
| Empyreal Register mute | `EmpyrealSpeechGate` + `EmpyrealMedia.PlaySpeechNotification` |

Lua (`operational-silence.lua`):

- `OperationalSilence.Enter(player)` — grant `operational-silence`, revoke `empyreal-link`, mirror onto owned `RESONANCE.SPIRE`
- `OperationalSilence.Exit(player)` — reverse
- `OperationalSilence.PlaySpeech(player, name)` — gated Speech helper

## Dynamic events

| Event | API |
|---|---|
| Dudael Breach | `Trigger.OnEnteredProximityTrigger` + `Actor.Create("AZAZEL.PROXY", ...)` |
| Eden dilation | `DateTime.GameTime` / `Trigger.AfterDelay` + player condition `eden-dilated` → `EdenTimeDilation` |

## Stub maps

- `mods/cydonian/maps/operational-silence-eden/`
- `mods/cydonian/maps/dudael-breach/`

## Canon

Raphael / Liaigh = Tobit Protocol support power only. Azazel = Nephilim.
Silence = severed Empyreal frequency channel (Acoustic Paradigm).
