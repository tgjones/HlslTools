using System;
using System.Collections.Generic;
using System.Linq;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Compilation
{
    public sealed class SemanticModel
    {
        //private readonly BindingResult _bindingResult;
        //private ImmutableArray<Diagnostic> _diagnostics;
        private readonly SymbolScopeStack _symbolStack;
        private readonly Dictionary<SyntaxNode, Symbol> _syntaxToSymbolLookup;

        public Compilation Compilation { get; }

        internal SemanticModel(Compilation compilation)
        {
            Compilation = compilation;

            //_bindingResult = new BindingResult(compilation.SyntaxTree.Root);

            // TODO: Bind symbols.
            // TODO: Bind function amd method bodies.
            //       - Function and method bodies may contain symbols.

            _symbolStack = new SymbolScopeStack();

            _syntaxToSymbolLookup = new Dictionary<SyntaxNode, Symbol>();

            var compilationUnitSyntax = (CompilationUnitSyntax) compilation.SyntaxTree.Root;
            foreach (var node in compilationUnitSyntax.Declarations)
            {
                switch (node.Kind)
                {
                    case SyntaxKind.TypeDeclarationStatement:
                    {
                        var statement = (TypeDeclarationStatementSyntax) node;
                        switch (statement.Type.Kind)
                        {
                            case SyntaxKind.StructType:
                                AddSymbol(node, CreateStructSymbol((StructTypeSyntax) statement.Type));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    }
                    case SyntaxKind.VariableDeclarationStatement:
                    {
                        var statement = (VariableDeclarationStatementSyntax) node;
                        var valueType = statement.Declaration.Type.GetTypeSymbol(_symbolStack);
                        AddSymbolLookup(statement.Declaration.Type, valueType);
                        foreach (var declarator in statement.Declaration.Variables)
                            AddSymbol(declarator, new GlobalVariableSymbol(declarator, valueType));
                        break;
                    }
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public Symbol GetSymbol(SyntaxNode node)
        {
            Symbol result;
            _syntaxToSymbolLookup.TryGetValue(node, out result);
            return result;
        }

        private void AddSymbolLookup(SyntaxNode node, Symbol symbol)
        {
            if (symbol == null)
                return;
            _syntaxToSymbolLookup.Add(node, symbol);
        }

        private void AddSymbol(SyntaxNode node, Symbol symbol)
        {
            if (symbol == null)
                return;
            AddSymbolLookup(node, symbol);
            _symbolStack.AddSymbol(symbol);
        }

        private StructSymbol CreateStructSymbol(StructTypeSyntax node)
        {
            Func<TypeSymbol, IEnumerable<FieldSymbol>> fields = t =>
            {
                var result = new List<FieldSymbol>();
                foreach (var declaration in node.Fields)
                {
                    var valueType = declaration.Declaration.Type.GetTypeSymbol(_symbolStack);
                    foreach (var declarator in declaration.Declaration.Variables)
                        result.Add(new SourceFieldSymbol(declarator, t, valueType));
                }
                return result;
            };
            return new StructSymbol(node, null, fields);
        }

        

        //public IEnumerable<Diagnostic> GetDiagnostics()
        //{
        //    if (_diagnostics.IsDefault)
        //        _diagnostics = _bindingResult.GetDiagnostics().ToImmutableArray();
        //    return _diagnostics;
        //}

        public IEnumerable<Symbol> LookupSymbols(SourceLocation position)
        {
            // TODO
            return IntrinsicFunctions.AllFunctions
                .AsEnumerable<Symbol>()
                .Union(IntrinsicSemantics.AllSemantics);

            //var node = FindClosestNodeWithBinder(_bindingResult.Root, position);
            //var binder = node == null ? null : _bindingResult.GetBinder(node);
            //return binder == null
            //    ? Enumerable.Empty<Symbol>()
            //    : LookupSymbols(binder);
        }

        //private static IEnumerable<Symbol> LookupSymbols(Binder binder)
        //{
        //    // NOTE: We want to only show the *available* symbols. That means, we need to
        //    //       hide symbols from the parent binder that have same name as the ones
        //    //       from a nested binder.
        //    //
        //    //       We do this by simply recording which names we've already seen.
        //    //       Please note that we *do* want to see duplicate names within the
        //    //       *same* binder.

        //    var allNames = new HashSet<string>();

        //    while (binder != null)
        //    {
        //        var localNames = new HashSet<string>();
        //        var localSymbols = binder.LocalSymbols
        //            .Where(s => !string.IsNullOrEmpty(s.Name));

        //        foreach (var symbol in localSymbols)
        //        {
        //            if (!allNames.Contains(symbol.Name))
        //            {
        //                localNames.Add(symbol.Name);
        //                yield return symbol;
        //            }
        //        }

        //        allNames.UnionWith(localNames);
        //        binder = binder.Parent;
        //    }
        //}

        //private SyntaxNode FindClosestNodeWithBinder(SyntaxNode root, SourceLocation position)
        //{
        //    var token = root.FindTokenContext(position);
        //    return (from n in token.Parent.AncestorsAndSelf()
        //            let bc = _bindingResult.GetBinder(n)
        //            where bc != null
        //            select n).FirstOrDefault();
        //}

        //protected abstract Binder GetEnclosingBinder(TextLocation location);

        //private TextSpan CheckAndAdjustPosition(int position, out SyntaxToken token)
        //{
        //    token = Root.FindToken(position);
        //    return token.Span;
        //}

        //public Symbol GetDeclaredSymbol(FunctionDefinitionSyntax syntax)
        //{
        //    return _bindingResult.GetSymbol(syntax);
        //}

        //public Symbol GetDeclaredSymbol(VariableDeclaratorSyntax syntax)
        //{
        //    return _bindingResult.GetSymbol(syntax);
        //}

        //public Symbol GetSymbol(ExpressionSyntax expression)
        //{
        //    var boundExpression = GetBoundExpression(expression);
        //    return boundExpression == null ? null : GetSymbol(boundExpression);
        //}

        //private static Symbol GetSymbol(BoundExpression expression)
        //{
        //    switch (expression.Kind)
        //    {
        //        case BoundNodeKind.FunctionInvocationExpression:
        //            return GetSymbol((BoundFunctionInvocationExpression)expression);
        //        default:
        //            return null;
        //    }
        //}

        //private static Symbol GetSymbol(BoundFunctionInvocationExpression expression)
        //{
        //    return expression.Symbol;
        //}

        //private BoundExpression GetBoundExpression(ExpressionSyntax expression)
        //{
        //    return _bindingResult.GetBoundNode(expression) as BoundExpression;
        //}
    }
}