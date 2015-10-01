using HlslTools.Syntax;
using HlslTools.Text;

namespace HlslTools.Parser
{
    public interface ILexer
    {
        SourceText Text { get; }

        SyntaxToken Lex(LexerMode mode);
    }
}