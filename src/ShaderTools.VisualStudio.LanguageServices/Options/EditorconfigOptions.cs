using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.CodeAnalysis.Options;
using OptionKey = Microsoft.CodeAnalysis.Options.OptionKey;

namespace ShaderTools.VisualStudio.LanguageServices.Implementation.Options
{
    // Hooks up .editorconfig settings, looked up from the workspace's ITextBuffer for the document.

    internal sealed class EditorconfigDocumentOptions : IDocumentOptions
    {
        public bool TryGetDocumentOption(Document document, OptionKey option, OptionSet underlyingOptions, out object value)
        {
            // From the document, look up the ITextBuffer, and find its IEditorOptions.
            // Note that IEditorOptions are also available from an ITextView. Not sure if those options can differ from the ones on the buffer.
            var workspace = PrimaryWorkspace.Workspace as VisualStudioWorkspace;
            var textBuffer = workspace?.GetTextBufferForDocument(document.Id);
            IEditorOptions editorOptions = textBuffer?.Properties[typeof(IEditorOptions)] as IEditorOptions;
            if (editorOptions == null)
            {
                value = null;
                return false;
            }

            // Check if the OptionKey is one of the ones we want to override with values from the IEditorOptions
            if (option.Option == FormattingOptions.UseTabs)
            {
                value = !editorOptions.IsConvertTabsToSpacesEnabled();
                return true;
            }
            else if (option.Option == FormattingOptions.TabSize)
            {
                value = editorOptions.GetTabSize();
                return true;
            }
            else if (option.Option == FormattingOptions.IndentationSize)
            {
                value = editorOptions.GetIndentSize();
                return true;
            }
            else if (option.Option == FormattingOptions.NewLine)
            {
                value = editorOptions.GetNewLineCharacter();
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }

    internal sealed class EditorconfigDocumentOptionsProvider : IDocumentOptionsProvider
    {
        private static readonly EditorconfigDocumentOptions _documentOptions = new EditorconfigDocumentOptions();

        public Task<IDocumentOptions> GetOptionsForDocumentAsync(Document document, CancellationToken cancellationToken)
        {
            return Task.FromResult<IDocumentOptions>(_documentOptions);
        }
    }
}
