using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp.SignatureHelpModelProviders
{
    internal interface ISignatureHelpModelProvider
    {
        SignatureHelpModel GetModel(SemanticModel semanticModel, SourceLocation position);
    }
}