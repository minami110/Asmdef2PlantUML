#nullable enable

using System;
using System.Collections.Generic;

namespace asmdef2pu.Interfaces
{
    internal interface INode : IEquatable<INode>
    {
        public string Name { get; }
        public string FullName { get; }
        public INode? Parent { get; }
        public IEnumerable<INode> Children { get; }
        public bool IsRoot => Parent is null;
        public bool HasChildren { get; }
        public bool IsLeaf { get; }

        #region IEquatable impls
        bool IEquatable<INode>.Equals(INode? other)
        {
            if (other is null)
                return false;
            if (System.Object.ReferenceEquals(this, other))
                return true;
            if (this.GetType() != other.GetType())
                return false;
            return (this as INode).FullName == other.FullName;
        }
        #endregion


    }
}