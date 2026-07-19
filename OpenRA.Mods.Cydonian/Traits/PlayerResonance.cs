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
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Traits
{
	[Desc("Spendable Resonance pool for the Acoustic Paradigm economy. " +
		"Attach to the Player actor. Distinct from AcousticResonance (proximity field emitter).")]
	[TraitLocation(SystemActors.Player)]
	public class PlayerResonanceInfo : TraitInfo
	{
		[Desc("Maximum Resonance that can be stored.")]
		public readonly int Capacity = 1000;

		[Desc("Resonance granted at match start (dev/stub harvest until collectors land).")]
		public readonly int InitialAmount = 500;

		public override object Create(ActorInitializer init)
		{
			return new PlayerResonance(this);
		}
	}

	public class PlayerResonance
	{
		readonly PlayerResonanceInfo info;

		public int Current { get; private set; }
		public int Capacity => info.Capacity;

		public PlayerResonance(PlayerResonanceInfo info)
		{
			this.info = info;
			Current = Math.Clamp(info.InitialAmount, 0, info.Capacity);
		}

		public bool CanTake(int amount)
		{
			return amount <= 0 || Current >= amount;
		}

		/// <summary>Deduct Resonance. Returns false if insufficient (no change).</summary>
		public bool Take(int amount)
		{
			if (amount <= 0)
				return true;

			if (Current < amount)
				return false;

			Current -= amount;
			return true;
		}

		/// <summary>Add Resonance, clamped to Capacity. Returns amount actually added.</summary>
		public int Give(int amount)
		{
			if (amount <= 0)
				return 0;

			var before = Current;
			Current = Math.Min(info.Capacity, Current + amount);
			return Current - before;
		}
	}
}
