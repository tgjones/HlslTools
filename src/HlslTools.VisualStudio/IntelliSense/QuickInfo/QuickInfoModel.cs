using HlslTools.Compilation;
using HlslTools.Text;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo
{
    internal sealed class QuickInfoModel
    {
        public QuickInfoModel(SemanticModel semanticModel, TextSpan span, string text)
        {
            SemanticModel = semanticModel;
            Span = span;
            Text = text;
        }

        public SemanticModel SemanticModel { get; }
        public TextSpan Span { get; }
        public string Text { get; }
    }
}