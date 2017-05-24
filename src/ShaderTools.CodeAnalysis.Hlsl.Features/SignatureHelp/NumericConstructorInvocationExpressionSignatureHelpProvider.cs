using System.Collections.Generic;
using System.Composition;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.SignatureHelp;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Hlsl.SignatureHelp
{
    [ExportSignatureHelpProvider(nameof(NumericConstructorInvocationExpressionSignatureHelpProvider), LanguageNames.Hlsl), Shared]
    internal sealed class NumericConstructorInvocationExpressionSignatureHelpProvider : InvocationExpressionSignatureHelpProvider<NumericConstructorInvocationExpressionSyntax>
    {
        protected override IEnumerable<FunctionSymbol> GetFunctionSymbols(SemanticModel semanticModel, NumericConstructorInvocationExpressionSyntax node, SourceLocation position)
        {
            var typeSymbol = semanticModel.GetExpressionType(node);
            if (typeSymbol.IsError())
                return SpecializedCollections.EmptyEnumerable<FunctionSymbol>();

            return semanticModel
                .LookupSymbols(node.Type.SourceRange.Start)
                .OfType<FunctionSymbol>()
                .Where(f => f.IsNumericConstructor && f.ReturnType.Equals(typeSymbol));
        }
    }
}
