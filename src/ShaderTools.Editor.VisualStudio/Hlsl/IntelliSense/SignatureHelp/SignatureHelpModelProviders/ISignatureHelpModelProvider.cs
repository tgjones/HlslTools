using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.SignatureHelp.SignatureHelpModelProviders
{
    internal interface ISignatureHelpModelProvider
    {
        SignatureHelpModel GetModel(SemanticModel semanticModel, SourceLocation position);
    }
}