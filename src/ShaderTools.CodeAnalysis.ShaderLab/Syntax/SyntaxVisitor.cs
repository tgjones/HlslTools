namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public abstract class SyntaxVisitor
    {
        public virtual void Visit(SyntaxNode node)
        {
            node?.Accept(this);
        }

        protected virtual void DefaultVisit(SyntaxNode node)
        {

        }

        public virtual void VisitSyntaxToken(SyntaxToken node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSyntaxTrivia(SyntaxTrivia node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSkippedTokensSyntaxTrivia(SkippedTokensTriviaSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitAttributeArgumentList(AttributeArgumentListSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShader(ShaderSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderProperties(ShaderPropertiesSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderProperty(ShaderPropertySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderPropertySimpleType(ShaderPropertySimpleTypeSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderPropertyRangeType(ShaderPropertyRangeTypeSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderPropertyNumericDefaultValue(ShaderPropertyNumericDefaultValueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderPropertyVectorDefaultValue(ShaderPropertyVectorDefaultValueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderPropertyTextureDefaultValue(ShaderPropertyTextureDefaultValueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitVector3(Vector3Syntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitVector4(Vector4Syntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderTags(ShaderTagsSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderTag(ShaderTagSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCategory(CategorySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSubShader(SubShaderSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitPass(PassSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderProgram(ShaderProgramSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderInclude(ShaderIncludeSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitShaderPropertyAttribute(ShaderPropertyAttributeSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandConstantValue(CommandConstantValueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandConstantColorValue(CommandConstantColorValueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandVariableValue(CommandVariableValueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandFallback(CommandFallbackSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandCustomEditor(CommandCustomEditorSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandCull(CommandCullSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandZWrite(CommandZWriteSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandZTest(CommandZTestSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandOffset(CommandOffsetSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandBlendOff(CommandBlendOffSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandBlendColor(CommandBlendColorSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandBlendColorAlpha(CommandBlendColorAlphaSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandColorMask(CommandColorMaskSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandLod(CommandLodSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandName(CommandNameSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandLighting(CommandLightingSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandStencil(CommandStencilSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandStencilRef(CommandStencilRefSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandStencilReadMask(CommandStencilReadMaskSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandStencilWriteMask(CommandStencilWriteMaskSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandStencilComp(CommandStencilCompSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandStencilPass(CommandStencilPassSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandStencilFail(CommandStencilFailSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandStencilZFail(CommandStencilZFailSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitUsePass(UsePassSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitGrabPass(GrabPassSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandDependency(CommandDependencySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandMaterial(CommandMaterialSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandMaterialDiffuse(CommandMaterialDiffuseSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandMaterialAmbient(CommandMaterialAmbientSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandMaterialShininess(CommandMaterialShininessSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandMaterialSpecular(CommandMaterialSpecularSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandMaterialEmission(CommandMaterialEmissionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandFog(CommandFogSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandFogMode(CommandFogModeSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandFogColor(CommandFogColorSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandFogDensity(CommandFogDensitySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandFogRange(CommandFogRangeSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSeparateSpecular(CommandSeparateSpecularSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSetTexture(CommandSetTextureSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSetTextureConstantColor(CommandSetTextureConstantColorSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSetTextureMatrix(CommandSetTextureMatrixSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSetTextureCombine(CommandSetTextureCombineSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSetTextureCombineSource(CommandSetTextureCombineSourceSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSetTextureCombineUnaryValue(CommandSetTextureCombineUnaryValueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSetTextureCombineBinaryValue(CommandSetTextureCombineBinaryValueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSetTextureCombineLerpValue(CommandSetTextureCombineLerpValueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSetTextureCombineMultiplyAlphaValue(CommandSetTextureCombineMultiplyAlphaValueSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandSetTextureCombineAlphaComponent(CommandSetTextureCombineAlphaComponentSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandAlphaTestOff(CommandAlphaTestOffSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandAlphaTestComparison(CommandAlphaTestComparisonSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandAlphaToMask(CommandAlphaToMaskSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandColorMaterial(CommandColorMaterialSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandBindChannels(CommandBindChannelsSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommandBindChannelsBind(CommandBindChannelsBindSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitEnumNameExpression(EnumNameExpressionSyntax node)
        {
            DefaultVisit(node);
        }
    }

    public abstract class SyntaxVisitor<T>
    {
        public virtual T Visit(SyntaxNode node)
        {
            if (node != null)
                return node.Accept(this);
            return default(T);
        }

        protected virtual T DefaultVisit(SyntaxNode node)
        {
            return default(T);
        }

        public virtual T VisitSyntaxToken(SyntaxToken node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSyntaxTrivia(SyntaxTrivia node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSkippedTokensSyntaxTrivia(SkippedTokensTriviaSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitAttributeArgumentList(AttributeArgumentListSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCompilationUnit(CompilationUnitSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShader(ShaderSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderProperties(ShaderPropertiesSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderProperty(ShaderPropertySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderTags(ShaderTagsSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderTag(ShaderTagSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCategory(CategorySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderPropertySimpleType(ShaderPropertySimpleTypeSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderPropertyRangeType(ShaderPropertyRangeTypeSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderPropertyNumericDefaultValue(ShaderPropertyNumericDefaultValueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderPropertyVectorDefaultValue(ShaderPropertyVectorDefaultValueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderPropertyTextureDefaultValue(ShaderPropertyTextureDefaultValueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitVector3(Vector3Syntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitVector4(Vector4Syntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitSubShader(SubShaderSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitPass(PassSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderProgram(ShaderProgramSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderInclude(ShaderIncludeSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitShaderPropertyAttribute(ShaderPropertyAttributeSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandConstantValue(CommandConstantValueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandConstantColorValue(CommandConstantColorValueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandVariableValue(CommandVariableValueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandFallback(CommandFallbackSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandCustomEditor(CommandCustomEditorSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandCull(CommandCullSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandZWrite(CommandZWriteSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandZTest(CommandZTestSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandOffset(CommandOffsetSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandBlendOff(CommandBlendOffSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandBlendColor(CommandBlendColorSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandBlendColorAlpha(CommandBlendColorAlphaSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandColorMask(CommandColorMaskSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandLod(CommandLodSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandName(CommandNameSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandLighting(CommandLightingSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandStencil(CommandStencilSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandStencilRef(CommandStencilRefSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandStencilReadMask(CommandStencilReadMaskSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandStencilWriteMask(CommandStencilWriteMaskSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandStencilComp(CommandStencilCompSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandStencilPass(CommandStencilPassSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandStencilFail(CommandStencilFailSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandStencilZFail(CommandStencilZFailSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitUsePass(UsePassSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitGrabPass(GrabPassSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandDependency(CommandDependencySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandMaterial(CommandMaterialSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandMaterialDiffuse(CommandMaterialDiffuseSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandMaterialAmbient(CommandMaterialAmbientSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandMaterialShininess(CommandMaterialShininessSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandMaterialSpecular(CommandMaterialSpecularSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandMaterialEmission(CommandMaterialEmissionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandFog(CommandFogSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandFogMode(CommandFogModeSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandFogColor(CommandFogColorSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandFogDensity(CommandFogDensitySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandFogRange(CommandFogRangeSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSeparateSpecular(CommandSeparateSpecularSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSetTexture(CommandSetTextureSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSetTextureConstantColor(CommandSetTextureConstantColorSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSetTextureMatrix(CommandSetTextureMatrixSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSetTextureCombine(CommandSetTextureCombineSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSetTextureCombineSource(CommandSetTextureCombineSourceSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSetTextureCombineUnaryValue(CommandSetTextureCombineUnaryValueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSetTextureCombineBinaryValue(CommandSetTextureCombineBinaryValueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSetTextureCombineLerpValue(CommandSetTextureCombineLerpValueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSetTextureCombineMultiplyAlphaValue(CommandSetTextureCombineMultiplyAlphaValueSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandSetTextureCombineAlphaComponent(CommandSetTextureCombineAlphaComponentSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandAlphaTestOff(CommandAlphaTestOffSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandAlphaTestComparison(CommandAlphaTestComparisonSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandAlphaToMask(CommandAlphaToMaskSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandColorMaterial(CommandColorMaterialSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandBindChannels(CommandBindChannelsSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitCommandBindChannelsBind(CommandBindChannelsBindSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual T VisitEnumNameExpression(EnumNameExpressionSyntax node)
        {
            return DefaultVisit(node);
        }
    }
}