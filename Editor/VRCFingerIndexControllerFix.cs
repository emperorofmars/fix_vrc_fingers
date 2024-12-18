// Adapted from: https://github.com/d4rkc0d3r/d4rkAvatarOptimizer/blob/main/Editor/AvatarBuildHook.cs

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;

namespace com.squirrelbite.fix_vrc_fingers
{
	[HelpURL("https://github.com/emperorofmars/fix_vrc_fingers")]
	public class VRCFingerIndexControllerFix : MonoBehaviour, IEditorOnly
	{
		public bool Enabled = true;
		public float Factor = 1;
	}

	[InitializeOnLoad]
	public class AvatarBuildHook : IVRCSDKPreprocessAvatarCallback
	{
		// Modular Avatar is at -25, we want to be after that. However usually vrcsdk removes IEditorOnly at -1024.
		// MA patches that to happen last so we can only be at -15 if MA is installed otherwise our component will be removed before getting run.
		#if MODULAR_AVATAR_EXISTS
		public int callbackOrder => -15;
		#else
		public int callbackOrder => -1025;
		#endif

		public bool OnPreprocessAvatar(GameObject Root)
		{
			var fixComponent = Root.GetComponent<VRCFingerIndexControllerFix>();
			if(!fixComponent || !fixComponent.Enabled) return true;
			try
			{
				Fixer.Fix(Root, fixComponent.Factor);
				return true;
			}
			catch (Exception exception)
			{
				Debug.LogError(exception);
				return false;
			}
		}
	}
}

#endif
