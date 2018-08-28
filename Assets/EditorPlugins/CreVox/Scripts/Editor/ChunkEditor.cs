using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace CreVox
{
	[CustomEditor (typeof(Chunk))]
	public class ChunkEditor : Editor
	{
		VolumeDataEditor.BlockBool workbbool;

		bool drawDef = false;
		[SerializeField]Chunk chunk;

		public static Color volColor = new Color (0.5f, 0.8f, 0.75f);

		void OnEnable ()
		{
			chunk = (Chunk)target;
			workbbool.layerMin = 0;
			workbbool.layerMax = chunk.cData.isFreeChunk ? chunk.cData.freeChunkSize.y : Chunk.chunkSize;

			workbbool.blockAirs = new bool[chunk.cData.blockAirs.Count];
		}

		public override void OnInspectorGUI ()
		{
			EditorGUIUtility.wideMode = true;
			ChunkEditor.DrawChunkData (chunk.cData, ref workbbool);

			drawDef = EditorGUILayout.Foldout (drawDef, "Default");
			if (drawDef) {
				EditorGUI.indentLevel++;
				DrawDefaultInspector ();
				EditorGUI.indentLevel--;
			}
		}
		public static void DrawChunkData(ChunkData cd, ref VolumeDataEditor.BlockBool workbbool)
		{
			string valueMinMax;
			Color defColor = GUI.color;
			using (var v = new EditorGUILayout.VerticalScope ("Box")) {
				GUI.color = volColor;
				using (var h1 = new EditorGUILayout.HorizontalScope (EditorStyles.helpBox)) {
					GUI.color = defColor;
					float labelDefW = EditorGUIUtility.labelWidth;
					EditorGUIUtility.labelWidth = 15f;
					if (cd.isFreeChunk) {
						EditorGUILayout.LabelField ("Free Size Chunk", EditorStyles.boldLabel, GUILayout.Width (110));
						EditorGUILayout.LabelField ("X", cd.freeChunkSize.x.ToString(), GUILayout.Width (40));
						EditorGUILayout.LabelField ("Y", cd.freeChunkSize.y.ToString(), GUILayout.Width (40));
						EditorGUILayout.LabelField ("Z", cd.freeChunkSize.z.ToString(), GUILayout.Width (40));
					} else {
						EditorGUILayout.LabelField ("Chunk", EditorStyles.boldLabel, GUILayout.Width (50));
						EditorGUILayout.IntField ("X", cd.ChunkPos.x, GUILayout.Width (40));
						EditorGUILayout.IntField ("Y", cd.ChunkPos.y, GUILayout.Width (40));
						EditorGUILayout.IntField ("Z", cd.ChunkPos.z, GUILayout.Width (40));
					}
					EditorGUIUtility.labelWidth = labelDefW;
				}

				using (var h2 = new EditorGUILayout.HorizontalScope ()) {
					workbbool.filter = EditorGUILayout.ToggleLeft ("Filter", workbbool.filter, GUILayout.Width (45));
					if (workbbool.filter)
						EditorGUILayout.MinMaxSlider (ref workbbool.layerMin, ref workbbool.layerMax, 0f, (float)(cd.isFreeChunk ? cd.freeChunkSize.y : Chunk.chunkSize));
					workbbool.layerMin = (int)workbbool.layerMin;
					workbbool.layerMax = (int)workbbool.layerMax;
					valueMinMax = workbbool.layerMin + "~" + workbbool.layerMax;
				}
				EditorGUILayout.LabelField ("Layer = " + (workbbool.filter ? valueMinMax : "all"));

				EditorGUI.indentLevel++;

				DrawBlock (ref cd, ref workbbool);
				DrawBlockAir (ref cd, ref workbbool);

				EditorGUI.indentLevel--;
			}
		}
		static void DrawBlock (ref ChunkData cd, ref VolumeDataEditor.BlockBool workbbool)
		{
			workbbool.showBlocks = EditorGUILayout.Foldout (workbbool.showBlocks, " Block(" + cd.blocks.Count);
			if (workbbool.showBlocks) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < cd.blocks.Count; i++) {
					Block workBlock = cd.blocks [i];
					if (workbbool.filter ? (workBlock.BlockPos.y >= workbbool.layerMin && workBlock.BlockPos.y <= workbbool.layerMax) : true) {
						EditorGUILayout.LabelField (
							"[" + workBlock.BlockPos.x +
							"," + workBlock.BlockPos.y +
							"," + workBlock.BlockPos.z +
							"]"
						);
					}
				}
				EditorGUI.indentLevel--;
			}
		}
		static void DrawBlockAir (ref ChunkData cd, ref VolumeDataEditor.BlockBool workbbool)
		{
			workbbool.showBlockAirs = EditorGUILayout.Foldout (workbbool.showBlockAirs, " BlockAir(" + cd.blockAirs.Count);
			if (workbbool.showBlockAirs) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < cd.blockAirs.Count; i++) {
					BlockAir workAir = cd.blockAirs [i];
					if (workbbool.filter ? (workAir.BlockPos.y >= workbbool.layerMin && workAir.BlockPos.y <= workbbool.layerMax) : true) {
						if (workAir.pieceNames != null) {
							workbbool.blockAirs [i] = EditorGUILayout.Foldout (workbbool.blockAirs [i], 
								"[" + workAir.BlockPos.x +
								"," + workAir.BlockPos.y +
								"," + workAir.BlockPos.z +
								"]"
							);
							if (workbbool.blockAirs [i]) {
								GUILayout.SelectionGrid (-1, workAir.pieceNames, 3
									, EditorStyles.miniButton
									, GUILayout.Width(Screen.width-45));
							}
						}
					}
				}
				EditorGUI.indentLevel--;
			}
		}
	}
}
