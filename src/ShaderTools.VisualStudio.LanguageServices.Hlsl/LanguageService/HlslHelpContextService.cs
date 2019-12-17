using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.VisualStudio.LanguageServices.Implementation.F1Help;

namespace ShaderTools.VisualStudio.LanguageServices.Hlsl.LanguageService
{
    [ExportLanguageService(typeof(IHelpContextService), LanguageNames.Hlsl)]
    internal sealed class HlslHelpContextService : AbstractHelpContextService
    {
        public override string Language => "hlsl";

        public override string Product => "hlsl";

        public override async Task<string> GetHelpTermAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            // Find the token under the start of the selection.
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var rootFileRange = syntaxTree.MapRootFileRange(span);
            var token = await syntaxTree.GetTouchingTokenAsync(rootFileRange.Start, cancellationToken, findInsideTrivia: true).ConfigureAwait(false);

            if (IsValid(token, span))
            {
                return ((SyntaxToken)token).Text;
            }

            return string.Empty;
        }

        private static bool IsValid(ISyntaxToken token, TextSpan span)
        {
            // If the token doesn't actually intersect with our position, give up
            return token.FileSpan.IsInRootFile && token.FileSpan.Span.IntersectsWith(span);
        }
    }
}
