using System.IO;
using System.Threading;
using ShaderTools.CodeAnalysis.Hlsl.Parser;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.EditorServices.Workspace.Host;

namespace ShaderTools.EditorServices.Hlsl.LanguageServices
{
    internal sealed class HlslSyntaxTreeFactoryService : ISyntaxTreeFactoryService
    {
        private readonly Workspace.Workspace _workspace;
        private readonly IIncludeFileSystem _fileSystem;

        public HlslSyntaxTreeFactoryService(Workspace.Workspace workspace, IIncludeFileSystem fileSystem)
        {
            _workspace = workspace;
            _fileSystem = fileSystem;
        }

        public SyntaxTreeBase ParseSyntaxTree(SourceText text, CancellationToken cancellationToken)
        {
            var configFile = _workspace.LoadConfigFile(Path.GetDirectoryName(text.Filename));

            var options = new ParserOptions();
            options.PreprocessorDefines.Add("__INTELLISENSE__", "1");

            foreach (var kvp in configFile.HlslPreprocessorDefinitions)
                options.PreprocessorDefines.Add(kvp.Key, kvp.Value);

            options.AdditionalIncludeDirectories.AddRange(configFile.HlslAdditionalIncludeDirectories);

            return SyntaxFactory.ParseSyntaxTree(
                text,
                options,
                _fileSystem,
                cancellationToken);
        }
    }
}
