using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace CreVox
{

	[CustomEditor (typeof(Volume))]
	public class VolumeeEditor : Editor
	{
		Volume volume;
		Dictionary<WorldPos, Chunk> dirtyChunks = new Dictionary<WorldPos, Chunk> ();
		int cx = 1;
		int cy = 1;
		int cz = 1;

		WorldPos workpos;

		private int fixY = 0;

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

		private PaletteItem _itemSelected;
		private Texture2D _itemPreview;
		private LevelPiece _pieceSelected;

		private void OnEnable ()
		{
			volume = (Volume)target;
			SubscribeEvents ();
		}

		private void OnDisable ()
		{
			UnsubscribeEvents ();
		}

		private void OnSceneGUI ()
		{
			DrawModeGUI ();
			ModeHandler ();
			if (!EditorApplication.isPlaying)
				EventHandler ();
		}

		public override void OnInspectorGUI ()
		{
			float lw = 60;
			float w = (Screen.width - 20 - lw) / 3 - 8;
			EditorGUIUtility.labelWidth = 20;

			using (var v = new GUILayout.VerticalScope (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20))) {
				EditorGUILayout.LabelField ("Chunk setting", EditorStyles.boldLabel);
				using (var h = new GUILayout.HorizontalScope ()) {
					EditorGUILayout.LabelField ("Count", GUILayout.Width (lw));
					cx = EditorGUILayout.IntField ("X", cx, GUILayout.Width (w));
					cy = EditorGUILayout.IntField ("Y", cy, GUILayout.Width (w));
					cz = EditorGUILayout.IntField ("Z", cz, GUILayout.Width (w));
				}
				if (GUILayout.Button ("Init")) {
					volume.Reset ();
					volume.Init (cx, cy, cz);
				}
			}

			using (var v = new GUILayout.VerticalScope (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20))) {
				using (var h = new GUILayout.HorizontalScope ()) {
					EditorGUILayout.LabelField ("Chunk Data", EditorStyles.boldLabel);
					if (GUILayout.Button ("Save", GUILayout.Width (40))) {
						string sPath = Serialization.GetSaveLocation (volume.workFile == "" ? null : volume.workFile);
						Serialization.SaveWorld (volume, sPath);
						volume.workFile = sPath.Remove (sPath.LastIndexOf (".")).Substring (sPath.IndexOf (PathCollect.resourceSubPath));
					}
					if (GUILayout.Button ("Load", GUILayout.Width (40))) {
						string lPath = Serialization.GetLoadLocation (volume.workFile == "" ? null : volume.workFile);
						volume.workFile = lPath.Remove (lPath.LastIndexOf (".")).Substring (lPath.IndexOf (PathCollect.resourceSubPath));
						Save save = Serialization.LoadRTWorld (volume.workFile);
						if (save != null)
							volume.BuildWorld (save);
						SceneView.RepaintAll ();
					}
				}
				EditorGUILayout.LabelField (volume.workFile, EditorStyles.miniLabel);
			}

			using (var v = new GUILayout.VerticalScope (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20))) {
				using (var h = new GUILayout.HorizontalScope ()) {
					EditorGUILayout.LabelField ("ArtPack", EditorStyles.boldLabel);
					if (GUILayout.Button ("Set", GUILayout.Width (40))) {
						string ppath = EditorUtility.OpenFolderPanel (
							"選擇場景風格元件包的目錄位置",
							PathCollect.resourcesPath + PathCollect.pieces,
							""
						);
						ppath = ppath.Substring (ppath.IndexOf (PathCollect.resourceSubPath));
						volume.piecePack = ppath;
						Save save;
						save = Serialization.LoadRTWorld (volume.workFile);
						if (save != null)
							volume.BuildWorld (save);
						SceneView.RepaintAll ();
					}
				}
				using (var h = new GUILayout.HorizontalScope ()) {
					EditorGUIUtility.labelWidth = 60;
					volume.canvas = (Canvas)EditorGUILayout.ObjectField ("Canvas", volume.canvas, typeof(Canvas), false);
					if (GUILayout.Button ("Gen", GUILayout.Width (40))) {
						volume.GenerateDecoration ();
					}
				}
				EditorGUILayout.LabelField (volume.piecePack, EditorStyles.miniLabel);
			}

			DrawPieceSelectedGUI ();

			if (GUI.changed) {
				EditorUtility.SetDirty (volume);
				volume.UpdateChunks ();
			}
		}

		private void DrawPieceSelectedGUI ()
		{
			GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20));
			EditorGUILayout.LabelField ("Piece Selected", EditorStyles.boldLabel);
			if (_pieceSelected == null) {
				EditorGUILayout.HelpBox ("No piece selected!", MessageType.Info);
			} else {
				EditorGUILayout.BeginVertical ("box");
				EditorGUILayout.LabelField (new GUIContent (_itemPreview), GUILayout.Height (40));
				EditorGUILayout.LabelField (_itemSelected.itemName);
				EditorGUILayout.EndVertical ();
			}
			GUILayout.EndVertical ();
		}

		private void SubscribeEvents ()
		{
			PaletteWindow.ItemSelectedEvent += new PaletteWindow.itemSelectedDelegate (UpdateCurrentPieceInstance);
		}

		private void UnsubscribeEvents ()
		{
			PaletteWindow.ItemSelectedEvent -= new PaletteWindow.itemSelectedDelegate (UpdateCurrentPieceInstance);
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
			EditorGUILayout.BeginHorizontal ();
			volume.editDis = EditorGUILayout.Slider ("Editable Distance", volume.editDis, 90f, 1000f);
			EditorGUILayout.EndHorizontal ();
			GUILayout.EndArea ();

			GUILayout.BeginArea (new Rect (10f, 65f, ButtonW * 2 + 10, 65f));
			DrawLayerModeGUI ();
			GUILayout.EndArea ();

			Handles.EndGUI ();
		}

		private void DrawLayerModeGUI ()
		{
			GUI.color = new Color (volume.YColor.r, volume.YColor.g, volume.YColor.b, 1.0f);
			EditorGUILayout.BeginHorizontal (EditorStyles.textArea/*"Box"*/, GUILayout.Width (90), GUILayout.Height (50f));
			GUI.color = Color.white;
			EditorGUILayout.BeginVertical ();
			if (GUILayout.Button ("▲", GUILayout.Width (65))) {
				fixY = volume.editY + 1;
				volume.ChangeEditY (fixY);
				fixY = volume.editY;
			}
			EditorGUILayout.LabelField (
				"Layer : " + volume.editY,
				EditorStyles.textArea,
				GUILayout.Width (65)
			);
			if (GUILayout.Button ("▼", GUILayout.Width (65))) {
				fixY = volume.editY - 1;
				volume.ChangeEditY (fixY);
				fixY = volume.editY;
			}
			EditorGUILayout.EndVertical ();

			if (GUILayout.Button (volume.pointer ? "Hide\n Pointer" : "Show\n Pointer", GUILayout.ExpandHeight (true))) {
				volume.pointer = !volume.pointer;
				fixY = volume.editY;
				volume.ChangeEditY (fixY);
			}
			EditorGUILayout.EndHorizontal ();
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
//					Tools.current = Tool.View;
				break;
			}
			if (selectedEditMode != currentEditMode) {
				currentEditMode = selectedEditMode;
				Repaint ();
			}
		}

		private void EventHandler ()
		{
			HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));
			int button = Event.current.button;

			if (!Event.current.alt) {
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

			EventHotkey ();

		}

		private void EventHotkey ()
		{
			int _index = (int)currentEditMode;
			int _count = System.Enum.GetValues (typeof(EditMode)).Length - 1;

			if (Event.current.type == EventType.KeyDown) {
				switch (Event.current.keyCode) {
				case KeyCode.W:
					fixY = volume.editY + 1;
					volume.ChangeEditY (fixY);
					fixY = volume.editY;
					break;

				case KeyCode.S:
					fixY = volume.editY - 1;
					volume.ChangeEditY (fixY);
					fixY = volume.editY;
					break;

				case KeyCode.A:
					if (_index == 0)
						currentEditMode = (EditMode)_count;
					else
						currentEditMode = (EditMode)(_index - 1);
					Repaint ();
					break;

				case KeyCode.D:
					if (_index == _count)
						currentEditMode = (EditMode)0;
					else
						currentEditMode = (EditMode)(_index + 1);
					Repaint ();
					break;

				case KeyCode.E:
					volume.pointer = !volume.pointer;
					fixY = volume.editY;
					volume.ChangeEditY (fixY);
					break;
				}
			}
		}

		private void DrawMarker (bool isErase)
		{
			RaycastHit hit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
			bool isHit = Physics.Raycast (worldRay, out hit, volume.editDis, _mask);

			if (isHit && !isErase && hit.collider.GetComponentInParent<Volume> () == volume) {
				WorldPos pos = EditTerrain.GetBlockPos (hit, Vector3.zero, isErase ? false : true);
				float x = pos.x * Block.w;
				float y = pos.y * Block.h;
				float z = pos.z * Block.d;

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
			bool isHit = Physics.Raycast (worldRay, out hit, volume.editDis, _mask);

			if (isHit && hit.collider.GetComponentInParent<Volume> () == volume) {
				WorldPos pos = EditTerrain.GetBlockPos (hit, Vector3.zero, false);
				float x = pos.x * Block.w;
				float y = pos.y * Block.h;
				float z = pos.z * Block.d;

				volume.useBox = true;
				BoxCursorUtils.UpdateBox (volume.box, new Vector3 (x, y, z), Vector3.zero);
				SceneView.RepaintAll ();
			} else {
				volume.useBox = false;
				SceneView.RepaintAll ();
			}
		}

		private void DrawGridMarker ()
		{
			if (_pieceSelected == null)
				return;
			bool isNotLayer = (currentEditMode != EditMode.ObjectLayer);
			RaycastHit hit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = isNotLayer ? 1 << LayerMask.NameToLayer ("Editor") : 1 << LayerMask.NameToLayer ("EditorLevel");
			bool isHit = Physics.Raycast (worldRay, out hit, (int)volume.editDis, _mask);

			if (isHit && hit.collider.GetComponentInParent<Volume> () == volume) {
				if (hit.normal.y <= 0)
					return;

//				hit.point -= volume.transform.position;
				WorldPos pos = EditTerrain.GetBlockPos (hit, Vector3.zero, isNotLayer);
				WorldPos gPos = EditTerrain.GetGridPos (hit.point);
				gPos.y = isNotLayer ? 0 : (int)Block.h;
				float x = pos.x * Block.w + gPos.x - 1;
				float y = pos.y * Block.h + gPos.y - 1;
				float z = pos.z * Block.d + gPos.z - 1;
				
				Handles.color = Color.white;
				Handles.lighting = true;
				Handles.RectangleCap (0, new Vector3 (pos.x * Block.w, y, pos.z * Block.d), Quaternion.Euler (90, 0, 0), Block.hw);
				Handles.DrawLine (hit.point, new Vector3 (pos.x * Block.w, pos.y * Block.h, pos.z * Block.d));

				LevelPiece.PivotType pivot = (_pieceSelected.isStair) ? LevelPiece.PivotType.Edge : _pieceSelected.pivot;
				if (CheckPlaceable ((int)gPos.x, (int)gPos.z, pivot)) {
					Handles.color = Color.red;
					Handles.RectangleCap (0, new Vector3 (x, y, z), Quaternion.Euler (90, 0, 0), 0.5f);
					Handles.color = Color.white;
				}

				volume.useBox = true;
				BoxCursorUtils.UpdateBox (volume.box, new Vector3 (pos.x * Block.w, pos.y * Block.h, pos.z * Block.d), Vector3.zero);
				SceneView.RepaintAll ();
			} else {
				volume.useBox = false;
				SceneView.RepaintAll ();
			}
		}

		private void Paint (bool isErase)
		{
			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer ("Editor");
			bool isHit = Physics.Raycast (worldRay, out gHit, volume.editDis, _mask);
			WorldPos pos;

			if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
				pos = EditTerrain.GetBlockPos (gHit, volume.transform.position, isErase ? false : true);
				Debug.Log (pos);

				volume.SetBlock (pos.x, pos.y, pos.z, isErase ? new BlockAir () : new Block ());
				Chunk chunk = volume.GetChunk (pos.x, pos.y, pos.z);

				if (chunk) {
					if (!dirtyChunks.ContainsKey (pos))
						dirtyChunks.Add (pos, chunk);
					chunk.UpdateMeshFilter ();
					SceneView.RepaintAll ();
				}
			}
		}

		private void PaintLayer (bool isErase)
		{
			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer ("EditorLevel");
			bool isHit = Physics.Raycast (worldRay, out gHit, volume.editDis, _mask);
			WorldPos pos;

			if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
				gHit.point = gHit.point + new Vector3 (0f, -Block.h, 0f);
				pos = EditTerrain.GetBlockPos (gHit, volume.transform.position, true);

				volume.SetBlock (pos.x, pos.y, pos.z, isErase ? new BlockAir () : new Block ());
				Chunk chunk = volume.GetChunk (pos.x, pos.y, pos.z);

				if (chunk) {
					if (!dirtyChunks.ContainsKey (pos))
						dirtyChunks.Add (pos, chunk);
					chunk.UpdateMeshFilter ();
					SceneView.RepaintAll ();
				}
			}
		}

		private void PaintPieces (bool isErase)
		{
			if (_pieceSelected == null)
				return;
			
			bool canPlace = false;

			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			LayerMask _mask = (currentEditMode == EditMode.Object) ? 1 << LayerMask.NameToLayer ("Editor") : 1 << LayerMask.NameToLayer ("EditorLevel");
			bool isHit = Physics.Raycast (worldRay, out gHit, volume.editDis, _mask);

			if (isHit && gHit.collider.GetComponentInParent<Volume> () == volume) {
				if (gHit.normal.y <= 0)
					return;

//				gHit.point -= volume.transform.position;
				WorldPos bPos = EditTerrain.GetBlockPos (gHit, volume.transform.position, (currentEditMode == EditMode.Object));
				WorldPos gPos = EditTerrain.GetGridPos (gHit.point, volume.transform.position);
				gPos.y = 0;
				int gx = gPos.x;
				int gz = gPos.z;

				if (CheckPlaceable (gx, gz, _pieceSelected.pivot)) {
					canPlace = true;
				}

				if (canPlace) {
					volume.PlacePiece (bPos, gPos, isErase ? null : _pieceSelected);
					SceneView.RepaintAll ();
				}
			}
		}

		private void UpdateDirtyChunks ()
		{
			foreach (KeyValuePair<WorldPos, Chunk> c in dirtyChunks) {
				c.Value.UodateMeshCollider ();
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

		private void UpdateCurrentPieceInstance (PaletteItem item, Texture2D preview)
		{
			_itemSelected = item;
			_itemPreview = preview;
			_pieceSelected = (LevelPiece)item.GetComponent<LevelPiece> ();
			Repaint ();
		}
	}
}