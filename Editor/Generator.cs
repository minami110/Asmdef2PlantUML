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
    }
}