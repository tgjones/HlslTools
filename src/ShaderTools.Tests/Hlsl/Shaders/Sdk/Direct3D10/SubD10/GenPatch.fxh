//--------------------------------------------------------------------------------------
// File: GenPatch.fxh
//
// This effect file contains functions to convert from a Catmull-Clark subdivision
// representation to a bicubic patch representation.
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// A sample extraordinary SubD quad is represented by the following diagram:
//
//                        15              Valences:
//                       /  \               Vertex 0: 5
//                      /    14             Vertex 1: 4
//          17---------16   /  \            Vertex 2: 5
//          | \         |  /    \           Vertex 3: 3
//          |  \        | /      13
//          |   \       |/      /         Prefixes:
//          |    3------2------12           Vertex 0: 9
//          |    |      |      |            Vertex 1: 12
//          |    |      |      |            Vertex 2: 16
//          4----0------1------11           Vertex 3: 18
//         /    /|      |      |
//        /    / |      |      |
//       5    /  8------9------10
//        \  /  /
//         6   /
//          \ /
//           7
//
// Where the quad bounded by vertices 0,1,2,3 represents the actual subd surface of interest
// The 1-ring neighborhood of the quad is represented by vertices 4 through 17.  The counter-
// clockwise winding of this 1-ring neighborhood is important, especially when it comes to compute
// the corner vertices of the bicubic patch that we will use to approximate the subd quad (0,1,2,3).
// 
// The resulting bicubic patch fits within the subd quad (0,1,2,3) and has the following control
// point layout:
//
//     12--13--14--15
//      8---9--10--11
//      4---5---6---7
//      0---1---2---3
//
// The inner 4 control points of the bicubic patch are a combination of only the vertices (0,1,2,3)
// of the subd quad.  However, the corner control points for the bicubic patch (0,3,15,12) are actually
// a much more complex weighting of the subd patch and the 1-ring neighborhood.  In the example above
// the bicubic control point 0 is actually a weighted combination of subd points 0,1,2,3 and 1-ring
// neighborhood points 17, 4, 5, 6, 7, 8, and 9.  We can see that the 1-ring neighbor hood is simply
// walked from the prefix value from the previous corner (corner 3 in this case) to the prefix 
// prefix value for the current corner.  We add one more vertex on either side of the prefix values
// and we have all the data necessary to calculate the value for the corner points.
//
// The edge control points of the bicubic patch (1,2,13,14,4,8,7,11) are also combinations of their 
// neighbors, but fortunately each one is only a combination of 6 values and no walk is required.
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Constant Buffers
//--------------------------------------------------------------------------------------
cbuffer cbTangentStencilConstants
{
    float g_TanM[1024]; // Tangent patch stencils precomputed by the application
    float g_fCi[16];    // Valence coefficients precomputed by the application
};

// Helps with getting tangent stencils from the g_TanM constant array
#define TANM(a,v) ( g_TanM[ Val[v]*64 + (a) ] )

//--------------------------------------------------------------------------------------
// Computes the interior vertices of the output bicubic patch.  The interior vertices
// (5,6,9,10) are a weighted combination of interior 4 vertices of the
// subdivicion patch.
//--------------------------------------------------------------------------------------
void ComputeInteriorVertices( out float4 Interior[4], in float4 Vert[MAX_POINTS], in uint Val[4] )
{
    // compute interior vertices
    Interior[0] = (Vert[0]*Val[0] + Vert[1]*2 +      Vert[2] +        Vert[3]*2)      / (5+Val[0]);
    Interior[1] = (Vert[0]*2 +      Vert[1]*Val[1] + Vert[2]*2 +      Vert[3])        / (5+Val[1]);
    Interior[2] = (Vert[0] +        Vert[1]*2 +      Vert[2]*Val[2] + Vert[3]*2)      / (5+Val[2]);
    Interior[3] = (Vert[0]*2 +      Vert[1] +        Vert[2]*2 +      Vert[3]*Val[3]) / (5+Val[3]);
}

