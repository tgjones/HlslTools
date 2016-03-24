using System;
using System.Diagnostics;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    internal static class TypeFacts
    {
        public static readonly TypeSymbol Missing = new IntrinsicObjectTypeSymbol("[Missing]", string.Empty, PredefinedObjectType.Texture);
        public static readonly TypeSymbol Unknown = new IntrinsicObjectTypeSymbol("[Unknown]", string.Empty, PredefinedObjectType.Texture);
        public static readonly TypeSymbol Variadic = new IntrinsicObjectTypeSymbol("[Variadic]", string.Empty, PredefinedObjectType.Texture);

        public static bool IsMissing(this TypeSymbol type)
        {
            return type == Missing;
        }

        public static bool IsUnknown(this TypeSymbol type)
        {
            return type == Unknown;
        }

        public static bool IsError(this TypeSymbol type)
        {
            return type.IsMissing() || type.IsUnknown();
        }

        public static string ToDisplayName(this TypeSymbol type)
        {
            if (type.IsUnknown())
                return "<?>";

            if (type.IsMissing())
                return "<missing>";

            return type.Name;
        }

        public static bool HasExplicitConversionTo(this TypeSymbol left, TypeSymbol right)
        {
            if (left.Equals(right))
                return true;

            if (left.IsIntrinsicNumericType() && right.Kind == SymbolKind.Struct)
                return true;

            return left.HasImplicitConversionTo(right);
        }

        public static bool HasImplicitConversionTo(this TypeSymbol left, TypeSymbol right)
        {
            if (left.Equals(right))
                return true;

            // TODO: Need to be able to implicitly cast classes to base class and interfaces?
            if (left.IsUserDefined() || right.IsUserDefined())
                return false;

            if (left.Kind == SymbolKind.IntrinsicObjectType || right.Kind == SymbolKind.IntrinsicObjectType)
            {
                if (left.Kind == SymbolKind.IntrinsicObjectType && right.Kind == SymbolKind.IntrinsicObjectType)
                {
                    var leftIntrinsicType = (IntrinsicObjectTypeSymbol) left;
                    var rightIntrinsicType = (IntrinsicObjectTypeSymbol) right;
                    if (leftIntrinsicType.PredefinedType == PredefinedObjectType.Sampler)
                    {
                        switch (rightIntrinsicType.PredefinedType)
                        {
                            case PredefinedObjectType.Sampler1D:
                            case PredefinedObjectType.Sampler2D:
                            case PredefinedObjectType.Sampler3D:
                            case PredefinedObjectType.SamplerCube:
                            case PredefinedObjectType.SamplerState:
                                return true;
                        }
                    }
                }
                return false;
            }

            if (left.Kind == SymbolKind.Array || right.Kind == SymbolKind.Array)
                return false;

            if (left.Kind == SymbolKind.IntrinsicScalarType || right.Kind == SymbolKind.IntrinsicScalarType)
                return true;

            switch (left.Kind)
            {
                case SymbolKind.IntrinsicVectorType:
                    switch (right.Kind)
                    {
                        case SymbolKind.IntrinsicScalarType:
                            return true;
                        case SymbolKind.IntrinsicVectorType:
                            return ((IntrinsicVectorTypeSymbol) left).NumComponents >= ((IntrinsicVectorTypeSymbol) right).NumComponents;
                        case SymbolKind.IntrinsicMatrixType:
                            var leftVector = (IntrinsicVectorTypeSymbol) left;
                            var rightMatrix = (IntrinsicMatrixTypeSymbol) right;
                            return (leftVector.NumComponents >= rightMatrix.Cols && rightMatrix.Rows == 1)
                                || (leftVector.NumComponents >= rightMatrix.Rows && rightMatrix.Cols == 1);
                    }
                    break;
                case SymbolKind.IntrinsicMatrixType:
                    switch (right.Kind)
                    {
                        case SymbolKind.IntrinsicScalarType:
                            return true;
                        case SymbolKind.IntrinsicVectorType:
                        {
                            var leftMatrix = (IntrinsicMatrixTypeSymbol) left;
                            var rightVector = (IntrinsicVectorTypeSymbol) right;
                            return (leftMatrix.Rows >= rightVector.NumComponents && leftMatrix.Cols == 1)
                                || (leftMatrix.Cols >= rightVector.NumComponents && leftMatrix.Rows == 1);
                        }
                        case SymbolKind.IntrinsicMatrixType:
                        {
                            var leftMatrix = (IntrinsicMatrixTypeSymbol) left;
                            var rightMatrix = (IntrinsicMatrixTypeSymbol) right;
                            return leftMatrix.Rows >= rightMatrix.Rows && leftMatrix.Cols >= rightMatrix.Cols;
                        }
                    }
                    break;
            }

            if (left.GetNumElements() >= right.GetNumElements())
                return true;

            return false;
        }

        public static bool IsIntrinsicNumericType(this TypeSymbol type)
        {
            return type.Kind == SymbolKind.IntrinsicMatrixType
                || type.Kind == SymbolKind.IntrinsicScalarType
                || type.Kind == SymbolKind.IntrinsicVectorType;
        }

        public static bool IsUserDefined(this TypeSymbol type)
        {
            return type.Kind == SymbolKind.Struct
                || type.Kind == SymbolKind.Class
                || type.Kind == SymbolKind.Interface;
        }

        public static int GetDimensionSize(this IntrinsicNumericTypeSymbol type, int dimension)
        {
            switch (type.Kind)
            {
                case SymbolKind.IntrinsicMatrixType:
                    var matrixType = (IntrinsicMatrixTypeSymbol) type;
                    return dimension == 0 ? matrixType.Rows : matrixType.Cols;
                case SymbolKind.IntrinsicVectorType:
                    var vectorType = (IntrinsicVectorTypeSymbol) type;
                    return dimension == 0 ? vectorType.NumComponents : 1;
                case SymbolKind.IntrinsicScalarType:
                    return 1;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static int GetNumElements(this TypeSymbol type)
        {
            switch (type.Kind)
            {
                case SymbolKind.Array:
                {
                    var arrayType = (ArraySymbol) type;
                    return arrayType.Dimension ?? 0;
                }
                case SymbolKind.IntrinsicMatrixType:
                {
                    var matrixType = (IntrinsicMatrixTypeSymbol) type;
                    return matrixType.Rows * matrixType.Cols;
                }
                case SymbolKind.IntrinsicScalarType:
                {
                    return 1;
                }
                case SymbolKind.IntrinsicVectorType:
                {
                    var vectorType = (IntrinsicVectorTypeSymbol) type;
                    return vectorType.NumComponents;
                }
                default:
                {
                    return 1;
                }
            }
        }

        public static bool IsPromotion(ScalarType left, ScalarType right)
        {
            if (left == right)
                return false;

            switch (right)
            {
                case ScalarType.Half:
                    switch (left)
                    {
                        case ScalarType.Float:
                        case ScalarType.Double:
                            return true;
                    }
                    break;
                case ScalarType.Float:
                    switch (left)
                    {
                        case ScalarType.Double:
                            return true;
                    }
                    break;
            }

            return false;
        }

        public static bool IsCast(ScalarType left, ScalarType right)
        {
            if (left == right)
                return false;

            switch (left)
            {
                case ScalarType.Int:
                    switch (right)
                    {
                        case ScalarType.Uint:
                            return false;
                    }
                    break;
                case ScalarType.Uint:
                    switch (right)
                    {
                        case ScalarType.Int:
                            return false;
                    }
                    break;
            }

            return true;
        }

        public static bool IsIntCast(ScalarType left, ScalarType right)
        {
            if (left == right)
                return false;

            // TODO

            return true;
        }

        public static ScalarType GetScalarType(ScalarTypeSyntax node)
        {
            if (node.TypeTokens.Count == 2 && node.TypeTokens[0].Kind == SyntaxKind.UnsignedKeyword && node.TypeTokens[1].Kind == SyntaxKind.IntKeyword)
                return ScalarType.Uint;

            Debug.Assert(node.TypeTokens.Count == 1);

            switch (node.TypeTokens[0].Kind)
            {
                case SyntaxKind.VoidKeyword:
                    return ScalarType.Void;
                case SyntaxKind.BoolKeyword:
                    return ScalarType.Bool;
                case SyntaxKind.IntKeyword:
                case SyntaxKind.DwordKeyword:
                    return ScalarType.Int;
                case SyntaxKind.UintKeyword:
                    return ScalarType.Uint;
                case SyntaxKind.HalfKeyword:
                    return ScalarType.Half;
                case SyntaxKind.FloatKeyword:
                    return ScalarType.Float;
                case SyntaxKind.DoubleKeyword:
                    return ScalarType.Double;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Tuple<ScalarType, int> GetVectorType(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.VectorKeyword:
                    return Tuple.Create(ScalarType.Float, 4);

                case SyntaxKind.Bool1Keyword:
                    return Tuple.Create(ScalarType.Bool, 1);
                case SyntaxKind.Bool2Keyword:
                    return Tuple.Create(ScalarType.Bool, 2);
                case SyntaxKind.Bool3Keyword:
                    return Tuple.Create(ScalarType.Bool, 3);
                case SyntaxKind.Bool4Keyword:
                    return Tuple.Create(ScalarType.Bool, 4);
                case SyntaxKind.Int1Keyword:
                    return Tuple.Create(ScalarType.Int, 1);
                case SyntaxKind.Int2Keyword:
                    return Tuple.Create(ScalarType.Int, 2);
                case SyntaxKind.Int3Keyword:
                    return Tuple.Create(ScalarType.Int, 3);
                case SyntaxKind.Int4Keyword:
                    return Tuple.Create(ScalarType.Int, 4);
                case SyntaxKind.Uint1Keyword:
                    return Tuple.Create(ScalarType.Uint, 1);
                case SyntaxKind.Uint2Keyword:
                    return Tuple.Create(ScalarType.Uint, 2);
                case SyntaxKind.Uint3Keyword:
                    return Tuple.Create(ScalarType.Uint, 3);
                case SyntaxKind.Uint4Keyword:
                    return Tuple.Create(ScalarType.Uint, 4);
                case SyntaxKind.Half1Keyword:
                    return Tuple.Create(ScalarType.Half, 1);
                case SyntaxKind.Half2Keyword:
                    return Tuple.Create(ScalarType.Half, 2);
                case SyntaxKind.Half3Keyword:
                    return Tuple.Create(ScalarType.Half, 3);
                case SyntaxKind.Half4Keyword:
                    return Tuple.Create(ScalarType.Half, 4);
                case SyntaxKind.Float1Keyword:
                    return Tuple.Create(ScalarType.Float, 1);
                case SyntaxKind.Float2Keyword:
                    return Tuple.Create(ScalarType.Float, 2);
                case SyntaxKind.Float3Keyword:
                    return Tuple.Create(ScalarType.Float, 3);
                case SyntaxKind.Float4Keyword:
                    return Tuple.Create(ScalarType.Float, 4);
                case SyntaxKind.Double1Keyword:
                    return Tuple.Create(ScalarType.Double, 1);
                case SyntaxKind.Double2Keyword:
                    return Tuple.Create(ScalarType.Double, 2);
                case SyntaxKind.Double3Keyword:
                    return Tuple.Create(ScalarType.Double, 3);
                case SyntaxKind.Double4Keyword:
                    return Tuple.Create(ScalarType.Double, 4);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Tuple<ScalarType, int, int> GetMatrixType(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.MatrixKeyword:
                    return Tuple.Create(ScalarType.Float, 4, 4);

                case SyntaxKind.Bool1x1Keyword:
                    return Tuple.Create(ScalarType.Bool, 1, 1);
                case SyntaxKind.Bool1x2Keyword:
                    return Tuple.Create(ScalarType.Bool, 1, 2);
                case SyntaxKind.Bool1x3Keyword:
                    return Tuple.Create(ScalarType.Bool, 1, 3);
                case SyntaxKind.Bool1x4Keyword:
                    return Tuple.Create(ScalarType.Bool, 1, 4);
                case SyntaxKind.Bool2x1Keyword:
                    return Tuple.Create(ScalarType.Bool, 2, 1);
                case SyntaxKind.Bool2x2Keyword:
                    return Tuple.Create(ScalarType.Bool, 2, 2);
                case SyntaxKind.Bool2x3Keyword:
                    return Tuple.Create(ScalarType.Bool, 2, 3);
                case SyntaxKind.Bool2x4Keyword:
                    return Tuple.Create(ScalarType.Bool, 2, 4);
                case SyntaxKind.Bool3x1Keyword:
                    return Tuple.Create(ScalarType.Bool, 3, 1);
                case SyntaxKind.Bool3x2Keyword:
                    return Tuple.Create(ScalarType.Bool, 3, 2);
                case SyntaxKind.Bool3x3Keyword:
                    return Tuple.Create(ScalarType.Bool, 3, 3);
                case SyntaxKind.Bool3x4Keyword:
                    return Tuple.Create(ScalarType.Bool, 3, 4);
                case SyntaxKind.Bool4x1Keyword:
                    return Tuple.Create(ScalarType.Bool, 4, 1);
                case SyntaxKind.Bool4x2Keyword:
                    return Tuple.Create(ScalarType.Bool, 4, 2);
                case SyntaxKind.Bool4x3Keyword:
                    return Tuple.Create(ScalarType.Bool, 4, 3);
                case SyntaxKind.Bool4x4Keyword:
                    return Tuple.Create(ScalarType.Bool, 4, 4);

                case SyntaxKind.Int1x1Keyword:
                    return Tuple.Create(ScalarType.Int, 1, 1);
                case SyntaxKind.Int1x2Keyword:
                    return Tuple.Create(ScalarType.Int, 1, 2);
                case SyntaxKind.Int1x3Keyword:
                    return Tuple.Create(ScalarType.Int, 1, 3);
                case SyntaxKind.Int1x4Keyword:
                    return Tuple.Create(ScalarType.Int, 1, 4);
                case SyntaxKind.Int2x1Keyword:
                    return Tuple.Create(ScalarType.Int, 2, 1);
                case SyntaxKind.Int2x2Keyword:
                    return Tuple.Create(ScalarType.Int, 2, 2);
                case SyntaxKind.Int2x3Keyword:
                    return Tuple.Create(ScalarType.Int, 2, 3);
                case SyntaxKind.Int2x4Keyword:
                    return Tuple.Create(ScalarType.Int, 2, 4);
                case SyntaxKind.Int3x1Keyword:
                    return Tuple.Create(ScalarType.Int, 3, 1);
                case SyntaxKind.Int3x2Keyword:
                    return Tuple.Create(ScalarType.Int, 3, 2);
                case SyntaxKind.Int3x3Keyword:
                    return Tuple.Create(ScalarType.Int, 3, 3);
                case SyntaxKind.Int3x4Keyword:
                    return Tuple.Create(ScalarType.Int, 3, 4);
                case SyntaxKind.Int4x1Keyword:
                    return Tuple.Create(ScalarType.Int, 4, 1);
                case SyntaxKind.Int4x2Keyword:
                    return Tuple.Create(ScalarType.Int, 4, 2);
                case SyntaxKind.Int4x3Keyword:
                    return Tuple.Create(ScalarType.Int, 4, 3);
                case SyntaxKind.Int4x4Keyword:
                    return Tuple.Create(ScalarType.Int, 4, 4);

                case SyntaxKind.Uint1x1Keyword:
                    return Tuple.Create(ScalarType.Uint, 1, 1);
                case SyntaxKind.Uint1x2Keyword:
                    return Tuple.Create(ScalarType.Uint, 1, 2);
                case SyntaxKind.Uint1x3Keyword:
                    return Tuple.Create(ScalarType.Uint, 1, 3);
                case SyntaxKind.Uint1x4Keyword:
                    return Tuple.Create(ScalarType.Uint, 1, 4);
                case SyntaxKind.Uint2x1Keyword:
                    return Tuple.Create(ScalarType.Uint, 2, 1);
                case SyntaxKind.Uint2x2Keyword:
                    return Tuple.Create(ScalarType.Uint, 2, 2);
                case SyntaxKind.Uint2x3Keyword:
                    return Tuple.Create(ScalarType.Uint, 2, 3);
                case SyntaxKind.Uint2x4Keyword:
                    return Tuple.Create(ScalarType.Uint, 2, 4);
                case SyntaxKind.Uint3x1Keyword:
                    return Tuple.Create(ScalarType.Uint, 3, 1);
                case SyntaxKind.Uint3x2Keyword:
                    return Tuple.Create(ScalarType.Uint, 3, 2);
                case SyntaxKind.Uint3x3Keyword:
                    return Tuple.Create(ScalarType.Uint, 3, 3);
                case SyntaxKind.Uint3x4Keyword:
                    return Tuple.Create(ScalarType.Uint, 3, 4);
                case SyntaxKind.Uint4x1Keyword:
                    return Tuple.Create(ScalarType.Uint, 4, 1);
                case SyntaxKind.Uint4x2Keyword:
                    return Tuple.Create(ScalarType.Uint, 4, 2);
                case SyntaxKind.Uint4x3Keyword:
                    return Tuple.Create(ScalarType.Uint, 4, 3);
                case SyntaxKind.Uint4x4Keyword:
                    return Tuple.Create(ScalarType.Uint, 4, 4);

                case SyntaxKind.Half1x1Keyword:
                    return Tuple.Create(ScalarType.Half, 1, 1);
                case SyntaxKind.Half1x2Keyword:
                    return Tuple.Create(ScalarType.Half, 1, 2);
                case SyntaxKind.Half1x3Keyword:
                    return Tuple.Create(ScalarType.Half, 1, 3);
                case SyntaxKind.Half1x4Keyword:
                    return Tuple.Create(ScalarType.Half, 1, 4);
                case SyntaxKind.Half2x1Keyword:
                    return Tuple.Create(ScalarType.Half, 2, 1);
                case SyntaxKind.Half2x2Keyword:
                    return Tuple.Create(ScalarType.Half, 2, 2);
                case SyntaxKind.Half2x3Keyword:
                    return Tuple.Create(ScalarType.Half, 2, 3);
                case SyntaxKind.Half2x4Keyword:
                    return Tuple.Create(ScalarType.Half, 2, 4);
                case SyntaxKind.Half3x1Keyword:
                    return Tuple.Create(ScalarType.Half, 3, 1);
                case SyntaxKind.Half3x2Keyword:
                    return Tuple.Create(ScalarType.Half, 3, 2);
                case SyntaxKind.Half3x3Keyword:
                    return Tuple.Create(ScalarType.Half, 3, 3);
                case SyntaxKind.Half3x4Keyword:
                    return Tuple.Create(ScalarType.Half, 3, 4);
                case SyntaxKind.Half4x1Keyword:
                    return Tuple.Create(ScalarType.Half, 4, 1);
                case SyntaxKind.Half4x2Keyword:
                    return Tuple.Create(ScalarType.Half, 4, 2);
                case SyntaxKind.Half4x3Keyword:
                    return Tuple.Create(ScalarType.Half, 4, 3);
                case SyntaxKind.Half4x4Keyword:
                    return Tuple.Create(ScalarType.Half, 4, 4);

                case SyntaxKind.Float1x1Keyword:
                    return Tuple.Create(ScalarType.Float, 1, 1);
                case SyntaxKind.Float1x2Keyword:
                    return Tuple.Create(ScalarType.Float, 1, 2);
                case SyntaxKind.Float1x3Keyword:
                    return Tuple.Create(ScalarType.Float, 1, 3);
                case SyntaxKind.Float1x4Keyword:
                    return Tuple.Create(ScalarType.Float, 1, 4);
                case SyntaxKind.Float2x1Keyword:
                    return Tuple.Create(ScalarType.Float, 2, 1);
                case SyntaxKind.Float2x2Keyword:
                    return Tuple.Create(ScalarType.Float, 2, 2);
                case SyntaxKind.Float2x3Keyword:
                    return Tuple.Create(ScalarType.Float, 2, 3);
                case SyntaxKind.Float2x4Keyword:
                    return Tuple.Create(ScalarType.Float, 2, 4);
                case SyntaxKind.Float3x1Keyword:
                    return Tuple.Create(ScalarType.Float, 3, 1);
                case SyntaxKind.Float3x2Keyword:
                    return Tuple.Create(ScalarType.Float, 3, 2);
                case SyntaxKind.Float3x3Keyword:
                    return Tuple.Create(ScalarType.Float, 3, 3);
                case SyntaxKind.Float3x4Keyword:
                    return Tuple.Create(ScalarType.Float, 3, 4);
                case SyntaxKind.Float4x1Keyword:
                    return Tuple.Create(ScalarType.Float, 4, 1);
                case SyntaxKind.Float4x2Keyword:
                    return Tuple.Create(ScalarType.Float, 4, 2);
                case SyntaxKind.Float4x3Keyword:
                    return Tuple.Create(ScalarType.Float, 4, 3);
                case SyntaxKind.Float4x4Keyword:
                    return Tuple.Create(ScalarType.Float, 4, 4);

                case SyntaxKind.Double1x1Keyword:
                    return Tuple.Create(ScalarType.Double, 1, 1);
                case SyntaxKind.Double1x2Keyword:
                    return Tuple.Create(ScalarType.Double, 1, 2);
                case SyntaxKind.Double1x3Keyword:
                    return Tuple.Create(ScalarType.Double, 1, 3);
                case SyntaxKind.Double1x4Keyword:
                    return Tuple.Create(ScalarType.Double, 1, 4);
                case SyntaxKind.Double2x1Keyword:
                    return Tuple.Create(ScalarType.Double, 2, 1);
                case SyntaxKind.Double2x2Keyword:
                    return Tuple.Create(ScalarType.Double, 2, 2);
                case SyntaxKind.Double2x3Keyword:
                    return Tuple.Create(ScalarType.Double, 2, 3);
                case SyntaxKind.Double2x4Keyword:
                    return Tuple.Create(ScalarType.Double, 2, 4);
                case SyntaxKind.Double3x1Keyword:
                    return Tuple.Create(ScalarType.Double, 3, 1);
                case SyntaxKind.Double3x2Keyword:
                    return Tuple.Create(ScalarType.Double, 3, 2);
                case SyntaxKind.Double3x3Keyword:
                    return Tuple.Create(ScalarType.Double, 3, 3);
                case SyntaxKind.Double3x4Keyword:
                    return Tuple.Create(ScalarType.Double, 3, 4);
                case SyntaxKind.Double4x1Keyword:
                    return Tuple.Create(ScalarType.Double, 4, 1);
                case SyntaxKind.Double4x2Keyword:
                    return Tuple.Create(ScalarType.Double, 4, 2);
                case SyntaxKind.Double4x3Keyword:
                    return Tuple.Create(ScalarType.Double, 4, 3);
                case SyntaxKind.Double4x4Keyword:
                    return Tuple.Create(ScalarType.Double, 4, 4);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static TypeSymbol GetMatchingBoolType(TypeSymbol numericType)
        {
            switch (numericType.Kind)
            {
                case SymbolKind.IntrinsicScalarType:
                    return IntrinsicTypes.Bool;
                case SymbolKind.IntrinsicVectorType:
                    return IntrinsicTypes.GetVectorType(ScalarType.Bool, ((IntrinsicVectorTypeSymbol)numericType).NumComponents);
                case SymbolKind.IntrinsicMatrixType:
                    var matrixType = (IntrinsicMatrixTypeSymbol)numericType;
                    return IntrinsicTypes.GetMatrixType(ScalarType.Bool, matrixType.Rows, matrixType.Cols);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}