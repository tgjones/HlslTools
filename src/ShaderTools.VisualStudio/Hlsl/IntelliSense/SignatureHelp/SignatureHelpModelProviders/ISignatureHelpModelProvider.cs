using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.SignatureHelp.SignatureHelpModelProviders
{
    internal interface ISignatureHelpModelProvider
    {
        SignatureHelpModel GetModel(SemanticModel semanticModel, SourceLocation position);
    }
}