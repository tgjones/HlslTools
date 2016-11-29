using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace ShaderTools.VisualStudio.Hlsl.Tagging
{
    internal interface IAsyncTagger
    {
        Task InvalidateTags(ITextSnapshot snapshot, CancellationToken cancellationToken);
    }
}