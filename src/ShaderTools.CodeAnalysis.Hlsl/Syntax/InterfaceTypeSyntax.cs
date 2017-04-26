using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class InterfaceTypeSyntax : TypeDefinitionSyntax
    {
        public readonly SyntaxToken InterfaceKeyword;
        public readonly SyntaxToken Name;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<FunctionDeclarationSyntax> Methods;
        public readonly SyntaxToken CloseBraceToken;

        public override SyntaxToken NameToken => Name;

        public InterfaceTypeSyntax(SyntaxToken interfaceKeyword, SyntaxToken name, SyntaxToken openBraceToken, List<FunctionDeclarationSyntax> methods, SyntaxToken closeBraceToken)
            : base(SyntaxKind.InterfaceType)
        {
            RegisterChildNode(out InterfaceKeyword, interfaceKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Methods, methods);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitInterfaceType(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitInterfaceType(this);
        }
    }
}