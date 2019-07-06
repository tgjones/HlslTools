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

            var completionList = await completionService.GetCompletionsAsync(document, position);
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
            var documentation = CommonCompletionItem.HasDescription(item)
                ? CommonCompletionItem.GetDescription(item).Text
                : string.Empty;

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
                case Glyph.Intrinsic:
                    return CompletionItemKind.Function;
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
