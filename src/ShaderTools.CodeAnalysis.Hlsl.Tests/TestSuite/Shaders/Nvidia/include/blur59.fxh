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

Comments:
    Utility declarations for doing 2D blur effects in FXComposer.

    To use:
    * Choose a render target size. The default is 256x256. You can
	change this by defining RTT_SIZE before including this header.
    * Choose filter size and filter weights. 9x9 and 5x5 filter routines
	are supplied in this file. They use filter weights defined
	as WT9_0 through WT9_4 and WT5_0 through WT5_2. Again, if you
	want something other than the default, specify these weights
	via #define before #including this header file.
    * Declare render targets. Use the SQUARE_TARGET macro to declare both the
	texturetarget and a sampler to read it. For two-pass
	convolutions you'll need at least two such targets.

    For an example of use, see the "post_glow_screenSize" shader



To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

*******************************************************************************
******************************************************************************/



#ifndef _H_BLUR59_
#define _H_BLUR59_

#include <include\\Quad.fxh>

// Default rendertargets are this size.
//   If you want a different size, define RTT_SIZE before
// 	including this file.
#ifndef RTT_SIZE
#define RTT_SIZE 256
#endif /* RTT_SIZE */

#define RTT_TEXEL_SIZE (1.0f/RTT_SIZE)

#ifdef ONLY_FIXED_SIZE_SQUARE_TEXTURES
QUAD_REAL gRTTTexelIncrement <
    string UIName =  "RTT Texel Size";
> = RTT_TEXEL_SIZE;
#endif /* ONLY_FIXED_SIZE_SQUARE_TEXTURES */

//
// By default, samples are one texel apart. You can redefine this value
//	(or assign it to a global parameter)
//
#ifndef BLUR_STRIDE
#define BLUR_STRIDE 1.0
#endif /* !BLUR_STRIDE */

/////////////////////////////////////////
/////// FILTER WEIGHTS //////////////////
/////////////////////////////////////////

//
// Relative filter weights for each texel.
// The default here is for symmetric distribution.
// To assign your own filter weights, just define WT9_0 through WT9_4,
//   *before* including this file.....
//
// WT9+ are for 9-tap filters, WT5+ are for 5-tap
//

// weights for 9x9 filtering

#ifndef WT9_0
// Relative filter weights indexed by distance (in texels) from "home" texel
//	(WT9_0 is the "home" or center of the filter, WT9_4 is four texels away)
#define WT9_0 1.0
#define WT9_1 0.9
#define WT9_2 0.55
#define WT9_3 0.18
#define WT9_4 0.1
#endif /* WT9_0 */

// weights for 5x5 filtering

#ifndef WT5_0
// Relative filter weights indexed by distance (in texels) from "home" texel
//	(WT5_0 is the "home" or center of the filter, WT5_2 is two texels away)
#define WT5_0 1.0
#define WT5_1 0.8
#define WT5_2 0.2
#endif /* WT5_0 */

////////////////////////////////////////

// these values are based on WT9_0 through WT9_4
#define WT9_NORMALIZE (WT9_0+2.0*(WT9_1+WT9_2+WT9_3+WT9_4))
#define K9_0 (WT9_0/WT9_NORMALIZE)
#define K9_1 (WT9_1/WT9_NORMALIZE)
#define K9_2 (WT9_2/WT9_NORMALIZE)
#define K9_3 (WT9_3/WT9_NORMALIZE)
#define K9_4 (WT9_4/WT9_NORMALIZE)

// these values are based on WT5_0 through WT5_2
#define WT5_NORMALIZE (WT5_0+2.0*(WT5_1+WT5_2))
#define K5_0 (WT5_0/WT5_NORMALIZE)
#define K5_1 (WT5_1/WT5_NORMALIZE)
#define K5_2 (WT5_2/WT5_NORMALIZE)

// RTT Textures

// call SQUARE_TARGET(tex,sampler) to create the declarations for a rendertarget
//	texture and its associated sampler. You will get a square 8-bit texture
//	of RTT_SIZE texels on each side

#define SQUARE_TARGET(texName,samplerName) texture texName : RENDERCOLORTARGET < \
    int width = RTT_SIZE; \
    int height = RTT_SIZE; \
    int MIPLEVELS = 1; \
    string format = "X8R8G8B8"; \
    string UIWidget = "None"; \
>; \
sampler2D samplerName = sampler_state { \
    texture = <texName>; \
    AddressU = Clamp; \
    AddressV = Clamp; \
    Filter = MIN_MAG_LINEAR_MIP_POINT; \
};

/************* DATA STRUCTS **************/

