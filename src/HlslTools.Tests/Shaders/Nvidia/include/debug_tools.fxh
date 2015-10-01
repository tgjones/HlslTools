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

% Some functions and macros that are commonly used in debugging shaders,
%    	surfaces, models, etc.
%
% This file uses the QUAD_REAL data type inherited from the "Quad" header.



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

#ifndef _H_DEBUG_TOOLS_
#define _H_DEBUG_TOOLS_

// data types, screen-spaced values,
//   stripe texture, stripe and checkerboard functions
#include <include\\Quad.fxh>
#include <include\\stripe_tex.fxh>

//
// Utility func: Instead of typical spherical normalization,
//	project against the boundaries of a (color) cube
//

QUAD_REAL3 cube_normalize(QUAD_REAL3 V) {
    QUAD_REAL3 Na = abs(V);
    QUAD_REAL r = max(max(Na.x,Na.y),Na.z);
    return (V/r);
}

QUAD_REAL3 cube_normalize(QUAD_REAL4 V) { return cube_normalize(V.xyz); }

//////////////////////////////////////////////////////////////////
/////////// functions to convert vectors into colors /////////////
//////////////////////////////////////////////////////////////////

// normalized vector (saturated, at least) as a color
QUAD_REAL3 as_rgb(QUAD_REAL3 V) { return (QUAD_REAL3(0.5,0.5,0.5) + 0.5*V); }
QUAD_REAL3 as_rgb(QUAD_REAL4 V) { return as_rgb(V.xyz); }

// un-normalized vector (spherical normalize)
QUAD_REAL3 as_rgb_n(QUAD_REAL3 V) {
	return (QUAD_REAL3(0.5,0.5,0.5) + 0.5*normalize(V)); }
QUAD_REAL3 as_rgb_n(QUAD_REAL4 V) {
	return (QUAD_REAL3(0.5,0.5,0.5) + 0.5*normalize(V.xyz)); }

// un-normalized vector (cube normalize)
QUAD_REAL3 as_rgb_cn(QUAD_REAL3 V) {
	return (QUAD_REAL3(0.5,0.5,0.5) + 0.5*cube_normalize(V)); }
QUAD_REAL3 as_rgb_cn(QUAD_REAL4 V) {
	return (QUAD_REAL3(0.5,0.5,0.5) + 0.5*cube_normalize(V.xyz)); }

// return RGBA instead of RGB /////////

// normalized vector (saturated, at least) as a color
QUAD_REAL4 as_rgba(QUAD_REAL3 V) {
	return QUAD_REAL4((QUAD_REAL3(0.5,0.5,0.5) + 0.5*V.xyz),1.0); }
QUAD_REAL4 as_rgba(QUAD_REAL4 V) {
	return QUAD_REAL4((QUAD_REAL3(0.5,0.5,0.5) + 0.5*V.xyz),V.w); }

// un-normalized vector (spherical normalize)
QUAD_REAL4 as_rgba_n(QUAD_REAL3 V) {
	return QUAD_REAL4((QUAD_REAL3(0.5,0.5,0.5) + 0.5*normalize(V.xyz)),1.0); }
QUAD_REAL4 as_rgba_n(QUAD_REAL4 V) {
	return QUAD_REAL4((QUAD_REAL3(0.5,0.5,0.5) + 0.5*normalize(V.xyz)),V.w); }

// un-normalized vector (cube normalize)
QUAD_REAL4 as_rgba_cn(QUAD_REAL3 V) {
	return QUAD_REAL4((QUAD_REAL3(0.5,0.5,0.5) +
		0.5*cube_normalize(V.xyz)),1.0); }
QUAD_REAL4 as_rgba_cn(QUAD_REAL4 V) {
	return QUAD_REAL4((QUAD_REAL3(0.5,0.5,0.5) +
		0.5*cube_normalize(V.xyz)),V.w); }

#endif /* ! _H_DEBUG_TOOLS_ */

/////////////////////////////////////////////
///////////////////////////////////// eof ///
/////////////////////////////////////////////

