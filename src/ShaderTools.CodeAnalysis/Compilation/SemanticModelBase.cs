using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Compilation
{
    public abstract class SemanticModelBase
    {
        public abstract SyntaxTreeBase SyntaxTree { get; }

        public abstract string Language { get; }

        public abstract IEnumerable<Diagnostic> GetDiagnostics();

        public abstract ISymbol GetDeclaredSymbol(SyntaxNodeBase node);
        public abstract SymbolInfo GetSymbolInfo(SyntaxNodeBase node);

        public abstract TypeInfo GetTypeInfo(SyntaxNodeBase node);

        public abstract IEnumerable<ISymbol> LookupSymbols(SourceLocation position);
    }
}
