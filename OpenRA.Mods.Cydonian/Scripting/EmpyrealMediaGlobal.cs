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

using OpenRA.Mods.Cydonian.Traits;
using OpenRA.Scripting;

namespace OpenRA.Mods.Cydonian.Scripting
{
	[ScriptGlobal("EmpyrealMedia")]
	public class EmpyrealMediaGlobal : ScriptGlobal
	{
		public EmpyrealMediaGlobal(ScriptContext context)
			: base(context) { }

		[Desc("Play a Speech notification unless Operational Silence mutes the Empyreal Register name.")]
		public void PlaySpeechNotification(Player player, string notification)
		{
			EmpyrealSpeechGate.TryPlaySpeechNotification(player, notification);
		}
	}
}
