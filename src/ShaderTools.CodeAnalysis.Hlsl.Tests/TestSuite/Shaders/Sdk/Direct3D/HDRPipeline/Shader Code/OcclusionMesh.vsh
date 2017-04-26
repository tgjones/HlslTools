//======================================================================
//
//      HIGH DYNAMIC RANGE RENDERING DEMONSTRATION
//      Written by Jack Hoxley, November 2005
//
//======================================================================



//------------------------------------------------------------------
//  GLOBAL VARIABLES
//------------------------------------------------------------------
float4x4 matWorldViewProj;                  // Used to transform the incoming geometry
float4x4 matInvTPoseWorld;                  // Used to transform the incoming normals


//------------------------------------------------------------------
//  I/O STRUCT DEFINITIONS
//------------------------------------------------------------------
struct VS_INPUT
{

    float3 position : POSITION;
    float3 normal : NORMAL;
    
};
    
struct VS_OUTPUT
{

    float4 position : POSITION;
    float4 diffuse : COLOR;
    
};
    
    
    
//------------------------------------------------------------------
//  SHADER ENTRY POINT
//  The lighting computed for this mesh is a very simple directional
//  white light. A more dynamic/configurable light is possible by
//  exposing the variables in this function as constants for the
//  application to configure.
//------------------------------------------------------------------
VS_OUTPUT main( in VS_INPUT vertex )
{

    // Declare the output vertex information
    // -------------------------------------
        VS_OUTPUT output = ( VS_OUTPUT )0;
    
    
    // Transform the incoming position into projection space
    // -----------------------------------------------------
        output.position = mul( float4( vertex.position, 1.0f ), matWorldViewProj );
        
    // Compute the lighting
    //---------------------
        float3 n = normalize( mul( vertex.normal, matInvTPoseWorld ) );
        float l = saturate( dot( n, float3( 0.0f, 1.0f, 0.0f ) ) );
        
    // Apply a basic ambient term, making sure the geometry isn't rendered black
    //--------------------------------------------------------------------------
        l = max( 0.3f, l );
    
    // Copy the computed colour directly to the output
    // -----------------------------------------------
        output.diffuse = float4( l, l, l, 1.0f );
    
    
    // Return the processed vertex data
    // --------------------------------
        return output;
    
}