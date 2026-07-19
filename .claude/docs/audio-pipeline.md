# Audio Pipeline — Cydonian Harmonics

Acoustic Paradigm SFX and voice integration for OpenRA. Enforced alongside
Loop Engineering (`make` / `--check-yaml` / `make test`).

## Design philosophy

Traditional ballistic “kaboom” vocabulary is banned. Weapon and UI clips must
read as clashing frequencies, resonant drones, or shatter harmonics. Filenames
encode the acoustic cause (`shatter-drone`, `dermal-mark-tone`), never
`explosion1`.

## Directory layout

```
mods/cydonian/
  audio/
    weapons/     # Report / ImpactSounds / OnFireSound / LaunchSound
    ui/          # Sounds: notification bank
    ambients/    # Faraday, node hums
    voices/      # Grok TTS (or placeholder) WAV payloads
      cian/
      brennan/
      guardians/
      nephilim/
      tobit/     # Raphael Empyreal Register — Tobit Protocol ONLY
  voices/        # VoiceSet MiniYaml (Select / Action / Die maps)
  notifications/ # Speech: + Sounds: banks
  weapons/       # Weapon defs (Weapons: package — not rules/)
```

OpenRA resolves VoiceSet / Speech clips as `prefix + stem + DefaultVariant`.
Always set `DefaultVariant: .wav` on Cydonian banks. Waveform files keep the
`.wav` extension on disk; YAML stems omit it.

Weapon `Report`, `ImpactSounds`, `OnFireSound`, and `AmbientSound.SoundFiles`
use **path-qualified filenames including `.wav`** (same style as Phase G).

## Performance gate (mandatory)

| Constraint | Value |
|---|---|
| Channels | Mono |
| Format | PCM 16-bit `.wav` (`SoundFormats: Wav`) |
| Sample rate | **22050 Hz** (SFX / speech) or **11025 Hz** (long ambients) |
| Unit response | ≤ 1.5 s |
| Weapon `Report` | ≤ 0.8 s |
| Impact | ≤ 1.2 s |
| Ambient loop | ≤ 4 s seamless |

Reject full-rate stereo masters in `mods/cydonian/audio/`. Westwood `.aud` /
`OpenRA.Mods.Cnc` is deferred — Phase I stays Wav-only.

Regenerate synthetic placeholders (dev only):

```
python mods/cydonian/audio/_gen_placeholders.py
```

## Grok TTS drop protocol

1. Author scripts under canon registers (Cian: world-weary Irish cadence;
   Raphael / Liaigh: Empyreal Register thee/thou + proclamatory caps —
   **Tobit Protocol support-power speech only**).
2. Synthesize via Grok TTS → master WAV.
3. Downsample to the performance gate (mono, 22050 Hz, duration caps).
4. Overwrite the matching file under `audio/voices/<register>/` (same stem).
5. Lore-pass spoken copy via `master-lore` before shipping campaign lines.

Placeholder tones ship first so YAML wiring stays green without waiting on TTS.

## YAML wiring cheat-sheet

```yaml
# weapons/*.yaml
Report: audio/weapons/shatter-clash.wav
Warhead@FX: CreateEffect
	ImpactSounds: audio/weapons/shatter-drone.wav

# rules — ResonanceSupportPower
OnFireSound: audio/weapons/shatter-onfire.wav
InsufficientPowerSpeechNotification: InsufficientPower

# voices/*.yaml
CianVoice:
	DefaultVariant: .wav
	Voices:
		Select: audio/voices/cian/select
		Action: audio/voices/cian/action
		Die: audio/voices/cian/die

# notifications/*.yaml
Speech:
	DefaultVariant: .wav
	Notifications:
		TobitProtocol: audio/voices/tobit/tobit-engage
```

## Canon hard lines

- Acoustic Paradigm naming only.
- Raphael is never a combat `Voiced` actor — `TobitProtocol` Speech only.
- Azazel = Nephilim; Binitarian theology only.

## Verify

```
.\make.cmd all
.\utility.cmd cydonian --check-yaml
.\make.cmd test
```
