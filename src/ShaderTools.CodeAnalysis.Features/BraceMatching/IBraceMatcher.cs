using System.Threading;
using Microsoft.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.BraceMatching
{
    internal interface IBraceMatcher : ILanguageService
    {
        BraceMatchingResult? FindBraces(SyntaxTreeBase syntaxTree, SourceLocation position, CancellationToken cancellationToken = default(CancellationToken));
    }
}
