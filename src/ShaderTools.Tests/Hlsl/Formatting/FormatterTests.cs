using System;
using System.Collections.Generic;
using NUnit.Framework;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;
using ShaderTools.Tests.Hlsl.Support;

namespace ShaderTools.Tests.Hlsl.Formatting
{
    [TestFixture]
    internal class FormatterTests
    {
        [Test]
        public void Annotations()
        {
            const string before = @"
float4x4 WorldXf : World < string UIWidget=""None""; >;

float bumpHeight <
    string UIWidget = ""slider"";
    float UIMin = 0.0;
    float UIMax = 2.0;
    float UIStep = 0.1;
    string UIName = ""Bump Height"";
> = 0.5;";

            AssertFormat(before, @"
float4x4 WorldXf : World < string UIWidget=""None""; >;

float bumpHeight <
    string UIWidget = ""slider"";
    float UIMin = 0.0;
    float UIMax = 2.0;
    float UIStep = 0.1;
    string UIName = ""Bump Height"";
> = 0.5;");
        }

        [Test]
        public void Attributes()
        {
            const string before = @"
void main()
{
    [unroll] for( uint v=0; v<4; ++ v )
    {
    }
}";

            AssertFormat(before, @"
void main()
{
    [unroll]
    for (uint v = 0; v < 4; ++v)
    {
    }
}");
        }

        [Test]
        public void BuiltInTypes()
        {
            const string before = @"
int i;
unsigned  int ui;
vector < float ,  3> v1;
vector v2;
float3 v4;
matrix m1;
float4x4 m2;
matrix < float ,  4,  5> m3;

struct MyStruct { float3 LightPosition; };

ConstantBuffer  <  MyStruct > cb;

Texture2D t1;
Texture2D  < float4   > t2;

 DepthStencilState  DisableDepth
{
    DepthEnable  =  FALSE;   DepthWriteMask = ALL;
    DepthFunc = LESS_EQUAL ;
 } ; 

samplerCUBE  envMapSampler = sampler_state  { 
    Texture  =  < cubeMap > ;    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};";

            AssertFormat(before, @"
int i;
unsigned int ui;
vector<float, 3> v1;
vector v2;
float3 v4;
matrix m1;
float4x4 m2;
matrix<float, 4, 5> m3;

struct MyStruct
{
    float3 LightPosition;
};

ConstantBuffer<MyStruct> cb;

Texture2D t1;
Texture2D<float4> t2;

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ALL;
    DepthFunc = LESS_EQUAL;
};

samplerCUBE envMapSampler = sampler_state
{
    Texture = <cubeMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};");
        }

        [Test]
        public void ConditionalExpression()
        {
            const string before = @"
void main()
{
    bool b =  ( 1  ==  1   )   ?  true   :  false  ;
}";

            AssertFormat(before, @"
void main()
{
    bool b = (1 == 1) ? true : false;
}");
        }

        [Test]
        public void Directives()
        {
            const string before = @"
 # if  1  ==  1
#endif

#ifdef FOO
 #endif

# ifndef FOO
#elif  defined ( BAR )  
# else  
#endif 

# define  FOO  
# define  BAZ(a ,  b)  a   +  b


# include  <include.hlsli> 

# undef  FOO  

# line  20  ""Myfile.hlsl"" ";

            var includes = new Dictionary<string, string>
            {
                { "include.hlsli", "// Nothing here" }
            };

            AssertFormat(before, @"
#if  1  ==  1
#endif

#ifdef FOO
#endif

#ifndef FOO
#elif  defined ( BAR )  
#else  
#endif 

#define  FOO  
#define  BAZ(a ,  b)  a   +  b


#include  <include.hlsli> 

#undef  FOO  

#line  20  ""Myfile.hlsl"" ", includes: includes);
        }

        [Test]
        public void MacroExpansion()
        {
            const string before = @"
#define FOO   1    + 2
#define BAR( a , b )  a  ##  b  

int i =  FOO ;
int j =  BAR( 1 , 2 ) ;";

            AssertFormat(before, @"
#define FOO   1    + 2
#define BAR( a , b )  a  ##  b  

int i = FOO;
int j = BAR(1, 2);");
        }

        [Test]
        public void StateArrays()
        {
            const string before = @"
RasterizerState g_rasterizerState[2]
{
{
    FillMode = SOLID;
    MultisampleEnable = true;
},
{    FillMode = WIREFRAME;
    MultisampleEnable = true;
}
};";

            AssertFormat(before, @"
RasterizerState g_rasterizerState[2]
{
    {
        FillMode = SOLID;
        MultisampleEnable = true;
    },
    {
        FillMode = WIREFRAME;
        MultisampleEnable = true;
    }
};");
        }

        [Test]
        public void Empty()
        {
            AssertFormat("", "");
        }

        [Test]
        public void VariableDeclaration1()
        {
            AssertFormat("int  i;", "int i;");
        }

        [Test]
        public void VariableDeclaration2()
        {
            AssertFormat("int  i; ", "int i;");
        }

        [Test]
        public void VariableDeclarationWithEqualsClauseInitializer()
        {
            AssertFormat("int  i  =  0 ;", "int i = 0;");
        }

        [Test]
        public void VariableDeclarationWithInlineComments()
        {
            AssertFormat("int  i  /*  comment */    /* another comment */  ;", "int i /*  comment */ /* another comment */;");
            AssertFormat("int  i/*  comment *//* another comment */  ;", "int i /*  comment */ /* another comment */;");
            AssertFormat("int  /*  comment */    /* another comment */   i;", "int /*  comment */ /* another comment */ i;");
        }

        [Test]
        public void VariableDeclarationWithMultiTokenType()
        {
            AssertFormat("unsigned  int  i  ;", "unsigned int i;");
        }

        [Test]
        public void VariableDeclarationWithMultipleVariables()
        {
            AssertFormat("int  i=0,j;", "int i = 0, j;");
        }

        [Test]
        public void ForLoop()
        {
            AssertFormat(@"
void main()
{
    for(int  i=0;i<3;i++ ) {}
}", @"
void main()
{
    for (int i = 0; i < 3; i++)
    {
    }
}");
        }

        [Test]
        public void IndentBlockContents()
        {
            const string before = @"
int Method()
{
    int x;
    int y;
}";

            var options = new FormattingOptions
            {
                Indentation =
                {
                    IndentBlockContents = false
                }
            };
            AssertFormat(before, @"
int Method()
{
int x;
int y;
}", options: options);

            options = new FormattingOptions
            {
                Indentation =
                {
                    IndentBlockContents = true
                }
            };
            AssertFormat(before, @"
int Method()
{
    int x;
    int y;
}", options: options);
        }

        [Test]
        public void IndentBraces()
        {
            const string before = @"
int Method()
{ 
    int x;
    int y;
}";

            var options = new FormattingOptions
            {
                Indentation =
                {
                    IndentOpenAndCloseBraces = false
                }
            };
            AssertFormat(before, @"
int Method()
{
    int x;
    int y;
}", options: options);

            options = new FormattingOptions
            {
                Indentation =
                {
                    IndentOpenAndCloseBraces = true
                }
            };
            AssertFormat(before, @"
int Method()
    {
    int x;
    int y;
    }", options: options);
        }

        [Test]
        public void IndentSwitchCase()
        {
            const string before = @"
void main(int foo)
{
     switch  ( foo ) 
     { 
         case  0 : 
         break ;

         default : 
         break ; 
     } 
}";

            var options = new FormattingOptions
            {
                Indentation =
                {
                    IndentCaseContents = false,
                    IndentCaseLabels = false
                }
            };
            AssertFormat(before, @"
void main(int foo)
{
    switch (foo)
    {
    case 0:
    break;

    default:
    break;
    }
}", options: options);

            options = new FormattingOptions
            {
                Indentation =
                {
                    IndentCaseContents = true,
                    IndentCaseLabels = true
                }
            };
            AssertFormat(before, @"
void main(int foo)
{
    switch (foo)
    {
        case 0:
            break;

        default:
            break;
    }
}", options: options);
        }

        [Test]
        public void IndentPreprocessorDirectives()
        {
            const string before = @"
void main(int foo)
{
    if (true)
    {
      #if TEXTURES
        float4 color = ReadTexture();
    #endif
    }
}";

            var options = new FormattingOptions
            {
                Indentation =
                {
                    PreprocessorDirectivePosition = PreprocessorDirectivePosition.LeaveIndented
                }
            };
            AssertFormat(before, @"
void main(int foo)
{
    if (true)
    {
      #if TEXTURES
        float4 color = ReadTexture();
    #endif
    }
}", options: options);

            options = new FormattingOptions
            {
                Indentation =
                {
                    PreprocessorDirectivePosition = PreprocessorDirectivePosition.MoveToLeftmostColumn
                }
            };
            AssertFormat(before, @"
void main(int foo)
{
    if (true)
    {
#if TEXTURES
        float4 color = ReadTexture();
#endif
    }
}", options: options);

            options = new FormattingOptions
            {
                Indentation =
                {
                    PreprocessorDirectivePosition = PreprocessorDirectivePosition.OneIndentToLeft
                }
            };
            AssertFormat(before, @"
void main(int foo)
{
    if (true)
    {
    #if TEXTURES
        float4 color = ReadTexture();
    #endif
    }
}", options: options);
        }

        [Test]
        public void BraceOnNewLineForTypes()
        {
            const string before = @"
 struct  S   {
     float4 position;
    float3 normal; };

 class  C   
{

float4 position;
    float3 normal; };";

            var options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForTypes = false
                }
            };
            AssertFormat(before, @"
struct S {
    float4 position;
    float3 normal;
};

class C {

    float4 position;
    float3 normal;
};", options: options);

            options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForTypes = true
                }
            };
            AssertFormat(before, @"
struct S
{
    float4 position;
    float3 normal;
};

class C
{

    float4 position;
    float3 normal;
};", options: options);
        }

        [Test]
        public void BraceOnNewLineWithInterveningSingleLineComment()
        {
            const string before = @"
struct  S

// This is a comment.


{
    float4 position;
    float3 normal;
};";

            var options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForTypes = false
                }
            };
            AssertFormat(before, @"
struct S

// This is a comment.
{
    float4 position;
    float3 normal;
};", options: options);

            options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForTypes = true
                }
            };
            AssertFormat(before, @"
struct S

// This is a comment.
{
    float4 position;
    float3 normal;
};", options: options);
        }

        [Test]
        public void BraceOnNewLineForTechniquesAndPasses()
        {
            const string before = @"
 technique  T  {  
     pass  P {  // A comment
        VertexShader  = compile  vs_2_0  VS ( ) ; 
        PixelShader  = compile ps_2_0 PS();
    }
}";

            var options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForTechniquesAndPasses = false
                }
            };
            AssertFormat(before, @"
technique T {
    pass P { // A comment
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}", options: options);

            options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForTechniquesAndPasses = true
                }
            };
            AssertFormat(before, @"
technique T
{
    pass P
    { // A comment
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}", options: options);
        }

        [Test]
        public void BraceOnNewLineForFunctions()
        {
            const string before = @"
float4 main() {
    return float4(1, 0, 0, 1);
}";

            var options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForFunctions = false
                }
            };
            AssertFormat(before, @"
float4 main() {
    return float4(1, 0, 0, 1);
}", options: options);

            options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForFunctions = true
                }
            };
            AssertFormat(before, @"
float4 main()
{
    return float4(1, 0, 0, 1);
}", options: options);
        }

        [Test]
        public void BraceOnNewLineForControlBlocks()
        {
            const string before = @"
float4 main() {
    for (int i = 0; i < 10; i++) {
    }
}";

            var options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForControlBlocks = false
                }
            };
            AssertFormat(before, @"
float4 main()
{
    for (int i = 0; i < 10; i++) {
    }
}", options: options);

            options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForControlBlocks = true
                }
            };
            AssertFormat(before, @"
float4 main()
{
    for (int i = 0; i < 10; i++)
    {
    }
}", options: options);
        }

        [Test]
        public void BraceOnNewLineForStateBlocks()
        {
            const string before = @"
DepthStencilState DepthDisabling {
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};";

            var options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForStateBlocks = false
                }
            };
            AssertFormat(before, @"
DepthStencilState DepthDisabling {
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};", options: options);

            options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForStateBlocks = true
                }
            };
            AssertFormat(before, @"
DepthStencilState DepthDisabling
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};", options: options);
        }

        [Test]
        public void BraceOnNewLineForArrayInitializerBraces()
        {
            const string before = @"
int arrayVariable[2] = { 
     1 ,  2 
 } ; 

float3  wave [ 2 ]  =  
{
	{ 1.0, 1.0, 0.5 },  
	{ 2.0, 0.5, 
        1.3
    }	
};";

            var options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForArrayInitializers = false
                }
            };
            AssertFormat(before, @"
int arrayVariable[2] = {
    1, 2
};

float3 wave[2] = {
    { 1.0, 1.0, 0.5 },
    {
        2.0, 0.5,
        1.3
    }
};", options: options);

            options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceOpenBraceOnNewLineForArrayInitializers = true
                }
            };
            AssertFormat(before, @"
int arrayVariable[2] =
{
    1, 2
};

float3 wave[2] =
{
    { 1.0, 1.0, 0.5 },
    {
        2.0, 0.5,
        1.3
    }
};", options: options);
        }

        [Test]
        public void ElseOnNewLine()
        {
            const string before = @"
void main()
{
    if (false) {
    } else  {
    }
}";

            var options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceElseOnNewLine = false
                }
            };
            AssertFormat(before, @"
void main()
{
    if (false)
    {
    } else
    {
    }
}", options: options);

            options = new FormattingOptions
            {
                NewLines =
                {
                    PlaceElseOnNewLine = true
                }
            };
            AssertFormat(before, @"
void main()
{
    if (false)
    {
    }
    else
    {
    }
}", options: options);
        }

        [Test]
        public void FunctionDeclarationSpacing()
        {
            const string before = @"
void Foo()
{
    Bar(1);
}

void Bar(int x)
{
    Foo();
}";

            var options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    FunctionDeclarationInsertSpaceAfterFunctionName = false,
                    FunctionDeclarationInsertSpaceWithinArgumentListParentheses = false,
                    FunctionDeclarationInsertSpaceWithinEmptyArgumentListParentheses = false
                }
            };
            AssertFormat(before, @"
void Foo()
{
    Bar(1);
}

void Bar(int x)
{
    Foo();
}", options: options);

            options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    FunctionDeclarationInsertSpaceAfterFunctionName = true,
                    FunctionDeclarationInsertSpaceWithinArgumentListParentheses = true,
                    FunctionDeclarationInsertSpaceWithinEmptyArgumentListParentheses = true
                }
            };
            AssertFormat(before, @"
void Foo ( )
{
    Bar(1);
}

void Bar ( int x )
{
    Foo();
}", options: options);
        }

        [Test]
        public void FunctionCallSpacing()
        {
            const string before = @"
void Foo()
{
    Bar(1);
}

void Bar(int x)
{
    Foo();

    float4 v = float4 ( 1, 2, 3, 4);
}";

            var options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    FunctionCallInsertSpaceAfterFunctionName = false,
                    FunctionCallInsertSpaceWithinArgumentListParentheses = false,
                    FunctionCallInsertSpaceWithinEmptyArgumentListParentheses = false
                }
            };
            AssertFormat(before, @"
void Foo()
{
    Bar(1);
}

void Bar(int x)
{
    Foo();

    float4 v = float4(1, 2, 3, 4);
}", options: options);

            options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    FunctionCallInsertSpaceAfterFunctionName = true,
                    FunctionCallInsertSpaceWithinArgumentListParentheses = true,
                    FunctionCallInsertSpaceWithinEmptyArgumentListParentheses = true
                }
            };
            AssertFormat(before, @"
void Foo()
{
    Bar ( 1 );
}

void Bar(int x)
{
    Foo ( );

    float4 v = float4 ( 1, 2, 3, 4 );
}", options: options);
        }

