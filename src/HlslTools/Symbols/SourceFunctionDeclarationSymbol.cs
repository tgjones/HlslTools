using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public class SourceFunctionDeclarationSymbol : FunctionDeclarationSymbol
    {
        public SourceFunctionDeclarationSymbol(FunctionDeclarationSyntax syntax, TypeSymbol returnType, Func<FunctionSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(syntax.Name.Text, string.Empty, returnType, lazyParameters)
        {
            Syntax = syntax;
        }

        public FunctionDeclarationSyntax Syntax { get; }
    }
}