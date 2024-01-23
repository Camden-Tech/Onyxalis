#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

float4x4 view_projection;
float2 u_Start; // Start position of the line
float2 u_End; // End position of the line
Texture2D u_WorldTexture; // Texture representing the world
SamplerState u_WorldTextureSampler; // Sampler for the texture
float u_Intensity; // Intensity of the light

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct PixelShaderInput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

PixelShaderInput MainVS(VertexShaderInput input)
{
    PixelShaderInput output;
    output.Position = mul(input.Position, view_projection);
    output.TexCoord = input.TexCoord;
    return output;
}

// Approximate line-of-sight calculation with fixed iterations
float LineOfSight(float2 start, float2 end, float intensity)
{
    float2 delta = end - start;
    const float maxSteps = 100.0; // Fixed number of steps
    float2 step = delta / maxSteps;
    
    float2 currentPos = start;
    for (int i = 0; i < maxSteps; i++)
    {
        float tileValue = u_WorldTexture.Sample(u_WorldTextureSampler, currentPos).r;
        if (tileValue < 0.5)
        {
            // Calculate light level based on intensity and distance
            float dist = length(currentPos - start);
            return max(0, 1 - dist / (intensity * 4));
        }
        currentPos += step;
    }
    return 1.0; // Full light level if unobstructed
}

float4 MainPS(PixelShaderInput input) : SV_TARGET
{
    float lightLevel = LineOfSight(u_Start, u_End, u_Intensity);
    return float4(lightLevel, lightLevel, lightLevel, 1.0);
}

technique BasicTechnique
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}