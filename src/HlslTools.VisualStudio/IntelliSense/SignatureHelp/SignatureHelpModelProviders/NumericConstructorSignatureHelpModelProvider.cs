using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp.SignatureHelpModelProviders
{
    [Export(typeof(ISignatureHelpModelProvider))]
    internal sealed class NumericConstructorSignatureHelpModelProvider : SignatureHelpModelProvider<NumericConstructorInvocationExpressionSyntax>
    {
        protected override SignatureHelpModel GetModel(SemanticModel semanticModel, NumericConstructorInvocationExpressionSyntax node, SourceLocation position)
        {
            var typeSymbol = semanticModel.GetExpressionType(node);
            if (typeSymbol.IsError())
                return null;

            var functionSignatures = semanticModel
                .LookupSymbols(node.Type.SourceRange.Start)
                .OfType<FunctionSymbol>()
                .Where(f => f.IsNumericConstructor && f.ReturnType.Equals(typeSymbol))
                .ToSignatureItems();

            var signatures = functionSignatures
                .OrderBy(s => s.Parameters.Length)
                .ToImmutableArray();

            if (signatures.Length == 0)
                return null;

            var span = node.GetTextSpanRoot();
            var parameterIndex = node.ArgumentList.GetParameterIndex(position);

            return new SignatureHelpModel(span, signatures,
                GetSelected(semanticModel.GetSymbol(node), signatures, parameterIndex),
                parameterIndex);
        }
    }
}