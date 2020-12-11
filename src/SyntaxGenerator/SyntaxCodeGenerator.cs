using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using SyntaxGenerator.Model;
using SyntaxGenerator.Writer;

namespace SyntaxGenerator
{
    [Generator]
    public class SyntaxCodeGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var fullSyntaxPath = context.AdditionalFiles[0].Path;

            FileStream fs = null;

            try
            {
                fs = File.OpenRead(fullSyntaxPath);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException)
            {
                var fnf = Diagnostic.Create("SCG_FileNotFound", "SyntaxCodeGenerator", $"The syntax file at {fullSyntaxPath} was not found.", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0);
                context.ReportDiagnostic(fnf);
                return;
            }

            using (fs)
            {
                var reader = XmlReader.Create(fs, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
                var serializer = new XmlSerializer(typeof(Tree));
                Tree tree = (Tree) serializer.Deserialize(reader);

                context.CancellationToken.ThrowIfCancellationRequested();

                var stringBuilder = new StringBuilder();
                var writer = new StringWriter(stringBuilder);
                SourceWriter.WriteAll(writer, tree);

                context.AddSource("Syntax.generated.cs", stringBuilder.ToString());
            }
        }
    }
}

