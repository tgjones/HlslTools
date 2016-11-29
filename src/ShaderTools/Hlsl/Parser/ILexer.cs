using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Hlsl.Parser
{
    public interface ILexer
    {
        SourceText Text { get; }

        SyntaxToken Lex(LexerMode mode);
    }
}