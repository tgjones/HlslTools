using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Syntax
{
    public interface ISyntaxNode
    {
        SyntaxNodeBase Parent { get; }
    }

    public interface ISyntaxToken : ISyntaxNode
    {
        ImmutableArray<SyntaxNodeBase> LeadingTrivia { get; }
        ImmutableArray<SyntaxNodeBase> TrailingTrivia { get; }

        SourceFileSpan FileSpan { get; }

        SourceRange SourceRange { get; }
        SourceRange FullSourceRange { get; }
    }
}
