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

		private void OnEnable ()
		{
			volume = (Volume)target;
			volume.ActiveRuler (true);
//			volume.BuildVolume ();
			SubscribeEvents ();
		}

		private void OnDisable ()
		{
			if (!VGlobal.GetSetting().debugRuler)
				volume.ActiveRuler (false);
			UnsubscribeEvents ();
		}

		public override void OnInspectorGUI ()
		{
			float buttonW = 120;
			float defLabelWidth = EditorGUIUtility.labelWidth;
			VGlobal vg = VGlobal.GetSetting ();
			GUI.color = Color.white;

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				GUI.backgroundColor = Color.white;
				GUILayout.Label ("VolumeData", EditorStyles.boldLabel);
				if (GUILayout.Button ("Refresh")) {
					UpdateVolume ();
				}
				using (var h = new EditorGUILayout.HorizontalScope ()) {
					EditorGUI.BeginChangeCheck ();
					volume.vd = (VolumeData)EditorGUILayout.ObjectField (volume.vd, typeof(VolumeData), false);
					if (EditorGUI.EndChangeCheck ()) {
						UpdateVolume ();
					}
					if (GUILayout.Button ("Backup", GUILayout.Width (buttonW))) {
						volume.SaveTempWorld ();
					}
				}
				EditorGUILayout.Separator ();
				using (var h = new EditorGUILayout.HorizontalScope ()) {
					EditorGUIUtility.labelWidth = 15;
					cx = EditorGUILayout.IntField ("X", cx);
					cy = EditorGUILayout.IntField ("Y", cy);
					cz = EditorGUILayout.IntField ("Z", cz);
					EditorGUIUtility.labelWidth = defLabelWidth;
				}
				if (GUILayout.Button ("Init")) {
					volume.vd = null;
					volume.Init (cx, cy, cz);
					WriteVData (volume);
				}
				if (GUILayout.Button ("Calculate BlockHold")) {
					foreach (ChunkData bh in volume.vd.chunkDatas) {
						bh.blockHolds.Clear ();
					}
					EditorUtility.SetDirty (volume);
					UpdateVolume ();
					volume.UpdateChunks ();
				}
			}

			EditorGUI.BeginChangeCheck ();
			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				GUILayout.Label ("ArtPack", EditorStyles.boldLabel);
				EditorGUILayout.HelpBox ((volume.ArtPack != null) ? (volume.ArtPack + volume.vd.subArtPack) : "(none.)", MessageType.Info, true);
				if (GUILayout.Button ("Set")) {
					string ppath = EditorUtility.OpenFolderPanel (
						               "選擇場景風格元件包的目錄位置",
						               Application.dataPath + PathCollect.resourcesPath.Substring (6) + PathCollect.artPack,
						               ""
					               );
					if (vg.saveBackup)
						volume.SaveTempWorld ();
					
					string artPackName = ppath.Substring (ppath.LastIndexOf ("/") + 1);
					if (artPackName.Length == 4) {
						volume.vd.subArtPack = ppath.Substring (ppath.Length - 1);
						ppath = ppath.Remove (ppath.Length - 1);
						artPackName = artPackName.Remove (3);
					} else {
						volume.vd.subArtPack = "";
					}
					
					volume.ArtPack = PathCollect.artPack + "/" + artPackName;
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
			EditorGUI.BeginChangeCheck ();
			DrawVGlobal ();
			if (EditorGUI.EndChangeCheck ()) {
				EditorUtility.SetDirty (vg);
				UpdateVolume ();
				if (!UnityEditor.EditorApplication.isPlaying) {
					volume.transform.root.BroadcastMessage ("ShowRuler", SendMessageOptions.DontRequireReceiver);
				}
			}
			if(selectedItemID != -1)
			DrawPieceInspectedGUI ();

			if (EditorGUI.EndChangeCheck()) {
				EditorUtility.SetDirty (volume);
				volume.UpdateChunks ();
			}
		}

		public static void DrawVGlobal ()
		{
			VGlobal vg = VGlobal.GetSetting ();
			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				EditorGUILayout.LabelField ("Global Setting", EditorStyles.boldLabel);
				vg.saveBackup = EditorGUILayout.ToggleLeft ("Auto Backup File(" + vg.saveBackup + ")", vg.saveBackup);
				vg.volumeShowArtPack = EditorGUILayout.ToggleLeft ("Volume Show ArtPack(" + vg.volumeShowArtPack + ")", vg.volumeShowArtPack);
				vg.Generation = EditorGUILayout.ToggleLeft ("Runtime Generation(" + vg.Generation + ")", vg.Generation);
				vg.debugRuler = EditorGUILayout.ToggleLeft ("Show Ruler(" + vg.debugRuler + ")", vg.debugRuler);
			}
		}

		#region Scene GUI
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

		private void OnSceneGUI ()
		{
			if (!EditorApplication.isPlaying) {
				ModeHandler ();
				DrawModeGUI ();
				EventHandler ();
			}
		}

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
				_itemInspected = null;
				Repaint ();
			}
		}

		private void DrawModeGUI ()
		{
			VGlobal vg = VGlobal.GetSetting ();
			List<EditMode> modes = EditorUtils.GetListFromEnum<EditMode> ();
			List<string> modeLabels = new List<string> ();
			foreach (EditMode mode in modes) {
				modeLabels.Add (mode.ToString ());
			}
			float ButtonW = 80;

			Handles.BeginGUI ();
			GUI.color = new Color (volume.YColor.r, volume.YColor.g, volume.YColor.b, 1.0f);
			GUILayout.BeginArea (new Rect (10f, 10f, modeLabels.Count * ButtonW, 50f), "", EditorStyles.textArea);
			GUI.color = Color.white;
			selectedEditMode = (EditMode)GUILayout.Toolbar ((int)currentEditMode, modeLabels.ToArray (), GUILayout.ExpandHeight (true));
			GUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Editable Distance", GUILayout.Width (105));
			EditorGUI.BeginChangeCheck ();
			vg.editDis = GUILayout.HorizontalSlider (vg.editDis, 99f, 999f);
			if (EditorGUI.EndChangeCheck ())
				EditorUtility.SetDirty (vg);
			EditorGUILayout.LabelField (((int)vg.editDis).ToString (), GUILayout.Width (25));
			if (selectedEditMode == EditMode.Item)
				isItemSnap = EditorGUILayout.ToggleLeft ("Snap Item", isItemSnap, GUILayout.Width (ButtonW));
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();
			
			DrawLayerModeGUI ();
			Handles.EndGUI ();
		}

		private void EventHandler ()
		{
			if (Event.current.alt) {
				return;
			}

			if (Event.current.type == EventType.KeyDown) {
				HotkeyFunction (Event.current.keyCode.ToString ());
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
					DrawLayerMarker ();

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
					DrawGridMarker ();

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
					DrawEditMarker (ref button);
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

		private void DrawLayerMarker ()
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
				
				Handles.RectangleCap (0, hitFix.point + new Vector3 (0, vg.hh, 0), Quaternion.Euler (90, 0, 0), vg.hw);
				Handles.DrawLine (hit.point, hitFix.point);
				volume.useBox = true;
				BoxCursorUtils.UpdateBox (volume.box, hitFix.point, Vector3.zero);
			} else {
				volume.useBox = false;
			}
			SceneView.RepaintAll ();
		}

		private void DrawGridMarker ()
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
				float gy = pos.y * vg.h + gPos.y - vg.hh;
				float gz = pos.z * vg.d + gPos.z + ((pos.z < 0) ? 1 : -1);

                LevelPiece.PivotType pivot = _pieceSelected.pivot;
				if (CheckPlaceable ((int)gPos.x, (int)gPos.z, pivot)) {
					Handles.color = Color.red;
					Handles.RectangleCap (0, new Vector3 (gx, gy, gz), Quaternion.Euler (90, 0, 0), 0.5f);
					Handles.color = Color.white;
				}

				Handles.color = Color.white;
				Handles.lighting = true;
				Handles.RectangleCap (0, hitFix.point - new Vector3 (0, vg.hh - gPos.y, 0), Quaternion.Euler (90, 0, 0), vg.hw);
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
		private void DrawEditMarker (ref int button)
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
					Vector3 handlePos = isItemSnap ? pos + new Vector3 (0, vg.hh, 0) : pos;
					// draw move & rotate handle.
					if (selectedItemID == i) {
						handlePos = Handles.DoPositionHandle (handlePos, ItemNode.rotation);

						if (isItemSnap) {
							float fixedX = Mathf.Round (handlePos.x);
							float fixedY = Mathf.Round (handlePos.y / vg.h) * vg.h - (vg.hh - 0.01f);
							float fixedZ = Mathf.Round (handlePos.z);
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
		#endregion

		#region LayerControl
		private int fixPointY = 0;
		private int fixCutY = 0;

		private void DrawLayerModeGUI ()
		{
			GUI.color = new Color (volume.YColor.r, volume.YColor.g, volume.YColor.b, 1.0f);
			float bwidth = 70f;
			float tile = (_pieceSelected == null) ? 2f : 3f;
			using (var a = new GUILayout.AreaScope (new Rect (10f, 65f, (bwidth + 5f) * tile, 85f), "", EditorStyles.textArea)) {
				using (var h = new GUILayout.HorizontalScope ()) {
					GUI.color = Color.white;
					using (var v = new GUILayout.VerticalScope ()) {
						if (GUILayout.Button ("Pointer(Q)", GUILayout.Width (bwidth)))
							HotkeyFunction ("Q");
						GUI.color = volume.pointer ? Color.white : Color.gray;
						EditorGUILayout.LabelField ("Y: " + volume.pointY, EditorStyles.textArea, GUILayout.Width (bwidth));
						if (GUILayout.Button ("▲(W)", GUILayout.Width (45)))
							HotkeyFunction ("W");
						if (GUILayout.Button ("▼(S)", GUILayout.Width (45)))
							HotkeyFunction ("S");
						GUI.color = Color.white;
					}

					using (var v = new GUILayout.VerticalScope ()) {
						if (GUILayout.Button ("Cutter(E)", GUILayout.Width (bwidth)))
							HotkeyFunction ("E");
						GUI.color = volume.cuter ? Color.white : Color.gray;
						EditorGUILayout.LabelField ("Y: " + volume.cutY, EditorStyles.textArea, GUILayout.Width (bwidth));
						if (GUILayout.Button ("▲(R)", GUILayout.Width (45)))
							HotkeyFunction ("R");
						if (GUILayout.Button ("▼(F)", GUILayout.Width (45)))
							HotkeyFunction ("F");
						GUI.color = Color.white;
					}

					DrawPieceSelectedGUI ();
				}
			}
		}

		private PaletteItem _itemSelected;
		private Texture2D _itemPreview;
		private LevelPiece _pieceSelected;
		private void DrawPieceSelectedGUI ()
		{
			using (var v = new EditorGUILayout.VerticalScope ()) {
				if (_pieceSelected != null) {
					EditorGUILayout.LabelField (new GUIContent (_itemPreview), GUILayout.Height (65));
					EditorGUILayout.LabelField (_itemSelected.itemName);
				}
			}
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

				volume.SetBlock (pos.x, pos.y, pos.z, isErase ? null : new Block ());
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

				volume.SetBlock (pos.x, pos.y, pos.z, isErase ? null : new Block ());

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
			
			bool canPlace = false;

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
				int gx = gPos.x;
				int gz = gPos.z;

				if (CheckPlaceable (gx, gz, _pieceSelected.pivot)) {
					canPlace = true;
				}

				if (canPlace) {
					volume.PlacePiece (bPos, gPos, isErase ? null : _pieceSelected);
					Chunk chunk = volume.GetChunk (bPos.x, bPos.y, bPos.z);
					chunk.UpdateMeshFilter ();
					chunk.UpdateMeshCollider ();
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
		private void HotkeyFunction (string funcKey = "")
		{
			int _index = (int)currentEditMode;
			int _count = System.Enum.GetValues (typeof(EditMode)).Length - 1;
			bool _hotkey = (currentEditMode != EditMode.View && currentEditMode != EditMode.Item);

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

			}
			if (_hotkey)
				Event.current.Use ();
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
			_itemInspected = (ItemNode != null) ? ItemNode.GetComponent<PaletteItem> () : null;
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
		#endregion

		#region SubscribeEvents
		private PaletteItem _itemInspected;

		private void DrawPieceInspectedGUI()
		{
			if (currentEditMode != EditMode.Item)
				return;

			if (_itemInspected != null) {
				using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
					using (var h = new EditorGUILayout.HorizontalScope ()) {
						string _label = "Piece Edited [" + selectedItemID + "]";
						EditorGUILayout.LabelField (_label, EditorStyles.boldLabel, GUILayout.Width (110));
						string _label2 = "(" + _itemInspected.GetComponent<LevelPiece> ().GetType ().Name + ") " + _itemInspected.name;
						EditorGUILayout.LabelField (_label2,EditorStyles.miniLabel);
					}
					if (_itemInspected.inspectedScript != null) {
						LevelPieceEditor e = (LevelPieceEditor)(Editor.CreateEditor (_itemInspected.inspectedScript));
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