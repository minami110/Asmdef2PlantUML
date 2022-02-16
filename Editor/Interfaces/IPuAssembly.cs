using System.Collections.Generic;

namespace asmdef2pu
{
    internal interface IPuAssembly
    {
        public string Name { get; }
        public string AsmdefPath { get; }
        public string OutputPath { get; }
        public IEnumerable<IPuAssembly> Dependencies { get; }
    }
}