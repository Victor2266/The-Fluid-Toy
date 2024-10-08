Shader "Instanced/Particle2D Fuzzy" {
	Properties {
		// Add any necessary properties here if required
	}
	SubShader {

		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass {

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5

			#include "UnityCG.cginc"
			
			StructuredBuffer<float2> Positions2D;
			StructuredBuffer<float2> Velocities;
			StructuredBuffer<float2> DensityData;
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

			v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
			{
				float speed = length(Velocities[instanceID]);
				float speedT = saturate(speed / velocityMax);
				float colT = speedT;
				
				float3 centreWorld = float3(Positions2D[instanceID], 0);
				float3 worldVertPos = centreWorld + mul(unity_ObjectToWorld, v.vertex * scale);
				float3 objectVertPos = mul(unity_WorldToObject, float4(worldVertPos.xyz, 1));

				v2f o;
				o.uv = v.texcoord;
				o.pos = UnityObjectToClipPos(objectVertPos);
				o.colour = ColourMap.SampleLevel(linear_clamp_sampler, float2(colT, 0.5), 0);

				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				// Calculate the offset from the center of the particle (0.5, 0.5 is the center)
				float2 centerOffset = i.uv - 0.5;
				
				// Get the distance from the center (for a circular gradient)
				float dist = length(centerOffset) * 2; // Normalize the distance for the fade-out

				// Smoothly transition alpha from opaque at the center to transparent at the edge
				float alpha = saturate(1 - dist); // Fully opaque at center, fades towards edges

				// Slight soft edge using smoothstep
				alpha = smoothstep(0.0, 1.0, alpha);

				// Sample the colour and apply the calculated alpha
				float3 colour = i.colour;
				return float4(colour, alpha);
			}

			ENDCG
		}
	}
}
