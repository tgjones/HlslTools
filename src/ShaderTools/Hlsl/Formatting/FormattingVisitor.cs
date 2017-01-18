using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Hlsl.Formatting
{
    internal sealed class FormattingVisitor : SyntaxWalker
    {
        private readonly SourceText _rootSourceText;
        private readonly List<LocatedNode> _locatedNodes;
        private readonly Dictionary<LocatedNode, int> _locatedNodeIndexLookup;

        private readonly TextSpan _spanToFormat;
        private readonly FormattingOptions _options;

        private readonly List<string> _whitespace = new List<string>();
        private int _indentLevel;

        public SortedList<int, Edit> Edits { get; }

        public FormattingVisitor(SyntaxTree tree, TextSpan spanToFormat, FormattingOptions options)
        {
            _rootSourceText = spanToFormat.SourceText;
            _locatedNodes = tree.Root.GetRootLocatedNodes().ToList();

            _locatedNodeIndexLookup = new Dictionary<LocatedNode, int>();
            for (var i = 0; i < _locatedNodes.Count; i++)
                _locatedNodeIndexLookup[_locatedNodes[i]] = i;

            _spanToFormat = ExpandToIncludeFullLines(spanToFormat);
            _options = options;
            Edits = new SortedList<int, Edit>();
        }

        private TextSpan ExpandToIncludeFullLines(TextSpan span)
        {
            var firstLocatedNodeIndex = _locatedNodes.FindIndex(x => x.Span.Start >= span.Start);
            var previousEndOfLineNode = FindPreviousEndOfLine(firstLocatedNodeIndex);
            var expandedSpan = TextSpan.FromBounds(span.SourceText, previousEndOfLineNode?.Span.End ?? 0, span.End);

            var lastLocatedNodeIndex = _locatedNodes.FindIndex(x => x.Span.End >= span.End);
            var nextEndOfLineNode = FindNextEndOfLine(lastLocatedNodeIndex);
            expandedSpan = TextSpan.FromBounds(span.SourceText, expandedSpan.Start, nextEndOfLineNode?.Span.End ?? _locatedNodes.Last().Span.End);

            return expandedSpan;
        }

        private LocatedNode FindPreviousEndOfLine(int nodeIndex)
        {
            for (var i = nodeIndex - 1; i >= 0; i--)
            {
                switch (_locatedNodes[i].Kind)
                {
                    case SyntaxKind.EndOfLineTrivia:
                        return _locatedNodes[i];
                }
            }
            return null;
        }

        private LocatedNode FindNextEndOfLine(int nodeIndex)
        {
            for (var i = nodeIndex; i < _locatedNodes.Count; i++)
            {
                switch (_locatedNodes[i].Kind)
                {
                    case SyntaxKind.EndOfLineTrivia:
                        return _locatedNodes[i];
                }
            }
            return null;
        }

        public override void Visit(SyntaxNode node)
        {
            // If this node falls completely outside of the range we're interested in,
            // ignore it and move on.
            var textSpan = node.GetTextSpanRoot();

            if (textSpan == TextSpan.None)
                return;

            if (!textSpan.IntersectsWith(_spanToFormat))
                return;

            base.Visit(node);
        }

        public override void VisitArgumentList(ArgumentListSyntax node)
        {
            if (!node.Arguments.Any())
            {
                FormatToken(node.OpenParenToken,
                    _options.Spacing.FunctionCallInsertSpaceAfterFunctionName
                        ? LeadingFormattingOperation.EnsureLeadingWhitespace
                        : LeadingFormattingOperation.RemoveLeadingWhitespace,
                    _options.Spacing.FunctionCallInsertSpaceWithinEmptyArgumentListParentheses
                        ? TrailingFormattingOperation.EnsureTrailingWhitespace
                        : TrailingFormattingOperation.RemoveTrailingWhitespace);
            }
            else
            {
                FormatToken(node.OpenParenToken,
                    _options.Spacing.FunctionCallInsertSpaceAfterFunctionName
                        ? LeadingFormattingOperation.EnsureLeadingWhitespace
                        : LeadingFormattingOperation.RemoveLeadingWhitespace,
                    _options.Spacing.FunctionCallInsertSpaceWithinArgumentListParentheses
                        ? TrailingFormattingOperation.EnsureTrailingWhitespace
                        : TrailingFormattingOperation.RemoveTrailingWhitespace);

                FormatCommaSeparatedSyntaxList(node.Arguments);

                FormatCloseParenToken(node.CloseParenToken, _options.Spacing.FunctionCallInsertSpaceWithinArgumentListParentheses);
            }
        }

        public override void VisitArrayInitializerExpression(ArrayInitializerExpressionSyntax node)
        {
            var forceOpenBraceOnNewline = node.Parent.Kind == SyntaxKind.ArrayInitializerExpression && !OnSameLine((ArrayInitializerExpressionSyntax) node.Parent);
            if (OnSameLine(node))
            {
                FormatToken(node.OpenBraceToken,
                    forceOpenBraceOnNewline
                        ? LeadingFormattingOperation.RemoveLeadingWhitespaceIncludingNewlinesAndEnsureLeadingNewline
                        : LeadingFormattingOperation.EnsureLeadingWhitespace,
                    _options.Spacing.InsertSpacesWithinArrayInitializerBraces
                        ? TrailingFormattingOperation.EnsureTrailingWhitespace
                        : TrailingFormattingOperation.RemoveTrailingWhitespace);

                FormatCommaSeparatedSyntaxList(node.Elements);

                FormatToken(node.CloseBraceToken,
                    _options.Spacing.InsertSpacesWithinArrayInitializerBraces
                        ? LeadingFormattingOperation.EnsureLeadingWhitespace
                        : LeadingFormattingOperation.RemoveLeadingWhitespace,
                    TrailingFormattingOperation.RemoveTrailingWhitespace);
            }
            else
            {
                FormatOpenBraceToken(node.OpenBraceToken, forceOpenBraceOnNewline || _options.NewLines.PlaceOpenBraceOnNewLineForArrayInitializers);

                Indent();

                if (node.Elements.Any() && node.Elements.First().Kind != SyntaxKind.ArrayInitializerExpression)
                    FormatToken(node.Elements.First().GetFirstTokenInDescendants(), LeadingFormattingOperation.EnsureLeadingNewline);

                FormatCommaSeparatedSyntaxList(node.Elements);

                Dedent();

                FormatCloseBraceToken(node.CloseBraceToken);
            }
        }

        private bool OnSameLine(ArrayInitializerExpressionSyntax node)
        {
            return OnSameLine(node.OpenBraceToken, node.CloseBraceToken);
        }

        public override void VisitArrayRankSpecifier(ArrayRankSpecifierSyntax node)
        {
            FormatToken(node.OpenBracketToken,
                _options.Spacing.InsertSpaceBeforeOpenSquareBracket
                    ? LeadingFormattingOperation.EnsureLeadingWhitespace
                    : LeadingFormattingOperation.RemoveLeadingWhitespace);

            if (node.Dimension == null)
            {
                FormatToken(node.OpenBracketToken,
                    _options.Spacing.InsertSpaceBeforeOpenSquareBracket
                        ? LeadingFormattingOperation.EnsureLeadingWhitespace
                        : LeadingFormattingOperation.RemoveLeadingWhitespace,
                    _options.Spacing.InsertSpaceWithinEmptySquareBrackets
                        ? TrailingFormattingOperation.EnsureTrailingWhitespace
                        : TrailingFormattingOperation.RemoveTrailingWhitespace);

                FormatToken(node.CloseBracketToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
            }
            else
            {
                FormatToken(node.OpenBracketToken,
                    _options.Spacing.InsertSpaceBeforeOpenSquareBracket
                        ? LeadingFormattingOperation.EnsureLeadingWhitespace
                        : LeadingFormattingOperation.RemoveLeadingWhitespace,
                    _options.Spacing.InsertSpacesWithinSquareBrackets
                        ? TrailingFormattingOperation.EnsureTrailingWhitespace
                        : TrailingFormattingOperation.RemoveTrailingWhitespace);

                Visit(node.Dimension);

                FormatToken(node.CloseBracketToken,
                    _options.Spacing.InsertSpacesWithinSquareBrackets
                        ? LeadingFormattingOperation.EnsureLeadingWhitespace
                        : LeadingFormattingOperation.RemoveLeadingWhitespace,
                    TrailingFormattingOperation.RemoveTrailingWhitespace);
            }
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            Visit(node.Left);

            FormatToken(node.OperatorToken, LeadingFormattingOperation.EnsureLeadingWhitespace, TrailingFormattingOperation.EnsureTrailingWhitespace);

            if (node.LessThanToken != null)
                FormatToken(node.LessThanToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.Right);

            if (node.GreaterThanToken != null)
                FormatToken(node.GreaterThanToken, LeadingFormattingOperation.RemoveLeadingWhitespace);
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            FormatToken(node.OpenBracketToken, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.Name);

            Visit(node.ArgumentList);

            FormatToken(node.CloseBracketToken, LeadingFormattingOperation.RemoveLeadingWhitespace, TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitAttributeArgumentList(AttributeArgumentListSyntax node)
        {
            FormatToken(node.OpenParenToken,
                _options.Spacing.FunctionCallInsertSpaceAfterFunctionName
                    ? LeadingFormattingOperation.EnsureLeadingWhitespace
                    : LeadingFormattingOperation.RemoveLeadingWhitespace,
                _options.Spacing.FunctionCallInsertSpaceWithinArgumentListParentheses
                    ? TrailingFormattingOperation.EnsureTrailingWhitespace
                    : TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatCommaSeparatedSyntaxList(node.Arguments);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.FunctionCallInsertSpaceWithinArgumentListParentheses);
        }

        public override void VisitAnnotations(AnnotationsSyntax node)
        {
            FormatToken(node.LessThanToken, LeadingFormattingOperation.EnsureLeadingWhitespace);

            //node.Annotations

            FormatToken(node.GreaterThanToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitBadDirectiveTrivia(BadDirectiveTriviaSyntax node)
        {
            
        }

        public override void VisitBaseList(BaseListSyntax node)
        {
            FormatColonToken(node.ColonToken,
                _options.Spacing.InsertSpaceBeforeColonForBaseOrInterfaceInTypeDeclaration,
                _options.Spacing.InsertSpaceAfterColonForBaseOrInterfaceInTypeDeclaration);

            Visit(node.BaseType);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            Visit(node.Left);

            switch (_options.Spacing.BinaryOperatorSpaces)
            {
                case BinaryOperatorSpaces.RemoveSpaces:
                    FormatToken(node.OperatorToken, LeadingFormattingOperation.RemoveLeadingWhitespace, TrailingFormattingOperation.RemoveTrailingWhitespace);
                    break;

                case BinaryOperatorSpaces.InsertSpaces:
                    FormatToken(node.OperatorToken, LeadingFormattingOperation.EnsureLeadingWhitespace, TrailingFormattingOperation.EnsureTrailingWhitespace);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Visit(node.Right);
        }

        public override void VisitBlock(BlockSyntax node)
        {
            if (_options.Indentation.IndentOpenAndCloseBraces)
                Indent();

            if (node.Parent != null)
            {
                switch (node.Parent.Kind)
                {
                    case SyntaxKind.FunctionDefinition:
                        FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForFunctions);
                        break;

                    case SyntaxKind.ForStatement:
                    case SyntaxKind.IfStatement:
                    case SyntaxKind.ElseClause:
                    case SyntaxKind.WhileStatement:
                    case SyntaxKind.DoStatement:
                        FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForControlBlocks);
                        break;

                    default:
                        FormatOpenBraceToken(node.OpenBraceToken, true);
                        break;
                }
            }

            if (_options.Indentation.IndentOpenAndCloseBraces)
                Dedent();

            if (_options.Indentation.IndentBlockContents)
                Indent();

            foreach (var statement in node.Statements)
                Visit(statement);

            if (_options.Indentation.IndentBlockContents)
                Dedent();

            if (_options.Indentation.IndentOpenAndCloseBraces)
                Indent();

            FormatCloseBraceToken(node.CloseBraceToken);

            if (_options.Indentation.IndentOpenAndCloseBraces)
                Dedent();
        }

        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            FormatToken(node.BreakKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatSemicolonToken(node.SemicolonToken);
        }

        public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            FormatToken(node.CaseKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingWhitespace);

            Visit(node.Value);

            FormatToken(node.ColonToken, LeadingFormattingOperation.RemoveLeadingWhitespace, TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitClassType(ClassTypeSyntax node)
        {
            FormatToken(node.ClassKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatToken(node.Name, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            if (node.BaseList != null)
                Visit(node.BaseList);

            FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForTypes);

            Indent();

            foreach (var member in node.Members)
                Visit(member);

            Dedent();

            FormatCloseBraceToken(node.CloseBraceToken);
        }

        public override void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            foreach (var declaration in node.Declarations)
                Visit(declaration);

            FormatToken(node.EndOfFileToken);
        }

        public override void VisitCompileExpression(CompileExpressionSyntax node)
        {
            FormatToken(node.CompileKeyword, trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatToken(node.ShaderTargetToken, trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            Visit(node.ShaderFunction);
        }

        public override void VisitCompoundExpression(CompoundExpressionSyntax node)
        {
            Visit(node.Left);

            FormatCommaToken(node.CommaToken);

            Visit(node.Right);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Visit(node.Condition);

            FormatToken(node.QuestionToken, LeadingFormattingOperation.EnsureLeadingWhitespace, TrailingFormattingOperation.EnsureTrailingWhitespace);

            Visit(node.WhenTrue);

            FormatToken(node.ColonToken, LeadingFormattingOperation.EnsureLeadingWhitespace, TrailingFormattingOperation.EnsureTrailingWhitespace);

            Visit(node.WhenFalse);
        }

        public override void VisitConstantBuffer(ConstantBufferSyntax node)
        {
            FormatToken(node.ConstantBufferKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatToken(node.Name, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            if (node.Register != null)
                Visit(node.Register);

            FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForTypes);

            Indent();

            foreach (var declaration in node.Declarations)
                Visit(declaration);

            Dedent();

            FormatCloseBraceToken(node.CloseBraceToken);
        }

        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            FormatToken(node.ContinueKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatSemicolonToken(node.SemicolonToken);
        }

        public override void VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
        {
            FormatToken(node.DefaultKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatToken(node.ColonToken, LeadingFormattingOperation.RemoveLeadingWhitespace, TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitDiscardStatement(DiscardStatementSyntax node)
        {
            FormatToken(node.DiscardKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatSemicolonToken(node.SemicolonToken);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            FormatToken(node.DoKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.RemoveTrailingWhitespace);

            VisitStatement(node.Statement);

            FormatToken(node.WhileKeyword, LeadingFormattingOperation.EnsureLeadingWhitespace, TrailingFormattingOperation.EnsureTrailingControlFlowWhitespace);

            FormatOpenParenToken(node.OpenParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfControlFlowStatements);

            Visit(node.Condition);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfControlFlowStatements);

            FormatSemicolonToken(node.SemicolonToken);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            Visit(node.Expression);

            FormatToken(node.OpenBracketToken,
                _options.Spacing.InsertSpaceBeforeOpenSquareBracket
                    ? LeadingFormattingOperation.EnsureLeadingWhitespace
                    : LeadingFormattingOperation.RemoveLeadingWhitespace,
                _options.Spacing.InsertSpacesWithinSquareBrackets
                    ? TrailingFormattingOperation.EnsureTrailingWhitespace
                    : TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.Index);

            FormatToken(node.CloseBracketToken,
                _options.Spacing.InsertSpacesWithinSquareBrackets
                    ? LeadingFormattingOperation.EnsureLeadingWhitespace
                    : LeadingFormattingOperation.RemoveLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitElseClause(ElseClauseSyntax node)
        {
            FormatToken(node.ElseKeyword,
                _options.NewLines.PlaceElseOnNewLine
                    ? LeadingFormattingOperation.RemoveLeadingWhitespaceIncludingNewlinesAndEnsureLeadingNewline
                    : LeadingFormattingOperation.RemoveLeadingWhitespaceIncludingNewlinesAndEnsureLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);

            VisitStatement(node.Statement);
        }

        public override void VisitEmptyStatement(EmptyStatementSyntax node)
        {
            FormatToken(node.SemicolonToken,
                LeadingFormattingOperation.RemoveLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            FormatToken(node.EqualsToken, LeadingFormattingOperation.EnsureLeadingWhitespace, TrailingFormattingOperation.EnsureTrailingWhitespace);

            Visit(node.Value);
        }

        public override void VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            FormatToken(node.Expression.GetFirstTokenInDescendants(), LeadingFormattingOperation.EnsureLeadingNewline);

            Visit(node.Expression);

            FormatSemicolonToken(node.SemicolonToken);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            FormatToken(node.ForKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingControlFlowWhitespace);

            FormatOpenParenToken(node.OpenParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfControlFlowStatements);

            if (node.Declaration != null)
                Visit(node.Declaration);

            if (node.Initializer != null)
                Visit(node.Initializer);

            FormatToken(node.FirstSemicolonToken,
                _options.Spacing.InsertSpaceBeforeSemicolonInForStatement
                    ? LeadingFormattingOperation.EnsureLeadingWhitespace
                    : LeadingFormattingOperation.RemoveLeadingWhitespace,
                _options.Spacing.InsertSpaceAfterSemicolonInForStatement
                    ? TrailingFormattingOperation.EnsureTrailingWhitespace
                    : TrailingFormattingOperation.RemoveTrailingWhitespace);

            if (node.Condition != null)
                Visit(node.Condition);

            FormatToken(node.SecondSemicolonToken,
                _options.Spacing.InsertSpaceBeforeSemicolonInForStatement
                    ? LeadingFormattingOperation.EnsureLeadingWhitespace
                    : LeadingFormattingOperation.RemoveLeadingWhitespace,
                _options.Spacing.InsertSpaceAfterSemicolonInForStatement
                    ? TrailingFormattingOperation.EnsureTrailingWhitespace
                    : TrailingFormattingOperation.RemoveTrailingWhitespace);

            if (node.Incrementor != null)
                Visit(node.Incrementor);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfControlFlowStatements);

            VisitStatement(node.Statement);
        }

        public override void VisitFunctionDeclaration(FunctionDeclarationSyntax node)
        {
            FormatToken(node.GetFirstTokenInDescendants(), LeadingFormattingOperation.EnsureLeadingNewline);

            foreach (var attribute in node.Attributes)
                Visit(attribute);

            Visit(node.ReturnType);

            Visit(node.Name);

            Visit(node.ParameterList);

            if (node.Semantic != null)
                Visit(node.Semantic);

            FormatSemicolonToken(node.SemicolonToken);
        }

        public override void VisitFunctionDefinition(FunctionDefinitionSyntax node)
        {
            FormatToken(node.GetFirstTokenInDescendants(), LeadingFormattingOperation.EnsureLeadingNewline);

            foreach (var attribute in node.Attributes)
                Visit(attribute);

            Visit(node.ReturnType);

            Visit(node.Name);

            Visit(node.ParameterList);

            if (node.Semantic != null)
                Visit(node.Semantic);

            Visit(node.Body);
        }

        public override void VisitFunctionInvocationExpression(FunctionInvocationExpressionSyntax node)
        {
            Visit(node.Name);

            Visit(node.ArgumentList);
        }

        public override void VisitFunctionLikeDefineDirectiveParameterList(FunctionLikeDefineDirectiveParameterListSyntax node)
        {
            FormatOpenParenToken(node.OpenParenToken, _options.Spacing.FunctionDeclarationInsertSpaceWithinArgumentListParentheses);

            FormatCommaSeparatedSyntaxList(node.Parameters);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.FunctionDeclarationInsertSpaceWithinArgumentListParentheses);
        }

        public override void VisitFunctionLikeDefineDirectiveTrivia(FunctionLikeDefineDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitGenericMatrixType(GenericMatrixTypeSyntax node)
        {
            FormatToken(node.MatrixKeyword, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatToken(node.LessThanToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.ScalarType);

            FormatToken(node.FirstCommaToken, trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatToken(node.RowsToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatToken(node.SecondCommaToken, trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatToken(node.ColsToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatToken(node.GreaterThanToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitGenericVectorType(GenericVectorTypeSyntax node)
        {
            FormatToken(node.VectorKeyword, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatToken(node.LessThanToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.ScalarType);

            FormatToken(node.CommaToken, trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatToken(node.SizeToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatToken(node.GreaterThanToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitIdentifierDeclarationName(IdentifierDeclarationNameSyntax node)
        {
            FormatToken(node.Name,
                LeadingFormattingOperation.EnsureLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            FormatToken(node.Name, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitIfDefDirectiveTrivia(IfDefDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitIfNDefDirectiveTrivia(IfNDefDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            FormatToken(node.IfKeyword,
                (node.Parent.Kind == SyntaxKind.ElseClause
                    ? LeadingFormattingOperation.RemoveLeadingWhitespaceIncludingNewlinesAndEnsureLeadingWhitespace
                    : LeadingFormattingOperation.EnsureLeadingNewline),
                TrailingFormattingOperation.EnsureTrailingControlFlowWhitespace);

            FormatOpenParenToken(node.OpenParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfControlFlowStatements);

            Visit(node.Condition);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfControlFlowStatements);

            VisitStatement(node.Statement);

            if (node.Else != null)
                Visit(node.Else);
        }

        public override void VisitIncludeDirectiveTrivia(IncludeDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitInterfaceType(InterfaceTypeSyntax node)
        {
            FormatToken(node.InterfaceKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingWhitespace);
            
            FormatToken(node.Name, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForTypes);

            Indent();

            foreach (var method in node.Methods)
                Visit(method);

            Dedent();

            FormatCloseBraceToken(node.CloseBraceToken);
        }

        public override void VisitLineDirectiveTrivia(LineDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            FormatToken(node.Token, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitMacroArgument(MacroArgumentSyntax node)
        {
            
        }

        public override void VisitMacroArgumentList(MacroArgumentListSyntax node)
        {
            FormatToken(node.OpenParenToken,
                    _options.Spacing.FunctionCallInsertSpaceAfterFunctionName
                        ? LeadingFormattingOperation.EnsureLeadingWhitespace
                        : LeadingFormattingOperation.RemoveLeadingWhitespace,
                    _options.Spacing.FunctionCallInsertSpaceWithinArgumentListParentheses
                        ? TrailingFormattingOperation.EnsureTrailingWhitespace
                        : TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatCommaSeparatedSyntaxList(node.Arguments);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.FunctionCallInsertSpaceWithinArgumentListParentheses);
        }

        public override void VisitMatrixType(MatrixTypeSyntax node)
        {
            FormatToken(node.TypeToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitFieldAccess(FieldAccessExpressionSyntax node)
        {
            Visit(node.Name);

            FormatDotToken(node.DotToken);

            Visit(node.Expression);
        }

        public override void VisitMethodInvocationExpression(MethodInvocationExpressionSyntax node)
        {
            Visit(node.Target);

            FormatDotToken(node.DotToken);

            FormatToken(node.Name, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.ArgumentList);
        }

        public override void VisitNamespace(NamespaceSyntax node)
        {
            FormatToken(node.NamespaceKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatToken(node.Name, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForTypes);

            Indent();

            foreach (var declaration in node.Declarations)
                Visit(declaration);

            Dedent();

            FormatCloseBraceToken(node.CloseBraceToken);
        }

        public override void VisitNumericConstructorInvocation(NumericConstructorInvocationExpressionSyntax node)
        {
            Visit(node.Type);

            Visit(node.ArgumentList);
        }

        public override void VisitObjectLikeDefineDirectiveTrivia(ObjectLikeDefineDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitPackOffsetComponentPart(PackOffsetComponentPart node)
        {
            FormatDotToken(node.DotToken);

            FormatToken(node.Component, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitPackOffsetLocation(PackOffsetLocation node)
        {
            FormatColonToken(node.ColonToken,
                _options.Spacing.InsertSpaceBeforeColonForSemanticOrRegisterOrPackOffset,
                _options.Spacing.InsertSpaceAfterColonForSemanticOrRegisterOrPackOffset);

            FormatToken(node.PackOffsetKeyword, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatOpenParenToken(node.OpenParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfRegisterOrPackOffsetQualifiers);

            FormatToken(node.Register, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            if (node.ComponentPart != null)
                Visit(node.ComponentPart);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfRegisterOrPackOffsetQualifiers);
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            foreach (var modifier in node.Modifiers)
                FormatToken(modifier, trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            Visit(node.Type);

            Visit(node.Declarator);
        }

        public override void VisitParameterList(ParameterListSyntax node)
        {
            if (!node.Parameters.Any())
            {
                FormatToken(node.OpenParenToken,
                    _options.Spacing.FunctionDeclarationInsertSpaceAfterFunctionName
                        ? LeadingFormattingOperation.EnsureLeadingWhitespace
                        : LeadingFormattingOperation.RemoveLeadingWhitespace,
                    _options.Spacing.FunctionDeclarationInsertSpaceWithinEmptyArgumentListParentheses
                        ? TrailingFormattingOperation.EnsureTrailingWhitespace
                        : TrailingFormattingOperation.RemoveTrailingWhitespace);

                FormatCommaSeparatedSyntaxList(node.Parameters);

                FormatToken(node.CloseParenToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
            }
            else
            {
                FormatToken(node.OpenParenToken,
                    _options.Spacing.FunctionDeclarationInsertSpaceAfterFunctionName
                        ? LeadingFormattingOperation.EnsureLeadingWhitespace
                        : LeadingFormattingOperation.RemoveLeadingWhitespace,
                    _options.Spacing.FunctionDeclarationInsertSpaceWithinArgumentListParentheses
                        ? TrailingFormattingOperation.EnsureTrailingWhitespace
                        : TrailingFormattingOperation.RemoveTrailingWhitespace);

                FormatCommaSeparatedSyntaxList(node.Parameters);

                FormatCloseParenToken(node.CloseParenToken, _options.Spacing.FunctionDeclarationInsertSpaceWithinArgumentListParentheses);
            }
        }

        public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            FormatOpenParenToken(node.OpenParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfExpressions);

            Visit(node.Expression);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfExpressions);
        }

        public override void VisitPass(PassSyntax node)
        {
            FormatToken(node.PassKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingWhitespace);

            if (node.Name != null)
                FormatToken(node.Name, trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForTechniquesAndPasses);

            Indent();

            foreach (var statement in node.Statements)
                Visit(statement);

            Dedent();

            FormatCloseBraceToken(node.CloseBraceToken);
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            Visit(node.Operand);

            FormatToken(node.OperatorToken, LeadingFormattingOperation.RemoveLeadingWhitespace, TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitPredefinedObjectType(PredefinedObjectTypeSyntax node)
        {
            FormatToken(node.ObjectTypeToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            if (node.TemplateArgumentList != null)
                Visit(node.TemplateArgumentList);
        }

        public override void VisitPrefixCastExpression(CastExpressionSyntax node)
        {
            FormatOpenParenToken(node.OpenParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfTypeCasts);

            Visit(node.Type);

            foreach (var arrayRankSpecifier in node.ArrayRankSpecifiers)
                Visit(arrayRankSpecifier);

            FormatToken(node.CloseParenToken,
                _options.Spacing.InsertSpacesWithinParenthesesOfTypeCasts
                ? LeadingFormattingOperation.EnsureLeadingWhitespace
                : LeadingFormattingOperation.RemoveLeadingWhitespace,
                _options.Spacing.InsertSpaceAfterCast
                ? TrailingFormattingOperation.EnsureTrailingWhitespace
                : TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.Expression);
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            FormatToken(node.OperatorToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.Operand);
        }

        public override void VisitQualifiedDeclarationName(QualifiedDeclarationNameSyntax node)
        {
            Visit(node.Left);

            FormatToken(node.ColonColonToken,
                LeadingFormattingOperation.RemoveLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatToken(node.Right.Name,
                LeadingFormattingOperation.RemoveLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitQualifiedName(QualifiedNameSyntax node)
        {
            Visit(node.Left);

            FormatToken(node.ColonColonToken,
                LeadingFormattingOperation.RemoveLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatToken(node.Right.Name,
                LeadingFormattingOperation.RemoveLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitRegisterLocation(RegisterLocation node)
        {
            FormatColonToken(node.Colon,
                _options.Spacing.InsertSpaceBeforeColonForSemanticOrRegisterOrPackOffset,
                _options.Spacing.InsertSpaceAfterColonForSemanticOrRegisterOrPackOffset);

            FormatToken(node.RegisterKeyword, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatOpenParenToken(node.OpenParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfRegisterOrPackOffsetQualifiers);

            FormatToken(node.Register, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            if (node.LogicalRegisterSpace != null)
                Visit(node.LogicalRegisterSpace);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfRegisterOrPackOffsetQualifiers);
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            FormatToken(node.ReturnKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingWhitespace);

            if (node.Expression != null)
                Visit(node.Expression);

            FormatSemicolonToken(node.SemicolonToken);
        }

        public override void VisitLogicalRegisterSpace(LogicalRegisterSpace node)
        {
            FormatCommaToken(node.CommaToken);

            FormatToken(node.SpaceToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitSamplerStateInitializer(SamplerStateInitializerSyntax node)
        {
            FormatToken(node.EqualsToken, LeadingFormattingOperation.EnsureLeadingWhitespace, TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatToken(node.SamplerStateKeyword, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.StateInitializer);
        }

        public override void VisitScalarType(ScalarTypeSyntax node)
        {
            for (var i = 0; i < node.TypeTokens.Count - 1; i++)
                FormatToken(node.TypeTokens[i], trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            if (node.TypeTokens.Any())
                FormatToken(node.TypeTokens.Last(), trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitSemantic(SemanticSyntax node)
        {
            FormatColonToken(node.ColonToken,
                _options.Spacing.InsertSpaceBeforeColonForSemanticOrRegisterOrPackOffset,
                _options.Spacing.InsertSpaceAfterColonForSemanticOrRegisterOrPackOffset);

            FormatToken(node.Semantic, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitSkippedTokensSyntaxTrivia(SkippedTokensTriviaSyntax node)
        {
            
        }

        public override void VisitStateArrayInitializer(StateArrayInitializerSyntax node)
        {
            FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForStateBlocks);

            Indent();

            foreach (var initializer in node.Initializers)
                Visit(initializer);

            Dedent();

            FormatCloseBraceToken(node.CloseBraceToken);
        }

        public override void VisitStateInitializer(StateInitializerSyntax node)
        {
            FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForStateBlocks);

            Indent();

            foreach (var property in node.Properties)
                Visit(property);

            Dedent();

            FormatCloseBraceToken(node.CloseBraceToken);
        }

        public override void VisitStateProperty(StatePropertySyntax node)
        {
            FormatToken(node.Name, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.RemoveTrailingWhitespace);

            if (node.ArrayRankSpecifier != null)
                Visit(node.ArrayRankSpecifier);

            FormatToken(node.EqualsToken, LeadingFormattingOperation.EnsureLeadingWhitespace, TrailingFormattingOperation.EnsureTrailingWhitespace);

            if (node.LessThanToken != null)
                FormatToken(node.LessThanToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.Value);

            if (node.GreaterThanToken != null)
                FormatToken(node.GreaterThanToken, LeadingFormattingOperation.RemoveLeadingWhitespace, TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatSemicolonToken(node.SemicolonToken);
        }

        public override void VisitStringLiteralExpression(StringLiteralExpressionSyntax node)
        {
            
        }

        public override void VisitStructType(StructTypeSyntax node)
        {
            FormatToken(node.StructKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatToken(node.Name, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForTypes);

            Indent();

            foreach (var member in node.Members)
                Visit(member);

            Dedent();

            FormatCloseBraceToken(node.CloseBraceToken);
        }

        public override void VisitSwitchSection(SwitchSectionSyntax node)
        {
            if (_options.Indentation.IndentCaseLabels)
                Indent();

            foreach (var label in node.Labels)
                Visit(label);

            if (_options.Indentation.IndentCaseContents)
                Indent();

            foreach (var statement in node.Statements)
                Visit(statement);

            if (_options.Indentation.IndentCaseContents)
                Dedent();

            if (_options.Indentation.IndentCaseLabels)
                Dedent();
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            FormatToken(node.SwitchKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingControlFlowWhitespace);

            FormatOpenParenToken(node.OpenParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfControlFlowStatements);

            Visit(node.Expression);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfControlFlowStatements);

            FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForControlBlocks);

            foreach (var section in node.Sections)
                Visit(section);

            FormatCloseBraceToken(node.CloseBraceToken);
        }

        public override void VisitSyntaxToken(SyntaxToken node)
        {
            
        }

        public override void VisitSyntaxTrivia(SyntaxTrivia node)
        {
            
        }

        public override void VisitTechnique(TechniqueSyntax node)
        {
            FormatToken(node.TechniqueKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingWhitespace);

            if (node.Name != null)
                FormatToken(node.Name, trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            FormatOpenBraceToken(node.OpenBraceToken, _options.NewLines.PlaceOpenBraceOnNewLineForTechniquesAndPasses);

            Indent();

            foreach (var pass in node.Passes)
                Visit(pass);

            Dedent();

            FormatCloseBraceToken(node.CloseBraceToken);
        }

        public override void VisitTemplateArgumentList(TemplateArgumentListSyntax node)
        {
            FormatToken(node.LessThanToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            FormatCommaSeparatedSyntaxList(node.Arguments);

            FormatToken(node.GreaterThanToken, LeadingFormattingOperation.RemoveLeadingWhitespace, TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitTypeDeclarationStatement(TypeDeclarationStatementSyntax node)
        {
            FormatToken(node.GetFirstTokenInDescendants(), LeadingFormattingOperation.EnsureLeadingNewline);

            foreach (var modifier in node.Modifiers)
                FormatToken(modifier, trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            Visit(node.Type);

            FormatSemicolonToken(node.SemicolonToken);
        }

        public override void VisitUndefDirectiveTrivia(UndefDirectiveTriviaSyntax node)
        {
            AlignDirective(node);
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            foreach (var modifier in node.Modifiers)
                FormatToken(modifier, trailing: TrailingFormattingOperation.EnsureTrailingWhitespace);

            Visit(node.Type);

            FormatCommaSeparatedSyntaxList(node.Variables);
        }

        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            FormatToken(node.Identifier, LeadingFormattingOperation.EnsureLeadingWhitespace, TrailingFormattingOperation.RemoveTrailingWhitespace);

            foreach (var arrayRankSpecifier in node.ArrayRankSpecifiers)
                Visit(arrayRankSpecifier);

            foreach (var qualifier in node.Qualifiers)
                Visit(qualifier);

            if (node.Annotations != null)
                Visit(node.Annotations);

            if (node.Initializer != null)
                Visit(node.Initializer);
        }

        public override void VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node)
        {
            FormatToken(node.GetFirstTokenInDescendants(), LeadingFormattingOperation.EnsureLeadingNewline);

            Visit(node.Declaration);

            FormatSemicolonToken(node.SemicolonToken);
        }

        public override void VisitVectorType(VectorTypeSyntax node)
        {
            FormatToken(node.TypeToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            FormatToken(node.WhileKeyword, LeadingFormattingOperation.EnsureLeadingNewline, TrailingFormattingOperation.EnsureTrailingControlFlowWhitespace);

            FormatOpenParenToken(node.OpenParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfControlFlowStatements);

            Visit(node.Condition);

            FormatCloseParenToken(node.CloseParenToken, _options.Spacing.InsertSpacesWithinParenthesesOfControlFlowStatements);

            VisitStatement(node.Statement);
        }

        public override void VisitObjectLikeMacroReference(ObjectLikeMacroReference node)
        {
            FormatToken(node.OriginalToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        public override void VisitFunctionLikeMacroReference(FunctionLikeMacroReference node)
        {
            FormatToken(node.OriginalToken, trailing: TrailingFormattingOperation.RemoveTrailingWhitespace);

            Visit(node.ArgumentList);
        }

        #region Helpers

        private void VisitStatement(StatementSyntax statement)
        {
            var shouldIndent = statement.Kind != SyntaxKind.Block 
                && (statement.Parent == null || statement.Kind != SyntaxKind.IfStatement || statement.Parent.Kind != SyntaxKind.ElseClause);

            if (shouldIndent)
                Indent();

            Visit(statement);

            if (shouldIndent)
                Dedent();
        }

        private void FormatColonToken(SyntaxToken colonToken, bool spaceBefore, bool spaceAfter)
        {
            Debug.Assert(colonToken.Kind == SyntaxKind.ColonToken);

            FormatToken(colonToken,
                spaceBefore
                    ? LeadingFormattingOperation.EnsureLeadingWhitespace
                    : LeadingFormattingOperation.RemoveLeadingWhitespace,
                spaceAfter
                    ? TrailingFormattingOperation.EnsureTrailingWhitespace
                    : TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        private void FormatCommaToken(SyntaxToken commaToken)
        {
            Debug.Assert(commaToken.Kind == SyntaxKind.CommaToken);

            FormatToken(commaToken,
                _options.Spacing.InsertSpaceBeforeComma
                    ? LeadingFormattingOperation.EnsureLeadingWhitespace
                    : LeadingFormattingOperation.RemoveLeadingWhitespace,
                _options.Spacing.InsertSpaceAfterComma
                    ? TrailingFormattingOperation.EnsureTrailingWhitespace
                    : TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        private void FormatDotToken(SyntaxToken dotToken)
        {
            Debug.Assert(dotToken.Kind == SyntaxKind.DotToken);

            FormatToken(dotToken,
                _options.Spacing.InsertSpaceBeforeDot
                    ? LeadingFormattingOperation.EnsureLeadingWhitespace
                    : LeadingFormattingOperation.RemoveLeadingWhitespace,
                _options.Spacing.InsertSpaceAfterDot
                    ? TrailingFormattingOperation.EnsureTrailingWhitespace
                    : TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        private void FormatOpenBraceToken(SyntaxToken openBraceToken, bool placeOpenBraceOnNewline)
        {
            Debug.Assert(openBraceToken.Kind == SyntaxKind.OpenBraceToken);

            FormatToken(openBraceToken,
                placeOpenBraceOnNewline
                    ? LeadingFormattingOperation.RemoveLeadingWhitespaceIncludingNewlinesAndEnsureLeadingNewline
                    : LeadingFormattingOperation.RemoveLeadingWhitespaceIncludingNewlinesAndEnsureLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        private void FormatCloseBraceToken(SyntaxToken closeBraceToken)
        {
            Debug.Assert(closeBraceToken.Kind == SyntaxKind.CloseBraceToken);

            FormatToken(closeBraceToken, 
                LeadingFormattingOperation.EnsureLeadingNewline,
                TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        private void FormatOpenParenToken(SyntaxToken openParenToken, bool spaceAfter)
        {
            Debug.Assert(openParenToken.Kind == SyntaxKind.OpenParenToken);

            FormatToken(openParenToken, trailing: spaceAfter
                ? TrailingFormattingOperation.EnsureTrailingWhitespace
                : TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        private void FormatCloseParenToken(SyntaxToken closeParenToken, bool spaceBefore)
        {
            Debug.Assert(closeParenToken.Kind == SyntaxKind.CloseParenToken);

            FormatToken(closeParenToken,
                spaceBefore
                    ? LeadingFormattingOperation.EnsureLeadingWhitespace
                    : LeadingFormattingOperation.RemoveLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        private void FormatSemicolonToken(SyntaxToken semiToken)
        {
            Debug.Assert(semiToken.Kind == SyntaxKind.SemiToken);

            FormatToken(semiToken,
                LeadingFormattingOperation.RemoveLeadingWhitespaceIncludingNewlinesAndRemoveLeadingWhitespace,
                TrailingFormattingOperation.RemoveTrailingWhitespace);
        }

        private void FormatCommaSeparatedSyntaxList<T>(SeparatedSyntaxList<T> separatedSyntaxList)
            where T : SyntaxNode
        {
            bool odd = true;
            foreach (var childNode in separatedSyntaxList.GetWithSeparators())
            {
                if (odd)
                    Visit((SyntaxNode) childNode);
                else
                    FormatCommaToken((SyntaxToken) childNode);

                odd = !odd;
            }
        }

        private ReplaceWith GetFlowBraceInsertion(int previousTokenIndex, int openBraceTokenIndex, bool openBracesOnNewLine, bool addSpace)
        {
            // By default we follow the option, but if we have code like:

            // if(x) // comment
            // {

            // does the line have a single line comment?
            var followedByCommentButNotOpenBrace = FollowedBySingleLineComment(previousTokenIndex);

            // does the line have the '{'
            var braceOnFollowingLine = OnSameLine(previousTokenIndex, openBraceTokenIndex);

            // if the brace on new line option is set, or the first line has a comment but not the brace,
            // we want the brace on the following line indented.
            if (openBracesOnNewLine || (followedByCommentButNotOpenBrace && braceOnFollowingLine))
                return ReplaceWith.InsertNewLineAndIndentation;

            return addSpace ? ReplaceWith.InsertSpace : ReplaceWith.RemoveSpace;
        }

        private bool OnSameLine(SyntaxToken left, SyntaxToken right)
        {
            var leftIndex = FindIndex(left);
            if (leftIndex == -1)
                return false;

            var rightIndex = FindIndex(right);
            if (rightIndex == -1)
                return false;

            return OnSameLine(leftIndex, rightIndex);
        }

        private bool OnSameLine(int leftIndex, int rightIndex)
        {
            for (var i = leftIndex; i < rightIndex; i++)
            {
                if (_locatedNodes[i].Kind == SyntaxKind.EndOfLineTrivia)
                    return false;
            }
            return true;
        }

        private bool FollowedBySingleLineComment(int tokenIndex)
        {
            for (var i = tokenIndex + 1; i < _locatedNodes.Count; i++)
            {
                switch (_locatedNodes[i].Kind)
                {
                    case SyntaxKind.WhitespaceTrivia:
                        break;

                    case SyntaxKind.SingleLineCommentTrivia:
                        return true;

                    default:
                        return false;
                }
            }
            return false;
        }

        private bool IsFirstNonWhitespaceOnLine(int tokenIndex)
        {
            for (var i = tokenIndex - 1; i >= 0; i--)
            {
                switch (_locatedNodes[i].Kind)
                {
                    case SyntaxKind.WhitespaceTrivia:
                        break;

                    case SyntaxKind.EndOfLineTrivia:
                        return true;

                    default:
                        return false;
                }
            }

            return true;
        }

        private void ReplacePreceedingWhitespace(int tokenIndex, string whitespace, bool force = false)
        {
            if (!force && IsFirstNonWhitespaceOnLine(tokenIndex))
                return;

            var previousLocatedNode = GetLocatedNode(tokenIndex - 1);
            if (previousLocatedNode == null)
                return;

            if (previousLocatedNode.Kind != SyntaxKind.WhitespaceTrivia)
            {
                MaybeReplaceText(new TextSpan(_rootSourceText, previousLocatedNode.Span.End, 0), whitespace);
                return;
            }

            var whitespaceTrivia = (SyntaxTrivia) previousLocatedNode;
            MaybeReplaceText(whitespaceTrivia.Span, whitespace);
        }

        private void ReplacePreceedingWhitespaceForOpenBraceIncludingNewLines(SyntaxToken openBraceToken, int openBraceTokenIndex, bool placeOpenBraceOnNewLine, bool addSpace)
        {
            var previousTokenIndex = GetPreviousTokenIndex(openBraceTokenIndex);
            if (previousTokenIndex == -1)
                return;

            var braceOnNewLine = GetFlowBraceInsertion(previousTokenIndex, openBraceTokenIndex, placeOpenBraceOnNewLine, addSpace);

            for (var nodeIndex = openBraceTokenIndex - 1; nodeIndex >= 0; nodeIndex--)
            {
                var previousNode = _locatedNodes[nodeIndex];
                switch (previousNode.Kind)
                {
                    case SyntaxKind.EndOfLineTrivia:
                        // new lines are always ok to replace...
                        break;

                    case SyntaxKind.WhitespaceTrivia:
                        // spaces are ok as long as we're not just trying to fix up newlines...
                        break;

                    default:
                        // hit a newline, replace the indentation with new indentation
                        MaybeReplaceText(
                            TextSpan.FromBounds(_rootSourceText, previousNode.Span.End, openBraceToken.Span.Start), 
                            (!OnSameLine(nodeIndex, openBraceTokenIndex) && !previousNode.IsToken)
                                ? GetBraceNewLineFormatting(ReplaceWith.InsertNewLineAndIndentation)
                                : GetBraceNewLineFormatting(braceOnNewLine));
                        return;
                }
            }
            MaybeReplaceText(
                new TextSpan(_rootSourceText, 0, openBraceToken.Span.Start),
                GetBraceNewLineFormatting(braceOnNewLine));
        }

        private string GetBraceNewLineFormatting(ReplaceWith format)
        {
            switch (format)
            {
                case ReplaceWith.InsertNewLineAndIndentation:
                    return Environment.NewLine + GetIndentation();

                case ReplaceWith.InsertSpace:
                    return " ";

                case ReplaceWith.RemoveSpace:
                    return "";

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Replaces immediately following whitespace, and then cleans up all whitespace up
        /// until the next non-whitespace node.
        /// </summary>
        private void ReplaceFollowingWhitespace(SyntaxToken token, int tokenIndex, string whitespace)
        {
            var startIndex = tokenIndex + 1;

            // Possibilities:
            // - {ws}{comment}{ws}{nl}
            // - {ws}{comment}{nl}
            // - {ws}{nl}
            // - {comment}
            // - {non-ws}

            var nextLocatedNode = GetLocatedNode(startIndex);
            if (nextLocatedNode == null)
                return;

            LocatedNode previousNonWhitespaceNode = token;
            TextSpan? previousWhitespaceSpan = null;

            if (nextLocatedNode.Kind == SyntaxKind.WhitespaceTrivia)
            {
                var whitespaceTrivia = (SyntaxTrivia) nextLocatedNode;
                MaybeReplaceText(whitespaceTrivia.Span, whitespace);
                previousWhitespaceSpan = whitespaceTrivia.Span;
                startIndex++;
            }
            else if (!string.IsNullOrEmpty(whitespace))
            {
                MaybeReplaceText(new TextSpan(_rootSourceText, token.Span.End, 0), whitespace);
                previousWhitespaceSpan = new TextSpan(_rootSourceText, token.Span.End, whitespace.Length);
            }

            for (var i = startIndex; i < _locatedNodes.Count; i++)
            {
                nextLocatedNode = _locatedNodes[i];
                switch (nextLocatedNode.Kind)
                {
                    case SyntaxKind.WhitespaceTrivia:
                        MaybeReplaceText(nextLocatedNode.Span, string.Empty);
                        previousWhitespaceSpan = nextLocatedNode.Span;
                        break;

                    case SyntaxKind.SingleLineCommentTrivia:
                        if (previousWhitespaceSpan == null)
                            InsertWhitespace(nextLocatedNode.Span.Start);
                        else if (whitespace == "")
                            MaybeReplaceText(previousWhitespaceSpan.Value, " ");
                        return;

                    case SyntaxKind.MultiLineCommentTrivia:
                        if (previousWhitespaceSpan == null)
                            InsertWhitespace(nextLocatedNode.Span.Start);
                        else if (whitespace == "")
                            MaybeReplaceText(previousWhitespaceSpan.Value, " ");
                        previousNonWhitespaceNode = nextLocatedNode;
                        previousWhitespaceSpan = null;
                        break;

                    case SyntaxKind.EndOfLineTrivia:
                        MaybeReplaceText(TextSpan.FromBounds(_rootSourceText,
                            previousWhitespaceSpan?.Start ?? previousNonWhitespaceNode.Span.End,
                            nextLocatedNode.Span.Start), "");
                        return;

                    default:
                        return;
                }
            }
        }

        private void InsertWhitespace(int position)
        {
            MaybeReplaceText(new TextSpan(_rootSourceText, position, 0), " ");
        }

        private LocatedNode GetLocatedNode(int index)
        {
            if (index < 0 || index >= _locatedNodes.Count)
                return null;
            return _locatedNodes[index];
        }

        private int GetPreviousTokenIndex(int tokenIndex)
        {
            while (tokenIndex > 0)
            {
                if (_locatedNodes[tokenIndex - 1].IsToken)
                    return tokenIndex - 1;
                tokenIndex--;
            }
            return -1;
        }

        private int FindIndex(LocatedNode token)
        {
            int result;
            if (!_locatedNodeIndexLookup.TryGetValue(token, out result))
                return -1;
            return result;
        }

        private void EnsureNewLinePreceeding(SyntaxToken token, int tokenIndex)
        {
            for (var i = tokenIndex - 1; i >= 0; i--)
            {
                if (_locatedNodes[i].Kind == SyntaxKind.WhitespaceTrivia)
                    continue;

                if (_locatedNodes[i].Kind == SyntaxKind.EndOfLineTrivia)
                {
                    MaybeReplaceText(
                        TextSpan.FromBounds(_rootSourceText, _locatedNodes[i].Span.End, token.Span.Start),
                        GetIndentation());
                    break;
                }

                MaybeReplaceText(
                    TextSpan.FromBounds(_rootSourceText, _locatedNodes[i].Span.End, token.Span.Start),
                    _options.NewLine + GetIndentation());
                break;
            }
        }

        private string GetIndentation()
        {
            if (_indentLevel < _whitespace.Count &&
                _whitespace[_indentLevel] != null)
            {
                return _whitespace[_indentLevel];
            }

            while (_indentLevel >= _whitespace.Count)
            {
                _whitespace.Add(null);
            }

            if (_options.SpacesPerIndent != null)
                return _whitespace[_indentLevel] = new string(
                    ' ',
                    _indentLevel * _options.SpacesPerIndent.Value);

            return _whitespace[_indentLevel] = new string('\t', _indentLevel);
        }

        private void Indent()
        {
            _indentLevel++;
        }

        private void Dedent()
        {
            _indentLevel--;
        }

        enum ReplaceWith
        {
            InsertNewLineAndIndentation,
            InsertSpace,
            RemoveSpace
        }

        private void MaybeReplaceText(TextSpan span, string newText)
        {
            Debug.Assert(span.IsInRootFile);

            if (!span.IntersectsWith(_spanToFormat))
                return;

            var indentation = newText ?? GetIndentation();
            var existingWsLength = span.Length;

            Edit edit;
            if (Edits.TryGetValue(span.Start, out edit))
                Edits.Remove(span.Start);
            Edits.Add(span.Start, new Edit(span.Start, existingWsLength, indentation));
        }

        private void AlignDirective(DirectiveTriviaSyntax directive)
        {
            if (!directive.HashToken.Span.IsInRootFile)
                return;

            var hashTokenIndex = FindIndex(directive.HashToken);

            switch (_options.Indentation.PreprocessorDirectivePosition)
            {
                case PreprocessorDirectivePosition.OneIndentToLeft:
                    ReplacePreceedingWhitespace(hashTokenIndex, GetIndentation(), true);
                    break;
                case PreprocessorDirectivePosition.MoveToLeftmostColumn:
                    ReplacePreceedingWhitespace(hashTokenIndex, "", true);
                    break;
                case PreprocessorDirectivePosition.LeaveIndented:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ReplaceFollowingWhitespace(directive.HashToken, hashTokenIndex, "");
        }

        private enum TrailingFormattingOperation
        {
            EnsureTrailingWhitespace,
            RemoveTrailingWhitespace,

            EnsureTrailingControlFlowWhitespace
        }

        private enum LeadingFormattingOperation
        {
            EnsureLeadingWhitespace,
            RemoveLeadingWhitespace, // Removes until after previous newline

            EnsureLeadingNewline,
            RemoveLeadingWhitespaceIncludingNewlinesAndEnsureLeadingWhitespace,
            RemoveLeadingWhitespaceIncludingNewlinesAndRemoveLeadingWhitespace,
            RemoveLeadingWhitespaceIncludingNewlinesAndEnsureLeadingNewline
        }

        private void FormatToken(SyntaxToken token, LeadingFormattingOperation? leading = null, TrailingFormattingOperation? trailing = null)
        {
            if (token == null || token.IsMissing)
                return;

            if (token.MacroReference != null)
            {
                if (token.IsFirstTokenInMacroExpansion)
                    token.MacroReference.Accept(this);
                return;
            }

            var locatedNodeIndex = FindIndex(token);
            if (locatedNodeIndex == -1)
                return;

            foreach (var trivia in token.LeadingTrivia)
                Visit(trivia);

            if (leading != null)
                switch (leading.Value)
                {
                    case LeadingFormattingOperation.EnsureLeadingWhitespace:
                        ReplacePreceedingWhitespace(locatedNodeIndex, " ");
                        break;
                    case LeadingFormattingOperation.RemoveLeadingWhitespace:
                        ReplacePreceedingWhitespace(locatedNodeIndex, "");
                        break;
                    case LeadingFormattingOperation.EnsureLeadingNewline:
                        EnsureNewLinePreceeding(token, locatedNodeIndex);
                        break;
                    case LeadingFormattingOperation.RemoveLeadingWhitespaceIncludingNewlinesAndEnsureLeadingWhitespace:
                        ReplacePreceedingWhitespaceForOpenBraceIncludingNewLines(token, locatedNodeIndex, false, true);
                        break;
                    case LeadingFormattingOperation.RemoveLeadingWhitespaceIncludingNewlinesAndRemoveLeadingWhitespace:
                        ReplacePreceedingWhitespaceForOpenBraceIncludingNewLines(token, locatedNodeIndex, false, false);
                        break;
                    case LeadingFormattingOperation.RemoveLeadingWhitespaceIncludingNewlinesAndEnsureLeadingNewline:
                        ReplacePreceedingWhitespaceForOpenBraceIncludingNewLines(token, locatedNodeIndex, true, true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            if (trailing != null)
                switch (trailing.Value)
                {
                    case TrailingFormattingOperation.EnsureTrailingWhitespace:
                        ReplaceFollowingWhitespace(token, locatedNodeIndex, " ");
                        break;
                    case TrailingFormattingOperation.RemoveTrailingWhitespace:
                        ReplaceFollowingWhitespace(token, locatedNodeIndex, "");
                        break;
                    case TrailingFormattingOperation.EnsureTrailingControlFlowWhitespace:
                        ReplaceFollowingWhitespace(token, locatedNodeIndex, _options.Spacing.InsertSpaceAfterKeywordsInControlFlowStatements ? " " : "");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            foreach (var trivia in token.TrailingTrivia)
                Visit(trivia);
        }

        #endregion
    }
}