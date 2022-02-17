#nullable enable

using System.Collections.Generic;

namespace asmdef2pu.Interfaces
{
    /// <summary>
    /// Options for the target assembly of the generated graph
    /// </summary>
    internal class TargetAssemblyOptions
    {
        /// <summary>
        /// If true exclude assemblies contained in Packages/ from the graph. (default: true)
        /// </summary>
        public bool bIgnorePackageAssembly = true;

        /// <summary>
        /// If true exclude assemblies with the name "Unity*" from the graph. (default: true)
        /// </summary>
        public bool bIgnoreUnityAssembly = true;

        /// <summary>
        /// if true Exclude Assembly-CSharp.dll from the graph. (default: true)
        /// </summary>
        public bool bIgnoreAssemblyCSharp = true;

        /// <summary>
        /// A list of regular expression patterns to specify paths in ".asmdef" that you don't want included in the graph.
        /// </summary>
        /// <returns></returns>
        public List<string> ignoreDirectoryPatterns = new();
    }

    /// <summary>
    /// 
    /// </summary>
    internal class ExportOptions
    {
        /// <summary>
        /// Generate namespace from assembly name split by dot
        /// e.g. Foo.Bar.dll => namespace Foo {}  
        /// </summary>
        public bool bNestedNamespace = true;

        public TargetAssemblyOptions TargetAssemblyOptions = new();

        #region Dependency Options

        /// <summary>
        /// 
        /// </summary>
        public bool bIgnoreUnityAssemblyDependency = false;

        /// <summary>
        /// 
        /// </summary>
        public bool bIgnoreUnityEngineUiDependency = true;


        #endregion
    }
}