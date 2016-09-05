using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace CreVox
{

	[CustomEditor(typeof(World))]
	public class WorldEditor : Editor
	{
		World world;
		Dictionary<WorldPos, Chunk> dirtyChunks = new Dictionary<WorldPos, Chunk>();
		int cx = 1;
		int cy = 1;
		int cz = 1;

		WorldPos workpos;

		private int fixY = 0;
		private bool showPointer = true;

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

		private void OnEnable()
		{
			world = (World)target;
			SubscribeEvents();
		}

		private void OnDisable()
		{
			UnsubscribeEvents();
		}

		void OnSceneGUI()
		{
			DrawModeGUI();
			ModeHandler();
			EventHandler();
		}

		public override void OnInspectorGUI()
		{
			//修改成適應視窗寬度
			float lw = 60;
			float w = (Screen.width - 20 - lw) / 3 - 8;
			EditorGUIUtility.labelWidth = 20;

			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(Screen.width - 20));
			EditorGUILayout.LabelField("Chunk setting", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Count", GUILayout.Width(lw));
			cx = EditorGUILayout.IntField("X", cx, GUILayout.Width(w));
			cy = EditorGUILayout.IntField("Y", cy, GUILayout.Width(w));
			cz = EditorGUILayout.IntField("Z", cz, GUILayout.Width(w));
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Init")) {
				world.Reset();
				world.workFile = "";
				world.Init(cx, cy, cz);
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(Screen.width - 20));
			EditorGUILayout.LabelField("Save & Load", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Save")) {
				world.workFile = Serialization.GetSaveLocation();
				Serialization.SaveWorld(world, world.workFile);
			}
			if (GUILayout.Button("Load")) {
				world.workFile = Serialization.GetLoadLocation();
				Save save = Serialization.LoadWorld(world.workFile);
				if (save != null)
					world.BuildWorld(save);
				SceneView.RepaintAll();
			}
			GUILayout.EndHorizontal();
			if (world.workFile != null)
				EditorGUILayout.LabelField("Working File : " + world.workFile.Substring(world.workFile.LastIndexOf("/") + 1));
			GUILayout.EndVertical();

			DrawPieceSelectedGUI();

			if (GUI.changed) {
				EditorUtility.SetDirty(world);
			}
		}

		private void DrawPieceSelectedGUI()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(Screen.width - 20));
			EditorGUILayout.LabelField("Piece Selected", EditorStyles.boldLabel);
			if (_pieceSelected == null) {
				EditorGUILayout.HelpBox("No piece selected!", MessageType.Info);
			} else {
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.LabelField(new GUIContent(_itemPreview), GUILayout.Height(40));
				EditorGUILayout.LabelField(_itemSelected.itemName);
				EditorGUILayout.EndVertical();
			}
			GUILayout.EndVertical();
		}

		private void SubscribeEvents()
		{
			PaletteWindow.ItemSelectedEvent += new PaletteWindow.itemSelectedDelegate(UpdateCurrentPieceInstance);
			EditorApplication.playmodeStateChanged += new EditorApplication.CallbackFunction(OnPlayModeChange);
			if (EditorUtils.ChkEventCallback(EditorApplication.playmodeStateChanged, "OnBeforePlay") == true)
				EditorApplication.playmodeStateChanged -= new EditorApplication.CallbackFunction(world.OnBeforePlay);
		}

		private void UnsubscribeEvents()
		{
			PaletteWindow.ItemSelectedEvent -= new PaletteWindow.itemSelectedDelegate(UpdateCurrentPieceInstance);
			EditorApplication.playmodeStateChanged -= new EditorApplication.CallbackFunction(OnPlayModeChange);
			if (EditorUtils.ChkEventCallback(EditorApplication.playmodeStateChanged, "OnBeforePlay") == false)
				EditorApplication.playmodeStateChanged += new EditorApplication.CallbackFunction(world.OnBeforePlay);
		}

		private void DrawModeGUI()
		{
			List<EditMode> modes = EditorUtils.GetListFromEnum<EditMode>();
			List<string> modeLabels = new List<string>();
			foreach (EditMode mode in modes) {
				modeLabels.Add(mode.ToString());
			}
			float ButtonW = 90;

			Handles.BeginGUI();
			GUILayout.BeginArea(new Rect(10f, 10f, modeLabels.Count * ButtonW, 60f), "", "Box"); //根據選項數量決定寬度
			selectedEditMode = (EditMode)GUILayout.Toolbar((int)currentEditMode, modeLabels.ToArray(), GUILayout.ExpandHeight(true));
			EditorGUILayout.BeginHorizontal();
			world.editDis = EditorGUILayout.Slider("Edibable Distance", world.editDis, 90f, 1000f);
			EditorGUILayout.EndHorizontal();
			GUILayout.EndArea();

			GUILayout.BeginArea(new Rect(10f, 75f, ButtonW * 2 + 10, 65f));
			DrawLayerModeGUI();
			GUILayout.EndArea();

			Handles.EndGUI();
		}

		private void DrawLayerModeGUI()
		{
			if (selectedEditMode == EditMode.VoxelLayer || selectedEditMode == EditMode.ObjectLayer) {
				GUI.color = new Color(world.YColor.r, world.YColor.g, world.YColor.b, 1.0f);
				EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(90));
				GUI.color = Color.white;
				EditorGUILayout.BeginVertical();
				if (GUILayout.Button("▲", GUILayout.Width(85))) {
					fixY = world.editY + 1;
					world.ChangeEditY(fixY);
					fixY = world.editY;
				}
				EditorGUILayout.LabelField("", "Layer : " + world.editY, "TextField", GUILayout.Width(85));
				if (GUILayout.Button("▼", GUILayout.Width(85))) {
					fixY = world.editY - 1;
					world.ChangeEditY(fixY);
					fixY = world.editY;
				}
				EditorGUILayout.EndVertical();

				if (GUILayout.Button(showPointer ? "Hide\n Pointer" : "Show\n Pointer", GUILayout.ExpandHeight(true))) {
					showPointer = !showPointer;
				}
				EditorGUILayout.EndHorizontal();
				
				world.pointer = showPointer ? true : false;
			} else
				world.pointer = false;
		}

		private void ModeHandler()
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
					Tools.current = Tool.View;
					break;
			}
			if (selectedEditMode != currentEditMode) {
				currentEditMode = selectedEditMode;
				Repaint();
			}
		}

		private void EventHandler()
		{
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			int button = Event.current.button;

			if (!Event.current.alt) {
				//DrawBoxcursor
				switch (currentEditMode) {
					case EditMode.Voxel:
					case EditMode.VoxelLayer: 
						world.useBox = true;
						break;

					default:
						world.useBox = false;
						break;
				}

				//
				switch (currentEditMode) {
					case EditMode.ObjectLayer:
					case EditMode.VoxelLayer:
						if (Event.current.shift) {
							if (Event.current.type == EventType.ScrollWheel) {
								if (Event.current.delta.y < 0)
									fixY = world.editY + 1;
								if (Event.current.delta.y > 0)
									fixY = world.editY - 1;
								world.ChangeEditY(fixY);
								fixY = world.editY;
								Event.current.Use();
							}
						}
						break;

					default:
						break;
				}

				switch (currentEditMode) {
					case EditMode.Voxel:
						if (button == 0)
							DrawMarker(false);
						else if (button <= 1) {
							DrawMarker(true);
						}
						if (Event.current.type == EventType.MouseDown) {
							if (button == 0)
								Paint(false);
							else if (button == 1) {
								Paint(true);
								Tools.viewTool = ViewTool.None;
								Event.current.Use();
							}
						}
						if (Event.current.type == EventType.MouseUp) {
							UpdateDirtyChunks();
						}               
						break;

					case EditMode.VoxelLayer: 
						DrawLayerMarker();

						if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) {
							if (button == 0)
								PaintLayer(false);
							else if (button == 1) {
								PaintLayer(true);
								Tools.viewTool = ViewTool.None;
								Event.current.Use();
							}
						}

						if (Event.current.type == EventType.MouseUp) {
							UpdateDirtyChunks();
						}
						break;

					case EditMode.Object:
					case EditMode.ObjectLayer:
						if (Event.current.type == EventType.MouseDown) {
							if (button == 0)
								PaintPieces(false);
							else if (button == 1) {
								PaintPieces(true);
								Tools.viewTool = ViewTool.None;
								Event.current.Use();
							}
						}
						DrawGridMarker();
                
						break;

					default:
						break;
				}
			}

			EventHotkey();

		}

		void EventHotkey()
		{
			int _index = (int)currentEditMode;
			int _count = System.Enum.GetValues(typeof(EditMode)).Length - 1;

			if (Event.current.type == EventType.KeyDown) {
				switch (Event.current.keyCode) {
					case KeyCode.W:
						fixY = world.editY + 1;
						world.ChangeEditY(fixY);
						fixY = world.editY;
						break;

					case KeyCode.S:
						fixY = world.editY - 1;
						world.ChangeEditY(fixY);
						fixY = world.editY;
						break;

					case KeyCode.A:
						if (_index == 0)
							currentEditMode = (EditMode)_count;
						else
							currentEditMode = (EditMode)(_index - 1);
						Repaint();
						break;

					case KeyCode.D:
						if (_index == _count)
							currentEditMode = (EditMode)0;
						else
							currentEditMode = (EditMode)(_index + 1);
						Repaint();
						break;

				}
			}
		}

		private void DrawMarker(bool isErase)
		{
			RaycastHit hit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer("Editor");
			bool isHit = Physics.Raycast(worldRay, out hit, world.editDis, _mask);

			if (isHit && !isErase) {
				WorldPos pos = EditTerrain.GetBlockPos(hit, isErase ? false : true);
				float x = pos.x * Block.w;
				float y = pos.y * Block.h;
				float z = pos.z * Block.d;

				if (hit.collider.gameObject.tag == PathCollect.rularTag) {
					hit.normal = Vector3.zero;
				}
				BoxCursorUtils.UpdateBox(world.box, new Vector3(x, y, z), hit.normal);
				SceneView.RepaintAll();
			} else {
				world.useBox = false;
				SceneView.RepaintAll();
			}
		}

		private void DrawLayerMarker()
		{
			RaycastHit hit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer("EditorLevel");
			bool isHit = Physics.Raycast(worldRay, out hit, world.editDis, _mask);

			if (isHit) {
				WorldPos pos = EditTerrain.GetBlockPos(hit, false);
				float x = pos.x * Block.w;
				float y = pos.y * Block.h;
				float z = pos.z * Block.d;

				world.useBox = true;
				BoxCursorUtils.UpdateBox(world.box, new Vector3(x, y, z), Vector3.zero);
				SceneView.RepaintAll();
			} else {
				world.useBox = false;
				SceneView.RepaintAll();
			}
		}

		private void DrawGridMarker()
		{
			if (_pieceSelected == null)
				return;
			bool isNotLayer = (currentEditMode != EditMode.ObjectLayer);
			RaycastHit hit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			LayerMask _mask = isNotLayer ? 1 << LayerMask.NameToLayer("Editor") : 1 << LayerMask.NameToLayer("EditorLevel");
			bool isHit = Physics.Raycast(worldRay, out hit, world.editDis, _mask);

			if (isHit) {
				if (hit.normal.y <= 0)
					return;

				WorldPos pos = EditTerrain.GetBlockPos(hit, isNotLayer);
				WorldPos gPos = EditTerrain.GetGridPos(hit.point);
				gPos.y = isNotLayer ? 0 : (int)Block.h;
				float x = pos.x * Block.w + gPos.x - 1;
				float y = pos.y * Block.h + gPos.y - 1;
				float z = pos.z * Block.d + gPos.z - 1;
				
				Handles.color = Color.white;
				Handles.lighting = true;
				Handles.RectangleCap(0, new Vector3(pos.x * Block.w, y, pos.z * Block.d), Quaternion.Euler(90, 0, 0), Block.hw);
				Handles.DrawLine(hit.point, new Vector3(pos.x * Block.w, pos.y * Block.h, pos.z * Block.d));

				LevelPiece.PivotType pivot = (_pieceSelected.isStair) ? LevelPiece.PivotType.Edge : _pieceSelected.pivot;
				if (CheckPlaceable((int)gPos.x, (int)gPos.z, pivot)) {
					Handles.color = Color.red;
//					Gizmos.DrawCube(new Vector3(x, y, z), new Vector3(Block.w / 3, 0.01f, Block.d / 3));
					Handles.RectangleCap(0, new Vector3(x, y, z), Quaternion.Euler(90, 0, 0), 0.5f);
					Handles.color = Color.white;
				}

				world.useBox = true;
				BoxCursorUtils.UpdateBox(world.box, new Vector3(pos.x * Block.w, pos.y * Block.h, pos.z * Block.d), Vector3.zero);
				SceneView.RepaintAll();
			} else {
				world.useBox = false;
				SceneView.RepaintAll();
			}
		}

		private void Paint(bool isErase)
		{
			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			LayerMask _mask = 1 << LayerMask.NameToLayer("Editor");
			bool isHit = Physics.Raycast(worldRay, out gHit, world.editDis, _mask);
			WorldPos pos;

			if (isHit) {
				pos = EditTerrain.GetBlockPos(gHit, isErase ? false : true);

				world.SetBlock(pos.x, pos.y, pos.z, isErase ? new BlockAir() : new Block());
				Chunk chunk = world.GetChunk(pos.x, pos.y, pos.z);

				if (chunk) {
					if (!dirtyChunks.ContainsKey(pos))
						dirtyChunks.Add(pos, chunk);
					chunk.UpdateMeshFilter();
					SceneView.RepaintAll();
				}
			}
		}

		//VoxelLayer------
		private void PaintLayer(bool isErase)
		{
			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			bool isHit = Physics.Raycast(worldRay, out gHit, world.editDis, 1 << LayerMask.NameToLayer("EditorLevel"));
			WorldPos pos;

			if (isHit) {
				gHit.point = gHit.point + new Vector3(0f, -Block.h, 0f);
				pos = EditTerrain.GetBlockPos(gHit, true);

				world.SetBlock(pos.x, pos.y, pos.z, isErase ? new BlockAir() : new Block());
				Chunk chunk = world.GetChunk(pos.x, pos.y, pos.z);

				if (chunk) {
					if (!dirtyChunks.ContainsKey(pos))
						dirtyChunks.Add(pos, chunk);
					chunk.UpdateMeshFilter();
					SceneView.RepaintAll();
				}
			}
		}
		//----------------

		private void PaintPieces(bool isErase)
		{
			if (_pieceSelected == null)
				return;
			
			bool canPlace = false;

			RaycastHit gHit;
			Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			LayerMask _mask = (currentEditMode == EditMode.Object) ? 1 << LayerMask.NameToLayer("Editor") : 1 << LayerMask.NameToLayer("EditorLevel");
			bool isHit = Physics.Raycast(worldRay, out gHit, world.editDis, _mask);

			if (isHit) {
				if (gHit.normal.y <= 0)
					return;
				
				WorldPos bPos = EditTerrain.GetBlockPos(gHit, (currentEditMode == EditMode.Object));
				WorldPos gPos = EditTerrain.GetGridPos(gHit.point);
				gPos.y = 0;
				int gx = gPos.x;
				int gz = gPos.z;

				if (_pieceSelected.isStair) {
					if (CheckPlaceable(gx, gz, LevelPiece.PivotType.Edge)) {
						if (gPos.x == 0 && gPos.z == 1)
							bPos.x -= 1;
						if (gPos.x == 2 && gPos.z == 1)
							bPos.x += 1;
						if (gPos.x == 1 && gPos.z == 0)
							bPos.z -= 1;
						if (gPos.x == 1 && gPos.z == 2)
							bPos.z += 1;
						bPos.y -= 1;
						canPlace = true;
					}
				}

				if (CheckPlaceable(gx, gz, _pieceSelected.pivot)) {
					canPlace = true;
				}

				if (canPlace) {
					world.PlacePiece(bPos, gPos, isErase ? null : _pieceSelected);
					SceneView.RepaintAll();
				}
			}
		}

		private void UpdateDirtyChunks()
		{
			foreach (KeyValuePair<WorldPos, Chunk> c in dirtyChunks) {
				c.Value.UodateMeshCollider();
			}
			dirtyChunks.Clear();
		}

		private bool CheckPlaceable(int x, int z, LevelPiece.PivotType pType)
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

		private void UpdateCurrentPieceInstance(PaletteItem item, Texture2D preview)
		{
			_itemSelected = item;
			_itemPreview = preview;
			_pieceSelected = (LevelPiece)item.GetComponent<LevelPiece>();
			Repaint();
		}

		private void OnPlayModeChange()
		{
			if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) {
				Debug.LogWarning("Save before play by Editor : playing(" + EditorApplication.isPlaying + ")");
				Serialization.SaveWorld(world, PathCollect.resourcesPath + PathCollect.testmap + ".bytes");
				AssetDatabase.Refresh();
				EditorApplication.isPlaying = true;
			}

			if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) {
				Debug.LogWarning("LoadRTWorld in Edit Mode : playing(" + EditorApplication.isPlaying + ")");
				Save save = Serialization.LoadRTWorld(PathCollect.testmap);
				if (save != null)
					world.BuildWorld(save);
				SceneView.RepaintAll();
			}
		}
	}
}