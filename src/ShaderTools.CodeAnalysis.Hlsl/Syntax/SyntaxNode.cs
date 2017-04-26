using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Parser;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    /// <summary>
    /// Base class for all nodes in the HLSL AST.
    /// </summary>
    [DebuggerDisplay("{ToString(true)}")]
    public abstract class SyntaxNode : SyntaxNodeBase
    {
        public SyntaxKind Kind => (SyntaxKind) RawKind;

        public new SyntaxNode Parent => (SyntaxNode) base.Parent;

        // TODO: Evaluate this upfront.
        public override bool ContainsDirectives => this.DescendantTokens().Any(t => t.ContainsDirectives);

        public virtual IEnumerable<DirectiveTriviaSyntax> GetDirectives()
        {
            return ChildNodes.Cast<SyntaxNode>().SelectMany(x => x.GetDirectives());
        }

        protected SyntaxNode(SyntaxKind kind, IEnumerable<Diagnostic> diagnostics)
            : base((ushort) kind, diagnostics)
        {
            
        }

        protected SyntaxNode(SyntaxKind kind)
            : base((ushort) kind)
        {
            
        }

        public abstract void Accept(SyntaxVisitor visitor);
        public abstract T Accept<T>(SyntaxVisitor<T> visitor);

        public SyntaxToken FindToken(SourceLocation position, bool descendIntoTrivia = false)
        {
            if (FullSourceRange.End == position)
            {
                var compilationUnit = this as CompilationUnitSyntax;
                if (compilationUnit != null)
                    return compilationUnit.EndOfFileToken;
            }

            if (!FullSourceRange.Contains(position))
                throw new ArgumentOutOfRangeException(nameof(position));

            var children = ChildNodes.Where(nodeOrToken => nodeOrToken.FullSourceRange.Contains(position));
            Debug.Assert(children.Any());

            var child = children.First();

            if (!child.IsToken)
                return ((SyntaxNode) child).FindToken(position, descendIntoTrivia);

            var token = (SyntaxToken) child;

            if (descendIntoTrivia)
            {
                var triviaStructure = token.LeadingTrivia.Concat(token.TrailingTrivia)
                    .FirstOrDefault(t => t is StructuredTriviaSyntax && t.FullSourceRange.Contains(position));

                if (triviaStructure != null)
                    return triviaStructure.FindToken(position, true);
            }

            return token;
        }

        internal virtual DirectiveStack ApplyDirectives(DirectiveStack stack)
        {
            if (ContainsDirectives)
            {
                foreach (var childNode in ChildNodes)
                    if (childNode != null)
                        stack = ((SyntaxNode) childNode).ApplyDirectives(stack);
            }
            return stack;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool includeNonRootFile)
        {
            var sb = new StringBuilder();
            WriteTo(sb, false, false, includeNonRootFile, false);
            return sb.ToString();
        }

        public string ToStringIgnoringMacroReferences()
        {
            var sb = new StringBuilder();
            WriteTo(sb, false, false, true, true);
            return sb.ToString();
        }

        public virtual string ToFullString()
        {
            var sb = new StringBuilder();
            WriteTo(sb, true, true, false, false);
            return sb.ToString();
        }

        protected internal virtual void WriteTo(StringBuilder sb, bool leading, bool trailing, bool includeNonRootFile, bool ignoreMacroReferences)
        {
            var first = true;
            var lastIndex = ChildNodes.Count - 1;
            for (; lastIndex >= 0; lastIndex--)
            {
                var child = ChildNodes[lastIndex];
                if (child != null)
                    break;
            }

            for (var i = 0; i <= lastIndex; i++)
            {
                var child = ChildNodes[i];
                if (child != null)
                {
                    ((SyntaxNode) child).WriteTo(sb, leading | !first, trailing | (i < lastIndex), includeNonRootFile, ignoreMacroReferences);
                    first = false;
                }
            }
        }

        public SyntaxToken GetLastChildToken()
        {
            return ChildNodes.LastOrDefault(n => n.IsToken) as SyntaxToken;
        }
    }
}