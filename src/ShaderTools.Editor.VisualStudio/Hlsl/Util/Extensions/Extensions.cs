using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Parser;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;
using ShaderTools.Editor.VisualStudio.Core.Text;
using ShaderTools.Editor.VisualStudio.Core.Util;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Tagging.Classification;
using ShaderTools.Editor.VisualStudio.Hlsl.Text;
using ShaderTools.Core.Options;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions
{
    internal static class Extensions
    {
        private static readonly ConditionalWeakTable<ITextSnapshot, SyntaxTree> CachedSyntaxTrees = new ConditionalWeakTable<ITextSnapshot, SyntaxTree>();
        private static readonly ConditionalWeakTable<ITextSnapshot, SemanticModel> CachedSemanticModels = new ConditionalWeakTable<ITextSnapshot, SemanticModel>();

        private static readonly object TextContainerKey = new object();
        private static readonly object IncludeFileSystemKey = new object();
        private static readonly object ConfigFileKey = new object();
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

        public static ConfigFile GetConfigFile(this ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(ConfigFileKey,
                () => ConfigFileLoader.LoadAndMergeConfigFile(textBuffer.GetTextDocument()?.FilePath));
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
            return CachedSyntaxTrees.GetValue(snapshot, key =>
            {
                var sourceText = key.ToSourceText();

                var configFile = snapshot.TextBuffer.GetConfigFile();

                var options = new ParserOptions();
                options.PreprocessorDefines.Add("__INTELLISENSE__", "1");

                foreach (var kvp in configFile.HlslPreprocessorDefinitions)
                    options.PreprocessorDefines.Add(kvp.Key, kvp.Value);

                options.AdditionalIncludeDirectories.AddRange(configFile.HlslAdditionalIncludeDirectories);

                var sourceTextFactory = VisualStudioSourceTextFactory.Instance ?? HlslPackage.Instance.AsVsServiceProvider().GetComponentModel().GetService<VisualStudioSourceTextFactory>();
                var fileSystem = (VisualStudioFileSystem) key.TextBuffer.GetIncludeFileSystem(sourceTextFactory);

                return SyntaxFactory.ParseSyntaxTree(sourceText, options, fileSystem, cancellationToken);
            });
        }

        public static bool TryGetSemanticModel(this ITextSnapshot snapshot, CancellationToken cancellationToken, out SemanticModel semanticModel)
        {
            if (HlslPackage.Instance != null && !HlslPackage.Instance.Options.AdvancedOptions.EnableIntelliSense)
            {
                semanticModel = null;
                return false;
            }

            try
            {
                semanticModel = CachedSemanticModels.GetValue(snapshot, key =>
                {
                    try
                    {
                        var syntaxTree = key.GetSyntaxTree(cancellationToken);
                        var compilation = new Compilation(syntaxTree);
                        return compilation.GetSemanticModel(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to create semantic model: {ex}");
                        return null;
                    }
                });
            }
            catch (OperationCanceledException)
            {
                semanticModel = null;
            }

            return semanticModel != null;
        }

        public static int? GetPosition(this ITextView syntaxEditor, ITextSnapshot snapshot)
        {
            var caretPoint = syntaxEditor.Caret.Position.BufferPosition;
            var snapshotPoint = syntaxEditor.BufferGraph.MapDownToSnapshot(caretPoint, PointTrackingMode.Positive, snapshot, PositionAffinity.Successor);
            if (snapshotPoint.HasValue)
                return snapshotPoint.Value.Position;
            return null;
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