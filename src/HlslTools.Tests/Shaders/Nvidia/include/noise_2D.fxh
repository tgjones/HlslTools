/*********************************************************************NVMH3****
*******************************************************************************
$Revision: #5 $

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


Read (Or Create) a 2D noise texture



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

#ifndef _H_NOISE2D
#define _H_NOISE2D

#ifdef PROCEDURAL_TEXTURE
#ifndef NOISE2D_SCALE
#define NOISE2D_SCALE 5
#endif /* NOISE2D_SCALE */

// define as "1" for "pure" noise
#ifndef NOISE2D_LIMIT
#define NOISE2D_LIMIT 256
#endif /* NOISE2D_LIMIT */

#ifndef NOISE_SHEET_SIZE
#define NOISE_SHEET_SIZE 128
#endif /* NOISE_SHEET_SIZE */

#ifndef NOISE2D_FORMAT
#define NOISE2D_FORMAT "A8B8G8R8"
#endif /* NOISE2D_FORMAT */

// function used to fill the volume noise texture
float4 noise_2d(float2 Pos : POSITION) : COLOR
{
    float4 Noise = (float4)0;
    for (int i = 1; i < NOISE2D_LIMIT; i += i) {
        Noise.r += (noise(Pos * NOISE2D_SCALE * i)) / i;
        Noise.g += (noise((Pos + 1)* NOISE2D_SCALE * i)) / i;
        Noise.b += (noise((Pos + 2) * NOISE2D_SCALE * i)) / i;
        Noise.a += (noise((Pos + 3) * NOISE2D_SCALE * i)) / i;
    }
    return (Noise + 0.5);
}

texture gProcNoise2DTex  <
    string TextureType = "2D";
    string function = "noise_2d";
    string Format = (NOISE2D_FORMAT);
	string UIName = "2D Noise Texture";
    string UIWidget = "None";
    int width = NOISE_SHEET_SIZE, height = NOISE_SHEET_SIZE;
>;
#else /* ! PROCEDURAL_TEXTURE */
texture gNoise2DTex  <
    string TextureType = "2D";
    string ResourceName = "Noise2D_std.dds";
    string UIName = "2D Noise Texture";
>;
#endif /* ! PROCEDURAL_TEXTURE */

// sampler
sampler2D gNoise2DSamp = sampler_state 
{
#ifdef PROCEDUAL_TEXTURE
	Texture = <gProcNoise2DTex>;
#else /* ! PROCEDURAL_TEXTURE */
	Texture = <gNoise2DTex>;
#endif /* ! PROCEDURAL_TEXTURE */
    AddressU  = Wrap;        
    AddressV  = Wrap;
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
};

////////////////// UTILITY MACROS ///////////////

// assumes identifier is defined
#define NOISE2D(p) tex2D(Noise2DSamp,(p))
#define SNOISE2D(p) (NOISE2D(p)-0.5)

#endif /* _H_NOISE2D */

///////////////////////////// eof ///
