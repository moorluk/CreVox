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
        public bool[] blockBounds;
		BlockBool fbool;
		BlockBool workbbool;
		ChunkData workCData;

		bool drawDef = false;
		public VolumeData vd;

		Color defColor;

		void OnEnable ()
		{
			vd = (VolumeData)target;
			defColor = GUI.color;

			int cdsCount = (vd == null) ? 0 : vd.chunkDatas.Count;
			bbool = new BlockBool[cdsCount];
			for (int i = 0; i < bbool.Length; i++) {
				bbool [i].blocks = new bool[vd.chunkDatas [i].blocks.Count];
				bbool [i].blockAirs = new bool[vd.chunkDatas [i].blockAirs.Count];
				bbool [i].blockHolds = new bool[vd.chunkDatas [i].blockHolds.Count];
				bbool [i].layerMin = 0;
				bbool [i].layerMax = Chunk.chunkSize;
			}
			fbool = new BlockBool ();
			fbool.blocks = new bool[vd.freeChunk.blocks.Count];
			fbool.blockAirs = new bool[vd.freeChunk.blockAirs.Count];
			fbool.blockHolds = new bool[vd.freeChunk.blockHolds.Count];
			fbool.layerMin = 0;
			fbool.layerMax = vd.freeChunk.freeChunkSize.y;

            blockItems = new bool[vd.blockItems.Count];
            blockBounds = new bool[vd.blockBounds.Count];

			UpdateList ();
		}

		void UpdateList()
		{
			if (blockItems.Length != vd.blockItems.Count) {
				blockItems = new bool[vd.blockItems.Count];
            }
            if (blockBounds.Length != vd.blockBounds.Count) {
                blockBounds = new bool[vd.blockBounds.Count];
            }
			for(int i = 0; i < bbool.Length; i++) {
				if (bbool[i].blocks.Length != vd.chunkDatas[i].blocks.Count) {
					bbool[i].blocks = new bool[vd.chunkDatas[i].blocks.Count];
				}

				if (bbool[i].blockAirs.Length != vd.chunkDatas[i].blockAirs.Count) {
					bbool[i].blockAirs = new bool[vd.chunkDatas[i].blockAirs.Count];
				}

				if (bbool[i].blockHolds.Length != vd.chunkDatas[i].blockHolds.Count) {
					bbool[i].blockHolds = new bool[vd.chunkDatas[i].blockHolds.Count];
				}
			}
		}

		public override void OnInspectorGUI ()
		{
			EditorGUIUtility.wideMode = true;
			UpdateList ();

			EditorGUI.BeginChangeCheck ();
            DrawBlockBounds ();
			DrawBlockItems ();
			if (vd.useFreeChunk) {
				ChunkEditor.DrawChunkData (vd.freeChunk, ref fbool);
			} else {
                vd.chunkSize = EditorGUILayout.IntField ("Chunk Size", vd.chunkSize);
				for (int i = 0; i < vd.chunkDatas.Count; i++) {
                    ChunkEditor.DrawChunkData (vd.chunkDatas [i], ref bbool[i]);
				}
			}

			if (GUI.changed)
				EditorUtility.SetDirty (vd);

			drawDef = EditorGUILayout.Foldout (drawDef, "Default");
			if (drawDef)
				DrawDefaultInspector ();
			if (EditorGUI.EndChangeCheck ())
				EditorUtility.SetDirty (vd);
		}

        void DrawBlockBounds()
        {
            using (var v = new EditorGUILayout.VerticalScope ("Box")) {
                float labelDefW = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 70f;
                GUI.color = ChunkEditor.volColor;
                using (var h1 = new EditorGUILayout.HorizontalScope (EditorStyles.helpBox)) {
                    GUI.color = defColor;
                    EditorGUILayout.LabelField ("Bounds", EditorStyles.boldLabel, GUILayout.Width (45));
                }

                EditorGUI.indentLevel++;
                EditorGUIUtility.labelWidth = 15f;
                for (int i = 0; i < vd.blockBounds.Count; i++) {
                    DrawBlockBound (i);
                }
                EditorGUI.indentLevel--;
                EditorGUIUtility.labelWidth = labelDefW;
            }
        }

        void DrawBlockBound(int i)
        {
            BlockBound b = vd.blockBounds [i];
            int idl = EditorGUI.indentLevel;
            blockBounds [i] = EditorGUILayout.Foldout (blockBounds [i], i.ToString ());
            if (blockBounds [i]) {
                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    EditorGUILayout.LabelField ("Min", EditorStyles.boldLabel, GUILayout.Width (60));
                    EditorGUI.indentLevel = 0;
                    b.min.x = EditorGUILayout.IntField ("X", b.min.x, GUILayout.Width (40));
                    b.min.y = EditorGUILayout.IntField ("Y", b.min.y, GUILayout.Width (40));
                    b.min.z = EditorGUILayout.IntField ("Z", b.min.z, GUILayout.Width (40));
                    EditorGUI.indentLevel = idl;
                }
                using (var h = new EditorGUILayout.HorizontalScope ()) {
                    EditorGUILayout.LabelField ("Max", EditorStyles.boldLabel, GUILayout.Width (60));
                    EditorGUI.indentLevel = 0;
                    b.max.x = EditorGUILayout.IntField ("X", b.max.x, GUILayout.Width (40));
                    b.max.y = EditorGUILayout.IntField ("Y", b.max.y, GUILayout.Width (40));
                    b.max.z = EditorGUILayout.IntField ("Z", b.max.z, GUILayout.Width (40));
                    EditorGUI.indentLevel = idl;
                }
            }
            EditorGUILayout.Space ();
        }

		void DrawBlockItems()
		{
			using (var v = new EditorGUILayout.VerticalScope ("Box")) {
				float labelDefW = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 70f;
				GUI.color = ChunkEditor.volColor;
				using (var h1 = new EditorGUILayout.HorizontalScope (EditorStyles.helpBox)) {
					GUI.color = defColor;
					EditorGUILayout.LabelField ("Items", EditorStyles.boldLabel, GUILayout.Width (45));
				}

				EditorGUI.indentLevel++;
				for (int i = 0; i < vd.blockItems.Count; i++) {
					DrawBlockItem (i);
				}
				EditorGUI.indentLevel--;
				EditorGUIUtility.labelWidth = labelDefW;
			}
		}

		void DrawBlockItem (int i)
		{
			BlockItem workItem = vd.blockItems [i];
			using (var h = new EditorGUILayout.HorizontalScope ()) {
				blockItems [i] = EditorGUILayout.Foldout (
					blockItems [i],
					"[" + workItem.BlockPos.x +
					"," + workItem.BlockPos.y +
					"," + workItem.BlockPos.z +
					"] "
				);
				workItem.pieceName = EditorGUILayout.TextField (workItem.pieceName,GUILayout.Width(Screen.width - 85));
			}
			if (blockItems [i]) {
				Vector3 pos = new Vector3 (workItem.posX, workItem.posY, workItem.posZ);
				pos = EditorGUILayout.Vector3Field ("Position", pos);
				workItem.posX = pos.x;
				workItem.posY = pos.y;
				workItem.posZ = pos.z;

				Quaternion rot = new Quaternion (workItem.rotX, workItem.rotY, workItem.rotZ, workItem.rotW);
				rot.eulerAngles = EditorGUILayout.Vector3Field ("Rotation", rot.eulerAngles);
				workItem.rotX = rot.x;
				workItem.rotY = rot.y;
				workItem.rotZ = rot.z;
				workItem.rotW = rot.w;

				for (int j = 0; j < workItem.attributes.Length; j++)
					workItem.attributes [j] = EditorGUILayout.TextField ("Attrib" + j.ToString (), workItem.attributes [j]);
			}
			EditorGUILayout.Space ();
		}
	}
}
