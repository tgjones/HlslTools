using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public class SourceMethodDeclarationSymbol : MethodDeclarationSymbol
    {
        public SourceMethodDeclarationSymbol(FunctionDeclarationSyntax syntax, TypeSymbol parent, TypeSymbol returnType, Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(syntax.Name.Text, string.Empty, parent, returnType, lazyParameters)
        {
        }
    }
}