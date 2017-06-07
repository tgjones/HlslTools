using System;
using System.Runtime.InteropServices;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Editor;
using ShaderTools.VisualStudio.LanguageServices.LanguageService;

namespace ShaderTools.VisualStudio.LanguageServices.Hlsl.LanguageService
{
    [Guid(Guids.HlslLanguageServiceIdString)]
    internal sealed class HlslLanguageService : AbstractLanguageService<HlslPackage, HlslLanguageService>
    {
        protected override string ContentTypeName => ContentTypeNames.HlslContentType;

        public override Guid LanguageServiceId => Guids.HlslLanguageServiceId;

        protected override string LanguageName => LanguageNames.Hlsl;

        protected override string ShaderToolsLanguageName => LanguageNames.Hlsl;

        internal HlslLanguageService(HlslPackage package)
            : base(package)
        {
        }
    }
}
