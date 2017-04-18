using ShaderTools.LanguageServer.Protocol.MessageProtocol.Channel;
using ShaderTools.LanguageServer.Protocol.Server;

namespace ShaderTools.LanguageServer.Protocol.Hlsl.Server
{
    public sealed class HlslLanguageServer : LanguageServerBase
    {
        public HlslLanguageServer(ChannelBase serverChannel)
            : base(serverChannel, new LanguageServerWorkspace(LanguageNames.Hlsl))
        {
            
        }
    }
}
