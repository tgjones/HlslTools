using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed class FunctionLikeMacroReference : MacroReference
    {
        public readonly SyntaxToken OriginalToken;
        public readonly MacroArgumentListSyntax ArgumentList;
        public readonly FunctionLikeDefineDirectiveTriviaSyntax Directive;

        public FunctionLikeMacroReference(SyntaxToken originalToken, MacroArgumentListSyntax argumentList, FunctionLikeDefineDirectiveTriviaSyntax directive)
        {
            OriginalToken = originalToken;
            ArgumentList = argumentList;
            Directive = directive;
        }

        internal override SourceRange SourceRange => SourceRange.Union(OriginalToken.SourceRange, ArgumentList.SourceRange);
        internal override SourceRange FullSourceRange => SourceRange.Union(OriginalToken.FullSourceRange, ArgumentList.FullSourceRange);

        public override DefineDirectiveTriviaSyntax DefineDirective => Directive;
        public override SyntaxToken NameToken => OriginalToken;
        public override SourceFileSpan FileSpan => new SourceFileSpan(OriginalToken.FileSpan.File, TextSpan.FromBounds(OriginalToken.FileSpan.Span.Start, ArgumentList.CloseParenToken.FileSpan.Span.End));

        public override IEnumerable<SyntaxNode> OriginalNodes
        {
            get
            {
                yield return OriginalToken;
                yield return ArgumentList;
            }
        }

        internal override MacroReference WithLeadingTrivia(ImmutableArray<SyntaxNode> trivia)
        {
            return new FunctionLikeMacroReference(OriginalToken.WithLeadingTrivia(trivia), ArgumentList, Directive);
        }

        internal override void WriteTo(StringBuilder sb, bool leading, bool trailing, bool includeNonRootFile, bool ignoreMacroReferences)
        {
            OriginalToken.WriteTo(sb, leading, trailing, includeNonRootFile, ignoreMacroReferences);
            ArgumentList?.WriteTo(sb, leading, trailing, includeNonRootFile, ignoreMacroReferences);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitFunctionLikeMacroReference(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitFunctionLikeMacroReference(this);
        }
    }
}