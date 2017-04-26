// ParticlesGS.fx
// Copyright (c) 2005 Microsoft Corporation. All rights reserved.
//

struct VSPipeIn
{
    float3 pos          : POSITION;
    float3 norm         : NORMAL;
    float3 dir          : DIRECTION;
    float2 timerNtype   : TIMERNTYPE;
    float3 targetdir    : TARGETDIR;
    uint currentFace    : CURRENTFACE;
    uint leaves         : LEAVES;
    float pipelife      : PIPELIFE;
};

struct VSParticleDrawOut
{
    float3 pos : SV_Position;
    float4 color : COLOR0;
    float radius : RADIUS;
};

struct VSSceneIn
{
    float3 pos : POSITION;
    float3 norm : NORMAL;
    float2 tex : TEXTURE0;
};

struct PSSceneIn
{
    float4 pos : SV_Position;
    float3 tex : TEXTURE0;
    float4 color : COLOR0;
};

struct VSSkyIn
{
    float3 pos : POSITION;
    float3 norm : NORMAL;
    float2 tex : TEXTURE0;
    uint VertexID : SV_VertexID;
};

struct PSSkyIn
{
    float4 pos : SV_Position;
    float3 tex : TEXTURE0;
};

cbuffer cb0
{
    float4x4 g_mWorldViewProj;
    float g_fGlobalTime;
    float g_fUndulate;
    float4 vMaterialSpec;
    float4 vMaterialDiff;
};

cbuffer cbImmutable
{
    float3 g_positions[7] =
    {
        float3( 1.0f, 0.0f,    0 ),
        float3( 0.5f, 0.866f,  0 ),
        float3( -0.5f, 0.866f, 0 ),
        float3( -1.0f, 0.0f,   0 ),
        float3( -0.5, -0.866f, 0 ),
        float3( 0.5, -0.866f,  0 ),
        float3( 1.0f, 0.0f,    0 ),
    };

    float3 g_leafpositions[4] =
    {
        float3( -1.5, 2, 0 ),
        float3( 1.5, 2, 0 ),
        float3( -1.5, 0, 0 ),
        float3( 1.5, 0, 0 ),
    };
    float2 g_leaftexcoords[4] = 
    { 
        float2(0,0), 
        float2(1,0),
        float2(0,1),
        float2(1,1),
    };
    
    float3 g_vLightDir = {-0.3,0.9056,-0.3};
};

cbuffer cbUIUpdates
{   
    float g_fLifeSpan;
    float g_fLifeSpanVar;
    float g_fRadiusMin;
    float g_fRadiusMax;
    float g_fGrowTime;
    float g_fStepSize;
    float g_fTurnRate;
    float g_fTurnSpeed;
    float g_fLeafRate;
    float g_fShrinkTime;
    uint g_uMaxFaces;
};

Texture2DArray g_tx2dArray;
Texture2D g_txDiffuse;

SamplerState g_samLinear
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Wrap;
    AddressV = Wrap;
};

SamplerState g_samClamp
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

Texture1D g_txRandom;
SamplerState g_samPoint
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Wrap;
};

//
//Input Buffers
//
Buffer<float3> g_adjBuffer;
Buffer<float4> g_triCenterBuffer;

//
// Rendering States
//
RasterizerState DisableCulling
{
    CullMode = NONE;
};

RasterizerState EnableCulling
{
    CullMode = BACK;
};

DepthStencilState DisableDepthTestWrite
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};

DepthStencilState EnableDepthTestWrite
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = 0;
};

//
// Explanation of different pipe sections
//
#define PT_START    0 //Start section - this section finds a location on the mesh and puts a GROW there
#define PT_GROW     1 //Grow section - this section spawns a new STATIC and a new GROW to continue growing
#define PT_SHRINK   2 //Shrink section - this section shrinks
#define PT_STATIC   3 //Static section - this section has grown and will stay indefinitly

//
// Passthrough vertex shader for pipes
//
VSPipeIn VSPassThrough(VSPipeIn input)
{
    return input;
}

//
// Simple VS for mesh rendering
//
PSSceneIn VSScene( VSSceneIn input )
{
    PSSceneIn output;
    
    output.pos = mul( float4(input.pos,1), g_mWorldViewProj );
    output.color = float4(0,0,0,1);
    output.tex = float3(input.tex, 0);
    
    return output;
}

