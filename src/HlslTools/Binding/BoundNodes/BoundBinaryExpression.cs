using HlslTools.Binding.Signatures;
using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BinaryOperatorKind OperatorKind { get; }
        public BoundExpression Left { get; }
        public BoundExpression Right { get; }
        public OverloadResolutionResult<BinaryOperatorSignature> Result { get; }

        public override TypeSymbol Type => Result.Selected == null ? TypeFacts.Unknown : Result.Selected.Signature.ReturnType;

        public BoundBinaryExpression(BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right, OverloadResolutionResult<BinaryOperatorSignature> result)
            : base(BoundNodeKind.BinaryExpression)
        {
            OperatorKind = operatorKind;
            Left = left;
            Right = right;
            Result = result;
        }
    }
}