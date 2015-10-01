using System.Threading;
using HlslTools.Compilation;
using HlslTools.Parser;
using HlslTools.Syntax;
using HlslTools.Text;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Tagging.Classification;
using HlslTools.VisualStudio.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.Util.Extensions
{
    internal static class Extensions
    {
        private static readonly object TextContainerKey = new object();
        private static readonly object IncludeFileSystemKey = new object();
        private static readonly object BackgroundParserKey = new object();

        public static VisualStudioSourceTextContainer GetTextContainer(this ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(TextContainerKey,
                () => new VisualStudioSourceTextContainer(textBuffer));
        }

        public static IIncludeFileSystem GetIncludeFileSystem(this ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(IncludeFileSystemKey,
                () => new VisualStudioFileSystem(textBuffer.GetTextContainer()));
        }

        public static BackgroundParser GetBackgroundParser(this ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(BackgroundParserKey,
                () => new BackgroundParser(textBuffer));
        }

        public static SyntaxTagger GetSyntaxTagger(this ITextBuffer textBuffer)
        {
            return (SyntaxTagger) textBuffer.Properties.GetProperty(typeof(SyntaxTagger));
        }

        public static SyntaxTree GetSyntaxTree(this ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            var sourceText = new VisualStudioSourceText(snapshot);

            var options = new ParserOptions();
            options.PreprocessorDefines.Add("__INTELLISENSE__");

            var fileSystem = snapshot.TextBuffer.GetIncludeFileSystem();

            return SyntaxFactory.ParseSyntaxTree(sourceText, options, fileSystem, cancellationToken);
        }

        public static SemanticModel GetSemanticModel(this ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            var syntaxTree = snapshot.GetSyntaxTree(cancellationToken);
            var compilation = new Compilation.Compilation(syntaxTree);
            return compilation.GetSemanticModel();
        }

        public static int GetPosition(this ITextView syntaxEditor, ITextSnapshot snapshot)
        {
            return syntaxEditor.Caret.Position.BufferPosition.TranslateTo(snapshot, PointTrackingMode.Negative);
        }

        // From https://github.com/dotnet/roslyn/blob/e39a3aeb1185ef0b349cad96a105969423065eac/src/EditorFeatures/Core/Shared/Extensions/ITextViewExtensions.cs#L278
        public static int? GetDesiredIndentation(this ITextView textView, ISmartIndentationService smartIndentService, ITextSnapshotLine line)
        {
            var pointInView = textView.BufferGraph.MapUpToSnapshot(line.Start, PointTrackingMode.Positive, PositionAffinity.Successor, textView.TextSnapshot);

            if (!pointInView.HasValue)
                return null;

            var lineInView = textView.TextSnapshot.GetLineFromPosition(pointInView.Value.Position);
            return smartIndentService.GetDesiredIndentation(textView, lineInView);
        }
    }
}