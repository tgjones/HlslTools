using System.Collections.Immutable;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp.SignatureHelpModelProviders
{
    internal sealed class FunctionSignatureHelpModelProvider : SignatureHelpModelProvider<InvocationExpressionSyntax>
    {
        protected override SignatureHelpModel GetModel(SemanticModel semanticModel, InvocationExpressionSyntax node, SourceLocation position)
        {
            if (node.Expression.Kind != SyntaxKind.IdentifierName)
                return null;

            // TODO: We need to use the resolved symbol as the currently selected one.

            var nameNode = (IdentifierNameSyntax) node.Expression;

            var name = nameNode.Name;
            var functionSignatures = semanticModel
                .LookupSymbols(name.SourceRange.Start)
                .OfType<FunctionSymbol>()
                .Where(f => name.Text == f.Name)
                .ToSignatureItems();

            var signatures = functionSignatures.OrderBy(s => s.Parameters.Length).ToImmutableArray();

            if (signatures.Length == 0)
                return null;

            var span = node.GetTextSpanRoot();
            var parameterIndex = node.ArgumentList.GetParameterIndex(position);
            var selected = signatures.FirstOrDefault(s => s.Parameters.Length > parameterIndex);

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }
    }
}