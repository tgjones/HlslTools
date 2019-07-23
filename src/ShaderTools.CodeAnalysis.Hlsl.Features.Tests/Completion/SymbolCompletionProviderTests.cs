using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Completion;
using ShaderTools.CodeAnalysis.Hlsl.Completion.CompletionProviders;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Testing.Workspaces;
using Xunit;

namespace ShaderTools.CodeAnalysis.Hlsl.Tests.Completion
{
    public class ScopedCompletionTests
    {
        [Fact]
        public async Task GlobalDeclaration_AboveSourceLocationInFunction_InCompletionList()
        {
            var markup = @"Texture2D InputTexture;
float4 PS(float4 pos : SV_Position) : SV_Target
{
    In$$
    float4 color;
    return color;
}";

            await VerifyItemExistsAsync(markup, "InputTexture");
        }

        [Fact]
        public async Task LocalDeclaration_AboveSourceLocation_SameFunction_InCompletionList()
        {
            var markup = @"float4 PS(float4 pos : SV_Position) : SV_Target
{
    float4 color;
    return co$$;
}";

            await VerifyItemExistsAsync(markup, "color");
        }

        [Fact]
        public async Task GlobalDeclaration_BelowSourcePosition_NotInCompletionList()
        {
            var markup = @"I$$
Texture2D InputTexture;
float4 PS(float4 pos : SV_Position) : SV_Target
{
    float4 color;
    return color;
}";

            await VerifyItemIsAbsentAsync(markup, "InputTexture");
        }


        [Fact]
        public async Task LocalDeclaration_BelowSourceLocation_SameFunction_NotInCompletionList()
        {
            var markup = @"float4 PS(float4 pos : SV_Position) : SV_Target
{
    co$$
    float4 color;
    return color;
}";

            await VerifyItemIsAbsentAsync(markup, "color");
        }

        [Fact]
        public async Task LocalDeclaration_AboveSourceLocation_DifferentScope_NotInCompletionList()
        {
            var markup = @"float4 PS(float4 pos : SV_Position) : SV_Target
{
    float4 color;
    return color;
}

float Dummy(float input)
{
    co$$
}";

            await VerifyItemIsAbsentAsync(markup, "color");
        }

        private async Task VerifyItemExistsAsync(string markup, string expectedItem)
        {
            var completionItems = await GetCompletionItems(markup);

            Assert.Contains(completionItems, x => x.DisplayText == expectedItem);
        }

        private async Task VerifyItemIsAbsentAsync(string markup, string expectedItem)
        {
            var completionItems = await GetCompletionItems(markup);

            Assert.DoesNotContain(completionItems, x => x.DisplayText == expectedItem);
        }

        private async Task<IReadOnlyList<CompletionItem>> GetCompletionItems(string testCode)
        {
            var index = testCode.IndexOf("$$");
            testCode = testCode.Remove(index, 2);

            var workspace = new TestWorkspace();

            var document = workspace.OpenDocument(
                DocumentId.CreateNewId(), 
                new SourceFile(SourceText.From(testCode)), 
                LanguageNames.Hlsl);

            var completionProvider = new SymbolCompletionProvider();

            var completionContext = new CompletionContext(
                completionProvider, 
                document, 
                index, 
                new TextSpan(), 
                Microsoft.CodeAnalysis.Completion.CompletionTrigger.Invoke,
                await document.GetOptionsAsync(),
                CancellationToken.None);

            await completionProvider.ProvideCompletionsAsync(completionContext);

            return completionContext.Items;
        }
    }
}
