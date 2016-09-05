using UnityEngine;
using System;

namespace CreVox
{

	[Serializable]
	public class BlockAir : Block
	{
		public string[] pieceNames;
		[NonSerialized]
		private GameObject[] parts;
		[NonSerialized]
		private GameObject node;
        [NonSerialized]
        private bool[] isSolid = new bool[6];

        public BlockAir()
			: base()
		{
            for (int i = 0; i < isSolid.Length; i++)
                isSolid[i] = false;
		}

		public override void Destroy()
		{
			if (parts != null) {
				foreach (GameObject o in parts)
					GameObject.DestroyImmediate(o);
			}

			if (node != null) {
				GameObject.DestroyImmediate(node);
			}
			base.Destroy();
		}

		public override MeshData Blockdata
        (Chunk chunk, int x, int y, int z, MeshData meshData)
		{
			return meshData;
		}

		public override bool IsSolid(Block.Direction direction)
		{
            return isSolid[(int)direction];
		}

		public void SetPart(WorldPos bPos, WorldPos gPos, LevelPiece piece)
		{
			GameObject go = (piece != null) ? piece.gameObject : null;
			int x = gPos.x;
			int z = gPos.z;

			int id = z * 3 + x;
			if (go != null) {
				if (parts == null) {
					if (node == null) {
						node = new GameObject ();
						node.name = bPos.ToString ();
						node.transform.parent = go.transform.parent;
					}
					parts = new GameObject[9];
					pieceNames = new string[9];
				}

				if (parts[id] != null) {
					GameObject.DestroyImmediate(parts[id]);
				}
					
				go.transform.parent = node.transform;
				parts[id] = go;
				pieceNames[id] = go.GetComponent<PaletteItem>().name;
                for(int i = 0; i < isSolid.Length; i++)
                    isSolid[i] = piece.IsSolid((Block.Direction)i);

            } else {
				if (parts != null) {
					if (parts[id] != null) {
                        for (int i = 0; i < isSolid.Length; i++)
                            if (IsSolid((Block.Direction)i))
                                isSolid[i] = false;
                        GameObject.DestroyImmediate(parts[id]);
						pieceNames[id] = null;
						parts[id] = null;
					}
				}
			}
		}

        public int GetPartAngle(int _x, int _y)
        {
            int id = _x + _y * 3;
            GameObject part = parts[id];
            return (part !=null) ? (int)(part.transform.eulerAngles.y + 360)%360 : -1;
        }

		public void ShowPiece(bool isHide) {
			if (node)
				node.SetActive (isHide);
		}
	}
}