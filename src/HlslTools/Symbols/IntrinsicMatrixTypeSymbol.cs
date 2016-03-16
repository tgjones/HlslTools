using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class IntrinsicMatrixTypeSymbol : IntrinsicTypeSymbol
    {
        public IntrinsicMatrixTypeSymbol(string name, string documentation, ScalarType scalarType, int rows, int cols, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(SymbolKind.IntrinsicMatrixType, name, documentation, lazyMembers)
        {
            ScalarType = scalarType;
            Rows = rows;
            Cols = cols;
        }

        public ScalarType ScalarType { get; }
        public int Rows { get; }
        public int Cols { get; }
    }
}