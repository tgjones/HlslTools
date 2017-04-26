using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;
using ShaderTools.CodeAnalysis.Hlsl.Binding.Signatures;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
{
    internal static class BinaryOperator
    {
        private static readonly BinaryOperatorSignature[] BuiltInMultiplySignatures;
        private static readonly BinaryOperatorSignature[] BuiltInDivideSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInModulusSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInAddSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInSubSignatures;

        private static readonly BinaryOperatorSignature[] BuiltInLeftShiftSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInRightShiftSignatures;

        private static readonly BinaryOperatorSignature[] BuiltInEqualSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInNotEqualSignatures;

        private static readonly BinaryOperatorSignature[] BuiltInLessSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInGreaterSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInLessOrEqualSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInGreaterOrEqualSignatures;

        private static readonly BinaryOperatorSignature[] BuiltInBitAndSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInBitOrSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInBitXorSignatures;

        private static readonly BinaryOperatorSignature[] BuiltInLogicalAndSignatures;
        private static readonly BinaryOperatorSignature[] BuiltInLogicalOrSignatures;

        static BinaryOperator()
        {
            BuiltInMultiplySignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.Multiply, x)).ToArray();
            BuiltInDivideSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.Divide, x)).ToArray();
            BuiltInModulusSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.Modulo, x)).ToArray();
            BuiltInAddSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.Add, x)).ToArray();
            BuiltInSubSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.Subtract, x)).ToArray();

            BuiltInLeftShiftSignatures = IntrinsicTypes.AllIntegralTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.LeftShift, x)).ToArray();
            BuiltInRightShiftSignatures = IntrinsicTypes.AllIntegralTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.RightShift, x)).ToArray();

            BuiltInEqualSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.Equal, TypeFacts.GetMatchingBoolType(x), x)).ToArray();
            BuiltInNotEqualSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.NotEqual, TypeFacts.GetMatchingBoolType(x), x)).ToArray();

            BuiltInLessSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.Less, TypeFacts.GetMatchingBoolType(x), x)).ToArray();
            BuiltInGreaterSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.Greater, TypeFacts.GetMatchingBoolType(x), x)).ToArray();
            BuiltInLessOrEqualSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.LessEqual, TypeFacts.GetMatchingBoolType(x), x)).ToArray();
            BuiltInGreaterOrEqualSignatures = IntrinsicTypes.AllNumericTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.GreaterEqual, TypeFacts.GetMatchingBoolType(x), x)).ToArray();

            BuiltInBitAndSignatures = IntrinsicTypes.AllIntegralTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.BitwiseAnd, x)).ToArray();
            BuiltInBitOrSignatures = IntrinsicTypes.AllIntegralTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.BitwiseOr, x)).ToArray();
            BuiltInBitXorSignatures = IntrinsicTypes.AllIntegralTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.BitwiseXor, x)).ToArray();

            BuiltInLogicalAndSignatures = IntrinsicTypes.AllBoolTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.LogicalAnd, x)).ToArray();
            BuiltInLogicalOrSignatures = IntrinsicTypes.AllBoolTypes.Select(x => new BinaryOperatorSignature(BinaryOperatorKind.LogicalOr, x)).ToArray();
        }

        public static OverloadResolutionResult<BinaryOperatorSignature> Resolve(BinaryOperatorKind kind, TypeSymbol leftOperandType, TypeSymbol rightOperandType)
        {
            return ResolveOverloads(kind, leftOperandType, rightOperandType);
        }

        private static OverloadResolutionResult<BinaryOperatorSignature> ResolveOverloads(BinaryOperatorKind kind, TypeSymbol leftOperandType, TypeSymbol rightOperandType)
        {
            var builtInSignatures = GetBuiltInSignatures(kind);

            if (BothTypesBuiltIn(leftOperandType, rightOperandType))
                return OverloadResolution.Perform(builtInSignatures, leftOperandType, rightOperandType);

            return OverloadResolutionResult<BinaryOperatorSignature>.None;
        }

        private static bool BothTypesBuiltIn(TypeSymbol leftOperandType, TypeSymbol rightOperandType)
        {
            return leftOperandType.IsIntrinsicNumericType() && rightOperandType.IsIntrinsicNumericType();
        }

        private static IEnumerable<BinaryOperatorSignature> GetBuiltInSignatures(BinaryOperatorKind kind)
        {
            switch (kind)
            {
                case BinaryOperatorKind.Multiply:
                    return BuiltInMultiplySignatures;
                case BinaryOperatorKind.Divide:
                    return BuiltInDivideSignatures;
                case BinaryOperatorKind.Modulo:
                    return BuiltInModulusSignatures;
                case BinaryOperatorKind.Add:
                    return BuiltInAddSignatures;
                case BinaryOperatorKind.Subtract:
                    return BuiltInSubSignatures;
                case BinaryOperatorKind.Equal:
                    return BuiltInEqualSignatures;
                case BinaryOperatorKind.NotEqual:
                    return BuiltInNotEqualSignatures;
                case BinaryOperatorKind.Less:
                    return BuiltInLessSignatures;
                case BinaryOperatorKind.LessEqual:
                    return BuiltInLessOrEqualSignatures;
                case BinaryOperatorKind.Greater:
                    return BuiltInGreaterSignatures;
                case BinaryOperatorKind.GreaterEqual:
                    return BuiltInGreaterOrEqualSignatures;
                case BinaryOperatorKind.BitwiseXor:
                    return BuiltInBitXorSignatures;
                case BinaryOperatorKind.BitwiseAnd:
                    return BuiltInBitAndSignatures;
                case BinaryOperatorKind.BitwiseOr:
                    return BuiltInBitOrSignatures;
                case BinaryOperatorKind.LeftShift:
                    return BuiltInLeftShiftSignatures;
                case BinaryOperatorKind.RightShift:
                    return BuiltInRightShiftSignatures;
                case BinaryOperatorKind.LogicalAnd:
                    return BuiltInLogicalAndSignatures;
                case BinaryOperatorKind.LogicalOr:
                    return BuiltInLogicalOrSignatures;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind.ToString());
            }
        }
    }
}