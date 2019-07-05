using System;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public static class SyntaxFacts
    {
        public static string GetDisplayText(this SyntaxToken token)
        {
            var result = token.Text;
            return !string.IsNullOrEmpty(result) ? result : token.Kind.GetDisplayText();
        }

        public static string GetDisplayText(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.EndOfFileToken:
                    return "<end-of-file>";

                case SyntaxKind.IdentifierToken:
                    return "<identifier>";

                case SyntaxKind.FloatLiteralToken:
                    return "<float-literal>";

                case SyntaxKind.IntegerLiteralToken:
                    return "<integer-literal>";

                case SyntaxKind.StringLiteralToken:
                    return "<string-literal>";

                default:
                    return GetText(kind);
            }
        }

        public static string GetText(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.TildeToken:
                    return "~";
                case SyntaxKind.NotToken:
                    return "!";
                case SyntaxKind.PercentToken:
                    return "%";
                case SyntaxKind.CaretToken:
                    return "^";
                case SyntaxKind.AmpersandToken:
                    return "&";
                case SyntaxKind.AsteriskToken:
                    return "*";
                case SyntaxKind.OpenParenToken:
                    return "(";
                case SyntaxKind.CloseParenToken:
                    return ")";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.EqualsToken:
                    return "=";
                case SyntaxKind.OpenBraceToken:
                    return "{";
                case SyntaxKind.CloseBraceToken:
                    return "}";
                case SyntaxKind.OpenBracketToken:
                    return "[";
                case SyntaxKind.CloseBracketToken:
                    return "]";
                case SyntaxKind.BarToken:
                    return "|";
                case SyntaxKind.ColonToken:
                    return ":";
                case SyntaxKind.SemiToken:
                    return ";";
                //case SyntaxKind.DoubleQuoteToken:
                //    return "\"";
                case SyntaxKind.LessThanToken:
                    return "<";
                case SyntaxKind.CommaToken:
                    return ",";
                case SyntaxKind.GreaterThanToken:
                    return ">";
                case SyntaxKind.DotToken:
                    return ".";
                case SyntaxKind.QuestionToken:
                    return "?";
                case SyntaxKind.SlashToken:
                    return "/";

                // compound
                case SyntaxKind.BarBarToken:
                    return "||";
                case SyntaxKind.AmpersandAmpersandToken:
                    return "&&";
                case SyntaxKind.MinusMinusToken:
                    return "--";
                case SyntaxKind.PlusPlusToken:
                    return "++";
                case SyntaxKind.ColonColonToken:
                    return "::";
                case SyntaxKind.ExclamationEqualsToken:
                    return "!=";
                case SyntaxKind.EqualsEqualsToken:
                    return "==";
                case SyntaxKind.LessThanEqualsToken:
                    return "<=";
                case SyntaxKind.LessThanLessThanToken:
                    return "<<";
                case SyntaxKind.LessThanLessThanEqualsToken:
                    return "<<=";
                case SyntaxKind.GreaterThanEqualsToken:
                    return ">=";
                case SyntaxKind.GreaterThanGreaterThanToken:
                    return ">>";
                case SyntaxKind.GreaterThanGreaterThanEqualsToken:
                    return ">>=";
                case SyntaxKind.SlashEqualsToken:
                    return "/=";
                case SyntaxKind.AsteriskEqualsToken:
                    return "*=";
                case SyntaxKind.BarEqualsToken:
                    return "|=";
                case SyntaxKind.AmpersandEqualsToken:
                    return "&=";
                case SyntaxKind.PlusEqualsToken:
                    return "+=";
                case SyntaxKind.MinusEqualsToken:
                    return "-=";
                case SyntaxKind.CaretEqualsToken:
                    return "^=";
                case SyntaxKind.PercentEqualsToken:
                    return "%=";

                // Keywords
                case SyntaxKind.FalseKeyword:
                    return "false";
                case SyntaxKind.TrueKeyword:
                    return "true";

                case SyntaxKind.ShaderKeyword:
                    return "Shader";
                case SyntaxKind.PropertiesKeyword:
                    return "Properties";
                case SyntaxKind.RangeKeyword:
                    return "Range";
                case SyntaxKind.FloatKeyword:
                    return "Float";
                case SyntaxKind.IntKeyword:
                    return "Int";
                case SyntaxKind.ColorKeyword:
                    return "Color";
                case SyntaxKind.VectorKeyword:
                    return "Vector";
                case SyntaxKind._2DKeyword:
                    return "2D";
                case SyntaxKind._3DKeyword:
                    return "3D";
                case SyntaxKind.CubeKeyword:
                    return "Cube";
                case SyntaxKind.AnyKeyword:
                    return "Any";
                case SyntaxKind.SubShaderKeyword:
                    return "SubShader";
                case SyntaxKind.CategoryKeyword:
                    return "Category";
                case SyntaxKind.TagsKeyword:
                    return "Tags";
                case SyntaxKind.PassKeyword:
                    return "Pass";
                case SyntaxKind.CgProgramKeyword:
                    return "CGPROGRAM";
                case SyntaxKind.HlslProgramKeyword:
                    return "HLSLPROGRAM";
                case SyntaxKind.CgIncludeKeyword:
                    return "CGINCLUDE";
                case SyntaxKind.HlslIncludeKeyword:
                    return "HLSLINCLUDE";
                case SyntaxKind.EndCgKeyword:
                    return "ENDCG";
                case SyntaxKind.EndHlslKeyword:
                    return "ENDHLSL";
                case SyntaxKind.FallbackKeyword:
                    return "Fallback";
                case SyntaxKind.CustomEditorKeyword:
                    return "CustomEditor";
                case SyntaxKind.CullKeyword:
                    return "Cull";
                case SyntaxKind.ZWriteKeyword:
                    return "ZWrite";
                case SyntaxKind.ZTestKeyword:
                    return "ZTest";
                case SyntaxKind.OffsetKeyword:
                    return "Offset";
                case SyntaxKind.BlendKeyword:
                    return "Blend";
                case SyntaxKind.BlendOpKeyword:
                    return "BlendOp";
                case SyntaxKind.ColorMaskKeyword:
                    return "ColorMask";
                case SyntaxKind.AlphaToMaskKeyword:
                    return "AlphaToMask";
                case SyntaxKind.LodKeyword:
                    return "LOD";
                case SyntaxKind.NameKeyword:
                    return "Name";
                case SyntaxKind.LightingKeyword:
                    return "Lighting";
                case SyntaxKind.StencilKeyword:
                    return "Stencil";
                case SyntaxKind.RefKeyword:
                    return "Ref";
                case SyntaxKind.ReadMaskKeyword:
                    return "ReadMask";
                case SyntaxKind.WriteMaskKeyword:
                    return "WriteMask";
                case SyntaxKind.CompKeyword:
                    return "Comp";
                case SyntaxKind.CompBackKeyword:
                    return "CompBack";
                case SyntaxKind.CompFrontKeyword:
                    return "CompFront";
                case SyntaxKind.FailKeyword:
                    return "Fail";
                case SyntaxKind.ZFailKeyword:
                    return "ZFail";
                case SyntaxKind.UsePassKeyword:
                    return "UsePass";
                case SyntaxKind.GrabPassKeyword:
                    return "GrabPass";
                case SyntaxKind.DependencyKeyword:
                    return "Dependency";
                case SyntaxKind.MaterialKeyword:
                    return "Material";
                case SyntaxKind.DiffuseKeyword:
                    return "Diffuse";
                case SyntaxKind.AmbientKeyword:
                    return "Ambient";
                case SyntaxKind.ShininessKeyword:
                    return "Shininess";
                case SyntaxKind.SpecularKeyword:
                    return "Specular";
                case SyntaxKind.EmissionKeyword:
                    return "Emission";
                case SyntaxKind.FogKeyword:
                    return "Fog";
                case SyntaxKind.ModeKeyword:
                    return "Mode";
                case SyntaxKind.DensityKeyword:
                    return "Density";
                case SyntaxKind.SeparateSpecularKeyword:
                    return "SeparateSpecular";
                case SyntaxKind.SetTextureKeyword:
                    return "SetTexture";
                case SyntaxKind.CombineKeyword:
                    return "Combine";
                case SyntaxKind.AlphaKeyword:
                    return "Alpha";
                case SyntaxKind.LerpKeyword:
                    return "lerp";
                case SyntaxKind.DoubleKeyword:
                    return "Double";
                case SyntaxKind.QuadKeyword:
                    return "Quad";
                case SyntaxKind.ConstantColorKeyword:
                    return "ConstantColor";
                case SyntaxKind.MatrixKeyword:
                    return "Matrix";
                case SyntaxKind.AlphaTestKeyword:
                    return "AlphaTest";
                case SyntaxKind.ColorMaterialKeyword:
                    return "ColorMaterial";
                case SyntaxKind.BindChannelsKeyword:
                    return "BindChannels";
                case SyntaxKind.BindKeyword:
                    return "Bind";

                default:
                    return string.Empty;
            }
        }

        public static SyntaxKind GetContextualKeywordKind(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            switch (text)
            {
                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static SyntaxKind GetUnityKeywordKind(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            // Every Unity keyword apart from CGPROGRAM and CGINCLUDE is case insensitive.
            switch (text)
            {
                case "CGPROGRAM":
                    return SyntaxKind.CgProgramKeyword;
                case "HLSLPROGRAM":
                    return SyntaxKind.HlslProgramKeyword;
                case "CGINCLUDE":
                    return SyntaxKind.CgIncludeKeyword;
                case "HLSLINCLUDE":
                    return SyntaxKind.HlslIncludeKeyword;
                case "ENDCG":
                    return SyntaxKind.EndCgKeyword;
                case "ENDHLSL":
                    return SyntaxKind.EndHlslKeyword;
            }

            switch (text.ToLower())
            {
                case "shader":
                    return SyntaxKind.ShaderKeyword;
                case "properties":
                    return SyntaxKind.PropertiesKeyword;
                case "range":
                    return SyntaxKind.RangeKeyword;
                case "float":
                    return SyntaxKind.FloatKeyword;
                case "int":
                    return SyntaxKind.IntKeyword;
                case "color":
                    return SyntaxKind.ColorKeyword;
                case "vector":
                    return SyntaxKind.VectorKeyword;
                case "2d":
                    return SyntaxKind._2DKeyword;
                case "3d":
                    return SyntaxKind._3DKeyword;
                case "cube":
                    return SyntaxKind.CubeKeyword;
                case "any":
                    return SyntaxKind.AnyKeyword;
                case "subshader":
                    return SyntaxKind.SubShaderKeyword;
                case "category":
                    return SyntaxKind.CategoryKeyword;
                case "tags":
                    return SyntaxKind.TagsKeyword;
                case "pass":
                    return SyntaxKind.PassKeyword;
                case "fallback":
                    return SyntaxKind.FallbackKeyword;
                case "customeditor":
                    return SyntaxKind.CustomEditorKeyword;
                case "cull":
                    return SyntaxKind.CullKeyword;
                case "zwrite":
                    return SyntaxKind.ZWriteKeyword;
                case "ztest":
                    return SyntaxKind.ZTestKeyword;
                case "offset":
                    return SyntaxKind.OffsetKeyword;
                case "blend":
                    return SyntaxKind.BlendKeyword;
                case "blendop":
                    return SyntaxKind.BlendOpKeyword;
                case "colormask":
                    return SyntaxKind.ColorMaskKeyword;
                case "alphatomask":
                    return SyntaxKind.AlphaToMaskKeyword;
                case "lod":
                    return SyntaxKind.LodKeyword;
                case "name":
                    return SyntaxKind.NameKeyword;
                case "lighting":
                    return SyntaxKind.LightingKeyword;
                case "stencil":
                    return SyntaxKind.StencilKeyword;
                case "ref":
                    return SyntaxKind.RefKeyword;
                case "readmask":
                    return SyntaxKind.ReadMaskKeyword;
                case "writemask":
                    return SyntaxKind.WriteMaskKeyword;
                case "comp":
                    return SyntaxKind.CompKeyword;
                case "compback":
                    return SyntaxKind.CompBackKeyword;
                case "compfront":
                    return SyntaxKind.CompFrontKeyword;
                case "fail":
                    return SyntaxKind.FailKeyword;
                case "zfail":
                    return SyntaxKind.ZFailKeyword;
                case "usepass":
                    return SyntaxKind.UsePassKeyword;
                case "grabpass":
                    return SyntaxKind.GrabPassKeyword;
                case "dependency":
                    return SyntaxKind.DependencyKeyword;
                case "material":
                    return SyntaxKind.MaterialKeyword;
                case "diffuse":
                    return SyntaxKind.DiffuseKeyword;
                case "ambient":
                    return SyntaxKind.AmbientKeyword;
                case "shininess":
                    return SyntaxKind.ShininessKeyword;
                case "specular":
                    return SyntaxKind.SpecularKeyword;
                case "emission":
                    return SyntaxKind.EmissionKeyword;
                case "fog":
                    return SyntaxKind.FogKeyword;
                case "mode":
                    return SyntaxKind.ModeKeyword;
                case "density":
                    return SyntaxKind.DensityKeyword;
                case "separatespecular":
                    return SyntaxKind.SeparateSpecularKeyword;
                case "settexture":
                    return SyntaxKind.SetTextureKeyword;
                case "combine":
                    return SyntaxKind.CombineKeyword;
                case "alpha":
                    return SyntaxKind.AlphaKeyword;
                case "lerp":
                    return SyntaxKind.LerpKeyword;
                case "double":
                    return SyntaxKind.DoubleKeyword;
                case "quad":
                    return SyntaxKind.QuadKeyword;
                case "constantcolor":
                    return SyntaxKind.ConstantColorKeyword;
                case "matrix":
                    return SyntaxKind.MatrixKeyword;
                case "alphatest":
                    return SyntaxKind.AlphaTestKeyword;
                case "colormaterial":
                    return SyntaxKind.ColorMaterialKeyword;
                case "bindchannels":
                    return SyntaxKind.BindChannelsKeyword;
                case "bind":
                    return SyntaxKind.BindKeyword;

                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static SyntaxKind GetLiteralExpression(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.FloatLiteralToken:
                case SyntaxKind.IntegerLiteralToken:
                    return SyntaxKind.NumericLiteralExpression;
                case SyntaxKind.TrueKeyword:
                    return SyntaxKind.TrueLiteralExpression;
                case SyntaxKind.FalseKeyword:
                    return SyntaxKind.FalseLiteralExpression;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static SyntaxKind GetPrefixUnaryExpression(SyntaxKind kind, bool forPreprocessor = false)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return SyntaxKind.UnaryPlusExpression;
                case SyntaxKind.MinusToken:
                    return SyntaxKind.UnaryMinusExpression;
                case SyntaxKind.NotToken:
                    return SyntaxKind.LogicalNotExpression;
                case SyntaxKind.TildeToken:
                    if (forPreprocessor)
                        goto default;
                    return SyntaxKind.BitwiseNotExpression;
                case SyntaxKind.MinusMinusToken:
                    if (forPreprocessor)
                        goto default;
                    return SyntaxKind.PreDecrementExpression;
                case SyntaxKind.PlusPlusToken:
                    if (forPreprocessor)
                        goto default;
                    return SyntaxKind.PreIncrementExpression;
                default:
                    return SyntaxKind.None;
            }
        }

        public static bool IsComment(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.BlockCommentEndOfFile:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsKeyword(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.ShaderKeyword:
                case SyntaxKind.PropertiesKeyword:
                case SyntaxKind.RangeKeyword:
                case SyntaxKind.FloatKeyword:
                case SyntaxKind.IntKeyword:
                case SyntaxKind.ColorKeyword:
                case SyntaxKind.VectorKeyword:
                case SyntaxKind._2DKeyword:
                case SyntaxKind._3DKeyword:
                case SyntaxKind.CubeKeyword:
                case SyntaxKind.AnyKeyword:
                case SyntaxKind.CategoryKeyword:
                case SyntaxKind.SubShaderKeyword:
                case SyntaxKind.TagsKeyword:
                case SyntaxKind.PassKeyword:
                case SyntaxKind.CgProgramKeyword:
                case SyntaxKind.CgIncludeKeyword:
                case SyntaxKind.EndCgKeyword:
                case SyntaxKind.FallbackKeyword:
                case SyntaxKind.CustomEditorKeyword:
                case SyntaxKind.CullKeyword:
                case SyntaxKind.ZWriteKeyword:
                case SyntaxKind.ZTestKeyword:
                case SyntaxKind.OffsetKeyword:
                case SyntaxKind.BlendKeyword:
                case SyntaxKind.BlendOpKeyword:
                case SyntaxKind.ColorMaskKeyword:
                case SyntaxKind.AlphaToMaskKeyword:
                case SyntaxKind.LodKeyword:
                case SyntaxKind.NameKeyword:
                case SyntaxKind.LightingKeyword:
                case SyntaxKind.StencilKeyword:
                case SyntaxKind.RefKeyword:
                case SyntaxKind.ReadMaskKeyword:
                case SyntaxKind.WriteMaskKeyword:
                case SyntaxKind.CompKeyword:
                case SyntaxKind.CompBackKeyword:
                case SyntaxKind.CompFrontKeyword:
                case SyntaxKind.FailKeyword:
                case SyntaxKind.ZFailKeyword:
                case SyntaxKind.UsePassKeyword:
                case SyntaxKind.GrabPassKeyword:
                case SyntaxKind.DependencyKeyword:
                case SyntaxKind.MaterialKeyword:
                case SyntaxKind.DiffuseKeyword:
                case SyntaxKind.AmbientKeyword:
                case SyntaxKind.ShininessKeyword:
                case SyntaxKind.SpecularKeyword:
                case SyntaxKind.EmissionKeyword:
                case SyntaxKind.FogKeyword:
                case SyntaxKind.ModeKeyword:
                case SyntaxKind.DensityKeyword:
                case SyntaxKind.SeparateSpecularKeyword:
                case SyntaxKind.SetTextureKeyword:
                case SyntaxKind.CombineKeyword:
                case SyntaxKind.AlphaKeyword:
                case SyntaxKind.LerpKeyword:
                case SyntaxKind.DoubleKeyword:
                case SyntaxKind.QuadKeyword:
                case SyntaxKind.ConstantColorKeyword:
                case SyntaxKind.MatrixKeyword:
                case SyntaxKind.AlphaTestKeyword:
                case SyntaxKind.ColorMaterialKeyword:
                case SyntaxKind.BindChannelsKeyword:
                case SyntaxKind.BindKeyword:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsPunctuation(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.OpenParenToken:
                case SyntaxKind.CloseParenToken:
                case SyntaxKind.OpenBracketToken:
                case SyntaxKind.CloseBracketToken:
                case SyntaxKind.OpenBraceToken:
                case SyntaxKind.CloseBraceToken:
                case SyntaxKind.SemiToken:
                case SyntaxKind.CommaToken:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsOperator(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.LessThanToken:
                case SyntaxKind.LessThanEqualsToken:
                case SyntaxKind.GreaterThanToken:
                case SyntaxKind.GreaterThanEqualsToken:
                case SyntaxKind.LessThanLessThanToken:
                case SyntaxKind.GreaterThanGreaterThanToken:
                case SyntaxKind.PlusToken:
                case SyntaxKind.PlusPlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.MinusMinusToken:
                case SyntaxKind.AsteriskToken:
                case SyntaxKind.SlashToken:
                case SyntaxKind.PercentToken:
                case SyntaxKind.AmpersandToken:
                case SyntaxKind.BarToken:
                case SyntaxKind.AmpersandAmpersandToken:
                case SyntaxKind.BarBarToken:
                case SyntaxKind.CaretToken:
                case SyntaxKind.NotToken:
                case SyntaxKind.TildeToken:
                case SyntaxKind.QuestionToken:
                case SyntaxKind.ColonToken:
                case SyntaxKind.ColonColonToken:
                case SyntaxKind.EqualsToken:
                case SyntaxKind.AsteriskEqualsToken:
                case SyntaxKind.SlashEqualsToken:
                case SyntaxKind.PercentEqualsToken:
                case SyntaxKind.PlusEqualsToken:
                case SyntaxKind.MinusEqualsToken:
                case SyntaxKind.LessThanLessThanEqualsToken:
                case SyntaxKind.GreaterThanGreaterThanEqualsToken:
                case SyntaxKind.AmpersandEqualsToken:
                case SyntaxKind.CaretEqualsToken:
                case SyntaxKind.BarEqualsToken:
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.ExclamationEqualsToken:
                case SyntaxKind.DotToken:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsWhitespace(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.WhitespaceTrivia:
                case SyntaxKind.EndOfLineTrivia:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsNumericLiteral(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.IntegerLiteralToken:
                case SyntaxKind.FloatLiteralToken:
                    return true;

                default:
                    return false;
            }
        }
    }
}