using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Parser
{
    internal partial class HlslParser
    {
        private PredefinedTypeSyntax ParsePredefinedType(bool allowVoid, bool parentIsParameter)
        {
            var token = NextToken();

            if (SyntaxFacts.IsPredefinedScalarType(token.Kind))
            {
                if (!allowVoid && token.Kind == SyntaxKind.VoidKeyword)
                    token = WithDiagnostic(token, parentIsParameter ? DiagnosticId.NoVoidParameter : DiagnosticId.NoVoidHere);
                return ParseScalarType(token);
            }

            if (SyntaxFacts.IsPredefinedVectorType(token.Kind))
            {
                if (token.Kind == SyntaxKind.VectorKeyword && Current.Kind == SyntaxKind.LessThanToken)
                    return ParseGenericVectorType(token);
                return ParseVectorType(token);
            }

            if (SyntaxFacts.IsPredefinedMatrixType(token.Kind))
            {
                if (token.Kind == SyntaxKind.MatrixKeyword && Current.Kind == SyntaxKind.LessThanToken)
                    return ParseGenericMatrixType(token);
                return ParseMatrixType(token);
            }

            if (SyntaxFacts.IsPredefinedObjectType(token))
                return ParseObjectType(token);

            Debug.Fail("Shouldn't be here.");
            return null;
        }

        private PredefinedObjectTypeSyntax ParseObjectType(SyntaxToken token)
        {
            switch (token.Kind)
            {
                case SyntaxKind.BufferKeyword:
                case SyntaxKind.RasterizerOrderedBufferKeyword:
                case SyntaxKind.RWBufferKeyword:
                    return ParseBufferType(token);
                case SyntaxKind.ByteAddressBufferKeyword:
                case SyntaxKind.RasterizerOrderedByteAddressBufferKeyword:
                case SyntaxKind.RWByteAddressBufferKeyword:
                case SyntaxKind.RasterizerStateKeyword:
                case SyntaxKind.BlendStateKeyword:
                case SyntaxKind.DepthStencilStateKeyword:
                case SyntaxKind.Texture2DLegacyKeyword:
                case SyntaxKind.TextureCubeLegacyKeyword:
                case SyntaxKind.SamplerKeyword:
                case SyntaxKind.Sampler1DKeyword:
                case SyntaxKind.Sampler2DKeyword:
                case SyntaxKind.Sampler3DKeyword:
                case SyntaxKind.SamplerCubeKeyword:
                case SyntaxKind.SamplerStateKeyword:
                case SyntaxKind.SamplerComparisonStateKeyword:
                    return new PredefinedObjectTypeSyntax(token, null);
                case SyntaxKind.InputPatchKeyword:
                case SyntaxKind.OutputPatchKeyword:
                    return ParsePatchType(token);
                case SyntaxKind.PointStreamKeyword:
                case SyntaxKind.LineStreamKeyword:
                case SyntaxKind.TriangleStreamKeyword:
                    return ParseStreamOutputType(token);
                case SyntaxKind.AppendStructuredBufferKeyword:
                case SyntaxKind.ConsumeStructuredBufferKeyword:
                case SyntaxKind.RasterizerOrderedStructuredBufferKeyword:
                case SyntaxKind.RWStructuredBufferKeyword:
                case SyntaxKind.StructuredBufferKeyword:
                    return ParseStructuredBufferType(token);
                case SyntaxKind.RasterizerOrderedTexture1DKeyword:
                case SyntaxKind.RasterizerOrderedTexture1DArrayKeyword:
                case SyntaxKind.RasterizerOrderedTexture2DKeyword:
                case SyntaxKind.RasterizerOrderedTexture2DArrayKeyword:
                case SyntaxKind.RasterizerOrderedTexture3DKeyword:
                case SyntaxKind.RWTexture1DKeyword:
                case SyntaxKind.RWTexture1DArrayKeyword:
                case SyntaxKind.RWTexture2DKeyword:
                case SyntaxKind.RWTexture2DArrayKeyword:
                case SyntaxKind.RWTexture3DKeyword:
                case SyntaxKind.Texture1DKeyword:
                case SyntaxKind.Texture1DArrayKeyword:
                case SyntaxKind.Texture2DKeyword:
                case SyntaxKind.Texture2DArrayKeyword:
                case SyntaxKind.Texture3DKeyword:
                case SyntaxKind.TextureCubeKeyword:
                case SyntaxKind.TextureCubeArrayKeyword:
                    return ParseTextureType(token);
                case SyntaxKind.Texture2DMSKeyword:
                case SyntaxKind.Texture2DMSArrayKeyword:
                    return ParseMultisampledTextureType(token);
            }

            switch (token.ContextualKind)
            {
                case SyntaxKind.TextureKeyword:
                case SyntaxKind.GeometryShaderKeyword:
                case SyntaxKind.PixelShaderKeyword:
                case SyntaxKind.VertexShaderKeyword:
                    return new PredefinedObjectTypeSyntax(token.WithKind(token.ContextualKind), null);
                case SyntaxKind.ConstantBufferKeyword:
                    return ParseTemplatedConstantBufferType(token);
            }

            TemplateArgumentListSyntax templateArgumentList = null;
            if (Current.Kind == SyntaxKind.LessThanToken)
                templateArgumentList = ParseTemplateArgumentList();
            return new PredefinedObjectTypeSyntax(token, templateArgumentList);
        }

        private PredefinedObjectTypeSyntax ParseBufferType(SyntaxToken token)
        {
            var lessThan = Match(SyntaxKind.LessThanToken);
            var scalarOrVectorType = ParseScalarOrVectorType();
            var greaterThan = Match(SyntaxKind.GreaterThanToken);
            var typeArgumentList = new TemplateArgumentListSyntax(lessThan,
                new SeparatedSyntaxList<ExpressionSyntax>(new List<SyntaxNodeBase> { scalarOrVectorType }),
                greaterThan);
            return new PredefinedObjectTypeSyntax(token, typeArgumentList);
        }

        private PredefinedObjectTypeSyntax ParsePatchType(SyntaxToken token)
        {
            var lessThan = Match(SyntaxKind.LessThanToken);
            var userDefinedType = ParseIdentifier();
            var comma = Match(SyntaxKind.CommaToken);

            ExpressionSyntax controlPoints;
            try
            {
                CommaIsSeparatorStack.Push(true);
                _greaterThanTokenIsNotOperator = true;
                controlPoints = ParseExpression();
            }
            finally
            {
                _greaterThanTokenIsNotOperator = false;
                CommaIsSeparatorStack.Pop();
            }

            var greaterThan = Match(SyntaxKind.GreaterThanToken);
            var typeArgumentList = new TemplateArgumentListSyntax(lessThan,
                new SeparatedSyntaxList<ExpressionSyntax>(new List<SyntaxNodeBase> { userDefinedType, comma, controlPoints }),
                greaterThan);
            return new PredefinedObjectTypeSyntax(token, typeArgumentList);
        }

        private PredefinedObjectTypeSyntax ParseStreamOutputType(SyntaxToken token)
        {
            var lessThan = Match(SyntaxKind.LessThanToken);
            var type = ParseType(false);
            var greaterThan = Match(SyntaxKind.GreaterThanToken);
            var typeArgumentList = new TemplateArgumentListSyntax(lessThan,
                new SeparatedSyntaxList<ExpressionSyntax>(new List<SyntaxNodeBase> { type }),
                greaterThan);
            return new PredefinedObjectTypeSyntax(token, typeArgumentList);
        }

        private PredefinedObjectTypeSyntax ParseStructuredBufferType(SyntaxToken token)
        {
            var lessThan = Match(SyntaxKind.LessThanToken);
            var type = ParseScalarOrVectorOrMatrixOrUserDefinedType();
            var greaterThan = Match(SyntaxKind.GreaterThanToken);
            var typeArgumentList = new TemplateArgumentListSyntax(lessThan,
                new SeparatedSyntaxList<ExpressionSyntax>(new List<SyntaxNodeBase> { type }),
                greaterThan);
            return new PredefinedObjectTypeSyntax(token, typeArgumentList);
        }

        private PredefinedObjectTypeSyntax ParseTemplatedConstantBufferType(SyntaxToken token)
        {
            var lessThan = Match(SyntaxKind.LessThanToken);
            var type = ParseScalarOrVectorOrUserDefinedType();
            var greaterThan = Match(SyntaxKind.GreaterThanToken);
            var typeArgumentList = new TemplateArgumentListSyntax(lessThan,
                new SeparatedSyntaxList<ExpressionSyntax>(new List<SyntaxNodeBase> { type }),
                greaterThan);
            return new PredefinedObjectTypeSyntax(token, typeArgumentList);
        }

        private PredefinedObjectTypeSyntax ParseTextureType(SyntaxToken token)
        {
            TemplateArgumentListSyntax templateArgumentList = null;
            if (Current.Kind == SyntaxKind.LessThanToken)
            {
                var lessThan = Match(SyntaxKind.LessThanToken);
                var type = ParseModifiedType();
                var greaterThan = Match(SyntaxKind.GreaterThanToken);
                templateArgumentList = new TemplateArgumentListSyntax(lessThan,
                    new SeparatedSyntaxList<ExpressionSyntax>(new List<SyntaxNodeBase> { type }),
                    greaterThan);
            }
            return new PredefinedObjectTypeSyntax(token, templateArgumentList);
        }

        private PredefinedObjectTypeSyntax ParseMultisampledTextureType(SyntaxToken token)
        {
            var lessThan = Match(SyntaxKind.LessThanToken);
            var type = ParseModifiedType();

            var arguments = new List<SyntaxNodeBase> { type };
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                var comma = Match(SyntaxKind.CommaToken);

                ExpressionSyntax samples;
                try
                {
                    CommaIsSeparatorStack.Push(true);
                    _greaterThanTokenIsNotOperator = true;
                    samples = ParseExpression();
                }
                finally
                {
                    _greaterThanTokenIsNotOperator = false;
                    CommaIsSeparatorStack.Pop();
                }

                arguments.Add(comma);
                arguments.Add(samples);
            }

            var greaterThan = Match(SyntaxKind.GreaterThanToken);
            var typeArgumentList = new TemplateArgumentListSyntax(lessThan,
                new SeparatedSyntaxList<ExpressionSyntax>(arguments),
                greaterThan);
            return new PredefinedObjectTypeSyntax(token, typeArgumentList);
        }

        private ModifiedTypeSyntax ParseModifiedType()
        {
            var modifiers = new List<SyntaxToken>();
            ParseDeclarationModifiers(modifiers);

            var type = ParseType(false);

            return new ModifiedTypeSyntax(modifiers, type);
        }

        private NumericTypeSyntax ParseScalarOrVectorType()
        {
            if (SyntaxFacts.IsPredefinedScalarType(Current.Kind))
                return ParseScalarType(NextToken());
            if (SyntaxFacts.IsPredefinedVectorType(Current.Kind))
                return ParseVectorType(NextToken());
            return ParseScalarType(InsertMissingToken(SyntaxKind.FloatKeyword));
        }

        private TypeSyntax ParseScalarOrVectorOrUserDefinedType()
        {
            if (SyntaxFacts.IsPredefinedScalarType(Current.Kind))
                return ParseScalarType(NextToken());
            if (SyntaxFacts.IsPredefinedVectorType(Current.Kind))
                return ParseVectorType(NextToken());
            return ParseIdentifier();
        }

        private TypeSyntax ParseScalarOrVectorOrMatrixOrUserDefinedType()
        {
            if (SyntaxFacts.IsPredefinedScalarType(Current.Kind))
                return ParseScalarType(NextToken());
            if (SyntaxFacts.IsPredefinedVectorType(Current.Kind))
                return ParseVectorType(NextToken());
            if (SyntaxFacts.IsPredefinedMatrixType(Current.Kind))
                return ParseMatrixType(NextToken());
            return ParseIdentifier();
        }

        private TemplateArgumentListSyntax ParseTemplateArgumentList()
        {
            SyntaxToken lessThanToken, greaterThanToken;
            SeparatedSyntaxList<ExpressionSyntax> arguments;
            ParseArgumentList(SyntaxKind.LessThanToken, SyntaxKind.GreaterThanToken, true,
                out lessThanToken, out arguments, out greaterThanToken);

            return new TemplateArgumentListSyntax(lessThanToken, arguments, greaterThanToken);
        }

        private ScalarTypeSyntax ParseScalarType(SyntaxToken token)
        {
            var tokens = new List<SyntaxToken> { token };

            switch (token.Kind)
            {
                case SyntaxKind.UnsignedKeyword:
                    tokens.Add(Match(SyntaxKind.IntKeyword));
                    break;

                case SyntaxKind.SNormKeyword:
                case SyntaxKind.UNormKeyword:
                    tokens.Add(Match(SyntaxKind.IntKeyword));
                    break;
            }

            return new ScalarTypeSyntax(tokens);
        }

        private ScalarTypeSyntax ParseExpectedScalarType()
        {
            if (SyntaxFacts.IsPredefinedScalarType(Current.Kind))
                return ParseScalarType(NextToken());

            return ParseScalarType(InsertMissingToken(SyntaxKind.FloatKeyword));
        }

        private GenericVectorTypeSyntax ParseGenericVectorType(SyntaxToken vectorKeyword)
        {
            return new GenericVectorTypeSyntax(vectorKeyword,
                Match(SyntaxKind.LessThanToken),
                ParseExpectedScalarType(),
                Match(SyntaxKind.CommaToken),
                Match(SyntaxKind.IntegerLiteralToken),
                Match(SyntaxKind.GreaterThanToken));
        }

        private VectorTypeSyntax ParseVectorType(SyntaxToken typeToken)
        {
            return new VectorTypeSyntax(typeToken);
        }

        private GenericMatrixTypeSyntax ParseGenericMatrixType(SyntaxToken matrixKeyword)
        {
            return new GenericMatrixTypeSyntax(matrixKeyword,
                Match(SyntaxKind.LessThanToken),
                ParseExpectedScalarType(),
                Match(SyntaxKind.CommaToken),
                Match(SyntaxKind.IntegerLiteralToken),
                Match(SyntaxKind.CommaToken),
                Match(SyntaxKind.IntegerLiteralToken),
                Match(SyntaxKind.GreaterThanToken));
        }

        private MatrixTypeSyntax ParseMatrixType(SyntaxToken typeToken)
        {
            return new MatrixTypeSyntax(typeToken);
        }

        private ScanTypeFlags ScanType()
        {
            SyntaxToken lastTokenOfType;
            return ScanType(out lastTokenOfType);
        }

        private ScanTypeFlags ScanType(out SyntaxToken lastTokenOfType)
        {
            ScanTypeFlags result = ScanNonArrayType(out lastTokenOfType);

            if (result == ScanTypeFlags.NotType)
            {
                return result;
            }

            // Finally, check for array types.
            while (Current.Kind == SyntaxKind.OpenBracketToken)
            {
                NextToken();
                if (Current.Kind != SyntaxKind.IntegerLiteralToken && Current.Kind != SyntaxKind.IdentifierToken)
                {
                    lastTokenOfType = null;
                    return ScanTypeFlags.NotType;
                }
                NextToken();
                if (Current.Kind != SyntaxKind.CloseBracketToken)
                {
                    lastTokenOfType = null;
                    return ScanTypeFlags.NotType;
                }
                lastTokenOfType = NextToken();
                result = ScanTypeFlags.TypeOrExpression;
            }

            return result;
        }

        private ScanTypeFlags ScanNonArrayType(out SyntaxToken lastTokenOfType)
        {
            ScanTypeFlags result;
            if (Current.Kind == SyntaxKind.IdentifierToken && Current.ContextualKind == SyntaxKind.IdentifierToken)
            {
                lastTokenOfType = NextToken();
                while (Current.Kind == SyntaxKind.ColonColonToken)
                {
                    NextToken();
                    if (Current.Kind != SyntaxKind.IdentifierToken)
                    {
                        lastTokenOfType = null;
                        return ScanTypeFlags.NotType;
                    }
                    lastTokenOfType = NextToken();
                }
                result = ScanTypeFlags.TypeOrExpression;
            }
            else if (SyntaxFacts.IsPredefinedType(Current))
            {
                if (Current.Kind == SyntaxKind.UnsignedKeyword)
                {
                    if (Lookahead.Kind != SyntaxKind.IntKeyword)
                    {
                        // Can't be a type!
                        lastTokenOfType = null;
                        return ScanTypeFlags.NotType;
                    }
                    else
                    {
                        NextToken();
                    }
                }
                if (Lookahead.Kind == SyntaxKind.LessThanToken)
                {
                    NextToken();
                    // Generic type.
                    while (Current.Kind != SyntaxKind.EndOfFileToken)
                    {
                        NextToken();
                        if (Current.Kind == SyntaxKind.GreaterThanToken)
                            break;
                    }
                }
                lastTokenOfType = NextToken();
                result = ScanTypeFlags.MustBeType;
            }
            else if (Current.Kind == SyntaxKind.StructKeyword || Current.Kind == SyntaxKind.ClassKeyword)
            {
                var unused = ParseStructType(Current.Kind);
                lastTokenOfType = unused.CloseBraceToken;
                result = ScanTypeFlags.MustBeType;
            }
            else if (Current.Kind == SyntaxKind.InterfaceKeyword)
            {
                var unused = ParseInterfaceType();
                lastTokenOfType = unused.CloseBraceToken;
                result = ScanTypeFlags.MustBeType;
            }
            else
            {
                // Can't be a type!
                lastTokenOfType = null;
                return ScanTypeFlags.NotType;
            }

            return result;
        }

        private enum ScanTypeFlags
        {
            /// <summary>
            /// Definitely not a type name.
            /// </summary>
            NotType,

            /// <summary>
            /// Definitely a type name: either a predefined type (int, string, etc.) or an array type name (ending with a bracket).
            /// </summary>
            MustBeType,

            /// <summary>
            /// Might be a qualified type name or an expression.
            /// </summary>
            TypeOrExpression
        }

        private TypeSyntax ParseType(bool parentIsParameter)
        {
            return ParseUnderlyingType(false, parentIsParameter);
        }

        private TypeSyntax ParseReturnType()
        {
            return ParseUnderlyingType(true, false);
        }

        private TypeSyntax ParseUnderlyingType(bool allowVoid, bool parentIsParameter)
        {
            if (SyntaxFacts.IsPredefinedType(Current))
                return ParsePredefinedType(allowVoid, parentIsParameter);

            switch (Current.Kind)
            {
                case SyntaxKind.IdentifierToken:
                    return ParseName();
                case SyntaxKind.StructKeyword:
                    return ParseStructType(SyntaxKind.StructKeyword);
                case SyntaxKind.ClassKeyword:
                    return ParseStructType(SyntaxKind.ClassKeyword);
                case SyntaxKind.InterfaceKeyword:
                    return ParseInterfaceType();
                default:
                    return WithDiagnostic(CreateMissingIdentifierName(), DiagnosticId.TypeExpected);
            }
        }

        private NameSyntax ParseName()
        {
            var result = ParseIdentifier() as NameSyntax;

            while (Current.Kind == SyntaxKind.ColonColonToken)
            {
                var colonColon = Match(SyntaxKind.ColonColonToken);
                var right = new IdentifierNameSyntax(Match(SyntaxKind.IdentifierToken));

                result = new QualifiedNameSyntax(result, colonColon, right);
            }

            return result;
        }

        protected IdentifierNameSyntax ParseIdentifier()
        {
            if (_allowLinearAndPointAsIdentifiers && (Current.Kind == SyntaxKind.LinearKeyword || Current.Kind == SyntaxKind.PointKeyword))
                return new IdentifierNameSyntax(NextToken().WithKind(SyntaxKind.IdentifierToken));
            return new IdentifierNameSyntax(Match(SyntaxKind.IdentifierToken));
        }

        private ArrayRankSpecifierSyntax ParseArrayRankSpecifier(bool expectSize)
        {
            var open = Match(SyntaxKind.OpenBracketToken);
            ExpressionSyntax dimension = null;
            if (Current.Kind != SyntaxKind.CloseBracketToken)
            {
                if (IsPossibleExpression())
                {
                    var size = ParseExpression();
                    dimension = size;
                }
                else
                {
                    SkipBadTokens(
                        p => p.Current.Kind != SyntaxKind.CloseBracketToken,
                        p => p.IsTerminator());
                }
            }

            if (dimension == null && expectSize)
            {
                var diagnostics = open.Diagnostics.ToList();
                diagnostics.ReportTokenExpected(GetDiagnosticSourceRangeForMissingToken(), Lookahead, SyntaxKind.NumericLiteralExpression);
                open = open.WithDiagnostics(diagnostics);
            }

            // Eat the close brace and we're done.
            var close = Match(SyntaxKind.CloseBracketToken);

            return new ArrayRankSpecifierSyntax(open, dimension, close);
        }
    }
}