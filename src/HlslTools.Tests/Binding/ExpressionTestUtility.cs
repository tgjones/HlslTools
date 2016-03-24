using System;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Symbols.Markup;

namespace HlslTools.Tests.Binding
{
    internal static class ExpressionTestUtility
    {
        public static string GetValue(string type)
        {
            return $"(({type}) 1)";
        }

        public static string GetExpressionTypeString(TypeSymbol type)
        {
            return SymbolMarkup.ForSymbol(type).ToString();
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(diagnosticId));
            }
        }
    }
}