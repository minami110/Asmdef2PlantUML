using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace asmdef2pu.Interfaces
{
    internal interface IAssembly
    {
        public string Name { get; }
        public string AsmdefPath { get; }
        public IEnumerable<IAssembly> Dependencies { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsUnityTechnologiesAssembly
        {
            get
            {
                string pattern = @"Unity.+";
                var match = Regex.Match(Name, pattern);
                return match.Success;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsAssemblyCSharp => Name == Constants.AssemblyCSharpName;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
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

        public bool IsUnityEngineUi => Name == Constants.UnityEngineUiName;

        public bool IsDependentUnityEngine { get; }

    }
}