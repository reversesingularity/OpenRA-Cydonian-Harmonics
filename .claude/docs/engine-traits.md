# Custom Engine Traits — OpenRA.Mods.Cydonian

Reference for the mod's custom C# traits. All follow the OpenRA pair pattern
(`*Info : TraitInfo` config + runtime class) and the performance rules in
`.claude/rules/rules-csharp.md`.

## AcousticResonance (`AcousticResonanceInfo`)

The core Acoustic Paradigm carrier: emits a harmonic field that grants
conditions to actors inside its range.

- **Config (`AcousticResonanceInfo : TraitInfo`)**: `Range` (WDist),
  `Frequency` (int, Hz — flavor + stacking key), `Condition` (string granted to
  affected actors), `AffectsAllies`/`AffectsEnemies` (bool), `Polarity`
  (`Creation` = teal-gold sustain, `Corruption` = amber detune).
- **Runtime**: implements `INotifyCreated` (caches condition tokens and the
  world actor map ONCE) and `ITick` throttled to every N ticks — the proximity
  scan reuses a pre-allocated buffer; zero allocations per tick.
- Same-`Frequency` fields do not stack; opposing polarities cancel toward
  whichever source is closer.
- YAML composition uses `@` labels for multi-field emitters:

  ```
  ResonanceSpire:
    AcousticResonance@SUSTAIN:
      Range: 8c0
      Polarity: Creation
      Condition: sustained
    AcousticResonance@WARD:
      Range: 4c0
      Polarity: Creation
      Condition: warded
  ```

## AnakimPhysicalPresenceScaler (`AnakimPhysicalPresenceScalerInfo`)

Scales Nephilim/Anakim-class units' physical presence — the lore-accurate
expression of hybrid PHYSICALITY (Oiketerion Principle: biology, not miracle).

- **Config**: `PresenceLevels` (int[], HP/damage multipliers per generation),
  `FootprintScale` (int), `CrushClassOverride` (string).
- **Runtime**: implements `INotifyCreated` — resolves the actor's generation
  from init data and applies scaling ONCE at creation. No tick component; the
  scale is immutable after spawn (reference implementation in
  `Systems_Engineering_and_Agentic_Workspace_Architecture_for_Cydonian_Harmonics.md`,
  which uses `INotifyCreated` and constructor-time caching).
- Rendering consequence: Anakim-class sheets exceed the 2048 cap — see
  render-pipeline.md sequence-splitting rules.

## CelestialAlignmentNetwork (World) + CelestialAlignmentProvider (structure)

Split design (Phase A):

- **`CelestialAlignmentNetwork`** on the World actor: cosmic Leyline clock
  (`CycleLength` / `ActiveDuration`) plus the authoritative fixed-point HR
  `CellLayer<int>`. API: `Register` / `Unregister`, `AddContribution` /
  `ClearContribution`, `GetHR(CPos)`, `AlignmentFactorFixed`, `IsLeylineActive`.
- **`CelestialAlignmentProvider`** on immobile Nephilim structures
  (`CELESTIAL_STARGATE`, `RESONANCE.SPIRE`): bakes a circular footprint in
  `INotifyCreated`, stamps `Φ × Ψ / d²` into the HR grid on a throttled
  `ITick`, grants `Condition` while aligned. Required Info fields:
  `Condition`, `BaseFrequency`, `ResonanceRadius`, `DecayScalar`.
- Hot path: no trait lookups, no `FindActorsInCircle`, no per-tick allocations
  after warmup. Unregister via `INotifyActorDisposing`.
- **Phase B (not yet):** path-cost consumers reading `GetHR`.

## Authoring checklist for new traits

1. Scaffold with `/scaffold-trait <Name> <interfaces>`.
2. Tunables on Info as `public readonly` + `[Desc]`; bind from MiniYaml.
3. Cache lookups in `INotifyCreated`/`INotifyOwnerCreated`; empty hot Tick.
4. `make` + `make test` observed green before the trait is "done".
5. Register in `mods/cydonian/rules/` and run `./utility.sh cydonian --check-yaml`.
