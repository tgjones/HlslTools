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
    [ExportSignatureHelpProvider(nameof(FunctionInvocationExpressionSignatureHelpProvider), LanguageNames.Hlsl), Shared]
    internal sealed class FunctionInvocationExpressionSignatureHelpProvider : InvocationExpressionSignatureHelpProvider<FunctionInvocationExpressionSyntax>
    {
        protected override IEnumerable<FunctionSymbol> GetFunctionSymbols(SemanticModel semanticModel, FunctionInvocationExpressionSyntax node, SourceLocation position)
        {
            var name = node.Name;
            return semanticModel
                .LookupSymbols(name.SourceRange.Start)
                .OfType<FunctionSymbol>()
                .Where(f => !f.IsNumericConstructor && name.GetUnqualifiedName().Name.Text == f.Name);
        }
    }
}
