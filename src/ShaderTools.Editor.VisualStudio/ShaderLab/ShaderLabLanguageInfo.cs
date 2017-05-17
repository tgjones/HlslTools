using System.Runtime.InteropServices;
using ShaderTools.CodeAnalysis;
using ShaderTools.Editor.VisualStudio.Core;
using ShaderTools.VisualStudio.LanguageServices;

namespace ShaderTools.Editor.VisualStudio.ShaderLab
{
    [Guid(Guids.ShaderLabLanguageServiceIdString)]
    internal sealed class ShaderLabLanguageInfo : LanguageInfoBase
    {
        public ShaderLabLanguageInfo(LanguagePackageBase languagePackage)
            : base(languagePackage)
        {
        }

        internal override string LanguageName { get; } = LanguageNames.ShaderLab;
    }
}