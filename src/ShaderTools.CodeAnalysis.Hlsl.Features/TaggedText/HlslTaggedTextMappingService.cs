using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Hlsl.Classification;

namespace ShaderTools.CodeAnalysis.Hlsl
{
    [ExportLanguageService(typeof(ITaggedTextMappingService), LanguageNames.Hlsl)]
    internal sealed class HlslTaggedTextMappingService : AbstractTaggedTextMappingService
    {
        public override string GetClassificationTypeName(string taggedTextTag)
        {
            switch (taggedTextTag)
            {
                case TextTags.Punctuation:
                    return HlslClassificationTypeNames.Punctuation;

                case TextTags.Semantic:
                    return HlslClassificationTypeNames.Semantic;

                case TextTags.PackOffset:
                    return HlslClassificationTypeNames.PackOffset;

                case TextTags.RegisterLocation:
                    return HlslClassificationTypeNames.RegisterLocation;

                case TextTags.Namespace:
                    return HlslClassificationTypeNames.NamespaceIdentifier;

                case TextTags.Global:
                    return HlslClassificationTypeNames.GlobalVariableIdentifier;

                case TextTags.Field:
                    return HlslClassificationTypeNames.FieldIdentifier;

                case TextTags.Local:
                    return HlslClassificationTypeNames.LocalVariableIdentifier;

                case TextTags.ConstantBufferField:
                    return HlslClassificationTypeNames.ConstantBufferVariableIdentifier;

                case TextTags.Parameter:
                    return HlslClassificationTypeNames.ParameterIdentifier;

                case TextTags.Function:
                    return HlslClassificationTypeNames.FunctionIdentifier;

                case TextTags.Method:
                    return HlslClassificationTypeNames.MethodIdentifier;

                case TextTags.Class:
                    return HlslClassificationTypeNames.ClassIdentifier;

                case TextTags.Struct:
                    return HlslClassificationTypeNames.StructIdentifier;

                case TextTags.Interface:
                    return HlslClassificationTypeNames.InterfaceIdentifier;

                case TextTags.ConstantBuffer:
                    return HlslClassificationTypeNames.ConstantBufferIdentifier;

                case TextTags.Technique:
                    return ClassificationTypeNames.Identifier;

                default:
                    return base.GetClassificationTypeName(taggedTextTag);
            }
        }
    }
}
