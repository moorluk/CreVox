using UnityEngine;

namespace CreVox
{

    public class BoxCursor : MonoBehaviour
    {
        public GameObject Center;
        public GameObject Xplus;
        public GameObject Xminor;
        public GameObject Yplus;
        public GameObject Yminor;
        public GameObject Zplus;
        public GameObject Zminor;

        public static bool visible {
            set {
                Box.SetActive (value);
            }
        }

        static GameObject Box {
            get;
            set;
        }

        public static void Create (Transform _Parent, VGlobal vg)
        {
            Destroy ();
            Box = Instantiate (Resources.Load<GameObject> (PathCollect.box));
            Box.transform.SetParent (_Parent);
            Box.transform.localScale = new Vector3 (vg.w, vg.h, vg.d);
            Box.transform.localRotation = Quaternion.Inverse (_Parent.rotation);
//            Box.hideFlags = HideFlags.HideInHierarchy;
            Update (_Parent.position, Vector3.zero);
        }

        public static void Destroy ()
        {
            if (Box)
                GameObject.DestroyImmediate (Box, false);
        }

        public static void Update (Vector3 _pos, Vector3 _dir)
        {
            if (!Box)
                return;
            Box.transform.position = _pos;
            //切換箭頭顯示方向
            BoxCursor dir = Box.GetComponent<BoxCursor> ();
            dir.Center.SetActive (_dir == Vector3.zero);
            dir.Xplus.SetActive (_dir.x > 0.5f);
            dir.Xminor.SetActive (_dir.x < -0.5f);
            dir.Yplus.SetActive (_dir.y > 0.5f);
            dir.Yminor.SetActive (_dir.y < -0.5f);
            dir.Zplus.SetActive (_dir.z > 0.5f);
            dir.Zminor.SetActive (_dir.z < -0.5f);
        }
    }
}