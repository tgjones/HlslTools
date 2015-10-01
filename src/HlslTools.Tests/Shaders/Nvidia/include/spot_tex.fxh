/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #4 $

Copyright NVIDIA Corporation 2008
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY
LOSS) ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF
NVIDIA HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

//
// Read (Or Create) a round pattern texture for spot lights
//
// Includes the function spot_pattern() which can also be caleld directly from pixel shaders
//


To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

*******************************************************************************
******************************************************************************/


//
// Un-Comment the PROCEDURAL_TEXTURE macro to enable texture generation in
//      DirectX9 ONLY
// DirectX10 may not issue errors, but will generate no texture either
//
// #define PROCEDURAL_TEXTURE
//

#ifndef _H_SPOT_TEX
#define _H_SPOT_TEX

float spot_pattern(float2 UV,float InnerRadius,float OuterRadius)
{
    float2 v = UV - float2(0.5,0.5);
    float d = length(v)/OuterRadius;
    float s = 1.0 - smoothstep(InnerRadius,1,d);
    return s;
}

///////////////////////////////////////////
// Texture Declaration ////////////////////
///////////////////////////////////////////

// Parameters for optional DirectX9 texture generation
#ifndef SPOT_TEX_SIZE
#define SPOT_TEX_SIZE 64
#endif /* SPOT_TEX_SIZE */

#ifndef SPOT_TEX_INSIDE
#define SPOT_TEX_INSIDE 0.4
#endif /* SPOT_TEX_INSIDE */

#ifdef PROCEDURAL_TEXTURE
// function used to fill texture
float4 spot_texel(float2 P : POSITION,float2 dP : PSIZE) : COLOR
{
    //adjust so the outer rows and columns are ALWAYS black
    float2 rad = float2(0.5,0.5) - dP;
    float s = spot_pattern(P,SPOT_TEX_INSIDE,rad.x); // for now simple case of (dP.x==dP.y)
    return float4(s.xxx,1.0);
}
texture gProcSpotTex  <
    string TextureType = "2D";
    string UIName = "Spotlight Shape Texture";
    string function = "spot_texel";
    string UIWidget = "None";
    int width = SPOT_TEX_SIZE, height = SPOT_TEX_SIZE;
>;
#else /* ! PROCEDURAL_TEXTURE */
texture gSpotTex  <
    string TextureType = "2D";
    string UIName = "Spotlight Shape Texture";
    string ResourceName = "Sunlight.tga";
>;
#endif /* ! PROCEDURAL_TEXTURE */

// samplers
sampler2D gSpotSamp = sampler_state 
{
#ifdef PROCEDUAL_TEXTURE
	Texture = <gProcSpotTex>;
#else /* ! PROCEDURAL_TEXTURE */
	Texture = <gSpotTex>;
#endif /* ! PROCEDURAL_TEXTURE */
    AddressU  = Clamp;        
    AddressV  = Clamp;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
};

#endif /* _H_SPOT_TEX */

