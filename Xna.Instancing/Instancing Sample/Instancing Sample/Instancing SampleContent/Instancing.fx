float4x4 WVP;
texture cubeTexture;
float4x4 RotationMat;

sampler TextureSampler = sampler_state
{
    texture = <cubeTexture>;
    mipfilter = LINEAR;
    minfilter = LINEAR;
    magfilter = LINEAR;
};

struct InstancingVSinput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(InstancingVSinput input, 
										float4 instanceTransform : POSITION1, 
										float2 atlasCoord : TEXCOORD1,
										float4 color : COLOR1)
{
    VertexShaderOutput output;
	float4 p1 = mul(input.Position, RotationMat);
	float4 pos = p1 + instanceTransform;
    pos = mul(pos, WVP);

    output.Position = pos;
    output.TexCoord = float2((input.TexCoord.x / 2.0f) + (1.0f / 2.0f * atlasCoord.x), (input.TexCoord.y / 2.0f) + (1.0f / 2.0f * atlasCoord.y));
	output.Color = color;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.

    return input.Color;
}

technique Instancing
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
