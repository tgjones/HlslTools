using System;
using System.Collections.Immutable;
using System.Linq;
using HlslTools.Binding;
using HlslTools.Diagnostics;
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
                case SyntaxKind.StructType:
                    {
                        // Inline struct.

                        var structType = (StructTypeSyntax)node;
                        
                        var symbol = binder.LookupTypeSymbol(structType);
                        if (symbol == null)
                        {
                            binder.Diagnostics.ReportUndeclaredType(node);
                            return TypeFacts.Unknown;
                        }

                        return symbol;
                    }
                case SyntaxKind.IdentifierName:
                    {
                        var identifierName = (IdentifierNameSyntax) node;
                        var symbols = binder.LookupTypeSymbol(identifierName.Name).ToImmutableArray();
                        if (symbols.Length == 0)
                        {
                            binder.Diagnostics.ReportUndeclaredType(node);
                            return TypeFacts.Unknown;
                        }

                        if (symbols.Length > 1)
                            binder.Diagnostics.ReportAmbiguousType(identifierName.Name, symbols);

                        return symbols.First();
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
            var scalarType = TypeFacts.GetScalarType(node);
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
                case ScalarType.String:
                    return IntrinsicTypes.String;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static TypeSymbol GetTypeSymbol(this VectorTypeSyntax node)
        {
            var vectorType = TypeFacts.GetVectorType(node.TypeToken.Kind);
            var scalarType = vectorType.Item1;
            var numComponents = vectorType.Item2;

            return IntrinsicTypes.GetVectorType(scalarType, numComponents);
        }

        public static TypeSymbol GetTypeSymbol(this GenericVectorTypeSyntax node)
        {
            var scalarType = TypeFacts.GetScalarType(node.ScalarType);
            var numComponents = (int) node.SizeToken.Value;

            return IntrinsicTypes.GetVectorType(scalarType, numComponents);
        }

        public static TypeSymbol GetTypeSymbol(this MatrixTypeSyntax node)
        {
            var matrixType = TypeFacts.GetMatrixType(node.TypeToken.Kind);
            var scalarType = matrixType.Item1;
            var numRows = matrixType.Item2;
            var numCols = matrixType.Item3;

            return IntrinsicTypes.GetMatrixType(scalarType, numRows, numCols);
        }

        public static TypeSymbol GetTypeSymbol(this GenericMatrixTypeSyntax node)
        {
            var scalarType = TypeFacts.GetScalarType(node.ScalarType);
            var numRows = (int) node.RowsToken.Value;
            var numCols = (int) node.ColsToken.Value;

            return IntrinsicTypes.GetMatrixType(scalarType, numRows, numCols);
        }

        public static TypeSymbol GetTypeSymbol(this PredefinedObjectTypeSyntax node, Binder binder)
        {
            if (node.ObjectTypeToken.ContextualKind == SyntaxKind.ConstantBufferKeyword)
            {
                var valueTypeSyntax = (TypeSyntax) node.TemplateArgumentList.Arguments[0];
                var valueType = GetTypeSymbol(valueTypeSyntax, binder);
                return IntrinsicTypes.CreateConstantBufferType(valueType);
            }

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
                                scalarType = TypeFacts.GetScalarType((ScalarTypeSyntax) valueTypeSyntax);
                                break;
                            case SyntaxKind.PredefinedVectorType:
                                scalarType = TypeFacts.GetVectorType(((VectorTypeSyntax) valueTypeSyntax).TypeToken.Kind).Item1;
                                break;
                            case SyntaxKind.PredefinedGenericVectorType:
                                scalarType = TypeFacts.GetScalarType(((GenericVectorTypeSyntax) valueTypeSyntax).ScalarType);
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
                    throw new NotImplementedException(predefinedObjectType.ToString());
                case PredefinedObjectType.AppendStructuredBuffer:
                case PredefinedObjectType.ConsumeStructuredBuffer:
                case PredefinedObjectType.StructuredBuffer:
                case PredefinedObjectType.RWStructuredBuffer:
                case PredefinedObjectType.InputPatch:
                case PredefinedObjectType.OutputPatch:
                case PredefinedObjectType.PointStream:
                case PredefinedObjectType.LineStream:
                case PredefinedObjectType.TriangleStream:
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
                        case PredefinedObjectType.RWStructuredBuffer:
                            return IntrinsicTypes.CreateRWStructuredBufferType(valueType);
                        case PredefinedObjectType.InputPatch:
                            return IntrinsicTypes.CreateInputPatchType(valueType);
                        case PredefinedObjectType.OutputPatch:
                            return IntrinsicTypes.CreateOutputPatchType(valueType);
                        case PredefinedObjectType.PointStream:
                        case PredefinedObjectType.LineStream:
                        case PredefinedObjectType.TriangleStream:
                            return IntrinsicTypes.CreateStreamOutputType(predefinedObjectType, valueType);
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                case PredefinedObjectType.ByteAddressBuffer:
                    return IntrinsicTypes.ByteAddressBuffer;
                case PredefinedObjectType.BlendState:
                    return IntrinsicTypes.BlendState;
                case PredefinedObjectType.DepthStencilState:
                    return IntrinsicTypes.DepthStencilState;
                case PredefinedObjectType.RasterizerState:
                    return IntrinsicTypes.RasterizerState;
                case PredefinedObjectType.RWByteAddressBuffer:
                    return IntrinsicTypes.RWByteAddressBuffer;
                case PredefinedObjectType.RasterizerOrderedBuffer:
                case PredefinedObjectType.RasterizerOrderedByteAddressBuffer:
                case PredefinedObjectType.RasterizerOrderedStructuredBuffer:
                case PredefinedObjectType.RasterizerOrderedTexture1D:
                case PredefinedObjectType.RasterizerOrderedTexture1DArray:
                case PredefinedObjectType.RasterizerOrderedTexture2D:
                case PredefinedObjectType.RasterizerOrderedTexture2DArray:
                case PredefinedObjectType.RasterizerOrderedTexture3D:
                    throw new NotImplementedException(predefinedObjectType.ToString());
                case PredefinedObjectType.Sampler:
                    return IntrinsicTypes.Sampler;
                case PredefinedObjectType.Sampler1D:
                    return IntrinsicTypes.Sampler1D;
                case PredefinedObjectType.Sampler2D:
                    return IntrinsicTypes.Sampler2D;
                case PredefinedObjectType.Sampler3D:
                    return IntrinsicTypes.Sampler3D;
                case PredefinedObjectType.SamplerCube:
                    return IntrinsicTypes.SamplerCube;
                case PredefinedObjectType.SamplerState:
                    return IntrinsicTypes.SamplerState;
                case PredefinedObjectType.SamplerComparisonState:
                    return IntrinsicTypes.SamplerComparisonState;
                case PredefinedObjectType.Texture:
                    return IntrinsicTypes.LegacyTexture;
                case PredefinedObjectType.GeometryShader:
                    return IntrinsicTypes.GeometryShader;
                case PredefinedObjectType.PixelShader:
                    return IntrinsicTypes.PixelShader;
                case PredefinedObjectType.VertexShader:
                    return IntrinsicTypes.VertexShader;
                default:
                    throw new InvalidOperationException(predefinedObjectType.ToString());
            }
        }
    }
}