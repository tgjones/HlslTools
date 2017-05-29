using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShaderTools.CodeAnalysis.QuickInfo
{
    internal interface IQuickInfoProviderCoordinator
    {
        Task<(QuickInfoItem, IQuickInfoProvider)> GetItemAsync(Document document, int position, CancellationToken cancellationToken);
    }
}