//
// VS for the skybox
//
PSSkyIn VSSkymain(VSSkyIn input)
{
    PSSkyIn output;
    
    //
    // Transform the vert to view-space
    //
    float4 v4Position = mul(float4(input.pos, 1), g_mWorldViewProj);
    output.pos = v4Position;
    
    //determine which face of the cube we're on
    uint iFace = input.VertexID/uint(6);
    output.tex = float3(input.tex, float(iFace) );
    
    return output;
}

//
// Sample a random direction from our random texture
//
float3 RandomDir(float fOffset)
{
    float tCoord = (g_fGlobalTime + fOffset) / 1024.0;
    return g_txRandom.SampleLevel( g_samPoint, tCoord, 0 );
}

float3 FixDir( float3 dir, float3 currentNorm )
{
    float3 left = cross( dir, currentNorm );
    float3 newDir = cross( currentNorm, left );
    return normalize(newDir);
}

void GSHandleStart(VSPipeIn input, inout PointStream<VSPipeIn> PointOutputStream)
{   
    VSPipeIn output = input;
    
    if(input.timerNtype.x == 0)
    {
        output.pos = g_triCenterBuffer.Load( input.currentFace*2 );
        output.dir = g_triCenterBuffer.Load( input.currentFace*2 + 1);
        output.targetdir = output.dir;
        
        float3 newNorm = normalize(RandomDir( input.currentFace ));
        output.norm = FixDir( newNorm, output.dir );
        
        float3 rand = normalize(RandomDir( input.currentFace + 100 ));
        float LifeVar = 10.0*rand.x;
        output.timerNtype.x = g_fLifeSpan+1.0;
        output.timerNtype.y = PT_GROW;
        output.currentFace = input.currentFace;
        output.pipelife = -LifeVar; 
    }
    else
        output.timerNtype.x --;
        
    PointOutputStream.Append(output);
}

void GSHandleGrow(VSPipeIn input, inout PointStream<VSPipeIn> PointOutputStream)
{   
    VSPipeIn output = input;
    float3 normRand;
    
    if(output.dir.x != output.targetdir.x || 
       output.dir.y != output.targetdir.y ||
       output.dir.z != output.targetdir.z )
    {
        //turn us towards our target
        float3 diffDir = output.targetdir - output.dir;
        if( length(diffDir) < g_fTurnSpeed )
            output.dir = output.targetdir;
        else
            output.dir = g_fTurnSpeed*normalize(diffDir) + output.dir;
    }
    else
    {
        //see if it's time to turn
        normRand = normalize( RandomDir( float(input.currentFace) ) );
        if( abs(normRand.x) < g_fTurnRate )
        {
            //if so, pick a new direction
            output.targetdir = normalize( RandomDir( 0 ) );
        }   
    }
            
    output.pos += output.dir*g_fStepSize;
    output.norm = FixDir( output.norm, output.dir );
    output.timerNtype.x = g_fLifeSpan;
    output.leaves = 0;  
    output.pipelife ++;

    normRand = normalize( RandomDir( output.currentFace+100 ) );
    if( abs(normRand.x) < g_fLeafRate )
        output.leaves = abs(normRand.y)*2000;
    
    if( output.pipelife > g_fLifeSpan )
    {
        output.timerNtype.x = 0;
        output.pipelife = 0;
        output.timerNtype.y = PT_START;
        output.currentFace = abs(normRand.z)*g_uMaxFaces;
    }
        
    PointOutputStream.Append(output);
}

//
// Main pipe crawling function
//
[maxvertexcount(2)]
void GSAdvancePipesMain(point VSPipeIn input[1], inout PointStream<VSPipeIn> PointOutputStream)
{
    if( PT_START == input[0].timerNtype.y )
    {
        GSHandleStart( input[0], PointOutputStream );
    }
    else
    {       
        if( PT_GROW == input[0].timerNtype.y )
            GSHandleGrow( input[0], PointOutputStream );
            
        //emit us as a static
        VSPipeIn output = input[0];
        output.timerNtype.y = PT_STATIC;
        output.timerNtype.x -= 1;
        output.pipelife ++;

        if(0 != output.timerNtype.x && output.pipelife < g_fLifeSpan)
            PointOutputStream.Append( output );
    }
}

