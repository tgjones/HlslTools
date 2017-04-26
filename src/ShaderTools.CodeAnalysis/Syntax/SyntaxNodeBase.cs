using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Syntax
{
    public abstract class SyntaxNodeBase
    {
        public ushort RawKind { get; }

        public SourceRange SourceRange { get; protected set; }
        public SourceRange FullSourceRange { get; protected set; }

        public string DocumentationSummary;

        public SyntaxNodeBase Parent { get; internal set; }

        public IList<SyntaxNodeBase> ChildNodes { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public virtual bool IsToken => false;

        public virtual bool IsMissing
        {
            get { return ChildNodes.All(n => n.IsMissing); }
        }

        public abstract bool ContainsDirectives { get; }

        public virtual bool ContainsDiagnostics
        {
            get { return Diagnostics.Any() || ChildNodes.Any(t => t.ContainsDiagnostics); }
        }

        protected SyntaxNodeBase(ushort rawKind, IEnumerable<Diagnostic> diagnostics)
        {
            ChildNodes = new List<SyntaxNodeBase>();
            RawKind = rawKind;
            Diagnostics = diagnostics.ToImmutableArray();
        }

        protected SyntaxNodeBase(ushort rawKind)
        {
            ChildNodes = new List<SyntaxNodeBase>();
            RawKind = rawKind;
            Diagnostics = ImmutableArray<Diagnostic>.Empty;
        }

        public virtual IEnumerable<Diagnostic> GetDiagnostics()
        {
            return Diagnostics.Union(ChildNodes.SelectMany(x => x.GetDiagnostics()));
        }

        protected void RegisterChildNodes<T>(out List<T> nodes, List<T> values)
            where T : SyntaxNodeBase
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
            where T : SyntaxNodeBase
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
            where T : SyntaxNodeBase
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

        // TODO: Make this abstract once we're code-generating syntax nodes.
        public virtual SyntaxNodeBase SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            throw new NotImplementedException("SetDiagnostics not implemented for " + GetType().Name);
        }
    }
}
