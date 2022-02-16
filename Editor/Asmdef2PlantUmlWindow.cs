#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace asmdef2pu
{
    public class Asmdef2PlantUmlWindow : EditorWindow
    {
        // Options
        ExportOptions _options = new();
        string _textResultPlantUml = "Nothing Opened...";
        Vector2 _scroll;

        [MenuItem("Tools/Asmdef2PlantUML")]
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
                GUILayout.Label("Target Options", EditorStyles.boldLabel);
                _options.bIgnoreUnityAssembly = EditorGUILayout.ToggleLeft("Unity の Assembly は除く", _options.bIgnoreUnityAssembly);
                _options.bIgnoreAssemblyCSharp = EditorGUILayout.ToggleLeft("Assembly-CSharp は除く", _options.bIgnoreAssemblyCSharp);
                GUILayout.Label("Dependency Options", EditorStyles.boldLabel);
                _options.bIgnoreUnityAssemblyDependency = EditorGUILayout.ToggleLeft("Unity の Assembly は依存元から除く", _options.bIgnoreUnityAssemblyDependency);
                _options.bIgnoreUnityEngineUiDependency = EditorGUILayout.ToggleLeft("UnityEngine.UI への依存は注釈にする", _options.bIgnoreUnityEngineUiDependency);
            }

            GUILayout.Space(10);

            // Draw Generate Button
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
}