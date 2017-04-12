using System.Collections.Immutable;
using ShaderTools.Core.Parser;
using ShaderTools.Core.Syntax;
using ShaderTools.Unity.Syntax;

namespace ShaderTools.Unity.Parser
{
    public sealed class UnityLexer
    {
        private readonly ImmutableArray<PretokenizedSyntaxToken> _pretokenizedTokens;
        private int _tokenIndex;
        private SourceLocation _tokenPosition;

        //public SourceText Text { get; }

        internal UnityLexer(ImmutableArray<PretokenizedSyntaxToken> pretokenizedTokens)
        {
            _pretokenizedTokens = pretokenizedTokens;
        }

        public SyntaxToken Lex()
        {
            var pretokenizedToken = _pretokenizedTokens[_tokenIndex];

            var result = new SyntaxToken(pretokenizedToken, _tokenPosition);

            _tokenIndex++;
            _tokenPosition += pretokenizedToken.FullWidth;

            return result;
        }
    }
}