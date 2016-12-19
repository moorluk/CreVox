using UnityEngine;
using System;

namespace CreVox
{
	
	[Serializable]
	public class BlockItem : Block
	{
		public string[] pieceNames = new string[9];
		private bool[] isSolid = new bool[6];

		public BlockItem () : base ()
		{
			if (isSolid == null || isSolid.Length != 6) {
				isSolid = new bool[6];
			}
			for (int i = 0; i < isSolid.Length; i++)
				isSolid [i] = false;
		}

		public override void Destroy ()
		{
			base.Destroy ();
		}

		public override MeshData MeahAddMe (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			return meshData;
		}

		public override MeshData ColliderAddMe (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			return meshData;
		}

		public override bool IsSolid (Direction direction)
		{
			if (isSolid == null || isSolid.Length != 6) {
				isSolid = new bool[6];
			}
			return isSolid [(int)direction];
		}

		public void SetPiece (WorldPos bPos, WorldPos gPos, LevelPiece piece)
		{
			if (pieceNames == null)
				pieceNames = new string[9];
			GameObject pObj = (piece != null) ? piece.gameObject : null;
			int id = gPos.z * 3 + gPos.x;

			if (pObj != null) {
				pieceNames [id] = pObj.GetComponent<PaletteItem> ().name;
			} else {
				pieceNames [id] = null;
			}
		}

		public void SolidCheck (GameObject[] pieces)
		{
			if (isSolid == null || isSolid.Length != 6)
				isSolid = new bool[6];
			for (int i = 0; i < isSolid.Length; i++) {
				isSolid [i] = false;
			}

			for (int p = 0; p < pieces.Length; p++) {
				if (pieces [p] != null) {
					for (int i = 0; i < isSolid.Length; i++) {
						if (pieces [p].GetComponent<LevelPiece> ().IsSolid ((Direction)i)) {
							isSolid [i] = true;
						}
					}
				}
			}
		}
	}
}