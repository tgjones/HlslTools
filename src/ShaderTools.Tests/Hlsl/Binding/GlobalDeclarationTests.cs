using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Tests.Hlsl.Binding
{
    [TestFixture]
    public class GlobalDeclarationTests
    {
        [Test]
        public void DetectsRedefinitionAsVariable()
        {
            var code = @"
struct foo {};
int foo;";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.That(diagnostics, Has.Length.EqualTo(1));
            Assert.That((DiagnosticId) diagnostics[0].Descriptor.Code, Is.EqualTo(DiagnosticId.SymbolRedefined));
        }

        [Test]
        public void DetectsRedefinitionAsFunction()
        {
            var code = @"
struct foo {};
void foo();";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.That(diagnostics, Has.Length.EqualTo(1));
            Assert.That((DiagnosticId) diagnostics[0].Descriptor.Code, Is.EqualTo(DiagnosticId.SymbolRedefined));
        }

        [Test]
        public void DetectsUndeclaredVariable()
        {
            var code = @"
void main()
{
    int foo = a;
}";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.That(diagnostics, Has.Length.EqualTo(1));
            Assert.That((DiagnosticId) diagnostics[0].Descriptor.Code, Is.EqualTo(DiagnosticId.UndeclaredVariable));
        }
    }
}