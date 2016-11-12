using System.Collections.Immutable;
using HlslTools.Binding;
using HlslTools.Syntax;

namespace HlslTools.Symbols
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