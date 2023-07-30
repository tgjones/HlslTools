namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public enum ScalarType
    {
        Void,

        Bool,

        Int,
        Uint,
        Int64_t,
        Int16_t,
		Uint64_t,
		Uint16_t,

        Half,
        Float16_t,
        Float,
        Double,

        Min16Float,
        Min10Float,
        Min16Int,
        Min12Int,
        Min16Uint,

        String
    }
}