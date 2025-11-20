Shader "Custom/Template"
{
     Properties
     {
        _Color ("Color", Color) = (1,0,0,1)
     }

     SubShader
     {

        Pass
        {
            Name "Template"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #include "UnityCG.cginc"

            // --- Constants --- 


            // --- Variables (That must be set) --- 

 
            // --- Variables (Set in shader) --- 



            // --- Structs --- 
            struct TestStruct
            {
                float testValue; 
            };

            // --- Buffers  (Must be set) --- 
            StructuredBuffer<TestStruct> TestBuffer; 
            int _testDataCount;


            // These are necessary.
            struct appdata
            {
                float4 vertex : POSITION; 
                float2 uv : TEXCOORD0; 
            };

            // These are necessary.
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
            };

            // These are necessary.
            v2f vert(appdata v)
            {                
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, o.vertex).xyz;

                return o;
            }


            float4 frag(v2f i) : SV_Target
            {
                // Gives a blue colour. 
                return float4(0.0, 0.0, 1.0, 1.0); 
            }
            ENDCG
        }
     }
}
