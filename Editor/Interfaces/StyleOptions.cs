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
    internal enum LineStyle : byte
    {
        Default,
        Polyline,
        Ortho
    }

    [Serializable]
    internal class StyleOptions
    {
        public DirectionStyle DirectionStyle = DirectionStyle.TopToBottom;
        public LineStyle LineStyle = LineStyle.Ortho;
    }
}