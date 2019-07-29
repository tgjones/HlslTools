// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Completion
{
    internal static class CommonCompletionUtilities
    {
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
    }
}
