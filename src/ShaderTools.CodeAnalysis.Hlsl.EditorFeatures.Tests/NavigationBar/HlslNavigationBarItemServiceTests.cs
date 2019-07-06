using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Editor.Hlsl.NavigationBar;
using ShaderTools.Testing.Workspaces;
using Xunit;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.Tests.NavigationBar
{
    public class HlslNavigationBarItemServiceTests
    {
        [Fact]
        public async Task CanGetTargets()
        {
            // Arrange.
            const string sourceCode = @"
void Func1(int a, bool b, float c = 1.0);

int Func2();

struct Struct1 {
    float StructField1;
};

class Class1 {};

float Variable1, Variable2;";
            var workspace = new TestWorkspace();
            var document = workspace.OpenDocument(DocumentId.CreateNewId(), new Text.SourceFile(SourceText.From(sourceCode)), LanguageNames.Hlsl);
            var service = new HlslNavigationBarItemService();

            // Act.
            var targets = await service.GetItemsAsync(document, CancellationToken.None);

            // Assert.
            Assert.Equal(3, targets.Count);

            Assert.Equal("(Global Scope)", targets[0].Text);
            Assert.Equal(Glyph.Namespace, targets[0].Glyph);

            Assert.Equal(4, targets[0].ChildItems.Count);
            Assert.Equal("Func1(int a, bool b, float c = 1.0)", targets[0].ChildItems[0].Text);
            Assert.Equal(Glyph.Method, targets[0].ChildItems[0].Glyph);
            Assert.Equal("Func2()", targets[0].ChildItems[1].Text);
            Assert.Equal(Glyph.Method, targets[0].ChildItems[1].Glyph);
            Assert.Equal("Variable1", targets[0].ChildItems[2].Text);
            Assert.Equal(Glyph.Local, targets[0].ChildItems[2].Glyph);
            Assert.Equal("Variable2", targets[0].ChildItems[3].Text);
            Assert.Equal(Glyph.Local, targets[0].ChildItems[3].Glyph);

            Assert.Equal("Class1", targets[1].Text);
            Assert.Equal(Glyph.Class, targets[1].Glyph);

            Assert.Equal(0, targets[1].ChildItems.Count);

            Assert.Equal("Struct1", targets[2].Text);
            Assert.Equal(Glyph.Structure, targets[2].Glyph);

            Assert.Equal(1, targets[2].ChildItems.Count);
            Assert.Equal("StructField1", targets[2].ChildItems[0].Text);
            Assert.Equal(Glyph.Field, targets[2].ChildItems[0].Glyph);
        }
    }
}
