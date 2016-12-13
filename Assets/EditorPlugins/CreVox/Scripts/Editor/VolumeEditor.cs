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
			if (volume.vd && !volume._useBytes)
				volume.BuildVolume (null, volume.vd);
			SubscribeEvents ();
		}

		private void OnDisable ()
		{
			if (!Volume.vg.debugRuler)
				volume.ActiveRuler (false);
			UnsubscribeEvents ();
		}

		public override void OnInspectorGUI ()
		{
			float buttonW = 40;
			float bh = 23.5f;
			float defLabelWidth = EditorGUIUtility.labelWidth;

			GUI.color = Color.gray;
			using (var v0 = new EditorGUILayout.VerticalScope ("ProgressBarBack")) {
				GUI.color = Color.white;
				using (var v = new EditorGUILayout.VerticalScope (EditorStyles.objectFieldThumb)) {
					EditorGUILayout.LabelField ("", "Chunk Data", "BoldLabel");
					EditorGUI.indentLevel++;
					volume.vd = (VolumeData)EditorGUILayout.ObjectField (volume.vd, typeof(VolumeData), false);
					EditorGUI.indentLevel--;
				}

				using (var h = new EditorGUILayout.HorizontalScope (GUILayout.Height(10))) {
					if (GUILayout.Button (new GUIContent ("Link\n▼", "將資料檔連結到場景"), "miniButtonLeft",GUILayout.Height(bh))) {
						volume._useBytes = false;
						volume.BuildVolume (new Save (), volume.vd);
						volume.tempPath = "";
						SceneView.RepaintAll ();
					}
					if (GUILayout.Button (new GUIContent ("▲\nWrite", "將場景現況 寫入/新建 資料檔(轉檔用)"), "miniButtonRight", GUILayout.Height(bh))) {
						volume._useBytes = false;
						volume.WriteVData ();
						EditorUtility.SetDirty (volume.vd);
						volume.tempPath = "";
						SceneView.RepaintAll ();
					}
				}

				bool linking;
				if (volume.chunks != null && volume.chunks.Count > 0 && volume.vd != null && volume.vd.chunkDatas.Count > 0)
					linking = ReferenceEquals (volume.chunks [0].cData, volume.vd.chunkDatas [0]);
				else {
//					Debug.LogWarning ("chunk list: " + ((volume.chunks == null) ? "null" : volume.chunks.Count.ToString()) +
//						"; chunkData: " + ((volume.vd == null) ? "null" : volume.vd.chunkDatas.Count.ToString()));
					linking = false;
				}
				
				GUI.backgroundColor = linking ? Color.green : Color.red;
				using (var v = new EditorGUILayout.VerticalScope (EditorStyles.objectFieldThumb)) {
					GUI.backgroundColor = Color.white;
					EditorGUILayout.LabelField ("","Scene Volume (" + (linking ? "連結中)" : "未連結)"), "BoldLabel");
					GUILayout.BeginHorizontal ();
					EditorGUIUtility.labelWidth = 15;
					EditorGUILayout.LabelField ("New");
					cx = EditorGUILayout.IntField ("X", cx, GUILayout.Width (buttonW-5));
					cy = EditorGUILayout.IntField ("Y", cy, GUILayout.Width (buttonW-5));
					cz = EditorGUILayout.IntField ("Z", cz, GUILayout.Width (buttonW-5));
					EditorGUIUtility.labelWidth = defLabelWidth;
					if (GUILayout.Button ("Init", GUILayout.Width (buttonW))) {
						volume.Reset ();
						volume.Init (cx, cy, cz);
						volume.workFile = "";
						volume.tempPath = "";
					}
					GUILayout.EndHorizontal ();
				}

				using (var h = new EditorGUILayout.HorizontalScope ()) {
					if (GUILayout.Button (new GUIContent ("Save\n▼", "同時按 SHIFT 快速存檔"), "miniButtonLeft", GUILayout.Height(bh))) {
						if (Event.current.shift) {
							if (volume.workFile != "") {
								string sPath = 
									Application.dataPath
									+ PathCollect.resourcesPath.Substring (6)
									+ volume.workFile + ".bytes";
								Serialization.SaveWorld (volume, sPath);
								volume.tempPath = "";
							}
						} else {
							string sPath = Serialization.GetSaveLocation (volume.workFile == "" ? null : volume.workFile);
							if (sPath != "") {
								Serialization.SaveWorld (volume, sPath);
								volume.workFile = sPath.Remove (sPath.LastIndexOf (".")).Substring (sPath.IndexOf (PathCollect.resourceSubPath));
								volume.tempPath = "";
							}
						}
					}
					if (GUILayout.Button (new GUIContent ("▲\nLoad", "同時按 SHIFT 快速讀檔"), "miniButtonRight", GUILayout.Height(bh))) {
						if (Event.current.shift) {
							if (volume.workFile != "") {
								string lPath = 
									Application.dataPath
									+ PathCollect.resourcesPath.Substring (6)
									+ volume.workFile + ".bytes";
								Save save = Serialization.LoadWorld (lPath);
								if (save != null) {
									volume._useBytes = true;
									volume.BuildVolume (save, volume.vd);
									volume.tempPath = "";
								}
								SceneView.RepaintAll ();
							}
						} else {
							string lPath = Serialization.GetLoadLocation (volume.workFile == "" ? null : volume.workFile);
							if (lPath != "") {
								Save save = Serialization.LoadWorld (lPath);
								if (save != null) {
									volume._useBytes = true;
									volume.workFile = lPath.Remove (lPath.LastIndexOf (".")).Substring (lPath.IndexOf (PathCollect.resourceSubPath));
									volume.BuildVolume (save);
									volume.tempPath = "";
								}
								SceneView.RepaintAll ();
							}
						}
					}
				}

				EditorGUI.BeginChangeCheck ();
				using (var v = new EditorGUILayout.VerticalScope (EditorStyles.objectFieldThumb)) {
					EditorGUILayout.LabelField ("", "Chunk Bytes(Old)", "BoldLabel");
					int cha = 17;
					using (var h = new EditorGUILayout.HorizontalScope ()) {
						EditorGUILayout.LabelField ("Main: ", GUILayout.Width (50f));
						if (volume.workFile.Length > cha && volume.workFile.Contains(PathCollect.save))
							EditorGUILayout.LabelField (volume.workFile, EditorStyles.miniLabel);
					}
					using (var h = new EditorGUILayout.HorizontalScope ()) {
						EditorGUILayout.LabelField ("Backup:", GUILayout.Width (50f));
						if (/*volume.tempPath.Length > 17 && */volume.tempPath.Contains (PathCollect.save))
							EditorGUILayout.LabelField (volume.tempPath, EditorStyles.miniLabel);
					}
				}
			}

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				GUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("ArtPack", EditorStyles.boldLabel);
				if (GUILayout.Button ("Set", GUILayout.Width (buttonW))) {
					string ppath = EditorUtility.OpenFolderPanel (
						               "選擇場景風格元件包的目錄位置",
									volume.vd.ArtPack,
						               ""
					               );
					if (Volume.vg.saveBackup)
						volume.SaveTempWorld ();
					
					ppath = ppath.Substring (ppath.IndexOf (PathCollect.resourcesPath));
					string[] mats = AssetDatabase.FindAssets ("voxel t:Material", new string[]{ ppath });
					if (mats.Length == 1) {
						string matPath = AssetDatabase.GUIDToAssetPath (mats [0]);
						volume.vertexMaterial = AssetDatabase.LoadAssetAtPath<Material> (matPath);
						volume.vd.vMaterial = matPath.Remove (matPath.Length - 4).Substring (matPath.IndexOf (PathCollect.resourceSubPath));
					} else
						volume.vertexMaterial = null;
					ppath = ppath.Substring (ppath.IndexOf (PathCollect.resourceSubPath));
					volume.vd.ArtPack = ppath;
					EditorUtility.SetDirty (volume.vd);

					volume.LoadTempWorld ();
				}
				GUILayout.EndHorizontal ();

				EditorGUIUtility.labelWidth = 120f;
				EditorGUILayout.LabelField (volume.vd.ArtPack, EditorStyles.miniLabel);
				volume.vertexMaterial = (Material)EditorGUILayout.ObjectField (
					new GUIContent ("Volume Material", "Auto Select if ONLY ONE Material's name contain \"voxel\"")
					, volume.vertexMaterial
					, typeof(Material)
					, false);
			}

			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				EditorGUILayout.LabelField ("Global Setting", EditorStyles.boldLabel);
				EditorGUI.BeginChangeCheck ();
				Volume.vg.saveBackup = EditorGUILayout.ToggleLeft ("Save Backup File(" + Volume.vg.saveBackup + ")", Volume.vg.saveBackup);
				Volume.vg.FakeDeco = EditorGUILayout.ToggleLeft ("Use Release Deco(" + Volume.vg.FakeDeco + ")", Volume.vg.FakeDeco);
				Volume.vg.debugRuler = EditorGUILayout.ToggleLeft ("Show Ruler(" + Volume.vg.debugRuler + ")", Volume.vg.debugRuler);
				if (EditorGUI.EndChangeCheck ()) {
					EditorUtility.SetDirty (Volume.vg);
					if (!UnityEditor.EditorApplication.isPlaying) {
						volume.transform.root.BroadcastMessage ("ShowRuler", SendMessageOptions.DontRequireReceiver);
					}
				}
			}

			DrawPieceSelectedGUI ();

			if (EditorGUI.EndChangeCheck()) {
				EditorUtility.SetDirty (volume);
//				volume.PlacePieces ();
				volume.UpdateChunks ();
			}
		}

		#region Scene GUI

		public enum EditMode
		{
			View,
			VoxelLayer,
			Voxel,
			ObjectLayer,
			Object
		}

		private EditMode selectedEditMode;
		private EditMode currentEditMode;

		private void OnSceneGUI ()
		{
			DrawModeGUI ();
			ModeHandler ();
			if (!EditorApplication.isPlaying)
				EventHandler ();
		}

		private void DrawModeGUI ()
		{
			List<EditMode> modes = EditorUtils.GetListFromEnum<EditMode> ();
			List<string> modeLabels = new List<string> ();
			foreach (EditMode mode in modes) {
				modeLabels.Add (mode.ToString ());
			}
			float ButtonW = 80;

			Handles.BeginGUI ();
			GUI.color = new Color (volume.YColor.r, volume.YColor.g, volume.YColor.b, 1.0f);
			GUILayout.BeginArea (new Rect (10f, 10f, modeLabels.Count * ButtonW, 50f), "", EditorStyles.textArea); //根據選項數量決定寬度
			GUI.color = Color.white;
			selectedEditMode = (EditMode)GUILayout.Toolbar ((int)currentEditMode, modeLabels.ToArray (), GUILayout.ExpandHeight (true));
			GUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Editable Distance", GUILayout.Width (105));
			EditorGUI.BeginChangeCheck ();
			Volume.vg.editDis = GUILayout.HorizontalSlider (Volume.vg.editDis, 99f, 999f);
			if (EditorGUI.EndChangeCheck ())
				EditorUtility.SetDirty (Volume.vg);
			EditorGUILayout.LabelField (((int)Volume.vg.editDis).ToString (), GUILayout.Width (25));
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();
			
			DrawLayerModeGUI ();
			Handles.EndGUI ();
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
			default:
				break;
			}
			if (selectedEditMode != currentEditMode) {
				currentEditMode = selectedEditMode;
				Repaint ();
			}
		}

		private void DrawMarker (bool isErase)
		{
			RaycastHit hit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
			bool isHit = Physics.Raycast (worldRay, out hit, Volume.vg.editDis, _mask);

			if (isHit && !isErase && hit.collider.GetComponentInParent<Volume> () == volume) {
				WorldPos pos = EditTerrain.GetBlockPos (hit, isErase ? false : true);
				float x = pos.x * Volume.vg.w;
				float y = pos.y * Volume.vg.h;
				float z = pos.z * Volume.vg.d;

				Handles.DrawLine (hit.point, hit.point + hit.normal);
				if (hit.collider.gameObject.tag == PathCollect.rularTag) {
					hit.normal = Vector3.zero;
				}
				BoxCursorUtils.UpdateBox (volume.box, new Vector3 (x, y, z), hit.normal);
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
			bool isHit = Physics.Raycast (worldRay, out hit, Volume.vg.editDis, _mask);

			if (isHit && hit.collider.GetComponentInParent<Volume> () == volume) {
				WorldPos pos = EditTerrain.GetBlockPos (hit, false);
				float x = pos.x * Volume.vg.w;
				float y = pos.y * Volume.vg.h;
				float z = pos.z * Volume.vg.d;

				Handles.DrawLine (hit.point, new Vector3 (pos.x * Volume.vg.w, pos.y * Volume.vg.h, pos.z * Volume.vg.d));
				volume.useBox = true;
				BoxCursorUtils.UpdateBox (volume.box, new Vector3 (x, y, z), Vector3.zero);
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
			bool isHit = Physics.Raycast (worldRay, out hit, (int)Volume.vg.editDis, _mask);

			if (isHit && hit.collider.GetComponentInParent<Volume> () == volume) {
				if (hit.normal.y <= 0)
					return;
				
				WorldPos pos = EditTerrain.GetBlockPos (hit, isNotLayer);
				WorldPos gPos = EditTerrain.GetGridPos (hit.point);
				gPos.y = isNotLayer ? 0 : (int)Volume.vg.h;

				float x = pos.x * Volume.vg.w + gPos.x + ((hit.point.x < 0) ? 1 : -1);
				float y = pos.y * Volume.vg.h + gPos.y - 1;
				float z = pos.z * Volume.vg.d + gPos.z + ((hit.point.z < 0) ? 1 : -1);

				Handles.color = Color.white;
				Handles.lighting = true;
				Handles.RectangleCap (0, new Vector3 (pos.x * Volume.vg.w, y, pos.z * Volume.vg.d), Quaternion.Euler (90, 0, 0), Volume.vg.hw);
				Handles.DrawLine (hit.point, new Vector3 (pos.x * Volume.vg.w, pos.y * Volume.vg.h, pos.z * Volume.vg.d));

				LevelPiece.PivotType pivot = _pieceSelected.pivot;
				if (CheckPlaceable ((int)gPos.x, (int)gPos.z, pivot)) {
					Handles.color = Color.red;
					Handles.RectangleCap (0, new Vector3 (x, y, z), Quaternion.Euler (90, 0, 0), 0.5f);
					Handles.color = Color.white;
				}

				volume.useBox = true;
				BoxCursorUtils.UpdateBox (volume.box, new Vector3 (pos.x * Volume.vg.w, pos.y * Volume.vg.h, pos.z * Volume.vg.d), Vector3.zero);
				SceneView.RepaintAll ();
			} else {
				volume.useBox = false;
				SceneView.RepaintAll ();
			}
		}

		private void EventHandler ()
		{
			if (Event.current.alt) {
				return;
			}

			if (Event.current.type == EventType.KeyDown) {
				HotkeyFunction (Event.current.keyCode.ToString ());
				Event.current.Use ();
				return;
			} 

			if (currentEditMode != EditMode.View) {
				HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));
				int button = Event.current.button;

				switch (currentEditMode) {
				case EditMode.Voxel:
				case EditMode.VoxelLayer: 
					volume.useBox = true;
					break;

				default:
					volume.useBox = false;
					break;
				}

				switch (currentEditMode) {
				case EditMode.Voxel:
					if (button == 0)
						DrawMarker (false);
					else if (button <= 1) {
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
					if (Event.current.type == EventType.MouseUp) {
						UpdateDirtyChunks ();
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
					if (Event.current.type == EventType.MouseUp) {
						UpdateDirtyChunks ();
					}
					break;

				case EditMode.Object:
				case EditMode.ObjectLayer:
					if (Event.current.type == EventType.MouseDown) {
						if (button == 0)
							PaintPieces (false);
						else if (button == 1) {
							PaintPieces (true);
							Tools.viewTool = ViewTool.None;
							Event.current.Use ();
						}
					}
					DrawGridMarker ();
					break;

				default:
					break;
				}
			}
		}

		#endregion

		#region LayerControl

		private int fixPointY = 0;
		private int fixCutY = 0;

		private void DrawLayerModeGUI ()
		{
			GUI.color = new Color (volume.YColor.r, volume.YColor.g, volume.YColor.b, 1.0f);
			float bwidth = 70f;
			using (var a = new GUILayout.AreaScope (new Rect (10f, 65f, bwidth * 2 + 10f, 85f), "", EditorStyles.textArea)) {
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
				}
			}
		}

		private void HotkeyFunction (string funcKey = "")
		{
			int _index = (int)currentEditMode;
			int _count = System.Enum.GetValues (typeof(EditMode)).Length - 1;

			switch (funcKey) {
			case "A":
				if (_index == 0)
					currentEditMode = (EditMode)_count;
				else
					currentEditMode = (EditMode)(_index - 1);
				Repaint ();
				break;

			case "D":
				if (_index == _count)
					currentEditMode = (EditMode)0;
				else
					currentEditMode = (EditMode)(_index + 1);
				Repaint ();
				break;

			case "Q":
				volume.pointer = !volume.pointer;
				fixPointY = volume.pointY;
				volume.ChangePointY (fixPointY);
				break;

			case "W":
				fixPointY = volume.pointY + 1;
				volume.ChangePointY (fixPointY);
				fixPointY = volume.pointY;
				break;

			case "S":
				fixPointY = volume.pointY - 1;
				volume.ChangePointY (fixPointY);
				fixPointY = volume.pointY;
				break;

			case "E":
				volume.cuter = !volume.cuter;
				fixCutY = volume.cutY;
				volume.ChangeCutY (fixCutY);
				break;

			case "R":
				fixCutY = volume.cutY + 1;
				volume.ChangeCutY (fixCutY);
				fixCutY = volume.cutY;
				break;

			case "F":
				fixCutY = volume.cutY - 1;
				volume.ChangeCutY (fixCutY);
				fixCutY = volume.cutY;
				break;

			}
		}

		#endregion

		private void Paint (bool isErase)
		{
			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
			bool isHit = Physics.Raycast (worldRay, out gHit, Volume.vg.editDis, _mask);
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
			bool isHit = Physics.Raycast (worldRay, out gHit, Volume.vg.editDis, _mask);
			WorldPos pos;

			if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
				gHit.point = gHit.point + new Vector3 (0f, -Volume.vg.h, 0f);
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

		private void PaintPieces (bool isErase)
		{
			if (_pieceSelected == null)
				return;
			
			bool canPlace = false;

			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = (currentEditMode == EditMode.Object) ? 1 << LayerMask.NameToLayer ("Editor") : 1 << LayerMask.NameToLayer ("EditorLevel");
			bool isHit = Physics.Raycast (worldRay, out gHit, Volume.vg.editDis, _mask);

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
					EditorUtility.SetDirty (volume.vd);
					SceneView.RepaintAll ();
				}
			}
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
			else if (pType == LevelPiece.PivotType.Edge && (x + z) % 2 == 1)
				return true;

			return false;
		}

		#region SubscribeEvents

		private PaletteItem _itemSelected;
		private Texture2D _itemPreview;
		private LevelPiece _pieceSelected;

		private void DrawPieceSelectedGUI ()
		{
			using (var v = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
				EditorGUILayout.LabelField ("Piece Selected", EditorStyles.boldLabel);
				if (_pieceSelected == null) {
					EditorGUILayout.HelpBox ("No piece selected!", MessageType.Info);
				} else {
					using (var v2 = new EditorGUILayout.VerticalScope (EditorStyles.helpBox)) {
						EditorGUILayout.LabelField (new GUIContent (_itemPreview), GUILayout.Height (40));
						EditorGUILayout.LabelField (_itemSelected.itemName);
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