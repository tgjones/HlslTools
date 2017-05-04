using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.ShaderLab.Syntax;
using ShaderTools.Editor.VisualStudio.Core.Text;
using ShaderTools.Editor.VisualStudio.ShaderLab.Parsing;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Util.Extensions
{
    internal static class Extensions
    {
        private static readonly ConditionalWeakTable<ITextSnapshot, SyntaxTree> CachedSyntaxTrees = new ConditionalWeakTable<ITextSnapshot, SyntaxTree>();

        private static readonly object BackgroundParserKey = new object();

        public static BackgroundParser GetBackgroundParser(this ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(BackgroundParserKey,
                () => new BackgroundParser(textBuffer));
        }

        public static SyntaxTree GetSyntaxTree(this ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            return CachedSyntaxTrees.GetValue(snapshot, key =>
            {
                var sourceText = key.ToSourceText();
                return SyntaxFactory.ParseUnitySyntaxTree(sourceText, cancellationToken);
            });
        }
    }
}