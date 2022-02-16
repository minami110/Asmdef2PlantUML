#nullable enable

using System;
using System.Collections.Generic;

using UnityEditor.Compilation;

namespace asmdef2pu
{
    class NamespaceDrawer
    {
        class Namespace : IEquatable<Namespace>
        {
            readonly Namespace? _parent;
            readonly List<Namespace> _children = new();
            readonly string _name;

            internal bool IsRoot => _parent is null;
            internal IEnumerable<Namespace> Children => _children;
            internal string Name => _name;

            void SetChild(Namespace ns)
            {
                if (!_children.Contains(ns))
                    _children.Add(ns);
            }

            public string FullName()
            {
                CollectFullNameRecuesive(out string result);
                return result;
            }

            void CollectFullNameRecuesive(out string result)
            {
                if (_parent is not null)
                {
                    _parent.CollectFullNameRecuesive(out result);
                    result += $".{_name}";
                }
                else
                {
                    result = $"{_name}";
                }
            }

            internal Namespace(Namespace? parent, string name)
            {
                _parent = parent;
                _name = name;

                if (parent is not null)
                {
                    parent.SetChild(this);
                }
            }

            #region IEquatable impls
            public override bool Equals(object? obj) => this.Equals(obj as Namespace);
            public bool Equals(Namespace? p)
            {
                if (p is null)
                    return false;
                if (System.Object.ReferenceEquals(this, p))
                    return true;
                if (this.GetType() != p.GetType())
                    return false;
                return FullName() == p.FullName();
            }
            public override int GetHashCode() => FullName().GetHashCode();
            public static bool operator ==(Namespace? lhs, Namespace? rhs)
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

            public static bool operator !=(Namespace? lhs, Namespace? rhs) => !(lhs == rhs);

            #endregion
        }

        readonly ExportOptions _options;
        readonly List<Namespace> _namespaceList = new();

        internal NamespaceDrawer(ExportOptions options)
        {
            _options = options;
        }

        internal void Add(IPuAssembly assembly)
        {
            if (_options.bIgnoreUnityAssembly)
            {
                if (assembly.IsUnityAssembly)
                    return;
            }

            if (_options.bIgnoreAssemblyCSharp)
            {
                if (assembly.IsAssemblyCSharp)
                    return;
            }

            // Assembly 
            {
                var sepName = assembly.Name.Split('.');
                var sepCount = sepName.Length;
                // Has namespace
                if (sepCount > 1)
                {
                    Namespace? parent = null;
                    for (int nest = 0; nest < sepCount; nest++)
                    {
                        // LeafNode is skip
                        if (nest == sepCount - 1)
                        {
                            continue;
                        }

                        var ns = new Namespace(parent, sepName[nest]);
                        var index = _namespaceList.IndexOf(ns);
                        if (index > -1)
                        {
                            ns = _namespaceList[index];
                        }
                        else
                        {
                            _namespaceList.Add(ns);
                        }
                        parent = ns;
                    }
                }
            }

            // References
            {
                foreach (var refas in assembly.Dependencies)
                {
                    var sepName = refas.Name.Split('.');
                    var sepCount = sepName.Length;
                    // Has namespace
                    if (sepCount > 1)
                    {

                        Namespace? parent = null;
                        for (int nest = 0; nest < sepCount; nest++)
                        {
                            // LeafNode is skip
                            if (nest == sepCount - 1)
                            {
                                continue;
                            }

                            var ns = new Namespace(parent, sepName[nest]);
                            var index = _namespaceList.IndexOf(ns);
                            if (index > -1)
                            {
                                ns = _namespaceList[index];
                            }
                            else
                            {
                                _namespaceList.Add(ns);
                            }
                            parent = ns;
                        }
                    }
                }
            }
        }

        internal string Draw()
        {
            string result = "";

            foreach (var ns in _namespaceList)
            {
                if (!ns.IsRoot)
                {
                    continue;
                }

                result += $"namespace {ns.Name}" + " {\n";
                foreach (var nsc in ns.Children)
                {
                    DrawChildNameSpace(ref result, nsc, 1);
                }
                result += "}\n";
            }

            return result;
        }

        void DrawChildNameSpace(ref string result, Namespace child, int nest)
        {
            for (var i = 0; i < nest; i++)
                result += "\t";

            result += $"namespace {child.Name}" + " {\n";

            foreach (var nsc in child.Children)
            {
                DrawChildNameSpace(ref result, nsc, nest + 1);
            }

            for (var i = 0; i < nest; i++)
                result += "\t";
            result += "}\n";
        }
    }
}