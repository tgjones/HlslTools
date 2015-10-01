using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public class SourceMethodDefinitionSymbol : MethodDefinitionSymbol
    {
        public SourceMethodDefinitionSymbol(FunctionDefinitionSyntax syntax, TypeSymbol parent, TypeSymbol returnType, Func<MethodSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(syntax.Name.GetName(), string.Empty, parent, returnType, lazyParameters)
        {
            Syntax = syntax;
        }

        public FunctionDefinitionSyntax Syntax { get; set; }
    }
}