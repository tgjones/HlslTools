using System.Windows.Input;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense
{
    internal sealed class HlslKeyProcessor : KeyProcessor
    {
        private readonly ITextView _textView;
        private readonly IIntellisenseSessionStackMapService _intellisenseSessionStackMapService;

        public HlslKeyProcessor(ITextView textView, IIntellisenseSessionStackMapService intellisenseSessionStackMapService)
        {
            _textView = textView;
            _intellisenseSessionStackMapService = intellisenseSessionStackMapService;
        }

        public override bool IsInterestedInHandledEvents { get; } = true;

        public override void PreviewKeyDown(KeyEventArgs args)
        {
            var key = args.Key;
            var modifiers = args.KeyboardDevice.Modifiers;
            
            if (modifiers == ModifierKeys.None)
            {
                switch (key)
                {
                    case Key.Escape:
                        args.Handled = ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand.Escape);
                        break;
                    case Key.Up:
                        args.Handled = ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand.Up);
                        break;
                    case Key.Down:
                        args.Handled = ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand.Down);
                        break;
                    case Key.PageUp:
                        args.Handled = ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand.PageUp);
                        break;
                    case Key.PageDown:
                        args.Handled = ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand.PageDown);
                        break;
                }
            }

            base.PreviewKeyDown(args);
        }

        private bool ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand command)
        {
            var stackForTextView = _intellisenseSessionStackMapService.GetStackForTextView(_textView);
            if (stackForTextView != null)
            {
                var containsSigHelp = false;
                foreach (var session in stackForTextView.Sessions)
                {
                    if (!containsSigHelp && (session is ISignatureHelpSession))
                    {
                        containsSigHelp = true;
                    }
                    else if (session is ICompletionSession)
                    {
                        if (containsSigHelp)
                            stackForTextView.MoveSessionToTop(session);
                        break;
                    }
                }
            }

            var target = stackForTextView as IIntellisenseCommandTarget;
            return target != null && target.ExecuteKeyboardCommand(command);
        }
    }
}