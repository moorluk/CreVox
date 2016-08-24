using UnityEngine;
using System;
//using System.Collections;
//using System.Collections.Generic;

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
        if(parts != null)
        {
            foreach (GameObject o in parts)
                GameObject.DestroyImmediate(o);
        }

        if(node != null)
        {
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

        if (parts == null)
        {
            node = new GameObject();
            node.name = bPos.ToString();
            node.transform.parent = go.transform.parent;
            parts = new GameObject[9];
			pieceNames = new string[9];
        }
        if(go != null)
            go.transform.parent = node.transform;

        if (parts[z * 3 + x] != null)
        {
            GameObject.DestroyImmediate(parts[z * 3 + x]);
            Debug.Log("Delete parts:" + x.ToString() + "," + z.ToString());
        }
        parts[z * 3 + x] = go;
        if (go == null)
            pieceNames[z * 3 + x] = "";
        else
            pieceNames [z * 3 + x] = go.GetComponent<PaletteItem> ().name;
    }
}
