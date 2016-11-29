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

// Matrix ops -- mostly useful for debugging.
//
// All functions return float4x4



To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

*******************************************************************************
******************************************************************************/



#ifndef _NVMATRIX_H
#define _NVMATRIX_H

// FOR NOW, use dummy function in place of HLSL intrinsic inverse()
#include <include\\nvInverse.fxh>
#ifdef _NVINVERSE_H
#define inverse nvInverse
#endif /* ! _NVINVERSE_H */

////////////////////// Super Basics /////////////////////

// Identity 
float4x4 nvIdentityXf() { return float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1); }

/// translate 

float4x4 nvTranslateXf(float4 Vect) { return float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, Vect.xyz,1); }
float4x4 nvTranslateXf(float3 Vect) { return float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, Vect.xyz,1); }
float4x4 nvTranslateXf(float2 Vect) { return float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, Vect.xy,0,1); }

float4x4 nvTranslateXf(float tx,float ty, float tz) {
	return float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, tx,ty,tz,1);
}

/// scale 

float4x4 nvScaleXf(float S) { return float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,S); }
float4x4 nvScaleXf(float3 S) { return float4x4(S.x,0,0,0, 0,S.y,0,0, 0,0,S.z,0, 0,0,0,1); }
float4x4 nvScaleXf(float4 S) { return float4x4(S.x,0,0,0, 0,S.y,0,0, 0,0,S.z,0, 0,0,0,S.w); }

float4x4 nvScaleXf(float Sx, float Sy, float Sz) {
			return float4x4(Sx,0,0,0, 0,Sy,0,0, 0,0,Sz,0, 0,0,0,1); }

/// rotate -- angles in radians
float4x4 nvXRotateXf(float A) {
	float s = sin(A);
	float c = cos(A);
	return float4x4(1,0,0,0,
					0,c,s,0,
					0,-s,c,0,
					0,0,0,1);
}

float4x4 nvYRotateXf(float A) {
	float s = sin(A);
	float c = cos(A);
	return float4x4(c,0,s,0,
					0,1,0,0,
					-s,0,c,0,
					0,0,0,1);
}

float4x4 nvZRotateXf(float A) {
	float s = sin(A);
	float c = cos(A);
	return float4x4(c,s,0,0,
					-s,c,0,0,
					0,0,1,0,
					0,0,0,1);
}

// we assume here that axis is NORMALIZED....
float4x4 nvRotateXf(float A,float3 Axis) {
	float s = sin(A);
	float c = cos(A);
	float t = 1.0 - c;
	float3 Ax2 = Axis*Axis;
	float txy = t*Axis.x*Axis.y;
	float txz = t*Axis.x*Axis.z;
	float tyz = t*Axis.y*Axis.z;
	return float4x4(t*Ax2.x+c,   txy+s*Axis.z,txz-s*Axis.y,0,
					txy-s*Axis.z,t*Ax2.y+c,   tyz+s*Axis.x,0,
					txz+s*Axis.y,tyz-s*Axis.x,t*Ax2.z+c,   0,
					0,0,0,1);
}

float4x4 nvRotateXf(float A,float4 Axis) { return nvRotateXf(A,Axis.xyz); }

float4x4 nvRotateXf(float A,float Ax,float Ay,float Az) {
			return nvRotateXf(A,normalize(float3(Ax,Ay,Az))); }

// rotation submatrix

// extract rotation matrix only
float4x4 nvRotOnly(float4x4 inMatrix) {
	return float4x4(inMatrix[0].xyz,0,
					inMatrix[1].xyz,0,
					inMatrix[2].xyz,0,
					0,0,0,1);
}

// extract translation matrix only
float4x4 nvTransOnly(float4x4 inMatrix)
{
	float4x4 R = nvRotOnly(inMatrix);
	float4x4 Rt = transpose(R);
	return mul(inMatrix,Rt);
}

