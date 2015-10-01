using System.Collections.Generic;
using HlslTools.Text;

namespace HlslTools.Tests.Support
{
    internal sealed class InMemoryFileSystem : IIncludeFileSystem
    {
        private readonly Dictionary<string, string> _includes;

        public InMemoryFileSystem(Dictionary<string, string> includes)
        {
            _includes = includes;
        }

        public SourceText GetInclude(string path)
        {
            string include;
            return _includes.TryGetValue(path, out include) 
                ? new StringText(include, path)
                : null;
        }
    }
}