using System.Runtime.InteropServices;
using ShaderTools.Editor.VisualStudio.Core;
using ShaderTools.VisualStudio.LanguageServices;

namespace ShaderTools.Editor.VisualStudio.Hlsl
{
    [Guid(Guids.HlslLanguageServiceIdString)]
    internal sealed class HlslLanguageInfo : LanguageInfoBase
    {
        public HlslLanguageInfo(LanguagePackageBase languagePackage)
            : base(languagePackage)
        {
        }

        internal override string LanguageName { get; } = HlslConstants.LanguageName;
    }
}