// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Shared.Extensions
{
    internal static class TextLineExtensions
    {
        public static int? GetLastNonWhitespacePosition(this TextLine line)
        {
            int startPosition = line.Start;
            var text = line.ToString();

            for (int i = text.Length - 1; i >= 0; i--)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return startPosition + i;
                }
            }

            return null;
        }
    }
}
