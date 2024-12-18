#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace com.squirrelbite.fix_vrc_fingers
{
	public static class FingerMapping
	{
		public static string[] Side = {"Left ", "Right "};
		public static string[] Finger = {"Thumb ", "Index ", "Middle ", "Ring ", "Little "};
		public static string[] Part = {"Proximal", "Intermediate", "Distal"};

		public static string GetFingerMapping(int Side, int Finger, int Part)
		{
			return FingerMapping.Side[Side] + FingerMapping.Finger[Finger] + FingerMapping.Part[Part];
		}
	}

	public static class Fixer
	{
		public static void Fix(GameObject Root, float Factor = 1)
		{
			Transform humanTargetL = null;
			Transform humanTargetR = null;

			try {
				Undo.RegisterFullObjectHierarchyUndo(Root, "Fix VRChat fingers");

				var animator = Root.gameObject.GetComponent<Animator>();
				if(!animator) throw new System.Exception("No Animator!");
				if(!animator.avatar) throw new System.Exception("Animator has no Avatar!");
				if(!animator.avatar.isHuman) throw new System.Exception("Can't fix non humanoid Unity Avatar!");

				var handL = GetHumanBone(animator, "LeftHand");
				var handR = GetHumanBone(animator, "RightHand");

				if(!handL.Success || !handR.Success) throw new System.Exception("Hands are not mapped!");

				for(int i = 0; i < handL.Target.childCount; i++)
					if(handL.Target.GetChild(i).name.EndsWith("_fingers_fixed")) throw new System.Exception("Avatar has already been unfucked!");
				for(int i = 0; i < handR.Target.childCount; i++)
					if(handR.Target.GetChild(i).name.EndsWith("_fingers_fixed")) throw new System.Exception("Avatar has already been unfucked!");

				humanTargetL = Object.Instantiate(handL.Target, handL.Target.transform.parent);
				humanTargetR = Object.Instantiate(handR.Target, handR.Target.transform.parent);

				foreach(var c in handL.Target.GetComponentsInChildren<Component>())
					if(c is not Transform && c.transform != handL.Target) Object.DestroyImmediate(c);
				foreach(var c in handR.Target.GetComponentsInChildren<Component>())
					if(c is not Transform && c.transform != handL.Target) Object.DestroyImmediate(c);

				ConstrainFingers(0, animator, humanTargetL, handL.Target, Factor);
				ConstrainFingers(1, animator, humanTargetR, handR.Target, Factor);

				foreach(var t in handL.Target.GetComponentsInChildren<Transform>()) if(t != handL.Target) t.name += "_fingers_fixed";
				foreach(var t in handR.Target.GetComponentsInChildren<Transform>()) if(t != handR.Target) t.name += "_fingers_fixed";

				for(int childIdx = humanTargetL.childCount - 1; childIdx >= 0; childIdx--)
				{
					var finger = humanTargetL.GetChild(childIdx);
					finger.SetParent(handL.Target);
				}
				for(int childIdx = humanTargetR.childCount - 1; childIdx >= 0; childIdx--)
				{
					var finger = humanTargetR.GetChild(childIdx);
					finger.SetParent(handR.Target);
				}
			}
			finally
			{
				if(humanTargetL) Object.DestroyImmediate(humanTargetL.gameObject);
				if(humanTargetR) Object.DestroyImmediate(humanTargetR.gameObject);
			}
		}

		private static void ConstrainFingers(int Side, Animator Animator, Transform HumanSearchRoot, Transform ShadowSearchRoot, float Factor)
		{
			for(int fingerIdx = 0; fingerIdx < 5; fingerIdx++) for(int partIdx = 0; partIdx < 3; partIdx++)
			{
				var mapping = FingerMapping.GetFingerMapping(Side, fingerIdx, partIdx);
				var (HumanDef, HumanTarget, UnfuckTarget, Success) = GetBoneWithShadowBone(Animator, mapping, HumanSearchRoot, ShadowSearchRoot);
				if(Success)
				{
					float guestimated = Guesstimator.GuesstimateFactor(HumanDef);
					guestimated = guestimated + (1-guestimated) * (1-Factor);
					SetupConstraint(UnfuckTarget, HumanTarget, guestimated);
				}
			}
		}

		private static (HumanBone HumanDef, Transform Target, bool Success) GetHumanBone(Animator Animator, string HumanName)
		{
			try
			{
				var humanDef = Animator.avatar.humanDescription.human.First(e => e.humanName == HumanName);
				var target = Animator.gameObject.GetComponentsInChildren<Transform>().First(e => e.name == humanDef.boneName);
				return (humanDef, target, true);
			}
			catch(System.Exception)
			{
				return (default, null, false);
			}
		}

		private static (HumanBone HumanDef, Transform HumanTarget, Transform UnfuckTarget, bool Success) GetBoneWithShadowBone(Animator Animator, string HumanName, Transform HumanSearchRoot, Transform ShadowSearchRoot)
		{
			try
			{
				var humanDef = Animator.avatar.humanDescription.human.First(e => e.humanName == HumanName);
				var humanTarget = HumanSearchRoot.gameObject.GetComponentsInChildren<Transform>().First(e => e.name == humanDef.boneName);
				var unfuckTarget = ShadowSearchRoot.gameObject.GetComponentsInChildren<Transform>().First(e => e.name == humanDef.boneName);
				return (humanDef, humanTarget, unfuckTarget, true);
			}
			catch(System.Exception)
			{
				return (default, null, null, false);
			}
		}

		public static void SetupConstraint(Transform Target, Transform Source, float Weight)
		{
			var converted = Target.gameObject.AddComponent<VRCRotationConstraint>();

			converted.GlobalWeight = Weight;

			converted.AffectsRotationX = true;
			converted.AffectsRotationY = true;
			converted.AffectsRotationZ = true;

			converted.Sources.Add(new VRC.Dynamics.VRCConstraintSource(Source, 1, Vector3.zero, Vector3.zero));

			converted.RotationOffset = (Quaternion.Inverse(Source.rotation) * converted.transform.rotation).eulerAngles;

			converted.Locked = true;
			converted.IsActive = true;
		}
	}
}

#endif
