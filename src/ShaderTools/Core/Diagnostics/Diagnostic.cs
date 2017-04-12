using System.Collections.Immutable;
using System.Globalization;
using ShaderTools.Core.Syntax;

namespace ShaderTools.Core.Diagnostics
{
    public sealed class Diagnostic
    {
        private static ImmutableDictionary<int, DiagnosticDescriptor> ErrorCodeToDescriptorMap = ImmutableDictionary<int, DiagnosticDescriptor>.Empty;

        public DiagnosticDescriptor Descriptor { get; }

        public SourceRange SourceRange { get; }

        public string Message { get; }

        public DiagnosticSeverity Severity => Descriptor.Severity;

        internal Diagnostic(DiagnosticDescriptor descriptor, SourceRange sourceRange, string message)
        {
            Descriptor = descriptor;
            SourceRange = sourceRange;
            Message = message;
        }

        internal static Diagnostic Create(MessageProvider messageProvider, SourceRange sourceRange, int errorCode, params object[] arguments)
        {
            var descriptor = GetOrCreateDescriptor(errorCode, messageProvider);
            var message = string.Format(CultureInfo.CurrentCulture, descriptor.MessageFormat, arguments);
            return new Diagnostic(descriptor, sourceRange, message);
        }

        internal static DiagnosticDescriptor GetOrCreateDescriptor(int errorCode, MessageProvider messageProvider)
        {
            return ImmutableInterlocked.GetOrAdd(ref ErrorCodeToDescriptorMap, errorCode, code => CreateDescriptor(code, messageProvider));
        }

        private static DiagnosticDescriptor CreateDescriptor(int errorCode, MessageProvider messageProvider)
        {
            var id = messageProvider.GetIdForErrorCode(errorCode);
            var messageFormat = messageProvider.GetMessageFormat(errorCode);
            var severity = messageProvider.GetSeverity(errorCode);
            return new DiagnosticDescriptor(id, errorCode, messageFormat, severity);
        }

        public override string ToString()
        {
            return $"{SourceRange} {Message}";
        }
    }
}