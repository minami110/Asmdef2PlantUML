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
        SerializedObject? _thisSerializedObject = null;
        SerializedObject ThisSerializedObject
        {
            get
            {
                if (_thisSerializedObject is null)
                    _thisSerializedObject = new(this);
                return _thisSerializedObject;
            }
        }
        ExportOptions _options = new();
        string _textResultPlantUml = "";
        Vector2 _scroll;

        [SerializeField]
        private List<string> _excludeDirectoryPattern = new() { "Assets/Plugins" };

        [MenuItem("Tools/Asmdef2PlantUML")]
        private static void ShowWindow()
        {
            //既存のウィンドウのインスタンスを表示。ない場合は作成します。
            var window = EditorWindow.GetWindow<Asmdef2PlantUmlWindow>();
            window.Show();
        }


        void OnGUI()
        {
            // Style Options
            {
                GUILayout.Label("Style Options", EditorStyles.boldLabel);
                _options.bNestedNamespace = EditorGUILayout.ToggleLeft("Assembly 名のドット区切りで Namespace を作成", _options.bNestedNamespace);
            }

            GUILayout.Space(10);

            // Target Options
            {
                GUILayout.Label("Target Options", EditorStyles.boldLabel);
                _options.bIgnorePackageAssembly = EditorGUILayout.ToggleLeft("Package/ 以下の Assembly は除外", _options.bIgnorePackageAssembly);
                _options.bIgnoreUnityAssembly = EditorGUILayout.ToggleLeft("Unity の Assembly は除外", _options.bIgnoreUnityAssembly);
                _options.bIgnoreAssemblyCSharp = EditorGUILayout.ToggleLeft("Assembly-CSharp は除外", _options.bIgnoreAssemblyCSharp);

                // 除外リスト
                {
                    ThisSerializedObject.Update();
                    var property = ThisSerializedObject.FindProperty(nameof(_excludeDirectoryPattern));
                    EditorGUILayout.PropertyField(property: property, label: new GUIContent("無視するディレクトリのリスト"), includeChildren: true);
                    ThisSerializedObject.ApplyModifiedProperties();
                }
            }

            GUILayout.Space(10);

            // Dependency Options
            {

                GUILayout.Label("Dependency Options", EditorStyles.boldLabel);
                _options.bIgnoreUnityAssemblyDependency = EditorGUILayout.ToggleLeft("Unity の Assembly は依存元から除く", _options.bIgnoreUnityAssemblyDependency);
                _options.bIgnoreUnityEngineUiDependency = EditorGUILayout.ToggleLeft("UnityEngine.UI への依存は注釈にする", _options.bIgnoreUnityEngineUiDependency);

            }

            GUILayout.Space(10);

            // Draw Generate Button
            if (GUILayout.Button("Generate PlantUML Text"))
            {
                _options.ignoreDirectoryPatterns = this._excludeDirectoryPattern;
                _textResultPlantUml = Generator.Generate(_options);
            }

            // Scrollable Text Area
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                // Readonly TextArea hack
                EditorGUILayout.SelectableLabel(_textResultPlantUml, EditorStyles.textArea, GUILayout.Height(position.height - 280));
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