using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[SelectionBase]
public class World : MonoBehaviour {
    public Dictionary<WorldPos, Chunk> chunks = new Dictionary<WorldPos, Chunk>();
    public int chunkX = 1;
    public int chunkY = 1;
    public int chunkZ = 1;
    public GameObject chunkPrefab;
    public GameObject ruler;
    MeshCollider bColl;

    public void Init()
    {
        CreateRuler();
        CreateChunks();
    }

    void CreateRuler()
    {
        ruler = new GameObject("Ruler");
        ruler.layer = LayerMask.NameToLayer("Editor");
        ruler.transform.parent = transform;
        MeshCollider f = ruler.AddComponent<MeshCollider>();
        f.hideFlags = HideFlags.HideInHierarchy;

        MeshData meshData = new MeshData();
        float x = -Block.hw;
        float y = -Block.hh;
        float z = -Block.hd;
        float w = chunkX * Chunk.chunkSize * Block.w+x;
        float d = chunkZ * Chunk.chunkSize * Block.d+z;
        meshData.useRenderDataForCol = true;
        meshData.AddVertex(new Vector3(x, 0, z));
        meshData.AddVertex(new Vector3(x, 0, d));
        meshData.AddVertex(new Vector3(w, 0, d));
        meshData.AddVertex(new Vector3(w, 0, z));
        meshData.AddQuadTriangles();

        f.sharedMesh = null;
        Mesh cmesh = new Mesh();
        cmesh.vertices = meshData.colVertices.ToArray();
        cmesh.triangles = meshData.colTriangles.ToArray();
        cmesh.RecalculateNormals();

        f.sharedMesh = cmesh;
    }

    void OnDrawGizmos()
    {
        float x = -Block.hw;
        float z = -Block.hd;
        float w = chunkX * Chunk.chunkSize * Block.w + x;
        float d = chunkZ * Chunk.chunkSize * Block.d + z;
        Vector3 v1 = new Vector3(x, 0, z);
        Vector3 v2 = new Vector3(x, 0, d);
        Vector3 v3 = new Vector3(w, 0, d);
        Vector3 v4 = new Vector3(w, 0, z);
        Gizmos.DrawLine(v1, v2);
        Gizmos.DrawLine(v2, v3);
        Gizmos.DrawLine(v3, v4);
        Gizmos.DrawLine(v4, v1);
    }

    void CreateChunks()
    {
        for (int x = 0; x < chunkX; x++)
        {
            for (int y = 0; y < chunkY; y++)
            {
                for (int z = 0; z < chunkZ; z++)
                {
                    CreateChunk(x * 16, y * 16, z * 16);
                    GetChunk(x * 16, y * 16, z * 16).Init();
                }
            }
        }
    }

    public void ChangeEditY(int _y)
    {
        //bColl.center = Vector3.up * _y * Block.h;
    }

    // Use this for initialization

	
	// Update is called once per frame
	void Update () {
        
    }

    public void CreateChunk(int x, int y, int z)
    {
        WorldPos worldPos = new WorldPos(x, y, z);

        //Instantiate the chunk at the coordinates using the chunk prefab
        GameObject newChunkObject = Instantiate(
                        chunkPrefab, new Vector3(x * Block.w, y * Block.h, z * Block.d),
                        Quaternion.Euler(Vector3.zero)
                    ) as GameObject;
        newChunkObject.transform.parent = transform;

        Chunk newChunk = newChunkObject.GetComponent<Chunk>();

        newChunk.pos = worldPos;
        newChunk.world = this;

        //Add it to the chunks dictionary with the position as the key
        chunks.Add(worldPos, newChunk);

        //Add the following:
        for (int xi = 0; xi < 16; xi++)
        {
            for (int yi = 0; yi < 16; yi++)
            {
                for (int zi = 0; zi < 16; zi++)
                {
                    SetBlock(x + xi, y + yi, z + zi, new BlockAir());
                }
            }
        }
    }
    public void DestroyChunk(int x, int y, int z)
    {
        Chunk chunk = null;
        if (chunks.TryGetValue(new WorldPos(x, y, z), out chunk))
        {
            Object.Destroy(chunk.gameObject);
            chunks.Remove(new WorldPos(x, y, z));
        }
    }

    public Chunk GetChunk(int x, int y, int z)
    {
        WorldPos pos = new WorldPos();
        float multiple = Chunk.chunkSize;
        pos.x = Mathf.FloorToInt(x / multiple) * Chunk.chunkSize;
        pos.y = Mathf.FloorToInt(y / multiple) * Chunk.chunkSize;
        pos.z = Mathf.FloorToInt(z / multiple) * Chunk.chunkSize;
        Chunk containerChunk = null;
        chunks.TryGetValue(pos, out containerChunk);

        return containerChunk;
    }
    public Block GetBlock(int x, int y, int z)
    {
        Chunk containerChunk = GetChunk(x, y, z);
        if (containerChunk != null)
        {
            Block block = containerChunk.GetBlock(
                x - containerChunk.pos.x,
                y - containerChunk.pos.y,
                z - containerChunk.pos.z);

            return block;
        }
        else
        {
            return new BlockAir();
        }

    }

    public void SetBlock(int x, int y, int z, Block block)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {
            chunk.SetBlock(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, block);
            chunk.update = true;
        }
    }

}
