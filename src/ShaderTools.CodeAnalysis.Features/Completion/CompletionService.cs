// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Shared.Utilities;
using ShaderTools.Utilities.Collections;
using ShaderTools.Utilities.Diagnostics;

namespace ShaderTools.CodeAnalysis.Completion
{
    /// <summary>
    /// A per language service for constructing context dependent list of completions that 
    /// can be presented to a user during typing in an editor.
    /// </summary>
    internal abstract class CompletionService : ILanguageService, IEqualityComparer<ImmutableHashSet<string>>
    {
        /// <summary>
        /// Gets the service corresponding to the specified document.
        /// </summary>
        public static CompletionService GetService(Document document)
            => document.GetLanguageService<CompletionService>();

        private static readonly Func<string, List<CompletionItem>> s_createList = _ => new List<CompletionItem>();

        private readonly object _gate = new object();

        private readonly ImmutableArray<CompletionProvider> _providers;
        private readonly Dictionary<string, CompletionProvider> _nameToProvider = new Dictionary<string, CompletionProvider>();

        private readonly Workspace _workspace;

        /// <summary>
        /// Internal for testing purposes.
        /// </summary>
        internal readonly ImmutableArray<CompletionProvider>? ExclusiveProviders;

        private IEnumerable<Lazy<CompletionProvider, OrderableLanguageMetadata>> _importedProviders;

        /// <summary>
        /// The language from <see cref="LanguageNames"/> this service corresponds to.
        /// </summary>
        public abstract string Language { get; }

        internal CompletionService(
            Workspace workspace,
            ImmutableArray<CompletionProvider>? exclusiveProviders = null)
        {
            _workspace = workspace;
            ExclusiveProviders = exclusiveProviders;
            _providers = CreateProviders();
        }

        public virtual CompletionRules GetRules()
        {
            return CompletionRules.Default;
        }

        /// <summary>
        /// Returns the providers always available to the service.
        /// This does not included providers imported via MEF composition.
        /// </summary>
        protected virtual ImmutableArray<CompletionProvider> GetBuiltInProviders()
        {
            return ImmutableArray<CompletionProvider>.Empty;
        }

        private IEnumerable<Lazy<CompletionProvider, OrderableLanguageMetadata>> GetImportedProviders()
        {
            if (_importedProviders == null)
            {
                var language = this.Language;
                var mefExporter = (IMefHostExportProvider) _workspace.Services.HostServices;

                var providers = ExtensionOrderer.Order(
                    mefExporter.GetExports<CompletionProvider, OrderableLanguageMetadata>()
                        .Where(lz => lz.Metadata.Language == language)
                ).ToList();

                Interlocked.CompareExchange(ref _importedProviders, providers, null);
            }

            return _importedProviders;
        }

        private ImmutableArray<CompletionProvider> _testProviders = ImmutableArray<CompletionProvider>.Empty;

        internal void SetTestProviders(IEnumerable<CompletionProvider> testProviders)
        {
            lock (_gate)
            {
                _testProviders = testProviders != null ? testProviders.ToImmutableArray() : ImmutableArray<CompletionProvider>.Empty;
                _nameToProvider.Clear();
            }
        }

        private ImmutableArray<CompletionProvider> CreateProviders()
        {
            var providers = GetAllProviders();

            foreach (var provider in providers)
            {
                _nameToProvider[provider.Name] = provider;
            }

            return providers;
        }

        private ImmutableArray<CompletionProvider> GetAllProviders()
        {
            if (ExclusiveProviders.HasValue)
            {
                return ExclusiveProviders.Value;
            }

            var builtin = GetBuiltInProviders();
            var imported = GetImportedProviders()
                .Select(lz => lz.Value);

            var providers = builtin.Concat(imported).Concat(_testProviders);
            return providers.ToImmutableArray();
        }

        protected virtual ImmutableArray<CompletionProvider> GetProviders(CompletionTrigger trigger)
        {
            return _providers;
        }

        public virtual TextSpan GetDefaultCompletionListSpan(SourceText text, int caretPosition)
        {
            return CommonCompletionUtilities.GetWordSpan(
                text, caretPosition, c => char.IsLetter(c), c => char.IsLetterOrDigit(c));
        }

        /// <summary>
        /// Gets the completions available at the caret position.
        /// </summary>
        /// <param name="document">The document that completion is occuring within.</param>
        /// <param name="caretPosition">The position of the caret after the triggering action.</param>
        /// <param name="trigger">The triggering action.</param>
        /// <param name="options">Optional options that override the default options.</param>
        /// <param name="cancellationToken"></param>
        public virtual async Task<CompletionList> GetCompletionsAsync(
            Document document,
            int caretPosition,
            CompletionTrigger trigger,
            OptionSet options = null,
            CancellationToken cancellationToken = default)
        {
            var text = document.SourceText;
            var defaultItemSpan = this.GetDefaultCompletionListSpan(text, caretPosition);

            options = options ?? await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);
            var providers = GetProviders(trigger);

