using System;
using System.Collections.Generic;
using System.Linq;

namespace CreVox
{
    public abstract class ATree<T>
    {
        public T Value { get; set; }

        public abstract ATree<T> Parent { get; }
        public abstract ATreeList<T> Children { get; }
        public abstract int Count { get; }
        public abstract int Degree { get; }
        public abstract int Depth { get; }
        public abstract int Level { get; }

        public ATree(T value)
        {
            this.Value = value;
        }

        public abstract void Add(T value);
        public abstract void Add(ATree<T> tree);
        public abstract void Remove();
        public abstract ATree<T> Clone();
    }

    public abstract class ATreeList<T> : IEnumerable<ATree<T>>
    {
        public abstract int Count { get; }
        public abstract IEnumerator<ATree<T>> GetEnumerator();

        IEnumerator<ATree<T>> IEnumerable<ATree<T>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
        
    public class DTree<T> : ATree<T>
    {
        protected LinkedList<DTree<T>> childrenList;

        protected DTree<T> parent;
        public override ATree<T> Parent
        {
            get
            {
                return parent;
            }
        }

        protected DTreeList<T> children;
        public override ATreeList<T> Children
        {
            get
            {
                return children;
            }
        }

        public override int Degree
        {
            get
            {
                return childrenList.Count;
            }
        }

        protected int count;
        public override int Count
        {
            get
            {
                return count;
            }
        }

        protected int depth;
        public override int Depth
        {
            get
            {
                return depth;
            }
        }

        protected int level;
        public override int Level
        {
            get
            {
                return level;
            }
        }

        public DTree(T value)
            : base(value)
        {
            childrenList = new LinkedList<DTree<T>>();
            children = new DTreeList<T>(childrenList);
            depth = 1;
            level = 1;
            count = 1;
        }

        public override void Add(T value)
        {
            Add(new DTree<T>(value));
        }

        public override void Add(ATree<T> tree)
        {
            DTree<T> gtree = (DTree<T>)tree;
            if (gtree.Parent != null)
                gtree.Remove();
            gtree.parent = this;
            if (gtree.depth + 1 > depth)
            {
                depth = gtree.depth + 1;
                BubbleDepth();
            }
            gtree.level = level + 1;
            gtree.UpdateLevel();
            childrenList.AddLast(gtree);
            count += tree.Count;
            BubbleCount(tree.Count);
        }

        public override void Remove()
        {
            if (parent == null)
                return;
            parent.childrenList.Remove(this);
            if (depth + 1 == parent.depth)
                parent.UpdateDepth();
            parent.count -= count;
            parent.BubbleCount(-count);
            parent = null;
        }

        public override ATree<T> Clone()
        {
            return Clone(1);
        }

        protected DTree<T> Clone(int level)
        {
            DTree<T> cloneTree = new DTree<T>(Value);
            cloneTree.depth = depth;
            cloneTree.level = level;
            cloneTree.count = count;
            foreach (DTree<T> child in childrenList)
            {
                DTree<T> cloneChild = child.Clone(level + 1);
                cloneChild.parent = cloneTree;
                cloneTree.childrenList.AddLast(cloneChild);
            }
            return cloneTree;
        }

        protected void BubbleDepth()
        {
            if (parent == null)
                return;

            if (depth + 1 > parent.depth)
            {
                parent.depth = depth + 1;
                parent.BubbleDepth();
            }
        }

        protected void UpdateDepth()
        {
            int tmpDepth = depth;
            depth = 1;
            foreach (DTree<T> child in childrenList)
                if (child.depth + 1 > depth)
                    depth = child.depth + 1;
            if (tmpDepth == depth || parent == null)
                return;
            if (tmpDepth + 1 == parent.depth)
                parent.UpdateDepth();
        }

        protected void BubbleCount(int diff)
        {
            if (parent == null)
                return;

            parent.count += diff;
            parent.BubbleCount(diff);
        }

        protected void UpdateLevel()
        {
            int childLevel = level + 1;
            foreach (DTree<T> child in childrenList)
            {
                child.level = childLevel;
                child.UpdateLevel();
            }
        }
    }

    public class DTreeList<T> : ATreeList<T>
    {
        protected LinkedList<DTree<T>> list;

        public DTreeList(LinkedList<DTree<T>> list)
        {
            this.list = list;
        }

        public override int Count
        {
            get
            {
                return list.Count;
            }
        }

        public override IEnumerator<ATree<T>> GetEnumerator()
        {
            return list.GetEnumerator() as IEnumerator<ATree<T>>;
        }
    }
}