////////////////////////////////
// Crawling Pipe Functions
////////////////////////////////

void GSCrawlHandleStart(VSPipeIn input, inout PointStream<VSPipeIn> PointOutputStreamCrawl )
{   
    VSPipeIn output = input;
    
    if(input.timerNtype.x == 0)
    {
        output.pos = g_triCenterBuffer.Load( input.currentFace*2 );
        output.norm = g_triCenterBuffer.Load( input.currentFace*2 + 1);
        output.targetdir = output.pos;
        
        float3 newDir = normalize(RandomDir( input.currentFace ));
        output.dir = FixDir( newDir, output.norm );
        
        float3 rand = normalize(RandomDir( input.currentFace + 100 ));
        float LifeVar = 10.0*rand.x;
        output.timerNtype.x = g_fLifeSpan+1.0;
        output.timerNtype.y = PT_GROW;
        output.currentFace = input.currentFace;
        output.pipelife = -LifeVar; 
    }
    else
        output.timerNtype.x --;
        
    PointOutputStreamCrawl.Append(output);
}

void GSCrawlPicNewTarget( VSPipeIn input, inout PointStream<VSPipeIn> PointOutputStreamCrawl )
{
    VSPipeIn output = input;
    output.pos = output.targetdir;
    
    float neighbor1 = g_adjBuffer.Load( output.currentFace*3     );
    float neighbor2 = g_adjBuffer.Load( output.currentFace*3 + 1 );
    float neighbor3 = g_adjBuffer.Load( output.currentFace*3 + 2 );
    
    float3 center1 = g_triCenterBuffer.Load( neighbor1*2 );
    float3 normal1 = g_triCenterBuffer.Load( neighbor1*2 + 1 );
    float3 dir1 = center1 - output.pos;
    
    float3 center2 = g_triCenterBuffer.Load( neighbor2*2 );
    float3 normal2 = g_triCenterBuffer.Load( neighbor2*2 + 1 );
    float3 dir2 = center2 - output.pos;
    
    float3 center3 = g_triCenterBuffer.Load( neighbor3*2 );
    float3 normal3 = g_triCenterBuffer.Load( neighbor3*2 + 1 );
    float3 dir3 = center3 - output.pos;
    
    float3 normRand = normalize( RandomDir( float(input.currentFace) ) );
    if( abs(normRand.x) < g_fTurnRate )
    {
        output.dir = RandomDir( 15 );
    }
    
    float d1 = dot( output.dir, normalize(dir1) );
    float d2 = dot( output.dir, normalize(dir2) );
    float d3 = dot( output.dir, normalize(dir3) );
    
    if( neighbor1 < 40000000 && d1 > d2 && d1 > d2 )
    {
    
        output.dir = FixDir( output.dir, normal1 );
        output.norm = normal1;
        output.currentFace = neighbor1;
        output.targetdir = center1;
    }
    
    else if( neighbor2 < 40000000 && d2 > d1 && d2 > d3 )
    {
        output.dir = FixDir( output.dir, normal2 );
        output.norm = normal2;
        output.currentFace = neighbor2;
        output.targetdir = center2;
    }
    
    else
    {
        output.dir = FixDir( output.dir, normal3 );
        output.norm = normal3;
        output.currentFace = neighbor3;
        output.targetdir = center3;
    }
    
    PointOutputStreamCrawl.Append( output );
}

