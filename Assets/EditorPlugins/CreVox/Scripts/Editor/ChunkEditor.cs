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

		Color defColor;
		Color volColor;

		void OnEnable ()
		{
			chunk = (Chunk)target;
			workbbool.layerMin = 0;
			workbbool.layerMax = Chunk.chunkSize;

			workbbool.blockAirs = new bool[chunk.cData.blockAirs.Count];
			workbbool.blockHolds = new bool[chunk.cData.blockHolds.Count];

			defColor = GUI.color;
			volColor = new Color (0.5f, 0.8f, 0.75f);
		}

		public override void OnInspectorGUI ()
		{
			string valueMinMax;
			EditorGUIUtility.wideMode = true;

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
					workbbool.filter = EditorGUILayout.ToggleLeft ("Filter", workbbool.filter, GUILayout.Width (45));
					if (workbbool.filter)
						EditorGUILayout.MinMaxSlider (ref workbbool.layerMin, ref workbbool.layerMax, 0f, (float)Chunk.chunkSize);
					workbbool.layerMin = (int)workbbool.layerMin;
					workbbool.layerMax = (int)workbbool.layerMax;
					valueMinMax = workbbool.layerMin + "~" + workbbool.layerMax;
				}
				EditorGUILayout.LabelField ("Layer = " + (workbbool.filter ? valueMinMax : "all"));

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
			workbbool.showBlocks = EditorGUILayout.Foldout (workbbool.showBlocks, " Block(" + chunk.cData.blocks.Count);
			if (workbbool.showBlocks) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < chunk.cData.blocks.Count; i++) {
					Block workBlock = chunk.cData.blocks [i];
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

		public void DrawBlockAir ()
		{
			workbbool.showBlockAirs = EditorGUILayout.Foldout (workbbool.showBlockAirs, " BlockAir(" + chunk.cData.blockAirs.Count);
			if (workbbool.showBlockAirs) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < chunk.cData.blockAirs.Count; i++) {
					BlockAir workAir = chunk.cData.blockAirs [i];
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

		public void DrawBlockHold ()
		{
			workbbool.showBlockHolds = EditorGUILayout.Foldout (workbbool.showBlockHolds, " BlockHold(" + chunk.cData.blockHolds.Count);
			if (workbbool.showBlockHolds) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < chunk.cData.blockHolds.Count; i++) {
					BlockHold workHold = chunk.cData.blockHolds [i];
					if (workbbool.filter ? (workHold.BlockPos.y >= workbbool.layerMin && workHold.BlockPos.y <= workbbool.layerMax) : true) {
						if (workHold.roots.Count > 0) {
							workbbool.blockHolds [i] = EditorGUILayout.Foldout (workbbool.blockHolds [i],
								"[" + workHold.BlockPos.x +
								"," + workHold.BlockPos.y +
								"," + workHold.BlockPos.z +
								"]" + ((workHold.IsSolid (Direction.south)) ? "(Solid)" : "")
							);
							if (workbbool.blockHolds [i]) {
								for (int j = 0; j < workHold.roots.Count; j++)
									EditorGUILayout.LabelField ("[" + workHold.roots [j].blockPos.ToString () + "]" +
									" Marker(" + workHold.roots [j].pieceID + ")",
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
