using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Editor.VisualStudio.Core.Glyphs;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.SyntaxOutput;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation
{
    internal sealed class NavigationTargetsVisitor : SyntaxVisitor<IEnumerable<EditorNavigationTarget>>
    {
        private readonly DispatcherGlyphService _glyphService;
        private readonly CancellationToken _cancellationToken;
        private readonly ITextSnapshot _snapshot;

        public NavigationTargetsVisitor(ITextSnapshot snapshot, DispatcherGlyphService glyphService, CancellationToken cancellationToken)
        {
            _glyphService = glyphService;
            _cancellationToken = cancellationToken;
            _snapshot = snapshot;
        }

        public override IEnumerable<EditorNavigationTarget> Visit(SyntaxNode node)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            return base.Visit(node);
        }

        public IEnumerable<EditorTypeNavigationTarget> GetTargets(CompilationUnitSyntax node)
        {
            var topLevelTargets = node.ChildNodes
                .Where(x => !x.IsToken)
                .Cast<SyntaxNode>()
                .SelectMany(x => Visit(x) ?? Enumerable.Empty<EditorNavigationTarget>())
                .ToList();

            var topLevelTypes = topLevelTargets
                .OfType<EditorTypeNavigationTarget>()
                .ToList();

            var topLevelMembers = topLevelTargets
                .Where(x => !(x is EditorTypeNavigationTarget))
                .ToList();

            yield return new EditorTypeNavigationTarget("(Global Scope)",
                new SnapshotSpan(_snapshot, 0, 0),
                topLevelMembers.Any() ? topLevelMembers.First().Seek : new SnapshotSpan(_snapshot, 0, 0),
                Glyph.TopLevel, Glyph.TopLevel.GetImageSource(_glyphService),
                topLevelMembers);

            foreach (var childTarget in topLevelTypes)
                yield return childTarget;
        }

        public override IEnumerable<EditorNavigationTarget> VisitNamespace(NamespaceSyntax node)
        {
            yield return CreateTypeTarget(node.Name, node.GetTextSpanSafe(), Glyph.Namespace,
                node.Declarations.Where(x => !IsTypeTarget(x.Kind)));

            foreach (var child in node.Declarations.Where(x => IsTypeTarget(x.Kind)).SelectMany(Visit))
                yield return child;
        }

        private static bool IsTypeTarget(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.Namespace:
                case SyntaxKind.TypeDeclarationStatement:
                case SyntaxKind.ConstantBufferDeclaration:
                    return true;

                default:
                    return false;
            }
        }

        public override IEnumerable<EditorNavigationTarget> VisitTypeDeclarationStatement(TypeDeclarationStatementSyntax node)
        {
            return Visit(node.Type);
        }

        public override IEnumerable<EditorNavigationTarget> VisitConstantBuffer(ConstantBufferSyntax node)
        {
            yield return CreateTypeTarget(node.Name, node.GetTextSpanSafe(), Glyph.ConstantBuffer,
                node.Declarations.OfType<VariableDeclarationStatementSyntax>());
        }

        public override IEnumerable<EditorNavigationTarget> VisitInterfaceType(InterfaceTypeSyntax node)
        {
            yield return CreateTypeTarget(node.Name, node.GetTextSpanSafe(), Glyph.Interface, node.Methods);
        }

        public override IEnumerable<EditorNavigationTarget> VisitStructType(StructTypeSyntax node)
        {
            yield return CreateTypeTarget(node.Name, node.GetTextSpanSafe(), node.IsClass ? Glyph.Class : Glyph.Struct, node.Members);
        }

        public override IEnumerable<EditorNavigationTarget> VisitFunctionDeclaration(FunctionDeclarationSyntax node)
        {
            yield return CreateTarget(node.Name.GetUnqualifiedName().Name, node.GetDescription(false, true), node.GetTextSpanSafe(), Glyph.Function);
        }

        public override IEnumerable<EditorNavigationTarget> VisitFunctionDefinition(FunctionDefinitionSyntax node)
        {
            yield return CreateTarget(node.Name.GetUnqualifiedName().Name, node.GetDescription(false, true), node.GetTextSpanSafe(), Glyph.Function);
        }

        public override IEnumerable<EditorNavigationTarget> VisitTechnique(TechniqueSyntax node)
        {
            yield return CreateTarget(node.Name, node.Name?.Text, node.GetTextSpanSafe(), Glyph.Technique);
        }

        public override IEnumerable<EditorNavigationTarget> VisitEmptyStatement(EmptyStatementSyntax node)
        {
            yield break;
        }

        public override IEnumerable<EditorNavigationTarget> VisitVariableDeclarationStatement(VariableDeclarationStatementSyntax node)
        {
            // The first declarator span includes the initial declaration.
            // The last declarator span includes the semicolon.

            var variables = node.Declaration.Variables.ToList();

            var lastDeclarator = variables.Last();
            var firstDeclarator = variables.First();

            if (!node.GetFirstToken().Span.IsInRootFile)
                yield break;

            var declarationRootSpan = node.Declaration.GetTextSpanRoot();
            var firstDeclaratorTextSpan = TextSpan.FromBounds(declarationRootSpan.SourceText, declarationRootSpan.Start, firstDeclarator.GetTextSpanRoot().End);

            if (firstDeclarator == lastDeclarator)
                firstDeclaratorTextSpan = TextSpan.FromBounds(declarationRootSpan.SourceText, firstDeclaratorTextSpan.Start, node.GetTextSpanRoot().End);

            yield return CreateTarget(firstDeclarator.Identifier, firstDeclarator.Identifier.Text, firstDeclaratorTextSpan, Glyph.Variable);

            foreach (var declarator in variables.Skip(1))
            {
                var declaratorTextSpan = declarator.GetTextSpanRoot();
                if (declarator == lastDeclarator)
                    declaratorTextSpan = TextSpan.FromBounds(declarationRootSpan.SourceText, declaratorTextSpan.Start, node.GetTextSpanRoot().End);
                yield return CreateTarget(declarator.Identifier, declarator.Identifier.Text, declaratorTextSpan, Glyph.Variable);
            }
        }

        private EditorNavigationTarget CreateTypeTarget(SyntaxToken name, TextSpan nodeSpan, Glyph icon, IEnumerable<SyntaxNode> childNodes)
        {
            if (!nodeSpan.IsInRootFile)
                return null;

            if (name == null)
                return null;

            return new EditorTypeNavigationTarget(name.GetFullyQualifiedName(),
                new SnapshotSpan(_snapshot, nodeSpan.Start, nodeSpan.Length),
                new SnapshotSpan(_snapshot, name.Span.Start, 0),
                icon, icon.GetImageSource(_glyphService),
                childNodes.SelectMany(Visit).ToList());
        }

        private EditorNavigationTarget CreateTarget(SyntaxToken name, string description, TextSpan nodeSpan, Glyph icon)
        {
            if (!nodeSpan.IsInRootFile)
                return null;

            if (name == null)
                return null;

            if (string.IsNullOrWhiteSpace(description))
                description = "?";

            return new EditorNavigationTarget(description,
                new SnapshotSpan(_snapshot, nodeSpan.Start, nodeSpan.Length),
                new SnapshotSpan(_snapshot, name.Span.Start, 0),
                icon, icon.GetImageSource(_glyphService));
        }
    }
}