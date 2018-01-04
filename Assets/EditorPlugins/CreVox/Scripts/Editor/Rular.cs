using UnityEngine;

namespace CreVox
{

    public static class Rular
    {
        [SerializeField]
        static MeshCollider mColl;
        [SerializeField]
        static BoxCollider bColl;
        [SerializeField]
        static GameObject ruler;
        [SerializeField]
        static GameObject layerRuler;

        public static void Create ()
        {
            //clear
            Destroy ();
            //rebuild
            CreateRuler ();
            CreateLevelRuler ();
            ShowRuler ();
        }

        public static void Destroy ()
        {
            mColl = null;
            if (ruler)
                Object.DestroyImmediate (ruler);
            bColl = null;
            if (layerRuler)
                Object.DestroyImmediate (layerRuler);
        }

        static void CreateRuler ()
        {
            Volume vol = Volume.focusVolume;
            VGlobal Vg = vol.Vg;
            VolumeData vd = vol.vd;

            ruler = new GameObject ("Ruler");
            ruler.layer = LayerMask.NameToLayer ("Editor");
            ruler.tag = PathCollect.rularTag;
            ruler.transform.parent = vol.transform;
            mColl = ruler.AddComponent<MeshCollider> ();

            MeshData meshData = new MeshData ();
            float x = -Vg.w / 2;
            float y = -Vg.h / 2;
            float z = -Vg.d / 2;
            float w = (vd == null) ? Vg.w : (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.x : vd.chunkX * vd.chunkSize) * Vg.w + x;
            float d = (vd == null) ? Vg.d : (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.z : vd.chunkZ * vd.chunkSize) * Vg.d + z;
            meshData.useRenderDataForCol = true;
            meshData.AddVertex (new Vector3 (x, y, z));
            meshData.AddVertex (new Vector3 (x, y, d));
            meshData.AddVertex (new Vector3 (w, y, d));
            meshData.AddVertex (new Vector3 (w, y, z));
            meshData.AddQuadTriangles ();

            mColl.sharedMesh = null;
            Mesh cmesh = new Mesh ();
            cmesh.vertices = meshData.colVertices.ToArray ();
            cmesh.triangles = meshData.colTriangles.ToArray ();
            cmesh.RecalculateNormals ();

            mColl.sharedMesh = cmesh;

            ruler.transform.localPosition = Vector3.zero;
            ruler.transform.localRotation = Quaternion.Euler (Vector3.zero);
        }

        static void CreateLevelRuler ()
        {
            Volume vol = Volume.focusVolume;
            VGlobal Vg = vol.Vg;
            VolumeData vd = vol.vd;

            float w = (vd == null) ? Vg.w : (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.x : vd.chunkX * vd.chunkSize) * Vg.w;
            float d = (vd == null) ? Vg.d : (vd.useFreeChunk ? vd.freeChunk.freeChunkSize.z : vd.chunkZ * vd.chunkSize) * Vg.d;
            layerRuler = new GameObject ("LevelRuler");
            layerRuler.layer = LayerMask.NameToLayer ("EditorLevel");
            layerRuler.transform.parent = vol.transform;
            layerRuler.transform.localPosition = new Vector3 (w / 2 - Vg.w / 2, 0f, d / 2 - Vg.d / 2);
            layerRuler.transform.localRotation = Quaternion.Euler (Vector3.zero);
            bColl = layerRuler.AddComponent<BoxCollider> ();
            bColl.size = new Vector3 (w, 0f, d);
            SetY (vol.pointY);
        }

        static void ActiveRuler (bool _active)
        {
            bool r = (Volume.focusVolume.Vm.DebugRuler);
            if (mColl) {
                mColl.enabled = _active;
                ruler.SetActive (_active);
                ruler.hideFlags = r ? HideFlags.None : HideFlags.HideInHierarchy;
            }
            if (bColl) {
                bColl.enabled = _active;
                layerRuler.SetActive (_active);
                layerRuler.hideFlags = r ? HideFlags.None : HideFlags.HideInHierarchy;
            }
            Volume.focusVolume.pointer = _active;
        }

        public static void ShowRuler ()
        {
            bool _active = !UnityEditor.EditorApplication.isPlaying && (Volume.focusVolume.Vm.DebugRuler);
            ActiveRuler (_active);
        }

        public static void SetY (int pointY)
        {
            Volume vol = Volume.focusVolume;
            VGlobal Vg = vol.Vg;
            VolumeData vd = vol.vd;

            int maxY = (vd == null) ? 0 : ((vd.useFreeChunk) ? vd.freeChunk.freeChunkSize.y : (vd.chunkY * vd.chunkSize)) - 1;
            pointY = Mathf.Clamp (pointY, 0, maxY);
            if (bColl)
                bColl.center = new Vector3 (bColl.center.x, (pointY + 0.5f) * Vg.h, bColl.center.z);
            vol.pointY = pointY;
            vol.YColor = new Color (
                (20 + (pointY % 10) * 20) / 255f,
                (200 - Mathf.Abs ((pointY % 10) - 5) * 20) / 255f,
                (200 - (pointY % 10) * 20) / 255f
            );
        }

    }
}
