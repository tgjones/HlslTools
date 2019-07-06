using System.ComponentModel.Composition;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
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
    [Export(typeof(ICommandHandler))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [Name(nameof(OpenIncludeFileCommandHandler))]
    internal sealed class OpenIncludeFileCommandHandler : ICommandHandler<OpenFileCommandArgs>
    {
        public string DisplayName => "Open Include File";

        public CommandState GetCommandState(OpenFileCommandArgs args)
        {
            var includeDirectiveTrivia = GetIncludeDirective(args.TextView, args.SubjectBuffer);
            if (includeDirectiveTrivia == null)
            {
                return CommandState.Unavailable;
            }

            var commandText = $"Open Document {includeDirectiveTrivia.Filename.Text}";

            return new CommandState(true, false, commandText);
        }

        public bool ExecuteCommand(OpenFileCommandArgs args, CommandExecutionContext context)
        {
            var includeDirectiveTrivia = GetIncludeDirective(args.TextView, args.SubjectBuffer);
            if (includeDirectiveTrivia == null)
            {
                return false;
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

                context.OperationContext.TakeOwnership();
                var notificationService = workspace.Services.GetService<INotificationService>();
                notificationService.SendNotification(
                    errorMessage.ToString(), 
                    title: "Open Include File", 
                    severity: NotificationSeverity.Information);

                return true;
            }

            var documentNavigationService = workspace.Services.GetRequiredService<IDocumentNavigationService>();
            documentNavigationService.TryNavigateToSpan(
                workspace, 
                document.Id,
                new SourceFileSpan(include, new TextSpan(0, 0)));

            return true;
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
