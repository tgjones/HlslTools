using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.Hlsl.Binding.BoundNodes;
using ShaderTools.Hlsl.Binding.Signatures;
using ShaderTools.Hlsl.Symbols;

namespace ShaderTools.Hlsl.Binding
{
    internal static class UnaryOperator
    {
        private static readonly UnaryOperatorSignature[] BuiltInIdentitySignatures;
        private static readonly UnaryOperatorSignature[] BuiltInNegationSignatures;

        private static readonly UnaryOperatorSignature[] BuiltInBitwiseNotSignatures;
        private static readonly UnaryOperatorSignature[] BuiltInLogicalNotSignatures;

        private static readonly UnaryOperatorSignature[] BuiltInPostDecrementSignatures;
        private static readonly UnaryOperatorSignature[] BuiltInPostIncrementSignatures;
        private static readonly UnaryOperatorSignature[] BuiltInPreDecrementSignatures;
        private static readonly UnaryOperatorSignature[] BuiltInPreIncrementSignatures;

        static UnaryOperator()
        {
            BuiltInIdentitySignatures = IntrinsicTypes.AllNumericTypes.Select(x => new UnaryOperatorSignature(UnaryOperatorKind.Plus, x)).ToArray();
            BuiltInNegationSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new UnaryOperatorSignature(UnaryOperatorKind.Minus, x)).ToArray();

            BuiltInBitwiseNotSignatures = IntrinsicTypes.AllIntegralTypes.Select(x => new UnaryOperatorSignature(UnaryOperatorKind.BitwiseNot, x)).ToArray();
            BuiltInLogicalNotSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new UnaryOperatorSignature(UnaryOperatorKind.LogicalNot, TypeFacts.GetMatchingBoolType(x), x)).ToArray();

            BuiltInPostDecrementSignatures = IntrinsicTypes.AllNumericNonBoolTypes.Select(x => new UnaryOperatorSignature(UnaryOperatorKind.PostDecrement, x)).ToArray();
            BuiltInPostIncrementSignatures = IntrinsicTypes.AllNumericNonBoolTypes.Select(x => new UnaryOperatorSignature(UnaryOperatorKind.PostIncrement, x)).ToArray();
            BuiltInPreDecrementSignatures = IntrinsicTypes.AllNumericNonBoolTypes.Select(x => new UnaryOperatorSignature(UnaryOperatorKind.PreDecrement, x)).ToArray();
            BuiltInPreIncrementSignatures = IntrinsicTypes.AllNumericNonBoolTypes.Select(x => new UnaryOperatorSignature(UnaryOperatorKind.PreIncrement, x)).ToArray();
        }

        public static OverloadResolutionResult<UnaryOperatorSignature> Resolve(UnaryOperatorKind kind, TypeSymbol operandType)
        {
            return ResolveOverloads(kind, operandType);
        }

        private static OverloadResolutionResult<UnaryOperatorSignature> ResolveOverloads(UnaryOperatorKind kind, TypeSymbol operandType)
        {
            var builtInSignatures = GetBuiltInSignatures(kind);

            if (TypeBuiltIn(operandType))
                return OverloadResolution.Perform(builtInSignatures, operandType);

            return OverloadResolutionResult<UnaryOperatorSignature>.None;
        }

        private static bool TypeBuiltIn(TypeSymbol operandType)
        {
            return operandType.IsIntrinsicNumericType();
        }

        private static IEnumerable<UnaryOperatorSignature> GetBuiltInSignatures(UnaryOperatorKind kind)
        {
            switch (kind)
            {
                case UnaryOperatorKind.Plus:
                    return BuiltInIdentitySignatures;
                case UnaryOperatorKind.Minus:
                    return BuiltInNegationSignatures;
                case UnaryOperatorKind.BitwiseNot:
                    return BuiltInBitwiseNotSignatures;
                case UnaryOperatorKind.LogicalNot:
                    return BuiltInLogicalNotSignatures;
                case UnaryOperatorKind.PostDecrement:
                    return BuiltInPostDecrementSignatures;
                case UnaryOperatorKind.PostIncrement:
                    return BuiltInPostIncrementSignatures;
                case UnaryOperatorKind.PreDecrement:
                    return BuiltInPreDecrementSignatures;
                case UnaryOperatorKind.PreIncrement:
                    return BuiltInPreIncrementSignatures;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind.ToString());
            }
        }
    }
}