//-----------------------------------------------------------------------------
// File: Flyer.fx
//
// Desc: The effect file for the EffectMesh sample.  The technique implements:
//
//       Texture mapping
//       Diffuse lighting
//       Specular lighting
//       Environment mapping
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------

int sas : SasGlobal
<
    bool SasUiVisible = false;
    int3 SasVersion= {1,0,1};
>;

texture g_txScene  // texture for scene rendering
<
    string SasUiLabel = "Scene Map";
    string SasUiControl= "FilePicker";
>;

texture g_txEnvMap // texture for environment map
<
    string SasUiLabel = "Environment Map";
    string SasUiControl= "FilePicker";
>;

shared float4x4 g_mWorld
<
    bool SasUiVisible = false;
    string SasBindAddress= "Sas.Skeleton.MeshToJointToWorld[0]";
>;     

shared float4x4 g_mView
<
    bool SasUiVisible = false;
    string SasBindAddress= "Sas.Camera.WorldToView";
>;               // View matrix for object

shared float4x4 g_mProj
<
    bool SasUiVisible = false;
    string SasBindAddress= "Sas.Camera.Projection";
>;   

shared float4 g_vLightColor 
<  
    string SasBindAddress= "Sas.SpotLight[0].Color";
    bool SasUiVisible = false;
> = {1.0f, 1.0f, 1.0f, 1.0f}; // Light value

shared float3 g_vLight 
<  
    string SasBindAddress= "Sas.SpotLight[0].Position";
    bool SasUiVisible = false;
>;                                 // Light position in view space

shared float  g_fTime 
<  
    string SasBindAddress= "Sas.Time.Now";
    bool SasUiVisible = false;
>;                                  // Time value

// Object material attributes
float4 Diffuse
<
    string SasUiLabel = "Material Diffuse";
    string SasUiControl = "ColorPicker";
>;      // Diffuse color of the material

float4 Specular
<
    string SasUiLabel = "Material Specular";
    string SasUiControl = "ColorPicker";
> = {1.0f, 1.0f, 1.0f, 1.0f};  // Specular color of the material

float  Reflectivity
<
    string SasUiLabel = "Material Reflectivity";
    string SasUiControl = "Numeric";
>; // Reflectivity of the material

float  Power
<
    string SasUiLabel = "Material Specular Power";
    string SasUiControl = "Numeric";
> = 1.0f;


//-----------------------------------------------------------------------------
// Texture samplers
//-----------------------------------------------------------------------------
sampler g_samScene<bool SasUiVisible = false;> =
sampler_state
{
    Texture = <g_txScene>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
};

sampler g_samEnv<bool SasUiVisible = false;> =
sampler_state
{
    Texture = <g_txEnvMap>;
    MipFilter = Linear;
    MinFilter = Linear;
    MagFilter = Linear;
};


float4x4 inverse_nonSAS(float4x4 m)
{
    float4x4 adj;
    
    // Calculate the adjoint matrix
    adj._11 = m._22*m._33*m._44 + m._23*m._34*m._42 + m._24*m._32*m._43 - m._22*m._34*m._43 - m._23*m._32*m._44 - m._24*m._33*m._42;
    adj._12 = m._12*m._34*m._43 + m._13*m._32*m._44 + m._14*m._33*m._42 - m._12*m._33*m._44 - m._13*m._34*m._42 - m._14*m._32*m._43;
    adj._13 = m._12*m._23*m._44 + m._13*m._24*m._42 + m._14*m._22*m._43 - m._12*m._24*m._43 - m._13*m._22*m._44 - m._14*m._23*m._42;
    adj._14 = m._12*m._24*m._33 + m._13*m._22*m._34 + m._14*m._23*m._32 - m._12*m._23*m._34 - m._13*m._24*m._32 - m._14*m._22*m._33;
    
    adj._21 = m._21*m._34*m._43 + m._23*m._31*m._44 + m._24*m._33*m._41 - m._21*m._33*m._44 - m._23*m._34*m._41 - m._24*m._31*m._43;
    adj._22 = m._11*m._33*m._44 + m._13*m._34*m._41 + m._14*m._31*m._43 - m._11*m._34*m._43 - m._13*m._31*m._44 - m._14*m._33*m._41;
    adj._23 = m._11*m._24*m._43 + m._13*m._21*m._44 + m._14*m._23*m._41 - m._11*m._23*m._44 - m._13*m._24*m._41 - m._14*m._21*m._43;
    adj._24 = m._11*m._23*m._34 + m._13*m._24*m._31 + m._14*m._21*m._33 - m._11*m._24*m._33 - m._13*m._21*m._34 - m._14*m._23*m._31;
    
    adj._31 = m._21*m._32*m._44 + m._22*m._34*m._41 + m._24*m._31*m._42 - m._21*m._34*m._42 - m._22*m._31*m._44 - m._24*m._32*m._41;
    adj._32 = m._11*m._34*m._42 + m._12*m._31*m._44 + m._14*m._32*m._41 - m._11*m._32*m._44 - m._12*m._34*m._41 - m._14*m._31*m._42;
    adj._33 = m._11*m._22*m._44 + m._12*m._24*m._41 + m._14*m._21*m._42 - m._11*m._24*m._42 - m._12*m._21*m._44 - m._14*m._22*m._41;
    adj._34 = m._11*m._24*m._32 + m._12*m._21*m._34 + m._14*m._22*m._31 - m._11*m._22*m._34 - m._12*m._24*m._31 - m._14*m._21*m._32;
    
    adj._41 = m._21*m._33*m._42 + m._22*m._31*m._43 + m._23*m._32*m._41 - m._21*m._32*m._43 - m._22*m._33*m._41 - m._23*m._31*m._42;
    adj._42 = m._11*m._32*m._43 + m._12*m._33*m._41 + m._13*m._31*m._42 - m._11*m._33*m._42 - m._12*m._31*m._43 - m._13*m._32*m._41;
    adj._43 = m._11*m._23*m._42 + m._12*m._21*m._43 + m._13*m._22*m._41 - m._11*m._22*m._43 - m._12*m._23*m._41 - m._13*m._21*m._42;
    adj._44 = m._11*m._22*m._33 + m._12*m._23*m._31 + m._13*m._21*m._32 - m._11*m._23*m._32 - m._12*m._21*m._33 - m._13*m._22*m._31;
    
    // Calculate the determinant
    float det = determinant(m);
    
    float4x4 result = 0;
    if( 0.0f != det )
        result = 1.0f/det * adj;
        
    return result;
}


