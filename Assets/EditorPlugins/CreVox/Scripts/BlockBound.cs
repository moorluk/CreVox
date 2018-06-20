using UnityEngine;

namespace CreVox
{
    [System.Serializable]
    public class BlockBound
    {
        public WorldPos min, max;
        public Vector3 Size {
            get {
                float x = (Mathf.Abs (max.x - min.x) + 1) * VGlobal.GetSetting ().w;
                float y = (Mathf.Abs (max.y - min.y) + 1) * VGlobal.GetSetting ().h;
                float z = (Mathf.Abs (max.z - min.z) + 1) * VGlobal.GetSetting ().d;
                return new Vector3 (x, y, z);
            }
        }
        public Vector3 Center {
            get {
                float x = (max.x + min.x) * VGlobal.GetSetting ().w / 2;
                float y = (max.y + min.y) * VGlobal.GetSetting ().h / 2;
                float z = (max.z + min.z) * VGlobal.GetSetting ().d / 2;
                return new Vector3 (x, y, z);
            }
        }
        public Vector3 GetMin(Transform a_t = null) { return GetMin (min, a_t); }
        public static Vector3 GetMin(WorldPos a_min, Transform a_t = null)
        {
            float x = (a_min.x - 0.5f) * VGlobal.GetSetting ().w;
            float y = (a_min.y - 0.5f) * VGlobal.GetSetting ().h;
            float z = (a_min.z - 0.5f) * VGlobal.GetSetting ().d;
            Vector3 result = new Vector3 (x, y, z);
            return a_t == null ? result : a_t.TransformPoint (result);
        }
        public Vector3 GetMax(Transform a_t = null) { return GetMax (max, a_t); }
        public static Vector3 GetMax(WorldPos a_max, Transform a_t = null)
        {
            float x = (a_max.x + 0.5f) * VGlobal.GetSetting ().w;
            float y = (a_max.y + 0.5f) * VGlobal.GetSetting ().h;
            float z = (a_max.z + 0.5f) * VGlobal.GetSetting ().d;
            Vector3 result = new Vector3 (x, y, z);
            return a_t == null ? result : a_t.TransformPoint (result);
        }
    }
}