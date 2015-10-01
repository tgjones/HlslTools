//--------------------------------------------------------------------------------------
// File: CreateTangentPatch.fxh
//
// This effect file contains functions for creating the Tangent patches as outlined
// by the ACC technique.
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Helper function
//--------------------------------------------------------------------------------------
void BezierRaise(inout float3 pQ[3], out float3 pC[4])
{
    pC[0] = pQ[0];
    pC[3] = pQ[2];

    for( int i=1; i<3; i++ ) 
    {
        pC[i] = ( 1.0f / 3.0f ) * ( pQ[i - 1] * i + ( 3.0f - i ) * pQ[i] );
    }
}

//--------------------------------------------------------------------------------------
// Computes the tangent patch from the input bezier patch
//--------------------------------------------------------------------------------------
void ComputeTanPatch(in float3 vIn[16], inout float3 vOut[16], in float fCWts[4], in float3 vCorner[4], in float3 vCornerLocal[4], in const uint cX, in const uint cY)
{
    float3 vQuad[3];
    float3 vQuadB[3];
    float3 vCubic[4];

    // boundary edges are really simple...
    vQuad[0] = vCornerLocal[0];
    vQuad[2] = vCornerLocal[1];
    vQuad[1] = 3.0f*(vIn[2*cX+0*cY]-vIn[1*cX+0*cY]);

    BezierRaise(vQuad,vCubic);
    vOut[1*cX + 0*cY] = vCubic[1];
    vOut[2*cX + 0*cY] = vCubic[2];

    vQuad[0] = vCornerLocal[2];
    vQuad[2] = vCornerLocal[3];
    vQuad[1] = 3.0f*(vIn[2*cX+3*cY]-vIn[1*cX+3*cY]);

    BezierRaise(vQuad,vCubic);
    vOut[1*cX + 3*cY] = vCubic[1];
    vOut[2*cX + 3*cY] = vCubic[2];

    // two internal edges - this is where work happens...
    float3 vA,vB,vC,vD,vE;
    float fC0,fC1;
    vQuad[1] = 3.0f*(vIn[2*cX+2*cY]-vIn[1*cX+2*cY]);
    // also do "second" scan line
    vQuadB[1] = 3.0f*(vIn[2*cX+1*cY]-vIn[1*cX+1*cY]);

    vD = 3.0f*(vIn[1*cX + 2*cY] - vIn[0*cX + 2*cY]);
    vE = 3.0f*(vIn[1*cX + 1*cY] - vIn[0*cX + 1*cY]); // used later...

    fC0 = fCWts[3];
    fC1 = fCWts[0];

    // sign flip
    vA = -vCorner[3];
    vB = 3.0f*(vIn[0*cX + 1*cY] - vIn[0*cX + 2*cY]);
    vC = -vCorner[0];

    vQuad[0] = 1.0f/3.0f*(2.0f*fC0*vB - fC1*vA) + vD;
    vQuadB[0] = 1.0f/3.0f*(fC0*vC - 2.0f*fC1*vB) + vE;

    // do end of strip - same as before, but stuff is switched around...
    vC = vCorner[2];
    vB = 3.0f*(vIn[3*cX + 2*cY] - vIn[3*cX + 1*cY]);
    vA = vCorner[1];

    vD = 3.0f*(vIn[2*cX + 1*cY] - vIn[3*cX + 1*cY]);
    vE = 3.0f*(vIn[2*cX + 2*cY] - vIn[3*cX + 2*cY]);
    
    fC0 = fCWts[1];
    fC1 = fCWts[2];
 
    vQuadB[2] = 1.0f/3.0f*(2.0f*fC0*vB - fC1*vA) + vD;
    vQuad[2] = 1.0f/3.0f*(fC0*vC - 2.0f*fC1*vB) + vE;

    vQuadB[2] *= -1.0f;
    vQuad[2] *= -1.0f;

    BezierRaise(vQuad,vCubic);

    vOut[0*cX + 2*cY] = vCubic[0];
    vOut[1*cX + 2*cY] = vCubic[1];
    vOut[2*cX + 2*cY] = vCubic[2];
    vOut[3*cX + 2*cY] = vCubic[3];

    BezierRaise(vQuadB,vCubic);

    vOut[0*cX + 1*cY] = vCubic[0];
    vOut[1*cX + 1*cY] = vCubic[1];
    vOut[2*cX + 1*cY] = vCubic[2];
    vOut[3*cX + 1*cY] = vCubic[3];
}