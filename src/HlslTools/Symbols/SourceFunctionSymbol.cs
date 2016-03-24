using System;
using System.Collections.Generic;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    public sealed class SourceFunctionSymbol : FunctionSymbol
    {
        public SourceFunctionSymbol(FunctionDeclarationSyntax syntax, Symbol parent, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(syntax.Name.GetName(), string.Empty, parent, returnType, lazyParameters)
        {
            DeclarationSyntaxes = new List<FunctionDeclarationSyntax> { syntax };
        }

        public SourceFunctionSymbol(FunctionDefinitionSyntax syntax, Symbol parent, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(syntax.Name.GetName(), string.Empty, parent, returnType, lazyParameters)
        {
            DeclarationSyntaxes = new List<FunctionDeclarationSyntax>();
            DefinitionSyntax = syntax;
        }

        public List<FunctionDeclarationSyntax> DeclarationSyntaxes { get; }
        public FunctionDefinitionSyntax DefinitionSyntax { get; internal set; }
    }
}