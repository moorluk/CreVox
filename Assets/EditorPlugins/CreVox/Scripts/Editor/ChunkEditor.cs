using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(Chunk))]
	public class ChunkEditor : Editor
	{

		bool showBlocks = false;
		bool showBlockAirs = false;
		bool[] blockAirs;
		bool showBlockHolds = false;
		bool[] blockHolds;

		bool filter;
		float layerMin;
		float layerMax;

		bool drawDef = false;
		[SerializeField]Chunk chunk;

		Color defColor;
		Color volColor;

		void OnEnable ()
		{
			chunk = (Chunk)target;
			layerMin = 0;
			layerMax = Chunk.chunkSize;

//			blocks = new bool[0];
			blockAirs = new bool[0];
			blockHolds = new bool[0];

			defColor = GUI.color;
			volColor = new Color (0.5f, 0.8f, 0.75f);
		}

		void UpdateList()
		{
//			if (blocks.Length != chunk.cData.blocks.Count) {
//				blocks = new bool[chunk.cData.blocks.Count];
//				for (int i = 0; i < blocks.Length; i++) {
//					blocks [i] = false;
//				}
//			}

			if (blockAirs.Length != chunk.cData.blockAirs.Count) {
				blockAirs = new bool[chunk.cData.blockAirs.Count];
				for (int j = 0; j < blockAirs.Length; j++) {
					blockAirs [j] = false;
				}
			}

			if (blockHolds.Length != chunk.cData.blockHolds.Count) {
				blockHolds = new bool[chunk.cData.blockHolds.Count];
				for (int k = 0; k < blockHolds.Length; k++) {
					blockHolds [k] = false;
				}
			}
		}

		public override void OnInspectorGUI ()
		{
			string valueMinMax;
			EditorGUIUtility.wideMode = true;

			UpdateList ();

			using (var v = new EditorGUILayout.VerticalScope ("Box")) {
				GUI.color = volColor;
				using (var h1 = new EditorGUILayout.HorizontalScope (EditorStyles.helpBox)) {
					GUI.color = defColor;
					float labelDefW = EditorGUIUtility.labelWidth;
					EditorGUIUtility.labelWidth = 15f;
					EditorGUILayout.LabelField ("Chunk", EditorStyles.boldLabel, GUILayout.Width (50));
					EditorGUILayout.IntField ("X", chunk.cData.ChunkPos.x, GUILayout.Width (40));
					EditorGUILayout.IntField ("Y", chunk.cData.ChunkPos.y, GUILayout.Width (40));
					EditorGUILayout.IntField ("Z", chunk.cData.ChunkPos.z, GUILayout.Width (40));
					EditorGUIUtility.labelWidth = labelDefW;
				}

				using (var h2 = new EditorGUILayout.HorizontalScope ()) {
					filter = EditorGUILayout.ToggleLeft ("Filter", filter, GUILayout.Width (45));
					if (filter)
						EditorGUILayout.MinMaxSlider (ref layerMin, ref layerMax, 0f, (float)Chunk.chunkSize);
					layerMin = (int)layerMin;
					layerMax = (int)layerMax;
					valueMinMax = layerMin + "~" + layerMax;
				}
				EditorGUILayout.LabelField ("Layer = " + (filter ? valueMinMax : "all"));

				EditorGUI.indentLevel++;
				DrawBlock ();
				DrawBlockAir ();
				DrawBlockHold ();
			}

			drawDef = EditorGUILayout.Foldout (drawDef, "Default");
			if (drawDef)
				DrawDefaultInspector ();
		}

		public void DrawBlock ()
		{
			showBlocks = EditorGUILayout.Foldout (showBlocks, " Block(" + chunk.cData.blocks.Count);
			if (showBlocks) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < chunk.cData.blocks.Count; i++) {
					if (filter ? (chunk.cData.blocks [i].BlockPos.y >= layerMin && chunk.cData.blocks [i].BlockPos.y <= layerMax) : true) {
						EditorGUILayout.LabelField (
							"[" + chunk.cData.blocks [i].BlockPos.x +
							"," + chunk.cData.blocks [i].BlockPos.y +
							"," + chunk.cData.blocks [i].BlockPos.z +
							"]"
						);
					}
				}
				EditorGUI.indentLevel--;
			}
		}

		public void DrawBlockAir ()
		{
			showBlockAirs = EditorGUILayout.Foldout (showBlockAirs, " BlockAir(" + chunk.cData.blockAirs.Count);
			if (showBlockAirs) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < chunk.cData.blockAirs.Count; i++) {
					if (filter ? (chunk.cData.blockAirs [i].BlockPos.y >= layerMin && chunk.cData.blockAirs [i].BlockPos.y <= layerMax) : true) {
						if (chunk.cData.blockAirs [i].pieceNames != null) {
							blockAirs [i] = EditorGUILayout.Foldout (blockAirs [i], 
								"[" + chunk.cData.blockAirs [i].BlockPos.x +
								"," + chunk.cData.blockAirs [i].BlockPos.y +
								"," + chunk.cData.blockAirs [i].BlockPos.z +
								"]"
							);
							if (blockAirs [i]) {
								GUILayout.SelectionGrid (-1, chunk.cData.blockAirs [i].pieceNames, 3
									, EditorStyles.miniButton
									, GUILayout.Width(Screen.width-45));
							}
						}
					}
				}
				EditorGUI.indentLevel--;
			}
		}

		public void DrawBlockHold ()
		{
			showBlockHolds = EditorGUILayout.Foldout (showBlockHolds, " BlockHold(" + chunk.cData.blockHolds.Count);
			if (showBlockHolds) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < chunk.cData.blockHolds.Count; i++) {
					if (filter ? (chunk.cData.blockHolds [i].BlockPos.y >= layerMin && chunk.cData.blockHolds [i].BlockPos.y <= layerMax) : true) {
						if (chunk.cData.blockHolds [i].roots.Count > 0) {
							blockHolds [i] = EditorGUILayout.Foldout (blockHolds [i],
								"[" + chunk.cData.blockHolds [i].BlockPos.x +
								"," + chunk.cData.blockHolds [i].BlockPos.y +
								"," + chunk.cData.blockHolds [i].BlockPos.z +
								"]" + ((chunk.cData.blockHolds [i].IsSolid (Direction.south)) ? "(Solid)" : "")
							);
							if (blockHolds [i]) {
								for (int j = 0; j < chunk.cData.blockHolds [i].roots.Count; j++)
									EditorGUILayout.LabelField ("[" + chunk.cData.blockHolds [i].roots [j].blockPos.ToString () + "]" +
										" Marker(" + chunk.cData.blockHolds [i].roots [j].pieceID + ")",
										EditorStyles.helpBox);
							}
						}
					}
				}
				EditorGUI.indentLevel--;
			}
		}
	}
}
