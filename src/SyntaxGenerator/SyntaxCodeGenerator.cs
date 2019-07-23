using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxGenerator.Model;
using SyntaxGenerator.Writer;

namespace SyntaxGenerator
{
    public class SyntaxCodeGenerator : IRichCodeGenerator
    {
        private readonly string _syntaxFile;

        public SyntaxCodeGenerator(AttributeData attributeData)
        {
            if (attributeData == null)
                throw new ArgumentException(nameof(attributeData));

            _syntaxFile = (string) attributeData.ConstructorArguments[0].Value;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SyntaxList<MemberDeclarationSyntax>());
        }

        public Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var fullSyntaxPath = Path.Combine(context.ProjectDirectory, _syntaxFile);

            FileStream fs = null;

            try
            {
                fs = File.OpenRead(fullSyntaxPath);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
            {
                var fnf = Diagnostic.Create("SCG_FileNotFound", "SyntaxCodeGenerator", $"The syntax file at {fullSyntaxPath} was not found.", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0);
                progress.Report(fnf);
                throw;
            }

            using (fs)
            {
                var reader = XmlReader.Create(fs, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
                var serializer = new XmlSerializer(typeof(Tree));
                Tree tree = (Tree) serializer.Deserialize(reader);

                cancellationToken.ThrowIfCancellationRequested();

                var stringBuilder = new StringBuilder();
                var writer = new StringWriter(stringBuilder);
                SourceWriter.WriteAll(writer, tree);

                var syntaxTree = CSharpSyntaxTree.ParseText(stringBuilder.ToString(), cancellationToken: cancellationToken);

                var root = syntaxTree.GetCompilationUnitRoot();
                var rgr = new RichGenerationResult
                {
                    Usings = root.Usings,
                    Members = root.Members
                };

                return Task.FromResult(rgr);
            }
        }
    }
}

