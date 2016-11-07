using UnityEngine;
using System.Collections;

namespace CreVox
{
	
	public static class EditTerrain
	{
		public static Block GetBlock(RaycastHit hit, bool adjacent = false)
		{
			Chunk chunk = hit.collider.GetComponent<Chunk>();
			if (chunk == null)
				return null;

			WorldPos pos = GetBlockPos(hit, adjacent);

			Block block = chunk.volume.GetBlock(pos.x, pos.y, pos.z);

			return block;
		}

		public static bool SetBlock(RaycastHit hit, Block block, bool adjacent = false)
		{
			Chunk chunk = hit.collider.GetComponent<Chunk>();
			if (chunk == null)
				return false;

			WorldPos pos = GetBlockPos(hit, adjacent);

			chunk.volume.SetBlock(pos.x, pos.y, pos.z, block);

			return true;
		}

		public static WorldPos GetBlockPos(Vector3 pos)
		{
			WorldPos blockPos = new WorldPos (
				                    Mathf.RoundToInt (pos.x / Block.w),
				                    Mathf.RoundToInt (pos.y / Block.h),
				                    Mathf.RoundToInt (pos.z / Block.d)
			                    );

			return blockPos;
		}

		public static WorldPos GetBlockPos(Vector3 pos, Transform localRoot)
		{
			pos = localRoot.InverseTransformPoint (pos);
			WorldPos blockPos = new WorldPos (
				                    Mathf.RoundToInt (pos.x / Block.w),
				                    Mathf.RoundToInt (pos.y / Block.h),
				                    Mathf.RoundToInt (pos.z / Block.d)
			                    );

			return blockPos;
		}

		public static WorldPos GetBlockPos (RaycastHit hit, bool adjacent = false)
		{
			Vector3 pos = hit.point + hit.normal * (adjacent ? 0.5f : -0.5f);
			return GetBlockPos (pos);
		}

		public static WorldPos GetGridPos(Vector3 pos)
		{
			WorldPos gridPos = new WorldPos (
				                   Mathf.RoundToInt ((int)(pos.x + Block.hw) % (int)Block.w),
				                   Mathf.RoundToInt ((int)(pos.y + Block.hh) % (int)Block.h),
				                   Mathf.RoundToInt ((int)(pos.z + Block.hd) % (int)Block.d)
			                   );
			return gridPos;
		}
	}
}