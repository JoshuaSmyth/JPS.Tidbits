float4x4 World;
float4x4 View;
float4x4 Projection;

// TODO Can combine WorldViewProjection into 1 parameter

float3 CameraPosition;

// TODO: add effect parameters here.
float4 lightDirection = { 1, -0.7, 1, 0};
float textureScale = 0.06f;

texture gTex0;
sampler ColorMapSampler = sampler_state
{
   Texture = <gTex0>;
   MinFilter = ANISOTROPIC;
   MagFilter = ANISOTROPIC;
   MipFilter = Linear;   
   AddressU  = Wrap;
   AddressV  = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 Normal : TEXCOORD0;
    float2 Texture : TEXCOORD1;
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position.xyz,1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Normal = normalize(input.Normal);
    output.Texture = float2(input.Position.w, input.Normal.w);
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
   
   float4 colour = tex2D(ColorMapSampler, input.Texture);
   float4 light = -normalize(lightDirection);
   float ldn = max(0, dot(light, input.Normal));
   float ambient = 0.95f;

   return float4(colour.xyz * (ambient + ldn), 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
