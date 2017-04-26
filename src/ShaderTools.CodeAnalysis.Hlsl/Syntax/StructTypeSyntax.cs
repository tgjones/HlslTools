using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class StructTypeSyntax : TypeDefinitionSyntax
    {
        public readonly SyntaxToken StructKeyword;
        public readonly SyntaxToken Name;
        public readonly BaseListSyntax BaseList;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<SyntaxNode> Members;
        public readonly SyntaxToken CloseBraceToken;

        public override SyntaxToken NameToken => Name;

        public bool IsClass => Kind == SyntaxKind.ClassType;

        public StructTypeSyntax(SyntaxToken structKeyword, SyntaxToken name, BaseListSyntax baseList, SyntaxToken openBraceToken, List<SyntaxNode> members, SyntaxToken closeBraceToken)
            : base(structKeyword.Kind == SyntaxKind.ClassKeyword ? SyntaxKind.ClassType : SyntaxKind.StructType)
        {
            RegisterChildNode(out StructKeyword, structKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out BaseList, baseList);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Members, members);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitStructType(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitStructType(this);
        }
    }
}