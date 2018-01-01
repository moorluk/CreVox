using UnityEngine;

namespace CreVox
{
    public static class EditTerrain
    {
        public static WorldPos GetBlockPos (Vector3 pos)
        {
            VGlobal vg = VGlobal.GetSetting ();
            WorldPos blockPos = new WorldPos (
                           Mathf.RoundToInt (pos.x / vg.w),
                           Mathf.RoundToInt (pos.y / vg.h),
                           Mathf.RoundToInt (pos.z / vg.d)
                       );

            return blockPos;
        }

        public static WorldPos GetBlockPos (Vector3 pos, Transform localRoot)
        {
            VGlobal vg = VGlobal.GetSetting ();
            pos = localRoot.InverseTransformPoint (pos);
            WorldPos blockPos = new WorldPos (
                           Mathf.RoundToInt (pos.x / vg.w),
                           Mathf.RoundToInt (pos.y / vg.h),
                           Mathf.RoundToInt (pos.z / vg.d)
                       );

            return blockPos;
        }

        public static WorldPos GetBlockPos (RaycastHit hit, bool adjacent = false)
        {
            Vector3 pos = hit.point + hit.normal * (adjacent ? 0.5f : -0.5f);
            return GetBlockPos (pos);
        }

        public static WorldPos GetGridPos (Vector3 pos)
        {
            VGlobal vg = VGlobal.GetSetting ();
            WorldPos gridPos = new WorldPos (
                          Mathf.RoundToInt ((int)(pos.x + vg.w / 2) % (int)vg.w),
                          Mathf.RoundToInt ((int)(pos.y + vg.h / 2) % (int)vg.h),
                          Mathf.RoundToInt ((int)(pos.z + vg.d / 2) % (int)vg.d)
                      );
            return gridPos;
        }
    }
}