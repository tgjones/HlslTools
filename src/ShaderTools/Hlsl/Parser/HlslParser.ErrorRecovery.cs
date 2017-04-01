using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Hlsl.Parser
{
    internal partial class HlslParser
    {
        private TerminatorState _termState; // Resettable

        [Flags]
        private enum TerminatorState
        {
            EndOfFile = 0,
            IsPossibleGlobalDeclarationStartOrStop = 1 << 0,
            IsAttributeDeclarationTerminator = 1 << 1,
            IsPossibleAggregateClauseStartOrStop = 1 << 2,
            IsPossibleMemberStartOrStop = 1 << 3,
            IsEndOfReturnType = 1 << 4,
            IsEndOfParameterList = 1 << 5,
            IsEndOfFieldDeclaration = 1 << 6,
            IsPossibleEndOfVariableDeclaration = 1 << 7,
            IsEndOfTypeArgumentList = 1 << 8,
            IsPossibleStatementStartOrStop = 1 << 9,
            IsEndOfDoWhileExpression = 1 << 15,
            IsEndOfForStatementArgument = 1 << 16,
            IsEndOfDeclarationClause = 1 << 17,
            IsEndOfArgumentList = 1 << 18,
            IsSwitchSectionStart = 1 << 19,
            IsEndOfTypeParameterList = 1 << 20,
            IsEndOfMethodSignature = 1 << 21
        }

        private const int LastTerminatorState = (int)TerminatorState.IsEndOfMethodSignature;

        private bool IsTerminator()
        {
            if (Current.Kind == SyntaxKind.EndOfFileToken)
                return true;

            for (int i = 1; i <= LastTerminatorState; i <<= 1)
            {
                TerminatorState isolated = _termState & (TerminatorState) i;
                if (isolated != 0)
                {
                    switch (isolated)
                    {
                        case TerminatorState.IsPossibleGlobalDeclarationStartOrStop:
                            if (this.IsPossibleGlobalDeclarationStartOrStop())
                            {
                                return true;
                            }

                            break;
                        //case TerminatorState.IsAttributeDeclarationTerminator:
                        //    if (this.IsAttributeDeclarationTerminator())
                        //    {
                        //        return true;
                        //    }

                        //    break;
                        //case TerminatorState.IsPossibleMemberStartOrStop:
                        //    if (this.IsPossibleMemberStartOrStop())
                        //    {
                        //        return true;
                        //    }

                        //    break;
                        //case TerminatorState.IsPossibleAggregateClauseStartOrStop:
                        //    if (this.IsPossibleAggregateClauseStartOrStop())
                        //    {
                        //        return true;
                        //    }

                        //    break;
                        //case TerminatorState.IsEndOfReturnType:
                        //    if (this.IsEndOfReturnType())
                        //    {
                        //        return true;
                        //    }

                        //    break;
                        //case TerminatorState.IsEndOfParameterList:
                        //    if (this.IsEndOfParameterList())
                        //    {
                        //        return true;
                        //    }

                        //    break;
                        //case TerminatorState.IsEndOfFieldDeclaration:
                        //    if (this.IsEndOfFieldDeclaration())
                        //    {
                        //        return true;
                        //    }

                        //    break;
                        //case TerminatorState.IsPossibleEndOfVariableDeclaration:
                        //    if (this.IsPossibleEndOfVariableDeclaration())
                        //    {
                        //        return true;
                        //    }

                        //    break;
                        //case TerminatorState.IsEndOfTypeArgumentList:
                        //    if (this.IsEndOfTypeArgumentList())
                        //    {
                        //        return true;
                        //    }

                        //    break;
                        case TerminatorState.IsPossibleStatementStartOrStop:
                            if (this.IsPossibleStatementStartOrStop())
                                return true;
                            break;

                            //case TerminatorState.IsEndOfDoWhileExpression:
                            //    if (this.IsEndOfDoWhileExpression())
                            //    {
                            //        return true;
                            //    }

                            //    break;
                            //case TerminatorState.IsEndOfForStatementArgument:
                            //    if (this.IsEndOfForStatementArgument())
                            //    {
                            //        return true;
                            //    }

                            //    break;
                            //case TerminatorState.IsEndOfDeclarationClause:
                            //    if (this.IsEndOfDeclarationClause())
                            //    {
                            //        return true;
                            //    }

                            //    break;
                            //case TerminatorState.IsEndOfArgumentList:
                            //    if (this.IsEndOfArgumentList())
                            //    {
                            //        return true;
                            //    }

                            //    break;
                            //case TerminatorState.IsSwitchSectionStart:
                            //    if (this.IsPossibleSwitchSection())
                            //    {
                            //        return true;
                            //    }

                            //    break;

                            //case TerminatorState.IsEndOfTypeParameterList:
                            //    if (this.IsEndOfTypeParameterList())
                            //    {
                            //        return true;
                            //    }

                            //    break;

                            //case TerminatorState.IsEndOfMethodSignature:
                            //    if (this.IsEndOfMethodSignature())
                            //    {
                            //        return true;
                            //    }

                            //    break;
                    }
                }
            }

            return false;
        }

        private bool IsPossibleGlobalDeclarationStartOrStop()
        {
            return IsPossibleGlobalDeclarationStart() || Current.Kind == SyntaxKind.CloseBraceToken || Current.Kind == SyntaxKind.SemiToken;
        }

        private bool IsPossibleGlobalDeclarationStart()
        {
            return CanStartGlobalDeclaration(Current.Kind);
        }

        private bool CanStartGlobalDeclaration(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.CBufferKeyword:
                case SyntaxKind.ClassKeyword:
                case SyntaxKind.InterfaceKeyword:
                case SyntaxKind.NamespaceKeyword:
                case SyntaxKind.StructKeyword:
                case SyntaxKind.TBufferKeyword:
                case SyntaxKind.TechniqueKeyword:
                case SyntaxKind.Technique10Keyword:
                case SyntaxKind.Technique11Keyword:
                    return true;

                default:
                    if (IsPossibleDeclarationStatement())
                        return true;

                    if (IsPossibleFunctionDeclaration())
                        return true;

                    return false;
            }
        }

        private PostSkipAction SkipBadStatementListTokens(SyntaxKind expected)
        {
            return SkipBadTokens(
                p => !p.IsPossibleStatement(),
                p => p.Current.Kind == SyntaxKind.CloseBraceToken || p.IsTerminator(),
                expected
            );
        }

        private bool IsPossibleStatementStartOrStop()
        {
            return Current.Kind == SyntaxKind.SemiToken || IsPossibleStatement();
        }

        private bool IsPossibleClassMember()
        {
            return IsPossibleFunctionDeclaration() || IsPossibleDeclarationStatement();
        }

        private bool IsPossibleStatement()
        {
            var tk = Current.Kind;
            switch (tk)
            {
                case SyntaxKind.BreakKeyword:
                case SyntaxKind.ContinueKeyword:
                case SyntaxKind.DiscardKeyword:
                case SyntaxKind.DoKeyword:
                case SyntaxKind.ForKeyword:
                case SyntaxKind.IfKeyword:
                case SyntaxKind.ReturnKeyword:
                case SyntaxKind.SwitchKeyword:
                case SyntaxKind.WhileKeyword:
                case SyntaxKind.TypedefKeyword:
                case SyntaxKind.OpenBraceToken:
                case SyntaxKind.OpenBracketToken:
                case SyntaxKind.SemiToken:
                case SyntaxKind.IdentifierToken:
                    return true;
                default:
                    return SyntaxFacts.IsDeclarationModifier(Current)
                           || SyntaxFacts.IsPredefinedType(Current)
                           || IsPossibleExpression();
            }
        }

        private PostSkipAction SkipBadTokens(
            Func<HlslParser, bool> isNotExpectedFunction,
            Func<HlslParser, bool> abortFunction,
            SyntaxKind? expected = null)
        {
            var action = PostSkipAction.Continue;
            var tokens = new List<SyntaxToken>();

            var first = true;
            while (isNotExpectedFunction(this))
            {
                if (abortFunction(this))
                {
                    action = PostSkipAction.Abort;
                    break;
                }

                tokens.Add(first && expected != null ? NextTokenWithPrejudice(expected.Value) : NextToken());
                first = false;
            }

            if (!_scanStack.Any() && tokens.Any())
            {
                var current = _tokens[_tokenIndex];

                var skippedTokensTrivia = CreateSkippedTokensTrivia(tokens);

                var leadingTrivia = new List<SyntaxNode>(current.LeadingTrivia.Length + 1);
                leadingTrivia.Add(skippedTokensTrivia);
                leadingTrivia.AddRange(current.LeadingTrivia);

                _tokens[_tokenIndex] = current
                    .WithLeadingTrivia(leadingTrivia)
                    .WithDiagnostic(Diagnostic.Create(
                        HlslMessageProvider.Instance,
                        tokens.First().SourceRange,
                        (int) DiagnosticId.TokenUnexpected,
                        tokens.First().Text));
            }

            return action;
        }

        private enum PostSkipAction
        {
            Continue,
            Abort
        }
    }
}