using UnityEngine;
using System.Collections;

namespace CreVox{

public static class EditTerrain
{
    public static Block GetBlock(RaycastHit hit, bool adjacent = false)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
            return null;

        WorldPos pos = GetBlockPos(hit, adjacent);

        Block block = chunk.world.GetBlock(pos.x, pos.y, pos.z);

        return block;
    }

    public static bool SetBlock(RaycastHit hit, Block block, bool adjacent = false)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
            return false;

        WorldPos pos = GetBlockPos(hit, adjacent);

        chunk.world.SetBlock(pos.x, pos.y, pos.z, block);

        return true;
    }

    public static WorldPos GetBlockPos(Vector3 pos)
    {
        WorldPos blockPos = new WorldPos(
            Mathf.RoundToInt(pos.x / Block.w),
            Mathf.RoundToInt(pos.y / Block.h),
            Mathf.RoundToInt(pos.z / Block.d)
            );

        return blockPos;
    }

    public static WorldPos GetBlockPos(RaycastHit hit, bool adjacent = false)
    {
        Vector3 pos = new Vector3(
            MoveWithinBlock(hit.point.x / Block.w, hit.normal.x, adjacent),
            MoveWithinBlock(hit.point.y / Block.h, hit.normal.y, adjacent),
            MoveWithinBlock(hit.point.z / Block.d, hit.normal.z, adjacent)
            );
        pos.x *= Block.w;
        pos.y *= Block.h;
        pos.z *= Block.d;
        return GetBlockPos(pos);
    }

    public static WorldPos GetGridPos(Vector3 pos)
    {
        WorldPos gridPos = new WorldPos(
            Mathf.RoundToInt((int)(pos.x+Block.hw)% (int)Block.w),
            Mathf.RoundToInt((int)(pos.y+Block.hh)% (int)Block.h),
            Mathf.RoundToInt((int)(pos.z+Block.hd)% (int)Block.d)
            );
        return gridPos;
    }

    static float MoveWithinBlock(float pos, float norm, bool adjacent = false)
    {
        if (pos - (int)pos == 0.5f || pos - (int)pos == -0.5f)
        {
            if (adjacent)
            {
                pos += (norm / 2);
            }
            else
            {
                pos -= (norm / 2);
            }
        }

        return (float)pos;
    }
}
}