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
        public GameObject root;

        void Awake ()
        {
            if (root == null)
                root = gameObject;
            SetupPiece (null);
        }

        public override void SetupPiece (BlockItem item)
        {
            if (tree.Count > 0 && root != null) {
                ClearRoot ();
                foreach (TreeElement te in tree) {
                    te.self.instance = null;
                }
            }
            tree [0].Generate (root ?? gameObject, this);
        }

        public void ClearRoot ()
        {
            for (int i = root.transform.childCount; i > 0; i--) {
                if (Application.isPlaying)
                    Destroy(root.transform.GetChild(i - 1).gameObject);
                else
                    DestroyImmediate(root.transform.GetChild(i - 1).gameObject);
            }
        }
    }
}