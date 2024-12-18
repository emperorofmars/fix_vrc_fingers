#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace com.squirrelbite.fix_vrc_fingers
{
	public class FixEditorWindow : EditorWindow
	{
		private VRC_AvatarDescriptor Selected;
		private float Factor = 1;
		private string ErrorMessage = null;
		private bool Success = false;

		[MenuItem("Tools/Fix VRC Fingers (For Valve Index Controllers)")]
		public static void Init()
		{
			FixEditorWindow window = GetWindow(typeof(FixEditorWindow)) as FixEditorWindow;
			window.titleContent = new GUIContent("Fix VRC Fingers (For Valve Index Controllers)");
			window.minSize = new Vector2(500, 700);
			window.Selected = null;
		}

		void OnGUI()
		{
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Select Object", EditorStyles.whiteLargeLabel, GUILayout.ExpandWidth(false));
			var newSelected = (VRC_AvatarDescriptor)EditorGUILayout.ObjectField(
				Selected,
				typeof(VRC_AvatarDescriptor),
				true,
				GUILayout.ExpandWidth(true)
			);
			GUILayout.EndHorizontal();

			if(newSelected != Selected)
			{
				Selected = newSelected;
				ErrorMessage = null;
				Success = false;
			}

			if(Success)
			{
				GUILayout.Space(5);
				GUILayout.Label("Success!", EditorStyles.label, GUILayout.ExpandWidth(false));
			}
			if(!string.IsNullOrWhiteSpace(ErrorMessage))
			{
				GUILayout.Space(5);
				GUILayout.Label("Error: " + ErrorMessage, EditorStyles.label, GUILayout.ExpandWidth(false));
			}

			GUILayout.Space(15);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Approximate Legacy Fingers", GUILayout.ExpandWidth(false))) Factor = 0.45f;
			if(GUILayout.Button("Full", GUILayout.ExpandWidth(false))) Factor = 1.0f;
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Factor", GUILayout.ExpandWidth(false));
			Factor = GUILayout.HorizontalSlider(Factor, 0.1f, 1.0f);
			GUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUILayout.Label(Factor.ToString());
			GUILayout.Space(10);

			if(Selected)
			{
				if(GUILayout.Button("Fix"))
				{
					ErrorMessage = null;
					Success = false;
					try
					{
						var instance = UnityEngine.Object.Instantiate(Selected);
						instance.name = Selected.name + "_fingers_fixed";
						Fixer.Fix(instance.gameObject, Factor);
						Success = true;
					}
					catch(System.Exception exception)
					{
						Debug.LogException(exception);
						ErrorMessage = exception.Message;
					}
				}
			}
		}
	}
}

#endif
