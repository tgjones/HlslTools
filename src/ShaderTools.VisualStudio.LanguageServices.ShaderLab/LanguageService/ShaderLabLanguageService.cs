using System;
using System.Runtime.InteropServices;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Editor;
using ShaderTools.VisualStudio.LanguageServices.LanguageService;

namespace ShaderTools.VisualStudio.LanguageServices.ShaderLab.LanguageService
{
    [Guid(Guids.ShaderLabLanguageServiceIdString)]
    internal sealed class ShaderLabLanguageService : AbstractLanguageService<ShaderLabPackage, ShaderLabLanguageService>
    {
        protected override string ContentTypeName => ContentTypeNames.ShaderLabContentType;

        public override Guid LanguageServiceId => Guids.ShaderLabLanguageServiceId;

        protected override string LanguageName => LanguageNames.ShaderLab;

        protected override string ShaderToolsLanguageName => LanguageNames.ShaderLab;

        internal ShaderLabLanguageService(ShaderLabPackage package)
            : base(package)
        {
        }
    }
}
