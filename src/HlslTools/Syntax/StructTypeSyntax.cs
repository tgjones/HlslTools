using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class StructTypeSyntax : TypeDefinitionSyntax
    {
        public readonly SyntaxToken StructKeyword;
        public readonly SyntaxToken Name;
        public readonly BaseListSyntax BaseList;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<VariableDeclarationStatementSyntax> Fields;
        public readonly SyntaxToken CloseBraceToken;

        public override SyntaxToken NameToken => Name;

        public StructTypeSyntax(SyntaxToken structKeyword, SyntaxToken name, BaseListSyntax baseList, SyntaxToken openBraceToken, List<VariableDeclarationStatementSyntax> fields, SyntaxToken closeBraceToken)
            : base(SyntaxKind.StructType)
        {
            RegisterChildNode(out StructKeyword, structKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out BaseList, baseList);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Fields, fields);
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