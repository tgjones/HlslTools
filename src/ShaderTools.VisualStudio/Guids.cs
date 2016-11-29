// Guids.cs
// MUST match guids.h

using System;

namespace ShaderTools.VisualStudio
{
    static class GuidList
    {
        public const string guidShaderStudio_VisualStudioPackagePkgString = "7def6c01-a05e-42e6-953d-3fdea1891737";
        public const string guidShaderStudio_VisualStudioPackageCmdSetString = "6b3323d3-d8fe-4f57-8e15-b70cfedc1692";

        public static readonly Guid guidShaderStudio_VisualStudioPackageCmdSet = new Guid(guidShaderStudio_VisualStudioPackageCmdSetString);

        public const string guidHlslEditorFactory = "A62BA04F-FE74-41C2-9E7C-74086DA858E1";

        internal const string DefaultLanguageServiceString = "{8239BEC4-EE87-11D0-8C98-00C04FC2AB22}";
        internal static readonly Guid DefaultLanguageService = new Guid(DefaultLanguageServiceString);
    };
}