using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;
using ShaderTools.VisualStudio.Hlsl.Glyphs;
using ShaderTools.VisualStudio.Hlsl.Util.SyntaxOutput;

namespace ShaderTools.VisualStudio.Hlsl.Navigation.NavigateTo
{
    internal sealed class NavigateToVisitor : SyntaxVisitor
    {
        private const string GlobalScope = "(Global Scope)";

        private readonly string _searchValue;
        private readonly ITextSnapshot _snapshot;
        private readonly IWpfTextView _textView;
        private readonly INavigateToCallback _callback;
        private readonly IBufferGraphFactoryService _bufferGraphFactoryService;
        private readonly INavigateToItemDisplayFactory _displayFactory;
        private readonly DispatcherGlyphService _glyphService;
        private readonly CancellationToken _cancellationToken;

        public NavigateToVisitor(
            string searchValue, 
            ITextSnapshot snapshot, IWpfTextView textView,
            INavigateToCallback callback, 
            IBufferGraphFactoryService bufferGraphFactoryService,
            INavigateToItemDisplayFactory displayFactory, 
            DispatcherGlyphService glyphService,
            CancellationToken cancellationToken)
        {
            _searchValue = searchValue;
            _snapshot = snapshot;
            _textView = textView;
            _callback = callback;
            _bufferGraphFactoryService = bufferGraphFactoryService;
            _displayFactory = displayFactory;
            _glyphService = glyphService;
            _cancellationToken = cancellationToken;
        }

        public override void Visit(SyntaxNode node)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            base.Visit(node);
        }

        public override void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            foreach (var childNode in node.Declarations)
                Visit(childNode);
        }

        public override void VisitNamespace(NamespaceSyntax node)
        {
            ProcessItem(node.Name, node.Name.Text, node.GetTextSpanSafe(), NavigateToItemKind.Module, node.Parent, Glyph.Namespace);
            foreach (var declaration in node.Declarations)
                Visit(declaration);
        }

        public override void VisitTypeDeclarationStatement(TypeDeclarationStatementSyntax node)
        {
            Visit(node.Type);
        }

        public override void VisitClassType(ClassTypeSyntax node)
        {
            ProcessItem(node.Name, node.Name.Text, node.GetTextSpanSafe(), NavigateToItemKind.Class, node.Parent, Glyph.Class);
            foreach (var member in node.Members)
                Visit(member);
        }

        public override void VisitConstantBuffer(ConstantBufferSyntax node)
        {
            ProcessItem(node.Name, node.Name.Text, node.GetTextSpanSafe(), NavigateToItemKind.Structure, node.Parent, Glyph.ConstantBuffer);
            foreach (var member in node.Declarations)
                Visit(member);
        }

        public override void VisitInterfaceType(InterfaceTypeSyntax node)
        {
            ProcessItem(node.Name, node.Name.Text, node.GetTextSpanSafe(), NavigateToItemKind.Interface, node.Parent, Glyph.Interface);
            foreach (var member in node.Methods)
                Visit(member);
        }

        public override void VisitStructType(StructTypeSyntax node)
        {
            ProcessItem(node.Name, node.Name.Text, node.GetTextSpanSafe(), NavigateToItemKind.Structure, node.Parent, Glyph.Struct);
            foreach (var member in node.Fields)
                Visit(member);
        }

        public override void VisitFunctionDeclaration(FunctionDeclarationSyntax node)
        {
            ProcessItem(node.Name.GetUnqualifiedName().Name, node.GetDescription(false, false), node.GetTextSpanSafe(), NavigateToItemKind.Method, node.Parent, Glyph.Function);
        }

        public override void VisitFunctionDefinition(FunctionDefinitionSyntax node)
        {
            ProcessItem(node.Name.GetUnqualifiedName().Name, node.GetDescription(false, false), node.GetTextSpanSafe(), NavigateToItemKind.Method, node.Parent, Glyph.Function);
        }

        public override void VisitTechnique(TechniqueSyntax node)
        {
            ProcessItem(node.Name, node.Name.Text, node.GetTextSpanSafe(), NavigateToItemKind.Structure, node.Parent, Glyph.Technique);
        }

        public override void VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node)
        {
            var variables = node.Declaration.Variables.ToList();

            var lastDeclarator = variables.Last();
            var firstDeclarator = variables.First();

            if (!node.GetFirstToken().Span.IsInRootFile)
                return;

            var declarationRootSpan = node.Declaration.GetTextSpanRoot();
            var firstDeclaratorTextSpan = TextSpan.FromBounds(declarationRootSpan.SourceText, declarationRootSpan.Start, firstDeclarator.GetTextSpanRoot().End);

            if (firstDeclarator == lastDeclarator)
                firstDeclaratorTextSpan = TextSpan.FromBounds(declarationRootSpan.SourceText, firstDeclaratorTextSpan.Start, node.GetTextSpanRoot().End);

            ProcessItem(firstDeclarator.Identifier, firstDeclarator.Identifier.Text, firstDeclaratorTextSpan, NavigateToItemKind.Field, node.Parent, Glyph.Variable);

            foreach (var declarator in variables.Skip(1))
            {
                var declaratorTextSpan = declarator.GetTextSpanRoot();
                if (declarator == lastDeclarator)
                    declaratorTextSpan = TextSpan.FromBounds(declarationRootSpan.SourceText, declaratorTextSpan.Start, node.GetTextSpanRoot().End);
                ProcessItem(declarator.Identifier, declarator.Identifier.Text, declaratorTextSpan, NavigateToItemKind.Field, node.Parent, Glyph.Variable);
            }
        }

        private static string GetDescription(SyntaxNode node)
        {
            if (node.Kind == SyntaxKind.TypeDeclarationStatement)
                node = node.Parent;

            switch (node.Kind)
            {
                case SyntaxKind.CompilationUnit:
                    return GlobalScope;

                case SyntaxKind.Namespace:
                    return "namespace " + node.GetFullyQualifiedName();

                case SyntaxKind.ClassType:
                    return "class " + node.GetFullyQualifiedName();

                case SyntaxKind.InterfaceType:
                    return "interface " + node.GetFullyQualifiedName();

                case SyntaxKind.StructType:
                    return "struct " + node.GetFullyQualifiedName();

                case SyntaxKind.ConstantBufferDeclaration:
                    return "constant buffer " + node.GetFullyQualifiedName();

                default:
                    return string.Empty;
            }
        }

        private void ProcessItem(SyntaxToken name, string nameText, TextSpan nodeSpan, string kind, SyntaxNode parent, Glyph icon)
        {
            if (!nodeSpan.IsInRootFile)
                return;

            if (name == null)
                return;

            if (!Contains(name.Text, _searchValue, StringComparison.CurrentCultureIgnoreCase))
                return;

            var description = GetDescription(parent);

            Action navigateCallback = () => _textView.NavigateTo(
                _bufferGraphFactoryService,
                new SnapshotSpan(_snapshot, nodeSpan.Start, nodeSpan.Length),
                new SnapshotSpan(_snapshot, name.Span.Start, name.Span.Length));

            var itemDisplay = new NavigateToItemDisplay(icon.GetIcon(_glyphService),
                nameText, description, null, navigateCallback);

            _callback.AddItem(new NavigateToItem(
                name.Text,
                kind,
                HlslConstants.LanguageName,
                name.Text, 
                itemDisplay,
                (name.Text == _searchValue) ? MatchKind.Exact : MatchKind.Substring,
                _displayFactory));
        }

        private static bool Contains(string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }
}