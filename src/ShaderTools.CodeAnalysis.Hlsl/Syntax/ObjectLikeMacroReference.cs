using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed class ObjectLikeMacroReference : MacroReference
    {
        public readonly SyntaxToken OriginalToken;
        public readonly ObjectLikeDefineDirectiveTriviaSyntax Directive;

        public ObjectLikeMacroReference(SyntaxToken originalToken, ObjectLikeDefineDirectiveTriviaSyntax directive)
        {
            OriginalToken = originalToken;
            Directive = directive;
        }

        internal override SourceRange SourceRange => OriginalToken.SourceRange;
        internal override SourceRange FullSourceRange => OriginalToken.FullSourceRange;

        public override DefineDirectiveTriviaSyntax DefineDirective => Directive;
        public override SyntaxToken NameToken => OriginalToken;
        public override TextSpan Span => OriginalToken.Span;

        public override IEnumerable<SyntaxNode> OriginalNodes
        {
            get { yield return OriginalToken; }
        }

        internal override MacroReference WithLeadingTrivia(ImmutableArray<SyntaxNode> trivia)
        {
            return new ObjectLikeMacroReference(OriginalToken.WithLeadingTrivia(trivia), Directive);
        }

        internal override void WriteTo(StringBuilder sb, bool leading, bool trailing, bool includeNonRootFile, bool ignoreMacroReferences)
        {
            OriginalToken.WriteTo(sb, leading, trailing, includeNonRootFile, ignoreMacroReferences);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitObjectLikeMacroReference(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitObjectLikeMacroReference(this);
        }
    }
}