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
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Traits
{
	/// <summary>Support powers that deduct PlayerResonance before charge is consumed.</summary>
	public interface IResonancePricedSupportPower
	{
		bool TryConsumeCost(Actor self);
	}

	/// <summary>Shared Resonance spend + nearest-instance selection for gated support powers.</summary>
	public static class ResonancePowerActivation
	{
		public static bool TryConsume(Actor self, PlayerResonance pool, SupportPowerInfo info, int resonanceCost)
		{
			if (pool == null)
			{
				Game.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech",
					info.InsufficientPowerSpeechNotification, self.Owner.Faction.InternalName);
				TextNotificationsManager.AddTransientLine(self.Owner, info.InsufficientPowerTextNotification);
				return false;
			}

			if (!pool.Take(resonanceCost))
			{
				Game.Sound.PlayToPlayer(SoundType.UI, self.Owner, info.InsufficientPowerSound);
				Game.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech",
					info.InsufficientPowerSpeechNotification, self.Owner.Faction.InternalName);
				TextNotificationsManager.AddTransientLine(self.Owner, info.InsufficientPowerTextNotification);
				return false;
			}

			return true;
		}

		public static SupportPower SelectNearestInstance(IEnumerable<SupportPower> instances, Order order)
		{
			return instances.Where(i => !i.IsTraitPaused && !i.IsTraitDisabled)
				.MinByOrDefault(a =>
				{
					if (a.Self.OccupiesSpace == null || order.Target.Type == TargetType.Invalid)
						return 0;

					return (a.Self.CenterPosition - order.Target.CenterPosition).HorizontalLengthSquared;
				});
		}
	}

	/// <summary>
	/// SupportPowerInstance that deducts Resonance before <see cref="SupportPowerInstance.Activate"/>
	/// consumes charge. Fail-closed: insufficient Resonance leaves the power Ready.
	/// </summary>
	public sealed class ResonanceGatedSupportPowerInstance : SupportPowerInstance
	{
		public ResonanceGatedSupportPowerInstance(string key, SupportPowerInfo info, SupportPowerManager manager)
			: base(key, info, manager) { }

		public override void Activate(Order order)
		{
			if (!Ready)
				return;

			var power = ResonancePowerActivation.SelectNearestInstance(Instances, order);
			if (power is not IResonancePricedSupportPower priced)
				return;

			if (!priced.TryConsumeCost(power.Self))
				return;

			base.Activate(order);
		}
	}
}
