﻿using UnityEngine;
using System.Collections.Generic;

using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(VolumeManager))]
	public class VolumeManagerEditor : Editor
	{
		VolumeManager vm;
		VGlobal vg;
		string[] artPacks;
		List<string> artPacksList;

		private void Awake ()
		{
			vm = (VolumeManager)target;
			vg = VGlobal.GetSetting ();
			artPacksList = ArtPackWindow.GetArtPacks ();
			artPacks = artPacksList.ToArray ();
			UpdateStatus ();
		}

		void OnEnable ()
		{
			Awake ();
		}
		
		float buttonW = 70;
		float lw = 60;

		public override void OnInspectorGUI ()
		{
			EditorGUIUtility.labelWidth = lw;

			DrawDef ();

			EditorGUI.BeginChangeCheck ();
			VolumeEditor.DrawVGlobal();
			if (EditorGUI.EndChangeCheck ()) {
				EditorUtility.SetDirty (vg);
				UpdateStatus ();
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

			for (int i = 0; i < vm.dungeons.Count; i++) {
				Dungeon _d = vm.dungeons [i];
				GUI.color = volColor;
				using (var v = new EditorGUILayout.VerticalScope ("Box")) {
					GUI.color = defColor;
					EditorGUIUtility.labelWidth = 92;
					EditorGUILayout.ObjectField ("VolumeData",_d.volumeData, typeof(VolumeData), true);
					EditorGUIUtility.labelWidth = 80;
					EditorGUILayout.Vector3Field ("Position", _d.position);
					EditorGUILayout.Vector3Field ("Rotation", _d.rotation.eulerAngles);
					string _APName = _d.ArtPack.Replace (PathCollect.artPack + "/", "");
					int _APNameIndex = artPacksList.IndexOf (_APName);
					EditorGUI.BeginChangeCheck ();
					_APNameIndex = EditorGUILayout.Popup("ArtPack",_APNameIndex,artPacks);
					if (EditorGUI.EndChangeCheck ()) {
						_APName = PathCollect.artPack + "/" + artPacks[_APNameIndex];
						_d.ArtPack = _APName;
						vm.dungeons [i] = _d;
					}
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
