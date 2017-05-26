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
		public string artPack;
		public int APId = 0;

		void Awake()
		{
		}

		public override void SetupPiece(BlockItem item)
		{
			if (item.attributes [0] != null && item.attributes [0].Length > 0) {
				if(artPrefab == null)
					artPrefab = (GameObject)Resources.Load (PathCollect.artDeco + "/" + artPack + "/ArtPrefab/" + item.attributes [0]);
			}
			if (artPrefab && !artInstance) {
				artInstance = Instantiate (artPrefab);
				artInstance.transform.parent = this.transform;
				artInstance.transform.localPosition = Vector3.zero;
				artInstance.transform.localRotation = Quaternion.Euler (Vector3.zero);
			}
        }
	}
}