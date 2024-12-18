#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.squirrelbite.fix_vrc_fingers
{
	public static class Guesstimator
	{
		public static Dictionary<string, (float Min, float Max)> LimitsVRCBuggedGuess = new() {
			{"Thumb Proximal", (-30, 30)},
			{"Thumb Intermediate", (-45, 40)},
			{"Thumb Distal", (-60, 55)},
			{"Index Proximal", (-90, 90)},
			{"Index Intermediate", (-90, 90)},
			{"Index Distal", (-90, 90)},
			{"Middle Proximal", (-90, 90)},
			{"Middle Intermediate", (-90, 90)},
			{"Middle Distal", (-90, 90)},
			{"Ring Proximal", (-90, 90)},
			{"Ring Intermediate", (-90, 90)},
			{"Ring Distal", (-90, 90)},
			{"Little Proximal", (-90, 90)},
			{"Little Intermediate", (-90, 90)},
			{"Little Distal", (-90, 90)},
		};
		public static Dictionary<string, (float Min, float Max)> LimitsUnityDefault = new() {
			{"Thumb Proximal", (-20, 20)},
			{"Thumb Intermediate", (-40, 35)},
			{"Thumb Distal", (-40, 35)},
			{"Index Proximal", (-50, 50)},
			{"Index Intermediate", (-45, 45)},
			{"Index Distal", (-45, 45)},
			{"Middle Proximal", (-50, 50)},
			{"Middle Intermediate", (-45, 45)},
			{"Middle Distal", (-45, 45)},
			{"Ring Proximal", (-50, 50)},
			{"Ring Intermediate", (-45, 45)},
			{"Ring Distal", (-45, 45)},
			{"Little Proximal", (-50, 50)},
			{"Little Intermediate", (-45, 45)},
			{"Little Distal", (-45, 45)},
		};

		public static float GuesstimateFactor(HumanBone Bone)
		{
			var limits = Bone.limit.useDefaultValues ? LimitsUnityDefault[Bone.humanName[(Bone.humanName.IndexOf(" ") + 1)..]] : (Bone.limit.min.z, Bone.limit.max.z);
			var limitsVRC = LimitsVRCBuggedGuess[Bone.humanName[(Bone.humanName.IndexOf(" ") + 1)..]];

			if(limits.Item1 == 0) limits.Item1 = 0.001f;
			if(limits.Item2 == 0) limits.Item2 = 0.001f;

			var diffMin = Math.Min(limits.Item1 / limitsVRC.Min, 10);
			var diffMax = Math.Min(limits.Item2 / limitsVRC.Max, 10);

			return  (diffMin + diffMax) / 2;
		}
	}
}

#endif
