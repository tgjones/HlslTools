using System;
using HlslTools.Binding.BoundNodes;

namespace HlslTools.Binding
{
    internal partial class BoundTreeWalker
    {
        public virtual void VisitExpression(BoundExpression node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    VisitLiteralExpression((BoundLiteralExpression) node);
                    break;
                case BoundNodeKind.StringLiteralExpression:
                    VisitStringLiteralExpression((BoundStringLiteralExpression) node);
                    break;
                case BoundNodeKind.UnaryExpression:
                    VisitUnaryExpression((BoundUnaryExpression) node);
                    break;
                case BoundNodeKind.BinaryExpression:
                    VisitBinaryExpression((BoundBinaryExpression) node);
                    break;
                case BoundNodeKind.FieldExpression:
                    VisitFieldExpression((BoundFieldExpression) node);
                    break;
                case BoundNodeKind.FunctionInvocationExpression:
                    VisitFunctionInvocationExpression((BoundFunctionInvocationExpression) node);
                    break;
                case BoundNodeKind.MethodInvocationExpression:
                    VisitMethodInvocationExpression((BoundMethodInvocationExpression) node);
                    break;
                case BoundNodeKind.NumericConstructorInvocationExpression:
                    VisitNumericConstructorInvocationExpression((BoundNumericConstructorInvocationExpression) node);
                    break;
                case BoundNodeKind.AssignmentExpression:
                    VisitAssignmentExpression((BoundAssignmentExpression) node);
                    break;
                case BoundNodeKind.ConversionExpression:
                    VisitConversionExpression((BoundConversionExpression) node);
                    break;
                case BoundNodeKind.CompoundExpression:
                    VisitCompoundExpression((BoundCompoundExpression) node);
                    break;
                case BoundNodeKind.ConditionalExpression:
                    VisitConditionExpression((BoundConditionalExpression) node);
                    break;
                case BoundNodeKind.ElementAccessExpression:
                    VisitElementAccessExpression((BoundElementAccessExpression) node);
                    break;
                case BoundNodeKind.ArrayInitializerExpression:
                    VisitArrayInitializerExpression((BoundArrayInitializerExpression) node);
                    break;
                case BoundNodeKind.ErrorExpression:
                    VisitErrorExpression((BoundErrorExpression) node);
                    break;
                case BoundNodeKind.VariableExpression:
                    VisitVariableExpression((BoundVariableExpression) node);
                    break;
                case BoundNodeKind.CompileExpression:
                    VisitCompileExpression((BoundCompileExpression)node);
                    break;
                default:
                    throw new InvalidOperationException(node.Kind.ToString());
            }
        }

        protected virtual void VisitCompileExpression(BoundCompileExpression node)
        {
            
        }

        protected virtual void VisitVariableExpression(BoundVariableExpression node)
        {
            
        }

        protected virtual void VisitErrorExpression(BoundErrorExpression node)
        {
            
        }

        protected virtual void VisitLiteralExpression(BoundLiteralExpression node)
        {
            
        }

        protected virtual void VisitStringLiteralExpression(BoundStringLiteralExpression node)
        {
            
        }

        protected virtual void VisitUnaryExpression(BoundUnaryExpression node)
        {
            VisitExpression(node.Expression);
        }

        protected virtual void VisitBinaryExpression(BoundBinaryExpression node)
        {
            VisitExpression(node.Left);
            VisitExpression(node.Right);
        }

        protected virtual void VisitFieldExpression(BoundFieldExpression node)
        {
            VisitExpression(node.ObjectReference);
        }

        protected virtual void VisitNumericConstructorInvocationExpression(BoundNumericConstructorInvocationExpression node)
        {
            foreach (var argument in node.Arguments)
                VisitExpression(argument);
        }

        protected virtual void VisitAssignmentExpression(BoundAssignmentExpression node)
        {
            VisitExpression(node.Left);
            VisitExpression(node.Right);
        }

        protected virtual void VisitConversionExpression(BoundConversionExpression node)
        {
            VisitExpression(node.Expression);
        }

        protected virtual void VisitCompoundExpression(BoundCompoundExpression node)
        {
            VisitExpression(node.Left);
            VisitExpression(node.Right);
        }

        protected virtual void VisitConditionExpression(BoundConditionalExpression node)
        {
            VisitExpression(node.Condition);
            VisitExpression(node.Consequence);
            VisitExpression(node.Alternative);
        }

        protected virtual void VisitElementAccessExpression(BoundElementAccessExpression node)
        {
            VisitExpression(node.Expression);
            VisitExpression(node.Index);
        }

        protected virtual void VisitArrayInitializerExpression(BoundArrayInitializerExpression node)
        {
            foreach (var element in node.Elements)
                VisitExpression(element);
        }

        protected virtual void VisitFunctionInvocationExpression(BoundFunctionInvocationExpression node)
        {
            foreach (var argument in node.Arguments)
                VisitExpression(argument);
        }

        protected virtual void VisitMethodInvocationExpression(BoundMethodInvocationExpression node)
        {
            VisitExpression(node.Target);
            foreach (var argument in node.Arguments)
                VisitExpression(argument);
        }

        protected virtual void VisitInitializer(BoundInitializer node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.EqualsValue:
                    VisitEqualsValueInitializer((BoundEqualsValue) node);
                    break;
                case BoundNodeKind.SamplerState:
                    VisitSamplerStateInitializer((BoundSamplerStateInitializer) node);
                    break;
                case BoundNodeKind.StateInitializer:
                    VisitStateInitializer((BoundStateInitializer) node);
                    break;
                case BoundNodeKind.StateArrayInitializer:
                    VisitStateArrayInitializer((BoundStateArrayInitializer) node);
                    break;
                default:
                    throw new InvalidOperationException(node.Kind.ToString());
            }
        }

        protected virtual void VisitEqualsValueInitializer(BoundEqualsValue node)
        {
            VisitExpression(node.Value);
        }

        protected virtual void VisitSamplerStateInitializer(BoundSamplerStateInitializer node)
        {
            
        }

        protected virtual void VisitStateInitializer(BoundStateInitializer node)
        {

        }

        protected virtual void VisitStateArrayInitializer(BoundStateArrayInitializer node)
        {

        }
    }
}