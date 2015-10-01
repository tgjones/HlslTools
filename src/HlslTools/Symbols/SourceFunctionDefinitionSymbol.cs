using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public class SourceFunctionDefinitionSymbol : FunctionDefinitionSymbol
    {
        public SourceFunctionDefinitionSymbol(FunctionDefinitionSyntax syntax, TypeSymbol returnType, Func<FunctionSymbol, IEnumerable<ParameterSymbol>> lazyParameters)
            : base(syntax.Name.GetName(), string.Empty, returnType, lazyParameters)
        {
            Syntax = syntax;
        }

        public FunctionDefinitionSyntax Syntax { get; }
    }
}