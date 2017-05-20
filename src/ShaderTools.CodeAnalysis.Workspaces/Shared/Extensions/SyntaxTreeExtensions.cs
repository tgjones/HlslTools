// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Diagnostics;

namespace ShaderTools.CodeAnalysis.Shared.Extensions
{
    internal static class SyntaxTreeExtensions
    {
        public static Task<ISyntaxToken> GetTouchingTokenAsync(
            this SyntaxTreeBase syntaxTree,
            SourceLocation position,
            CancellationToken cancellationToken,
            bool findInsideTrivia = false)
        {
            return GetTouchingTokenAsync(syntaxTree, position, _ => true, cancellationToken, findInsideTrivia);
        }

        public static async Task<ISyntaxToken> GetTouchingTokenAsync(
            this SyntaxTreeBase syntaxTree,
            SourceLocation position,
            Predicate<ISyntaxToken> predicate,
            CancellationToken cancellationToken,
            bool findInsideTrivia = false)
        {
            Contract.ThrowIfNull(syntaxTree);

            if (position > syntaxTree.Root.FullSourceRange.End)
            {
                return default(ISyntaxToken);
            }

            var root = syntaxTree.Root;
            var token = root.FindToken(position, findInsideTrivia);

            if ((token.SourceRange.Contains(position) || token.SourceRange.End == position) && predicate(token))
            {
                return token;
            }

            token = token.GetPreviousToken();

            if (token.SourceRange.End == position && predicate(token))
            {
                return token;
            }

            // SyntaxKind = None
            return default(ISyntaxToken);
        }
    }
}
