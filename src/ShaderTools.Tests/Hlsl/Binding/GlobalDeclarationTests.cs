using System.Collections.Immutable;
using System.Linq;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Syntax;
using Xunit;

namespace ShaderTools.Tests.Hlsl.Binding
{
    public class GlobalDeclarationTests
    {
        [Fact]
        public void DetectsRedefinitionAsVariable()
        {
            var code = @"
struct foo {};
int foo;";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.SymbolRedefined, (DiagnosticId) diagnostics[0].Descriptor.Code);
        }

        [Fact]
        public void DetectsRedefinitionAsFunction()
        {
            var code = @"
struct foo {};
void foo();";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.SymbolRedefined, (DiagnosticId) diagnostics[0].Descriptor.Code);
        }

        [Fact]
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

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.UndeclaredVariable, (DiagnosticId) diagnostics[0].Descriptor.Code);
        }
    }
}