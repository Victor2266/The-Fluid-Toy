Shader "Instanced/MultiFluidParticle2D"
{
    // Properties is a special section in a Unity shader which allows you to expose parameters
    // to the Unity editor. These parameters can be changed in the Material Inspector for a
    // material that is using this shader. The parameters are used to control the appearance
    // of lava and steam particles in the shader. 
    // - LavaGlowIntensity: controls the brightness of lava particles
    // - LavaMinTemp and LavaMaxTemp: control the color of lava particles based on their
    //                                temperature
    // - SteamNoiseScale: controls the scale of the noise pattern used to simulate steam
    // - SteamTimeFactor: controls the speed of the noise pattern used to simulate steam
    Properties
    {
        _LavaGlowIntensity ("Lava Glow Intensity", Range(0, 2)) = 1.0
        _LavaMinTemp ("Lava Min Temperature", Float) = 800
        _LavaMaxTemp ("Lava Max Temperature", Float) = 1200
        _SteamNoiseScale ("Steam Noise Scale", Float) = 10
        _SteamTimeFactor ("Steam Time Factor", Float) = 1
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
            
            // Particle struct matching CPU side
            struct Particle
            {
                float2 density;
                float2 velocity;
                float2 predictedPosition;
                float2 position;
                float temperature;
                int type;
            };
            
            StructuredBuffer<Particle> Particles;
            float scale;
            Texture2D<float4> ColourMap;
            SamplerState linear_clamp_sampler;
            float velocityMax;
            
            // Added properties
            float _LavaGlowIntensity;
            float _LavaMinTemp;
            float _LavaMaxTemp;
            float _SteamNoiseScale;
            float _SteamTimeFactor;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 colour : TEXCOORD1;
                float valid : TEXCOORD2;
                float temperature : TEXCOORD3;
                int type : TEXCOORD4;
                float2 worldPos : TEXCOORD5;
            };

            // Simple noise function for steam effect
            float2 hash2D(float2 p)
            {
                float2 k = float2(0.3183099, 0.3678794);
                p = p * k + k.yx;
                return -1.0 + 2.0 * frac(16.0 * k * frac(p.x * p.y * (p.x + p.y)));
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(dot(hash2D(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
                               dot(hash2D(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
                          lerp(dot(hash2D(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
                               dot(hash2D(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
            }

            v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                Particle particle = Particles[instanceID];
                
                o.valid = particle.type != 0;
                o.type = particle.type;
                o.temperature = particle.temperature;
                
                float3 worldVertPos;
                if (o.valid)
                {
                    float3 centreWorld = float3(particle.position, 0);
                    worldVertPos = centreWorld + mul(unity_ObjectToWorld, v.vertex * scale);
                    o.worldPos = worldVertPos.xy;
                    
                    // Base color calculation based on type
                    switch(particle.type)
                    {
                        case 1: // Water
                            float speed = length(particle.velocity);
                            float speedT = saturate(speed / velocityMax);
                            o.colour = ColourMap.SampleLevel(linear_clamp_sampler, float2(speedT, 0.5), 0);
                            break;
                            
                        case 2: // Steam
                            o.colour = float3(0.8, 0.8, 0.8); // Base steam color
                            break;
                            
                        case 3: // Honey
                            o.colour = float3(0.8, 0.6, 0.2); // Golden color
                            break;
                            
                        case 4: // Lava
                            float tempFactor = saturate((particle.temperature - _LavaMinTemp) / (_LavaMaxTemp - _LavaMinTemp));
                            o.colour = lerp(float3(0.8, 0.2, 0), float3(1, 0.7, 0), tempFactor);
                            break;
                            
                        default:
                            o.colour = float3(1, 1, 1);
                            break;
                    }
                }
                else
                {
                    worldVertPos = float3(100000, 100000, 100000);
                    o.colour = float3(0, 0, 0);
                    o.worldPos = float2(0, 0);
                }

                float3 objectVertPos = mul(unity_WorldToObject, float4(worldVertPos.xyz, 1));
                o.uv = v.texcoord;
                o.pos = UnityObjectToClipPos(objectVertPos);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                if (!i.valid) {
                    discard;
                }
                
                float2 centreOffset = (i.uv.xy - 0.5) * 2;
                float sqrDst = dot(centreOffset, centreOffset);
                float delta = fwidth(sqrt(sqrDst));
                float baseAlpha = 1 - smoothstep(1 - delta, 1 + delta, sqrDst);
                
                float3 finalColor = i.colour;
                float alpha = baseAlpha;
                
                switch(i.type)
                {
                    case 2: // Steam
                        // Add noise-based displacement for steam
                        float2 noiseCoord = i.worldPos * _SteamNoiseScale + _Time.y * _SteamTimeFactor;
                        float noiseVal = noise(noiseCoord) * 0.5 + 0.5;
                        alpha *= noiseVal * 0.7; // Make steam more transparent and varied
                        break;
                        
                    case 4: // Lava
                        // Add glow effect for lava
                        float glowFactor = 1 - sqrDst; // More intense at center
                        finalColor *= 1 + glowFactor * _LavaGlowIntensity;
                        // Add subtle noise for lava texture
                        float2 lavaNoiseCoord = i.worldPos * 5 + _Time.y * 0.2;
                        float lavaNoise = noise(lavaNoiseCoord) * 0.1 + 0.9;
                        finalColor *= lavaNoise;
                        break;
                }
                
                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
}