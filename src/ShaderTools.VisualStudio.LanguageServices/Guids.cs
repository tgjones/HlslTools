using System;

namespace ShaderTools.VisualStudio.LanguageServices
{
    internal static class Guids
    {
        public const string HlslLanguageServiceIdString = "80329450-4B0D-4EC7-A4E4-A57C024888D5";
        public const string ShaderLabLanguageServiceIdString = "E77D10A9-504A-45D6-B1AC-8492236D9564";

        public static readonly Guid HlslLanguageServiceId = new Guid(HlslLanguageServiceIdString);
        public static readonly Guid ShaderLabLanguageServiceId = new Guid(ShaderLabLanguageServiceIdString);
    }
}
