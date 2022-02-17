#nullable enable

using System;
using System.Collections.Generic;

namespace asmdef2pu.Interfaces
{
    [Serializable]
    internal enum DirectionStyle : byte
    {
        TopToBottom,
        BottomToTop
    }

    [Serializable]
    internal class StyleOptions
    {
        public DirectionStyle DirectionStyle = DirectionStyle.TopToBottom;
    }

    /// <summary>
    /// Options for the target assembly of the generated graph
    /// </summary>
    [Serializable]
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
        public StyleOptions StyleOptions = new();

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