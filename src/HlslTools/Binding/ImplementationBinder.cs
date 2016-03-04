//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Diagnostics;
//using HlslTools.Binding.BoundNodes;
//using HlslTools.Diagnostics;
//using HlslTools.Symbols;

//namespace HlslTools.Binding
//{
//    internal sealed class ImplementationBinder : ILocalSymbolTable
//    {
//        private readonly List<Diagnostic> _diagnostics;

//        private SymbolScope _rootScope;
//        private SymbolScope _currentScope;

//        private int _generatedSymbolCount;

//        public ImplementationBinder()
//        {
//            _diagnostics = new List<Diagnostic>();
//        }

//        public BoundExpression BuildField(SourceFieldSymbol fieldSymbol)
//        {
//            _rootScope = new SymbolScope((ISymbolTable)fieldSymbol.Parent);
//            _currentScope = _rootScope;

//            BoundExpression initializerExpression = null;

//            var fieldDeclarationNode = fieldSymbol.Syntax;
//            Debug.Assert(fieldDeclarationNode != null);

//            //var initializerNode = fieldDeclarationNode.Initializer;
//            //if (initializerNode.Value != null)
//            //{
//            //    var expressionBuilder = new ExpressionBinder(this, fieldSymbol, _diagnostics);
//            //    initializerExpression = expressionBuilder.BindExpression(initializerNode.Value);
//            //    if (initializerExpression is BoundMemberExpression)
//            //        initializerExpression = expressionBuilder.TransformMemberExpression((MemberExpression) initializerExpression);
//            //}

//            throw new NotImplementedException();

//            // TODO: Cache result.

//            return initializerExpression;
//        }

//        public SymbolImplementation BuildMethod(SourceMethodDefinitionSymbol methodSymbol)
//        {
//            var methodBody = methodSymbol.Syntax.Body;
//            var symbolTable = (ISymbolTable) methodSymbol.Parent;

//            _rootScope = new SymbolScope(symbolTable);
//            _currentScope = _rootScope;

//            var symbolContext = methodSymbol;
//            var implementationNode = methodBody;

//            var statements = new List<BoundStatement>();
//            var statementBuilder = new StatementBinder(this, symbolContext, _diagnostics);

//            if (symbolContext.Parameters != null)
//            {
//                int parameterCount = symbolContext.Parameters.Length;
//                for (int paramIndex = 0; paramIndex < parameterCount; paramIndex++)
//                    _currentScope.AddSymbol(symbolContext.Parameters[paramIndex]);
//            }

//            foreach (var statementNode in implementationNode.Statements)
//            {
//                var statement = statementBuilder.BuildStatement(statementNode);
//                if (statement != null)
//                    statements.Add(statement);
//            }

//            return new SymbolImplementation(statements.ToImmutableArray(), _rootScope);
//        }

//        #region ISymbolTable Members

//        ICollection ISymbolTable.Symbols
//        {
//            get
//            {
//                Debug.Assert(_currentScope != null);
//                return ((ISymbolTable)_currentScope).Symbols;
//            }
//        }

//        Symbol ISymbolTable.FindSymbol(string name, Symbol context)
//        {
//            Debug.Assert(_currentScope != null);
//            return ((ISymbolTable)_currentScope).FindSymbol(name, context);
//        }

//        #endregion

//        #region ILocalSymbolTable Members

//        void ILocalSymbolTable.AddSymbol(LocalSymbol symbol)
//        {
//            Debug.Assert(_currentScope != null);
//            _currentScope.AddSymbol(symbol);
//        }

//        void ILocalSymbolTable.PopScope()
//        {
//            Debug.Assert(_currentScope != null);
//            _currentScope = _currentScope.Parent;
//        }

//        void ILocalSymbolTable.PushScope()
//        {
//            Debug.Assert(_currentScope != null);

//            var parentScope = _currentScope;
//            _currentScope = new SymbolScope(parentScope);
//            parentScope.AddChildScope(_currentScope);
//        }

//        #endregion
//    }
//}