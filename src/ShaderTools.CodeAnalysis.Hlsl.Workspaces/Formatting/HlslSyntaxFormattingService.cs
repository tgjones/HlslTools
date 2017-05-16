using System.Composition;
using System.Threading;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.CodeAnalysis.Hlsl.Options;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

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
            var edits = Formatter.GetEdits((SyntaxTree) tree, span, _optionsService.GetFormattingOptions(options));
            return Formatter.ApplyEdits(text.ToString(), edits);
        }
    }
}
