using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
        }

        internal SourceFunctionSymbol(FunctionDefinitionSyntax syntax, Symbol parent, TypeSymbol returnType, Func<InvocableSymbol, IEnumerable<ParameterSymbol>> lazyParameters = null)
            : base(syntax.Name.GetUnqualifiedName().Name.Text, string.Empty, parent, returnType, lazyParameters)
        {
            DeclarationSyntaxes = new List<FunctionDeclarationSyntax>();
            DefinitionSyntax = syntax;
        }

        public List<FunctionDeclarationSyntax> DeclarationSyntaxes { get; }
        public FunctionDefinitionSyntax DefinitionSyntax { get; internal set; }

        public override ImmutableArray<SourceRange> Locations
        {
            get
            {
                var result = ImmutableArray.CreateBuilder<SourceRange>();

                SourceRange getNameRange(FunctionSyntax function) => function.Name.GetUnqualifiedName().Name.SourceRange;

                result.AddRange(DeclarationSyntaxes.Select(getNameRange));

                if (DefinitionSyntax != null)
                    result.Add(getNameRange(DefinitionSyntax));

                return result.ToImmutable();
            }
        }
    }
}