using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Symbols.Markup;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols.Markup
{
    internal static class SymbolMarkupExtensions
    {
        public static void AppendSymbol(this ICollection<SymbolMarkupToken> markup, Symbol symbol, SymbolDisplayFormat format)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Array:
                    break;
                case SymbolKind.Field:
                    markup.AppendFieldSymbolInfo((FieldSymbol) symbol, format);
                    break;
                case SymbolKind.Function:
                    markup.AppendFunctionSymbolInfo((FunctionSymbol) symbol, format);
                    break;
                case SymbolKind.Variable:
                    markup.AppendVariableSymbolInfo((VariableSymbol) symbol, format);
                    break;
                case SymbolKind.Parameter:
                    markup.AppendParameterSymbolInfo((ParameterSymbol) symbol, true, format);
                    break;
                case SymbolKind.Indexer:
                    break;
                case SymbolKind.IntrinsicScalarType:
                case SymbolKind.IntrinsicVectorType:
                case SymbolKind.IntrinsicMatrixType:
                case SymbolKind.IntrinsicObjectType:
                case SymbolKind.Class:
                case SymbolKind.Struct:
                case SymbolKind.Interface:
                    markup.AppendTypeDeclaration((TypeSymbol) symbol, format);
                    break;
                case SymbolKind.Namespace:
                    markup.AppendNamespace((NamespaceSymbol) symbol, format);
                    break;
                case SymbolKind.Semantic:
                    markup.AppendSemantic((SemanticSymbol) symbol);
                    break;
                case SymbolKind.Technique:
                    markup.AppendTechnique((TechniqueSymbol) symbol, format);
                    break;
                case SymbolKind.TypeAlias:
                    markup.AppendTypeAlias((TypeAliasSymbol) symbol, format);
                    break;
                case SymbolKind.Attribute:
                    markup.AppendAttribute((AttributeSymbol) symbol, format);
                    break;
                case SymbolKind.ConstantBuffer:
                    markup.AppendConstantBuffer((ConstantBufferSymbol) symbol, format);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(symbol), symbol.Kind.ToString());
            }
        }

        private static void AppendFunctionSymbolInfo(this ICollection<SymbolMarkupToken> markup, FunctionSymbol symbol, SymbolDisplayFormat format)
        {
            if (format != SymbolDisplayFormat.NavigateTo && format != SymbolDisplayFormat.NavigationBar)
            {
                markup.AppendType(symbol.ReturnType, false);
                markup.AppendSpace();

                if (symbol.Parent is TypeSymbol)
                {
                    markup.AppendTypeName((TypeSymbol) symbol.Parent);
                    markup.AppendPunctuation(".");
                }
            }

            if (symbol.IsNumericConstructor)
                markup.AppendKeyword(symbol.Name);
            else
                markup.AppendName(SymbolMarkupKind.FunctionName, symbol.Name);

            if (format == SymbolDisplayFormat.MinimallyQualifiedWithoutParameters)
                return;

            markup.AppendParameters(symbol.Parameters, format);
        }

        private static void AppendParameters(this ICollection<SymbolMarkupToken> markup, ImmutableArray<ParameterSymbol> parameters, SymbolDisplayFormat format)
        {
            markup.AppendPunctuation("(");

            var isFirst = true;
            foreach (var parameterSymbol in parameters)
            {
                if (isFirst)
                    isFirst = false;
                else
                {
                    markup.AppendPunctuation(",");
                    markup.AppendSpace();
                }

                markup.AppendParameterSymbolInfo(parameterSymbol, false, format);
            }

            markup.AppendPunctuation(")");
        }

        private static void AppendParameterSymbolInfo(this ICollection<SymbolMarkupToken> markup, ParameterSymbol symbol, bool includeInfo, SymbolDisplayFormat format)
        {
            if (includeInfo && format == SymbolDisplayFormat.QuickInfo)
            {
                markup.AppendPlainText("(parameter)");
                markup.AppendSpace();
            }

            if (symbol.Direction == ParameterDirection.Inout)
            {
                markup.AppendKeyword("inout");
                markup.AppendSpace();
            }
            else if (symbol.Direction == ParameterDirection.Out)
            {
                markup.AppendKeyword("out");
                markup.AppendSpace();
            }

            markup.AppendType(symbol.ValueType, false);

            if (format != SymbolDisplayFormat.NavigateTo)
            {
                markup.AppendSpace();
                markup.AppendParameterName(symbol.Name);

                if (symbol.HasDefaultValue)
                {
                    markup.AppendSpace();
                    markup.AppendPlainText(symbol.DefaultValueText);
                }
            }
        }

        private static void AppendFieldSymbolInfo(this ICollection<SymbolMarkupToken> markup, FieldSymbol symbol, SymbolDisplayFormat format)
        {
            if (format == SymbolDisplayFormat.QuickInfo)
            {
                markup.AppendPlainText("(field)");
                markup.AppendSpace();

                markup.AppendType(symbol.ValueType, true);
                markup.AppendSpace();

                markup.AppendType((TypeSymbol) symbol.Parent, false);
                markup.AppendPunctuation(".");
            }

            markup.AppendName(SymbolMarkupKind.FieldName, symbol.Name);
        }

        private enum VariableType
        {
            Local,
            ConstantBuffer,
            Global
        }

        private static void AppendVariableSymbolInfo(this ICollection<SymbolMarkupToken> markup, VariableSymbol symbol, SymbolDisplayFormat format)
        {
            VariableType variableType;
            if (symbol.Parent == null)
                variableType = VariableType.Global;
            else if (symbol.Parent.Kind == SymbolKind.ConstantBuffer)
                variableType = VariableType.ConstantBuffer;
            else
                variableType = VariableType.Local;

            if (format == SymbolDisplayFormat.QuickInfo)
            {
                switch (variableType)
                {
                    case VariableType.Local:
                        markup.AppendPlainText("(local variable)");
                        break;
                    case VariableType.ConstantBuffer:
                        markup.AppendPlainText("(constant buffer variable)");
                        break;
                    case VariableType.Global:
                        markup.AppendPlainText("(global variable)");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                markup.AppendSpace();
                markup.AppendType(symbol.ValueType, true);
                markup.AppendSpace();
            }

            switch (variableType)
            {
                case VariableType.Local:
                    markup.AppendName(SymbolMarkupKind.LocalVariableName, symbol.Name);
                    break;
                case VariableType.ConstantBuffer:
                    markup.AppendName(SymbolMarkupKind.ConstantBufferVariableName, symbol.Name);
                    break;
                case VariableType.Global:
                    markup.AppendName(SymbolMarkupKind.GlobalVariableName, symbol.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void AppendType(this ICollection<SymbolMarkupToken> markup, TypeSymbol symbol, bool includeParentScope)
        {
            if (includeParentScope && symbol.Parent != null)
                markup.AppendParentScope(symbol.Parent);

            markup.AppendTypeName(symbol);
        }

        private static void AppendTypeDeclaration(this ICollection<SymbolMarkupToken> markup, TypeSymbol symbol, SymbolDisplayFormat format)
        {
            if (format != SymbolDisplayFormat.NavigationBar)
            {
                switch (symbol.Kind)
                {
                    case SymbolKind.Class:
                        markup.AppendKeyword("class");
                        markup.AppendSpace();
                        break;
                    case SymbolKind.Interface:
                        markup.AppendKeyword("interface");
                        markup.AppendSpace();
                        break;
                    case SymbolKind.Struct:
                        markup.AppendKeyword("struct");
                        markup.AppendSpace();
                        break;
                }

                if (symbol.Parent != null)
                    markup.AppendParentScope(symbol.Parent);
            }

            markup.AppendTypeName(symbol);
        }

        private static void AppendTypeName(this ICollection<SymbolMarkupToken> markup, TypeSymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Class:
                    markup.AppendName(SymbolMarkupKind.ClassName, symbol.Name);
                    break;
                case SymbolKind.Interface:
                    markup.AppendName(SymbolMarkupKind.InterfaceName, symbol.Name);
                    break;
                case SymbolKind.Struct:
                    markup.AppendName(SymbolMarkupKind.StructName, symbol.Name);
                    break;
                case SymbolKind.IntrinsicMatrixType:
                case SymbolKind.IntrinsicObjectType:
                case SymbolKind.IntrinsicScalarType:
                case SymbolKind.IntrinsicVectorType:
                    // TODO: Need something better for templated predefined objects.
                    markup.AppendName(SymbolMarkupKind.Keyword, symbol.Name);
                    break;
                case SymbolKind.Array:
                    // TODO: Could separate out square brackets as punctuation.
                    markup.AppendName(SymbolMarkupKind.IntrinsicTypeName, symbol.Name);
                    break;
            }
        }

        private static void AppendSemantic(this ICollection<SymbolMarkupToken> markup, SemanticSymbol symbol)
        {
            markup.AppendPlainText("(semantic)");
            markup.AppendSpace();

            markup.AppendName(SymbolMarkupKind.SemanticName, symbol.Name);
        }

        private static void AppendConstantBuffer(this ICollection<SymbolMarkupToken> markup, ConstantBufferSymbol symbol, SymbolDisplayFormat format)
        {
            if (format == SymbolDisplayFormat.QuickInfo)
            {
                markup.AppendPlainText("(constant buffer)");
                markup.AppendSpace();
            }

            markup.AppendName(SymbolMarkupKind.ConstantBufferVariableName, symbol.Name);
        }

        private static void AppendAttribute(this ICollection<SymbolMarkupToken> markup, AttributeSymbol symbol, SymbolDisplayFormat format)
        {
            markup.AppendName(SymbolMarkupKind.FunctionName, symbol.Name);
            markup.AppendParameters(symbol.Parameters, format);
        }

        private static void AppendTechnique(this ICollection<SymbolMarkupToken> markup, TechniqueSymbol symbol, SymbolDisplayFormat format)
        {
            if (format == SymbolDisplayFormat.QuickInfo)
            {
                markup.AppendKeyword("technique");
                markup.AppendSpace();
            }

            markup.AppendName(SymbolMarkupKind.TechniqueName, symbol.Name);
        }

        private static void AppendTypeAlias(this ICollection<SymbolMarkupToken> markup, TypeAliasSymbol symbol, SymbolDisplayFormat format)
        {
            if (format == SymbolDisplayFormat.QuickInfo)
            {
                markup.AppendKeyword("typedef");
                markup.AppendSpace();

                markup.AppendType(symbol.ValueType, true);
                markup.AppendSpace();
            }

            markup.AppendName(SymbolMarkupKind.GlobalVariableName, symbol.Name);
        }

        private static void AppendNamespace(this ICollection<SymbolMarkupToken> markup, NamespaceSymbol symbol, SymbolDisplayFormat format)
        {
            if (format == SymbolDisplayFormat.QuickInfo)
            {
                markup.AppendKeyword("namespace");
                markup.AppendSpace();

                if (symbol.Parent != null)
                    markup.AppendParentScope(symbol.Parent);
            }

            markup.AppendName(SymbolMarkupKind.NamespaceName, symbol.Name);
        }

        private static void AppendParentScope(this ICollection<SymbolMarkupToken> markup, Symbol symbol)
        {
            if (symbol.Parent != null)
                AppendParentScope(markup, symbol.Parent);

            switch (symbol.Kind)
            {
                case SymbolKind.Namespace:
                    markup.AppendName(SymbolMarkupKind.NamespaceName, symbol.Name);
                    markup.AppendPunctuation("::");
                    break;
                case SymbolKind.Struct:
                    markup.AppendName(SymbolMarkupKind.StructName, symbol.Name);
                    markup.AppendPunctuation("::");
                    break;
                case SymbolKind.Class:
                    markup.AppendName(SymbolMarkupKind.ClassName, symbol.Name);
                    markup.AppendPunctuation("::");
                    break;
                default:
                    return;
            }
        }

        private static void AppendParameterName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.ParameterName, text);
        }
    }
}