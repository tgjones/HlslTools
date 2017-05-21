using System;
using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class TypeAliasSymbol : TypeSymbol, IAliasSymbol
    {
        internal TypeAliasSymbol(TypeAliasSyntax syntax, TypeSymbol valueType)
            : base(SymbolKind.TypeAlias, syntax.Identifier.Text, string.Empty, null)
        {
            ValueType = valueType;
            Location = syntax.Identifier.SourceRange;
        }

        public TypeSymbol ValueType { get; }

        ITypeSymbol IAliasSymbol.Target => ValueType;

        public override SourceRange? Location { get; }

        public override IEnumerable<T> LookupMembers<T>(string name)
        {
            return ValueType.LookupMembers<T>(name);
        }
    }
}