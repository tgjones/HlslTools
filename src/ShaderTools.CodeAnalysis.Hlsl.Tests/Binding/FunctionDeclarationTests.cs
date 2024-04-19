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
        public void AllowsNamespaceFunctionOverloads()
        {
            var code = @"
namespace Shading { void foo(int x) {} }
namespace Shading { void foo() {} }
namespace Culling { namespace Decals { void foo(int x) {} } }
namespace Culling { namespace Decals { void foo() {} } }
namespace Shading { namespace Decals { void foo(int x) {} } }
namespace Shading { namespace Decals { void foo() {} } }
namespace Culling { void foo(int x) {} }
namespace Culling { void foo() {} }";
            AssertNoDiagnostics(code);
        }

        [Fact]
        public void DetectsNamespaceFunctionRedefinition()
        {
            var code = @"
namespace Shading { void foo() {} }
namespace Shading { void foo() {} }
namespace Shading { namespace Decals { void foo() {} } }
namespace Shading { namespace Decals { void foo() {} } }";
            AssertDiagnostics(code, DiagnosticId.SymbolRedefined, DiagnosticId.SymbolRedefined);
        }

        [Fact]
        public void DetectsNamespaceMissingFunctionDeclaration()
        {
            var code = @"
namespace Shading {
    void foo();
    namespace Decals {}
}

void main()
{
    Shading::Decals::foo2();
    Shading::Decals::foo();
    Shading::foo();
}";
            AssertDiagnostics(code, DiagnosticId.UndeclaredFunction, DiagnosticId.UndeclaredFunction, DiagnosticId.FunctionMissingImplementation);
        }

        [Fact]
        public void DetectsNamespaceMissingVariableDeclaration()
        {
            var code = @"
namespace Shading {
    static const uint x1 = 1;
    namespace Decals {}
}

void main()
{
    uint x;
    x = Shading::Decals::y1;
    x = Shading::Decals::x1;
    x = Shading::x1;
}";
            AssertDiagnostics(code, DiagnosticId.UndeclaredVariable, DiagnosticId.UndeclaredVariable);
        }

#if false
        [Fact]
        public void AllowsNesterNamespaceFunctionDeclarations()
        {
            var code = @"
namespace Shading { void foo() {} }
namespace Shading { namespace Shading { void foo() {} } }";
            AssertNoDiagnostics(code);
        }
#endif

        [Fact]
        public void DetectsNesterNamespaceFunctionRedefinition()
        {
            var code = @"
namespace Shading {
    namespace Shading { void foo() {} }
    namespace Shading { void foo() {} }
}";
            AssertDiagnostics(code, DiagnosticId.SymbolRedefined);
        }

        [Fact]
        public void AllowsMultiNamespaceFunctionDeclarations()
        {
            var code = @"
namespace Shading { void foo(int x) {} }
namespace Shading { void foo() {} }
void foo(uint i) { Shading::foo(); }
namespace Shading { void foo2() {} }
namespace Shading { namespace Decals { void foo(uint i) {} } }
namespace Shading { namespace Decals { void foo2() { foo(1.0); } } }

void main()
{
    foo(1.0);
    Shading::foo(1.0);
    Shading::foo2();
    Shading::Decals::foo(1.0);
    Shading::Decals::foo2();
}";
            AssertNoDiagnostics(code);
        }

        [Fact]
        public void AllowsMultiNamespaceVariableDeclarations()
        {
            var code = @"
namespace Shading { static const uint x1 = 1; }
namespace Shading { static const uint x2 = 2; }
namespace Shading { static const uint x3 = 3; }
namespace Shading { namespace Decals { static const uint y1 = 1; } }
namespace Shading { namespace Decals { static const uint y2 = 2; } }
namespace Shading { namespace Decals { static const uint y3 = 3; } }

void main()
{
    uint x;
    x = Shading::x1;
    x = Shading::x2;
    x = Shading::x3;
    x = Shading::Decals::y1;
    x = Shading::Decals::y2;
    x = Shading::Decals::y3;
}";
            AssertNoDiagnostics(code);
        }

        [Fact]
        public void DetectsMultiNamespaceAmbiguousFunctionInvocation()
        {
            var code = @"
namespace Shading { void foo(int i) {} }
namespace Shading { void foo(uint i) {} }
namespace Culling { namespace Decals { void foo(int i) {} } }
namespace Culling { namespace Decals { void foo(uint i) {} } }
namespace Shading { namespace Decals { void foo(int i) {} } }
namespace Shading { namespace Decals { void foo(uint i) {} } }

void main()
{
    Shading::foo(1.0);
    Culling::Decals::foo(1.0);
    Shading::Decals::foo(1.0);
}";
            AssertDiagnostics(code, DiagnosticId.AmbiguousInvocation, DiagnosticId.AmbiguousInvocation, DiagnosticId.AmbiguousInvocation);
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
