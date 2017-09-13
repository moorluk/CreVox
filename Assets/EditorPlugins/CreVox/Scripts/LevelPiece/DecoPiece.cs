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

        void Start ()
        {
            SetupPiece (null);
        }

        public override void SetupPiece(BlockItem item)
        {
            if (tree.Count > 0) {
                GameObject root = tree [0].self.instance;
                if (root != null)
                    GameObject.DestroyImmediate (root);
                tree [0].Generate (gameObject, this);
            }
        }

    }
}