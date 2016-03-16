using System;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding.BoundNodes;
using HlslTools.Binding.Signatures;
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
                    return BindLiteralExpression((LiteralExpressionSyntax) node);
                case SyntaxKind.StringLiteralExpression:
                    return BindStringLiteralExpression((StringLiteralExpressionSyntax) node);
                case SyntaxKind.IdentifierName:
                    return BindIdentifierName((IdentifierNameSyntax) node);
                case SyntaxKind.PreDecrementExpression:
                case SyntaxKind.PreIncrementExpression:
                    return BindPrefixUnaryExpression((PrefixUnaryExpressionSyntax) node);
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.LogicalNotExpression:
                case SyntaxKind.BitwiseNotExpression:
                case SyntaxKind.PostDecrementExpression:
                case SyntaxKind.PostIncrementExpression:
                    return BindPostfixUnaryExpression((PostfixUnaryExpressionSyntax)node);
                case SyntaxKind.AddExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax) node);
                case SyntaxKind.FieldAccessExpression:
                    return BindFieldAccessExpression((FieldAccessExpressionSyntax) node);
                case SyntaxKind.FunctionInvocationExpression:
                    return BindFunctionInvocationExpression((FunctionInvocationExpressionSyntax) node);
                case SyntaxKind.MethodInvocationExpression:
                    return BindMethodInvocationExpression((MethodInvocationExpressionSyntax) node);
                case SyntaxKind.NumericConstructorInvocationExpression:
                    return BindNumericConstructorInvocationExpression((NumericConstructorInvocationExpressionSyntax) node);
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax) node);
                case SyntaxKind.CastExpression:
                    return BindCastExpression((CastExpressionSyntax) node);
                default:
                    throw new ArgumentOutOfRangeException(node.Kind.ToString());
            }
        }

        private BoundBinaryExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var operatorKind = SyntaxFacts.GetBinaryOperatorKind(syntax.Kind);
            var boundLeft = Bind(syntax.Left, BindExpression);
            var boundRight = Bind(syntax.Right, BindExpression);

            var expressionType = GetBinaryExpressionType(operatorKind, boundLeft, boundRight);

            return new BoundBinaryExpression(
                operatorKind,
                boundLeft, boundRight,
                expressionType);
        }

        private TypeSymbol GetBinaryExpressionType(BinaryOperatorKind operatorKind, BoundExpression boundLeft, BoundExpression boundRight)
        {
            switch (operatorKind)
            {
                case BinaryOperatorKind.Less:
                case BinaryOperatorKind.Greater:
                case BinaryOperatorKind.LessEqual:
                case BinaryOperatorKind.GreaterEqual:
                case BinaryOperatorKind.Equal:
                case BinaryOperatorKind.NotEqual:
                case BinaryOperatorKind.LogicalAnd:
                case BinaryOperatorKind.LogicalOr:
                    return IntrinsicTypes.Bool;
                case BinaryOperatorKind.Multiply:
                case BinaryOperatorKind.Divide:
                case BinaryOperatorKind.Modulo:
                case BinaryOperatorKind.Add:
                case BinaryOperatorKind.Subtract:
                case BinaryOperatorKind.LeftShift:
                case BinaryOperatorKind.RightShift:
                case BinaryOperatorKind.BitwiseAnd:
                case BinaryOperatorKind.BitwiseXor:
                case BinaryOperatorKind.BitwiseOr:
                    return boundLeft.Type; // TODO
                default:
                    throw new ArgumentOutOfRangeException(nameof(operatorKind), operatorKind, null);
            }
        }

        private BoundCastExpression BindCastExpression(CastExpressionSyntax syntax)
        {
            return new BoundCastExpression(LookupSymbol(syntax.Type), Bind(syntax.Expression, BindExpression));
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax node)
        {
            var operatorKind = (node.Kind != SyntaxKind.SimpleAssignmentExpression) ? (BinaryOperatorKind?) SyntaxFacts.GetBinaryOperatorKind(node.Kind) : null;

            return new BoundAssignmentExpression(BindExpression(node.Left), operatorKind, BindExpression(node.Right));
        }

        private static BoundExpression BindLiteralExpression(LiteralExpressionSyntax node)
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

        private static BoundExpression BindStringLiteralExpression(StringLiteralExpressionSyntax syntax)
        {
            return new BoundStringLiteralExpression(syntax.Tokens.Select(x => x.Text).ToImmutableArray());
        }

        private BoundExpression BindIdentifierName(IdentifierNameSyntax node)
        {
            var symbol = LookupSymbol(node.Name);

            if (symbol == null)
                return new BoundBadExpression(node);

            switch (symbol.Kind)
            {
                case SymbolKind.Variable:
                    return new BoundVariableExpression((VariableSymbol) symbol);
                case SymbolKind.Function:
                    return new BoundFunctionName((FunctionSymbol) symbol);
                case SymbolKind.Method:
                    return new BoundMethodName((MethodSymbol) symbol);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private BoundExpression BindPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var expression = BindExpression(node.Operand);
            var operatorKind = SyntaxFacts.GetUnaryOperatorKind(node.Kind);
            var expressionType = expression.Type;

            return new BoundUnaryExpression(expression, operatorKind, expressionType);
        }

        private BoundExpression BindPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
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

        private BoundExpression BindNumericConstructorInvocationExpression(NumericConstructorInvocationExpressionSyntax syntax)
        {
            return new BoundNumericConstructorInvocationExpression(
                LookupSymbol(syntax.Type),
                BindArgumentList(syntax.ArgumentList));
        }

        private ImmutableArray<BoundExpression> BindArgumentList(ArgumentListSyntax syntax)
        {
            return syntax.Arguments.Select(x => Bind(x, BindExpression)).ToImmutableArray();
        }

        private BoundMethodInvocationExpression BindMethodInvocationExpression(MethodInvocationExpressionSyntax syntax)
        {
            return new BoundMethodInvocationExpression(
                Bind(syntax.Target, BindExpression),
                BindArgumentList(syntax.ArgumentList),
                null); // TODO
        }

        private BoundFunctionInvocationExpression BindFunctionInvocationExpression(FunctionInvocationExpressionSyntax syntax)
        {
            //var boundArguments = BindArgumentList(syntax.ArgumentList);
            //var argumentTypes = boundArguments.Select(a => a.Type).ToImmutableArray();

            //var anyErrorsInArguments = argumentTypes.Any(a => a.IsError());
            //if (anyErrorsInArguments)
            //    return new BoundFunctionInvocationExpression(expression, boundArguments, OverloadResolutionResult<FunctionSymbolSignature>.None);

            //var result = LookupFunction(name, argumentTypes);

            //if (result.Best == null)
            //{
            //    if (result.Selected == null)
            //    {
            //        Diagnostics.ReportUndeclaredFunction(node, argumentTypes);
            //        return new BoundErrorExpression();
            //    }

            //    var symbol1 = result.Selected.Signature.Symbol;
            //    var symbol2 = result.Candidates.First(c => c.IsSuitable && c.Signature.Symbol != symbol1).Signature.Symbol;
            //    Diagnostics.ReportAmbiguousInvocation(node.Span, symbol1, symbol2, argumentTypes);
            //}

            //var convertedArguments = boundArguments.Select((a, i) => BindArgument(a, result, i)).ToImmutableArray();

            //return new BoundFunctionInvocationExpression(
            //    expression,
            //    convertedArguments,
            //    result);

            throw new NotImplementedException();
        }

        private BoundExpression BindFieldAccessExpression(FieldAccessExpressionSyntax node)
        {
            var objectReference = BindExpression(node.Expression);

            var member = objectReference.Type.GetMember(node.Name.Text);

            return new BoundFieldExpression(objectReference, member);
        }

        private BoundInitializer BindInitializer(InitializerSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.EqualsValueClause:
                    return BindEqualsValue((EqualsValueClauseSyntax) syntax);

                default:
                    throw new NotSupportedException(syntax.Kind.ToString());
            }
        }

        private BoundEqualsValue BindEqualsValue(EqualsValueClauseSyntax syntax)
        {
            return new BoundEqualsValue(Bind(syntax.Value, BindExpression));
        }
    }
}