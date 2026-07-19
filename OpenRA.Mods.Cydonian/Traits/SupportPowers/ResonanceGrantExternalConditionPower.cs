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

using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.Cydonian.Traits
{
	[Desc("Resonance-gated GrantExternalConditionPower (e.g. Sustain Harmony). " +
		"Locks a Creation Frequency over a footprint, granting a timed ExternalCondition to allies. " +
		"Deducts from the Resonance pool before charge is consumed.")]
	public class ResonanceGrantExternalConditionPowerInfo : GrantExternalConditionPowerInfo
	{
		[FieldLoader.Require]
		[Desc("Resonance deducted from PlayerResonance on successful activation.")]
		public readonly int ResonanceCost = 200;

		public override object Create(ActorInitializer init)
		{
			return new ResonanceGrantExternalConditionPower(init.Self, this);
		}
	}

	public class ResonanceGrantExternalConditionPower : GrantExternalConditionPower, IResonancePricedSupportPower
	{
		readonly ResonanceGrantExternalConditionPowerInfo info;
		PlayerResonance pool;

		public ResonanceGrantExternalConditionPower(Actor self, ResonanceGrantExternalConditionPowerInfo info)
			: base(self, info)
		{
			this.info = info;
		}

		protected override void Created(Actor self)
		{
			// Must call base so ConditionalTrait / PausableConditionalTrait init runs.
			pool = self.Owner.PlayerActor.TraitOrDefault<PlayerResonance>();
			base.Created(self);
		}

		public override SupportPowerInstance CreateInstance(string key, SupportPowerManager manager)
		{
			return new ResonanceGatedSupportPowerInstance(key, info, manager);
		}

		/// <summary>Attempt Resonance spend. False leaves the support power charged.</summary>
		public bool TryConsumeCost(Actor self)
		{
			return ResonancePowerActivation.TryConsume(self, pool, Info, info.ResonanceCost);
		}
	}
}
