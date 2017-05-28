using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.Completion.CompletionProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.IntelliSense
{
    public class ScopedCompletionTests
    {
        [Fact()]
        public void GlobalDeclaration_AboveSourceLocationInFunction_InCompletionList()
        {
            string testCode = @"Texture2D InputTexture;
            float4 PS(float4 pos : SV_Position) : SV_Target
            {
                In
                float4 color;
                return color;
            }";

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(testCode));
            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            int index = testCode.LastIndexOf("In") + 2; //Go to In inside PS function

            var completionItems = new SymbolCompletionProvider().GetItems(semanticModel, new SourceLocation(index));

            var expectedCompletionItem = completionItems.Where(ci => ci.Symbol.Name == "InputTexture" && ci.Symbol.Kind == SymbolKind.Variable).FirstOrDefault();

            Assert.NotNull(expectedCompletionItem);
        }

        [Fact()]
        public void LocalDeclaration_AboveSourceLocation_SameFunction_InCompletionList()
        {
            string testCode = @"float4 PS(float4 pos : SV_Position) : SV_Target
            {
                float4 color;
                return co;
            }";

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(testCode));
            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            int index = testCode.LastIndexOf("co") + 2; //Gets at "return co" location

            var completionItems = new SymbolCompletionProvider().GetItems(semanticModel, new SourceLocation(index));

            var expectedCompletionItem = completionItems.Where(ci => ci.Symbol.Name == "color" && ci.Symbol.Kind == SymbolKind.Variable).FirstOrDefault();

            Assert.NotNull(expectedCompletionItem);
        }

        [Fact()]
        public void GlobalDeclaration_BelowSourcePosition_NotInCompletionList()
        {
            string testCode = @"I  
            Texture2D InputTexture;
            float4 PS(float4 pos : SV_Position) : SV_Target
            {
                float4 color;
                return color;
            }";

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(testCode));
            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var completionItems = new SymbolCompletionProvider().GetItems(semanticModel, new SourceLocation(1)); //First I at the beginning

            var nonExpectedCompletionItem = completionItems.Where(ci => ci.Symbol.Name == "InputTexture" && ci.Symbol.Kind == SymbolKind.Variable).FirstOrDefault();

            Assert.Null(nonExpectedCompletionItem);
        }


        [Fact()]
        public void LocalDeclaration_BelowSourceLocation_SameFunction_NotInCompletionList()
        {
            string testCode = @"float4 PS(float4 pos : SV_Position) : SV_Target
            {
                co
                float4 color;
                return color;
            }";

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(testCode));
            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            int index = testCode.IndexOf("co") + 2; //Gets at first "co" location inside PS function

            var completionItems = new SymbolCompletionProvider().GetItems(semanticModel, new SourceLocation(index));

            var nonExpectedCompletionItem = completionItems.Where(ci => ci.Symbol.Name == "color" && ci.Symbol.Kind == SymbolKind.Variable).FirstOrDefault();

            Assert.Null(nonExpectedCompletionItem);
        }

        [Fact()]
        public void LocalDeclaration_AboveSourceLocation_DifferentScope_NotInCompletionList()
        {
            string testCode = @"float4 PS(float4 pos : SV_Position) : SV_Target
            {
                float4 color;
                return color;
            }

            float Dummy(float input)
            {
                co
            }";

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(testCode));
            var compilation = new CodeAnalysis.Hlsl.Compilation.Compilation(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            int index = testCode.LastIndexOf("co") + 2; //Gets at co inside the dummy function, since color is not part of that function, it should not be in the list

            var completionItems = new SymbolCompletionProvider().GetItems(semanticModel, new SourceLocation(index));

            var nonExpectedCompletionItem = completionItems.Where(ci => ci.Symbol.Name == "color" && ci.Symbol.Kind == SymbolKind.Variable).FirstOrDefault();

            Assert.Null(nonExpectedCompletionItem);
        }



    }
}
