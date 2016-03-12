using System;
using System.Diagnostics;
using HlslTools.Binding.BoundNodes;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    partial class Binder
    {
        private BoundExpression BindExpression(ExpressionSyntax node)
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
            var symbol = LookupSymbol(node.Name);

            //var globalSymbol = symbol as GlobalSymbol;
            //if (globalSymbol != null)
            //    return new BoundExpression(node, globalSymbol);

            // TODO: Static method calls.

            Debug.Fail("Shouldn't be here.");
            return null;
        }

        private BoundExpression ProcessPrefixUnary(PrefixUnaryExpressionSyntax node)
        {
            var expression = BindExpression(node.Operand);
            var operatorKind = SyntaxFacts.GetUnaryOperatorKind(node.Kind);
            var expressionType = expression.Type;

            return new BoundUnaryExpression(expression, operatorKind, expressionType);
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

            return new BoundUnaryExpression(expression, operatorKind, expressionType);
        }

        private BoundExpression ProcessMemberAccess(MemberAccessExpressionSyntax node)
        {
            var objectReference = BindExpression(node.Expression);

            var member = objectReference.Type.GetMember(node.Name.Name.Text);

            return new BoundMemberExpression(objectReference, member);
        }
    }
}