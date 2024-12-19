Shader "Instanced/Particle2D AoS"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"
            
            struct Particle
            {
                float2 density;           // density and near density
                float2 velocity;          // velocity
                float2 predictedPosition; // predicted position
                float2 position;          // current position
                float temperature;        // temperature
                int type;                 // fluid type
            };
            
            StructuredBuffer<Particle> Particles;
            float scale;
            float4 colA;
            Texture2D<float4> ColourMap;
            SamplerState linear_clamp_sampler;
            float velocityMax;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 colour : TEXCOORD1;
            };

            v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
            {
                // Get particle data
                Particle particle = Particles[instanceID];
                
                // Calculate velocity magnitude for color
                float speed = length(particle.velocity);
                float speedT = saturate(speed / velocityMax);
                float colT = speedT;
                
                // Calculate world position
                float3 centreWorld = float3(particle.position, 0);
                float3 worldVertPos = centreWorld + mul(unity_ObjectToWorld, v.vertex * scale);
                float3 objectVertPos = mul(unity_WorldToObject, float4(worldVertPos.xyz, 1));

                v2f o;
                o.uv = v.texcoord;
                o.pos = UnityObjectToClipPos(objectVertPos);
                o.colour = ColourMap.SampleLevel(linear_clamp_sampler, float2(colT, 0.5), 0);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 centreOffset = (i.uv.xy - 0.5) * 2;
                float sqrDst = dot(centreOffset, centreOffset);
                float delta = fwidth(sqrt(sqrDst));
                float alpha = 1 - smoothstep(1 - delta, 1 + delta, sqrDst);

                float3 colour = i.colour;
                return float4(colour, alpha);
            }

            ENDCG
        }
    }
}