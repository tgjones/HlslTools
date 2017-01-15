using System.Collections.Immutable;
using System.Linq;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Syntax;
using Xunit;

namespace ShaderTools.Tests.Hlsl.Binding
{
    public class FunctionDeclarationTests
    {
        [Fact]
        public void DetectsFunctionRedefinition()
        {
            var code = @"
void foo() {}
void foo() {}";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.SymbolRedefined, (DiagnosticId) diagnostics[0].Descriptor.Code);
        }

        [Fact]
        public void AllowsFunctionOverloads()
        {
            var code = @"
void foo(int x) {}
void foo() {}";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Empty(diagnostics);
        }

        [Fact]
        public void AllowsMultipleMatchingFunctionDeclarations()
        {
            var code = @"
void foo();
void foo();";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Empty(diagnostics);
        }

        [Fact]
        public void AllowsMissingFunctionImplementationIfUnused()
        {
            var code = @"void foo();";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Empty(diagnostics);
        }

        [Fact]
        public void DetectsMissingFunctionImplementation()
        {
            var code = @"
void foo();

void main()
{
    foo();
}";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.FunctionMissingImplementation, (DiagnosticId) diagnostics[0].Descriptor.Code);
        }
    }
}