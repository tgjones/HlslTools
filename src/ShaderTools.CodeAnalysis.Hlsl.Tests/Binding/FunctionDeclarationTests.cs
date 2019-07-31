using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Binding
{
    public class FunctionDeclarationTests : BindingTestsBase
    {
        [Fact]
        public void DetectsFunctionRedefinition()
        {
            var code = @"
void foo() {}
void foo() {}";
            AssertDiagnostics(code, DiagnosticId.SymbolRedefined);
        }

        [Fact]
        public void AllowsFunctionOverloads()
        {
            var code = @"
void foo(int x) {}
void foo() {}";
            AssertNoDiagnostics(code);
        }

        [Fact]
        public void AllowsMultipleMatchingFunctionDeclarations()
        {
            var code = @"
void foo();
void foo();";
            AssertNoDiagnostics(code);
        }

        [Fact]
        public void AllowsMissingFunctionImplementationIfUnused()
        {
            var code = @"void foo();";
            AssertNoDiagnostics(code);
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
            AssertDiagnostics(code, DiagnosticId.FunctionMissingImplementation);
        }

        [Fact]
        public void DetectsAmbiguousFunctionInvocation()
        {
            var code = @"
void foo(int i) {}
void foo(uint i) {}

void main()
{
    foo(1.0);
}";
            AssertDiagnostics(code, DiagnosticId.AmbiguousInvocation);
        }

        [Fact]
        public void DetectsOverloadResolutionFailure()
        {
            var code = @"
void foo(int i) {}

void main()
{
    foo();
}";
            AssertDiagnostics(code, DiagnosticId.FunctionOverloadResolutionFailure);
        }

        [Fact]
        public void AllowsFunctionDeclaredInMacro()
        {
            var code = @"
#define DECLARE_FUNC(name) float name() { return 1.0; }
DECLARE_FUNC(myfunc)

void main()
{
	myfunc();
}
";
            AssertNoDiagnostics(code);
        }

        [Fact]
        public void DetectsReturnValueFromVoidFunction()
        {
            var code = "void foo() { return 1; }";
            AssertDiagnostics(code, DiagnosticId.RetNoObjectRequired);
        }

        [Fact]
        public void DetectsNoReturnValueFromNonVoidFunction()
        {
            var code = "int foo() { return; }";
            AssertDiagnostics(code, DiagnosticId.RetObjectRequired);
        }
	
	[Fact]
        public void DetectsMissingReturnStatementFromNonVoidFunction()
        {
            var code = "int foo() { }";
            AssertDiagnostics(code, DiagnosticId.ReturnExpected);
        }
    }
}
