using System.Linq;
using HlslTools.Binding.BoundNodes;
using HlslTools.Symbols;

namespace HlslTools.Binding
{
    internal static class SemanticFacts
    {
        public static bool RequiresNumericTypes(this BinaryOperatorKind op)
        {
            return op.IsArithmetic() || op.IsLogical() || op.IsComparison();
        }

        public static bool RequiresIntegralTypes(this BinaryOperatorKind op)
        {
            return op.IsBitwise();
        }

        public static bool IsBitwise(this BinaryOperatorKind op)
        {
            switch (op)
            {
                case BinaryOperatorKind.LeftShift:
                case BinaryOperatorKind.RightShift:
                case BinaryOperatorKind.BitwiseAnd:
                case BinaryOperatorKind.BitwiseXor:
                case BinaryOperatorKind.BitwiseOr:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsArithmetic(this BinaryOperatorKind op)
        {
            switch (op)
            {
                case BinaryOperatorKind.Multiply:
                case BinaryOperatorKind.Divide:
                case BinaryOperatorKind.Modulo:
                case BinaryOperatorKind.Add:
                case BinaryOperatorKind.Subtract:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsLogical(this BinaryOperatorKind op)
        {
            switch (op)
            {
                case BinaryOperatorKind.LogicalAnd:
                case BinaryOperatorKind.LogicalOr:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsComparison(this BinaryOperatorKind op)
        {
            switch (op)
            {
                case BinaryOperatorKind.Less:
                case BinaryOperatorKind.Greater:
                case BinaryOperatorKind.LessEqual:
                case BinaryOperatorKind.GreaterEqual:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsIntegral(this ScalarType scalarType)
        {
            switch (scalarType)
            {
                case ScalarType.Bool:
                case ScalarType.Int:
                case ScalarType.Uint:
                case ScalarType.Min16Int:
                case ScalarType.Min12Int:
                case ScalarType.Min16Uint:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFloat(this ScalarType scalarType)
        {
            switch (scalarType)
            {
                case ScalarType.Half:
                case ScalarType.Float:
                case ScalarType.Double:
                case ScalarType.Min16Float:
                case ScalarType.Min10Float:
                    return true;
                default:
                    return false;
            }
        }
    }
}