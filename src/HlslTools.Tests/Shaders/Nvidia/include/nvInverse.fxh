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

// Machine code for matrix inverse



To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

*******************************************************************************
******************************************************************************/



#ifndef _NVINVERSE_H
#define _NVINVERSE_H

// okay, so this one seems excessive
float nvMatrixCollapse(float2x2 inpMatrix, int row, int column)
{
    float result = 0;
	int i;
	int m = 0;
    for (i = 0; i < 1; i += 1) {
		if (m == row) m += 1;
		int j;
		int n = 0;
		for ( j = 0; j < 1; j += 1) {
			if (n == column) n += 1;
			result = inpMatrix[m][n];
			n += 1;
		}
		m  += 1;
    }
    return result;
}

float2x2 nvMatrixCollapse(float3x3 inpMatrix, int row, int column)
{
    float2x2 result = (float2x2)0;
	int i;
	int m = 0;
    for (i = 0; i < 2; i += 1) {
		if (m == row) m += 1;
		int j;
		int n = 0;
		for ( j = 0; j < 2; j += 1) {
			if (n == column) n += 1;
			result[i][j] = inpMatrix[m][n];
			n += 1;
		}
		m  += 1;
    }
    return result;
}

float3x3 nvMatrixCollapse(float4x4 inpMatrix, int row, int column )
{
    float3x3 result = (float3x3)0;
	int i;
	int m = 0;
    for (i = 0; i < 3; i += 1) {
		if (m == row) m += 1;
		int j;
		int n = 0;
		for ( j = 0; j < 3; j += 1) {
			if (n == column) n += 1;
			result[i][j] = inpMatrix[m][n];
			n += 1;
		}
		m  += 1;
    }
    return result;
}

// Returns a matrix which is the Adjoint of inpMatrix.
float4x4 nvAdjoint(float4x4 inpMatrix)
{
    float4x4 result = (float4x4)0;
	result[0][0] =  determinant(nvMatrixCollapse(inpMatrix, 0, 0));
	result[0][1] = -determinant(nvMatrixCollapse(inpMatrix, 0, 1));
	result[0][2] =  determinant(nvMatrixCollapse(inpMatrix, 0, 2));
	result[1][0] = -determinant(nvMatrixCollapse(inpMatrix, 1, 0));
	result[1][1] =  determinant(nvMatrixCollapse(inpMatrix, 1, 1));
	result[1][2] = -determinant(nvMatrixCollapse(inpMatrix, 1, 2));
	result[2][0] =  determinant(nvMatrixCollapse(inpMatrix, 2, 0));
	result[2][1] = -determinant(nvMatrixCollapse(inpMatrix, 2, 1));
	result[2][2] =  determinant(nvMatrixCollapse(inpMatrix, 2, 2));
	// Cofactor of 4th column
	result[0][3] = -determinant(nvMatrixCollapse(inpMatrix, 0, 3));
	result[1][3] =  determinant(nvMatrixCollapse(inpMatrix, 1, 3));
	result[2][3] = -determinant(nvMatrixCollapse(inpMatrix, 2, 3));
	// Cofactor of 4th row
	result[3][0] = -determinant(nvMatrixCollapse(inpMatrix, 3, 0));
	result[3][1] =  determinant(nvMatrixCollapse(inpMatrix, 3, 1));
	result[3][2] = -determinant(nvMatrixCollapse(inpMatrix, 3, 2));
	result[3][3] =  determinant(nvMatrixCollapse(inpMatrix, 3, 3));
    // Adjoint is TRANSPOSE of matrix containing cofactors
    return transpose(result);
}

// Returns a matrix which is the Adjoint of inpMatrix.
float3x3 nvAdjoint(float3x3 inpMatrix)
{
    float3x3 result = (float3x3)0;
	// Cofactor of top-left 3×3 matrix
	result[0][0] =  determinant(nvMatrixCollapse(inpMatrix, 0, 0));
	result[0][1] = -determinant(nvMatrixCollapse(inpMatrix, 0, 1));
	result[0][2] =  determinant(nvMatrixCollapse(inpMatrix, 0, 2));
	result[1][0] = -determinant(nvMatrixCollapse(inpMatrix, 1, 0));
	result[1][1] =  determinant(nvMatrixCollapse(inpMatrix, 1, 1));
	result[1][2] = -determinant(nvMatrixCollapse(inpMatrix, 1, 2));
	result[2][0] =  determinant(nvMatrixCollapse(inpMatrix, 2, 0));
	result[2][1] = -determinant(nvMatrixCollapse(inpMatrix, 2, 1));
	result[2][2] =  determinant(nvMatrixCollapse(inpMatrix, 2, 2));
    // Adjoint is TRANSPOSE of matrix containing cofactors
    return transpose(result);
}

// Returns a matrix which is the Inverse of inpMatrix.
float4x4 nvInverse(float4x4 inpMatrix)
{
    float4x4 outMatrix = (float4x4)0;
    float det = determinant(inpMatrix);
    if ( det != 0.0 )
	    outMatrix = (1.0/ det) * nvAdjoint(inpMatrix);
    return outMatrix;
}

// Returns a matrix which is the Inverse of inpMatrix.
float3x3 nvInverse(float3x3 inpMatrix)
{
    float3x3 outMatrix = (float3x3)0;
    float det = determinant(inpMatrix);
    if ( det != 0.0 )
	    outMatrix = (1.0/ det) * nvAdjoint(inpMatrix);
    return outMatrix;
}

#endif /* _NVINVERSE_H */

///////////////////////////////////////////// eof ///
