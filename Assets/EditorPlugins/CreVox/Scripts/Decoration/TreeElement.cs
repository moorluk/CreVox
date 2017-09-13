using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreVox
{
    public enum DecoType
    {
        Node,
        Tree,
        RandomOne,
        RandomAll
    }

    public enum turnSide
    {
        one,
        two,
        four
    }

    [Serializable]
    public class TreeElement
    {
        #if UNITY_EDITOR
        public bool showNode = true;
        #endif

        public NIndex parent = new NIndex();
        public Node self = new Node ();
        public List<NIndex> childs = new List<NIndex>();

        public void Generate (GameObject parent, DecoPiece rootObject)
        {
            switch (self.type) {
            case (int)DecoType.Node:
                self.Generate (parent);
                break;
            case (int)DecoType.Tree:
                GameObject r = self.Generate (parent);
                foreach (NIndex d in childs) {
                    // save but slow
//                    rootObject.tree[TreeElement.FindListByNode(rootObject.tree,d)].Generate (r, rootObject);
                    rootObject.tree[d.treeIndex].Generate (r, rootObject);
                }
                break;
            case (int)DecoType.RandomOne:
                foreach (NIndex d in childs) {
                    float p = UnityEngine.Random.Range (0f, 1f);
                    if (d.probability >= p) {
                        // save but slow
//                        rootObject.tree[TreeElement.FindListByNode(rootObject.tree,d)].Generate (parent, rootObject);
                        rootObject.tree[d.treeIndex].Generate (parent, rootObject);
                        break;
                    }
                }
                break;
            case (int)DecoType.RandomAll:
                int c = childs.Count;
                for (int i = 0; i < c; i++) {
                    NIndex d = childs [i];
                    float p = UnityEngine.Random.Range (0f, 1f);
                    if ((d.probability * (c - i) / c) >= p) {
                        // save but slow
//                        rootObject.tree[TreeElement.FindListByNode(rootObject.tree,d)].Generate (parent, rootObject);
                        rootObject.tree[d.treeIndex].Generate (parent, rootObject);
                    }
                }
                break;
            }
        }
    }

}
