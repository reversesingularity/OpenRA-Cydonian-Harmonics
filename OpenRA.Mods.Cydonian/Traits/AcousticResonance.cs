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
	public enum HarmonicPolarity { Creation, Corruption }

	[Desc("Emits a harmonic field that applies a condition to actors in range (Acoustic Paradigm). " +
		"Creation polarity carries teal-gold sustaining harmonics; Corruption is the same physics detuned to amber. " +
		"Receiving actors must list the condition in an ExternalCondition trait.")]
	public class AcousticResonanceInfo : ProximityExternalConditionInfo
	{
		[Desc("Harmonic frequency of this field in Hz. Fields sharing a frequency do not stack.")]
		public readonly int Frequency = 432;

		[Desc("Creation sustains allies; Corruption detunes and disrupts.")]
		public readonly HarmonicPolarity Polarity = HarmonicPolarity.Creation;

		public override object Create(ActorInitializer init) { return new AcousticResonance(init.Self, this); }
	}

	public class AcousticResonance : ProximityExternalCondition
	{
		public readonly AcousticResonanceInfo ResonanceInfo;

		public AcousticResonance(Actor self, AcousticResonanceInfo info)
			: base(self, info)
		{
			ResonanceInfo = info;
		}
	}
}
