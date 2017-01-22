using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Diagnostics;
using ShaderTools.Hlsl.Syntax;
using Xunit;

namespace ShaderTools.Tests.Hlsl.Binding
{
    public abstract class BindingTestsBase
    {
        protected void AssertNoDiagnostics(string code)
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Empty(diagnostics);
        }

        protected void AssertDiagnostics(string code, params DiagnosticId[] expectedDiagnosticIds)
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(code));
            var compilation = new ShaderTools.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(expectedDiagnosticIds.Length, diagnostics.Length);
            for (var i = 0; i < expectedDiagnosticIds.Length; i++)
                Assert.Equal(expectedDiagnosticIds[i], (DiagnosticId) diagnostics[i].Descriptor.Code);
        }
    }
}
