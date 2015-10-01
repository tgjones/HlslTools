using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HlslTools.Syntax;

namespace HlslTools.Symbols
{
    internal sealed class SymbolSet : ISymbolTable
    {
        private readonly List<Symbol> _globals;
        private readonly Dictionary<string, Symbol> _globalMap;

        public SymbolSet()
        {
            _globals = new List<Symbol>();
            _globalMap = new Dictionary<string, Symbol>();
        }

        public bool IsSymbol(TypeSymbol symbol, string symbolName)
        {
            return (((ISymbolTable)this).FindSymbol(symbolName, null) == symbol);
        }

        public TypeSymbol ResolveIntrinsicType(PredefinedTypeSyntax type, ISymbolTable symbolTable, Symbol contextSymbol)
        {
            switch (type.Kind)
            {
                case SyntaxKind.PredefinedScalarType:
                    return ResolveIntrinsicScalarType((ScalarTypeSyntax) type);

                case SyntaxKind.PredefinedVectorType:
                    return ResolveIntrinsicVectorType((VectorTypeSyntax)type);

                case SyntaxKind.PredefinedGenericVectorType:
                    return ResolveIntrinsicGenericVectorType((GenericVectorTypeSyntax)type);

                case SyntaxKind.PredefinedMatrixType:
                    return ResolveIntrinsicMatrixType((MatrixTypeSyntax)type);

                case SyntaxKind.PredefinedGenericMatrixType:
                    return ResolveIntrinsicGenericMatrixType((GenericMatrixTypeSyntax)type);

                case SyntaxKind.PredefinedObjectType:
                    return ResolveIntrinsicObjectType((PredefinedObjectTypeSyntax) type, symbolTable, contextSymbol);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unmapped intrinsic type");
            }
        }

        private static TypeSymbol ResolveIntrinsicScalarType(ScalarTypeSyntax type)
        {
            var scalarType = SyntaxFacts.GetScalarType(type.TypeTokens.Select(x => x.Kind).ToArray());
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

        private static TypeSymbol ResolveIntrinsicVectorType(VectorTypeSyntax type)
        {
            var vectorType = SyntaxFacts.GetVectorType(type.TypeToken.Kind);
            var scalarType = vectorType.Item1;
            var numComponents = vectorType.Item2;

            return IntrinsicTypes.GetVectorType(scalarType, numComponents);
        }

        private static TypeSymbol ResolveIntrinsicGenericVectorType(GenericVectorTypeSyntax type)
        {
            var scalarType = SyntaxFacts.GetScalarType(type.ScalarType.TypeTokens.Select(x => x.Kind).ToArray());
            var numComponents = (int) type.SizeToken.Value;

            return IntrinsicTypes.GetVectorType(scalarType, numComponents);
        }

        private static TypeSymbol ResolveIntrinsicMatrixType(MatrixTypeSyntax type)
        {
            var matrixType = SyntaxFacts.GetMatrixType(type.TypeToken.Kind);
            var scalarType = matrixType.Item1;
            var numRows = matrixType.Item2;
            var numCols = matrixType.Item3;

            return IntrinsicTypes.GetMatrixType(scalarType, numRows, numCols);
        }

        private static TypeSymbol ResolveIntrinsicGenericMatrixType(GenericMatrixTypeSyntax type)
        {
            var scalarType = SyntaxFacts.GetScalarType(type.ScalarType.TypeTokens.Select(x => x.Kind).ToArray());
            var numRows = (int) type.RowsToken.Value;
            var numCols = (int) type.ColsToken.Value;

            return IntrinsicTypes.GetMatrixType(scalarType, numRows, numCols);
        }

        private TypeSymbol ResolveIntrinsicObjectType(PredefinedObjectTypeSyntax type, ISymbolTable symbolTable, Symbol contextSymbol)
        {
            var predefinedObjectType = SyntaxFacts.GetPredefinedObjectType(type.ObjectTypeToken.Kind);
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
                    if (type.TemplateArgumentList != null)
                    {
                        var valueTypeSyntax = type.TemplateArgumentList.Arguments[0];
                        valueType = ResolveIntrinsicType((PredefinedTypeSyntax) valueTypeSyntax, symbolTable, contextSymbol);
                        switch (valueTypeSyntax.Kind)
                        {
                            case SyntaxKind.PredefinedScalarType:
                                scalarType = SyntaxFacts.GetScalarType(((ScalarTypeSyntax) valueTypeSyntax).TypeTokens.Select(x => x.Kind).ToArray());
                                break;
                            case SyntaxKind.PredefinedVectorType:
                                scalarType = SyntaxFacts.GetVectorType(((VectorTypeSyntax) valueTypeSyntax).TypeToken.Kind).Item1;
                                break;
                            case SyntaxKind.PredefinedGenericVectorType:
                                scalarType = SyntaxFacts.GetScalarType(((GenericVectorTypeSyntax) valueTypeSyntax).ScalarType.TypeTokens.Select(x => x.Kind).ToArray());
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
                    var valueTypeSyntax = (TypeSyntax) type.TemplateArgumentList.Arguments[0];
                    var valueType = ResolveType(valueTypeSyntax, symbolTable, contextSymbol);
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
                    return IntrinsicTypes.CreateByteAddressBufferType();
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

        public TypeSymbol ResolveType(TypeSyntax node, ISymbolTable symbolTable, Symbol contextSymbol)
        {
            if (node is PredefinedTypeSyntax)
            {
                return ResolveIntrinsicType((PredefinedTypeSyntax) node, symbolTable, contextSymbol);
            }
            //else if (node is ArrayTypeNode)
            //{
            //    ArrayTypeNode arrayTypeNode = (ArrayTypeNode)node;

            //    TypeSymbol itemTypeSymbol = ResolveType(arrayTypeNode.BaseType, symbolTable, contextSymbol);
            //    Debug.Assert(itemTypeSymbol != null);

            //    return CreateArrayTypeSymbol(itemTypeSymbol);
            //}
            else
            {
                Debug.Assert(node is IdentifierNameSyntax);
                var nameNode = (IdentifierNameSyntax) node;

                return (TypeSymbol)symbolTable.FindSymbol(nameNode.Name.Text, contextSymbol);
            }
        }

        public void AddGlobal(Symbol symbol)
        {
            _globals.Add(symbol);
            _globalMap.Add(symbol.Name, symbol);
        }

        #region ISymbolTable Members

        ICollection ISymbolTable.Symbols => _globals;

        Symbol ISymbolTable.FindSymbol(string name, Symbol context)
        {
            Symbol symbol;
            _globalMap.TryGetValue(name, out symbol);
            return symbol;
        }

        #endregion
    }
}