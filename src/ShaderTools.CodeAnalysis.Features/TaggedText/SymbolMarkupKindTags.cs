using ShaderTools.CodeAnalysis.Symbols.Markup;

namespace ShaderTools.CodeAnalysis
{
    internal static class SymbolMarkupKindTags
    {
        public static string GetTag(SymbolMarkupKind kind)
        {
            switch (kind)
            {
                case SymbolMarkupKind.Whitespace:
                    return TextTags.Space;
                case SymbolMarkupKind.Punctuation:
                    return TextTags.Punctuation;
                case SymbolMarkupKind.Keyword:
                    return TextTags.Keyword;
                case SymbolMarkupKind.FieldName:
                    return TextTags.Field;
                case SymbolMarkupKind.LocalVariableName:
                    return TextTags.Local;
                case SymbolMarkupKind.ConstantBufferVariableName:
                    return TextTags.ConstantBufferField;
                case SymbolMarkupKind.GlobalVariableName:
                    return TextTags.Global;
                case SymbolMarkupKind.ParameterName:
                    return TextTags.Parameter;
                case SymbolMarkupKind.FunctionName:
                    return TextTags.Function;
                case SymbolMarkupKind.MethodName:
                    return TextTags.Method;
                case SymbolMarkupKind.ClassName:
                    return TextTags.Class;
                case SymbolMarkupKind.StructName:
                    return TextTags.Struct;
                case SymbolMarkupKind.InterfaceName:
                    return TextTags.Interface;
                case SymbolMarkupKind.ConstantBufferName:
                    return TextTags.ConstantBuffer;
                case SymbolMarkupKind.IntrinsicTypeName:
                    return TextTags.Keyword;
                case SymbolMarkupKind.NamespaceName:
                    return TextTags.Namespace;
                case SymbolMarkupKind.SemanticName:
                    return TextTags.Semantic;
                case SymbolMarkupKind.TechniqueName:
                    return TextTags.Technique;
                case SymbolMarkupKind.PlainText:
                    return TextTags.Text;
                default:
                    return string.Empty;
            }
        }
    }
}