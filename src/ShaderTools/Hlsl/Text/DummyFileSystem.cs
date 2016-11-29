using System;

namespace ShaderTools.Hlsl.Text
{
    internal sealed class DummyFileSystem : IIncludeFileSystem
    {
        public SourceText GetInclude(string path)
        {
            throw new NotSupportedException();
        }
    }
}