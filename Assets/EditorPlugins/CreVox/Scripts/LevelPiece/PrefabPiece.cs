using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CreVox
{

	#if UNITY_EDITOR
	[ExecuteInEditMode]
	#endif
	public class PrefabPiece : LevelPiece 
	{
		public GameObject artPrefab;
		public GameObject artInstance;
		public bool isRoot;
		public string artPack;
		public int APId = 0;

		void Awake()
		{
		}
		#if UNITY_EDITOR
		void Update()
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
		#endif

		public override void SetupPiece(BlockItem item)
		{
			if (item.attributes [0] != null && item.attributes [0].Length > 0) {
				if(artPrefab == null)
					artPrefab = (GameObject)Resources.Load (PathCollect.artDeco + "/" + artPack + "/ArtPrefab/" + item.attributes [0]);
			}
			if (artPrefab && !artInstance) {
				#if UNITY_EDITOR
				artInstance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab (artPrefab);
				#else
				artInstance = Instantiate (artPrefab);
				#endif

				this.name = artPrefab.name;
				artInstance.transform.parent = this.transform;
				if (item.attributes [1] == "True") {
					Vector3 _pos = this.transform.localPosition;
					artInstance.transform.localPosition = new Vector3 (-_pos.x, -_pos.y, -_pos.z);
				} else {
					artInstance.transform.localPosition = Vector3.zero;
				}
				artInstance.transform.localRotation = Quaternion.Euler (Vector3.zero);
			}
			isRoot = (item.attributes [1] == "True");
        }
	}
}