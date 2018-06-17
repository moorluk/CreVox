using UnityEngine;
using System.Collections.Generic;
using System;

namespace CreVox
{
    [ExecuteInEditMode]
    [Serializable]
    public class DecoPiece : LevelPiece
    {
        public List<TreeElement> tree = new List<TreeElement> ();
        [SerializeField]
        GameObject root;

        public GameObject Root {
            get {
                root = root ?? transform.Find ("Root").gameObject;
                root = root ?? gameObject;
                return root;
            }
            set {
                root = value;
            }
        }

        void Awake ()
        {
            enabled = true;
            if (tree.Count > 0) {
                ClearRoot ();
                foreach (TreeElement te in tree) {
                    te.self.instance = null;
                }
            }
        }

        void Start ()
        {
            tree [0].Generate (Root, this);
        }

        public override void SetupPiece(BlockItem item)
        {
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (tree[0].self.instance == null || Application.isPlaying || gameObject.scene.IsValid()) return;
            UnityEditor.EditorApplication.delayCall += delegate
            {
                if (!root) return;
                for (int i = root.transform.childCount; i > 0; i--)
                    DestroyImmediate(root.transform.GetChild(i - 1).gameObject, true);
            };
        }
#endif

        void ClearRoot ()
        {
            for (int i = Root.transform.childCount; i > 0; i--) {
                if (Application.isPlaying)
                    Destroy (Root.transform.GetChild (i - 1).gameObject);
                else
                    DestroyImmediate (Root.transform.GetChild (i - 1).gameObject);
            }
        }
    }
}