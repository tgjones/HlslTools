using System.Collections.Immutable;
using System.Linq;
using HlslTools.Diagnostics;
using HlslTools.Syntax;
using HlslTools.Text;
using NUnit.Framework;

namespace HlslTools.Tests.Binding
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
            var compilation = new HlslTools.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.That(diagnostics, Has.Length.EqualTo(1));
            Assert.That(diagnostics[0].DiagnosticId, Is.EqualTo(DiagnosticId.SymbolRedefined));
        }

        [Test]
        public void DetectsRedefinitionAsFunction()
        {
            var code = @"
struct foo {};
void foo();";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new HlslTools.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.That(diagnostics, Has.Length.EqualTo(1));
            Assert.That(diagnostics[0].DiagnosticId, Is.EqualTo(DiagnosticId.SymbolRedefined));
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
            var compilation = new HlslTools.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.That(diagnostics, Has.Length.EqualTo(1));
            Assert.That(diagnostics[0].DiagnosticId, Is.EqualTo(DiagnosticId.UndeclaredVariable));
        }
    }
}