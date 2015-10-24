using System;

namespace HlslTools.Symbols
{
    [Flags]
    public enum SemanticUsages
    {
        None = 0,

        VertexShaderInput = 1 << 0,
        VertexShaderOutput = 1 << 1,

        PixelShaderInput = 1 << 2,
        PixelShaderOutput = 1 << 3,

        GeometryShaderInput = 1 << 4,
        GeometryShaderOutput = 1 << 5,

        HullShaderInput = 1 << 6,
        HullShaderOutput = 1 << 7,

        DomainShaderInput = 1 << 8,
        DomainShaderOutput = 1 << 9,

        ComputeShaderInput = 1 << 10,
        ComputeShaderOutput = 1 << 11,

        AllShaders = VertexShaderInput | VertexShaderOutput
                     | PixelShaderInput | PixelShaderOutput 
                     | GeometryShaderInput | GeometryShaderOutput
                     | HullShaderInput | HullShaderOutput
                     | DomainShaderInput | DomainShaderOutput
                     | ComputeShaderInput | ComputeShaderOutput
    }
}