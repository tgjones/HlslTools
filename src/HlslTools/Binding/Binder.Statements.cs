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
                case SyntaxKind.VariableDeclarationStatement:
                    return BindVariableDeclaration((VariableDeclarationStatementSyntax) syntax);

                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax) syntax);

                default:
                    throw new NotSupportedException("Not supported: " + syntax.Kind);
            }
        }

        private BoundExpressionStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            return new BoundExpressionStatement(Bind(syntax.Expression, BindExpression));
        }
    }
}