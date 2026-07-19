# Phase I: Audio Pipeline and Acoustic Weaponry

**Status:** IMPLEMENTED — `.\make.cmd all`, `--check-yaml`, `.\make.cmd test` green.  
**Predecessor:** Phase H `b6430a2`.

## Shipped

| Artifact | Role |
|---|---|
| `mods/cydonian/audio/{weapons,ui,ambients,voices/*}` | Waveform layout; Faraday migrated to `ambients/` |
| Placeholder mono 22050 Hz WAVs | Clash / drone / voice stubs under performance gate |
| `weapons/acoustic-common.yaml` | `^AcousticStrike` template |
| `weapons/shatter-protocol.yaml` | `Report` + `ImpactSounds` |
| `weapons/dermal-mark.yaml` | `Report` + `PlayUiSound.Sound` |
| `rules/harmonics.yaml` | `OnFireSound` / `LaunchSound` / speech notifications; Faraday path; `Voiced` |
| `voices/example.yaml` | Guardian / Nephilim / Cian / Brennan VoiceSets |
| `notifications/example.yaml` | Speech + Sounds banks incl. `TobitProtocol` |
| `.claude/docs/audio-pipeline.md` | Performance gate + Grok TTS drop protocol |

## Canon

Acoustic Paradigm SFX only. Raphael = Tobit Protocol Speech only (no combat VoiceSet).

## Verify

- `.\make.cmd all`
- `.\utility.cmd cydonian --check-yaml`
- `.\make.cmd test`
