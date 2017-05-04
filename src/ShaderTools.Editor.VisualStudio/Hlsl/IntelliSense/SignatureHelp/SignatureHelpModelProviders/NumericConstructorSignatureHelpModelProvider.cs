using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.SignatureHelp.SignatureHelpModelProviders
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
            if (span == null)
                return null;

            var parameterIndex = node.ArgumentList.GetParameterIndex(position);

            return new SignatureHelpModel(span.Value.Span, signatures,
                GetSelected(semanticModel.GetSymbol(node), signatures, parameterIndex),
                parameterIndex);
        }
    }
}