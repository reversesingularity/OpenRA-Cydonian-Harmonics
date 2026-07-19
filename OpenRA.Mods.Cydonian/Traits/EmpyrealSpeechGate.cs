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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Traits
{
	[Desc("While enabled (Operational Silence), mutes listed Empyreal Register Speech notifications. " +
		"Attach to the Player actor with RequiresCondition: operational-silence.")]
	[TraitLocation(SystemActors.Player)]
	public class EmpyrealSpeechGateInfo : ConditionalTraitInfo
	{
		[Desc("Speech notification names silenced while the gate is active (e.g. TobitProtocol).")]
		public readonly HashSet<string> MutedNotifications = new()
		{
			"TobitProtocol"
		};

		public override object Create(ActorInitializer init)
		{
			return new EmpyrealSpeechGate(this);
		}
	}

	public class EmpyrealSpeechGate : ConditionalTrait<EmpyrealSpeechGateInfo>
	{
		public EmpyrealSpeechGate(EmpyrealSpeechGateInfo info)
			: base(info) { }

		/// <summary>True when Operational Silence is active and the notification is on the mute list.</summary>
		public bool ShouldMute(string notification)
		{
			if (IsTraitDisabled || string.IsNullOrEmpty(notification))
				return false;

			return Info.MutedNotifications.Contains(notification);
		}

		/// <summary>Play a Speech notification unless this player's EmpyrealSpeechGate mutes it.</summary>
		public static bool TryPlaySpeechNotification(Player player, string notification)
		{
			if (player == null || string.IsNullOrEmpty(notification))
				return false;

			var gate = player.PlayerActor.TraitOrDefault<EmpyrealSpeechGate>();
			if (gate != null && gate.ShouldMute(notification))
				return false;

			return Game.Sound.PlayNotification(player.World.Map.Rules, player, "Speech",
				notification, player.Faction.InternalName);
		}
	}
}
