using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Text.Shared.Extensions;
using QuickInfoService = ShaderTools.CodeAnalysis.QuickInfo.QuickInfoService;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.QuickInfo
{
    internal sealed class QuickInfoSource : IAsyncQuickInfoSource
    {
        private readonly ITextBuffer _subjectBuffer;
        private readonly IDocumentProvider _documentProvider;

        public QuickInfoSource(
            ITextBuffer subjectBuffer,
            IDocumentProvider documentProvider)
        {
            _subjectBuffer = subjectBuffer;
            _documentProvider = documentProvider;
        }

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            var snapshot = _subjectBuffer.CurrentSnapshot;
            var position = session.GetTriggerPoint(snapshot);
            if (position.HasValue)
            {
                var document = await _documentProvider.GetDocumentAsync(snapshot, cancellationToken).ConfigureAwait(false);
                if (document == null)
                {
                    return null;
                }

                var quickInfoService = QuickInfoService.GetService(document);

                var item = await quickInfoService.GetQuickInfoAsync(document, position.Value.Position, cancellationToken).ConfigureAwait(false);
                if (item == null)
                {
                    return null;
                }

                var quickInfoElement = CreateElement(item.Content);

                return new QuickInfoItem(
                    snapshot.CreateTrackingSpan(item.TextSpan.ToSpan(), SpanTrackingMode.EdgeInclusive),
                    quickInfoElement);
            }

            return null;
        }

        private static ContainerElement CreateElement(CodeAnalysis.QuickInfo.QuickInfoContent content)
        {
            var taggedTextMappingService = PrimaryWorkspace.Workspace.Services.GetLanguageServices(content.Language).GetService<ITaggedTextMappingService>();

            var glyphElement = new ImageElement(content.Glyph.GetImageMoniker().ToImageId());

            var elements = new List<object>
            {
                new ContainerElement(
                    ContainerElementStyle.Wrapped,
                    glyphElement,
                    content.MainDescription.ToClassifiedTextElement(taggedTextMappingService))
            };

            if (!content.Documentation.IsEmpty)
            {
                elements.Add(new ContainerElement(
                    ContainerElementStyle.Wrapped,
                    content.Documentation.ToClassifiedTextElement(taggedTextMappingService)));
            }

            return new ContainerElement(
                ContainerElementStyle.Stacked,
                elements);
        }

        public void Dispose()
        {
        }
    }
}
