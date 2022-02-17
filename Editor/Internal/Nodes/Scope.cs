#nullable enable

using System;
using System.Collections.Generic;
using asmdef2pu.Interfaces;

namespace asmdef2pu.Internal
{
    internal class Scope : INode
    {
        readonly string _name;
        readonly string _fullname;
        readonly Scope? _parent;
        readonly HashSet<INode> _children;

        #region Constructors

        internal Scope(string name, Scope? parent)
        {
            _name = name;
            _parent = parent;
            _children = new();

            // Collect FullName
            _CollectFullNameRecursively(this, out _fullname);

            // Add Parent to this
            if (parent is not null)
            {
                parent._AddChild(this);
            }
        }

        #endregion

        #region INode impls

        string INode.Name => _name;
        string INode.FullName => _fullname;
        INode? INode.Parent => _parent;
        IEnumerable<INode> INode.Children => _children;
        bool INode.HasChildren => _children.Count > 0;
        bool INode.IsLeaf => false;

        #endregion

        #region IEquatable
        public override bool Equals(object? obj) => this.Equals(obj as INode);
        public override int GetHashCode() => (this as INode).FullName.GetHashCode();
        public static bool operator ==(Scope? lhs, Scope? rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                    return true;
                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }
        public static bool operator !=(Scope? lhs, Scope? rhs) => !(lhs == rhs);
        #endregion

        #region Helper methods
        internal void _AddChild(INode ns)
        {
            _children.Add(ns);
        }

        internal static void _CollectFullNameRecursively(INode node, out string result, string sep = ".")
        {
            if (node.IsRoot)
            {
                result = $"{node.Name}";
            }
            else
            {
                _CollectFullNameRecursively(node.Parent!, out result);
                result += $"{sep}{node.Name}";
            }
        }
        #endregion


    }
}