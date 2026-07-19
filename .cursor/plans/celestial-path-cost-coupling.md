# Plan: Phase B — Celestial Path-Cost Coupling (HR → Transit Cost)

**Status:** AWAITING APPROVAL — no Phase B C# until signed off.  
**Worktree:** `.worktrees/phase-b-pathing` on branch `feat/phase-b-pathing` (from `main` @ `ff97fdc`)  
**Depends on:** Phase A `CelestialAlignmentNetwork.GetHR(CPos)` (fixed-point `CellLayer<int>`)

---

## 1. Goal

Multiply (or band-scale) ground transit cost by a Harmonic Resonance penalty so units path *around* high-HR Leyline fields when advantageous — Acoustic Paradigm “terrain detune,” not magic slow aura alone.

**Done when:**

- Path searches observe elevated cell costs in high-HR regions.
- HierarchicalPathFinder stays coherent (cost cache invalidation respected).
- No per-search allocations; fixed-point only; `make all` + YAML lint green in the worktree.

---

## 2. Engine realities (calibrated — this checkout)

| Spec intuition | Actual OpenRA behavior |
|---|---|
| `PathGraph.CalculateCellCost` | **Does not exist** as a named API here. Graphs call `Locomotor.MovementCostToEnterCell` → `MovementCostForCell`. |
| Deep cost eval | `DensePathGraph` / `PathSearch` / `HierarchicalPathFinder` all defer to **`Locomotor`**. |
| `TerrainSpeeds` | Built once into `terrainInfos[]`; **baked into `cellsCost` `CellLayer<short>`** at `WorldLoaded` and on `Tiles` / `CustomTerrain` change via `UpdateCellCost`. |
| Hot lookup | `MovementCostForCell` returns **`cellsCost[layer][cell]`** — O(1) cache, not a live `TerrainSpeeds` dictionary walk. |
| Extensibility | `MovementCostForCell` / `MovementCostToEnterCell` are **non-virtual**. `new` hiding on a subclass **will not** intercept calls through `Locomotor` references. |
| Existing subclass pattern | `SubterraneanLocomotor : Locomotor` only overrides domain-passability Info flags — **not** cost math. |
| Invalidation hook | `Locomotor.CellCostChanged` event — fired when cache updates; HPF subscribes (`RequireCostRefreshInCell`). Injectors must go through real cache updates, not bypass them. |

**Implication:** Dynamically multiplying cost “inside PathSearch” without touching Locomotor’s cache will desync HPF abstractions from leaf costs. Phase B must either (A) feed the existing cache pipeline or (B) ship a full cost-aware locomotor replacement.

---

## 3. Options analysis

### Option A — Recommended: HR bands → `Map.CustomTerrain` + per-faction `TerrainSpeeds`

**Idea:** Quantize `GetHR(cell)` into discrete band terrain types (e.g. `Hr0`…`Hr3`). When providers stamp/clear HR, update `CustomTerrain[cell]` to the band. Locomotor’s existing `UpdateCellCost` recomputes `cellsCost` from `TerrainSpeeds[band]`; `CellCostChanged` refreshes HPF.

```text
GetHR(cell) → band index → CustomTerrain[cell] = bandTerrain
                 ↓
        Locomotor.UpdateCellCost (existing)
                 ↓
        cellsCost[cell] = TerrainSpeeds[band].Cost
                 ↓
        CellCostChanged → HierarchicalPathFinder
```

| Pros | Cons |
|---|---|
| No Locomotor fork; PathSearch untouched | Discrete bands, not continuous `cost * f(HR)` |
| Uses stock cache + HPF invalidation | Must save/restore prior `CustomTerrain` under overlays |
| **Per-Locomotor** `TerrainSpeeds` → Guardians pay more on corruption bands than Nephilim (asymmetric) | Band count / thresholds are balance YAML |
| Aligns with Phase A O(1) `GetHR` | Conflicts if other systems also own `CustomTerrain` on same cells |

**MiniYaml sketch (world locomotor):**

```yaml
Locomotor@GUARDIANFOOT:
	TerrainSpeeds:
		Clear: 100
		Hr1: 120
		Hr2: 160
		Hr3: 220
Locomotor@NEPHILIMFOOT:
	TerrainSpeeds:
		Clear: 100
		Hr1: 100
		Hr2: 110
		Hr3: 130
```

**C# surface:** `HarmonicResonanceTerrainBridge` (World trait) or Network-owned applier:

- On HR stamp/clear (from Network after provider write): for each dirty cell, `band = Quantize(GetHR(cell))`, set CustomTerrain, track overlay ownership.
- Throttle: only cells whose band **changed**.
- Fixed-point thresholds on Info (`BandThresholds:` int list).

### Option B — Continuous scalar via forked `CydonianLocomotor`

**Idea:** Copy `Locomotor` into `OpenRA.Mods.Cydonian` as `CydonianLocomotor`, and in the cost path:

```text
baseCost = cellsCost[cell]   // terrain bake unchanged
hr = network.GetHR(cell)
return (short)min(unreachable-1, baseCost * (1024 + hr * PenaltyPerHr) / 1024)
```

