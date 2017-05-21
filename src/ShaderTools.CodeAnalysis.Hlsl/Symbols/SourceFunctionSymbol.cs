using System;
using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class SourceFunctionSymbol : FunctionSymbol
    {
        internal SourceFunctionSymbol(FunctionDeclarationSyntax syntax, Symbol parent, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(syntax.Name.GetUnqualifiedName().Name.Text, string.Empty, parent, returnType, lazyParameters)
        {
            DeclarationSyntaxes = new List<FunctionDeclarationSyntax> { syntax };

            Location = syntax.Name.SourceRange;
        }

        internal SourceFunctionSymbol(FunctionDefinitionSyntax syntax, Symbol parent, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(syntax.Name.GetUnqualifiedName().Name.Text, string.Empty, parent, returnType, lazyParameters)
        {
            DeclarationSyntaxes = new List<FunctionDeclarationSyntax>();
            DefinitionSyntax = syntax;

            Location = syntax.Name.SourceRange;
        }

        public List<FunctionDeclarationSyntax> DeclarationSyntaxes { get; }
        public FunctionDefinitionSyntax DefinitionSyntax { get; internal set; }

        public override SourceRange? Location { get; }
    }
}