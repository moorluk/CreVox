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
			public bool showBlockHolds;
			public bool[] blockHolds;

			public bool filter;
			public float layerMin;
			public float layerMax;
		}
		BlockBool[] bbool;
		public bool[] blockItems;
		BlockBool workbbool;
		ChunkData workCData;

		bool drawDef = false;
		public VolumeData vd;

		Color defColor;
		Color volColor;

		void OnEnable ()
		{
			vd = (VolumeData)target;
			defColor = GUI.color;
			volColor = new Color (0.5f, 0.8f, 0.75f);

			int cdsCount = (vd == null) ? 0 : vd.chunkDatas.Count;
			bbool = new BlockBool[cdsCount];
			for (int i = 0; i < bbool.Length; i++) {
				bbool [i].blocks = new bool[vd.chunkDatas [i].blocks.Count];
				bbool [i].blockAirs = new bool[vd.chunkDatas [i].blockAirs.Count];
				bbool [i].blockHolds = new bool[vd.chunkDatas [i].blockHolds.Count];
				bbool[i].layerMin = 0;
				bbool[i].layerMax = Chunk.chunkSize;
			}
			blockItems = new bool[vd.blockItems.Count];

			UpdateList ();
		}

		void UpdateList()
		{
			if (blockItems.Length != vd.blockItems.Count) {
				blockItems = new bool[vd.blockItems.Count];
				for (int j = 0; j < blockItems.Length; j++) {
					blockItems [j] = false;
				}
			}
			for(int i = 0; i < bbool.Length; i++) {
				if (bbool[i].blocks.Length != vd.chunkDatas[i].blocks.Count) {
					bbool[i].blocks = new bool[vd.chunkDatas[i].blocks.Count];
					for (int j = 0; j < bbool[i].blocks.Length; j++) {
						bbool[i].blocks [j] = false;
					}
				}

				if (bbool[i].blockAirs.Length != vd.chunkDatas[i].blockAirs.Count) {
					bbool[i].blockAirs = new bool[vd.chunkDatas[i].blockAirs.Count];
					for (int j = 0; j < bbool[i].blockAirs.Length; j++) {
						bbool[i].blockAirs [j] = false;
					}
				}

				if (bbool[i].blockHolds.Length != vd.chunkDatas[i].blockHolds.Count) {
					bbool[i].blockHolds = new bool[vd.chunkDatas[i].blockHolds.Count];
					for (int j = 0; j < bbool[i].blockHolds.Length; j++) {
						bbool[i].blockHolds [j] = false;
					}
				}
			}
		}

		public override void OnInspectorGUI ()
		{
			EditorGUIUtility.wideMode = true;
			UpdateList ();

			EditorGUI.BeginChangeCheck ();
			DrawItem ();
			for (int _index = 0; _index < vd.chunkDatas.Count; _index++) {
				DrawChunkData (_index);
			}

			if (GUI.changed)
				EditorUtility.SetDirty (vd);

			drawDef = EditorGUILayout.Foldout (drawDef, "Default");
			if (drawDef)
				DrawDefaultInspector ();
			if (EditorGUI.EndChangeCheck ())
				EditorUtility.SetDirty (vd);
		}

		void DrawItem()
		{
			using (var v = new EditorGUILayout.VerticalScope ("Box")) {
				float labelDefW = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 70f;
				GUI.color = volColor;
				using (var h1 = new EditorGUILayout.HorizontalScope (EditorStyles.helpBox)) {
					GUI.color = defColor;
					EditorGUILayout.LabelField ("Items", EditorStyles.boldLabel, GUILayout.Width (45));
				}

				EditorGUI.indentLevel++;
				DrawBlockItem ();
				EditorGUI.indentLevel--;
				EditorGUIUtility.labelWidth = labelDefW;
			}
		}

		void DrawChunkData(int _index)
		{
			string valueMinMax;
			workbbool = bbool[_index];
			workCData = vd.chunkDatas [_index];

			using (var v = new EditorGUILayout.VerticalScope ("Box")) {
				GUI.color = volColor;
				using (var h1 = new EditorGUILayout.HorizontalScope (EditorStyles.helpBox)) {
					GUI.color = defColor;
					float labelDefW = EditorGUIUtility.labelWidth;
					EditorGUIUtility.labelWidth = 15f;
					EditorGUILayout.LabelField ("Chunk", EditorStyles.boldLabel, GUILayout.Width (45));
					using (var v1 = new EditorGUILayout.VerticalScope ()) {
						using (var h2 = new EditorGUILayout.HorizontalScope ()) {
							EditorGUILayout.IntField ("X", workCData.ChunkPos.x, GUILayout.Width (40));
							EditorGUILayout.IntField ("Y", workCData.ChunkPos.y, GUILayout.Width (40));
							EditorGUILayout.IntField ("Z", workCData.ChunkPos.z, GUILayout.Width (40));
							EditorGUIUtility.labelWidth = labelDefW;
						}
					}
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

				EditorGUI.indentLevel --;
			}

			bbool [_index] = workbbool;
			vd.chunkDatas [_index] = workCData;
		}

		public void DrawBlock ()
		{
			workbbool.showBlocks = EditorGUILayout.Foldout (workbbool.showBlocks, " Block(" + workCData.blocks.Count);
			if (workbbool.showBlocks) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < workCData.blocks.Count; i++) {
					Block workBlock = workCData.blocks [i];
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
			workbbool.showBlockAirs = EditorGUILayout.Foldout (workbbool.showBlockAirs, " BlockAir(" + workCData.blockAirs.Count);
			if (workbbool.showBlockAirs) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < workCData.blockAirs.Count; i++) {
					BlockAir workAir = workCData.blockAirs [i];
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
			workbbool.showBlockHolds = EditorGUILayout.Foldout (workbbool.showBlockHolds, " BlockHold(" + workCData.blockHolds.Count);
			if (workbbool.showBlockHolds) {
				EditorGUI.indentLevel++;
				for (int i = 0; i < workCData.blockHolds.Count; i++) {
					BlockHold workHold = workCData.blockHolds [i];
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

		public void DrawBlockItem ()
		{
			for (int i = 0; i < vd.blockItems.Count; i++) {
				BlockItem workItem = vd.blockItems [i];
				blockItems [i] = EditorGUILayout.Foldout (
					blockItems [i],
					"[" + workItem.BlockPos.x +
					"," + workItem.BlockPos.y +
					"," + workItem.BlockPos.z +
					"] " + workItem.pieceName
				);
				if (blockItems [i]) {
					Vector3 pos = new Vector3 (workItem.posX, workItem.posY, workItem.posZ);
					pos = EditorGUILayout.Vector3Field ("Position",pos);
					workItem.posX = pos.x;
					workItem.posY = pos.y;
					workItem.posZ = pos.z;

					Quaternion rot = new Quaternion (workItem.rotX, workItem.rotY, workItem.rotZ, workItem.rotW);
					rot.eulerAngles = EditorGUILayout.Vector3Field ("Rotation", rot.eulerAngles);
					workItem.rotX = rot.x;
					workItem.rotY = rot.y;
					workItem.rotZ = rot.z;
					workItem.rotW = rot.w;

                    for(int j =0; j < workItem.attributes.Length; j++)
						workItem.attributes[j] = EditorGUILayout.TextField("Attrib" + j.ToString(), workItem.attributes[j]);
                }
			}
		}
	}
}
