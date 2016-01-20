Shader "Unlit/Crosshair"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
        _InnerRadius("Inner Radius", Float) = 0.21
        _RadiusOffset("Radius Offset", Float) = 0.1
        _Thickness("Thickness", Float) = 0.02
        _CircleSpeed("Circle Speed", Float) = 7
        _Speed("Speed", Float) = 10
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            static const float PI = 3.14159265;

			fixed4 _Color;
            float _InnerRadius;
            float _RadiusOffset;
            float _Thickness;
            float _CircleSpeed;
            float _Speed;

            struct v2f {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            fixed4 drawCirclePoint(float radius, float relX, float relY, float angle, float completeness)
            {
                float circ = (relX) * (relX) + (relY) * (relY) - (radius * radius);
                float intensity = 1 - (abs(circ) / _Thickness);
                float val = lerp(0, 1, intensity);
                float realpos = 0;
                if (relY >= 0)
                {
                    if (relX >= 0)
                    {
                        realpos = 0.5 * PI - angle;
                    }
                    else
                    {
                        realpos = 1.5 * PI + angle;
                    }
                }
                else
                {
                    if (relX >= 0)
                    {
                        realpos = 0.5 * PI + angle;
                    }
                    else
                    {
                        realpos = 1.5 * PI - angle;
                    }
                }

                if (realpos <= (2 * PI) * completeness && intensity > 0)
                {
					fixed4 color = _Color * val;
					color.a = intensity;
					return color;
                }
                else
                {
                    return 0;
                }
            }

            v2f vert(float4 vertex : POSITION, float4 texcoord : TEXCOORD0)
            {
                v2f o;
                o.pos = mul(UNITY_MATRIX_MVP, vertex);
                o.uv = texcoord;
                return o;
            }
            
            fixed4 frag(v2f var) : SV_Target
            {
                fixed4 result = 0;

                float relX = var.uv.x - 0.5;
                float relY = var.uv.y - 0.5;
                float angle = atan(abs(relY) / abs(relX));

                for (float i = 0; i * _CircleSpeed < _Speed; i+=1.0)
                {
                    result += drawCirclePoint(_InnerRadius + i * _RadiusOffset, relX, relY, angle, (_Speed - i * _CircleSpeed) / _CircleSpeed);
                }
                return result;
            }
            ENDCG
        }
    }
}