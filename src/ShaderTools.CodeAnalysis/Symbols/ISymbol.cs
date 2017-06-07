using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Symbols.Markup;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Symbols
{
    public interface ISymbol
    {
        SymbolKind Kind { get; }
        string Name { get; }
        string Documentation { get; }
        ISymbol Parent { get; }

        SyntaxTreeBase SourceTree { get; }

        /// <summary>
        /// If this symbol is defined in source code, gets the location.
        /// There might be more than one location, for example for separate function declaration and definition.
        /// </summary>
        ImmutableArray<SourceRange> Locations { get; }

        /// <summary>
        /// Get the syntax node(s) where this symbol was declared in source. Some symbols (for example,
        /// partial classes) may be defined in more than one location. This property should return
        /// one or more syntax nodes only if the symbol was declared in source code and also was
        /// not implicitly declared (see the IsImplicitlyDeclared property). 
        /// 
        /// <para>
        /// Note that for namespace symbol, the declaring syntax might be declaring a nested namespace.
        /// For example, the declaring syntax node for N1 in "namespace N1.N2 {...}" is the entire
        /// NamespaceDeclarationSyntax for N1.N2. For the global namespace, the declaring syntax will
        /// be the CompilationUnitSyntax.
        /// </para>
        /// </summary>
        /// <returns>
        /// The syntax node(s) that declared the symbol. If the symbol was declared in metadata
        /// or was implicitly declared, returns an empty read-only array.
        /// </returns>
        ImmutableArray<SyntaxNodeBase> DeclaringSyntaxNodes { get; }

        SymbolMarkup ToMarkup(SymbolDisplayFormat format = SymbolDisplayFormat.QuickInfo);
    }

    public enum SymbolDisplayFormat
    {
        QuickInfo,
        MinimallyQualified,
        MinimallyQualifiedWithoutParameters,
        NavigateTo,
        NavigationBar
    }
}
