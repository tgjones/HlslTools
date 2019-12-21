using System;
using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
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

                case SyntaxKind.CharacterLiteralToken:
                    return "<character-literal>";

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
                case SyntaxKind.HashToken:
                    return "#";
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
                case SyntaxKind.BoolKeyword:
                    return "bool";
                case SyntaxKind.BreakKeyword:
                    return "break";
                case SyntaxKind.CaseKeyword:
                    return "case";
                case SyntaxKind.ClassKeyword:
                    return "class";
                case SyntaxKind.ConstantBufferKeyword:
                    return "ConstantBuffer";
                case SyntaxKind.ConstKeyword:
                    return "const";
                case SyntaxKind.ContinueKeyword:
                    return "continue";
                case SyntaxKind.DefKeyword:
                    return "def";
                case SyntaxKind.DefineKeyword:
                    return "define";
                case SyntaxKind.DoKeyword:
                    return "do";
                case SyntaxKind.DefaultKeyword:
                    return "default";
                case SyntaxKind.DoubleKeyword:
                    return "double";
                case SyntaxKind.ElifKeyword:
                    return "elif";
                case SyntaxKind.ElseKeyword:
                    return "else";
                case SyntaxKind.EndIfKeyword:
                    return "endif";
                case SyntaxKind.ErrorKeyword:
                    return "error";
                case SyntaxKind.ExportKeyword:
                    return "export";
                case SyntaxKind.ExternKeyword:
                    return "extern";
                case SyntaxKind.FalseKeyword:
                    return "false";
                case SyntaxKind.FloatKeyword:
                    return "float";
                case SyntaxKind.ForKeyword:
                    return "for";
                case SyntaxKind.IfKeyword:
                    return "if";
                case SyntaxKind.IncludeKeyword:
                    return "include";
                case SyntaxKind.InKeyword:
                    return "in";
                case SyntaxKind.InlineKeyword:
                    return "inline";
                case SyntaxKind.InoutKeyword:
                    return "inout";
                case SyntaxKind.InterfaceKeyword:
                    return "interface";
                case SyntaxKind.IntKeyword:
                    return "int";
                case SyntaxKind.LineKeyword:
                    return "line";
                case SyntaxKind.MessageKeyword:
                    return "message";
                case SyntaxKind.Min10FloatKeyword:
                    return "min10float";
                case SyntaxKind.Min12IntKeyword:
                    return "min12int";
                case SyntaxKind.Min16FloatKeyword:
                    return "min16float";
                case SyntaxKind.Min16IntKeyword:
                    return "min16int";
                case SyntaxKind.Min16UintKeyword:
                    return "min16uint";
                case SyntaxKind.OutKeyword:
                    return "out";
                case SyntaxKind.PackMatrixKeyword:
                    return "pack_matrix";
                case SyntaxKind.PackoffsetKeyword:
                    return "packoffset";
                case SyntaxKind.PragmaKeyword:
                    return "pragma";
                case SyntaxKind.RegisterKeyword:
                    return "register";
                case SyntaxKind.ReturnKeyword:
                    return "return";
                case SyntaxKind.SNormKeyword:
                    return "snorm";
                case SyntaxKind.StaticKeyword:
                    return "static";
                case SyntaxKind.StringKeyword:
                    return "string";
                case SyntaxKind.StructKeyword:
                    return "struct";
                case SyntaxKind.SwitchKeyword:
                    return "switch";
                case SyntaxKind.TrueKeyword:
                    return "true";
                case SyntaxKind.UintKeyword:
                    return "uint";
                case SyntaxKind.UndefKeyword:
                    return "undef";
                case SyntaxKind.UNormKeyword:
                    return "unorm";
                case SyntaxKind.VoidKeyword:
                    return "void";
                case SyntaxKind.VolatileKeyword:
                    return "volatile";
                case SyntaxKind.WarningKeyword:
                    return "warning";
                case SyntaxKind.WhileKeyword:
                    return "while";

                default:
                    return string.Empty;
            }
        }

        public static bool IsIdentifierOrKeyword(this SyntaxKind kind)
        {
            return kind == SyntaxKind.IdentifierToken || kind.IsKeyword();
        }

        public static bool IsRightAssociative(SyntaxKind op)
        {
            switch (op)
            {
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAssignmentExpression(SyntaxKind token)
        {
            return GetAssignmentExpression(token) != SyntaxKind.None;
        }

        public static SyntaxKind GetAssignmentExpression(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.EqualsToken:
                    return SyntaxKind.SimpleAssignmentExpression;
                case SyntaxKind.AsteriskEqualsToken:
                    return SyntaxKind.MultiplyAssignmentExpression;
                case SyntaxKind.SlashEqualsToken:
                    return SyntaxKind.DivideAssignmentExpression;
                case SyntaxKind.PercentEqualsToken:
                    return SyntaxKind.ModuloAssignmentExpression;
                case SyntaxKind.PlusEqualsToken:
                    return SyntaxKind.AddAssignmentExpression;
                case SyntaxKind.MinusEqualsToken:
                    return SyntaxKind.SubtractAssignmentExpression;
                case SyntaxKind.LessThanLessThanEqualsToken:
                    return SyntaxKind.LeftShiftAssignmentExpression;
                case SyntaxKind.GreaterThanGreaterThanEqualsToken:
                    return SyntaxKind.RightShiftAssignmentExpression;
                case SyntaxKind.AmpersandEqualsToken:
                    return SyntaxKind.AndAssignmentExpression;
                case SyntaxKind.CaretEqualsToken:
                    return SyntaxKind.ExclusiveOrAssignmentExpression;
                case SyntaxKind.BarEqualsToken:
                    return SyntaxKind.OrAssignmentExpression;
                default:
                    return SyntaxKind.None;
            }
        }

        public static bool IsBinaryExpression(SyntaxKind token)
        {
            return GetBinaryExpression(token) != SyntaxKind.None;
        }

        public static SyntaxKind GetBinaryExpression(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.AsteriskToken:
                    return SyntaxKind.MultiplyExpression;
                case SyntaxKind.SlashToken:
                    return SyntaxKind.DivideExpression;
                case SyntaxKind.PercentToken:
                    return SyntaxKind.ModuloExpression;
                case SyntaxKind.PlusToken:
                    return SyntaxKind.AddExpression;
                case SyntaxKind.MinusToken:
                    return SyntaxKind.SubtractExpression;
                case SyntaxKind.LessThanLessThanToken:
                    return SyntaxKind.LeftShiftExpression;
                case SyntaxKind.GreaterThanGreaterThanToken:
                    return SyntaxKind.RightShiftExpression;
                case SyntaxKind.LessThanToken:
                    return SyntaxKind.LessThanExpression;
                case SyntaxKind.GreaterThanToken:
                    return SyntaxKind.GreaterThanExpression;
                case SyntaxKind.LessThanEqualsToken:
                    return SyntaxKind.LessThanOrEqualExpression;
                case SyntaxKind.GreaterThanEqualsToken:
                    return SyntaxKind.GreaterThanOrEqualExpression;
                case SyntaxKind.EqualsEqualsToken:
                    return SyntaxKind.EqualsExpression;
                case SyntaxKind.ExclamationEqualsToken:
                    return SyntaxKind.NotEqualsExpression;
                case SyntaxKind.AmpersandToken:
                    return SyntaxKind.BitwiseAndExpression;
                case SyntaxKind.CaretToken:
                    return SyntaxKind.ExclusiveOrExpression;
                case SyntaxKind.BarToken:
                    return SyntaxKind.BitwiseOrExpression;
                case SyntaxKind.AmpersandAmpersandToken:
                    return SyntaxKind.LogicalAndExpression;
                case SyntaxKind.BarBarToken:
                    return SyntaxKind.LogicalOrExpression;
                default:
                    return SyntaxKind.None;
            }
        }

        public static bool IsPredefinedScalarType(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.BoolKeyword:
                case SyntaxKind.IntKeyword:
                case SyntaxKind.UnsignedKeyword: // Needs to be followed by IntKeyword
                case SyntaxKind.DwordKeyword:
                case SyntaxKind.UintKeyword:
                case SyntaxKind.HalfKeyword:
                case SyntaxKind.FloatKeyword:
                case SyntaxKind.DoubleKeyword:
                case SyntaxKind.Min16FloatKeyword:
                case SyntaxKind.Min10FloatKeyword:
                case SyntaxKind.Min16IntKeyword:
                case SyntaxKind.Min12IntKeyword:
                case SyntaxKind.Min16UintKeyword:
                case SyntaxKind.VoidKeyword:
                case SyntaxKind.StringKeyword:
                case SyntaxKind.SNormKeyword:
                case SyntaxKind.UNormKeyword:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsPredefinedVectorType(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.VectorKeyword:
                case SyntaxKind.Bool1Keyword:
                case SyntaxKind.Bool2Keyword:
                case SyntaxKind.Bool3Keyword:
                case SyntaxKind.Bool4Keyword:
                case SyntaxKind.Half1Keyword:
                case SyntaxKind.Half2Keyword:
                case SyntaxKind.Half3Keyword:
                case SyntaxKind.Half4Keyword:
                case SyntaxKind.Int1Keyword:
                case SyntaxKind.Int2Keyword:
                case SyntaxKind.Int3Keyword:
                case SyntaxKind.Int4Keyword:
                case SyntaxKind.Uint1Keyword:
                case SyntaxKind.Uint2Keyword:
                case SyntaxKind.Uint3Keyword:
                case SyntaxKind.Uint4Keyword:
                case SyntaxKind.Float1Keyword:
                case SyntaxKind.Float2Keyword:
                case SyntaxKind.Float3Keyword:
                case SyntaxKind.Float4Keyword:
                case SyntaxKind.Double1Keyword:
                case SyntaxKind.Double2Keyword:
                case SyntaxKind.Double3Keyword:
                case SyntaxKind.Double4Keyword:
                case SyntaxKind.Min16Float1Keyword:
                case SyntaxKind.Min16Float2Keyword:
                case SyntaxKind.Min16Float3Keyword:
                case SyntaxKind.Min16Float4Keyword:
                case SyntaxKind.Min10Float1Keyword:
                case SyntaxKind.Min10Float2Keyword:
                case SyntaxKind.Min10Float3Keyword:
                case SyntaxKind.Min10Float4Keyword:
                case SyntaxKind.Min16Int1Keyword:
                case SyntaxKind.Min16Int2Keyword:
                case SyntaxKind.Min16Int3Keyword:
                case SyntaxKind.Min16Int4Keyword:
                case SyntaxKind.Min12Int1Keyword:
                case SyntaxKind.Min12Int2Keyword:
                case SyntaxKind.Min12Int3Keyword:
                case SyntaxKind.Min12Int4Keyword:
                case SyntaxKind.Min16Uint1Keyword:
                case SyntaxKind.Min16Uint2Keyword:
                case SyntaxKind.Min16Uint3Keyword:
                case SyntaxKind.Min16Uint4Keyword:
                case SyntaxKind.SNormKeyword:
                case SyntaxKind.UNormKeyword:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsPredefinedMatrixType(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.MatrixKeyword:
                case SyntaxKind.Bool1x1Keyword:
                case SyntaxKind.Bool1x2Keyword:
                case SyntaxKind.Bool1x3Keyword:
                case SyntaxKind.Bool1x4Keyword:
                case SyntaxKind.Bool2x1Keyword:
                case SyntaxKind.Bool2x2Keyword:
                case SyntaxKind.Bool2x3Keyword:
                case SyntaxKind.Bool2x4Keyword:
                case SyntaxKind.Bool3x1Keyword:
                case SyntaxKind.Bool3x2Keyword:
                case SyntaxKind.Bool3x3Keyword:
                case SyntaxKind.Bool3x4Keyword:
                case SyntaxKind.Bool4x1Keyword:
                case SyntaxKind.Bool4x2Keyword:
                case SyntaxKind.Bool4x3Keyword:
                case SyntaxKind.Bool4x4Keyword:
                case SyntaxKind.Double1x1Keyword:
                case SyntaxKind.Double1x2Keyword:
                case SyntaxKind.Double1x3Keyword:
                case SyntaxKind.Double1x4Keyword:
                case SyntaxKind.Double2x1Keyword:
                case SyntaxKind.Double2x2Keyword:
                case SyntaxKind.Double2x3Keyword:
                case SyntaxKind.Double2x4Keyword:
                case SyntaxKind.Double3x1Keyword:
                case SyntaxKind.Double3x2Keyword:
                case SyntaxKind.Double3x3Keyword:
                case SyntaxKind.Double3x4Keyword:
                case SyntaxKind.Double4x1Keyword:
                case SyntaxKind.Double4x2Keyword:
                case SyntaxKind.Double4x3Keyword:
                case SyntaxKind.Double4x4Keyword:
                case SyntaxKind.Float1x1Keyword:
                case SyntaxKind.Float1x2Keyword:
                case SyntaxKind.Float1x3Keyword:
                case SyntaxKind.Float1x4Keyword:
                case SyntaxKind.Float2x1Keyword:
                case SyntaxKind.Float2x2Keyword:
                case SyntaxKind.Float2x3Keyword:
                case SyntaxKind.Float2x4Keyword:
                case SyntaxKind.Float3x1Keyword:
                case SyntaxKind.Float3x2Keyword:
                case SyntaxKind.Float3x3Keyword:
                case SyntaxKind.Float3x4Keyword:
                case SyntaxKind.Float4x1Keyword:
                case SyntaxKind.Float4x2Keyword:
                case SyntaxKind.Float4x3Keyword:
                case SyntaxKind.Float4x4Keyword:
                case SyntaxKind.Half1x1Keyword:
                case SyntaxKind.Half1x2Keyword:
                case SyntaxKind.Half1x3Keyword:
                case SyntaxKind.Half1x4Keyword:
                case SyntaxKind.Half2x1Keyword:
                case SyntaxKind.Half2x2Keyword:
                case SyntaxKind.Half2x3Keyword:
                case SyntaxKind.Half2x4Keyword:
                case SyntaxKind.Half3x1Keyword:
                case SyntaxKind.Half3x2Keyword:
                case SyntaxKind.Half3x3Keyword:
                case SyntaxKind.Half3x4Keyword:
                case SyntaxKind.Half4x1Keyword:
                case SyntaxKind.Half4x2Keyword:
                case SyntaxKind.Half4x3Keyword:
                case SyntaxKind.Half4x4Keyword:
                case SyntaxKind.Int1x1Keyword:
                case SyntaxKind.Int1x2Keyword:
                case SyntaxKind.Int1x3Keyword:
                case SyntaxKind.Int1x4Keyword:
                case SyntaxKind.Int2x1Keyword:
                case SyntaxKind.Int2x2Keyword:
                case SyntaxKind.Int2x3Keyword:
                case SyntaxKind.Int2x4Keyword:
                case SyntaxKind.Int3x1Keyword:
                case SyntaxKind.Int3x2Keyword:
                case SyntaxKind.Int3x3Keyword:
                case SyntaxKind.Int3x4Keyword:
                case SyntaxKind.Int4x1Keyword:
                case SyntaxKind.Int4x2Keyword:
                case SyntaxKind.Int4x3Keyword:
                case SyntaxKind.Int4x4Keyword:
                case SyntaxKind.Min10Float1x1Keyword:
                case SyntaxKind.Min10Float1x2Keyword:
                case SyntaxKind.Min10Float1x3Keyword:
                case SyntaxKind.Min10Float1x4Keyword:
                case SyntaxKind.Min10Float2x1Keyword:
                case SyntaxKind.Min10Float2x2Keyword:
                case SyntaxKind.Min10Float2x3Keyword:
                case SyntaxKind.Min10Float2x4Keyword:
                case SyntaxKind.Min10Float3x1Keyword:
                case SyntaxKind.Min10Float3x2Keyword:
                case SyntaxKind.Min10Float3x3Keyword:
                case SyntaxKind.Min10Float3x4Keyword:
                case SyntaxKind.Min10Float4x1Keyword:
                case SyntaxKind.Min10Float4x2Keyword:
                case SyntaxKind.Min10Float4x3Keyword:
                case SyntaxKind.Min10Float4x4Keyword:
                case SyntaxKind.Min12Int1x1Keyword:
                case SyntaxKind.Min12Int1x2Keyword:
                case SyntaxKind.Min12Int1x3Keyword:
                case SyntaxKind.Min12Int1x4Keyword:
                case SyntaxKind.Min12Int2x1Keyword:
                case SyntaxKind.Min12Int2x2Keyword:
                case SyntaxKind.Min12Int2x3Keyword:
                case SyntaxKind.Min12Int2x4Keyword:
                case SyntaxKind.Min12Int3x1Keyword:
                case SyntaxKind.Min12Int3x2Keyword:
                case SyntaxKind.Min12Int3x3Keyword:
                case SyntaxKind.Min12Int3x4Keyword:
                case SyntaxKind.Min12Int4x1Keyword:
                case SyntaxKind.Min12Int4x2Keyword:
                case SyntaxKind.Min12Int4x3Keyword:
                case SyntaxKind.Min12Int4x4Keyword:
                case SyntaxKind.Min16Float1x1Keyword:
                case SyntaxKind.Min16Float1x2Keyword:
                case SyntaxKind.Min16Float1x3Keyword:
                case SyntaxKind.Min16Float1x4Keyword:
                case SyntaxKind.Min16Float2x1Keyword:
                case SyntaxKind.Min16Float2x2Keyword:
                case SyntaxKind.Min16Float2x3Keyword:
                case SyntaxKind.Min16Float2x4Keyword:
                case SyntaxKind.Min16Float3x1Keyword:
                case SyntaxKind.Min16Float3x2Keyword:
                case SyntaxKind.Min16Float3x3Keyword:
                case SyntaxKind.Min16Float3x4Keyword:
                case SyntaxKind.Min16Float4x1Keyword:
                case SyntaxKind.Min16Float4x2Keyword:
                case SyntaxKind.Min16Float4x3Keyword:
                case SyntaxKind.Min16Float4x4Keyword:
                case SyntaxKind.Min16Int1x1Keyword:
                case SyntaxKind.Min16Int1x2Keyword:
                case SyntaxKind.Min16Int1x3Keyword:
                case SyntaxKind.Min16Int1x4Keyword:
                case SyntaxKind.Min16Int2x1Keyword:
                case SyntaxKind.Min16Int2x2Keyword:
                case SyntaxKind.Min16Int2x3Keyword:
                case SyntaxKind.Min16Int2x4Keyword:
                case SyntaxKind.Min16Int3x1Keyword:
                case SyntaxKind.Min16Int3x2Keyword:
                case SyntaxKind.Min16Int3x3Keyword:
                case SyntaxKind.Min16Int3x4Keyword:
                case SyntaxKind.Min16Int4x1Keyword:
                case SyntaxKind.Min16Int4x2Keyword:
                case SyntaxKind.Min16Int4x3Keyword:
                case SyntaxKind.Min16Int4x4Keyword:
                case SyntaxKind.Min16Uint1x1Keyword:
                case SyntaxKind.Min16Uint1x2Keyword:
                case SyntaxKind.Min16Uint1x3Keyword:
                case SyntaxKind.Min16Uint1x4Keyword:
                case SyntaxKind.Min16Uint2x1Keyword:
                case SyntaxKind.Min16Uint2x2Keyword:
                case SyntaxKind.Min16Uint2x3Keyword:
                case SyntaxKind.Min16Uint2x4Keyword:
                case SyntaxKind.Min16Uint3x1Keyword:
                case SyntaxKind.Min16Uint3x2Keyword:
                case SyntaxKind.Min16Uint3x3Keyword:
                case SyntaxKind.Min16Uint3x4Keyword:
                case SyntaxKind.Min16Uint4x1Keyword:
                case SyntaxKind.Min16Uint4x2Keyword:
                case SyntaxKind.Min16Uint4x3Keyword:
                case SyntaxKind.Min16Uint4x4Keyword:
                case SyntaxKind.Uint1x1Keyword:
                case SyntaxKind.Uint1x2Keyword:
                case SyntaxKind.Uint1x3Keyword:
                case SyntaxKind.Uint1x4Keyword:
                case SyntaxKind.Uint2x1Keyword:
                case SyntaxKind.Uint2x2Keyword:
                case SyntaxKind.Uint2x3Keyword:
                case SyntaxKind.Uint2x4Keyword:
                case SyntaxKind.Uint3x1Keyword:
                case SyntaxKind.Uint3x2Keyword:
                case SyntaxKind.Uint3x3Keyword:
                case SyntaxKind.Uint3x4Keyword:
                case SyntaxKind.Uint4x1Keyword:
                case SyntaxKind.Uint4x2Keyword:
                case SyntaxKind.Uint4x3Keyword:
                case SyntaxKind.Uint4x4Keyword:
                case SyntaxKind.SNormKeyword:
                case SyntaxKind.UNormKeyword:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsPredefinedObjectType(SyntaxToken token)
        {
            switch (token.Kind)
            {
                case SyntaxKind.AppendStructuredBufferKeyword:
                case SyntaxKind.BlendStateKeyword:
                case SyntaxKind.BufferKeyword:
                case SyntaxKind.ByteAddressBufferKeyword:
                case SyntaxKind.ConsumeStructuredBufferKeyword:
                case SyntaxKind.DepthStencilStateKeyword:
                case SyntaxKind.InputPatchKeyword:
                case SyntaxKind.LineStreamKeyword:
                case SyntaxKind.OutputPatchKeyword:
                case SyntaxKind.PointStreamKeyword:
                case SyntaxKind.RasterizerOrderedBufferKeyword:
                case SyntaxKind.RasterizerOrderedByteAddressBufferKeyword:
                case SyntaxKind.RasterizerOrderedStructuredBufferKeyword:
                case SyntaxKind.RasterizerOrderedTexture1DKeyword:
                case SyntaxKind.RasterizerOrderedTexture1DArrayKeyword:
                case SyntaxKind.RasterizerOrderedTexture2DKeyword:
                case SyntaxKind.RasterizerOrderedTexture2DArrayKeyword:
                case SyntaxKind.RasterizerOrderedTexture3DKeyword:
                case SyntaxKind.RasterizerStateKeyword:
                case SyntaxKind.RWBufferKeyword:
                case SyntaxKind.RWByteAddressBufferKeyword:
                case SyntaxKind.RWStructuredBufferKeyword:
                case SyntaxKind.RWTexture1DKeyword:
                case SyntaxKind.RWTexture1DArrayKeyword:
                case SyntaxKind.RWTexture2DKeyword:
                case SyntaxKind.RWTexture2DArrayKeyword:
                case SyntaxKind.RWTexture3DKeyword:
                case SyntaxKind.SamplerKeyword:
                case SyntaxKind.Sampler1DKeyword:
                case SyntaxKind.Sampler2DKeyword:
                case SyntaxKind.Sampler3DKeyword:
                case SyntaxKind.SamplerCubeKeyword:
                case SyntaxKind.SamplerStateKeyword:
                case SyntaxKind.SamplerComparisonStateKeyword:
                case SyntaxKind.StructuredBufferKeyword:
                case SyntaxKind.Texture2DLegacyKeyword:
                case SyntaxKind.TextureCubeLegacyKeyword:
                case SyntaxKind.Texture1DKeyword:
                case SyntaxKind.Texture1DArrayKeyword:
                case SyntaxKind.Texture2DKeyword:
                case SyntaxKind.Texture2DArrayKeyword:
                case SyntaxKind.Texture2DMSKeyword:
                case SyntaxKind.Texture2DMSArrayKeyword:
                case SyntaxKind.Texture3DKeyword:
                case SyntaxKind.TextureCubeKeyword:
                case SyntaxKind.TextureCubeArrayKeyword:
                case SyntaxKind.TriangleStreamKeyword:
                    return true;

                default:
                    switch (token.ContextualKind)
                    {
                        case SyntaxKind.ConstantBufferKeyword:
                        case SyntaxKind.TextureKeyword:
                        case SyntaxKind.GeometryShaderKeyword:
                        case SyntaxKind.PixelShaderKeyword:
                        case SyntaxKind.VertexShaderKeyword:
                            return true;

                        default:
                            return false;
                    }
            }
        }

        public static bool IsPredefinedType(SyntaxToken token)
        {
            if (IsPredefinedScalarType(token.Kind))
                return true;

            if (IsPredefinedVectorType(token.Kind))
                return true;

            if (IsPredefinedMatrixType(token.Kind))
                return true;

            if (IsPredefinedObjectType(token))
                return true;

            return false;
        }

        public static PredefinedObjectType GetPredefinedObjectType(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.AppendStructuredBufferKeyword:
                    return PredefinedObjectType.AppendStructuredBuffer;
                case SyntaxKind.BlendStateKeyword:
                    return PredefinedObjectType.BlendState;
                case SyntaxKind.BufferKeyword:
                    return PredefinedObjectType.Buffer;
                case SyntaxKind.ByteAddressBufferKeyword:
                    return PredefinedObjectType.ByteAddressBuffer;
                case SyntaxKind.ConsumeStructuredBufferKeyword:
                    return PredefinedObjectType.ConsumeStructuredBuffer;
                case SyntaxKind.DepthStencilStateKeyword:
                    return PredefinedObjectType.DepthStencilState;
                case SyntaxKind.InputPatchKeyword:
                    return PredefinedObjectType.InputPatch;
                case SyntaxKind.LineStreamKeyword:
                    return PredefinedObjectType.LineStream;
                case SyntaxKind.OutputPatchKeyword:
                    return PredefinedObjectType.OutputPatch;
                case SyntaxKind.PointStreamKeyword:
                    return PredefinedObjectType.PointStream;
                case SyntaxKind.RasterizerStateKeyword:
                    return PredefinedObjectType.RasterizerState;
                case SyntaxKind.RWBufferKeyword:
                    return PredefinedObjectType.RWBuffer;
                case SyntaxKind.RWByteAddressBufferKeyword:
                    return PredefinedObjectType.RWByteAddressBuffer;
                case SyntaxKind.RWStructuredBufferKeyword:
                    return PredefinedObjectType.RWStructuredBuffer;
                case SyntaxKind.RWTexture1DKeyword:
                    return PredefinedObjectType.RWTexture1D;
                case SyntaxKind.RWTexture1DArrayKeyword:
                    return PredefinedObjectType.RWTexture1DArray;
                case SyntaxKind.RWTexture2DKeyword:
                    return PredefinedObjectType.RWTexture2D;
                case SyntaxKind.RWTexture2DArrayKeyword:
                    return PredefinedObjectType.RWTexture2DArray;
                case SyntaxKind.RWTexture3DKeyword:
                    return PredefinedObjectType.RWTexture3D;
                case SyntaxKind.Sampler1DKeyword:
                    return PredefinedObjectType.Sampler1D;
                case SyntaxKind.SamplerKeyword:
                    return PredefinedObjectType.Sampler;
                case SyntaxKind.Sampler2DKeyword:
                    return PredefinedObjectType.Sampler2D;
                case SyntaxKind.Sampler3DKeyword:
                    return PredefinedObjectType.Sampler3D;
                case SyntaxKind.SamplerCubeKeyword:
                    return PredefinedObjectType.SamplerCube;
                case SyntaxKind.SamplerStateKeyword:
                    return PredefinedObjectType.SamplerState;
                case SyntaxKind.SamplerComparisonStateKeyword:
                    return PredefinedObjectType.SamplerComparisonState;
                case SyntaxKind.StructuredBufferKeyword:
                    return PredefinedObjectType.StructuredBuffer;
                case SyntaxKind.TextureKeyword:
                case SyntaxKind.Texture2DLegacyKeyword:
                case SyntaxKind.TextureCubeLegacyKeyword:
                    return PredefinedObjectType.Texture;
                case SyntaxKind.Texture1DKeyword:
                    return PredefinedObjectType.Texture1D;
                case SyntaxKind.Texture1DArrayKeyword:
                    return PredefinedObjectType.Texture1DArray;
                case SyntaxKind.Texture2DKeyword:
                    return PredefinedObjectType.Texture2D;
                case SyntaxKind.Texture2DArrayKeyword:
                    return PredefinedObjectType.Texture2DArray;
                case SyntaxKind.Texture2DMSKeyword:
                    return PredefinedObjectType.Texture2DMS;
                case SyntaxKind.Texture2DMSArrayKeyword:
                    return PredefinedObjectType.Texture2DMSArray;
                case SyntaxKind.Texture3DKeyword:
                    return PredefinedObjectType.Texture3D;
                case SyntaxKind.TextureCubeKeyword:
                    return PredefinedObjectType.TextureCube;
                case SyntaxKind.TextureCubeArrayKeyword:
                    return PredefinedObjectType.TextureCubeArray;
                case SyntaxKind.TriangleStreamKeyword:
                    return PredefinedObjectType.TriangleStream;
                case SyntaxKind.RasterizerOrderedBufferKeyword:
                    return PredefinedObjectType.RasterizerOrderedBuffer;
                case SyntaxKind.RasterizerOrderedByteAddressBufferKeyword:
                    return PredefinedObjectType.RasterizerOrderedByteAddressBuffer;
                case SyntaxKind.RasterizerOrderedStructuredBufferKeyword:
                    return PredefinedObjectType.RasterizerOrderedStructuredBuffer;
                case SyntaxKind.RasterizerOrderedTexture1DArrayKeyword:
                    return PredefinedObjectType.RasterizerOrderedTexture1DArray;
                case SyntaxKind.RasterizerOrderedTexture1DKeyword:
                    return PredefinedObjectType.RasterizerOrderedTexture1D;
                case SyntaxKind.RasterizerOrderedTexture2DArrayKeyword:
                    return PredefinedObjectType.RasterizerOrderedTexture2DArray;
                case SyntaxKind.RasterizerOrderedTexture2DKeyword:
                    return PredefinedObjectType.RasterizerOrderedTexture2D;
                case SyntaxKind.RasterizerOrderedTexture3DKeyword:
                    return PredefinedObjectType.RasterizerOrderedTexture3D;
                case SyntaxKind.GeometryShaderKeyword:
                    return PredefinedObjectType.GeometryShader;
                case SyntaxKind.PixelShaderKeyword:
                    return PredefinedObjectType.PixelShader;
                case SyntaxKind.VertexShaderKeyword:
                    return PredefinedObjectType.VertexShader;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind.ToString());
            }
        }

        public static bool CanFollowCast(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.SemiToken:
                case SyntaxKind.CloseParenToken:
                case SyntaxKind.CloseBracketToken:
                case SyntaxKind.OpenBraceToken:
                case SyntaxKind.CloseBraceToken:
                case SyntaxKind.CommaToken:
                case SyntaxKind.EqualsToken:
                case SyntaxKind.PlusEqualsToken:
                case SyntaxKind.MinusEqualsToken:
                case SyntaxKind.AsteriskEqualsToken:
                case SyntaxKind.SlashEqualsToken:
                case SyntaxKind.PercentEqualsToken:
                case SyntaxKind.AmpersandEqualsToken:
                case SyntaxKind.CaretEqualsToken:
                case SyntaxKind.BarEqualsToken:
                case SyntaxKind.LessThanLessThanEqualsToken:
                case SyntaxKind.GreaterThanGreaterThanEqualsToken:
                case SyntaxKind.QuestionToken:
                case SyntaxKind.ColonToken:
                case SyntaxKind.BarBarToken:
                case SyntaxKind.AmpersandAmpersandToken:
                case SyntaxKind.BarToken:
                case SyntaxKind.CaretToken:
                case SyntaxKind.AmpersandToken:
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.ExclamationEqualsToken:
                case SyntaxKind.LessThanToken:
                case SyntaxKind.LessThanEqualsToken:
                case SyntaxKind.GreaterThanToken:
                case SyntaxKind.GreaterThanEqualsToken:
                case SyntaxKind.LessThanLessThanToken:
                case SyntaxKind.GreaterThanGreaterThanToken:
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.AsteriskToken:
                case SyntaxKind.SlashToken:
                case SyntaxKind.PercentToken:
                case SyntaxKind.PlusPlusToken:
                case SyntaxKind.MinusMinusToken:
                case SyntaxKind.OpenBracketToken:
                case SyntaxKind.DotToken:
                case SyntaxKind.EndOfFileToken:
                    return false;
                default:
                    return true;
            }
        }

        public static bool IsAnyUnaryExpression(SyntaxKind token)
        {
            return IsPrefixUnaryExpression(token) || IsPostfixUnaryExpression(token);
        }

        public static bool IsPrefixUnaryExpression(SyntaxKind token, bool forPreprocessor = false)
        {
            return GetPrefixUnaryExpression(token) != SyntaxKind.None;
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

        public static bool IsPostfixUnaryExpression(SyntaxKind token)
        {
            return GetPostfixUnaryExpression(token) != SyntaxKind.None;
        }

        public static SyntaxKind GetPostfixUnaryExpression(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.MinusMinusToken:
                    return SyntaxKind.PostDecrementExpression;
                case SyntaxKind.PlusPlusToken:
                    return SyntaxKind.PostIncrementExpression;
                default:
                    return SyntaxKind.None;
            }
        }

        public static uint GetOperatorPrecedence(SyntaxKind op)
        {
            switch (op)
            {
                case SyntaxKind.CompoundExpression:
                    return 1;
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                case SyntaxKind.ConditionalExpression:
                    return 2;
                case SyntaxKind.LogicalOrExpression:
                    return 3;
                case SyntaxKind.LogicalAndExpression:
                    return 4;
                case SyntaxKind.BitwiseOrExpression:
                    return 5;
                case SyntaxKind.ExclusiveOrExpression:
                    return 6;
                case SyntaxKind.BitwiseAndExpression:
                    return 7;
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                    return 8;
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                    return 9;
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                    return 10;
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                    return 11;
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                    return 12;
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.BitwiseNotExpression:
                case SyntaxKind.LogicalNotExpression:
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PreDecrementExpression:
                    return 13;
                case SyntaxKind.CastExpression:
                    return 14;
                default:
                    return 0;
            }
        }

        public static SyntaxKind GetLiteralExpression(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.FloatLiteralToken:
                case SyntaxKind.IntegerLiteralToken:
                    return SyntaxKind.NumericLiteralExpression;
                case SyntaxKind.CharacterLiteralToken:
                    return SyntaxKind.CharacterLiteralExpression;
                case SyntaxKind.TrueKeyword:
                    return SyntaxKind.TrueLiteralExpression;
                case SyntaxKind.FalseKeyword:
                    return SyntaxKind.FalseLiteralExpression;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static BinaryOperatorKind GetBinaryOperatorKind(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                    return BinaryOperatorKind.Multiply;
                case SyntaxKind.DivideExpression:
                case SyntaxKind.DivideAssignmentExpression:
                    return BinaryOperatorKind.Divide;
                case SyntaxKind.ModuloExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                    return BinaryOperatorKind.Modulo;
                case SyntaxKind.AddExpression:
                case SyntaxKind.AddAssignmentExpression:
                    return BinaryOperatorKind.Add;
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                    return BinaryOperatorKind.Subtract;
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                    return BinaryOperatorKind.LeftShift;
                case SyntaxKind.RightShiftExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                    return BinaryOperatorKind.RightShift;
                case SyntaxKind.LessThanExpression:
                    return BinaryOperatorKind.Less;
                case SyntaxKind.GreaterThanExpression:
                    return BinaryOperatorKind.Greater;
                case SyntaxKind.LessThanOrEqualExpression:
                    return BinaryOperatorKind.LessEqual;
                case SyntaxKind.GreaterThanOrEqualExpression:
                    return BinaryOperatorKind.GreaterEqual;
                case SyntaxKind.EqualsExpression:
                    return BinaryOperatorKind.Equal;
                case SyntaxKind.NotEqualsExpression:
                    return BinaryOperatorKind.NotEqual;
                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.AndAssignmentExpression:
                    return BinaryOperatorKind.BitwiseAnd;
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                    return BinaryOperatorKind.BitwiseXor;
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.OrAssignmentExpression:
                    return BinaryOperatorKind.BitwiseOr;
                case SyntaxKind.LogicalAndExpression:
                    return BinaryOperatorKind.LogicalAnd;
                case SyntaxKind.LogicalOrExpression:
                    return BinaryOperatorKind.LogicalOr;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static UnaryOperatorKind GetUnaryOperatorKind(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.UnaryPlusExpression:
                    return UnaryOperatorKind.Plus;
                case SyntaxKind.UnaryMinusExpression:
                    return UnaryOperatorKind.Minus;
                case SyntaxKind.PreIncrementExpression:
                    return UnaryOperatorKind.PreIncrement;
                case SyntaxKind.PostIncrementExpression:
                    return UnaryOperatorKind.PostIncrement;
                case SyntaxKind.PreDecrementExpression:
                    return UnaryOperatorKind.PreDecrement;
                case SyntaxKind.PostDecrementExpression:
                    return UnaryOperatorKind.PostDecrement;
                case SyntaxKind.LogicalNotExpression:
                    return UnaryOperatorKind.LogicalNot;
                case SyntaxKind.BitwiseNotExpression:
                    return UnaryOperatorKind.BitwiseNot;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool IsTrivia(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.WhitespaceTrivia:
                case SyntaxKind.EndOfLineTrivia:
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.BlockCommentEndOfFile:
                case SyntaxKind.SkippedTokensTrivia:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsLiteral(this SyntaxKind kind)
        {
            return kind == SyntaxKind.FloatLiteralToken ||
                   kind == SyntaxKind.IntegerLiteralToken ||
                   kind == SyntaxKind.CharacterLiteralToken ||
                   kind == SyntaxKind.StringLiteralToken;
        }

        public static bool IsKeyword(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.AppendStructuredBufferKeyword:
                case SyntaxKind.BlendStateKeyword:
                case SyntaxKind.BoolKeyword:
                case SyntaxKind.Bool1Keyword:
                case SyntaxKind.Bool2Keyword:
                case SyntaxKind.Bool3Keyword:
                case SyntaxKind.Bool4Keyword:
                case SyntaxKind.Bool1x1Keyword:
                case SyntaxKind.Bool1x2Keyword:
                case SyntaxKind.Bool1x3Keyword:
                case SyntaxKind.Bool1x4Keyword:
                case SyntaxKind.Bool2x1Keyword:
                case SyntaxKind.Bool2x2Keyword:
                case SyntaxKind.Bool2x3Keyword:
                case SyntaxKind.Bool2x4Keyword:
                case SyntaxKind.Bool3x1Keyword:
                case SyntaxKind.Bool3x2Keyword:
                case SyntaxKind.Bool3x3Keyword:
                case SyntaxKind.Bool3x4Keyword:
                case SyntaxKind.Bool4x1Keyword:
                case SyntaxKind.Bool4x2Keyword:
                case SyntaxKind.Bool4x3Keyword:
                case SyntaxKind.Bool4x4Keyword:
                case SyntaxKind.BufferKeyword:
                case SyntaxKind.ByteAddressBufferKeyword:
                case SyntaxKind.BreakKeyword:
                case SyntaxKind.CaseKeyword:
                case SyntaxKind.CBufferKeyword:
                case SyntaxKind.CentroidKeyword:
                case SyntaxKind.ClassKeyword:
                case SyntaxKind.ColumnMajorKeyword:
                case SyntaxKind.CompileKeyword:
                case SyntaxKind.CompileShaderKeyword:
                case SyntaxKind.ConstantBufferKeyword:
                case SyntaxKind.ConstKeyword:
                case SyntaxKind.ConsumeStructuredBufferKeyword:
                case SyntaxKind.ContinueKeyword:
                case SyntaxKind.DefaultKeyword:
                case SyntaxKind.DepthStencilStateKeyword:
                case SyntaxKind.DiscardKeyword:
                case SyntaxKind.DoKeyword:
                case SyntaxKind.DoubleKeyword:
                case SyntaxKind.Double1Keyword:
                case SyntaxKind.Double2Keyword:
                case SyntaxKind.Double3Keyword:
                case SyntaxKind.Double4Keyword:
                case SyntaxKind.Double1x1Keyword:
                case SyntaxKind.Double1x2Keyword:
                case SyntaxKind.Double1x3Keyword:
                case SyntaxKind.Double1x4Keyword:
                case SyntaxKind.Double2x1Keyword:
                case SyntaxKind.Double2x2Keyword:
                case SyntaxKind.Double2x3Keyword:
                case SyntaxKind.Double2x4Keyword:
                case SyntaxKind.Double3x1Keyword:
                case SyntaxKind.Double3x2Keyword:
                case SyntaxKind.Double3x3Keyword:
                case SyntaxKind.Double3x4Keyword:
                case SyntaxKind.Double4x1Keyword:
                case SyntaxKind.Double4x2Keyword:
                case SyntaxKind.Double4x3Keyword:
                case SyntaxKind.Double4x4Keyword:
                case SyntaxKind.ElseKeyword:
                case SyntaxKind.ExportKeyword:
                case SyntaxKind.ExternKeyword:
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.FloatKeyword:
                case SyntaxKind.Float1Keyword:
                case SyntaxKind.Float2Keyword:
                case SyntaxKind.Float3Keyword:
                case SyntaxKind.Float4Keyword:
                case SyntaxKind.Float1x1Keyword:
                case SyntaxKind.Float1x2Keyword:
                case SyntaxKind.Float1x3Keyword:
                case SyntaxKind.Float1x4Keyword:
                case SyntaxKind.Float2x1Keyword:
                case SyntaxKind.Float2x2Keyword:
                case SyntaxKind.Float2x3Keyword:
                case SyntaxKind.Float2x4Keyword:
                case SyntaxKind.Float3x1Keyword:
                case SyntaxKind.Float3x2Keyword:
                case SyntaxKind.Float3x3Keyword:
                case SyntaxKind.Float3x4Keyword:
                case SyntaxKind.Float4x1Keyword:
                case SyntaxKind.Float4x2Keyword:
                case SyntaxKind.Float4x3Keyword:
                case SyntaxKind.Float4x4Keyword:
                case SyntaxKind.ForKeyword:
                case SyntaxKind.GeometryShaderKeyword:
                case SyntaxKind.GloballycoherentKeyword:
                case SyntaxKind.GroupsharedKeyword:
                case SyntaxKind.HalfKeyword:
                case SyntaxKind.Half1Keyword:
                case SyntaxKind.Half2Keyword:
                case SyntaxKind.Half3Keyword:
                case SyntaxKind.Half4Keyword:
                case SyntaxKind.Half1x1Keyword:
                case SyntaxKind.Half1x2Keyword:
                case SyntaxKind.Half1x3Keyword:
                case SyntaxKind.Half1x4Keyword:
                case SyntaxKind.Half2x1Keyword:
                case SyntaxKind.Half2x2Keyword:
                case SyntaxKind.Half2x3Keyword:
                case SyntaxKind.Half2x4Keyword:
                case SyntaxKind.Half3x1Keyword:
                case SyntaxKind.Half3x2Keyword:
                case SyntaxKind.Half3x3Keyword:
                case SyntaxKind.Half3x4Keyword:
                case SyntaxKind.Half4x1Keyword:
                case SyntaxKind.Half4x2Keyword:
                case SyntaxKind.Half4x3Keyword:
                case SyntaxKind.Half4x4Keyword:
                case SyntaxKind.IfKeyword:
                case SyntaxKind.InKeyword:
                case SyntaxKind.InlineKeyword:
                case SyntaxKind.InoutKeyword:
                case SyntaxKind.InputPatchKeyword:
                case SyntaxKind.IntKeyword:
                case SyntaxKind.Int1Keyword:
                case SyntaxKind.Int2Keyword:
                case SyntaxKind.Int3Keyword:
                case SyntaxKind.Int4Keyword:
                case SyntaxKind.Int1x1Keyword:
                case SyntaxKind.Int1x2Keyword:
                case SyntaxKind.Int1x3Keyword:
                case SyntaxKind.Int1x4Keyword:
                case SyntaxKind.Int2x1Keyword:
                case SyntaxKind.Int2x2Keyword:
                case SyntaxKind.Int2x3Keyword:
                case SyntaxKind.Int2x4Keyword:
                case SyntaxKind.Int3x1Keyword:
                case SyntaxKind.Int3x2Keyword:
                case SyntaxKind.Int3x3Keyword:
                case SyntaxKind.Int3x4Keyword:
                case SyntaxKind.Int4x1Keyword:
                case SyntaxKind.Int4x2Keyword:
                case SyntaxKind.Int4x3Keyword:
                case SyntaxKind.Int4x4Keyword:
                case SyntaxKind.InterfaceKeyword:
                case SyntaxKind.LineKeyword:
                case SyntaxKind.LineAdjKeyword:
                case SyntaxKind.LinearKeyword:
                case SyntaxKind.LineStreamKeyword:
                case SyntaxKind.MatrixKeyword:
                case SyntaxKind.Min10FloatKeyword:
                case SyntaxKind.Min10Float1Keyword:
                case SyntaxKind.Min10Float2Keyword:
                case SyntaxKind.Min10Float3Keyword:
                case SyntaxKind.Min10Float4Keyword:
                case SyntaxKind.Min10Float1x1Keyword:
                case SyntaxKind.Min10Float1x2Keyword:
                case SyntaxKind.Min10Float1x3Keyword:
                case SyntaxKind.Min10Float1x4Keyword:
                case SyntaxKind.Min10Float2x1Keyword:
                case SyntaxKind.Min10Float2x2Keyword:
                case SyntaxKind.Min10Float2x3Keyword:
                case SyntaxKind.Min10Float2x4Keyword:
                case SyntaxKind.Min10Float3x1Keyword:
                case SyntaxKind.Min10Float3x2Keyword:
                case SyntaxKind.Min10Float3x3Keyword:
                case SyntaxKind.Min10Float3x4Keyword:
                case SyntaxKind.Min10Float4x1Keyword:
                case SyntaxKind.Min10Float4x2Keyword:
                case SyntaxKind.Min10Float4x3Keyword:
                case SyntaxKind.Min10Float4x4Keyword:
                case SyntaxKind.Min12IntKeyword:
                case SyntaxKind.Min12Int1Keyword:
                case SyntaxKind.Min12Int2Keyword:
                case SyntaxKind.Min12Int3Keyword:
                case SyntaxKind.Min12Int4Keyword:
                case SyntaxKind.Min12Int1x1Keyword:
                case SyntaxKind.Min12Int1x2Keyword:
                case SyntaxKind.Min12Int1x3Keyword:
                case SyntaxKind.Min12Int1x4Keyword:
                case SyntaxKind.Min12Int2x1Keyword:
                case SyntaxKind.Min12Int2x2Keyword:
                case SyntaxKind.Min12Int2x3Keyword:
                case SyntaxKind.Min12Int2x4Keyword:
                case SyntaxKind.Min12Int3x1Keyword:
                case SyntaxKind.Min12Int3x2Keyword:
                case SyntaxKind.Min12Int3x3Keyword:
                case SyntaxKind.Min12Int3x4Keyword:
                case SyntaxKind.Min12Int4x1Keyword:
                case SyntaxKind.Min12Int4x2Keyword:
                case SyntaxKind.Min12Int4x3Keyword:
                case SyntaxKind.Min12Int4x4Keyword:
                case SyntaxKind.Min16FloatKeyword:
                case SyntaxKind.Min16Float1Keyword:
                case SyntaxKind.Min16Float2Keyword:
                case SyntaxKind.Min16Float3Keyword:
                case SyntaxKind.Min16Float4Keyword:
                case SyntaxKind.Min16Float1x1Keyword:
                case SyntaxKind.Min16Float1x2Keyword:
                case SyntaxKind.Min16Float1x3Keyword:
                case SyntaxKind.Min16Float1x4Keyword:
                case SyntaxKind.Min16Float2x1Keyword:
                case SyntaxKind.Min16Float2x2Keyword:
                case SyntaxKind.Min16Float2x3Keyword:
                case SyntaxKind.Min16Float2x4Keyword:
                case SyntaxKind.Min16Float3x1Keyword:
                case SyntaxKind.Min16Float3x2Keyword:
                case SyntaxKind.Min16Float3x3Keyword:
                case SyntaxKind.Min16Float3x4Keyword:
                case SyntaxKind.Min16Float4x1Keyword:
                case SyntaxKind.Min16Float4x2Keyword:
                case SyntaxKind.Min16Float4x3Keyword:
                case SyntaxKind.Min16Float4x4Keyword:
                case SyntaxKind.Min16IntKeyword:
                case SyntaxKind.Min16Int1Keyword:
                case SyntaxKind.Min16Int2Keyword:
                case SyntaxKind.Min16Int3Keyword:
                case SyntaxKind.Min16Int4Keyword:
                case SyntaxKind.Min16Int1x1Keyword:
                case SyntaxKind.Min16Int1x2Keyword:
                case SyntaxKind.Min16Int1x3Keyword:
                case SyntaxKind.Min16Int1x4Keyword:
                case SyntaxKind.Min16Int2x1Keyword:
                case SyntaxKind.Min16Int2x2Keyword:
                case SyntaxKind.Min16Int2x3Keyword:
                case SyntaxKind.Min16Int2x4Keyword:
                case SyntaxKind.Min16Int3x1Keyword:
                case SyntaxKind.Min16Int3x2Keyword:
                case SyntaxKind.Min16Int3x3Keyword:
                case SyntaxKind.Min16Int3x4Keyword:
                case SyntaxKind.Min16Int4x1Keyword:
                case SyntaxKind.Min16Int4x2Keyword:
                case SyntaxKind.Min16Int4x3Keyword:
                case SyntaxKind.Min16Int4x4Keyword:
                case SyntaxKind.Min16UintKeyword:
                case SyntaxKind.Min16Uint1Keyword:
                case SyntaxKind.Min16Uint2Keyword:
                case SyntaxKind.Min16Uint3Keyword:
                case SyntaxKind.Min16Uint4Keyword:
                case SyntaxKind.Min16Uint1x1Keyword:
                case SyntaxKind.Min16Uint1x2Keyword:
                case SyntaxKind.Min16Uint1x3Keyword:
                case SyntaxKind.Min16Uint1x4Keyword:
                case SyntaxKind.Min16Uint2x1Keyword:
                case SyntaxKind.Min16Uint2x2Keyword:
                case SyntaxKind.Min16Uint2x3Keyword:
                case SyntaxKind.Min16Uint2x4Keyword:
                case SyntaxKind.Min16Uint3x1Keyword:
                case SyntaxKind.Min16Uint3x2Keyword:
                case SyntaxKind.Min16Uint3x3Keyword:
                case SyntaxKind.Min16Uint3x4Keyword:
                case SyntaxKind.Min16Uint4x1Keyword:
                case SyntaxKind.Min16Uint4x2Keyword:
                case SyntaxKind.Min16Uint4x3Keyword:
                case SyntaxKind.Min16Uint4x4Keyword:
                case SyntaxKind.NamespaceKeyword:
                case SyntaxKind.NointerpolationKeyword:
                case SyntaxKind.NoperspectiveKeyword:
                case SyntaxKind.NullKeyword:
                case SyntaxKind.OutKeyword:
                case SyntaxKind.OutputPatchKeyword:
                case SyntaxKind.PackoffsetKeyword:
                case SyntaxKind.PassKeyword:
                case SyntaxKind.PixelShaderKeyword:
                case SyntaxKind.PointKeyword:
                case SyntaxKind.PointStreamKeyword:
                case SyntaxKind.PreciseKeyword:
                case SyntaxKind.RasterizerOrderedBufferKeyword:
                case SyntaxKind.RasterizerOrderedByteAddressBufferKeyword:
                case SyntaxKind.RasterizerOrderedStructuredBufferKeyword:
                case SyntaxKind.RasterizerOrderedTexture1DKeyword:
                case SyntaxKind.RasterizerOrderedTexture1DArrayKeyword:
                case SyntaxKind.RasterizerOrderedTexture2DKeyword:
                case SyntaxKind.RasterizerOrderedTexture2DArrayKeyword:
                case SyntaxKind.RasterizerOrderedTexture3DKeyword:
                case SyntaxKind.RasterizerStateKeyword:
                case SyntaxKind.RegisterKeyword:
                case SyntaxKind.ReturnKeyword:
                case SyntaxKind.RowMajorKeyword:
                case SyntaxKind.RWBufferKeyword:
                case SyntaxKind.RWByteAddressBufferKeyword:
                case SyntaxKind.RWStructuredBufferKeyword:
                case SyntaxKind.RWTexture1DKeyword:
                case SyntaxKind.RWTexture1DArrayKeyword:
                case SyntaxKind.RWTexture2DKeyword:
                case SyntaxKind.RWTexture2DArrayKeyword:
                case SyntaxKind.RWTexture3DKeyword:
                case SyntaxKind.SampleKeyword:
                case SyntaxKind.SamplerKeyword:
                case SyntaxKind.Sampler1DKeyword:
                case SyntaxKind.Sampler2DKeyword:
                case SyntaxKind.Sampler3DKeyword:
                case SyntaxKind.SamplerCubeKeyword:
                case SyntaxKind.SamplerComparisonStateKeyword:
                case SyntaxKind.SamplerStateKeyword:
                case SyntaxKind.SamplerStateLegacyKeyword:
                case SyntaxKind.SharedKeyword:
                case SyntaxKind.SNormKeyword:
                case SyntaxKind.StaticKeyword:
                case SyntaxKind.StringKeyword:
                case SyntaxKind.StructKeyword:
                case SyntaxKind.StructuredBufferKeyword:
                case SyntaxKind.SwitchKeyword:
                case SyntaxKind.TBufferKeyword:
                case SyntaxKind.TechniqueKeyword:
                case SyntaxKind.Technique10Keyword:
                case SyntaxKind.Technique11Keyword:
                case SyntaxKind.TextureKeyword:
                case SyntaxKind.Texture2DLegacyKeyword:
                case SyntaxKind.TextureCubeLegacyKeyword:
                case SyntaxKind.Texture1DKeyword:
                case SyntaxKind.Texture1DArrayKeyword:
                case SyntaxKind.Texture2DKeyword:
                case SyntaxKind.Texture2DArrayKeyword:
                case SyntaxKind.Texture2DMSKeyword:
                case SyntaxKind.Texture2DMSArrayKeyword:
                case SyntaxKind.Texture3DKeyword:
                case SyntaxKind.TextureCubeKeyword:
                case SyntaxKind.TextureCubeArrayKeyword:
                case SyntaxKind.TriangleKeyword:
                case SyntaxKind.TriangleAdjKeyword:
                case SyntaxKind.TriangleStreamKeyword:
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.TypedefKeyword:
                case SyntaxKind.UniformKeyword:
                case SyntaxKind.UNormKeyword:
                case SyntaxKind.UnsignedKeyword:
                case SyntaxKind.UintKeyword:
                case SyntaxKind.Uint1Keyword:
                case SyntaxKind.Uint2Keyword:
                case SyntaxKind.Uint3Keyword:
                case SyntaxKind.Uint4Keyword:
                case SyntaxKind.Uint1x1Keyword:
                case SyntaxKind.Uint1x2Keyword:
                case SyntaxKind.Uint1x3Keyword:
                case SyntaxKind.Uint1x4Keyword:
                case SyntaxKind.Uint2x1Keyword:
                case SyntaxKind.Uint2x2Keyword:
                case SyntaxKind.Uint2x3Keyword:
                case SyntaxKind.Uint2x4Keyword:
                case SyntaxKind.Uint3x1Keyword:
                case SyntaxKind.Uint3x2Keyword:
                case SyntaxKind.Uint3x3Keyword:
                case SyntaxKind.Uint3x4Keyword:
                case SyntaxKind.Uint4x1Keyword:
                case SyntaxKind.Uint4x2Keyword:
                case SyntaxKind.Uint4x3Keyword:
                case SyntaxKind.Uint4x4Keyword:
                case SyntaxKind.VectorKeyword:
                case SyntaxKind.VertexShaderKeyword:
                case SyntaxKind.VolatileKeyword:
                case SyntaxKind.VoidKeyword:
                case SyntaxKind.WhileKeyword:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsWord(this SyntaxToken token)
        {
            return token.Kind == SyntaxKind.IdentifierToken
                || token.Kind.IsKeyword()
                || token.Kind.IsPreprocessorKeyword();
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

        public static bool IsPreprocessorDirective(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.IfKeyword:
                case SyntaxKind.IfDefKeyword:
                case SyntaxKind.IfNDefKeyword:
                case SyntaxKind.ElseKeyword:
                case SyntaxKind.ElifKeyword:
                case SyntaxKind.EndIfKeyword:
                case SyntaxKind.DefineKeyword:
                case SyntaxKind.UndefKeyword:
                case SyntaxKind.IncludeKeyword:
                case SyntaxKind.LineKeyword:
                case SyntaxKind.ErrorKeyword:
                case SyntaxKind.PragmaKeyword:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsIfLikeDirective(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.IfDirectiveTrivia:
                case SyntaxKind.IfDefDirectiveTrivia:
                case SyntaxKind.IfNDefDirectiveTrivia:
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

        public static SyntaxKind GetKeywordKind(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            switch (text)
            {
                case "AppendStructuredBuffer":
                    return SyntaxKind.AppendStructuredBufferKeyword;
                case "BlendState":
                    return SyntaxKind.BlendStateKeyword;
                case "bool":
                    return SyntaxKind.BoolKeyword;
                case "bool1":
                    return SyntaxKind.Bool1Keyword;
                case "bool2":
                    return SyntaxKind.Bool2Keyword;
                case "bool3":
                    return SyntaxKind.Bool3Keyword;
                case "bool4":
                    return SyntaxKind.Bool4Keyword;
                case "bool1x1":
                    return SyntaxKind.Bool1x1Keyword;
                case "bool1x2":
                    return SyntaxKind.Bool1x2Keyword;
                case "bool1x3":
                    return SyntaxKind.Bool1x3Keyword;
                case "bool1x4":
                    return SyntaxKind.Bool1x4Keyword;
                case "bool2x1":
                    return SyntaxKind.Bool2x1Keyword;
                case "bool2x2":
                    return SyntaxKind.Bool2x2Keyword;
                case "bool2x3":
                    return SyntaxKind.Bool2x3Keyword;
                case "bool2x4":
                    return SyntaxKind.Bool2x4Keyword;
                case "bool3x1":
                    return SyntaxKind.Bool3x1Keyword;
                case "bool3x2":
                    return SyntaxKind.Bool3x2Keyword;
                case "bool3x3":
                    return SyntaxKind.Bool3x3Keyword;
                case "bool3x4":
                    return SyntaxKind.Bool3x4Keyword;
                case "bool4x1":
                    return SyntaxKind.Bool4x1Keyword;
                case "bool4x2":
                    return SyntaxKind.Bool4x2Keyword;
                case "bool4x3":
                    return SyntaxKind.Bool4x3Keyword;
                case "bool4x4":
                    return SyntaxKind.Bool4x4Keyword;
                case "Buffer":
                    return SyntaxKind.BufferKeyword;
                case "ByteAddressBuffer":
                    return SyntaxKind.ByteAddressBufferKeyword;
                case "break":
                    return SyntaxKind.BreakKeyword;
                case "case":
                    return SyntaxKind.CaseKeyword;
                case "cbuffer":
                    return SyntaxKind.CBufferKeyword;
                case "centroid":
                    return SyntaxKind.CentroidKeyword;
                case "class":
                    return SyntaxKind.ClassKeyword;
                case "column_major":
                    return SyntaxKind.ColumnMajorKeyword;
                case "compile":
                    return SyntaxKind.CompileKeyword;
                case "const":
                    return SyntaxKind.ConstKeyword;
                case "ConsumeStructuredBuffer":
                    return SyntaxKind.ConsumeStructuredBufferKeyword;
                case "continue":
                    return SyntaxKind.ContinueKeyword;
                case "default":
                    return SyntaxKind.DefaultKeyword;
                case "DepthStencilState":
                    return SyntaxKind.DepthStencilStateKeyword;
                case "discard":
                    return SyntaxKind.DiscardKeyword;
                case "do":
                    return SyntaxKind.DoKeyword;
                case "double":
                    return SyntaxKind.DoubleKeyword;
                case "double1":
                    return SyntaxKind.Double1Keyword;
                case "double2":
                    return SyntaxKind.Double2Keyword;
                case "double3":
                    return SyntaxKind.Double3Keyword;
                case "double4":
                    return SyntaxKind.Double4Keyword;
                case "double1x1":
                    return SyntaxKind.Double1x1Keyword;
                case "double1x2":
                    return SyntaxKind.Double1x2Keyword;
                case "double1x3":
                    return SyntaxKind.Double1x3Keyword;
                case "double1x4":
                    return SyntaxKind.Double1x4Keyword;
                case "double2x1":
                    return SyntaxKind.Double2x1Keyword;
                case "double2x2":
                    return SyntaxKind.Double2x2Keyword;
                case "double2x3":
                    return SyntaxKind.Double2x3Keyword;
                case "double2x4":
                    return SyntaxKind.Double2x4Keyword;
                case "double3x1":
                    return SyntaxKind.Double3x1Keyword;
                case "double3x2":
                    return SyntaxKind.Double3x2Keyword;
                case "double3x3":
                    return SyntaxKind.Double3x3Keyword;
                case "double3x4":
                    return SyntaxKind.Double3x4Keyword;
                case "double4x1":
                    return SyntaxKind.Double4x1Keyword;
                case "double4x2":
                    return SyntaxKind.Double4x2Keyword;
                case "double4x3":
                    return SyntaxKind.Double4x3Keyword;
                case "double4x4":
                    return SyntaxKind.Double4x4Keyword;
                case "else":
                    return SyntaxKind.ElseKeyword;
                case "export":
                    return SyntaxKind.ExportKeyword;
                case "extern":
                    return SyntaxKind.ExternKeyword;
                case "float":
                    return SyntaxKind.FloatKeyword;
                case "float1":
                    return SyntaxKind.Float1Keyword;
                case "float2":
                    return SyntaxKind.Float2Keyword;
                case "float3":
                    return SyntaxKind.Float3Keyword;
                case "float4":
                    return SyntaxKind.Float4Keyword;
                case "float1x1":
                    return SyntaxKind.Float1x1Keyword;
                case "float1x2":
                    return SyntaxKind.Float1x2Keyword;
                case "float1x3":
                    return SyntaxKind.Float1x3Keyword;
                case "float1x4":
                    return SyntaxKind.Float1x4Keyword;
                case "float2x1":
                    return SyntaxKind.Float2x1Keyword;
                case "float2x2":
                    return SyntaxKind.Float2x2Keyword;
                case "float2x3":
                    return SyntaxKind.Float2x3Keyword;
                case "float2x4":
                    return SyntaxKind.Float2x4Keyword;
                case "float3x1":
                    return SyntaxKind.Float3x1Keyword;
                case "float3x2":
                    return SyntaxKind.Float3x2Keyword;
                case "float3x3":
                    return SyntaxKind.Float3x3Keyword;
                case "float3x4":
                    return SyntaxKind.Float3x4Keyword;
                case "float4x1":
                    return SyntaxKind.Float4x1Keyword;
                case "float4x2":
                    return SyntaxKind.Float4x2Keyword;
                case "float4x3":
                    return SyntaxKind.Float4x3Keyword;
                case "float4x4":
                    return SyntaxKind.Float4x4Keyword;
                case "for":
                    return SyntaxKind.ForKeyword;
                case "globallycoherent":
                    return SyntaxKind.GloballycoherentKeyword;
                case "groupshared":
                    return SyntaxKind.GroupsharedKeyword;
                case "half":
                    return SyntaxKind.HalfKeyword;
                case "half1":
                    return SyntaxKind.Half1Keyword;
                case "half2":
                    return SyntaxKind.Half2Keyword;
                case "half3":
                    return SyntaxKind.Half3Keyword;
                case "half4":
                    return SyntaxKind.Half4Keyword;
                case "half1x1":
                    return SyntaxKind.Half1x1Keyword;
                case "half1x2":
                    return SyntaxKind.Half1x2Keyword;
                case "half1x3":
                    return SyntaxKind.Half1x3Keyword;
                case "half1x4":
                    return SyntaxKind.Half1x4Keyword;
                case "half2x1":
                    return SyntaxKind.Half2x1Keyword;
                case "half2x2":
                    return SyntaxKind.Half2x2Keyword;
                case "half2x3":
                    return SyntaxKind.Half2x3Keyword;
                case "half2x4":
                    return SyntaxKind.Half2x4Keyword;
                case "half3x1":
                    return SyntaxKind.Half3x1Keyword;
                case "half3x2":
                    return SyntaxKind.Half3x2Keyword;
                case "half3x3":
                    return SyntaxKind.Half3x3Keyword;
                case "half3x4":
                    return SyntaxKind.Half3x4Keyword;
                case "half4x1":
                    return SyntaxKind.Half4x1Keyword;
                case "half4x2":
                    return SyntaxKind.Half4x2Keyword;
                case "half4x3":
                    return SyntaxKind.Half4x3Keyword;
                case "half4x4":
                    return SyntaxKind.Half4x4Keyword;
                case "if":
                    return SyntaxKind.IfKeyword;
                case "in":
                    return SyntaxKind.InKeyword;
                case "inline":
                    return SyntaxKind.InlineKeyword;
                case "inout":
                    return SyntaxKind.InoutKeyword;
                case "InputPatch":
                    return SyntaxKind.InputPatchKeyword;
                case "int":
                    return SyntaxKind.IntKeyword;
                case "int1":
                    return SyntaxKind.Int1Keyword;
                case "int2":
                    return SyntaxKind.Int2Keyword;
                case "int3":
                    return SyntaxKind.Int3Keyword;
                case "int4":
                    return SyntaxKind.Int4Keyword;
                case "int1x1":
                    return SyntaxKind.Int1x1Keyword;
                case "int1x2":
                    return SyntaxKind.Int1x2Keyword;
                case "int1x3":
                    return SyntaxKind.Int1x3Keyword;
                case "int1x4":
                    return SyntaxKind.Int1x4Keyword;
                case "int2x1":
                    return SyntaxKind.Int2x1Keyword;
                case "int2x2":
                    return SyntaxKind.Int2x2Keyword;
                case "int2x3":
                    return SyntaxKind.Int2x3Keyword;
                case "int2x4":
                    return SyntaxKind.Int2x4Keyword;
                case "int3x1":
                    return SyntaxKind.Int3x1Keyword;
                case "int3x2":
                    return SyntaxKind.Int3x2Keyword;
                case "int3x3":
                    return SyntaxKind.Int3x3Keyword;
                case "int3x4":
                    return SyntaxKind.Int3x4Keyword;
                case "int4x1":
                    return SyntaxKind.Int4x1Keyword;
                case "int4x2":
                    return SyntaxKind.Int4x2Keyword;
                case "int4x3":
                    return SyntaxKind.Int4x3Keyword;
                case "int4x4":
                    return SyntaxKind.Int4x4Keyword;
                case "interface":
                    return SyntaxKind.InterfaceKeyword;
                case "lineadj":
                    return SyntaxKind.LineAdjKeyword;
                case "linear":
                    return SyntaxKind.LinearKeyword;
                case "LineStream":
                    return SyntaxKind.LineStreamKeyword;
                case "Matrix":
                case "matrix":
                    return SyntaxKind.MatrixKeyword;
                case "min10float":
                    return SyntaxKind.Min10FloatKeyword;
                case "min10float1":
                    return SyntaxKind.Min10Float1Keyword;
                case "min10float2":
                    return SyntaxKind.Min10Float2Keyword;
                case "min10float3":
                    return SyntaxKind.Min10Float3Keyword;
                case "min10float4":
                    return SyntaxKind.Min10Float4Keyword;
                case "min10float1x1":
                    return SyntaxKind.Min10Float1x1Keyword;
                case "min10float1x2":
                    return SyntaxKind.Min10Float1x2Keyword;
                case "min10float1x3":
                    return SyntaxKind.Min10Float1x3Keyword;
                case "min10float1x4":
                    return SyntaxKind.Min10Float1x4Keyword;
                case "min10float2x1":
                    return SyntaxKind.Min10Float2x1Keyword;
                case "min10float2x2":
                    return SyntaxKind.Min10Float2x2Keyword;
                case "min10float2x3":
                    return SyntaxKind.Min10Float2x3Keyword;
                case "min10float2x4":
                    return SyntaxKind.Min10Float2x4Keyword;
                case "min10float3x1":
                    return SyntaxKind.Min10Float3x1Keyword;
                case "min10float3x2":
                    return SyntaxKind.Min10Float3x2Keyword;
                case "min10float3x3":
                    return SyntaxKind.Min10Float3x3Keyword;
                case "min10float3x4":
                    return SyntaxKind.Min10Float3x4Keyword;
                case "min10float4x1":
                    return SyntaxKind.Min10Float4x1Keyword;
                case "min10float4x2":
                    return SyntaxKind.Min10Float4x2Keyword;
                case "min10float4x3":
                    return SyntaxKind.Min10Float4x3Keyword;
                case "min10float4x4":
                    return SyntaxKind.Min10Float4x4Keyword;
                case "min12int":
                    return SyntaxKind.Min12IntKeyword;
                case "min12int1":
                    return SyntaxKind.Min12Int1Keyword;
                case "min12int2":
                    return SyntaxKind.Min12Int2Keyword;
                case "min12int3":
                    return SyntaxKind.Min12Int3Keyword;
                case "min12int4":
                    return SyntaxKind.Min12Int4Keyword;
                case "min12int1x1":
                    return SyntaxKind.Min12Int1x1Keyword;
                case "min12int1x2":
                    return SyntaxKind.Min12Int1x2Keyword;
                case "min12int1x3":
                    return SyntaxKind.Min12Int1x3Keyword;
                case "min12int1x4":
                    return SyntaxKind.Min12Int1x4Keyword;
                case "min12int2x1":
                    return SyntaxKind.Min12Int2x1Keyword;
                case "min12int2x2":
                    return SyntaxKind.Min12Int2x2Keyword;
                case "min12int2x3":
                    return SyntaxKind.Min12Int2x3Keyword;
                case "min12int2x4":
                    return SyntaxKind.Min12Int2x4Keyword;
                case "min12int3x1":
                    return SyntaxKind.Min12Int3x1Keyword;
                case "min12int3x2":
                    return SyntaxKind.Min12Int3x2Keyword;
                case "min12int3x3":
                    return SyntaxKind.Min12Int3x3Keyword;
                case "min12int3x4":
                    return SyntaxKind.Min12Int3x4Keyword;
                case "min12int4x1":
                    return SyntaxKind.Min12Int4x1Keyword;
                case "min12int4x2":
                    return SyntaxKind.Min12Int4x2Keyword;
                case "min12int4x3":
                    return SyntaxKind.Min12Int4x3Keyword;
                case "min12int4x4":
                    return SyntaxKind.Min12Int4x4Keyword;
                case "min16float":
                    return SyntaxKind.Min16FloatKeyword;
                case "min16float1":
                    return SyntaxKind.Min16Float1Keyword;
                case "min16float2":
                    return SyntaxKind.Min16Float2Keyword;
                case "min16float3":
                    return SyntaxKind.Min16Float3Keyword;
                case "min16float4":
                    return SyntaxKind.Min16Float4Keyword;
                case "min16float1x1":
                    return SyntaxKind.Min16Float1x1Keyword;
                case "min16float1x2":
                    return SyntaxKind.Min16Float1x2Keyword;
                case "min16float1x3":
                    return SyntaxKind.Min16Float1x3Keyword;
                case "min16float1x4":
                    return SyntaxKind.Min16Float1x4Keyword;
                case "min16float2x1":
                    return SyntaxKind.Min16Float2x1Keyword;
                case "min16float2x2":
                    return SyntaxKind.Min16Float2x2Keyword;
                case "min16float2x3":
                    return SyntaxKind.Min16Float2x3Keyword;
                case "min16float2x4":
                    return SyntaxKind.Min16Float2x4Keyword;
                case "min16float3x1":
                    return SyntaxKind.Min16Float3x1Keyword;
                case "min16float3x2":
                    return SyntaxKind.Min16Float3x2Keyword;
                case "min16float3x3":
                    return SyntaxKind.Min16Float3x3Keyword;
                case "min16float3x4":
                    return SyntaxKind.Min16Float3x4Keyword;
                case "min16float4x1":
                    return SyntaxKind.Min16Float4x1Keyword;
                case "min16float4x2":
                    return SyntaxKind.Min16Float4x2Keyword;
                case "min16float4x3":
                    return SyntaxKind.Min16Float4x3Keyword;
                case "min16float4x4":
                    return SyntaxKind.Min16Float4x4Keyword;
                case "min16int":
                    return SyntaxKind.Min16IntKeyword;
                case "min16int1":
                    return SyntaxKind.Min16Int1Keyword;
                case "min16int2":
                    return SyntaxKind.Min16Int2Keyword;
                case "min16int3":
                    return SyntaxKind.Min16Int3Keyword;
                case "min16int4":
                    return SyntaxKind.Min16Int4Keyword;
                case "min16int1x1":
                    return SyntaxKind.Min16Int1x1Keyword;
                case "min16int1x2":
                    return SyntaxKind.Min16Int1x2Keyword;
                case "min16int1x3":
                    return SyntaxKind.Min16Int1x3Keyword;
                case "min16int1x4":
                    return SyntaxKind.Min16Int1x4Keyword;
                case "min16int2x1":
                    return SyntaxKind.Min16Int2x1Keyword;
                case "min16int2x2":
                    return SyntaxKind.Min16Int2x2Keyword;
                case "min16int2x3":
                    return SyntaxKind.Min16Int2x3Keyword;
                case "min16int2x4":
                    return SyntaxKind.Min16Int2x4Keyword;
                case "min16int3x1":
                    return SyntaxKind.Min16Int3x1Keyword;
                case "min16int3x2":
                    return SyntaxKind.Min16Int3x2Keyword;
                case "min16int3x3":
                    return SyntaxKind.Min16Int3x3Keyword;
                case "min16int3x4":
                    return SyntaxKind.Min16Int3x4Keyword;
                case "min16int4x1":
                    return SyntaxKind.Min16Int4x1Keyword;
                case "min16int4x2":
                    return SyntaxKind.Min16Int4x2Keyword;
                case "min16int4x3":
                    return SyntaxKind.Min16Int4x3Keyword;
                case "min16int4x4":
                    return SyntaxKind.Min16Int4x4Keyword;
                case "min16uint":
                    return SyntaxKind.Min16UintKeyword;
                case "min16uint1":
                    return SyntaxKind.Min16Uint1Keyword;
                case "min16uint2":
                    return SyntaxKind.Min16Uint2Keyword;
                case "min16uint3":
                    return SyntaxKind.Min16Uint3Keyword;
                case "min16uint4":
                    return SyntaxKind.Min16Uint4Keyword;
                case "min16uint1x1":
                    return SyntaxKind.Min16Uint1x1Keyword;
                case "min16uint1x2":
                    return SyntaxKind.Min16Uint1x2Keyword;
                case "min16uint1x3":
                    return SyntaxKind.Min16Uint1x3Keyword;
                case "min16uint1x4":
                    return SyntaxKind.Min16Uint1x4Keyword;
                case "min16uint2x1":
                    return SyntaxKind.Min16Uint2x1Keyword;
                case "min16uint2x2":
                    return SyntaxKind.Min16Uint2x2Keyword;
                case "min16uint2x3":
                    return SyntaxKind.Min16Uint2x3Keyword;
                case "min16uint2x4":
                    return SyntaxKind.Min16Uint2x4Keyword;
                case "min16uint3x1":
                    return SyntaxKind.Min16Uint3x1Keyword;
                case "min16uint3x2":
                    return SyntaxKind.Min16Uint3x2Keyword;
                case "min16uint3x3":
                    return SyntaxKind.Min16Uint3x3Keyword;
                case "min16uint3x4":
                    return SyntaxKind.Min16Uint3x4Keyword;
                case "min16uint4x1":
                    return SyntaxKind.Min16Uint4x1Keyword;
                case "min16uint4x2":
                    return SyntaxKind.Min16Uint4x2Keyword;
                case "min16uint4x3":
                    return SyntaxKind.Min16Uint4x3Keyword;
                case "min16uint4x4":
                    return SyntaxKind.Min16Uint4x4Keyword;
                case "namespace":
                    return SyntaxKind.NamespaceKeyword;
                case "nointerpolation":
                    return SyntaxKind.NointerpolationKeyword;
                case "noperspective":
                    return SyntaxKind.NoperspectiveKeyword;
                case "out":
                    return SyntaxKind.OutKeyword;
                case "OutputPatch":
                    return SyntaxKind.OutputPatchKeyword;
                case "packoffset":
                    return SyntaxKind.PackoffsetKeyword;
                case "pass":
                case "Pass":
                    return SyntaxKind.PassKeyword;
                case "point":
                    return SyntaxKind.PointKeyword;
                case "PointStream":
                    return SyntaxKind.PointStreamKeyword;
                case "precise":
                    return SyntaxKind.PreciseKeyword;
                case "RasterizerOrderedBuffer":
                    return SyntaxKind.RasterizerOrderedBufferKeyword;
                case "RasterizerOrderedByteAddressBuffer":
                    return SyntaxKind.RasterizerOrderedByteAddressBufferKeyword;
                case "RasterizerOrderedStructuredBuffer":
                    return SyntaxKind.RasterizerOrderedStructuredBufferKeyword;
                case "RasterizerOrderedTexture1D":
                    return SyntaxKind.RasterizerOrderedTexture1DKeyword;
                case "RasterizerOrderedTexture1DArray":
                    return SyntaxKind.RasterizerOrderedTexture1DArrayKeyword;
                case "RasterizerOrderedTexture2D":
                    return SyntaxKind.RasterizerOrderedTexture2DKeyword;
                case "RasterizerOrderedTexture2DArray":
                    return SyntaxKind.RasterizerOrderedTexture2DArrayKeyword;
                case "RasterizerOrderedTexture3D":
                    return SyntaxKind.RasterizerOrderedTexture3DKeyword;
                case "RasterizerState":
                    return SyntaxKind.RasterizerStateKeyword;
                case "register":
                    return SyntaxKind.RegisterKeyword;
                case "return":
                    return SyntaxKind.ReturnKeyword;
                case "row_major":
                    return SyntaxKind.RowMajorKeyword;
                case "RWBuffer":
                    return SyntaxKind.RWBufferKeyword;
                case "RWByteAddressBuffer":
                    return SyntaxKind.RWByteAddressBufferKeyword;
                case "RWStructuredBuffer":
                    return SyntaxKind.RWStructuredBufferKeyword;
                case "RWTexture1D":
                    return SyntaxKind.RWTexture1DKeyword;
                case "RWTexture1DArray":
                    return SyntaxKind.RWTexture1DArrayKeyword;
                case "RWTexture2D":
                    return SyntaxKind.RWTexture2DKeyword;
                case "RWTexture2DArray":
                    return SyntaxKind.RWTexture2DArrayKeyword;
                case "RWTexture3D":
                    return SyntaxKind.RWTexture3DKeyword;
                case "sampler":
                    return SyntaxKind.SamplerKeyword;
                case "sampler1D":
                    return SyntaxKind.Sampler1DKeyword;
                case "sampler2D":
                    return SyntaxKind.Sampler2DKeyword;
                case "sampler3D":
                    return SyntaxKind.Sampler3DKeyword;
                case "samplerCUBE":
                    return SyntaxKind.SamplerCubeKeyword;
                case "SamplerComparisonState":
                    return SyntaxKind.SamplerComparisonStateKeyword;
                case "SamplerState":
                    return SyntaxKind.SamplerStateKeyword;
                case "sampler_state":
                    return SyntaxKind.SamplerStateLegacyKeyword;
                case "shared":
                    return SyntaxKind.SharedKeyword;
                case "snorm":
                    return SyntaxKind.SNormKeyword;
                case "static":
                    return SyntaxKind.StaticKeyword;
                case "string":
                    return SyntaxKind.StringKeyword;
                case "struct":
                    return SyntaxKind.StructKeyword;
                case "StructuredBuffer":
                    return SyntaxKind.StructuredBufferKeyword;
                case "switch":
                    return SyntaxKind.SwitchKeyword;
                case "tbuffer":
                    return SyntaxKind.TBufferKeyword;
                case "technique":
                case "Technique":
                    return SyntaxKind.TechniqueKeyword;
                case "technique10":
                    return SyntaxKind.Technique10Keyword;
                case "technique11":
                    return SyntaxKind.Technique11Keyword;
                case "texture2D":
                    return SyntaxKind.Texture2DLegacyKeyword;
                case "textureCUBE":
                    return SyntaxKind.TextureCubeLegacyKeyword;
                case "Texture1D":
                    return SyntaxKind.Texture1DKeyword;
                case "Texture1DArray":
                    return SyntaxKind.Texture1DArrayKeyword;
                case "Texture2D":
                    return SyntaxKind.Texture2DKeyword;
                case "Texture2DArray":
                    return SyntaxKind.Texture2DArrayKeyword;
                case "Texture2DMS":
                    return SyntaxKind.Texture2DMSKeyword;
                case "Texture2DMSArray":
                    return SyntaxKind.Texture2DMSArrayKeyword;
                case "Texture3D":
                    return SyntaxKind.Texture3DKeyword;
                case "TextureCube":
                    return SyntaxKind.TextureCubeKeyword;
                case "TextureCubeArray":
                    return SyntaxKind.TextureCubeArrayKeyword;
                case "triangle":
                    return SyntaxKind.TriangleKeyword;
                case "triangleadj":
                    return SyntaxKind.TriangleAdjKeyword;
                case "TriangleStream":
                    return SyntaxKind.TriangleStreamKeyword;
                case "typedef":
                    return SyntaxKind.TypedefKeyword;
                case "uniform":
                    return SyntaxKind.UniformKeyword;
                case "unorm":
                    return SyntaxKind.UNormKeyword;
                case "unsigned":
                    return SyntaxKind.UnsignedKeyword;
                case "uint":
                    return SyntaxKind.UintKeyword;
                case "dword":
                    return SyntaxKind.DwordKeyword;
                case "uint1":
                    return SyntaxKind.Uint1Keyword;
                case "uint2":
                    return SyntaxKind.Uint2Keyword;
                case "uint3":
                    return SyntaxKind.Uint3Keyword;
                case "uint4":
                    return SyntaxKind.Uint4Keyword;
                case "uint1x1":
                    return SyntaxKind.Uint1x1Keyword;
                case "uint1x2":
                    return SyntaxKind.Uint1x2Keyword;
                case "uint1x3":
                    return SyntaxKind.Uint1x3Keyword;
                case "uint1x4":
                    return SyntaxKind.Uint1x4Keyword;
                case "uint2x1":
                    return SyntaxKind.Uint2x1Keyword;
                case "uint2x2":
                    return SyntaxKind.Uint2x2Keyword;
                case "uint2x3":
                    return SyntaxKind.Uint2x3Keyword;
                case "uint2x4":
                    return SyntaxKind.Uint2x4Keyword;
                case "uint3x1":
                    return SyntaxKind.Uint3x1Keyword;
                case "uint3x2":
                    return SyntaxKind.Uint3x2Keyword;
                case "uint3x3":
                    return SyntaxKind.Uint3x3Keyword;
                case "uint3x4":
                    return SyntaxKind.Uint3x4Keyword;
                case "uint4x1":
                    return SyntaxKind.Uint4x1Keyword;
                case "uint4x2":
                    return SyntaxKind.Uint4x2Keyword;
                case "uint4x3":
                    return SyntaxKind.Uint4x3Keyword;
                case "uint4x4":
                    return SyntaxKind.Uint4x4Keyword;
                case "vector":
                    return SyntaxKind.VectorKeyword;
                case "volatile":
                    return SyntaxKind.VolatileKeyword;
                case "void":
                    return SyntaxKind.VoidKeyword;
                case "while":
                    return SyntaxKind.WhileKeyword;

                case "false":
                    return SyntaxKind.FalseKeyword;
                case "true":
                    return SyntaxKind.TrueKeyword;

                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static SyntaxKind GetContextualKeywordKind(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            switch (text)
            {
                case "CompileShader":
                    return SyntaxKind.CompileShaderKeyword;
                case "ConstantBuffer":
                    return SyntaxKind.ConstantBufferKeyword;
                case "NULL":
                    return SyntaxKind.NullKeyword;
                case "sample":
                    return SyntaxKind.SampleKeyword;
                case "line":
                    return SyntaxKind.LineKeyword;
                case "texture":
                    return SyntaxKind.TextureKeyword;
                case "GeometryShader":
                    return SyntaxKind.GeometryShaderKeyword;
                case "PixelShader":
                    return SyntaxKind.PixelShaderKeyword;
                case "VertexShader":
                    return SyntaxKind.VertexShaderKeyword;
                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static SyntaxKind GetPreprocessorKeywordKind(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            switch (text)
            {
                case "define":
                    return SyntaxKind.DefineKeyword;
                case "defined":
                    return SyntaxKind.DefinedKeyword;
                case "if":
                    return SyntaxKind.IfKeyword;
                case "elif":
                    return SyntaxKind.ElifKeyword;
                case "else":
                    return SyntaxKind.ElseKeyword;
                case "endif":
                    return SyntaxKind.EndIfKeyword;
                case "ifdef":
                    return SyntaxKind.IfDefKeyword;
                case "ifndef":
                    return SyntaxKind.IfNDefKeyword;
                case "undef":
                    return SyntaxKind.UndefKeyword;
                case "include":
                    return SyntaxKind.IncludeKeyword;
                case "line":
                    return SyntaxKind.LineKeyword;
                case "error":
                    return SyntaxKind.ErrorKeyword;
                case "pragma":
                    return SyntaxKind.PragmaKeyword;
                case "def":
                    return SyntaxKind.DefKeyword;
                case "message":
                    return SyntaxKind.MessageKeyword;
                case "pack_matrix":
                    return SyntaxKind.PackMatrixKeyword;
                case "warning":
                    return SyntaxKind.WarningKeyword;
                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static bool IsPreprocessorKeyword(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.DefineKeyword:
                case SyntaxKind.DefinedKeyword:
                case SyntaxKind.IfKeyword:
                case SyntaxKind.ElifKeyword:
                case SyntaxKind.ElseKeyword:
                case SyntaxKind.EndIfKeyword:
                case SyntaxKind.IfDefKeyword:
                case SyntaxKind.IfNDefKeyword:
                case SyntaxKind.UndefKeyword:
                case SyntaxKind.IncludeKeyword:
                case SyntaxKind.LineKeyword:
                case SyntaxKind.ErrorKeyword:
                case SyntaxKind.PragmaKeyword:
                case SyntaxKind.DefKeyword:
                case SyntaxKind.MessageKeyword:
                case SyntaxKind.PackMatrixKeyword:
                case SyntaxKind.WarningKeyword:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsDeclarationModifier(SyntaxToken token)
        {
            switch (token.Kind)
            {
                case SyntaxKind.ConstKeyword:
                case SyntaxKind.RowMajorKeyword:
                case SyntaxKind.ColumnMajorKeyword:
                    return true;

                case SyntaxKind.ExportKeyword:
                case SyntaxKind.ExternKeyword:
                case SyntaxKind.InlineKeyword:
                case SyntaxKind.PreciseKeyword:
                case SyntaxKind.SharedKeyword:
                case SyntaxKind.GloballycoherentKeyword:
                case SyntaxKind.GroupsharedKeyword:
                case SyntaxKind.StaticKeyword:
                case SyntaxKind.UniformKeyword:
                case SyntaxKind.VolatileKeyword:
                    return true;

                case SyntaxKind.SNormKeyword:
                case SyntaxKind.UNormKeyword:
                    return true;

                case SyntaxKind.LinearKeyword:
                case SyntaxKind.CentroidKeyword:
                case SyntaxKind.NointerpolationKeyword:
                case SyntaxKind.NoperspectiveKeyword:
                    return true;

                default:
                    switch (token.ContextualKind)
                    {
                        case SyntaxKind.SampleKeyword:
                            return true;

                        default:
                            return false;
                    }
            }
        }

        public static bool IsParameterModifier(SyntaxToken token)
        {
            if (IsDeclarationModifier(token))
                return true;

            switch (token.Kind)
            {
                case SyntaxKind.InKeyword:
                case SyntaxKind.OutKeyword:
                case SyntaxKind.InoutKeyword:
                    return true;

                // Geometry shader modifiers.
                case SyntaxKind.PointKeyword:
                case SyntaxKind.TriangleKeyword:
                case SyntaxKind.TriangleAdjKeyword:
                case SyntaxKind.LineAdjKeyword:
                    return true;

                default:
                    switch (token.ContextualKind)
                    {
                        case SyntaxKind.LineKeyword:
                            return true;

                        default:
                            return false;
                    }
            }
        }

        public static ParameterDirection GetParameterDirection(IList<SyntaxToken> modifiers)
        {
            if ((modifiers.Any(x => x.Kind == SyntaxKind.InKeyword) && modifiers.Any(x => x.Kind == SyntaxKind.OutKeyword))
                || modifiers.Any(x => x.Kind == SyntaxKind.InoutKeyword))
                return ParameterDirection.Inout;

            if (modifiers.Any(x => x.Kind == SyntaxKind.OutKeyword))
                return ParameterDirection.Out;

            return ParameterDirection.In;
        }

        public static bool HaveMatchingSignatures(FunctionSyntax left, FunctionSyntax right)
        {
            // TODO: Whitespace differences will result in a false negative.
            // Instead we should do this test on FunctionSymbol objects.

            if (left.Name.GetUnqualifiedName().Name.Text != right.Name.GetUnqualifiedName().Name.Text)
                return false;

            if (left.ParameterList.Parameters.Count != right.ParameterList.Parameters.Count)
                return false;

            for (var i = 0; i < left.ParameterList.Parameters.Count; i++)
            {
                var leftParameter = left.ParameterList.Parameters[i];
                var rightParameter = right.ParameterList.Parameters[i];

                if (leftParameter.Type.ToStringIgnoringMacroReferences() != rightParameter.Type.ToStringIgnoringMacroReferences())
                    return false;

                if (leftParameter.Declarator.ArrayRankSpecifiers.Count != rightParameter.Declarator.ArrayRankSpecifiers.Count)
                    return false;

                for (var j = 0; j < leftParameter.Declarator.ArrayRankSpecifiers.Count; j++)
                {
                    var leftDimension = leftParameter.Declarator.ArrayRankSpecifiers[j].Dimension;
                    var rightDimension = rightParameter.Declarator.ArrayRankSpecifiers[j].Dimension;
                    if ((leftDimension != null) != (rightDimension != null))
                        return false;
                    if (leftDimension == null)
                        return false;
                    if (leftDimension.ToStringIgnoringMacroReferences() != rightDimension.ToStringIgnoringMacroReferences())
                        return false;
                }

                if (leftParameter.Modifiers.Count != rightParameter.Modifiers.Count)
                    return false;

                for (var j = 0; j < leftParameter.Modifiers.Count; j++)
                    if (leftParameter.Modifiers[j].Text != rightParameter.Modifiers[j].Text)
                        return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the Unicode character can be the starting character of a C# identifier.
        /// </summary>
        /// <param name="ch">The Unicode character.</param>
        public static bool IsIdentifierStartCharacter(char ch)
        {
            return (ch >= 'a' && ch <= 'z')
                   || (ch >= 'A' && ch <= 'Z')
                   || ch == '_';
        }

        /// <summary>
        /// Returns true if the Unicode character can be a part of a C# identifier.
        /// </summary>
        /// <param name="ch">The Unicode character.</param>
        public static bool IsIdentifierPartCharacter(char ch)
        {
            return IsIdentifierStartCharacter(ch) || (ch >= '0' && ch <= '9');
        }
    }
}