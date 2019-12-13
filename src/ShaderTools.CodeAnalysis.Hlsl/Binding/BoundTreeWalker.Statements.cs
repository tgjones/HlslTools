using System;
using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
{
    internal partial class BoundTreeWalker
    {
        protected virtual void VisitBlock(BoundBlock node)
        {
            foreach (var statement in node.Statements)
                VisitStatement(statement);
        }

        protected virtual void VisitStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.Block:
                    VisitBlock((BoundBlock) node);
                    break;
                case BoundNodeKind.BreakStatement:
                    VisitBreakStatement((BoundBreakStatement) node);
                    break;
                case BoundNodeKind.DiscardStatement:
                    VisitDiscardStatement((BoundDiscardStatement) node);
                    break;
                case BoundNodeKind.DoStatement:
                    VisitDoStatement((BoundDoStatement) node);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    VisitExpressionStatement((BoundExpressionStatement) node);
                    break;
                case BoundNodeKind.ForStatement:
                    VisitForStatement((BoundForStatement) node);
                    break;
                case BoundNodeKind.IfStatement:
                    VisitIfStatement((BoundIfStatement) node);
                    break;
                case BoundNodeKind.ReturnStatement:
                    VisitReturnStatement((BoundReturnStatement) node);
                    break;
                case BoundNodeKind.VariableDeclaration:
                    VisitVariableDeclaration((BoundVariableDeclaration) node);
                    break;
                case BoundNodeKind.MultipleVariableDeclarations:
                    VisitMultipleVariableDeclarations((BoundMultipleVariableDeclarations) node);
                    break;
                case BoundNodeKind.SwitchStatement:
                    VisitSwitchStatement((BoundSwitchStatement) node);
                    break;
                case BoundNodeKind.WhileStatement:
                    VisitWhileStatement((BoundWhileStatement) node);
                    break;
                case BoundNodeKind.ContinueStatement:
                    VisitContinueStatement((BoundContinueStatement) node);
                    break;
                case BoundNodeKind.NoOpStatement:
                    VisitNoOpStatement((BoundNoOpStatement) node);
                    break;
                default:
                    throw new InvalidOperationException(node.Kind.ToString());
            }
        }

        protected virtual void VisitNoOpStatement(BoundNoOpStatement node)
        {
            
        }

        protected virtual void VisitContinueStatement(BoundContinueStatement node)
        {

        }

        protected virtual void VisitWhileStatement(BoundWhileStatement node)
        {
            VisitExpression(node.Condition);
            VisitStatement(node.Body);
        }

        private void VisitDoStatement(BoundDoStatement node)
        {
            VisitStatement(node.Body);
            VisitExpression(node.Condition);
        }

        protected virtual void VisitBreakStatement(BoundBreakStatement node)
        {
            
        }

        protected virtual void VisitDiscardStatement(BoundDiscardStatement node)
        {
            
        }

        protected virtual void VisitExpressionStatement(BoundExpressionStatement node)
        {
            VisitExpression(node.Expression);
        }

        protected virtual void VisitForStatement(BoundForStatement node)
        {
            if (node.Declarations != null)
                VisitMultipleVariableDeclarations(node.Declarations);

            if (node.Initializer != null)
                VisitExpression(node.Initializer);

            if (node.Condition != null)
                VisitExpression(node.Condition);

            if (node.Incrementor != null)
                VisitExpression(node.Incrementor);

            VisitStatement(node.Body);
        }

        protected virtual void VisitIfStatement(BoundIfStatement node)
        {
            VisitExpression(node.Condition);

            VisitStatement(node.Consequence);

            if (node.AlternativeOpt != null)
                VisitStatement(node.AlternativeOpt);
        }

        protected virtual void VisitReturnStatement(BoundReturnStatement node)
        {
            if (node.ExpressionOpt != null)
                VisitExpression(node.ExpressionOpt);
        }

        protected virtual void VisitSwitchStatement(BoundSwitchStatement node)
        {
            VisitExpression(node.Expression);

            foreach (var section in node.Sections)
                VisitSwitchSection(section);
        }

        protected virtual void VisitSwitchSection(BoundSwitchSection node)
        {
            foreach (var label in node.Labels)
                VisitSwitchLabel(label);

            foreach (var statement in node.Statements)
                VisitStatement(statement);
        }

        protected virtual void VisitSwitchLabel(BoundSwitchLabel node)
        {
            if (node.Expression != null)
                VisitExpression(node.Expression);
        }
    }
}