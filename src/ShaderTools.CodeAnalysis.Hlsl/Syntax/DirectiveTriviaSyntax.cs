using System.Collections.Generic;
using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Parser;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract class DirectiveTriviaSyntax : StructuredTriviaSyntax
    {
        protected DirectiveTriviaSyntax(SyntaxKind kind, IEnumerable<Diagnostic> diagnostics)
            : base(kind, diagnostics)
        {
        }

        protected DirectiveTriviaSyntax(SyntaxKind kind)
            : base(kind)
        {
        }

        public abstract SyntaxToken HashToken { get; }

        public abstract SyntaxToken EndOfDirectiveToken { get; }

        public abstract bool IsActive { get; }

        internal override DirectiveStack ApplyDirectives(DirectiveStack stack)
        {
            return stack.Add(new Directive(this));
        }
    }

    public sealed class BadDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken Identifier;

        public BadDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken identifier, SyntaxToken endOfDirectiveToken, bool isActive)
            : base(SyntaxKind.BadDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out Identifier, identifier);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        internal BadDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken identifier, SyntaxToken endOfDirectiveToken, bool isActive, ImmutableArray<Diagnostic> diagnostics)
            : base(SyntaxKind.BadDirectiveTrivia, diagnostics)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out Identifier, identifier);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitBadDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitBadDirectiveTrivia(this);
        }

        public override SyntaxNodeBase SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            return new BadDirectiveTriviaSyntax(_hashToken, Identifier, _endOfDirectiveToken, IsActive, diagnostics);
        }
    }

    public abstract class BranchingDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        protected BranchingDirectiveTriviaSyntax(SyntaxKind kind)
            : base(kind)
        {
        }

        public abstract bool BranchTaken { get; }
    }

    public abstract class ConditionalDirectiveTriviaSyntax : BranchingDirectiveTriviaSyntax
    {
        protected ConditionalDirectiveTriviaSyntax(SyntaxKind kind)
            : base(kind)
        {
        }

        public abstract ExpressionSyntax Condition { get; }

        public abstract bool ConditionValue { get; }
    }

    public sealed class IfDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly ExpressionSyntax _condition;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken IfKeyword;

        public IfDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken ifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
            : base(SyntaxKind.IfDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out IfKeyword, ifKeyword);
            RegisterChildNode(out _condition, condition);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
            BranchTaken = branchTaken;
            ConditionValue = conditionValue;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override ExpressionSyntax Condition => _condition;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }
        public override bool BranchTaken { get; }
        public override bool ConditionValue { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitIfDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitIfDirectiveTrivia(this);
        }
    }

    public sealed class IfDefDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken IfDefKeyword;
        public readonly SyntaxToken Name;

        public IfDefDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken ifDefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
            : base(SyntaxKind.IfDefDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out IfDefKeyword, ifDefKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
            BranchTaken = branchTaken;
            ConditionValue = conditionValue;
        }

        public override SyntaxToken HashToken => _hashToken;

        public override ExpressionSyntax Condition => null;

        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }
        public override bool BranchTaken { get; }
        public override bool ConditionValue { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitIfDefDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitIfDefDirectiveTrivia(this);
        }
    }

    public sealed class IfNDefDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken IfNDefKeyword;
        public readonly SyntaxToken Name;

        public IfNDefDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken ifNDefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
            : base(SyntaxKind.IfNDefDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out IfNDefKeyword, ifNDefKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
            BranchTaken = branchTaken;
            ConditionValue = conditionValue;
        }

        public override SyntaxToken HashToken => _hashToken;

        public override ExpressionSyntax Condition => null;

        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }
        public override bool BranchTaken { get; }
        public override bool ConditionValue { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitIfNDefDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitIfNDefDirectiveTrivia(this);
        }
    }

    public sealed class ElifDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly ExpressionSyntax _condition;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken ElifKeyword;

        public ElifDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken elifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
            : base(SyntaxKind.ElifDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out ElifKeyword, elifKeyword);
            RegisterChildNode(out _condition, condition);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
            BranchTaken = branchTaken;
            ConditionValue = conditionValue;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override ExpressionSyntax Condition => _condition;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }
        public override bool BranchTaken { get; }
        public override bool ConditionValue { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitElifDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitElifDirectiveTrivia(this);
        }
    }

    public sealed class ElseDirectiveTriviaSyntax : BranchingDirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken ElseKeyword;

        public ElseDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken elseKeyword, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken)
            : base(SyntaxKind.ElseDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out ElseKeyword, elseKeyword);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
            BranchTaken = branchTaken;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }
        public override bool BranchTaken { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitElseDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitElseDirectiveTrivia(this);
        }
    }

    public sealed class EndIfDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken EndIfKeyword;

        public EndIfDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken endIfKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
            : base(SyntaxKind.EndIfDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out EndIfKeyword, endIfKeyword);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitEndIfDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitEndIfDirectiveTrivia(this);
        }
    }

    public abstract class DefineDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        public abstract SyntaxToken MacroName { get; }
        public abstract List<SyntaxToken> MacroBody { get; }

        protected DefineDirectiveTriviaSyntax(SyntaxKind kind)
            : base(kind)
        {
        }
    }

    public sealed class ObjectLikeDefineDirectiveTriviaSyntax : DefineDirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken DefineKeyword;
        public readonly SyntaxToken Name;

        public readonly List<SyntaxToken> Body;

        public override SyntaxToken MacroName => Name;
        public override List<SyntaxToken> MacroBody => Body;

        public ObjectLikeDefineDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken defineKeyword, SyntaxToken name, List<SyntaxToken> body, SyntaxToken endOfDirectiveToken, bool isActive)
            : base(SyntaxKind.ObjectLikeDefineDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out DefineKeyword, defineKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNodes(out Body, body);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitObjectLikeDefineDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitObjectLikeDefineDirectiveTrivia(this);
        }
    }

    public sealed class FunctionLikeDefineDirectiveTriviaSyntax : DefineDirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken DefineKeyword;
        public readonly SyntaxToken Name;

        public override SyntaxToken MacroName => Name;
        public override List<SyntaxToken> MacroBody => Body;

        public readonly FunctionLikeDefineDirectiveParameterListSyntax Parameters;

        public readonly List<SyntaxToken> Body;

        public FunctionLikeDefineDirectiveTriviaSyntax(
            SyntaxToken hashToken, SyntaxToken defineKeyword, SyntaxToken name,
            FunctionLikeDefineDirectiveParameterListSyntax parameters,
            List<SyntaxToken> body, 
            SyntaxToken endOfDirectiveToken, bool isActive)
            : base(SyntaxKind.FunctionLikeDefineDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out DefineKeyword, defineKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out Parameters, parameters);
            RegisterChildNodes(out Body, body);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitFunctionLikeDefineDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitFunctionLikeDefineDirectiveTrivia(this);
        }
    }

    public sealed class FunctionLikeDefineDirectiveParameterListSyntax : SyntaxNode
    {
        public readonly SyntaxToken OpenParenToken;
        public readonly SeparatedSyntaxList<SyntaxToken> Parameters;
        public readonly SyntaxToken CloseParenToken;

        public FunctionLikeDefineDirectiveParameterListSyntax(SyntaxToken openParenToken, SeparatedSyntaxList<SyntaxToken> parameters, SyntaxToken closeParenToken)
            : base(SyntaxKind.FunctionLikeDefineDirectiveParameterList)
        {
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNodes(out Parameters, parameters);
            RegisterChildNode(out CloseParenToken, closeParenToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitFunctionLikeDefineDirectiveParameterList(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitFunctionLikeDefineDirectiveParameterList(this);
        }
    }

    public sealed class IncludeDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken IncludeKeyword;
        public readonly SyntaxToken Filename;

        public string TrimmedFilename => Filename.Text.TrimStart('<', '"').TrimEnd('>', '"');

        public bool IsLocal => !Filename.Text.StartsWith("<");

        public IncludeDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken includeKeyword, SyntaxToken filename, SyntaxToken endOfDirectiveToken, bool isActive, IEnumerable<Diagnostic> diagnostics)
            : base(SyntaxKind.IncludeDirectiveTrivia, diagnostics)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out IncludeKeyword, includeKeyword);
            RegisterChildNode(out Filename, filename);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        public IncludeDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken includeKeyword, SyntaxToken filename, SyntaxToken endOfDirectiveToken, bool isActive)
            : base(SyntaxKind.IncludeDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out IncludeKeyword, includeKeyword);
            RegisterChildNode(out Filename, filename);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitIncludeDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitIncludeDirectiveTrivia(this);
        }

        public override SyntaxNodeBase SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)
        {
            return new IncludeDirectiveTriviaSyntax(_hashToken, IncludeKeyword, Filename, _endOfDirectiveToken, IsActive, diagnostics);
        }
    }

    public sealed class UndefDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        private readonly SyntaxToken _hashToken;
        private readonly SyntaxToken _endOfDirectiveToken;

        public readonly SyntaxToken UndefKeyword;
        public readonly SyntaxToken Name;

        public UndefDirectiveTriviaSyntax(SyntaxToken hashToken, SyntaxToken undefKeyword, SyntaxToken name, SyntaxToken endOfDirectiveToken, bool isActive)
            : base(SyntaxKind.UndefDirectiveTrivia)
        {
            RegisterChildNode(out _hashToken, hashToken);
            RegisterChildNode(out UndefKeyword, undefKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out _endOfDirectiveToken, endOfDirectiveToken);
            IsActive = isActive;
        }

        public override SyntaxToken HashToken => _hashToken;
        public override SyntaxToken EndOfDirectiveToken => _endOfDirectiveToken;
        public override bool IsActive { get; }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitUndefDirectiveTrivia(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitUndefDirectiveTrivia(this);
        }
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