using System.Linq;
using System.Threading;
using ShaderTools.Core.Text;
using ShaderTools.Editor.VisualStudio.Core.Glyphs;
using ShaderTools.Editor.VisualStudio.Hlsl.Navigation;
using ShaderTools.Editor.VisualStudio.Hlsl.Text;
using ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support;
using ShaderTools.Hlsl.Syntax;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Navigation
{
    public class NavigationTargetsVisitorTests : MefTestsBase
    {
        [Fact]
        public void CanGetTargets()
        {
            // Arrange.
            var sourceCode = @"
void Func1(int a, bool b, float c = 1.0);

int Func2();

struct Struct1 {
    float StructField1;
};

class Class1 {};

float Variable1, Variable2;";
            var textBuffer = TextBufferUtility.CreateTextBuffer(Container, sourceCode);
            var snapshot = textBuffer.CurrentSnapshot;
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(sourceCode));
            var visitor = new NavigationTargetsVisitor(
                snapshot,
                syntaxTree, 
                Container.GetExportedValue<DispatcherGlyphService>(), 
                CancellationToken.None);

            // Act.
            var targets = visitor.GetTargets((CompilationUnitSyntax) syntaxTree.Root).ToList();

            // Assert.
            Assert.Equal(3, targets.Count);

            Assert.Equal("(Global Scope)", targets[0].Name);
            Assert.Equal(Glyph.TopLevel, targets[0].Icon);

            Assert.Equal("Struct1", targets[1].Name);
            Assert.Equal(Glyph.Struct, targets[1].Icon);

            Assert.Equal("Class1", targets[2].Name);
            Assert.Equal(Glyph.Class, targets[2].Icon);

            Assert.Equal(4, targets[0].Children.Count);
            Assert.Equal("Func1(int a, bool b, float c = 1.0)", targets[0].Children[0].Name);
            Assert.Equal(Glyph.Function, targets[0].Children[0].Icon);
            Assert.Equal("Func2()", targets[0].Children[1].Name);
            Assert.Equal(Glyph.Function, targets[0].Children[1].Icon);
            Assert.Equal("Variable1", targets[0].Children[2].Name);
            Assert.Equal(Glyph.Variable, targets[0].Children[2].Icon);
            Assert.Equal("Variable2", targets[0].Children[3].Name);
            Assert.Equal(Glyph.Variable, targets[0].Children[3].Icon);

            Assert.Equal(1, targets[1].Children.Count);
            Assert.Equal("StructField1", targets[1].Children[0].Name);
            Assert.Equal(Glyph.Variable, targets[1].Children[0].Icon);

            Assert.Equal(0, targets[2].Children.Count);
        }
    }
}
