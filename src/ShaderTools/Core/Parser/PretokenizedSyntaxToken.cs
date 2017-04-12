using ShaderTools.Core.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ShaderTools.Core.Parser
{
    internal sealed class PretokenizedSyntaxToken : PretokenizedSyntaxNode
    {
        /// <summary>
        /// The kind of token, given its position in the syntax. This differs from <see
        /// cref="RawKind"/> when a contextual keyword is used in a place in the syntax that gives it
        /// its keyword meaning.
        /// </summary>
        /// <remarks>
        /// The ContextualKind is relevant only on contextual keyword tokens. ContextualKind differs
        /// from Kind when a token is used in context where the token should be interpreted as a
        /// keyword.
        /// </remarks>
        public ushort RawContextualKind { get; }

        public object Value { get; }

        public string ValueText => Value as string ?? Text;

        public ImmutableArray<PretokenizedSyntaxTrivia> LeadingTrivia { get; }
        public int LeadingTriviaWidth => LeadingTrivia.Sum(x => x.FullWidth);

        public ImmutableArray<PretokenizedSyntaxTrivia> TrailingTrivia { get; }
        public int TrailingTriviaWidth => TrailingTrivia.Sum(x => x.FullWidth);

        public PretokenizedSyntaxToken(
            ushort rawKind,
            ushort rawContextualKind,
            string text, 
            object value,
            ImmutableArray<PretokenizedSyntaxTrivia> leadingTrivia,
            ImmutableArray<PretokenizedSyntaxTrivia> trailingTrivia,
            ImmutableArray<PretokenizedDiagnostic> diagnostics)
            : base(rawKind, text, diagnostics)
        {
            RawContextualKind = rawContextualKind;
            Value = value;

            LeadingTrivia = leadingTrivia;
            TrailingTrivia = trailingTrivia;

            FullWidth = LeadingTriviaWidth + text.Length + TrailingTriviaWidth;
        }
    }
}
