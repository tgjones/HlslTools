using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    /// <summary>
    /// Base class for all nodes in the Unity AST.
    /// </summary>
    [DebuggerDisplay("{ToFullString()}")]
    public abstract class SyntaxNode : SyntaxNodeBase
    {
        public SyntaxKind Kind => (SyntaxKind) RawKind;

        public new SyntaxNode Parent => (SyntaxNode) base.Parent;

        public override bool ContainsDirectives => false;

        public override string Language => LanguageNames.ShaderLab;

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

        public override ISyntaxToken FindToken(SourceLocation position, bool descendIntoTrivia = false)
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            WriteTo(sb, false, false);
            return sb.ToString();
        }

        public override string ToFullString()
        {
            var sb = new StringBuilder();
            WriteTo(sb, true, true);
            return sb.ToString();
        }

        protected internal virtual void WriteTo(StringBuilder sb, bool leading, bool trailing)
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
                    ((SyntaxNode) child).WriteTo(sb, leading | !first, trailing | (i < lastIndex));
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