using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Support
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
            string include;
            if (_includes.TryGetValue(path, out include))
            {
                text = SourceText.From(include);
                return true;
            }
            text = null;
            return false;
        }
    }
}