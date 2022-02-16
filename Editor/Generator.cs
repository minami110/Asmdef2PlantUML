#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace asmdef2pu
{
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
                // Check Options specified ignore patten
                {
                    var _puAssembly = new PUAssembly(assembly);

                    // Exclude Packages/ .asmdef
                    if (options.bIgnorePackageAssembly)
                    {
                        if (_puAssembly.IsExistsInPackage)
                            continue;
                    }

                    // Exclude Unity .asmdef
                    if (options.bIgnoreUnityAssembly)
                    {
                        if (_puAssembly.IsUnityAssembly)
                            continue;
                    }

                    // Exclude Assembly-CSharp.dll
                    if (options.bIgnoreAssemblyCSharp)
                    {
                        if (_puAssembly.IsAssemblyCSharp)
                            continue;
                    }

                    // Check User defined ignored pattern
                    if (options.ignoreDirectoryPatterns.Count > 0)
                    {
                        bool bExcluded = false;
                        foreach (var pattern in options.ignoreDirectoryPatterns)
                        {
                            var asmpath = _puAssembly.AsmdefPath;
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
                            continue;
                        }
                    }
                }

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

            // To PlantUml
            string output = "";
            output += "@startuml\n\n";

            // Package Defines
            if (options.bNestedNamespace)
            {
                output += "' ----- Begin Assembly Namespaces Definition -----\n\n";

                var nsd = new NamespaceDrawer();
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