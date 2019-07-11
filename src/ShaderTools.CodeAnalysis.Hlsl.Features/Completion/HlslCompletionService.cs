// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Completion;
using ShaderTools.CodeAnalysis.Hlsl.Completion.CompletionProviders;
using ShaderTools.CodeAnalysis.Hlsl.Completion.Providers;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;
using ILanguageService = Microsoft.CodeAnalysis.Host.ILanguageService;

namespace ShaderTools.CodeAnalysis.Hlsl.Completion
{
    [ExportLanguageServiceFactory(typeof(CompletionService), LanguageNames.Hlsl), Shared]
    internal class HlslCompletionServiceFactory : ILanguageServiceFactory
    {
        public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        {
            return new HlslCompletionService(languageServices.WorkspaceServices.Workspace);
        }
    }

    internal class HlslCompletionService : CommonCompletionService
    {
        private readonly ImmutableArray<CompletionProvider> _defaultCompletionProviders =
            ImmutableArray.Create<CompletionProvider>(
                new SemanticCompletionProvider(),
                new SymbolCompletionProvider(),
                new KeywordCompletionProvider()
            );

        private readonly Workspace _workspace;

        public HlslCompletionService(
            Workspace workspace, ImmutableArray<CompletionProvider>? exclusiveProviders = null)
            : base(workspace, exclusiveProviders)
        {
            _workspace = workspace;
        }

        public override string Language => LanguageNames.Hlsl;

        protected override ImmutableArray<CompletionProvider> GetBuiltInProviders()
        {
            return _defaultCompletionProviders;
        }

        public override TextSpan GetDefaultCompletionListSpan(SourceText text, int caretPosition)
        {
            return CompletionUtilities.GetCompletionItemSpan(text, caretPosition);
        }

        private CompletionRules _latestRules = CompletionRules.Default;

        public override CompletionRules GetRules()
        {
            var options = _workspace.Options;

            var enterRule = options.GetOption(CompletionOptions.EnterKeyBehavior, LanguageNames.Hlsl);

            // Although EnterKeyBehavior is a per-language setting, the meaning of an unset setting (Default) differs between C# and VB
            // In C# the default means Never to maintain previous behavior
            if (enterRule == EnterKeyRule.Default)
            {
                enterRule = EnterKeyRule.Never;
            }

            // use interlocked + stored rules to reduce # of times this gets created when option is different than default
            var newRules = _latestRules.WithDefaultEnterKeyRule(enterRule);

            Interlocked.Exchange(ref _latestRules, newRules);

            return newRules;
        }
    }
}