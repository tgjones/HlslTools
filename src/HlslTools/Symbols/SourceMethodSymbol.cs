using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class SourceMethodSymbol : MethodSymbol
    {
        public SourceMethodSymbol(FunctionDeclarationSyntax syntax, TypeSymbol parent, TypeSymbol returnType, Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(syntax.Name.Text, string.Empty, parent, returnType, lazyParameters)
        {
            DeclarationSyntax = syntax;
        }

        public SourceMethodSymbol(FunctionDefinitionSyntax syntax, TypeSymbol parent, TypeSymbol returnType, Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(syntax.Name.GetName(), string.Empty, parent, returnType, lazyParameters)
        {
            DefinitionSyntax = syntax;
        }

        public FunctionDeclarationSyntax DeclarationSyntax { get; }
        public FunctionDefinitionSyntax DefinitionSyntax { get; internal set; }
    }
}