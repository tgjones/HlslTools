using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShaderTools.CodeAnalysis.QuickInfo
{
    internal interface IQuickInfoProviderCoordinator
    {
        Task<Tuple<QuickInfoItem, IQuickInfoProvider>> GetItemAsync(Document document, int position, CancellationToken cancellationToken);
    }
}
