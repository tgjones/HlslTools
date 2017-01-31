using ShaderTools.Hlsl.Diagnostics;
using Xunit;

namespace ShaderTools.Tests.Hlsl.Binding
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
#define DECLARE_FUNC(name) float name() {}
DECLARE_FUNC(myfunc)

void main()
{
	myfunc();
}
";
            AssertNoDiagnostics(code);
        }
    }
}