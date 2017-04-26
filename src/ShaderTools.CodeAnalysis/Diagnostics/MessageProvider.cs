namespace ShaderTools.CodeAnalysis.Diagnostics
{
    internal abstract class MessageProvider
    {
        public abstract string CodePrefix { get; }

        public string GetIdForErrorCode(int errorCode)
        {
            return CodePrefix + errorCode.ToString("0000");
        }

        public abstract string GetMessageFormat(int code);

        public abstract DiagnosticSeverity GetSeverity(int code);
    }
}