using System;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Symbols;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.SymbolSearch
{
    internal struct SymbolSpan : IEquatable<SymbolSpan>
    {
        public SymbolSpan(SymbolSpanKind kind, Symbol symbol, SourceRange sourceRange, TextSpan span)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            Kind = kind;
            Symbol = symbol;
            SourceRange = sourceRange;
            Span = span;
        }

        public SymbolSpanKind Kind { get; }

        public Symbol Symbol { get; }

        public SourceRange SourceRange { get; }
        public TextSpan Span { get; }

        public bool Equals(SymbolSpan other)
        {
            return Kind == other.Kind &&
                   Symbol == other.Symbol &&
                   Span == other.Span;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SymbolSpan?;
            return other.HasValue && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Kind;
                hashCode = (hashCode * 397) ^ Symbol.GetHashCode();
                hashCode = (hashCode * 397) ^ Span.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SymbolSpan left, SymbolSpan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SymbolSpan left, SymbolSpan right)
        {
            return !left.Equals(right);
        }

        public static SymbolSpan CreateReference(Symbol symbol, SourceRange sourceRange, TextSpan span)
        {
            return new SymbolSpan(SymbolSpanKind.Reference, symbol, sourceRange, span);
        }

        public static SymbolSpan CreateDefinition(Symbol symbol, SourceRange sourceRange, TextSpan span)
        {
            return new SymbolSpan(SymbolSpanKind.Definition, symbol, sourceRange, span);
        }
    }
}