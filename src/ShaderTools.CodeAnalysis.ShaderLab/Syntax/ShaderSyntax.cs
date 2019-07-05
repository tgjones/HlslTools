using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class ShaderSyntax : SyntaxNode
    {
        public readonly SyntaxToken ShaderKeyword;
        public readonly SyntaxToken NameToken;
        public readonly SyntaxToken OpenBraceToken;
        public readonly ShaderPropertiesSyntax Properties;
        public readonly ShaderIncludeSyntax ShaderInclude;
        public readonly List<SyntaxNode> Statements;
        public readonly List<CommandSyntax> StateProperties;
        public readonly SyntaxToken CloseBraceToken;

        public ShaderSyntax(SyntaxToken shaderKeyword, SyntaxToken nameToken, SyntaxToken openBraceToken, ShaderPropertiesSyntax properties, ShaderIncludeSyntax shaderInclude, List<SyntaxNode> statements, List<CommandSyntax> stateProperties, SyntaxToken closeBraceToken)
            : base(SyntaxKind.Shader)
        {
            RegisterChildNode(out ShaderKeyword, shaderKeyword);
            RegisterChildNode(out NameToken, nameToken);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNode(out Properties, properties);
            RegisterChildNode(out ShaderInclude, shaderInclude);
            RegisterChildNodes(out Statements, statements);
            RegisterChildNodes(out StateProperties, stateProperties);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitShader(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitShader(this);
        }
    }
}