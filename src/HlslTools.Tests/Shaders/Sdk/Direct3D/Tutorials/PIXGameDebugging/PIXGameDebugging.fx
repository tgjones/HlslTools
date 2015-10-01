//--------------------------------------------------------------------------------------
// File: PIXGameDebugging.fx
//
// The effect file for the PIXGameDebugging sample.  
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

#define WATERCOLOR 0x00006688
#define COS_0_15F 0.98877f
#define SIN_0_15F 0.14944f

float4x4 WorldViewProj;
float4x4 WorldView;
float3x3 WorldViewIT;
float    LightShaftZ;
float	 LightShaftAlpha;
float	 Time;
float4   Light1Dir;
float4   Light1Ambient;
float4   FogData;


texture  CausticTexture;
texture  MeshTexture;

struct VS_IN 
{
    float4 Position  : POSITION;
    float3 Normal    : NORMAL;
    float2 TexCoord  : TEXCOORD0;
};

struct VS_OUT 
{
    float4 Position  : POSITION;
    float4 Color     : COLOR0;
    float2 TexCoord0 : TEXCOORD0;
    float2 TexCoord1 : TEXCOORD1;
    float  Fog       : FOG;
};

struct VS_LIGHTSHAFT_IN
{
    float4 Position  : POSITION;
    float4 Color     : COLOR0;
};

struct VS_LIGHTSHAFT_OUT
{
    float4 Position  : POSITION;
    float4 Color     : COLOR0;
    float2 TexCoord0  : TEXCOORD0;
};
    

//-------------------------------------------------------------------
// Texture Samplers
//-------------------------------------------------------------------

sampler MeshTextureSampler = sampler_state
{
    Texture = <MeshTexture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler CausticTextureSampler = sampler_state
{
    Texture = <CausticTexture>;
    MinFilter = LINEAR;  
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU  = WRAP;
    AddressV  = WRAP;
};


//-------------------------------------------------------------------
// Vertex Shaders
//-------------------------------------------------------------------


VS_OUT CausticVS(VS_IN IN)
{
    VS_OUT OUT;
    
    float2x2 rotation = {COS_0_15F, -SIN_0_15F, SIN_0_15F, COS_0_15F};            
    float4 viewPosition = mul(IN.Position, WorldView);
    float3 viewNormal = normalize(mul(IN.Normal, WorldViewIT));
    
    OUT.Position = mul(IN.Position, WorldViewProj);
    
    OUT.Color = Light1Ambient + dot(viewNormal, Light1Dir);
    
    OUT.TexCoord0 = IN.TexCoord;
    OUT.TexCoord1.xy = mul(viewPosition.xz, rotation);
    
    OUT.Fog = FogData.z * (FogData.y - length(viewPosition.xyz));
            
    return OUT;
}


VS_LIGHTSHAFT_OUT LightShaftVS(VS_LIGHTSHAFT_IN IN)
{
    VS_LIGHTSHAFT_OUT OUT;
    
    float2x2 rotation = {COS_0_15F, -SIN_0_15F, SIN_0_15F, COS_0_15F};
    
    OUT.Position = IN.Position;
    float4 movement = IN.Position;
    movement.x = movement.x + cos(Time * 0.2f) * 0.3f;
    movement.z = movement.z + sin(Time * 0.3f) * 0.2f;
    
    OUT.Position.z = OUT.Position.z + LightShaftZ;

    OUT.Color = IN.Color;
    OUT.Color.a = LightShaftAlpha;
    
    OUT.TexCoord0.xy = mul(OUT.Position.xz, rotation) + movement.xz;
    OUT.TexCoord0.xy = OUT.TexCoord0.xy * 0.9f + 1.0f / (1.0f - OUT.Position.z);

    return OUT;
}

VS_LIGHTSHAFT_OUT LightShaftAlternateVS(VS_LIGHTSHAFT_IN IN)
{
    VS_LIGHTSHAFT_OUT OUT;
    
    OUT.Position = IN.Position;
    OUT.Position.z = OUT.Position.z + LightShaftZ;
    
    OUT.Color = IN.Color;
    
    OUT.TexCoord0.xy = OUT.Position.xz*0.9f;
    
    return OUT;
}

//-------------------------------------------------------------------
// Pixel Shaders
//-------------------------------------------------------------------

float4 CausticPS(VS_OUT IN) : COLOR
{
    float2 movement = IN.TexCoord1.xy;
    
    movement.x = movement.x + cos(Time * 0.2f) * 0.3f;
    movement.y = movement.y + sin(Time * 0.3f) * 0.2f;
    
    float3 color = IN.Color.rgb * tex2D(CausticTextureSampler, movement.xy * 0.9f);
    color = color * tex2D(MeshTextureSampler, IN.TexCoord0.xy);

    return float4(color, 1.0f);
}

float4 LightShaftPS(VS_LIGHTSHAFT_OUT IN) : COLOR
{
    float3 color = IN.Color.rgb * tex2D(CausticTextureSampler, IN.TexCoord0.xy);
    return float4(color, IN.Color.a);
}

//-------------------------------------------------------------------
// Techniques
//-------------------------------------------------------------------

Technique Caustic
{
    Pass P0
    {
        VertexShader = compile vs_2_0 CausticVS();
        PixelShader  = compile ps_2_0 CausticPS();
        
        AlphaBlendEnable = FALSE;
        FogColor = WATERCOLOR;
        FogTableMode = NONE;
        FogVertexMode = NONE;
        RangeFogEnable = FALSE;
    }
}

Technique LightShaft
{
    Pass P0
    {
        VertexShader = compile vs_2_0 LightShaftVS();
        PixelShader  = compile ps_2_0 LightShaftPS();
        
        FogEnable = FALSE;
        BlendOp = ADD;
        SrcBlend = SRCALPHA;
        DestBlend = ONE;
        AlphaTestEnable  = FALSE;
    }
}

Technique LightShaftAlternate
{
    Pass P0
    {
        VertexShader = compile vs_2_0 LightShaftAlternateVS();
        PixelShader  = compile ps_2_0 LightShaftPS();
        
        FogEnable = FALSE;
        BlendOp = ADD;
        SrcBlend = SRCALPHA;
        DestBlend = ONE;
        AlphaTestEnable  = FALSE;
    }
}