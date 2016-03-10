using System.Collections.Generic;
using System.Collections.Immutable;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal sealed class BindingResult
    {
        private readonly IDictionary<SyntaxNode, BoundNode> _boundNodeFromSyntaxNode;
        private readonly IDictionary<BoundNode, Binder> _binderFromBoundNode;

        public CompilationUnitSyntax Root { get; }

        public BoundNode BoundRoot { get; }

        public Binder RootBinder => _binderFromBoundNode[BoundRoot];

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public BindingResult(CompilationUnitSyntax root, BoundCompilationUnit boundRoot, IDictionary<SyntaxNode, BoundNode> boundNodeFromSyntaxNode, IDictionary<BoundNode, Binder> binderFromBoundNode, IList<Diagnostic> diagnostics)
        {
            Root = root;
            BoundRoot = boundRoot;

            _boundNodeFromSyntaxNode = boundNodeFromSyntaxNode;
            _binderFromBoundNode = binderFromBoundNode;

            Diagnostics = diagnostics.ToImmutableArray();
        }

        public BoundNode GetBoundNode(SyntaxNode syntaxNode)
        {
            BoundNode result;
            _boundNodeFromSyntaxNode.TryGetValue(syntaxNode, out result);
            return result;
        }

        public Binder GetBinder(SyntaxNode syntaxNode)
        {
            var boundNode = GetBoundNode(syntaxNode);
            return boundNode == null ? null : GetBinder(boundNode);
        }

        public Binder GetBinder(BoundNode boundNode)
        {
            Binder result;
            _binderFromBoundNode.TryGetValue(boundNode, out result);
            return result;
        }
    }
}