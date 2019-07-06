using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.Utilities.Collections;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Formatting
{
    public static class Formatter
    {
        public static async Task<Document> FormatAsync(Document document, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            var formatter = document.LanguageServices.GetService<ISyntaxFormattingService>();

            var formatted = formatter.Format(tree, new TextSpan(0, tree.Text.Length), options, cancellationToken);

            return document.WithText(SourceText.From(formatted));
        }

        public static async Task<Document> FormatAsync(Document document, IEnumerable<TextSpan> spans, OptionSet options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var documentOptions = options ?? await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);

            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            var formatter = document.LanguageServices.GetService<ISyntaxFormattingService>();

            var formattingResult = await formatter.FormatAsync(syntaxTree, syntaxTree.Root, spans, documentOptions, cancellationToken).ConfigureAwait(false);

            var newText = document.SourceText.WithChanges(formattingResult.GetTextChanges(cancellationToken));

            return document.WithText(newText);
        }

        internal static IList<TextChange> GetFormattedTextChanges(SyntaxTreeBase tree, SyntaxNodeBase node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options, CancellationToken cancellationToken)
        {
            return GetFormattedTextChangesAsync(tree, node, spans, workspace, options, cancellationToken).WaitAndGetResult(cancellationToken);
        }

        internal static async Task<IList<TextChange>> GetFormattedTextChangesAsync(SyntaxTreeBase tree, SyntaxNodeBase node, IEnumerable<TextSpan> spans, Workspace workspace, OptionSet options, CancellationToken cancellationToken)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (spans == null)
            {
                throw new ArgumentNullException(nameof(spans));
            }

            var languageFormatter = workspace.Services.GetLanguageServices(node.Language).GetService<ISyntaxFormattingService>();
            if (languageFormatter != null)
            {
                options = options ?? workspace.Options;
                return (await languageFormatter.FormatAsync(tree, node, spans, options, cancellationToken).ConfigureAwait(false)).GetTextChanges(cancellationToken);
            }
            else
            {
                return SpecializedCollections.EmptyList<TextChange>();
            }
        }
    }
}
