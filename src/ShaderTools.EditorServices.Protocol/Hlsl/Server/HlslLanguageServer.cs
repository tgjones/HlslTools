using ShaderTools.EditorServices.Protocol.MessageProtocol.Channel;
using ShaderTools.EditorServices.Protocol.Server;
using ShaderTools.EditorServices.Workspace.Hlsl;

namespace ShaderTools.EditorServices.Protocol.Hlsl.Server
{
    public sealed class HlslLanguageServer : LanguageServerBase
    {
        public HlslLanguageServer(ChannelBase serverChannel)
            : base(serverChannel, new HlslWorkspace())
        {
            
        }
    }
}
