using System;
using HlslTools.Binding;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    internal static class IntrinsicTypeResolver
    {
        public static TypeSymbol GetTypeSymbol(this TypeSyntax node, Binder binder)
        {
            switch (node.Kind)
            {
                case SyntaxKind.PredefinedScalarType:
                case SyntaxKind.PredefinedVectorType:
                case SyntaxKind.PredefinedGenericVectorType:
                case SyntaxKind.PredefinedMatrixType:
                case SyntaxKind.PredefinedGenericMatrixType:
                case SyntaxKind.PredefinedObjectType:
                    return ((PredefinedTypeSyntax) node).GetTypeSymbol(binder);
                case SyntaxKind.IdentifierName:
                    {
                        var identifierName = (IdentifierNameSyntax) node;
                        return binder.LookupSymbol(identifierName.Name) as TypeSymbol;
                    }
                default:
                    throw new NotImplementedException(node.Kind.ToString());
            }
        }

        public static TypeSymbol GetTypeSymbol(this PredefinedTypeSyntax type, Binder binder)
        {
            switch (type.Kind)
            {
                case SyntaxKind.PredefinedScalarType:
                    return GetTypeSymbol((ScalarTypeSyntax) type);

                case SyntaxKind.PredefinedVectorType:
                    return GetTypeSymbol((VectorTypeSyntax) type);

                case SyntaxKind.PredefinedGenericVectorType:
                    return GetTypeSymbol((GenericVectorTypeSyntax) type);

                case SyntaxKind.PredefinedMatrixType:
                    return GetTypeSymbol((MatrixTypeSyntax) type);

                case SyntaxKind.PredefinedGenericMatrixType:
                    return GetTypeSymbol((GenericMatrixTypeSyntax) type);

                case SyntaxKind.PredefinedObjectType:
                    return GetTypeSymbol((PredefinedObjectTypeSyntax) type, binder);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unmapped intrinsic type");
            }
        }

        public static TypeSymbol GetTypeSymbol(this ScalarTypeSyntax node)
        {
            var scalarType = SyntaxFacts.GetScalarType(node);
            switch (scalarType)
            {
                case ScalarType.Void:
                    return IntrinsicTypes.Void;
                case ScalarType.Bool:
                    return IntrinsicTypes.Bool;
                case ScalarType.Int:
                    return IntrinsicTypes.Int;
                case ScalarType.Uint:
                    return IntrinsicTypes.Uint;
                case ScalarType.Half:
                    return IntrinsicTypes.Half;
                case ScalarType.Float:
                    return IntrinsicTypes.Float;
                case ScalarType.Double:
                    return IntrinsicTypes.Double;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static TypeSymbol GetTypeSymbol(this VectorTypeSyntax node)
        {
            var vectorType = SyntaxFacts.GetVectorType(node.TypeToken.Kind);
            var scalarType = vectorType.Item1;
            var numComponents = vectorType.Item2;

            return IntrinsicTypes.GetVectorType(scalarType, numComponents);
        }

        public static TypeSymbol GetTypeSymbol(this GenericVectorTypeSyntax node)
        {
            var scalarType = SyntaxFacts.GetScalarType(node.ScalarType);
            var numComponents = (int) node.SizeToken.Value;

            return IntrinsicTypes.GetVectorType(scalarType, numComponents);
        }

        public static TypeSymbol GetTypeSymbol(this MatrixTypeSyntax node)
        {
            var matrixType = SyntaxFacts.GetMatrixType(node.TypeToken.Kind);
            var scalarType = matrixType.Item1;
            var numRows = matrixType.Item2;
            var numCols = matrixType.Item3;

            return IntrinsicTypes.GetMatrixType(scalarType, numRows, numCols);
        }

        public static TypeSymbol GetTypeSymbol(this GenericMatrixTypeSyntax node)
        {
            var scalarType = SyntaxFacts.GetScalarType(node.ScalarType);
            var numRows = (int) node.RowsToken.Value;
            var numCols = (int) node.ColsToken.Value;

            return IntrinsicTypes.GetMatrixType(scalarType, numRows, numCols);
        }

        public static TypeSymbol GetTypeSymbol(this PredefinedObjectTypeSyntax node, Binder binder)
        {
            var predefinedObjectType = SyntaxFacts.GetPredefinedObjectType(node.ObjectTypeToken.Kind);
            switch (predefinedObjectType)
            {
                case PredefinedObjectType.Buffer:
                case PredefinedObjectType.Texture1D:
                case PredefinedObjectType.Texture1DArray:
                case PredefinedObjectType.Texture2D:
                case PredefinedObjectType.Texture2DArray:
                case PredefinedObjectType.Texture3D:
                case PredefinedObjectType.TextureCube:
                case PredefinedObjectType.TextureCubeArray:
                case PredefinedObjectType.Texture2DMS:
                case PredefinedObjectType.Texture2DMSArray:
                {
                    TypeSymbol valueType;
                    ScalarType scalarType;
                    if (node.TemplateArgumentList != null)
                    {
                        var valueTypeSyntax = node.TemplateArgumentList.Arguments[0];
                        valueType = GetTypeSymbol((PredefinedTypeSyntax) valueTypeSyntax, binder);
                        switch (valueTypeSyntax.Kind)
                        {
                            case SyntaxKind.PredefinedScalarType:
                                scalarType = SyntaxFacts.GetScalarType((ScalarTypeSyntax) valueTypeSyntax);
                                break;
                            case SyntaxKind.PredefinedVectorType:
                                scalarType = SyntaxFacts.GetVectorType(((VectorTypeSyntax) valueTypeSyntax).TypeToken.Kind).Item1;
                                break;
                            case SyntaxKind.PredefinedGenericVectorType:
                                scalarType = SyntaxFacts.GetScalarType(((GenericVectorTypeSyntax) valueTypeSyntax).ScalarType);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        valueType = IntrinsicTypes.Float4;
                        scalarType = ScalarType.Float;
                    }
                    return IntrinsicTypes.CreateTextureType(predefinedObjectType, valueType, scalarType);
                }
                case PredefinedObjectType.RWBuffer:
                case PredefinedObjectType.RWTexture1D:
                case PredefinedObjectType.RWTexture1DArray:
                case PredefinedObjectType.RWTexture2D:
                case PredefinedObjectType.RWTexture2DArray:
                case PredefinedObjectType.RWTexture3D:
                    throw new NotImplementedException();
                case PredefinedObjectType.AppendStructuredBuffer:
                case PredefinedObjectType.ConsumeStructuredBuffer:
                case PredefinedObjectType.StructuredBuffer:
                {
                    var valueTypeSyntax = (TypeSyntax) node.TemplateArgumentList.Arguments[0];
                    var valueType = GetTypeSymbol(valueTypeSyntax, binder);
                    switch (predefinedObjectType)
                    {
                        case PredefinedObjectType.AppendStructuredBuffer:
                            return IntrinsicTypes.CreateAppendStructuredBufferType(valueType);
                        case PredefinedObjectType.ConsumeStructuredBuffer:
                            return IntrinsicTypes.CreateConsumeStructuredBufferType(valueType);
                        case PredefinedObjectType.StructuredBuffer:
                            return IntrinsicTypes.CreateStructuredBufferType(valueType);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                case PredefinedObjectType.ByteAddressBuffer:
                    return IntrinsicTypes.ByteAddressBuffer;
                case PredefinedObjectType.RWByteAddressBuffer:
                case PredefinedObjectType.RWStructuredBuffer:
                case PredefinedObjectType.InputPatch:
                case PredefinedObjectType.OutputPatch:
                case PredefinedObjectType.PointStream:
                case PredefinedObjectType.LineStream:
                case PredefinedObjectType.TriangleStream:
                    throw new NotImplementedException();
                case PredefinedObjectType.Sampler:
                case PredefinedObjectType.SamplerState:
                    return IntrinsicTypes.SamplerState;
                case PredefinedObjectType.SamplerComparisonState:
                    return IntrinsicTypes.SamplerComparisonState;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}