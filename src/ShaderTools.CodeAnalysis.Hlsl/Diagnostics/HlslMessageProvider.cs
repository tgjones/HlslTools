using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Properties;

namespace ShaderTools.CodeAnalysis.Hlsl.Diagnostics
{
    internal sealed class HlslMessageProvider : MessageProvider
    {
        public static readonly HlslMessageProvider Instance = new HlslMessageProvider();

        private HlslMessageProvider() { }

        public override string CodePrefix { get; } = "HLSL";

        public override string GetMessageFormat(int code)
        {
            return Resources.ResourceManager.GetString(((DiagnosticId) code).ToString());
        }

        public override DiagnosticSeverity GetSeverity(int code)
        {
            switch ((DiagnosticId) code)
            {
                case DiagnosticId.LoopControlVariableConflict:
                case DiagnosticId.ImplicitTruncation:
                    return DiagnosticSeverity.Warning;
                default:
                    return DiagnosticSeverity.Error;
            }
        }
    }
}
