using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CreVox
{
	public class PrefabPiece : LevelPiece 
	{
		public GameObject artPrefab;
		public GameObject artInstance;
		public bool isRoot;
		public string artPack;
		public int APId = 0;

		public override void SetupPiece(BlockItem item)
		{
			if (item.attributes [0] != null && item.attributes [0].Length > 0) {
				if(artPrefab == null)
					artPrefab = (GameObject)Resources.Load (PathCollect.artDeco + "/" + artPack + "/" + item.attributes [0]);
			}
			isRoot = (item.attributes [1] == "True");
			if (artPrefab && !artInstance) {
				#if UNITY_EDITOR
				artInstance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab (artPrefab);
				#else
				artInstance = Instantiate (artPrefab);
				#endif

				name = artPrefab.name;
				artInstance.transform.parent = transform;
                UpdatePos();
				artInstance.transform.localRotation = Quaternion.Euler (Vector3.zero);
			}
        }

        public void UpdatePos()
        {
            if (artInstance) {
                if (isRoot) {
					Vector3 _pos = this.transform.localPosition;
					artInstance.transform.localPosition = new Vector3 (-_pos.x, -_pos.y, -_pos.z);
				} else {
					artInstance.transform.localPosition = Vector3.zero;
				}
			}
		}
	}
}