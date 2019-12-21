using System;
using System.Collections.Immutable;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
{
    internal partial class Binder
    {
        private BoundType BindType(TypeSyntax syntax, Symbol parent)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.PredefinedScalarType:
                    return BindScalarType((ScalarTypeSyntax) syntax);
                case SyntaxKind.PredefinedVectorType:
                    return BindVectorType((VectorTypeSyntax) syntax);
                case SyntaxKind.PredefinedGenericVectorType:
                    return BindGenericVectorType((GenericVectorTypeSyntax) syntax);
                case SyntaxKind.PredefinedMatrixType:
                    return BindMatrixType((MatrixTypeSyntax) syntax);
                case SyntaxKind.PredefinedGenericMatrixType:
                    return BindGenericMatrixType((GenericMatrixTypeSyntax) syntax);
                case SyntaxKind.PredefinedObjectType:
                    return new BoundObjectType(BindObjectType((PredefinedObjectTypeSyntax) syntax));
                case SyntaxKind.StructType:
                case SyntaxKind.ClassType:
                    {
                        // Inline struct.
                        return BindStructDeclaration((StructTypeSyntax) syntax, parent);
                    }
                case SyntaxKind.IdentifierName:
                    {
                        var identifierName = (IdentifierNameSyntax) syntax;
                        var symbols = LookupTypeSymbol(identifierName.Name).ToImmutableArray();
                        if (symbols.Length == 0)
                        {
                            Diagnostics.ReportUndeclaredType(syntax);
                            return new BoundUnknownType();
                        }

                        if (symbols.Length > 1)
                            Diagnostics.ReportAmbiguousType(identifierName.Name, symbols);

                        return new BoundName(symbols.First());
                    }
                case SyntaxKind.QualifiedName:
                    {
                        var qualifiedName = (QualifiedNameSyntax) syntax;
                        return BindQualifiedType(qualifiedName);
                    }
                case SyntaxKind.ModifiedType:
                    {
                        var modifiedType = (ModifiedTypeSyntax) syntax;
                        return BindType(modifiedType.Type, parent);
                    }
                default:
                    throw new InvalidOperationException(syntax.Kind.ToString());
            }
        }

        private BoundType BindQualifiedType(QualifiedNameSyntax qualifiedName)
        {
            var container = LookupContainer(qualifiedName.Left);

            if (container == null)
                return new BoundUnknownType();

            var symbols = container.Members.OfType<TypeSymbol>()
                .Where(x => x.Name == qualifiedName.Right.Name.Text)
                .ToImmutableArray();

            if (symbols.Length == 0)
            {
                Diagnostics.ReportUndeclaredType(qualifiedName);
                return new BoundUnknownType();
            }

            if (symbols.Length > 1)
                Diagnostics.ReportAmbiguousType(qualifiedName.Right.Name, symbols);

            Bind(qualifiedName.Right, x => new BoundName(symbols.First()));

            return new BoundName(symbols.First());
        }

        private BoundScalarType BindScalarType(ScalarTypeSyntax node)
        {
            var scalarType = TypeFacts.GetScalarType(node);
            switch (scalarType)
            {
                case ScalarType.Void:
                    return new BoundScalarType(IntrinsicTypes.Void);
                case ScalarType.Bool:
                    return new BoundScalarType(IntrinsicTypes.Bool);
                case ScalarType.Int:
                    return new BoundScalarType(IntrinsicTypes.Int);
                case ScalarType.Uint:
                    return new BoundScalarType(IntrinsicTypes.Uint);
                case ScalarType.Half:
                    return new BoundScalarType(IntrinsicTypes.Half);
                case ScalarType.Float:
                    return new BoundScalarType(IntrinsicTypes.Float);
                case ScalarType.Double:
                    return new BoundScalarType(IntrinsicTypes.Double);
                case ScalarType.Min16Float:
                    return new BoundScalarType(IntrinsicTypes.Min16Float);
                case ScalarType.Min10Float:
                    return new BoundScalarType(IntrinsicTypes.Min10Float);
                case ScalarType.Min16Int:
                    return new BoundScalarType(IntrinsicTypes.Min16Int);
                case ScalarType.Min12Int:
                    return new BoundScalarType(IntrinsicTypes.Min12Int);
                case ScalarType.Min16Uint:
                    return new BoundScalarType(IntrinsicTypes.Min16Uint);
                case ScalarType.String:
                    return new BoundScalarType(IntrinsicTypes.String);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private BoundVectorType BindVectorType(VectorTypeSyntax node)
        {
            var vectorType = TypeFacts.GetVectorType(node.TypeToken.Kind);
            var scalarType = vectorType.Item1;
            var numComponents = vectorType.Item2;

            return new BoundVectorType(IntrinsicTypes.GetVectorType(scalarType, numComponents));
        }

        private BoundGenericVectorType BindGenericVectorType(GenericVectorTypeSyntax node)
        {
            var scalarType = TypeFacts.GetScalarType(node.ScalarType);
            var numComponents = (int) node.SizeToken.Value;

            return new BoundGenericVectorType(
                IntrinsicTypes.GetVectorType(scalarType, numComponents),
                Bind(node.ScalarType, BindScalarType));
        }

        private BoundMatrixType BindMatrixType(MatrixTypeSyntax node)
        {
            var matrixType = TypeFacts.GetMatrixType(node.TypeToken.Kind);
            var scalarType = matrixType.Item1;
            var numRows = matrixType.Item2;
            var numCols = matrixType.Item3;

            return new BoundMatrixType(IntrinsicTypes.GetMatrixType(scalarType, numRows, numCols));
        }

        private BoundGenericMatrixType BindGenericMatrixType(GenericMatrixTypeSyntax node)
        {
            var scalarType = TypeFacts.GetScalarType(node.ScalarType);
            var numRows = (int) node.RowsToken.Value;
            var numCols = (int) node.ColsToken.Value;

            return new BoundGenericMatrixType(
                IntrinsicTypes.GetMatrixType(scalarType, numRows, numCols),
                Bind(node.ScalarType, BindScalarType));
        }

        private IntrinsicObjectTypeSymbol BindObjectType(PredefinedObjectTypeSyntax node)
        {
            if (node.ObjectTypeToken.ContextualKind == SyntaxKind.ConstantBufferKeyword)
            {
                var valueTypeSyntax = (TypeSyntax) node.TemplateArgumentList.Arguments[0];
                var valueType = Bind(valueTypeSyntax, x => BindType(x, null));
                return IntrinsicTypes.CreateConstantBufferType(valueType.TypeSymbol);
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
                        GetTextureValueAndScalarType(node, out valueType, out scalarType);
                        return IntrinsicTypes.CreateTextureType(predefinedObjectType, valueType, scalarType);
                    }
                case PredefinedObjectType.RWBuffer:
                case PredefinedObjectType.RasterizerOrderedBuffer:
                case PredefinedObjectType.RWTexture1D:
                case PredefinedObjectType.RWTexture1DArray:
                case PredefinedObjectType.RWTexture2D:
                case PredefinedObjectType.RWTexture2DArray:
                case PredefinedObjectType.RWTexture3D:
                case PredefinedObjectType.RasterizerOrderedTexture1D:
                case PredefinedObjectType.RasterizerOrderedTexture1DArray:
                case PredefinedObjectType.RasterizerOrderedTexture2D:
                case PredefinedObjectType.RasterizerOrderedTexture2DArray:
                case PredefinedObjectType.RasterizerOrderedTexture3D:
                    {
                        TypeSymbol valueType;
                        ScalarType scalarType;
                        GetTextureValueAndScalarType(node, out valueType, out scalarType);
                        return IntrinsicTypes.CreateRWTextureType(predefinedObjectType, valueType, scalarType);
                    }
                case PredefinedObjectType.AppendStructuredBuffer:
                case PredefinedObjectType.ConsumeStructuredBuffer:
                case PredefinedObjectType.StructuredBuffer:
                case PredefinedObjectType.RWStructuredBuffer:
                case PredefinedObjectType.RasterizerOrderedStructuredBuffer:
                case PredefinedObjectType.InputPatch:
                case PredefinedObjectType.OutputPatch:
                case PredefinedObjectType.PointStream:
                case PredefinedObjectType.LineStream:
                case PredefinedObjectType.TriangleStream:
                    {
                        var valueTypeSyntax = (TypeSyntax) node.TemplateArgumentList.Arguments[0];
                        var valueType = Bind(valueTypeSyntax, x => BindType(x, null)).TypeSymbol;
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
                            case PredefinedObjectType.RasterizerOrderedStructuredBuffer:
                                return IntrinsicTypes.CreateRasterizerOrderedStructuredBufferType(valueType);
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
                case PredefinedObjectType.RasterizerOrderedByteAddressBuffer:
                    return IntrinsicTypes.RasterizerOrderedByteAddressBuffer;
                case PredefinedObjectType.RWByteAddressBuffer:
                    return IntrinsicTypes.RWByteAddressBuffer;
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

        private void GetTextureValueAndScalarType(PredefinedObjectTypeSyntax node, out TypeSymbol valueType, out ScalarType scalarType)
        {
            if (node.TemplateArgumentList != null)
            {
                var valueTypeSyntax = (TypeSyntax) node.TemplateArgumentList.Arguments[0];
                if (valueTypeSyntax is ModifiedTypeSyntax modifiedType)
                {
                    valueTypeSyntax = modifiedType.Type;
                }
                valueType = Bind(valueTypeSyntax, x => BindType(x, null)).TypeSymbol;
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
                        Diagnostics.ReportInvalidType(valueTypeSyntax);
                        scalarType = ScalarType.Float;
                        break;
                }
            }
            else
            {
                valueType = IntrinsicTypes.Float4;
                scalarType = ScalarType.Float;
            }
        }
    }
}