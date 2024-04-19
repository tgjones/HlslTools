using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;
using ShaderTools.CodeAnalysis.Hlsl.Binding.Signatures;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
{
    partial class Binder
    {
        private BoundExpression BindExpression(ExpressionSyntax node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            switch (node.Kind)
            {
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                case SyntaxKind.NumericLiteralExpression:
                case SyntaxKind.CharacterLiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax) node);
                case SyntaxKind.StringLiteralExpression:
                    return BindStringLiteralExpression((StringLiteralExpressionSyntax) node);
                case SyntaxKind.IdentifierName:
                    return BindIdentifierName((IdentifierNameSyntax) node);
                case SyntaxKind.QualifiedName:
                    return BindQualifiedName((QualifiedNameSyntax) node);
                case SyntaxKind.PreDecrementExpression:
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.LogicalNotExpression:
                case SyntaxKind.BitwiseNotExpression:
                    return BindPrefixUnaryExpression((PrefixUnaryExpressionSyntax) node);
                case SyntaxKind.PostDecrementExpression:
                case SyntaxKind.PostIncrementExpression:
                    return BindPostfixUnaryExpression((PostfixUnaryExpressionSyntax) node);
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
                case SyntaxKind.CompileExpression:
                    return BindCompileExpression((CompileExpressionSyntax) node);
                default:
                    throw new ArgumentOutOfRangeException(node.Kind.ToString());
            }
        }

        private BoundExpression BindCompileExpression(CompileExpressionSyntax syntax)
        {
            return new BoundCompileExpression();
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
                var symbol2 = result.Candidates.First(c => !Equals(c.Signature.Symbol, symbol1)).Signature.Symbol;
                Diagnostics.ReportAmbiguousInvocation(syntax.SourceRange, symbol1, symbol2, indexTypes);
            }

            // Convert all arguments (if necessary)

            var convertedIndex = BindArgument(index, result, 0, syntax.Index.SourceRange);

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

            if (boundLeft.Type.IsError() || boundRight.Type.IsError())
                return new BoundBinaryExpression(operatorKind, boundLeft, boundRight, OverloadResolutionResult<BinaryOperatorSignature>.None);

            var leftType = boundLeft.Type;
            var rightType = boundRight.Type;

            var requiresNumericType = operatorKind.RequiresNumericTypes();
            var requiresIntegralType = operatorKind.RequiresIntegralTypes();
            if (!ValidateTypeForBinaryExpression(leftType, requiresIntegralType, requiresNumericType)
                || !ValidateTypeForBinaryExpression(rightType, requiresIntegralType, requiresNumericType))
            {
                Diagnostics.ReportCannotApplyBinaryOperator(syntax.OperatorToken, leftType, rightType);
                return new BoundBinaryExpression(operatorKind, boundLeft, boundRight, OverloadResolutionResult<BinaryOperatorSignature>.None);
            }

            var result = LookupBinaryOperator(operatorKind, boundLeft, boundRight);
            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    Diagnostics.ReportCannotApplyBinaryOperator(syntax.OperatorToken, boundLeft.Type, boundRight.Type);
                }
                else
                {
                    Diagnostics.ReportAmbiguousBinaryOperator(syntax.OperatorToken, boundLeft.Type, boundRight.Type);
                }
            }

            // Convert arguments (if necessary)

            var convertedLeft = BindArgument(boundLeft, result, 0, syntax.Left.SourceRange);
            var convertedRight = BindArgument(boundRight, result, 1, syntax.Right.SourceRange);

            return new BoundBinaryExpression(operatorKind, convertedLeft, convertedRight, result);
        }

        private static bool ValidateTypeForBinaryExpression(TypeSymbol type,
            bool requiresIntegralType, bool requiresNumericType)
        {
            IntrinsicNumericTypeSymbol numericType = null;
            if (requiresIntegralType || requiresNumericType)
            {
                numericType = type as IntrinsicNumericTypeSymbol;
                if (numericType == null)
                    return false;
            }

            if (requiresIntegralType && !numericType.ScalarType.IsIntegral())
                return false;

            return true;
        }

        private BoundExpression BindCastExpression(CastExpressionSyntax syntax)
        {
            var boundTargetType = Bind(syntax.Type, x => BindType(x, null));
            var targetType = BindArrayRankSpecifiers(syntax.ArrayRankSpecifiers, boundTargetType.TypeSymbol);

            return BindConversion(syntax.SourceRange, Bind(syntax.Expression, BindExpression), targetType);
        }

        private BoundExpression BindCompoundExpression(CompoundExpressionSyntax syntax)
        {
            return new BoundCompoundExpression(
                Bind(syntax.Left, BindExpression),
                Bind(syntax.Right, BindExpression));
        }

        private BoundExpression BindConversion(SourceRange diagnosticSpan, BoundExpression expression, TypeSymbol targetType)
        {
            var sourceType = expression.Type;

            // TODO: Need to do Conversion.Classify to see if this conversion is possible.
            var conversion = sourceType.Equals(targetType) ? Conversion.Identity : Conversion.Explicit;

            if (conversion.IsIdentity)
                return expression;

            // To avoid cascading errors, we'll only validate the result
            // if we could resolve both, the expression as well as the
            // target type.

            if (!sourceType.IsError() && !targetType.IsError())
            {
                if (!sourceType.HasExplicitConversionTo(targetType))
                    Diagnostics.ReportCannotConvert(diagnosticSpan, sourceType, targetType);
            }

            return new BoundConversionExpression(expression, targetType, conversion);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax node)
        {
            // TODO: Need to apply similar overload resolution as BindBinaryExpression.
            var operatorKind = (node.Kind != SyntaxKind.SimpleAssignmentExpression)
                ? (BinaryOperatorKind?) SyntaxFacts.GetBinaryOperatorKind(node.Kind)
                : null;

            return new BoundAssignmentExpression(
                Bind(node.Left, BindExpression),
                operatorKind,
                Bind(node.Right, BindExpression));
        }

        private static BoundExpression BindLiteralExpression(LiteralExpressionSyntax node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                    return new BoundLiteralExpression(node, IntrinsicTypes.Bool);
                case SyntaxKind.NumericLiteralExpression:
                case SyntaxKind.CharacterLiteralExpression:
                    switch (node.Token.Kind)
                    {
                        case SyntaxKind.IntegerLiteralToken:
                        case SyntaxKind.CharacterLiteralToken:
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

        private BoundExpression BindQualifiedName(QualifiedNameSyntax node)
        {
            var container = LookupContainer(node.Left);

            if (container == null)
                return new BoundErrorExpression();

            var symbols = container.Members.OfType<VariableSymbol>()
                .Where(x => x.Name == node.Right.Name.Text)
                .ToImmutableArray();

            if (symbols.Length == 0)
            {
                Diagnostics.ReportVariableNotDeclared(node.Right.Name);
                return new BoundErrorExpression();
            }

            if (symbols.Length > 1)
                Diagnostics.ReportAmbiguousName(node.Right.Name, symbols);

            return Bind(node.Right, x => new BoundVariableExpression(symbols.First()));
        }

        private BoundExpression BindPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            var operatorKind = SyntaxFacts.GetUnaryOperatorKind(node.Kind);

            return BindUnaryExpression(node.OperatorToken, node.Operand, operatorKind);
        }

        private BoundExpression BindPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            var operatorKind = SyntaxFacts.GetUnaryOperatorKind(node.Kind);

            return BindUnaryExpression(node.OperatorToken, node.Operand, operatorKind);
        }

        private BoundExpression BindUnaryExpression(SyntaxToken operatorToken, ExpressionSyntax operand, UnaryOperatorKind operatorKind)
        {
            var expression = Bind(operand, BindExpression);

            // To avoid cascading errors, we'll return a unary expression that isn't bound to
            // an operator if the expression couldn't be resolved.

            if (expression.Type.IsError())
                return new BoundUnaryExpression(operatorKind, expression, OverloadResolutionResult<UnaryOperatorSignature>.None);

            var result = LookupUnaryOperator(operatorKind, expression);
            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    Diagnostics.ReportCannotApplyUnaryOperator(operatorToken, expression.Type);
                }
                else
                {
                    Diagnostics.ReportAmbiguousUnaryOperator(operatorToken, expression.Type);
                }
            }

            // Convert argument (if necessary)

            var convertedArgument = BindArgument(expression, result, 0, operand.SourceRange);

            return new BoundUnaryExpression(operatorKind, convertedArgument, result);
        }

        private BoundExpression BindNumericConstructorInvocationExpression(NumericConstructorInvocationExpressionSyntax syntax)
        {
            var boundArguments = BindArgumentList(syntax.ArgumentList);
            var argumentTypes = boundArguments.Select(a => a.Type).ToImmutableArray();

            var typeSymbol = BindType(syntax.Type, null).TypeSymbol;
            Debug.Assert(typeSymbol.IsError() == false);

            var anyErrorsInArguments = argumentTypes.Any(a => a.IsError());
            if (anyErrorsInArguments)
                return new BoundNumericConstructorInvocationExpression(syntax, typeSymbol, boundArguments, OverloadResolutionResult<FunctionSymbolSignature>.None);

            var result = LookupNumericConstructor(typeSymbol, argumentTypes);

            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    if (result.Candidates.IsEmpty)
                    {
                        Diagnostics.ReportUndeclaredNumericConstructor(syntax, argumentTypes);
                        return new BoundErrorExpression();
                    }
                    else
                    {
                        Diagnostics.ReportOverloadResolutionFailure(syntax, boundArguments.Length);
                    }
                }
                else
                {
                    var symbol1 = result.Selected.Signature.Symbol;
                    var symbol2 = result.Candidates.First(c => !c.Signature.Symbol.Equals(symbol1)).Signature.Symbol;
                    Diagnostics.ReportAmbiguousInvocation(syntax.SourceRange, symbol1, symbol2, argumentTypes);
                }
            }

            var convertedArguments = boundArguments.Select((a, i) => BindArgument(a, result, i, syntax.ArgumentList.Arguments[i].SourceRange)).ToImmutableArray();

            return new BoundNumericConstructorInvocationExpression(syntax, typeSymbol, convertedArguments, result);
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
                return new BoundMethodInvocationExpression(syntax, target, arguments, OverloadResolutionResult<FunctionSymbolSignature>.None);

            var result = LookupMethod(target.Type, name, argumentTypes);

            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    if (result.Candidates.IsEmpty)
                    {
                        Diagnostics.ReportUndeclaredMethod(syntax, target.Type, argumentTypes);
                        return new BoundErrorExpression();
                    }
                    else
                    {
                        Diagnostics.ReportOverloadResolutionFailure(syntax, arguments.Length);
                    }
                }
                else
                {
                    var symbol1 = result.Selected.Signature.Symbol;
                    var symbol2 = result.Candidates.First(c => !c.Signature.Symbol.Equals(symbol1)).Signature.Symbol;
                    Diagnostics.ReportAmbiguousInvocation(syntax.SourceRange, symbol1, symbol2, argumentTypes);
                }
            }

            // Convert all arguments (if necessary)

            var convertedArguments = arguments.Select((a, i) => BindArgument(a, result, i, syntax.ArgumentList.Arguments[i].SourceRange)).ToImmutableArray();

            return new BoundMethodInvocationExpression(syntax, target, convertedArguments, result);
        }

        private BoundExpression BindFunctionInvocationExpression(FunctionInvocationExpressionSyntax syntax)
        {
            // Don't try to bind CompileShader function calls, for now.
            if ((syntax.Name.Kind == SyntaxKind.IdentifierName) && ((IdentifierNameSyntax) syntax.Name).Name.ContextualKind == SyntaxKind.CompileShaderKeyword)
                return new BoundFunctionInvocationExpression(syntax,
                    syntax.ArgumentList.Arguments.Select(x => (BoundExpression) new BoundErrorExpression()).ToImmutableArray(),
                    OverloadResolutionResult<FunctionSymbolSignature>.None);

            var name = syntax.Name;
            var boundArguments = BindArgumentList(syntax.ArgumentList);
            var argumentTypes = boundArguments.Select(a => a.Type).ToImmutableArray();

            var anyErrorsInArguments = argumentTypes.Any(a => a.IsError());
            if (anyErrorsInArguments)
                return new BoundFunctionInvocationExpression(syntax, boundArguments, OverloadResolutionResult<FunctionSymbolSignature>.None);

            ContainerSymbol containerSymbol;
            SyntaxToken actualName;
            switch (name.Kind)
            {
                case SyntaxKind.IdentifierName:
                    containerSymbol = null;
                    actualName = ((IdentifierNameSyntax) name).Name;
                    break;
                case SyntaxKind.QualifiedName:
                    containerSymbol = LookupContainer(((QualifiedNameSyntax) syntax.Name).Left);
                    actualName = ((QualifiedNameSyntax) name).Right.Name;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            OverloadResolutionResult<FunctionSymbolSignature> result;
            if (containerSymbol != null && containerSymbol.Binder != null)
                result = containerSymbol.Binder.LookupFunction(containerSymbol, actualName, argumentTypes);
            else
                result = LookupFunction(actualName, argumentTypes);

            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    if (result.Candidates.IsEmpty)
                    {
                        Diagnostics.ReportUndeclaredFunction(syntax, argumentTypes);
                        return new BoundErrorExpression();
                    }
                    else
                    {
                        Diagnostics.ReportOverloadResolutionFailure(syntax, boundArguments.Length);
                    }
                }
                else
                {
                    var symbol1 = result.Selected.Signature.Symbol;
                    var symbol2 = result.Candidates.First(c => !c.Signature.Symbol.Equals(symbol1)).Signature.Symbol;
                    Diagnostics.ReportAmbiguousInvocation(syntax.SourceRange, symbol1, symbol2, argumentTypes);
                }
            }

            var convertedArguments = boundArguments.Select((a, i) => BindArgument(a, result, i, syntax.ArgumentList.Arguments[i].SourceRange)).ToImmutableArray();

            return new BoundFunctionInvocationExpression(syntax, convertedArguments, result);
        }

        private BoundExpression BindArgument<T>(BoundExpression expression, OverloadResolutionResult<T> result, int argumentIndex, SourceRange diagnosticSpan)
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

            if (conversion.ImplicitConversionType.IsImplicitNarrowing())
                Diagnostics.ReportImplicitTruncation(diagnosticSpan, expression.Type, targetType);

            return conversion.IsIdentity
                ? expression
                : new BoundConversionExpression(expression, targetType, conversion);
        }

        private BoundExpression BindFieldAccessExpression(FieldAccessExpressionSyntax node)
        {
            var target = Bind(node.Expression, BindExpression);

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