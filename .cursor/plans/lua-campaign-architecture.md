# Phase J: Lua Campaign Architecture and Operational Silence

**Status:** IMPLEMENTED  
**Predecessor:** Phase I `db295e7`.

## Shipped

| Artifact | Role |
|---|---|
| `mods/cydonian/rules/world.yaml` | `LuaScript` sandbox on World |
| `mods/cydonian/rules/player.yaml` | Silence conditions, ProvidesRadar, Tobit SP, EmpyrealSpeechGate, EdenTimeDilation |
| `OpenRA.Mods.Cydonian/Traits/EmpyrealSpeechGate.cs` | Mute Empyreal Register Speech |
| `OpenRA.Mods.Cydonian/Traits/EdenTimeDilation.cs` | Resonance amplifier under `eden-dilated` |
| `OpenRA.Mods.Cydonian/Scripting/EmpyrealMediaGlobal.cs` | Gated Lua Speech helper |
| `mods/cydonian/weapons/tobit-protocol.yaml` | Tobit acoustic binding weapon |
| `maps/operational-silence-eden/` | Book 3 Ch14 silence + Eden dilation |
| `maps/dudael-breach/` | Ice-shelf `Actor.Create` swarms |
| `.claude/docs/lua-campaign.md` | Operator docs |

## Canon

Acoustic Paradigm only. Raphael = Tobit Protocol Speech / support power only.
Azazel = Nephilim. Binitarian theology only.

## Verify

- `.\make.cmd`
- `.\utility.cmd cydonian --check-yaml`
- `.\make.cmd test`
