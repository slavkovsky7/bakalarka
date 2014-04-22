 // Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

 

    Shader "atmScatter" {

       Properties {

         _v4CameraPos("Camera Position",Vector) = (0,0,0,0)

         _v4LightDir("Light Direction",Vector) = (0,0,0,0)

         _cInvWaveLength("Inverse WaveLength",Color) = (0,0,0,0)

         _fCameraHeight("Camera Height",Float) = 0

         _fCameraHeight2("Camera Height2",Float) = 0

         _fOuterRadius("Outer Radius",Float) = 0

         _fOuterRadius2("Outer Radius 2",Float) = 0

         _fInnerRadius("Inner Radius",Float) = 0

         _fInnerRadius2("Inner Radius 2",Float) = 0

         _fKrESun("KrESun",Float) = 0

         _fKmESun("KmESun",Float) = 0

         _fKr4PI("Kr4PI",Float) = 0

         _fKm4PI("Km4PI",Float) = 0

         _fScale("Scale",Float) = 0

         _fScaleDepth("Scale Depth",Float) = 0

         _fScaleOverScaleDepth("Scale Over Scale Depth",Float) = 0

         _Samples("Samples",Float) = 0

         _G("G",Float) = 0

         _G2("G2",Float) = 0

       }

       SubShader {

         Tags {"Queue" = "Transparent" }

          Pass {

             Cull Front

             Blend One One

             

    CGPROGRAM

    #pragma vertex vert

    #pragma fragment frag

    #include "UnityCG.cginc"

     

    float4 _v4CameraPos;

    float4 _v4LightDir;

    float4 _cInvWaveLength;

    float _fCameraHeight;

    float _fCameraHeight2;    

    float _fOuterRadius;

    float _fOuterRadius2;    

    float _fInnerRadius;

    float _fInnerRadius2;

    float _fKrESun;

    float _fKmESun;

    float _fKr4PI;

    float _fKm4PI;

    float _fScale;

    float _fScaleDepth;

    float _fScaleOverScaleDepth;

    float _Samples;

    float _G;

    float _G2;

     

    struct v2f {

      float4 position : POSITION;

      float3 c0 : COLOR0;

      float3 c1 : COLOR1;

      float3 t0 : TEXCOORD0;

    };

     

    float expScale (float cos) {

       float x = 1 - cos;

       return _fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));

    }

     

    v2f vert (float4 vertex : POSITION) {

      float3 v3Pos = vertex.xyz;

      float3 v3Ray = v3Pos - _v4CameraPos.xyz;

      float fFar = length(v3Ray);

      v3Ray /= fFar;  

     

    float3 v3Start = _v4CameraPos.xyz;

     

     

      float v3StartAngle = dot(v3Ray,v3Start) / length(v3Start);

     

      float v3StartDepth = exp (_fScaleOverScaleDepth * ( - _fInnerRadius + _fCameraHeight));

     

      float v3StartOffset = v3StartDepth * expScale(v3StartAngle);

     

      float fSampleLength = fFar / _Samples;

      float fScaledLength = fSampleLength * _fScale;

      float3 sampleRay = v3Ray * fSampleLength;

      float3 samplePoint = v3Start + sampleRay * 0.5f;

     

      float3 frontColor = float3(0,0,0);

      float3 attenuate;

     

      for (int i = 0; i < 1; i++) {

        float height = length (samplePoint);

        float depth = exp(_fScaleOverScaleDepth * (_fInnerRadius - height));

        float lightAngle = dot(_v4LightDir.xyz, samplePoint) / height;

        float cameraAngle = dot(v3Ray, samplePoint) / height;

        float scatter = (v3StartOffset + depth * (expScale (lightAngle) - expScale (cameraAngle)));

     

        attenuate = exp(-scatter * (_cInvWaveLength.xyz * _fKr4PI + _fKm4PI));

        frontColor += attenuate * (depth * fScaledLength);

        samplePoint += sampleRay;

      }

     

      v2f o;

      o.position = mul( UNITY_MATRIX_MVP, vertex);

      o.t0 = _v4CameraPos.xyz - vertex.xyz;

      o.c0.rgb = frontColor * (_cInvWaveLength.xyz * _fKrESun);

      o.c1.rgb = frontColor * _fKmESun;

      return o;

    }

     

    float4 frag (v2f i) : COLOR {

      float cos = dot(_v4LightDir.xyz, i.t0) / length(i.t0);

      float cos2 = cos * cos;

      float miePhase = 1.5 * ((1.0 - _G2) / (2.0 + _G2)) * (1.0 + cos2) / pow(1.0 + _G2 - 2.0*_G*cos, 1.5);

      float rayleighPhase = 0.75 * (1.0 + cos2);

     

     

      float4 fragColor;

     

      fragColor.xyz = (rayleighPhase * i.c0) + (miePhase * i.c1);

      fragColor.w = fragColor.z;

     

      if(fragColor.x > 1.0){ fragColor.x = 1.0;}

      if(fragColor.y > 1.0){ fragColor.y = 1.0;}

      if(fragColor.z > 1.0){ fragColor.z = 1.0;}

     

      return fragColor;  

    }

     

    ENDCG  

          }

       }

       FallBack "None"

    }