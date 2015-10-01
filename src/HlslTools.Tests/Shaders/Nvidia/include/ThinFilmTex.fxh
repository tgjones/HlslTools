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
// Declarations for "thinfilm" effect, this reads (or creates a special
//   spectral texture for use in highlights, creating the illusion of colored refraction.
//
// Pixel (or vertex) shaders should call the supplied function
//	float calc_view_depth()
// and then index into the sampler "gFringeMapSampler" to get the spectral color, as in
//     this simple example:
//
// float u = calc_view_depth(dot(Nn,Vn),Thickness);
// float3 spectral = tex2D(gFringeMapSampler,float2(u,0.5));
//
// See the shader library "ThinFilm" effect as a more complete example.
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

#ifndef _H_THINFILMTEX
#define _H_THINFILMTEX

//////////////////////////////////////////////////////////////////////////
// tweakables ////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

float gFilmDepth <
	string UIName = "Film Thickness";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 0.25;
	float UIStep = 0.001;
> = 0.05f;

////////////////////////////////////////////////////////////////////////
// Color-Fringe Generator function /////////////////////////////////////
////////////////////////////////////////////////////////////////////////

#ifdef PROCEDURAL_TEXTURE
// function to generate color-fringe map
float4 CreateFringeMap(float2 Pos:POSITION, float2 Psize : PSIZE) : COLOR
{
    // these lambdas are in 100s of nm,
    //  they represent the wavelengths of light for each respective 
    //  color channel.  They are only approximate so that the texture
    //  can repeat.
    // (600,500,400)nm - should be more like (600,550,440):
    float3 lamRGB = float3(6,5,4);
    // these offsets are used to perturb the phase of the interference
    //   if you are using very thick "thin films" you will want to
    //   modify these offests to avoid complete contructive interference
    //   at a particular depth.. Just a tweak-able.
    float3 offsetRGB = (float3)0;
    // p is the period of the texture, it is the LCM of the wavelengths,
    //  this is the depth in nm when the pattern will repeat.  I was too
    //  lazy to write up a LCM function, so you have to provide it.
    //lcm(6,5,4)
    float p = 60; 
    // vd is the depth of the thin film relative to the texture index
    float vd = p;
    // now compute the color values using this formula:
    //  1/2 ( Sin( 2Pi * d/lam* + Pi/2 + O) + 1 )
    //   where d is the current depth, or "i*vd" and O is some offset* so that
    //   we avoid complete constructive interference in all wavelenths at some
    //   depth.
    float pi = 3.1415926535f;
    float3 rgb = 0.5*(sin(2*pi*(Pos.x*vd)/lamRGB + pi/2.0 + offsetRGB) + 1);
    return float4(rgb,0);
}
#endif /* PROCEDURAL_TEXTURE */

//============================================================================
// texture declaration
//============================================================================

#ifdef PROCEDURAL_TEXTURE
#define FILM_TEX_SIZE 256

texture gProcFringeMap <
    string function = "CreateFringeMap";
    string UIWidget = "None";
    string UIName = "Thinfilm Gradient";
    int width = FILM_TEX_SIZE;
    int height = 1;	// 1D lookup
>;

#else /* ! PROCEDURAL_TEXTURE */
// texture containig thin-film gradient color ramp
texture gFringeMap <
    string ResourceName = "ColorRamp01.png";
    string UIName = "Thinfilm Gradient";
>;
#endif /* ! PROCEDURAL_TEXTURE */

sampler2D gFringeMapSampler = sampler_state 
{
#ifdef PROCEDUAL_TEXTURE
	Texture = <gProcFringeMap>;
#else /* ! PROCEDURAL_TEXTURE */
	Texture = <gFringeMap>;
#endif /* ! PROCEDURAL_TEXTURE */
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    // AddressU  = Wrap;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

//////////////////////////////////////////////////////////////////////////
// Function to Index this texture - use in vertex or pixel shaders ///////
//////////////////////////////////////////////////////////////////////////

float calc_view_depth(float NDotV,float Thickness)
{
    // return (1.0 / NDotV) * Thickness;
    return (Thickness / NDotV);
}

#endif /* _H_THINFILMTEX */
