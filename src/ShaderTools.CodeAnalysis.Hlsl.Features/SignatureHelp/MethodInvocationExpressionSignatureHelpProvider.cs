using System.Collections.Generic;
using System.Composition;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.SignatureHelp;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.SignatureHelp
{
    [ExportSignatureHelpProvider(nameof(MethodInvocationExpressionSignatureHelpProvider), LanguageNames.Hlsl), Shared]
    internal sealed class MethodInvocationExpressionSignatureHelpProvider : InvocationExpressionSignatureHelpProvider<MethodInvocationExpressionSyntax>
    {
        protected override IEnumerable<FunctionSymbol> GetFunctionSymbols(SemanticModel semanticModel, MethodInvocationExpressionSyntax node, SourceLocation position)
        {
            var targetType = semanticModel.GetExpressionType(node.Target);
            var name = node.Name;
            return targetType
                .LookupMembers<FunctionSymbol>(name.Text)
                .OrderBy(f => f.Parameters.Length);
        }
    }
}
