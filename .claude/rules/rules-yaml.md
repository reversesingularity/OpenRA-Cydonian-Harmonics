---
paths: ["mods/**/*.yaml"]
---

# MiniYaml Rules (mods/cydonian)

MiniYaml is OpenRA's dialect — indentation-scoped key/value trees. It is NOT
standard YAML; generic YAML tooling will lie to you. Trust
`./utility.sh cydonian --check-yaml` only.

## Formatting (lint-enforced)

- **Strict 2-space indentation. Tabs are forbidden** and break the parser.
- Keys end with `:`; values follow on the same line. No flow syntax (`{}`/`[]`),
  no anchors, no multi-line scalars.
- One actor definition per top-level key; group files by domain
  (`infantry.yaml`, `structures.yaml`, `defaults.yaml`).

## Composition Patterns

- **Templates:** shared trait bundles live in abstract template actors prefixed
  with `^` (e.g. `^BasicBuilding`, `^GuardianInfantry`). Concrete actors inherit
  via `Inherits: ^BasicBuilding` (additional parents:
  `Inherits@AURA: ^ResonanceAura`). Never copy-paste trait blocks — extend a
  template.
- **Duplicate traits:** the same trait attached twice MUST be disambiguated with
  an `@` label suffix:

  ```
  GrantConditionOnDamageState@CRIPPLED:
  GrantConditionOnDamageState@BERSERK:
  ```

- **Removal:** strip an inherited trait with the `-` prefix (`-Selectable:`).
- Faction/canon vocabulary in names and tooltips must pass `master-lore`
  validation (Azazel = Nephilim; no Trinitarian language).

## Verification (Loop Engineering)

After every edit run `./utility.sh cydonian --check-yaml` and read the output.
The PostToolUse hook runs it automatically; fix every reported error before
declaring the change complete.
