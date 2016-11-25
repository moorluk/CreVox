using UnityEngine;
using System.Collections.Generic;
using CreVox;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(VolumeData))]
	public class VolumeDataEditor : Editor
	{
		public struct BlockBool
		{
			public bool showBlocks;
			public bool[] blocks;
			public bool showBlockAirs;
			public bool[] blockAirs;

			public bool filter;
			public float layerMin;
			public float layerMax;
		}
		BlockBool[] cb;

		bool drawDef = false;
		public VolumeData vd;

		Color defColor;
		Color volColor;

		void OnEnable ()
		{
			vd = (VolumeData)target;
			defColor = GUI.color;
			volColor = new Color (0.5f, 0.8f, 0.75f);

			cb = new BlockBool[vd.chunkDatas.Count];
			for (int i = 0; i < cb.Length; i++) {
				cb [i].blocks = new bool[vd.chunkDatas [i].blocks.Count];
				for (int j = 0; j < cb [i].blocks.Length; j++) {
					cb [i].blocks [j] = false;
				}

				cb [i].blockAirs = new bool[vd.chunkDatas [i].blockAirs.Count];
				for (int k = 0; k < cb [i].blockAirs.Length; k++) {
					cb [i].blockAirs [k] = false;
				}
			}

			for(int i = 0; i < cb.Length; i++) {
				cb[i].layerMin = 0;
				cb[i].layerMax = Chunk.chunkSize;
			}
			UpdateList ();
		}

		void UpdateList()
		{
			for(int i = 0; i < cb.Length; i++) {
				if (cb[i].blocks.Length != vd.chunkDatas[i].blocks.Count) {
					cb[i].blocks = new bool[vd.chunkDatas[i].blocks.Count];
					for (int j = 0; j < cb[i].blocks.Length; j++) {
						cb[i].blocks [j] = false;
					}
				}

				if (cb[i].blockAirs.Length != vd.chunkDatas[i].blockAirs.Count) {
					cb[i].blockAirs = new bool[vd.chunkDatas[i].blockAirs.Count];
					for (int k = 0; k < cb[i].blockAirs.Length; k++) {
						cb[i].blockAirs [k] = false;
					}
				}
			}
		}

		public override void OnInspectorGUI ()
		{
			EditorGUIUtility.wideMode = true;

			UpdateList ();

			for (int _index = 0; _index < vd.chunkDatas.Count; _index++) {
				DrawChunkData (_index);
			}

			if (GUI.changed)
				EditorUtility.SetDirty (vd);

			drawDef = EditorGUILayout.Foldout (drawDef, "Default");
			if (drawDef)
				DrawDefaultInspector ();
		}

		public void DrawChunkData(int _index)
		{
			string valueMinMax;

			using (var v = new EditorGUILayout.VerticalScope ("Box")) {
				GUI.color = volColor;
				using (var h1 = new EditorGUILayout.HorizontalScope (EditorStyles.helpBox)) {
					GUI.color = defColor;
					float labelDefW = EditorGUIUtility.labelWidth;
					EditorGUIUtility.labelWidth = 15f;
					EditorGUILayout.LabelField ("Chunk", EditorStyles.boldLabel, GUILayout.Width (50));
					EditorGUILayout.IntField ("X", vd.chunkDatas [_index].ChunkPos.x, GUILayout.Width (40));
					EditorGUILayout.IntField ("Y", vd.chunkDatas [_index].ChunkPos.y, GUILayout.Width (40));
					EditorGUILayout.IntField ("Z", vd.chunkDatas [_index].ChunkPos.z, GUILayout.Width (40));
					EditorGUIUtility.labelWidth = labelDefW;
				}

				using (var h2 = new EditorGUILayout.HorizontalScope ()) {
					cb [_index].filter = EditorGUILayout.ToggleLeft ("Filter", cb [_index].filter, GUILayout.Width (45));
					if (cb [_index].filter)
						EditorGUILayout.MinMaxSlider (ref cb [_index].layerMin, ref cb [_index].layerMax, 0f, (float)Chunk.chunkSize);
					cb [_index].layerMin = (int)cb [_index].layerMin;
					cb [_index].layerMax = (int)cb [_index].layerMax;
					valueMinMax = cb [_index].layerMin + "~" + cb [_index].layerMax;
				}
				EditorGUILayout.LabelField ("Layer = " + (cb [_index].filter ? valueMinMax : "all"));

				EditorGUI.indentLevel++;
				cb [_index].showBlocks = EditorGUILayout.Foldout (cb [_index].showBlocks, " Block(" + cb [_index].blocks.Length);

				EditorGUI.indentLevel++;
				if (cb [_index].showBlocks) {
					List<Block> Blocks = vd.chunkDatas [_index].blocks;
					for (int i = 0; i < Blocks.Count; i++) {
						if (cb [_index].filter ? (Blocks [i].BlockPos.y >= cb [_index].layerMin && Blocks [i].BlockPos.y <= cb [_index].layerMax) : true) {
							if (Blocks [i] != null)
								EditorGUILayout.LabelField (
									"[" + Blocks [i].BlockPos.x +
									"," + Blocks [i].BlockPos.y +
									"," + Blocks [i].BlockPos.z +
									"](Voxel)"
								);
						}
					}
				}
				EditorGUI.indentLevel--;

				cb [_index].showBlockAirs = EditorGUILayout.Foldout (cb [_index].showBlockAirs, " BlockAir(" + cb [_index].blockAirs.Length);

				EditorGUI.indentLevel++;
				if (cb [_index].showBlockAirs) {
					List<BlockAir> BlockAirs = vd.chunkDatas [_index].blockAirs;
					for (int i = 0; i < BlockAirs.Count; i++) {
						if (cb [_index].filter ? (BlockAirs [i].BlockPos.y >= cb [_index].layerMin && BlockAirs [i].BlockPos.y <= cb [_index].layerMax) : true) {
							cb [_index].blockAirs [i] = EditorGUILayout.Foldout (cb [_index].blockAirs [i], 
								"[" + BlockAirs [i].BlockPos.x +
								"," + BlockAirs [i].BlockPos.y +
								"," + BlockAirs [i].BlockPos.z +
								"]"
							);
							if (cb [_index].blockAirs [i]) {
								GUILayout.SelectionGrid (-1, BlockAirs [i].pieceNames, 3
									, EditorStyles.miniButton
									, GUILayout.Width (Screen.width - 45));
							}
						}
					}
				}
				EditorGUI.indentLevel --;
//				EditorGUI.indentLevel --;
			}
		}
	}
}
