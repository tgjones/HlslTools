using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Hlsl.Parser
{
    public interface ILexer
    {
        SourceText Text { get; }

        SyntaxToken Lex(LexerMode mode);
    }
}