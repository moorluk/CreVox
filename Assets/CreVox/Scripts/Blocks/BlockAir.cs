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

		public BlockAir()
			: base()
		{
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
			return false;
		}

		public void SetPart(WorldPos bPos, WorldPos gPos, GameObject go)
		{
			int x = gPos.x;
			int z = gPos.z;

			int id = z * 3 + x;

			if (parts == null) {
				node = new GameObject();
				node.name = bPos.ToString();
				node.transform.parent = go.transform.parent;
				parts = new GameObject[9];
				pieceNames = new string[9];
			}

			if (parts[id] != null) {
				GameObject.DestroyImmediate(parts[id]);
			}

			if (go == null) {
				pieceNames[id] = null;
				parts[id] = null;
			} else {
				go.transform.parent = node.transform;
				parts[id] = go;
				pieceNames[id] = go.GetComponent<PaletteItem>().name;
			}
		}
	}
}