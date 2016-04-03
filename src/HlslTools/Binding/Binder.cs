using System;
using System.Collections.Generic;
using System.Threading;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal partial class Binder
    {
        private readonly SharedBinderState _sharedBinderState;
        private readonly List<Symbol> _symbols;

        protected Binder(SharedBinderState sharedBinderState, Binder parent)
        {
            _sharedBinderState = sharedBinderState;
            _symbols = new List<Symbol>();
            Parent = parent;
        }

        public Binder Parent { get; }

        internal List<Diagnostic> Diagnostics => _sharedBinderState.Diagnostics;

        public static BindingResult Bind(SyntaxNode syntaxRoot, CancellationToken cancellationToken)
        {
            var sharedBinderState = new SharedBinderState(cancellationToken);

            var intrinsicBinder = new IntrinsicBinder(sharedBinderState);
            var binder = new Binder(sharedBinderState, intrinsicBinder);

            var boundRoot = binder.Bind(syntaxRoot, binder.BindRoot);

            return new BindingResult(syntaxRoot, boundRoot, 
                sharedBinderState.BoundNodeFromSyntaxNode, 
                sharedBinderState.BinderFromBoundNode, 
                sharedBinderState.Diagnostics);
        }

        private BoundNode BindRoot(SyntaxNode syntax)
        {
            var functionImplementationChecker = new FunctionImplementationChecker(Diagnostics);

            var compilationUnit = syntax as CompilationUnitSyntax;
            if (compilationUnit != null)
            {
                var result = BindCompilationUnit(compilationUnit);
                functionImplementationChecker.VisitCompilationUnit(result);
                return result;
            }

            var expressionSyntax = syntax as ExpressionSyntax;
            if (expressionSyntax != null)
            {
                var result = BindExpression(expressionSyntax);
                functionImplementationChecker.VisitExpression(result);
                return result;
            }

            throw new InvalidOperationException();
        }

        private TResult Bind<TInput, TResult>(TInput node, Func<TInput, TResult> bindMethod)
            where TInput : SyntaxNode
            where TResult : BoundNode
        {
            _sharedBinderState.CancellationToken.ThrowIfCancellationRequested();

            var boundNode = bindMethod(node);

            Bind(node, boundNode);

            return boundNode;
        }

        private void Bind<TInput, TResult>(TInput node, TResult boundNode)
            where TInput : SyntaxNode
            where TResult : BoundNode
        {
            _sharedBinderState.BoundNodeFromSyntaxNode.Add(node, boundNode);
            if (!_sharedBinderState.BinderFromBoundNode.ContainsKey(boundNode))
                _sharedBinderState.BinderFromBoundNode.Add(boundNode, this);
        }

        private T GetBoundNode<T>(SyntaxNode node)
            where T : BoundNode
        {
            BoundNode result;
            _sharedBinderState.BoundNodeFromSyntaxNode.TryGetValue(node, out result);
            return result as T;
        }
    }
}