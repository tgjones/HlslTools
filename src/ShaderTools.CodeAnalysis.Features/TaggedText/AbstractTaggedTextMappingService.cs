using Microsoft.CodeAnalysis.Classification;
using ShaderTools.Utilities.ErrorReporting;

namespace ShaderTools.CodeAnalysis
{
    internal abstract class AbstractTaggedTextMappingService : ITaggedTextMappingService
    {
        public virtual string GetClassificationTypeName(string taggedTextTag)
        {
            switch (taggedTextTag)
            {
                case TextTags.Keyword:
                    return ClassificationTypeNames.Keyword;

                case TextTags.Alias:
                case TextTags.ErrorType:
                    return ClassificationTypeNames.Identifier;

                case TextTags.NumericLiteral:
                    return ClassificationTypeNames.NumericLiteral;

                case TextTags.StringLiteral:
                    return ClassificationTypeNames.StringLiteral;

                case TextTags.Space:
                case TextTags.LineBreak:
                    return ClassificationTypeNames.WhiteSpace;

                case TextTags.Operator:
                    return ClassificationTypeNames.Operator;

                case TextTags.Text:
                    return ClassificationTypeNames.Text;

                default:
                    throw ExceptionUtilities.UnexpectedValue(taggedTextTag);
            }
        }
    }
}
