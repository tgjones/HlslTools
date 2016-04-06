using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HlslTools.Symbols.Markup
{
    internal static class SymbolMarkupBuilder
    {
        public static void AppendSymbol(this ICollection<SymbolMarkupToken> markup, Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Array:
                    break;
                case SymbolKind.Field:
                    markup.AppendFieldSymbolInfo((FieldSymbol) symbol);
                    break;
                case SymbolKind.Function:
                    markup.AppendFunctionSymbolInfo((FunctionSymbol) symbol);
                    break;
                case SymbolKind.Variable:
                    markup.AppendVariableSymbolInfo((VariableSymbol) symbol);
                    break;
                case SymbolKind.Parameter:
                    markup.AppendParameterSymbolInfo((ParameterSymbol) symbol, true);
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
                    markup.AppendTypeDeclaration((TypeSymbol) symbol);
                    break;
                case SymbolKind.Namespace:
                    markup.AppendNamespace((NamespaceSymbol) symbol);
                    break;
                case SymbolKind.Semantic:
                    markup.AppendSemantic((SemanticSymbol) symbol);
                    break;
                case SymbolKind.Technique:
                    markup.AppendTechnique((TechniqueSymbol) symbol);
                    break;
                case SymbolKind.Attribute:
                    markup.AppendAttribute((AttributeSymbol) symbol);
                    break;
                case SymbolKind.ConstantBuffer:
                    markup.AppendConstantBuffer((ConstantBufferSymbol) symbol);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void Append(this ICollection<SymbolMarkupToken> markup, SymbolMarkupKind kind, string text)
        {
            markup.Add(new SymbolMarkupToken(kind, text));
        }

        private static void AppendName(this ICollection<SymbolMarkupToken> markup, SymbolMarkupKind kind, string name)
        {
            var displayName = !string.IsNullOrEmpty(name)
                ? name
                : "?";
            markup.Append(kind, displayName);
        }

        private static void AppendKeyword(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.Append(SymbolMarkupKind.Keyword, text);
        }

        private static void AppendSpace(this ICollection<SymbolMarkupToken> markup)
        {
            markup.Append(SymbolMarkupKind.Whitespace, " ");
        }

        private static void AppendFunctionSymbolInfo(this ICollection<SymbolMarkupToken> markup, FunctionSymbol symbol)
        {
            markup.AppendType(symbol.ReturnType, false);
            markup.AppendSpace();

            if (symbol.Parent is TypeSymbol)
            {
                markup.AppendTypeName((TypeSymbol) symbol.Parent);
                markup.AppendPunctuation(".");
            }

            markup.AppendName(SymbolMarkupKind.FunctionName, symbol.Name);
            markup.AppendParameters(symbol.Parameters);
        }

        private static void AppendParameters(this ICollection<SymbolMarkupToken> markup, ImmutableArray<ParameterSymbol> parameters)
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

                markup.AppendParameterSymbolInfo(parameterSymbol, false);
            }

            markup.AppendPunctuation(")");
        }

        private static void AppendPunctuation(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.Append(SymbolMarkupKind.Punctuation, text);
        }

        private static void AppendParameterSymbolInfo(this ICollection<SymbolMarkupToken> markup, ParameterSymbol symbol, bool includeInfo)
        {
            if (includeInfo)
            {
                markup.AppendPlainText("(parameter)");
                markup.AppendSpace();
            }
            markup.AppendType(symbol.ValueType, false);
            markup.AppendSpace();
            markup.AppendParameterName(symbol.Name);
        }

        private static void AppendFieldSymbolInfo(this ICollection<SymbolMarkupToken> markup, FieldSymbol symbol)
        {
            markup.AppendPlainText("(field)");
            markup.AppendSpace();
            markup.AppendType(symbol.ValueType, true);
            markup.AppendSpace();
            markup.AppendType((TypeSymbol) symbol.Parent, false);
            markup.AppendPunctuation(".");
            markup.AppendName(SymbolMarkupKind.FieldName, symbol.Name);
        }

        private static void AppendVariableSymbolInfo(this ICollection<SymbolMarkupToken> markup, VariableSymbol symbol)
        {
            var isGlobalVariable = symbol.Parent == null || symbol.Parent.Kind == SymbolKind.ConstantBuffer;
            if (symbol.Parent == null)
                markup.AppendPlainText("(global variable)");
            else if (symbol.Parent.Kind == SymbolKind.ConstantBuffer)
                markup.AppendPlainText("(constant buffer variable)");
            else
                markup.AppendPlainText("(local variable)");
            markup.AppendSpace();
            markup.AppendType(symbol.ValueType, true);
            markup.AppendSpace();
            markup.AppendName(isGlobalVariable ? SymbolMarkupKind.GlobalVariableName : SymbolMarkupKind.LocalVariableName, symbol.Name);
        }

        private static void AppendParameterName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.ParameterName, text);
        }

        private static void AppendPlainText(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.PlainText, text);
        }

        private static void AppendType(this ICollection<SymbolMarkupToken> markup, TypeSymbol symbol, bool includeParentScope)
        {
            if (includeParentScope && symbol.Parent != null)
                markup.AppendParentScope(symbol.Parent);

            markup.AppendTypeName(symbol);
        }

        private static void AppendTypeDeclaration(this ICollection<SymbolMarkupToken> markup, TypeSymbol symbol)
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

            markup.AppendTypeName(symbol);
        }

        private static void AppendTypeName(this ICollection<SymbolMarkupToken> markup, TypeSymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Class:
                case SymbolKind.Interface:
                case SymbolKind.Struct:
                    markup.AppendName(SymbolMarkupKind.TypeName, symbol.Name);
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
                    markup.AppendName(SymbolMarkupKind.TypeName, symbol.Name);
                    break;
            }
        }

        private static void AppendSemantic(this ICollection<SymbolMarkupToken> markup, SemanticSymbol symbol)
        {
            markup.AppendPlainText("(semantic)");
            markup.AppendSpace();

            markup.AppendName(SymbolMarkupKind.SemanticName, symbol.Name);
        }

        private static void AppendConstantBuffer(this ICollection<SymbolMarkupToken> markup, ConstantBufferSymbol symbol)
        {
            markup.AppendPlainText("(constant buffer)");
            markup.AppendSpace();

            markup.AppendName(SymbolMarkupKind.TypeName, symbol.Name);
        }

        private static void AppendAttribute(this ICollection<SymbolMarkupToken> markup, AttributeSymbol symbol)
        {
            markup.AppendName(SymbolMarkupKind.FunctionName, symbol.Name);
            markup.AppendParameters(symbol.Parameters);
        }

        private static void AppendTechnique(this ICollection<SymbolMarkupToken> markup, TechniqueSymbol symbol)
        {
            markup.AppendKeyword("technique");
            markup.AppendSpace();

            markup.AppendName(SymbolMarkupKind.TechniqueName, symbol.Name);
        }

        private static void AppendNamespace(this ICollection<SymbolMarkupToken> markup, NamespaceSymbol symbol)
        {
            markup.AppendKeyword("namespace");
            markup.AppendSpace();

            if (symbol.Parent != null)
                markup.AppendParentScope(symbol.Parent);

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
                case SymbolKind.Class:
                    markup.AppendName(SymbolMarkupKind.TypeName, symbol.Name);
                    markup.AppendPunctuation("::");
                    break;
                default:
                    return;
            }
        }
    }
}