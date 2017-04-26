using System.Globalization;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.CodeAnalysis.Parser
{
    internal sealed class PretokenizedDiagnostic
    {
        public DiagnosticDescriptor Descriptor { get; }

        public int Offset { get; }
        public int Width { get; }

        public string Message { get; }

        private PretokenizedDiagnostic(DiagnosticDescriptor descriptor, int offset, int width, string message)
        {
            Descriptor = descriptor;
            Offset = offset;
            Width = width;
            Message = message;
        }

        public static PretokenizedDiagnostic Create(MessageProvider messageProvider, int offset, int width, int errorCode, params object[] arguments)
        {
            var descriptor = Diagnostic.GetOrCreateDescriptor(errorCode, messageProvider);
            var message = string.Format(CultureInfo.CurrentCulture, descriptor.MessageFormat, arguments);

            return new PretokenizedDiagnostic(
                descriptor,
                offset,
                width,
                message);
        }

        public PretokenizedDiagnostic WithOffset(int offset)
        {
            return new PretokenizedDiagnostic(Descriptor, offset, Width, Message);
        }
    }
}
