using System;
using System.Windows.Controls;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.Editor.VisualStudio.Core.Options.ViewModels;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Options.ViewModels
{
    internal sealed class FormattingSpacingViewModel : HlslOptionsPreviewViewModelBase
    {
        private const string FunctionSpacingPreview = @"
void Bar(int x);
//[
void Foo()
{
    Bar(1);
}

void Bar(int x)
{
    Foo();
}
//]";

        private const string ControlFlowPreview = @"
void main()
{
//[
    for (int i = 0; i < 10; i++)
    {
    }
//]
}";

        private const string ExpressionParenthesesPreview = @"
void main()
{
//[
    int x = 3;
    int y = 4;
    int z = (x * y) - ((y - x) * 3);
//]
}";

        private const string CastPreview = @"
void main()
{
    float x;
//[
    int y = (int) x;
//]
}";

        private const string BracketsPreview = @"
//[
int x[] = { 0, 1 };
int y[2] = { 2, 3 };
//]";

        private const string BaseTypePreview = @"
interface iBaseLight
{
    float3 IlluminateAmbient(float3 vNormal);
};
//[
class cAmbientLight : iBaseLight
{
    float3 IlluminateAmbient(float3 vNormal);
};

class cHemiAmbientLight : cAmbientLight
{
    float3 IlluminateAmbient(float3 vNormal);
};
//]";

        private const string SemanticPreview = @"
//[
struct VertexInput
{
    float3 position : POSITION;
    float3 normal : NORMAL;
};

cbuffer cbPerObject : register( b0 )
{
	float4 g_vObjectColor : packoffset( c0 );
};
//]";

        private const string CommaDotPreview = @"
Texture2D myTex;
SamplerState mySampler;
void main(float2 coords)
{
//[
    float4 c = myTex.Sample(mySampler, coords);
//]
}";

        private const string BinaryOperatorSpacesPreview = @"
int main(int x, int y)
{
//[
    return x * (x * y);
//]
}";

        public FormattingSpacingViewModel(IServiceProvider serviceProvider, SpacingOptions page)
            : base(serviceProvider)
        {
            Items.Add(new TextBlock { Text = "Set spacing for function declarations" });

            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space between function name and its opening parenthesis", () => page.FunctionDeclarationInsertSpaceAfterFunctionName, v => page.FunctionDeclarationInsertSpaceAfterFunctionName = v), FunctionSpacingPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space within argument list parenthesis", () => page.FunctionDeclarationInsertSpaceWithinArgumentListParentheses, v => page.FunctionDeclarationInsertSpaceWithinArgumentListParentheses = v), FunctionSpacingPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space within empty argument list parenthesis", () => page.FunctionDeclarationInsertSpaceWithinEmptyArgumentListParentheses, v => page.FunctionDeclarationInsertSpaceWithinEmptyArgumentListParentheses = v), FunctionSpacingPreview, this));

            Items.Add(new TextBlock { Text = "Set spacing for function calls" });

            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space between function name and its opening parenthesis", () => page.FunctionDeclarationInsertSpaceAfterFunctionName, v => page.FunctionCallInsertSpaceAfterFunctionName = v), FunctionSpacingPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space within argument list parenthesis", () => page.FunctionDeclarationInsertSpaceWithinArgumentListParentheses, v => page.FunctionCallInsertSpaceWithinArgumentListParentheses = v), FunctionSpacingPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space within empty argument list parenthesis", () => page.FunctionDeclarationInsertSpaceWithinEmptyArgumentListParentheses, v => page.FunctionCallInsertSpaceWithinEmptyArgumentListParentheses = v), FunctionSpacingPreview, this));

            Items.Add(new TextBlock { Text = "Set other spacing options" });

            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space after keywords in control flow statements", () => page.InsertSpaceAfterKeywordsInControlFlowStatements, v => page.InsertSpaceAfterKeywordsInControlFlowStatements = v), ControlFlowPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert spaces within parentheses of expressions", () => page.InsertSpacesWithinParenthesesOfExpressions, v => page.InsertSpacesWithinParenthesesOfExpressions = v), ExpressionParenthesesPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert spaces within parentheses of type casts", () => page.InsertSpacesWithinParenthesesOfTypeCasts, v => page.InsertSpacesWithinParenthesesOfTypeCasts = v), CastPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert spaces within parentheses of control flow statements", () => page.InsertSpacesWithinParenthesesOfControlFlowStatements, v => page.InsertSpacesWithinParenthesesOfControlFlowStatements = v), ControlFlowPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert spaces within parentheses of register or packoffset qualifiers", () => page.InsertSpacesWithinParenthesesOfRegisterOrPackOffsetQualifiers, v => page.InsertSpacesWithinParenthesesOfRegisterOrPackOffsetQualifiers = v), SemanticPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert spaces within array initializer braces", () => page.InsertSpacesWithinArrayInitializerBraces, v => page.InsertSpacesWithinArrayInitializerBraces = v), BracketsPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space after type cast", () => page.InsertSpaceAfterCast, v => page.InsertSpaceAfterCast = v), CastPreview, this));

            Items.Add(new TextBlock { Text = "Set spacing for brackets" });

            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space before open square bracket", () => page.InsertSpaceBeforeOpenSquareBracket, v => page.InsertSpaceBeforeOpenSquareBracket = v), BracketsPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space within empty square brackets", () => page.InsertSpaceWithinEmptySquareBrackets, v => page.InsertSpaceWithinEmptySquareBrackets = v), BracketsPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert spaces within square bracket", () => page.InsertSpacesWithinSquareBrackets, v => page.InsertSpacesWithinSquareBrackets = v), BracketsPreview, this));

            Items.Add(new TextBlock { Text = "Set spacing for delimiters" });

            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space before colon for base or interface in type declaration", () => page.InsertSpaceBeforeColonForBaseOrInterfaceInTypeDeclaration, v => page.InsertSpaceBeforeColonForBaseOrInterfaceInTypeDeclaration = v), BaseTypePreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space after colon for base or interface in type declaration", () => page.InsertSpaceAfterColonForBaseOrInterfaceInTypeDeclaration, v => page.InsertSpaceAfterColonForBaseOrInterfaceInTypeDeclaration = v), BaseTypePreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space before colon for semantic or register or packoffset", () => page.InsertSpaceBeforeColonForSemanticOrRegisterOrPackOffset, v => page.InsertSpaceBeforeColonForSemanticOrRegisterOrPackOffset = v), SemanticPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space after colon for semantic or register or packoffset", () => page.InsertSpaceAfterColonForSemanticOrRegisterOrPackOffset, v => page.InsertSpaceAfterColonForSemanticOrRegisterOrPackOffset = v), SemanticPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space before comma", () => page.InsertSpaceBeforeComma, v => page.InsertSpaceBeforeComma = v), CommaDotPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space after comma", () => page.InsertSpaceAfterComma, v => page.InsertSpaceAfterComma = v), CommaDotPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space before dot", () => page.InsertSpaceBeforeDot, v => page.InsertSpaceBeforeDot = v), CommaDotPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space after dot", () => page.InsertSpaceAfterDot, v => page.InsertSpaceAfterDot = v), CommaDotPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space before semicolon in \"for\" statement", () => page.InsertSpaceBeforeSemicolonInForStatement, v => page.InsertSpaceBeforeSemicolonInForStatement = v), ControlFlowPreview, this));
            Items.Add(new CheckBoxOptionViewModel(new Option<bool>("Insert space after semicolon in \"for\" statement", () => page.InsertSpaceAfterSemicolonInForStatement, v => page.InsertSpaceAfterSemicolonInForStatement = v), ControlFlowPreview, this));

            Items.Add(new TextBlock { Text = "Set spacing for operators" });

            Items.Add(new RadioButtonViewModel<BinaryOperatorSpaces>(BinaryOperatorSpacesPreview, "binaryspaces", BinaryOperatorSpaces.RemoveSpaces,
                new Option<BinaryOperatorSpaces>("Remove spaces before and after binary operators", () => page.BinaryOperatorSpaces, v => page.BinaryOperatorSpaces = v), this));
            Items.Add(new RadioButtonViewModel<BinaryOperatorSpaces>(BinaryOperatorSpacesPreview, "binaryspaces", BinaryOperatorSpaces.InsertSpaces,
                new Option<BinaryOperatorSpaces>("Insert space before and after binary operators", () => page.BinaryOperatorSpaces, v => page.BinaryOperatorSpaces = v), this));
        }
    }
}