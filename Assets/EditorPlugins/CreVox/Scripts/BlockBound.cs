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
        public Vector3 GetMin(Transform a_t)
        {
            float x = (min.x - 0.5f) * VGlobal.GetSetting ().w;
            float y = (min.y - 0.5f) * VGlobal.GetSetting ().h;
            float z = (min.z - 0.5f) * VGlobal.GetSetting ().d;
            return a_t.TransformPoint (new Vector3 (x, y, z));
        }
        public Vector3 GetMax(Transform a_t)
        {
            float x = (max.x + 0.5f) * VGlobal.GetSetting ().w;
            float y = (max.y + 0.5f) * VGlobal.GetSetting ().h;
            float z = (max.z + 0.5f) * VGlobal.GetSetting ().d;
            return a_t.TransformPoint (new Vector3 (x, y, z));
        }
    }
}