| Pros | Cons |
|---|---|
| True continuous Φ-coupled penalty | **Large** engine-parallel class (~maintain forever) |
| Exact blueprint formula feel | Non-virtual API ⇒ must replace methods by **full type substitution**, not override |
| | Must still fire HPF-equivalent invalidation when HR changes (recompute dirty cells / raise compatible refresh) |
| | `PathFinder` / `Mobile` assume stock `Locomotor` behaviors — high regression risk |

Engine is **read-only** in this workspace — we cannot add `virtual` hooks upstream. Fork-in-mod is the only continuous path without violating that rule.

### Option C — Rejected for pathing: `ISpeedModifier` / condition slow

Affects move **speed** after path choice; units still plan through high-HR cells. Useful as **VFX/feel layer**, not as transit-cost coupling.

### Option D — Rejected: mutate `TerrainSpeeds` dictionary at runtime

Dictionary is Info (immutable rules). Cache would not rebuild correctly; desync / no-op risk.

---

## 4. Recommendation

**Ship Option A (CustomTerrain HR bands)** as Phase B v1.

- Cleanest interface that multiplies effective cell cost **without** breaking `PathSearch` / HPF.
- Preserves Phase A’s O(1) `GetHR` as the source of truth; pathing reads bands derived from it.
- Leaves Option B as a documented escape hatch if design demands continuous costs later.

**Not recommended:** a thin `CydonianLocomotor` that only subclasses stock `Locomotor` and hopes to intercept `TerrainSpeeds` checks — those checks are already baked into `cellsCost` before search runs.

---

## 5. Proposed class / YAML structure (Option A)

### 5.1 `HarmonicResonancePathCostInfo` (World trait)

```csharp
[TraitLocation(SystemActors.World)]
public class HarmonicResonancePathCostInfo : TraitInfo, Requires<CelestialAlignmentNetworkInfo>
{
    [FieldLoader.Require]
    [Desc("Ordered HR thresholds (fixed-point). Band i applies when GetHR >= threshold[i].")]
    public readonly int[] BandThresholds;

    [FieldLoader.Require]
    [Desc("Terrain type names written into CustomTerrain for each band (length = thresholds.Length).")]
    public readonly string[] BandTerrainTypes;

    [Desc("Ticks between band reconciliation sweeps for dirty cells.")]
    public readonly int UpdateInterval = 5;
}
```

Runtime: `ITick` + `IWorldLoaded` — cache `CelestialAlignmentNetwork`, `Map`, terrain indices for band names; maintain `CellLayer<byte>` lastBand; apply CustomTerrain deltas only on change.

### 5.2 Network integration

- Provider stamp path already dirtying cells: either  
  - **(preferred)** Network exposes `IEnumerable` / ring-buffer of dirty cells since last path-cost tick, or  
  - Path-cost trait scans provider footprints (cached lists from Phase A register) — no full-map scan.
- Never `FindActorsInCircle` on Tick.

### 5.3 Tileset + Locomotor YAML

- Add `Hr1`…`HrN` terrain types to `mods/cydonian/tilesets/example.yaml` (or successor).
- Extend each ground `Locomotor` `TerrainSpeeds` with band costs (Guardian vs Nephilim asymmetry).

### 5.4 Lore / faction

- High HR from Nephilim Stargates = amber corruption field → Guardians pay higher transit; Nephilim partially adapted (lower band costs). Vocabulary: detune / resonance pressure — not “magic mud.”

---

## 6. Performance & desync checklist

- Fixed-point thresholds only; no float.
- Dirty-cell set clear-and-reuse; no LINQ on Tick.
- CustomTerrain writes must be deterministic and identical on all peers (HR grid already is).
- When band returns to 0: restore **previous** CustomTerrain (store underlay in `CellLayer<byte>`).
- Confirm HPF refreshes via stock `CellCostChanged` after CustomTerrain mutation (no manual HPF calls).
- Cap dirty cells per interval if needed.

---

## 7. Build order (post-approval, in worktree)

1. Expand repo contents in git (worktree currently has Phase A commit only — need project scaffold committed or copied for `make`) — **approval item**.
2. Implement `HarmonicResonancePathCost` + Network dirty-cell API.
3. Tileset terrain types + Locomotor `TerrainSpeeds` bands.
4. `.\make.cmd all` then `.\utility.cmd cydonian --check-yaml`.
5. Manual path smoke: unit paths around high-HR stargate under Guardian locomotor.

---

## 8. Risks

| Risk | Mitigation |
|---|---|
| `CustomTerrain` fights bridges/other overlays | Ownership mask; only overwrite cells we tagged |
| Sparse git worktree (Phase A files only) | Commit remaining mod scaffold before Phase B coding |
| Band quantization feel | Tune thresholds in YAML; document Option B escape |
| HPF stale costs | Rely on Locomotor `UpdateCellCost` path only |

---

## 9. Approval checklist

1. **Accept Option A (CustomTerrain bands)** as Phase B v1?  
2. Reject Option B (forked continuous Locomotor) for now?  
3. Band count default (e.g. 3) OK?  
4. May we commit the rest of the mod scaffold to `main` so the worktree can build?

---

*No Phase B implementation until this plan is explicitly approved.*
