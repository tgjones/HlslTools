using System;
using System.Runtime.InteropServices;
using ShaderTools.Editor.VisualStudio.Core;

namespace ShaderTools.Editor.VisualStudio.ShaderLab
{
    [ComVisible(true)]
    [Guid("84F980F5-0602-4E31-BF14-80023F2CCF39")]
    internal sealed class ShaderLabEditorFactory : EditorFactoryBase
    {
        public ShaderLabEditorFactory(ShaderLabPackage package)
            : base(package)
        {
        }

        protected override Type GetLanguageInfoType()
        {
            return typeof(ShaderLabLanguageInfo);
        }
    }
}