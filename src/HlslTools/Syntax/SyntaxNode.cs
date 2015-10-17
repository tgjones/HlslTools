using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HlslTools.Diagnostics;
using HlslTools.Parser;

namespace HlslTools.Syntax
{
    /// <summary>
    /// Base class for all nodes in the HLSL AST.
    /// </summary>
    [DebuggerDisplay("{ToString(true)}")]
    public abstract class SyntaxNode
    {
        public readonly SyntaxKind Kind;
        public SyntaxNode Parent { get; internal set; }
        public SourceRange SourceRange { get; protected set; }
        public SourceRange FullSourceRange { get; protected set; }

        public string DocumentationSummary;

        public IList<SyntaxNode> ChildNodes { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public virtual bool IsToken => false;

        public virtual bool IsMissing
        {
            get { return ChildNodes.All(n => n.IsMissing); }
        }

        public virtual bool ContainsDiagnostics
        {
            get { return Diagnostics.Any() || ChildNodes.Any(t => t.ContainsDiagnostics); }
        }

        public virtual bool ContainsDirectives
        {
            get { return this.DescendantTokens().Any(t => t.ContainsDirectives); }
        }

        public virtual IEnumerable<Diagnostic> GetDiagnostics()
        {
            return Diagnostics.Union(ChildNodes.SelectMany(x => x.GetDiagnostics()));
        }

        public virtual IEnumerable<DirectiveTriviaSyntax> GetDirectives()
        {
            return ChildNodes.SelectMany(x => x.GetDirectives());
        }

        protected SyntaxNode(SyntaxKind kind, IEnumerable<Diagnostic> diagnostics)
        {
            ChildNodes = new List<SyntaxNode>();
            Kind = kind;
            Diagnostics = diagnostics.ToImmutableArray();
        }

        protected SyntaxNode(SyntaxKind kind)
        {
            ChildNodes = new List<SyntaxNode>();
            Kind = kind;
            Diagnostics = ImmutableArray<Diagnostic>.Empty;
        }

        protected void RegisterChildNodes<T>(out List<T> nodes, List<T> values)
            where T : SyntaxNode
        {
            nodes = values;
            foreach (var childNode in values)
            {
                SourceRange = SourceRange.Union(SourceRange, childNode.SourceRange);
                FullSourceRange = SourceRange.Union(FullSourceRange, childNode.FullSourceRange);
                ChildNodes.Add(childNode);
                childNode.Parent = this;
            }
        }

        protected void RegisterChildNodes<T>(out SeparatedSyntaxList<T> nodes, SeparatedSyntaxList<T> values)
            where T : SyntaxNode
        {
            nodes = values;
            foreach (var childNode in values.GetWithSeparators())
            {
                SourceRange = SourceRange.Union(SourceRange, childNode.SourceRange);
                FullSourceRange = SourceRange.Union(FullSourceRange, childNode.FullSourceRange);
                ChildNodes.Add(childNode);
                childNode.Parent = this;
            }
        }

        protected void RegisterChildNode<T>(out T node, T value)
            where T : SyntaxNode
        {
            if (value == null)
            {
                node = null;
                return;
            }
            SourceRange = SourceRange.Union(SourceRange, value.SourceRange);
            FullSourceRange = SourceRange.Union(FullSourceRange, value.FullSourceRange);
            node = value;
            ChildNodes.Add(node);
            node.Parent = this;
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
                return child.FindToken(position, descendIntoTrivia);

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
                        stack = childNode.ApplyDirectives(stack);
            }
            return stack;
        }

        public virtual SyntaxNode SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            throw new NotImplementedException("SetDiagnostics not implemented for " + GetType().Name);
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
                    child.WriteTo(sb, leading | !first, trailing | (i < lastIndex), includeNonRootFile, ignoreMacroReferences);
                    first = false;
                }
            }
        }
    }
}