using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp.SignatureHelpModelProviders
{
    [Export(typeof(ISignatureHelpModelProvider))]
    internal sealed class MethodSignatureHelpModelProvider : SignatureHelpModelProvider<MethodInvocationExpressionSyntax>
    {
        protected override SignatureHelpModel GetModel(SemanticModel semanticModel, MethodInvocationExpressionSyntax node, SourceLocation position)
        {
            var targetType = semanticModel.GetExpressionType(node.Target);
            var name = node.Name;
            var signatures = targetType
                .LookupMembers<FunctionSymbol>(name.Text)
                .OrderBy(f => f.Parameters.Length)
                .ToSignatureItems()
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