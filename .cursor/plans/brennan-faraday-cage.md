# Phase G: Brennan McNeeve and the Enochian Faraday Cage

**Status:** IMPLEMENTED — `.\make.cmd all` + `.\make.cmd test` green.  
**Pattern:** Pure MiniYaml ECS composition (no new C#), mirroring Phase F Cian.

---

## 1. Goal

Brennan projects an Enochian Faraday Cage (432 Hz archive jam) that grants `faraday-shield` to nearby allies and gates Azazel's Layer One `RevealsShroud` LoS leak without cleansing `dermal-mark`.

## 2. Traits / YAML surface

| Artifact | Change |
|---|---|
| [`mods/cydonian/rules/harmonics.yaml`](../../mods/cydonian/rules/harmonics.yaml) `^GuardianUnit` | `ExternalCondition@FARADAY` + `RevealsShroud@DERMALMARK` → `RequiresCondition: dermal-mark && !faraday-shield` |
| Same file `BRENNAN.MCNEEVE` | `ProximityExternalCondition@FARADAY` (Ally, 5c0, AffectsParent), `WithRangeCircle@FARADAY` (silver-grey `A0A8B0A0`), `AmbientSound@FARADAY` |
| [`mods/cydonian/fluent/rules.ftl`](../../mods/cydonian/fluent/rules.ftl) | `actor-brennan-mcneeve` |
| [`mods/cydonian/audio/faraday-hum.wav`](../../mods/cydonian/audio/faraday-hum.wav) | Low-frequency placeholder hum |

## 3. Canon check

- Brennan **McNeeve** (not Webb); Acoustic Paradigm (432 Hz archive); Azazel Layer One jam only; mark persists under shield; Raphael out of scope.

## 4. Condition jam

`RevealsShroud` is `ConditionalTrait` only — use `dermal-mark && !faraday-shield`, not `PauseOnCondition`.

## 5. Verify

- `.\make.cmd all` — green
- `.\make.cmd test` (`--check-yaml`) — green
