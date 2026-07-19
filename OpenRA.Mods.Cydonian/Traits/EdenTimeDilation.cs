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
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Traits
{
	[Desc("Narnia-style Eden time dilation: while enabled, amplifies Resonance accrual. " +
		"Attach to the Player actor with RequiresCondition: eden-dilated. Tunables in YAML only.")]
	[TraitLocation(SystemActors.Player)]
	public class EdenTimeDilationInfo : ConditionalTraitInfo
	{
		[Desc("Resonance granted each Interval while dilation is active.")]
		public readonly int ResonancePerInterval = 25;

		[Desc("Ticks between Resonance grants while dilation is active.")]
		public readonly int Interval = 25;

		public override object Create(ActorInitializer init)
		{
			return new EdenTimeDilation(this);
		}
	}

	public class EdenTimeDilation : ConditionalTrait<EdenTimeDilationInfo>, ITick
	{
		PlayerResonance pool;

		[Sync]
		int ticks;

		public EdenTimeDilation(EdenTimeDilationInfo info)
			: base(info) { }

		protected override void Created(Actor self)
		{
			pool = self.TraitOrDefault<PlayerResonance>();
			ticks = Info.Interval;
			base.Created(self);
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitDisabled || pool == null || Info.Interval <= 0 || Info.ResonancePerInterval <= 0)
			{
				ticks = Info.Interval;
				return;
			}

			if (--ticks > 0)
				return;

			ticks = Info.Interval;
			pool.Give(Info.ResonancePerInterval);
		}
	}
}
