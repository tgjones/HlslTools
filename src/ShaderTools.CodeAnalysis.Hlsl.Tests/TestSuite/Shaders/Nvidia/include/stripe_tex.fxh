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

// Utility for texture-based stripes and checks.
// Reads (or Creates) the specially-MIP-mapped texture and provides functions:
//    	stripe() <- uses texture for high performance
//		numeric_stripe() <- uses math nstead for high quality antialiasing
//	checker2D() checker3D() checker3Drgb()
//
// Be sure to call DECLARE_BALANCE or define "gBalance" yourself if you plan to use it!
//



To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

*******************************************************************************
******************************************************************************/



#ifndef _H_STRIPE_TEX_
#define _H_STRIPE_TEX_

/////////// Override-able Macro Flags /////////////////

// The balance between light and dark parts of the stripe
//    ranges from 0.0 to 1.0
#ifndef DEFAULT_BALANCE
#define DEFAULT_BALANCE (0.5)
#endif /* DEFAULT_BALANCE */

//
// Un-Comment the PROCEDURAL_TEXTURE macro to enable texture generation in
//      DirectX9 ONLY
// DirectX10 may not issue errors, but will generate no texture either
//
// #define PROCEDURAL_TEXTURE
//

#ifndef STRIPE_TEX_SIZE /* only used for DX9 Virtual machine */
#define STRIPE_TEX_SIZE 128
#endif /* STRIPE_TEX_SIZE */

/************************************************************/
/*** TWEAKABLES *********************************************/
/************************************************************/

#define DECLARE_BALANCE float gBalance < \
    string UIWidget = "slider"; \
    float uimin = 0.01; \
    float uimax = 0.99; \
    float uistep = 0.01; \
    string UIName = "Balance"; \
> = DEFAULT_BALANCE;

/*****************************************************/
/*********** texture declaration *********************/
/*****************************************************/

#ifdef PROCEDURAL_TEXTURE
// texture-generator function for DirectX9 only
float4 make_stripe_tex(float2 Pos : POSITION,float ps : PSIZE) : COLOR
{
   float v = 0;
   float nx = Pos.x+ps; // keep the last column full-on, always
   v = nx > Pos.y;
   return float4(v.xxxx);
}

// texture declaration

texture gProcStripeTexture <
    string function = "make_stripe_tex";
    // string UIWidget = "None";
    string UIDesc = "Procedural Texture";
    float2 Dimensions = { STRIPE_TEX_SIZE, STRIPE_TEX_SIZE };
>;

#else /* ! PROCEDURAL_TEXTURE */
// special texture with last column white at all mip levels
texture gStripeTexture <
    string ResourceName = "aa_stripe.dds";
    string ResourceType = "2D";
    // string UIWidget = "None";
    string UIDesc = "Special Mipped Stripe";
>;
#endif /* ! PROCEDURAL_TEXTURE */

sampler2D gStripeSampler = sampler_state {
#ifdef PROCEDUAL_TEXTURE
	Texture = <gProcStripeTexture>;
#else /* ! PROCEDURAL_TEXTURE */
	Texture = <gStripeTexture>;
#endif /* ! PROCEDURAL_TEXTURE */
#if DIRECT3D_VERSION >= 0xa00
    Filter = MIN_MAG_MIP_LINEAR;
#else /* DIRECT3D_VERSION < 0xa00 */
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
#endif /* DIRECT3D_VERSION */
    AddressU = Wrap;
    AddressV = Clamp;
};

////////////////////////////////////////////
// Utility Functions ///////////////////////
////////////////////////////////////////////

#define _SS uniform sampler2D StripeSampler

// base function: "Balance" is in W term
float stripe(float4 XYZW,_SS) { return tex2D(StripeSampler,XYZW.xw).x; }

float stripe(float4 XYZW,float Balance,_SS) {
    return stripe(float4(XYZW.xyz,Balance),StripeSampler); }

float stripe(float3 XYZ,float Balance,_SS) {
    return stripe(float4(XYZ.xyz,Balance),StripeSampler); }

float stripe(float2 XY,float Balance,_SS) {
    return stripe(float4(XY.xyy,Balance),StripeSampler); }

float stripe(float X,float Balance,_SS) {
    return stripe(float4(X.xxx,Balance),StripeSampler); }

// use default balance (can't do float4 version, would interfere): //