// nine texcoords, to sample (usually) nine in-line texels
struct ScreenAligned9TexelVOut
{
    QUAD_REAL4 Position : POSITION;
    QUAD_REAL2 UV  : TEXCOORD0;
    QUAD_REAL4 UV1 : TEXCOORD1; // these contain xy and zw pairs
    QUAD_REAL4 UV2 : TEXCOORD2;
    QUAD_REAL4 UV3 : TEXCOORD3;
    QUAD_REAL4 UV4 : TEXCOORD4;
};

// five texcoords, to sample (usually) five in-line texels
struct ScreenAligned5TexelVOut
{
    QUAD_REAL4 Position : POSITION;
    QUAD_REAL2 UV  : TEXCOORD0;
    QUAD_REAL4 UV1 : TEXCOORD1; // these contain xy and zw pairs
    QUAD_REAL4 UV2 : TEXCOORD2;
};

/*********** vertex shaders ******/

// vertex shader to align blur samples vertically
ScreenAligned9TexelVOut vert9BlurVS(
    QUAD_REAL3 Position	: POSITION,
    QUAD_REAL3 TexCoord	: TEXCOORD0
) {
    ScreenAligned9TexelVOut OUT = (ScreenAligned9TexelVOut)0;
    OUT.Position = QUAD_REAL4(Position, 1);
    QUAD_REAL TexelIncrement = BLUR_STRIDE/QuadScreenSize.y;
    QUAD_REAL2 Coord = QUAD_REAL2(TexCoord.xy+QuadTexelOffsets);
    OUT.UV = Coord;
    OUT.UV1 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement,
			 Coord.x, Coord.y - TexelIncrement);
    OUT.UV2 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement*2,
			 Coord.x, Coord.y - TexelIncrement*2);
    OUT.UV3 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement*3,
			 Coord.x, Coord.y - TexelIncrement*3);
    OUT.UV4 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement*4,
			 Coord.x, Coord.y - TexelIncrement*4);
    return OUT;
}

// vertex shader to align blur samples horizontally
ScreenAligned9TexelVOut horiz9BlurVS(
    QUAD_REAL3 Position	: POSITION,
    QUAD_REAL3 TexCoord	: TEXCOORD0
) {
    ScreenAligned9TexelVOut OUT = (ScreenAligned9TexelVOut)0;
    OUT.Position = QUAD_REAL4(Position, 1);
    QUAD_REAL TexelIncrement = BLUR_STRIDE/QuadScreenSize.x;
    QUAD_REAL2 Coord = QUAD_REAL2(TexCoord.xy+QuadTexelOffsets);
    OUT.UV = Coord;
    OUT.UV1 = QUAD_REAL4(Coord.x + TexelIncrement, Coord.y,
			 Coord.x - TexelIncrement, Coord.y);
    OUT.UV2 = QUAD_REAL4(Coord.x + TexelIncrement*2, Coord.y,
			 Coord.x - TexelIncrement*2, Coord.y);
    OUT.UV3 = QUAD_REAL4(Coord.x + TexelIncrement*3, Coord.y,
			 Coord.x - TexelIncrement*3, Coord.y);
    OUT.UV4 = QUAD_REAL4(Coord.x + TexelIncrement*4, Coord.y,
			 Coord.x - TexelIncrement*4, Coord.y);
    return OUT;
}

// vertex shader to align blur samples vertically
ScreenAligned5TexelVOut vert5BlurVS(
    QUAD_REAL3 Position	: POSITION,
    QUAD_REAL3 TexCoord	: TEXCOORD0
) {
    ScreenAligned5TexelVOut OUT = (ScreenAligned5TexelVOut)0;
    OUT.Position = QUAD_REAL4(Position, 1);
    QUAD_REAL TexelIncrement = BLUR_STRIDE/QuadScreenSize.y;
    QUAD_REAL2 Coord = QUAD_REAL2(TexCoord.xy+QuadTexelOffsets);
    OUT.UV = Coord;
    OUT.UV1 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement,
			 Coord.x, Coord.y - TexelIncrement);
    OUT.UV2 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement*2,
			 Coord.x, Coord.y - TexelIncrement*2);
    return OUT;
}

