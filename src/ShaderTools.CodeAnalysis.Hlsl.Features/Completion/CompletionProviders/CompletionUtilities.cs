// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Completion;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Hlsl.Completion.Providers
{
    internal static class CompletionUtilities
    {
        internal static TextSpan GetCompletionItemSpan(SourceText text, int position)
        {
            return CommonCompletionUtilities.GetWordSpan(text, position, IsCompletionItemStartCharacter, IsWordCharacter);
        }

        public static bool IsWordStartCharacter(char ch)
        {
            return SyntaxFacts.IsIdentifierStartCharacter(ch);
        }

        public static bool IsWordCharacter(char ch)
        {
            return SyntaxFacts.IsIdentifierStartCharacter(ch) || SyntaxFacts.IsIdentifierPartCharacter(ch);
        }

        public static bool IsCompletionItemStartCharacter(char ch)
        {
            return ch == '@' || IsWordCharacter(ch);
        }

        internal static bool IsTriggerCharacter(SourceText text, int characterPosition, OptionSet options)
        {
            var ch = text[characterPosition];
            if (ch == '.')
            {
                return true;
            }

            // Trigger for directive
            if (ch == '#')
            {
                return true;
            }

            // Trigger on pointer member access
            if (ch == '>' && characterPosition >= 1 && text[characterPosition - 1] == '-')
            {
                return true;
            }

            // Trigger on alias name
            if (ch == ':' && characterPosition >= 1 && text[characterPosition - 1] == ':')
            {
                return true;
            }

            if (options.GetOption(CompletionOptions.TriggerOnTypingLetters, LanguageNames.Hlsl) && IsStartingNewWord(text, characterPosition))
            {
                return true;
            }

            return false;
        }

        public static bool IsStartingNewWord(SourceText text, int characterPosition)
        {
            return CommonCompletionUtilities.IsStartingNewWord(
                text, characterPosition, IsWordStartCharacter, IsWordCharacter);
        }
    }
}
