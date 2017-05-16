using System;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;

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

            var formatted = formatter.Format(tree, new Text.TextSpan(0, tree.Text.Length), options, cancellationToken);

            return document.WithText(SourceText.From(formatted, document.SourceText.FilePath));
        }
    }
}
