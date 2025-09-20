Shader "Hidden/UnderwaterEffectShader"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _DepthMap ("Texture", 2D) = "black" {}
        [HideInInspector] _DepthStart ("Depth Start Distance", Float) = 1
        [HideInInspector] _DepthEnd   ("Depth End Distance",   Float) = 300
        [HideInInspector] _DepthColour("Depth Colour", Color) = (1,1,1,1)
        [HideInInspector] _WaterLevel("WaterLevel", Vector) = (0.5, 0.5, 0)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            
            //UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            sampler2D _CameraDepthTexture, _MainTex, _DepthMap;
            float _DepthStart, _DepthEnd;
            fixed4 _DepthColour;
            Vector _WaterLevel;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv       : TEXCOORD0;
                float4 vertex   : SV_POSITION;
                float4 screenPos: TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex    = UnityObjectToClipPos(v.vertex);
                o.uv        = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex); // maybe comment out
                return o;
            }

            

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.screenPos);
                Float s = (_WaterLevel.x * i.screenPos.x);
                Float s2 = _WaterLevel.y;
                Float s3 = _WaterLevel * 0.5;
                Float s4 = s + s2 - s3;

                if(i.screenPos.y > s4) { return col;}
                //float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                float depth = LinearEyeDepth(tex2D(_DepthMap, i.screenPos.xy ));
                //float w = saturate( (sceneZ - _DepthStart) / max(1e-5, (_DepthEnd - _DepthStart)) );
                depth = saturate( (depth - _DepthStart) / _DepthEnd );
                
                //fixed4 col = tex2D(_MainTex, i.screenPos);
                
                //fixed4 ambient = UNITY_LIGHTMODEL_AMBIENT;
                //return lerp(col, ambient.w * (0.5 * ambient + 0.5 * _DepthColour), depth);//

                return lerp(col, _DepthColour, depth);
            }
            ENDCG
        }
    }
}
