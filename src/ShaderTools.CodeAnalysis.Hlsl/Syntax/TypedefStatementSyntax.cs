using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed partial class TypedefStatementSyntax : StatementSyntax
    {
        public TypedefStatementSyntax(SyntaxToken typedefKeyword, List<SyntaxToken> modifiers, TypeSyntax type, SeparatedSyntaxList<TypeAliasSyntax> declarators, SyntaxToken semicolonToken)
            : this(new List<AttributeSyntax>(), typedefKeyword, modifiers, type, declarators, semicolonToken)
        {
        }
    }
}