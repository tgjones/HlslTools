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
                default:
                    throw new NotSupportedException("Not supported: " + syntax.Kind);
            }
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