using UnityEngine;

//using System;
//using System.Collections;
//using System.Collections.Generic;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(VolumeManager))]
	[ExecuteInEditMode]
	public class VolumeManagerEditor : Editor
	{
		VolumeManager vm;
		SerializedProperty p_saveBackupFile;
		SerializedProperty p_showDebugRuler;

		private void Awake ()
		{
			vm = (VolumeManager)target;
			p_saveBackupFile = serializedObject.FindProperty ("saveBackupFile");
			p_showDebugRuler = serializedObject.FindProperty ("showDebugRuler");
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
			
			serializedObject.Update ();
			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20))) {
				EditorGUILayout.LabelField ("Global Setting", EditorStyles.boldLabel);
				p_saveBackupFile.boolValue = EditorGUILayout.ToggleLeft ("Save Backup File(" + VolumeGlobal.saveBackup + ")", p_saveBackupFile.boolValue);
				p_showDebugRuler.boolValue = EditorGUILayout.ToggleLeft ("Show Ruler", p_showDebugRuler.boolValue);
			}
			serializedObject.ApplyModifiedProperties ();

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20))) {
				EditorGUILayout.LabelField ("Dungeon Setting", EditorStyles.boldLabel);
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Generate", GUILayout.Width (buttonW))) {
					vm.CreateDeco ();
				}
				if (GUILayout.Button ("Clear", GUILayout.Width (buttonW))) {
					vm.ClearDeco ();
				}
				GUILayout.EndHorizontal ();
			}

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20))) {
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
				Volume vol = vm.dungeons [i].volume;
				VolumeData vData = Resources.Load (vol.workFile + ".asset") as VolumeData;

				GUI.color = volColor;
				using (var v2 = new EditorGUILayout.VerticalScope ("Box")) {
					GUI.color = defColor;
					EditorGUILayout.ObjectField (vol, typeof(Volume), true);
					GUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Data", GUILayout.Width (buttonW))) {
						if (Event.current.shift) {
							if (vol.workFile != "") {
								string lPath = 
									Application.dataPath
									+ PathCollect.resourcesPath.Substring (6)
									+ vol.workFile + ".bytes";
								Save save = Serialization.LoadWorld (lPath);
								if (save != null) {
									vol.BuildVolume (save);
									vol.tempPath = "";
								}
								SceneView.RepaintAll ();
							}
						} else {
							string lPath = Serialization.GetLoadLocation (vol.workFile == "" ? null : vol.workFile);
							if (lPath != "") {
								Save save = Serialization.LoadWorld (lPath);
								if (save != null) {
									vol.BuildVolume (save);
									vol.workFile = lPath.Remove (lPath.LastIndexOf (".")).Substring (lPath.IndexOf (PathCollect.resourceSubPath));
									vol.tempPath = "";
								}
								SceneView.RepaintAll ();
							}
						}
					}
					string sfPath = vol.workFile.Substring (vol.workFile.LastIndexOf ("VolumeData/") + 10);
					EditorGUILayout.LabelField (sfPath);
					GUILayout.EndHorizontal ();

					EditorGUILayout.Vector3Field ("Position", vm.dungeons [i].position);
					EditorGUILayout.Vector3Field ("Rotation", vm.dungeons [i].rotation.eulerAngles);

					if (vm.dungeons [i].artPack == null || vm.dungeons [i].artPack.Length < 1) {
						vm.dungeons [i].artPack = PathCollect.resourcesPath + PathCollect.pieces;
					}
					string ppath = vm.dungeons [i].artPack;

					GUILayout.BeginHorizontal ();
					if (GUILayout.Button ("ArtPack", GUILayout.Width (buttonW))) {
						ppath = EditorUtility.OpenFolderPanel (
							"選擇場景風格元件包的目錄位置", 
							Application.dataPath + "/Resources/" + PathCollect.resourceSubPath + "/VolumeArtPack", 
							""
						);
						if (ppath.Contains (PathCollect.resourcesPath))
							ppath = ppath.Substring (ppath.IndexOf (PathCollect.resourcesPath));
					}

					vm.dungeons [i].artPack = ppath.Substring (ppath.IndexOf (PathCollect.resourceSubPath));
					EditorGUILayout.LabelField (vm.dungeons [i].artPack.Substring (vm.dungeons [i].artPack.LastIndexOf ("/")));
					GUILayout.EndHorizontal ();

					vm.dungeons [i].vertexMaterial = (Material)EditorGUILayout.ObjectField (vm.FindMaterial (ppath), typeof(Material), false);
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
			if (VolumeGlobal.saveBackup != vm.saveBackupFile) {
				VolumeGlobal.saveBackup = vm.saveBackupFile;
			}
			if (VolumeGlobal.debugRuler != vm.showDebugRuler && !UnityEditor.EditorApplication.isPlaying) {
				VolumeGlobal.debugRuler = vm.showDebugRuler;
				Debug.LogWarning ("Show Debug Ruler : " + VolumeGlobal.debugRuler);
				vm.BroadcastMessage ("ShowRuler", SendMessageOptions.DontRequireReceiver);
			}
		}

		#endregion
	}
}
