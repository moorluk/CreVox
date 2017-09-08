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

        public Node parent = new Node();
        public Node self = new Node ();
        public List<Node> childs = new List<Node>();

        public void Generate (GameObject parent, DecoPiece rootObject)
        {
            switch (self.type) {
            case (int)DecoType.Node:
                self.Generate (parent);
                break;
            case (int)DecoType.Tree:
                GameObject r = self.Generate (parent);
                foreach (Node d in childs) {
//                    rootObject.tree[TreeElement.FindListByNode(rootObject.tree,d)].Generate (r, rootObject);
                    rootObject.tree[d.treeIndex].Generate (r, rootObject);
                }
                break;
            case (int)DecoType.RandomOne:
                foreach (Node d in childs) {
                    float p = UnityEngine.Random.Range (0f, 1f);
                    if (d.probability >= p) {
//                        rootObject.tree[TreeElement.FindListByNode(rootObject.tree,d)].Generate (parent, rootObject);
                        rootObject.tree[d.treeIndex].Generate (parent, rootObject);
                        break;
                    }
                }
                break;
            case (int)DecoType.RandomAll:
                int c = childs.Count;
                for (int i = 0; i < c; i++) {
                    Node d = childs [i] as Node;
                    float p = UnityEngine.Random.Range (0f, 1f);
                    if ((d.probability * (c - i) / c) >= p) {
//                        rootObject.tree[TreeElement.FindListByNode(rootObject.tree,d)].Generate (parent, rootObject);
                        rootObject.tree[d.treeIndex].Generate (parent, rootObject);
                    }
                }
                break;
            }
        }
    }

}
