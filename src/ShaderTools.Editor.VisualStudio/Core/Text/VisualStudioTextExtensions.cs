using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Core.Text
{
    internal static class VisualStudioTextExtensions
    {
        private static readonly ConditionalWeakTable<ITextSnapshot, SourceText> SnapshotMap = new ConditionalWeakTable<ITextSnapshot, SourceText>();

        public static SourceText ToSourceText(this ITextSnapshot textSnapshot)
        {
            if (textSnapshot == null)
                throw new ArgumentNullException(nameof(textSnapshot));

            return SnapshotMap.GetValue(textSnapshot, ts => new VisualStudioSourceText(ts, ts.TextBuffer.GetTextDocument()?.FilePath, true));
        }

        public static ITextSnapshot ToTextSnapshot(this SourceText text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var visualStudioSourceText = text as VisualStudioSourceText;
            if (visualStudioSourceText == null)
                throw new ArgumentException("The source text didn't originate from a Visual Studio Editor", nameof(text));

            return visualStudioSourceText.Snapshot;
        }
    }
}