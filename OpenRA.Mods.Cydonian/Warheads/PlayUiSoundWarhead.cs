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
using OpenRA.Mods.Common.Warheads;
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Warheads
{
	[Desc("Play a UI sound to the firing player on impact (e.g. Dermal Mark static interference). " +
		"No-ops when Sound is empty so YAML stubs lint clean without audio assets.")]
	public class PlayUiSoundWarhead : Warhead
	{
		[Desc("Sound played via SoundType.UI to the firer's owner. Leave empty to skip.")]
		public readonly string Sound = null;

		public override void DoImpact(in Target target, WarheadArgs args)
		{
			if (string.IsNullOrEmpty(Sound))
				return;

			var firedBy = args.SourceActor;
			if (firedBy == null || firedBy.IsDead)
				return;

			Game.Sound.PlayToPlayer(SoundType.UI, firedBy.Owner, Sound);
		}
	}
}
