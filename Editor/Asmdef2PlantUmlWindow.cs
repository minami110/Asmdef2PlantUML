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

        [SerializeField]
        private StyleOptions _styleOptions = new();

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
                        ThisSerializedObject.Update();
                        var property = ThisSerializedObject.FindProperty(nameof(_styleOptions));
                        EditorGUILayout.PropertyField(property: property, label: new GUIContent("Style Options"), includeChildren: true);
                        ThisSerializedObject.ApplyModifiedProperties();
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
                        targetOptions.bIgnorePackageAssembly = EditorGUILayout.ToggleLeft("Exclude assemblies in Packages/*", targetOptions.bIgnorePackageAssembly);
                        targetOptions.bIgnoreUnityAssembly = EditorGUILayout.ToggleLeft("Exclude assemblies of Unity", targetOptions.bIgnoreUnityAssembly);
                        targetOptions.bIgnoreAssemblyCSharp = EditorGUILayout.ToggleLeft("Exclude Assembly-CSharp.dll", targetOptions.bIgnoreAssemblyCSharp);
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
                        _options.bIgnorePackageAssemblyDependency = EditorGUILayout.ToggleLeft("Exclude assemblies in Packages/*", _options.bIgnorePackageAssemblyDependency);
                        _options.bIgnoreUnityAssemblyDependency = EditorGUILayout.ToggleLeft("Exclude assemblies of Unity", _options.bIgnoreUnityAssemblyDependency);
                        _options.bHideUnityEngineDependency = EditorGUILayout.ToggleLeft("UnityEngine.UI への依存は注釈にする", _options.bHideUnityEngineDependency);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            GUILayout.Space(_oneLineHeight);

            // Draw Generate Button
            if (GUILayout.Button("Generate PlantUML", GUILayout.Height(_oneLineHeight * 2))) // x2 larger
            {
                _options.StyleOptions = this._styleOptions;
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