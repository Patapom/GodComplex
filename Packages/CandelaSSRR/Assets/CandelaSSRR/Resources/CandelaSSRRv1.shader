Shader "Hidden/CandelaSSRRv1" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }
	Pass {
				Program "vp" {
// Vertex combos: 1
//   d3d9 - ALU: 5 to 5
//   d3d11 - ALU: 4 to 4, TEX: 0 to 0, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
"!!GLSL
#ifdef VERTEX
varying vec2 xlv_TEXCOORD0;

void main ()
{
  gl_Position = (gl_ModelViewProjectionMatrix * gl_Vertex);
  xlv_TEXCOORD0 = gl_MultiTexCoord0.xy;
}


#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec2 xlv_TEXCOORD0;
uniform float _SSRRcomposeMode;
uniform sampler2D _CameraNormalsTexture;
uniform mat4 _ViewMatrix;
uniform mat4 _ProjectionInv;
uniform mat4 _ProjMatrix;
uniform float _bias;
uniform float _stepGlobalScale;
uniform float _maxStep;
uniform float _maxFineStep;
uniform float _maxDepthCull;
uniform float _fadePower;
uniform sampler2D _MainTex;
uniform sampler2D _depthTexCustom;
uniform vec4 _ZBufferParams;
uniform vec4 _ScreenParams;
void main ()
{
  vec3 lkjwejhsdkl_1;
  vec4 opahwcte_2;
  vec4 xbfaeiaej12s_3;
  vec4 tmpvar_4;
  tmpvar_4 = texture2DLod (_MainTex, xlv_TEXCOORD0, 0.0);
  if ((tmpvar_4.w == 0.0)) {
    xbfaeiaej12s_3 = vec4(0.0, 0.0, 0.0, 0.0);
  } else {
    vec4 tmpvar_5;
    tmpvar_5 = texture2DLod (_depthTexCustom, xlv_TEXCOORD0, 0.0);
    float tmpvar_6;
    tmpvar_6 = tmpvar_5.x;
    float tmpvar_7;
    tmpvar_7 = (1.0/(((_ZBufferParams.x * tmpvar_5.x) + _ZBufferParams.y)));
    if ((tmpvar_7 > _maxDepthCull)) {
      xbfaeiaej12s_3 = vec4(0.0, 0.0, 0.0, 0.0);
    } else {
      vec4 acccols_8;
      int s_9;
      vec4 uiduefa_10;
      int icoiuf_11;
      bool biifejd_12;
      vec4 rensfief_13;
      float lenfaiejd_14;
      int vbdueff_15;
      vec3 eiieiaced_16;
      vec3 jjdafhue_17;
      vec3 hgeiald_18;
      vec4 loveeaed_19;
      vec4 mcjkfeeieijd_20;
      vec3 xvzyufalj_21;
      vec4 efljafolclsdf_22;
      int tmpvar_23;
      tmpvar_23 = int(_maxStep);
      efljafolclsdf_22.w = 1.0;
      efljafolclsdf_22.xy = ((xlv_TEXCOORD0 * 2.0) - 1.0);
      efljafolclsdf_22.z = tmpvar_6;
      vec4 tmpvar_24;
      tmpvar_24 = (_ProjectionInv * efljafolclsdf_22);
      vec4 tmpvar_25;
      tmpvar_25 = (tmpvar_24 / tmpvar_24.w);
      xvzyufalj_21.xy = efljafolclsdf_22.xy;
      xvzyufalj_21.z = tmpvar_6;
      mcjkfeeieijd_20.w = 0.0;
      mcjkfeeieijd_20.xyz = ((texture2DLod (_CameraNormalsTexture, xlv_TEXCOORD0, 0.0).xyz * 2.0) - 1.0);
      vec3 tmpvar_26;
      tmpvar_26 = normalize(tmpvar_25.xyz);
      vec3 tmpvar_27;
      tmpvar_27 = normalize((_ViewMatrix * mcjkfeeieijd_20).xyz);
      vec3 tmpvar_28;
      tmpvar_28 = normalize((tmpvar_26 - (2.0 * (dot (tmpvar_27, tmpvar_26) * tmpvar_27))));
      loveeaed_19.w = 1.0;
      loveeaed_19.xyz = (tmpvar_25.xyz + tmpvar_28);
      vec4 tmpvar_29;
      tmpvar_29 = (_ProjMatrix * loveeaed_19);
      vec3 tmpvar_30;
      tmpvar_30 = normalize(((tmpvar_29.xyz / tmpvar_29.w) - xvzyufalj_21));
      lkjwejhsdkl_1.z = tmpvar_30.z;
      lkjwejhsdkl_1.xy = (tmpvar_30.xy * 0.5);
      hgeiald_18.xy = xlv_TEXCOORD0;
      hgeiald_18.z = tmpvar_6;
      float tmpvar_31;
      tmpvar_31 = (2.0 / _ScreenParams.x);
      float tmpvar_32;
      tmpvar_32 = sqrt(dot (lkjwejhsdkl_1.xy, lkjwejhsdkl_1.xy));
      vec3 tmpvar_33;
      tmpvar_33 = (lkjwejhsdkl_1 * ((tmpvar_31 * _stepGlobalScale) / tmpvar_32));
      jjdafhue_17 = tmpvar_33;
      vbdueff_15 = int(_maxStep);
      lenfaiejd_14 = 0.0;
      biifejd_12 = bool(0);
      eiieiaced_16 = (hgeiald_18 + tmpvar_33);
      icoiuf_11 = 0;
      s_9 = 0;
      for (int s_9 = 0; s_9 < 100; ) {
        if ((icoiuf_11 >= vbdueff_15)) {
          break;
        };
        float tmpvar_34;
        tmpvar_34 = (1.0/(((_ZBufferParams.x * texture2DLod (_depthTexCustom, eiieiaced_16.xy, 0.0).x) + _ZBufferParams.y)));
        float tmpvar_35;
        tmpvar_35 = (1.0/(((_ZBufferParams.x * eiieiaced_16.z) + _ZBufferParams.y)));
        if ((tmpvar_34 < (tmpvar_35 - 1e-06))) {
          uiduefa_10.w = 1.0;
          uiduefa_10.xyz = eiieiaced_16;
          rensfief_13 = uiduefa_10;
          biifejd_12 = bool(1);
          break;
        };
        eiieiaced_16 = (eiieiaced_16 + jjdafhue_17);
        lenfaiejd_14 = (lenfaiejd_14 + 1.0);
        icoiuf_11 = (icoiuf_11 + 1);
        s_9 = (s_9 + 1);
      };
      if ((biifejd_12 == bool(0))) {
        vec4 vartfie_36;
        vartfie_36.w = 0.0;
        vartfie_36.xyz = eiieiaced_16;
        rensfief_13 = vartfie_36;
        biifejd_12 = bool(1);
      };
      opahwcte_2 = rensfief_13;
      float tmpvar_37;
      tmpvar_37 = abs((rensfief_13.x - 0.5));
      acccols_8 = vec4(0.0, 0.0, 0.0, 0.0);
      if ((_SSRRcomposeMode > 0.0)) {
        vec4 tmpvar_38;
        tmpvar_38.w = 0.0;
        tmpvar_38.xyz = tmpvar_4.xyz;
        acccols_8 = tmpvar_38;
      };
      if ((tmpvar_37 > 0.5)) {
        xbfaeiaej12s_3 = acccols_8;
      } else {
        float tmpvar_39;
        tmpvar_39 = abs((rensfief_13.y - 0.5));
        if ((tmpvar_39 > 0.5)) {
          xbfaeiaej12s_3 = acccols_8;
        } else {
          if (((1.0/(((_ZBufferParams.x * rensfief_13.z) + _ZBufferParams.y))) > _maxDepthCull)) {
            xbfaeiaej12s_3 = vec4(0.0, 0.0, 0.0, 0.0);
          } else {
            if ((rensfief_13.z < 0.1)) {
              xbfaeiaej12s_3 = vec4(0.0, 0.0, 0.0, 0.0);
            } else {
              if ((rensfief_13.w == 1.0)) {
                int j_40;
                vec4 greyfsd_41;
                vec3 poffses_42;
                int i_49_43;
                bool fjekfesa_44;
                vec4 alsdmes_45;
                int maxfeis_46;
                vec3 refDir_44_47;
                vec3 oifejef_48;
                vec3 tmpvar_49;
                tmpvar_49 = (rensfief_13.xyz - tmpvar_33);
                vec3 tmpvar_50;
                tmpvar_50 = (lkjwejhsdkl_1 * (tmpvar_31 / tmpvar_32));
                refDir_44_47 = tmpvar_50;
                maxfeis_46 = int(_maxFineStep);
                fjekfesa_44 = bool(0);
                poffses_42 = tmpvar_49;
                oifejef_48 = (tmpvar_49 + tmpvar_50);
                i_49_43 = 0;
                j_40 = 0;
                for (int j_40 = 0; j_40 < 20; ) {
                  if ((i_49_43 >= maxfeis_46)) {
                    break;
                  };
                  float tmpvar_51;
                  tmpvar_51 = (1.0/(((_ZBufferParams.x * texture2DLod (_depthTexCustom, oifejef_48.xy, 0.0).x) + _ZBufferParams.y)));
                  float tmpvar_52;
                  tmpvar_52 = (1.0/(((_ZBufferParams.x * oifejef_48.z) + _ZBufferParams.y)));
                  if ((tmpvar_51 < tmpvar_52)) {
                    if (((tmpvar_52 - tmpvar_51) < _bias)) {
                      greyfsd_41.w = 1.0;
                      greyfsd_41.xyz = oifejef_48;
                      alsdmes_45 = greyfsd_41;
                      fjekfesa_44 = bool(1);
                      break;
                    };
                    vec3 tmpvar_53;
                    tmpvar_53 = (refDir_44_47 * 0.5);
                    refDir_44_47 = tmpvar_53;
                    oifejef_48 = (poffses_42 + tmpvar_53);
                  } else {
                    poffses_42 = oifejef_48;
                    oifejef_48 = (oifejef_48 + refDir_44_47);
                  };
                  i_49_43 = (i_49_43 + 1);
                  j_40 = (j_40 + 1);
                };
                if ((fjekfesa_44 == bool(0))) {
                  vec4 tmpvar_55_54;
                  tmpvar_55_54.w = 0.0;
                  tmpvar_55_54.xyz = oifejef_48;
                  alsdmes_45 = tmpvar_55_54;
                  fjekfesa_44 = bool(1);
                };
                opahwcte_2 = alsdmes_45;
              };
              if ((opahwcte_2.w < 0.01)) {
                xbfaeiaej12s_3 = acccols_8;
              } else {
                vec4 tmpvar_57_55;
                tmpvar_57_55.xyz = texture2DLod (_MainTex, opahwcte_2.xy, 0.0).xyz;
                tmpvar_57_55.w = (((opahwcte_2.w * (1.0 - (tmpvar_7 / _maxDepthCull))) * (1.0 - pow ((lenfaiejd_14 / float(tmpvar_23)), _fadePower))) * pow (clamp (((dot (normalize(tmpvar_28), normalize(tmpvar_25).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                xbfaeiaej12s_3 = tmpvar_57_55;
              };
            };
          };
        };
      };
    };
  };
  gl_FragData[0] = xbfaeiaej12s_3;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
"vs_3_0
; 5 ALU
dcl_position o0
dcl_texcoord0 o1
dcl_position0 v0
dcl_texcoord0 v1
mov o1.xy, v1
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

SubProgram "d3d11 " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
ConstBuffer "UnityPerDraw" 336 // 64 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
BindCB "UnityPerDraw" 0
// 6 instructions, 1 temp regs, 0 temp arrays:
// ALU 4 float, 0 int, 0 uint
// TEX 0 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"vs_4_0
eefiecedaffpdldohodkdgpagjklpapmmnbhcfmlabaaaaaaoeabaaaaadaaaaaa
cmaaaaaaiaaaaaaaniaaaaaaejfdeheoemaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaaebaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafaepfdejfeejepeoaafeeffiedepepfceeaaklkl
epfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
fdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefcaeabaaaa
eaaaabaaebaaaaaafjaaaaaeegiocaaaaaaaaaaaaeaaaaaafpaaaaadpcbabaaa
aaaaaaaafpaaaaaddcbabaaaabaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaaddccabaaaabaaaaaagiaaaaacabaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaaaaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaaaaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaaaaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaaaaaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafdccabaaaabaaaaaaegbabaaaabaaaaaa
doaaaaab"
}

SubProgram "gles " {
Keywords { }
"!!GLES


#ifdef VERTEX

varying highp vec2 xlv_TEXCOORD0;
uniform highp mat4 glstate_matrix_mvp;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesVertex;
void main ()
{
  highp vec2 tmpvar_1;
  mediump vec2 tmpvar_2;
  tmpvar_2 = _glesMultiTexCoord0.xy;
  tmpvar_1 = tmpvar_2;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = tmpvar_1;
}



#endif
#ifdef FRAGMENT

#extension GL_EXT_shader_texture_lod : enable
varying highp vec2 xlv_TEXCOORD0;
uniform highp float _SSRRcomposeMode;
uniform sampler2D _CameraNormalsTexture;
uniform highp mat4 _ViewMatrix;
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ProjMatrix;
uniform highp float _bias;
uniform highp float _stepGlobalScale;
uniform highp float _maxStep;
uniform highp float _maxFineStep;
uniform highp float _maxDepthCull;
uniform highp float _fadePower;
uniform sampler2D _MainTex;
uniform sampler2D _depthTexCustom;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 _ScreenParams;
void main ()
{
  mediump vec4 tmpvar_1;
  highp vec4 odheoldj_2;
  highp vec3 lkjwejhsdkl_3;
  highp vec4 opahwcte_4;
  highp vec4 xbfaeiaej12s_5;
  lowp vec4 tmpvar_6;
  tmpvar_6 = texture2DLodEXT (_MainTex, xlv_TEXCOORD0, 0.0);
  odheoldj_2 = tmpvar_6;
  if ((odheoldj_2.w == 0.0)) {
    xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
  } else {
    highp float tmpskdkx_7;
    lowp float tmpvar_8;
    tmpvar_8 = texture2DLodEXT (_depthTexCustom, xlv_TEXCOORD0, 0.0).x;
    tmpskdkx_7 = tmpvar_8;
    highp float tmpvar_9;
    tmpvar_9 = (1.0/(((_ZBufferParams.x * tmpskdkx_7) + _ZBufferParams.y)));
    if ((tmpvar_9 > _maxDepthCull)) {
      xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
    } else {
      highp vec4 acccols_10;
      int s_11;
      highp vec4 uiduefa_12;
      int icoiuf_13;
      bool biifejd_14;
      highp vec4 rensfief_15;
      highp float lenfaiejd_16;
      int vbdueff_17;
      highp vec3 eiieiaced_18;
      highp vec3 jjdafhue_19;
      highp vec3 hgeiald_20;
      highp vec4 loveeaed_21;
      highp vec4 mcjkfeeieijd_22;
      highp vec3 xvzyufalj_23;
      highp vec4 efljafolclsdf_24;
      int tmpvar_25;
      tmpvar_25 = int(_maxStep);
      efljafolclsdf_24.w = 1.0;
      efljafolclsdf_24.xy = ((xlv_TEXCOORD0 * 2.0) - 1.0);
      efljafolclsdf_24.z = tmpskdkx_7;
      highp vec4 tmpvar_26;
      tmpvar_26 = (_ProjectionInv * efljafolclsdf_24);
      highp vec4 tmpvar_27;
      tmpvar_27 = (tmpvar_26 / tmpvar_26.w);
      xvzyufalj_23.xy = efljafolclsdf_24.xy;
      xvzyufalj_23.z = tmpskdkx_7;
      mcjkfeeieijd_22.w = 0.0;
      lowp vec3 tmpvar_28;
      tmpvar_28 = ((texture2DLodEXT (_CameraNormalsTexture, xlv_TEXCOORD0, 0.0).xyz * 2.0) - 1.0);
      mcjkfeeieijd_22.xyz = tmpvar_28;
      highp vec3 tmpvar_29;
      tmpvar_29 = normalize(tmpvar_27.xyz);
      highp vec3 tmpvar_30;
      tmpvar_30 = normalize((_ViewMatrix * mcjkfeeieijd_22).xyz);
      highp vec3 tmpvar_31;
      tmpvar_31 = normalize((tmpvar_29 - (2.0 * (dot (tmpvar_30, tmpvar_29) * tmpvar_30))));
      loveeaed_21.w = 1.0;
      loveeaed_21.xyz = (tmpvar_27.xyz + tmpvar_31);
      highp vec4 tmpvar_32;
      tmpvar_32 = (_ProjMatrix * loveeaed_21);
      highp vec3 tmpvar_33;
      tmpvar_33 = normalize(((tmpvar_32.xyz / tmpvar_32.w) - xvzyufalj_23));
      lkjwejhsdkl_3.z = tmpvar_33.z;
      lkjwejhsdkl_3.xy = (tmpvar_33.xy * 0.5);
      hgeiald_20.xy = xlv_TEXCOORD0;
      hgeiald_20.z = tmpskdkx_7;
      highp float tmpvar_34;
      tmpvar_34 = (2.0 / _ScreenParams.x);
      highp float tmpvar_35;
      tmpvar_35 = sqrt(dot (lkjwejhsdkl_3.xy, lkjwejhsdkl_3.xy));
      highp vec3 tmpvar_36;
      tmpvar_36 = (lkjwejhsdkl_3 * ((tmpvar_34 * _stepGlobalScale) / tmpvar_35));
      jjdafhue_19 = tmpvar_36;
      vbdueff_17 = int(_maxStep);
      lenfaiejd_16 = 0.0;
      biifejd_14 = bool(0);
      eiieiaced_18 = (hgeiald_20 + tmpvar_36);
      icoiuf_13 = 0;
      s_11 = 0;
      for (int s_11 = 0; s_11 < 100; ) {
        if ((icoiuf_13 >= vbdueff_17)) {
          break;
        };
        lowp vec4 tmpvar_37;
        tmpvar_37 = texture2DLodEXT (_depthTexCustom, eiieiaced_18.xy, 0.0);
        highp float tmpvar_38;
        tmpvar_38 = (1.0/(((_ZBufferParams.x * tmpvar_37.x) + _ZBufferParams.y)));
        highp float tmpvar_39;
        tmpvar_39 = (1.0/(((_ZBufferParams.x * eiieiaced_18.z) + _ZBufferParams.y)));
        if ((tmpvar_38 < (tmpvar_39 - 1e-06))) {
          uiduefa_12.w = 1.0;
          uiduefa_12.xyz = eiieiaced_18;
          rensfief_15 = uiduefa_12;
          biifejd_14 = bool(1);
          break;
        };
        eiieiaced_18 = (eiieiaced_18 + jjdafhue_19);
        lenfaiejd_16 = (lenfaiejd_16 + 1.0);
        icoiuf_13 = (icoiuf_13 + 1);
        s_11 = (s_11 + 1);
      };
      if ((biifejd_14 == bool(0))) {
        highp vec4 vartfie_40;
        vartfie_40.w = 0.0;
        vartfie_40.xyz = eiieiaced_18;
        rensfief_15 = vartfie_40;
        biifejd_14 = bool(1);
      };
      opahwcte_4 = rensfief_15;
      highp float tmpvar_41;
      tmpvar_41 = abs((rensfief_15.x - 0.5));
      acccols_10 = vec4(0.0, 0.0, 0.0, 0.0);
      if ((_SSRRcomposeMode > 0.0)) {
        highp vec4 tmpvar_42;
        tmpvar_42.w = 0.0;
        tmpvar_42.xyz = odheoldj_2.xyz;
        acccols_10 = tmpvar_42;
      };
      if ((tmpvar_41 > 0.5)) {
        xbfaeiaej12s_5 = acccols_10;
      } else {
        highp float tmpvar_43;
        tmpvar_43 = abs((rensfief_15.y - 0.5));
        if ((tmpvar_43 > 0.5)) {
          xbfaeiaej12s_5 = acccols_10;
        } else {
          if (((1.0/(((_ZBufferParams.x * rensfief_15.z) + _ZBufferParams.y))) > _maxDepthCull)) {
            xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
          } else {
            if ((rensfief_15.z < 0.1)) {
              xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
            } else {
              if ((rensfief_15.w == 1.0)) {
                int j_44;
                highp vec4 greyfsd_45;
                highp vec3 poffses_46;
                int i_49_47;
                bool fjekfesa_48;
                highp vec4 alsdmes_49;
                int maxfeis_50;
                highp vec3 refDir_44_51;
                highp vec3 oifejef_52;
                highp vec3 tmpvar_53;
                tmpvar_53 = (rensfief_15.xyz - tmpvar_36);
                highp vec3 tmpvar_54;
                tmpvar_54 = (lkjwejhsdkl_3 * (tmpvar_34 / tmpvar_35));
                refDir_44_51 = tmpvar_54;
                maxfeis_50 = int(_maxFineStep);
                fjekfesa_48 = bool(0);
                poffses_46 = tmpvar_53;
                oifejef_52 = (tmpvar_53 + tmpvar_54);
                i_49_47 = 0;
                j_44 = 0;
                for (int j_44 = 0; j_44 < 20; ) {
                  if ((i_49_47 >= maxfeis_50)) {
                    break;
                  };
                  lowp vec4 tmpvar_55;
                  tmpvar_55 = texture2DLodEXT (_depthTexCustom, oifejef_52.xy, 0.0);
                  highp float tmpvar_56;
                  tmpvar_56 = (1.0/(((_ZBufferParams.x * tmpvar_55.x) + _ZBufferParams.y)));
                  highp float tmpvar_57;
                  tmpvar_57 = (1.0/(((_ZBufferParams.x * oifejef_52.z) + _ZBufferParams.y)));
                  if ((tmpvar_56 < tmpvar_57)) {
                    if (((tmpvar_57 - tmpvar_56) < _bias)) {
                      greyfsd_45.w = 1.0;
                      greyfsd_45.xyz = oifejef_52;
                      alsdmes_49 = greyfsd_45;
                      fjekfesa_48 = bool(1);
                      break;
                    };
                    highp vec3 tmpvar_58;
                    tmpvar_58 = (refDir_44_51 * 0.5);
                    refDir_44_51 = tmpvar_58;
                    oifejef_52 = (poffses_46 + tmpvar_58);
                  } else {
                    poffses_46 = oifejef_52;
                    oifejef_52 = (oifejef_52 + refDir_44_51);
                  };
                  i_49_47 = (i_49_47 + 1);
                  j_44 = (j_44 + 1);
                };
                if ((fjekfesa_48 == bool(0))) {
                  highp vec4 tmpvar_55_59;
                  tmpvar_55_59.w = 0.0;
                  tmpvar_55_59.xyz = oifejef_52;
                  alsdmes_49 = tmpvar_55_59;
                  fjekfesa_48 = bool(1);
                };
                opahwcte_4 = alsdmes_49;
              };
              if ((opahwcte_4.w < 0.01)) {
                xbfaeiaej12s_5 = acccols_10;
              } else {
                highp vec4 tmpvar_57_60;
                lowp vec3 tmpvar_61;
                tmpvar_61 = texture2DLodEXT (_MainTex, opahwcte_4.xy, 0.0).xyz;
                tmpvar_57_60.xyz = tmpvar_61;
                tmpvar_57_60.w = (((opahwcte_4.w * (1.0 - (tmpvar_9 / _maxDepthCull))) * (1.0 - pow ((lenfaiejd_16 / float(tmpvar_25)), _fadePower))) * pow (clamp (((dot (normalize(tmpvar_31), normalize(tmpvar_27).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                xbfaeiaej12s_5 = tmpvar_57_60;
              };
            };
          };
        };
      };
    };
  };
  tmpvar_1 = xbfaeiaej12s_5;
  gl_FragData[0] = tmpvar_1;
}



#endif"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES


#ifdef VERTEX

varying highp vec2 xlv_TEXCOORD0;
uniform highp mat4 glstate_matrix_mvp;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesVertex;
void main ()
{
  highp vec2 tmpvar_1;
  mediump vec2 tmpvar_2;
  tmpvar_2 = _glesMultiTexCoord0.xy;
  tmpvar_1 = tmpvar_2;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = tmpvar_1;
}



#endif
#ifdef FRAGMENT

#extension GL_EXT_shader_texture_lod : enable
varying highp vec2 xlv_TEXCOORD0;
uniform highp float _SSRRcomposeMode;
uniform sampler2D _CameraNormalsTexture;
uniform highp mat4 _ViewMatrix;
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ProjMatrix;
uniform highp float _bias;
uniform highp float _stepGlobalScale;
uniform highp float _maxStep;
uniform highp float _maxFineStep;
uniform highp float _maxDepthCull;
uniform highp float _fadePower;
uniform sampler2D _MainTex;
uniform sampler2D _depthTexCustom;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 _ScreenParams;
void main ()
{
  mediump vec4 tmpvar_1;
  highp vec4 odheoldj_2;
  highp vec3 lkjwejhsdkl_3;
  highp vec4 opahwcte_4;
  highp vec4 xbfaeiaej12s_5;
  lowp vec4 tmpvar_6;
  tmpvar_6 = texture2DLodEXT (_MainTex, xlv_TEXCOORD0, 0.0);
  odheoldj_2 = tmpvar_6;
  if ((odheoldj_2.w == 0.0)) {
    xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
  } else {
    highp float tmpskdkx_7;
    lowp float tmpvar_8;
    tmpvar_8 = texture2DLodEXT (_depthTexCustom, xlv_TEXCOORD0, 0.0).x;
    tmpskdkx_7 = tmpvar_8;
    highp float tmpvar_9;
    tmpvar_9 = (1.0/(((_ZBufferParams.x * tmpskdkx_7) + _ZBufferParams.y)));
    if ((tmpvar_9 > _maxDepthCull)) {
      xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
    } else {
      highp vec4 acccols_10;
      int s_11;
      highp vec4 uiduefa_12;
      int icoiuf_13;
      bool biifejd_14;
      highp vec4 rensfief_15;
      highp float lenfaiejd_16;
      int vbdueff_17;
      highp vec3 eiieiaced_18;
      highp vec3 jjdafhue_19;
      highp vec3 hgeiald_20;
      highp vec4 loveeaed_21;
      highp vec4 mcjkfeeieijd_22;
      highp vec3 xvzyufalj_23;
      highp vec4 efljafolclsdf_24;
      int tmpvar_25;
      tmpvar_25 = int(_maxStep);
      efljafolclsdf_24.w = 1.0;
      efljafolclsdf_24.xy = ((xlv_TEXCOORD0 * 2.0) - 1.0);
      efljafolclsdf_24.z = tmpskdkx_7;
      highp vec4 tmpvar_26;
      tmpvar_26 = (_ProjectionInv * efljafolclsdf_24);
      highp vec4 tmpvar_27;
      tmpvar_27 = (tmpvar_26 / tmpvar_26.w);
      xvzyufalj_23.xy = efljafolclsdf_24.xy;
      xvzyufalj_23.z = tmpskdkx_7;
      mcjkfeeieijd_22.w = 0.0;
      lowp vec3 tmpvar_28;
      tmpvar_28 = ((texture2DLodEXT (_CameraNormalsTexture, xlv_TEXCOORD0, 0.0).xyz * 2.0) - 1.0);
      mcjkfeeieijd_22.xyz = tmpvar_28;
      highp vec3 tmpvar_29;
      tmpvar_29 = normalize(tmpvar_27.xyz);
      highp vec3 tmpvar_30;
      tmpvar_30 = normalize((_ViewMatrix * mcjkfeeieijd_22).xyz);
      highp vec3 tmpvar_31;
      tmpvar_31 = normalize((tmpvar_29 - (2.0 * (dot (tmpvar_30, tmpvar_29) * tmpvar_30))));
      loveeaed_21.w = 1.0;
      loveeaed_21.xyz = (tmpvar_27.xyz + tmpvar_31);
      highp vec4 tmpvar_32;
      tmpvar_32 = (_ProjMatrix * loveeaed_21);
      highp vec3 tmpvar_33;
      tmpvar_33 = normalize(((tmpvar_32.xyz / tmpvar_32.w) - xvzyufalj_23));
      lkjwejhsdkl_3.z = tmpvar_33.z;
      lkjwejhsdkl_3.xy = (tmpvar_33.xy * 0.5);
      hgeiald_20.xy = xlv_TEXCOORD0;
      hgeiald_20.z = tmpskdkx_7;
      highp float tmpvar_34;
      tmpvar_34 = (2.0 / _ScreenParams.x);
      highp float tmpvar_35;
      tmpvar_35 = sqrt(dot (lkjwejhsdkl_3.xy, lkjwejhsdkl_3.xy));
      highp vec3 tmpvar_36;
      tmpvar_36 = (lkjwejhsdkl_3 * ((tmpvar_34 * _stepGlobalScale) / tmpvar_35));
      jjdafhue_19 = tmpvar_36;
      vbdueff_17 = int(_maxStep);
      lenfaiejd_16 = 0.0;
      biifejd_14 = bool(0);
      eiieiaced_18 = (hgeiald_20 + tmpvar_36);
      icoiuf_13 = 0;
      s_11 = 0;
      for (int s_11 = 0; s_11 < 100; ) {
        if ((icoiuf_13 >= vbdueff_17)) {
          break;
        };
        lowp vec4 tmpvar_37;
        tmpvar_37 = texture2DLodEXT (_depthTexCustom, eiieiaced_18.xy, 0.0);
        highp float tmpvar_38;
        tmpvar_38 = (1.0/(((_ZBufferParams.x * tmpvar_37.x) + _ZBufferParams.y)));
        highp float tmpvar_39;
        tmpvar_39 = (1.0/(((_ZBufferParams.x * eiieiaced_18.z) + _ZBufferParams.y)));
        if ((tmpvar_38 < (tmpvar_39 - 1e-06))) {
          uiduefa_12.w = 1.0;
          uiduefa_12.xyz = eiieiaced_18;
          rensfief_15 = uiduefa_12;
          biifejd_14 = bool(1);
          break;
        };
        eiieiaced_18 = (eiieiaced_18 + jjdafhue_19);
        lenfaiejd_16 = (lenfaiejd_16 + 1.0);
        icoiuf_13 = (icoiuf_13 + 1);
        s_11 = (s_11 + 1);
      };
      if ((biifejd_14 == bool(0))) {
        highp vec4 vartfie_40;
        vartfie_40.w = 0.0;
        vartfie_40.xyz = eiieiaced_18;
        rensfief_15 = vartfie_40;
        biifejd_14 = bool(1);
      };
      opahwcte_4 = rensfief_15;
      highp float tmpvar_41;
      tmpvar_41 = abs((rensfief_15.x - 0.5));
      acccols_10 = vec4(0.0, 0.0, 0.0, 0.0);
      if ((_SSRRcomposeMode > 0.0)) {
        highp vec4 tmpvar_42;
        tmpvar_42.w = 0.0;
        tmpvar_42.xyz = odheoldj_2.xyz;
        acccols_10 = tmpvar_42;
      };
      if ((tmpvar_41 > 0.5)) {
        xbfaeiaej12s_5 = acccols_10;
      } else {
        highp float tmpvar_43;
        tmpvar_43 = abs((rensfief_15.y - 0.5));
        if ((tmpvar_43 > 0.5)) {
          xbfaeiaej12s_5 = acccols_10;
        } else {
          if (((1.0/(((_ZBufferParams.x * rensfief_15.z) + _ZBufferParams.y))) > _maxDepthCull)) {
            xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
          } else {
            if ((rensfief_15.z < 0.1)) {
              xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
            } else {
              if ((rensfief_15.w == 1.0)) {
                int j_44;
                highp vec4 greyfsd_45;
                highp vec3 poffses_46;
                int i_49_47;
                bool fjekfesa_48;
                highp vec4 alsdmes_49;
                int maxfeis_50;
                highp vec3 refDir_44_51;
                highp vec3 oifejef_52;
                highp vec3 tmpvar_53;
                tmpvar_53 = (rensfief_15.xyz - tmpvar_36);
                highp vec3 tmpvar_54;
                tmpvar_54 = (lkjwejhsdkl_3 * (tmpvar_34 / tmpvar_35));
                refDir_44_51 = tmpvar_54;
                maxfeis_50 = int(_maxFineStep);
                fjekfesa_48 = bool(0);
                poffses_46 = tmpvar_53;
                oifejef_52 = (tmpvar_53 + tmpvar_54);
                i_49_47 = 0;
                j_44 = 0;
                for (int j_44 = 0; j_44 < 20; ) {
                  if ((i_49_47 >= maxfeis_50)) {
                    break;
                  };
                  lowp vec4 tmpvar_55;
                  tmpvar_55 = texture2DLodEXT (_depthTexCustom, oifejef_52.xy, 0.0);
                  highp float tmpvar_56;
                  tmpvar_56 = (1.0/(((_ZBufferParams.x * tmpvar_55.x) + _ZBufferParams.y)));
                  highp float tmpvar_57;
                  tmpvar_57 = (1.0/(((_ZBufferParams.x * oifejef_52.z) + _ZBufferParams.y)));
                  if ((tmpvar_56 < tmpvar_57)) {
                    if (((tmpvar_57 - tmpvar_56) < _bias)) {
                      greyfsd_45.w = 1.0;
                      greyfsd_45.xyz = oifejef_52;
                      alsdmes_49 = greyfsd_45;
                      fjekfesa_48 = bool(1);
                      break;
                    };
                    highp vec3 tmpvar_58;
                    tmpvar_58 = (refDir_44_51 * 0.5);
                    refDir_44_51 = tmpvar_58;
                    oifejef_52 = (poffses_46 + tmpvar_58);
                  } else {
                    poffses_46 = oifejef_52;
                    oifejef_52 = (oifejef_52 + refDir_44_51);
                  };
                  i_49_47 = (i_49_47 + 1);
                  j_44 = (j_44 + 1);
                };
                if ((fjekfesa_48 == bool(0))) {
                  highp vec4 tmpvar_55_59;
                  tmpvar_55_59.w = 0.0;
                  tmpvar_55_59.xyz = oifejef_52;
                  alsdmes_49 = tmpvar_55_59;
                  fjekfesa_48 = bool(1);
                };
                opahwcte_4 = alsdmes_49;
              };
              if ((opahwcte_4.w < 0.01)) {
                xbfaeiaej12s_5 = acccols_10;
              } else {
                highp vec4 tmpvar_57_60;
                lowp vec3 tmpvar_61;
                tmpvar_61 = texture2DLodEXT (_MainTex, opahwcte_4.xy, 0.0).xyz;
                tmpvar_57_60.xyz = tmpvar_61;
                tmpvar_57_60.w = (((opahwcte_4.w * (1.0 - (tmpvar_9 / _maxDepthCull))) * (1.0 - pow ((lenfaiejd_16 / float(tmpvar_25)), _fadePower))) * pow (clamp (((dot (normalize(tmpvar_31), normalize(tmpvar_27).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                xbfaeiaej12s_5 = tmpvar_57_60;
              };
            };
          };
        };
      };
    };
  };
  tmpvar_1 = xbfaeiaej12s_5;
  gl_FragData[0] = tmpvar_1;
}



#endif"
}

SubProgram "gles3 " {
Keywords { }
"!!GLES3#version 300 es


#ifdef VERTEX

#define gl_Vertex _glesVertex
in vec4 _glesVertex;
#define gl_MultiTexCoord0 _glesMultiTexCoord0
in vec4 _glesMultiTexCoord0;

#line 151
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 187
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 181
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 330
struct v2f {
    highp vec4 pos;
    highp vec2 uv;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[8];
uniform highp vec4 unity_LightPosition[8];
uniform highp vec4 unity_LightAtten[8];
#line 19
uniform highp vec4 unity_SpotDirection[8];
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
#line 23
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
#line 27
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
#line 31
uniform highp vec4 _LightSplitsNear;
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
#line 35
uniform highp vec4 unity_ShadowFadeCenterAndType;
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
#line 39
uniform highp mat4 _Object2World;
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
#line 43
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
#line 47
uniform highp mat4 glstate_matrix_projection;
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
#line 51
uniform lowp vec4 unity_ColorSpaceGrey;
#line 77
#line 82
#line 87
#line 91
#line 96
#line 120
#line 137
#line 158
#line 166
#line 193
#line 206
#line 215
#line 220
#line 229
#line 234
#line 243
#line 260
#line 265
#line 291
#line 299
#line 307
#line 311
#line 315
uniform sampler2D _depthTexCustom;
uniform sampler2D _MainTex;
uniform highp float _fadePower;
uniform highp float _maxDepthCull;
#line 319
uniform highp float _maxFineStep;
uniform highp float _maxStep;
uniform highp float _stepGlobalScale;
uniform highp float _bias;
#line 323
uniform highp mat4 _ProjMatrix;
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ViewMatrix;
uniform highp vec4 _ProjInfo;
#line 327
uniform sampler2D _CameraNormalsTexture;
uniform sampler2D _CameraDepthTexture;
uniform highp float _SSRRcomposeMode;
#line 336
#line 336
v2f vert( in appdata_img v ) {
    v2f o;
    o.pos = (glstate_matrix_mvp * v.vertex);
    #line 340
    o.uv = v.texcoord.xy;
    return o;
}
out highp vec2 xlv_TEXCOORD0;
void main() {
    v2f xl_retval;
    appdata_img xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xlt_v.texcoord = vec2(gl_MultiTexCoord0);
    xl_retval = vert( xlt_v);
    gl_Position = vec4(xl_retval.pos);
    xlv_TEXCOORD0 = vec2(xl_retval.uv);
}


#endif
#ifdef FRAGMENT

#define gl_FragData _glesFragData
layout(location = 0) out mediump vec4 _glesFragData[4];
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return textureLod( s, coord.xy, coord.w);
}
#line 151
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 187
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 181
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 330
struct v2f {
    highp vec4 pos;
    highp vec2 uv;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[8];
uniform highp vec4 unity_LightPosition[8];
uniform highp vec4 unity_LightAtten[8];
#line 19
uniform highp vec4 unity_SpotDirection[8];
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
#line 23
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
#line 27
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
#line 31
uniform highp vec4 _LightSplitsNear;
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
#line 35
uniform highp vec4 unity_ShadowFadeCenterAndType;
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
#line 39
uniform highp mat4 _Object2World;
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
#line 43
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
#line 47
uniform highp mat4 glstate_matrix_projection;
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
#line 51
uniform lowp vec4 unity_ColorSpaceGrey;
#line 77
#line 82
#line 87
#line 91
#line 96
#line 120
#line 137
#line 158
#line 166
#line 193
#line 206
#line 215
#line 220
#line 229
#line 234
#line 243
#line 260
#line 265
#line 291
#line 299
#line 307
#line 311
#line 315
uniform sampler2D _depthTexCustom;
uniform sampler2D _MainTex;
uniform highp float _fadePower;
uniform highp float _maxDepthCull;
#line 319
uniform highp float _maxFineStep;
uniform highp float _maxStep;
uniform highp float _stepGlobalScale;
uniform highp float _bias;
#line 323
uniform highp mat4 _ProjMatrix;
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ViewMatrix;
uniform highp vec4 _ProjInfo;
#line 327
uniform sampler2D _CameraNormalsTexture;
uniform sampler2D _CameraDepthTexture;
uniform highp float _SSRRcomposeMode;
#line 336
#line 343
mediump vec4 frag( in v2f i ) {
    #line 345
    highp vec4 xbfaeiaej12s;
    highp float jaglkje92an;
    highp vec4 opahwcte;
    highp vec3 lkjwejhsdkl;
    #line 349
    highp int mnafeiefj45f;
    highp vec4 odheoldj = xll_tex2Dlod( _MainTex, vec4( i.uv, 0.0, 0.0));
    if ((odheoldj.w == 0.0)){
        #line 353
        xbfaeiaej12s = vec4( 0.0, 0.0, 0.0, 0.0);
    }
    else{
        #line 357
        highp float tmpskdkx = xll_tex2Dlod( _depthTexCustom, vec4( i.uv, 0.0, 0.0)).x;
        highp float pootanfflkd = tmpskdkx;
        highp float greysaalwe = (1.0 / ((_ZBufferParams.x * tmpskdkx) + _ZBufferParams.y));
        if ((greysaalwe > _maxDepthCull)){
            #line 362
            xbfaeiaej12s = vec4( 0.0, 0.0, 0.0, 0.0);
        }
        else{
            #line 366
            mnafeiefj45f = int(_maxStep);
            highp vec4 efljafolclsdf;
            efljafolclsdf.w = 1.0;
            efljafolclsdf.xy = ((i.uv * 2.0) - 1.0);
            #line 370
            efljafolclsdf.z = pootanfflkd;
            highp vec4 aflkjeoifa = (_ProjectionInv * efljafolclsdf);
            highp vec4 mvaoieije = (aflkjeoifa / aflkjeoifa.w);
            highp vec3 xvzyufalj;
            #line 374
            xvzyufalj.xy = efljafolclsdf.xy;
            xvzyufalj.z = pootanfflkd;
            highp vec4 mcjkfeeieijd;
            mcjkfeeieijd.w = 0.0;
            #line 378
            mcjkfeeieijd.xyz = ((xll_tex2Dlod( _CameraNormalsTexture, vec4( i.uv, 0.0, 0.0)).xyz * 2.0) - 1.0);
            highp vec3 omfeief = normalize(mvaoieije.xyz);
            highp vec3 kdfefis = normalize((_ViewMatrix * mcjkfeeieijd).xyz);
            highp vec3 jfeiiwsi = normalize((omfeief - (2.0 * (dot( kdfefis, omfeief) * kdfefis))));
            #line 382
            highp vec4 loveeaed;
            loveeaed.w = 1.0;
            loveeaed.xyz = (mvaoieije.xyz + jfeiiwsi);
            highp vec4 qeuaife = (_ProjMatrix * loveeaed);
            #line 386
            highp vec3 justae = normalize(((qeuaife.xyz / qeuaife.w) - xvzyufalj));
            lkjwejhsdkl.z = justae.z;
            lkjwejhsdkl.xy = (justae.xy * 0.5);
            highp vec3 hgeiald;
            #line 390
            hgeiald.xy = i.uv;
            hgeiald.z = pootanfflkd;
            jaglkje92an = 0.0;
            highp float kjafjie = (2.0 / _ScreenParams.x);
            #line 394
            highp float nfeiefie = sqrt(dot( lkjwejhsdkl.xy, lkjwejhsdkl.xy));
            highp vec3 jjdafhue = (lkjwejhsdkl * ((kjafjie * _stepGlobalScale) / nfeiefie));
            highp vec3 eiieiaced;
            highp int vbdueff = int(_maxStep);
            #line 398
            highp float lenfaiejd = jaglkje92an;
            highp vec4 rensfief;
            bool biifejd = false;
            eiieiaced = (hgeiald + jjdafhue);
            #line 402
            highp int icoiuf = 0;
            highp float zzddfeef;
            highp float fejoijfe;
            highp vec4 uiduefa;
            #line 406
            highp int s = 0;
            s = 0;
            for ( ; (s < 100); (s++)) {
                #line 411
                if ((icoiuf >= vbdueff)){
                    break;
                }
                #line 415
                zzddfeef = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _depthTexCustom, vec4( eiieiaced.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
                fejoijfe = (1.0 / ((_ZBufferParams.x * eiieiaced.z) + _ZBufferParams.y));
                if ((zzddfeef < (fejoijfe - 1e-06))){
                    #line 419
                    uiduefa.w = 1.0;
                    uiduefa.xyz = eiieiaced;
                    rensfief = uiduefa;
                    biifejd = true;
                    #line 423
                    break;
                }
                eiieiaced = (eiieiaced + jjdafhue);
                lenfaiejd = (lenfaiejd + 1.0);
                #line 427
                icoiuf = (icoiuf + 1);
            }
            if ((biifejd == false)){
                #line 431
                highp vec4 vartfie;
                vartfie.w = 0.0;
                vartfie.xyz = eiieiaced;
                rensfief = vartfie;
                #line 435
                biifejd = true;
            }
            jaglkje92an = lenfaiejd;
            opahwcte = rensfief;
            #line 439
            highp float getthead;
            getthead = abs((rensfief.x - 0.5));
            highp vec4 acccols = vec4( 0.0, 0.0, 0.0, 0.0);
            if ((_SSRRcomposeMode > 0.0)){
                acccols = vec4( odheoldj.xyz, 0.0);
            }
            #line 443
            if ((getthead > 0.5)){
                xbfaeiaej12s = acccols;
            }
            else{
                #line 449
                highp float varefeid = abs((rensfief.y - 0.5));
                if ((varefeid > 0.5)){
                    xbfaeiaej12s = acccols;
                }
                else{
                    #line 456
                    if (((1.0 / ((_ZBufferParams.x * rensfief.z) + _ZBufferParams.y)) > _maxDepthCull)){
                        xbfaeiaej12s = vec4( 0.0, 0.0, 0.0, 0.0);
                    }
                    else{
                        #line 462
                        if ((rensfief.z < 0.1)){
                            xbfaeiaej12s = vec4( 0.0, 0.0, 0.0, 0.0);
                        }
                        else{
                            #line 468
                            if ((rensfief.w == 1.0)){
                                highp vec3 fdfk41fe = (rensfief.xyz - jjdafhue);
                                highp vec3 efijef42s = (lkjwejhsdkl * (kjafjie / nfeiefie));
                                #line 472
                                highp vec3 oifejef;
                                highp vec3 refDir_44;
                                refDir_44 = efijef42s;
                                highp int maxfeis = int(_maxFineStep);
                                #line 476
                                highp vec4 alsdmes;
                                bool fjekfesa = false;
                                highp int i_49;
                                highp vec3 poffses = fdfk41fe;
                                #line 480
                                oifejef = (fdfk41fe + efijef42s);
                                i_49 = 0;
                                highp float jjuuddsfe;
                                highp float taycosl;
                                #line 484
                                highp vec4 greyfsd;
                                highp vec3 blacsjd;
                                highp int j = 0;
                                j = 0;
                                for ( ; (j < 20); (j++)) {
                                    #line 491
                                    if ((i_49 >= maxfeis)){
                                        break;
                                    }
                                    #line 495
                                    jjuuddsfe = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _depthTexCustom, vec4( oifejef.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
                                    taycosl = (1.0 / ((_ZBufferParams.x * oifejef.z) + _ZBufferParams.y));
                                    if ((jjuuddsfe < taycosl)){
                                        #line 499
                                        if (((taycosl - jjuuddsfe) < _bias)){
                                            greyfsd.w = 1.0;
                                            greyfsd.xyz = oifejef;
                                            #line 503
                                            alsdmes = greyfsd;
                                            fjekfesa = true;
                                            break;
                                        }
                                        #line 507
                                        blacsjd = (refDir_44 * 0.5);
                                        refDir_44 = blacsjd;
                                        oifejef = (poffses + blacsjd);
                                    }
                                    else{
                                        #line 513
                                        poffses = oifejef;
                                        oifejef = (oifejef + refDir_44);
                                    }
                                    i_49 = (i_49 + 1);
                                }
                                #line 518
                                if ((fjekfesa == false)){
                                    highp vec4 tmpvar_55;
                                    tmpvar_55.w = 0.0;
                                    #line 522
                                    tmpvar_55.xyz = oifejef;
                                    alsdmes = tmpvar_55;
                                    fjekfesa = true;
                                }
                                #line 526
                                opahwcte = alsdmes;
                            }
                            if ((opahwcte.w < 0.01)){
                                #line 530
                                xbfaeiaej12s = acccols;
                            }
                            else{
                                #line 534
                                highp vec4 tmpvar_57;
                                tmpvar_57.xyz = xll_tex2Dlod( _MainTex, vec4( opahwcte.xy, 0.0, 0.0)).xyz;
                                tmpvar_57.w = (((opahwcte.w * (1.0 - (greysaalwe / _maxDepthCull))) * (1.0 - pow( (lenfaiejd / float(mnafeiefj45f)), _fadePower))) * pow( clamp( ((dot( normalize(jfeiiwsi), normalize(mvaoieije).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                                xbfaeiaej12s = tmpvar_57;
                            }
                        }
                    }
                }
            }
        }
    }
    #line 545
    return xbfaeiaej12s;
}
in highp vec2 xlv_TEXCOORD0;
void main() {
    mediump vec4 xl_retval;
    v2f xlt_i;
    xlt_i.pos = vec4(0.0);
    xlt_i.uv = vec2(xlv_TEXCOORD0);
    xl_retval = frag( xlt_i);
    gl_FragData[0] = vec4(xl_retval);
}


#endif"
}

}
Program "fp" {
// Fragment combos: 1
//   d3d9 - ALU: 176 to 176, TEX: 12 to 12, FLOW: 25 to 25
//   d3d11 - ALU: 107 to 107, TEX: 0 to 0, FLOW: 29 to 29
SubProgram "opengl " {
Keywords { }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { }
Vector 12 [_ScreenParams]
Vector 13 [_ZBufferParams]
Float 14 [_fadePower]
Float 15 [_maxDepthCull]
Float 16 [_maxFineStep]
Float 17 [_maxStep]
Float 18 [_stepGlobalScale]
Float 19 [_bias]
Matrix 0 [_ProjMatrix]
Matrix 4 [_ProjectionInv]
Matrix 8 [_ViewMatrix]
Float 20 [_SSRRcomposeMode]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_depthTexCustom] 2D
SetTexture 2 [_CameraNormalsTexture] 2D
"ps_3_0
; 176 ALU, 12 TEX, 25 FLOW
dcl_2d s0
dcl_2d s1
dcl_2d s2
def c21, 0.00000000, 2.00000000, -1.00000000, 1.00000000
def c22, 0.50000000, 1.00000000, -0.00000100, -0.50000000
defi i0, 100, 0, 1, 0
def c23, 0.10000000, 0.01000000, 0, 0
defi i1, 20, 0, 1, 0
dcl_texcoord0 v0.xy
mov r0.xy, v0
mov r0.z, c21.x
texldl r3, r0.xyzz, s0
if_eq r3.w, c21.x
mov r0, c21.x
else
mov r0.xy, v0
mov r0.z, c21.x
texldl r0.x, r0.xyzz, s1
mad r0.y, r0.x, c13.x, c13
rcp r4.w, r0.y
if_gt r4.w, c15.x
mov r0, c21.x
else
mad r8.xy, v0, c21.y, c21.z
mov r5.z, r0.x
mov r5.xy, r8
mov r5.w, c21
dp4 r0.y, r5, c7
mov r1.w, r0.y
mov r6.w, c21
dp4 r1.z, r5, c6
dp4 r1.y, r5, c5
dp4 r1.x, r5, c4
rcp r0.y, r0.y
mul r1, r1, r0.y
dp3 r0.z, r1, r1
mov r5.w, c21.x
mov r8.z, r0.x
rsq r0.z, r0.z
mov r5.z, c21.x
mov r5.xy, v0
texldl r5.xyz, r5.xyzz, s2
mad r5.xyz, r5, c21.y, c21.z
dp4 r6.z, r5, c10
dp4 r6.x, r5, c8
dp4 r6.y, r5, c9
dp3 r0.y, r6, r6
rsq r0.y, r0.y
mul r5.xyz, r0.z, r1
mul r6.xyz, r0.y, r6
dp3 r0.y, r6, r5
mul r6.xyz, r0.y, r6
mad r5.xyz, -r6, c21.y, r5
dp3 r0.y, r5, r5
rsq r0.y, r0.y
mul r5.xyz, r0.y, r5
add r6.xyz, r1, r5
dp4 r0.y, r6, c3
dp4 r7.z, r6, c2
dp4 r7.y, r6, c1
dp4 r7.x, r6, c0
rcp r0.y, r0.y
mad r6.xyz, r7, r0.y, -r8
dp3 r0.y, r6, r6
rsq r0.y, r0.y
mul r6.xyz, r0.y, r6
mul r0.zw, r6.xyxy, c22.x
rcp r0.y, c12.x
mul r0.zw, r0, r0
mul r7.w, r0.y, c21.y
add r0.y, r0.z, r0.w
rsq r5.w, r0.y
mul r0.z, r7.w, c18.x
mul r0.y, r5.w, r0.z
abs r0.w, c17.x
frc r3.w, r0
add r0.w, r0, -r3
mul r6.xyz, r6, c22.xxyw
mul r7.xyz, r6, r0.y
mov r0.z, r0.x
mov r0.xy, v0
add r8.xyz, r7, r0
cmp r0.x, c17, r0.w, -r0.w
rcp r8.w, r5.w
mov r5.w, r0.x
mov r9.x, r0
mov r6.w, c21.x
mov_pp r3.w, c21.x
mov r9.y, c21.x
loop aL, i0
break_ge r9.y, r9.x
mad r0.w, r8.z, c13.x, c13.y
mov r0.z, c21.x
mov r0.xy, r8
texldl r0.x, r0.xyzz, s1
rcp r0.y, r0.w
mad r0.x, r0, c13, c13.y
add r9.w, r0.y, c22.z
rcp r9.z, r0.x
add r10.x, r9.z, -r9.w
mov r0.xyz, r8
mov r0.w, c21
cmp r2, r10.x, r2, r0
cmp_pp r3.w, r10.x, r3, c21
break_lt r9.z, r9.w
add r8.xyz, r8, r7
add r6.w, r6, c21
add r9.y, r9, c21.w
endloop
mov r0.xyz, r8
abs_pp r3.w, r3
mov r0.w, c21.x
cmp r0, -r3.w, r0, r2
add r3.w, r0.x, c22
mov r2, r0
mov r0.xyz, r0.xyww
abs r8.x, r3.w
mov r3.w, c21.x
mov r0.w, c21.x
cmp r3, -c20.x, r0.w, r3
if_gt r8.x, c22.x
mov r0, r3
else
add r0.w, r2.y, c22
abs r0.w, r0
if_gt r0.w, c22.x
mov r0, r3
else
mad r0.w, r2.z, c13.x, c13.y
rcp r0.w, r0.w
if_gt r0.w, c15.x
mov r0, c21.x
else
if_lt r2.z, c23.x
mov r0, c21.x
else
if_eq r2.w, c21.w
abs r0.y, c16.x
rcp r0.x, r8.w
mul r0.x, r7.w, r0
mul r6.xyz, r6, r0.x
add r2.xyz, r2, -r7
frc r0.z, r0.y
add r0.x, r0.y, -r0.z
add r7.xyz, r6, r2
cmp r2.w, c16.x, r0.x, -r0.x
mov_pp r0.w, c21.x
mov r7.w, c21.x
loop aL, i1
break_ge r7.w, r2.w
mov r0.z, c21.x
mov r0.xy, r7
texldl r0.x, r0.xyzz, s1
mad r0.y, r7.z, c13.x, c13
mad r0.x, r0, c13, c13.y
rcp r0.y, r0.y
rcp r0.x, r0.x
add r0.z, -r0.x, r0.y
add r0.x, r0, -r0.y
add r0.z, r0, -c19.x
cmp r0.y, r0.z, c21.x, c21.w
cmp r8.w, r0.x, c21.x, c21
mul_pp r8.x, r8.w, r0.y
mov r0.xy, r7
mov r0.z, c21.w
cmp r4.xyz, -r8.x, r4, r0
cmp_pp r0.w, -r8.x, r0, c21
break_gt r8.x, c21.x
mul r0.xyz, r6, c22.x
add r8.xyz, r0, r2
cmp r8.xyz, -r8.w, r7, r8
cmp r6.xyz, -r8.w, r6, r0
abs_pp r8.w, r8
add r0.xyz, r6, r8
cmp r7.xyz, -r8.w, r0, r8
cmp r2.xyz, -r8.w, r8, r2
add r7.w, r7, c21
endloop
mov r0.xy, r7
mov r0.z, c21.x
abs_pp r0.w, r0
cmp r0.xyz, -r0.w, r0, r4
endif
if_lt r0.z, c23.y
mov r0, r3
else
dp4 r1.w, r1, r1
rsq r1.w, r1.w
dp3 r0.w, r5, r5
mul r2.xyz, r1.w, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r5
dp3 r1.x, r1, r2
mov r0.w, c14.x
mad r0.w, c23.x, r0, r1.x
add_sat r2.x, r0.w, c21.w
pow r1, r2.x, c14.x
rcp r0.w, r5.w
mul r0.w, r6, r0
pow r2, r0.w, c14.x
rcp r0.w, c15.x
mad r0.w, -r4, r0, c21
mul r0.z, r0, r0.w
mov r1.y, r1.x
mov r1.x, r2
add r1.x, -r1, c21.w
mul r0.z, r0, r1.x
mul r0.w, r0.z, r1.y
mov r0.z, c21.x
texldl r0.xyz, r0.xyzz, s0
endif
endif
endif
endif
endif
endif
endif
mov_pp oC0, r0
"
}

SubProgram "d3d11 " {
Keywords { }
ConstBuffer "$Globals" 272 // 260 used size, 12 vars
Float 16 [_fadePower]
Float 20 [_maxDepthCull]
Float 24 [_maxFineStep]
Float 28 [_maxStep]
Float 32 [_stepGlobalScale]
Float 36 [_bias]
Matrix 48 [_ProjMatrix] 4
Matrix 112 [_ProjectionInv] 4
Matrix 176 [_ViewMatrix] 4
Float 256 [_SSRRcomposeMode]
ConstBuffer "UnityPerCamera" 128 // 128 used size, 8 vars
Vector 96 [_ScreenParams] 4
Vector 112 [_ZBufferParams] 4
BindCB "$Globals" 0
BindCB "UnityPerCamera" 1
SetTexture 0 [_MainTex] 2D 1
SetTexture 1 [_depthTexCustom] 2D 0
SetTexture 2 [_CameraNormalsTexture] 2D 2
// 201 instructions, 13 temp regs, 0 temp arrays:
// ALU 98 float, 8 int, 1 uint
// TEX 0 (6 load, 0 comp, 0 bias, 0 grad)
// FLOW 14 static, 15 dynamic
"ps_4_0
eefiecedijhklfcekgemlkdgneijamfhbkbkgijfabaaaaaapibeaaaaadaaaaaa
cmaaaaaaieaaaaaaliaaaaaaejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcdibeaaaa
eaaaaaaaaoafaaaafjaaaaaeegiocaaaaaaaaaaabbaaaaaafjaaaaaeegiocaaa
abaaaaaaaiaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaae
aahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaagcbaaaad
dcbabaaaabaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacanaaaaaaeiaaaaal
pcaabaaaaaaaaaaaegbabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaa
abeaaaaaaaaaaaaabiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaaaaabpaaaeaddkaabaaaaaaaaaaadgaaaaaipccabaaaaaaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabcaaaaabeiaaaaalpcaabaaaabaaaaaa
egbabaaaabaaaaaajghmbaaaabaaaaaaaagabaaaaaaaaaaaabeaaaaaaaaaaaaa
dcaaaaalicaabaaaaaaaaaaaakiacaaaabaaaaaaahaaaaaackaabaaaabaaaaaa
bkiacaaaabaaaaaaahaaaaaaaoaaaaakicaabaaaaaaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaadbaaaaaiicaabaaaabaaaaaa
bkiacaaaaaaaaaaaabaaaaaadkaabaaaaaaaaaaabpaaaeaddkaabaaaabaaaaaa
dgaaaaaipccabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
bcaaaaabdcaaaaapdcaabaaaacaaaaaaegbabaaaabaaaaaaaceaaaaaaaaaaaea
aaaaaaeaaaaaaaaaaaaaaaaaaceaaaaaaaaaialpaaaaialpaaaaaaaaaaaaaaaa
diaaaaaipcaabaaaadaaaaaafgafbaaaacaaaaaaegiocaaaaaaaaaaaaiaaaaaa
dcaaaaakpcaabaaaacaaaaaaegiocaaaaaaaaaaaahaaaaaaagaabaaaacaaaaaa
egaobaaaadaaaaaadcaaaaakpcaabaaaacaaaaaaegiocaaaaaaaaaaaajaaaaaa
kgakbaaaabaaaaaaegaobaaaacaaaaaaaaaaaaaipcaabaaaacaaaaaaegaobaaa
acaaaaaaegiocaaaaaaaaaaaakaaaaaaaoaaaaahpcaabaaaacaaaaaaegaobaaa
acaaaaaapgapbaaaacaaaaaaeiaaaaalpcaabaaaadaaaaaaegbabaaaabaaaaaa
eghobaaaacaaaaaaaagabaaaacaaaaaaabeaaaaaaaaaaaaadcaaaaaphcaabaaa
adaaaaaaegacbaaaadaaaaaaaceaaaaaaaaaaaeaaaaaaaeaaaaaaaeaaaaaaaaa
aceaaaaaaaaaialpaaaaialpaaaaialpaaaaaaaabaaaaaahicaabaaaabaaaaaa
egacbaaaacaaaaaaegacbaaaacaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaadiaaaaahhcaabaaaaeaaaaaapgapbaaaabaaaaaaegacbaaaacaaaaaa
diaaaaaihcaabaaaafaaaaaafgafbaaaadaaaaaaegiccaaaaaaaaaaaamaaaaaa
dcaaaaaklcaabaaaadaaaaaaegiicaaaaaaaaaaaalaaaaaaagaabaaaadaaaaaa
egaibaaaafaaaaaadcaaaaakhcaabaaaadaaaaaaegiccaaaaaaaaaaaanaaaaaa
kgakbaaaadaaaaaaegadbaaaadaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaa
adaaaaaaegacbaaaadaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaa
diaaaaahhcaabaaaadaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaabaaaaaah
icaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaaeaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaapgapbaaaabaaaaaadcaaaaanhcaabaaaadaaaaaa
egacbaiaebaaaaaaadaaaaaaaceaaaaaaaaaaaeaaaaaaaeaaaaaaaeaaaaaaaaa
egacbaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaa
adaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
aeaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaadaaaaaa
egacbaaaadaaaaaapgapbaaaabaaaaaaegacbaaaacaaaaaadiaaaaaipcaabaaa
afaaaaaafgafbaaaadaaaaaaegiocaaaaaaaaaaaaeaaaaaadcaaaaakpcaabaaa
afaaaaaaegiocaaaaaaaaaaaadaaaaaaagaabaaaadaaaaaaegaobaaaafaaaaaa
dcaaaaakpcaabaaaadaaaaaaegiocaaaaaaaaaaaafaaaaaakgakbaaaadaaaaaa
egaobaaaafaaaaaaaaaaaaaipcaabaaaadaaaaaaegaobaaaadaaaaaaegiocaaa
aaaaaaaaagaaaaaaaoaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaapgapbaaa
adaaaaaadcaaaaapdcaabaaaabaaaaaaegbabaaaabaaaaaaaceaaaaaaaaaaaea
aaaaaaeaaaaaaaaaaaaaaaaaaceaaaaaaaaaialpaaaaialpaaaaaaaaaaaaaaaa
aaaaaaaihcaabaaaadaaaaaaegacbaiaebaaaaaaabaaaaaaegacbaaaadaaaaaa
baaaaaahicaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaadaaaaaaeeaaaaaf
icaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaapgapbaaa
abaaaaaaegacbaaaadaaaaaadiaaaaakdcaabaaaafaaaaaaegaabaaaadaaaaaa
aceaaaaaaaaaaadpaaaaaadpaaaaaaaaaaaaaaaaaoaaaaaiicaabaaaabaaaaaa
abeaaaaaaaaaaaeaakiacaaaabaaaaaaagaaaaaaapaaaaahicaabaaaadaaaaaa
egaabaaaafaaaaaaegaabaaaafaaaaaaelaaaaaficaabaaaadaaaaaadkaabaaa
adaaaaaadiaaaaaiicaabaaaaeaaaaaadkaabaaaabaaaaaaakiacaaaaaaaaaaa
acaaaaaaaoaaaaahicaabaaaaeaaaaaadkaabaaaaeaaaaaadkaabaaaadaaaaaa
diaaaaakhcaabaaaadaaaaaaegacbaaaadaaaaaaaceaaaaaaaaaaadpaaaaaadp
aaaaiadpaaaaaaaablaaaaagbcaabaaaafaaaaaadkiacaaaaaaaaaaaabaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaabaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaadaaaaaapgapbaaaaeaaaaaaegacbaaaabaaaaaadgaaaaaficaabaaa
agaaaaaaabeaaaaaaaaaiadpdgaaaaaipcaabaaaahaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaadgaaaaafhcaabaaaagaaaaaaegacbaaaabaaaaaa
dgaaaaaiocaabaaaafaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
dgaaaaafbcaabaaaaiaaaaaaabeaaaaaaaaaaaaadaaaaaabcbaaaaahccaabaaa
aiaaaaaaakaabaaaaiaaaaaaabeaaaaageaaaaaaadaaaeadbkaabaaaaiaaaaaa
cbaaaaahccaabaaaaiaaaaaadkaabaaaafaaaaaaakaabaaaafaaaaaabpaaaead
bkaabaaaaiaaaaaaacaaaaabbfaaaaabeiaaaaalpcaabaaaajaaaaaaegaabaaa
agaaaaaaeghobaaaabaaaaaaaagabaaaaaaaaaaaabeaaaaaaaaaaaaadcaaaaal
ccaabaaaaiaaaaaaakiacaaaabaaaaaaahaaaaaaakaabaaaajaaaaaabkiacaaa
abaaaaaaahaaaaaaaoaaaaakccaabaaaaiaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpbkaabaaaaiaaaaaadcaaaaalecaabaaaaiaaaaaaakiacaaa
abaaaaaaahaaaaaackaabaaaagaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaak
ecaabaaaaiaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpckaabaaa
aiaaaaaaaaaaaaahecaabaaaaiaaaaaackaabaaaaiaaaaaaabeaaaaalndhiglf
dbaaaaahccaabaaaaiaaaaaabkaabaaaaiaaaaaackaabaaaaiaaaaaabpaaaead
bkaabaaaaiaaaaaadgaaaaafpcaabaaaahaaaaaaegaobaaaagaaaaaadgaaaaaf
ecaabaaaafaaaaaaabeaaaaappppppppacaaaaabbfaaaaabdcaaaaajhcaabaaa
agaaaaaaegacbaaaadaaaaaapgapbaaaaeaaaaaaegacbaaaagaaaaaaaaaaaaah
ccaabaaaafaaaaaabkaabaaaafaaaaaaabeaaaaaaaaaiadpboaaaaahicaabaaa
afaaaaaadkaabaaaafaaaaaaabeaaaaaabaaaaaaboaaaaahbcaabaaaaiaaaaaa
akaabaaaaiaaaaaaabeaaaaaabaaaaaadgaaaaaipcaabaaaahaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadgaaaaafecaabaaaafaaaaaaabeaaaaa
aaaaaaaabgaaaaabdgaaaaaficaabaaaagaaaaaaabeaaaaaaaaaaaaadhaaaaaj
pcaabaaaagaaaaaakgakbaaaafaaaaaaegaobaaaahaaaaaaegaobaaaagaaaaaa
aaaaaaahbcaabaaaabaaaaaaakaabaaaagaaaaaaabeaaaaaaaaaaalpdbaaaaai
ccaabaaaabaaaaaaabeaaaaaaaaaaaaaakiacaaaaaaaaaaabaaaaaaaabaaaaah
hcaabaaaahaaaaaaegacbaaaaaaaaaaafgafbaaaabaaaaaadgaaaaaficaabaaa
ahaaaaaaabeaaaaaaaaaaaaadbaaaaaibcaabaaaaaaaaaaaabeaaaaaaaaaaadp
akaabaiaibaaaaaaabaaaaaabpaaaeadakaabaaaaaaaaaaadgaaaaafpccabaaa
aaaaaaaaegaobaaaahaaaaaabcaaaaabaaaaaaahbcaabaaaaaaaaaaabkaabaaa
agaaaaaaabeaaaaaaaaaaalpdbaaaaaibcaabaaaaaaaaaaaabeaaaaaaaaaaadp
akaabaiaibaaaaaaaaaaaaaabpaaaeadakaabaaaaaaaaaaadgaaaaafpccabaaa
aaaaaaaaegaobaaaahaaaaaabcaaaaabdcaaaaalbcaabaaaaaaaaaaaakiacaaa
abaaaaaaahaaaaaackaabaaaagaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaak
bcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaa
aaaaaaaadbaaaaaibcaabaaaaaaaaaaabkiacaaaaaaaaaaaabaaaaaaakaabaaa
aaaaaaaabpaaaeadakaabaaaaaaaaaaadgaaaaaipccabaaaaaaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabcaaaaabdbaaaaahbcaabaaaaaaaaaaa
ckaabaaaagaaaaaaabeaaaaamnmmmmdnbpaaaeadakaabaaaaaaaaaaadgaaaaai
pccabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabcaaaaab
biaaaaahbcaabaaaaaaaaaaadkaabaaaagaaaaaaabeaaaaaaaaaiadpbpaaaead
akaabaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaaegacbaiaebaaaaaaadaaaaaa
pgapbaaaaeaaaaaaegacbaaaagaaaaaaaoaaaaahbcaabaaaabaaaaaadkaabaaa
abaaaaaadkaabaaaadaaaaaadiaaaaahocaabaaaabaaaaaaagaabaaaabaaaaaa
agajbaaaadaaaaaablaaaaagicaabaaaadaaaaaackiacaaaaaaaaaaaabaaaaaa
dcaaaaajhcaabaaaadaaaaaaegacbaaaadaaaaaaagaabaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaafecaabaaaaiaaaaaaabeaaaaaaaaaiadpdgaaaaafncaabaaa
afaaaaaafgaobaaaabaaaaaadgaaaaaihcaabaaaajaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaadgaaaaafhcaabaaaakaaaaaaegacbaaaaaaaaaaa
dgaaaaafhcaabaaaalaaaaaaegacbaaaadaaaaaadgaaaaafbcaabaaaabaaaaaa
abeaaaaaaaaaaaaadgaaaaaficaabaaaaeaaaaaaabeaaaaaaaaaaaaadgaaaaaf
icaabaaaagaaaaaaabeaaaaaaaaaaaaadaaaaaabcbaaaaahicaabaaaaiaaaaaa
dkaabaaaagaaaaaaabeaaaaabeaaaaaaadaaaeaddkaabaaaaiaaaaaacbaaaaah
icaabaaaaiaaaaaadkaabaaaaeaaaaaadkaabaaaadaaaaaabpaaaeaddkaabaaa
aiaaaaaaacaaaaabbfaaaaabeiaaaaalpcaabaaaamaaaaaaegaabaaaalaaaaaa
eghobaaaabaaaaaaaagabaaaaaaaaaaaabeaaaaaaaaaaaaadcaaaaalicaabaaa
aiaaaaaaakiacaaaabaaaaaaahaaaaaaakaabaaaamaaaaaabkiacaaaabaaaaaa
ahaaaaaaaoaaaaakicaabaaaaiaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpdkaabaaaaiaaaaaadcaaaaalicaabaaaajaaaaaaakiacaaaabaaaaaa
ahaaaaaackaabaaaalaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaakicaabaaa
ajaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaajaaaaaa
dbaaaaahicaabaaaakaaaaaadkaabaaaaiaaaaaadkaabaaaajaaaaaabpaaaead
dkaabaaaakaaaaaaaaaaaaaiicaabaaaaiaaaaaadkaabaiaebaaaaaaaiaaaaaa
dkaabaaaajaaaaaadbaaaaaiicaabaaaaiaaaaaadkaabaaaaiaaaaaabkiacaaa
aaaaaaaaacaaaaaabpaaaeaddkaabaaaaiaaaaaadgaaaaafdcaabaaaaiaaaaaa
egaabaaaalaaaaaadgaaaaafhcaabaaaajaaaaaaegacbaaaaiaaaaaadgaaaaaf
bcaabaaaabaaaaaaabeaaaaappppppppacaaaaabbfaaaaabdiaaaaaklcaabaaa
aiaaaaaaigambaaaafaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaaaaaaaaaadp
dcaaaaamhcaabaaaalaaaaaaigadbaaaafaaaaaaaceaaaaaaaaaaadpaaaaaadp
aaaaaadpaaaaaaaaegacbaaaakaaaaaadgaaaaafncaabaaaafaaaaaaaganbaaa
aiaaaaaabcaaaaabaaaaaaahlcaabaaaaiaaaaaaigambaaaafaaaaaaegaibaaa
alaaaaaadgaaaaafhcaabaaaakaaaaaaegacbaaaalaaaaaadgaaaaafhcaabaaa
alaaaaaaegadbaaaaiaaaaaabfaaaaabboaaaaahicaabaaaaeaaaaaadkaabaaa
aeaaaaaaabeaaaaaabaaaaaaboaaaaahicaabaaaagaaaaaadkaabaaaagaaaaaa
abeaaaaaabaaaaaadgaaaaaihcaabaaaajaaaaaaaceaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaaabeaaaaaaaaaaaaabgaaaaab
dgaaaaafecaabaaaalaaaaaaabeaaaaaaaaaaaaadhaaaaajhcaabaaaagaaaaaa
agaabaaaabaaaaaaegacbaaaajaaaaaaegacbaaaalaaaaaabcaaaaabdgaaaaaf
ecaabaaaagaaaaaaabeaaaaaaaaaaaaabfaaaaabdbaaaaahbcaabaaaaaaaaaaa
ckaabaaaagaaaaaaabeaaaaaaknhcddmbpaaaeadakaabaaaaaaaaaaadgaaaaaf
pccabaaaaaaaaaaaegaobaaaahaaaaaabcaaaaabeiaaaaalpcaabaaaabaaaaaa
egaabaaaagaaaaaaeghobaaaaaaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaa
aoaaaaaibcaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaabaaaaaa
aaaaaaaibcaabaaaaaaaaaaaakaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadp
diaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaackaabaaaagaaaaaaedaaaaag
ccaabaaaaaaaaaaadkiacaaaaaaaaaaaabaaaaaaaoaaaaahccaabaaaaaaaaaaa
bkaabaaaafaaaaaabkaabaaaaaaaaaaacpaaaaafccaabaaaaaaaaaaabkaabaaa
aaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaaaaaaaaa
abaaaaaabjaaaaafccaabaaaaaaaaaaabkaabaaaaaaaaaaaaaaaaaaiccaabaaa
aaaaaaaabkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahbcaabaaa
aaaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaabbaaaaahccaabaaaaaaaaaaa
egaobaaaacaaaaaaegaobaaaacaaaaaaeeaaaaafccaabaaaaaaaaaaabkaabaaa
aaaaaaaadiaaaaahocaabaaaaaaaaaaafgafbaaaaaaaaaaaagajbaaaacaaaaaa
baaaaaahccaabaaaaaaaaaaaegacbaaaaeaaaaaajgahbaaaaaaaaaaaaaaaaaah
ccaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaiadpdccaaaakccaabaaa
aaaaaaaaakiacaaaaaaaaaaaabaaaaaaabeaaaaamnmmmmdnbkaabaaaaaaaaaaa
cpaaaaafccaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakiacaaaaaaaaaaaabaaaaaabjaaaaafccaabaaaaaaaaaaa
bkaabaaaaaaaaaaadiaaaaahiccabaaaaaaaaaaabkaabaaaaaaaaaaaakaabaaa
aaaaaaaadgaaaaafhccabaaaaaaaaaaaegacbaaaabaaaaaabfaaaaabbfaaaaab
bfaaaaabbfaaaaabbfaaaaabbfaaaaabbfaaaaabdoaaaaab"
}

SubProgram "gles " {
Keywords { }
"!!GLES"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES"
}

SubProgram "gles3 " {
Keywords { }
"!!GLES3"
}

}

#LINE 297

	}
	
	
	//================================================================================================================================================
	//USING UNITY DEPTH TEXTURE - BEGIN OF PASS 2
	//================================================================================================================================================
	
	Pass {
				Program "vp" {
// Vertex combos: 1
//   d3d9 - ALU: 5 to 5
//   d3d11 - ALU: 4 to 4, TEX: 0 to 0, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
"!!GLSL
#ifdef VERTEX
varying vec2 xlv_TEXCOORD0;

void main ()
{
  gl_Position = (gl_ModelViewProjectionMatrix * gl_Vertex);
  xlv_TEXCOORD0 = gl_MultiTexCoord0.xy;
}


#endif
#ifdef FRAGMENT
#extension GL_ARB_shader_texture_lod : enable
varying vec2 xlv_TEXCOORD0;
uniform float _SSRRcomposeMode;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _CameraNormalsTexture;
uniform mat4 _ViewMatrix;
uniform mat4 _ProjectionInv;
uniform mat4 _ProjMatrix;
uniform float _bias;
uniform float _stepGlobalScale;
uniform float _maxStep;
uniform float _maxFineStep;
uniform float _maxDepthCull;
uniform float _fadePower;
uniform sampler2D _MainTex;
uniform vec4 _ZBufferParams;
uniform vec4 _ScreenParams;
void main ()
{
  vec3 lkjwejhsdkl_1;
  vec4 opahwcte_2;
  vec4 xbfaeiaej12s_3;
  vec4 tmpvar_4;
  tmpvar_4 = texture2DLod (_MainTex, xlv_TEXCOORD0, 0.0);
  if ((tmpvar_4.w == 0.0)) {
    xbfaeiaej12s_3 = vec4(0.0, 0.0, 0.0, 0.0);
  } else {
    vec4 tmpvar_5;
    tmpvar_5 = texture2DLod (_CameraDepthTexture, xlv_TEXCOORD0, 0.0);
    float tmpvar_6;
    tmpvar_6 = tmpvar_5.x;
    float tmpvar_7;
    tmpvar_7 = (1.0/(((_ZBufferParams.x * tmpvar_5.x) + _ZBufferParams.y)));
    if ((tmpvar_7 > _maxDepthCull)) {
      xbfaeiaej12s_3 = vec4(0.0, 0.0, 0.0, 0.0);
    } else {
      vec4 acccols_8;
      int s_9;
      vec4 uiduefa_10;
      int icoiuf_11;
      bool biifejd_12;
      vec4 rensfief_13;
      float lenfaiejd_14;
      int vbdueff_15;
      vec3 eiieiaced_16;
      vec3 jjdafhue_17;
      vec3 hgeiald_18;
      vec4 loveeaed_19;
      vec4 mcjkfeeieijd_20;
      vec3 xvzyufalj_21;
      vec4 efljafolclsdf_22;
      int tmpvar_23;
      tmpvar_23 = int(_maxStep);
      efljafolclsdf_22.w = 1.0;
      efljafolclsdf_22.xy = ((xlv_TEXCOORD0 * 2.0) - 1.0);
      efljafolclsdf_22.z = tmpvar_6;
      vec4 tmpvar_24;
      tmpvar_24 = (_ProjectionInv * efljafolclsdf_22);
      vec4 tmpvar_25;
      tmpvar_25 = (tmpvar_24 / tmpvar_24.w);
      xvzyufalj_21.xy = efljafolclsdf_22.xy;
      xvzyufalj_21.z = tmpvar_6;
      mcjkfeeieijd_20.w = 0.0;
      mcjkfeeieijd_20.xyz = ((texture2DLod (_CameraNormalsTexture, xlv_TEXCOORD0, 0.0).xyz * 2.0) - 1.0);
      vec3 tmpvar_26;
      tmpvar_26 = normalize(tmpvar_25.xyz);
      vec3 tmpvar_27;
      tmpvar_27 = normalize((_ViewMatrix * mcjkfeeieijd_20).xyz);
      vec3 tmpvar_28;
      tmpvar_28 = normalize((tmpvar_26 - (2.0 * (dot (tmpvar_27, tmpvar_26) * tmpvar_27))));
      loveeaed_19.w = 1.0;
      loveeaed_19.xyz = (tmpvar_25.xyz + tmpvar_28);
      vec4 tmpvar_29;
      tmpvar_29 = (_ProjMatrix * loveeaed_19);
      vec3 tmpvar_30;
      tmpvar_30 = normalize(((tmpvar_29.xyz / tmpvar_29.w) - xvzyufalj_21));
      lkjwejhsdkl_1.z = tmpvar_30.z;
      lkjwejhsdkl_1.xy = (tmpvar_30.xy * 0.5);
      hgeiald_18.xy = xlv_TEXCOORD0;
      hgeiald_18.z = tmpvar_6;
      float tmpvar_31;
      tmpvar_31 = (2.0 / _ScreenParams.x);
      float tmpvar_32;
      tmpvar_32 = sqrt(dot (lkjwejhsdkl_1.xy, lkjwejhsdkl_1.xy));
      vec3 tmpvar_33;
      tmpvar_33 = (lkjwejhsdkl_1 * ((tmpvar_31 * _stepGlobalScale) / tmpvar_32));
      jjdafhue_17 = tmpvar_33;
      vbdueff_15 = int(_maxStep);
      lenfaiejd_14 = 0.0;
      biifejd_12 = bool(0);
      eiieiaced_16 = (hgeiald_18 + tmpvar_33);
      icoiuf_11 = 0;
      s_9 = 0;
      for (int s_9 = 0; s_9 < 100; ) {
        if ((icoiuf_11 >= vbdueff_15)) {
          break;
        };
        float tmpvar_34;
        tmpvar_34 = (1.0/(((_ZBufferParams.x * texture2DLod (_CameraDepthTexture, eiieiaced_16.xy, 0.0).x) + _ZBufferParams.y)));
        float tmpvar_35;
        tmpvar_35 = (1.0/(((_ZBufferParams.x * eiieiaced_16.z) + _ZBufferParams.y)));
        if ((tmpvar_34 < (tmpvar_35 - 1e-06))) {
          uiduefa_10.w = 1.0;
          uiduefa_10.xyz = eiieiaced_16;
          rensfief_13 = uiduefa_10;
          biifejd_12 = bool(1);
          break;
        };
        eiieiaced_16 = (eiieiaced_16 + jjdafhue_17);
        lenfaiejd_14 = (lenfaiejd_14 + 1.0);
        icoiuf_11 = (icoiuf_11 + 1);
        s_9 = (s_9 + 1);
      };
      if ((biifejd_12 == bool(0))) {
        vec4 vartfie_36;
        vartfie_36.w = 0.0;
        vartfie_36.xyz = eiieiaced_16;
        rensfief_13 = vartfie_36;
        biifejd_12 = bool(1);
      };
      opahwcte_2 = rensfief_13;
      float tmpvar_37;
      tmpvar_37 = abs((rensfief_13.x - 0.5));
      acccols_8 = vec4(0.0, 0.0, 0.0, 0.0);
      if ((_SSRRcomposeMode > 0.0)) {
        vec4 tmpvar_38;
        tmpvar_38.w = 0.0;
        tmpvar_38.xyz = tmpvar_4.xyz;
        acccols_8 = tmpvar_38;
      };
      if ((tmpvar_37 > 0.5)) {
        xbfaeiaej12s_3 = acccols_8;
      } else {
        float tmpvar_39;
        tmpvar_39 = abs((rensfief_13.y - 0.5));
        if ((tmpvar_39 > 0.5)) {
          xbfaeiaej12s_3 = acccols_8;
        } else {
          if (((1.0/(((_ZBufferParams.x * rensfief_13.z) + _ZBufferParams.y))) > _maxDepthCull)) {
            xbfaeiaej12s_3 = vec4(0.0, 0.0, 0.0, 0.0);
          } else {
            if ((rensfief_13.z < 0.1)) {
              xbfaeiaej12s_3 = vec4(0.0, 0.0, 0.0, 0.0);
            } else {
              if ((rensfief_13.w == 1.0)) {
                int j_40;
                vec4 greyfsd_41;
                vec3 poffses_42;
                int i_49_43;
                bool fjekfesa_44;
                vec4 alsdmes_45;
                int maxfeis_46;
                vec3 refDir_44_47;
                vec3 oifejef_48;
                vec3 tmpvar_49;
                tmpvar_49 = (rensfief_13.xyz - tmpvar_33);
                vec3 tmpvar_50;
                tmpvar_50 = (lkjwejhsdkl_1 * (tmpvar_31 / tmpvar_32));
                refDir_44_47 = tmpvar_50;
                maxfeis_46 = int(_maxFineStep);
                fjekfesa_44 = bool(0);
                poffses_42 = tmpvar_49;
                oifejef_48 = (tmpvar_49 + tmpvar_50);
                i_49_43 = 0;
                j_40 = 0;
                for (int j_40 = 0; j_40 < 20; ) {
                  if ((i_49_43 >= maxfeis_46)) {
                    break;
                  };
                  float tmpvar_51;
                  tmpvar_51 = (1.0/(((_ZBufferParams.x * texture2DLod (_CameraDepthTexture, oifejef_48.xy, 0.0).x) + _ZBufferParams.y)));
                  float tmpvar_52;
                  tmpvar_52 = (1.0/(((_ZBufferParams.x * oifejef_48.z) + _ZBufferParams.y)));
                  if ((tmpvar_51 < tmpvar_52)) {
                    if (((tmpvar_52 - tmpvar_51) < _bias)) {
                      greyfsd_41.w = 1.0;
                      greyfsd_41.xyz = oifejef_48;
                      alsdmes_45 = greyfsd_41;
                      fjekfesa_44 = bool(1);
                      break;
                    };
                    vec3 tmpvar_53;
                    tmpvar_53 = (refDir_44_47 * 0.5);
                    refDir_44_47 = tmpvar_53;
                    oifejef_48 = (poffses_42 + tmpvar_53);
                  } else {
                    poffses_42 = oifejef_48;
                    oifejef_48 = (oifejef_48 + refDir_44_47);
                  };
                  i_49_43 = (i_49_43 + 1);
                  j_40 = (j_40 + 1);
                };
                if ((fjekfesa_44 == bool(0))) {
                  vec4 tmpvar_55_54;
                  tmpvar_55_54.w = 0.0;
                  tmpvar_55_54.xyz = oifejef_48;
                  alsdmes_45 = tmpvar_55_54;
                  fjekfesa_44 = bool(1);
                };
                opahwcte_2 = alsdmes_45;
              };
              if ((opahwcte_2.w < 0.01)) {
                xbfaeiaej12s_3 = acccols_8;
              } else {
                vec4 tmpvar_57_55;
                tmpvar_57_55.xyz = texture2DLod (_MainTex, opahwcte_2.xy, 0.0).xyz;
                tmpvar_57_55.w = (((opahwcte_2.w * (1.0 - (tmpvar_7 / _maxDepthCull))) * (1.0 - pow ((lenfaiejd_14 / float(tmpvar_23)), _fadePower))) * pow (clamp (((dot (normalize(tmpvar_28), normalize(tmpvar_25).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                xbfaeiaej12s_3 = tmpvar_57_55;
              };
            };
          };
        };
      };
    };
  };
  gl_FragData[0] = xbfaeiaej12s_3;
}


#endif
"
}

SubProgram "d3d9 " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
"vs_3_0
; 5 ALU
dcl_position o0
dcl_texcoord0 o1
dcl_position0 v0
dcl_texcoord0 v1
mov o1.xy, v1
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

SubProgram "d3d11 " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
ConstBuffer "UnityPerDraw" 336 // 64 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
BindCB "UnityPerDraw" 0
// 6 instructions, 1 temp regs, 0 temp arrays:
// ALU 4 float, 0 int, 0 uint
// TEX 0 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"vs_4_0
eefiecedaffpdldohodkdgpagjklpapmmnbhcfmlabaaaaaaoeabaaaaadaaaaaa
cmaaaaaaiaaaaaaaniaaaaaaejfdeheoemaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaaebaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafaepfdejfeejepeoaafeeffiedepepfceeaaklkl
epfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
fdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefcaeabaaaa
eaaaabaaebaaaaaafjaaaaaeegiocaaaaaaaaaaaaeaaaaaafpaaaaadpcbabaaa
aaaaaaaafpaaaaaddcbabaaaabaaaaaaghaaaaaepccabaaaaaaaaaaaabaaaaaa
gfaaaaaddccabaaaabaaaaaagiaaaaacabaaaaaadiaaaaaipcaabaaaaaaaaaaa
fgbfbaaaaaaaaaaaegiocaaaaaaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaa
egiocaaaaaaaaaaaaaaaaaaaagbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaak
pcaabaaaaaaaaaaaegiocaaaaaaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaa
aaaaaaaadcaaaaakpccabaaaaaaaaaaaegiocaaaaaaaaaaaadaaaaaapgbpbaaa
aaaaaaaaegaobaaaaaaaaaaadgaaaaafdccabaaaabaaaaaaegbabaaaabaaaaaa
doaaaaab"
}

SubProgram "gles " {
Keywords { }
"!!GLES


#ifdef VERTEX

varying highp vec2 xlv_TEXCOORD0;
uniform highp mat4 glstate_matrix_mvp;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesVertex;
void main ()
{
  highp vec2 tmpvar_1;
  mediump vec2 tmpvar_2;
  tmpvar_2 = _glesMultiTexCoord0.xy;
  tmpvar_1 = tmpvar_2;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = tmpvar_1;
}



#endif
#ifdef FRAGMENT

#extension GL_EXT_shader_texture_lod : enable
varying highp vec2 xlv_TEXCOORD0;
uniform highp float _SSRRcomposeMode;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _CameraNormalsTexture;
uniform highp mat4 _ViewMatrix;
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ProjMatrix;
uniform highp float _bias;
uniform highp float _stepGlobalScale;
uniform highp float _maxStep;
uniform highp float _maxFineStep;
uniform highp float _maxDepthCull;
uniform highp float _fadePower;
uniform sampler2D _MainTex;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 _ScreenParams;
void main ()
{
  mediump vec4 tmpvar_1;
  highp vec4 odheoldj_2;
  highp vec3 lkjwejhsdkl_3;
  highp vec4 opahwcte_4;
  highp vec4 xbfaeiaej12s_5;
  lowp vec4 tmpvar_6;
  tmpvar_6 = texture2DLodEXT (_MainTex, xlv_TEXCOORD0, 0.0);
  odheoldj_2 = tmpvar_6;
  if ((odheoldj_2.w == 0.0)) {
    xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
  } else {
    highp float tmpskdkx_7;
    lowp float tmpvar_8;
    tmpvar_8 = texture2DLodEXT (_CameraDepthTexture, xlv_TEXCOORD0, 0.0).x;
    tmpskdkx_7 = tmpvar_8;
    highp float tmpvar_9;
    tmpvar_9 = (1.0/(((_ZBufferParams.x * tmpskdkx_7) + _ZBufferParams.y)));
    if ((tmpvar_9 > _maxDepthCull)) {
      xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
    } else {
      highp vec4 acccols_10;
      int s_11;
      highp vec4 uiduefa_12;
      int icoiuf_13;
      bool biifejd_14;
      highp vec4 rensfief_15;
      highp float lenfaiejd_16;
      int vbdueff_17;
      highp vec3 eiieiaced_18;
      highp vec3 jjdafhue_19;
      highp vec3 hgeiald_20;
      highp vec4 loveeaed_21;
      highp vec4 mcjkfeeieijd_22;
      highp vec3 xvzyufalj_23;
      highp vec4 efljafolclsdf_24;
      int tmpvar_25;
      tmpvar_25 = int(_maxStep);
      efljafolclsdf_24.w = 1.0;
      efljafolclsdf_24.xy = ((xlv_TEXCOORD0 * 2.0) - 1.0);
      efljafolclsdf_24.z = tmpskdkx_7;
      highp vec4 tmpvar_26;
      tmpvar_26 = (_ProjectionInv * efljafolclsdf_24);
      highp vec4 tmpvar_27;
      tmpvar_27 = (tmpvar_26 / tmpvar_26.w);
      xvzyufalj_23.xy = efljafolclsdf_24.xy;
      xvzyufalj_23.z = tmpskdkx_7;
      mcjkfeeieijd_22.w = 0.0;
      lowp vec3 tmpvar_28;
      tmpvar_28 = ((texture2DLodEXT (_CameraNormalsTexture, xlv_TEXCOORD0, 0.0).xyz * 2.0) - 1.0);
      mcjkfeeieijd_22.xyz = tmpvar_28;
      highp vec3 tmpvar_29;
      tmpvar_29 = normalize(tmpvar_27.xyz);
      highp vec3 tmpvar_30;
      tmpvar_30 = normalize((_ViewMatrix * mcjkfeeieijd_22).xyz);
      highp vec3 tmpvar_31;
      tmpvar_31 = normalize((tmpvar_29 - (2.0 * (dot (tmpvar_30, tmpvar_29) * tmpvar_30))));
      loveeaed_21.w = 1.0;
      loveeaed_21.xyz = (tmpvar_27.xyz + tmpvar_31);
      highp vec4 tmpvar_32;
      tmpvar_32 = (_ProjMatrix * loveeaed_21);
      highp vec3 tmpvar_33;
      tmpvar_33 = normalize(((tmpvar_32.xyz / tmpvar_32.w) - xvzyufalj_23));
      lkjwejhsdkl_3.z = tmpvar_33.z;
      lkjwejhsdkl_3.xy = (tmpvar_33.xy * 0.5);
      hgeiald_20.xy = xlv_TEXCOORD0;
      hgeiald_20.z = tmpskdkx_7;
      highp float tmpvar_34;
      tmpvar_34 = (2.0 / _ScreenParams.x);
      highp float tmpvar_35;
      tmpvar_35 = sqrt(dot (lkjwejhsdkl_3.xy, lkjwejhsdkl_3.xy));
      highp vec3 tmpvar_36;
      tmpvar_36 = (lkjwejhsdkl_3 * ((tmpvar_34 * _stepGlobalScale) / tmpvar_35));
      jjdafhue_19 = tmpvar_36;
      vbdueff_17 = int(_maxStep);
      lenfaiejd_16 = 0.0;
      biifejd_14 = bool(0);
      eiieiaced_18 = (hgeiald_20 + tmpvar_36);
      icoiuf_13 = 0;
      s_11 = 0;
      for (int s_11 = 0; s_11 < 100; ) {
        if ((icoiuf_13 >= vbdueff_17)) {
          break;
        };
        lowp vec4 tmpvar_37;
        tmpvar_37 = texture2DLodEXT (_CameraDepthTexture, eiieiaced_18.xy, 0.0);
        highp float tmpvar_38;
        tmpvar_38 = (1.0/(((_ZBufferParams.x * tmpvar_37.x) + _ZBufferParams.y)));
        highp float tmpvar_39;
        tmpvar_39 = (1.0/(((_ZBufferParams.x * eiieiaced_18.z) + _ZBufferParams.y)));
        if ((tmpvar_38 < (tmpvar_39 - 1e-06))) {
          uiduefa_12.w = 1.0;
          uiduefa_12.xyz = eiieiaced_18;
          rensfief_15 = uiduefa_12;
          biifejd_14 = bool(1);
          break;
        };
        eiieiaced_18 = (eiieiaced_18 + jjdafhue_19);
        lenfaiejd_16 = (lenfaiejd_16 + 1.0);
        icoiuf_13 = (icoiuf_13 + 1);
        s_11 = (s_11 + 1);
      };
      if ((biifejd_14 == bool(0))) {
        highp vec4 vartfie_40;
        vartfie_40.w = 0.0;
        vartfie_40.xyz = eiieiaced_18;
        rensfief_15 = vartfie_40;
        biifejd_14 = bool(1);
      };
      opahwcte_4 = rensfief_15;
      highp float tmpvar_41;
      tmpvar_41 = abs((rensfief_15.x - 0.5));
      acccols_10 = vec4(0.0, 0.0, 0.0, 0.0);
      if ((_SSRRcomposeMode > 0.0)) {
        highp vec4 tmpvar_42;
        tmpvar_42.w = 0.0;
        tmpvar_42.xyz = odheoldj_2.xyz;
        acccols_10 = tmpvar_42;
      };
      if ((tmpvar_41 > 0.5)) {
        xbfaeiaej12s_5 = acccols_10;
      } else {
        highp float tmpvar_43;
        tmpvar_43 = abs((rensfief_15.y - 0.5));
        if ((tmpvar_43 > 0.5)) {
          xbfaeiaej12s_5 = acccols_10;
        } else {
          if (((1.0/(((_ZBufferParams.x * rensfief_15.z) + _ZBufferParams.y))) > _maxDepthCull)) {
            xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
          } else {
            if ((rensfief_15.z < 0.1)) {
              xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
            } else {
              if ((rensfief_15.w == 1.0)) {
                int j_44;
                highp vec4 greyfsd_45;
                highp vec3 poffses_46;
                int i_49_47;
                bool fjekfesa_48;
                highp vec4 alsdmes_49;
                int maxfeis_50;
                highp vec3 refDir_44_51;
                highp vec3 oifejef_52;
                highp vec3 tmpvar_53;
                tmpvar_53 = (rensfief_15.xyz - tmpvar_36);
                highp vec3 tmpvar_54;
                tmpvar_54 = (lkjwejhsdkl_3 * (tmpvar_34 / tmpvar_35));
                refDir_44_51 = tmpvar_54;
                maxfeis_50 = int(_maxFineStep);
                fjekfesa_48 = bool(0);
                poffses_46 = tmpvar_53;
                oifejef_52 = (tmpvar_53 + tmpvar_54);
                i_49_47 = 0;
                j_44 = 0;
                for (int j_44 = 0; j_44 < 20; ) {
                  if ((i_49_47 >= maxfeis_50)) {
                    break;
                  };
                  lowp vec4 tmpvar_55;
                  tmpvar_55 = texture2DLodEXT (_CameraDepthTexture, oifejef_52.xy, 0.0);
                  highp float tmpvar_56;
                  tmpvar_56 = (1.0/(((_ZBufferParams.x * tmpvar_55.x) + _ZBufferParams.y)));
                  highp float tmpvar_57;
                  tmpvar_57 = (1.0/(((_ZBufferParams.x * oifejef_52.z) + _ZBufferParams.y)));
                  if ((tmpvar_56 < tmpvar_57)) {
                    if (((tmpvar_57 - tmpvar_56) < _bias)) {
                      greyfsd_45.w = 1.0;
                      greyfsd_45.xyz = oifejef_52;
                      alsdmes_49 = greyfsd_45;
                      fjekfesa_48 = bool(1);
                      break;
                    };
                    highp vec3 tmpvar_58;
                    tmpvar_58 = (refDir_44_51 * 0.5);
                    refDir_44_51 = tmpvar_58;
                    oifejef_52 = (poffses_46 + tmpvar_58);
                  } else {
                    poffses_46 = oifejef_52;
                    oifejef_52 = (oifejef_52 + refDir_44_51);
                  };
                  i_49_47 = (i_49_47 + 1);
                  j_44 = (j_44 + 1);
                };
                if ((fjekfesa_48 == bool(0))) {
                  highp vec4 tmpvar_55_59;
                  tmpvar_55_59.w = 0.0;
                  tmpvar_55_59.xyz = oifejef_52;
                  alsdmes_49 = tmpvar_55_59;
                  fjekfesa_48 = bool(1);
                };
                opahwcte_4 = alsdmes_49;
              };
              if ((opahwcte_4.w < 0.01)) {
                xbfaeiaej12s_5 = acccols_10;
              } else {
                highp vec4 tmpvar_57_60;
                lowp vec3 tmpvar_61;
                tmpvar_61 = texture2DLodEXT (_MainTex, opahwcte_4.xy, 0.0).xyz;
                tmpvar_57_60.xyz = tmpvar_61;
                tmpvar_57_60.w = (((opahwcte_4.w * (1.0 - (tmpvar_9 / _maxDepthCull))) * (1.0 - pow ((lenfaiejd_16 / float(tmpvar_25)), _fadePower))) * pow (clamp (((dot (normalize(tmpvar_31), normalize(tmpvar_27).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                xbfaeiaej12s_5 = tmpvar_57_60;
              };
            };
          };
        };
      };
    };
  };
  tmpvar_1 = xbfaeiaej12s_5;
  gl_FragData[0] = tmpvar_1;
}



#endif"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES


#ifdef VERTEX

varying highp vec2 xlv_TEXCOORD0;
uniform highp mat4 glstate_matrix_mvp;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesVertex;
void main ()
{
  highp vec2 tmpvar_1;
  mediump vec2 tmpvar_2;
  tmpvar_2 = _glesMultiTexCoord0.xy;
  tmpvar_1 = tmpvar_2;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = tmpvar_1;
}



#endif
#ifdef FRAGMENT

#extension GL_EXT_shader_texture_lod : enable
varying highp vec2 xlv_TEXCOORD0;
uniform highp float _SSRRcomposeMode;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _CameraNormalsTexture;
uniform highp mat4 _ViewMatrix;
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ProjMatrix;
uniform highp float _bias;
uniform highp float _stepGlobalScale;
uniform highp float _maxStep;
uniform highp float _maxFineStep;
uniform highp float _maxDepthCull;
uniform highp float _fadePower;
uniform sampler2D _MainTex;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 _ScreenParams;
void main ()
{
  mediump vec4 tmpvar_1;
  highp vec4 odheoldj_2;
  highp vec3 lkjwejhsdkl_3;
  highp vec4 opahwcte_4;
  highp vec4 xbfaeiaej12s_5;
  lowp vec4 tmpvar_6;
  tmpvar_6 = texture2DLodEXT (_MainTex, xlv_TEXCOORD0, 0.0);
  odheoldj_2 = tmpvar_6;
  if ((odheoldj_2.w == 0.0)) {
    xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
  } else {
    highp float tmpskdkx_7;
    lowp float tmpvar_8;
    tmpvar_8 = texture2DLodEXT (_CameraDepthTexture, xlv_TEXCOORD0, 0.0).x;
    tmpskdkx_7 = tmpvar_8;
    highp float tmpvar_9;
    tmpvar_9 = (1.0/(((_ZBufferParams.x * tmpskdkx_7) + _ZBufferParams.y)));
    if ((tmpvar_9 > _maxDepthCull)) {
      xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
    } else {
      highp vec4 acccols_10;
      int s_11;
      highp vec4 uiduefa_12;
      int icoiuf_13;
      bool biifejd_14;
      highp vec4 rensfief_15;
      highp float lenfaiejd_16;
      int vbdueff_17;
      highp vec3 eiieiaced_18;
      highp vec3 jjdafhue_19;
      highp vec3 hgeiald_20;
      highp vec4 loveeaed_21;
      highp vec4 mcjkfeeieijd_22;
      highp vec3 xvzyufalj_23;
      highp vec4 efljafolclsdf_24;
      int tmpvar_25;
      tmpvar_25 = int(_maxStep);
      efljafolclsdf_24.w = 1.0;
      efljafolclsdf_24.xy = ((xlv_TEXCOORD0 * 2.0) - 1.0);
      efljafolclsdf_24.z = tmpskdkx_7;
      highp vec4 tmpvar_26;
      tmpvar_26 = (_ProjectionInv * efljafolclsdf_24);
      highp vec4 tmpvar_27;
      tmpvar_27 = (tmpvar_26 / tmpvar_26.w);
      xvzyufalj_23.xy = efljafolclsdf_24.xy;
      xvzyufalj_23.z = tmpskdkx_7;
      mcjkfeeieijd_22.w = 0.0;
      lowp vec3 tmpvar_28;
      tmpvar_28 = ((texture2DLodEXT (_CameraNormalsTexture, xlv_TEXCOORD0, 0.0).xyz * 2.0) - 1.0);
      mcjkfeeieijd_22.xyz = tmpvar_28;
      highp vec3 tmpvar_29;
      tmpvar_29 = normalize(tmpvar_27.xyz);
      highp vec3 tmpvar_30;
      tmpvar_30 = normalize((_ViewMatrix * mcjkfeeieijd_22).xyz);
      highp vec3 tmpvar_31;
      tmpvar_31 = normalize((tmpvar_29 - (2.0 * (dot (tmpvar_30, tmpvar_29) * tmpvar_30))));
      loveeaed_21.w = 1.0;
      loveeaed_21.xyz = (tmpvar_27.xyz + tmpvar_31);
      highp vec4 tmpvar_32;
      tmpvar_32 = (_ProjMatrix * loveeaed_21);
      highp vec3 tmpvar_33;
      tmpvar_33 = normalize(((tmpvar_32.xyz / tmpvar_32.w) - xvzyufalj_23));
      lkjwejhsdkl_3.z = tmpvar_33.z;
      lkjwejhsdkl_3.xy = (tmpvar_33.xy * 0.5);
      hgeiald_20.xy = xlv_TEXCOORD0;
      hgeiald_20.z = tmpskdkx_7;
      highp float tmpvar_34;
      tmpvar_34 = (2.0 / _ScreenParams.x);
      highp float tmpvar_35;
      tmpvar_35 = sqrt(dot (lkjwejhsdkl_3.xy, lkjwejhsdkl_3.xy));
      highp vec3 tmpvar_36;
      tmpvar_36 = (lkjwejhsdkl_3 * ((tmpvar_34 * _stepGlobalScale) / tmpvar_35));
      jjdafhue_19 = tmpvar_36;
      vbdueff_17 = int(_maxStep);
      lenfaiejd_16 = 0.0;
      biifejd_14 = bool(0);
      eiieiaced_18 = (hgeiald_20 + tmpvar_36);
      icoiuf_13 = 0;
      s_11 = 0;
      for (int s_11 = 0; s_11 < 100; ) {
        if ((icoiuf_13 >= vbdueff_17)) {
          break;
        };
        lowp vec4 tmpvar_37;
        tmpvar_37 = texture2DLodEXT (_CameraDepthTexture, eiieiaced_18.xy, 0.0);
        highp float tmpvar_38;
        tmpvar_38 = (1.0/(((_ZBufferParams.x * tmpvar_37.x) + _ZBufferParams.y)));
        highp float tmpvar_39;
        tmpvar_39 = (1.0/(((_ZBufferParams.x * eiieiaced_18.z) + _ZBufferParams.y)));
        if ((tmpvar_38 < (tmpvar_39 - 1e-06))) {
          uiduefa_12.w = 1.0;
          uiduefa_12.xyz = eiieiaced_18;
          rensfief_15 = uiduefa_12;
          biifejd_14 = bool(1);
          break;
        };
        eiieiaced_18 = (eiieiaced_18 + jjdafhue_19);
        lenfaiejd_16 = (lenfaiejd_16 + 1.0);
        icoiuf_13 = (icoiuf_13 + 1);
        s_11 = (s_11 + 1);
      };
      if ((biifejd_14 == bool(0))) {
        highp vec4 vartfie_40;
        vartfie_40.w = 0.0;
        vartfie_40.xyz = eiieiaced_18;
        rensfief_15 = vartfie_40;
        biifejd_14 = bool(1);
      };
      opahwcte_4 = rensfief_15;
      highp float tmpvar_41;
      tmpvar_41 = abs((rensfief_15.x - 0.5));
      acccols_10 = vec4(0.0, 0.0, 0.0, 0.0);
      if ((_SSRRcomposeMode > 0.0)) {
        highp vec4 tmpvar_42;
        tmpvar_42.w = 0.0;
        tmpvar_42.xyz = odheoldj_2.xyz;
        acccols_10 = tmpvar_42;
      };
      if ((tmpvar_41 > 0.5)) {
        xbfaeiaej12s_5 = acccols_10;
      } else {
        highp float tmpvar_43;
        tmpvar_43 = abs((rensfief_15.y - 0.5));
        if ((tmpvar_43 > 0.5)) {
          xbfaeiaej12s_5 = acccols_10;
        } else {
          if (((1.0/(((_ZBufferParams.x * rensfief_15.z) + _ZBufferParams.y))) > _maxDepthCull)) {
            xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
          } else {
            if ((rensfief_15.z < 0.1)) {
              xbfaeiaej12s_5 = vec4(0.0, 0.0, 0.0, 0.0);
            } else {
              if ((rensfief_15.w == 1.0)) {
                int j_44;
                highp vec4 greyfsd_45;
                highp vec3 poffses_46;
                int i_49_47;
                bool fjekfesa_48;
                highp vec4 alsdmes_49;
                int maxfeis_50;
                highp vec3 refDir_44_51;
                highp vec3 oifejef_52;
                highp vec3 tmpvar_53;
                tmpvar_53 = (rensfief_15.xyz - tmpvar_36);
                highp vec3 tmpvar_54;
                tmpvar_54 = (lkjwejhsdkl_3 * (tmpvar_34 / tmpvar_35));
                refDir_44_51 = tmpvar_54;
                maxfeis_50 = int(_maxFineStep);
                fjekfesa_48 = bool(0);
                poffses_46 = tmpvar_53;
                oifejef_52 = (tmpvar_53 + tmpvar_54);
                i_49_47 = 0;
                j_44 = 0;
                for (int j_44 = 0; j_44 < 20; ) {
                  if ((i_49_47 >= maxfeis_50)) {
                    break;
                  };
                  lowp vec4 tmpvar_55;
                  tmpvar_55 = texture2DLodEXT (_CameraDepthTexture, oifejef_52.xy, 0.0);
                  highp float tmpvar_56;
                  tmpvar_56 = (1.0/(((_ZBufferParams.x * tmpvar_55.x) + _ZBufferParams.y)));
                  highp float tmpvar_57;
                  tmpvar_57 = (1.0/(((_ZBufferParams.x * oifejef_52.z) + _ZBufferParams.y)));
                  if ((tmpvar_56 < tmpvar_57)) {
                    if (((tmpvar_57 - tmpvar_56) < _bias)) {
                      greyfsd_45.w = 1.0;
                      greyfsd_45.xyz = oifejef_52;
                      alsdmes_49 = greyfsd_45;
                      fjekfesa_48 = bool(1);
                      break;
                    };
                    highp vec3 tmpvar_58;
                    tmpvar_58 = (refDir_44_51 * 0.5);
                    refDir_44_51 = tmpvar_58;
                    oifejef_52 = (poffses_46 + tmpvar_58);
                  } else {
                    poffses_46 = oifejef_52;
                    oifejef_52 = (oifejef_52 + refDir_44_51);
                  };
                  i_49_47 = (i_49_47 + 1);
                  j_44 = (j_44 + 1);
                };
                if ((fjekfesa_48 == bool(0))) {
                  highp vec4 tmpvar_55_59;
                  tmpvar_55_59.w = 0.0;
                  tmpvar_55_59.xyz = oifejef_52;
                  alsdmes_49 = tmpvar_55_59;
                  fjekfesa_48 = bool(1);
                };
                opahwcte_4 = alsdmes_49;
              };
              if ((opahwcte_4.w < 0.01)) {
                xbfaeiaej12s_5 = acccols_10;
              } else {
                highp vec4 tmpvar_57_60;
                lowp vec3 tmpvar_61;
                tmpvar_61 = texture2DLodEXT (_MainTex, opahwcte_4.xy, 0.0).xyz;
                tmpvar_57_60.xyz = tmpvar_61;
                tmpvar_57_60.w = (((opahwcte_4.w * (1.0 - (tmpvar_9 / _maxDepthCull))) * (1.0 - pow ((lenfaiejd_16 / float(tmpvar_25)), _fadePower))) * pow (clamp (((dot (normalize(tmpvar_31), normalize(tmpvar_27).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                xbfaeiaej12s_5 = tmpvar_57_60;
              };
            };
          };
        };
      };
    };
  };
  tmpvar_1 = xbfaeiaej12s_5;
  gl_FragData[0] = tmpvar_1;
}



#endif"
}

SubProgram "gles3 " {
Keywords { }
"!!GLES3#version 300 es


#ifdef VERTEX

#define gl_Vertex _glesVertex
in vec4 _glesVertex;
#define gl_MultiTexCoord0 _glesMultiTexCoord0
in vec4 _glesMultiTexCoord0;

#line 151
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 187
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 181
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 330
struct v2f {
    highp vec4 pos;
    highp vec2 uv;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[8];
uniform highp vec4 unity_LightPosition[8];
uniform highp vec4 unity_LightAtten[8];
#line 19
uniform highp vec4 unity_SpotDirection[8];
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
#line 23
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
#line 27
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
#line 31
uniform highp vec4 _LightSplitsNear;
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
#line 35
uniform highp vec4 unity_ShadowFadeCenterAndType;
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
#line 39
uniform highp mat4 _Object2World;
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
#line 43
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
#line 47
uniform highp mat4 glstate_matrix_projection;
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
#line 51
uniform lowp vec4 unity_ColorSpaceGrey;
#line 77
#line 82
#line 87
#line 91
#line 96
#line 120
#line 137
#line 158
#line 166
#line 193
#line 206
#line 215
#line 220
#line 229
#line 234
#line 243
#line 260
#line 265
#line 291
#line 299
#line 307
#line 311
#line 315
uniform sampler2D _depthTexCustom;
uniform sampler2D _MainTex;
uniform highp float _fadePower;
uniform highp float _maxDepthCull;
#line 319
uniform highp float _maxFineStep;
uniform highp float _maxStep;
uniform highp float _stepGlobalScale;
uniform highp float _bias;
#line 323
uniform highp mat4 _ProjMatrix;
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ViewMatrix;
uniform highp vec4 _ProjInfo;
#line 327
uniform sampler2D _CameraNormalsTexture;
uniform sampler2D _CameraDepthTexture;
uniform highp float _SSRRcomposeMode;
#line 336
#line 336
v2f vert( in appdata_img v ) {
    v2f o;
    o.pos = (glstate_matrix_mvp * v.vertex);
    #line 340
    o.uv = v.texcoord.xy;
    return o;
}
out highp vec2 xlv_TEXCOORD0;
void main() {
    v2f xl_retval;
    appdata_img xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xlt_v.texcoord = vec2(gl_MultiTexCoord0);
    xl_retval = vert( xlt_v);
    gl_Position = vec4(xl_retval.pos);
    xlv_TEXCOORD0 = vec2(xl_retval.uv);
}


#endif
#ifdef FRAGMENT

#define gl_FragData _glesFragData
layout(location = 0) out mediump vec4 _glesFragData[4];
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return textureLod( s, coord.xy, coord.w);
}
#line 151
struct v2f_vertex_lit {
    highp vec2 uv;
    lowp vec4 diff;
    lowp vec4 spec;
};
#line 187
struct v2f_img {
    highp vec4 pos;
    mediump vec2 uv;
};
#line 181
struct appdata_img {
    highp vec4 vertex;
    mediump vec2 texcoord;
};
#line 330
struct v2f {
    highp vec4 pos;
    highp vec2 uv;
};
uniform highp vec4 _Time;
uniform highp vec4 _SinTime;
#line 3
uniform highp vec4 _CosTime;
uniform highp vec4 unity_DeltaTime;
uniform highp vec3 _WorldSpaceCameraPos;
uniform highp vec4 _ProjectionParams;
#line 7
uniform highp vec4 _ScreenParams;
uniform highp vec4 _ZBufferParams;
uniform highp vec4 unity_CameraWorldClipPlanes[6];
uniform highp vec4 _WorldSpaceLightPos0;
#line 11
uniform highp vec4 _LightPositionRange;
uniform highp vec4 unity_4LightPosX0;
uniform highp vec4 unity_4LightPosY0;
uniform highp vec4 unity_4LightPosZ0;
#line 15
uniform highp vec4 unity_4LightAtten0;
uniform highp vec4 unity_LightColor[8];
uniform highp vec4 unity_LightPosition[8];
uniform highp vec4 unity_LightAtten[8];
#line 19
uniform highp vec4 unity_SpotDirection[8];
uniform highp vec4 unity_SHAr;
uniform highp vec4 unity_SHAg;
uniform highp vec4 unity_SHAb;
#line 23
uniform highp vec4 unity_SHBr;
uniform highp vec4 unity_SHBg;
uniform highp vec4 unity_SHBb;
uniform highp vec4 unity_SHC;
#line 27
uniform highp vec3 unity_LightColor0;
uniform highp vec3 unity_LightColor1;
uniform highp vec3 unity_LightColor2;
uniform highp vec3 unity_LightColor3;
uniform highp vec4 unity_ShadowSplitSpheres[4];
uniform highp vec4 unity_ShadowSplitSqRadii;
uniform highp vec4 unity_LightShadowBias;
#line 31
uniform highp vec4 _LightSplitsNear;
uniform highp vec4 _LightSplitsFar;
uniform highp mat4 unity_World2Shadow[4];
uniform highp vec4 _LightShadowData;
#line 35
uniform highp vec4 unity_ShadowFadeCenterAndType;
uniform highp mat4 glstate_matrix_mvp;
uniform highp mat4 glstate_matrix_modelview0;
uniform highp mat4 glstate_matrix_invtrans_modelview0;
#line 39
uniform highp mat4 _Object2World;
uniform highp mat4 _World2Object;
uniform highp vec4 unity_Scale;
uniform highp mat4 glstate_matrix_transpose_modelview0;
#line 43
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_texture1;
uniform highp mat4 glstate_matrix_texture2;
uniform highp mat4 glstate_matrix_texture3;
#line 47
uniform highp mat4 glstate_matrix_projection;
uniform highp vec4 glstate_lightmodel_ambient;
uniform highp mat4 unity_MatrixV;
uniform highp mat4 unity_MatrixVP;
#line 51
uniform lowp vec4 unity_ColorSpaceGrey;
#line 77
#line 82
#line 87
#line 91
#line 96
#line 120
#line 137
#line 158
#line 166
#line 193
#line 206
#line 215
#line 220
#line 229
#line 234
#line 243
#line 260
#line 265
#line 291
#line 299
#line 307
#line 311
#line 315
uniform sampler2D _depthTexCustom;
uniform sampler2D _MainTex;
uniform highp float _fadePower;
uniform highp float _maxDepthCull;
#line 319
uniform highp float _maxFineStep;
uniform highp float _maxStep;
uniform highp float _stepGlobalScale;
uniform highp float _bias;
#line 323
uniform highp mat4 _ProjMatrix;
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ViewMatrix;
uniform highp vec4 _ProjInfo;
#line 327
uniform sampler2D _CameraNormalsTexture;
uniform sampler2D _CameraDepthTexture;
uniform highp float _SSRRcomposeMode;
#line 336
#line 343
mediump vec4 frag( in v2f i ) {
    #line 345
    highp vec4 xbfaeiaej12s;
    highp float jaglkje92an;
    highp vec4 opahwcte;
    highp vec3 lkjwejhsdkl;
    #line 349
    highp int mnafeiefj45f;
    highp vec4 odheoldj = xll_tex2Dlod( _MainTex, vec4( i.uv, 0.0, 0.0));
    if ((odheoldj.w == 0.0)){
        #line 353
        xbfaeiaej12s = vec4( 0.0, 0.0, 0.0, 0.0);
    }
    else{
        #line 357
        highp float tmpskdkx = xll_tex2Dlod( _CameraDepthTexture, vec4( i.uv, 0.0, 0.0)).x;
        highp float pootanfflkd = tmpskdkx;
        highp float greysaalwe = (1.0 / ((_ZBufferParams.x * tmpskdkx) + _ZBufferParams.y));
        if ((greysaalwe > _maxDepthCull)){
            #line 362
            xbfaeiaej12s = vec4( 0.0, 0.0, 0.0, 0.0);
        }
        else{
            #line 366
            mnafeiefj45f = int(_maxStep);
            highp vec4 efljafolclsdf;
            efljafolclsdf.w = 1.0;
            efljafolclsdf.xy = ((i.uv * 2.0) - 1.0);
            #line 370
            efljafolclsdf.z = pootanfflkd;
            highp vec4 aflkjeoifa = (_ProjectionInv * efljafolclsdf);
            highp vec4 mvaoieije = (aflkjeoifa / aflkjeoifa.w);
            highp vec3 xvzyufalj;
            #line 374
            xvzyufalj.xy = efljafolclsdf.xy;
            xvzyufalj.z = pootanfflkd;
            highp vec4 mcjkfeeieijd;
            mcjkfeeieijd.w = 0.0;
            #line 378
            mcjkfeeieijd.xyz = ((xll_tex2Dlod( _CameraNormalsTexture, vec4( i.uv, 0.0, 0.0)).xyz * 2.0) - 1.0);
            highp vec3 omfeief = normalize(mvaoieije.xyz);
            highp vec3 kdfefis = normalize((_ViewMatrix * mcjkfeeieijd).xyz);
            highp vec3 jfeiiwsi = normalize((omfeief - (2.0 * (dot( kdfefis, omfeief) * kdfefis))));
            #line 382
            highp vec4 loveeaed;
            loveeaed.w = 1.0;
            loveeaed.xyz = (mvaoieije.xyz + jfeiiwsi);
            highp vec4 qeuaife = (_ProjMatrix * loveeaed);
            #line 386
            highp vec3 justae = normalize(((qeuaife.xyz / qeuaife.w) - xvzyufalj));
            lkjwejhsdkl.z = justae.z;
            lkjwejhsdkl.xy = (justae.xy * 0.5);
            highp vec3 hgeiald;
            #line 390
            hgeiald.xy = i.uv;
            hgeiald.z = pootanfflkd;
            jaglkje92an = 0.0;
            highp float kjafjie = (2.0 / _ScreenParams.x);
            #line 394
            highp float nfeiefie = sqrt(dot( lkjwejhsdkl.xy, lkjwejhsdkl.xy));
            highp vec3 jjdafhue = (lkjwejhsdkl * ((kjafjie * _stepGlobalScale) / nfeiefie));
            highp vec3 eiieiaced;
            highp int vbdueff = int(_maxStep);
            #line 398
            highp float lenfaiejd = jaglkje92an;
            highp vec4 rensfief;
            bool biifejd = false;
            eiieiaced = (hgeiald + jjdafhue);
            #line 402
            highp int icoiuf = 0;
            highp float zzddfeef;
            highp float fejoijfe;
            highp vec4 uiduefa;
            #line 406
            highp int s = 0;
            s = 0;
            for ( ; (s < 100); (s++)) {
                #line 411
                if ((icoiuf >= vbdueff)){
                    break;
                }
                #line 415
                zzddfeef = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _CameraDepthTexture, vec4( eiieiaced.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
                fejoijfe = (1.0 / ((_ZBufferParams.x * eiieiaced.z) + _ZBufferParams.y));
                if ((zzddfeef < (fejoijfe - 1e-06))){
                    #line 419
                    uiduefa.w = 1.0;
                    uiduefa.xyz = eiieiaced;
                    rensfief = uiduefa;
                    biifejd = true;
                    #line 423
                    break;
                }
                eiieiaced = (eiieiaced + jjdafhue);
                lenfaiejd = (lenfaiejd + 1.0);
                #line 427
                icoiuf = (icoiuf + 1);
            }
            if ((biifejd == false)){
                #line 431
                highp vec4 vartfie;
                vartfie.w = 0.0;
                vartfie.xyz = eiieiaced;
                rensfief = vartfie;
                #line 435
                biifejd = true;
            }
            jaglkje92an = lenfaiejd;
            opahwcte = rensfief;
            #line 439
            highp float getthead;
            getthead = abs((rensfief.x - 0.5));
            highp vec4 acccols = vec4( 0.0, 0.0, 0.0, 0.0);
            if ((_SSRRcomposeMode > 0.0)){
                acccols = vec4( odheoldj.xyz, 0.0);
            }
            #line 443
            if ((getthead > 0.5)){
                xbfaeiaej12s = acccols;
            }
            else{
                #line 449
                highp float varefeid = abs((rensfief.y - 0.5));
                if ((varefeid > 0.5)){
                    xbfaeiaej12s = acccols;
                }
                else{
                    #line 456
                    if (((1.0 / ((_ZBufferParams.x * rensfief.z) + _ZBufferParams.y)) > _maxDepthCull)){
                        xbfaeiaej12s = vec4( 0.0, 0.0, 0.0, 0.0);
                    }
                    else{
                        #line 462
                        if ((rensfief.z < 0.1)){
                            xbfaeiaej12s = vec4( 0.0, 0.0, 0.0, 0.0);
                        }
                        else{
                            #line 468
                            if ((rensfief.w == 1.0)){
                                highp vec3 fdfk41fe = (rensfief.xyz - jjdafhue);
                                highp vec3 efijef42s = (lkjwejhsdkl * (kjafjie / nfeiefie));
                                #line 472
                                highp vec3 oifejef;
                                highp vec3 refDir_44;
                                refDir_44 = efijef42s;
                                highp int maxfeis = int(_maxFineStep);
                                #line 476
                                highp vec4 alsdmes;
                                bool fjekfesa = false;
                                highp int i_49;
                                highp vec3 poffses = fdfk41fe;
                                #line 480
                                oifejef = (fdfk41fe + efijef42s);
                                i_49 = 0;
                                highp float jjuuddsfe;
                                highp float taycosl;
                                #line 484
                                highp vec4 greyfsd;
                                highp vec3 blacsjd;
                                highp int j = 0;
                                j = 0;
                                for ( ; (j < 20); (j++)) {
                                    #line 491
                                    if ((i_49 >= maxfeis)){
                                        break;
                                    }
                                    #line 495
                                    jjuuddsfe = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _CameraDepthTexture, vec4( oifejef.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
                                    taycosl = (1.0 / ((_ZBufferParams.x * oifejef.z) + _ZBufferParams.y));
                                    if ((jjuuddsfe < taycosl)){
                                        #line 499
                                        if (((taycosl - jjuuddsfe) < _bias)){
                                            greyfsd.w = 1.0;
                                            greyfsd.xyz = oifejef;
                                            #line 503
                                            alsdmes = greyfsd;
                                            fjekfesa = true;
                                            break;
                                        }
                                        #line 507
                                        blacsjd = (refDir_44 * 0.5);
                                        refDir_44 = blacsjd;
                                        oifejef = (poffses + blacsjd);
                                    }
                                    else{
                                        #line 513
                                        poffses = oifejef;
                                        oifejef = (oifejef + refDir_44);
                                    }
                                    i_49 = (i_49 + 1);
                                }
                                #line 518
                                if ((fjekfesa == false)){
                                    highp vec4 tmpvar_55;
                                    tmpvar_55.w = 0.0;
                                    #line 522
                                    tmpvar_55.xyz = oifejef;
                                    alsdmes = tmpvar_55;
                                    fjekfesa = true;
                                }
                                #line 526
                                opahwcte = alsdmes;
                            }
                            if ((opahwcte.w < 0.01)){
                                #line 530
                                xbfaeiaej12s = acccols;
                            }
                            else{
                                #line 534
                                highp vec4 tmpvar_57;
                                tmpvar_57.xyz = xll_tex2Dlod( _MainTex, vec4( opahwcte.xy, 0.0, 0.0)).xyz;
                                tmpvar_57.w = (((opahwcte.w * (1.0 - (greysaalwe / _maxDepthCull))) * (1.0 - pow( (lenfaiejd / float(mnafeiefj45f)), _fadePower))) * pow( clamp( ((dot( normalize(jfeiiwsi), normalize(mvaoieije).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                                xbfaeiaej12s = tmpvar_57;
                            }
                        }
                    }
                }
            }
        }
    }
    #line 545
    return xbfaeiaej12s;
}
in highp vec2 xlv_TEXCOORD0;
void main() {
    mediump vec4 xl_retval;
    v2f xlt_i;
    xlt_i.pos = vec4(0.0);
    xlt_i.uv = vec2(xlv_TEXCOORD0);
    xl_retval = frag( xlt_i);
    gl_FragData[0] = vec4(xl_retval);
}


#endif"
}

}
Program "fp" {
// Fragment combos: 1
//   d3d9 - ALU: 176 to 176, TEX: 12 to 12, FLOW: 25 to 25
//   d3d11 - ALU: 107 to 107, TEX: 0 to 0, FLOW: 29 to 29
SubProgram "opengl " {
Keywords { }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { }
Vector 12 [_ScreenParams]
Vector 13 [_ZBufferParams]
Float 14 [_fadePower]
Float 15 [_maxDepthCull]
Float 16 [_maxFineStep]
Float 17 [_maxStep]
Float 18 [_stepGlobalScale]
Float 19 [_bias]
Matrix 0 [_ProjMatrix]
Matrix 4 [_ProjectionInv]
Matrix 8 [_ViewMatrix]
Float 20 [_SSRRcomposeMode]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_CameraDepthTexture] 2D
SetTexture 2 [_CameraNormalsTexture] 2D
"ps_3_0
; 176 ALU, 12 TEX, 25 FLOW
dcl_2d s0
dcl_2d s1
dcl_2d s2
def c21, 0.00000000, 2.00000000, -1.00000000, 1.00000000
def c22, 0.50000000, 1.00000000, -0.00000100, -0.50000000
defi i0, 100, 0, 1, 0
def c23, 0.10000000, 0.01000000, 0, 0
defi i1, 20, 0, 1, 0
dcl_texcoord0 v0.xy
mov r0.xy, v0
mov r0.z, c21.x
texldl r3, r0.xyzz, s0
if_eq r3.w, c21.x
mov r0, c21.x
else
mov r0.xy, v0
mov r0.z, c21.x
texldl r0.x, r0.xyzz, s1
mad r0.y, r0.x, c13.x, c13
rcp r4.w, r0.y
if_gt r4.w, c15.x
mov r0, c21.x
else
mad r8.xy, v0, c21.y, c21.z
mov r5.z, r0.x
mov r5.xy, r8
mov r5.w, c21
dp4 r0.y, r5, c7
mov r1.w, r0.y
mov r6.w, c21
dp4 r1.z, r5, c6
dp4 r1.y, r5, c5
dp4 r1.x, r5, c4
rcp r0.y, r0.y
mul r1, r1, r0.y
dp3 r0.z, r1, r1
mov r5.w, c21.x
mov r8.z, r0.x
rsq r0.z, r0.z
mov r5.z, c21.x
mov r5.xy, v0
texldl r5.xyz, r5.xyzz, s2
mad r5.xyz, r5, c21.y, c21.z
dp4 r6.z, r5, c10
dp4 r6.x, r5, c8
dp4 r6.y, r5, c9
dp3 r0.y, r6, r6
rsq r0.y, r0.y
mul r5.xyz, r0.z, r1
mul r6.xyz, r0.y, r6
dp3 r0.y, r6, r5
mul r6.xyz, r0.y, r6
mad r5.xyz, -r6, c21.y, r5
dp3 r0.y, r5, r5
rsq r0.y, r0.y
mul r5.xyz, r0.y, r5
add r6.xyz, r1, r5
dp4 r0.y, r6, c3
dp4 r7.z, r6, c2
dp4 r7.y, r6, c1
dp4 r7.x, r6, c0
rcp r0.y, r0.y
mad r6.xyz, r7, r0.y, -r8
dp3 r0.y, r6, r6
rsq r0.y, r0.y
mul r6.xyz, r0.y, r6
mul r0.zw, r6.xyxy, c22.x
rcp r0.y, c12.x
mul r0.zw, r0, r0
mul r7.w, r0.y, c21.y
add r0.y, r0.z, r0.w
rsq r5.w, r0.y
mul r0.z, r7.w, c18.x
mul r0.y, r5.w, r0.z
abs r0.w, c17.x
frc r3.w, r0
add r0.w, r0, -r3
mul r6.xyz, r6, c22.xxyw
mul r7.xyz, r6, r0.y
mov r0.z, r0.x
mov r0.xy, v0
add r8.xyz, r7, r0
cmp r0.x, c17, r0.w, -r0.w
rcp r8.w, r5.w
mov r5.w, r0.x
mov r9.x, r0
mov r6.w, c21.x
mov_pp r3.w, c21.x
mov r9.y, c21.x
loop aL, i0
break_ge r9.y, r9.x
mad r0.w, r8.z, c13.x, c13.y
mov r0.z, c21.x
mov r0.xy, r8
texldl r0.x, r0.xyzz, s1
rcp r0.y, r0.w
mad r0.x, r0, c13, c13.y
add r9.w, r0.y, c22.z
rcp r9.z, r0.x
add r10.x, r9.z, -r9.w
mov r0.xyz, r8
mov r0.w, c21
cmp r2, r10.x, r2, r0
cmp_pp r3.w, r10.x, r3, c21
break_lt r9.z, r9.w
add r8.xyz, r8, r7
add r6.w, r6, c21
add r9.y, r9, c21.w
endloop
mov r0.xyz, r8
abs_pp r3.w, r3
mov r0.w, c21.x
cmp r0, -r3.w, r0, r2
add r3.w, r0.x, c22
mov r2, r0
mov r0.xyz, r0.xyww
abs r8.x, r3.w
mov r3.w, c21.x
mov r0.w, c21.x
cmp r3, -c20.x, r0.w, r3
if_gt r8.x, c22.x
mov r0, r3
else
add r0.w, r2.y, c22
abs r0.w, r0
if_gt r0.w, c22.x
mov r0, r3
else
mad r0.w, r2.z, c13.x, c13.y
rcp r0.w, r0.w
if_gt r0.w, c15.x
mov r0, c21.x
else
if_lt r2.z, c23.x
mov r0, c21.x
else
if_eq r2.w, c21.w
abs r0.y, c16.x
rcp r0.x, r8.w
mul r0.x, r7.w, r0
mul r6.xyz, r6, r0.x
add r2.xyz, r2, -r7
frc r0.z, r0.y
add r0.x, r0.y, -r0.z
add r7.xyz, r6, r2
cmp r2.w, c16.x, r0.x, -r0.x
mov_pp r0.w, c21.x
mov r7.w, c21.x
loop aL, i1
break_ge r7.w, r2.w
mov r0.z, c21.x
mov r0.xy, r7
texldl r0.x, r0.xyzz, s1
mad r0.y, r7.z, c13.x, c13
mad r0.x, r0, c13, c13.y
rcp r0.y, r0.y
rcp r0.x, r0.x
add r0.z, -r0.x, r0.y
add r0.x, r0, -r0.y
add r0.z, r0, -c19.x
cmp r0.y, r0.z, c21.x, c21.w
cmp r8.w, r0.x, c21.x, c21
mul_pp r8.x, r8.w, r0.y
mov r0.xy, r7
mov r0.z, c21.w
cmp r4.xyz, -r8.x, r4, r0
cmp_pp r0.w, -r8.x, r0, c21
break_gt r8.x, c21.x
mul r0.xyz, r6, c22.x
add r8.xyz, r0, r2
cmp r8.xyz, -r8.w, r7, r8
cmp r6.xyz, -r8.w, r6, r0
abs_pp r8.w, r8
add r0.xyz, r6, r8
cmp r7.xyz, -r8.w, r0, r8
cmp r2.xyz, -r8.w, r8, r2
add r7.w, r7, c21
endloop
mov r0.xy, r7
mov r0.z, c21.x
abs_pp r0.w, r0
cmp r0.xyz, -r0.w, r0, r4
endif
if_lt r0.z, c23.y
mov r0, r3
else
dp4 r1.w, r1, r1
rsq r1.w, r1.w
dp3 r0.w, r5, r5
mul r2.xyz, r1.w, r1
rsq r0.w, r0.w
mul r1.xyz, r0.w, r5
dp3 r1.x, r1, r2
mov r0.w, c14.x
mad r0.w, c23.x, r0, r1.x
add_sat r2.x, r0.w, c21.w
pow r1, r2.x, c14.x
rcp r0.w, r5.w
mul r0.w, r6, r0
pow r2, r0.w, c14.x
rcp r0.w, c15.x
mad r0.w, -r4, r0, c21
mul r0.z, r0, r0.w
mov r1.y, r1.x
mov r1.x, r2
add r1.x, -r1, c21.w
mul r0.z, r0, r1.x
mul r0.w, r0.z, r1.y
mov r0.z, c21.x
texldl r0.xyz, r0.xyzz, s0
endif
endif
endif
endif
endif
endif
endif
mov_pp oC0, r0
"
}

SubProgram "d3d11 " {
Keywords { }
ConstBuffer "$Globals" 272 // 260 used size, 12 vars
Float 16 [_fadePower]
Float 20 [_maxDepthCull]
Float 24 [_maxFineStep]
Float 28 [_maxStep]
Float 32 [_stepGlobalScale]
Float 36 [_bias]
Matrix 48 [_ProjMatrix] 4
Matrix 112 [_ProjectionInv] 4
Matrix 176 [_ViewMatrix] 4
Float 256 [_SSRRcomposeMode]
ConstBuffer "UnityPerCamera" 128 // 128 used size, 8 vars
Vector 96 [_ScreenParams] 4
Vector 112 [_ZBufferParams] 4
BindCB "$Globals" 0
BindCB "UnityPerCamera" 1
SetTexture 0 [_MainTex] 2D 0
SetTexture 1 [_CameraDepthTexture] 2D 2
SetTexture 2 [_CameraNormalsTexture] 2D 1
// 201 instructions, 13 temp regs, 0 temp arrays:
// ALU 98 float, 8 int, 1 uint
// TEX 0 (6 load, 0 comp, 0 bias, 0 grad)
// FLOW 14 static, 15 dynamic
"ps_4_0
eefiecednfeagaegmgcehdmbihgepeakhgepapinabaaaaaapibeaaaaadaaaaaa
cmaaaaaaieaaaaaaliaaaaaaejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcdibeaaaa
eaaaaaaaaoafaaaafjaaaaaeegiocaaaaaaaaaaabbaaaaaafjaaaaaeegiocaaa
abaaaaaaaiaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaae
aahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaagcbaaaad
dcbabaaaabaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacanaaaaaaeiaaaaal
pcaabaaaaaaaaaaaegbabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaa
abeaaaaaaaaaaaaabiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaaaaabpaaaeaddkaabaaaaaaaaaaadgaaaaaipccabaaaaaaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabcaaaaabeiaaaaalpcaabaaaabaaaaaa
egbabaaaabaaaaaajghmbaaaabaaaaaaaagabaaaacaaaaaaabeaaaaaaaaaaaaa
dcaaaaalicaabaaaaaaaaaaaakiacaaaabaaaaaaahaaaaaackaabaaaabaaaaaa
bkiacaaaabaaaaaaahaaaaaaaoaaaaakicaabaaaaaaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaadbaaaaaiicaabaaaabaaaaaa
bkiacaaaaaaaaaaaabaaaaaadkaabaaaaaaaaaaabpaaaeaddkaabaaaabaaaaaa
dgaaaaaipccabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
bcaaaaabdcaaaaapdcaabaaaacaaaaaaegbabaaaabaaaaaaaceaaaaaaaaaaaea
aaaaaaeaaaaaaaaaaaaaaaaaaceaaaaaaaaaialpaaaaialpaaaaaaaaaaaaaaaa
diaaaaaipcaabaaaadaaaaaafgafbaaaacaaaaaaegiocaaaaaaaaaaaaiaaaaaa
dcaaaaakpcaabaaaacaaaaaaegiocaaaaaaaaaaaahaaaaaaagaabaaaacaaaaaa
egaobaaaadaaaaaadcaaaaakpcaabaaaacaaaaaaegiocaaaaaaaaaaaajaaaaaa
kgakbaaaabaaaaaaegaobaaaacaaaaaaaaaaaaaipcaabaaaacaaaaaaegaobaaa
acaaaaaaegiocaaaaaaaaaaaakaaaaaaaoaaaaahpcaabaaaacaaaaaaegaobaaa
acaaaaaapgapbaaaacaaaaaaeiaaaaalpcaabaaaadaaaaaaegbabaaaabaaaaaa
eghobaaaacaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaadcaaaaaphcaabaaa
adaaaaaaegacbaaaadaaaaaaaceaaaaaaaaaaaeaaaaaaaeaaaaaaaeaaaaaaaaa
aceaaaaaaaaaialpaaaaialpaaaaialpaaaaaaaabaaaaaahicaabaaaabaaaaaa
egacbaaaacaaaaaaegacbaaaacaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaadiaaaaahhcaabaaaaeaaaaaapgapbaaaabaaaaaaegacbaaaacaaaaaa
diaaaaaihcaabaaaafaaaaaafgafbaaaadaaaaaaegiccaaaaaaaaaaaamaaaaaa
dcaaaaaklcaabaaaadaaaaaaegiicaaaaaaaaaaaalaaaaaaagaabaaaadaaaaaa
egaibaaaafaaaaaadcaaaaakhcaabaaaadaaaaaaegiccaaaaaaaaaaaanaaaaaa
kgakbaaaadaaaaaaegadbaaaadaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaa
adaaaaaaegacbaaaadaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaa
diaaaaahhcaabaaaadaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaabaaaaaah
icaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaaeaaaaaadiaaaaahhcaabaaa
adaaaaaaegacbaaaadaaaaaapgapbaaaabaaaaaadcaaaaanhcaabaaaadaaaaaa
egacbaiaebaaaaaaadaaaaaaaceaaaaaaaaaaaeaaaaaaaeaaaaaaaeaaaaaaaaa
egacbaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaa
adaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaa
aeaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaaadaaaaaa
egacbaaaadaaaaaapgapbaaaabaaaaaaegacbaaaacaaaaaadiaaaaaipcaabaaa
afaaaaaafgafbaaaadaaaaaaegiocaaaaaaaaaaaaeaaaaaadcaaaaakpcaabaaa
afaaaaaaegiocaaaaaaaaaaaadaaaaaaagaabaaaadaaaaaaegaobaaaafaaaaaa
dcaaaaakpcaabaaaadaaaaaaegiocaaaaaaaaaaaafaaaaaakgakbaaaadaaaaaa
egaobaaaafaaaaaaaaaaaaaipcaabaaaadaaaaaaegaobaaaadaaaaaaegiocaaa
aaaaaaaaagaaaaaaaoaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaapgapbaaa
adaaaaaadcaaaaapdcaabaaaabaaaaaaegbabaaaabaaaaaaaceaaaaaaaaaaaea
aaaaaaeaaaaaaaaaaaaaaaaaaceaaaaaaaaaialpaaaaialpaaaaaaaaaaaaaaaa
aaaaaaaihcaabaaaadaaaaaaegacbaiaebaaaaaaabaaaaaaegacbaaaadaaaaaa
baaaaaahicaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaadaaaaaaeeaaaaaf
icaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaapgapbaaa
abaaaaaaegacbaaaadaaaaaadiaaaaakdcaabaaaafaaaaaaegaabaaaadaaaaaa
aceaaaaaaaaaaadpaaaaaadpaaaaaaaaaaaaaaaaaoaaaaaiicaabaaaabaaaaaa
abeaaaaaaaaaaaeaakiacaaaabaaaaaaagaaaaaaapaaaaahicaabaaaadaaaaaa
egaabaaaafaaaaaaegaabaaaafaaaaaaelaaaaaficaabaaaadaaaaaadkaabaaa
adaaaaaadiaaaaaiicaabaaaaeaaaaaadkaabaaaabaaaaaaakiacaaaaaaaaaaa
acaaaaaaaoaaaaahicaabaaaaeaaaaaadkaabaaaaeaaaaaadkaabaaaadaaaaaa
diaaaaakhcaabaaaadaaaaaaegacbaaaadaaaaaaaceaaaaaaaaaaadpaaaaaadp
aaaaiadpaaaaaaaablaaaaagbcaabaaaafaaaaaadkiacaaaaaaaaaaaabaaaaaa
dgaaaaafdcaabaaaabaaaaaaegbabaaaabaaaaaadcaaaaajhcaabaaaabaaaaaa
egacbaaaadaaaaaapgapbaaaaeaaaaaaegacbaaaabaaaaaadgaaaaaficaabaaa
agaaaaaaabeaaaaaaaaaiadpdgaaaaaipcaabaaaahaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaadgaaaaafhcaabaaaagaaaaaaegacbaaaabaaaaaa
dgaaaaaiocaabaaaafaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
dgaaaaafbcaabaaaaiaaaaaaabeaaaaaaaaaaaaadaaaaaabcbaaaaahccaabaaa
aiaaaaaaakaabaaaaiaaaaaaabeaaaaageaaaaaaadaaaeadbkaabaaaaiaaaaaa
cbaaaaahccaabaaaaiaaaaaadkaabaaaafaaaaaaakaabaaaafaaaaaabpaaaead
bkaabaaaaiaaaaaaacaaaaabbfaaaaabeiaaaaalpcaabaaaajaaaaaaegaabaaa
agaaaaaaeghobaaaabaaaaaaaagabaaaacaaaaaaabeaaaaaaaaaaaaadcaaaaal
ccaabaaaaiaaaaaaakiacaaaabaaaaaaahaaaaaaakaabaaaajaaaaaabkiacaaa
abaaaaaaahaaaaaaaoaaaaakccaabaaaaiaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpbkaabaaaaiaaaaaadcaaaaalecaabaaaaiaaaaaaakiacaaa
abaaaaaaahaaaaaackaabaaaagaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaak
ecaabaaaaiaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpckaabaaa
aiaaaaaaaaaaaaahecaabaaaaiaaaaaackaabaaaaiaaaaaaabeaaaaalndhiglf
dbaaaaahccaabaaaaiaaaaaabkaabaaaaiaaaaaackaabaaaaiaaaaaabpaaaead
bkaabaaaaiaaaaaadgaaaaafpcaabaaaahaaaaaaegaobaaaagaaaaaadgaaaaaf
ecaabaaaafaaaaaaabeaaaaappppppppacaaaaabbfaaaaabdcaaaaajhcaabaaa
agaaaaaaegacbaaaadaaaaaapgapbaaaaeaaaaaaegacbaaaagaaaaaaaaaaaaah
ccaabaaaafaaaaaabkaabaaaafaaaaaaabeaaaaaaaaaiadpboaaaaahicaabaaa
afaaaaaadkaabaaaafaaaaaaabeaaaaaabaaaaaaboaaaaahbcaabaaaaiaaaaaa
akaabaaaaiaaaaaaabeaaaaaabaaaaaadgaaaaaipcaabaaaahaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadgaaaaafecaabaaaafaaaaaaabeaaaaa
aaaaaaaabgaaaaabdgaaaaaficaabaaaagaaaaaaabeaaaaaaaaaaaaadhaaaaaj
pcaabaaaagaaaaaakgakbaaaafaaaaaaegaobaaaahaaaaaaegaobaaaagaaaaaa
aaaaaaahbcaabaaaabaaaaaaakaabaaaagaaaaaaabeaaaaaaaaaaalpdbaaaaai
ccaabaaaabaaaaaaabeaaaaaaaaaaaaaakiacaaaaaaaaaaabaaaaaaaabaaaaah
hcaabaaaahaaaaaaegacbaaaaaaaaaaafgafbaaaabaaaaaadgaaaaaficaabaaa
ahaaaaaaabeaaaaaaaaaaaaadbaaaaaibcaabaaaaaaaaaaaabeaaaaaaaaaaadp
akaabaiaibaaaaaaabaaaaaabpaaaeadakaabaaaaaaaaaaadgaaaaafpccabaaa
aaaaaaaaegaobaaaahaaaaaabcaaaaabaaaaaaahbcaabaaaaaaaaaaabkaabaaa
agaaaaaaabeaaaaaaaaaaalpdbaaaaaibcaabaaaaaaaaaaaabeaaaaaaaaaaadp
akaabaiaibaaaaaaaaaaaaaabpaaaeadakaabaaaaaaaaaaadgaaaaafpccabaaa
aaaaaaaaegaobaaaahaaaaaabcaaaaabdcaaaaalbcaabaaaaaaaaaaaakiacaaa
abaaaaaaahaaaaaackaabaaaagaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaak
bcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpakaabaaa
aaaaaaaadbaaaaaibcaabaaaaaaaaaaabkiacaaaaaaaaaaaabaaaaaaakaabaaa
aaaaaaaabpaaaeadakaabaaaaaaaaaaadgaaaaaipccabaaaaaaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabcaaaaabdbaaaaahbcaabaaaaaaaaaaa
ckaabaaaagaaaaaaabeaaaaamnmmmmdnbpaaaeadakaabaaaaaaaaaaadgaaaaai
pccabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabcaaaaab
biaaaaahbcaabaaaaaaaaaaadkaabaaaagaaaaaaabeaaaaaaaaaiadpbpaaaead
akaabaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaaegacbaiaebaaaaaaadaaaaaa
pgapbaaaaeaaaaaaegacbaaaagaaaaaaaoaaaaahbcaabaaaabaaaaaadkaabaaa
abaaaaaadkaabaaaadaaaaaadiaaaaahocaabaaaabaaaaaaagaabaaaabaaaaaa
agajbaaaadaaaaaablaaaaagicaabaaaadaaaaaackiacaaaaaaaaaaaabaaaaaa
dcaaaaajhcaabaaaadaaaaaaegacbaaaadaaaaaaagaabaaaabaaaaaaegacbaaa
aaaaaaaadgaaaaafecaabaaaaiaaaaaaabeaaaaaaaaaiadpdgaaaaafncaabaaa
afaaaaaafgaobaaaabaaaaaadgaaaaaihcaabaaaajaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaadgaaaaafhcaabaaaakaaaaaaegacbaaaaaaaaaaa
dgaaaaafhcaabaaaalaaaaaaegacbaaaadaaaaaadgaaaaafbcaabaaaabaaaaaa
abeaaaaaaaaaaaaadgaaaaaficaabaaaaeaaaaaaabeaaaaaaaaaaaaadgaaaaaf
icaabaaaagaaaaaaabeaaaaaaaaaaaaadaaaaaabcbaaaaahicaabaaaaiaaaaaa
dkaabaaaagaaaaaaabeaaaaabeaaaaaaadaaaeaddkaabaaaaiaaaaaacbaaaaah
icaabaaaaiaaaaaadkaabaaaaeaaaaaadkaabaaaadaaaaaabpaaaeaddkaabaaa
aiaaaaaaacaaaaabbfaaaaabeiaaaaalpcaabaaaamaaaaaaegaabaaaalaaaaaa
eghobaaaabaaaaaaaagabaaaacaaaaaaabeaaaaaaaaaaaaadcaaaaalicaabaaa
aiaaaaaaakiacaaaabaaaaaaahaaaaaaakaabaaaamaaaaaabkiacaaaabaaaaaa
ahaaaaaaaoaaaaakicaabaaaaiaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadp
aaaaiadpdkaabaaaaiaaaaaadcaaaaalicaabaaaajaaaaaaakiacaaaabaaaaaa
ahaaaaaackaabaaaalaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaakicaabaaa
ajaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaajaaaaaa
dbaaaaahicaabaaaakaaaaaadkaabaaaaiaaaaaadkaabaaaajaaaaaabpaaaead
dkaabaaaakaaaaaaaaaaaaaiicaabaaaaiaaaaaadkaabaiaebaaaaaaaiaaaaaa
dkaabaaaajaaaaaadbaaaaaiicaabaaaaiaaaaaadkaabaaaaiaaaaaabkiacaaa
aaaaaaaaacaaaaaabpaaaeaddkaabaaaaiaaaaaadgaaaaafdcaabaaaaiaaaaaa
egaabaaaalaaaaaadgaaaaafhcaabaaaajaaaaaaegacbaaaaiaaaaaadgaaaaaf
bcaabaaaabaaaaaaabeaaaaappppppppacaaaaabbfaaaaabdiaaaaaklcaabaaa
aiaaaaaaigambaaaafaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaaaaaaaaaadp
dcaaaaamhcaabaaaalaaaaaaigadbaaaafaaaaaaaceaaaaaaaaaaadpaaaaaadp
aaaaaadpaaaaaaaaegacbaaaakaaaaaadgaaaaafncaabaaaafaaaaaaaganbaaa
aiaaaaaabcaaaaabaaaaaaahlcaabaaaaiaaaaaaigambaaaafaaaaaaegaibaaa
alaaaaaadgaaaaafhcaabaaaakaaaaaaegacbaaaalaaaaaadgaaaaafhcaabaaa
alaaaaaaegadbaaaaiaaaaaabfaaaaabboaaaaahicaabaaaaeaaaaaadkaabaaa
aeaaaaaaabeaaaaaabaaaaaaboaaaaahicaabaaaagaaaaaadkaabaaaagaaaaaa
abeaaaaaabaaaaaadgaaaaaihcaabaaaajaaaaaaaceaaaaaaaaaaaaaaaaaaaaa
aaaaaaaaaaaaaaaadgaaaaafbcaabaaaabaaaaaaabeaaaaaaaaaaaaabgaaaaab
dgaaaaafecaabaaaalaaaaaaabeaaaaaaaaaaaaadhaaaaajhcaabaaaagaaaaaa
agaabaaaabaaaaaaegacbaaaajaaaaaaegacbaaaalaaaaaabcaaaaabdgaaaaaf
ecaabaaaagaaaaaaabeaaaaaaaaaaaaabfaaaaabdbaaaaahbcaabaaaaaaaaaaa
ckaabaaaagaaaaaaabeaaaaaaknhcddmbpaaaeadakaabaaaaaaaaaaadgaaaaaf
pccabaaaaaaaaaaaegaobaaaahaaaaaabcaaaaabeiaaaaalpcaabaaaabaaaaaa
egaabaaaagaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaaabeaaaaaaaaaaaaa
aoaaaaaibcaabaaaaaaaaaaadkaabaaaaaaaaaaabkiacaaaaaaaaaaaabaaaaaa
aaaaaaaibcaabaaaaaaaaaaaakaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadp
diaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaackaabaaaagaaaaaaedaaaaag
ccaabaaaaaaaaaaadkiacaaaaaaaaaaaabaaaaaaaoaaaaahccaabaaaaaaaaaaa
bkaabaaaafaaaaaabkaabaaaaaaaaaaacpaaaaafccaabaaaaaaaaaaabkaabaaa
aaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaaakiacaaaaaaaaaaa
abaaaaaabjaaaaafccaabaaaaaaaaaaabkaabaaaaaaaaaaaaaaaaaaiccaabaaa
aaaaaaaabkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaahbcaabaaa
aaaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaabbaaaaahccaabaaaaaaaaaaa
egaobaaaacaaaaaaegaobaaaacaaaaaaeeaaaaafccaabaaaaaaaaaaabkaabaaa
aaaaaaaadiaaaaahocaabaaaaaaaaaaafgafbaaaaaaaaaaaagajbaaaacaaaaaa
baaaaaahccaabaaaaaaaaaaaegacbaaaaeaaaaaajgahbaaaaaaaaaaaaaaaaaah
ccaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaiadpdccaaaakccaabaaa
aaaaaaaaakiacaaaaaaaaaaaabaaaaaaabeaaaaamnmmmmdnbkaabaaaaaaaaaaa
cpaaaaafccaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaa
bkaabaaaaaaaaaaaakiacaaaaaaaaaaaabaaaaaabjaaaaafccaabaaaaaaaaaaa
bkaabaaaaaaaaaaadiaaaaahiccabaaaaaaaaaaabkaabaaaaaaaaaaaakaabaaa
aaaaaaaadgaaaaafhccabaaaaaaaaaaaegacbaaaabaaaaaabfaaaaabbfaaaaab
bfaaaaabbfaaaaabbfaaaaabbfaaaaabbfaaaaabdoaaaaab"
}

SubProgram "gles " {
Keywords { }
"!!GLES"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES"
}

SubProgram "gles3 " {
Keywords { }
"!!GLES3"
}

}

#LINE 595

	}
	
	//================================================================================================================================================
	//END OF PASS 2
	//================================================================================================================================================
}

Fallback off

}