using System.Collections.Generic;
using System.Runtime.InteropServices;
using ShaderTools.VisualStudio.Core;

namespace ShaderTools.VisualStudio.ShaderLab
{
    [Guid("E77D10A9-504A-45D6-B1AC-8492236D9564")]
    internal sealed class ShaderLabLanguageInfo : LanguageInfoBase
    {
        public ShaderLabLanguageInfo(LanguagePackageBase languagePackage)
            : base(languagePackage)
        {
        }

        protected override string LanguageName { get; } = ShaderLabConstants.LanguageName;
        protected override IEnumerable<string> FileExtensions { get; } = new[] { ShaderLabConstants.FileExtension };
    }
}