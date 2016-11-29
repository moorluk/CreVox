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
				EditorGUI.BeginChangeCheck ();
				vg.debugRuler = EditorGUILayout.ToggleLeft ("Show Ruler(" + vg.debugRuler + ")", vg.debugRuler);
				if (EditorGUI.EndChangeCheck ())
					UpdateStatus ();
				if (EditorGUI.EndChangeCheck ())
					EditorUtility.SetDirty (vg);
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
				Volume vol = vm.dungeons [i].volume;
//				VolumeData vData = Resources.Load (vol.workFile + ".asset") as VolumeData;

				GUI.color = volColor;
				using (var v = new EditorGUILayout.VerticalScope ("Box")) {
					GUI.color = defColor;

					GUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("GameObj", GUILayout.Width (buttonW));
					EditorGUILayout.ObjectField (vol, typeof(Volume), true);
					GUILayout.EndHorizontal ();

					EditorGUILayout.Vector3Field ("Position", vm.dungeons [i].position);
					EditorGUILayout.Vector3Field ("Rotation", vm.dungeons [i].rotation.eulerAngles);

					using (var v2 = new EditorGUILayout.VerticalScope ("Box")) {
						int sfIndex = vol.workFile.LastIndexOf ("VolumeData/");
						string sfPath = (sfIndex > 0) ? vol.workFile.Substring (sfIndex + 10) : "Empty!!!";
						EditorGUILayout.LabelField (sfPath, EditorStyles.miniLabel);
						GUILayout.BeginHorizontal ();
						if (GUILayout.Button ("Load.byte")) {
							if (Event.current.shift) {
								if (vol.workFile != "") {
									string lPath = 
										Application.dataPath
										+ PathCollect.resourcesPath.Substring (6)
										+ vol.workFile + ".bytes";
									Save save = Serialization.LoadWorld (lPath);
									if (save != null) {
										vol._useBytes = true;
										vol.BuildVolume (save,vol.vd);
										vol.tempPath = "";
									}
								}
							} else {
								string lPath = Serialization.GetLoadLocation (vol.workFile == "" ? null : vol.workFile);
								if (lPath != "") {
									Save save = Serialization.LoadWorld (lPath);
									if (save != null) {
										vol._useBytes = true;
										vol.BuildVolume (save,vol.vd);
										vol.workFile = lPath.Remove (lPath.LastIndexOf (".")).Substring (lPath.IndexOf (PathCollect.resourceSubPath));
										vol.tempPath = "";
									}
								}
							}
							SceneView.RepaintAll ();
						}
						if (GUILayout.Button ("byte2VData")) {
							vol.WriteVData ();
						}
						GUILayout.EndHorizontal ();
					}

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

					vm.dungeons [i].volume.piecePack = vm.dungeons [i].artPack;
					vm.dungeons [i].volume.vertexMaterial = vm.dungeons [i].vertexMaterial;
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
