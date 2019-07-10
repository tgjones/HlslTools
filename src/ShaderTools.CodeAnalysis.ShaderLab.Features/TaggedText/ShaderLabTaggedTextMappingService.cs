using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.ShaderLab.Classification;

namespace ShaderTools.CodeAnalysis.ShaderLab.TaggedText
{
    [ExportLanguageService(typeof(ITaggedTextMappingService), LanguageNames.ShaderLab)]
    internal sealed class ShaderLabTaggedTextMappingService : AbstractTaggedTextMappingService
    {
        public override string GetClassificationTypeName(string taggedTextTag)
        {
            switch (taggedTextTag)
            {
                case TextTags.Punctuation:
                    return ShaderLabClassificationTypeNames.Punctuation;

                case TextTags.Property:
                    return ShaderLabClassificationTypeNames.ShaderProperty;

                default:
                    return base.GetClassificationTypeName(taggedTextTag);
            }
        }
    }
}
