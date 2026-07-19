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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using OpenRA.GameRules;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Traits;

namespace OpenRA.Mods.Cydonian.Warheads
{
	[Desc("Revoke all ExternalCondition grants matching Condition within Range. " +
		"Used by Shatter Protocol to strip Oathbound acoustic wards.")]
	public class RevokeExternalConditionWarhead : Warhead
	{
		static readonly FieldInfo PermanentTokensField;
		static readonly FieldInfo TimedTokensField;

		static RevokeExternalConditionWarhead()
		{
			PermanentTokensField = typeof(ExternalCondition).GetField("permanentTokens",
				BindingFlags.Instance | BindingFlags.NonPublic);
			TimedTokensField = typeof(ExternalCondition).GetField("timedTokens",
				BindingFlags.Instance | BindingFlags.NonPublic);

			if (PermanentTokensField == null || TimedTokensField == null)
				throw new InvalidOperationException(
					"RevokeExternalConditionWarhead: ExternalCondition private token fields missing — engine API changed.");
		}

		[FieldLoader.Require]
		[Desc("The condition to strip. Must match an ExternalCondition on the target.")]
		public readonly string Condition = null;

		public readonly WDist Range = WDist.FromCells(2);

		public override void DoImpact(in Target target, WarheadArgs args)
		{
			var firedBy = args.SourceActor;
			if (target.Type == TargetType.Invalid || firedBy == null)
				return;

			var actors = target.Type == TargetType.Actor ? new[] { target.Actor } :
				firedBy.World.FindActorsInCircle(target.CenterPosition, Range);

			foreach (var a in actors)
			{
				if (!IsValidAgainst(a, firedBy))
					continue;

				foreach (var external in a.TraitsImplementing<ExternalCondition>())
				{
					if (external.Info.Condition != Condition)
						continue;

					RevokeAll(external, a);
				}
			}
		}

		static void RevokeAll(ExternalCondition external, Actor self)
		{
			// ExternalCondition tracks tokens privately; revoke via public TryRevokeCondition
			// after enumerating the private maps (no engine fork required).
			if (PermanentTokensField.GetValue(external) is IDictionary permanent)
			{
				var pairs = new List<(object Source, int Token)>();
				foreach (DictionaryEntry entry in permanent)
				{
					if (entry.Value is not IEnumerable tokens)
						continue;

					foreach (var tokenObj in tokens)
						if (tokenObj is int token)
							pairs.Add((entry.Key, token));
				}

				foreach (var (source, token) in pairs)
					external.TryRevokeCondition(self, source, token);
			}

			if (TimedTokensField.GetValue(external) is IEnumerable timed)
			{
				var pairs = new List<(object Source, int Token)>();
				foreach (var item in timed)
				{
					if (item == null)
						continue;

					var type = item.GetType();
					var tokenField = type.GetField("Token");
					var sourceField = type.GetField("Source");
					if (tokenField == null || sourceField == null)
						continue;

					pairs.Add((sourceField.GetValue(item), (int)tokenField.GetValue(item)));
				}

				foreach (var (source, token) in pairs)
					external.TryRevokeCondition(self, source, token);
			}
		}
	}
}
