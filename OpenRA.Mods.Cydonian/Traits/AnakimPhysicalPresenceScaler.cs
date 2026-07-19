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
	[Desc("Scales the physical presence of Anakim-class Nephilim hybrids. " +
		"Presence is biological mass (Oiketerion Principle), not a miracle: incoming damage " +
		"is soaked proportionally to the presence factor.")]
	public class AnakimPhysicalPresenceScalerInfo : TraitInfo
	{
		[Desc("Presence multiplier in percent. 100 = baseline human frame; 300 = triple-mass Anakim.")]
		public readonly int VisualScaleFactor = 300;

		[Desc("Collision bounds of the giant frame in world units (1024 = 1 cell).")]
		public readonly WDist CustomCollisionRadius = new(768);

		public override object Create(ActorInitializer init) { return new AnakimPhysicalPresenceScaler(this); }
	}

	public class AnakimPhysicalPresenceScaler : IDamageModifier
	{
		readonly AnakimPhysicalPresenceScalerInfo info;

		public AnakimPhysicalPresenceScaler(AnakimPhysicalPresenceScalerInfo info)
		{
			this.info = info;
		}

		public WDist CollisionRadius => info.CustomCollisionRadius;

		// A 300% presence frame resolves incoming damage at 33%.
		int IDamageModifier.GetDamageModifier(Actor attacker, Damage damage)
		{
			return 10000 / info.VisualScaleFactor;
		}
	}
}
