namespace ShaderTools.Hlsl.Syntax
{
    public enum PredefinedObjectType
    {
        Texture,

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

        ConstantBuffer,

        RasterizerOrderedBuffer,
        RasterizerOrderedByteAddressBuffer,
        RasterizerOrderedStructuredBuffer,
        RasterizerOrderedTexture1D,
        RasterizerOrderedTexture1DArray,
        RasterizerOrderedTexture2D,
        RasterizerOrderedTexture2DArray,
        RasterizerOrderedTexture3D,

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

        Sampler1D,
        Sampler2D,
        Sampler3D,
        SamplerCube,

        SamplerState,
        SamplerComparisonState,

        GeometryShader,
        PixelShader,
        VertexShader
    }
}