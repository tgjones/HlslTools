using System;
using System.Runtime.InteropServices;
using ShaderTools.VisualStudio.Core;
using ShaderTools.VisualStudio.Hlsl.Navigation;

namespace ShaderTools.VisualStudio.Hlsl
{
    [ComVisible(true)]
    [Guid(GuidList.guidHlslEditorFactory)]
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