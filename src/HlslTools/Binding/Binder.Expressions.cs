using System;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding.BoundNodes;
using HlslTools.Binding.Signatures;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;
using HlslTools.Text;

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
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.LogicalNotExpression:
                case SyntaxKind.BitwiseNotExpression:
                    return BindPrefixUnaryExpression((PrefixUnaryExpressionSyntax) node);
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
                case SyntaxKind.LogicalOrExpression:
                case SyntaxKind.LogicalAndExpression:
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
                case SyntaxKind.CompoundExpression:
                    return BindCompoundExpression((CompoundExpressionSyntax) node);
                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedExpressionSyntax) node);
                case SyntaxKind.ConditionalExpression:
                    return BindConditionalExpression((ConditionalExpressionSyntax) node);
                case SyntaxKind.ElementAccessExpression:
                    return BindElementAccessExpression((ElementAccessExpressionSyntax) node);
                case SyntaxKind.ArrayInitializerExpression:
                    return BindArrayInitializerExpression((ArrayInitializerExpressionSyntax) node);
                default:
                    throw new ArgumentOutOfRangeException(node.Kind.ToString());
            }
        }

        private BoundExpression BindArrayInitializerExpression(ArrayInitializerExpressionSyntax syntax)
        {
            return new BoundArrayInitializerExpression(syntax.Elements.Select(x => Bind(x, BindExpression)).ToImmutableArray());
        }

        private BoundExpression BindElementAccessExpression(ElementAccessExpressionSyntax syntax)
        {
            var target = Bind(syntax.Expression, BindExpression);
            var index = Bind(syntax.Index, BindExpression);
            var indexTypes = new[] { index.Type }.ToImmutableArray();

            // To avoid cascading errors, we'll return a node that isn't bound to
            // any method if we couldn't resolve our target or any of our arguments.

            var anyErrors = target.Type.IsError() || index.Type.IsError();
            if (anyErrors)
                return new BoundElementAccessExpression(target, index, OverloadResolutionResult<IndexerSymbolSignature>.None);

            var result = LookupIndexer(target.Type, indexTypes);

            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    Diagnostics.ReportUndeclaredIndexer(syntax, target.Type, indexTypes);
                    return new BoundErrorExpression();
                }

                var symbol1 = result.Selected.Signature.Symbol;
                var symbol2 = result.Candidates.First(c => c.IsSuitable && c.Signature.Symbol != symbol1).Signature.Symbol;
                Diagnostics.ReportAmbiguousInvocation(syntax.GetTextSpanSafe(), symbol1, symbol2, indexTypes);
            }

            // Convert all arguments (if necessary)

            var convertedIndex = BindArgument(index, result, 0);

            return new BoundElementAccessExpression(target, convertedIndex, result);
        }

        private BoundExpression BindConditionalExpression(ConditionalExpressionSyntax node)
        {
            return new BoundConditionalExpression(
                Bind(node.Condition, BindExpression),
                Bind(node.WhenTrue, BindExpression),
                Bind(node.WhenFalse, BindExpression));
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            return BindExpression(syntax.Expression);
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

        private BoundExpression BindCastExpression(CastExpressionSyntax syntax)
        {
            return BindConversion(syntax.GetTextSpanSafe(), Bind(syntax.Expression, BindExpression), LookupType(syntax.Type));
        }

        private BoundExpression BindCompoundExpression(CompoundExpressionSyntax syntax)
        {
            return new BoundCompoundExpression(
                Bind(syntax.Left, BindExpression),
                Bind(syntax.Right, BindExpression));
        }

        private BoundExpression BindConversion(TextSpan diagnosticSpan, BoundExpression expression, TypeSymbol targetType)
        {
            var sourceType = expression.Type;
            var conversion = Conversion.Classify(sourceType, targetType);
            if (conversion.IsIdentity)
                return expression;

            // To avoid cascading errors, we'll only validate the result
            // if we could resolve both, the expression as well as the
            // target type.

            if (!sourceType.IsError() && !targetType.IsError())
            {
                if (!conversion.Exists)
                    Diagnostics.ReportCannotConvert(diagnosticSpan, sourceType, targetType);
            }

            return new BoundConversionExpression(expression, targetType, conversion);
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
            if (node.Name.IsMissing)
                return new BoundErrorExpression();

            var name = node.Name;
            var symbols = LookupVariable(name).ToImmutableArray();

            if (symbols.Length == 0)
            {
                var isInvocable = LookupSymbols<FunctionSymbol>(name).Any();
                if (isInvocable)
                    Diagnostics.ReportInvocationRequiresParenthesis(name);
                else
                    Diagnostics.ReportVariableNotDeclared(name);

                return new BoundErrorExpression();
            }

            if (symbols.Length > 1)
                Diagnostics.ReportAmbiguousName(name, symbols);

            var symbol = symbols.First();

            return new BoundVariableExpression(symbol);
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
            // TODO: Check that we have the correct number of arguments.
            return new BoundNumericConstructorInvocationExpression(
                LookupType(syntax.Type),
                BindArgumentList(syntax.ArgumentList));
        }

        private ImmutableArray<BoundExpression> BindArgumentList(ArgumentListSyntax syntax)
        {
            return syntax.Arguments.Select(x => Bind(x, BindExpression)).ToImmutableArray();
        }

        private BoundExpression BindMethodInvocationExpression(MethodInvocationExpressionSyntax syntax)
        {
            var target = Bind(syntax.Target, BindExpression);
            var name = syntax.Name;
            var arguments = BindArgumentList(syntax.ArgumentList);
            var argumentTypes = arguments.Select(a => a.Type).ToImmutableArray();

            // To avoid cascading errors, we'll return a node that isn't bound to
            // any method if we couldn't resolve our target or any of our arguments.

            var anyErrors = target.Type.IsError() || argumentTypes.Any(a => a.IsError());
            if (anyErrors)
                return new BoundMethodInvocationExpression(target, arguments, OverloadResolutionResult<MethodSymbolSignature>.None);

            var result = LookupMethod(target.Type, name, argumentTypes);

            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    Diagnostics.ReportUndeclaredMethod(syntax, target.Type, argumentTypes);
                    return new BoundErrorExpression();
                }

                var symbol1 = result.Selected.Signature.Symbol;
                var symbol2 = result.Candidates.First(c => c.IsSuitable && c.Signature.Symbol != symbol1).Signature.Symbol;
                Diagnostics.ReportAmbiguousInvocation(syntax.GetTextSpanSafe(), symbol1, symbol2, argumentTypes);
            }

            // Convert all arguments (if necessary)

            var convertedArguments = arguments.Select((a, i) => BindArgument(a, result, i)).ToImmutableArray();

            return new BoundMethodInvocationExpression(target, convertedArguments, result);
        }

        private BoundExpression BindFunctionInvocationExpression(FunctionInvocationExpressionSyntax syntax)
        {
            var name = syntax.Name;
            var boundArguments = BindArgumentList(syntax.ArgumentList);
            var argumentTypes = boundArguments.Select(a => a.Type).ToImmutableArray();

            var anyErrorsInArguments = argumentTypes.Any(a => a.IsError());
            if (anyErrorsInArguments)
                return new BoundFunctionInvocationExpression(boundArguments, OverloadResolutionResult<FunctionSymbolSignature>.None);

            var result = LookupFunction(name, argumentTypes);

            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    Diagnostics.ReportUndeclaredFunction(syntax, argumentTypes);
                    return new BoundErrorExpression();
                }

                var symbol1 = result.Selected.Signature.Symbol;
                var symbol2 = result.Candidates.First(c => c.IsSuitable && c.Signature.Symbol != symbol1).Signature.Symbol;
                Diagnostics.ReportAmbiguousInvocation(syntax.GetTextSpanSafe(), symbol1, symbol2, argumentTypes);
            }

            var convertedArguments = boundArguments.Select((a, i) => BindArgument(a, result, i)).ToImmutableArray();

            return new BoundFunctionInvocationExpression(convertedArguments, result);
        }

        private static BoundExpression BindArgument<T>(BoundExpression expression, OverloadResolutionResult<T> result, int argumentIndex)
            where T : Signature
        {
            var selected = result.Selected;
            if (selected == null)
                return expression;

            if (argumentIndex >= selected.Signature.ParameterCount && selected.Signature.HasVariadicParameter)
                return expression;

            var targetType = selected.Signature.GetParameterType(argumentIndex);
            var conversion = selected.ArgumentConversions[argumentIndex];

            // TODO: We need check for ambiguous conversions here as well.

            return conversion.IsIdentity
                       ? expression
                       : new BoundConversionExpression(expression, targetType, conversion);
        }

        private BoundExpression BindFieldAccessExpression(FieldAccessExpressionSyntax node)
        {
            var target = BindExpression(node.Expression);

            var name = node.Name;
            if (target.Type.IsError())
            {
                // To avoid cascading errors, we'll give up early.
                return new BoundErrorExpression();
            }

            var fieldSymbols = LookupField(target.Type, name).ToImmutableArray();

            if (fieldSymbols.Length == 0)
            {
                var hasMethods = LookupMethod(target.Type, name).Any();
                if (hasMethods)
                    Diagnostics.ReportInvocationRequiresParenthesis(name);
                else
                    Diagnostics.ReportUndeclaredField(node, target.Type);

                return new BoundErrorExpression();
            }

            if (fieldSymbols.Length > 1)
                Diagnostics.ReportAmbiguousField(name);

            var fieldSymbol = fieldSymbols[0];
            return new BoundFieldExpression(target, fieldSymbol);
        }

        private BoundInitializer BindInitializer(InitializerSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.EqualsValueClause:
                    return BindEqualsValue((EqualsValueClauseSyntax) syntax);
                case SyntaxKind.StateInitializer:
                    return BindStateInitializer((StateInitializerSyntax) syntax);
                case SyntaxKind.StateArrayInitializer:
                    return BindStateArrayInitializer((StateArrayInitializerSyntax) syntax);
                case SyntaxKind.SamplerStateInitializer:
                    return BindSamplerStateInitializer((SamplerStateInitializerSyntax) syntax);
                default:
                    throw new NotSupportedException(syntax.Kind.ToString());
            }
        }

        private BoundEqualsValue BindEqualsValue(EqualsValueClauseSyntax syntax)
        {
            return new BoundEqualsValue(Bind(syntax.Value, BindExpression));
        }

        private BoundStateInitializer BindStateInitializer(StateInitializerSyntax syntax)
        {
            return new BoundStateInitializer();
        }

        private BoundStateArrayInitializer BindStateArrayInitializer(StateArrayInitializerSyntax syntax)
        {
            return new BoundStateArrayInitializer();
        }

        private BoundSamplerStateInitializer BindSamplerStateInitializer(SamplerStateInitializerSyntax syntax)
        {
            return new BoundSamplerStateInitializer();
        }
    }
}