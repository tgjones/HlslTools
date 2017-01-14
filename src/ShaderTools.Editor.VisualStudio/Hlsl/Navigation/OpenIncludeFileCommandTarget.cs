using System;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Editor.VisualStudio.Core.Navigation;
using ShaderTools.Editor.VisualStudio.Core.Util;
using ShaderTools.Editor.VisualStudio.Hlsl.Text;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation
{
    internal sealed class OpenIncludeFileCommandTarget : CommandTargetBase<VSConstants.VSStd2KCmdID>
    {
        private readonly IWpfTextView _textView;
        private readonly IServiceProvider _serviceProvider;
        private readonly VisualStudioSourceTextFactory _sourceTextFactory;

        public OpenIncludeFileCommandTarget(IVsTextView adapter, IWpfTextView textView, IServiceProvider serviceProvider, VisualStudioSourceTextFactory sourceTextFactory)
            : base(adapter, textView, VSConstants.VSStd2KCmdID.OPENFILE)
        {
            _textView = textView;
            _serviceProvider = serviceProvider;
            _sourceTextFactory = sourceTextFactory;
        }

        protected override bool IsEnabled(VSConstants.VSStd2KCmdID commandId, ref string commandText)
        {
            var includeDirectiveTrivia = GetIncludeDirective();
            if (includeDirectiveTrivia == null)
                return false;

            commandText = $"Open Document {includeDirectiveTrivia.Filename.Text}";
            return true;
        }

        protected override bool Execute(VSConstants.VSStd2KCmdID commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            var includeDirectiveTrivia = GetIncludeDirective();
            if (includeDirectiveTrivia == null)
                return false;

            var includeFileResolver = new IncludeFileResolver(_textView.TextBuffer.GetIncludeFileSystem(_sourceTextFactory));

            var include = includeFileResolver.OpenInclude(
                includeDirectiveTrivia.TrimmedFilename,
                _textView.TextBuffer.GetTextContainer().Filename,
                _textView.TextBuffer.GetTextContainer().Filename,
                _textView.TextBuffer.GetConfigFile().HlslAdditionalIncludeDirectories);

            if (include == null)
                return false;

            _serviceProvider.NavigateTo(include.Filename, 0, 0, 0, 0);
            return true;
        }

        private IncludeDirectiveTriviaSyntax GetIncludeDirective()
        {
            var pos = _textView.Caret.Position.BufferPosition;
            var syntaxTree = pos.Snapshot.GetSyntaxTree(CancellationToken.None);
            var sourceLocation = syntaxTree.MapRootFilePosition(pos.Position);
            var syntaxToken = syntaxTree.Root.FindToken(sourceLocation, true);

            if (syntaxToken.Parent == null || syntaxToken.Parent.Kind != SyntaxKind.IncludeDirectiveTrivia)
                return null;

            var includeDirectiveTrivia = (IncludeDirectiveTriviaSyntax)syntaxToken.Parent;
            if (includeDirectiveTrivia.Filename != syntaxToken)
                return null;

            return includeDirectiveTrivia;
        }
    }
}