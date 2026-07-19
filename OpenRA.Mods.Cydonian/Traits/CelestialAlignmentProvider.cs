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

using System.Collections.Generic;
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Traits
{
	[Desc("Projects a Leyline resonance field tied to Cydonian orbital alignment. " +
		"Attach to immobile Nephilim Stargates / monoliths. Writes fixed-point HR into the world's " +
		"CelestialAlignmentNetwork CellLayer and grants a condition while aligned.")]
	public class CelestialAlignmentProviderInfo : TraitInfo
	{
		[FieldLoader.Require]
		[GrantedConditionReference]
		[Desc("Condition to grant on this actor while celestially aligned.")]
		public readonly string Condition = null;

		[FieldLoader.Require]
		[Desc("Base tuning frequency Φ of this transceiver (Hz-equivalent int).")]
		public readonly int BaseFrequency;

		[FieldLoader.Require]
		[Desc("Spatial falloff radius of the Leyline resonance field.")]
		public readonly WDist ResonanceRadius;

		[FieldLoader.Require]
		[Desc("Decay scalar λ applied to HR contribution (fixed-point, 1024 = 1.0).")]
		public readonly int DecayScalar;

		[Desc("Only contribute / grant while CelestialAlignmentNetwork reports an active Leyline window.")]
		public readonly bool RequiresAlignmentNode = true;

		[Desc("Ticks between HR grid writes and condition refresh (throttle hot path).")]
		public readonly int UpdateInterval = 5;

		public override object Create(ActorInitializer init)
		{
			return new CelestialAlignmentProvider(this);
		}
	}

	public class CelestialAlignmentProvider : ITick, INotifyCreated, INotifyActorDisposing
	{
		// NOTE: release-20250330 exposes INotifyCreated as the creation-time hook
		// (INotifyOwnerCreated does not exist in this engine version).
		readonly CelestialAlignmentProviderInfo info;

		CelestialAlignmentNetwork network;
		CPos[] footprint;
		int[] distSq;
		CPos originCell;
		int token = Actor.InvalidConditionToken;
		int tickAccumulator;
		bool registered;
		bool clearedOnDeath;

		public CelestialAlignmentProvider(CelestialAlignmentProviderInfo info)
		{
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			network = self.World.WorldActor.TraitOrDefault<CelestialAlignmentNetwork>();
			originCell = self.Location;
			BakeFootprint(self.World.Map, originCell);

			if (network != null)
			{
				network.Register(self);
				registered = true;
			}
		}

		void BakeFootprint(Map map, CPos origin)
		{
			var radiusCells = info.ResonanceRadius.Length / 1024;
			if (radiusCells < 0)
				radiusCells = 0;

			// Creation-time allocation only: count, then fill parallel arrays.
			var cells = new List<CPos>();
			var distances = new List<int>();
			for (var dy = -radiusCells; dy <= radiusCells; dy++)
			{
				for (var dx = -radiusCells; dx <= radiusCells; dx++)
				{
					var d2 = dx * dx + dy * dy;
					if (d2 > radiusCells * radiusCells)
						continue;

					var cell = origin + new CVec(dx, dy);
					if (!map.Contains(cell))
						continue;

					cells.Add(cell);
					distances.Add(d2);
				}
			}

			footprint = cells.ToArray();
			distSq = distances.ToArray();
		}

		void ITick.Tick(Actor self)
		{
			// HOT PATH: no allocations, no trait lookups, no FindActorsInCircle.
			if (self.IsDead)
			{
				if (!clearedOnDeath)
				{
					clearedOnDeath = true;
					RevokeCondition(self);
					network?.ClearContribution(self);
				}

				return;
			}

			var interval = info.UpdateInterval < 1 ? 1 : info.UpdateInterval;
			if (++tickAccumulator < interval)
				return;

			tickAccumulator = 0;

			var aligned = !info.RequiresAlignmentNode || (network != null && network.IsLeylineActive);

			if (aligned && token == Actor.InvalidConditionToken)
				token = self.GrantCondition(info.Condition);
			else if (!aligned && token != Actor.InvalidConditionToken)
				token = self.RevokeCondition(token);

			if (network == null)
				return;

			network.ClearContribution(self);

			if (!aligned)
				return;

			// When RequiresAlignmentNode is false, contribute at full Ψ regardless of Leyline window.
			var psi = info.RequiresAlignmentNode ? network.AlignmentFactorFixed : 1024;

			// Fixed-point via long to avoid overflow: (Φ * Ψ / 1024) * λ / 1024
			var magnitude = (int)((long)info.BaseFrequency * psi / 1024 * info.DecayScalar / 1024);
			if (magnitude <= 0)
				return;

			network.AddContribution(self, originCell, footprint, distSq, magnitude);
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			RevokeCondition(self);

			if (network == null)
				return;

			if (registered)
			{
				network.Unregister(self);
				registered = false;
			}
			else
				network.ClearContribution(self);
		}

		void RevokeCondition(Actor self)
		{
			if (token != Actor.InvalidConditionToken)
				token = self.RevokeCondition(token);
		}
	}
}
