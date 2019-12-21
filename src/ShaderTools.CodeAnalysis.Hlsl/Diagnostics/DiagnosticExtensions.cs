using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Diagnostics
{
    internal static class DiagnosticExtensions
    {
        public static void Report(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, DiagnosticId diagnosticId, params object[] args)
        {
            var diagnostic = Diagnostic.Create(HlslMessageProvider.Instance, sourceRange, (int) diagnosticId, args);
            diagnostics.Add(diagnostic);
        }

        #region Lexer errors

        public static void ReportIllegalInputCharacter(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, char character)
        {
            diagnostics.Report(sourceRange, DiagnosticId.IllegalInputCharacter, character);
        }

        public static void ReportUnterminatedComment(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange)
        {
            diagnostics.Report(sourceRange, DiagnosticId.UnterminatedComment);
        }

        public static void ReportUnterminatedString(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange)
        {
            diagnostics.Report(sourceRange, DiagnosticId.UnterminatedString);
        }

        public static void ReportInvalidCharacterLiteral(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange)
        {
            diagnostics.Report(sourceRange, DiagnosticId.InvalidCharacterLiteral);
        }

        public static void ReportInvalidInteger(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, string tokenText)
        {
            diagnostics.Report(sourceRange, DiagnosticId.InvalidInteger, tokenText);
        }

        public static void ReportInvalidReal(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, string tokenText)
        {
            diagnostics.Report(sourceRange, DiagnosticId.InvalidReal, tokenText);
        }

        public static void ReportInvalidOctal(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, string tokenText)
        {
            diagnostics.Report(sourceRange, DiagnosticId.InvalidOctal, tokenText);
        }

        public static void ReportInvalidHex(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, string tokenText)
        {
            diagnostics.Report(sourceRange, DiagnosticId.InvalidHex, tokenText);
        }

        public static void ReportNumberTooLarge(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, string tokenText)
        {
            diagnostics.Report(sourceRange, DiagnosticId.NumberTooLarge, tokenText);
        }

        #endregion

        #region Parser errors

        public static void ReportTokenExpected(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, SyntaxToken actual, SyntaxKind expected)
        {
            var actualText = actual.GetDisplayText();
            var expectedText = expected.GetDisplayText();
            diagnostics.Report(sourceRange, DiagnosticId.TokenExpected, actualText, expectedText);
        }

        public static void ReportTokenUnexpected(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, SyntaxToken actual)
        {
            var actualText = actual.GetDisplayText();
            diagnostics.Report(sourceRange, DiagnosticId.TokenUnexpected, actualText);
        }

        public static void ReportNoVoidHere(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange)
        {
            diagnostics.Report(sourceRange, DiagnosticId.NoVoidHere);
        }

        public static void ReportNoVoidParameter(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange)
        {
            diagnostics.Report(sourceRange, DiagnosticId.NoVoidParameter);
        }

        #endregion

        #region Semantic errors

        public static void ReportUndeclaredType(this ICollection<Diagnostic> diagnostics, SyntaxNode type)
        {
            diagnostics.Report(type.SourceRange, DiagnosticId.UndeclaredType, type.ToStringIgnoringMacroReferences());
        }

        public static void ReportUndeclaredFunction(this ICollection<Diagnostic> diagnostics, FunctionInvocationExpressionSyntax node, IEnumerable<TypeSymbol> argumentTypes)
        {
            var name = node.Name.ToStringIgnoringMacroReferences();
            var argumentTypeList = string.Join(@", ", argumentTypes.Select(t => t.ToDisplayName()));
            diagnostics.Report(node.SourceRange, DiagnosticId.UndeclaredFunction, name, argumentTypeList);
        }

        public static void ReportUndeclaredNumericConstructor(this ICollection<Diagnostic> diagnostics, NumericConstructorInvocationExpressionSyntax node, IEnumerable<TypeSymbol> argumentTypes)
        {
            var name = node.Type.ToStringIgnoringMacroReferences();
            var argumentTypeList = string.Join(@", ", argumentTypes.Select(t => t.ToDisplayName()));
            diagnostics.Report(node.SourceRange, DiagnosticId.UndeclaredFunction, name, argumentTypeList);
        }

        public static void ReportUndeclaredMethod(this ICollection<Diagnostic> diagnostics, MethodInvocationExpressionSyntax node, TypeSymbol declaringType, IEnumerable<TypeSymbol> argumentTypes)
        {
            var name = node.Name.ValueText;
            var declaringTypeName = declaringType.ToDisplayName();
            var argumentTypeNames = string.Join(@", ", argumentTypes.Select(t => t.ToDisplayName()));
            diagnostics.Report(node.SourceRange, DiagnosticId.UndeclaredMethod, declaringTypeName, name, argumentTypeNames);
        }

        public static void ReportUndeclaredFunctionInNamespaceOrClass(this ICollection<Diagnostic> diagnostics, QualifiedDeclarationNameSyntax name)
        {
            var declaringTypeName = name.Left.ToStringIgnoringMacroReferences();
            diagnostics.Report(name.SourceRange, DiagnosticId.UndeclaredFunctionInNamespaceOrClass, declaringTypeName, name.GetUnqualifiedName().Name.Text);
        }

        public static void ReportUndeclaredIndexer(this ICollection<Diagnostic> diagnostics, ElementAccessExpressionSyntax node, TypeSymbol declaringType, IEnumerable<TypeSymbol> argumentTypes)
        {
            var declaringTypeName = declaringType.ToDisplayName();
            var argumentTypeNames = string.Join(@", ", argumentTypes.Select(t => t.ToDisplayName()));
            diagnostics.Report(node.SourceRange, DiagnosticId.UndeclaredIndexer, declaringTypeName, argumentTypeNames);
        }

        public static void ReportVariableNotDeclared(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            diagnostics.Report(name.SourceRange, DiagnosticId.UndeclaredVariable, name.ValueText);
        }

        public static void ReportUndeclaredField(this ICollection<Diagnostic> diagnostics, FieldAccessExpressionSyntax node, TypeSymbol type)
        {
            var typeName = type.ToDisplayName();
            var propertyName = node.Name.ValueText;
            diagnostics.Report(node.SourceRange, DiagnosticId.UndeclaredField, typeName, propertyName);
        }

        public static void ReportUndeclaredNamespaceOrType(this ICollection<Diagnostic> diagnostics, QualifiedDeclarationNameSyntax node)
        {
            var typeName = node.Left.ToStringIgnoringMacroReferences();
            diagnostics.Report(node.SourceRange, DiagnosticId.UndeclaredNamespaceOrType, typeName);
        }

        public static void ReportAmbiguousInvocation(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, InvocableSymbol symbol1, InvocableSymbol symbol2, IReadOnlyList<TypeSymbol> argumentTypes)
        {
            diagnostics.Report(sourceRange, DiagnosticId.AmbiguousInvocation, symbol1, symbol2);
        }

        public static void ReportOverloadResolutionFailure(this ICollection<Diagnostic> diagnostics, FunctionInvocationExpressionSyntax node, int argumentCount)
        {
            var name = node.Name.ToStringIgnoringMacroReferences();
            diagnostics.Report(node.SourceRange, DiagnosticId.FunctionOverloadResolutionFailure, name, argumentCount);
        }

        public static void ReportOverloadResolutionFailure(this ICollection<Diagnostic> diagnostics, MethodInvocationExpressionSyntax node, int argumentCount)
        {
            var name = node.Name.ToStringIgnoringMacroReferences();
            diagnostics.Report(node.SourceRange, DiagnosticId.MethodOverloadResolutionFailure, name, argumentCount);
        }

        public static void ReportOverloadResolutionFailure(this ICollection<Diagnostic> diagnostics, NumericConstructorInvocationExpressionSyntax node, int argumentCount)
        {
            var name = node.Type.ToStringIgnoringMacroReferences();
            diagnostics.Report(node.SourceRange, DiagnosticId.FunctionOverloadResolutionFailure, name, argumentCount);
        }

        public static void ReportAmbiguousField(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            diagnostics.Report(name.SourceRange, DiagnosticId.AmbiguousField, name.ValueText);
        }

        public static void ReportCannotConvert(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, TypeSymbol sourceType, TypeSymbol targetType)
        {
            var sourceTypeName = sourceType.ToDisplayName();
            var targetTypeName = targetType.ToDisplayName();
            diagnostics.Report(sourceRange, DiagnosticId.CannotConvert, sourceTypeName, targetTypeName);
        }

        public static void ReportAmbiguousName(this ICollection<Diagnostic> diagnostics, SyntaxToken name, IReadOnlyList<Symbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            diagnostics.Report(name.SourceRange, DiagnosticId.AmbiguousReference, name.ValueText, symbol1.Name, symbol2.Name);
        }

        public static void ReportAmbiguousType(this ICollection<Diagnostic> diagnostics, SyntaxToken name, IReadOnlyList<Symbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            diagnostics.Report(name.SourceRange, DiagnosticId.AmbiguousType, name.ValueText, symbol1.Name, symbol2.Name);
        }

        public static void ReportAmbiguousNamespaceOrType(this ICollection<Diagnostic> diagnostics, QualifiedDeclarationNameSyntax syntax, IReadOnlyList<Symbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            diagnostics.Report(syntax.SourceRange, DiagnosticId.AmbiguousNamespaceOrType, syntax.ToStringIgnoringMacroReferences(), symbol1.Name, symbol2.Name);
        }

        public static void ReportInvocationRequiresParenthesis(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            diagnostics.Report(name.SourceRange, DiagnosticId.InvocationRequiresParenthesis, name.ValueText);
        }

        public static void ReportCannotApplyBinaryOperator(this ICollection<Diagnostic> diagnostics, SyntaxToken operatorToken, TypeSymbol leftType, TypeSymbol rightType)
        {
            var operatorName = operatorToken.Text;
            var leftTypeName = leftType.ToDisplayName();
            var rightTypeName = rightType.ToDisplayName();
            diagnostics.Report(operatorToken.SourceRange, DiagnosticId.CannotApplyBinaryOperator, operatorName, leftTypeName, rightTypeName);
        }

        public static void ReportAmbiguousBinaryOperator(this ICollection<Diagnostic> diagnostics, SyntaxToken operatorToken, TypeSymbol leftType, TypeSymbol rightType)
        {
            var operatorName = operatorToken.Text;
            var leftTypeName = leftType.ToDisplayName();
            var rightTypeName = rightType.ToDisplayName();
            diagnostics.Report(operatorToken.SourceRange, DiagnosticId.AmbiguousBinaryOperator, operatorName, leftTypeName, rightTypeName);
        }

        public static void ReportCannotApplyUnaryOperator(this ICollection<Diagnostic> diagnostics, SyntaxToken operatorToken, TypeSymbol type)
        {
            var operatorName = operatorToken.Text;
            var typeName = type.ToDisplayName();
            diagnostics.Report(operatorToken.SourceRange, DiagnosticId.CannotApplyUnaryOperator, operatorName, typeName);
        }

        public static void ReportAmbiguousUnaryOperator(this ICollection<Diagnostic> diagnostics, SyntaxToken operatorToken, TypeSymbol type)
        {
            var operatorName = operatorToken.Text;
            var typeName = type.ToDisplayName();
            diagnostics.Report(operatorToken.SourceRange, DiagnosticId.AmbiguousUnaryOperator, operatorName, typeName);
        }

        public static void ReportFunctionMissingImplementation(this ICollection<Diagnostic> diagnostics, FunctionInvocationExpressionSyntax syntax)
        {
            diagnostics.Report(syntax.Name.SourceRange, DiagnosticId.FunctionMissingImplementation, syntax.Name.ToStringIgnoringMacroReferences());
        }

        public static void ReportMethodMissingImplementation(this ICollection<Diagnostic> diagnostics, MethodInvocationExpressionSyntax syntax)
        {
            diagnostics.Report(syntax.Name.SourceRange, DiagnosticId.FunctionMissingImplementation, syntax.Name.Text);
        }

        public static void ReportSymbolRedefined(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, Symbol symbol)
        {
            diagnostics.Report(sourceRange, DiagnosticId.SymbolRedefined, symbol.Name);
        }

        public static void ReportLoopControlVariableConflict(this ICollection<Diagnostic> diagnostics, VariableDeclaratorSyntax syntax)
        {
            diagnostics.Report(syntax.Identifier.SourceRange, DiagnosticId.LoopControlVariableConflict, syntax.Identifier.Text);
        }

        public static void ReportImplicitTruncation(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, TypeSymbol sourceType, TypeSymbol destinationType)
        {
            diagnostics.Report(sourceRange, DiagnosticId.ImplicitTruncation, sourceType.Name, destinationType.Name);
        }

        public static void ReportInvalidType(this ICollection<Diagnostic> diagnostics, TypeSyntax syntax)
        {
            diagnostics.Report(syntax.SourceRange, DiagnosticId.InvalidType, syntax.ToStringIgnoringMacroReferences());
        }

        #endregion
    }
}