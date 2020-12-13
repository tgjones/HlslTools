using System;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.VisualStudio.LanguageServices.Options.UI;

namespace ShaderTools.VisualStudio.LanguageServices.Hlsl.Options.Formatting
{
    internal sealed class SpacingViewModel : AbstractOptionPreviewViewModel
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
    float3 normal   : NORMAL;
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

        public SpacingViewModel(OptionSet options, IServiceProvider serviceProvider) 
            : base(options, serviceProvider, LanguageNames.Hlsl)
        {
            Items.Add(new HeaderItemViewModel { Header = "Set spacing for function declarations" });

            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceAfterFunctionDeclarationName, "Insert space between function name and its opening parenthesis", FunctionSpacingPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinFunctionDeclarationParentheses, "Insert space within argument list parenthesis", FunctionSpacingPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinFunctionDeclarationEmptyParentheses, "Insert space within empty argument list parenthesis", FunctionSpacingPreview, this, Options));

            Items.Add(new HeaderItemViewModel { Header = "Set spacing for function calls" });

            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceAfterFunctionCallName, "Insert space between function name and its opening parenthesis", FunctionSpacingPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinFunctionCallParentheses, "Insert space within argument list parenthesis", FunctionSpacingPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinFunctionCallEmptyParentheses, "Insert space within empty argument list parenthesis", FunctionSpacingPreview, this, Options));

            Items.Add(new HeaderItemViewModel { Header = "Set other spacing options" });

            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceAfterControlFlowStatementKeyword, "Insert space after keywords in control flow statements", ControlFlowPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinExpressionParentheses, "Insert spaces within parentheses of expressions", ExpressionParenthesesPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinTypeCastParentheses, "Insert spaces within parentheses of type casts", CastPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinControlFlowStatementParentheses, "Insert spaces within parentheses of control flow statements", ControlFlowPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinRegisterOrPackOffsetParentheses, "Insert spaces within parentheses of register or packoffset qualifiers", SemanticPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinArrayInitializerBraces, "Insert spaces within array initializer braces", BracketsPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceAfterTypeCast, "Insert space after type cast", CastPreview, this, Options));

            Items.Add(new HeaderItemViewModel { Header = "Set spacing for brackets" });

            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceBeforeOpenSquareBracket, "Insert space before open square bracket", BracketsPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinEmptySquareBrackets, "Insert space within empty square brackets", BracketsPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceWithinSquareBrackets, "Insert spaces within square bracket", BracketsPreview, this, Options));

            Items.Add(new HeaderItemViewModel { Header = "Set spacing for delimiters" });

            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceBeforeColonInBaseTypeDeclaration, "Insert space before colon for base or interface in type declaration", BaseTypePreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceAfterColonInBaseTypeDeclaration, "Insert space after colon for base or interface in type declaration", BaseTypePreview, this, Options));
            Items.Add(new EnumRadioButtonsViewModel<InsertSpaceOption>("Insert space before colon for semantic or register or packoffset", SemanticPreview, "spacebeforecolon", HlslFormattingOptions.SpaceBeforeColonInSemanticOrRegisterOrPackOffset, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceAfterColonInSemanticOrRegisterOrPackOffset, "Insert space after colon for semantic or register or packoffset", SemanticPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceBeforeComma, "Insert space before comma", CommaDotPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceAfterComma, "Insert space after comma", CommaDotPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceBeforeDot, "Insert space before dot", CommaDotPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceAfterDot, "Insert space after dot", CommaDotPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceBeforeSemicolonsInForStatement, "Insert space before semicolon in \"for\" statement", ControlFlowPreview, this, Options));
            Items.Add(new CheckBoxOptionViewModel(HlslFormattingOptions.SpaceAfterSemicolonsInForStatement, "Insert space after semicolon in \"for\" statement", ControlFlowPreview, this, Options));

            Items.Add(new HeaderItemViewModel { Header = "Set spacing for operators" });

            Items.Add(new RadioButtonViewModel<InsertSpaceOption>("Remove spaces before and after binary operators", BinaryOperatorSpacesPreview, "binaryspaces", InsertSpaceOption.RemoveSpaces, HlslFormattingOptions.BinaryOperatorSpaces, this, Options));
            Items.Add(new RadioButtonViewModel<InsertSpaceOption>("Insert space before and after binary operators", BinaryOperatorSpacesPreview, "binaryspaces", InsertSpaceOption.InsertSpaces, HlslFormattingOptions.BinaryOperatorSpaces, this, Options));
        }
    }
}
