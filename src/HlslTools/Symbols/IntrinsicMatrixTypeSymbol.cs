using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class IntrinsicMatrixTypeSymbol : IntrinsicNumericTypeSymbol
    {
        public IntrinsicMatrixTypeSymbol(string name, string documentation, ScalarType scalarType, int rows, int cols, Func<TypeSymbol, IEnumerable<Symbol>> lazyMembers = null)
            : base(SymbolKind.IntrinsicMatrixType, name, documentation, scalarType, lazyMembers)
        {
            Rows = rows;
            Cols = cols;
        }
        
        public int Rows { get; }
        public int Cols { get; }
    }
}