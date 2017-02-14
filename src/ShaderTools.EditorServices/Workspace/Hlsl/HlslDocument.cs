using System;
using System.IO;
using System.Threading;
using ShaderTools.Core.Compilation;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Compilation;
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

        protected override SemanticModelBase CreateSemanticModel(SyntaxTreeBase syntaxTree, CancellationToken cancellationToken)
        {
            var compilation = new Compilation((SyntaxTree) syntaxTree);
            return compilation.GetSemanticModel(cancellationToken);
        }

        public override Document WithSourceText(SourceText sourceText)
        {
            return new HlslDocument(sourceText, ClientFilePath, _fileSystem, _workspace);
        }
    }
}
