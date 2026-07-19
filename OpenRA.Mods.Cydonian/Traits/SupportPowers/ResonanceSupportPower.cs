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

using OpenRA.GameRules;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Traits
{
	[Desc("Support power that expends PlayerResonance to fire a weapon (e.g. Shatter Protocol). " +
		"Deducts from the Resonance pool before charge is consumed.")]
	public class ResonanceSupportPowerInfo : SupportPowerInfo
	{
		[FieldLoader.Require]
		[Desc("Resonance deducted from PlayerResonance on successful activation.")]
		public readonly int ResonanceCost = 250;

		[WeaponReference]
		[FieldLoader.Require]
		[Desc("Weapon fired at the target cell (warheads handle ward strip + structural damage).")]
		public readonly string Weapon = null;

		[Desc("World sound played at the target on launch (deep resonant drone).")]
		public readonly string OnFireSound = null;

		public WeaponInfo WeaponInfo { get; private set; }

		public override object Create(ActorInitializer init)
		{
			return new ResonanceSupportPower(init.Self, this);
		}

		public override void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			var weaponToLower = (Weapon ?? string.Empty).ToLowerInvariant();
			if (!rules.Weapons.TryGetValue(weaponToLower, out var weapon))
				throw new YamlException($"Weapons Ruleset does not contain an entry '{weaponToLower}'");

			WeaponInfo = weapon;
			base.RulesetLoaded(rules, ai);
		}
	}

	public class ResonanceSupportPower : SupportPower, IResonancePricedSupportPower
	{
		readonly ResonanceSupportPowerInfo info;
		PlayerResonance pool;

		public ResonanceSupportPower(Actor self, ResonanceSupportPowerInfo info)
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

		public override void Activate(Actor self, Order order, SupportPowerManager manager)
		{
			base.Activate(self, order, manager);

			PlayLaunchSounds();

			if (!string.IsNullOrEmpty(info.OnFireSound))
				Game.Sound.Play(SoundType.World, info.OnFireSound, order.Target.CenterPosition);

			if (order.Target.Type == TargetType.Invalid)
				return;

			info.WeaponInfo.Impact(order.Target, self);
		}
	}
}
