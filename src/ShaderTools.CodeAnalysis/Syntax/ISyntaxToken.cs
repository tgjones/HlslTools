using System.Collections.Immutable;

namespace ShaderTools.CodeAnalysis.Syntax
{
    public interface ISyntaxToken
    {
        ImmutableArray<SyntaxNodeBase> LeadingTrivia { get; }
        ImmutableArray<SyntaxNodeBase> TrailingTrivia { get; }
    }
}
