// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.LanguageServices;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.Utilities.Collections;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.Completion
{
    internal static class CommonCompletionUtilities
    {
        private const string NonBreakingSpaceString = "\x00A0";

        public static TextSpan GetWordSpan(SourceText text, int position,
            Func<char, bool> isWordStartCharacter, Func<char, bool> isWordCharacter)
        {
            int start = position;
            while (start > 0 && isWordStartCharacter(text[start - 1]))
            {
                start--;
            }

            // If we're brought up in the middle of a word, extend to the end of the word as well.
            // This means that if a user brings up the completion list at the start of the word they
            // will "insert" the text before what's already there (useful for qualifying existing
            // text).  However, if they bring up completion in the "middle" of a word, then they will
            // "overwrite" the text. Useful for correcting misspellings or just replacing unwanted
            // code with new code.
            int end = position;
            if (start != position)
            {
                while (end < text.Length && isWordCharacter(text[end]))
                {
                    end++;
                }
            }

            return TextSpan.FromBounds(start, end);
        }

        public static bool IsStartingNewWord(SourceText text, int characterPosition, Func<char, bool> isWordStartCharacter, Func<char, bool> isWordCharacter)
        {
            var ch = text[characterPosition];
            if (!isWordStartCharacter(ch))
            {
                return false;
            }

            // Only want to trigger if we're the first character in an identifier.  If there's a
            // character before or after us, then we don't want to trigger.
            if (characterPosition > 0 &&
                isWordCharacter(text[characterPosition - 1]))
            {
                return false;
            }

            if (characterPosition < text.Length - 1 &&
                isWordCharacter(text[characterPosition + 1]))
            {
                return false;
            }

            return true;
        }

        public static Func<CancellationToken, Task<CompletionDescription>> CreateDescriptionFactory(
            Workspace workspace,
            SemanticModelBase semanticModel,
            int position,
            ISymbol symbol)
        {
            return CreateDescriptionFactory(workspace, semanticModel, position, new[] { symbol });
        }

        public static Func<CancellationToken, Task<CompletionDescription>> CreateDescriptionFactory(
            Workspace workspace, SemanticModelBase semanticModel, int position, IReadOnlyList<ISymbol> symbols)
        {
            return c => CreateDescriptionAsync(workspace, semanticModel, position, symbols, cancellationToken: c);
        }

        public static async Task<CompletionDescription> CreateDescriptionAsync(
            Workspace workspace, SemanticModelBase semanticModel, int position, IReadOnlyList<ISymbol> symbols, CancellationToken cancellationToken)
        {
            var symbolDisplayService = workspace.Services.GetLanguageServices(semanticModel.Language).GetService<ISymbolDisplayService>();

            // TODO(cyrusn): Figure out a way to cancel this.
            var symbol = symbols[0];
            var sections = await symbolDisplayService.ToDescriptionGroupsAsync(workspace, semanticModel, position, ImmutableArray.Create(symbol), cancellationToken).ConfigureAwait(false);

            if (!sections.ContainsKey(SymbolDescriptionGroups.MainDescription))
            {
                return CompletionDescription.Empty;
            }

            var textContentBuilder = new List<TaggedText>();
            textContentBuilder.AddRange(sections[SymbolDescriptionGroups.MainDescription]);

            switch (symbol.Kind)
            {
                case SymbolKind.Function:
                    if (symbols.Count > 1)
                    {
                        var overloadCount = symbols.Count - 1;
                        var isGeneric = false;

                        textContentBuilder.AddSpace();
                        textContentBuilder.AddPunctuation("(");
                        textContentBuilder.AddPunctuation("+");
                        textContentBuilder.AddText(NonBreakingSpaceString + overloadCount.ToString());

                        AddOverloadPart(textContentBuilder, overloadCount, isGeneric);

                        textContentBuilder.AddPunctuation(")");
                    }

                    break;
            }

            AddDocumentationPart(textContentBuilder, symbol, semanticModel, position, cancellationToken);

            return CompletionDescription.Create(textContentBuilder.AsImmutable());
        }

        private static void AddOverloadPart(List<TaggedText> textContentBuilder, int overloadCount, bool isGeneric)
        {
            var text = isGeneric
                ? overloadCount == 1
                    ? "generic overload"
                    : "generic overloads"
                : overloadCount == 1
                    ? "overload"
                    : "overloads";

            textContentBuilder.AddText(NonBreakingSpaceString + text);
        }

        private static void AddDocumentationPart(
            List<TaggedText> textContentBuilder, ISymbol symbol, SemanticModelBase semanticModel, int position, CancellationToken cancellationToken)
        {
            var documentation = symbol.GetDocumentationParts(semanticModel, position, cancellationToken);

            if (documentation.Any())
            {
                textContentBuilder.AddLineBreak();
                textContentBuilder.AddRange(documentation);
            }
        }

        internal static bool IsTextualTriggerString(SourceText text, int characterPosition, string value)
        {
            // The character position starts at the last character of 'value'.  So if 'value' has
            // length 1, then we don't want to move, if it has length 2 we want to move back one,
            // etc.
            characterPosition = characterPosition - value.Length + 1;

            for (int i = 0; i < value.Length; i++, characterPosition++)
            {
                if (characterPosition < 0 || characterPosition >= text.Length)
                {
                    return false;
                }

                if (text[characterPosition] != value[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
