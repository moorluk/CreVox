using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CreVox
{
    [CustomEditor (typeof(Volume))]
    public class VolumeEditor : Editor
    {
        Volume volume;

        Dictionary<WorldPos, Chunk> dirtyChunks = new Dictionary<WorldPos, Chunk> ();
        int cx = 1;
        int cy = 1;
        int cz = 1;
        WorldPos workpos;

        VGlobal vg { get { return volume.Vg; } }

        public struct TranslatedGo
        {
            public GameObject go;
            public WorldPos gPos;
        }

        public struct SelectedBlock
        {
            public Vector3 pos;
            public Block block;
        }

        void OnEnable ()
        {
            volume = (Volume)target;
            Volume.focusVolume = volume;
            BoxCursor.Create (volume.transform, vg);
            Rular.Create ();
            SubscribeEvents ();
        }

        void OnDisable ()
        {
            Volume.focusVolume = null;
            BoxCursor.Destroy ();
            Rular.Destroy ();
            UnsubscribeEvents ();
        }

        public override void OnInspectorGUI ()
        {
            GUI.color = (volume.vd == null) ? Color.red : Color.white;
            EditorGUIUtility.wideMode = true;

            DrawInsVData ();

            if (volume.vd == null)
                return;
            using (var ch1 = new EditorGUI.ChangeCheckScope ()) {
                DrawInsArtPack ();
                DrawInsSetting ();

                if (ch1.changed) {
                    EditorUtility.SetDirty (volume);
                    BuildVolume ();
                }
            }
            DrawPieceInspectedGUI ();
        }

        void DrawInsVData ()
        {
            float defLabelWidth = EditorGUIUtility.labelWidth;
            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                GUI.backgroundColor = Color.white;
                GUILayout.Label ("VolumeData", EditorStyles.boldLabel);
                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    if (GUILayout.Button ("Refresh")) {
                        BuildVolume ();
                    }
                    if (GUILayout.Button ("Calculate BlockHold")) {
                        CalculateBlockHold ();
                    }
                }
                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    using (var ch = new EditorGUI.ChangeCheckScope ()) {
                        volume.vd = (VolumeData)EditorGUILayout.ObjectField (volume.vd, typeof(VolumeData), false);
                        if (ch.changed) {
                            BuildVolume ();
                            volume.gameObject.name = volume.vd ? volume.vd.name.Replace ("_vData", "") : "???";
                        }
                    }
                    if (GUILayout.Button ("Backup", GUILayout.Width (ButtonW))) {
                        volume.SaveTempWorld ();
                    }
                }
                EditorGUILayout.Separator ();
                using (var ch = new EditorGUI.ChangeCheckScope ()) {
                    if (volume.vd == null)
                        return;
                    float intW = Mathf.Ceil (Screen.width - 119) / 3;
                    if (volume.vd.useFreeChunk) {
                        ChunkData c = volume.vd.freeChunk;
                        using (var h = new EditorGUILayout.HorizontalScope ()) {
                            EditorGUIUtility.labelWidth = 15;
                            EditorGUILayout.LabelField ("Chunk Size");
                            c.freeChunkSize.x = EditorGUILayout.DelayedIntField ("X", c.freeChunkSize.x, GUILayout.Width (intW));
                            c.freeChunkSize.y = EditorGUILayout.DelayedIntField ("Y", c.freeChunkSize.y, GUILayout.Width (intW));
                            c.freeChunkSize.z = EditorGUILayout.DelayedIntField ("Z", c.freeChunkSize.z, GUILayout.Width (intW));
                            EditorGUIUtility.labelWidth = defLabelWidth;
                        }
                        if (GUILayout.Button ("Convert to Voxel Chunk")) {
                            volume.vd.ConvertToVoxelChunk ();
                            CalculateBlockHold ();
                        }
                    } else {
                        using (var h = new EditorGUILayout.HorizontalScope ()) {
                            EditorGUIUtility.labelWidth = 15;
                            cx = EditorGUILayout.IntField ("X", cx, GUILayout.Width (intW));
                            cy = EditorGUILayout.IntField ("Y", cy, GUILayout.Width (intW));
                            cz = EditorGUILayout.IntField ("Z", cz, GUILayout.Width (intW));
                            EditorGUIUtility.labelWidth = defLabelWidth;
                            if (GUILayout.Button ("Init")) {
                                WriteVData (volume);
                            }
                        }
                        if (GUILayout.Button ("Convert to FreeSize Chunk")) {
                            volume.vd.ConvertToFreeChunk ();
                            CalculateBlockHold ();
                        }
                    }
                    if (ch.changed) {
                        EditorUtility.SetDirty (volume.vd);
                        EditorUtility.SetDirty (volume);
                        BuildVolume ();
                    }
                }
            }
        }

        void DrawInsArtPack ()
        {
            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                GUILayout.Label ("ArtPack", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox ((volume.ArtPack != null) ? (volume.ArtPack + volume.vd.subArtPack) : "(none.)", MessageType.Info, true);
                if (GUILayout.Button ("Set")) {
                    string ppath = EditorUtility.OpenFolderPanel (
                                       "選擇場景風格元件包的目錄位置",
                                       Application.dataPath + PathCollect.resourcesPath.Substring (6) + PathCollect.artPack,
                                       ""
                                   );
                    if (volume.Vm.SaveBackup)
                        volume.SaveTempWorld ();

                    string artPackName = ppath.Substring (ppath.LastIndexOf ("/") + 1);
                    if (artPackName.Length == 4) {
                        volume.vd.subArtPack = ppath.Substring (ppath.Length - 1);
                        ppath = ppath.Remove (ppath.Length - 1);
                        artPackName = artPackName.Remove (3);
                    } else {
                        volume.vd.subArtPack = "";
                    }

                    volume.ArtPack = PathCollect.artPack + artPackName;
                    volume.vMaterial = volume.ArtPack + "/" + artPackName + "_voxel";
                    ppath = ppath.Substring (ppath.IndexOf (PathCollect.resourcesPath)) + "/" + artPackName + "_voxel.mat";
                    volume.VertexMaterial = AssetDatabase.LoadAssetAtPath<Material> (ppath);
                    EditorUtility.SetDirty (volume.vd);
                    BuildVolume ();
                }
                EditorGUIUtility.labelWidth = 120f;
                volume.vd.subArtPack = EditorGUILayout.TextField ("SubArtPack", volume.vd.subArtPack);
                volume.VertexMaterial = (Material)EditorGUILayout.ObjectField (
                    new GUIContent ("Volume Material", "Auto Select if Material's name is ArtPack's name + \"_voxel\"")
					, volume.VertexMaterial
					, typeof(Material)
					, false);
            }
        }

        void DrawInsSetting ()
        {
            using (var ch2 = new EditorGUI.ChangeCheckScope ()) {
                VolumeManagerEditor.DrawVGlobal (volume.Vm);
                if (ch2.changed) {
                    BuildVolume ();
                }
            }
        }

        #region Scene GUI

        void OnSceneGUI ()
        {
            if (volume.vd == null)
                return;

            DrawMarkers ();
            Handles.BeginGUI ();
            DrawSelected ();
            DrawModeGUI ();
            DrawLayerModeGUI ();
            DrawModifyGUI ();
            Handles.EndGUI ();
            if (volume._itemInspected != null && volume._itemInspected.inspectedScript is PropertyPiece)
                ((PropertyPiece)volume._itemInspected.inspectedScript).DrawPatrolPoints ();
            
            EventHandler ();
            ModeHandler ();
        }

        static float editDis = 999;
        const float ButtonW = 80;
        const float blockW = 85f;
        bool isItemSnap;

        void DrawModeGUI ()
        {
            List<EditMode> modes = EditorUtils.GetListFromEnum<EditMode> ();
            List<string> modeLabels = new List<string> ();
            foreach (EditMode mode in modes) {
                modeLabels.Add (mode.ToString ());
            }

            GUI.color = volume.YColor;
            using (var a = new GUILayout.AreaScope (new Rect (10f, 10f, modeLabels.Count * ButtonW, 50f), "", EditorStyles.textArea)) {
                GUI.color = Color.white;
                selectedEditMode = (EditMode)GUILayout.Toolbar ((int)selectedEditMode, modeLabels.ToArray (), GUILayout.ExpandHeight (true));
                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    EditorGUILayout.LabelField ("Editable Distance", GUILayout.Width (105));
                    using (var c = new EditorGUI.ChangeCheckScope ()) {
                        editDis = GUILayout.HorizontalSlider (editDis, 99f, 999f);
                        if (c.changed)
                            EditorUtility.SetDirty (vg);
                    }
                    EditorGUILayout.LabelField (((int)editDis).ToString (), GUILayout.Width (25));
                    if (selectedEditMode == EditMode.Item)
                    isItemSnap = EditorGUILayout.ToggleLeft ("Snap Item", isItemSnap, GUILayout.Width (ButtonW));
                }
            }
        }

        void DrawLayerModeGUI ()
        {
            int tile = 2;
            if (_pieceSelected != null)
                tile++;
            GameObject itemObj = selectedItemID > -1 ? volume.GetItemNode (volume.vd.blockItems [selectedItemID]) : null;
            if(itemObj)
                tile++;
            using (var a = new GUILayout.AreaScope (new Rect (10f, 65f, (blockW + 7f) * tile, 65f), "")) {
                using (var h = new GUILayout.HorizontalScope ()) {
                    bool _hotkey = currentEditMode != EditMode.View && currentEditMode != EditMode.Item;

                    GUI.color = volume.YColor;
                    using (var v = new GUILayout.VerticalScope (EditorStyles.textArea, GUILayout.Width (blockW))) {
                        GUI.color = Color.white;
                        if (GUILayout.Button ("Pointer" + (_hotkey ? "(Q)" : "")))
                            HotkeyFunction ("Q");
                        using (var d = new EditorGUI.DisabledGroupScope (!volume.pointer)) {
                            using (var h2 = new GUILayout.HorizontalScope ()) {
                                GUILayout.Label (volume.pointY.ToString (), "TL Selection H2", GUILayout.Height (0), GUILayout.Width (24));
                                using (var v2 = new GUILayout.VerticalScope ()) {
                                    if (GUILayout.Button ("▲" + (_hotkey ? "(W)" : ""), GUILayout.Width (45)))
                                        HotkeyFunction ("W");
                                    if (GUILayout.Button ("▼" + (_hotkey ? "(S)" : ""), GUILayout.Width (45)))
                                        HotkeyFunction ("S");
                                }
                            }
                        }
                    }

                    GUI.color = volume.YColor;
                    using (var v = new GUILayout.VerticalScope (EditorStyles.textArea, GUILayout.Width (blockW))) {
                        GUI.color = Color.white;
                        if (GUILayout.Button ("Cutter" + (_hotkey ? "(E)" : "")))
                            HotkeyFunction ("E");
                        using (var d = new EditorGUI.DisabledGroupScope (!volume.cuter)) {
                            using (var h2 = new GUILayout.HorizontalScope ()) {
                                GUILayout.Label (volume.cutY.ToString (), "TL Selection H2", GUILayout.Height (0), GUILayout.Width (24));
                                using (var v2 = new GUILayout.VerticalScope ()) {
                                    if (GUILayout.Button ("▲" + (_hotkey ? "(R)" : ""), GUILayout.Width (45)))
                                        HotkeyFunction ("R");
                                    if (GUILayout.Button ("▼" + (_hotkey ? "(F)" : ""), GUILayout.Width (45)))
                                        HotkeyFunction ("F");
                                }
                            }
                        }
                    }

                    if (_pieceSelected != null) {
                        GUI.color = volume.YColor;
                        using (var v = new GUILayout.VerticalScope (EditorStyles.textArea, GUILayout.Width (blockW))) {
                            GUI.color = Color.white;
                            GUILayout.Space (0);
                            Rect pr = new Rect (GUILayoutUtility.GetLastRect ().position, new Vector2 (63, 63));
                            GUI.DrawTexture (pr, _itemPreview);
                            GUILayout.Label ("Marker", EditorStyles.boldLabel, GUILayout.Width (blockW));
                            GUILayout.Space (26);
                            GUILayout.Label (_pieceSelected.GetComponent<PaletteItem> ().itemName, GUILayout.Width (blockW));
                        }
                    }

                    if (itemObj) {
                        using (var v = new GUILayout.VerticalScope (EditorStyles.textArea, GUILayout.Width (blockW))) {
                            GUILayout.Space (0);
                            Texture2D _prev = AssetPreview.GetAssetPreview (PrefabUtility.GetPrefabParent (itemObj)) ?? AssetPreview.GetMiniTypeThumbnail (typeof(GameObject));
                            Rect pr = new Rect (GUILayoutUtility.GetLastRect ().position, new Vector2 (63, 63));
                            GUI.DrawTexture (pr, _prev);
                            GUILayout.Label ("Editing Item",EditorStyles.boldLabel, GUILayout.Width (blockW));
                            GUILayout.Space (26);
                            GUILayout.Label (itemObj.GetComponent<PaletteItem>().itemName ?? "", GUILayout.Width (blockW));
                        }
                    }
                }
            }
        }

        bool m_mappingX;
        bool m_mappingZ;
        Vector3 m_selectedMin;
        Vector3 m_selectedMax;
        List<Vector3> m_selectedBlocks = new List<Vector3> ();
        Vector3 m_translate;

        void DrawModifyGUI ()
        {
            using (var a = new GUILayout.AreaScope (new Rect (10f, 135f, blockW * 2 + 4, 230f), "")) {
                EditorGUIUtility.wideMode = true;

                GUI.color = volume.YColor;
                using (var v = new GUILayout.VerticalScope (EditorStyles.textArea)) {
                    GUI.color = Color.white;
                    EditorGUIUtility.labelWidth = 40;
                    EditorGUILayout.LabelField ("Offset All", EditorStyles.boldLabel);
                    using (var h = new GUILayout.HorizontalScope ()) {
                        GUILayout.Space (40);
                        if (GUILayout.Button ("Ｘ＋"))
                            OffsetBlock (new WorldPos (1, 0, 0));
                        if (GUILayout.Button ("Ｙ＋"))
                            OffsetBlock (new WorldPos (0, 1, 0));
                        if (GUILayout.Button ("Ｚ＋"))
                            OffsetBlock (new WorldPos (0, 0, 1));
                    }
                    using (var h = new GUILayout.HorizontalScope ()) {
                        GUILayout.Space (40);
                        if (GUILayout.Button ("Ｘ－"))
                            OffsetBlock (new WorldPos (-1, 0, 0));
                        if (GUILayout.Button ("Ｙ－"))
                            OffsetBlock (new WorldPos (0, -1, 0));
                        if (GUILayout.Button ("Ｚ－"))
                            OffsetBlock (new WorldPos (0, 0, -1));
                    }
                }

                GUI.color = volume.YColor;
                using (var v = new GUILayout.VerticalScope (EditorStyles.textArea)) {
                    GUI.color = Color.white;
                    using (var h = new GUILayout.HorizontalScope ()) {
                        EditorGUIUtility.labelWidth = 10;
                        EditorGUILayout.LabelField ("Mirror Edit", EditorStyles.boldLabel);
                        m_mappingX = GUILayout.Toggle (m_mappingX, "X", GUILayout.Width (40));
                        m_mappingZ = GUILayout.Toggle (m_mappingZ, "Z", GUILayout.Width (40));
                    }
                }

                GUI.color = volume.YColor;
                using (var v = new GUILayout.VerticalScope (EditorStyles.textArea)) {
                    GUI.color = Color.white;
                    EditorGUIUtility.labelWidth = 40;
                    EditorGUILayout.LabelField ("Select", EditorStyles.boldLabel);
                    m_selectedMin = EditorGUILayout.Vector3Field ("min", m_selectedMin);
                    m_selectedMax = EditorGUILayout.Vector3Field ("max", m_selectedMax);
                    using (var h = new GUILayout.HorizontalScope ()) {
                        if (GUILayout.Button ("Add"))
                            HotkeyFunction ("Add");
                        if (GUILayout.Button ("Remove"))
                            HotkeyFunction ("Remove");
                    }
                }

                GUI.color = volume.YColor;
                using (var v = new GUILayout.VerticalScope (EditorStyles.textArea)) {
                    GUI.color = Color.white;
                    EditorGUILayout.LabelField ("Translate", EditorStyles.boldLabel);
                    m_translate = EditorGUILayout.Vector3Field ("Offset", m_translate);
                    using (var h = new GUILayout.HorizontalScope ()) {
                        if (GUILayout.Button ("Translate"))
                            HotkeyFunction ("Translate");
                        if (GUILayout.Button ("Copy"))
                            HotkeyFunction ("Copy");
                    }
                }
            }
        }

        #endregion

        #region Draw Marker
        int button;
        bool selected;

        void DrawMarkers ()
        {
            if (Event.current.alt) {
                BoxCursor.visible = false;
                return;
            }

            if (Event.current.type == EventType.KeyDown) {
                HotkeyFunction (Event.current.keyCode.ToString (), true);
                return;
            } 

            if (currentEditMode == EditMode.View)
                return; 

            button = Event.current.button;
            HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

            switch (currentEditMode) {
            case EditMode.Voxel:
                if (button == 0)
                    DrawMarkerVoxel (false);
                else if (button == 1) {
                    DrawMarkerVoxel (true);
                }
                break;

            case EditMode.VoxelLayer: 
                DrawMarkerVoxelLayer ();
                break;

            case EditMode.Object:
            case EditMode.ObjectLayer:
                DrawMarkerObject ();
                break;

            case EditMode.Item:
                DrawMarkerItem ();
                break;
            }
        }

        void DrawMarkerVoxel (bool isErase)
        {
            RaycastHit hit;
            Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
            LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
            bool isHit = Physics.Raycast (worldRay, out hit, editDis, _mask);

            if (isHit && !isErase && hit.collider.GetComponentInParent<Volume> () == volume) {
                RaycastHit hitFix = hit;
                WorldPos pos = EditTerrain.GetBlockPos (hitFix, !isErase);
                hitFix.point = new Vector3 (pos.x * vg.w, pos.y * vg.h, pos.z * vg.d);

                if (hit.collider.gameObject.tag == PathCollect.rularTag) {
                    hit.normal = Vector3.zero;
                }
                Handles.RectangleCap (0, hitFix.point - new Vector3 (0, vg.h / 2, 0), Quaternion.Euler (90, 0, 0), vg.w / 2);
                Handles.DrawLine (hit.point, hitFix.point);
                BoxCursor.Update (hitFix.point, hit.normal);
                BoxCursor.visible = true;
            } else {
                BoxCursor.visible = false;
            }
            SceneView.RepaintAll ();
        }

        void DrawMarkerVoxelLayer ()
        {
            RaycastHit hit;
            Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
            LayerMask _mask = 1 << LayerMask.NameToLayer ("EditorLevel");
            bool isHit = Physics.Raycast (worldRay, out hit, editDis, _mask);

            if (isHit && hit.collider.GetComponentInParent<Volume> () == volume) {
                RaycastHit hitFix = hit;
                WorldPos pos = EditTerrain.GetBlockPos (hitFix, false);
                hitFix.point = new Vector3 (pos.x * vg.w, pos.y * vg.h, pos.z * vg.d);

                Handles.RectangleCap (0, hitFix.point + new Vector3 (0, vg.h / 2, 0), Quaternion.Euler (90, 0, 0), vg.w / 2);
                Handles.DrawLine (hit.point, hitFix.point);
                BoxCursor.Update (hitFix.point, hit.normal);
                BoxCursor.visible = true;
            } else {
                BoxCursor.visible = false;
            }
            SceneView.RepaintAll ();
        }

        void DrawMarkerObject ()
        {
            if (_pieceSelected == null)
                return;
            bool isNotLayer = (currentEditMode != EditMode.ObjectLayer);
            RaycastHit hit;
            Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
            LayerMask _mask = isNotLayer ? 1 << LayerMask.NameToLayer ("Editor") : 1 << LayerMask.NameToLayer ("EditorLevel");
            bool isHit = Physics.Raycast (worldRay, out hit, editDis, _mask);

            if (isHit && hit.collider.GetComponentInParent<Volume> () == volume) {
                if (hit.normal.y <= 0) {
                    BoxCursor.visible = false;
                    return;
                }

                RaycastHit hitFix = hit;
                WorldPos pos = EditTerrain.GetBlockPos (hitFix, isNotLayer);
                hitFix.point = new Vector3 (pos.x * vg.w, pos.y * vg.h, pos.z * vg.d);

                WorldPos gPos = EditTerrain.GetGridPos (hit.point);
                gPos.y = isNotLayer ? 0 : (int)vg.h;

                float gx = pos.x * vg.w + gPos.x + ((pos.x < 0) ? 1 : -1);
                float gy = pos.y * vg.h + gPos.y - vg.h / 2;
                float gz = pos.z * vg.d + gPos.z + ((pos.z < 0) ? 1 : -1);

                LevelPiece.PivotType pivot = _pieceSelected.pivot;
                if (CheckPlaceable (gPos.x, gPos.z, pivot)) {
                    Handles.color = Color.red;
                    Handles.RectangleCap (0, new Vector3 (gx, gy, gz), Quaternion.Euler (90, 0, 0), 0.5f);
                    Handles.color = Color.white;
                }

                Handles.color = Color.white;
                Handles.lighting = true;
                Handles.RectangleCap (0, hitFix.point - new Vector3 (0, vg.h / 2 - gPos.y, 0), Quaternion.Euler (90, 0, 0), vg.w / 2);
                Handles.DrawLine (hit.point, hitFix.point);

                BoxCursor.Update (hitFix.point, Vector3.zero);
                BoxCursor.visible = true;
            } else {
                BoxCursor.visible = false;
            }
            SceneView.RepaintAll ();
        }

        int workItemId = -1;
        int selectedItemID = -1;

        void DrawMarkerItem ()
        {
            Color defColor = Handles.color;
            Quaternion facingCamera;

                //draw handle 'can paint item'.
                RaycastHit hit;
                Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
                LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
                bool isHit = Physics.Raycast (worldRay, out hit, editDis, _mask);
                if (isHit && hit.collider.GetComponentInParent<Volume> () == volume && hit.normal.y > 0) {
                    Handles.color = new Color (0f / 255f, 202f / 255f, 255f / 255f, 0.3f);
                    Handles.DrawWireDisc (hit.point, hit.normal, 0.5f);
                }

            if (selectedItemID != -1) {
                // draw move & rotate handle.
                BlockItem blockItem = volume.vd.blockItems [selectedItemID];
                GameObject itemObj = volume.GetItemNode (blockItem);
                if (itemObj != null) {
                    Transform ItemNode = itemObj.transform;
                    Vector3 pos = ItemNode.position;
                    Vector3 handlePos = isItemSnap ? pos + new Vector3 (0, vg.h / 2, 0) : pos;
                    handlePos = Handles.DoPositionHandle (handlePos, ItemNode.rotation);

                    if (isItemSnap) {
                        float fixedX = Mathf.Round (handlePos.x * 2) / 2;
                        float fixedY = Mathf.Round (handlePos.y / vg.h) * vg.h - (vg.h / 2 - 0.01f);
                        float fixedZ = Mathf.Round (handlePos.z * 2) / 2;
                        pos = new Vector3 (fixedX, fixedY, fixedZ);
                    } else {
                        pos = handlePos;
                    }
                    ItemNode.position = pos;
                    blockItem.BlockPos = EditTerrain.GetBlockPos (ItemNode.localPosition);
                    blockItem.posX = ItemNode.localPosition.x;
                    blockItem.posY = ItemNode.localPosition.y;
                    blockItem.posZ = ItemNode.localPosition.z;

                    ItemNode.localRotation = Handles.RotationHandle (ItemNode.localRotation, pos);
                    Quaternion tmp = ItemNode.localRotation;
                    Vector3 rot = tmp.eulerAngles;
                    rot.x = Mathf.Round (rot.x / 15f) * 15f;
                    rot.y = Mathf.Round (rot.y / 15f) * 15f;
                    rot.z = Mathf.Round (rot.z / 15f) * 15f;
                    tmp.eulerAngles = rot;
                    ItemNode.localRotation = tmp;
                    blockItem.rotX = ItemNode.localRotation.x;
                    blockItem.rotY = ItemNode.localRotation.y;
                    blockItem.rotZ = ItemNode.localRotation.z;
                    blockItem.rotW = ItemNode.localRotation.w;

                    EditorUtility.SetDirty (volume.vd);
                }
            }

            for (int i = 0; i < volume.vd.blockItems.Count; i++) {
                BlockItem blockItem = volume.vd.blockItems [i];
                GameObject itemObj = volume.GetItemNode (blockItem);
                if (itemObj != null) {
                    Transform ItemNode = itemObj.transform;
                    Vector3 pos = ItemNode.position;
                    // draw button.
                    float handleSize = HandleUtility.GetHandleSize (pos) * 0.15f;
                    Handles.color = new Color (0f / 255f, 202f / 255f, 255f / 255f, 0.1f);
                    facingCamera = Camera.current.transform.rotation * Quaternion.Euler (0, 0, 180);
                    selected = Handles.Button (pos, facingCamera, handleSize, handleSize, Handles.SphereHandleCap);
                    Handles.color = defColor;
                    //compare selectedItemID.
                    if (selected) {
                        workItemId = i;
                        break;
                    }
                }
            }
            SceneView.RepaintAll ();
        }

        void DrawSelected ()
        {
            Color old = Handles.color;
            Handles.color = Color.red;
            float width = vg.w;
            float height = vg.h;
            float depth = vg.d;

            int count = m_selectedBlocks.Count;
            for (int i = 0; i < count; ++i) {
                float x = m_selectedBlocks [i].x * width;
                float y = m_selectedBlocks [i].y * height;
                float z = m_selectedBlocks [i].z * depth;
                Handles.DrawWireCube (new Vector3 (x, y, z), new Vector3 (width, height, depth));
            }

            Handles.color = old;
        }

        #endregion

        #region Handlar

        public enum EditMode
        {
            View,
            VoxelLayer,
            Voxel,
            ObjectLayer,
            Object,
            Item
        }

        EditMode selectedEditMode;
        EditMode currentEditMode;

        void ModeHandler ()
        {
            switch (selectedEditMode) {
            case EditMode.Voxel:
            case EditMode.VoxelLayer:
            case EditMode.Object:
            case EditMode.ObjectLayer:
                Tools.current = Tool.None;
                break;

            case EditMode.View:
            case EditMode.Item:
                break;
            }
            if (selectedEditMode != currentEditMode) {
                currentEditMode = selectedEditMode;
                volume._itemInspected = null;
                selectedItemID = -1;
                Repaint ();
            }
        }

        void EventHandler ()
        {
            if (Event.current.alt) {
                BoxCursor.visible = false;
                return;
            }

            if (Event.current.type == EventType.KeyDown) {
                HotkeyFunction (Event.current.keyCode.ToString (), true);
                return;
            } 

            if (currentEditMode == EditMode.View)
                return;

            switch (currentEditMode) {
            case EditMode.Voxel:
                if (Event.current.type == EventType.MouseDown) {
                    if (button == 0)
                        Paint (false);
                    else if (button == 1) {
                        Paint (true);
                        Tools.viewTool = ViewTool.None;
                        Event.current.Use ();
                    }
                }
                break;

            case EditMode.VoxelLayer:
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) {
                    if (button == 0)
                        PaintLayer (false);
                    else if (button == 1) {
                        PaintLayer (true);
                        Tools.viewTool = ViewTool.None;
                        Event.current.Use ();
                    }
                }
                break;

            case EditMode.Object:
            case EditMode.ObjectLayer:
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) {
                    if (button == 0)
                        PaintPiece (false);
                    else if (button == 1) {
                        PaintPiece (true);
                        Tools.viewTool = ViewTool.None;
                        Event.current.Use ();
                    }
                }
                break;

            case EditMode.Item:
                if (selected) {
                    if (workItemId != -1) {
                        // toggle item selection.
                        selectedItemID = (selectedItemID != workItemId) ? workItemId : -1;
                        Debug.Log ("[Sel]Wrk:<b>" + workItemId + "</b> Slt:<b>" + selectedItemID + "</b>");
                        UpdateInapectedItem (selectedItemID);
                        workItemId = -1;
                    }
                    selected = false;
                } else if (Event.current.type == EventType.MouseDown && button == 0) {
                    if (workItemId == -1 && selectedItemID == -1) {
                        // add new item.
                        PaintItem (false);
                        Debug.Log ("[Add]Wrk:<b>" + workItemId + "</b> Slt:<b>" + selectedItemID + "</b>");
                        UpdateInapectedItem (selectedItemID);
                        workItemId = -1;
                    }
                }
                break;
            }

            if (Event.current.type == EventType.MouseUp) {
                UpdateDirtyChunks ();
            }
        }

        #endregion

        #region Paint

        void Paint (bool isErase)
        {
            RaycastHit gHit;
            Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
            LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
            bool isHit = Physics.Raycast (worldRay, out gHit, editDis, _mask);
            WorldPos pos;

            if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
                gHit.point = volume.transform.InverseTransformPoint (gHit.point);
                gHit.normal = volume.transform.InverseTransformDirection (gHit.normal);
                pos = EditTerrain.GetBlockPos (gHit, !isErase);

                //volume.SetBlock (pos.x, pos.y, pos.z, isErase ? null : new Block ());
                VolumeHelper.MirrorPosition (volume,
                    pos,
                    new WorldPos (cx, cy, cz),
                    isErase,
                    m_mappingX,
                    m_mappingZ);
                Chunk chunk = volume.GetChunk (pos.x, pos.y, pos.z);

                if (chunk) {
                    if (!dirtyChunks.ContainsKey (pos))
                        dirtyChunks.Add (pos, chunk);
                    chunk.UpdateMeshFilter ();
                    SceneView.RepaintAll ();
                }
            }
            EditorUtility.SetDirty (volume.vd);
        }

        void PaintLayer (bool isErase)
        {
            RaycastHit gHit;
            Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
            LayerMask _mask = 1 << LayerMask.NameToLayer ("EditorLevel");
            bool isHit = Physics.Raycast (worldRay, out gHit, editDis, _mask);
            WorldPos pos;

            if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
                gHit.point = gHit.point + new Vector3 (0f, -vg.h, 0f);
                gHit.point = volume.transform.InverseTransformPoint (gHit.point);
                pos = EditTerrain.GetBlockPos (gHit, true);

                //volume.SetBlock (pos.x, pos.y, pos.z, isErase ? null : new Block ());
                VolumeHelper.MirrorPosition (volume, 
                    pos, 
                    new WorldPos (cx, cy, cz), 
                    isErase, 
                    m_mappingX, 
                    m_mappingZ);

                Chunk chunk = volume.GetChunk (pos.x, pos.y, pos.z);
                if (chunk) {
                    if (!dirtyChunks.ContainsKey (pos))
                        dirtyChunks.Add (pos, chunk);
                    chunk.UpdateMeshFilter ();
                    SceneView.RepaintAll ();
                }
            }
            EditorUtility.SetDirty (volume.vd);
        }

        void PaintPiece (bool isErase)
        {
            if (_pieceSelected == null)
                return;
			
            RaycastHit gHit;
            Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
            LayerMask _mask = (currentEditMode == EditMode.Object) ? 1 << LayerMask.NameToLayer ("Editor") : 1 << LayerMask.NameToLayer ("EditorLevel");
            bool isHit = Physics.Raycast (worldRay, out gHit, editDis, _mask);

            if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
                if (gHit.normal.y <= 0)
                    return;

                gHit.point = volume.transform.InverseTransformPoint (gHit.point);
                WorldPos bPos = EditTerrain.GetBlockPos (gHit, (currentEditMode == EditMode.Object));
                WorldPos gPos = EditTerrain.GetGridPos (gHit.point);
                gPos.y = 0;
                
                if (CheckPlaceable (gPos.x, gPos.z, _pieceSelected.pivot)) {

                    VolumeHelper.Mirror (volume,
                        bPos,
                        gPos,
                        new WorldPos (cx, cy, cz),
                        isErase ? null : _pieceSelected,
                        m_mappingX,
                        m_mappingZ);
                    
                    Chunk chunk = volume.GetChunk (bPos.x, bPos.y, bPos.z);
                    chunk.UpdateChunk ();
                    EditorUtility.SetDirty (volume.vd);
                    SceneView.RepaintAll ();
                }
            }
        }

        void PaintItem (bool isErase)
        {
            if (_pieceSelected == null)
                return;

            if (isErase) {
                volume.PlaceItem (selectedItemID, null);
                workItemId = -1;
                selectedItemID = -1;
                UpdateInapectedItem (selectedItemID);
            } else {
                RaycastHit gHit;
                Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
                LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
                bool isHit = Physics.Raycast (worldRay, out gHit, editDis, _mask);

                if (!isHit || gHit.collider.GetComponentInParent<Volume> () != volume || gHit.normal.y <= 0)
                    return;
                gHit.point = volume.transform.InverseTransformPoint (gHit.point);
                volume.PlaceItem (volume.vd.blockItems.Count, _pieceSelected, gHit.point);
            }
            SceneView.RepaintAll ();
            EditorUtility.SetDirty (volume.vd);
        }

        #endregion

        #region Tools

        void HotkeyFunction (string funcKey = "", bool isKeyEvent = false)
        {
            int _index = (int)currentEditMode;
            int _count = Enum.GetValues (typeof(EditMode)).Length - 1;
            bool _hotkey = !isKeyEvent || (currentEditMode != EditMode.View && currentEditMode != EditMode.Item);

            switch (funcKey) {
            case "A":
                selectedEditMode = _index == 0 ? (EditMode)_count : (EditMode)(_index - 1);
                Repaint ();
                break;

            case "D":
                selectedEditMode = _index == _count ? (EditMode)0 : (EditMode)(_index + 1);
                Repaint ();
                break;

            case "Q":
                if (_hotkey) {
                    volume.pointer = !volume.pointer;
                    Rular.SetY (volume.pointY);
                }
                break;

            case "W":
                if (_hotkey)
                    Rular.SetY (++volume.pointY);
                break;

            case "S":
                if (_hotkey)
                    Rular.SetY (--volume.pointY);
                break;

            case "E":
                if (_hotkey) {
                    volume.cuter = !volume.cuter;
                    volume.ChangeCutY (volume.cutY);
                }
                break;

            case "R":
                if (_hotkey)
                    volume.ChangeCutY (++volume.cutY);
                break;

            case "F":
                if (_hotkey)
                    volume.ChangeCutY (--volume.cutY);
                break;

            case "Add":
                if (_hotkey) {
                    VolumeHelper.SelectedAdd (ref m_selectedBlocks, m_selectedMin, m_selectedMax);
                }
                break;

            case "Remove":
                if (_hotkey) {
                    VolumeHelper.SelectedRemove (ref m_selectedBlocks, m_selectedMin, m_selectedMax);
                }
                break;
            
            case "Translate":
                if (_hotkey) {
                    Translate ();
                    EditorUtility.SetDirty (volume.vd);
                }
                break;

            case "Copy":
                if (_hotkey) {
                    Translate (false);
                    EditorUtility.SetDirty (volume.vd);
                }
                break;
            }

            if (_hotkey)
                Event.current.Use ();
        }

        void CalculateBlockHold ()
        {
            if (volume.vd.useFreeChunk) {
                volume.vd.freeChunk.blockHolds.Clear ();
            } else {
                foreach (ChunkData bh in volume.vd.chunkDatas) {
                    bh.blockHolds.Clear ();
                }
            }
            string apOld = volume.ArtPack;
            volume.ArtPack = PathCollect.pieces;
            BuildVolume ();
            volume.ArtPack = apOld;
            BuildVolume ();
            EditorUtility.SetDirty (volume);
        }

        void BuildVolume ()
        {
            selectedItemID = -1;
            volume.Build ();
            BoxCursor.Create (volume.transform, vg);
            Rular.Create ();
            SceneView.RepaintAll ();
        }

        void UpdateInapectedItem (int id)
        {
            GameObject ItemNode = (id > -1) ? volume.GetItemNode (volume.vd.blockItems [id]) : null;
            volume._itemInspected = (ItemNode != null) ? ItemNode.GetComponent<PaletteItem> () : null;
            EditorUtility.SetDirty (volume);
        }

        void UpdateDirtyChunks ()
        {
            foreach (KeyValuePair<WorldPos, Chunk> c in dirtyChunks) {
                c.Value.UpdateMeshCollider ();
            }
            dirtyChunks.Clear ();
        }

        static bool CheckPlaceable (int x, int z, LevelPiece.PivotType pType)
        {
            if (pType == LevelPiece.PivotType.Grid)
                return true;
            if (pType == LevelPiece.PivotType.Center && (x * z) == 1)
                return true;
            if (pType == LevelPiece.PivotType.Vertex && (x + z) % 2 == 0 && x * z != 1)
                return true;
            if (pType == LevelPiece.PivotType.Edge && (Mathf.Abs (x) + Mathf.Abs (z)) % 2 == 1)
                return true;

            return false;
        }

        public static void WriteVData (Volume _volume)
        {
            string sPath = Application.dataPath + PathCollect.resourcesPath.Substring (6) + PathCollect.save;
            sPath = EditorUtility.SaveFilePanel ("save vData", sPath, _volume.name + "_vData", "asset");
            if (sPath.Length < 1)
                return;
            sPath = sPath.Substring (sPath.LastIndexOf (PathCollect.resourcesPath));
            VolumeData vd = Resources.Load (sPath) as VolumeData;
            if (vd == null) {
                vd = ScriptableObject.CreateInstance<VolumeData> ();
                AssetDatabase.CreateAsset (vd, sPath);
                AssetDatabase.Refresh ();
            }

            _volume.vd = vd;
        }

        void OffsetBlock (WorldPos _offset)
        {
            bool isFreeChunk = volume.vd.useFreeChunk;
            if (!isFreeChunk)
                volume.vd.ConvertToFreeChunk ();
            ChunkData newc = new ChunkData ();
            newc.freeChunkSize = volume.vd.freeChunk.freeChunkSize;
            foreach (var b in volume.vd.freeChunk.blocks) {
                b.BlockPos.x += _offset.x;
                while (b.BlockPos.x < 0)
                    b.BlockPos.x += newc.freeChunkSize.x;
                while (b.BlockPos.x >= newc.freeChunkSize.x)
                    b.BlockPos.x -= newc.freeChunkSize.x;
                b.BlockPos.y += _offset.y;
                while (b.BlockPos.y < 0)
                    b.BlockPos.y += newc.freeChunkSize.y;
                while (b.BlockPos.y >= newc.freeChunkSize.y)
                    b.BlockPos.y -= newc.freeChunkSize.y;
                b.BlockPos.z += _offset.z;
                while (b.BlockPos.z < 0)
                    b.BlockPos.z += newc.freeChunkSize.z;
                while (b.BlockPos.z >= newc.freeChunkSize.z)
                    b.BlockPos.z -= newc.freeChunkSize.z;
                newc.blocks.Add (b);
            }
            foreach (var ba in volume.vd.freeChunk.blockAirs) {
                ba.BlockPos.x += _offset.x;
                while (ba.BlockPos.x < 0)
                    ba.BlockPos.x += newc.freeChunkSize.x;
                while (ba.BlockPos.x >= newc.freeChunkSize.x)
                    ba.BlockPos.x -= newc.freeChunkSize.x;
                ba.BlockPos.y += _offset.y;
                while (ba.BlockPos.y < 0)
                    ba.BlockPos.y += newc.freeChunkSize.y;
                while (ba.BlockPos.y >= newc.freeChunkSize.y)
                    ba.BlockPos.y -= newc.freeChunkSize.y;
                ba.BlockPos.z += _offset.z;
                while (ba.BlockPos.z < 0)
                    ba.BlockPos.z += newc.freeChunkSize.z;
                while (ba.BlockPos.z >= newc.freeChunkSize.z)
                    ba.BlockPos.z -= newc.freeChunkSize.z;
                newc.blockAirs.Add (ba);
            }
            foreach (var bi in volume.vd.blockItems) {
                bi.posX += _offset.x * vg.w;
                while (bi.posX < -vg.w / 2)
                    bi.posX += newc.freeChunkSize.x * vg.w;
                while (bi.posX >= newc.freeChunkSize.x * vg.w - vg.w / 2)
                    bi.posX -= newc.freeChunkSize.x * vg.w;
                bi.posY += _offset.y * vg.h;
                while (bi.posY < -vg.h / 2)
                    bi.posY += newc.freeChunkSize.y * vg.h;
                while (bi.posY >= newc.freeChunkSize.y * vg.h - vg.h / 2)
                    bi.posY -= newc.freeChunkSize.y * vg.h;
                bi.posZ += _offset.z * vg.d;
                while (bi.posZ < -vg.d / 2)
                    bi.posZ += newc.freeChunkSize.z * vg.d;
                while (bi.posZ >= newc.freeChunkSize.z * vg.d - vg.d / 2)
                    bi.posZ -= newc.freeChunkSize.z * vg.d;

                bi.BlockPos = EditTerrain.GetBlockPos (new Vector3 (bi.posX, bi.posY, bi.posZ));
            }

            volume.vd.freeChunk = newc;
            if (!isFreeChunk)
                volume.vd.ConvertToVoxelChunk ();
            CalculateBlockHold ();
            EditorUtility.SetDirty (volume.vd);
            BuildVolume ();
            SceneView.RepaintAll ();
        }

        public void Translate (bool a_cut = true)
        {
            List<SelectedBlock> translatedBlocks = new List<SelectedBlock> ();
            List<TranslatedGo> translatedObjects = new List<TranslatedGo> ();

            CopyObjectAndBlock (ref translatedObjects, ref translatedBlocks, a_cut);

            TranslateBlock (translatedBlocks);
            TranslateObject (translatedObjects);
        }

        void CopyObjectAndBlock (ref List<TranslatedGo> a_objects, ref  List<SelectedBlock> a_blocks, bool a_cut)
        {
            int count = m_selectedBlocks.Count;
            for (int i = 0; i < count; ++i) {
                Vector3 pos = m_selectedBlocks [i];
                SelectedBlock sb;
                sb.pos = pos;

                Block block = volume.GetBlock ((int)pos.x, (int)pos.y, (int)pos.z);
                Chunk chunk = volume.GetChunk ((int)pos.x, (int)pos.y, (int)pos.z);

                if (chunk != null) {
                    WorldPos chunkBlockPos = new WorldPos ((int)pos.x, (int)pos.y, (int)pos.z);
                    bool objectPlaced = false;
                    for (int r = 0; r <= 8; ++r) {
                        WorldPos gPos = new WorldPos (r % 3, 0, (r / 3));
                        GameObject go = volume.CopyPiece (chunkBlockPos, gPos, a_cut);
                        if (go != null) {
                            TranslatedGo tg;
                            tg.go = go;
                            tg.gPos = gPos;
                            a_objects.Add (tg);

                            objectPlaced = true;
                        }
                    }

                    if (block != null) {
                        if (!objectPlaced) {
                            sb.block = new Block (block);
                            a_blocks.Add (sb);
                        }

                        if (a_cut) {
                            switch (block.GetType ().ToString ()) {
                            case "CreVox.BlockAir":
                                List<BlockAir> bAirs = chunk.cData.blockAirs;
                                for (int j = bAirs.Count - 1; j > -1; j--) {
                                    if (bAirs [j].BlockPos.Compare (chunkBlockPos))
                                        bAirs.RemoveAt (j);
                                }
                                break;
                            case "CreVox.BlockHold":
                                List<BlockHold> bHolds = chunk.cData.blockHolds;
                                for (int j = bHolds.Count - 1; j > -1; j--) {
                                    if (bHolds [j].BlockPos.Compare (chunkBlockPos))
                                        bHolds.RemoveAt (j);
                                }
                                break;
                            case "CreVox.Block":
                                List<Block> blocks = chunk.cData.blocks;
                                for (int j = blocks.Count - 1; j > -1; j--) {
                                    if (blocks [j].BlockPos.Compare (chunkBlockPos))
                                        blocks.RemoveAt (j);
                                }
                                break;
                            }
                        }
                    }

                }

                if (block == null) {
                    sb.block = null;
                    a_blocks.Add (sb);
                }
            }
        }

        void TranslateBlock (List<SelectedBlock> a_blocks)
        {
            int translateX = (int)m_translate.x;
            int translateY = (int)m_translate.y;
            int translateZ = (int)m_translate.z;

            int count = a_blocks.Count;
            for (int i = 0; i < count; ++i) {
                Vector3 pos = a_blocks [i].pos;
                Block _block = a_blocks [i].block;

                Block oldBlock = volume.GetBlock ((int)pos.x + translateX, (int)pos.y + translateY, (int)pos.z + translateZ);
                Chunk chunk = volume.GetChunk ((int)pos.x + translateX, (int)pos.y + translateY, (int)pos.z + translateZ);
                if (chunk != null) {
                    WorldPos chunkBlockPos = new WorldPos ((int)pos.x + translateX - chunk.cData.ChunkPos.x,
                                                 (int)pos.y + translateY - chunk.cData.ChunkPos.y,
                                                 (int)pos.z + translateZ - chunk.cData.ChunkPos.z);
                    if (_block != null) {
                        _block.BlockPos = chunkBlockPos;
                        Predicate<BlockAir> sameBlockAir = blockAir => blockAir.BlockPos.Compare (chunkBlockPos);
                        switch (_block.GetType ().ToString ()) {
                        case "CreVox.BlockAir":
                            if (!chunk.cData.blockAirs.Exists (sameBlockAir)) {
                                chunk.cData.blockAirs.Add (_block as BlockAir);
                            }
                            break;
                        case "CreVox.BlockHold":
                            Predicate<BlockHold> sameBlockHold = blockHold => blockHold.BlockPos.Compare (chunkBlockPos);
                            if (!chunk.cData.blockHolds.Exists (sameBlockHold)) {
                                chunk.cData.blockHolds.Add (_block as BlockHold);
                            }
                            break;
                        case "CreVox.Block":
                            Predicate<Block> sameBlock = block => block.BlockPos.Compare (chunkBlockPos);
                            if (chunk.cData.blockAirs.Exists (sameBlockAir)) {
                                BlockAir ba = oldBlock as BlockAir;
                                for (int j = 0; j < 8; j++) {
                                    volume.PlacePiece (ba.BlockPos, new WorldPos (j % 3, 0, (j / 3)), null);
                                }
                            }
                            if (!chunk.cData.blocks.Exists (sameBlock)) {
                                chunk.cData.blocks.Add (_block);
                            }
                            break;
                        }
                    } else if (oldBlock != null) {
                        switch (oldBlock.GetType ().ToString ()) {
                        case "CreVox.BlockAir":
                            List<BlockAir> bAirs = chunk.cData.blockAirs;
                            for (int j = bAirs.Count - 1; j > -1; j--) {
                                if (bAirs [j].BlockPos.Compare (chunkBlockPos))
                                    bAirs.RemoveAt (j);
                            }
                            break;
                        case "CreVox.BlockHold":
                            List<BlockHold> bHolds = chunk.cData.blockHolds;
                            for (int j = bHolds.Count - 1; j > -1; j--) {
                                if (bHolds [j].BlockPos.Compare (chunkBlockPos))
                                    bHolds.RemoveAt (j);
                            }
                            break;
                        case "CreVox.Block":
                            List<Block> blocks = chunk.cData.blocks;
                            for (int j = blocks.Count - 1; j > -1; j--) {
                                if (blocks [j].BlockPos.Compare (chunkBlockPos))
                                    blocks.RemoveAt (j);
                            }
                            break;

                        }
                    }

                    chunk.UpdateChunk ();
                }
            }
        }

        void TranslateObject (List<TranslatedGo> objects)
        {
            int count = objects.Count;
            for (int i = 0; i < count; ++i) {
                TranslatedGo tg = objects [i];

                Vector3 goPos = tg.go.transform.position;
                WorldPos gPos = tg.gPos;

                Vector3 pos = Volume.GetPieceOffset (gPos.x, gPos.z);

                WorldPos bPos;
                bPos.x = (int)((goPos.x - pos.x) / vg.w);
                bPos.y = (int)((goPos.y - pos.y) / vg.h);
                bPos.z = (int)((goPos.z - pos.z) / vg.d);

                volume.RemoveNode (bPos);
            }
            for (int i = 0; i < count; ++i) {
                TranslatedGo tg = objects [i];
                LevelPiece lp = tg.go.GetComponent<LevelPiece> ();
                if (lp != null) {
                    Vector3 goPos = tg.go.transform.position;
                    WorldPos gPos = tg.gPos;

                    Vector3 pos = Volume.GetPieceOffset (gPos.x, gPos.z);

                    WorldPos bPos;
                    bPos.x = Mathf.RoundToInt ((goPos.x - pos.x) / vg.w + m_translate.x);
                    bPos.y = Mathf.RoundToInt ((goPos.y - pos.y) / vg.h + m_translate.y);
                    bPos.z = Mathf.RoundToInt ((goPos.z - pos.z) / vg.d + m_translate.z);
                    volume.PlacePiece (bPos, gPos, lp, false);

                    Chunk chunk = volume.GetChunk (bPos.x, bPos.y, bPos.z);
                    chunk.UpdateChunk ();
                }
            }
        }

        #endregion

        #region SubscribeEvents

        PaletteItem _itemSelected;
        Texture2D _itemPreview;
        LevelPiece _pieceSelected;

        void DrawPieceInspectedGUI ()
        {
            using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
                EditorGUILayout.LabelField ("Piece Edited", EditorStyles.boldLabel);
                string _label2 = "[" + selectedItemID + "] " + volume._itemInspected.name + " (" + volume._itemInspected.GetComponent<LevelPiece> ().GetType ().Name + ") ";

                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    GUILayout.Label (_label2, EditorStyles.miniLabel);
                    if (GUILayout.Button ("Delete", GUILayout.Width (60))) {
                        PaintItem (true);
                        return;
                    }
                }

                if (currentEditMode == EditMode.Item && volume._itemInspected != null) {
                    if (volume._itemInspected.inspectedScript != null) {
                        LevelPieceEditor e = (LevelPieceEditor)(CreateEditor (volume._itemInspected.inspectedScript));
                        BlockItem item = volume.vd.blockItems [selectedItemID];

                        if (e != null)
                            e.OnEditorGUI (ref item);
                        else
                            EditorGUILayout.HelpBox ("Something Wrong...!", MessageType.Info);
                    } else {
                        EditorGUILayout.HelpBox ("Item doesn't have inspectedScript !", MessageType.Info);
                    }
                }
            }
        }

        void SubscribeEvents ()
        {
            PaletteWindow.ItemSelectedEvent += UpdateCurrentPieceInstance;
        }

        void UnsubscribeEvents ()
        {
            PaletteWindow.ItemSelectedEvent -= UpdateCurrentPieceInstance;
        }

        void UpdateCurrentPieceInstance (PaletteItem item, Texture2D preview)
        {
            _itemSelected = item;
            _itemPreview = preview;
            _pieceSelected = item.GetComponent<LevelPiece> ();
            Repaint ();
        }

        #endregion
    }
}