            var completionProviderToIndex = GetCompletionProviderToIndex(providers);

            var triggeredProviders = ImmutableArray<CompletionProvider>.Empty;
            switch (trigger.Kind)
            {
                case CompletionTriggerKind.Insertion:
                case CompletionTriggerKind.Deletion:
                    if (this.ShouldTriggerCompletion(text, caretPosition, trigger, options))
                    {
                        triggeredProviders = providers.Where(p => p.ShouldTriggerCompletion(text, caretPosition, trigger, options)).ToImmutableArrayOrEmpty();
                        if (triggeredProviders.Length == 0)
                        {
                            triggeredProviders = providers;
                        }
                    }
                    break;
                default:
                    triggeredProviders = providers;
                    break;
            }

            // Now, ask all the triggered providers, in parallel, to populate a completion context.
            // Note: we keep any context with items *or* with a suggested item.  
            var triggeredCompletionContexts = await ComputeNonEmptyCompletionContextsAsync(
                document, caretPosition, trigger, options,
                defaultItemSpan, triggeredProviders,
                cancellationToken).ConfigureAwait(false);

            // If we didn't even get any back with items, then there's nothing to do.
            // i.e. if only got items back that had only suggestion items, then we don't
            // want to show any completion.
            if (!triggeredCompletionContexts.Any(cc => cc.Items.Count > 0))
            {
                return null;
            }

            // All the contexts should be non-empty or have a suggestion item.
            Debug.Assert(triggeredCompletionContexts.All(HasAnyItems));

            // See if there was a completion context provided that was exclusive.  If so, then
            // that's all we'll return.
            var firstExclusiveContext = triggeredCompletionContexts.FirstOrDefault(t => t.IsExclusive);

            if (firstExclusiveContext != null)
            {
                return MergeAndPruneCompletionLists(
                    SpecializedCollections.SingletonEnumerable(firstExclusiveContext),
                    defaultItemSpan,
                    isExclusive: true);
            }

            // Shouldn't be any exclusive completion contexts at this point.
            Debug.Assert(triggeredCompletionContexts.All(cc => !cc.IsExclusive));

            // Great!  We had some items.  Now we want to see if any of the other providers 
            // would like to augment the completion list.  For example, we might trigger
            // enum-completion on space.  If enum completion results in any items, then 
            // we'll want to augment the list with all the regular symbol completion items.
            var augmentingProviders = providers.Except(triggeredProviders).ToImmutableArray();

            var augmentingCompletionContexts = await ComputeNonEmptyCompletionContextsAsync(
                document, caretPosition, trigger, options, defaultItemSpan,
                augmentingProviders, cancellationToken).ConfigureAwait(false);

            var allContexts = triggeredCompletionContexts.Concat(augmentingCompletionContexts);
            Debug.Assert(allContexts.Length > 0);

            // Providers are ordered, but we processed them in our own order.  Ensure that the
            // groups are properly ordered based on the original providers.
            allContexts = allContexts.Sort((p1, p2) => completionProviderToIndex[p1.Provider] - completionProviderToIndex[p2.Provider]);

            return MergeAndPruneCompletionLists(allContexts, defaultItemSpan, isExclusive: false);
        }

        private static bool HasAnyItems(CompletionContext cc)
        {
            return cc.Items.Count > 0 || cc.SuggestionModeItem != null;
        }

        private async Task<ImmutableArray<CompletionContext>> ComputeNonEmptyCompletionContextsAsync(
            Document document, int caretPosition, CompletionTrigger trigger,
            OptionSet options, TextSpan defaultItemSpan,
            ImmutableArray<CompletionProvider> providers,
            CancellationToken cancellationToken)
        {
            var completionContextTasks = new List<Task<CompletionContext>>();
            foreach (var provider in providers)
            {
                completionContextTasks.Add(GetContextAsync(
                    provider, document, caretPosition, trigger,
                    options, defaultItemSpan, cancellationToken));
            }

            var completionContexts = await Task.WhenAll(completionContextTasks).ConfigureAwait(false);
            var nonEmptyContexts = completionContexts.Where(HasAnyItems).ToImmutableArray();
            return nonEmptyContexts;
        }

        private CompletionList MergeAndPruneCompletionLists(
            IEnumerable<CompletionContext> completionContexts,
            TextSpan defaultSpan,
            bool isExclusive)
        {
            // See if any contexts changed the completion list span.  If so, the first context that
            // changed it 'wins' and picks the span that will be used for all items in the completion
            // list.  If no contexts changed it, then just use the default span provided by the service.
            var finalCompletionListSpan = completionContexts.FirstOrDefault(c => c.CompletionListSpan != defaultSpan)?.CompletionListSpan ?? defaultSpan;

            var displayNameToItemsMap = new Dictionary<string, List<CompletionItem>>();
            CompletionItem suggestionModeItem = null;

            foreach (var context in completionContexts)
            {
                Contract.Assert(context != null);

                foreach (var item in context.Items)
                {
                    Contract.Assert(item != null);
                    AddToDisplayMap(item, displayNameToItemsMap);
                }

                // first one wins
                suggestionModeItem = suggestionModeItem ?? context.SuggestionModeItem;
            }

            if (displayNameToItemsMap.Count == 0)
            {
                return CompletionList.Empty;
            }

            // TODO(DustinCa): Revisit performance of this.
            var totalItems = displayNameToItemsMap.Values.Flatten().ToList();
            totalItems.Sort();

            return CompletionList.Create(
                finalCompletionListSpan,
                totalItems.ToImmutableArray(),
                this.GetRules(),
                suggestionModeItem,
                isExclusive);
        }

