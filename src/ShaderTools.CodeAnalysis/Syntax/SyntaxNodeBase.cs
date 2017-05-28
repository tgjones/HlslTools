using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.PooledObjects;

namespace ShaderTools.CodeAnalysis.Syntax
{
    public abstract class SyntaxNodeBase
    {
        private SyntaxTreeBase _syntaxTree;

        public ushort RawKind { get; }

        public SourceRange SourceRange { get; protected set; }
        public SourceRange FullSourceRange { get; protected set; }

        public string DocumentationSummary;

        /// <summary>
        /// Returns SyntaxTree that owns the node or null if node does not belong to a
        /// SyntaxTree
        /// </summary>
        public SyntaxTreeBase SyntaxTree
        {
            get
            {
                var result = this._syntaxTree ?? ComputeSyntaxTree(this);
                Debug.Assert(result != null);
                return result;
            }
        }

        internal void SetSyntaxTree(SyntaxTreeBase syntaxTree)
        {
            _syntaxTree = syntaxTree;
        }

        private static SyntaxTreeBase ComputeSyntaxTree(SyntaxNodeBase node)
        {
            ArrayBuilder<SyntaxNodeBase> nodes = null;
            SyntaxTreeBase tree = null;

            // Find the nearest parent with a non-null syntax tree
            while (true)
            {
                tree = node._syntaxTree;
                if (tree != null)
                {
                    break;
                }

                var parent = node.Parent;
                if (parent == null)
                {
                    // root node: unexpected, root node should have a tree.
                    //Interlocked.CompareExchange(ref node._syntaxTree, CSharpSyntaxTree.CreateWithoutClone(node), null);
                    //tree = node._syntaxTree;
                    break;
                }

                tree = parent._syntaxTree;
                if (tree != null)
                {
                    node._syntaxTree = tree;
                    break;
                }

                (nodes ?? (nodes = ArrayBuilder<SyntaxNodeBase>.GetInstance())).Add(node);
                node = parent;
            }

            // Propagate the syntax tree downwards if necessary
            if (nodes != null)
            {
                Debug.Assert(tree != null);

                foreach (var n in nodes)
                {
                    var existingTree = n._syntaxTree;
                    if (existingTree != null)
                    {
                        Debug.Assert(existingTree == tree, "how could this node belong to a different tree?");

                        // yield the race
                        break;
                    }
                    n._syntaxTree = tree;
                }

                nodes.Free();
            }

            return tree;
        }

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

        public abstract string Language { get; }

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

        public abstract string ToFullString();

        public IEnumerable<SyntaxNodeBase> Ancestors()
        {
            return AncestorsAndSelf().Skip(1);
        }

        public IEnumerable<SyntaxNodeBase> AncestorsAndSelf()
        {
            var node = this;
            while (node != null)
            {
                yield return node;
                node = node.Parent;
            }
        }

        internal virtual bool IsSkippedTokensTrivia => false;
        internal virtual bool IsStructuredTrivia => false;

        #region Token Lookup

        /// <summary>
        /// Finds a descendant token of this node whose span includes the supplied position. 
        /// </summary>
        /// <param name="position">The character position of the token relative to the beginning of the file.</param>
        /// <param name="findInsideTrivia">
        /// True to return tokens that are part of trivia. If false finds the token whose full span (including trivia)
        /// includes the position.
        /// </param>
        public abstract ISyntaxToken FindToken(SourceLocation position, bool findInsideTrivia = false);

        #endregion
    }
}
