using UnityEngine;
using System;

namespace CreVox
{
	
	[Serializable]
	public class BlockAir : Block
	{
		public string[] pieceNames;
		private bool[] isSolid = new bool[6];

		public BlockAir () : base ()
		{
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

		public override bool IsSolid (Direction direction)
		{
			return isSolid [(int)direction];
		}

		public void SetPiece (WorldPos bPos, WorldPos gPos, LevelPiece piece)
		{
			if (pieceNames == null)
				pieceNames = new string[9];
			GameObject pObj = (piece != null) ? piece.gameObject : null;
			int x = gPos.x;
			int z = gPos.z;
			int id = z * 3 + x;
			if (pObj != null) {
				pieceNames [id] = pObj.GetComponent<PaletteItem> ().name;
			} else {
				pieceNames [id] = null;
			}
		}

		public void SolidCheck (GameObject[] pieces)
		{
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