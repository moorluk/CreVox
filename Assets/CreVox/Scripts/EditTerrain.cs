using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CreVox
{
	
	public static class EditTerrain
	{
		public static Block GetBlock(RaycastHit hit, Vector3 offset = default(Vector3), bool adjacent = false)
		{
			Chunk chunk = hit.collider.GetComponent<Chunk>();
			if (chunk == null)
				return null;

			WorldPos pos = GetBlockPos(hit, offset, adjacent);

			Block block = chunk.volume.GetBlock(pos.x, pos.y, pos.z);

			return block;
		}

		public static bool SetBlock(RaycastHit hit, Block block, Vector3 offset = default(Vector3), bool adjacent = false)
		{
			Chunk chunk = hit.collider.GetComponent<Chunk>();
			if (chunk == null)
				return false;

			WorldPos pos = GetBlockPos(hit, offset, adjacent);

			chunk.volume.SetBlock(pos.x, pos.y, pos.z, block);

			return true;
		}

		public static WorldPos GetBlockPos(Vector3 pos, Vector3 offset = default(Vector3))
		{
			pos -= offset;
			WorldPos blockPos = new WorldPos(
				                    Mathf.RoundToInt(pos.x / Block.w),
				                    Mathf.RoundToInt(pos.y / Block.h),
				                    Mathf.RoundToInt(pos.z / Block.d)
			                    );

			return blockPos;
		}

		public static WorldPos GetBlockPos(Vector3 pos, Transform localRoot)
		{
			pos = localRoot.InverseTransformPoint(pos);
			WorldPos blockPos = new WorldPos(
				Mathf.RoundToInt(pos.x / Block.w),
				Mathf.RoundToInt(pos.y / Block.h),
				Mathf.RoundToInt(pos.z / Block.d)
			);

			return blockPos;
		}

		public static WorldPos GetBlockPos (RaycastHit hit, Vector3 offset = default(Vector3), bool adjacent = false)
		{
			Vector3 pos = hit.point + hit.normal * (adjacent ? 0.5f : -0.5f);
			return GetBlockPos (pos, offset);
		}

		public static WorldPos GetGridPos(Vector3 pos, Vector3 offset = default(Vector3))
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