float stripe(float3 XYZ,_SS) {
    return stripe(float4(XYZ.xyz,DEFAULT_BALANCE),StripeSampler); }

float stripe(float2 XY,_SS) {
    return stripe(float4(XY.xyy,DEFAULT_BALANCE),StripeSampler); }

float stripe(float X,_SS) {
    return stripe(float4(X.xxx,DEFAULT_BALANCE),StripeSampler); }

///////////////////////////////////
// texture-free alternative ///////
///////////////////////////////////

float numeric_stripe(
	    float Value,
	    float Balance,
	    float Oversample,
	    float PatternScale
) {
    float width = abs(ddx(Value)) + abs(ddy(Value));
    float w = width*Oversample;
    float x0 = Value/PatternScale - (w/2.0);
    float x1 = x0 + w;
    float i0 = (1.0-Balance)*floor(x0) + max(0.0, frac(x0)-Balance);
    float i1 = (1.0-Balance)*floor(x1) + max(0.0, frac(x1)-Balance);
    float stripe = (i1 - i0)/w;
    stripe = min(1.0,max(0.0,stripe)); 
    return stripe;
}

///////////////////////////////////
// 2D checkerboard ////////////////
///////////////////////////////////

float checker2D(float4 XYZW,_SS)
{
    float stripex = tex2D(StripeSampler,XYZW.xw).x;
    float stripey = tex2D(StripeSampler,XYZW.yw).x;
    return abs(stripex - stripey);
}

// overloads of the above

float checker2D(float4 XYZW,float Balance,_SS) {
    return checker2D(float4(XYZW.xyz,Balance),StripeSampler); }

float checker2D(float3 XYZ,float Balance,_SS) {
    return checker2D(float4(XYZ.xyz,Balance),StripeSampler); }

float checker2D(float2 XY,float Balance,_SS) {
    return checker2D(float4(XY.xyy,Balance),StripeSampler); }

// use default balance ////////////////////////

float checker2D(float3 XYZ,_SS) {
    return checker2D(float4(XYZ.xyz,DEFAULT_BALANCE),StripeSampler); }

float checker2D(float2 XY,_SS) {
    return checker2D(float4(XY.xyy,DEFAULT_BALANCE),StripeSampler); }

float checker2D(float X,_SS) {
    return checker2D(float4(X.xxx,DEFAULT_BALANCE),StripeSampler); }

///////////////////////////////////
// 3D checkerboard ////////////////
///////////////////////////////////

float checker3D(float4 XYZW,_SS)
{
    float stripex = tex2D(StripeSampler,XYZW.xw).x;
    float stripey = tex2D(StripeSampler,XYZW.yw).x;
    float stripez = tex2D(StripeSampler,XYZW.zw).x;
    float check = abs(abs(stripex - stripey) - stripez);
    return check;
}

// overloads of the above

float checker3D(float3 XYZ,float Balance,_SS) {
    return checker3D(float4(XYZ.xyz,Balance),StripeSampler); }

float checker3D(float4 XYZW,float Balance,_SS) {
    return checker3D(float4(XYZW.xyz,Balance),StripeSampler); }

// use default balance ////////////////////////

float checker3D(float3 XYZ,_SS) {
    return checker3D(float4(XYZ.xyz,DEFAULT_BALANCE),StripeSampler); }

float checker3D(float2 XY,_SS) {
    return checker3D(float4(XY.xyy,DEFAULT_BALANCE),StripeSampler); }

float checker3D(float X,_SS) {
    return checker3D(float4(X.xxx,DEFAULT_BALANCE),StripeSampler); }

/////////////

float3 checker3Drgb(float4 XYZW,_SS)
{
    float3 result;
    result.x = tex2D(StripeSampler,XYZW.xw).x;
    result.y = tex2D(StripeSampler,XYZW.yw).x;
    result.z = tex2D(StripeSampler,XYZW.zw).x;
    return result;
}

float3 checker3Drgb(float3 XYZ,float Balance,_SS) {
    return checker3Drgb(float4(XYZ.xyz,Balance),StripeSampler); }

float3 checker3Drgb(float3 XYZ,_SS) {
    return checker3Drgb(float4(XYZ.xyz,DEFAULT_BALANCE),StripeSampler); }

#endif /* _H_STRIPE_TEX_ */

/***************************** eof ***/
