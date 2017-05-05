//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using ShaderTools.LanguageServer.Protocol.Utilities;
namespace ShaderTools.LanguageServer.Protocol.MessageProtocol.Channel
{
    public class NamedPipeServerChannel : ChannelBase
    {
        private string pipeName;
        private NamedPipeServerStream pipeServer;

        public NamedPipeServerChannel(string pipeName)
        {
            this.pipeName = pipeName;
        }

        public override async Task WaitForConnection()
        {
            await this.pipeServer.WaitForConnectionAsync();

            this.IsConnected = true;
        }

        protected override void Initialize(IMessageSerializer messageSerializer)
        {
            try
            {
                this.pipeServer =
                    new NamedPipeServerStream(
                        pipeName,
                        PipeDirection.InOut,
                        1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);
            }
            catch (IOException e)
            {
                Logger.Write(
                    LogLevel.Verbose,
                    "Named pipe server failed to start due to exception:\r\n\r\n" + e.Message);

                throw e;
            }

            this.MessageReader =
                new MessageReader(
                    this.pipeServer,
                    messageSerializer);

            this.MessageWriter =
                new MessageWriter(
                    this.pipeServer,
                    messageSerializer);
        }

        protected override void Shutdown()
        {
            if (this.pipeServer != null)
            {
                Logger.Write(LogLevel.Verbose, "Named pipe server shutting down...");

                this.pipeServer.Dispose();

                Logger.Write(LogLevel.Verbose, "Named pipe server has been disposed.");
            }
        }
    }
}

