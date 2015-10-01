using System.Collections.Generic;
using System.Collections.Immutable;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal sealed class BindingResult
    {
        private readonly IDictionary<SyntaxNode, Symbol> _symbolFromSyntaxNode;
        private readonly IDictionary<SyntaxNode, BoundNode> _boundNodeFromSyntaxNode;
        private readonly List<Diagnostic> _diagnostics;

        public CompilationUnitSyntax Root { get; }

        public BindingResult(CompilationUnitSyntax root)
        {
            Root = root;

            _symbolFromSyntaxNode = new Dictionary<SyntaxNode, Symbol>();
            _boundNodeFromSyntaxNode = new Dictionary<SyntaxNode, BoundNode>();
            _diagnostics = new List<Diagnostic>();
        }

        public void AddSymbol(SyntaxNode syntaxNode, Symbol symbol)
        {
            _symbolFromSyntaxNode[syntaxNode] = symbol;
        }

        public Symbol GetSymbol(SyntaxNode syntaxNode)
        {
            Symbol result;
            _symbolFromSyntaxNode.TryGetValue(syntaxNode, out result);
            return result;
        }

        public void AddBoundNode(SyntaxNode syntaxNode, BoundNode boundNode)
        {
            _boundNodeFromSyntaxNode.Add(syntaxNode, boundNode);
        }

        public BoundNode GetBoundNode(SyntaxNode syntaxNode)
        {
            BoundNode result;
            _boundNodeFromSyntaxNode.TryGetValue(syntaxNode, out result);
            return result;
        }

        public void AddDiagnostic(Diagnostic diagnostic)
        {
            _diagnostics.Add(diagnostic);
        }

        public ImmutableArray<Diagnostic> GetDiagnostics()
        {
            return _diagnostics.ToImmutableArray();
        }
    }
}