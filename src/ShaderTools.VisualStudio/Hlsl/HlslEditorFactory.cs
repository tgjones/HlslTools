using System;
using System.Runtime.InteropServices;
using ShaderTools.VisualStudio.Core;

namespace ShaderTools.VisualStudio.Hlsl
{
    [ComVisible(true)]
    [Guid("A62BA04F-FE74-41C2-9E7C-74086DA858E1")]
    internal sealed class HlslEditorFactory : EditorFactoryBase
    {
        public HlslEditorFactory(HlslPackage package)
            : base(package)
        {
        }

        protected override Type GetLanguageInfoType()
        {
            return typeof(HlslLanguageInfo);
        }
    }
}