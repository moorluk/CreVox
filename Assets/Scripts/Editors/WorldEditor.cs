﻿using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(World))]
public class WorldEditor : Editor {
    World world;
	Dictionary<WorldPos, Chunk> dirtyChunks = new Dictionary<WorldPos, Chunk>();
    bool update = false;
    bool disable = false;

	//VoxelLayer------
	private int editY = 0;
	private bool showPointer = true;
	//----------------

    public enum EditMode
    {
        View,
        Voxel,
		VoxelLayer,//VoxelLayer------
        Object,
    }

    private EditMode selectedEditMode;
    private EditMode currentEditMode;

    private PaletteItem _itemSelected;
    private Texture2D _itemPreview;
    private LevelPiece _pieceSelected;

    public void OnEnable()
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

		GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20));
		EditorGUILayout.LabelField ("Chunk setting", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Prefab", GUILayout.Width (lw));
		world.chunkPrefab = EditorGUILayout.ObjectField (world.chunkPrefab, typeof(GameObject), false) as GameObject;
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Count", GUILayout.Width (lw));
		world.chunkX = EditorGUILayout.IntField ("X", world.chunkX, GUILayout.Width (w));
		world.chunkY = EditorGUILayout.IntField ("Y", world.chunkY, GUILayout.Width (w));
		world.chunkZ = EditorGUILayout.IntField ("Z", world.chunkZ, GUILayout.Width (w));
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();

		if (GUILayout.Button ("Init"))
		{
			world.Init ();
		}

        DrawPieceSelectedGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(world);
        }
        //SceneView.RepaintAll();
    }

    private void UpdateCurrentPieceInstance(PaletteItem item, Texture2D preview)
    {
        _itemSelected = item;
        _itemPreview = preview;
        _pieceSelected = (LevelPiece)item.GetComponent<LevelPiece>();
        Repaint();
    }

    private void SubscribeEvents()
    {
        PaletteWindow.ItemSelectedEvent += new PaletteWindow.itemSelectedDelegate(UpdateCurrentPieceInstance);
    }

    private void UnsubscribeEvents()
    {
        PaletteWindow.ItemSelectedEvent -= new PaletteWindow.itemSelectedDelegate(UpdateCurrentPieceInstance);
    }

    private void DrawPieceSelectedGUI()
	{
		GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (Screen.width - 20));
        EditorGUILayout.LabelField ("Piece Selected", EditorStyles.boldLabel);
        if (_pieceSelected == null)
        {
            EditorGUILayout.HelpBox("No piece selected!", MessageType.Info);
        }
        else {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(new GUIContent(_itemPreview), GUILayout.Height(40));
            EditorGUILayout.LabelField(_itemSelected.itemName);
            EditorGUILayout.EndVertical();
		}
		GUILayout.EndVertical ();
    }

    private void DrawModeGUI()
    {
        List<EditMode> modes = EditorUtils.GetListFromEnum<EditMode>();
        List<string> modeLabels = new List<string>();
        foreach (EditMode mode in modes)
        {
            modeLabels.Add(mode.ToString());
        }
		float ButtonW = 90;

        Handles.BeginGUI();
		GUILayout.BeginArea(new Rect(10f, 10f, modeLabels.Count * ButtonW, 40f)); //根據選項數量決定寬度
        selectedEditMode = (EditMode)GUILayout.Toolbar((int)currentEditMode, modeLabels.ToArray(), GUILayout.ExpandHeight(true));
        GUILayout.EndArea();

		//VoxelLayer------
		if (selectedEditMode == EditMode.VoxelLayer) {
			DrawLayerModeGUI ();
			world.pointer = showPointer?true:false;
		} else
			world.pointer = false;
		//----------------
		Handles.EndGUI();
	}

	//VoxelLayer------
	private void DrawLayerModeGUI()
	{
		GUILayout.BeginArea (new Rect (10f, 55f, 360f, 20f));
		GUILayout.BeginHorizontal ();
		GUILayout.Box ("Current Edit Y :" + world.editY);
		if (GUILayout.Button (showPointer ? "Hide Pointer" : "Show Pointer", GUILayout.Width (120))) {
			showPointer = !showPointer;
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();
	}
	//----------------

    private void ModeHandler()
    {
        switch (selectedEditMode)
        {
            case EditMode.Voxel:
			case EditMode.VoxelLayer: //VoxelLayer------
            case EditMode.Object:
                Tools.current = Tool.None;
                
                break;
            case EditMode.View:
            default:
                Tools.current = Tool.View;
                //Tools.viewTool = ViewTool.Orbit;
                break;
        }
        if (selectedEditMode != currentEditMode)
        {
            currentEditMode = selectedEditMode;
            Repaint();
        }
    }

    private void EventHandler()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        int button = Event.current.button;
        //Debug.LogFormat("MousePos: {0}", mousePosition);

        //DrawMarker();

        switch (currentEditMode)
        {
            case EditMode.Voxel:
                Debug.Log(button.ToString());
                if (button == 0)
                    DrawMarker(false);
                else if (button <= 1)
                {
                    DrawMarker(true);
                }
                if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
                {
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
		//VoxelLayer------
		case EditMode.VoxelLayer: 
			if (!Event.current.alt) {
				if (button == 0)
					DrawLayerMarker (false);
				else if (button <= 1)
					DrawLayerMarker (true);

				if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) {
					if (button == 0)
						PaintLayer (false);
					else if (button == 1) {
						PaintLayer (true);
						Tools.viewTool = ViewTool.None;
						Event.current.Use ();
					}
				}

				if (Event.current.type == EventType.ScrollWheel) {
					if (Event.current.delta.y < 0)
						editY++;
					if (Event.current.delta.y > 0)
						editY--;
					editY = Mathf.Clamp (editY, 0, world.chunkY * 16 - 1);
					world.ChangeEditY (editY);
					Event.current.Use ();
				}

				if (Event.current.type == EventType.MouseUp) {
					UpdateDirtyChunks ();
				}
			}
			break;
		//----------------

		case EditMode.Object:
			DrawGridMarker ();
                //PlaceObject();
                break;

            case EditMode.View:
            default:
                break;
        }
    }

    private void DrawMarker(bool isErase)
    {
        update = true;
        RaycastHit hit;
        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		if (Physics.Raycast(worldRay, out hit, 500f, 1 << LayerMask.NameToLayer("Editor")))
        {
            WorldPos pos = EditTerrain.GetBlockPos(hit, isErase ? false : true);
            float x = pos.x * Block.w;
            float y = pos.y * Block.h;
            float z = pos.z * Block.d;
            //float x = Mathf.FloorToInt((hitInfo.point.x + Block.hw) / Block.w) * Block.w;
            Handles.CubeCap(0, new Vector3(x, y, z), Quaternion.identity, 2f);
            SceneView.RepaintAll();
        }
    }

	//VoxelLayer------
	private void DrawLayerMarker(bool isErase)
	{
		update = true;
		RaycastHit hit;
		Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		if (Physics.Raycast(worldRay, out hit, 500f, 1 << LayerMask.NameToLayer("EditorLevel")))
		{
			WorldPos pos = EditTerrain.GetBlockPos(hit, isErase ? false : true);
			float x = pos.x * Block.w;
			float y = pos.y * Block.h;
			float z = pos.z * Block.d;
			Handles.CubeCap(0, new Vector3(x, y, z), Quaternion.identity, 2f);
			SceneView.RepaintAll();
		}
	}
	//----------------

    private void DrawGridMarker()
    {
        update = true;
        RaycastHit hit;
        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		if (Physics.Raycast(worldRay, out hit, 500f, 1 << LayerMask.NameToLayer("Editor")))
        {
            WorldPos pos = EditTerrain.GetBlockPos(hit, true);
            WorldPos gPos = EditTerrain.GetGridPos(hit.point);
            float x = pos.x * Block.w + gPos.x -1;
            float y = pos.y * Block.h + gPos.y -1;
            float z = pos.z * Block.d + gPos.z -1;
            Debug.Log("wpos: " + pos.ToString() + "gPos: " + gPos.ToString());
            Handles.CubeCap(0, new Vector3(x, y, z), Quaternion.identity, 1f);
            SceneView.RepaintAll();
        }
    }

    private void Paint(bool isErase)
    {
        RaycastHit gHit;
        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        bool isHit = Physics.Raycast(worldRay, out gHit, 500f, 1 << LayerMask.NameToLayer("Editor"));
        WorldPos pos;

        if (isHit)
        {
            //Debug.Log("GHIT!");
            pos = EditTerrain.GetBlockPos(gHit, isErase ? false : true);
            //Debug.Log(pos.ToString());

            world.SetBlock(pos.x, pos.y, pos.z, isErase ? new BlockAir() : new Block());
            Chunk chunk = world.GetChunk(pos.x, pos.y, pos.z);

            if (chunk) {
				if(!dirtyChunks.ContainsKey(pos))
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
		bool isHit = Physics.Raycast(worldRay, out gHit, 500f, 1 << LayerMask.NameToLayer("EditorLevel"));
		WorldPos pos;

		if (isHit)
		{
			//Debug.Log("GHIT!");
			pos = EditTerrain.GetBlockPos(gHit, isErase ? false : true);
			//Debug.Log(pos.ToString());

			world.SetBlock(pos.x, pos.y, pos.z, isErase ? new BlockAir() : new Block());
			Chunk chunk = world.GetChunk(pos.x, pos.y, pos.z);

			if (chunk) {
				if(!dirtyChunks.ContainsKey(pos))
					dirtyChunks.Add(pos, chunk);
				chunk.UpdateMeshFilter();
				SceneView.RepaintAll();
			}
		}
	}
	//----------------

    private void PlaceObject()
    {
        RaycastHit gHit;
        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		bool isHit = Physics.Raycast(worldRay, out gHit, 500f, 1 << LayerMask.NameToLayer("Editor"));
        WorldPos pos;

        if (isHit)
        {
        }
    }

	private void UpdateDirtyChunks() {
		foreach (KeyValuePair<WorldPos, Chunk> c in dirtyChunks) {
			c.Value.UodateMeshCollider();
		}
		dirtyChunks.Clear ();
	}
}

/*
using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

[CustomEditor(typeof(VoxelMap))]
public class VoxelMapEditor : Editor
{
    VoxelMap voxelMap;
	VoxelGrid voxelGrid;
	private int editY = 0;

    public void OnEnable()
    {
        voxelMap = (VoxelMap)target;
    }

	void OnSceneGUI()
	{
		int controlID = GUIUtility.GetControlID (FocusType.Passive);
		Event e = Event.current;

		if (e.control) {
			if (e.type == EventType.MouseDown)
			{
				Event.current.Use();
				bool state = false;
				if (e.button == 0)
					state = true;
				else if (e.button == 1)
					state = false;

				Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
				RaycastHit hitInfo;

				if (Physics.Raycast (worldRay, out hitInfo)) {
					if (hitInfo.collider.gameObject.name == target.name) {
						Vector3 pos = hitInfo.collider.gameObject.transform.InverseTransformPoint (hitInfo.point);
						if (state)
							voxelMap.EditEdge (pos, Tile.eEdgeType.STAIR);
						else
							voxelMap.EditEdge(pos, Tile.eEdgeType.NONE);
					}
				}
				GUIUtility.hotControl = 0;
				Event.current.Use ();
			}
		}

		if (e.alt) {
            if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button <= 1)
            {
                Event.current.Use();
                bool state = false;
                if (e.button == 0)
                    state = true;
                else if (e.button == 1)
                    state = false;

                Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
				RaycastHit hitInfo;

				if (Physics.Raycast (worldRay, out hitInfo)) {
					if (hitInfo.collider.gameObject.name == target.name) {
						Vector3 pos = hitInfo.collider.gameObject.transform.InverseTransformPoint (hitInfo.point);
                        if (state)
						    voxelMap.EditEdge (pos, Tile.eEdgeType.RAIL);
                        else
                            voxelMap.EditEdge(pos, Tile.eEdgeType.NONE);
                    }
				}
				GUIUtility.hotControl = 0;
				Event.current.Use ();
			}
		}
		if (e.shift) {
			
				
			if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button <= 1) {
				Event.current.Use ();
				bool state = false;
				if (e.button == 0)
					state = true;
				else if (e.button == 1)
					state = false;
			
				Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
				RaycastHit hitInfo;

				if (Physics.Raycast (worldRay, out hitInfo)) {
					if (hitInfo.collider.gameObject.name == target.name) {
						voxelMap.EditVoxel (hitInfo.collider.gameObject.transform.InverseTransformPoint (hitInfo.point), state);
					}
				}
				GUIUtility.hotControl = 0;
                Event.current.Use ();
			}

			if (e.type == EventType.ScrollWheel) {
				Debug.Log (e.delta.ToString ());
				if (e.delta.y< 0)
                    editY++;
				if (e.delta.y > 0)
					editY--;

				editY = Mathf.Clamp (editY, 0, voxelMap.yCount - 1);
				voxelMap.ChangeEditY (editY);
				Event.current.Use ();
			}
		}
	}

    public override void OnInspectorGUI()
{
    GUILayout.BeginHorizontal();
    GUILayout.Label(" Width ");
    voxelMap.xCount = EditorGUILayout.IntField(voxelMap.xCount, GUILayout.Width(40));
    GUILayout.Label(" Depth ");
    voxelMap.zCount = EditorGUILayout.IntField(voxelMap.zCount, GUILayout.Width(40));
    GUILayout.Label(" Height ");
    voxelMap.yCount = EditorGUILayout.IntField(voxelMap.yCount, GUILayout.Width(40));
    GUILayout.EndHorizontal();

    GUILayout.BeginHorizontal();
    voxelMap.tileSize = EditorGUILayout.Vector3Field("Tile Size", voxelMap.tileSize);
    GUILayout.EndHorizontal();

    //增加讀tile的相關設定
    voxelMap.tilePath = EditorGUILayout.TextField("Tile Path", voxelMap.tilePath);
    voxelMap.oldCode = EditorGUILayout.Toggle("is Old Code?", voxelMap.oldCode);

    GUILayout.BeginHorizontal();
    if (GUILayout.Button("Init"))
    {
        voxelMap.Init();
    }
    GUILayout.EndHorizontal();

    EditorGUILayout.IntField("Edit Y", voxelMap.editY);

    SceneView.RepaintAll();
}
}*/
