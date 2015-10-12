using HlslTools.Diagnostics;
using HlslTools.VisualStudio.ErrorList;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace HlslTools.VisualStudio.Tagging.Squiggles
{
    internal abstract class ErrorTagger : AsyncTagger<IErrorTag>
    {
        private readonly string _errorType;
        private readonly IErrorListHelper _errorListHelper;

        protected ErrorTagger(string errorType, IErrorListHelper errorListHelper)
        {
            _errorType = errorType;
            _errorListHelper = errorListHelper;
        }

        protected ITagSpan<IErrorTag> CreateTagSpan(ITextSnapshot snapshot, Diagnostic diagnostic, bool squigglesEnabled)
        {
            _errorListHelper.AddError(diagnostic, diagnostic.Span);

            if (!diagnostic.Span.IsInRootFile || !squigglesEnabled)
                return null;

            var span = new Span(diagnostic.Span.Start, diagnostic.Span.Length);
            var snapshotSpan = new SnapshotSpan(snapshot, span);
            var errorTag = new ErrorTag(_errorType, diagnostic.Message);
            var errorTagSpan = new TagSpan<IErrorTag>(snapshotSpan, errorTag);

            return errorTagSpan;
        }
    }
}