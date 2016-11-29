using System;

namespace ShaderTools.Hlsl.Syntax
{
    public abstract class DeclarationNameSyntax : SyntaxNode
    {
        protected DeclarationNameSyntax(SyntaxKind kind)
            : base(kind)
        {

        }

        public string GetName()
        {
            return ToString(true).Replace(Environment.NewLine, string.Empty);
        }

        public abstract IdentifierDeclarationNameSyntax GetUnqualifiedName();
    }
}