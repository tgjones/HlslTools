using System;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Binding
{
    internal static class ExpressionTestUtility
    {
        public static string GetValue(string type)
        {
            return $"(({type}) 1)";
        }

        public static string GetExpressionTypeString(TypeSymbol type)
        {
            return type.ToMarkup().ToString();
        }

        public static string GetErrorString(DiagnosticId diagnosticId)
        {
            switch (diagnosticId)
            {
                case DiagnosticId.CannotApplyUnaryOperator:
                case DiagnosticId.CannotApplyBinaryOperator:
                    return "#inapplicable";
                case DiagnosticId.AmbiguousInvocation:
                case DiagnosticId.AmbiguousUnaryOperator:
                case DiagnosticId.AmbiguousBinaryOperator:
                    return "#ambiguous";
                case DiagnosticId.UndeclaredFunction:
                    return "#undeclared";
                case DiagnosticId.CannotConvert:
                    return "#cannotconvert";
                case DiagnosticId.FunctionOverloadResolutionFailure:
                case DiagnosticId.MethodOverloadResolutionFailure:
                    return "#novalidoverload";
                default:
                    throw new ArgumentOutOfRangeException(nameof(diagnosticId));
            }
        }
    }
}