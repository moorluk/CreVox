using UnityEngine;
using System.Collections.Generic;

using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(VolumeManager))]
	[ExecuteInEditMode]
	public class VolumeManagerEditor : Editor
	{
		VolumeManager vm;
		VGlobal vg;

		private void Awake ()
		{
			vm = (VolumeManager)target;
			vg = VGlobal.GetSetting ();
			UpdateStatus ();
		}

		void OnEnable ()
		{
			Awake ();
			vm.UpdateDungeon ();
		}
		
		float buttonW = 70;
		float lw = 60;

		public override void OnInspectorGUI ()
		{
			EditorGUIUtility.labelWidth = lw;

			DrawDef ();

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				EditorGUILayout.LabelField ("Global Setting", EditorStyles.boldLabel);
				EditorGUI.BeginChangeCheck ();
				vg.saveBackup = EditorGUILayout.ToggleLeft ("Save Backup File(" + vg.saveBackup + ")", vg.saveBackup);
				vg.FakeDeco = EditorGUILayout.ToggleLeft ("Use Fake Deco(" + vg.FakeDeco + ")", vg.FakeDeco);
				vg.debugRuler = EditorGUILayout.ToggleLeft ("Show Ruler(" + vg.debugRuler + ")", vg.debugRuler);
				if (EditorGUI.EndChangeCheck ()) {
					EditorUtility.SetDirty (vg);
					UpdateStatus ();
				}
			}

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				EditorGUILayout.LabelField ("Decoration Setting", EditorStyles.boldLabel);
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Generate", GUILayout.Width (buttonW))) {
					vm.CreateDeco ();
				}
				if (GUILayout.Button ("Clear", GUILayout.Width (buttonW))) {
					vm.ClearDeco ();
				}
				GUILayout.EndHorizontal ();
			}

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				GUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Volume List", EditorStyles.boldLabel);
				if (GUILayout.Button ("Update", GUILayout.Width (buttonW)))
					vm.UpdateDungeon ();
				GUILayout.EndHorizontal ();

				EditorGUIUtility.wideMode = true;
				DrawVolumeList ();
			}

			if (GUI.changed)
				UpdateStatus ();
		}
		void DrawVolumeList()
		{
			Color defColor = GUI.color;
			Color volColor = new Color (0.5f, 0.8f, 0.75f);

			for (int i = 0; i < vm.dungeons.Length; i++) {

				GUI.color = volColor;
				using (var v = new EditorGUILayout.VerticalScope ("Box")) {
					GUI.color = defColor;
					EditorGUIUtility.labelWidth = 92;
					EditorGUILayout.ObjectField ("VolumeData",vm.dungeons [i].volumeData, typeof(VolumeData), true);
					EditorGUIUtility.labelWidth = 80;
					EditorGUILayout.Vector3Field ("Position", vm.dungeons [i].position);
					EditorGUILayout.Vector3Field ("Rotation", vm.dungeons [i].rotation.eulerAngles);
				}
			}
		}

		#region Inspector Function

		bool drawDef;

		void DrawDef ()
		{
			drawDef = EditorGUILayout.ToggleLeft ("Draw Default Inspector", drawDef, EditorStyles.miniLabel);
			if (drawDef)
				DrawDefaultInspector ();
		}

		void UpdateStatus ()
		{
			if (!UnityEditor.EditorApplication.isPlaying) {
				Debug.Log ("Show Debug Ruler : " + vg.debugRuler);
				vm.BroadcastMessage ("ShowRuler", SendMessageOptions.DontRequireReceiver);
			}
		}

		#endregion
	}
}
