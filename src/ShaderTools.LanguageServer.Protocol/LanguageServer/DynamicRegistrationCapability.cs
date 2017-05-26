using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTools.LanguageServer.Protocol.LanguageServer
{
    /// <summary>
    /// Class to represent if a capability supports dynamic registration.
    /// </summary>
    public class DynamicRegistrationCapability
    {
        /// <summary>
        /// Whether the capability supports dynamic registration.
        /// </summary>
        public bool? DynamicRegistration { get; set; }
    }
}
