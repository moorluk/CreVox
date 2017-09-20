using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(VolumeManager))]
	public class VolumeManagerEditor : Editor
	{
		VolumeManager vm;
		VGlobal vg;
        int APIndex = 0;
        string[] artPacks;
        List<string> artPacksList { get{return VGlobal.GetArtPacks();}}

		void OnEnable ()
		{
			vm = (VolumeManager)target;
			vg = VGlobal.GetSetting ();
			ArtPackWindow.UpdateItemArrays (vg);
			artPacks = artPacksList.ToArray ();
			UpdateStatus ();
		}
		
		float buttonW = 50;
		float lw = 60;

		public override void OnInspectorGUI ()
        {
            EditorGUIUtility.labelWidth = lw;

            EditorGUI.BeginChangeCheck ();
            vm.useLocalSetting = EditorGUILayout.ToggleLeft ("Use Local Setting", vm.useLocalSetting);
            if (vm.useLocalSetting) {
                DrawVLocal (vm);
            } else {
                DrawVGlobal ();
            }
            if (EditorGUI.EndChangeCheck ()) {
                Volume[] vs = vm.GetComponentsInChildren<Volume> ();
                foreach (Volume v in vs) {
                    v.vm = vm.useLocalSetting ? vm : null;
                }
                vm.BroadcastMessage ("BuildVolume");
            }

            EditorGUIUtility.wideMode = true;
            using (var v = new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.LabelField ("Volume SetAll Function", EditorStyles.boldLabel);
                DrawDungeonList ();
                using (var h = new EditorGUILayout.HorizontalScope()) {
                    if (GUILayout.Button("Refresh all"))
                        vm.BroadcastMessage("BuildVolume");
                    if (GUILayout.Button ("Generate")) {
                        vm.ClearVolumes (false);
                        vm.CreateVolumes ();
                    }
                    if (GUILayout.Button("Update Portal"))
                        VolumeAdapter.UpdatePortals(vm.gameObject);
                }
                using (var h = new EditorGUILayout.HorizontalScope()) {
                    APIndex = EditorGUILayout.Popup("ArtPack", APIndex, artPacks);
                    if (GUILayout.Button("Set", GUILayout.Width (buttonW))){
                        Volume[] volumes = vm.transform.GetComponentsInChildren<Volume>(includeInactive: false);
                        foreach (Volume _v in volumes){
                            _v.ArtPack = PathCollect.artPack + artPacks[APIndex];
                            _v.vMaterial = _v.ArtPack + "/" + artPacks[APIndex] + "_voxel";
                            string ppath = PathCollect.resourcesPath + _v.vMaterial + ".mat";
                            _v.vertexMaterial = AssetDatabase.LoadAssetAtPath<Material> (ppath);
                            EditorUtility.SetDirty (_v.vd);
                        }
                        vm.BroadcastMessage("BuildVolume");
                        vm.UpdateDungeon ();
                    }
                }
            }

            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    EditorGUILayout.LabelField ("Volume List (" + vm.dungeons.Count + ")", EditorStyles.boldLabel);
                    if (GUILayout.Button ("Update", GUILayout.Width (buttonW)))
                        vm.UpdateDungeon ();
                    if (GUILayout.Button ("Clear", GUILayout.Width (buttonW)))
                        vm.dungeons.Clear ();
                }
                DrawVolumeList ();
            }

            DrawDef ();

            if (GUI.changed)
                UpdateStatus ();
        }

        private void DrawDungeonList()
        {
            int workIndex = -1;
            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    vm.useStageData = EditorGUILayout.ToggleLeft ("Use StageData", vm.useStageData);
                    if (!vm.useStageData)
                        return;
                    if (GUILayout.Button ("TEST")) {
                        vm.RandomDungeon ();
                    }
                }
                vm.stageData = EditorGUILayout.ObjectField (vm.stageData, typeof(StageData), false) as StageData;
                if (vm.stageData == null)
                    return;
                for (int i = 0; i < vm.stageData.stageList.Count; i++) {
                    using (var h = new EditorGUILayout.HorizontalScope ()) {
                        EditorGUILayout.LabelField (i.ToString () + " (" + vm.stageData.stageList [i].Dlist.Count + ")", GUILayout.Width (45));
                        if (GUILayout.Button ("Save")) {
                            vm.stageData.stageList [i].Dlist.Clear ();
                            foreach (var d in vm.dungeons) {
                                vm.stageData.stageList [i].Dlist.Add (d);
                            }
                        }
                        if (GUILayout.Button ("Load")) {
                            vm.dungeons.Clear ();
                            foreach (Dungeon d in vm.stageData.stageList[i].Dlist) {
                                vm.dungeons.Add (d);
                            }
                        }
                        if (GUILayout.Button ("Delete")) {
                            workIndex = i;
                        }
                    }
                }
                if (workIndex > -1) {
                    vm.stageData.stageList.RemoveAt (workIndex);
                }
                if (GUILayout.Button ("Add New")) {
                    vm.stageData.stageList.Add (new DList ());
                }
                EditorUtility.SetDirty (vm.stageData);
            }
        }

		private void DrawVolumeList()
		{
			Color defColor = GUI.color;
			Color volColor = new Color (0.5f, 0.8f, 0.75f);
			float DefaultLabelWidth = EditorGUIUtility.labelWidth;
            if (vm.dungeons == null)
                vm.UpdateDungeon ();
			for (int i = 0; i < vm.dungeons.Count; i++) {
				Dungeon _d = vm.dungeons [i];
				GUI.color = volColor;
				using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
					GUI.color = defColor;
					EditorGUIUtility.labelWidth = 100;
					EditorGUILayout.ObjectField ("VolumeData", _d.volumeData, typeof(VolumeData), true);
					EditorGUIUtility.labelWidth = 88;
					EditorGUILayout.Vector3Field ("Position", _d.position);
					EditorGUILayout.Vector3Field ("Rotation", _d.rotation.eulerAngles);
					string _APName = _d.ArtPack.Replace (PathCollect.artPack, "");
					int _APNameIndex = artPacksList.IndexOf (_APName);
					EditorGUI.BeginChangeCheck ();
					_APNameIndex = EditorGUILayout.Popup ("ArtPack", _APNameIndex, artPacks);
					EditorGUILayout.LabelField ("Final ArtPack", _APName + _d.volumeData.subArtPack, EditorStyles.miniLabel);
					EditorGUILayout.LabelField ("Voxel Material", _d.vMaterial.Substring (_d.vMaterial.LastIndexOf ("/") + 1), EditorStyles.miniLabel);
					if (EditorGUI.EndChangeCheck ()) {
						_APName = artPacks [_APNameIndex];
						if (_APName.Length == 4)
							_APName = _APName.Remove (3);
						string _APPath = PathCollect.artPack + _APName;
						_d.ArtPack = _APPath;
						_d.vMaterial = _APPath + "/" + _APName + "_voxel";
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

        public static void DrawVGlobal ()
        {
            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                EditorGUILayout.LabelField ("Global Setting", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup (true);
                EditorGUILayout.ToggleLeft ("Auto Backup File", VolumeManager.saveBackup);
                EditorGUILayout.ToggleLeft ("Volume Show ArtPack", VolumeManager.volumeShowArtPack);
                EditorGUILayout.ToggleLeft ("Runtime Generation", VolumeManager.Generation);
                EditorGUILayout.ToggleLeft ("Snap Grid", VolumeManager.snapGrid);
                EditorGUILayout.ToggleLeft ("Show Ruler", VolumeManager.debugRuler);
                EditorGUILayout.ToggleLeft ("Show BlockHold", VolumeManager.showBlockHold);
                EditorGUI.EndDisabledGroup ();
            }
        }

        public static void DrawVLocal (VolumeManager vm)
        {
            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                EditorGUILayout.LabelField ("Local Setting", EditorStyles.boldLabel);
                vm.saveBackupL = EditorGUILayout.ToggleLeft ("Auto Backup File", vm.saveBackupL);
                vm.volumeShowArtPackL = EditorGUILayout.ToggleLeft ("Volume Show ArtPack", vm.volumeShowArtPackL);
                vm.GenerationL = EditorGUILayout.ToggleLeft ("Runtime Generation", vm.GenerationL);
                vm.snapGridL = EditorGUILayout.ToggleLeft ("Snap Grid", vm.snapGridL);
                vm.debugRulerL = EditorGUILayout.ToggleLeft ("Show Ruler", vm.debugRulerL);
                vm.showBlockHoldL = EditorGUILayout.ToggleLeft ("Show BlockHold", vm.showBlockHoldL);
            }
        }

		void UpdateStatus ()
		{
            vm.BroadcastMessage ("ShowRuler", SendMessageOptions.DontRequireReceiver);
		}

		#endregion
    }
}
