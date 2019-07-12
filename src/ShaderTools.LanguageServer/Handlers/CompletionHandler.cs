using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Completion;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using CompletionItem = OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionItem;
using CompletionList = OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionList;

namespace ShaderTools.LanguageServer.Handlers
{
    internal sealed class CompletionHandler : ICompletionHandler
    {
        private readonly LanguageServerWorkspace _workspace;
        private readonly TextDocumentRegistrationOptions _registrationOptions;

        public CompletionHandler(LanguageServerWorkspace workspace, TextDocumentRegistrationOptions registrationOptions)
        {
            _workspace = workspace;
            _registrationOptions = registrationOptions;
        }

        public CompletionRegistrationOptions GetRegistrationOptions()
        {
            return new CompletionRegistrationOptions
            {
                DocumentSelector = _registrationOptions.DocumentSelector,
                TriggerCharacters = new Container<string>(".", ":"),
                ResolveProvider = false
            };
        }

        public async Task<CompletionList> Handle(CompletionParams request, CancellationToken token)
        {
            var (document, position) = _workspace.GetLogicalDocument(request);

            var completionService = document.GetLanguageService<CompletionService>();

            var triggerKind = request.Context.TriggerKind == OmniSharp.Extensions.LanguageServer.Protocol.Models.CompletionTriggerKind.TriggerCharacter
                ? CodeAnalysis.Completion.CompletionTriggerKind.Insertion
                : CodeAnalysis.Completion.CompletionTriggerKind.Invoke;
            var triggerChar = triggerKind == CodeAnalysis.Completion.CompletionTriggerKind.Insertion
                ? request.Context.TriggerCharacter[0]
                : default;
            var trigger = new CompletionTrigger(triggerKind, triggerChar);

            var completionList = await completionService.GetCompletionsAsync(document, position, trigger, cancellationToken: token);
            if (completionList == null)
            {
                return new CompletionList();
            }

            var completionItems = completionList.Items
                .Select(x => ConvertCompletionItem(document, completionList.Rules, x))
                .ToArray();

            return completionItems;
        }

        public void SetCapability(CompletionCapability capability) { }

        private static CompletionItem ConvertCompletionItem(Document document, CompletionRules completionRules, CodeAnalysis.Completion.CompletionItem item)
        {
            var description = CommonCompletionItem.GetDescription(item);

            // A bit hacky: everything before the line break is the Detail, everything after is the Documentation.
            var detail = string.Empty;
            var documentation = string.Empty;
            var seenLineBreak = false;
            foreach (var taggedText in description.TaggedParts)
            {
                if (seenLineBreak)
                {
                    documentation += taggedText.Text;
                }
                else
                {
                    if (taggedText.Text.ContainsLineBreak())
                    {
                        seenLineBreak = true;
                    }
                    else
                    {
                        detail += taggedText.Text;
                    }
                }
            }

            return new CompletionItem
            {
                Label = item.DisplayText,
                SortText = item.SortText,
                FilterText = item.FilterText,
                Kind = GetKind(item.Glyph),
                TextEdit = new TextEdit
                {
                    NewText = item.DisplayText,
                    Range = Helpers.ToRange(document.SourceText, item.Span)
                },
                Detail = detail,
                Documentation = documentation,
                CommitCharacters = completionRules.DefaultCommitCharacters.Select(x => x.ToString()).ToArray()
            };
        }

        private static CompletionItemKind GetKind(Glyph glyph)
        {
            switch (glyph)
            {
                case Glyph.None:
                    return CompletionItemKind.Class;
                case Glyph.Class:
                    return CompletionItemKind.Class;
                case Glyph.Constant:
                    return CompletionItemKind.Constant;
                case Glyph.Field:
                    return CompletionItemKind.Field;
                case Glyph.Interface:
                    return CompletionItemKind.Interface;
                case Glyph.IntrinsicClass:
                    return CompletionItemKind.Class;
                case Glyph.IntrinsicStruct:
                    return CompletionItemKind.Struct;
                case Glyph.Keyword:
                    return CompletionItemKind.Keyword;
                case Glyph.Label:
                    return CompletionItemKind.Keyword;
                case Glyph.Local:
                    return CompletionItemKind.Variable;
                case Glyph.Macro:
                    return CompletionItemKind.Reference;
                case Glyph.Namespace:
                    return CompletionItemKind.Module;
                case Glyph.Method:
                    return CompletionItemKind.Method;
                case Glyph.Module:
                    return CompletionItemKind.Module;
                case Glyph.OpenFolder:
                    return CompletionItemKind.Folder;
                case Glyph.Operator:
                    return CompletionItemKind.Operator;
                case Glyph.Parameter:
                    return CompletionItemKind.Variable;
                case Glyph.Structure:
                    return CompletionItemKind.Struct;
                case Glyph.Typedef:
                    return CompletionItemKind.Variable;
                case Glyph.TypeParameter:
                    return CompletionItemKind.TypeParameter;
                case Glyph.CompletionWarning:
                    return CompletionItemKind.Snippet;
                default:
                    throw new ArgumentOutOfRangeException(nameof(glyph));
            }
        }
    }
}