void GSCrawlHandleGrow(VSPipeIn input, inout PointStream<VSPipeIn> PointOutputStreamCrawl)
{   
    float fLen = length( input.pos - input.targetdir );
    if( fLen < g_fStepSize )
    {
        GSCrawlPicNewTarget( input, PointOutputStreamCrawl );
    }
    else
    {
        VSPipeIn output = input;
        float distToTarget = length( output.targetdir - output.pos );
        float3 uncorrectedPos = output.pos + output.dir*distToTarget;
        float3 delta = output.targetdir - uncorrectedPos;
        output.dir = (uncorrectedPos + 0.5*delta) - output.pos;
        output.dir = normalize( output.dir );
        output.pos += output.dir*g_fStepSize;
        output.norm = FixDir( output.norm, output.dir );
        output.timerNtype.x = g_fLifeSpan;
        output.leaves = 0;  
    
        float3 normRand = normalize( RandomDir( output.currentFace ) );
        if( abs(normRand.x) < g_fLeafRate )
            output.leaves = abs(normRand.y)*2000;
    
        if( output.pipelife > g_fLifeSpan )
        {
            output.timerNtype.x = 0;
            output.pipelife = 0;
            output.timerNtype.y = PT_START;
            output.currentFace = abs(normRand.z)*g_uMaxFaces;
        }
    
        PointOutputStreamCrawl.Append(output);
    }
}

//
// Main pipe crawling function
//
[maxvertexcount(2)]
void GSCrawlPipesMain(point VSPipeIn input[1], inout PointStream<VSPipeIn> PointOutputStreamCrawl)
{
    if( PT_START == input[0].timerNtype.y )
    {
        GSCrawlHandleStart( input[0], PointOutputStreamCrawl );
    }
    else
    {       
        if( PT_GROW == input[0].timerNtype.y )
            GSCrawlHandleGrow( input[0], PointOutputStreamCrawl );
            
        //emit us as a static
        VSPipeIn output = input[0];
        output.timerNtype.y = PT_STATIC;
        output.timerNtype.x -= 1;
        output.pipelife ++;

        if(0 != output.timerNtype.x && output.pipelife < g_fLifeSpan)
            PointOutputStreamCrawl.Append( output );
    }
}

float GetRadius( float fMinRad, float timer, float pipetime )
{
    float fTime = saturate( (g_fLifeSpan-timer)/g_fGrowTime );
    float fRad = fMinRad + ((g_fRadiusMax - fMinRad)*(fTime));
    
    //shrink amount
    if( g_fLifeSpan - pipetime < g_fShrinkTime )
    {
        fRad *= (g_fLifeSpan - pipetime)/g_fShrinkTime;
    }
    
    return fRad;
}

void GSOutputPoint( float3x3 m, float3 inpos, int i, float ty, float timer, float pipetime, inout TriangleStream<PSSceneIn> TriangleOutputStream )
{
    PSSceneIn output;

    float fRad = GetRadius( g_fRadiusMin, timer, pipetime );
    float3 pos = mul( g_positions[i]*fRad, m );
    float3 norm = normalize(pos);
    pos += inpos;
    output.pos = mul( float4(pos,1), g_mWorldViewProj );
    output.color = dot( g_vLightDir, norm );
    output.color.a = 1;
    output.tex.x = float(i)/6.0;
    output.tex.y = ty;
    output.tex.z = 0;
    TriangleOutputStream.Append(output);
}

void GSOutputLeaf( float3x3 m, float3 inpos, float timer, float pipetime, uint leaf, inout TriangleStream<PSSceneIn> TriangleOutputStream )
{
    PSSceneIn output;
    float fLeafMul = 2.0;
    float fRad = GetRadius( 0.0, timer, pipetime );
    output.color = abs( dot( g_vLightDir, m._m00_m01_m02 ) );
    output.color.a = 1;
    uint iLeafTex = 1+ leaf - (leaf/(uint)5)*(uint)5;
    output.tex.z = float( iLeafTex );
        
    // Emit two new triangles
    //loops cannot index constant buffers right now
    for(int i=0; i<4; i++)
    {
        float3 pos = mul( g_leafpositions[i]*fRad*fLeafMul, m );
        pos += inpos;
        output.pos = mul( float4(pos,1), g_mWorldViewProj );
        output.tex.xy = g_leaftexcoords[i];
        TriangleOutputStream.Append(output);
    }
    
    TriangleOutputStream.RestartStrip();
}

