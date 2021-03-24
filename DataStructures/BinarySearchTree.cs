using System;
using System.Collections.Generic;
using System.Linq;

namespace Choker.DataStructures
{
    /// <summary>
    /// An implementation of a Binary Search Tree (BST).
    /// Any comparable data is allowed within the tree.
    /// BST allows for fast data retrieval/insertion in O(log n) time given it is balanced.
    /// Supported operations include add, remove, containment, height.
    /// In addition, multiple tree traversal algorithms are provided including:
    /// 1) Preorder 2) Inorder 3) Postorder 4) Levelorder.
    /// By design all left nodes are less than to parent node which itself is less than all its right nodes, the tree doesn't allow duplicate values.
    /// </summary>
    /// <date>23.03.2020</date>
    /// <author>Jalal Choker, jalal.choker@gmail.com</author>
    public class BinarySearchTree<T> where T : IComparable<T>
    {
        public BinarySearchTree() { }

        public BinarySearchTree(T root)
        {
            this.root = new Node(root);
            this.Count++;
        }

        Node root;

        readonly List<T> values = new List<T>(); // values added in insertion order

        public IEnumerable<T> Values => this.values;
        public int Count { get; private set; } = 0;
        public bool IsEmpty => this.Count == 0;
        public int GetHeight() => GetHeight(root);

        public bool Insert(T value) // O(n) worst case, O(log n) average case
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (!Contains(value))
            {
                root = Insert(root, value);
                values.Add(value);
                Count++;

                return true;
            }
            return false;
        }

        static Node Insert(Node node, T value)
        {
            if (node == null) return new Node(value);

            var cmp = value.CompareTo(node.Value);

            if (cmp < 0)
                node.Left = Insert(node.Left, value);
            else
                node.Right = Insert(node.Right, value);

            return node;
        }

        public bool Contains(T value) => Contains(root, value);

        // O(N), notice that because of the recursive nature of the method an extra memory & performance overhead is incurred due to stack allocation.
        // Alternatively, we can implement it iteratively as done with the AVL tree contains method and get rid of the consecutive stack memory allocation.
        static bool Contains(Node node, T value)
        {
            if (value == null || node == null) return false;

            var cmp = value.CompareTo(node.Value);

            return (cmp == 0) ? true : ((cmp < 0) ? Contains(node.Left, value) : Contains(node.Right, value));
        }

        public bool Remove(T value)
        {
            if (value == null) return false;

            if (Contains(value))
            {
                root = Remove(root, value);
                values.Remove(value);
                this.Count--;

                return true;
            }
            return false;
        }

        static Node Remove(Node node, T value)
        {
            if (node == null) return null;

            var cmp = value.CompareTo(node.Value);
            if (cmp < 0)
            {
                node.Left = Remove(node.Left, value);
            }
            else if (cmp > 0)
            {
                node.Right = Remove(node.Right, value);
            }
            else
            {
                // the case with only a right subtree or no subtree at all, swap the node to remove with its right child
                if (node.Left == null) node = node.Right;
                // the case with only a left subtree or no subtree at all, swap the node to remove with its left child
                else if (node.Right == null) node = node.Left;
                else // the case with a left & right subtree then remove predecessor or successor 
                {
                    // micro-optimization to keep the tree as balanced as possible
                    if (GetHeight(node.Left) > GetHeight(node.Right)) // remove predecessor
                    {
                        var max = FindMax(node.Left); // get predecessor
                        node.Value = max.Value; // swap data
                        node.Left = Remove(node.Left, max.Value);
                    }
                    else // remove successor
                    {
                        var min = FindMin(node.Right);
                        node.Value = min.Value;
                        node.Right = Remove(node.Right, min.Value);
                    }
                }
            }
            return node;

            Node FindMax(Node n)
            {
                var max = n;
                while (max.Right != null) max = max.Right;
                return max;
            }

            Node FindMin(Node n)
            {
                var min = n;
                while (min.Left != null) min = min.Right;
                return min;
            }
        }

        // T: O(N) since the method will be called for every child, alternatively T(n) = 2T(n/2) + c;
        // S: O(N) concurrent method calls on the call stack
        static int GetHeight(Node node)
        {
            if (node == null) return 0;

            return 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));
        }

        public IEnumerable<T> Traverse(TreeTraversalOrder order)
        {
            if (this.root == null) return Enumerable.Empty<T>();

            switch (order)
            {
                case TreeTraversalOrder.PreOrder: return TraversePreOrder(root);
                case TreeTraversalOrder.InOrder: return TraverseInOrder(root);
                case TreeTraversalOrder.PostOrder: return TraversePostOrder(root);
                case TreeTraversalOrder.LevelOrder: return TraverseLevelOrder(root);
                default:
                    throw new NotImplementedException();
            }
        }

        static IEnumerable<T> TraversePreOrder(Node node)
        {
            yield return node.Value;

            if (node.Left != null)
                foreach (var item in TraversePreOrder(node.Left))
                    yield return item;

            if (node.Right != null)
                foreach (var item in TraversePreOrder(node.Right))
                    yield return item;
        }

        static IEnumerable<T> TraverseInOrder(Node node)
        {
            if (node.Left != null)            
                foreach (var item in TraverseInOrder(node.Left))
                    yield return item;
            
            yield return node.Value;

            if (node.Right != null)             
                foreach (var item in TraverseInOrder(node.Right))
                    yield return item;
        }

        static IEnumerable<T> TraversePostOrder(Node node)
        {
            if (node.Left != null)
                foreach (var item in TraversePostOrder(node.Left))
                    yield return item;

            if (node.Right != null)
                foreach (var item in TraversePostOrder(node.Right))
                    yield return item;

            yield return node.Value;
        }

        static IEnumerable<T> TraverseLevelOrder(Node node) // todo: analyze T & S
        {
            var h = GetHeight(node);

            for (int l = 0; l < h; l++)
            {
                foreach (var item in TraverseLevel(node, l))
                {
                    yield return item;
                }
            }
        }
        static IEnumerable<T> TraverseLevel(Node node, int level)
        {
            if (node == null)
            {
                yield break;
            }
            else if (level == 1)
            {
                yield return node.Value;
            }
            else
            {
                foreach (var item in TraverseLevel(node.Left, level - 1).Concat(TraverseLevel(node.Right, level - 1)))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Represents a node in a BST
        /// </summary>
        private class Node
        {
            public Node(T value) : this(value, null, null) { } // instantiates a leaf

            // Designated constructor
            public Node(T value, Node left, Node right)
            {
                this.Value = value;
                this.Left = left;
                this.Right = right;
            }

            public T Value { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }
        }
    }

    public enum TreeTraversalOrder
    {
        PreOrder,
        InOrder,
        PostOrder,
        LevelOrder
    }
}