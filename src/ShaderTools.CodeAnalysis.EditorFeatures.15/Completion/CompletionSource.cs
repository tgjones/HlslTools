using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Text.Shared.Extensions;
using CompletionService = ShaderTools.CodeAnalysis.Completion.CompletionService;

namespace ShaderTools.CodeAnalysis.Editor.Completion
{
    internal sealed class CompletionSource : IAsyncCompletionSource
    {
        private readonly ITextView _textView;
        private readonly CompletionService _completionService;

        public CompletionSource(ITextView textView)
        {
            _textView = textView;

            if (!Workspace.TryGetWorkspace(textView.TextBuffer.AsTextContainer(), out var workspace))
            {
                throw new InvalidOperationException();
            }

            _completionService = workspace.Services.GetLanguageServices(textView.TextBuffer).GetService<CompletionService>();
        }

        public async Task<CompletionContext> GetCompletionContextAsync(InitialTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var document = applicableToSpan.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

            Microsoft.CodeAnalysis.Completion.CompletionTrigger CreateTrigger()
            {
                switch (trigger.Reason)
                {
                    case InitialTriggerReason.Invoke:
                    case InitialTriggerReason.InvokeAndCommitIfUnique:
                        return Microsoft.CodeAnalysis.Completion.CompletionTrigger.Invoke;
                    case InitialTriggerReason.Insertion:
                        return Microsoft.CodeAnalysis.Completion.CompletionTrigger.CreateInsertionTrigger(trigger.Character);
                    case InitialTriggerReason.Deletion:
                        return Microsoft.CodeAnalysis.Completion.CompletionTrigger.CreateDeletionTrigger(trigger.Character);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var completions = await _completionService.GetCompletionsAsync(
                document,
                triggerLocation.Position,
                CreateTrigger(),
                document.Workspace.Options,
                cancellationToken: token);

            if (completions == null)
            {
                return CompletionContext.Empty;
            }

            var items = completions.Items
                .Select(x =>
                {
                    var item = new CompletionItem(x.DisplayText, this, new ImageElement(x.Glyph.GetImageId()));
                    item.Properties.AddProperty("Document", document);
                    item.Properties.AddProperty("CompletionItem", x);
                    return item;
                })
                .ToImmutableArray();

            return new CompletionContext(items);
        }

        public Task<object> GetDescriptionAsync(CompletionItem item, CancellationToken token)
        {
            var document = item.Properties.GetProperty<Document>("Document");
            var taggedTextMappingService = document.LanguageServices.GetService<ITaggedTextMappingService>();

            var ourCompletionItem = item.Properties.GetProperty<CodeAnalysis.Completion.CompletionItem>("CompletionItem");
            var description = CodeAnalysis.Completion.CommonCompletionItem.GetDescription(ourCompletionItem);

            return Task.FromResult((object)description.TaggedParts.ToClassifiedTextElement(taggedTextMappingService));
        }

        public bool TryGetApplicableToSpan(char typedChar, SnapshotPoint triggerLocation, out SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var document = triggerLocation.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
            var span = _completionService.GetDefaultCompletionListSpan(document.SourceText, triggerLocation.Position);
            applicableToSpan = span.ToSnapshotSpan(triggerLocation.Snapshot);
            return true;
        }
    }
}