        private void AddToDisplayMap(
            CompletionItem item,
            Dictionary<string, List<CompletionItem>> displayNameToItemsMap)
        {
            var sameNamedItems = displayNameToItemsMap.GetOrAdd(item.DisplayText, s_createList);

            // If two items have the same display text choose which one to keep.
            // If they don't actually match keep both.

            for (int i = 0; i < sameNamedItems.Count; i++)
            {
                var existingItem = sameNamedItems[i];

                Contract.Assert(item.DisplayText == existingItem.DisplayText);

                if (ItemsMatch(item, existingItem))
                {
                    sameNamedItems[i] = GetBetterItem(item, existingItem);
                    return;
                }
            }

            sameNamedItems.Add(item);
        }

        /// <summary>
        /// Determines if the items are similar enough they should be represented by a single item in the list.
        /// </summary>
        protected virtual bool ItemsMatch(CompletionItem item, CompletionItem existingItem)
        {
            return item.Span == existingItem.Span
                   && item.SortText == existingItem.SortText;
        }

        /// <summary>
        /// Determines which of two items should represent the matching pair.
        /// </summary>
        protected virtual CompletionItem GetBetterItem(CompletionItem item, CompletionItem existingItem)
        {
            // the item later in the sort order (determined by provider order) wins?
            return item;
        }

        private Dictionary<CompletionProvider, int> GetCompletionProviderToIndex(IEnumerable<CompletionProvider> completionProviders)
        {
            var result = new Dictionary<CompletionProvider, int>();

            int i = 0;
            foreach (var completionProvider in completionProviders)
            {
                result[completionProvider] = i;
                i++;
            }

            return result;
        }

        private async Task<CompletionContext> GetContextAsync(
            CompletionProvider provider,
            Document document,
            int position,
            CompletionTrigger triggerInfo,
            OptionSet options,
            TextSpan? defaultSpan,
            CancellationToken cancellationToken)
        {
            options = options ?? document.Workspace.Options;

            if (defaultSpan == null)
            {
                var text = document.SourceText;
                defaultSpan = this.GetDefaultCompletionListSpan(text, position);
            }

            var context = new CompletionContext(provider, document, position, defaultSpan.Value, triggerInfo, options, cancellationToken);
            await provider.ProvideCompletionsAsync(context).ConfigureAwait(false);
            return context;
        }

        /// <summary>
        /// Returns true if the character recently inserted or deleted in the text should trigger completion.
        /// </summary>
        /// <param name="text">The document text to trigger completion within </param>
        /// <param name="caretPosition">The position of the caret after the triggering action.</param>
        /// <param name="trigger">The potential triggering action.</param>
        /// <param name="options">Optional options that override the default options.</param>
        /// <remarks>
        /// This API uses SourceText instead of Document so implementations can only be based on text, not syntax or semantics.
        /// </remarks>
        public virtual bool ShouldTriggerCompletion(
            SourceText text, int caretPosition, CompletionTrigger trigger, OptionSet options = null)
        {
            options = options ?? _workspace.Options;
            if (!options.GetOption(CompletionOptions.TriggerOnTyping, this.Language))
            {
                return false;
            }

            if (trigger.Kind == CompletionTriggerKind.Deletion && this.SupportsTriggerOnDeletion(options))
            {
                return Char.IsLetterOrDigit(trigger.Character) || trigger.Character == '.';
            }

            var providers = GetProviders(trigger);
            return providers.Any(p => p.ShouldTriggerCompletion(text, caretPosition, trigger, options));
        }

        internal virtual bool SupportsTriggerOnDeletion(OptionSet options)
        {
            var opt = options.GetOption(CompletionOptions.TriggerOnDeletion, this.Language);
            return opt == true;
        }

        bool IEqualityComparer<ImmutableHashSet<string>>.Equals(ImmutableHashSet<string> x, ImmutableHashSet<string> y)
        {
            if (x == y)
            {
                return true;
            }

            if (x.Count != y.Count)
            {
                return false;
            }

            foreach (var v in x)
            {
                if (!y.Contains(v))
                {
                    return false;
                }
            }

            return true;
        }

        int IEqualityComparer<ImmutableHashSet<string>>.GetHashCode(ImmutableHashSet<string> obj)
        {
            var hash = 0;
            foreach (var o in obj)
            {
                hash += o.GetHashCode();
            }

            return hash;
        }
    }
}