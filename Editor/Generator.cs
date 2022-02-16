#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace asmdef2pu
{
    internal class ExportOptions
    {
        public bool bNestedNamespace = true;
        public bool bIgnoreUnityAssembly = true;
        public bool bIgnoreAssemblyCSharp = true;
        public bool bIgnoreUnityEngineUiDependency = true;
    }

    public class Asmdef2PlantUmlWindow : EditorWindow
    {
        // Options
        ExportOptions _options = new();

        string _textResultPlantUml = "Nothing Opened...";
        Vector2 _scroll;

        // Window メニューに "My Window" というメニュー項目を追加
        [MenuItem("Tools/asmdef2pu")]
        private static void ShowWindow()
        {
            //既存のウィンドウのインスタンスを表示。ない場合は作成します。
            var window = EditorWindow.GetWindow<Asmdef2PlantUmlWindow>();
            window.Show();
        }


        void OnGUI()
        {
            // Draw Options
            {
                GUILayout.Label("Style Options", EditorStyles.boldLabel);
                _options.bNestedNamespace = EditorGUILayout.ToggleLeft("Assembly 名のドット区切りで Namespace を作成", _options.bNestedNamespace);
                GUILayout.Label("Assembly Options", EditorStyles.boldLabel);
                _options.bIgnoreUnityAssembly = EditorGUILayout.ToggleLeft("Unity の Assembly は除く", _options.bIgnoreUnityAssembly);
                _options.bIgnoreAssemblyCSharp = EditorGUILayout.ToggleLeft("Assembly-CSharp は除く", _options.bIgnoreAssemblyCSharp);
                GUILayout.Label("Dependency Options", EditorStyles.boldLabel);
                _options.bIgnoreUnityEngineUiDependency = EditorGUILayout.ToggleLeft("UnityEngine.UI への依存は注釈にする", _options.bIgnoreUnityEngineUiDependency);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Generate PlantUML Text"))
            {
                _textResultPlantUml = Generator.Generate(_options);
            }

            // Scrollable Text Area
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                _textResultPlantUml = EditorGUILayout.TextArea(_textResultPlantUml, GUILayout.Height(position.height - 200));
                EditorGUILayout.EndScrollView();
            }

            // Copy Text Button
            if (GUILayout.Button("Copy to clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = _textResultPlantUml;
            }
        }
    }

    static class Generator
    {
        internal static string Generate(ExportOptions options)
        {
            // PlantUml Assembly Cache
            var puAssemblies = new List<PUAssembly>();

            // local functions
            PUAssembly GetPuAssembly(Assembly assembly)
            {
                // Make PlantUml Assembly
                var puAssembly = new PUAssembly(assembly);

                // Already existed in cache use it
                {
                    var index = puAssemblies.IndexOf(puAssembly);
                    if (index > -1)
                    {
                        puAssembly = puAssemblies[index];
                    }
                    else
                    {
                        puAssemblies.Add(puAssembly);
                    }
                }

                return puAssembly;
            }

            // Player Build included only (Excluded test assembly)
            var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
            foreach (var assembly in assemblies)
            {
                // Make PlantUml Assembly
                var puAssembly = GetPuAssembly(assembly);

                // Add References
                var assemblyRefs = assembly.assemblyReferences;
                foreach (var assmblyRef in assemblyRefs)
                {
                    // Make PlantUml Assembly
                    var puAssemblyRef = GetPuAssembly(assmblyRef);

                    // Add Reference
                    puAssembly.AddDependency(puAssemblyRef);
                }
            }

            // Log
            /*
            foreach (var pua in puAssemblies)
            {
                string msg = "";

                msg += $"{pua.Name}:\n";
                msg += $"\toutputPath: {pua.OutputPath}\n";
                msg += $"\tasmdefPath: {pua.AsmdefPath}\n";
                msg += $"\tdependencies:\n";
                foreach (var d in pua.Dependencies)
                {
                    msg += $"\t\t{d.Name}\n";
                }

                Debug.Log(msg);
            }
            */

            // To PlantUml
            string output = "";
            output += "@startuml\n\n";

            // PlantUml Options
            {
                output += "' ----- Begin PlantUML Options -----\n\n";

                output += "\n' ----- End PlantUML Options -----\n\n";
            }

            // Package Defines
            if (options.bNestedNamespace)
            {
                output += "' ----- Begin Assembly Namespaces Definition -----\n\n";

                var nsd = new NamespaceDrawer(options);
                foreach (var pua in puAssemblies)
                {
                    nsd.Add(pua);
                }
                output += nsd.Draw();
                output += "\n' ----- End Assembly Namespaces Definition -----\n\n";
            }

            // Assembly Defines
            {
                output += "' ----- Begin Assembly -----\n\n";
                foreach (var pua in puAssemblies)
                {
                    output += pua.Asm(options);
                }
                output += "\n' ----- End Assembly -----\n\n";
            }

            // Dependency Defines
            {
                output += "' ----- Begin Dependencies -----\n\n";
                foreach (var pua in puAssemblies)
                {
                    output += pua.Dep(options);
                }
                output += "\n' ----- End Dependencies -----\n\n";
            }

            output += "\n@enduml";

            return output;
        }

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
                    if (assembly.Name.Contains("Unity"))
                    {
                        return;
                    }
                }

                if (_options.bIgnoreAssemblyCSharp)
                {
                    if (assembly.Name == "Assembly-CSharp")
                    {
                        return;
                    }
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
}