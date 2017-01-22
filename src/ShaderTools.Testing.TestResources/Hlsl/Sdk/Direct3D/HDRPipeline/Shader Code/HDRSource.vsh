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



//------------------------------------------------------------------
//  I/O STRUCT DEFINITIONS
//------------------------------------------------------------------
struct VS_INPUT
{
    float3 position     : POSITION;     // The incoming, model space, coordinate
    float4 diffuse      : COLOR;        // The incoming vertex colour   
};
    
    
    
struct VS_OUTPUT
{
    float4 position : POSITION;         // Projection space vertex position
    float4 diffuse  : COLOR;            // Colour to be interpolated    
};
    
    
    
//------------------------------------------------------------------
//  SHADER ENTRY POINT
//------------------------------------------------------------------
VS_OUTPUT main( in VS_INPUT v )
{

    // Declare the output vertex information
    // -------------------------------------
        VS_OUTPUT output = ( VS_OUTPUT )0;
    
    
    // Transform the incoming position into projection space
    // -----------------------------------------------------
        output.position = mul( float4( v.position, 1.0f ), matWorldViewProj );
    
    
    // Copy the incoming colour directly to the output
    // -----------------------------------------------
        output.diffuse = v.diffuse;
    
    
    // Return the processed vertex data
    // --------------------------------
        return output;
        
}