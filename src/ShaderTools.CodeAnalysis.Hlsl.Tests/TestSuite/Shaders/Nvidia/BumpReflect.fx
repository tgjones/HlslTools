/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/ShaderLibrary/main/HLSL/BumpReflect.fx#1 $

Copyright NVIDIA Corporation 2002-2007
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.


% Pure Bump-mapped Reflection in DirectX9

keywords: material environment classic bumpmap

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?BumpReflect;";
> = 0.8;

/// un-tweakables //////////////////////

float4x4 WorldXf : World < string UIWidget="None"; >;
float4x4 WVPXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldViewXf : WorldView < string UIWidget="None"; >;
float4x4 WorldViewIXf : WorldViewInverse < string UIWidget="None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;
// float4x4 ViewXf : View < string UIWidget="None"; >;

// Tweakables /////////////////

float bumpHeight <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 2.0;
    float UIStep = 0.1;
	string UIName = "Bump Height";
> = 0.5;

///////////////////

texture normalMap : NORMAL <
    string ResourceName = "default_bump_normal.dds";
    string ResourceType = "2D";
	string UIName = "Normal Map";
>;

texture cubeMap : ENVIRONMENT <
    string ResourceName = "default_reflection.dds";
    string ResourceType = "Cube";
	string UIName = "Environment CubeMap";
>;

sampler2D normalMapSampler = sampler_state
{
    Texture = <normalMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

samplerCUBE envMapSampler = sampler_state
{
    Texture = <cubeMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

////////// CONNECTOR STRUCTS ///////////////////

struct AppData {
    float4 Position : POSITION; //in object space
    float3 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL; //in object space    
    float3 Tangent : TANGENT0; //in object space
    float3 Binormal : BINORMAL0; //in object space
};

struct VertexOutput {
    float4 Position : POSITION; //in projection space
    float4 TexCoord : TEXCOORD0;
    float4 TexCoord1 : TEXCOORD1; //first row of the 3x3 transform from tangent to cube space
    float4 TexCoord2 : TEXCOORD2; //second row of the 3x3 transform from tangent to cube space
    float4 TexCoord3 : TEXCOORD3; //third row of the 3x3 transform from tangent to cube space
};

///////////////// VERTEX SHADER //////////////////

VertexOutput BumpReflectVS(AppData IN,
        uniform float4x4 WorldViewProj,
        uniform float4x4 World,
        uniform float4x4 ViewIT)
{
    VertexOutput OUT;
    // Position in screen space.
    OUT.Position = mul(IN.Position, WorldViewProj);
    // pass texture coordinates for fetching the normal map
    OUT.TexCoord.xyz = IN.TexCoord;
    OUT.TexCoord.w = 1.0;
    // compute the 4x4 tranform from tangent space to object space
    float3x3 TangentToObjSpace;
    // first rows are the tangent and binormal scaled by the bump scale
    TangentToObjSpace[0] = float3(IN.Tangent.x, IN.Binormal.x, IN.Normal.x);
    TangentToObjSpace[1] = float3(IN.Tangent.y, IN.Binormal.y, IN.Normal.y);
    TangentToObjSpace[2] = float3(IN.Tangent.z, IN.Binormal.z, IN.Normal.z);
    OUT.TexCoord1.x = dot(World[0].xyz, TangentToObjSpace[0]);
    OUT.TexCoord1.y = dot(World[1].xyz, TangentToObjSpace[0]);
    OUT.TexCoord1.z = dot(World[2].xyz, TangentToObjSpace[0]);
    OUT.TexCoord2.x = dot(World[0].xyz, TangentToObjSpace[1]);
    OUT.TexCoord2.y = dot(World[1].xyz, TangentToObjSpace[1]);
    OUT.TexCoord2.z = dot(World[2].xyz, TangentToObjSpace[1]);
    OUT.TexCoord3.x = dot(World[0].xyz, TangentToObjSpace[2]);
    OUT.TexCoord3.y = dot(World[1].xyz, TangentToObjSpace[2]);
    OUT.TexCoord3.z = dot(World[2].xyz, TangentToObjSpace[2]);
    float4 worldPos = mul(IN.Position, World);
    // compute the eye vector (going from shaded point to eye) in cube space
    float4 eyeVector = worldPos - ViewIT[3]; // view inv. transpose contains eye position in world space in last row.
    OUT.TexCoord1.w = eyeVector.x;
    OUT.TexCoord2.w = eyeVector.y;
    OUT.TexCoord3.w = eyeVector.z;
    return OUT;
}

///////////////// PIXEL SHADER //////////////////

float4 BumpReflectPS(VertexOutput IN,
                uniform sampler2D NormalMap,
                uniform samplerCUBE EnvironmentMap,
                uniform float BumpScale) : COLOR
{
    // fetch the bump normal from the normal map
    float3 Nt = tex2D(NormalMap, IN.TexCoord.xy).xyz * 2.0 - 1.0;
    Nt = normalize(float3(Nt.x * BumpScale, Nt.y * BumpScale, Nt.z)); 
    // transform the bump normal into cube space
    // then use the transformed normal and eye vector to compute a reflection vector
    // used to fetch the cube map
    // (we multiply by 2 only to increase brightness)
    float3 eyevec = float3(IN.TexCoord1.w, IN.TexCoord2.w, IN.TexCoord3.w);
    float3 Nw;
    Nw.x = dot(IN.TexCoord1.xyz,Nt);
    Nw.y = dot(IN.TexCoord2.xyz,Nt);
    Nw.z = dot(IN.TexCoord3.xyz,Nt);
    float3 lookup = reflect(eyevec, Nw);
    return texCUBE(EnvironmentMap, lookup); 
//    return float4(Nt *0.5 + 0.5, 1);
}

////////////////////////////////////////////////////////////
//////////////////////////////// TECHNIQUES ////////////////
////////////////////////////////////////////////////////////

technique BumpReflect <
    string Script = "Pass=p0;";
> {
    pass p0 <
        string Script = "Draw=geometry;";
    > {
        VertexShader = compile vs_2_0 BumpReflectVS(WVPXf,WorldXf,ViewIXf);
        
        Zenable = true;
        ZWriteEnable = true;
        CullMode = None;

        PixelShader = compile ps_2_0 BumpReflectPS(normalMapSampler,envMapSampler,bumpHeight);
    }
}

/////////////////////// eof ///
