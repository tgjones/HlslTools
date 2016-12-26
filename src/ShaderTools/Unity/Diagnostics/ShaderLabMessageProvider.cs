using ShaderTools.Core.Diagnostics;
using ShaderTools.Properties;

namespace ShaderTools.Unity.Diagnostics
{
    internal sealed class ShaderLabMessageProvider : MessageProvider
    {
        public static readonly ShaderLabMessageProvider Instance = new ShaderLabMessageProvider();

        private ShaderLabMessageProvider() { }

        public override string CodePrefix { get; } = "SL";

        public override string GetMessageFormat(int code)
        {
            return Resources.ResourceManager.GetString(((DiagnosticId) code).ToString());
        }

        public override DiagnosticSeverity GetSeverity(int code)
        {
            return DiagnosticSeverity.Error;
        }
    }
}