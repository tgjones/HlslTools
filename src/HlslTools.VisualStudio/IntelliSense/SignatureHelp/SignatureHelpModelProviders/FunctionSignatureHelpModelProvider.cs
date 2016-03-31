using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp.SignatureHelpModelProviders
{
    [Export(typeof(ISignatureHelpModelProvider))]
    internal sealed class FunctionSignatureHelpModelProvider : SignatureHelpModelProvider<FunctionInvocationExpressionSyntax>
    {
        protected override SignatureHelpModel GetModel(SemanticModel semanticModel, FunctionInvocationExpressionSyntax node, SourceLocation position)
        {
            var name = node.Name;
            var functionSignatures = semanticModel
                .LookupSymbols(name.SourceRange.Start)
                .OfType<FunctionSymbol>()
                .Where(f => name.GetUnqualifiedName().Name.Text == f.Name)
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