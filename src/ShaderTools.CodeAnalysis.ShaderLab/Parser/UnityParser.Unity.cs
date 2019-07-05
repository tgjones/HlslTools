using System;
using System.Collections.Generic;
using System.Threading;
using ShaderTools.CodeAnalysis.ShaderLab.Diagnostics;
using ShaderTools.CodeAnalysis.ShaderLab.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.ShaderLab.Parser
{
    internal partial class UnityParser
    {
        public CompilationUnitSyntax ParseUnityCompilationUnit(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            var shader = ParseUnityShader();

            var eof = Match(SyntaxKind.EndOfFileToken);

            return new CompilationUnitSyntax(shader, eof);
        }

        private ShaderSyntax ParseUnityShader()
        {
            var shaderKeyword = Match(SyntaxKind.ShaderKeyword);
            var nameToken = Match(SyntaxKind.StringLiteralToken);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            ShaderPropertiesSyntax properties = null;
            if (Current.Kind == SyntaxKind.PropertiesKeyword)
                properties = ParseUnityShaderProperties();

            ShaderIncludeSyntax cgInclude = null;
            if (Current.Kind == SyntaxKind.CgIncludeKeyword || Current.Kind == SyntaxKind.HlslIncludeKeyword)
                cgInclude = ParseUnityShaderInclude();

            var statements = new List<SyntaxNode>();
            while (Current.Kind == SyntaxKind.SubShaderKeyword || Current.Kind == SyntaxKind.CategoryKeyword)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.SubShaderKeyword:
                        statements.Add(ParseUnitySubShader());
                        break;
                    case SyntaxKind.CategoryKeyword:
                        statements.Add(ParseUnityCategory());
                        break;
                }
            }

            var stateProperties = new List<CommandSyntax>();
            var shouldContinue = true;
            while (shouldContinue && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.FallbackKeyword:
                        stateProperties.Add(ParseUnityFallback());
                        break;
                    case SyntaxKind.CustomEditorKeyword:
                        stateProperties.Add(ParseUnityCustomEditor());
                        break;
                    case SyntaxKind.DependencyKeyword:
                        stateProperties.Add(ParseUnityDependency());
                        break;

                    default:
                        shouldContinue = false;
                        break;
                }
            }

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new ShaderSyntax(
                shaderKeyword,
                nameToken,
                openBraceToken,
                properties,
                cgInclude,
                statements,
                stateProperties,
                closeBraceToken);
        }

        private ShaderPropertiesSyntax ParseUnityShaderProperties()
        {
            var propertiesKeyword = Match(SyntaxKind.PropertiesKeyword);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var properties = new List<ShaderPropertySyntax>();
            while (Current.Kind == SyntaxKind.IdentifierToken || Current.Kind == SyntaxKind.OpenBracketToken)
                properties.Add(ParseUnityShaderProperty());

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new ShaderPropertiesSyntax(propertiesKeyword, openBraceToken, properties, closeBraceToken);
        }

        private ShaderPropertySyntax ParseUnityShaderProperty()
        {
            var attributes = new List<ShaderPropertyAttributeSyntax>();
            while (Current.Kind == SyntaxKind.OpenBracketToken)
                attributes.Add(ParseUnityShaderPropertyAttribute());

            var nameToken = Match(SyntaxKind.IdentifierToken);
            var openParenToken = Match(SyntaxKind.OpenParenToken);
            var displayNameToken = Match(SyntaxKind.StringLiteralToken);
            var commaToken = Match(SyntaxKind.CommaToken);
            var propertyType = ParseUnityShaderPropertyType();
            var closeParenToken = Match(SyntaxKind.CloseParenToken);
            var equalsToken = Match(SyntaxKind.EqualsToken);
            var defaultValue = ParseUnityShaderPropertyDefaultValue(propertyType);

            return new ShaderPropertySyntax(
                attributes,
                nameToken,
                openParenToken,
                displayNameToken,
                commaToken,
                propertyType,
                closeParenToken,
                equalsToken,
                defaultValue);
        }

        private ShaderPropertyAttributeSyntax ParseUnityShaderPropertyAttribute()
        {
            var openBracketToken = Match(SyntaxKind.OpenBracketToken);
            var name = Match(SyntaxKind.IdentifierToken);
            var arguments = ParseUnityShaderPropertyAttributeArgumentList();
            var closeBracketToken = Match(SyntaxKind.CloseBracketToken);

            return new ShaderPropertyAttributeSyntax(
                openBracketToken,
                name,
                arguments,
                closeBracketToken);
        }

        private AttributeArgumentListSyntax ParseUnityShaderPropertyAttributeArgumentList()
        {
            AttributeArgumentListSyntax result = null;

            if (Current.Kind == SyntaxKind.OpenParenToken)
            {
                var openParen = Match(SyntaxKind.OpenParenToken);

                var argumentsList = new List<SyntaxNodeBase>();
                argumentsList.Add(ParseUnityExpression());

                while (Current.Kind == SyntaxKind.CommaToken)
                {
                    argumentsList.Add(Match(SyntaxKind.CommaToken));
                    argumentsList.Add(ParseUnityExpression());
                }

                var closeParen = Match(SyntaxKind.CloseParenToken);

                result = new AttributeArgumentListSyntax(
                    openParen,
                    new SeparatedSyntaxList<LiteralExpressionSyntax>(argumentsList),
                    closeParen);
            }

            return result;
        }

        private ExpressionSyntax ParseUnityExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.IdentifierToken:
                    return ParseUnityEnumNameExpression();

                default:
                    return ParseTerm();
            }
        }

        private ExpressionSyntax ParseTerm()
        {
            ExpressionSyntax expr;

            var tk = Current.Kind;
            switch (tk)
            {
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.IntegerLiteralToken:
                case SyntaxKind.FloatLiteralToken:
                    expr = new LiteralExpressionSyntax(SyntaxFacts.GetLiteralExpression(Current.Kind), NextToken());
                    break;
                default:
                    expr = new EnumNameExpressionSyntax(new List<SyntaxToken> { InsertMissingToken(SyntaxKind.IdentifierToken) });

                    if (tk == SyntaxKind.EndOfFileToken)
                        expr = WithDiagnostic(expr, DiagnosticId.ExpressionExpected);
                    else
                        expr = WithDiagnostic(expr, DiagnosticId.InvalidExprTerm, tk.GetText());

                    break;
            }

            return expr;
        }

        private ExpressionSyntax ParseUnityEnumNameExpression()
        {
            var nameTokens = new List<SyntaxToken>();
            while (Current.Kind != SyntaxKind.CommaToken && Current.Kind != SyntaxKind.CloseParenToken)
                nameTokens.Add(NextToken());

            return new EnumNameExpressionSyntax(nameTokens);
        }

        private ShaderPropertyTypeSyntax ParseUnityShaderPropertyType()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.RangeKeyword:
                    return ParseUnityShaderPropertyRangeType();
                default:
                    return ParseUnityShaderPropertySimpleType();
            }
        }

        private ShaderPropertyRangeTypeSyntax ParseUnityShaderPropertyRangeType()
        {
            var rangeKeyword = Match(SyntaxKind.RangeKeyword);
            var openParenToken = Match(SyntaxKind.OpenParenToken);
            var minValue = ParseUnityPossiblyNegativeNumericLiteralExpression();
            var commaToken = Match(SyntaxKind.CommaToken);
            var maxValue = ParseUnityPossiblyNegativeNumericLiteralExpression();

            var closeParenToken = Match(SyntaxKind.CloseParenToken);

            return new ShaderPropertyRangeTypeSyntax(
                rangeKeyword,
                openParenToken,
                minValue,
                commaToken,
                maxValue,
                closeParenToken);
        }

        private ExpressionSyntax ParseUnityPossiblyNegativeNumericLiteralExpression()
        {
            if (Current.Kind == SyntaxKind.MinusToken)
            {
                var operatorToken = NextToken();
                var literalToken = MatchOneOf(SyntaxKind.IntegerLiteralToken, SyntaxKind.FloatLiteralToken);
                var operand = new LiteralExpressionSyntax(
                    SyntaxFacts.GetLiteralExpression(literalToken.Kind),
                    literalToken);
                return new PrefixUnaryExpressionSyntax(
                    SyntaxFacts.GetPrefixUnaryExpression(operatorToken.Kind),
                    operatorToken, operand);
            }
            else
            {
                var literalToken = MatchOneOf(SyntaxKind.IntegerLiteralToken, SyntaxKind.FloatLiteralToken);
                return new LiteralExpressionSyntax(
                    SyntaxFacts.GetLiteralExpression(literalToken.Kind),
                    literalToken);
            }
        }

        private ShaderPropertySimpleTypeSyntax ParseUnityShaderPropertySimpleType()
        {
            var typeKeyword = MatchOneOf(
                SyntaxKind.IntKeyword,
                SyntaxKind.FloatKeyword,
                SyntaxKind.ColorKeyword,
                SyntaxKind.VectorKeyword,
                SyntaxKind._2DKeyword,
                SyntaxKind._3DKeyword,
                SyntaxKind.CubeKeyword,
                SyntaxKind.AnyKeyword);

            return new ShaderPropertySimpleTypeSyntax(typeKeyword);
        }

        private ShaderPropertyDefaultValueSyntax ParseUnityShaderPropertyDefaultValue(ShaderPropertyTypeSyntax propertyType)
        {
            switch (propertyType.TypeKind)
            {
                case SyntaxKind.RangeKeyword:
                case SyntaxKind.FloatKeyword:
                case SyntaxKind.IntKeyword:
                    return ParseUnityShaderPropertyNumericDefaultValue(propertyType.TypeKind);
                case SyntaxKind.ColorKeyword:
                case SyntaxKind.VectorKeyword:
                    return ParseUnityShaderPropertyVectorDefaultValue();
                case SyntaxKind._2DKeyword:
                case SyntaxKind._3DKeyword:
                case SyntaxKind.CubeKeyword:
                case SyntaxKind.AnyKeyword:
                    return ParseUnityShaderPropertyTextureDefaultValue();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ShaderPropertyNumericDefaultValueSyntax ParseUnityShaderPropertyNumericDefaultValue(SyntaxKind propertyType)
        {
            var number = ParseUnityPossiblyNegativeNumericLiteralExpression();
            return new ShaderPropertyNumericDefaultValueSyntax(number);
        }

        private ShaderPropertyVectorDefaultValueSyntax ParseUnityShaderPropertyVectorDefaultValue()
        {
            var vector = ParseUnityVector();

            return new ShaderPropertyVectorDefaultValueSyntax(vector);
        }

        private BaseVectorSyntax ParseUnityVector()
        {
            var openParenToken = Match(SyntaxKind.OpenParenToken);
            var x = ParseUnityPossiblyNegativeNumericLiteralExpression();
            var firstCommaToken = Match(SyntaxKind.CommaToken);
            var y = ParseUnityPossiblyNegativeNumericLiteralExpression();
            var secondCommaToken = Match(SyntaxKind.CommaToken);
            var z = ParseUnityPossiblyNegativeNumericLiteralExpression();

            if (Current.Kind == SyntaxKind.CloseParenToken)
                return new Vector3Syntax(
                    openParenToken,
                    x,
                    firstCommaToken,
                    y,
                    secondCommaToken,
                    z,
                    NextToken());

            var thirdCommaToken = Match(SyntaxKind.CommaToken);
            var w = ParseUnityPossiblyNegativeNumericLiteralExpression();
            var closeParenToken = Match(SyntaxKind.CloseParenToken);

            return new Vector4Syntax(
                openParenToken,
                x,
                firstCommaToken,
                y,
                secondCommaToken,
                z,
                thirdCommaToken,
                w,
                closeParenToken);
        }

        private ShaderPropertyTextureDefaultValueSyntax ParseUnityShaderPropertyTextureDefaultValue()
        {
            var defaultTextureToken = Match(SyntaxKind.StringLiteralToken);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var options = new List<SyntaxToken>();
            while (Current.Kind != SyntaxKind.CloseBraceToken)
                options.Add(Match(SyntaxKind.IdentifierToken));

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new ShaderPropertyTextureDefaultValueSyntax(
                defaultTextureToken,
                openBraceToken,
                options,
                closeBraceToken);
        }

        private CategorySyntax ParseUnityCategory()
        {
            var categoryKeyword = Match(SyntaxKind.CategoryKeyword);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var statements = new List<SyntaxNode>();
            var shouldContinue = true;
            while (shouldContinue && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.TagsKeyword:
                        statements.Add(ParseUnityShaderTags());
                        break;
                    case SyntaxKind.SubShaderKeyword:
                        statements.Add(ParseUnitySubShader());
                        break;

                    default:
                        shouldContinue = TryParseStateProperty(statements);
                        break;
                }
            }

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new CategorySyntax(
                categoryKeyword,
                openBraceToken,
                statements,
                closeBraceToken);
        }

        private SubShaderSyntax ParseUnitySubShader()
        {
            var subShaderKeyword = Match(SyntaxKind.SubShaderKeyword);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var statements = new List<SyntaxNode>();
            var shouldContinue = true;
            while (shouldContinue && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.TagsKeyword:
                        statements.Add(ParseUnityShaderTags());
                        break;
                    case SyntaxKind.PassKeyword:
                        statements.Add(ParseUnityPass());
                        break;
                    case SyntaxKind.UsePassKeyword:
                        statements.Add(ParseUnityUsePass());
                        break;
                    case SyntaxKind.GrabPassKeyword:
                        statements.Add(ParseUnityGrabPass());
                        break;
                    case SyntaxKind.CgProgramKeyword:
                    case SyntaxKind.HlslProgramKeyword:
                        statements.Add(ParseUnityShaderProgram());
                        break;
                    case SyntaxKind.CgIncludeKeyword:
                    case SyntaxKind.HlslIncludeKeyword:
                        statements.Add(ParseUnityShaderInclude());
                        break;

                    default:
                        shouldContinue = TryParseStateProperty(statements);
                        break;
                }
            }

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new SubShaderSyntax(
                subShaderKeyword,
                openBraceToken,
                statements,
                closeBraceToken);
        }

        private ShaderTagsSyntax ParseUnityShaderTags()
        {
            var tagsKeyword = Match(SyntaxKind.TagsKeyword);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var tags = new List<ShaderTagSyntax>();
            while (Current.Kind == SyntaxKind.StringLiteralToken)
                tags.Add(ParseUnityShaderTag());

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new ShaderTagsSyntax(tagsKeyword, openBraceToken, tags, closeBraceToken);
        }

        private ShaderTagSyntax ParseUnityShaderTag()
        {
            var nameToken = Match(SyntaxKind.StringLiteralToken);
            var equalsToken = Match(SyntaxKind.EqualsToken);
            var valueToken = Match(SyntaxKind.StringLiteralToken);

            return new ShaderTagSyntax(nameToken, equalsToken, valueToken);
        }

        private PassSyntax ParseUnityPass()
        {
            var passKeyword = Match(SyntaxKind.PassKeyword);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var statements = new List<SyntaxNode>();
            var shouldContinue = true;
            while (shouldContinue && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.TagsKeyword:
                        statements.Add(ParseUnityShaderTags());
                        break;
                    case SyntaxKind.NameKeyword:
                        statements.Add(ParseUnityName());
                        break;

                    default:
                        shouldContinue = TryParseStateProperty(statements);
                        break;
                }
            }

            ShaderProgramSyntax shaderProgram = null;
            if (Current.Kind == SyntaxKind.CgProgramKeyword || Current.Kind == SyntaxKind.HlslProgramKeyword)
                shaderProgram = ParseUnityShaderProgram();

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new PassSyntax(
                passKeyword,
                openBraceToken,
                statements,
                shaderProgram,
                closeBraceToken);
        }

        private GrabPassSyntax ParseUnityGrabPass()
        {
            var grabPassKeyword = Match(SyntaxKind.GrabPassKeyword);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var statements = new List<SyntaxNode>();
            var shouldContinue = true;
            while (shouldContinue && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.TagsKeyword:
                        statements.Add(ParseUnityShaderTags());
                        break;
                    case SyntaxKind.NameKeyword:
                        statements.Add(ParseUnityName());
                        break;

                    default:
                        shouldContinue = TryParseStateProperty(statements);
                        break;
                }
            }

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new GrabPassSyntax(
                grabPassKeyword,
                openBraceToken,
                statements,
                closeBraceToken);
        }

        private UsePassSyntax ParseUnityUsePass()
        {
            var usePassKeyword = Match(SyntaxKind.UsePassKeyword);
            var passName = Match(SyntaxKind.StringLiteralToken);

            return new UsePassSyntax(
                usePassKeyword,
                passName);
        }

        private ShaderProgramSyntax ParseUnityShaderProgram()
        {
            var cgProgramKeyword = MatchOneOf(SyntaxKind.CgProgramKeyword, SyntaxKind.HlslProgramKeyword);

            var programKind = cgProgramKeyword.Kind == SyntaxKind.CgProgramKeyword
                ? SyntaxKind.CgProgram
                : SyntaxKind.HlslProgram;

            var endKeywordKind = cgProgramKeyword.Kind == SyntaxKind.CgProgramKeyword
                ? SyntaxKind.EndCgKeyword
                : SyntaxKind.EndHlslKeyword;

            var endCgKeyword = Match(endKeywordKind);

            return new ShaderProgramSyntax(programKind, cgProgramKeyword, endCgKeyword);
        }

        private ShaderIncludeSyntax ParseUnityShaderInclude()
        {
            var cgIncludeKeyword = MatchOneOf(SyntaxKind.CgIncludeKeyword, SyntaxKind.HlslIncludeKeyword);

            var programKind = cgIncludeKeyword.Kind == SyntaxKind.CgIncludeKeyword
                ? SyntaxKind.CgInclude
                : SyntaxKind.HlslInclude;

            var endKeywordKind = cgIncludeKeyword.Kind == SyntaxKind.CgIncludeKeyword
                ? SyntaxKind.EndCgKeyword
                : SyntaxKind.EndHlslKeyword;

            var endCgKeyword = Match(endKeywordKind);

            return new ShaderIncludeSyntax(programKind, cgIncludeKeyword, endCgKeyword);
        }

        private void ParseUnityCgProgramOrInclude(out SyntaxToken endCgKeyword)
        {
            endCgKeyword = Match(SyntaxKind.EndCgKeyword);
        }

        private bool TryParseStateProperty(List<SyntaxNode> stateProperties)
        {
            switch (Current.Kind)
            {
                case SyntaxKind.CullKeyword:
                    stateProperties.Add(ParseUnityCull());
                    return true;
                case SyntaxKind.ZWriteKeyword:
                    stateProperties.Add(ParseUnityZWrite());
                    return true;
                case SyntaxKind.ZTestKeyword:
                    stateProperties.Add(ParseUnityZTest());
                    return true;
                case SyntaxKind.OffsetKeyword:
                    stateProperties.Add(ParseUnityOffset());
                    return true;
                case SyntaxKind.BlendKeyword:
                    stateProperties.Add(ParseUnityBlend());
                    return true;
                case SyntaxKind.ColorMaskKeyword:
                    stateProperties.Add(ParseUnityColorMask());
                    return true;
                case SyntaxKind.LodKeyword:
                    stateProperties.Add(ParseUnityLod());
                    return true;
                case SyntaxKind.LightingKeyword:
                    stateProperties.Add(ParseUnityLighting());
                    return true;
                case SyntaxKind.StencilKeyword:
                    stateProperties.Add(ParseUnityStencil());
                    return true;
                case SyntaxKind.MaterialKeyword:
                    stateProperties.Add(ParseUnityMaterial());
                    return true;
                case SyntaxKind.FogKeyword:
                    stateProperties.Add(ParseUnityFog());
                    return true;
                case SyntaxKind.SeparateSpecularKeyword:
                    stateProperties.Add(ParseUnitySeparateSpecular());
                    return true;
                case SyntaxKind.SetTextureKeyword:
                    stateProperties.Add(ParseUnitySetTexture());
                    return true;
                case SyntaxKind.AlphaTestKeyword:
                    stateProperties.Add(ParseUnityAlphaTest());
                    return true;
                case SyntaxKind.AlphaToMaskKeyword:
                    stateProperties.Add(ParseUnityAlphaToMask());
                    return true;
                case SyntaxKind.ColorMaterialKeyword:
                    stateProperties.Add(ParseUnityColorMaterial());
                    return true;
                case SyntaxKind.BindChannelsKeyword:
                    stateProperties.Add(ParseUnityBindChannels());
                    return true;

                default:
                    return false;
            }
        }

        private CommandVariableValueSyntax ParseUnityCommandVariableValue()
        {
            var openBracketToken = NextToken();
            var identifier = Match(SyntaxKind.IdentifierToken);
            var closeBracketToken = Match(SyntaxKind.CloseBracketToken);

            return new CommandVariableValueSyntax(
                openBracketToken,
                identifier,
                closeBracketToken);
        }

        private CommandValueSyntax ParseUnityCommandValue(SyntaxKind preferred, params SyntaxKind[] otherOptions)
        {
            if (Current.Kind == SyntaxKind.OpenBracketToken)
                return ParseUnityCommandVariableValue();

            var valueToken = MatchOneOf(preferred, otherOptions);
            return new CommandConstantValueSyntax(valueToken);
        }

        private CommandSyntax ParseUnityFallback()
        {
            var keyword = Match(SyntaxKind.FallbackKeyword);
            var value = MatchOneOf(SyntaxKind.IdentifierToken, SyntaxKind.StringLiteralToken);

            return new CommandFallbackSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityCustomEditor()
        {
            var keyword = Match(SyntaxKind.CustomEditorKeyword);
            var value = Match(SyntaxKind.StringLiteralToken);

            return new CommandCustomEditorSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityDependency()
        {
            var keyword = Match(SyntaxKind.DependencyKeyword);
            var name = Match(SyntaxKind.StringLiteralToken);
            var equalsToken = Match(SyntaxKind.EqualsToken);
            var dependentShaderToken = Match(SyntaxKind.StringLiteralToken);

            return new CommandDependencySyntax(keyword, name, equalsToken, dependentShaderToken);
        }

        private CommandSyntax ParseUnityCull()
        {
            var keyword = Match(SyntaxKind.CullKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            return new CommandCullSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityZWrite()
        {
            var keyword = Match(SyntaxKind.ZWriteKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            return new CommandZWriteSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityZTest()
        {
            var keyword = Match(SyntaxKind.ZTestKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            return new CommandZTestSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityBlend()
        {
            var keyword = Match(SyntaxKind.BlendKeyword);

            if (Current.Kind == SyntaxKind.IdentifierToken && string.Equals(Current.Text, "Off", StringComparison.OrdinalIgnoreCase))
            {
                return new CommandBlendOffSyntax(keyword, NextToken());
            }

            var srcFactor = ParseUnityCommandValue(SyntaxKind.IdentifierToken);
            var dstFactor = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            if (Current.Kind == SyntaxKind.CommaToken)
            {
                var commaToken = Match(SyntaxKind.CommaToken);
                var srcFactorA = ParseUnityCommandValue(SyntaxKind.IdentifierToken);
                var dstFactorA = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

                return new CommandBlendColorAlphaSyntax(
                    keyword,
                    srcFactor,
                    dstFactor,
                    commaToken,
                    srcFactorA,
                    dstFactorA);
            }

            return new CommandBlendColorSyntax(
                keyword,
                srcFactor,
                dstFactor);
        }

        private CommandSyntax ParseUnityColorMask()
        {
            var keyword = Match(SyntaxKind.ColorMaskKeyword);
            var maskToken = ParseUnityCommandValue(
                SyntaxKind.IdentifierToken,
                SyntaxKind.IntegerLiteralToken);

            return new CommandColorMaskSyntax(keyword, maskToken);
        }

        private CommandSyntax ParseUnityLod()
        {
            var keyword = Match(SyntaxKind.LodKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IntegerLiteralToken);

            return new CommandLodSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityName()
        {
            var keyword = Match(SyntaxKind.NameKeyword);
            var value = Match(SyntaxKind.StringLiteralToken);

            return new CommandNameSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityLighting()
        {
            var keyword = Match(SyntaxKind.LightingKeyword);
            var value = Match(SyntaxKind.IdentifierToken);

            return new CommandLightingSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityOffset()
        {
            var keyword = Match(SyntaxKind.OffsetKeyword);
            var factor = ParseUnityCommandValue(SyntaxKind.FloatLiteralToken, SyntaxKind.IntegerLiteralToken);
            var commaToken = Match(SyntaxKind.CommaToken);
            var units = ParseUnityCommandValue(SyntaxKind.FloatLiteralToken, SyntaxKind.IntegerLiteralToken);

            return new CommandOffsetSyntax(keyword, factor, commaToken, units);
        }

        private CommandSyntax ParseUnityStencil()
        {
            var keyword = Match(SyntaxKind.StencilKeyword);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var stateProperties = new List<CommandSyntax>();
            var shouldContinue = true;
            while (shouldContinue && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.RefKeyword:
                        stateProperties.Add(ParseUnityStencilRef());
                        break;
                    case SyntaxKind.ReadMaskKeyword:
                        stateProperties.Add(ParseUnityStencilReadMask());
                        break;
                    case SyntaxKind.WriteMaskKeyword:
                        stateProperties.Add(ParseUnityStencilWriteMask());
                        break;
                    case SyntaxKind.CompKeyword:
                    case SyntaxKind.CompBackKeyword:
                    case SyntaxKind.CompFrontKeyword:
                        stateProperties.Add(ParseUnityStencilComp());
                        break;
                    case SyntaxKind.PassKeyword:
                        stateProperties.Add(ParseUnityStencilPass());
                        break;
                    case SyntaxKind.FailKeyword:
                        stateProperties.Add(ParseUnityStencilFail());
                        break;
                    case SyntaxKind.ZFailKeyword:
                        stateProperties.Add(ParseUnityStencilZFail());
                        break;

                    default:
                        shouldContinue = false;
                        break;
                }
            }

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new CommandStencilSyntax(
                keyword,
                openBraceToken,
                stateProperties,
                closeBraceToken);
        }

        private CommandSyntax ParseUnityStencilRef()
        {
            var keyword = Match(SyntaxKind.RefKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IntegerLiteralToken);

            return new CommandStencilRefSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityStencilReadMask()
        {
            var keyword = Match(SyntaxKind.ReadMaskKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IntegerLiteralToken);

            return new CommandStencilReadMaskSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityStencilWriteMask()
        {
            var keyword = Match(SyntaxKind.WriteMaskKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IntegerLiteralToken);

            return new CommandStencilWriteMaskSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityStencilComp()
        {
            var keyword = NextToken();
            var value = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            return new CommandStencilCompSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityStencilPass()
        {
            var keyword = Match(SyntaxKind.PassKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            return new CommandStencilPassSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityStencilFail()
        {
            var keyword = Match(SyntaxKind.FailKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            return new CommandStencilFailSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityStencilZFail()
        {
            var keyword = Match(SyntaxKind.ZFailKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            return new CommandStencilZFailSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityMaterial()
        {
            var keyword = Match(SyntaxKind.MaterialKeyword);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var stateProperties = new List<CommandSyntax>();
            var shouldContinue = true;
            while (shouldContinue && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.DiffuseKeyword:
                        stateProperties.Add(ParseUnityMaterialDiffuse());
                        break;
                    case SyntaxKind.AmbientKeyword:
                        stateProperties.Add(ParseUnityMaterialAmbient());
                        break;
                    case SyntaxKind.ShininessKeyword:
                        stateProperties.Add(ParseUnityMaterialShininess());
                        break;
                    case SyntaxKind.SpecularKeyword:
                        stateProperties.Add(ParseUnityMaterialSpecular());
                        break;
                    case SyntaxKind.EmissionKeyword:
                        stateProperties.Add(ParseUnityMaterialEmission());
                        break;

                    default:
                        shouldContinue = false;
                        break;
                }
            }

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new CommandMaterialSyntax(
                keyword,
                openBraceToken,
                stateProperties,
                closeBraceToken);
        }

        private CommandValueSyntax ParseUnityCommandColorValue()
        {
            if (Current.Kind == SyntaxKind.OpenBracketToken)
                return ParseUnityCommandVariableValue();

            var vector = ParseUnityVector();
            return new CommandConstantColorValueSyntax(vector);
        }

        private CommandSyntax ParseUnityMaterialDiffuse()
        {
            var keyword = Match(SyntaxKind.DiffuseKeyword);
            var value = ParseUnityCommandColorValue();

            return new CommandMaterialDiffuseSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityMaterialAmbient()
        {
            var keyword = Match(SyntaxKind.AmbientKeyword);
            var value = ParseUnityCommandColorValue();

            return new CommandMaterialAmbientSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityMaterialShininess()
        {
            var keyword = Match(SyntaxKind.ShininessKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.FloatLiteralToken, SyntaxKind.IntegerLiteralToken);

            return new CommandMaterialShininessSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityMaterialSpecular()
        {
            var keyword = Match(SyntaxKind.SpecularKeyword);
            var value = ParseUnityCommandColorValue();

            return new CommandMaterialSpecularSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityMaterialEmission()
        {
            var keyword = Match(SyntaxKind.EmissionKeyword);
            var value = ParseUnityCommandColorValue();

            return new CommandMaterialEmissionSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityFog()
        {
            var keyword = Match(SyntaxKind.FogKeyword);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var commands = new List<CommandSyntax>();
            var shouldContinue = true;
            while (shouldContinue && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.ModeKeyword:
                        commands.Add(ParseUnityFogMode());
                        break;
                    case SyntaxKind.ColorKeyword:
                        commands.Add(ParseUnityFogColor());
                        break;
                    case SyntaxKind.DensityKeyword:
                        commands.Add(ParseUnityFogDensity());
                        break;
                    case SyntaxKind.RangeKeyword:
                        commands.Add(ParseUnityFogRange());
                        break;

                    default:
                        shouldContinue = false;
                        break;
                }
            }

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new CommandFogSyntax(
                keyword,
                openBraceToken,
                commands,
                closeBraceToken);
        }

        private CommandSyntax ParseUnityFogMode()
        {
            var keyword = Match(SyntaxKind.ModeKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            return new CommandFogModeSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityFogColor()
        {
            var keyword = Match(SyntaxKind.ColorKeyword);
            var value = ParseUnityCommandColorValue();

            return new CommandFogColorSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityFogDensity()
        {
            var keyword = Match(SyntaxKind.DensityKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.FloatLiteralToken, SyntaxKind.IntegerLiteralToken);

            return new CommandFogDensitySyntax(keyword, value);
        }

        private CommandSyntax ParseUnityFogRange()
        {
            var keyword = Match(SyntaxKind.RangeKeyword);
            var near = ParseUnityCommandValue(SyntaxKind.FloatLiteralToken, SyntaxKind.IntegerLiteralToken);
            var commaToken = Match(SyntaxKind.CommaToken);
            var far = ParseUnityCommandValue(SyntaxKind.FloatLiteralToken, SyntaxKind.IntegerLiteralToken);

            return new CommandFogRangeSyntax(keyword, near, commaToken, far);
        }

        private CommandSyntax ParseUnitySeparateSpecular()
        {
            var keyword = Match(SyntaxKind.SeparateSpecularKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            return new CommandSeparateSpecularSyntax(keyword, value);
        }

        private CommandSyntax ParseUnitySetTexture()
        {
            var keyword = Match(SyntaxKind.SetTextureKeyword);
            var textureName = ParseUnityCommandVariableValue();
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var commands = new List<CommandSyntax>();
            var shouldContinue = true;
            while (shouldContinue && Current.Kind != SyntaxKind.CloseBraceToken)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.ConstantColorKeyword:
                        commands.Add(ParseUnitySetTextureConstantColor());
                        break;
                    case SyntaxKind.MatrixKeyword:
                        commands.Add(ParseUnitySetTextureMatrix());
                        break;
                    case SyntaxKind.CombineKeyword:
                        commands.Add(ParseUnitySetTextureCombine());
                        break;

                    default:
                        shouldContinue = false;
                        break;
                }
            }

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new CommandSetTextureSyntax(
                keyword,
                textureName,
                openBraceToken,
                commands,
                closeBraceToken);
        }

        private CommandSyntax ParseUnitySetTextureConstantColor()
        {
            var keyword = Match(SyntaxKind.ConstantColorKeyword);
            var value = ParseUnityCommandColorValue();

            return new CommandSetTextureConstantColorSyntax(keyword, value);
        }

        private CommandSyntax ParseUnitySetTextureMatrix()
        {
            var keyword = Match(SyntaxKind.MatrixKeyword);
            var value = ParseUnityCommandVariableValue();

            return new CommandSetTextureMatrixSyntax(keyword, value);
        }

        private CommandSyntax ParseUnitySetTextureCombine()
        {
            var keyword = Match(SyntaxKind.CombineKeyword);
            var value = ParseUnitySetTextureCombineValue();

            SyntaxToken modifierToken = null;
            if (Current.Kind == SyntaxKind.DoubleKeyword || Current.Kind == SyntaxKind.QuadKeyword)
                modifierToken = NextToken();

            CommandSetTextureCombineAlphaComponentSyntax alphaComponent = null;
            if (Current.Kind == SyntaxKind.CommaToken)
                alphaComponent = ParseUnityCommandSetTextureCombineAlphaComponent();

            return new CommandSetTextureCombineSyntax(keyword, value, modifierToken, alphaComponent);
        }

        private CommandSetTextureCombineAlphaComponentSyntax ParseUnityCommandSetTextureCombineAlphaComponent()
        {
            var commaToken = Match(SyntaxKind.CommaToken);
            var value = ParseUnitySetTextureCombineValue();

            return new CommandSetTextureCombineAlphaComponentSyntax(
                commaToken,
                value);
        }

        private BaseCommandSetTextureCombineValueSyntax ParseUnitySetTextureCombineValue()
        {
            var source1 = ParseUnitySetTextureCombineSource();

            switch (Current.Kind)
            {
                case SyntaxKind.LerpKeyword:
                    return ParseUnitySetTextureCombineLerpValue(source1);

                case SyntaxKind.AsteriskToken:
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    var operatorToken = MatchOneOf(SyntaxKind.AsteriskToken, SyntaxKind.PlusToken, SyntaxKind.MinusToken);
                    var source2 = ParseUnitySetTextureCombineSource();

                    if (operatorToken.Kind == SyntaxKind.AsteriskToken && Current.Kind == SyntaxKind.PlusToken)
                        return ParseUnitySetTextureCombineMultiplyAlphaValue(source1, operatorToken, source2);

                    return new CommandSetTextureCombineBinaryValueSyntax(source1, operatorToken, source2);

                default:
                    return new CommandSetTextureCombineUnaryValueSyntax(source1);
            }
        }

        private CommandSetTextureCombineSourceSyntax ParseUnitySetTextureCombineSource()
        {
            var sourceToken = Match(SyntaxKind.IdentifierToken);
            var alphaKeyword = NextTokenIf(SyntaxKind.AlphaKeyword);

            return new CommandSetTextureCombineSourceSyntax(
                sourceToken,
                alphaKeyword);
        }

        private BaseCommandSetTextureCombineValueSyntax ParseUnitySetTextureCombineLerpValue(CommandSetTextureCombineSourceSyntax source1)
        {
            var lerpKeyword = Match(SyntaxKind.LerpKeyword);
            var openParenToken = Match(SyntaxKind.OpenParenToken);
            var source2 = ParseUnitySetTextureCombineSource();
            var closeParenToken = Match(SyntaxKind.CloseParenToken);
            var source3 = ParseUnitySetTextureCombineSource();

            return new CommandSetTextureCombineLerpValueSyntax(
                source1,
                lerpKeyword,
                openParenToken,
                source2,
                closeParenToken,
                source3);
        }

        private BaseCommandSetTextureCombineValueSyntax ParseUnitySetTextureCombineMultiplyAlphaValue(CommandSetTextureCombineSourceSyntax source1, SyntaxToken operatorToken, CommandSetTextureCombineSourceSyntax source2)
        {
            var plusToken = Match(SyntaxKind.PlusToken);
            var source3 = ParseUnitySetTextureCombineSource();

            return new CommandSetTextureCombineMultiplyAlphaValueSyntax(
                source1,
                operatorToken,
                source2,
                plusToken,
                source3);
        }

        private CommandSyntax ParseUnityAlphaTest()
        {
            var keyword = Match(SyntaxKind.AlphaTestKeyword);
            var identifier = Match(SyntaxKind.IdentifierToken);

            if (string.Equals(identifier.Text, "Off", StringComparison.OrdinalIgnoreCase))
                return new CommandAlphaTestOffSyntax(keyword, identifier);

            var alphaValue = ParseUnityCommandValue(SyntaxKind.FloatLiteralToken, SyntaxKind.IntegerLiteralToken);
            return new CommandAlphaTestComparisonSyntax(keyword, identifier, alphaValue);
        }

        private CommandSyntax ParseUnityAlphaToMask()
        {
            var keyword = Match(SyntaxKind.AlphaToMaskKeyword);
            var value = ParseUnityCommandValue(SyntaxKind.IdentifierToken);

            return new CommandAlphaToMaskSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityColorMaterial()
        {
            var keyword = Match(SyntaxKind.ColorMaterialKeyword);
            var value = Match(SyntaxKind.IdentifierToken);

            return new CommandColorMaterialSyntax(keyword, value);
        }

        private CommandSyntax ParseUnityBindChannels()
        {
            var keyword = Match(SyntaxKind.BindChannelsKeyword);
            var openBraceToken = Match(SyntaxKind.OpenBraceToken);

            var commands = new List<CommandSyntax>();
            while (Current.Kind == SyntaxKind.BindKeyword)
                commands.Add(ParseUnityBind());

            var closeBraceToken = Match(SyntaxKind.CloseBraceToken);

            return new CommandBindChannelsSyntax(
                keyword,
                openBraceToken,
                commands,
                closeBraceToken);
        }

        private CommandSyntax ParseUnityBind()
        {
            var keyword = Match(SyntaxKind.BindKeyword);
            var sourceToken = Match(SyntaxKind.StringLiteralToken);
            var commaToken = Match(SyntaxKind.CommaToken);
            var targetToken = MatchOneOf(SyntaxKind.IdentifierToken, SyntaxKind.ColorKeyword);
            if (targetToken.Kind == SyntaxKind.ColorKeyword)
                targetToken = targetToken.WithKind(SyntaxKind.IdentifierToken);

            return new CommandBindChannelsBindSyntax(keyword, sourceToken, commaToken, targetToken);
        }
    }
}