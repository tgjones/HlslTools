using System;
using System.Collections.Generic;
using System.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Text
{
    /// <summary>
    /// Defines hlsl include type
    /// </summary>
    public enum IncludeType
    {
        /// <summary>
        /// Local include (we should look in folder relative to the code file)
        /// </summary>
        Local,
        /// <summary>
        /// System include generally defined by executable path, or application specified folders
        /// </summary>
        System
    }
}