        [Test]
        public void ControlFlowStatements()
        {
            const string before = @"
void main()
{
    for ( int  i  =  0 ; i  <  x ; i ++)
    {  
    }

    while (true)
    {
    }

    do
    {

    } while (true);

    switch (1)
    {     
    }

    if (true)
    {
    }
    else if (false)
    {
    }
    else
    {
    }
}";

            var options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpaceAfterKeywordsInControlFlowStatements = false,
                    InsertSpacesWithinParenthesesOfControlFlowStatements = false
                }
            };
            AssertFormat(before, @"
void main()
{
    for(int i = 0; i < x; i++)
    {
    }

    while(true)
    {
    }

    do
    {

    } while(true);

    switch(1)
    {
    }

    if(true)
    {
    }
    else if(false)
    {
    }
    else
    {
    }
}", options: options);

            options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpaceAfterKeywordsInControlFlowStatements = true,
                    InsertSpacesWithinParenthesesOfControlFlowStatements = true
                }
            };
            AssertFormat(before, @"
void main()
{
    for ( int i = 0; i < x; i++ )
    {
    }

    while ( true )
    {
    }

    do
    {

    } while ( true );

    switch ( 1 )
    {
    }

    if ( true )
    {
    }
    else if ( false )
    {
    }
    else
    {
    }
}", options: options);
        }

        [Test]
        public void SpaceWithinExpressionParentheses()
        {
            const string before = @"
void main()
{
    int x = 3;
    int y = 4;
    int z = (x * y) - ((y - x) * 3);
}";

            AssertFormat(before, @"
void main()
{
    int x = 3;
    int y = 4;
    int z = (x * y) - ((y - x) * 3);
}", options: new FormattingOptions { Spacing = new SpacingOptions { InsertSpacesWithinParenthesesOfExpressions = false } });

            AssertFormat(before, @"
void main()
{
    int x = 3;
    int y = 4;
    int z = ( x * y ) - ( ( y - x ) * 3 );
}", options: new FormattingOptions { Spacing = new SpacingOptions { InsertSpacesWithinParenthesesOfExpressions = true } });
        }

        [Test]
        public void TypeCastSpacing()
        {
            const string before = @"
void main()
{
    int y = (int) x;
}";

            var options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpaceAfterCast = false,
                    InsertSpacesWithinParenthesesOfTypeCasts = false
                }
            };
            AssertFormat(before, @"
void main()
{
    int y = (int)x;
}", options: options);

            options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpaceAfterCast = true,
                    InsertSpacesWithinParenthesesOfTypeCasts = true
                }
            };
            AssertFormat(before, @"
void main()
{
    int y = ( int ) x;
}", options: options);
        }

        [Test]
        public void DeclarationStatementSpacing()
        {
            const string before = @"
int        index = 0;
float3     pos   = float3(1, 2, 3);
";

            AssertFormat(before, @"
int index = 0;
float3 pos = float3(1, 2, 3);
");
        }

        [Test]
        public void SemanticRegisterPackOffsetSpacing()
        {
            const string before = @"
struct  VertexInput
{
    float3 position : POSITION  : register(b1);
    float3 normal : NORMAL;
};

cbuffer  cbPerObject : register(b0 )  
{
	float4 g_vObjectColor : packoffset(c0);
};
";

            var options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpacesWithinParenthesesOfRegisterOrPackOffsetQualifiers = false,
                    InsertSpaceBeforeColonForSemanticOrRegisterOrPackOffset = false,
                    InsertSpaceAfterColonForSemanticOrRegisterOrPackOffset = false
                }
            };
            AssertFormat(before, @"
struct VertexInput
{
    float3 position:POSITION:register(b1);
    float3 normal:NORMAL;
};

cbuffer cbPerObject:register(b0)
{
    float4 g_vObjectColor:packoffset(c0);
};
", options: options);

            options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpacesWithinParenthesesOfRegisterOrPackOffsetQualifiers = true,
                    InsertSpaceBeforeColonForSemanticOrRegisterOrPackOffset = true,
                    InsertSpaceAfterColonForSemanticOrRegisterOrPackOffset = true
                }
            };
            AssertFormat(before, @"
struct VertexInput
{
    float3 position : POSITION : register( b1 );
    float3 normal : NORMAL;
};

cbuffer cbPerObject : register( b0 )
{
    float4 g_vObjectColor : packoffset( c0 );
};
", options: options);
        }

        [Test]
        public void BracketsSpacing()
        {
            const string before = @"
int x[] = { 0, 1 };
int y[2] = { 2, 3 };

void main()
{
    int z = x [ 0 ];
}";

            var options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpacesWithinSquareBrackets = false,
                    InsertSpaceWithinEmptySquareBrackets = false,
                    InsertSpaceBeforeOpenSquareBracket = false,
                    InsertSpacesWithinArrayInitializerBraces = false
                }
            };
            AssertFormat(before, @"
int x[] = {0, 1};
int y[2] = {2, 3};

void main()
{
    int z = x[0];
}", options: options);

            options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpacesWithinSquareBrackets = true,
                    InsertSpaceWithinEmptySquareBrackets = true,
                    InsertSpaceBeforeOpenSquareBracket = true,
                    InsertSpacesWithinArrayInitializerBraces = true
                }
            };
            AssertFormat(before, @"
int x [ ] = { 0, 1 };
int y [ 2 ] = { 2, 3 };

void main()
{
    int z = x [ 0 ];
}", options: options);
        }

        [Test]
        public void BaseTypeSpacing()
        {
            const string before = @"
 interface  iBaseLight
{
     float3  IlluminateAmbient ( float3 vNormal ) ; 
} ;

 class  cAmbientLight  :  iBaseLight  // Comment
{
    float3 IlluminateAmbient(float3 vNormal);
};

 class  cHemiAmbientLight  :  cAmbientLight 
{
    float3 IlluminateAmbient(float3 vNormal);  
};  
";

            var options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpaceBeforeColonForBaseOrInterfaceInTypeDeclaration = false,
                    InsertSpaceAfterColonForBaseOrInterfaceInTypeDeclaration = false
                }
            };
            AssertFormat(before, @"
interface iBaseLight
{
    float3 IlluminateAmbient(float3 vNormal);
};

class cAmbientLight:iBaseLight // Comment
{
    float3 IlluminateAmbient(float3 vNormal);
};

class cHemiAmbientLight:cAmbientLight
{
    float3 IlluminateAmbient(float3 vNormal);
};
", options: options);

            options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpaceBeforeColonForBaseOrInterfaceInTypeDeclaration = true,
                    InsertSpaceAfterColonForBaseOrInterfaceInTypeDeclaration = true
                }
            };
            AssertFormat(before, @"
interface iBaseLight
{
    float3 IlluminateAmbient(float3 vNormal);
};

class cAmbientLight : iBaseLight // Comment
{
    float3 IlluminateAmbient(float3 vNormal);
};

class cHemiAmbientLight : cAmbientLight
{
    float3 IlluminateAmbient(float3 vNormal);
};
", options: options);
        }

        [Test]
        public void CommaDotSpacing()
        {
            const string before = @"
void main()
{
    float4 c = myTex. Sample(mySampler, coords);
}
";

            var options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpaceBeforeComma = false,
                    InsertSpaceAfterComma = false,
                    InsertSpaceBeforeDot = false,
                    InsertSpaceAfterDot = false,
                }
            };
            AssertFormat(before, @"
void main()
{
    float4 c = myTex.Sample(mySampler,coords);
}
", options: options);

            options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    InsertSpaceBeforeComma = true,
                    InsertSpaceAfterComma = true,
                    InsertSpaceBeforeDot = true,
                    InsertSpaceAfterDot = true
                }
            };
            AssertFormat(before, @"
void main()
{
    float4 c = myTex . Sample(mySampler , coords);
}
", options: options);
        }

        [Test]
        public void BinaryOperatorSpacing()
        {
            const string before = @"
void main(int x, int y)
{
    int z = x *  (x * y);
}";

            var options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    BinaryOperatorSpaces = BinaryOperatorSpaces.InsertSpaces
                }
            };
            AssertFormat(before, @"
void main(int x, int y)
{
    int z = x * (x * y);
}", options: options);

            options = new FormattingOptions
            {
                Spacing = new SpacingOptions
                {
                    BinaryOperatorSpaces = BinaryOperatorSpaces.RemoveSpaces
                }
            };
            AssertFormat(before, @"
void main(int x, int y)
{
    int z = x*(x*y);
}", options: options);
        }

        [Test]
        public void DoNotKeepStatementAndMemberDeclarationsOnSameLine()
        {
            const string before = @"
void main()
{
    { int a = 0; }
    int i = 0; float4 pos;
    for/* comment */(int  i=0;i<3;i ++ ) {}
}";

            AssertFormat(before, @"
void main()
{
    {
        int a = 0;
    }
    int i = 0;
    float4 pos;
    for /* comment */(int i = 0; i < 3; i++)
    {
    }
}");
        }

        [Test]
        public void LeaveMultilineExpressionsAlone()
        {
            const string before = @"
int main(int x, int y)
{
    int z = x * 
        (x * 
y);

      int b = (true) ? 1
 : 0;

    if (true
        || false)
    {

     }
}
";

            AssertFormat(before, @"
int main(int x, int y)
{
    int z = x *
        (x *
y);

    int b = (true) ? 1
 : 0;

    if (true
        || false)
    {

    }
}
");
        }

        [Test]
        public void TrailingWhitespace()
        {
            const string before = @"
void main()
{
        /* compute triangle edges */
    float3 e0 = v1 - v0;       /* tri edge 0 */
    float3 e1 = v2 - v1;       /* tri edge 1 */
    float3 e2 = v0 - v2;       /* tri edge 2 */
    ;  /* Comment after whitespace, followed by whitespace */ 
    ;  /* Comment after whitespace */
    ;  // Comment after whitespace
    ;  
    ;// Comment
    ;;
}";

            AssertFormat(before, @"
void main()
{
        /* compute triangle edges */
    float3 e0 = v1 - v0; /* tri edge 0 */
    float3 e1 = v2 - v1; /* tri edge 1 */
    float3 e2 = v0 - v2; /* tri edge 2 */
    ; /* Comment after whitespace, followed by whitespace */
    ; /* Comment after whitespace */
    ; // Comment after whitespace
    ;
    ; // Comment
    ;;
}");
        }

        [Test]
        public void KitchenSink()
        {
            const string before = @"
// A single line comment
/* A multiline comment */
struct  VertexInput
{
    float3 Position  : POSITION;
    float3 Normal : NORMAL;
} ;

struct VertexOutput  
{
  float4 Position : SV_Position;
 }
;

 matrix  WorldViewProjection  :  WVP;

#define TRANSFORM(m,  v) mul(m,  \
    v)

 VertexOutput  VS( VertexInput  input  ) {
          VertexOutput output;   // Single line comment
output.Position = TRANSFORM(WorldViewProjection,   input.Position );
return output;// Another comment
 }

struct S
#include ""BadlyFormed.hlsl""
};


";

            var includes = new Dictionary<string, string>
            {
                { "BadlyFormed.hlsl", "{ float4 position;" }
            };

            AssertFormat(before, @"
// A single line comment
/* A multiline comment */
struct VertexInput
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
};

struct VertexOutput
{
    float4 Position : SV_Position;
};

matrix WorldViewProjection : WVP;

#define TRANSFORM(m,  v) mul(m,  \
    v)

VertexOutput VS(VertexInput input)
{
    VertexOutput output; // Single line comment
    output.Position = TRANSFORM(WorldViewProjection, input.Position);
    return output; // Another comment
}

struct S
#include ""BadlyFormed.hlsl""
};


", includes: includes);
        }

        [Test]
        public void LeaveSkippedTokensAlone()
        {
            // This is the current behaviour, but it's not ideal.
            const string before = @"
float 4;
float  f3;
";

            AssertFormat(before, @"
float4;
float f3;
", allowSyntaxErrors: true);
        }

        private static void AssertFormat(string unformattedText, string expectedNewText, 
            TextSpan? textSpan = null, FormattingOptions options = null,
            Dictionary<string, string> includes = null,
            bool allowSyntaxErrors = false)
        {
            Func<string, SyntaxTree> parse = code =>
                SyntaxFactory.ParseSyntaxTree(SourceText.From(code, "__RootFile__.hlsl"),
                    fileSystem: new InMemoryFileSystem(includes ?? new Dictionary<string, string>()));

            // Arrange.
            var syntaxTree = parse(unformattedText);
            if (textSpan == null)
                textSpan = new TextSpan(syntaxTree.Text, 0, unformattedText.Length);
            if (options == null)
                options = new FormattingOptions();

            // Act.
            var edits = Formatter.GetEdits(syntaxTree, textSpan.Value, options);
            var formattedCode = Formatter.ApplyEdits(unformattedText, edits);

            // Assert.
            if (!allowSyntaxErrors)
                ShaderTestUtility.CheckForParseErrors(parse(formattedCode));
            Assert.That(formattedCode, Is.EqualTo(expectedNewText));
        }

        // TODO: CanFormatInvalidCode
    }
}