using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Structure;

namespace ShaderTools.CodeAnalysis.Hlsl.Structure
{
    [ExportLanguageService(typeof(IBlockStructureProvider), LanguageNames.Hlsl)]
    internal sealed class HlslBlockStructureProvider : IBlockStructureProvider
    {
        public async Task<ImmutableArray<BlockSpan>> ProvideBlockStructureAsync(Document document, CancellationToken cancellationToken)
        {
            var results = ImmutableArray.CreateBuilder<BlockSpan>();
            var outliningVisitor = new OutliningVisitor(document.SourceText, results, cancellationToken);

            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            if (syntaxTree != null)
                outliningVisitor.VisitCompilationUnit((CompilationUnitSyntax) syntaxTree.Root);

            return results.ToImmutable();
        }
    }
}
