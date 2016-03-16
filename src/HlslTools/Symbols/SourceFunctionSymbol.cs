using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class SourceFunctionSymbol : FunctionSymbol
    {
        public SourceFunctionSymbol(FunctionDeclarationSyntax syntax, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(syntax.Name.Text, string.Empty, returnType, lazyParameters)
        {
            DeclarationSyntax = syntax;
        }

        public SourceFunctionSymbol(FunctionDefinitionSyntax syntax, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(syntax.Name.GetName(), string.Empty, returnType, lazyParameters)
        {
            DefinitionSyntax = syntax;
        }

        public FunctionDeclarationSyntax DeclarationSyntax { get; }
        public FunctionDefinitionSyntax DefinitionSyntax { get; internal set; }
    }
}