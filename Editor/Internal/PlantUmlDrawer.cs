#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Compilation;
using asmdef2pu.Interfaces;

namespace asmdef2pu.Internal
{
    class PlantUmlDrawer
    {
        readonly Dictionary<string, INode> _nodeMap = new();

        internal IAssembly Add(UnityEditor.Compilation.Assembly unityAssembly)
        {
            // Get info from Unity assembly
            var assemblyName = unityAssembly.name ?? "";
            var asmdefPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(unityAssembly.name) ?? "";
            var dependencies = unityAssembly.assemblyReferences;

            return CreateOrGetAssembly(assemblyName, asmdefPath, dependencies.Any(d => d.name == Constants.UnityEngineUiName));
        }

        internal void AddDependency(IAssembly target, UnityEditor.Compilation.Assembly unityAssembly)
        {
            if (_nodeMap.ContainsKey(target.Name))
            {
                // Create dependncy assembly
                var dep = Add(unityAssembly);

                // Add Reference
                ((Assembly)_nodeMap[target.Name]).AddDependency(dep);
            }
        }

        private IAssembly CreateOrGetAssembly(string name, string filePath, bool isDependentUnityEngine)
        {
            // Split Names
            // Foo => {"Foo"}
            // Foo.Bar.Baz => {"Foo", "Bar", "Baz"}
            var sep = ".";
            var sepName = name.Split(sep);
            var sepCount = sepName.Length;
            Scope? parent = null;
            string currentId = "";

            for (int level = 0; level < sepCount; level++)
            {
                // Update currentId
                // Foo -> Foo.Bar -> Foo.Bar.Baz
                if (level == 0)
                {
                    currentId += sepName[level];
                }
                else
                {
                    currentId += sep + sepName[level];
                }

                // Final level make assembly
                if (level == sepCount - 1)
                {
                    // If already cached, do not anything
                    if (_nodeMap.ContainsKey(currentId))
                    {
                        return (Assembly)_nodeMap[currentId];
                    }
                    else
                    {
                        var asmb = new Assembly(currentId, parent, filePath, isDependentUnityEngine);
                        _nodeMap.Add(currentId, asmb);
                        return asmb;
                    }
                }
                // Other levels make scope
                else
                {
                    Scope scope;
                    // If already cached, use this one
                    if (_nodeMap.ContainsKey(currentId))
                    {
                        scope = (Scope)_nodeMap[currentId];
                    }
                    // Not exists in cache, create new one
                    else
                    {
                        scope = new Scope(sepName[level], parent);
                        _nodeMap.Add(currentId, scope);
                    }
                    // Update parent
                    parent = scope;
                }
            }

            throw new InvalidProgramException();
        }

        string DrawPlantUmlComponent(INode node, ExportOptions options, int indent = 0)
        {
            var result = "";

            // Calc indent tabs
            var thisNest = "";
            for (var i = 0; i < indent; i++)
                thisNest += "\t";

            // Convert name for Plant UML component 
            // Foo-Bar => Foo_Bar
            var componentName = node.FullName;
            componentName = componentName.Replace("-", "_");

            result += $"{thisNest}component {componentName} [\n";
            result += $"{thisNest}\t{node.Name}\n";

            // Draw comment
            if (options.bHideUnityEngineDependency)
            {
                if (node is IAssembly assembly)
                {
                    if (assembly.IsDependentUnityEngine)
                    {
                        result += $"{thisNest}\t==\n";
                        result += $"{thisNest}\t" + "Use UnityEngine" + "\n";
                    }
                }
            }

            result += $"{thisNest}]\n";
            return result;
        }

        internal string DrawComponents(ExportOptions options)
        {
            string result = "";

            foreach (var pair in _nodeMap)
            {
                var node = pair.Value;

                // Skip not root node
                if (!node.IsRoot)
                {
                    continue;
                }

                // Root and Leaf node, draw component
                if (node is IAssembly assembly)
                {
                    result += DrawPlantUmlComponent(node, options, 0);
                    continue;
                }

                if (node is Scope scope)
                {
                    result += $"package {node.Name}" + " {\n";
                    foreach (var nsc in node.Children)
                    {
                        DrawChildNameSpace(ref result, nsc, 1, options);
                    }
                    result += "}\n";
                }
            }

            return result;
        }

        internal string DrawDependencies(ExportOptions option)
        {
            string result = "";

            foreach (var pair in _nodeMap)
            {
                var node = pair.Value;

                // Root and Leaf node, draw component
                if (node is Assembly assembly)
                {
                    result += assembly.Dep(option);
                }
            }

            return result;
        }

        void DrawChildNameSpace(ref string result, INode node, int nest, ExportOptions options)
        {
            var thisNest = "";
            for (var i = 0; i < nest; i++)
                thisNest += "\t";

            if (node is IAssembly assembly)
            {
                result += DrawPlantUmlComponent(node, options, nest);
            }
            else
            {
                result += $"{thisNest}package {node.FullName}" + $" as \"{node.Name}\"" + " {\n";

                foreach (var nsc in node.Children)
                {
                    DrawChildNameSpace(ref result, nsc, nest + 1, options);
                }

                result += $"{thisNest}" + "}\n";
            }
        }
    }
}