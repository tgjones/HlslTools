using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Parser
{
    public interface ILexer
    {
        SourceText Text { get; }

        SyntaxToken Lex(LexerMode mode);
    }
}