//-----------------------------------------------------------------------------
// Name: VertScene
// Type: Vertex shader
// Desc: This shader computes standard transform and lighting
//-----------------------------------------------------------------------------
void VertScene( float4 vPos : POSITION,
                float3 vNormal : NORMAL,
                float2 vTex0 : TEXCOORD0,
                out float4 oPos : POSITION,
                out float4 oDiffuse : COLOR0,
                out float2 oTex0 : TEXCOORD0,
                out float3 oViewPos : TEXCOORD1,
                out float3 oViewNormal : TEXCOORD2,
                out float3 oEnvTex : TEXCOORD3 )
{
    float4x4 ViewInv= inverse_nonSAS(g_mView);

    // Transform the position from object space to homogeneous projection space
    oPos = mul( vPos, g_mWorld );
    oPos = mul( oPos, g_mView );
    oPos = mul( oPos, g_mProj );


    // Compute the view-space position
    oViewPos = mul( vPos, g_mWorld );
    oViewPos = mul( float4( oViewPos, 1) , g_mView );

    // Compute view-space normal
    oViewNormal = mul( vNormal, (float3x3)g_mWorld );
    oViewNormal = normalize( mul( oViewNormal, (float3x3)g_mView ) );

    // Compute lighting
    oDiffuse = dot( oViewNormal, normalize( g_vLight - oViewPos ) ) * Diffuse;

    // Just copy the texture coordinate through
    oTex0 = vTex0;

    // Compute the texture coord for the environment map.
    oEnvTex = 2 * dot( -oViewPos, oViewNormal ) * oViewNormal + oViewPos;
    oEnvTex = mul( oEnvTex, (float3x3)ViewInv );
}


//-----------------------------------------------------------------------------
// Name: PixScene
// Type: Pixel shader
// Desc: This shader outputs the pixel's color by modulating the texture's
//         color with diffuse material color
//-----------------------------------------------------------------------------
float4 PixScene( float4 MatDiffuse : COLOR0,
                 float2 Tex0 : TEXCOORD0,
                 float3 ViewPos : TEXCOORD1,
                 float3 ViewNormal : TEXCOORD2,
                 float3 EnvTex : TEXCOORD3 ) : COLOR0
{
    // Compute half vector for specular lighting
    float3 vHalf = normalize( normalize( -ViewPos ) + normalize( g_vLight - ViewPos ) );

    // Compute normal dot half for specular light
    float4 fSpecular = pow( saturate( dot( vHalf, normalize( ViewNormal ) ) ) * Specular, Power );

    // Lookup mesh texture and modulate it with diffuse
    return float4( (float3)( g_vLightColor * ( tex2D( g_samScene, Tex0 ) * MatDiffuse + fSpecular ) * ( 1 - Reflectivity )
                             + texCUBE( g_samEnv, EnvTex ) * Reflectivity
                           ), 1.0f );
}


//-----------------------------------------------------------------------------
// Name: RenderScene
// Type: Technique
// Desc: Renders scene to render target
//-----------------------------------------------------------------------------
technique RenderScene
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertScene();
        PixelShader  = compile ps_2_0 PixScene();
        ZEnable = true;
        AlphaBlendEnable = false;
    }
}
