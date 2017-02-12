using System;
using ShaderTools.EditorServices.Protocol.MessageProtocol.Channel;

namespace ShaderTools.EditorServices.Protocol.Server
{
    public sealed class ShaderLabLanguageServer : LanguageServerBase
    {
        public ShaderLabLanguageServer(ChannelBase serverChannel)
            : base(serverChannel)
        {
        }

        protected override void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
