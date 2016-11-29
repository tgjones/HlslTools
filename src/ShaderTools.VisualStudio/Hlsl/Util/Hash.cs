namespace ShaderTools.VisualStudio.Hlsl.Util
{
    // From https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/InternalUtilities/Hash.cs
    internal static class Hash
    {
        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// </summary>
        internal static int Combine(int newKey, int currentKey)
        {
            return unchecked((currentKey * (int) 0xA5555529) + newKey);
        }
    }
}