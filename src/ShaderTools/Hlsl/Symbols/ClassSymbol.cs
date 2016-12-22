using System.Collections.Immutable;
using ShaderTools.Core.Symbols;
using ShaderTools.Hlsl.Binding;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Hlsl.Symbols
{
    public sealed class ClassSymbol : ClassOrStructSymbol
    {
        public ClassTypeSyntax Syntax { get; }

        internal ClassSymbol(ClassTypeSyntax syntax, Symbol parent, ClassOrStructSymbol baseType, ImmutableArray<InterfaceSymbol> baseInterfaces, Binder binder)
            : base(SymbolKind.Class, syntax.Name.Text, parent, baseType, baseInterfaces)
        {
            Syntax = syntax;
            Binder = binder;
        }
    }
}