#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.Compilation;
using asmdef2pu.Interfaces;

namespace asmdef2pu.Internal
{
    internal class PUAssembly : IPuAssembly, IPlantUmlResult, IEquatable<PUAssembly>
    {
        private readonly List<IPuAssembly> _dependencies = new List<IPuAssembly>();

        #region Constructors

        public PUAssembly(Assembly assembly)
        {
            Name = assembly.name;
            AsmdefPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
            OutputPath = assembly.outputPath;
        }

        #endregion

        #region IPuAssembly impls

        public string Name { get; set; } = "";
        public string AsmdefPath { get; set; } = "";
        public string OutputPath { get; set; } = "";
        IEnumerable<IPuAssembly> IPuAssembly.Dependencies => _dependencies;
        public bool IsUnityAssembly
        {
            get
            {
                string pattern = @"Unity.+";
                var match = Regex.Match(Name, pattern);
                return match.Success;
            }
        }

        public bool IsAssemblyCSharp
        {
            get
            {
                return Name == "Assembly-CSharp";
            }
        }

        public bool IsExistsInPackage
        {
            get
            {
                // Assembly-CSharp.dll null assigned
                if (string.IsNullOrEmpty(AsmdefPath))
                {
                    return false;
                }

                string pattern = @"Packages.+";
                var match = Regex.Match(AsmdefPath, pattern);
                return match.Success;
            }
        }

        #endregion

        #region IPlantUmlResult impls

        string IPlantUmlResult.Result => "";

        #endregion

        #region IEquatable impls
        public override bool Equals(object? obj) => this.Equals(obj as PUAssembly);
        public bool Equals(PUAssembly? p)
        {
            if (p is null)
                return false;
            if (System.Object.ReferenceEquals(this, p))
                return true;
            if (this.GetType() != p.GetType())
                return false;
            return Name == p.Name;
        }
        public override int GetHashCode() => Name.GetHashCode();
        public static bool operator ==(PUAssembly? lhs, PUAssembly? rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(PUAssembly? lhs, PUAssembly? rhs) => !(lhs == rhs);

        #endregion

        #region Helper methods

        public bool AddDependency(IPuAssembly assembly)
        {
            if (_dependencies.Contains(assembly))
            {
                return false;
            }
            _dependencies.Add(assembly);
            return true;
        }

        public string Asm(ExportOptions options)
        {
            string result = "";

            result += $"class \"{this.Name}\" << (A, orchid) >>" + " {\n";
            {
                if (options.bIgnoreUnityEngineUiDependency)
                {
                    bool bDependentUnityEngineUI = false;
                    foreach (var d in _dependencies)
                    {
                        if (d.Name == "UnityEngine.UI")
                        {
                            bDependentUnityEngineUI = true;
                            break;
                        }
                    }
                    if (bDependentUnityEngineUI)
                    {
                        result += $"\t use UnityEngine\n";
                    }
                }
            }
            result += "}\n";

            return result;
        }

        public string Dep(ExportOptions options)
        {
            string result = "";

            foreach (var d in _dependencies)
            {
                if (options.bIgnoreUnityAssemblyDependency)
                {
                    if (d.IsUnityAssembly)
                        continue;
                }
                else
                {
                    if (options.bIgnoreUnityEngineUiDependency)
                    {
                        if (d.Name == "UnityEngine.UI")
                            continue;
                    }
                }
                result += $"\"{d.Name}\" <-- \"{this.Name}\"\n";
            }
            return result;
        }

        #endregion
    }
}