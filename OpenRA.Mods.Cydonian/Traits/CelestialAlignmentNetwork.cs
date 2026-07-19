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
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Traits
{
	[TraitLocation(SystemActors.World)]
	[Desc("The Great Cosmic Clock plus the authoritative Harmonic Resonance (HR) CellLayer. " +
		"Attach to the World actor. Providers register and stamp fixed-point HR contributions.")]
	public class CelestialAlignmentNetworkInfo : TraitInfo
	{
		[Desc("Full alignment cycle length in ticks.")]
		public readonly int CycleLength = 1500;

		[Desc("Ticks per cycle during which the Leyline network is active.")]
		public readonly int ActiveDuration = 500;

		public override object Create(ActorInitializer init)
		{
			return new CelestialAlignmentNetwork(init.World, this);
		}
	}

	public class CelestialAlignmentNetwork : ITick
	{
		sealed class ProviderStamp
		{
			public CPos[] Cells;
			public int[] Amounts;
			public int ActiveLength;
		}

		readonly CelestialAlignmentNetworkInfo info;
		readonly CellLayer<int> hrField;
		readonly Dictionary<Actor, ProviderStamp> stamps = new Dictionary<Actor, ProviderStamp>();

		int cycleTick;

		public bool IsLeylineActive { get; private set; }

		/// <summary>Orbital alignment factor Ψ in fixed-point (1024 = 1.0). Phase A: full or zero.</summary>
		public int AlignmentFactorFixed => IsLeylineActive ? 1024 : 0;

		public CelestialAlignmentNetwork(World world, CelestialAlignmentNetworkInfo info)
		{
			this.info = info;
			hrField = new CellLayer<int>(world.Map);
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
			}

			// origin reserved for Phase B diagnostics / overlays.
			_ = origin;
		}

		void ITick.Tick(Actor self)
		{
			// HOT PATH: advance cosmic clock only — no cell walks.
			var cycleLength = Math.Max(1, info.CycleLength);
			if (++cycleTick >= cycleLength)
				cycleTick = 0;

			var activeDuration = Math.Max(0, Math.Min(info.ActiveDuration, cycleLength));
			IsLeylineActive = cycleTick < activeDuration;
		}
	}
}
