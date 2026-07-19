#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Traits
{
	[TraitLocation(SystemActors.World)]
	[Desc("Great Cosmic Clock, authoritative HR CellLayer, and CustomTerrain band bridge for path costs. " +
		"Quantized Resonance_Tier* terrain types feed Locomotor.cellsCost via stock UpdateCellCost.")]
	public class CelestialAlignmentNetworkInfo : TraitInfo
	{
		[Desc("Full alignment cycle length in ticks.")]
		public readonly int CycleLength = 1500;

		[Desc("Ticks per cycle during which the Leyline network is active.")]
		public readonly int ActiveDuration = 500;

		[FieldLoader.Require]
		[Desc("Ascending fixed-point HR thresholds. Band i+1 applies when GetHR >= threshold[i].")]
		public readonly int[] BandThresholds;

		[FieldLoader.Require]
		[Desc("Terrain type names for each band (same length as BandThresholds), e.g. Resonance_Tier1.")]
		public readonly string[] BandTerrainTypes;

		[Desc("Ticks between CustomTerrain band reconciliation for dirty cells.")]
		public readonly int TerrainSyncInterval = 5;

		public override object Create(ActorInitializer init)
		{
			return new CelestialAlignmentNetwork(init.World, this);
		}
	}

	public class CelestialAlignmentNetwork : ITick, IWorldLoaded
	{
		sealed class ProviderStamp
		{
			public CPos[] Cells;
			public int[] Amounts;
			public int ActiveLength;
		}

		readonly CelestialAlignmentNetworkInfo info;
		readonly World world;
		readonly Map map;
		readonly CellLayer<int> hrField;
		readonly CellLayer<byte> underlayCustom;
		readonly CellLayer<bool> resonanceOwned;
		readonly CellLayer<bool> dirtyFlags;
		readonly List<CPos> dirtyCells = new List<CPos>();
		readonly Dictionary<Actor, ProviderStamp> stamps = new Dictionary<Actor, ProviderStamp>();

		byte[] bandTerrainIndices;
		int cycleTick;
		int terrainSyncAccumulator;
		bool terrainBridgeReady;

		public bool IsLeylineActive { get; private set; }

		/// <summary>Orbital alignment factor Ψ in fixed-point (1024 = 1.0). Phase A: full or zero.</summary>
		public int AlignmentFactorFixed => IsLeylineActive ? 1024 : 0;

		/// <summary>Count of Locomotor.CellCostChanged notifications (proves cellsCost cache updates).</summary>
		public int CellCostChangeNotifications { get; private set; }

		public CelestialAlignmentNetwork(World world, CelestialAlignmentNetworkInfo info)
		{
			this.world = world;
			this.info = info;
			map = world.Map;
			hrField = new CellLayer<int>(map);
			underlayCustom = new CellLayer<byte>(map);
			resonanceOwned = new CellLayer<bool>(map);
			dirtyFlags = new CellLayer<bool>(map);
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			if (info.BandThresholds == null || info.BandTerrainTypes == null ||
				info.BandThresholds.Length == 0 || info.BandThresholds.Length != info.BandTerrainTypes.Length)
				throw new YamlException("CelestialAlignmentNetwork requires BandThresholds and BandTerrainTypes of equal non-zero length.");

			for (var i = 1; i < info.BandThresholds.Length; i++)
				if (info.BandThresholds[i] < info.BandThresholds[i - 1])
					throw new YamlException("CelestialAlignmentNetwork BandThresholds must be ascending.");

			bandTerrainIndices = new byte[info.BandTerrainTypes.Length];
			var terrainInfo = map.Rules.TerrainInfo;
			for (var i = 0; i < info.BandTerrainTypes.Length; i++)
				bandTerrainIndices[i] = terrainInfo.GetTerrainIndex(info.BandTerrainTypes[i]);

			foreach (var locomotor in w.WorldActor.TraitsImplementing<Locomotor>())
				locomotor.CellCostChanged += OnCellCostChanged;

			terrainBridgeReady = true;
		}

		void OnCellCostChanged(CPos cell, short oldCost, short newCost)
		{
			CellCostChangeNotifications++;
		}

		public void Register(Actor provider)
		{
			if (!stamps.ContainsKey(provider))
				stamps.Add(provider, new ProviderStamp());
		}

		public void Unregister(Actor provider)
		{
			ClearContribution(provider);
			stamps.Remove(provider);
		}

		public int GetHR(CPos cell)
		{
			return hrField.Contains(cell) ? hrField[cell] : 0;
		}

		public void ClearContribution(Actor provider)
		{
			if (!stamps.TryGetValue(provider, out var stamp) || stamp.Cells == null)
				return;

			var cells = stamp.Cells;
			var amounts = stamp.Amounts;
			var length = stamp.ActiveLength;
			for (var i = 0; i < length; i++)
			{
				var cell = cells[i];
				if (!hrField.Contains(cell))
					continue;

				hrField[cell] -= amounts[i];
				MarkDirty(cell);
			}

			// Keep Amounts buffer for reuse on the next AddContribution (zero-alloc Tick path).
			stamp.Cells = null;
			stamp.ActiveLength = 0;
		}

		/// <summary>
		/// Stamp HR contributions: cellHR += magnitudeFixed / max(1, distSq[i]).
		/// Caller must ClearContribution first (or call this only after Clear).
		/// Arrays are borrowed from the provider — not copied; do not mutate after stamp.
		/// </summary>
		public void AddContribution(Actor provider, CPos origin, CPos[] cells, int[] distSq, int magnitudeFixed)
		{
			if (cells == null || distSq == null || cells.Length != distSq.Length || magnitudeFixed <= 0)
				return;

			if (!stamps.TryGetValue(provider, out var stamp))
			{
				stamp = new ProviderStamp();
				stamps.Add(provider, stamp);
			}

			// Reuse amount buffer when footprint size is unchanged (zero-alloc after warmup).
			if (stamp.Amounts == null || stamp.Amounts.Length != cells.Length)
				stamp.Amounts = new int[cells.Length];

			stamp.Cells = cells;
			stamp.ActiveLength = cells.Length;

			for (var i = 0; i < cells.Length; i++)
			{
				var cell = cells[i];
				var amount = magnitudeFixed / Math.Max(1, distSq[i]);
				stamp.Amounts[i] = amount;

				if (!hrField.Contains(cell))
					continue;

				hrField[cell] += amount;
				MarkDirty(cell);
			}

			_ = origin;
		}

		void MarkDirty(CPos cell)
		{
			if (!dirtyFlags.Contains(cell) || dirtyFlags[cell])
				return;

			dirtyFlags[cell] = true;
			dirtyCells.Add(cell);
		}

		int QuantizeBand(int hr)
		{
			var band = 0;
			var thresholds = info.BandThresholds;
			for (var i = 0; i < thresholds.Length; i++)
			{
				if (hr >= thresholds[i])
					band = i + 1;
				else
					break;
			}

			return band;
		}

		void ApplyBand(CPos cell, int band)
		{
			if (!map.CustomTerrain.Contains(cell))
				return;

			byte desired;
			if (band <= 0)
			{
				if (!resonanceOwned[cell])
					return;

				desired = underlayCustom[cell];
				resonanceOwned[cell] = false;
			}
			else
			{
				var index = bandTerrainIndices[band - 1];
				if (!resonanceOwned[cell])
				{
					underlayCustom[cell] = map.CustomTerrain[cell];
					resonanceOwned[cell] = true;
				}

				desired = index;
			}

			if (map.CustomTerrain[cell] == desired)
				return;

			// Triggers Map.InvalidateTerrainIndex → Locomotor.UpdateCellCost → CellCostChanged → HPF.
			map.CustomTerrain[cell] = desired;
		}

		void SyncDirtyTerrainBands()
		{
			if (!terrainBridgeReady || dirtyCells.Count == 0)
				return;

			for (var i = 0; i < dirtyCells.Count; i++)
			{
				var cell = dirtyCells[i];
				dirtyFlags[cell] = false;
				ApplyBand(cell, QuantizeBand(GetHR(cell)));
			}

			dirtyCells.Clear();
		}

		void ITick.Tick(Actor self)
		{
			// HOT PATH: clock + throttled dirty-cell CustomTerrain sync (no full-map scans).
			var cycleLength = Math.Max(1, info.CycleLength);
			if (++cycleTick >= cycleLength)
				cycleTick = 0;

			var activeDuration = Math.Max(0, Math.Min(info.ActiveDuration, cycleLength));
			IsLeylineActive = cycleTick < activeDuration;

			var interval = info.TerrainSyncInterval < 1 ? 1 : info.TerrainSyncInterval;
			if (++terrainSyncAccumulator < interval)
				return;

			terrainSyncAccumulator = 0;
			SyncDirtyTerrainBands();
		}
	}
}
