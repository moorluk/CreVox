using UnityEngine;
using System.Collections;
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
        public GameObject Root
        {
            get
            {
                root = root ?? transform.Find("Root").gameObject;
                root = root ?? gameObject;
                return root;
            }
            set
            {
                root = value;
            }
        }

        void Awake()
        {
            if (Application.isPlaying)
                SetupPiece(null);
        }

        private void OnValidate()
        {
            if (tree[0].self.instance != null && !Application.isPlaying)
                UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(tree[0].self.instance); };
        }

        public override void SetupPiece (BlockItem item)
        {
            if (tree.Count > 0) {
                ClearRoot ();
                foreach (TreeElement te in tree) {
                    te.self.instance = null;
                }
            }
            tree [0].Generate (Root, this);
        }

        public void ClearRoot ()
        {
            for (int i = Root.transform.childCount; i > 0; i--) {
                if (Application.isPlaying)
                    Destroy(Root.transform.GetChild(i - 1).gameObject);
                else
                    DestroyImmediate(Root.transform.GetChild(i - 1).gameObject);
            }
        }
    }
}