//
// GS for rendering the pipes.
//
[maxvertexcount(20)]
void GSScenemain(line VSPipeIn input[2], inout TriangleStream<PSSceneIn> TriangleOutputStream)
{
    if( PT_STATIC == input[1].timerNtype.y )
    {
        //
        // setup the coordinate systems for each point
        //
        float3 left0 = cross( input[0].dir, input[0].norm );
        float3x3 m0 = float3x3( -left0, input[0].norm, input[0].dir );
        float3 left1 = cross( input[1].dir, input[1].norm );
        float3x3 m1 = float3x3( -left1, input[1].norm, input[1].dir );

        float3 pos0 = input[0].pos;
        float3 pos1 = input[1].pos;
        
        //loops don't index constant buffers correctly yet
        for(int i=0; i<7; i++)
        {
            GSOutputPoint( m0, pos0, i, 0.0, input[0].timerNtype.x, input[0].pipelife, TriangleOutputStream );
            GSOutputPoint( m1, pos1, i, 1.0, input[1].timerNtype.x, input[1].pipelife, TriangleOutputStream );
        }
        TriangleOutputStream.RestartStrip();
        
        //if we have leaves, draw them
        if( input[0].leaves > 0)
        {
            GSOutputLeaf( m0, pos0, input[0].timerNtype.x, input[0].pipelife, input[0].leaves, TriangleOutputStream );
        }
    }
}

//
// PS for rendering texture * color
//
float4 PSScenemain(PSSceneIn input) : SV_Target
{   
    float4 color = g_tx2dArray.Sample( g_samLinear, input.tex )*input.color;
    if(color.a < 0.5)
        discard;
    return color;
}

//
// PS for rendering transparent non-textured materials
//
float4 PSAlphamain(PSSceneIn input) : SV_Target
{   
    float4 col = vMaterialDiff*input.color;
    col.a = 0.5;
    return col;
}

//
// PS for rendering white materials
//
float4 PSTextureOnlymain(PSSceneIn input) : SV_Target
{   
    float4 col = g_txDiffuse.Sample( g_samClamp, input.tex );
    return col;
}

//
// PS for rendering the skybox
//
float4 PSQuadmain(PSSkyIn input) : SV_Target
{
    float4 color = g_tx2dArray.Sample( g_samClamp, input.tex );
    return color;
}

//
// RenderPipes - renders pipes on the screen
//
technique10 RenderPipes
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSPassThrough() ) );
        SetGeometryShader( CompileShader( gs_4_0, GSScenemain() ) );
        SetPixelShader( CompileShader( ps_4_0, PSScenemain() ) );

        SetRasterizerState( DisableCulling );
        SetDepthStencilState( EnableDepthTestWrite, 0 );
    }  
}

//
// AdvancePipes - advances the pipe system one time step
//
GeometryShader gsStreamOut = ConstructGSWithSO( CompileShader( gs_4_0,  GSAdvancePipesMain() ), "POSITION.xyz; NORMAL.xyz; DIRECTION.xyz; TIMERNTYPE.xy; TARGETDIR.xyz; CURRENTFACE.x; LEAVES.x; PIPELIFE.x" );
technique10 AdvancePipes
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSPassThrough() ) );
        SetGeometryShader( gsStreamOut );
        SetPixelShader( NULL );
        
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

//
// AdvancePipesCrawl - advances the pipe system one time step (crawl method)
//
GeometryShader gsStreamOutCrawl = ConstructGSWithSO( CompileShader( gs_4_0, GSCrawlPipesMain() ), "POSITION.xyz; NORMAL.xyz; DIRECTION.xyz; TIMERNTYPE.xy; TARGETDIR.xyz; CURRENTFACE.x; LEAVES.x; PIPELIFE.x" );
technique10 AdvancePipesCrawl
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSPassThrough() ) );
        SetGeometryShader( gsStreamOutCrawl );
        SetPixelShader( NULL );
        
        SetDepthStencilState( DisableDepth, 0 );
    }  
}

//
// RenderMesh - simple technique to render a mesh
//
technique10 RenderMesh
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSScene() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSTextureOnlymain() ) );
        
        SetRasterizerState( EnableCulling );
        SetDepthStencilState( EnableDepthTestWrite, 0 );
    }  
}

//
// RenderSkybox - render the skybox
//
technique10 RenderSkybox
{
    pass p0
    {
        SetVertexShader( CompileShader( vs_4_0, VSSkymain() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PSQuadmain() ) );
        
        SetDepthStencilState( DisableDepthTestWrite, 0 );
    }  
}