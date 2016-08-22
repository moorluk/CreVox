using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

public class BlockAir : Block
{
    private GameObject[] parts;
    private GameObject node;
    WorldPos pos;
   
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
        pos = new WorldPos(x, y, z);
        return meshData;
    }
    public override bool IsSolid(Block.Direction direction)
    {
        return false;
    }

    public void SetPart(int x, int z, GameObject go)
    {
        if (parts == null)
        {
            node = new GameObject();
            node.name = pos.ToString();
            node.transform.parent = go.transform.parent;
            parts = new GameObject[9];
        }
        if(go != null)
            go.transform.parent = node.transform;

        if (parts[z * 3 + x] != null)
        {
            GameObject.DestroyImmediate(parts[z * 3 + x]);
            Debug.Log("Delete parts:" + x.ToString() + "," + z.ToString());
        }
        parts[z * 3 + x] = go;
    }
}
