#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace asmdef2pu
{
    internal class ExportOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public bool bNestedNamespace = true;

        #region Target Assembly Options

        /// <summary>
        /// 
        /// </summary>
        public bool bIgnorePackageAssembly = true;

        /// <summary>
        /// 
        /// </summary>
        public bool bIgnoreUnityAssembly = true;


        /// <summary>
        /// 
        /// </summary>
        public bool bIgnoreAssemblyCSharp = true;

        public List<string> ignoreDirectoryPatterns = new();

        #endregion

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