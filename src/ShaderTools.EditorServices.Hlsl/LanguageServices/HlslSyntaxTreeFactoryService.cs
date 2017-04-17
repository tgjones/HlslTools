using System;
using System.Threading;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.EditorServices.Workspace.Host;
using ShaderTools.Hlsl.Parser;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.EditorServices.Hlsl.LanguageServices
{
    internal sealed class HlslSyntaxTreeFactoryService : ISyntaxTreeFactoryService
    {
        public SyntaxTreeBase ParseSyntaxTree(string filePath, SourceText text, CancellationToken cancellationToken)
        {
            return null;

            //var configFile = _workspace.LoadConfigFile(Path.GetDirectoryName(sourceText.Filename));

            //var options = new ParserOptions();
            //options.PreprocessorDefines.Add("__INTELLISENSE__", "1");

            //foreach (var kvp in configFile.HlslPreprocessorDefinitions)
            //    options.PreprocessorDefines.Add(kvp.Key, kvp.Value);

            //options.AdditionalIncludeDirectories.AddRange(configFile.HlslAdditionalIncludeDirectories);

            //return SyntaxFactory.ParseSyntaxTree(
            //    text, 
            //    options,
            //    null,
            //    cancellationToken);
        }
    }
}
