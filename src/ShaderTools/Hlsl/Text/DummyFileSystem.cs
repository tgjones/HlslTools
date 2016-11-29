using System;
using ShaderTools.Core.Text;

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