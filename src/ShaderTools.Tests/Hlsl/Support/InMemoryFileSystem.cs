using System.Collections.Generic;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Tests.Hlsl.Support
{
    internal sealed class InMemoryFileSystem : IIncludeFileSystem
    {
        private readonly Dictionary<string, string> _includes;

        public InMemoryFileSystem(Dictionary<string, string> includes)
        {
            _includes = includes;
        }

        public bool TryGetFile(string path, out SourceText text)
        {
            if (_includes.TryGetValue(path, out string include))
            {
                text = new StringText(include, path);
                return true;
            }
            text = null;
            return false;
        }
    }
}