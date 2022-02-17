#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using asmdef2pu.Interfaces;
using asmdef2pu.Internal;

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
        static readonly string _urlPlantuml = "https://plantuml.com/plantuml/uml/";
        static readonly float _oneLineHeight = 20f;

        [SerializeField]
        private List<string> _excludeDirectoryPattern = new() { "Assets/Plugins" };

        [MenuItem("Tools/Asmdef2PlantUML")]
        private static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<Asmdef2PlantUmlWindow>(title: "Asmdef2PlantUML");
            window.Show();
        }


        void OnGUI()
        {
            // Style Options
            {
                GUILayout.Label("Style Options", EditorStyles.boldLabel);
                _options.bNestedNamespace = EditorGUILayout.ToggleLeft("Assembly 名のドット区切りで Namespace を作成", _options.bNestedNamespace);
            }

            GUILayout.Space(_oneLineHeight);

            // Target Options
            {
                var targetOptions = _options.TargetAssemblyOptions;
                GUILayout.Label("Target Options", EditorStyles.boldLabel);
                targetOptions.bIgnorePackageAssembly = EditorGUILayout.ToggleLeft("Package/ 以下の Assembly は除外", targetOptions.bIgnorePackageAssembly);
                targetOptions.bIgnoreUnityAssembly = EditorGUILayout.ToggleLeft("Unity の Assembly は除外", targetOptions.bIgnoreUnityAssembly);
                targetOptions.bIgnoreAssemblyCSharp = EditorGUILayout.ToggleLeft("Assembly-CSharp は除外", targetOptions.bIgnoreAssemblyCSharp);

                // 除外リスト
                {
                    ThisSerializedObject.Update();
                    var property = ThisSerializedObject.FindProperty(nameof(_excludeDirectoryPattern));
                    EditorGUILayout.PropertyField(property: property, label: new GUIContent("無視するディレクトリのリスト"), includeChildren: true);
                    ThisSerializedObject.ApplyModifiedProperties();
                }
            }

            GUILayout.Space(_oneLineHeight);

            // Dependency Options
            {

                GUILayout.Label("Dependency Options", EditorStyles.boldLabel);
                _options.bIgnoreUnityAssemblyDependency = EditorGUILayout.ToggleLeft("Unity の Assembly は依存元から除く", _options.bIgnoreUnityAssemblyDependency);
                _options.bIgnoreUnityEngineUiDependency = EditorGUILayout.ToggleLeft("UnityEngine.UI への依存は注釈にする", _options.bIgnoreUnityEngineUiDependency);

            }

            GUILayout.Space(_oneLineHeight);

            // Draw Generate Button
            if (GUILayout.Button("Generate PlantUML Text"))
            {
                _options.TargetAssemblyOptions.ignoreDirectoryPatterns = this._excludeDirectoryPattern;
                _textResultPlantUml = Generator.Generate(_options);
            }

            // Scrollable Text Area
            using (var s = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = s.scrollPosition;
                // Readonly TextArea hack
                EditorGUILayout.SelectableLabel(_textResultPlantUml, EditorStyles.textArea, GUILayout.Height(position.height - 350));
            }

            // Copy Text Button
            if (GUILayout.Button("Copy to clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = _textResultPlantUml;
            }

            // Web Jump
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.SelectableLabel(_urlPlantuml, EditorStyles.textField, GUILayout.Height(_oneLineHeight));
                if (GUILayout.Button("Open URL"))
                {
                    Application.OpenURL(_urlPlantuml);
                }
            }
        }
    }
}