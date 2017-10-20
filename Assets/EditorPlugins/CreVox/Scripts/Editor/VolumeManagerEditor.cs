using UnityEngine;
using UnityEditor;

namespace CreVox
{
    [CustomEditor (typeof(VolumeManager))]
    public class VolumeManagerEditor : Editor
    {
        VolumeManager vm;
        VGlobal vg;
        int APIndex;
        string[] artPacks;

        void OnEnable ()
        {
            vm = (VolumeManager)target;
            vg = VGlobal.GetSetting ();
            ArtPackWindow.UpdateItemArrays (vg);
            artPacks = VGlobal.GetArtPacks ().ToArray ();
            UpdateStatus ();
        }

        const float buttonW = 50;
        const float lw = 60;

        public override void OnInspectorGUI ()
        {
            EditorGUIUtility.labelWidth = lw;

            using (var ch = new EditorGUI.ChangeCheckScope ()) {
                vm.useLocalSetting = EditorGUILayout.ToggleLeft ("Use Local Setting", vm.useLocalSetting);
                if (vm.useLocalSetting)
                    DrawVLocal (vm);
                else
                    DrawVGlobal ();

                if (ch.changed) {
                    UpdateLocalSetting ();
                }
            }

            EditorGUIUtility.wideMode = true;
            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                EditorGUILayout.LabelField ("StageData", EditorStyles.boldLabel);
                DrawStageData ();
            }
            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                EditorGUILayout.LabelField ("Volume SetAll Function", EditorStyles.boldLabel);
                DrawSetAll ();
            }

            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    EditorGUILayout.LabelField ("Volume List (" + vm.dungeons.Count + ")", EditorStyles.boldLabel);
                    if (GUILayout.Button ("Update", GUILayout.Width (buttonW)))
                        Button_VolumeList_Update ();
                    if (GUILayout.Button ("Clear", GUILayout.Width (buttonW)))
                        Button_VolumeList_Clear();
                }
                DrawVolumeList ();
            }

            DrawDef ();

            if (GUI.changed)
                UpdateStatus ();
        }

        public static void DrawVGlobal ()
        {
            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                EditorGUILayout.LabelField ("Global Setting", EditorStyles.boldLabel);
                using (var dis = new EditorGUI.DisabledScope (true)) {
                    EditorGUILayout.ToggleLeft ("Auto Backup File", VolumeManager.saveBackup);
                    EditorGUILayout.ToggleLeft ("Volume Show ArtPack", VolumeManager.volumeShowArtPack);
                    EditorGUILayout.ToggleLeft ("Runtime Generation", VolumeManager.Generation);
                    EditorGUILayout.ToggleLeft ("Snap Grid", VolumeManager.snapGrid);
                    EditorGUILayout.ToggleLeft ("Show Ruler", VolumeManager.debugRuler);
                    EditorGUILayout.ToggleLeft ("Show BlockHold", VolumeManager.showBlockHold);
                }
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

        void DrawStageData ()
        {
            using (var h = new EditorGUILayout.HorizontalScope ()) {
                vm.useStageData = EditorGUILayout.ToggleLeft ("Use StageData", vm.useStageData);
                if (!vm.useStageData)
                    return;
                if (GUILayout.Button ("TEST"))
                    Button_Test ();
            }
            vm.stageData = EditorGUILayout.ObjectField (vm.stageData, typeof(StageData), false) as StageData;
            if (vm.stageData == null)
                return;
            Color defColor = GUI.color;
            for (int i = 0; i < vm.stageData.stageList.Count; i++) {
                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    EditorGUILayout.LabelField (i + " (" + vm.stageData.stageList [i].Dlist.Count + ")", GUILayout.Width (38));
                    GUI.color = i == vm.currentStageData ? Color.green : defColor;
                    vm.stageData.stageList [i].Name = GUILayout.TextField (vm.stageData.stageList [i].Name);
                    GUI.color = defColor;
                    if (GUILayout.Button ("Save", GUILayout.Width (buttonW - 10)))
                        Button_Save (i);
                    if (GUILayout.Button ("Load", GUILayout.Width (buttonW - 10)))
                        Button_Load (i);
                    if (GUILayout.Button ("Delete", GUILayout.Width (buttonW))) {
                        Button_Delete (i);
                        break;
                    }
                }
            }
            using (var h = new EditorGUILayout.HorizontalScope ()) {
                GUILayout.FlexibleSpace ();
                if (GUILayout.Button ("Add", GUILayout.Width (buttonW)))
                    Button_Add ();
            }
            EditorUtility.SetDirty (vm.stageData);
        }

        void DrawSetAll ()
        {
            using (var h = new EditorGUILayout.HorizontalScope ()) {
                if (GUILayout.Button ("Build"))
                    Button_Build ();
                if (GUILayout.Button ("Generate"))
                    Button_Generate ();
                if (GUILayout.Button ("Update Portal"))
                    Button_UpdatePortal ();
            }
            using (var h = new EditorGUILayout.HorizontalScope ()) {
                APIndex = EditorGUILayout.Popup ("ArtPack", APIndex, artPacks);
                if (GUILayout.Button ("Set", GUILayout.Width (buttonW)))
                    Button_SetArtPack ();
            }
        }

        void DrawVolumeList ()
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
                    GUI.color = _d.volumeData == null ? Color.red : defColor;
                    EditorGUIUtility.labelWidth = 100;
                    EditorGUILayout.ObjectField ("[" + i + "]vData", _d.volumeData, typeof(VolumeData), true);
                    EditorGUIUtility.labelWidth = 88;
                    EditorGUILayout.Vector3Field ("Position", _d.position);
                    EditorGUILayout.Vector3Field ("Rotation", _d.rotation.eulerAngles);
                    string _APName = _d.ArtPack.Replace (PathCollect.artPack, "");
                    int _APNameIndex = VGlobal.GetArtPacks ().IndexOf (_APName);
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

        bool drawDef;

        void DrawDef ()
        {
            drawDef = EditorGUILayout.ToggleLeft ("Draw Default Inspector", drawDef, EditorStyles.miniLabel);
            if (drawDef)
                DrawDefaultInspector ();
        }

        #region Inspector Function

        void Button_Test ()
        {
            vm.RandomDungeon ();
            Button_Generate ();
        }

        void Button_Save (int _index)
        {
            vm.stageData.stageList [_index].Dlist.Clear ();
            vm.UpdateDungeon ();
            foreach (var d in vm.dungeons) {
                vm.stageData.stageList [_index].Dlist.Add (d);
            }
            vm.currentStageData = _index;
        }

        void Button_Load (int _index)
        {
            vm.dungeons.Clear ();
            foreach (var d in vm.stageData.stageList[_index].Dlist) {
                vm.dungeons.Add (d);
            }
            Button_Generate ();
            vm.currentStageData = _index;
        }

        void Button_Delete (int _index)
        {
            vm.stageData.stageList.RemoveAt (_index);
        }

        void Button_Add ()
        {
            vm.stageData.stageList.Add (new DList ());
        }

        void Button_Build ()
        {
            vm.BroadcastMessage ("BuildVolume", SendMessageOptions.DontRequireReceiver);
        }

        void Button_Generate ()
        {
            vm.ClearVolumes (false);
            foreach (Dungeon d in vm.dungeons) {
                GameObject volume = new GameObject (d.volumeData.name);
                volume.transform.parent = vm.transform;
                volume.transform.localPosition = d.position;
                volume.transform.localRotation = d.rotation;
                Volume v = volume.AddComponent<Volume> ();
                v.vd = d.volumeData;
                v.ArtPack = d.ArtPack;
                v.vMaterial = d.vMaterial;
            }
            UpdateLocalSetting ();
            Button_Build ();
        }

        void Button_UpdatePortal ()
        {
            VolumeAdapter.UpdatePortals (vm.gameObject);
        }

        void Button_SetArtPack ()
        {
            Volume[] volumes = vm.transform.GetComponentsInChildren<Volume> (false);
            foreach (Volume _v in volumes) {
                _v.ArtPack = PathCollect.artPack + artPacks [APIndex];
                _v.vMaterial = _v.ArtPack + "/" + artPacks [APIndex] + "_voxel";
                string ppath = PathCollect.resourcesPath + _v.vMaterial + ".mat";
                _v.vertexMaterial = AssetDatabase.LoadAssetAtPath<Material> (ppath);
                EditorUtility.SetDirty (_v.vd);
            }
            Button_Build ();
            vm.UpdateDungeon ();
        }

        void Button_VolumeList_Update()
        {
            vm.UpdateDungeon ();
        }

        void Button_VolumeList_Clear()
        {
            vm.dungeons.Clear ();
        }

        void UpdateStatus ()
        {
            vm.BroadcastMessage ("ShowRuler", SendMessageOptions.DontRequireReceiver);
        }

        void UpdateLocalSetting ()
        {
            Volume[] vs = vm.GetComponentsInChildren<Volume> ();
            foreach (Volume v in vs)
                v.vm = vm.useLocalSetting ? vm : null;
        }

        #endregion
    }
}
