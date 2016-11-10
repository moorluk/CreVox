using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(VolumeManager))]
	public class VolumeManagerEditor : Editor
	{
		VolumeManager vm;
		SerializedProperty p_saveBackupFile;
		SerializedProperty p_showDebugRuler;
		bool updateDungeon = true;

		private void OnEnable ()
		{
			vm = (VolumeManager)target;
			p_saveBackupFile = serializedObject.FindProperty ("saveBackupFile");
			p_showDebugRuler = serializedObject.FindProperty ("showDebugRuler");
		}

		public override void OnInspectorGUI ()
		{
			float lw = 60;
			float w = (Screen.width - 20 - lw) / 3 - 8;
			float buttonW = 60;
//			EditorGUIUtility.labelWidth = 20;

			serializedObject.Update ();

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20))) {
				EditorGUILayout.LabelField ("Global Setting", EditorStyles.boldLabel);
				vm.saveBackupFile = EditorGUILayout.Toggle ("Save Backup File", vm.saveBackupFile);
				vm.showDebugRuler = EditorGUILayout.Toggle ("Show Ruler", vm.showDebugRuler);
			}

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20))) {
				GUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Dungeon Setting", EditorStyles.boldLabel);
				if (GUILayout.Button ("Update")) {
					vm.UpdateDungeon ();
				}
				GUILayout.EndHorizontal ();
				for (int i = 0; i < vm.dungeons.Length; i++) {
					Volume vol = vm.dungeons [i].volume;
					using (var v2 = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
						EditorGUILayout.ObjectField (vol,typeof(Volume),true);

						GUILayout.BeginHorizontal ();
//						EditorGUILayout.LabelField ("ArtPack", EditorStyles.boldLabel);
						if (GUILayout.Button ("ArtPack", GUILayout.Width (buttonW))) {
							string ppath = EditorUtility.OpenFolderPanel (
								"選擇場景風格元件包的目錄位置",
								vm.dungeons [i].artPack,
								""
							);
							if (VolumeGlobal.saveBackup)
								vol.SaveTempWorld ();

							ppath = ppath.Substring (ppath.IndexOf (PathCollect.resourcesPath));
							vol.vertexMaterial = FindMaterial (ppath);

							ppath = ppath.Substring (ppath.IndexOf (PathCollect.resourceSubPath));
							vol.piecePack = ppath;
							vm.dungeons [i].artPack = ppath;

							vol.LoadTempWorld ();
						}
						EditorGUILayout.LabelField (vol.piecePack.Substring(vol.piecePack.LastIndexOf("/")+1), EditorStyles.miniLabel);
						GUILayout.EndHorizontal ();
					}
				}
			}

			serializedObject.ApplyModifiedPropertiesWithoutUndo ();

			if (VolumeGlobal.saveBackup != vm.saveBackupFile) {
				VolumeGlobal.saveBackup = vm.saveBackupFile;
			}
			if (VolumeGlobal.debugRuler != vm.showDebugRuler && !UnityEditor.EditorApplication.isPlaying) {
				VolumeGlobal.debugRuler = vm.showDebugRuler;
				Debug.LogWarning ("Show Debug Ruler : " + VolumeGlobal.debugRuler);
//				vm.BroadcastShowRuler();
				vm.BroadcastMessage ("ShowRuler", SendMessageOptions.DontRequireReceiver);
			}
			DrawDefaultInspector ();
		}

		Material FindMaterial(string _path)
		{
			string[] mats = AssetDatabase.FindAssets ("voxel t:Material", new string[]{ _path });
			if (mats.Length == 1) {
				string matPath = AssetDatabase.GUIDToAssetPath (mats [0]);
				return AssetDatabase.LoadAssetAtPath<Material> (matPath);
			} else
				return null;

		}
	}
}
