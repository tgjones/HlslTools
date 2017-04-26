using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class MacroReference
    {
        internal abstract SourceRange SourceRange { get; }
        internal abstract SourceRange FullSourceRange { get; }

        public abstract DefineDirectiveTriviaSyntax DefineDirective { get; }
        public abstract SyntaxToken NameToken { get; }
        public abstract TextSpan Span { get; }

        public abstract IEnumerable<SyntaxNode> OriginalNodes { get; }

        internal abstract MacroReference WithLeadingTrivia(ImmutableArray<SyntaxNode> trivia);

        internal abstract void WriteTo(StringBuilder sb, bool leading, bool trailing, bool includeNonRootFile, bool ignoreMacroReferences);

        public abstract void Accept(SyntaxVisitor visitor);
        public abstract T Accept<T>(SyntaxVisitor<T> visitor);
    }
}