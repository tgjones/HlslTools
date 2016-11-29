using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Symbols;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.SignatureHelp.SignatureHelpModelProviders
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
                .Where(f => !f.IsNumericConstructor && name.GetUnqualifiedName().Name.Text == f.Name)
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