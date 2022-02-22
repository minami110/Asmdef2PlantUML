#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using asmdef2pu.Interfaces;
using asmdef2pu.Internal;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace asmdef2pu
{
    using DotNetAssembly = System.Reflection.Assembly;

    public class TypeScopeWindow : EditorWindow
    {
        static readonly float _oneLineHeight = 20f;
        Vector2 _textResultScroll;
        string _textResultPlantUml = "";
        string _targetAssembryName = "";

        DotNetAssembly[] _assembries = null!;

        [MenuItem("Tools/Asmdef2PlantUML/Type Scope")]
        private static void ShowWindow()
        {
            var window = GetWindow<TypeScopeWindow>(title: "Type Scope");
            window.Show();
        }

        void OnGUI()
        {
            // Draw Target Assembry Name Field
            {
                _targetAssembryName = GUILayout.TextField(_targetAssembryName);
            }

            // Draw Generate Button
            if (GUILayout.Button("Generate PlantUML", GUILayout.Height(_oneLineHeight * 2))) // x2 larger
            {
                // first 
                _textResultPlantUml = "Generating ...";
                Repaint();

                var asmname = $"{_targetAssembryName}";
                DotNetAssembly ass;
                try
                {
                    var assemblyName = new AssemblyName(asmname);
                    ass = DotNetAssembly.Load(assemblyName);
                }
                catch (System.Exception exception)
                {
                    if (exception is System.ArgumentException or
                                     System.IO.FileNotFoundException
                    )
                    {
                        string msg = $"Failed to found {_targetAssembryName}\n";
                        msg += "Availables:\n";

                        var asms = AppDomain.CurrentDomain.GetAssemblies();

                        foreach (var asm in asms)
                        {
                            msg += $"{asm.GetName()}\n";
                        }
                        _textResultPlantUml = msg;
                        return;
                    }
                    else
                    {
                        throw exception;
                    }
                }

                string plantUmlStr = "@startuml\n";
                plantUmlStr += "scale 2048 width\n";
                plantUmlStr += "left to right direction\n";
                plantUmlStr += $"title {_targetAssembryName}\n";

                var dependenciesStrList = new List<string>();

                // Assebly 内に含まれる Public なクラスの一覧
                foreach (Type typeClass in ass.GetTypes().Where(x => x.IsPublic))
                {
                    var className = typeClass.FullName;

                    if (typeClass.IsInterface)
                    {
                        plantUmlStr += "\n" + $"interface {className}" + " {";

                    }
                    else if (typeClass.IsEnum)
                    {
                        plantUmlStr += "\n" + $"enum {className}" + " {";
                    }
                    // Struct
                    // https://nyama41.hatenablog.com/entry/is_struct
                    else if (typeClass.IsValueType && !typeClass.IsPrimitive && !typeClass.IsEnum)
                    {
                        plantUmlStr += "\n" + $"class {className} << (S,green) >>" + " {";
                    }
                    else if (typeClass.IsClass)
                    {
                        if (typeClass.IsAbstract)
                        {
                            plantUmlStr += "\n" + $"abstract {className}" + " {";
                        }
                        else
                        {
                            plantUmlStr += "\n" + $"class {className}" + " {";
                        }
                    }

                    // この型の中で 実装された メソッドの一覧を取得する
                    /*
                    MethodInfo[] methods = typeClass.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                    foreach (MethodInfo m in methods.Where(x => x.DeclaringType == typeClass))
                    {
                        string pfx = "\n\t";
                        string returnTypeStr = m.ReturnType.Name;
                        // Get Params
                        string parmTypeStr = "";
                        var parms = m.GetParameters();
                        foreach (var parm in parms)
                        {
                            parmTypeStr += parm.ParameterType.Name + ", ";
                        }
                        plantUmlStr += pfx + $"+ {returnTypeStr} {m.Name}({parmTypeStr})";
                    }
                    */

                    plantUmlStr += "\n" + "}";


                    // クラスが実装してる クラスの一覧
                    var baseType = typeClass.BaseType;

                    if (baseType is not null)
                    {
                        if (
                            baseType.FullName == "System.Object" ||
                            baseType.FullName == "System.ValueType" ||
                            baseType.FullName == "System.Enum"
                        )
                        { }
                        else
                        {
                            dependenciesStrList.Add($"\"{baseType.Name}\" <|-- \"{className}\"");
                        }
                    }

                    // クラスが実装しているインタフェースの一覧
                    foreach (Type typeInterface in typeClass.GetInterfaces())
                    {
                        // Enum のばあい, 絶対に依存するInterface があるので取り除く
                        if (typeClass.IsEnum)
                        {
                            // なにもしない
                        }
                        else
                        {
                            if (typeInterface.IsGenericType)
                            {
                                // なにもしない
                            }
                            else
                            {
                                var interfaceName = typeInterface.ToString();
                                dependenciesStrList.Add($"\"{interfaceName}\" <|.. \"{className}\"");
                            }
                        }
                    };
                }

                // Draw Dependencies
                plantUmlStr += "\npackage dependencies {";
                foreach (var dStr in dependenciesStrList)
                {
                    plantUmlStr += "\n" + dStr;
                }
                plantUmlStr += "\n}";

                plantUmlStr += "\n@enduml";

                _textResultPlantUml = plantUmlStr;
                Repaint();

            }

            // Scrollable Text Area
            using (var s = new EditorGUILayout.ScrollViewScope(_textResultScroll))
            {
                var areaStyle = new GUIStyle(GUI.skin.textArea);
                areaStyle.wordWrap = true;
                areaStyle.fixedHeight = 0;
                areaStyle.fixedHeight = areaStyle.CalcHeight(new GUIContent(_textResultPlantUml), position.width - 35);

                // Readonly TextArea hack
                _textResultScroll = s.scrollPosition;
                EditorGUILayout.SelectableLabel(_textResultPlantUml, EditorStyles.textArea, GUILayout.Height(areaStyle.fixedHeight));
            }
        }
    }
}