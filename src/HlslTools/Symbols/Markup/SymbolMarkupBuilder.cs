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
                case SymbolKind.Struct:
                    break;
                case SymbolKind.Class:
                    break;
                case SymbolKind.Interface:
                    break;
                case SymbolKind.Field:
                    break;
                case SymbolKind.FunctionDeclaration:
                case SymbolKind.FunctionDefinition:
                    markup.AppendFunctionSymbolInfo((FunctionSymbol) symbol);
                    break;
                case SymbolKind.MethodDeclaration:
                case SymbolKind.MethodDefinition:
                    markup.AppendMethodSymbolInfo((MethodSymbol) symbol);
                    break;
                case SymbolKind.Variable:
                    break;
                case SymbolKind.Parameter:
                    markup.AppendParameterSymbolInfo((ParameterSymbol)symbol);
                    break;
                case SymbolKind.Indexer:
                    break;
                case SymbolKind.IntrinsicType:
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

        private static void AppendMethodSymbolInfo(this ICollection<SymbolMarkupToken> markup, MethodSymbol symbol)
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
            markup.AppendType(symbol.ValueType);
            markup.AppendSpace();
            markup.AppendParameterName(symbol.Name);
        }

        private static void AppendParameterName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.ParameterName, text);
        }

        private static void AppendType(this ICollection<SymbolMarkupToken> markup, TypeSymbol symbol)
        {
            markup.AppendName(SymbolMarkupKind.TypeName, symbol.FullName);
        }
    }
}