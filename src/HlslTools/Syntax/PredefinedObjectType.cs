namespace HlslTools.Syntax
{
    public enum PredefinedObjectType
    {
        Texture1D,
        Texture1DArray,
        Texture2D,
        Texture2DArray,
        Texture3D,
        TextureCube,
        TextureCubeArray,
        Texture2DMS,
        Texture2DMSArray,

        RWTexture1D,
        RWTexture1DArray,
        RWTexture2D,
        RWTexture2DArray,
        RWTexture3D,

        AppendStructuredBuffer,
        Buffer,
        ByteAddressBuffer,
        ConsumeStructuredBuffer,
        StructuredBuffer,

        RWBuffer,
        RWByteAddressBuffer,
        RWStructuredBuffer,

        InputPatch,
        OutputPatch,

        PointStream,
        LineStream,
        TriangleStream,

        BlendState,
        DepthStencilState,
        RasterizerState,
        Sampler,
        SamplerState,
        SamplerComparisonState
    }
}