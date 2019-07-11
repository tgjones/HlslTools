using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.CodeAnalysis.Hlsl.Options;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.Formatting
{
    [ExportLanguageService(typeof(IEditorFormattingService), LanguageNames.Hlsl), Shared]
    internal sealed class HlslEditorFormattingService : IEditorFormattingService
    {
        // All the characters that might potentially trigger formatting when typed
        private readonly char[] _supportedChars = ";{}#)".ToCharArray();
        private readonly IHlslOptionsService _optionsService;

        public bool SupportsFormatDocument => true;
        public bool SupportsFormatOnPaste => true;
        public bool SupportsFormatSelection => true;
        public bool SupportsFormatOnReturn => true;

        [ImportingConstructor]
        public HlslEditorFormattingService(IHlslOptionsService optionsService)
        {
            _optionsService = optionsService;
        }

        public async Task<IList<TextChange>> GetFormattingChangesAsync(Document document, TextSpan? textSpan, CancellationToken cancellationToken)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var options = await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);

            var span = textSpan ?? new TextSpan(0, syntaxTree.Text.Length);

            return Formatter.GetFormattedTextChanges(
                syntaxTree,
                syntaxTree.Root,
                SpecializedCollections.SingletonEnumerable(span),
                document.Workspace, 
                options, 
                cancellationToken);
        }

        public async Task<IList<TextChange>> GetFormattingChangesAsync(Document document, char typedChar, int position, CancellationToken cancellationToken)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var options = await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);

            return CodeAnalysis.Hlsl.Formatting.Formatter.GetEditsAfterKeystroke(
                (SyntaxTree) syntaxTree,
                position,
                typedChar,
                _optionsService.GetFormattingOptions(options));
        }

        public async Task<IList<TextChange>> GetFormattingChangesOnPasteAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var options = await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);

            return Formatter.GetFormattedTextChanges(
                syntaxTree,
                syntaxTree.Root,
                SpecializedCollections.SingletonEnumerable(textSpan), 
                document.Workspace,
                options,
                cancellationToken);
        }

        public Task<IList<TextChange>> GetFormattingChangesOnReturnAsync(Document document, int position, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<TextChange>>(null);
        }

        public bool SupportsFormattingOnTypedCharacter(Document document, char ch)
        {
            // Performance: This method checks several options to determine if we should do smart
            // indent, none of which are controlled by editorconfig. Instead of calling 
            // document.GetOptionsAsync we can use the Workspace's global options and thus save the
            // work of attempting to read in the editorconfig file.
            var options = document.Workspace.Options;

            var smartIndentOn = options.GetOption(FormattingOptions.SmartIndent, LanguageNames.Hlsl) == FormattingOptions.IndentStyle.Smart;

            // We consider the proper placement of a close curly when it is typed at the start of the
            // line to be a smart-indentation operation.  As such, even if "format on typing" is off,
            // if "smart indent" is on, we'll still format this.  (However, we won't touch anything
            // else in the block this close curly belongs to.).
            //
            // TODO(cyrusn): Should we expose an option for this?  Personally, i don't think so.
            // If a user doesn't want this behavior, they can turn off 'smart indent' and control
            // everything themselves.  
            if (ch == '}' && smartIndentOn)
            {
                return true;
            }

            // If format-on-typing is not on, then we don't support formatting on any other characters.
            var autoFormattingOnTyping = options.GetOption(FeatureOnOffOptions.AutoFormattingOnTyping, LanguageNames.Hlsl);
            if (!autoFormattingOnTyping)
            {
                return false;
            }

            if (ch == '}' && !options.GetOption(FeatureOnOffOptions.AutoFormattingOnCloseBrace, LanguageNames.Hlsl))
            {
                return false;
            }

            if (ch == ')' && !options.GetOption(FeatureOnOffOptions.AutoFormattingOnCloseParen, LanguageNames.Hlsl))
            {
                return false;
            }

            if (ch == ';' && !options.GetOption(FeatureOnOffOptions.AutoFormattingOnSemicolon, LanguageNames.Hlsl))
            {
                return false;
            }

            // don't auto format after these keys if smart indenting is not on.
            if ((ch == '#') && !smartIndentOn)
            {
                return false;
            }

            return _supportedChars.Contains(ch);
        }
    }
}
