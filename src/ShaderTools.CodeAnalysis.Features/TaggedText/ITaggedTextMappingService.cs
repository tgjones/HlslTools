using Microsoft.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis
{
    internal interface ITaggedTextMappingService : ILanguageService
    {
        string GetClassificationTypeName(string taggedTextTag);
    }
}
