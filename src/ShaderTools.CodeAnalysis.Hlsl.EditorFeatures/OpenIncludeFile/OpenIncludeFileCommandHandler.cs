using System;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Editor.Commands;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Hlsl;
using ShaderTools.CodeAnalysis.Hlsl.LanguageServices;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Text;
using ShaderTools.CodeAnalysis.Navigation;
using ShaderTools.CodeAnalysis.Notification;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.OpenIncludeFile
{
    [ExportCommandHandler(nameof(OpenIncludeFileCommandHandler), ContentTypeNames.HlslContentType)]
    internal sealed class OpenIncludeFileCommandHandler : ICommandHandler<OpenFileCommandArgs>
    {
        public CommandState GetCommandState(OpenFileCommandArgs args, Func<CommandState> nextHandler)
        {
            var includeDirectiveTrivia = GetIncludeDirective(args.TextView, args.SubjectBuffer);
            if (includeDirectiveTrivia == null)
            {
                return CommandState.Unavailable;
            }

            var commandText = $"Open Document {includeDirectiveTrivia.Filename.Text}";

            return new CommandState(true, false, commandText);
        }

        public void ExecuteCommand(OpenFileCommandArgs args, Action nextHandler)
        {
            var includeDirectiveTrivia = GetIncludeDirective(args.TextView, args.SubjectBuffer);
            if (includeDirectiveTrivia == null)
            {
                nextHandler();
                return;
            }

            var document = args.SubjectBuffer.AsTextContainer().GetOpenDocumentInCurrentContext();

            var syntaxTree = document.GetSyntaxTreeSynchronously(CancellationToken.None);

            var workspace = document.Workspace;

            var includeFileSystem = workspace.Services.GetRequiredService<IWorkspaceIncludeFileSystem>();

            var parseOptions = (HlslParseOptions) syntaxTree.Options;
            var includeFileResolver = new IncludeFileResolver(includeFileSystem, parseOptions);

            var currentFile = ((SyntaxTree) syntaxTree).File;

            var include = includeFileResolver.OpenInclude(includeDirectiveTrivia.TrimmedFilename, currentFile);

            if (include == null)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"Cannot open source file '{includeDirectiveTrivia.TrimmedFilename}'.");
                errorMessage.AppendLine();
                errorMessage.AppendLine("Searched paths:");
                foreach (var includeDirectory in includeFileResolver.GetSearchDirectories(includeDirectiveTrivia.TrimmedFilename, currentFile))
                {
                    errorMessage.AppendLine(includeDirectory);
                }

                var notificationService = workspace.Services.GetService<INotificationService>();
                notificationService.SendNotification(
                    errorMessage.ToString(), 
                    title: "Open Include File", 
                    severity: NotificationSeverity.Information);

                return;
            }

            var documentNavigationService = workspace.Services.GetRequiredService<IDocumentNavigationService>();
            documentNavigationService.TryNavigateToSpan(
                workspace, 
                document.Id,
                new SourceFileSpan(include, new TextSpan(0, 0)));
        }

        private IncludeDirectiveTriviaSyntax GetIncludeDirective(ITextView textView, ITextBuffer textBuffer)
        {
            var document = textBuffer.AsTextContainer().GetOpenDocumentInCurrentContext();
            if (document == null)
            {
                return null;
            }

            var caretPoint = textView.GetCaretPoint(textBuffer);
            if (caretPoint == null)
            {
                return null;
            }

            var syntaxTree = document.GetSyntaxTreeSynchronously(CancellationToken.None);

            var sourceLocation = syntaxTree.MapRootFilePosition(caretPoint.Value.Position);

            var syntaxToken = (SyntaxToken) syntaxTree.Root.FindToken(sourceLocation, true);

            if (syntaxToken.Parent == null || syntaxToken.Parent.Kind != SyntaxKind.IncludeDirectiveTrivia)
            {
                return null;
            }

            var includeDirectiveTrivia = (IncludeDirectiveTriviaSyntax) syntaxToken.Parent;
            if (includeDirectiveTrivia.Filename != syntaxToken)
            {
                return null;
            }

            return includeDirectiveTrivia;
        }
    }
}
