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

		void Awake()
		{
		}

		public override void SetupPiece(BlockItem item)
		{
			if (item.attributes [0] != null && item.attributes [0].Length > 0) {
				if(artPrefab == null)
				artPrefab = (GameObject)Resources.Load (item.attributes [0]);
			}
			if (artPrefab && !artInstance) {
				artInstance = Instantiate (artPrefab);
				artInstance.transform.parent = this.transform;
			}
        }
	}
}