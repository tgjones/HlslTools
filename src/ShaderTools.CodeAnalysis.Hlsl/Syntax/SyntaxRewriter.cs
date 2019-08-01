using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract partial class SyntaxRewriter : SyntaxVisitor<SyntaxNode>
    {
        public List<TNode> VisitList<TNode>(List<TNode> nodeList) where TNode : SyntaxNode
        {
            List<TNode> newNodes = null;
            for (var i = 0; i < nodeList.Count; i++)
            {
                var n = nodeList[i];
                var nt = n as SyntaxToken;
                var nn = nt == null ? n.Accept(this) : this.VisitToken(nt);

                if (nn != n)
                {
                    if (newNodes == null)
                        newNodes = new List<TNode>(nodeList.Take(i));
                    if (nn != null)
                    {
                        if (!(nn is TNode))
                            throw new Exception($"Expected a node of type {typeof(TNode)}, but got a node of type {nn.GetType()}.");
                        newNodes.Add((TNode) nn);
                    }
                }
                else if (newNodes != null)
                {
                    newNodes.Add(n);
                }
            }

            return newNodes ?? nodeList;
        }

        public SeparatedSyntaxList<TNode> VisitList<TNode>(SeparatedSyntaxList<TNode> nodeList) where TNode : SyntaxNode
        {
            List<SyntaxNodeBase> newNodes = null;
            for (var i = 0; i < nodeList.Count; i++)
            {
                var n = nodeList[i];
                var nn = n.Accept(this);
                if (nn != n)
                {
                    if (newNodes == null)
                        newNodes = new List<SyntaxNodeBase>(nodeList.Take(i));
                    newNodes.Add(nn);
                }
                else if (newNodes != null)
                {
                    newNodes.Add(n);
                }
            }

            if (newNodes == null)
                return nodeList;
            return new SeparatedSyntaxList<TNode>(newNodes);
        }

        public virtual SyntaxToken VisitToken(SyntaxToken token)
        {
            return token;
        }
    }
}