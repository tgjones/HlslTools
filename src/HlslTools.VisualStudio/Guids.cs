// Guids.cs
// MUST match guids.h

using System;

namespace HlslTools.VisualStudio
{
    static class GuidList
    {
        public const string guidShaderStudio_VisualStudioPackagePkgString = "7def6c01-a05e-42e6-953d-3fdea1891737";
        public const string guidShaderStudio_VisualStudioPackageCmdSetString = "6b3323d3-d8fe-4f57-8e15-b70cfedc1692";

        public static readonly Guid guidShaderStudio_VisualStudioPackageCmdSet = new Guid(guidShaderStudio_VisualStudioPackageCmdSetString);
    };
}