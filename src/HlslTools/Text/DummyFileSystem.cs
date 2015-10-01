using System;

namespace HlslTools.Text
{
    internal sealed class DummyFileSystem : IIncludeFileSystem
    {
        public SourceText GetInclude(string path)
        {
            throw new NotSupportedException();
        }
    }
}