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
        bool _bShowStyleOptions = true;
        bool _bShowTargetOptions = true;
        bool _bShowDependencyOptions = true;

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
                _bShowStyleOptions = EditorGUILayout.BeginFoldoutHeaderGroup(_bShowStyleOptions, "Style Options");
                if (_bShowStyleOptions)
                {
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        _options.bNestedNamespace = EditorGUILayout.ToggleLeft("Assembly 名のドット区切りで Namespace を作成", _options.bNestedNamespace);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            // Target Options
            {
                _bShowTargetOptions = EditorGUILayout.BeginFoldoutHeaderGroup(_bShowTargetOptions, "Target Options");
                if (_bShowTargetOptions)
                {
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        var targetOptions = _options.TargetAssemblyOptions;
                        targetOptions.bIgnorePackageAssembly = EditorGUILayout.ToggleLeft("Package/ 以下の Assembly は除外", targetOptions.bIgnorePackageAssembly);
                        targetOptions.bIgnoreUnityAssembly = EditorGUILayout.ToggleLeft("Unity の Assembly は除外", targetOptions.bIgnoreUnityAssembly);
                        targetOptions.bIgnoreAssemblyCSharp = EditorGUILayout.ToggleLeft("Assembly-CSharp は除外", targetOptions.bIgnoreAssemblyCSharp);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                // 除外リスト
                {
                    ThisSerializedObject.Update();
                    var property = ThisSerializedObject.FindProperty(nameof(_excludeDirectoryPattern));
                    EditorGUILayout.PropertyField(property: property, label: new GUIContent("Exclude .asmdef directory regex patterns"), includeChildren: true);
                    ThisSerializedObject.ApplyModifiedProperties();
                }
            }

            // Dependency Options
            {
                _bShowDependencyOptions = EditorGUILayout.BeginFoldoutHeaderGroup(_bShowDependencyOptions, "Dependency Options");
                if (_bShowDependencyOptions)
                {
                    using (new EditorGUI.IndentLevelScope(1))
                    {
                        _options.bIgnoreUnityAssemblyDependency = EditorGUILayout.ToggleLeft("Unity の Assembly は依存元から除く", _options.bIgnoreUnityAssemblyDependency);
                        _options.bIgnoreUnityEngineUiDependency = EditorGUILayout.ToggleLeft("UnityEngine.UI への依存は注釈にする", _options.bIgnoreUnityEngineUiDependency);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            GUILayout.Space(_oneLineHeight);

            // Draw Generate Button
            if (GUILayout.Button("Generate PlantUML", GUILayout.Height(_oneLineHeight * 2))) // x2 larger
            {
                _options.TargetAssemblyOptions.ignoreDirectoryPatterns = this._excludeDirectoryPattern;
                _textResultPlantUml = Generator.Generate(_options);
            }

            // Scrollable Text Area
            using (var s = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = s.scrollPosition;
                // Readonly TextArea hack
                EditorGUILayout.SelectableLabel(_textResultPlantUml, EditorStyles.textArea, GUILayout.Height(position.height));
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