//--------------------------------------------------------------------------------------
// Computes the corner vertices of the output UV patch.  The corner vertices are
// a weighted combination of all points that are "connected" to that corner by an edge.
// The interior 4 points of the original subd quad are easy to get.  The points in the
// 1-ring neighborhood around the interior quad are not.
//
// Because the valence of that corner could be any number between 3 and 16, we need to
// walk around the subd patch vertices connected to that point.  This is there the
// Pref (prefix) values come into play.  Each corner has a prefix value that is the index
// of the last value around the 1-ring neighborhood that should be used in calculating
// the coefficient of that corner.  The walk goes from the prefix value of the previous
// corner to the prefix value of the current corner.
//--------------------------------------------------------------------------------------
void ComputeCornerVertices( out float4 CornersB[4], out float4 CornersU[4], out float4 CornersV[4], 
                            in float4 Vert[MAX_POINTS], in uint Val[4], in uint Pref[4] )
{
    const uint mod4[8] = {0,1,2,3,0,1,2,3};
    const uint cCorners[4] = {0,3,15,12};
    const float fOWt = 1;
    const float fEWt = 4;
    
    // Loop through all four corners
    for(int i=0;i<4;i++) 
    { 
        // Figure out where to start the walk by using the previous corner's prefix value
        uint PrefIm1 = 0;
        uint uStart = 4;
        if( i )
        {
            PrefIm1 = Pref[i-1];
            uStart = PrefIm1;
        }
        
        // Setup the walk indices
        uint uTIndexStart = 2 - (i&1);
        uint uTIndex = uTIndexStart;

        // Calculate the N*N weight for the final value
        CornersB[i] = (Val[i]*Val[i])*Vert[i]; // n^2 part

        // Zero out the corners
        CornersU[i] = float4(0,0,0,0);
        CornersV[i] = float4(0,0,0,0);
            
        // Start the walk with the uStart prefix (the prefix of the corner before us)
        const uint uV = Val[i]  + ( ( i & 1 ) ? 1 : -1 );
        CornersB[i] += Vert[uStart] * fEWt;
        CornersU[i] += Vert[uStart] * TANM( uTIndex * 2, i );
        CornersV[i] += Vert[uStart] * TANM( ( ( uTIndex + uV ) % Val[i] ) * 2, i);

        // Gather all vertices between the previous corner's prefix and our own prefix
        // We'll do two at a time, since they always come in twos
        while(uStart < Pref[i]-1) 
        {
            ++uStart;
            CornersB[i] += Vert[uStart] * fOWt;
            CornersU[i] += Vert[uStart] * TANM( uTIndex * 2 + 1, i );
            CornersV[i] += Vert[uStart] * TANM( ( ( uTIndex + uV ) % Val[i] ) * 2 + 1, i );

            ++uTIndex;
            ++uStart;
            CornersB[i] += Vert[uStart] * fEWt;
            CornersU[i] += Vert[uStart] * TANM( ( uTIndex % Val[i] ) * 2, i );
            CornersV[i] += Vert[uStart] * TANM( ( ( uTIndex+uV)%Val[i]) * 2, i );
        }
        ++uStart;

        // Add in the last guy and make sure to wrap to the beginning if we're the last corner
        if (i == 3)
            uStart = 4; 
        CornersB[i] += Vert[uStart] * fOWt;
        CornersU[i] += Vert[uStart] * TANM( ( uTIndex % Val[i] ) * 2 + 1, i );
        CornersV[i] += Vert[uStart] * TANM( ( ( uTIndex + uV ) % Val[i] ) * 2 + 1, i );

        // Add in the guy before the prefix as well
        if (i)
            uStart = PrefIm1-1;
        else
            uStart = Pref[3]-1;
        uTIndex = uTIndexStart-1;

        CornersB[i] += Vert[uStart] * fOWt;
        CornersU[i] += Vert[uStart] * TANM( ( uTIndex % Val[i] ) * 2 + 1, i );
        CornersV[i] += Vert[uStart] * TANM( ( ( uTIndex + uV ) % Val[i] ) * 2 + 1, i );

        // We're done with the walk now.  Now we need to add the contributions of the original subd quad.
        CornersB[i] += Vert[mod4[i+1]] * fEWt;
        CornersB[i] += Vert[mod4[i+2]] * fOWt;
        CornersB[i] += Vert[mod4[i+3]] * fEWt;
        
        uTIndex = 0 + (i&1)*(Val[i]-1);
        uStart = mod4[i+1];
        CornersU[i] += Vert[uStart] * TANM( ( uTIndex % Val[i] ) * 2, i );
        CornersV[i] += Vert[uStart] * TANM( ( ( uTIndex + uV ) % Val[i] ) * 2, i );
        
        uStart = mod4[i+2];
        CornersU[i] += Vert[uStart] * TANM( ( uTIndex % Val[i] ) * 2 + 1, i );
        CornersV[i] += Vert[uStart] * TANM( ( ( uTIndex + uV ) % Val[i] ) * 2 + 1, i );

        uStart = mod4[i+3];
        uTIndex = (uTIndex+1)%Val[i];

        CornersU[i] += Vert[uStart] * TANM( ( uTIndex % Val[i] ) * 2, i );
        CornersV[i] += Vert[uStart] * TANM( ( ( uTIndex + uV ) % Val[i] ) * 2, i );

        // Normalize the corner weights
        CornersB[i] *= 1.0f / ( Val[i] * Val[i] + 5 * Val[i] ); // normalize
    }

    // fixup signs from directional derivatives...
    CornersU[1] *= -1;
    CornersU[2] *= -1;
    CornersV[3] *= -1;
    CornersV[2] *= -1;
}

