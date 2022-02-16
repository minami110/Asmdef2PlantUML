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
        public bool bNestedNamespace = true;
        public bool bIgnoreUnityAssembly = true;
        public bool bIgnoreAssemblyCSharp = true;
        public bool bIgnoreUnityEngineUiDependency = true;
    }
}