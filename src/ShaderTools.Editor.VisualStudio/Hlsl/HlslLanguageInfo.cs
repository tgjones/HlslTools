using System.Collections.Generic;
using System.Runtime.InteropServices;
using ShaderTools.Editor.VisualStudio.Core;

namespace ShaderTools.Editor.VisualStudio.Hlsl
{
    [Guid("80329450-4B0D-4EC7-A4E4-A57C024888D5")]
    internal sealed class HlslLanguageInfo : LanguageInfoBase
    {
        public HlslLanguageInfo(LanguagePackageBase languagePackage)
            : base(languagePackage)
        {
        }

        protected override string LanguageName { get; } = HlslConstants.LanguageName;
        protected override IEnumerable<string> FileExtensions { get; } = HlslConstants.FileExtensions;
    }
}