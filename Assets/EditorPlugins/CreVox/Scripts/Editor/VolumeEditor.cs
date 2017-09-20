using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

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

		private void OnEnable ()
		{
            volume = (Volume)target;
            Volume.focusVolume = volume;
			volume.ActiveRuler (true);
			SubscribeEvents ();
		}

		private void OnDisable ()
        {
            Volume.focusVolume = null;
			volume.ActiveRuler (false);
			UnsubscribeEvents ();
		}

		public override void OnInspectorGUI ()
		{
			float buttonW = 80;
			float defLabelWidth = EditorGUIUtility.labelWidth;
			GUI.color = (volume.vd == null) ? Color.red : Color.white;

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				GUI.backgroundColor = Color.white;
				GUILayout.Label ("VolumeData", EditorStyles.boldLabel);
				using (var h = new EditorGUILayout.HorizontalScope ()) {
					if (GUILayout.Button ("Refresh")) {
						UpdateVolume ();
					}
					if (GUILayout.Button ("Calculate BlockHold")) {
                        CalculateBlockHold ();
					}
				}
				using (var h = new EditorGUILayout.HorizontalScope ()) {
					EditorGUI.BeginChangeCheck ();
					volume.vd = (VolumeData)EditorGUILayout.ObjectField (volume.vd, typeof(VolumeData), false);
					if (EditorGUI.EndChangeCheck ()) {
						UpdateVolume ();
                        volume.gameObject.name = volume.vd.name.Replace ("_vData", "");
					}
					if (GUILayout.Button ("Backup", GUILayout.Width (buttonW))) {
						volume.SaveTempWorld ();
					}
				}
				EditorGUILayout.Separator ();
				EditorGUI.BeginChangeCheck ();
                if (volume.vd != null) {
                    if (volume.vd.useFreeChunk) {
                        ChunkData c = volume.vd.freeChunk;
                        using (var h = new EditorGUILayout.HorizontalScope ()) {
                            EditorGUIUtility.labelWidth = 15;
                            float w = Mathf.Ceil (Screen.width - 119) / 3;
                            EditorGUILayout.LabelField ("Chunk Size");
                            c.freeChunkSize.x = EditorGUILayout.IntField ("X", c.freeChunkSize.x, GUILayout.Width (w));
                            c.freeChunkSize.y = EditorGUILayout.IntField ("Y", c.freeChunkSize.y, GUILayout.Width (w));
                            c.freeChunkSize.z = EditorGUILayout.IntField ("Z", c.freeChunkSize.z, GUILayout.Width (w));
                            EditorGUIUtility.labelWidth = defLabelWidth;
                        }
                        if (GUILayout.Button ("Convert to Voxel Chunk")) {
                            volume.vd.ConvertToVoxelChunk ();
                            CalculateBlockHold ();
                        }
                    } else {
                        using (var h = new EditorGUILayout.HorizontalScope ()) {
                            EditorGUIUtility.labelWidth = 15;
                            float w = Mathf.Ceil (Screen.width - 119) / 3;
                            cx = EditorGUILayout.IntField ("X", cx, GUILayout.Width (w));
                            cy = EditorGUILayout.IntField ("Y", cy, GUILayout.Width (w));
                            cz = EditorGUILayout.IntField ("Z", cz, GUILayout.Width (w));
                            EditorGUIUtility.labelWidth = defLabelWidth;
                            if (GUILayout.Button ("Init", GUILayout.Width (buttonW))) {
                                volume.vd = null;
                                volume.Init (cx, cy, cz);
                                WriteVData (volume);
                            }
                        }
                        if (GUILayout.Button ("Convert to FreeSize Chunk")) {
                            volume.vd.ConvertToFreeChunk ();
                            CalculateBlockHold ();
                        }
                    }
                }
				if (EditorGUI.EndChangeCheck ()) {
					EditorUtility.SetDirty (volume.vd);
					EditorUtility.SetDirty (volume);
					volume.UpdateChunks ();
				}
			}

			if (volume.vd != null) {
				EditorGUI.BeginChangeCheck ();
				DrawArtPack ();
				EditorGUI.BeginChangeCheck ();
                if (volume.vm) {
                    VolumeManagerEditor.DrawVLocal (volume.vm);
                }
				if (EditorGUI.EndChangeCheck ()) {
					UpdateVolume ();
					volume.transform.root.BroadcastMessage ("ShowRuler", SendMessageOptions.DontRequireReceiver);
				}
				if (selectedItemID != -1)
					DrawPieceInspectedGUI ();

				if (EditorGUI.EndChangeCheck ()) {
					EditorUtility.SetDirty (volume);
					volume.UpdateChunks ();
				}
			}
		}

		void DrawArtPack ()
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
                    if (volume.vm != null ? volume.vm.saveBackupL : VolumeManager.saveBackup)
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
					volume.vertexMaterial = AssetDatabase.LoadAssetAtPath<Material> (ppath);
					EditorUtility.SetDirty (volume.vd);

					volume.LoadTempWorld ();
				}
				EditorGUIUtility.labelWidth = 120f;
				volume.vd.subArtPack = EditorGUILayout.TextField ("SubArtPack", volume.vd.subArtPack);
				volume.vertexMaterial = (Material)EditorGUILayout.ObjectField (
					new GUIContent ("Volume Material", "Auto Select if Material's name is ArtPack's name + \"_voxel\"")
					, volume.vertexMaterial
					, typeof(Material)
					, false);
			}
		}

        #region Scene GUI
        private void OnSceneGUI ()
        {
            ModeHandler ();

            EventHandler ();

            Handles.BeginGUI ();
            DrawModeGUI ();
            DrawLayerModeGUI ();
            DrawSelectedGUI ();
            Handles.EndGUI ();

        }

		public enum EditMode
		{
			View,
			VoxelLayer,
			Voxel,
			ObjectLayer,
			Object,
			Item
		}
		private EditMode selectedEditMode;
		private EditMode currentEditMode;
		private void ModeHandler ()
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
			default:
				break;
			}
			if (selectedEditMode != currentEditMode) {
				currentEditMode = selectedEditMode;
				volume._itemInspected = null;
				Repaint ();
			}
        }

        private int fixPointY = 0;
        private int fixCutY = 0;
        float ButtonW = 80;
        private void DrawModeGUI ()
        {
            VGlobal vg = VGlobal.GetSetting ();
            List<EditMode> modes = EditorUtils.GetListFromEnum<EditMode> ();
            List<string> modeLabels = new List<string> ();
            foreach (EditMode mode in modes) {
                modeLabels.Add (mode.ToString ());
            }

            GUI.color = volume.YColor;
            using (var a = new GUILayout.AreaScope (new Rect (10f, 10f, modeLabels.Count * ButtonW, 50f), "", EditorStyles.textArea)) {
                GUI.color = Color.white;
                selectedEditMode = (EditMode)GUILayout.Toolbar ((int)currentEditMode, modeLabels.ToArray (), GUILayout.ExpandHeight (true));
                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    EditorGUILayout.LabelField ("Editable Distance", GUILayout.Width (105));
                    EditorGUI.BeginChangeCheck ();
                    vg.editDis = GUILayout.HorizontalSlider (vg.editDis, 99f, 999f);
                    if (EditorGUI.EndChangeCheck ())
                        EditorUtility.SetDirty (vg);
                    EditorGUILayout.LabelField (((int)vg.editDis).ToString (), GUILayout.Width (25));
                    if (selectedEditMode == EditMode.Item)
                        isItemSnap = EditorGUILayout.ToggleLeft ("Snap Item", isItemSnap, GUILayout.Width (ButtonW));
                }
            }
        }

        float blockW = 85f;
        private void DrawLayerModeGUI ()
        {
            int tile = (_pieceSelected == null) ? 3 : 4;
            using (var a = new GUILayout.AreaScope (new Rect (10f, 65f, (blockW + 3f) * tile, 65f), "")) {
                using (var h = new GUILayout.HorizontalScope ()) {
                    bool _hotkey = currentEditMode != EditMode.View && currentEditMode != EditMode.Item;
                    GUILayoutOption[] block = new GUILayoutOption[]{
                        GUILayout.Width (blockW),
//                        GUILayout.MinWidth (blockW),
//                        GUILayout.ExpandWidth (true),
//                        GUILayout.ExpandHeight (true)
                    };

                    GUI.color = volume.YColor;
                    using (var v = new GUILayout.VerticalScope (EditorStyles.textArea, block)) {
                        GUI.color = Color.white;
                        if (GUILayout.Button ("Pointer" + (_hotkey ? "(Q)" : "")))
                            HotkeyFunction ("Q");
                        using (var d = new EditorGUI.DisabledGroupScope (!volume.pointer)) {
                            using (var h2 = new GUILayout.HorizontalScope ()) {
                                GUILayout.Label (volume.pointY.ToString(), "TL Selection H2", GUILayout.Height(0), GUILayout.Width (24));
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
                    using (var v = new GUILayout.VerticalScope (EditorStyles.textArea, block)) {
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
                        using (var v = new GUILayout.VerticalScope (EditorStyles.textArea, block)) {
                            GUI.color = Color.white;
                            if (_pieceSelected != null) {
                                GUILayout.Label (new GUIContent (_itemPreview), GUILayout.Height (63), GUILayout.Width (63));
                                GUILayout.Space (-20);
                                GUILayout.Label (_itemSelected.itemName, GUILayout.Width (blockW));
                            }
                        }
                    }
                }
            }
        }

        private bool m_mappingX = false;
        private bool m_mappingZ = false;
        private Vector3 m_selectedMin;
        private Vector3 m_selectedMax;
        private List<Vector3> m_selectedBlocks = new List<Vector3>();
        private Vector3 m_translate;
        private void DrawSelectedGUI ()
        {
            using (var a = new GUILayout.AreaScope (new Rect (10f, 135f, blockW * 2 + 4, 170f), "")) {
                EditorGUIUtility.wideMode = true;

                GUI.color = volume.YColor;
                using (var v = new GUILayout.VerticalScope (EditorStyles.textArea)) {
                    GUI.color = Color.white;
                    using (var h = new GUILayout.HorizontalScope ()) {
                        EditorGUIUtility.labelWidth = 10;
                        EditorGUILayout.LabelField ("Mirror", EditorStyles.boldLabel);
                        m_mappingX =GUILayout.Toggle (m_mappingX, "X", GUILayout.Width (40));
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
                    m_translate = EditorGUILayout.Vector3Field("Offset", m_translate);
                    using (var h = new GUILayout.HorizontalScope ()) {
                        if (GUILayout.Button("Translate"))
                            HotkeyFunction("Translate");
                        if (GUILayout.Button("Copy"))
                            HotkeyFunction("Copy");
                    }
                }
            }
        }
        #endregion

        #region Draw Marker
		private void EventHandler ()
		{
			if (Event.current.alt) {
				return;
            }

            DrawMarkerSelected();

			if (Event.current.type == EventType.KeyDown) {
                HotkeyFunction (Event.current.keyCode.ToString (), true);
				return;
			} 

			if (currentEditMode != EditMode.View) {
				int button = -1;
				if (currentEditMode != EditMode.Item) {
					button = Event.current.button;
				}
				if (Event.current.type == EventType.MouseUp && Event.current.button < 2) {
					button = Event.current.button;
					if (Event.current.button == 1)
						Event.current.button = 0;
				}
				HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

				switch (currentEditMode) {
				case EditMode.View:
				case EditMode.Item:
					volume.useBox = false;
					break;

				default:
					volume.useBox = true;
					break;
				}

				switch (currentEditMode) {
				case EditMode.Voxel:
					if (button == 0)
						DrawMarker (false);
					else if (button == 1) {
						DrawMarker (true);
					}

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
                    DrawMarkerLayer ();

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
                    DrawMarkerGrid ();

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
					if (Event.current.type == EventType.MouseDown) {
						if (button == 1) {
							Tools.viewTool = ViewTool.None;
							Event.current.Use ();
						}
					}
                    DrawMarkerEdit (ref button);
					break;

				default:
					break;
				}

				if (Event.current.type == EventType.MouseUp) {
					UpdateDirtyChunks ();
				}
			}
		}

		private void DrawMarker (bool isErase)
		{
			RaycastHit hit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
			VGlobal vg = VGlobal.GetSetting ();
			bool isHit = Physics.Raycast (worldRay, out hit, vg.editDis, _mask);

			if (isHit && !isErase && hit.collider.GetComponentInParent<Volume> () == volume) {
				RaycastHit hitFix = hit;
				WorldPos pos = EditTerrain.GetBlockPos (hitFix, isErase ? false : true);
				hitFix.point = new Vector3 (pos.x * vg.w, pos.y * vg.h, pos.z * vg.d);

				Handles.DrawLine (hit.point, hit.point + hit.normal);
				if (hit.collider.gameObject.tag == PathCollect.rularTag) {
					hit.normal = Vector3.zero;
				}
				BoxCursorUtils.UpdateBox (volume.box, hitFix.point, hit.normal);
				SceneView.RepaintAll ();
			} else {
				volume.useBox = false;
				SceneView.RepaintAll ();
			}
		}

        private void DrawMarkerLayer ()
		{
			RaycastHit hit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer ("EditorLevel");
			VGlobal vg = VGlobal.GetSetting ();
			bool isHit = Physics.Raycast (worldRay, out hit, vg.editDis, _mask);

			if (isHit && hit.collider.GetComponentInParent<Volume> () == volume) {
				RaycastHit hitFix = hit;
				WorldPos pos = EditTerrain.GetBlockPos (hitFix, false);
				hitFix.point = new Vector3 (pos.x * vg.w, pos.y * vg.h, pos.z * vg.d);
				
				Handles.RectangleCap (0, hitFix.point + new Vector3 (0, vg.h/2, 0), Quaternion.Euler (90, 0, 0), vg.w/2);
				Handles.DrawLine (hit.point, hitFix.point);
				volume.useBox = true;
				BoxCursorUtils.UpdateBox (volume.box, hitFix.point, Vector3.zero);
			} else {
				volume.useBox = false;
			}
			SceneView.RepaintAll ();
		}

        private void DrawMarkerGrid ()
		{
			if (_pieceSelected == null)
				return;
			bool isNotLayer = (currentEditMode != EditMode.ObjectLayer);
			RaycastHit hit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = isNotLayer ? 1 << LayerMask.NameToLayer ("Editor") : 1 << LayerMask.NameToLayer ("EditorLevel");
			VGlobal vg = VGlobal.GetSetting ();
			bool isHit = Physics.Raycast (worldRay, out hit, (int)vg.editDis, _mask);

			if (isHit && hit.collider.GetComponentInParent<Volume> () == volume) {
				if (hit.normal.y <= 0) {
					volume.useBox = false;
					return;
				} else
					volume.useBox = true;

				RaycastHit hitFix = hit;
				WorldPos pos = EditTerrain.GetBlockPos (hitFix, isNotLayer);
				hitFix.point = new Vector3 (pos.x * vg.w, pos.y * vg.h, pos.z * vg.d);

				WorldPos gPos = EditTerrain.GetGridPos (hit.point);
				gPos.y = isNotLayer ? 0 : (int)vg.h;

				float gx = pos.x * vg.w + gPos.x + ((pos.x < 0) ? 1 : -1);
				float gy = pos.y * vg.h + gPos.y - vg.h / 2;
				float gz = pos.z * vg.d + gPos.z + ((pos.z < 0) ? 1 : -1);

                LevelPiece.PivotType pivot = _pieceSelected.pivot;
				if (CheckPlaceable ((int)gPos.x, (int)gPos.z, pivot)) {
					Handles.color = Color.red;
					Handles.RectangleCap (0, new Vector3 (gx, gy, gz), Quaternion.Euler (90, 0, 0), 0.5f);
					Handles.color = Color.white;
				}

				Handles.color = Color.white;
				Handles.lighting = true;
				Handles.RectangleCap (0, hitFix.point - new Vector3 (0, vg.h/2 - gPos.y, 0), Quaternion.Euler (90, 0, 0), vg.w/2);
				Handles.DrawLine (hit.point, hitFix.point);

				volume.useBox = true;
				BoxCursorUtils.UpdateBox (volume.box, hitFix.point, Vector3.zero);
				SceneView.RepaintAll ();
			} else {
				volume.useBox = false;
				SceneView.RepaintAll ();
			}
		}

		private int workItemId = -1; 
		private int selectedItemID = -1;
		private bool isItemSnap = false;
        private void DrawMarkerEdit (ref int button)
		{
			VGlobal vg = VGlobal.GetSetting ();
			Matrix4x4 defMatrix = Handles.matrix;
			Color defColor = Handles.color;
			Quaternion facingCamera;
			bool selected = false;
			string log = "";

			// if no item selected, draw handle 'can paint item'.
			if (selectedItemID == -1) {
				RaycastHit hit;
				Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
				LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
				bool isHit = Physics.Raycast (worldRay, out hit, VGlobal.GetSetting ().editDis, _mask);
				if (isHit && hit.collider.GetComponentInParent<Volume> () == volume && hit.normal.y > 0) {
					Handles.color = new Color (0f / 255f, 202f / 255f, 255f / 255f, 0.3f);
					Handles.DrawSolidDisc (hit.point, hit.normal, 0.5f);
				}
			}
			// detect item select.
			for (int i = 0; i < volume.vd.blockItems.Count; i++) {
				BlockItem blockItem = volume.vd.blockItems [i];
				GameObject itemObj = volume.GetItemNode (blockItem);
				if (itemObj != null) {
					Transform ItemNode = itemObj.transform;
					Vector3 pos = ItemNode.position;
					Vector3 handlePos = isItemSnap ? pos + new Vector3 (0, vg.h/2, 0) : pos;
					// draw move & rotate handle.
					if (selectedItemID == i) {
						handlePos = Handles.DoPositionHandle (handlePos, ItemNode.rotation);

						if (isItemSnap) {
							float fixedX = Mathf.Round (handlePos.x*2)/2;
							float fixedY = Mathf.Round (handlePos.y / vg.h) * vg.h - (vg.h/2 - 0.01f);
							float fixedZ = Mathf.Round (handlePos.z*2)/2;
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
					// draw button.
					float handleSize = HandleUtility.GetHandleSize (pos) * 0.15f;
					Handles.color = new Color (0f / 255f, 202f / 255f, 255f / 255f, 0.1f);
					facingCamera = Camera.current.transform.rotation * Quaternion.Euler (0, 0, 180);
					selected = Handles.Button (pos, facingCamera, handleSize, handleSize, Handles.SphereCap);
					Handles.color = defColor;
					//compare selectedItemID.
					if (selected && button > -1 && button < 2) {
						log += "[Before Button]Btn:<b>" + button + "</b> Wrk:<b>" + workItemId + "</b> Slt:<b>" + selectedItemID + "</b>";
						workItemId = i;
						break;
					}
				}
			}
			// check mouse event & select item
			switch (button) {
			case 0:
				// check if is selected item.
				if (workItemId != -1) {
					// toggle item selection.
					selectedItemID = (selectedItemID == workItemId) ? -1 : workItemId;
				} else if (!selected && selectedItemID == -1) {
					// add new item.
					int oldcount = volume.vd.blockItems.Count;
					PaintItem (false);
					if (volume.vd.blockItems.Count != oldcount)
						selectedItemID = volume.vd.blockItems.Count - 1;
				}
				UpdateInapectedItem (selectedItemID);
				log += "\n[After Button]Btn:<b>" + button + "</b> Wrk:<b>" + workItemId + "</b> Slt:<b>" + selectedItemID + "</b>";
				break;
			case 1:
				if (workItemId != -1 && selectedItemID == workItemId) {
					// delette selected item
					PaintItem (true);
					selectedItemID = -1;
				}
				UpdateInapectedItem (selectedItemID);
				log += "\n[After Button]Btn:<b>" + button + "</b> Wrk:<b>" + workItemId + "</b> Slt:<b>" + selectedItemID + "</b>";
				break;
			default :
				break;
			}
			workItemId = -1;
			if (log.Length > 0) Debug.Log (log);
			Handles.matrix = defMatrix;
			SceneView.RepaintAll ();
        }

        void DrawMarkerSelected()
        {
            Color old = Handles.color;
            Handles.color = Color.red;
            VGlobal vg = VGlobal.GetSetting ();
            float width = vg.w;
            float height = vg.h;
            float depth = vg.d;

            int count = m_selectedBlocks.Count;
            for (int i = 0; i < count; ++i)
            {
                float x = m_selectedBlocks[i].x * width;
                float y = m_selectedBlocks[i].y * height;
                float z = m_selectedBlocks[i].z * depth;
                Handles.DrawWireCube (new Vector3(x, y, z), new Vector3 (width, height, depth));
            }

            Handles.color = old;
        }
		#endregion

		#region Paint
		private void Paint (bool isErase)
		{
			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
			VGlobal vg = VGlobal.GetSetting ();
			bool isHit = Physics.Raycast (worldRay, out gHit, vg.editDis, _mask);
			WorldPos pos;

			if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
				gHit.point = volume.transform.InverseTransformPoint (gHit.point);
				gHit.normal = volume.transform.InverseTransformDirection (gHit.normal);
				pos = EditTerrain.GetBlockPos (gHit, isErase ? false : true);

				//volume.SetBlock (pos.x, pos.y, pos.z, isErase ? null : new Block ());
                VolumeHelper.MirrorPosition(volume,
                    pos,
                    new WorldPos(cx, cy, cz),
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

		private void PaintLayer (bool isErase)
		{
			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer ("EditorLevel");
			VGlobal vg = VGlobal.GetSetting ();
			bool isHit = Physics.Raycast (worldRay, out gHit, vg.editDis, _mask);
			WorldPos pos;

			if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
				gHit.point = gHit.point + new Vector3 (0f, -vg.h, 0f);
				gHit.point = volume.transform.InverseTransformPoint (gHit.point);
				pos = EditTerrain.GetBlockPos (gHit, true);

				//volume.SetBlock (pos.x, pos.y, pos.z, isErase ? null : new Block ());
                VolumeHelper.MirrorPosition(volume, 
                    pos, 
                    new WorldPos(cx, cy, cz), 
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

		private void PaintPiece (bool isErase)
		{
			if (_pieceSelected == null)
				return;
			
			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = (currentEditMode == EditMode.Object) ? 1 << LayerMask.NameToLayer ("Editor") : 1 << LayerMask.NameToLayer ("EditorLevel");
			VGlobal vg = VGlobal.GetSetting ();
			bool isHit = Physics.Raycast (worldRay, out gHit, vg.editDis, _mask);

			if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
				if (gHit.normal.y <= 0)
					return;

				gHit.point = volume.transform.InverseTransformPoint (gHit.point);
				WorldPos bPos = EditTerrain.GetBlockPos (gHit, (currentEditMode == EditMode.Object));
				WorldPos gPos = EditTerrain.GetGridPos (gHit.point);
				gPos.y = 0;
                
				if (CheckPlaceable(gPos.x, gPos.z, _pieceSelected.pivot)) {

                    VolumeHelper.Mirror(volume,
                        bPos,
                        gPos,
                        new WorldPos(cx, cy, cz),
                        isErase ? null : _pieceSelected,
                        m_mappingX,
                        m_mappingZ);
                    
                    Chunk chunk = volume.GetChunk(bPos.x, bPos.y, bPos.z);
                    chunk.UpdateChunk ();
					EditorUtility.SetDirty (volume.vd);
					SceneView.RepaintAll ();
				}
			}
		}

		private void PaintItem (bool isErase)
		{
			if (_pieceSelected == null)
				return;
			
			if (isErase) {
				volume.PlaceItem (workItemId, null);
			} else {
				RaycastHit gHit;
				Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
				LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
				VGlobal vg = VGlobal.GetSetting ();
				bool isHit = Physics.Raycast (worldRay, out gHit, vg.editDis, _mask);

				if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
					if (gHit.normal.y <= 0)
						return;

					gHit.point = volume.transform.InverseTransformPoint (gHit.point);
					volume.PlaceItem (volume.vd.blockItems.Count, _pieceSelected,gHit.point);
					SceneView.RepaintAll ();
				}
			}
			EditorUtility.SetDirty (volume.vd);
		}
		#endregion

		#region Tools
        private void HotkeyFunction (string funcKey = "", bool isKeyEvent = false)
        {
            int _index = (int)currentEditMode;
            int _count = System.Enum.GetValues (typeof(EditMode)).Length - 1;
            bool _hotkey = isKeyEvent ? (currentEditMode != EditMode.View && currentEditMode != EditMode.Item) : true;

            switch (funcKey) {
            case "A":
                if (_index == 0)
                    selectedEditMode = (EditMode)_count;
                else
                    selectedEditMode = (EditMode)(_index - 1);
                Repaint ();
                break;

            case "D":
                if (_index == _count)
                    selectedEditMode = (EditMode)0;
                else
                    selectedEditMode = (EditMode)(_index + 1);
                Repaint ();
                break;

            case "Q":
                if (_hotkey) {
                    volume.pointer = !volume.pointer;
                    fixPointY = volume.pointY;
                    volume.ChangePointY (fixPointY);
                }
                break;

            case "W":
                if (_hotkey) {
                    fixPointY = volume.pointY + 1;
                    volume.ChangePointY (fixPointY);
                    fixPointY = volume.pointY;
                }
                break;

            case "S":
                if (_hotkey) {
                    fixPointY = volume.pointY - 1;
                    volume.ChangePointY (fixPointY);
                    fixPointY = volume.pointY;
                }
                break;

            case "E":
                if (_hotkey) {
                    volume.cuter = !volume.cuter;
                    fixCutY = volume.cutY;
                    volume.ChangeCutY (fixCutY);
                }
                break;

            case "R":
                if (_hotkey) {
                    fixCutY = volume.cutY + 1;
                    volume.ChangeCutY (fixCutY);
                    fixCutY = volume.cutY;
                }
                break;

            case "F":
                if (_hotkey) {
                    fixCutY = volume.cutY - 1;
                    volume.ChangeCutY (fixCutY);
                    fixCutY = volume.cutY;
                }
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
            UpdateVolume ();
            volume.ArtPack = apOld;
            UpdateVolume ();
            EditorUtility.SetDirty (volume);
        }

        private void UpdateVolume()
		{
			selectedItemID = -1;
			volume.BuildVolume ();
			SceneView.RepaintAll ();
		}

		private void UpdateInapectedItem (int id)
		{
			GameObject ItemNode = (id > -1) ? volume.GetItemNode (volume.vd.blockItems [id]) : null;
            volume._itemInspected = (ItemNode != null) ? ItemNode.GetComponent<PaletteItem> () : null;
			EditorUtility.SetDirty (volume);
		}

		private void UpdateDirtyChunks ()
		{
			foreach (KeyValuePair<WorldPos, Chunk> c in dirtyChunks) {
				c.Value.UpdateMeshCollider ();
			}
			dirtyChunks.Clear ();
		}

		private bool CheckPlaceable (int x, int z, LevelPiece.PivotType pType)
		{
			if (pType == LevelPiece.PivotType.Grid)
				return true;
			else if (pType == LevelPiece.PivotType.Center && (x * z) == 1)
				return true;
			else if (pType == LevelPiece.PivotType.Vertex && (x + z) % 2 == 0 && x * z != 1)
				return true;
			else if (pType == LevelPiece.PivotType.Edge && (Mathf.Abs(x) + Mathf.Abs(z)) % 2 == 1)
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

			VolumeData vd = ScriptableObject.CreateInstance<VolumeData> ();
			UnityEditor.AssetDatabase.CreateAsset (vd, sPath);
			UnityEditor.AssetDatabase.Refresh();

			_volume.vd = vd;
			vd.chunkX = _volume.chunkX;
			vd.chunkY = _volume.chunkY;
			vd.chunkZ = _volume.chunkZ;
			vd.chunkDatas = new List<ChunkData> ();
			vd.blockItems = _volume.vd.blockItems;
			foreach (Chunk _chunk in _volume.chunks.Values) {
				WorldPos _pos = _chunk.cData.ChunkPos;

				ChunkData newChunkData = new ChunkData ();
				newChunkData.ChunkPos = _pos;
				newChunkData.blocks = _chunk.cData.blocks;
				newChunkData.blockAirs = _chunk.cData.blockAirs;
				newChunkData.blockHolds = _chunk.cData.blockHolds;

				vd.chunkDatas.Add (newChunkData);
				_chunk.cData = newChunkData;
			}
		}

        public void Translate(bool a_cut = true)
        {
            List<SelectedBlock> translatedBlocks = new List<SelectedBlock> ();
            List<TranslatedGo> translatedObjects = new List<TranslatedGo> ();

            CopyObjectAndBlock(ref translatedObjects, ref translatedBlocks, a_cut);

            TranslateBlock(translatedBlocks);
            TranslateObject(translatedObjects);
        }

        void CopyObjectAndBlock(ref List<TranslatedGo> a_objects, ref  List<SelectedBlock> a_blocks, bool a_cut)
        {
            int count = m_selectedBlocks.Count;
            for (int i = 0; i < count; ++i)
            {
                Vector3 pos = m_selectedBlocks[i];
                SelectedBlock sb;
                sb.pos = pos;

                Block block = volume.GetBlock( (int)pos.x, (int)pos.y, (int)pos.z);
                Chunk chunk = volume.GetChunk ( (int)pos.x, (int)pos.y, (int)pos.z);

                if (chunk != null)
                {
                    WorldPos chunkBlockPos = new WorldPos ((int)pos.x, (int)pos.y, (int)pos.z);
                    bool objectPlaced = false;
                    for (int r = 0; r <= 8; ++r) {
                        WorldPos gPos = new WorldPos (r % 3, 0, (int)(r / 3));
                        GameObject go = volume.CopyPiece(chunkBlockPos, gPos, a_cut);
                        if (go != null)
                        {
                            TranslatedGo tg;
                            tg.go = go;
                            tg.gPos = gPos;
                            a_objects.Add(tg);

                            objectPlaced = true;
                        }
                    }

                    if (block != null)
                    {
                        if (objectPlaced == false)
                        {
                            sb.block = new Block (block);
                            a_blocks.Add(sb);
                        }

                        if (a_cut)
                        {
                            switch (block.GetType().ToString())
                            {
                            case "CreVox.BlockAir":
                                List<BlockAir> bAirs = chunk.cData.blockAirs;
                                for (int j = bAirs.Count - 1; j > -1; j--)
                                {
                                    if (bAirs[j].BlockPos.Compare(chunkBlockPos))
                                        bAirs.RemoveAt(j);
                                }
                                break;
                            case "CreVox.BlockHold":
                                List<BlockHold> bHolds = chunk.cData.blockHolds;
                                for (int j = bHolds.Count - 1; j > -1; j--)
                                {
                                    if (bHolds[j].BlockPos.Compare(chunkBlockPos))
                                        bHolds.RemoveAt(j);
                                }
                                break;
                            case "CreVox.Block":
                                List<Block> blocks = chunk.cData.blocks;
                                for (int j = blocks.Count - 1; j > -1; j--)
                                {
                                    if (blocks[j].BlockPos.Compare(chunkBlockPos))
                                        blocks.RemoveAt(j);
                                }
                                break;
                            }
                        }
                    }

                }

                if (block == null)
                {
                    sb.block = null;
                    a_blocks.Add(sb);
                }
            }
        }

        void TranslateBlock(List<SelectedBlock> a_blocks)
        {
            int translateX = (int)m_translate.x;
            int translateY = (int)m_translate.y;
            int translateZ = (int)m_translate.z;

            int count = a_blocks.Count;
            for (int i = 0; i < count; ++i)
            {
                Vector3 pos = a_blocks[i].pos;
                Block _block = a_blocks[i].block;

                Block oldBlock = volume.GetBlock((int)pos.x + translateX, (int)pos.y + translateY, (int)pos.z + translateZ);
                Chunk chunk = volume.GetChunk((int)pos.x + translateX, (int)pos.y + translateY, (int)pos.z + translateZ);
                if (chunk != null)
                {
                    WorldPos chunkBlockPos = new WorldPos ((int)pos.x + translateX - chunk.cData.ChunkPos.x,
                        (int)pos.y + translateY - chunk.cData.ChunkPos.y,
                        (int)pos.z + translateZ - chunk.cData.ChunkPos.z);
                    if (_block != null)
                    {
                        _block.BlockPos = chunkBlockPos;
                        Predicate<BlockAir> sameBlockAir = delegate(BlockAir b) {
                            return b.BlockPos.Compare(chunkBlockPos);
                        };
                        switch (_block.GetType().ToString())
                        {
                        case "CreVox.BlockAir":
                            if (!chunk.cData.blockAirs.Exists(sameBlockAir))
                            {
                                chunk.cData.blockAirs.Add(_block as BlockAir);
                            }
                            break;
                        case "CreVox.BlockHold":
                            Predicate<BlockHold> sameBlockHold = delegate(BlockHold b) {
                                return b.BlockPos.Compare(chunkBlockPos);
                            };
                            if (!chunk.cData.blockHolds.Exists(sameBlockHold))
                            {
                                chunk.cData.blockHolds.Add(_block as BlockHold);
                            }
                            break;
                        case "CreVox.Block":
                            Predicate<Block> sameBlock = delegate(Block b) {
                                return b.BlockPos.Compare(chunkBlockPos);
                            };
                            if (chunk.cData.blockAirs.Exists(sameBlockAir))
                            {
                                BlockAir ba = oldBlock as BlockAir;
                                for (int j = 0; j < 8; j++)
                                {
                                    volume.PlacePiece(ba.BlockPos, new WorldPos (j % 3, 0, (int)(j / 3)), null);
                                }
                            }
                            if (!chunk.cData.blocks.Exists(sameBlock))
                            {
                                chunk.cData.blocks.Add(_block);
                            }
                            break;
                        }
                    }
                    else if (oldBlock != null)
                    {
                        switch (oldBlock.GetType().ToString())
                        {
                        case "CreVox.BlockAir":
                            List<BlockAir> bAirs = chunk.cData.blockAirs;
                            for (int j = bAirs.Count - 1; j > -1; j--)
                            {
                                if (bAirs[j].BlockPos.Compare(chunkBlockPos))
                                    bAirs.RemoveAt(j);
                            }
                            break;
                        case "CreVox.BlockHold":
                            List<BlockHold> bHolds = chunk.cData.blockHolds;
                            for (int j = bHolds.Count - 1; j > -1; j--)
                            {
                                if (bHolds[j].BlockPos.Compare(chunkBlockPos))
                                    bHolds.RemoveAt(j);
                            }
                            break;
                        case "CreVox.Block":
                            List<Block> blocks = chunk.cData.blocks;
                            for (int j = blocks.Count - 1; j > -1; j--)
                            {
                                if (blocks[j].BlockPos.Compare(chunkBlockPos))
                                    blocks.RemoveAt(j);
                            }
                            break;

                        }
                    }

                    chunk.UpdateChunk ();
                }
            }
        }

        void TranslateObject(List<TranslatedGo> objects)
        {
            int count = objects.Count;
            for (int i = 0; i < count; ++i)
            {
                TranslatedGo tg = objects[i];

                Vector3 goPos = tg.go.transform.position;
                WorldPos gPos = tg.gPos;

                Vector3 pos = Volume.GetPieceOffset(gPos.x, gPos.z);
                VGlobal vg = VGlobal.GetSetting();

                WorldPos bPos;
                bPos.x = (int)((goPos.x - pos.x) / vg.w);
                bPos.y = (int)((goPos.y - pos.y) / vg.h);
                bPos.z = (int)((goPos.z - pos.z) / vg.d);

                volume.RemoveNode (bPos);
            }
            for (int i = 0; i < count; ++i)
            {
                TranslatedGo tg = objects[i];
                LevelPiece lp = tg.go.GetComponent<LevelPiece>();
                if (lp != null)
                {
                    Vector3 goPos = tg.go.transform.position;
                    WorldPos gPos = tg.gPos;

                    Vector3 pos = Volume.GetPieceOffset(gPos.x, gPos.z);
                    VGlobal vg = VGlobal.GetSetting();

                    WorldPos bPos;
                    bPos.x = Mathf.RoundToInt((goPos.x - pos.x) / vg.w + m_translate.x);
                    bPos.y = Mathf.RoundToInt((goPos.y - pos.y) / vg.h + m_translate.y);
                    bPos.z = Mathf.RoundToInt((goPos.z - pos.z) / vg.d + m_translate.z);
                    volume.PlacePiece(bPos, gPos, lp, false);

                    Chunk chunk = volume.GetChunk(bPos.x, bPos.y, bPos.z);
                    chunk.UpdateChunk ();
                }
            }
        }
		#endregion

		#region SubscribeEvents
//        private PaletteItem _itemInspected;
        private PaletteItem _itemSelected;
        private Texture2D _itemPreview;
        private LevelPiece _pieceSelected;

		private void DrawPieceInspectedGUI()
		{
			if (currentEditMode != EditMode.Item)
				return;

            if (volume._itemInspected != null) {
				using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
					using (var h = new EditorGUILayout.HorizontalScope ()) {
						string _label = "Piece Edited [" + selectedItemID + "]";
						EditorGUILayout.LabelField (_label, EditorStyles.boldLabel, GUILayout.Width (110));
                        string _label2 = "(" + volume._itemInspected.GetComponent<LevelPiece> ().GetType ().Name + ") " + volume._itemInspected.name;
						EditorGUILayout.LabelField (_label2,EditorStyles.miniLabel);
					}
                    if (volume._itemInspected.inspectedScript != null) {
                        LevelPieceEditor e = (LevelPieceEditor)(Editor.CreateEditor (volume._itemInspected.inspectedScript));
						BlockItem item = volume.vd.blockItems [selectedItemID];

						if (e != null)
							e.OnEditorGUI (ref item);
					} else {
						EditorGUILayout.HelpBox ("Item doesn't have inspectedScript !", MessageType.Info);
					}
				}
			}
		}

		private void SubscribeEvents ()
		{
			PaletteWindow.ItemSelectedEvent += new PaletteWindow.itemSelectedDelegate (UpdateCurrentPieceInstance);
		}

		private void UnsubscribeEvents ()
		{
			PaletteWindow.ItemSelectedEvent -= new PaletteWindow.itemSelectedDelegate (UpdateCurrentPieceInstance);
		}

		private void UpdateCurrentPieceInstance (PaletteItem item, Texture2D preview)
		{
			_itemSelected = item;
			_itemPreview = preview;
			_pieceSelected = (LevelPiece)item.GetComponent<LevelPiece> ();
			Repaint ();
		}
		#endregion
	}
}