//////////////// Projection ///////////////////////////////////////

// D = distance to near Z
float4x4 nvFrustumXf(float D) {
	return float4x4(1,0,0,0,
					0,1,0,0,
					0,0,1,(1.0/D),
					0,0,-D,0); }

// angles in radians
float4x4 nvPerspAnglesXf(float AngleX,float AngleY,float ZNear, float ZFar) {
	float Q = ZFar/(ZFar-ZNear);
	float w = 1.0/tan(AngleX/2.0);
	float h = 1.0/tan(AngleY/2.0);
	return float4x4(w,0,0,0,
					0,h,0,0,
					0,0,Q,1,
					0,0,-Q*ZNear,0);
}

// formula like D3DXMatrixPerspectiveLH()
float4x4 nvPerspViewportXf(float Width,float Height,float ZNear, float ZFar) {
	float Q = ZFar/(ZFar-ZNear);
	float w = 2.0*ZNear / Width;
	float h = 2.0*ZNear / Height;
	return float4x4(w,0,0,0,
					0,h,0,0,
					0,0,Q,1,
					0,0,-Q*ZNear,0);
}

float4x4 nvClipProjXf(float ZNear, float ZFar) {
	float zr = ZFar-ZNear;
	float a = (ZNear+ZFar)/zr;
	float b = (-2.0*ZNear*ZFar)/zr;
	return float4x4(1,0,0,0,
					0,1,0,0,
					0,0,a,-1,
					0,0,b,0);
}

// Angle in Radians!
float4x4 nvPerspectiveXf(float Angle,float Aspect,float ZNear, float ZFar) {
	float zr = ZFar-ZNear;
	float a = (ZNear+ZFar)/zr;
	float b = (-2.0*ZNear*ZFar)/zr;
	float t = 1.0 / tan(Angle/2.0);
	return float4x4(t/Aspect,0,0,0,
					0,t,0,0,
					0,0,a,-1,
					0,0,b,0);
}

///////// orthographic

float4x4 nvOrthoProjXf(float Left, float Right,
						float Top, float Bottom,
						float ZNear, float ZFar) {
	float xr = Right-Left;
	float yr = Bottom-Top;
	float zr = ZFar-ZNear;
	float a = (ZNear+ZFar)/zr;
	float b = (-2.0*ZNear*ZFar)/zr;
	return float4x4(2.0/xr,0,0,0,
					0,2.0/yr,0,0,
					0,0,-2.0/zr,0,
					-(Right+Left)/xr,-(Top+Bottom)/yr,(ZNear+ZFar)/zr,1);
}

// handy matrices for spotlight projections //////////////////////////////////

// Since we might not ave the full xfrom of a spotlight, we can make a guess --
//	we DO know its location and orientation, so by adding an arbitrary "up"
//	vector we can create a useful matrix for this spotlamp
//
// Inputs should be normalized
//
float4x4 nv_spot_xf(float3 Pos,float3 Aim,float3 Up)
{
	float3 side = cross(Up,Aim);	// to the side
	side = normalize(side);
	float3 top = cross(side,Aim);
	top = normalize(top);
	float4x4 tXf =nvTranslateXf(Pos.xyz);
	float4x4 rota = float4x4(side.xyz,0,
							top.xyz,0,
							Aim.xyz,0,
							0,0,0,1);
	return mul(rota,tXf);
}

//
// pass xf of a spotlight, its coneangle (in radians), near and far distances.
//	Handy for shadow mapping!
//
float4x4 nv_spot_proj_xf(float4x4 SpotXf,float ShadCone,float ShadNear,float ShadFar)
{
	float4x4 invSpot = inverse(SpotXf);
	float4x4 proj = nvPerspAnglesXf(ShadCone,ShadCone,ShadNear, ShadFar);
	return mul(invSpot,proj);
}

#endif /* _NVMATRIX_H */

///////////////////////////////////////////// eof ///
