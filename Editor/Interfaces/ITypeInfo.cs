#nullable enable

using System;
using System.Collections.Generic;

namespace asmdef2pu.Interfaces
{
    internal interface ITypeInfo
    {
        public string Name { get; }
        public string FullName { get; }
        public bool IsClass { get; }
        public bool IsAbstract { get; }
        public bool IsInterface { get; }
        public bool IsEnum { get; }
    }
}