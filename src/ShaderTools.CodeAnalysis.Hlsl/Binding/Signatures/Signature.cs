using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Hlsl.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding.Signatures
{
    internal abstract class Signature
    {
        public abstract TypeSymbol ReturnType { get; }
        public abstract ParameterDirection GetParameterDirection(int index);
        public abstract bool ParameterHasDefaultValue(int index);
        public abstract TypeSymbol GetParameterType(int index);
        public abstract int ParameterCount { get; }
        public abstract bool HasVariadicParameter { get; }

        public IEnumerable<TypeSymbol> GetParameterTypes()
        {
            for (var i = 0; i < ParameterCount; i++)
                yield return GetParameterType(i);
        }
    }
}