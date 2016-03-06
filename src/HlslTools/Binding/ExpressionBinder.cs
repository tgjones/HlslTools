using System;
using System.Collections.Generic;
using System.Diagnostics;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal sealed class ExpressionBinder
    {
        private readonly ILocalSymbolTable _symbolTable;
        private readonly Symbol _symbolContext;
        //private readonly SymbolSet _symbolSet;

        public ExpressionBinder(ILocalSymbolTable symbolTable, MemberSymbol memberContext, List<Diagnostic> diagnostics)
        {
            _symbolTable = symbolTable;
            _symbolContext = memberContext;
            //_symbolSet = memberContext.SymbolSet;
        }

        public ExpressionBinder(ILocalSymbolTable symbolTable, FieldSymbol fieldContext, List<Diagnostic> diagnostics)
        {
            _symbolTable = symbolTable;
            _symbolContext = fieldContext;
            //_symbolSet = fieldContext.SymbolSet;
        }

        public BoundExpression BindExpression(ExpressionSyntax node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                case SyntaxKind.NumericLiteralExpression:
                case SyntaxKind.StringLiteralExpression:
                    return ProcessLiteral((LiteralExpressionSyntax) node);
                case SyntaxKind.IdentifierName:
                    return ProcessIdentifierName((IdentifierNameSyntax) node);
                case SyntaxKind.PreDecrementExpression:
                case SyntaxKind.PreIncrementExpression:
                    return ProcessPrefixUnary((PrefixUnaryExpressionSyntax) node);
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.LogicalNotExpression:
                case SyntaxKind.BitwiseNotExpression:
                case SyntaxKind.PostDecrementExpression:
                case SyntaxKind.PostIncrementExpression:
                    return ProcessPostfixUnary((PostfixUnaryExpressionSyntax)node);
                case SyntaxKind.MemberAccessExpression:
                    return ProcessMemberAccess((MemberAccessExpressionSyntax) node);
                //case SyntaxKind.FunctionInvocationExpression:
                //    return ProcessFunctionInvocation((InvocationExpressionSyntax) node);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static BoundExpression ProcessLiteral(LiteralExpressionSyntax node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                    return new BoundLiteralExpression(node, IntrinsicTypes.Bool);
                case SyntaxKind.NumericLiteralExpression:
                    switch (node.Token.Kind)
                    {
                        case SyntaxKind.IntegerLiteralToken:
                            return new BoundLiteralExpression(node, IntrinsicTypes.Int);
                        case SyntaxKind.FloatLiteralToken:
                            return new BoundLiteralExpression(node, IntrinsicTypes.Float);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private BoundExpression ProcessIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = _symbolTable.FindSymbol(node.Name.Text, _symbolContext);

            var localSymbol = symbol as LocalSymbol;
            if (localSymbol != null)
                return new BoundLocalExpression(node, localSymbol);

            var globalSymbol = symbol as GlobalSymbol;
            if (globalSymbol != null)
                return new BoundGlobalExpression(node, globalSymbol);

            // TODO: Static method calls.

            Debug.Fail("Shouldn't be here.");
            return null;
        }

        private BoundExpression ProcessPrefixUnary(PrefixUnaryExpressionSyntax node)
        {
            var expression = BindExpression(node.Operand);
            var operatorKind = SyntaxFacts.GetUnaryOperatorKind(node.Kind);
            var expressionType = expression.Type;

            return new BoundUnaryExpression(node, expression, operatorKind, expressionType);
        }

        private BoundExpression ProcessPostfixUnary(PostfixUnaryExpressionSyntax node)
        {
            var expression = BindExpression(node.Operand);
            var operatorKind = SyntaxFacts.GetUnaryOperatorKind(node.Kind);

            TypeSymbol expressionType;
            switch (operatorKind)
            {
                case UnaryOperatorKind.LogicalNot:
                    expressionType = IntrinsicTypes.Bool;
                    break;
                case UnaryOperatorKind.BitwiseNot:
                    expressionType = IntrinsicTypes.Uint;
                    break;
                case UnaryOperatorKind.Plus:
                case UnaryOperatorKind.Minus:
                case UnaryOperatorKind.PostIncrement:
                case UnaryOperatorKind.PostDecrement:
                    expressionType = expression.Type;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new BoundUnaryExpression(node, expression, operatorKind, expressionType);
        }

        private BoundExpression ProcessMemberAccess(MemberAccessExpressionSyntax node)
        {
            var objectReference = BindExpression(node.Expression);

            var typeSymbolTable = (ISymbolTable) objectReference.Type;
            var member = (MemberSymbol) typeSymbolTable.FindSymbol(node.Name.Name.Text, _symbolContext);

            return new BoundMemberExpression(node, objectReference, member);
        }
    }
}