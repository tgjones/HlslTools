using ShaderTools.Core.Diagnostics;
using System.Collections.Immutable;

namespace ShaderTools.Core.Parser
{
    internal abstract class PretokenizedSyntaxNode
    {
        /// <summary>
        /// An integer representing the language specific kind of this token.
        /// </summary>
        public ushort RawKind { get; }

        /// <summary>
        /// The complete width of the token in characters including its leading and trailing trivia.
        /// </summary>
        public int FullWidth { get; protected set; }

        public int Width => Text.Length;

        public string Text { get; }

        public ImmutableArray<PretokenizedDiagnostic> Diagnostics { get; }

        protected PretokenizedSyntaxNode(
            ushort rawKind,
            string text,
            ImmutableArray<PretokenizedDiagnostic> diagnostics)
        {
            RawKind = rawKind;
            Text = text;
            Diagnostics = diagnostics;
        }
    }
}
