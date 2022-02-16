#nullable enable

using System;
using System.Collections.Generic;
using UnityEditor.Compilation;

namespace asmdef2pu
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

            if (options.bIgnoreUnityAssembly)
            {
                if (Name.Contains("Unity"))
                {
                    return result;
                }
            }

            if (options.bIgnoreAssemblyCSharp)
            {
                if (Name == "Assembly-CSharp")
                {
                    return result;
                }
            }

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

            if (options.bIgnoreUnityAssembly)
            {
                if (Name.Contains("Unity"))
                {
                    return result;
                }
            }

            if (options.bIgnoreAssemblyCSharp)
            {
                if (Name == "Assembly-CSharp")
                {
                    return result;
                }
            }

            foreach (var d in _dependencies)
            {
                if (options.bIgnoreUnityEngineUiDependency)
                {
                    if (d.Name == "UnityEngine.UI")
                    {
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