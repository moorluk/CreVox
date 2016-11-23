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
			public bool showThis;
			public bool[] showSingle;

			public bool filter;
			public float layerMin;
			public float layerMax;
		}
		BlockBool[] cb;

		bool drawDef = false;
		public VolumeData vd;

		Color defColor = GUI.color;
		Color volColor = new Color (0.5f, 0.8f, 0.75f);

		void Init ()
		{
			vd = (VolumeData)target;
			cb = new BlockBool[vd.chunkDatas.Count];
			for (int i = 0; i < cb.Length; i++) {
				cb [i].showThis = false;
				cb [i].showSingle = new bool[vd.chunkDatas [i].blocks.Count];
				for (int j = 0; j < cb [i].showSingle.Length; j++) {
					cb [i].showSingle [j] = false;
				}
				cb [i].layerMin = 0;
				cb [i].layerMax = Chunk.chunkSize;
			}
		}

		public override void OnInspectorGUI ()
		{
			Init ();

			EditorGUIUtility.wideMode = true;

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

				EditorGUI.indentLevel++;
				cb [_index].showThis = EditorGUILayout.Foldout (cb [_index].showThis
					, "Blocks: (Layer = " 
					+ (cb [_index].filter ? valueMinMax : "all-" + vd.chunkDatas [_index].blocks.Count) 
					+ ")"
				);

				if (cb [_index].showThis) {
					var Blocks = vd.chunkDatas [_index].blocks;
					for (int i = 0; i < Blocks.Count; i++) {
						if (cb [_index].filter ? (Blocks [i].BlockPos.y >= cb [_index].layerMin && Blocks [i].BlockPos.y <= cb [_index].layerMax) : true) {
							if (Blocks [i] is BlockAir) {
								BlockAir bAir = Blocks [i] as BlockAir;
								if (bAir.pieceNames != null) {
									cb [_index].showSingle [i] = EditorGUILayout.Foldout (cb [_index].showSingle [i], 
										"[" + Blocks [i].BlockPos.x + 
										"," + Blocks [i].BlockPos.y + 
										"," + Blocks [i].BlockPos.z + 
										"]"
									);
									if (cb [_index].showSingle [i]) {
										GUILayout.SelectionGrid (-1, bAir.pieceNames, 3, EditorStyles.miniButton);
									}
								}
							} else {
								if (!(Blocks [i] == null))
									EditorGUILayout.LabelField (
										"[" + Blocks [i].BlockPos.x + 
										"," + Blocks [i].BlockPos.y + 
										"," + Blocks [i].BlockPos.z + 
										"](Voxel)"
									);
							}
						}
					}
				}
				EditorGUI.indentLevel--;
			}
		}
	}
}
