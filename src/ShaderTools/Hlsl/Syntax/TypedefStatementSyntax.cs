using System.Collections.Generic;
using ShaderTools.Core.Syntax;

namespace ShaderTools.Hlsl.Syntax
{
    public class TypedefStatementSyntax : StatementSyntax
    {
        public readonly SyntaxToken TypedefKeyword;
        public readonly List<SyntaxToken> Modifiers;
        public readonly TypeSyntax Type;
        public readonly SeparatedSyntaxList<TypeAliasSyntax> Declarators;
        public readonly SyntaxToken SemicolonToken;

        public TypedefStatementSyntax(SyntaxToken typedefKeyword, List<SyntaxToken> modifiers, TypeSyntax type, SeparatedSyntaxList<TypeAliasSyntax> declarators, SyntaxToken semicolonToken)
            : base(SyntaxKind.TypedefStatement, new List<AttributeSyntax>())
        {
            RegisterChildNode(out TypedefKeyword, typedefKeyword);
            RegisterChildNodes(out Modifiers, modifiers);
            RegisterChildNode(out Type, type);
            RegisterChildNodes(out Declarators, declarators);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitTypedefStatement(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitTypedefStatement(this);
        }
    }
}