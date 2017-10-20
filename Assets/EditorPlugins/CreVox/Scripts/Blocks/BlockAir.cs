using UnityEngine;
using System;

namespace CreVox
{
	
    [Serializable]
    public class BlockAir : Block
    {
        public string[] pieceNames = new string[9];
        public bool[] isSolid = new bool[6];

        public BlockAir ()
        {
        }
        // [XAOCX add]
        public BlockAir (BlockAir clone) : base (clone)
        {
            pieceNames = (string[])clone.pieceNames.Clone ();
            isSolid = (bool[])clone.isSolid.Clone ();
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
            if (isSolid.Length < 6)
                isSolid = new bool[6];
            return isSolid [(int)direction];
        }

        public void SetPiece (WorldPos gPos, LevelPiece piece)
        {
            GameObject pObj = (piece != null) ? piece.gameObject : null;
            int id = gPos.z * 3 + gPos.x;

            pieceNames [id] = pObj == null ? null : pObj.name;
        }

        public void SolidCheck (GameObject[] pieces)
        {
            if (isSolid == null || isSolid.Length != 6)
                isSolid = new bool[6];
            for (int i = 0; i < isSolid.Length; i++)
                isSolid [i] = false;

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