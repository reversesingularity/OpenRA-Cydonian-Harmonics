---
paths: ["src/**/*.cs"]
---

# C# Trait Rules (OpenRA.Mods.Cydonian)

## Structure

- Every trait is a pair: `FooInfo : TraitInfo` (config) + `Foo` (runtime).
  `FooInfo.Create(ActorInitializer)` returns the runtime instance.
- All YAML-bound tunables are `public readonly` fields on the `*Info` class
  with a `[Desc("...")]` attribute. Runtime classes never mutate Info.
- One trait pair per file; file name matches the trait
  (`AcousticResonance.cs` holds `AcousticResonanceInfo` + `AcousticResonance`).

## Performance (tick loops are the hot path)

- **NO inline allocations in `ITick.Tick` / `ITickRender`**: no `new`, no LINQ,
  no lambdas/closures, no string building, no params-array calls.
- Resolve cross-trait references ONCE at creation using `INotifyCreated`; cache
  into fields. Never `self.Trait<T>()` inside Tick.
  (Verified against engine release-20250330: `INotifyOwnerCreated` does NOT
  exist in this version — use `INotifyCreated`, plus `INotifyOwnerChanged` for
  ownership transfers.)
- Reuse collections (clear-and-refill) instead of reallocating per tick.
- Simulation math is fixed-point: `WDist`, `WAngle`, `WPos`, `int`. Introducing
  `float`/`double` into sim state is a desync bug, not a style issue.

## Correctness

- Conditions (`GrantCondition`/`RequiresCondition`) are the preferred
  inter-trait channel; direct trait references need justification in review.
- Handle actor death/disposal: check `self.IsDead`/`self.Disposed` before
  touching world state in deferred callbacks.
- No exceptions for flow control in sim code.

## Verification (Loop Engineering)

After every edit: `make` must compile clean; new behavior requires a test run
via `make test`. The PostToolUse hook enforces this — a red build blocks
completion.
