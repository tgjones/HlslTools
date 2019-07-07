namespace ShaderTools.CodeAnalysis
{
    public static class LanguageNames
    {
        /// <summary>
        /// The common name used for the HLSL language.
        /// </summary>
        public const string Hlsl = "HLSL";

        /// <summary>
        /// The common name used for the Unity ShaderLab language.
        /// </summary>
        public const string ShaderLab = "ShaderLab";

        public static readonly string[] AllLanguages = { Hlsl, ShaderLab };
    }
}
