Shader "Instanced/Particle2D SoA"
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
            
            struct ParticleSoA
            {
                float2[] density;           // density and near density
                float2[] velocity;          // velocity
                float2[] predictedPosition; // predicted position
                float2[] position;          // current position
                float[] temperature;        // temperature
                int[] type;                 // fluid type
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
                float valid : TEXCOORD2;  // Added to pass type validation to fragment shader
            };

            v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
            {
                v2f o;

                // Get particle data
                //Particle particle = Particles[instanceID];
                float2 pDensity = Particles.density[instanceID];
                float2 pVelocity = Particles.velocity[instanceID];
                float2 pPredictedPosition = Particles.predictedPosition[instanceID];
                float2 pPosition = Particles.position[instanceID];
                float ptemp = Particles.temperature[instanceID];
                int pType = Particles.type[instanceID];

                // Check if particle should be rendered
                o.valid = pType != 0;

                // If type is 0, move vertex far off screen
                float3 worldVertPos;
                if (o.valid)
                {
                    // Calculate velocity magnitude for color
                    float speed = length(pVelocity);
                    float speedT = saturate(speed / velocityMax);
                    float colT = speedT;
                    
                    // Calculate world position
                    float3 centreWorld = float3(pPosition, 0);
                    worldVertPos = centreWorld + mul(unity_ObjectToWorld, v.vertex * scale);
                    o.colour = ColourMap.SampleLevel(linear_clamp_sampler, float2(colT, 0.5), 0);
                }
                else
                {
                    // Move invalid particles far off screen
                    worldVertPos = float3(100000, 100000, 100000);
                    o.colour = float3(0, 0, 0);
                }

                float3 objectVertPos = mul(unity_WorldToObject, float4(worldVertPos.xyz, 1));
                o.uv = v.texcoord;
                o.pos = UnityObjectToClipPos(objectVertPos);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Discard fragment if particle type was 0
                if (!i.valid)
                {
                    discard;
                }
                
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