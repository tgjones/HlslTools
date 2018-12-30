using System;
using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract partial class SyntaxRewriter : SyntaxVisitor<SyntaxNode>
    {
        public List<TNode> VisitList<TNode>(List<TNode> nodeList) where TNode : SyntaxNode
        {
            throw new NotImplementedException();
        }

        public SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> nodeList) where TNode : SyntaxNode
        {
            throw new NotImplementedException();
        }

        public new virtual SyntaxToken VisitSyntaxToken(SyntaxToken node)
        {
            throw new NotImplementedException();
        }
    }
}