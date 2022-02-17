#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEditor.Compilation;
using asmdef2pu.Interfaces;
using asmdef2pu.Internal;

namespace asmdef2pu
{
    static class Generator
    {
        class AssemblyChecker : IAssembly
        {
            readonly UnityEditor.Compilation.Assembly _unityAssembly;
            readonly ExportOptions _options;

            public AssemblyChecker(UnityEditor.Compilation.Assembly unityAssembly, ExportOptions options)
            {
                _unityAssembly = unityAssembly;
                _options = options;
            }

            #region IAssembly impls

            string IAssembly.Name => _unityAssembly.name ?? "";
            string IAssembly.AsmdefPath => CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(_unityAssembly.name) ?? "";
            IEnumerable<IAssembly> IAssembly.Dependencies => new IAssembly[0];
            bool IAssembly.IsDependentUnityEngine => false;

            #endregion

            public bool IsTargetAssembly()
            {
                // Exclude Packages/ .asmdef
                if (_options.TargetAssemblyOptions.bIgnorePackageAssembly)
                {
                    if ((this as IAssembly).IsExistsInPackage)
                        return false;
                }

                // Exclude Unity .asmdef
                if (_options.TargetAssemblyOptions.bIgnoreUnityAssembly)
                {
                    if ((this as IAssembly).IsUnityTechnologiesAssembly)
                        return false;
                }

                // Exclude Assembly-CSharp.dll
                if (_options.TargetAssemblyOptions.bIgnoreAssemblyCSharp)
                {
                    if ((this as IAssembly).IsAssemblyCSharp)
                        return false;
                }

                // Check User defined ignored pattern
                if (_options.TargetAssemblyOptions.ignoreDirectoryPatterns.Count > 0)
                {
                    bool bExcluded = false;
                    foreach (var pattern in _options.TargetAssemblyOptions.ignoreDirectoryPatterns)
                    {
                        var asmpath = (this as IAssembly).AsmdefPath;
                        if (!string.IsNullOrEmpty(asmpath)) // Assembly-CSharp may be null
                        {
                            var match = Regex.Match(asmpath, pattern);
                            if (match.Success)
                            {
                                bExcluded = true;
                                break;
                            }
                        }
                    }
                    if (bExcluded)
                    {
                        // skip this assembly
                        return false;
                    }
                }

                return true;
            }

            public bool IsDependencyTargetAssembly()
            {
                if (_options.bIgnorePackageAssemblyDependency)
                {
                    if ((this as IAssembly).IsExistsInPackage)
                        return false;
                }

                if (_options.bIgnoreUnityAssemblyDependency)
                {
                    if ((this as IAssembly).IsUnityTechnologiesAssembly)
                        return false;
                }

                return true;
            }
        }

        internal static string Generate(ExportOptions options)
        {
            // Make Drawer
            var drawer = new ComponentDrawer();

            // Gather Unity Assemblies
            // Player Build included only (Excluded test assembly)
            var assemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
            foreach (var unityAssembly in assemblies)
            {
                IAssembly targetAssembly;
                {
                    var checker = new AssemblyChecker(unityAssembly, options);
                    if (!checker.IsTargetAssembly())
                    {
                        continue;
                    }
                    // Add Drawer
                    targetAssembly = drawer.Add(unityAssembly);
                }

                // Make Dependencies Assemblies
                var assemblyRefs = unityAssembly.assemblyReferences;
                foreach (var assmblyRef in assemblyRefs)
                {
                    var checker = new AssemblyChecker(assmblyRef, options);
                    if (checker.IsDependencyTargetAssembly())
                    {
                        // Add Drawer
                        drawer.AddDependency(targetAssembly, assmblyRef);
                    }
                }
            }

            // To PlantUml
            string output = "";
            output += "@startuml\n";
            output += "skinparam componentStyle uml1\n";
            switch (options.StyleOptions.LineStyle)
            {
                case LineStyle.Polyline:
                    output += "skinparam linetype polyline\n";
                    break;
                case LineStyle.Ortho:
                    output += "skinparam linetype ortho\n";
                    break;
                default:
                    break;
            }
            output += "\n";

            // Package Defines
            output += "' ----- Begin Assembly Namespaces Definition -----\n\n";
            output += drawer.DrawComponents(options);
            output += "\n' ----- End Assembly Namespaces Definition -----\n\n";

            // Dependency Defines
            output += "' ----- Begin Dependencies -----\n\n";
            output += drawer.DrawDependencies(options);
            output += "\n' ----- End Dependencies -----\n\n";

            output += "\n@enduml";

            return output;
        }
    }
}