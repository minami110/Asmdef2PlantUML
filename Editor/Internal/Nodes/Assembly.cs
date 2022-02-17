#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using asmdef2pu.Interfaces;

namespace asmdef2pu.Internal
{
    internal class Assembly : IAssembly, INode
    {
        private readonly HashSet<IAssembly> _dependencies = new();
        private readonly string _name;
        private readonly string _asmdefPath;
        private readonly Scope? _parent;
        private readonly bool _isDependentUnityEngine;

        #region Constructors

        public Assembly(string name, Scope? parent, string asmdefPath, bool isDependentUnityEngine)
        {
            _name = name;
            _parent = parent;
            _asmdefPath = asmdefPath;
            _isDependentUnityEngine = isDependentUnityEngine;

            // Add Parent to this
            if (parent is not null)
            {
                parent._AddChild(this);
            }
        }

        #endregion

        #region IPuAssembly impls

        string IAssembly.Name => _name;
        string IAssembly.AsmdefPath => _asmdefPath;
        IEnumerable<IAssembly> IAssembly.Dependencies => _dependencies;
        bool IAssembly.IsDependentUnityEngine => _isDependentUnityEngine;

        #endregion

        #region INode impls
        string INode.Name
        {
            get
            {
                // Foo.Bar.Baz => Baz
                var nameSplit = _name.Split(".");
                return nameSplit.Last();
            }
        }
        string INode.FullName => _name;
        INode? INode.Parent => _parent;
        IEnumerable<INode> INode.Children => new INode[0];
        bool INode.HasChildren => false;
        bool INode.IsLeaf => true;

        #endregion

        #region IEquatable
        public override bool Equals(object? obj) => this.Equals(obj as INode);
        public override int GetHashCode() => (this as INode).FullName.GetHashCode();
        public static bool operator ==(Assembly? lhs, Assembly? rhs)
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
        public static bool operator !=(Assembly? lhs, Assembly? rhs) => !(lhs == rhs);
        #endregion

        #region Helper methods

        public void AddDependency(IAssembly assembly)
        {
            _dependencies.Add(assembly);
        }

        // FIXME: MoveDrawer
        public string Dep(ExportOptions options)
        {
            string result = "";

            foreach (var d in _dependencies)
            {
                if (options.bIgnoreUnityAssemblyDependency)
                {
                    if (d.IsUnityTechnologiesAssembly)
                        continue;
                }

                if (options.bHideUnityEngineDependency)
                {
                    if (d.IsUnityEngineUi)
                        continue;
                }

                // arrow direction style top or bottom
                switch (options.StyleOptions.DirectionStyle)
                {
                    case DirectionStyle.BottomToTop:
                        result += $"\"{d.Name.Replace("-", "_")}\" <-- \"{_name.Replace("-", "_")}\"" + "\n";
                        break;
                    case DirectionStyle.TopToBottom:
                        result += $"\"{_name.Replace("-", "_")}\" --> \"{d.Name.Replace("-", "_")}\"" + "\n";
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        #endregion
    }
}