#nullable enable

using System;
using System.Collections.Generic;
using asmdef2pu.Interfaces;

namespace asmdef2pu.Internal
{
    class NamespaceDrawer
    {
        class Namespace : IEquatable<Namespace>
        {
            readonly Namespace? _parent;
            readonly List<Namespace> _children = new();
            readonly string _name;

            internal bool IsRoot => _parent is null;
            internal bool IsLeaf => _children.Count == 0;
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

        readonly List<Namespace> _namespaceList = new();

        internal NamespaceDrawer() { }

        internal void Add(IPuAssembly assembly)
        {
            // Assembly 
            {
                var sepName = assembly.Name.Split('.');
                var sepCount = sepName.Length;
                if (sepCount > 0)
                {
                    Namespace? parent = null;
                    for (int nest = 0; nest < sepCount; nest++)
                    {
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
                    if (sepCount > 0)
                    {
                        Namespace? parent = null;
                        for (int nest = 0; nest < sepCount; nest++)
                        {
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
                // Skip not root node
                if (!ns.IsRoot)
                {
                    continue;
                }

                // Root and Leaf node, draw component
                if (ns.IsLeaf)
                {
                    result += $"component {ns.FullName()} [\n";
                    result += $"\t{ns.Name}\n";
                    // Draw comment
                    /*
                    result += "==\n";
                    */
                    result += "]";
                }
                // Root but not Leaf node
                else
                {
                    result += $"package {ns.Name}" + " {\n";
                    foreach (var nsc in ns.Children)
                    {
                        DrawChildNameSpace(ref result, nsc, 1);
                    }
                    result += "}\n";
                }
            }

            return result;
        }

        void DrawChildNameSpace(ref string result, Namespace child, int nest)
        {
            var thisNest = "";
            for (var i = 0; i < nest; i++)
                thisNest += "\t";

            if (child.IsLeaf)
            {
                result += $"{thisNest}component {child.FullName()} [\n";
                result += $"{thisNest}\t{child.Name}\n";
                // Draw comment
                /*
                result += "==\n";
                */
                result += $"{thisNest}]\n";
            }
            else
            {
                result += $"{thisNest}package {child.Name}" + " {\n";

                foreach (var nsc in child.Children)
                {
                    DrawChildNameSpace(ref result, nsc, nest + 1);
                }

                result += $"{thisNest}" + "}\n";
            }
        }
    }
}