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
                    break;
                case SymbolKind.Function:
                    markup.AppendFunctionSymbolInfo((FunctionSymbol) symbol);
                    break;
                case SymbolKind.Method:
                    markup.AppendMethodSymbolInfo((FunctionSymbol) symbol);
                    break;
                case SymbolKind.Variable:
                    break;
                case SymbolKind.Parameter:
                    markup.AppendParameterSymbolInfo((ParameterSymbol) symbol);
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

        private static void AppendMethodSymbolInfo(this ICollection<SymbolMarkupToken> markup, FunctionSymbol symbol)
        {
            markup.AppendType(symbol.ReturnType);
            markup.AppendSpace();
            markup.AppendName(SymbolMarkupKind.MethodName, symbol.Name);
            markup.AppendParameters(symbol.Parameters);
        }

        private static void AppendFunctionSymbolInfo(this ICollection<SymbolMarkupToken> markup, FunctionSymbol symbol)
        {
            markup.AppendType(symbol.ReturnType);
            markup.AppendSpace();
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

                markup.AppendParameterSymbolInfo(parameterSymbol);               
            }

            markup.AppendPunctuation(")");
        }

        private static void AppendPunctuation(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.Append(SymbolMarkupKind.Punctuation, text);
        }

        private static void AppendParameterSymbolInfo(this ICollection<SymbolMarkupToken> markup, ParameterSymbol symbol)
        {
            markup.AppendPlainText("(parameter)");
            markup.AppendSpace();
            markup.AppendType(symbol.ValueType);
            markup.AppendSpace();
            markup.AppendParameterName(symbol.Name);
        }

        private static void AppendParameterName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.ParameterName, text);
        }

        private static void AppendPlainText(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.PlainText, text);
        }

        private static void AppendType(this ICollection<SymbolMarkupToken> markup, TypeSymbol symbol)
        {
            if (symbol.Parent != null)
                markup.AppendParentScope(symbol.Parent);

            markup.AppendName(SymbolMarkupKind.TypeName, symbol.Name);
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

            markup.AppendName(SymbolMarkupKind.TypeName, symbol.Name);
        }

        private static void AppendSemantic(this ICollection<SymbolMarkupToken> markup, SemanticSymbol symbol)
        {
            markup.AppendPlainText("(semantic)");
            markup.AppendSpace();

            markup.AppendName(SymbolMarkupKind.SemanticName, symbol.Name);
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