// vertex shader to align blur samples horizontally
ScreenAligned5TexelVOut horiz5BlurVS(
    QUAD_REAL3 Position	: POSITION,
    QUAD_REAL3 TexCoord	: TEXCOORD0
) {
    ScreenAligned5TexelVOut OUT = (ScreenAligned5TexelVOut)0;
    OUT.Position = QUAD_REAL4(Position, 1);
    QUAD_REAL TexelIncrement = BLUR_STRIDE/QuadScreenSize.x;
    QUAD_REAL2 Coord = QUAD_REAL2(TexCoord.xy+QuadTexelOffsets);
    OUT.UV = Coord;
    OUT.UV1 = QUAD_REAL4(Coord.x + TexelIncrement, Coord.y,
			 Coord.x - TexelIncrement, Coord.y);
    OUT.UV2 = QUAD_REAL4(Coord.x + TexelIncrement*2, Coord.y,
			 Coord.x - TexelIncrement*2, Coord.y);
    return OUT;
}

//////////////////////////////////
////////// Pixel Shaders /////////
//////////////////////////////////

QUAD_REAL4 blur9PS(ScreenAligned9TexelVOut IN,
		uniform sampler2D SrcSamp) : COLOR
{   
    QUAD_REAL3 OutCol = tex2D(SrcSamp, IN.UV4.zw).rgb * K9_4;
    OutCol += tex2D(SrcSamp, IN.UV3.zw).rgb * K9_3;
    OutCol += tex2D(SrcSamp, IN.UV2.zw).rgb * K9_2;
    OutCol += tex2D(SrcSamp, IN.UV1.zw).rgb * K9_1;
    OutCol += tex2D(SrcSamp, IN.UV).rgb * K9_0;
    OutCol += tex2D(SrcSamp, IN.UV1.xy).rgb * K9_1;
    OutCol += tex2D(SrcSamp, IN.UV2.xy).rgb * K9_2;
    OutCol += tex2D(SrcSamp, IN.UV3.xy).rgb * K9_3;
    OutCol += tex2D(SrcSamp, IN.UV4.xy).rgb * K9_4;
    return QUAD_REAL4(OutCol.rgb,1.0);
} 

QUAD_REAL4 blur5PS(ScreenAligned5TexelVOut IN,
		uniform sampler2D SrcSamp) : COLOR
{   
    QUAD_REAL3 OutCol = tex2D(SrcSamp, IN.UV2.zw).rgb * K5_2;
    OutCol += tex2D(SrcSamp, IN.UV1.zw).rgb * K5_1;
    OutCol += tex2D(SrcSamp, IN.UV).rgb * K5_0;
    OutCol += tex2D(SrcSamp, IN.UV1.xy).rgb * K5_1;
    OutCol += tex2D(SrcSamp, IN.UV2.xy).rgb * K5_2;
    return QUAD_REAL4(OutCol.rgb,1.0);
} 

//
// These macros provided for engines that prefer having the sampler
//   come from a globally-scoped variable, rather than being passed
//   as a uniform argument
//

#define BLUR_GLOBAL_9(FuncName,Samp2D) QUAD_REAL4 FuncName(ScreenAligned9TexelVOut IN) : COLOR { \
    QUAD_REAL3 OutCol = tex2D(Samp2D, IN.UV4.zw).rgb * K9_4; \
    OutCol += tex2D(Samp2D, IN.UV3.zw).rgb * K9_3; \
    OutCol += tex2D(Samp2D, IN.UV2.zw).rgb * K9_2; \
    OutCol += tex2D(Samp2D, IN.UV1.zw).rgb * K9_1; \
    OutCol += tex2D(Samp2D, IN.UV).rgb * K9_0; \
    OutCol += tex2D(Samp2D, IN.UV1.xy).rgb * K9_1; \
    OutCol += tex2D(Samp2D, IN.UV2.xy).rgb * K9_2; \
    OutCol += tex2D(Samp2D, IN.UV3.xy).rgb * K9_3; \
    OutCol += tex2D(Samp2D, IN.UV4.xy).rgb * K9_4; \
    return QUAD_REAL4(OutCol.rgb,1.0); } 

#define BLUR_GLOBAL_5(FuncName,Samp2D) QUAD_REAL4 FuncName(ScreenAligned5TexelVOut IN) : COLOR { \
    QUAD_REAL3 OutCol = tex2D(Samp2D, IN.UV2.zw).rgb * K5_2; \
    OutCol += tex2D(Samp2D, IN.UV1.zw).rgb * K5_1; \
    OutCol += tex2D(Samp2D, IN.UV).rgb * K5_0; \
    OutCol += tex2D(Samp2D, IN.UV1.xy).rgb * K5_1; \
    OutCol += tex2D(Samp2D, IN.UV2.xy).rgb * K5_2; \
    return QUAD_REAL4(OutCol.rgb,1.0); } 

#endif /* ! _H_BLUR59_ */

///////////////////////////// eof ////
