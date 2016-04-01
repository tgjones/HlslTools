using System;
using System.Runtime.CompilerServices;
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
        private static readonly ConditionalWeakTable<ITextSnapshot, SyntaxTree> CachedSyntaxTrees = new ConditionalWeakTable<ITextSnapshot, SyntaxTree>();
        private static readonly ConditionalWeakTable<ITextSnapshot, SemanticModel> CachedSemanticModels = new ConditionalWeakTable<ITextSnapshot, SemanticModel>();

        private static readonly object TextContainerKey = new object();
        private static readonly object IncludeFileSystemKey = new object();
        private static readonly object BackgroundParserKey = new object();

        public static VisualStudioSourceTextContainer GetTextContainer(this ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(TextContainerKey,
                () => new VisualStudioSourceTextContainer(textBuffer));
        }

        public static IIncludeFileSystem GetIncludeFileSystem(this ITextBuffer textBuffer, VisualStudioSourceTextFactory sourceTextFactory)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(IncludeFileSystemKey,
                () => new VisualStudioFileSystem(textBuffer.GetTextContainer(), sourceTextFactory));
        }

        public static BackgroundParser GetBackgroundParser(this ITextBuffer textBuffer, VisualStudioSourceTextFactory sourceTextFactory)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(BackgroundParserKey,
                () => new BackgroundParser(textBuffer, sourceTextFactory));
        }

        public static SyntaxTagger GetSyntaxTagger(this ITextBuffer textBuffer)
        {
            return (SyntaxTagger) textBuffer.Properties.GetProperty(typeof(SyntaxTagger));
        }

        public static SyntaxTree GetSyntaxTree(this ITextSnapshot snapshot, VisualStudioSourceTextFactory sourceTextFactory, CancellationToken cancellationToken)
        {
            return CachedSyntaxTrees.GetValue(snapshot, key =>
            {
                var sourceText = key.ToSourceText();

                var options = new ParserOptions();
                options.PreprocessorDefines.Add("__INTELLISENSE__");

                var fileSystem = key.TextBuffer.GetIncludeFileSystem(sourceTextFactory);

                return SyntaxFactory.ParseSyntaxTree(sourceText, options, fileSystem, cancellationToken);
            });
        }

        public static bool TryGetSemanticModel(this ITextSnapshot snapshot, VisualStudioSourceTextFactory sourceTextFactory, CancellationToken cancellationToken, out SemanticModel semanticModel)
        {
            if (!HlslToolsPackage.Instance.Options.AdvancedOptions.EnableIntelliSense)
            {
                semanticModel = null;
                return false;
            }

            semanticModel = CachedSemanticModels.GetValue(snapshot, key =>
            {
                try
                {
                    var syntaxTree = key.GetSyntaxTree(sourceTextFactory, cancellationToken);
                    var compilation = new Compilation.Compilation(syntaxTree);
                    return compilation.GetSemanticModel();
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to create semantic model: {ex}");
                    return null;
                }
            });
            return semanticModel != null;
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