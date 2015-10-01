using System.Threading;
using System.Threading.Tasks;
using HlslTools.VisualStudio.Parsing;

namespace HlslTools.VisualStudio.Tagging
{
    internal interface IAsyncTagger
    {
        Task InvalidateTags(SnapshotSyntaxTree snapshotSyntaxTree, CancellationToken cancellationToken);
    }
}