using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.CodeAnalysis.Hlsl.Options;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Formatting
{
    [ExportLanguageService(typeof(ISyntaxFormattingService), LanguageNames.Hlsl)]
    internal sealed class HlslSyntaxFormattingService : ISyntaxFormattingService
    {
        private readonly IHlslOptionsService _optionsService;

        [ImportingConstructor]
        public HlslSyntaxFormattingService(IHlslOptionsService optionsService)
        {
            _optionsService = optionsService;
        }

        public string Format(SyntaxTreeBase tree, TextSpan span, OptionSet options, CancellationToken cancellationToken)
        {
            var text = tree.Text;
            var edits = Formatter.GetEdits((SyntaxTree) tree, (SyntaxNode) tree.Root, span, _optionsService.GetFormattingOptions(options));
            return Formatter.ApplyEdits(text.ToString(), edits);
        }

        public Task<IFormattingResult> FormatAsync(SyntaxTreeBase tree, SyntaxNodeBase node, IEnumerable<TextSpan> spans, OptionSet options, CancellationToken cancellationToken)
        {
            var edits = new List<TextChange>();

            foreach (var span in spans)
                edits.AddRange(Formatter.GetEdits((SyntaxTree) tree, (SyntaxNode) node, span, _optionsService.GetFormattingOptions(options)));

            return Task.FromResult((IFormattingResult) new FormattingResult(edits));
        }
    }
}
