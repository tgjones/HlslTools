using System.IO;
using System.Threading;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Parser;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.EditorServices.Workspace.Hlsl
{
    public sealed class HlslDocument : Document
    {
        private readonly IIncludeFileSystem _fileSystem;
        private readonly Workspace _workspace;

        public HlslDocument(SourceText sourceText, string clientFilePath, IIncludeFileSystem fileSystem, Workspace workspace)
            : base(sourceText, clientFilePath)
        {
            _fileSystem = fileSystem;
            _workspace = workspace;
        }

        protected override SyntaxTreeBase Compile(SourceText sourceText, CancellationToken cancellationToken)
        {
            var configFile = _workspace.LoadConfigFile(Path.GetDirectoryName(sourceText.Filename));

            var options = new ParserOptions();
            options.PreprocessorDefines.Add("__INTELLISENSE__", "1");

            foreach (var kvp in configFile.HlslPreprocessorDefinitions)
                options.PreprocessorDefines.Add(kvp.Key, kvp.Value);

            options.AdditionalIncludeDirectories.AddRange(configFile.HlslAdditionalIncludeDirectories);

            return SyntaxFactory.ParseSyntaxTree(sourceText, options, _fileSystem, cancellationToken);
        }
    }
}
