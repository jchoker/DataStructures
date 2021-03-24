using System;
using System.Collections;
using System.Collections.Generic;

namespace Choker.DataStructures
{
    /// <summary>
    /// An implementation of an AVL tree.
    /// An AVL tree is a special type of binary tree which self balances itself to keep operations logarithmic.
    /// It allows for fast data retrieval/insertion in O(logn) time.
    /// Supported operations include insert, remove, containment, height.
    /// By design all left nodes are less than to parent node which itself is less than all its right nodes, the tree doesn't allow duplicates.
    /// </summary>
    /// <date>19.03.2020</date>
    /// <author>Jalal Choker, jalal.choker@gmail.com</author>
    public class AVLTree<T> : IEnumerable<T> where T : IComparable<T>
    {
        public AVLTree() { } // parameterless constructor

        Node root;

        public int Count { get; private set; } = 0;
        public bool IsEmpty => this.Count == 0;
        public int Height => (root?.Height).GetValueOrDefault();

        public bool Contains(T value) => Contains(root, value); // log N

        static bool Contains(Node node, T value) // non-recursive, memory efficient
        {
            if (value == null || node == null) return false;

            var trv = node;
            while (trv != null)
            {
                var cmp = value.CompareTo(trv.Value);

                if (cmp == 0) return true;

                trv = cmp < 0 ? trv.Left : trv.Right;
            }

            return false;
        }

        public bool Insert(T value) // logN
        {
            if (value == null) throw new ArgumentNullException(nameof(value)); // null insertion not allowed

            if (!Contains(root, value))
            {
                root = Insert(root, value);
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

            Update(node);

            return Balance(node);
        }

        public bool Insert_Optimized(T value) // logN, doesn't check for contains in advance so a logN method call time is saved
        {
            if (value == null) throw new ArgumentNullException(nameof(value)); // null insertion not allowed

            var newRoot = Insert_Optimized(root, value);
            if (newRoot != null)
            {
                this.root = newRoot;
                this.Count++;
            }
            return newRoot != null;
        }

        static Node Insert_Optimized(Node node, T value)
        {
            if (node == null) return new Node(value);

            var cmp = value.CompareTo(node.Value);
            if (cmp < 0)
            {
                var newLeftNode = Insert(node.Left, value);
                if (newLeftNode == null) return null;
                node.Left = newLeftNode;
            }
            else if (cmp > 0)
            {
                var newRightNode = Insert(node.Right, value);
                if (newRightNode == null) return null;
                node.Right = newRightNode;
            }
            else return null;

            Update(node);

            return Balance(node);
        }

        static void Update(Node node)
        {
            var rh = (node.Right != null) ? node.Right.Height : -1;
            var lh = (node.Left != null) ? node.Left.Height : -1;

            node.BalanceFactor = rh - lh;
            node.Height = 1 + Math.Max(rh, lh);
        }

        static Node Balance(Node node)
        {
            if (node.BalanceFactor == -2) // left-heavy subtree
            {
                return (node.Left.BalanceFactor <= 0) ? LeftLeftCase(node) : LeftRightCase(node);
            }
            else if (node.BalanceFactor == +2) // right-heavy subtree
            {
                return (node.Left.BalanceFactor >= 0) ? RightRightCase(node) : RightLeftCase(node);
            }
            return node; // subtree already balanced
        }

        static Node LeftLeftCase(Node node) => RotateRight(node);

        static Node LeftRightCase(Node node) // rotate left then rotate right
        {
            node.Left = RotateLeft(node.Left);
            return LeftLeftCase(node);
        }
        static Node RightRightCase(Node node) => RotateLeft(node);

        static Node RotateRight(Node node)
        {
            var newParent = node.Left;
            node.Left = newParent.Right;
            newParent.Right = node;

            Update(node);
            Update(newParent);

            return newParent;
        }

        static Node RightLeftCase(Node node) // rotate right then rotate left
        {
            node.Right = RotateRight(node.Right);
            return RightRightCase(node);
        }

        static Node RotateLeft(Node node)
        {
            var newParent = node.Right;
            node.Right = newParent.Left;
            newParent.Left = node;

            Update(node);
            Update(newParent);

            return newParent;
        }

        public bool Remove(T value)
        {
            if (value == null) return false; // null doesn't exist

            if (Contains(root, value))
            {
                root = Remove(root, value);
                Count--;
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
            else // found
            {
                if (node.Left == null) // has no or 1 child
                {
                    return node.Right;
                }
                else if (node.Right == null)
                {
                    return node.Left;
                }
                else // has 2 children
                {
                    if (node.Left.Height > node.Right.Height)
                    {
                        var successorValue = FindMax(node.Left);
                        node.Value = successorValue;
                        node.Left = Remove(node.Left, successorValue);
                    }
                    else
                    {
                        var successorValue = FindMin(node.Right);
                        node.Value = successorValue;
                        node.Right = Remove(node.Right, successorValue);
                    }
                }
            }

            Update(node);
            return Balance(node);
        }

        static T FindMax(Node node) // inorder predecessor
        {
            var max = node;
            while (max.Right != null) max = max.Right;
            return max.Value;
        }

        static T FindMin(Node node) // inorder successor
        {
            var min = node;
            while (min.Left != null) min = min.Left;
            return min.Value;
        }

        // inorder traversal
        public IEnumerator<T> GetEnumerator() => TraverseInOrder(root).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        static IEnumerable<T> TraverseInOrder(Node node)
        {
            if (node != null)
            {
                if (node.Left != null)
                {
                    foreach (var item in TraverseInOrder(node.Left))
                    {
                        yield return item;
                    }
                }
                yield return node.Value;
                if (node.Right != null)
                {
                    foreach (var item in TraverseInOrder(node.Right))
                    {
                        yield return item;
                    }
                }
            }
        }

        public static bool IsBSTInvariant(AVLTree<T> tree) // for testing purposes
        {
            return IsBSTInvariant(tree?.root);

            bool IsBSTInvariant(Node node)
            {
                if (node == null) return true;

                var isValid = (node.Left == null || node.Left.Value.CompareTo(node.Value) < 0) && (node.Right == null || node.Right.Value.CompareTo(node.Value) > 0);

                return isValid && IsBSTInvariant(node.Left) && IsBSTInvariant(node.Right);
            }
        }

        class Node
        {
            public Node(T value) : this(value, 0, 0, null, null) { } // leaf creation

            public Node(T value, int height, int balanceFactor, Node left, Node right)
            {
                this.Value = value;
                this.Height = height;
                this.BalanceFactor = balanceFactor;
                this.Left = left;
                this.Right = right;
            }

            public T Value { get; set; }
            public int Height { get; set; }
            public int BalanceFactor { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }
        }
    }
}