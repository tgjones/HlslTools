using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class ClassTypeSyntax : TypeDefinitionSyntax
    {
        public readonly SyntaxToken ClassKeyword;
        public readonly SyntaxToken Name;
        public readonly BaseListSyntax BaseList;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<SyntaxNode> Members;
        public readonly SyntaxToken CloseBraceToken;

        public override SyntaxToken NameToken => Name;

        public ClassTypeSyntax(SyntaxToken classKeyword, SyntaxToken name, BaseListSyntax baseList, SyntaxToken openBraceToken, List<SyntaxNode> members, SyntaxToken closeBraceToken)
            : base(SyntaxKind.ClassType)
        {
            RegisterChildNode(out ClassKeyword, classKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out BaseList, baseList);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Members, members);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitClassType(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitClassType(this);
        }
    }
}