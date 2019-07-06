using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Parser;
using ShaderTools.CodeAnalysis.ShaderLab.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.ShaderLab.Parser
{
    public sealed class UnityLexer
    {
        private readonly ImmutableArray<PretokenizedSyntaxToken> _pretokenizedTokens;
        private int _tokenIndex;
        private SourceLocation _tokenPosition;

        public SourceText Text { get; }

        internal UnityLexer(SourceText text, ImmutableArray<PretokenizedSyntaxToken> pretokenizedTokens)
        {
            Text = text;
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