using System;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding.BoundNodes;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal partial class Binder
    {
        private BoundBlock BindBlock(BlockSyntax syntax)
        {
            var blockBinder = new Binder(_sharedBinderState, this);
            return new BoundBlock(syntax.Statements.Select(x => blockBinder.Bind(x, blockBinder.BindStatement)).ToImmutableArray());
        }

        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.Block:
                    return BindBlock((BlockSyntax) syntax);
                case SyntaxKind.BreakStatement:
                    return BindBreakStatement((BreakStatementSyntax) syntax);
                case SyntaxKind.DiscardStatement:
                    return BindDiscardStatement((DiscardStatementSyntax) syntax);
                case SyntaxKind.DoStatement:
                    return BindDoStatement((DoStatementSyntax) syntax);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax) syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForStatementSyntax) syntax);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfStatementSyntax) syntax);
                case SyntaxKind.ReturnStatement:
                    return BindReturnStatement((ReturnStatementSyntax) syntax);
                case SyntaxKind.VariableDeclarationStatement:
                    return BindVariableDeclarationStatement((VariableDeclarationStatementSyntax) syntax);
                case SyntaxKind.SwitchStatement:
                    return BindSwitchStatement((SwitchStatementSyntax) syntax);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileStatementSyntax) syntax);
                default:
                    throw new NotSupportedException("Not supported: " + syntax.Kind);
            }
        }

        private BoundStatement BindDoStatement(DoStatementSyntax syntax)
        {
            return new BoundDoStatement(
                Bind(syntax.Condition, BindExpression),
                Bind(syntax.Statement, BindStatement));
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            return new BoundWhileStatement(
                Bind(syntax.Condition, BindExpression),
                Bind(syntax.Statement, BindStatement));
        }

        private BoundStatement BindSwitchStatement(SwitchStatementSyntax syntax)
        {
            var switchBinder = new Binder(_sharedBinderState, this);
            var boundSections = syntax.Sections.Select(x => switchBinder.Bind(x, switchBinder.BindSwitchSection)).ToImmutableArray();

            return new BoundSwitchStatement(
                Bind(syntax.Expression, BindExpression),
                boundSections);
        }

        private BoundSwitchSection BindSwitchSection(SwitchSectionSyntax syntax)
        {
            return new BoundSwitchSection(
                syntax.Labels.Select(x => Bind(x, BindSwitchLabel)).ToImmutableArray(),
                syntax.Statements.Select(x => Bind(x, BindStatement)).ToImmutableArray());
        }

        private BoundSwitchLabel BindSwitchLabel(SwitchLabelSyntax syntax)
        {
            BoundExpression boundExpression;
            switch (syntax.Kind)
            {
                case SyntaxKind.CaseSwitchLabel:
                    var caseSwitchLabel = (CaseSwitchLabelSyntax) syntax;
                    boundExpression = Bind(caseSwitchLabel.Value, BindExpression);
                    break;
                case SyntaxKind.DefaultSwitchLabel:
                    boundExpression = null;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return new BoundSwitchLabel(boundExpression);
        }

        private BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
        {
            return new BoundBreakStatement();
        }

        private BoundStatement BindDiscardStatement(DiscardStatementSyntax syntax)
        {
            return new BoundDiscardStatement();
        }

        private BoundExpressionStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            return new BoundExpressionStatement(Bind(syntax.Expression, BindExpression));
        }

        private BoundForStatement BindForStatement(ForStatementSyntax syntax)
        {
            return new BoundForStatement(
                Bind(syntax.Declaration, BindVariableDeclaration),
                syntax.Initializer != null ? Bind(syntax.Initializer, BindExpression) : null,
                Bind(syntax.Condition, BindExpression),
                Bind(syntax.Incrementor, BindExpression),
                Bind(syntax.Statement, BindStatement));
        }

        private BoundIfStatement BindIfStatement(IfStatementSyntax syntax)
        {
            return new BoundIfStatement(
                Bind(syntax.Condition, BindExpression),
                Bind(syntax.Statement, BindStatement),
                syntax.Else != null ? Bind(syntax.Else.Statement, BindStatement) : null);
        }

        private BoundReturnStatement BindReturnStatement(ReturnStatementSyntax syntax)
        {
            return new BoundReturnStatement(syntax.Expression != null ? Bind(syntax.Expression, BindExpression) : null);
        }

        private BoundMultipleVariableDeclarations BindVariableDeclarationStatement(VariableDeclarationStatementSyntax syntax)
        {
            return BindVariableDeclaration(syntax.Declaration);
        }
    }
}