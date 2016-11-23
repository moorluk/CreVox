using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(Chunk))]
	public class ChunkEditor : Editor
	{

		bool showBlocks = false;
		bool[] blocks;
		bool showBlockAirs = false;
		bool[] blockAirs;

		bool filter;
		float layerMin;
		float layerMax;

		bool drawDef = false;
		[SerializeField]Chunk chunk;

		Color defColor = GUI.color;
		Color volColor = new Color (0.5f, 0.8f, 0.75f);

		void OnEnable ()
		{
			chunk = (Chunk)target;
			layerMin = 0;
			layerMax = Chunk.chunkSize;

			blocks = new bool[0];
			blockAirs = new bool[0];
		}

		void UpdateList()
		{
			if (blocks.Length != chunk.blocks.Count) {
				blocks = new bool[chunk.blocks.Count];
				for (int i = 0; i < blocks.Length; i++) {
					blocks [i] = false;
				}
			}

			if (blockAirs.Length != chunk.blockAirs.Count) {
				blockAirs = new bool[chunk.blockAirs.Count];
				for (int j = 0; j < blockAirs.Length; j++) {
					blockAirs [j] = false;
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
					EditorGUILayout.IntField ("X", chunk.pos.x, GUILayout.Width (40));
					EditorGUILayout.IntField ("Y", chunk.pos.y, GUILayout.Width (40));
					EditorGUILayout.IntField ("Z", chunk.pos.z, GUILayout.Width (40));
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
			}

			drawDef = EditorGUILayout.Foldout (drawDef, "Default");
			if (drawDef)
				DrawDefaultInspector ();
		}

		public void DrawBlock ()
		{
			showBlocks = EditorGUILayout.Foldout (showBlocks, " Block(" + chunk.blocks.Count);
			if (showBlocks) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < chunk.blocks.Count; i++) {
					if (filter ? (chunk.blocks [i].BlockPos.y >= layerMin && chunk.blocks [i].BlockPos.y <= layerMax) : true) {
						EditorGUILayout.LabelField (
							"[" + chunk.blocks [i].BlockPos.x +
							"," + chunk.blocks [i].BlockPos.y +
							"," + chunk.blocks [i].BlockPos.z +
							"](Voxel)"
						);
					}
				}
				EditorGUI.indentLevel--;
			}
		}

		public void DrawBlockAir ()
		{
			showBlockAirs = EditorGUILayout.Foldout (showBlockAirs, " BlockAir(" + chunk.blockAirs.Count);
			if (showBlockAirs) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < chunk.blockAirs.Count; i++) {
					if (filter ? (chunk.blockAirs [i].BlockPos.y >= layerMin && chunk.blockAirs [i].BlockPos.y <= layerMax) : true) {
						if (chunk.blockAirs [i].pieceNames != null) {
							blockAirs [i] = EditorGUILayout.Foldout (blockAirs [i], 
								"[" + chunk.blockAirs [i].BlockPos.x +
								"," + chunk.blockAirs [i].BlockPos.y +
								"," + chunk.blockAirs [i].BlockPos.z +
								"]"
							);
							if (blockAirs [i]) {
								GUILayout.SelectionGrid (-1, chunk.blockAirs [i].pieceNames, 3
									, EditorStyles.miniButton,GUILayout.Width(Screen.width-45));
							}
						}
					}
				}
				EditorGUI.indentLevel--;
			}
		}
	}
}
