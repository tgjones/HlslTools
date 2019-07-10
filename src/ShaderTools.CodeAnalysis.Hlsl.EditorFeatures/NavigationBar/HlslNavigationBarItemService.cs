using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Editor.Extensibility.NavigationBar;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Navigation;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.NavigationBar
{
    [ExportLanguageService(typeof(INavigationBarItemService), LanguageNames.Hlsl), Shared]
    internal sealed class HlslNavigationBarItemService : INavigationBarItemService
    {
        public async Task<IList<NavigationBarItem>> GetItemsAsync(Document document, CancellationToken cancellationToken)
        {
            var typesInFile = await GetTypesInFileAsync(document, cancellationToken).ConfigureAwait(false);
            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var membersInTypes = GetMembersInTypes(tree, typesInFile, cancellationToken);

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var globalMembers = GetGlobalMembers(semanticModel, cancellationToken).ToList();

            if (globalMembers.Any())
            {
                var globalMemberNavigationItems = globalMembers
                    .SelectMany(x => CreateItems(x, null, tree, cancellationToken))
                    .ToList();

                if (globalMemberNavigationItems.Any())
                {
                    globalMemberNavigationItems.Sort((x, y) =>
                    {
                        var textComparison = x.Text.CompareTo(y.Text);
                        return textComparison != 0 ? textComparison : x.Grayed.CompareTo(y.Grayed);
                    });

                    var firstGlobalScopeIndex = globalMemberNavigationItems
                        .Cast<NavigationBarSymbolItem>()
                        .Min(x => x.NavigationSpan.Start);

                    membersInTypes.Insert(0, new NavigationBarSymbolItem(
                        "(Global Scope)",
                        Glyph.Namespace,
                        new[] { new TextSpan(0, document.SourceText.Length) },
                        new TextSpan(firstGlobalScopeIndex, 0),
                        globalMemberNavigationItems));
                }
            }

            return membersInTypes;
        }

        private async Task<IEnumerable<INamespaceOrTypeSymbol>> GetTypesInFileAsync(Document document, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            return GetTypesInFile(semanticModel, cancellationToken);
        }

        private static IEnumerable<INamespaceOrTypeSymbol> GetTypesInFile(SemanticModelBase semanticModel, CancellationToken cancellationToken)
        {
            //using (Logger.LogBlock(FunctionId.NavigationBar_ItemService_GetTypesInFile_CSharp, cancellationToken))
            {
                var types = new HashSet<INamespaceOrTypeSymbol>();
                var nodesToVisit = new Stack<SyntaxNodeBase>();

                nodesToVisit.Push(semanticModel.SyntaxTree.Root);

                while (!nodesToVisit.IsEmpty())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return SpecializedCollections.EmptyEnumerable<INamespaceOrTypeSymbol>();
                    }

                    var node = nodesToVisit.Pop();
                    var type = GetType(semanticModel, node);

                    if (type != null)
                    {
                        types.Add((INamespaceOrTypeSymbol) type);
                    }

                    if (node is FunctionSyntax ||
                        node is VariableDeclarationSyntax ||
                        (node is StatementSyntax && !(node is TypeDeclarationStatementSyntax)) ||
                        node is ExpressionSyntax)
                    {
                        // quick bail out to prevent us from visiting every node in current file
                        continue;
                    }

                    foreach (var child in node.ChildNodes)
                    {
                        if (!child.IsToken)
                        {
                            nodesToVisit.Push(child);
                        }
                    }
                }

                return types;
            }
        }

        private static IEnumerable<ISymbol> GetGlobalMembers(SemanticModelBase semanticModel, CancellationToken cancellationToken)
        {
            //using (Logger.LogBlock(FunctionId.NavigationBar_ItemService_GetTypesInFile_CSharp, cancellationToken))
            {
                var members = new HashSet<ISymbol>();
                var nodesToVisit = new Stack<SyntaxNodeBase>();

                nodesToVisit.Push(semanticModel.SyntaxTree.Root);

                while (!nodesToVisit.IsEmpty())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return SpecializedCollections.EmptyEnumerable<ISymbol>();
                    }

                    var node = nodesToVisit.Pop();
                    var member = GetGlobalMember(semanticModel, node);

                    if (member != null)
                    {
                        members.Add(member);
                    }

                    if (node is FunctionSyntax ||
                        node is TypeDeclarationStatementSyntax ||
                        node is ExpressionSyntax ||
                        node is TechniqueSyntax ||
                        node is ConstantBufferSyntax)
                    {
                        // quick bail out to prevent us from visiting every node in current file
                        continue;
                    }

                    foreach (var child in node.ChildNodes)
                    {
                        if (!child.IsToken)
                        {
                            nodesToVisit.Push(child);
                        }
                    }
                }

                return members;
            }
        }

        private static ISymbol GetType(SemanticModelBase semanticModel, SyntaxNodeBase node)
        {
            switch (node)
            {
                case ConstantBufferSyntax t: return semanticModel.GetDeclaredSymbol(t);
                case NamespaceSyntax t: return semanticModel.GetDeclaredSymbol(t);
                case TechniqueSyntax t: return semanticModel.GetDeclaredSymbol(t);
                case TypeDefinitionSyntax t: return semanticModel.GetDeclaredSymbol(t);
            }

            return null;
        }

        private static ISymbol GetGlobalMember(SemanticModelBase semanticModel, SyntaxNodeBase node)
        {
            switch (node)
            {
                case FunctionSyntax t: return semanticModel.GetDeclaredSymbol(t);
                case VariableDeclaratorSyntax t: return semanticModel.GetDeclaredSymbol(t);
            }

            return null;
        }

        private IList<NavigationBarItem> GetMembersInTypes(
            SyntaxTreeBase tree, 
            IEnumerable<INamespaceOrTypeSymbol> types, 
            CancellationToken cancellationToken)
        {
            //using (Logger.LogBlock(FunctionId.NavigationBar_ItemService_GetMembersInTypes_CSharp, cancellationToken))
            {
                var items = new List<NavigationBarItem>();

                foreach (var type in types)
                {
                    var memberItems = new List<NavigationBarItem>();
                    foreach (var member in type.GetMembers())
                    {
                        if (member.Kind == SymbolKind.Struct || member.Kind == SymbolKind.Interface)
                        {
                            continue;
                        }

                        memberItems.AddRange(CreateItems(
                            member,
                            null,
                            tree,
                            cancellationToken));
                    }

                    memberItems.Sort((x, y) =>
                    {
                        var textComparison = x.Text.CompareTo(y.Text);
                        return textComparison != 0 ? textComparison : x.Grayed.CompareTo(y.Grayed);
                    });

                    var typeItems = CreateItems(type, memberItems, tree, cancellationToken);

                    items.AddRange(typeItems);
                }

                items.Sort((x1, x2) => x1.Text.CompareTo(x2.Text));
                return items;
            }
        }

        private static IList<NavigationBarItem> CreateItems(ISymbol symbol, IList<NavigationBarItem> childItems, SyntaxTreeBase tree, CancellationToken cancellationToken)
        {
            var result = new List<NavigationBarItem>();

            if (symbol.Locations.Length == 0)
            {
                return result;
            }

            var locationsLength = symbol.Kind == SymbolKind.Function
                ? symbol.Locations.Length
                : 1;

            for (var i = 0; i < locationsLength; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return SpecializedCollections.EmptyList<NavigationBarItem>();
                }

                // TODO: This is a hack, it assumes too much about ISymbol's properties.
                var nameSourceRange = symbol.Locations[i];
                var node = (SyntaxNode) symbol.DeclaringSyntaxNodes[i];

                var nameFileSpan = tree.GetSourceFileSpan(nameSourceRange);
                if (!nameFileSpan.IsInRootFile)
                {
                    continue;
                }

                var nodeFileSpan = node.GetTextSpanSafe();
                if (nodeFileSpan == null || !nodeFileSpan.Value.IsInRootFile)
                {
                    continue;
                }

                var nodeSpan = nodeFileSpan.Value.Span;
                if (symbol.Kind == SymbolKind.Field || symbol.Kind == SymbolKind.Variable)
                {
                    nodeSpan = GetVariableDeclarationSpan(tree, node, nodeFileSpan.Value);
                }

                result.Add(new NavigationBarSymbolItem(
                    symbol.ToDisplayString(SymbolDisplayFormat.NavigationBar),
                    symbol.GetGlyph(),
                    SpecializedCollections.SingletonList(nodeSpan),
                    nameFileSpan.Span,
                    childItems));
            }

            return result;
        }

        /// <summary>
        /// Computes a span for a given field symbol, expanding to the outer 
        /// </summary>
        private static TextSpan GetVariableDeclarationSpan(SyntaxTreeBase tree, SyntaxNode node, SourceFileSpan nodeFileSpan)
        {
            int spanStart = nodeFileSpan.Span.Start;
            int spanEnd = nodeFileSpan.Span.End;

            var fieldDeclaration = node.GetAncestor<VariableDeclarationStatementSyntax>();
            if (fieldDeclaration != null)
            {
                var variables = fieldDeclaration.Declaration.Variables;

                var fieldDeclarationFileSpan = tree.GetSourceFileSpan(fieldDeclaration.SourceRange);
                if (fieldDeclarationFileSpan.IsInRootFile)
                {
                    if (variables.FirstOrDefault() == node)
                    {
                        spanStart = fieldDeclarationFileSpan.Span.Start;
                    }

                    if (variables.LastOrDefault() == node)
                    {
                        spanEnd = fieldDeclarationFileSpan.Span.End;
                    }
                }
            }

            return TextSpan.FromBounds(spanStart, spanEnd);
        }

        public bool ShowItemGrayedIfNear(NavigationBarItem item)
        {
            return true;
        }

        public void NavigateToItem(Document document, NavigationBarItem item, ITextView view, CancellationToken cancellationToken)
        {
            var symbolItem = (NavigationBarSymbolItem) item;

            var navigationService = document.Workspace.Services.GetService<IDocumentNavigationService>();

            navigationService.TryNavigateToPosition(
                document.Workspace, 
                document.Id, 
                symbolItem.NavigationSpan.Start);
        }
    }
}