//--------------------------------------------------------------------------------------
// Computes the edge vertices of the output bicubic patch.  The edge vertices
// (1,2,4,7,8,11,13,14) are a weighted (by valence) combination of 6 interior and 1-ring
// neighborhood points.  However, we don't have to do the walk on this one since we
// don't need all of the neighbor points attached to this vertex.
//--------------------------------------------------------------------------------------
void ComputeEdgeVertices( out float4 Edges[8], in float4 Vert[MAX_POINTS], in uint Val[4], in uint Pref[4] )
{
    // Precompute some weight values that we use multiple times below
    float val1 = 2 * Val[0] + 10;
    float val2 = 2 * Val[1] + 10;
    float val13 = 2 * Val[3] + 10;
    float val14 = 2 * Val[2] + 10;
    float val4 = val1;
    float val8 = val13;
    float val7 = val2;
    float val11 = val14;
    
    // compute edge points - horizontal
    Edges[0] = (Val[0]*2*Vert[0] + 4*Vert[1] + Vert[2] + Vert[3]*2 +
              2*Vert[Pref[0]-1] + Vert[Pref[0]])/(val1);
    Edges[1] = (4*Vert[0] + Val[1]*2*Vert[1] + Vert[2]*2 + Vert[3] +
              Vert[Pref[0]-1] + 2*Vert[Pref[0]])/(val2);

    Edges[2] = (2*Vert[0] + Vert[1] + 4*Vert[2] + Vert[3]*2*Val[3] +
               2*Vert[Pref[2]] + Vert[Pref[2]-1])/(val13);
    Edges[3] = (Vert[0] + 2*Vert[1] + Val[2]*2*Vert[2] + Vert[3]*4 +
               Vert[Pref[2]] + 2*Vert[Pref[2]-1])/(val14);

    // edge points - vertical
    Edges[4] = (Val[0]*2*Vert[0] + 2*Vert[1] + Vert[2] + Vert[3]*4 +
              2*Vert[4] + Vert[Pref[3]-1])/(val4);
    Edges[5] = (4*Vert[0] + Vert[1] + 2*Vert[2] + Vert[3]*2*Val[3] +
              Vert[4] + 2*Vert[Pref[3]-1])/(val8);

    Edges[6] = (2*Vert[0] + Val[1]*2*Vert[1] + 4*Vert[2] + Vert[3] +
              2*Vert[Pref[1]-1] + Vert[Pref[1]])/(val7);
    Edges[7] = (Vert[0] + 4*Vert[1] + Val[2]*2*Vert[2] + 2*Vert[3] +
               Vert[Pref[1]-1] + 2*Vert[Pref[1]])/(val11);
}

