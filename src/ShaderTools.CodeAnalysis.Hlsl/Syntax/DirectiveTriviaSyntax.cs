using System.Collections.Generic;
using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Parser;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract partial class DirectiveTriviaSyntax : StructuredTriviaSyntax
    {
        internal override DirectiveStack ApplyDirectives(DirectiveStack stack)
        {
            return stack.Add(new Directive(this));
        }
    }

    public sealed partial class IfDefDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
    {
        public override ExpressionSyntax Condition => null;
    }

    public sealed partial class IfNDefDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
    {
        public override ExpressionSyntax Condition => null;
    }

    public sealed partial class IncludeDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        public string TrimmedFilename => Filename.Text.TrimStart('<', '"').TrimEnd('>', '"');
    }

    public sealed class LineDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken LineKeyword;
        public readonly SyntaxToken Line;
        public readonly SyntaxToken File;

        public LineDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken lineKeyword, SyntaxToken line, SyntaxToken file, SyntaxToken endOfDirectiveToken, bool isActive)
            : base(SyntaxKind.LineDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out LineKeyword, lineKeyword);
            RegisterChildNode(out Line, line);
            RegisterChildNode(out File, file);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitLineDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitLineDirectiveTrivia(this);
        }
    }

    public sealed class ErrorDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken ErrorKeyword;
        public readonly List<SyntaxToken> TokenString;

        public ErrorDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken errorKeyword, List<SyntaxToken> tokenString, SyntaxToken endOfDirectiveToken, bool isActive)
            : base(SyntaxKind.ErrorDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out ErrorKeyword, errorKeyword);
            RegisterChildNodes(out TokenString, tokenString);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitErrorDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitErrorDirectiveTrivia(this);
        }
    }

    public sealed class PragmaDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken PragmaKeyword;
        public readonly List<SyntaxToken> TokenString;

        public PragmaDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken pragmaKeyword, List<SyntaxToken> tokenString, SyntaxToken endOfDirectiveToken, bool isActive)
            : base(SyntaxKind.PragmaDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out PragmaKeyword, pragmaKeyword);
            RegisterChildNodes(out TokenString, tokenString);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitPragmaDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitPragmaDirectiveTrivia(this);
        }
    }
}