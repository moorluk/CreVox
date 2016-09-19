using UnityEngine;
using System;

namespace CreVox
{

	[Serializable]
	public class BlockAir : Block
	{
		public string[] pieceNames;
		[NonSerialized]
		private GameObject[] pieces;
		[NonSerialized]
		private GameObject node;
		[NonSerialized]
		private bool[] isSolid = new bool[6];

		public BlockAir (): base ()
		{
			for (int i = 0; i < isSolid.Length; i++)
				isSolid [i] = false;
		}

		public override void Destroy ()
		{
			if (pieces != null) {
				foreach (GameObject o in pieces)
					GameObject.DestroyImmediate (o);
			}

			if (node != null) {
				GameObject.DestroyImmediate (node);
			}
			base.Destroy ();
		}

		public override MeshData Blockdata (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			return meshData;
		}

		public override bool IsSolid (Block.Direction direction)
		{
			return isSolid [(int)direction];
		}

		public void SetPiece (WorldPos bPos, WorldPos gPos, LevelPiece piece)
		{
			GameObject go = (piece != null) ? piece.gameObject : null;
			int x = gPos.x;
			int z = gPos.z;
			int id = z * 3 + x;

			if (go != null) {
				if (pieces == null) {
					if (node == null) {
						node = new GameObject ();
						node.name = bPos.ToString ();
						node.transform.parent = go.transform.parent;
					}
					pieces = new GameObject[9];
					pieceNames = new string[9];
				}

				if (pieces [id] != null) {
					GameObject.DestroyImmediate (pieces [id]);
				}
					
				go.transform.parent = node.transform;
				pieces [id] = go;
				pieceNames [id] = go.GetComponent<PaletteItem> ().name;
				SolidCheck ();
			} else {
				if (pieces != null) {
					if (pieces [id] != null) {
						GameObject.DestroyImmediate (pieces [id]);
						pieceNames [id] = null;
						pieces [id] = null;
						SolidCheck ();
					}
				}
			}
		}

		void SolidCheck()
		{
			for (int i = 0; i < isSolid.Length; i++) {
				isSolid [i] = false;
			}

			for (int p = 0; p < pieces.Length; p++) {
				if (pieces [p] != null) {
					for (int i = 0; i < isSolid.Length; i++) {
						if (pieces [p].GetComponent<LevelPiece> ().IsSolid ((Block.Direction)i)) {
							isSolid [i] = true;
						}
					}
				}
			}
		}

		public int GetPartAngle (int _x, int _y)
		{
			int id = _x + _y * 3;
			GameObject part = pieces [id];
			return (part != null) ? (int)(part.transform.eulerAngles.y + 360) % 360 : -1;
		}

		public void ShowPiece (bool isHide)
		{
			if (node)
				node.SetActive (isHide);
		}
	}
}