//--------------------------------------------------------------------------------------
// Creates a bezier patch from Interior points, Corner points, and Edge points
//--------------------------------------------------------------------------------------
void ConcatenateBezierPatch( in float4 Interior[4], in float4 Corners[4], in float4 Edges[8], out float4 Bez[PATCH_STRIDE] )
{
    Bez[5] = Interior[0];
    Bez[6] = Interior[1];
    Bez[10] = Interior[2];
    Bez[9] = Interior[3];
    
    Bez[0] = Corners[0];
    Bez[3] = Corners[1];
    Bez[15] = Corners[2];
    Bez[12] = Corners[3];
    
    Bez[1] = Edges[0];
    Bez[2] = Edges[1];
    Bez[13] = Edges[2];
    Bez[14] = Edges[3];
    Bez[4] = Edges[4];
    Bez[8] = Edges[5];
    Bez[7] = Edges[6];
    Bez[11] = Edges[7];
}

//--------------------------------------------------------------------------------------
// Generates a bicubic patch (without tangent patches) from a subd patch
//--------------------------------------------------------------------------------------
void GenPatchB( in float4 Vert[MAX_POINTS], in uint Val[4], in uint Pref[4],
                out float4 Bez[PATCH_STRIDE] )
{		
    // compute interior vertices and put them in the Bez array
    float4 Interior[4];
    ComputeInteriorVertices( Interior, Vert, Val );
    
    // Compute corner vertices.
    // We will ignore the corners for the tangent patches since they are handled
    // in a separate pass.
    float4 CornersB[4];
    float4 IgnoreCornersU[4];
    float4 IgnoreCornersV[4];
    ComputeCornerVertices( CornersB, IgnoreCornersU, IgnoreCornersV, Vert, Val, Pref );
    
    // Compute edge vertices
    float4 Edges[8];
    ComputeEdgeVertices( Edges, Vert, Val, Pref );
    
    // Add interiors, corners, and edges
    ConcatenateBezierPatch( Interior, CornersB, Edges, Bez );
}

//--------------------------------------------------------------------------------------
// Generates the tangent patches (without the bicubic patch) from a subd patch
//--------------------------------------------------------------------------------------
void GenPatchTanCompact( in float4 Vert[MAX_POINTS], in uint Val[4], in uint Pref[4],
                         out float4 TanUV[9] )
{   
    // Compute corner vertices for the UV tangent patches.
    // We will ignore the bicubic patch coefficients because they don't matter for calculating
    // the tangent patches.
    float4 IgnoreCornersB[4];
    float4 CornersU[4];
    float4 CornersV[4];
    ComputeCornerVertices( IgnoreCornersB, CornersU, CornersV, Vert, Val, Pref );
    
    // Store the corner vertices and some helper variables so that we can reconstruct.
    // the entire UV tangent patch when we reconstruct the bicubic surface
    float fCWts[4];
    fCWts[0] = g_fCi[Val[0]-3];
    fCWts[1] = g_fCi[Val[1]-3];
    fCWts[2] = g_fCi[Val[2]-3];
    fCWts[3] = g_fCi[Val[3]-3];

    TanUV[0] = CornersV[0];
    TanUV[1] = CornersV[1];
    TanUV[2] = CornersV[2];
    TanUV[3] = CornersV[3];
    TanUV[4] = CornersU[0];
    TanUV[5] = CornersU[1];
    TanUV[6] = CornersU[2];
    TanUV[7] = CornersU[3];
    TanUV[8].x = fCWts[0];
    TanUV[8].y = fCWts[1];
    TanUV[8].z = fCWts[2];
    TanUV[8].w = fCWts[3];
}