Shader "Hidden/CandelaSSRRv1_POM" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

//================================================================================================================================================
//USING UNITY DEPTH TEXTURE - BEGIN OF PASS 2
//================================================================================================================================================
	
SubShader {
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }
	Pass {

Program "vp" {
// Vertex combos: 1
//   d3d9 - ALU: 8 to 8
//   d3d11 - ALU: 6 to 6, TEX: 0 to 0, FLOW: 1 to 1
SubProgram "opengl " {
Keywords { }
"!!GLSL
#ifdef VERTEX
varying vec2 xlv_TEXCOORD0;


void main ()
{
  vec2 tmpvar_1;
  tmpvar_1 = gl_MultiTexCoord0.xy;
  vec4 tmpvar_2;
  tmpvar_2.zw = vec2(0.0, 0.0);
  tmpvar_2.x = tmpvar_1.x;
  tmpvar_2.y = tmpvar_1.y;
  gl_Position = (gl_ModelViewProjectionMatrix * gl_Vertex);
  xlv_TEXCOORD0 = (gl_TextureMatrix[0] * tmpvar_2).xy;
}


#endif
#ifdef FRAGMENT
#ifndef SHADER_API_OPENGL
    #define SHADER_API_OPENGL 1
#endif
#ifndef SHADER_API_DESKTOP
    #define SHADER_API_DESKTOP 1
#endif
#extension GL_ARB_shader_texture_lod : require
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLod( s, coord.xy, coord.w);
}
#line 151
struct v2f_vertex_lit {
    vec2 uv;
    vec4 diff;
    vec4 spec;
};
#line 187
struct v2f_img {
    vec4 pos;
    vec2 uv;
};
#line 181
struct appdata_img {
    vec4 vertex;
    vec2 texcoord;
};
#line 328
struct PS_IN {
    vec4 pos;
    vec2 uv;
};
uniform vec4 _Time;
uniform vec4 _SinTime;
#line 3
uniform vec4 _CosTime;
uniform vec4 unity_DeltaTime;
uniform vec3 _WorldSpaceCameraPos;
uniform vec4 _ProjectionParams;
#line 7
uniform vec4 _ScreenParams;
uniform vec4 _ZBufferParams;
uniform vec4 unity_CameraWorldClipPlanes[6];
uniform vec4 _WorldSpaceLightPos0;
#line 11
uniform vec4 _LightPositionRange;
uniform vec4 unity_4LightPosX0;
uniform vec4 unity_4LightPosY0;
uniform vec4 unity_4LightPosZ0;
#line 15
uniform vec4 unity_4LightAtten0;
uniform vec4 unity_LightColor[8];
uniform vec4 unity_LightPosition[8];
uniform vec4 unity_LightAtten[8];
#line 19
uniform vec4 unity_SpotDirection[8];
uniform vec4 unity_SHAr;
uniform vec4 unity_SHAg;
uniform vec4 unity_SHAb;
#line 23
uniform vec4 unity_SHBr;
uniform vec4 unity_SHBg;
uniform vec4 unity_SHBb;
uniform vec4 unity_SHC;
#line 27
uniform vec3 unity_LightColor0;
uniform vec3 unity_LightColor1;
uniform vec3 unity_LightColor2;
uniform vec3 unity_LightColor3;
uniform vec4 unity_ShadowSplitSpheres[4];
uniform vec4 unity_ShadowSplitSqRadii;
uniform vec4 unity_LightShadowBias;
#line 31
uniform vec4 _LightSplitsNear;
uniform vec4 _LightSplitsFar;
uniform mat4 unity_World2Shadow[4];
uniform vec4 _LightShadowData;
#line 35
uniform vec4 unity_ShadowFadeCenterAndType;



#line 39
uniform mat4 _Object2World;
uniform mat4 _World2Object;
uniform vec4 unity_Scale;
uniform mat4 glstate_matrix_transpose_modelview0;
#line 43




#line 47


uniform mat4 unity_MatrixV;
uniform mat4 unity_MatrixVP;
#line 51
uniform vec4 unity_ColorSpaceGrey;
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
uniform float _SSRRcomposeMode;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _CameraNormalsTexture;
uniform mat4 _ViewMatrix;
#line 319
uniform mat4 _ProjectionInv;
uniform mat4 _ProjMatrix;
uniform float _bias;
uniform float _stepGlobalScale;
#line 323
uniform float _maxStep;
uniform float _maxFineStep;
uniform float _maxDepthCull;
uniform float _fadePower;
#line 327
uniform sampler2D _MainTex;
#line 334
#line 341
vec4 PS( in PS_IN _In ) {
    #line 343
    vec3 lkjwejhsdkl_1;
    vec4 opahwcte_2;
    vec4 Result = vec4( 0.0);
    vec4 SourceColor = xll_tex2Dlod( _MainTex, vec4( _In.uv, 0.0, 0.0));
    #line 347
    if ((SourceColor.w == 0.0)){
        Result = vec4( 0.0, 0.0, 0.0, 0.0);
        return Result;
    }
    #line 352
    vec4 tmpvar_5;
    tmpvar_5 = xll_tex2Dlod( _CameraDepthTexture, vec4( _In.uv, 0.0, 0.0));
    float tmpvar_6;
    tmpvar_6 = tmpvar_5.x;
    #line 356
    float tmpvar_7;
    tmpvar_7 = (1.0 / ((_ZBufferParams.x * tmpvar_5.x) + _ZBufferParams.y));
    if ((tmpvar_7 > _maxDepthCull)){
        #line 360
        Result = vec4( 0.0, 0.0, 0.0, 0.0);
    }
    else{
        #line 364
        vec4 acccols_8;
        int s_9;
        vec4 uiduefa_10;
        int icoiuf_11;
        #line 368
        bool biifejd_12;
        vec4 rensfief_13;
        float lenfaiejd_14;
        int vbdueff_15;
        #line 372
        vec3 eiieiaced_16;
        vec3 jjdafhue_17;
        vec3 hgeiald_18;
        vec4 loveeaed_19;
        #line 376
        vec4 mcjkfeeieijd_20;
        vec3 xvzyufalj_21;
        vec4 efljafolclsdf_22;
        int tmpvar_23;
        #line 380
        tmpvar_23 = int(_maxStep);
        efljafolclsdf_22.w = 1.0;
        efljafolclsdf_22.xy = ((_In.uv * 2.0) - 1.0);
        efljafolclsdf_22.z = tmpvar_6;
        #line 384
        vec4 tmpvar_24;
        tmpvar_24 = (_ProjectionInv * efljafolclsdf_22);
        vec4 tmpvar_25;
        tmpvar_25 = (tmpvar_24 / tmpvar_24.w);
        #line 388
        xvzyufalj_21.xy = efljafolclsdf_22.xy;
        xvzyufalj_21.z = tmpvar_6;
        mcjkfeeieijd_20.w = 0.0;
        mcjkfeeieijd_20.xyz = ((xll_tex2Dlod( _CameraNormalsTexture, vec4( _In.uv, 0.0, 0.0)).xyz * 2.0) - 1.0);
        #line 392
        vec3 tmpvar_26;
        tmpvar_26 = normalize(tmpvar_25.xyz);
        vec3 tmpvar_27;
        tmpvar_27 = normalize((_ViewMatrix * mcjkfeeieijd_20).xyz);
        #line 396
        vec3 tmpvar_28;
        tmpvar_28 = normalize((tmpvar_26 - (2.0 * (dot( tmpvar_27, tmpvar_26) * tmpvar_27))));
        loveeaed_19.w = 1.0;
        loveeaed_19.xyz = (tmpvar_25.xyz + tmpvar_28);
        #line 400
        vec4 tmpvar_29;
        tmpvar_29 = (_ProjMatrix * loveeaed_19);
        vec3 tmpvar_30;
        tmpvar_30 = normalize(((tmpvar_29.xyz / tmpvar_29.w) - xvzyufalj_21));
        #line 404
        lkjwejhsdkl_1.z = tmpvar_30.z;
        lkjwejhsdkl_1.xy = (tmpvar_30.xy * 0.5);
        hgeiald_18.xy = _In.uv;
        hgeiald_18.z = tmpvar_6;
        #line 408
        float tmpvar_31;
        tmpvar_31 = (2.0 / _ScreenParams.x);
        float tmpvar_32;
        tmpvar_32 = sqrt(dot( lkjwejhsdkl_1.xy, lkjwejhsdkl_1.xy));
        #line 412
        vec3 tmpvar_33;
        tmpvar_33 = (lkjwejhsdkl_1 * ((tmpvar_31 * _stepGlobalScale) / tmpvar_32));
        jjdafhue_17 = tmpvar_33;
        vbdueff_15 = int(_maxStep);
        #line 416
        lenfaiejd_14 = 0.0;
        biifejd_12 = false;
        eiieiaced_16 = (hgeiald_18 + tmpvar_33);
        icoiuf_11 = 0;
        #line 420
        s_9 = 0;
        int s_9_1 = 0;
        for ( ; (s_9_1 < 100); ) {
            #line 425
            if ((icoiuf_11 >= vbdueff_15)){
                break;
            }
            #line 430
            float tmpvar_34;
            tmpvar_34 = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _CameraDepthTexture, vec4( eiieiaced_16.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
            float tmpvar_35;
            tmpvar_35 = (1.0 / ((_ZBufferParams.x * eiieiaced_16.z) + _ZBufferParams.y));
            #line 434
            if ((tmpvar_34 < (tmpvar_35 - 1e-06))){
                uiduefa_10.w = 1.0;
                uiduefa_10.xyz = eiieiaced_16;
                #line 438
                rensfief_13 = uiduefa_10;
                biifejd_12 = true;
                break;
            }
            #line 443
            eiieiaced_16 = (eiieiaced_16 + jjdafhue_17);
            lenfaiejd_14 = (lenfaiejd_14 + 1.0);
            icoiuf_11 = (icoiuf_11 + 1);
            s_9_1 = (s_9_1 + 1);
        }
        #line 449
        if ((biifejd_12 == false)){
            vec4 vartfie_36;
            vartfie_36.w = 0.0;
            #line 453
            vartfie_36.xyz = eiieiaced_16;
            rensfief_13 = vartfie_36;
            biifejd_12 = true;
        }
        #line 458
        opahwcte_2 = rensfief_13;
        float tmpvar_37;
        tmpvar_37 = abs((rensfief_13.x - 0.5));
        acccols_8 = vec4( 0.0, 0.0, 0.0, 0.0);
        #line 462
        if ((_SSRRcomposeMode > 0.0)){
            vec4 tmpvar_38;
            tmpvar_38.w = 0.0;
            #line 466
            tmpvar_38.xyz = SourceColor.xyz;
            acccols_8 = tmpvar_38;
        }
        #line 470
        if ((tmpvar_37 > 0.5)){
            Result = acccols_8;
        }
        else{
            #line 476
            float tmpvar_39;
            tmpvar_39 = abs((rensfief_13.y - 0.5));
            if ((tmpvar_39 > 0.5)){
                #line 480
                Result = acccols_8;
            }
            else{
                #line 484
                if (((1.0 / ((_ZBufferParams.x * rensfief_13.z) + _ZBufferParams.y)) > _maxDepthCull)){
                    Result = vec4( 0.0, 0.0, 0.0, 0.0);
                }
                else{
                    #line 490
                    if ((rensfief_13.z < 0.1)){
                        Result = vec4( 0.0, 0.0, 0.0, 0.0);
                    }
                    else{
                        #line 496
                        if ((rensfief_13.w == 1.0)){
                            int j_40;
                            vec4 greyfsd_41;
                            #line 500
                            vec3 poffses_42;
                            int i_49_43;
                            bool fjekfesa_44;
                            vec4 alsdmes_45;
                            #line 504
                            int maxfeis_46;
                            vec3 refDir_44_47;
                            vec3 oifejef_48;
                            vec3 tmpvar_49;
                            #line 508
                            tmpvar_49 = (rensfief_13.xyz - tmpvar_33);
                            vec3 tmpvar_50;
                            tmpvar_50 = (lkjwejhsdkl_1 * (tmpvar_31 / tmpvar_32));
                            refDir_44_47 = tmpvar_50;
                            #line 512
                            maxfeis_46 = int(_maxFineStep);
                            fjekfesa_44 = false;
                            poffses_42 = tmpvar_49;
                            oifejef_48 = (tmpvar_49 + tmpvar_50);
                            #line 516
                            i_49_43 = 0;
                            j_40 = 0;
                            int j_40_1 = 0;
                            for ( ; (j_40_1 < 20); ) {
                                #line 522
                                if ((i_49_43 >= maxfeis_46)){
                                    break;
                                }
                                #line 527
                                float tmpvar_51;
                                tmpvar_51 = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _CameraDepthTexture, vec4( oifejef_48.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
                                float tmpvar_52;
                                tmpvar_52 = (1.0 / ((_ZBufferParams.x * oifejef_48.z) + _ZBufferParams.y));
                                #line 531
                                if ((tmpvar_51 < tmpvar_52)){
                                    if (((tmpvar_52 - tmpvar_51) < _bias)){
                                        #line 535
                                        greyfsd_41.w = 1.0;
                                        greyfsd_41.xyz = oifejef_48;
                                        alsdmes_45 = greyfsd_41;
                                        fjekfesa_44 = true;
                                        #line 539
                                        break;
                                    }
                                    vec3 tmpvar_53;
                                    #line 543
                                    tmpvar_53 = (refDir_44_47 * 0.5);
                                    refDir_44_47 = tmpvar_53;
                                    oifejef_48 = (poffses_42 + tmpvar_53);
                                }
                                else{
                                    #line 549
                                    poffses_42 = oifejef_48;
                                    oifejef_48 = (oifejef_48 + refDir_44_47);
                                }
                                #line 553
                                i_49_43 = (i_49_43 + 1);
                                j_40_1 = (j_40_1 + 1);
                            }
                            if ((fjekfesa_44 == false)){
                                #line 558
                                vec4 tmpvar_55_54;
                                tmpvar_55_54.w = 0.0;
                                tmpvar_55_54.xyz = oifejef_48;
                                alsdmes_45 = tmpvar_55_54;
                                #line 562
                                fjekfesa_44 = true;
                            }
                            opahwcte_2 = alsdmes_45;
                        }
                        #line 566
                        if ((opahwcte_2.w < 0.01)){
                            Result = acccols_8;
                        }
                        else{
                            #line 572
                            vec4 tmpvar_57_55;
                            tmpvar_57_55.xyz = xll_tex2Dlod( _MainTex, vec4( opahwcte_2.xy, 0.0, 0.0)).xyz;
                            tmpvar_57_55.w = (((opahwcte_2.w * (1.0 - (tmpvar_7 / _maxDepthCull))) * (1.0 - pow( (lenfaiejd_14 / float(tmpvar_23)), _fadePower))) * pow( clamp( ((dot( normalize(tmpvar_28), normalize(tmpvar_25).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                            Result = tmpvar_57_55;
                        }
                    }
                }
            }
        }
    }
    #line 582
    return Result;
}
varying vec2 xlv_TEXCOORD0;
void main() {
    vec4 xl_retval;
    PS_IN xlt__In;
    xlt__In.pos = vec4(0.0);
    xlt__In.uv = vec2(xlv_TEXCOORD0);
    xl_retval = PS( xlt__In);
    gl_FragData[0] = vec4(xl_retval);
}
/* NOTE: GLSL optimization failed
0:588(15): error: identifier `xlt__In' uses reserved `__' string
*/

#endif
"
}

SubProgram "d3d9 " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Matrix 4 [glstate_matrix_texture0]
"vs_3_0
; 8 ALU
dcl_position o0
dcl_texcoord0 o1
def c8, 0.00000000, 0, 0, 0
dcl_position0 v0
dcl_texcoord0 v1
mov r0.zw, c8.x
mov r0.xy, v1
dp4 o1.y, r0, c5
dp4 o1.x, r0, c4
dp4 o0.w, v0, c3
dp4 o0.z, v0, c2
dp4 o0.y, v0, c1
dp4 o0.x, v0, c0
"
}

SubProgram "xbox360 " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
Matrix 0 [glstate_matrix_mvp] 4
Matrix 4 [glstate_matrix_texture0] 2
// Shader Timing Estimate, in Cycles/64 vertex vector:
// ALU: 8.00 (6 instructions), vertex: 32, texture: 0,
//   sequencer: 10,  3 GPRs, 31 threads,
// Performance (if enough threads): ~32 cycles per vector
// * Vertex cycle estimates are assuming 3 vfetch_minis for every vfetch_full,
//     with <= 32 bytes per vfetch_full group.

"vs_360
backbbabaaaaaapeaaaaaajaaaaaaaaaaaaaaaceaaaaaaaaaaaaaalmaaaaaaaa
aaaaaaaaaaaaaajeaaaaaabmaaaaaaihpppoadaaaaaaaaacaaaaaabmaaaaaaaa
aaaaaaiaaaaaaaeeaaacaaaaaaaeaaaaaaaaaafiaaaaaaaaaaaaaagiaaacaaae
aaacaaaaaaaaaafiaaaaaaaaghgmhdhegbhegffpgngbhehcgjhifpgnhghaaakl
aaadaaadaaaeaaaeaaabaaaaaaaaaaaaghgmhdhegbhegffpgngbhehcgjhifphe
gfhihehfhcgfdaaahghdfpddfpdaaadccodacodcdadddfddcodaaaklaaaaaaaa
aaaaaajaaaabaaacaaaaaaaaaaaaaaaaaaaaaicbaaaaaaabaaaaaaacaaaaaaab
aaaaacjaaabaaaadaadafaaeaaaadafaaaaabaakdaafcaadaaaabcaamcaaaaaa
aaaaeaafaaaabcaameaaaaaaaaaacaajaaaaccaaaaaaaaaaafpicaaaaaaaagii
aaaaaaaaafpiaaaaaaaaaohiaaaaaaaamiapaaabaabliiaakbacadaamiapaaab
aamgiiaaklacacabmiapaaabaalbdejeklacababmiapiadoaagmaadeklacaaab
miadaaaaaagmlaaakbaaaeaamiadiaaaaamglalaklaaafaaaaaaaaaaaaaaaaaa
aaaaaaaa"
}

SubProgram "ps3 " {
Keywords { }
Matrix 256 [glstate_matrix_mvp]
Matrix 260 [glstate_matrix_texture0]
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
"sce_vp_rsx // 8 instructions using 1 registers
[Configuration]
8
0000000801010100
[Defaults]
1
467 1
00000000
[Microcode]
128
00001c6c004008080106c08360419ffc00001c6c005d30000186c08360407ffc
401f9c6c01d0300d8106c0c360403f80401f9c6c01d0200d8106c0c360405f80
401f9c6c01d0100d8106c0c360409f80401f9c6c01d0000d8106c0c360411f80
401f9c6c01d0500d8086c0c360409f9c401f9c6c01d0400d8086c0c360411f9d
"
}

SubProgram "d3d11 " {
Keywords { }
Bind "vertex" Vertex
Bind "texcoord" TexCoord0
ConstBuffer "UnityPerDraw" 336 // 64 used size, 6 vars
Matrix 0 [glstate_matrix_mvp] 4
ConstBuffer "UnityPerDrawTexMatrices" 768 // 576 used size, 5 vars
Matrix 512 [glstate_matrix_texture0] 4
BindCB "UnityPerDraw" 0
BindCB "UnityPerDrawTexMatrices" 1
// 7 instructions, 1 temp regs, 0 temp arrays:
// ALU 6 float, 0 int, 0 uint
// TEX 0 (0 load, 0 comp, 0 bias, 0 grad)
// FLOW 1 static, 0 dynamic
"vs_4_0
eefiecedjlfomejbofdklfcgafioaaodagpgfnjcabaaaaaaciacaaaaadaaaaaa
cmaaaaaaiaaaaaaaniaaaaaaejfdeheoemaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaaebaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafaepfdejfeejepeoaafeeffiedepepfceeaaklkl
epfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
fdfgfpfagphdgjhegjgpgoaafeeffiedepepfceeaaklklklfdeieefceiabaaaa
eaaaabaafcaaaaaafjaaaaaeegiocaaaaaaaaaaaaeaaaaaafjaaaaaeegiocaaa
abaaaaaaccaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaabaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaaddccabaaaabaaaaaagiaaaaac
abaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaaaaaaaaa
abaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaaaaaaaaaaaaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaaaaaaaaa
acaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaa
egiocaaaaaaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadiaaaaai
dcaabaaaaaaaaaaafgbfbaaaabaaaaaaegiacaaaabaaaaaacbaaaaaadcaaaaak
dccabaaaabaaaaaaegiacaaaabaaaaaacaaaaaaaagbabaaaabaaaaaaegaabaaa
aaaaaaaadoaaaaab"
}

SubProgram "gles " {
Keywords { }
"!!GLES


#ifdef VERTEX

varying highp vec2 xlv_TEXCOORD0;
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_mvp;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesVertex;
void main ()
{
  vec2 tmpvar_1;
  tmpvar_1 = _glesMultiTexCoord0.xy;
  highp vec4 tmpvar_2;
  tmpvar_2.zw = vec2(0.0, 0.0);
  tmpvar_2.x = tmpvar_1.x;
  tmpvar_2.y = tmpvar_1.y;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = (glstate_matrix_texture0 * tmpvar_2).xy;
}



#endif
#ifdef FRAGMENT

#ifndef SHADER_API_GLES
    #define SHADER_API_GLES 1
#endif
#ifndef SHADER_API_MOBILE
    #define SHADER_API_MOBILE 1
#endif
#extension GL_EXT_shader_texture_lod : require
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLodEXT( s, coord.xy, coord.w);
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
#line 328
struct PS_IN {
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
uniform highp float _SSRRcomposeMode;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _CameraNormalsTexture;
uniform highp mat4 _ViewMatrix;
#line 319
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ProjMatrix;
uniform highp float _bias;
uniform highp float _stepGlobalScale;
#line 323
uniform highp float _maxStep;
uniform highp float _maxFineStep;
uniform highp float _maxDepthCull;
uniform highp float _fadePower;
#line 327
uniform sampler2D _MainTex;
#line 334
#line 341
highp vec4 PS( in PS_IN _In ) {
    #line 343
    highp vec3 lkjwejhsdkl_1;
    highp vec4 opahwcte_2;
    highp vec4 Result = vec4( 0.0);
    highp vec4 SourceColor = xll_tex2Dlod( _MainTex, vec4( _In.uv, 0.0, 0.0));
    #line 347
    if ((SourceColor.w == 0.0)){
        Result = vec4( 0.0, 0.0, 0.0, 0.0);
        return Result;
    }
    #line 352
    highp vec4 tmpvar_5;
    tmpvar_5 = xll_tex2Dlod( _CameraDepthTexture, vec4( _In.uv, 0.0, 0.0));
    highp float tmpvar_6;
    tmpvar_6 = tmpvar_5.x;
    #line 356
    highp float tmpvar_7;
    tmpvar_7 = (1.0 / ((_ZBufferParams.x * tmpvar_5.x) + _ZBufferParams.y));
    if ((tmpvar_7 > _maxDepthCull)){
        #line 360
        Result = vec4( 0.0, 0.0, 0.0, 0.0);
    }
    else{
        #line 364
        highp vec4 acccols_8;
        highp int s_9;
        highp vec4 uiduefa_10;
        highp int icoiuf_11;
        #line 368
        bool biifejd_12;
        highp vec4 rensfief_13;
        highp float lenfaiejd_14;
        highp int vbdueff_15;
        #line 372
        highp vec3 eiieiaced_16;
        highp vec3 jjdafhue_17;
        highp vec3 hgeiald_18;
        highp vec4 loveeaed_19;
        #line 376
        highp vec4 mcjkfeeieijd_20;
        highp vec3 xvzyufalj_21;
        highp vec4 efljafolclsdf_22;
        highp int tmpvar_23;
        #line 380
        tmpvar_23 = int(_maxStep);
        efljafolclsdf_22.w = 1.0;
        efljafolclsdf_22.xy = ((_In.uv * 2.0) - 1.0);
        efljafolclsdf_22.z = tmpvar_6;
        #line 384
        highp vec4 tmpvar_24;
        tmpvar_24 = (_ProjectionInv * efljafolclsdf_22);
        highp vec4 tmpvar_25;
        tmpvar_25 = (tmpvar_24 / tmpvar_24.w);
        #line 388
        xvzyufalj_21.xy = efljafolclsdf_22.xy;
        xvzyufalj_21.z = tmpvar_6;
        mcjkfeeieijd_20.w = 0.0;
        mcjkfeeieijd_20.xyz = ((xll_tex2Dlod( _CameraNormalsTexture, vec4( _In.uv, 0.0, 0.0)).xyz * 2.0) - 1.0);
        #line 392
        highp vec3 tmpvar_26;
        tmpvar_26 = normalize(tmpvar_25.xyz);
        highp vec3 tmpvar_27;
        tmpvar_27 = normalize((_ViewMatrix * mcjkfeeieijd_20).xyz);
        #line 396
        highp vec3 tmpvar_28;
        tmpvar_28 = normalize((tmpvar_26 - (2.0 * (dot( tmpvar_27, tmpvar_26) * tmpvar_27))));
        loveeaed_19.w = 1.0;
        loveeaed_19.xyz = (tmpvar_25.xyz + tmpvar_28);
        #line 400
        highp vec4 tmpvar_29;
        tmpvar_29 = (_ProjMatrix * loveeaed_19);
        highp vec3 tmpvar_30;
        tmpvar_30 = normalize(((tmpvar_29.xyz / tmpvar_29.w) - xvzyufalj_21));
        #line 404
        lkjwejhsdkl_1.z = tmpvar_30.z;
        lkjwejhsdkl_1.xy = (tmpvar_30.xy * 0.5);
        hgeiald_18.xy = _In.uv;
        hgeiald_18.z = tmpvar_6;
        #line 408
        highp float tmpvar_31;
        tmpvar_31 = (2.0 / _ScreenParams.x);
        highp float tmpvar_32;
        tmpvar_32 = sqrt(dot( lkjwejhsdkl_1.xy, lkjwejhsdkl_1.xy));
        #line 412
        highp vec3 tmpvar_33;
        tmpvar_33 = (lkjwejhsdkl_1 * ((tmpvar_31 * _stepGlobalScale) / tmpvar_32));
        jjdafhue_17 = tmpvar_33;
        vbdueff_15 = int(_maxStep);
        #line 416
        lenfaiejd_14 = 0.0;
        biifejd_12 = false;
        eiieiaced_16 = (hgeiald_18 + tmpvar_33);
        icoiuf_11 = 0;
        #line 420
        s_9 = 0;
        highp int s_9_1 = 0;
        for ( ; (s_9_1 < 100); ) {
            #line 425
            if ((icoiuf_11 >= vbdueff_15)){
                break;
            }
            #line 430
            highp float tmpvar_34;
            tmpvar_34 = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _CameraDepthTexture, vec4( eiieiaced_16.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
            highp float tmpvar_35;
            tmpvar_35 = (1.0 / ((_ZBufferParams.x * eiieiaced_16.z) + _ZBufferParams.y));
            #line 434
            if ((tmpvar_34 < (tmpvar_35 - 1e-06))){
                uiduefa_10.w = 1.0;
                uiduefa_10.xyz = eiieiaced_16;
                #line 438
                rensfief_13 = uiduefa_10;
                biifejd_12 = true;
                break;
            }
            #line 443
            eiieiaced_16 = (eiieiaced_16 + jjdafhue_17);
            lenfaiejd_14 = (lenfaiejd_14 + 1.0);
            icoiuf_11 = (icoiuf_11 + 1);
            s_9_1 = (s_9_1 + 1);
        }
        #line 449
        if ((biifejd_12 == false)){
            highp vec4 vartfie_36;
            vartfie_36.w = 0.0;
            #line 453
            vartfie_36.xyz = eiieiaced_16;
            rensfief_13 = vartfie_36;
            biifejd_12 = true;
        }
        #line 458
        opahwcte_2 = rensfief_13;
        highp float tmpvar_37;
        tmpvar_37 = abs((rensfief_13.x - 0.5));
        acccols_8 = vec4( 0.0, 0.0, 0.0, 0.0);
        #line 462
        if ((_SSRRcomposeMode > 0.0)){
            highp vec4 tmpvar_38;
            tmpvar_38.w = 0.0;
            #line 466
            tmpvar_38.xyz = SourceColor.xyz;
            acccols_8 = tmpvar_38;
        }
        #line 470
        if ((tmpvar_37 > 0.5)){
            Result = acccols_8;
        }
        else{
            #line 476
            highp float tmpvar_39;
            tmpvar_39 = abs((rensfief_13.y - 0.5));
            if ((tmpvar_39 > 0.5)){
                #line 480
                Result = acccols_8;
            }
            else{
                #line 484
                if (((1.0 / ((_ZBufferParams.x * rensfief_13.z) + _ZBufferParams.y)) > _maxDepthCull)){
                    Result = vec4( 0.0, 0.0, 0.0, 0.0);
                }
                else{
                    #line 490
                    if ((rensfief_13.z < 0.1)){
                        Result = vec4( 0.0, 0.0, 0.0, 0.0);
                    }
                    else{
                        #line 496
                        if ((rensfief_13.w == 1.0)){
                            highp int j_40;
                            highp vec4 greyfsd_41;
                            #line 500
                            highp vec3 poffses_42;
                            highp int i_49_43;
                            bool fjekfesa_44;
                            highp vec4 alsdmes_45;
                            #line 504
                            highp int maxfeis_46;
                            highp vec3 refDir_44_47;
                            highp vec3 oifejef_48;
                            highp vec3 tmpvar_49;
                            #line 508
                            tmpvar_49 = (rensfief_13.xyz - tmpvar_33);
                            highp vec3 tmpvar_50;
                            tmpvar_50 = (lkjwejhsdkl_1 * (tmpvar_31 / tmpvar_32));
                            refDir_44_47 = tmpvar_50;
                            #line 512
                            maxfeis_46 = int(_maxFineStep);
                            fjekfesa_44 = false;
                            poffses_42 = tmpvar_49;
                            oifejef_48 = (tmpvar_49 + tmpvar_50);
                            #line 516
                            i_49_43 = 0;
                            j_40 = 0;
                            highp int j_40_1 = 0;
                            for ( ; (j_40_1 < 20); ) {
                                #line 522
                                if ((i_49_43 >= maxfeis_46)){
                                    break;
                                }
                                #line 527
                                highp float tmpvar_51;
                                tmpvar_51 = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _CameraDepthTexture, vec4( oifejef_48.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
                                highp float tmpvar_52;
                                tmpvar_52 = (1.0 / ((_ZBufferParams.x * oifejef_48.z) + _ZBufferParams.y));
                                #line 531
                                if ((tmpvar_51 < tmpvar_52)){
                                    if (((tmpvar_52 - tmpvar_51) < _bias)){
                                        #line 535
                                        greyfsd_41.w = 1.0;
                                        greyfsd_41.xyz = oifejef_48;
                                        alsdmes_45 = greyfsd_41;
                                        fjekfesa_44 = true;
                                        #line 539
                                        break;
                                    }
                                    highp vec3 tmpvar_53;
                                    #line 543
                                    tmpvar_53 = (refDir_44_47 * 0.5);
                                    refDir_44_47 = tmpvar_53;
                                    oifejef_48 = (poffses_42 + tmpvar_53);
                                }
                                else{
                                    #line 549
                                    poffses_42 = oifejef_48;
                                    oifejef_48 = (oifejef_48 + refDir_44_47);
                                }
                                #line 553
                                i_49_43 = (i_49_43 + 1);
                                j_40_1 = (j_40_1 + 1);
                            }
                            if ((fjekfesa_44 == false)){
                                #line 558
                                highp vec4 tmpvar_55_54;
                                tmpvar_55_54.w = 0.0;
                                tmpvar_55_54.xyz = oifejef_48;
                                alsdmes_45 = tmpvar_55_54;
                                #line 562
                                fjekfesa_44 = true;
                            }
                            opahwcte_2 = alsdmes_45;
                        }
                        #line 566
                        if ((opahwcte_2.w < 0.01)){
                            Result = acccols_8;
                        }
                        else{
                            #line 572
                            highp vec4 tmpvar_57_55;
                            tmpvar_57_55.xyz = xll_tex2Dlod( _MainTex, vec4( opahwcte_2.xy, 0.0, 0.0)).xyz;
                            tmpvar_57_55.w = (((opahwcte_2.w * (1.0 - (tmpvar_7 / _maxDepthCull))) * (1.0 - pow( (lenfaiejd_14 / float(tmpvar_23)), _fadePower))) * pow( clamp( ((dot( normalize(tmpvar_28), normalize(tmpvar_25).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                            Result = tmpvar_57_55;
                        }
                    }
                }
            }
        }
    }
    #line 582
    return Result;
}
varying highp vec2 xlv_TEXCOORD0;
void main() {
    highp vec4 xl_retval;
    PS_IN xlt__In;
    xlt__In.pos = vec4(0.0);
    xlt__In.uv = vec2(xlv_TEXCOORD0);
    xl_retval = PS( xlt__In);
    gl_FragData[0] = vec4(xl_retval);
}
/* NOTE: GLSL optimization failed
0:588(15): error: identifier `xlt__In' uses reserved `__' string
*/


#endif"
}

SubProgram "glesdesktop " {
Keywords { }
"!!GLES


#ifdef VERTEX

varying highp vec2 xlv_TEXCOORD0;
uniform highp mat4 glstate_matrix_texture0;
uniform highp mat4 glstate_matrix_mvp;
attribute vec4 _glesMultiTexCoord0;
attribute vec4 _glesVertex;
void main ()
{
  vec2 tmpvar_1;
  tmpvar_1 = _glesMultiTexCoord0.xy;
  highp vec4 tmpvar_2;
  tmpvar_2.zw = vec2(0.0, 0.0);
  tmpvar_2.x = tmpvar_1.x;
  tmpvar_2.y = tmpvar_1.y;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = (glstate_matrix_texture0 * tmpvar_2).xy;
}



#endif
#ifdef FRAGMENT

#ifndef SHADER_API_GLES
    #define SHADER_API_GLES 1
#endif
#ifndef SHADER_API_DESKTOP
    #define SHADER_API_DESKTOP 1
#endif
#extension GL_EXT_shader_texture_lod : require
vec4 xll_tex2Dlod(sampler2D s, vec4 coord) {
   return texture2DLodEXT( s, coord.xy, coord.w);
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
#line 328
struct PS_IN {
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
uniform highp float _SSRRcomposeMode;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _CameraNormalsTexture;
uniform highp mat4 _ViewMatrix;
#line 319
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ProjMatrix;
uniform highp float _bias;
uniform highp float _stepGlobalScale;
#line 323
uniform highp float _maxStep;
uniform highp float _maxFineStep;
uniform highp float _maxDepthCull;
uniform highp float _fadePower;
#line 327
uniform sampler2D _MainTex;
#line 334
#line 341
highp vec4 PS( in PS_IN _In ) {
    #line 343
    highp vec3 lkjwejhsdkl_1;
    highp vec4 opahwcte_2;
    highp vec4 Result = vec4( 0.0);
    highp vec4 SourceColor = xll_tex2Dlod( _MainTex, vec4( _In.uv, 0.0, 0.0));
    #line 347
    if ((SourceColor.w == 0.0)){
        Result = vec4( 0.0, 0.0, 0.0, 0.0);
        return Result;
    }
    #line 352
    highp vec4 tmpvar_5;
    tmpvar_5 = xll_tex2Dlod( _CameraDepthTexture, vec4( _In.uv, 0.0, 0.0));
    highp float tmpvar_6;
    tmpvar_6 = tmpvar_5.x;
    #line 356
    highp float tmpvar_7;
    tmpvar_7 = (1.0 / ((_ZBufferParams.x * tmpvar_5.x) + _ZBufferParams.y));
    if ((tmpvar_7 > _maxDepthCull)){
        #line 360
        Result = vec4( 0.0, 0.0, 0.0, 0.0);
    }
    else{
        #line 364
        highp vec4 acccols_8;
        highp int s_9;
        highp vec4 uiduefa_10;
        highp int icoiuf_11;
        #line 368
        bool biifejd_12;
        highp vec4 rensfief_13;
        highp float lenfaiejd_14;
        highp int vbdueff_15;
        #line 372
        highp vec3 eiieiaced_16;
        highp vec3 jjdafhue_17;
        highp vec3 hgeiald_18;
        highp vec4 loveeaed_19;
        #line 376
        highp vec4 mcjkfeeieijd_20;
        highp vec3 xvzyufalj_21;
        highp vec4 efljafolclsdf_22;
        highp int tmpvar_23;
        #line 380
        tmpvar_23 = int(_maxStep);
        efljafolclsdf_22.w = 1.0;
        efljafolclsdf_22.xy = ((_In.uv * 2.0) - 1.0);
        efljafolclsdf_22.z = tmpvar_6;
        #line 384
        highp vec4 tmpvar_24;
        tmpvar_24 = (_ProjectionInv * efljafolclsdf_22);
        highp vec4 tmpvar_25;
        tmpvar_25 = (tmpvar_24 / tmpvar_24.w);
        #line 388
        xvzyufalj_21.xy = efljafolclsdf_22.xy;
        xvzyufalj_21.z = tmpvar_6;
        mcjkfeeieijd_20.w = 0.0;
        mcjkfeeieijd_20.xyz = ((xll_tex2Dlod( _CameraNormalsTexture, vec4( _In.uv, 0.0, 0.0)).xyz * 2.0) - 1.0);
        #line 392
        highp vec3 tmpvar_26;
        tmpvar_26 = normalize(tmpvar_25.xyz);
        highp vec3 tmpvar_27;
        tmpvar_27 = normalize((_ViewMatrix * mcjkfeeieijd_20).xyz);
        #line 396
        highp vec3 tmpvar_28;
        tmpvar_28 = normalize((tmpvar_26 - (2.0 * (dot( tmpvar_27, tmpvar_26) * tmpvar_27))));
        loveeaed_19.w = 1.0;
        loveeaed_19.xyz = (tmpvar_25.xyz + tmpvar_28);
        #line 400
        highp vec4 tmpvar_29;
        tmpvar_29 = (_ProjMatrix * loveeaed_19);
        highp vec3 tmpvar_30;
        tmpvar_30 = normalize(((tmpvar_29.xyz / tmpvar_29.w) - xvzyufalj_21));
        #line 404
        lkjwejhsdkl_1.z = tmpvar_30.z;
        lkjwejhsdkl_1.xy = (tmpvar_30.xy * 0.5);
        hgeiald_18.xy = _In.uv;
        hgeiald_18.z = tmpvar_6;
        #line 408
        highp float tmpvar_31;
        tmpvar_31 = (2.0 / _ScreenParams.x);
        highp float tmpvar_32;
        tmpvar_32 = sqrt(dot( lkjwejhsdkl_1.xy, lkjwejhsdkl_1.xy));
        #line 412
        highp vec3 tmpvar_33;
        tmpvar_33 = (lkjwejhsdkl_1 * ((tmpvar_31 * _stepGlobalScale) / tmpvar_32));
        jjdafhue_17 = tmpvar_33;
        vbdueff_15 = int(_maxStep);
        #line 416
        lenfaiejd_14 = 0.0;
        biifejd_12 = false;
        eiieiaced_16 = (hgeiald_18 + tmpvar_33);
        icoiuf_11 = 0;
        #line 420
        s_9 = 0;
        highp int s_9_1 = 0;
        for ( ; (s_9_1 < 100); ) {
            #line 425
            if ((icoiuf_11 >= vbdueff_15)){
                break;
            }
            #line 430
            highp float tmpvar_34;
            tmpvar_34 = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _CameraDepthTexture, vec4( eiieiaced_16.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
            highp float tmpvar_35;
            tmpvar_35 = (1.0 / ((_ZBufferParams.x * eiieiaced_16.z) + _ZBufferParams.y));
            #line 434
            if ((tmpvar_34 < (tmpvar_35 - 1e-06))){
                uiduefa_10.w = 1.0;
                uiduefa_10.xyz = eiieiaced_16;
                #line 438
                rensfief_13 = uiduefa_10;
                biifejd_12 = true;
                break;
            }
            #line 443
            eiieiaced_16 = (eiieiaced_16 + jjdafhue_17);
            lenfaiejd_14 = (lenfaiejd_14 + 1.0);
            icoiuf_11 = (icoiuf_11 + 1);
            s_9_1 = (s_9_1 + 1);
        }
        #line 449
        if ((biifejd_12 == false)){
            highp vec4 vartfie_36;
            vartfie_36.w = 0.0;
            #line 453
            vartfie_36.xyz = eiieiaced_16;
            rensfief_13 = vartfie_36;
            biifejd_12 = true;
        }
        #line 458
        opahwcte_2 = rensfief_13;
        highp float tmpvar_37;
        tmpvar_37 = abs((rensfief_13.x - 0.5));
        acccols_8 = vec4( 0.0, 0.0, 0.0, 0.0);
        #line 462
        if ((_SSRRcomposeMode > 0.0)){
            highp vec4 tmpvar_38;
            tmpvar_38.w = 0.0;
            #line 466
            tmpvar_38.xyz = SourceColor.xyz;
            acccols_8 = tmpvar_38;
        }
        #line 470
        if ((tmpvar_37 > 0.5)){
            Result = acccols_8;
        }
        else{
            #line 476
            highp float tmpvar_39;
            tmpvar_39 = abs((rensfief_13.y - 0.5));
            if ((tmpvar_39 > 0.5)){
                #line 480
                Result = acccols_8;
            }
            else{
                #line 484
                if (((1.0 / ((_ZBufferParams.x * rensfief_13.z) + _ZBufferParams.y)) > _maxDepthCull)){
                    Result = vec4( 0.0, 0.0, 0.0, 0.0);
                }
                else{
                    #line 490
                    if ((rensfief_13.z < 0.1)){
                        Result = vec4( 0.0, 0.0, 0.0, 0.0);
                    }
                    else{
                        #line 496
                        if ((rensfief_13.w == 1.0)){
                            highp int j_40;
                            highp vec4 greyfsd_41;
                            #line 500
                            highp vec3 poffses_42;
                            highp int i_49_43;
                            bool fjekfesa_44;
                            highp vec4 alsdmes_45;
                            #line 504
                            highp int maxfeis_46;
                            highp vec3 refDir_44_47;
                            highp vec3 oifejef_48;
                            highp vec3 tmpvar_49;
                            #line 508
                            tmpvar_49 = (rensfief_13.xyz - tmpvar_33);
                            highp vec3 tmpvar_50;
                            tmpvar_50 = (lkjwejhsdkl_1 * (tmpvar_31 / tmpvar_32));
                            refDir_44_47 = tmpvar_50;
                            #line 512
                            maxfeis_46 = int(_maxFineStep);
                            fjekfesa_44 = false;
                            poffses_42 = tmpvar_49;
                            oifejef_48 = (tmpvar_49 + tmpvar_50);
                            #line 516
                            i_49_43 = 0;
                            j_40 = 0;
                            highp int j_40_1 = 0;
                            for ( ; (j_40_1 < 20); ) {
                                #line 522
                                if ((i_49_43 >= maxfeis_46)){
                                    break;
                                }
                                #line 527
                                highp float tmpvar_51;
                                tmpvar_51 = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _CameraDepthTexture, vec4( oifejef_48.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
                                highp float tmpvar_52;
                                tmpvar_52 = (1.0 / ((_ZBufferParams.x * oifejef_48.z) + _ZBufferParams.y));
                                #line 531
                                if ((tmpvar_51 < tmpvar_52)){
                                    if (((tmpvar_52 - tmpvar_51) < _bias)){
                                        #line 535
                                        greyfsd_41.w = 1.0;
                                        greyfsd_41.xyz = oifejef_48;
                                        alsdmes_45 = greyfsd_41;
                                        fjekfesa_44 = true;
                                        #line 539
                                        break;
                                    }
                                    highp vec3 tmpvar_53;
                                    #line 543
                                    tmpvar_53 = (refDir_44_47 * 0.5);
                                    refDir_44_47 = tmpvar_53;
                                    oifejef_48 = (poffses_42 + tmpvar_53);
                                }
                                else{
                                    #line 549
                                    poffses_42 = oifejef_48;
                                    oifejef_48 = (oifejef_48 + refDir_44_47);
                                }
                                #line 553
                                i_49_43 = (i_49_43 + 1);
                                j_40_1 = (j_40_1 + 1);
                            }
                            if ((fjekfesa_44 == false)){
                                #line 558
                                highp vec4 tmpvar_55_54;
                                tmpvar_55_54.w = 0.0;
                                tmpvar_55_54.xyz = oifejef_48;
                                alsdmes_45 = tmpvar_55_54;
                                #line 562
                                fjekfesa_44 = true;
                            }
                            opahwcte_2 = alsdmes_45;
                        }
                        #line 566
                        if ((opahwcte_2.w < 0.01)){
                            Result = acccols_8;
                        }
                        else{
                            #line 572
                            highp vec4 tmpvar_57_55;
                            tmpvar_57_55.xyz = xll_tex2Dlod( _MainTex, vec4( opahwcte_2.xy, 0.0, 0.0)).xyz;
                            tmpvar_57_55.w = (((opahwcte_2.w * (1.0 - (tmpvar_7 / _maxDepthCull))) * (1.0 - pow( (lenfaiejd_14 / float(tmpvar_23)), _fadePower))) * pow( clamp( ((dot( normalize(tmpvar_28), normalize(tmpvar_25).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                            Result = tmpvar_57_55;
                        }
                    }
                }
            }
        }
    }
    #line 582
    return Result;
}
varying highp vec2 xlv_TEXCOORD0;
void main() {
    highp vec4 xl_retval;
    PS_IN xlt__In;
    xlt__In.pos = vec4(0.0);
    xlt__In.uv = vec2(xlv_TEXCOORD0);
    xl_retval = PS( xlt__In);
    gl_FragData[0] = vec4(xl_retval);
}
/* NOTE: GLSL optimization failed
0:588(15): error: identifier `xlt__In' uses reserved `__' string
*/


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
#line 328
struct PS_IN {
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
uniform highp float _SSRRcomposeMode;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _CameraNormalsTexture;
uniform highp mat4 _ViewMatrix;
#line 319
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ProjMatrix;
uniform highp float _bias;
uniform highp float _stepGlobalScale;
#line 323
uniform highp float _maxStep;
uniform highp float _maxFineStep;
uniform highp float _maxDepthCull;
uniform highp float _fadePower;
#line 327
uniform sampler2D _MainTex;
#line 334
#line 193
highp vec2 MultiplyUV( in highp mat4 mat, in highp vec2 inUV ) {
    highp vec4 temp = vec4( inUV.x, inUV.y, 0.0, 0.0);
    temp = (mat * temp);
    #line 197
    return temp.xy;
}
#line 334
PS_IN VS( in appdata_img v ) {
    PS_IN o;
    o.pos = (glstate_matrix_mvp * v.vertex);
    #line 338
    o.uv = MultiplyUV( glstate_matrix_texture0, v.texcoord);
    return o;
}
out highp vec2 xlv_TEXCOORD0;
void main() {
    PS_IN xl_retval;
    appdata_img xlt_v;
    xlt_v.vertex = vec4(gl_Vertex);
    xlt_v.texcoord = vec2(gl_MultiTexCoord0);
    xl_retval = VS( xlt_v);
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
#line 328
struct PS_IN {
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
uniform highp float _SSRRcomposeMode;
uniform sampler2D _CameraDepthTexture;
uniform sampler2D _CameraNormalsTexture;
uniform highp mat4 _ViewMatrix;
#line 319
uniform highp mat4 _ProjectionInv;
uniform highp mat4 _ProjMatrix;
uniform highp float _bias;
uniform highp float _stepGlobalScale;
#line 323
uniform highp float _maxStep;
uniform highp float _maxFineStep;
uniform highp float _maxDepthCull;
uniform highp float _fadePower;
#line 327
uniform sampler2D _MainTex;
#line 334
#line 341
highp vec4 PS( in PS_IN _In ) {
    #line 343
    highp vec3 lkjwejhsdkl_1;
    highp vec4 opahwcte_2;
    highp vec4 Result = vec4( 0.0);
    highp vec4 SourceColor = xll_tex2Dlod( _MainTex, vec4( _In.uv, 0.0, 0.0));
    #line 347
    if ((SourceColor.w == 0.0)){
        Result = vec4( 0.0, 0.0, 0.0, 0.0);
        return Result;
    }
    #line 352
    highp vec4 tmpvar_5;
    tmpvar_5 = xll_tex2Dlod( _CameraDepthTexture, vec4( _In.uv, 0.0, 0.0));
    highp float tmpvar_6;
    tmpvar_6 = tmpvar_5.x;
    #line 356
    highp float tmpvar_7;
    tmpvar_7 = (1.0 / ((_ZBufferParams.x * tmpvar_5.x) + _ZBufferParams.y));
    if ((tmpvar_7 > _maxDepthCull)){
        #line 360
        Result = vec4( 0.0, 0.0, 0.0, 0.0);
    }
    else{
        #line 364
        highp vec4 acccols_8;
        highp int s_9;
        highp vec4 uiduefa_10;
        highp int icoiuf_11;
        #line 368
        bool biifejd_12;
        highp vec4 rensfief_13;
        highp float lenfaiejd_14;
        highp int vbdueff_15;
        #line 372
        highp vec3 eiieiaced_16;
        highp vec3 jjdafhue_17;
        highp vec3 hgeiald_18;
        highp vec4 loveeaed_19;
        #line 376
        highp vec4 mcjkfeeieijd_20;
        highp vec3 xvzyufalj_21;
        highp vec4 efljafolclsdf_22;
        highp int tmpvar_23;
        #line 380
        tmpvar_23 = int(_maxStep);
        efljafolclsdf_22.w = 1.0;
        efljafolclsdf_22.xy = ((_In.uv * 2.0) - 1.0);
        efljafolclsdf_22.z = tmpvar_6;
        #line 384
        highp vec4 tmpvar_24;
        tmpvar_24 = (_ProjectionInv * efljafolclsdf_22);
        highp vec4 tmpvar_25;
        tmpvar_25 = (tmpvar_24 / tmpvar_24.w);
        #line 388
        xvzyufalj_21.xy = efljafolclsdf_22.xy;
        xvzyufalj_21.z = tmpvar_6;
        mcjkfeeieijd_20.w = 0.0;
        mcjkfeeieijd_20.xyz = ((xll_tex2Dlod( _CameraNormalsTexture, vec4( _In.uv, 0.0, 0.0)).xyz * 2.0) - 1.0);
        #line 392
        highp vec3 tmpvar_26;
        tmpvar_26 = normalize(tmpvar_25.xyz);
        highp vec3 tmpvar_27;
        tmpvar_27 = normalize((_ViewMatrix * mcjkfeeieijd_20).xyz);
        #line 396
        highp vec3 tmpvar_28;
        tmpvar_28 = normalize((tmpvar_26 - (2.0 * (dot( tmpvar_27, tmpvar_26) * tmpvar_27))));
        loveeaed_19.w = 1.0;
        loveeaed_19.xyz = (tmpvar_25.xyz + tmpvar_28);
        #line 400
        highp vec4 tmpvar_29;
        tmpvar_29 = (_ProjMatrix * loveeaed_19);
        highp vec3 tmpvar_30;
        tmpvar_30 = normalize(((tmpvar_29.xyz / tmpvar_29.w) - xvzyufalj_21));
        #line 404
        lkjwejhsdkl_1.z = tmpvar_30.z;
        lkjwejhsdkl_1.xy = (tmpvar_30.xy * 0.5);
        hgeiald_18.xy = _In.uv;
        hgeiald_18.z = tmpvar_6;
        #line 408
        highp float tmpvar_31;
        tmpvar_31 = (2.0 / _ScreenParams.x);
        highp float tmpvar_32;
        tmpvar_32 = sqrt(dot( lkjwejhsdkl_1.xy, lkjwejhsdkl_1.xy));
        #line 412
        highp vec3 tmpvar_33;
        tmpvar_33 = (lkjwejhsdkl_1 * ((tmpvar_31 * _stepGlobalScale) / tmpvar_32));
        jjdafhue_17 = tmpvar_33;
        vbdueff_15 = int(_maxStep);
        #line 416
        lenfaiejd_14 = 0.0;
        biifejd_12 = false;
        eiieiaced_16 = (hgeiald_18 + tmpvar_33);
        icoiuf_11 = 0;
        #line 420
        s_9 = 0;
        highp int s_9_1 = 0;
        for ( ; (s_9_1 < 100); ) {
            #line 425
            if ((icoiuf_11 >= vbdueff_15)){
                break;
            }
            #line 430
            highp float tmpvar_34;
            tmpvar_34 = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _CameraDepthTexture, vec4( eiieiaced_16.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
            highp float tmpvar_35;
            tmpvar_35 = (1.0 / ((_ZBufferParams.x * eiieiaced_16.z) + _ZBufferParams.y));
            #line 434
            if ((tmpvar_34 < (tmpvar_35 - 1e-06))){
                uiduefa_10.w = 1.0;
                uiduefa_10.xyz = eiieiaced_16;
                #line 438
                rensfief_13 = uiduefa_10;
                biifejd_12 = true;
                break;
            }
            #line 443
            eiieiaced_16 = (eiieiaced_16 + jjdafhue_17);
            lenfaiejd_14 = (lenfaiejd_14 + 1.0);
            icoiuf_11 = (icoiuf_11 + 1);
            s_9_1 = (s_9_1 + 1);
        }
        #line 449
        if ((biifejd_12 == false)){
            highp vec4 vartfie_36;
            vartfie_36.w = 0.0;
            #line 453
            vartfie_36.xyz = eiieiaced_16;
            rensfief_13 = vartfie_36;
            biifejd_12 = true;
        }
        #line 458
        opahwcte_2 = rensfief_13;
        highp float tmpvar_37;
        tmpvar_37 = abs((rensfief_13.x - 0.5));
        acccols_8 = vec4( 0.0, 0.0, 0.0, 0.0);
        #line 462
        if ((_SSRRcomposeMode > 0.0)){
            highp vec4 tmpvar_38;
            tmpvar_38.w = 0.0;
            #line 466
            tmpvar_38.xyz = SourceColor.xyz;
            acccols_8 = tmpvar_38;
        }
        #line 470
        if ((tmpvar_37 > 0.5)){
            Result = acccols_8;
        }
        else{
            #line 476
            highp float tmpvar_39;
            tmpvar_39 = abs((rensfief_13.y - 0.5));
            if ((tmpvar_39 > 0.5)){
                #line 480
                Result = acccols_8;
            }
            else{
                #line 484
                if (((1.0 / ((_ZBufferParams.x * rensfief_13.z) + _ZBufferParams.y)) > _maxDepthCull)){
                    Result = vec4( 0.0, 0.0, 0.0, 0.0);
                }
                else{
                    #line 490
                    if ((rensfief_13.z < 0.1)){
                        Result = vec4( 0.0, 0.0, 0.0, 0.0);
                    }
                    else{
                        #line 496
                        if ((rensfief_13.w == 1.0)){
                            highp int j_40;
                            highp vec4 greyfsd_41;
                            #line 500
                            highp vec3 poffses_42;
                            highp int i_49_43;
                            bool fjekfesa_44;
                            highp vec4 alsdmes_45;
                            #line 504
                            highp int maxfeis_46;
                            highp vec3 refDir_44_47;
                            highp vec3 oifejef_48;
                            highp vec3 tmpvar_49;
                            #line 508
                            tmpvar_49 = (rensfief_13.xyz - tmpvar_33);
                            highp vec3 tmpvar_50;
                            tmpvar_50 = (lkjwejhsdkl_1 * (tmpvar_31 / tmpvar_32));
                            refDir_44_47 = tmpvar_50;
                            #line 512
                            maxfeis_46 = int(_maxFineStep);
                            fjekfesa_44 = false;
                            poffses_42 = tmpvar_49;
                            oifejef_48 = (tmpvar_49 + tmpvar_50);
                            #line 516
                            i_49_43 = 0;
                            j_40 = 0;
                            highp int j_40_1 = 0;
                            for ( ; (j_40_1 < 20); ) {
                                #line 522
                                if ((i_49_43 >= maxfeis_46)){
                                    break;
                                }
                                #line 527
                                highp float tmpvar_51;
                                tmpvar_51 = (1.0 / ((_ZBufferParams.x * xll_tex2Dlod( _CameraDepthTexture, vec4( oifejef_48.xy, 0.0, 0.0)).x) + _ZBufferParams.y));
                                highp float tmpvar_52;
                                tmpvar_52 = (1.0 / ((_ZBufferParams.x * oifejef_48.z) + _ZBufferParams.y));
                                #line 531
                                if ((tmpvar_51 < tmpvar_52)){
                                    if (((tmpvar_52 - tmpvar_51) < _bias)){
                                        #line 535
                                        greyfsd_41.w = 1.0;
                                        greyfsd_41.xyz = oifejef_48;
                                        alsdmes_45 = greyfsd_41;
                                        fjekfesa_44 = true;
                                        #line 539
                                        break;
                                    }
                                    highp vec3 tmpvar_53;
                                    #line 543
                                    tmpvar_53 = (refDir_44_47 * 0.5);
                                    refDir_44_47 = tmpvar_53;
                                    oifejef_48 = (poffses_42 + tmpvar_53);
                                }
                                else{
                                    #line 549
                                    poffses_42 = oifejef_48;
                                    oifejef_48 = (oifejef_48 + refDir_44_47);
                                }
                                #line 553
                                i_49_43 = (i_49_43 + 1);
                                j_40_1 = (j_40_1 + 1);
                            }
                            if ((fjekfesa_44 == false)){
                                #line 558
                                highp vec4 tmpvar_55_54;
                                tmpvar_55_54.w = 0.0;
                                tmpvar_55_54.xyz = oifejef_48;
                                alsdmes_45 = tmpvar_55_54;
                                #line 562
                                fjekfesa_44 = true;
                            }
                            opahwcte_2 = alsdmes_45;
                        }
                        #line 566
                        if ((opahwcte_2.w < 0.01)){
                            Result = acccols_8;
                        }
                        else{
                            #line 572
                            highp vec4 tmpvar_57_55;
                            tmpvar_57_55.xyz = xll_tex2Dlod( _MainTex, vec4( opahwcte_2.xy, 0.0, 0.0)).xyz;
                            tmpvar_57_55.w = (((opahwcte_2.w * (1.0 - (tmpvar_7 / _maxDepthCull))) * (1.0 - pow( (lenfaiejd_14 / float(tmpvar_23)), _fadePower))) * pow( clamp( ((dot( normalize(tmpvar_28), normalize(tmpvar_25).xyz) + 1.0) + (_fadePower * 0.1)), 0.0, 1.0), _fadePower));
                            Result = tmpvar_57_55;
                        }
                    }
                }
            }
        }
    }
    #line 582
    return Result;
}
in highp vec2 xlv_TEXCOORD0;
void main() {
    highp vec4 xl_retval;
    PS_IN xlt__In;
    xlt__In.pos = vec4(0.0);
    xlt__In.uv = vec2(xlv_TEXCOORD0);
    xl_retval = PS( xlt__In);
    gl_FragData[0] = vec4(xl_retval);
}


#endif"
}

}
Program "fp" {
// Fragment combos: 1
//   d3d9 - ALU: 178 to 178, TEX: 12 to 12, FLOW: 24 to 24
//   d3d11 - ALU: 107 to 107, TEX: 0 to 0, FLOW: 29 to 29
SubProgram "opengl " {
Keywords { }
"!!GLSL"
}

SubProgram "d3d9 " {
Keywords { }
Vector 12 [_ScreenParams]
Vector 13 [_ZBufferParams]
Float 14 [_SSRRcomposeMode]
Matrix 0 [_ViewMatrix]
Matrix 4 [_ProjectionInv]
Matrix 8 [_ProjMatrix]
Float 15 [_bias]
Float 16 [_stepGlobalScale]
Float 17 [_maxStep]
Float 18 [_maxFineStep]
Float 19 [_maxDepthCull]
Float 20 [_fadePower]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_CameraDepthTexture] 2D
SetTexture 2 [_CameraNormalsTexture] 2D
"ps_3_0
; 178 ALU, 12 TEX, 24 FLOW
dcl_2d s0
dcl_2d s1
dcl_2d s2
def c21, 0.00000000, 1.00000000, 2.00000000, -1.00000000
def c22, 0.50000000, 1.00000000, -0.00000100, -0.50000000
defi i0, 100, 0, 1, 0
def c23, 0.10000000, 0.01000000, 0, 0
defi i1, 20, 0, 1, 0
dcl_texcoord0 v0.xy
mov r1.xy, v0
mov r1.z, c21.x
texldl r3, r1.xyzz, s0
abs r1.x, r3.w
cmp_pp r1.y, -r1.x, c21.x, c21
cmp oC0, -r1.x, c21.x, r0
if_gt r1.y, c21.x
mov r0.xy, v0
mov r0.z, c21.x
texldl r0.x, r0.xyzz, s1
mad r0.y, r0.x, c13.x, c13
rcp r4.w, r0.y
if_gt r4.w, c19.x
mov r0, c21.x
else
mad r8.xy, v0, c21.z, c21.w
mov r5.z, r0.x
mov r5.xy, r8
mov r5.w, c21.y
dp4 r0.y, r5, c7
mov r1.w, r0.y
mov r6.w, c21.y
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
mad r5.xyz, r5, c21.z, c21.w
dp4 r6.z, r5, c2
dp4 r6.x, r5, c0
dp4 r6.y, r5, c1
dp3 r0.y, r6, r6
rsq r0.y, r0.y
mul r5.xyz, r0.z, r1
mul r6.xyz, r0.y, r6
dp3 r0.y, r6, r5
mul r6.xyz, r0.y, r6
mad r5.xyz, -r6, c21.z, r5
dp3 r0.y, r5, r5
rsq r0.y, r0.y
mul r5.xyz, r0.y, r5
add r6.xyz, r1, r5
dp4 r0.y, r6, c11
dp4 r7.z, r6, c10
dp4 r7.y, r6, c9
dp4 r7.x, r6, c8
rcp r0.y, r0.y
mad r6.xyz, r7, r0.y, -r8
dp3 r0.y, r6, r6
rsq r0.y, r0.y
mul r6.xyz, r0.y, r6
mul r0.zw, r6.xyxy, c22.x
rcp r0.y, c12.x
mul r0.zw, r0, r0
mul r7.w, r0.y, c21.z
add r0.y, r0.z, r0.w
rsq r5.w, r0.y
mul r0.z, r7.w, c16.x
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
mov r0.w, c21.y
cmp r2, r10.x, r2, r0
cmp_pp r3.w, r10.x, r3, c21.y
break_lt r9.z, r9.w
add r8.xyz, r8, r7
add r6.w, r6, c21.y
add r9.y, r9, c21
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
cmp r3, -c14.x, r0.w, r3
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
if_gt r0.w, c19.x
mov r0, c21.x
else
if_lt r2.z, c23.x
mov r0, c21.x
else
if_eq r2.w, c21.y
abs r0.y, c18.x
rcp r0.x, r8.w
mul r0.x, r7.w, r0
mul r6.xyz, r6, r0.x
add r2.xyz, r2, -r7
frc r0.z, r0.y
add r0.x, r0.y, -r0.z
add r7.xyz, r6, r2
cmp r2.w, c18.x, r0.x, -r0.x
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
add r0.z, r0, -c15.x
cmp r0.y, r0.z, c21.x, c21
cmp r8.w, r0.x, c21.x, c21.y
mul_pp r8.x, r8.w, r0.y
mov r0.xy, r7
mov r0.z, c21.y
cmp r4.xyz, -r8.x, r4, r0
cmp_pp r0.w, -r8.x, r0, c21.y
break_gt r8.x, c21.x
mul r0.xyz, r6, c22.x
add r8.xyz, r0, r2
cmp r8.xyz, -r8.w, r7, r8
cmp r6.xyz, -r8.w, r6, r0
abs_pp r8.w, r8
add r0.xyz, r6, r8
cmp r7.xyz, -r8.w, r0, r8
cmp r2.xyz, -r8.w, r8, r2
add r7.w, r7, c21.y
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
mov r0.w, c20.x
mad r0.w, c23.x, r0, r1.x
add_sat r2.x, r0.w, c21.y
pow r1, r2.x, c20.x
rcp r0.w, r5.w
mul r0.w, r6, r0
pow r2, r0.w, c20.x
rcp r0.w, c19.x
mad r0.w, -r4, r0, c21.y
mul r0.z, r0, r0.w
mov r1.y, r1.x
mov r1.x, r2
add r1.x, -r1, c21.y
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
mov oC0, r0
endif
"
}

SubProgram "ps3 " {
Keywords { }
Vector 0 [_ScreenParams]
Vector 1 [_ZBufferParams]
Float 2 [_SSRRcomposeMode]
Matrix 196611 [_ViewMatrix]
Matrix 262150 [_ProjectionInv]
Matrix 262154 [_ProjMatrix]
Float 14 [_bias]
Float 15 [_stepGlobalScale]
Float 16 [_maxStep]
Float 17 [_maxFineStep]
Float 18 [_maxDepthCull]
Float 19 [_fadePower]
SetTexture 0 [_MainTex] 2D
SetTexture 1 [_CameraDepthTexture] 2D
SetTexture 2 [_CameraNormalsTexture] 2D
"sce_fp_rsx // 266 instructions using 9 registers
[Configuration]
24
ffffffff000040200001ffff000000000000844009000000
[Offsets]
20
_ScreenParams 2 0
00000560000004c0
_ZBufferParams 6 0
00000c6000000c40000009b0000006e000000670000000e0
_SSRRcomposeMode 1 0
00000890
_ViewMatrix[0] 1 0
00000220
_ViewMatrix[1] 1 0
000001e0
_ViewMatrix[2] 1 0
000001c0
_ProjectionInv[0] 1 0
00000310
_ProjectionInv[1] 1 0
000002c0
_ProjectionInv[2] 1 0
000002a0
_ProjectionInv[3] 1 0
00000280
_ProjMatrix[0] 1 0
00000420
_ProjMatrix[1] 1 0
00000400
_ProjMatrix[2] 1 0
000003e0
_ProjMatrix[3] 1 0
00000440
_bias 1 0
00000cc0
_stepGlobalScale 1 0
000004f0
_maxStep 4 0
00000e8000000e6000000610000005f0
_maxFineStep 2 0
00000b9000000b70
_maxDepthCull 3 0
00001000000009e000000110
_fadePower 3 0
0000105000000f9000000f00
[Microcode]
4256
9e082f00c8011c9d00020000c8003fe100000000000000000000000000000000
117e4180c8101c9dc8000001c80000010280014000021ff4c8000001c8000001
00003f80000000000000000000000000037e4180c9001c9dc8000001c8000001
1e00010000021c9cc8000001c800000100000000000000000000000000000000
1e000100c8041ff5c8000001c8000001000045400000000c0000800000000000
1e7e7e00c8001c9dc8000001c8000001820a2f02c8011c9d00020000c8003fe1
0000000000000000000000000000000002000400c8141c9d00020000aa020000
0000000000000000000000000000000010041a00c8001c9dc8000001c8000001
037e4d00fe081c9d00020000c800000100000000000000000000000000000000
000042400000001000548000042400001e00010000021c9cc8000001c8000001
000000000000000000000000000000001000010000021c9cc8000001c8000001
00003f800000000000000000000000008e002f04c8011c9d00020000c8003fe1
000000000000000000000000000000001c0a040020001c9d00020000aa020000
000040000000bf8000000000000000000806050072141c9cc8020001c8000001
000000000000000000000000000000000406050072141c9cc8020001c8000001
0000000000000000000000000000000086100100c8011c9dc8000001c8003fe1
0810010000141c9cc8000001c80000010206050072141c9cc8020001c8000001
00000000000000000000000000000000060c0400c8201c9d00020000aa020000
000040000000bf8000000000000000000800010000141c9cc8000001c8000001
06000100c8181c9dc8000001c800000110060600c8001c9dc8020001c8000001
00000000000000000000000000000000080e0600c8001c9dc8020001c8000001
00000000000000000000000000000000040e0600c8001c9dc8020001c8000001
00000000000000000000000000000000080c0500c80c1c9dc80c0001c8000001
100e0100c80c1c9dc8000001c80000010e063b00c80c1c9d54180001c8000001
020e0600c8001c9dc8020001c800000100000000000000000000000000000000
080c010000141c9cc8000001c80000011e003a00c81c1c9dfe0c0001c8000001
10060500c8001c9dc8000001c80000010e0e3b00c8001c9dfe0c0001c8000001
100e010000021c9cc8000001c800000100003f80000000000000000000000000
10060500c80c1c9dc81c1001c80000010e060400fe0c1c9fc80c0001c81c0001
10060500c80c1c9dc80c0001c80000010e063b00c80c1c9dfe0c0001c8000001
0e0e0300c8001c9dc80c0001c8000001100a0600c81c1c9dc8020001c8000001
00000000000000000000000000000000080a0600c81c1c9dc8020001c8000001
00000000000000000000000000000000040a0600c81c1c9dc8020001c8000001
00000000000000000000000000000000020e0600c81c1c9dc8020001c8000001
000000000000000000000000000000001c0a3a00c8141c9dc81c0001c8000001
0e0c0300f2141c9dc8180003c8000001100a010000021c9cc8000001c8000001
0000000000000000000000000000000010060500c8181c9dc8180001c8000001
0e0c3b00c8181c9dfe0c0001c80000011006010000021c9cc8000001c8000001
00000000000000000000000000000000060c0100c8181c9dc8005001c8000001
10083a0000021c9cfe0c1001c800000100000000000000000000000000000000
100c3800c8181c9dc8180001c800000110061b00fe181c9dc8000001c8000001
0e0e0200c8181c9dfe100001c80000010e0e0200fe0c1c9dc81c0001c8000001
0e0a0300c81c1c9dc8200001c800000110061a0000021c9cc8001001c8000001
00000000000000000000000000000000100e3b00c8183c9dfe180001c8000001
100c010000021c9cc8000001c800000100000000000000000000000000000000
1e7e7d00c8001c9dc8000001c80000010892018000021c9cc8000001c8000001
0000000000000000000000000000000000004340c8001c9d0190800802040000
1010110000023c9cc8000001c800000100000000000000000000000000000000
037e410000021c9cc8000001c800000100000000000000000000000000000000
02100100fe201c9dc8000001c800000102100100fe200007c8000001c8000001
037e4b00fe181c9dc8200001c800000100004040000000100000800000000000
0210040054141c9d00020000aa02000000000000000000000000000000000000
1092018054021c9dc8000001c8000001000000000000000000003f8000000000
04101a00c8201c9dc8000001c800000102102f02c8141c9d00020000c8000001
000000000000000000000000000000001010040000201c9c00020000aa020000
0000000000000000000000000000000002100300aa201c9c54020001c8000001
000000000000000037bdb5860000000004101a00fe201c9dc8000001c8000001
02100100c8201c9dc8000001c8000001037e4a00aa201c9cc8200001c8000001
1010010000021c9cc8000001c800000100003f80000000000000000000000000
0e100100c8141c9dc8000001c80000011092014055240009c8000001c8000001
1e100100c8040009c8000001c800000108920180ff241c9dc8000001c8000001
1e020100c8201c9dc8000001c800000100004040000000100000800000000000
0e0a0300c8141c9dc81c0001c8000001100a0300c8141c9d00020000c8000001
00003f80000000000000000000000000100c0300c8181c9d00020000c8000001
00003f80000000000000000000000000117e418055241c9dc8000001c8000001
0e100100c8141c9dc8000001c80000011010010000021c9cc8000001c8000001
000000000000000000000000000000001e100100c8041ff5c8000001c8000001
02020300c8201c9daa020000c8000001000000000000bf000000000000000000
117e418000021c9cc8000001c800000100000000000000000000000000000000
037e4d00c8043c9d00020000c800000100003f00000000000000000000000000
10080100aa021c9cc8000001c800000100000000000000000000000000000000
1e020100c8201c9dc8000001c80000011e08010000021fecc8000001c8000001
000000000000000000000000000000000e0a010068201c9dc8000001c8000001
000042400000001002508000042400001e000100c8101c9dc8000001c8000001
100c0300aa041c9c00020000c80000010000bf00000000000000000000000000
037e4d00fe183c9d00020000c800000100003f00000000000000000000000000
000042400000001002688000042400001e000100c8101c9dc8000001c8000001
100c040054041c9d00020000aa02000000000000000000000000000000000000
100c1a00fe181c9dc8000001c8000001037e4d00fe181c9d00020000c8000001
0000000000000000000000000000000000004240000000100288800004240000
1e00010000021c9cc8000001c800000100000000000000000000000000000000
037e4a0054041c9d00020000c8000001cccd3dcc000000000000000000000000
0000424000000010029c8000042400001e00010000021c9cc8000001c8000001
00000000000000000000000000000000037e4f00fe041c9d00020000c8000001
00003f8000000000000000000000000000004240000000100394800003940000
1e7e7e00c8001c9dc8000001c800000110023a00c80c1c9dfe1c0001c8000001
0e0e0100c81c1c9dc8000001c80000010e020300c8041c9dc81c0003c8000001
0e0c0200c8181c9dfe040001c8000001109a018000021c9cc8000001c8000001
000000000000000000000000000000000e0a0300c8181c9dc8040001c8000001
1e7e7d00c8001c9dc8000001c80000011002010000021c9cc8000001c8000001
0000000000000000000000000000000000004340c8001c9d0050800803840000
037e410000021c9cc8000001c800000100000000000000000000000000000000
1006110000023c9cc8000001c800000100000000000000000000000000000000
020e0100fe0c1c9dc8000001c800000110060100c80c1c9dc8000001c8000001
020e0100fe0c0007c8000001c8000001037e4b00fe041c9dc81c0001c8000001
00004040000000100000800000000000080e010000021c9cc8000001c8000001
00003f80000000000000000000000000020e2f02c8141c9d00020000c8000001
0000000000000000000000000000000010060400001c1c9c00020000aa020000
00000000000000000000000000000000020e040054141c9d00020000aa020000
0000000000000000000000000000000010061a00fe0c1c9dc8000001c8000001
020e1a00c81c1c9dc8000001c8000001100e0300c80c1c9f001c0000c8000001
089a0a00fe0c1c9d001c0000c8000001088e0a00fe1c1c9d00020000c8000001
00000000000000000000000000000000117e428055341c9d551c0001c8000001
060e0100c8141c9dc8000001c8000001109e018000021c9cc8000001c8000001
00003f800000000000000000000000000e0e0100c8081fe9c8000001c8000001
109e0140c9341fe9c8000001c8000001109a0180c93c1c9dc8000001c8000001
0e040100c81c1c9dc8000001c80000010000404000001ff00000800000000000
117e418055341c9dc8000001c80000010e0e0100c8141c9dc8000001c8000001
0e0a0200c8181c9d00020000c800000100003f00000000000000000000000000
0e0c0100c8141ff5c8000001c80000010e0e0300c8141ff5c8040001c8000001
10020300c8041c9d00020000c800000100003f80000000000000000000000000
0e020100c81c1fe9c8000001c80000010e0a0100c81c1c9dc8000001c8000001
0e0a0300c8181fe9c81c0001c8000001117e4180c9341c9dc8000001c8000001
080a010000021c9cc8000001c800000100000000000000000000000000000000
0e0a0100c8081ff5c8000001c8000001117e410000021c9cc8000001c8000001
000000000000000000000000000000000402110000023c9cc8000001c8000001
0000000000000000000000000000000010060600c8001c9dc8000001c8000001
10000500c80c1c9dc8000001c800000102000500c80c1c9dc80c0001c8000001
02020200fe0c1c9dc8000001c800000108020100aa041c9cc8000001c8000001
08020100aa041fe6c8000001c80000010204010000021c9cc8000001c8000001
00000000000000000000000000000000037e4a0054141c9d00020000c8000001
d70a3c2300000000000000000000000010023a00c8141c9d54040001c8000001
04021d00fe041c9dc8000001c80000010e002f00c8141c9d00020000c8000001
00000000000000000000000000000000020a3b00fe001c9dc8040001c8000001
100a0200aa041c9c00020000c800000100000000000000000000000000000000
02020400c8081c9d00020000c8140001cccd3dcc000000000000000000000000
020a1c00fe141c9dc8000001c8000001040a830000041c9caa020000c8000001
0000000000003f80000000000000000010003a00c8081c9d00020000c8000001
00000000000000000000000000000000100a1d00aa141c9cc8000001c8000001
040a040054141c9dfe00000354140001020a0400c8141c9faa140000aa140000
10000200c8141c9d00020000c800000100000000000000000000000000000000
10001c00fe001c9dc8000001c80000011000020000141c9cc8000001c8000001
1e000100c8100015c8000001c80000011e010100c8001c9dc8000001c8000001
"
}

SubProgram "d3d11 " {
Keywords { }
ConstBuffer "$Globals" 256 // 248 used size, 11 vars
Float 16 [_SSRRcomposeMode]
Matrix 32 [_ViewMatrix] 4
Matrix 96 [_ProjectionInv] 4
Matrix 160 [_ProjMatrix] 4
Float 224 [_bias]
Float 228 [_stepGlobalScale]
Float 232 [_maxStep]
Float 236 [_maxFineStep]
Float 240 [_maxDepthCull]
Float 244 [_fadePower]
ConstBuffer "UnityPerCamera" 128 // 128 used size, 8 vars
Vector 96 [_ScreenParams] 4
Vector 112 [_ZBufferParams] 4
BindCB "$Globals" 0
BindCB "UnityPerCamera" 1
SetTexture 0 [_MainTex] 2D 2
SetTexture 1 [_CameraDepthTexture] 2D 0
SetTexture 2 [_CameraNormalsTexture] 2D 1
// 201 instructions, 13 temp regs, 0 temp arrays:
// ALU 98 float, 8 int, 1 uint
// TEX 0 (6 load, 0 comp, 0 bias, 0 grad)
// FLOW 14 static, 15 dynamic
"ps_4_0
eefiecedjaiijbffjegaemndlfgnenbfccjofopnabaaaaaapibeaaaaadaaaaaa
cmaaaaaaieaaaaaaliaaaaaaejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafdfgfpfagphdgjhegjgpgoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcdibeaaaa
eaaaaaaaaoafaaaafjaaaaaeegiocaaaaaaaaaaabaaaaaaafjaaaaaeegiocaaa
abaaaaaaaiaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaa
fkaaaaadaagabaaaacaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaae
aahabaaaabaaaaaaffffaaaafibiaaaeaahabaaaacaaaaaaffffaaaagcbaaaad
dcbabaaaabaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacanaaaaaaeiaaaaal
pcaabaaaaaaaaaaaegbabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaacaaaaaa
abeaaaaaaaaaaaaabiaaaaahicaabaaaaaaaaaaadkaabaaaaaaaaaaaabeaaaaa
aaaaaaaabpaaaeaddkaabaaaaaaaaaaadgaaaaaipccabaaaaaaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadoaaaaabbfaaaaabeiaaaaalpcaabaaa
abaaaaaaegbabaaaabaaaaaajghmbaaaabaaaaaaaagabaaaaaaaaaaaabeaaaaa
aaaaaaaadcaaaaalicaabaaaaaaaaaaaakiacaaaabaaaaaaahaaaaaackaabaaa
abaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaakicaabaaaaaaaaaaaaceaaaaa
aaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaaaaaaaaaadbaaaaaiicaabaaa
abaaaaaaakiacaaaaaaaaaaaapaaaaaadkaabaaaaaaaaaaabpaaaeaddkaabaaa
abaaaaaadgaaaaaipccabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
aaaaaaaabcaaaaabdcaaaaapdcaabaaaacaaaaaaegbabaaaabaaaaaaaceaaaaa
aaaaaaeaaaaaaaeaaaaaaaaaaaaaaaaaaceaaaaaaaaaialpaaaaialpaaaaaaaa
aaaaaaaadiaaaaaipcaabaaaadaaaaaafgafbaaaacaaaaaaegiocaaaaaaaaaaa
ahaaaaaadcaaaaakpcaabaaaacaaaaaaegiocaaaaaaaaaaaagaaaaaaagaabaaa
acaaaaaaegaobaaaadaaaaaadcaaaaakpcaabaaaacaaaaaaegiocaaaaaaaaaaa
aiaaaaaakgakbaaaabaaaaaaegaobaaaacaaaaaaaaaaaaaipcaabaaaacaaaaaa
egaobaaaacaaaaaaegiocaaaaaaaaaaaajaaaaaaaoaaaaahpcaabaaaacaaaaaa
egaobaaaacaaaaaapgapbaaaacaaaaaaeiaaaaalpcaabaaaadaaaaaaegbabaaa
abaaaaaaeghobaaaacaaaaaaaagabaaaabaaaaaaabeaaaaaaaaaaaaadcaaaaap
hcaabaaaadaaaaaaegacbaaaadaaaaaaaceaaaaaaaaaaaeaaaaaaaeaaaaaaaea
aaaaaaaaaceaaaaaaaaaialpaaaaialpaaaaialpaaaaaaaabaaaaaahicaabaaa
abaaaaaaegacbaaaacaaaaaaegacbaaaacaaaaaaeeaaaaaficaabaaaabaaaaaa
dkaabaaaabaaaaaadiaaaaahhcaabaaaaeaaaaaapgapbaaaabaaaaaaegacbaaa
acaaaaaadiaaaaaihcaabaaaafaaaaaafgafbaaaadaaaaaaegiccaaaaaaaaaaa
adaaaaaadcaaaaaklcaabaaaadaaaaaaegiicaaaaaaaaaaaacaaaaaaagaabaaa
adaaaaaaegaibaaaafaaaaaadcaaaaakhcaabaaaadaaaaaaegiccaaaaaaaaaaa
aeaaaaaakgakbaaaadaaaaaaegadbaaaadaaaaaabaaaaaahicaabaaaabaaaaaa
egacbaaaadaaaaaaegacbaaaadaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaa
abaaaaaadiaaaaahhcaabaaaadaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaa
baaaaaahicaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaaeaaaaaadiaaaaah
hcaabaaaadaaaaaaegacbaaaadaaaaaapgapbaaaabaaaaaadcaaaaanhcaabaaa
adaaaaaaegacbaiaebaaaaaaadaaaaaaaceaaaaaaaaaaaeaaaaaaaeaaaaaaaea
aaaaaaaaegacbaaaaeaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaadaaaaaa
egacbaaaadaaaaaaeeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaah
hcaabaaaaeaaaaaapgapbaaaabaaaaaaegacbaaaadaaaaaadcaaaaajhcaabaaa
adaaaaaaegacbaaaadaaaaaapgapbaaaabaaaaaaegacbaaaacaaaaaadiaaaaai
pcaabaaaafaaaaaafgafbaaaadaaaaaaegiocaaaaaaaaaaaalaaaaaadcaaaaak
pcaabaaaafaaaaaaegiocaaaaaaaaaaaakaaaaaaagaabaaaadaaaaaaegaobaaa
afaaaaaadcaaaaakpcaabaaaadaaaaaaegiocaaaaaaaaaaaamaaaaaakgakbaaa
adaaaaaaegaobaaaafaaaaaaaaaaaaaipcaabaaaadaaaaaaegaobaaaadaaaaaa
egiocaaaaaaaaaaaanaaaaaaaoaaaaahhcaabaaaadaaaaaaegacbaaaadaaaaaa
pgapbaaaadaaaaaadcaaaaapdcaabaaaabaaaaaaegbabaaaabaaaaaaaceaaaaa
aaaaaaeaaaaaaaeaaaaaaaaaaaaaaaaaaceaaaaaaaaaialpaaaaialpaaaaaaaa
aaaaaaaaaaaaaaaihcaabaaaadaaaaaaegacbaiaebaaaaaaabaaaaaaegacbaaa
adaaaaaabaaaaaahicaabaaaabaaaaaaegacbaaaadaaaaaaegacbaaaadaaaaaa
eeaaaaaficaabaaaabaaaaaadkaabaaaabaaaaaadiaaaaahhcaabaaaadaaaaaa
pgapbaaaabaaaaaaegacbaaaadaaaaaadiaaaaakdcaabaaaafaaaaaaegaabaaa
adaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaaaaaaaaaaaaaoaaaaaiicaabaaa
abaaaaaaabeaaaaaaaaaaaeaakiacaaaabaaaaaaagaaaaaaapaaaaahicaabaaa
adaaaaaaegaabaaaafaaaaaaegaabaaaafaaaaaaelaaaaaficaabaaaadaaaaaa
dkaabaaaadaaaaaadiaaaaaiicaabaaaaeaaaaaadkaabaaaabaaaaaabkiacaaa
aaaaaaaaaoaaaaaaaoaaaaahicaabaaaaeaaaaaadkaabaaaaeaaaaaadkaabaaa
adaaaaaadiaaaaakhcaabaaaadaaaaaaegacbaaaadaaaaaaaceaaaaaaaaaaadp
aaaaaadpaaaaiadpaaaaaaaablaaaaagbcaabaaaafaaaaaackiacaaaaaaaaaaa
aoaaaaaadgaaaaafdcaabaaaabaaaaaaegbabaaaabaaaaaadcaaaaajhcaabaaa
abaaaaaaegacbaaaadaaaaaapgapbaaaaeaaaaaaegacbaaaabaaaaaadgaaaaaf
icaabaaaagaaaaaaabeaaaaaaaaaiadpdgaaaaaipcaabaaaahaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadgaaaaaiocaabaaaafaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadgaaaaafhcaabaaaagaaaaaaegacbaaa
abaaaaaadgaaaaafbcaabaaaaiaaaaaaabeaaaaaaaaaaaaadaaaaaabcbaaaaah
ccaabaaaaiaaaaaaakaabaaaaiaaaaaaabeaaaaageaaaaaaadaaaeadbkaabaaa
aiaaaaaacbaaaaahccaabaaaaiaaaaaabkaabaaaafaaaaaaakaabaaaafaaaaaa
bpaaaeadbkaabaaaaiaaaaaaacaaaaabbfaaaaabeiaaaaalpcaabaaaajaaaaaa
egaabaaaagaaaaaaeghobaaaabaaaaaaaagabaaaaaaaaaaaabeaaaaaaaaaaaaa
dcaaaaalccaabaaaaiaaaaaaakiacaaaabaaaaaaahaaaaaaakaabaaaajaaaaaa
bkiacaaaabaaaaaaahaaaaaaaoaaaaakccaabaaaaiaaaaaaaceaaaaaaaaaiadp
aaaaiadpaaaaiadpaaaaiadpbkaabaaaaiaaaaaadcaaaaalecaabaaaaiaaaaaa
akiacaaaabaaaaaaahaaaaaackaabaaaagaaaaaabkiacaaaabaaaaaaahaaaaaa
aoaaaaakecaabaaaaiaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
ckaabaaaaiaaaaaaaaaaaaahecaabaaaaiaaaaaackaabaaaaiaaaaaaabeaaaaa
lndhiglfdbaaaaahccaabaaaaiaaaaaabkaabaaaaiaaaaaackaabaaaaiaaaaaa
bpaaaeadbkaabaaaaiaaaaaadgaaaaafpcaabaaaahaaaaaaegaobaaaagaaaaaa
dgaaaaafecaabaaaafaaaaaaabeaaaaappppppppacaaaaabbfaaaaabdcaaaaaj
hcaabaaaagaaaaaaegacbaaaadaaaaaapgapbaaaaeaaaaaaegacbaaaagaaaaaa
aaaaaaahicaabaaaafaaaaaadkaabaaaafaaaaaaabeaaaaaaaaaiadpboaaaaah
ccaabaaaafaaaaaabkaabaaaafaaaaaaabeaaaaaabaaaaaaboaaaaahbcaabaaa
aiaaaaaaakaabaaaaiaaaaaaabeaaaaaabaaaaaadgaaaaaipcaabaaaahaaaaaa
aceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadgaaaaafecaabaaaafaaaaaa
abeaaaaaaaaaaaaabgaaaaabdgaaaaaficaabaaaagaaaaaaabeaaaaaaaaaaaaa
dhaaaaajpcaabaaaagaaaaaakgakbaaaafaaaaaaegaobaaaahaaaaaaegaobaaa
agaaaaaaaaaaaaahbcaabaaaabaaaaaaakaabaaaagaaaaaaabeaaaaaaaaaaalp
dbaaaaaiccaabaaaabaaaaaaabeaaaaaaaaaaaaaakiacaaaaaaaaaaaabaaaaaa
abaaaaahhcaabaaaahaaaaaaegacbaaaaaaaaaaafgafbaaaabaaaaaadgaaaaaf
icaabaaaahaaaaaaabeaaaaaaaaaaaaadbaaaaaibcaabaaaaaaaaaaaabeaaaaa
aaaaaadpakaabaiaibaaaaaaabaaaaaabpaaaeadakaabaaaaaaaaaaadgaaaaaf
pccabaaaaaaaaaaaegaobaaaahaaaaaabcaaaaabaaaaaaahbcaabaaaaaaaaaaa
bkaabaaaagaaaaaaabeaaaaaaaaaaalpdbaaaaaibcaabaaaaaaaaaaaabeaaaaa
aaaaaadpakaabaiaibaaaaaaaaaaaaaabpaaaeadakaabaaaaaaaaaaadgaaaaaf
pccabaaaaaaaaaaaegaobaaaahaaaaaabcaaaaabdcaaaaalbcaabaaaaaaaaaaa
akiacaaaabaaaaaaahaaaaaackaabaaaagaaaaaabkiacaaaabaaaaaaahaaaaaa
aoaaaaakbcaabaaaaaaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadp
akaabaaaaaaaaaaadbaaaaaibcaabaaaaaaaaaaaakiacaaaaaaaaaaaapaaaaaa
akaabaaaaaaaaaaabpaaaeadakaabaaaaaaaaaaadgaaaaaipccabaaaaaaaaaaa
aceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabcaaaaabdbaaaaahbcaabaaa
aaaaaaaackaabaaaagaaaaaaabeaaaaamnmmmmdnbpaaaeadakaabaaaaaaaaaaa
dgaaaaaipccabaaaaaaaaaaaaceaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
bcaaaaabbiaaaaahbcaabaaaaaaaaaaadkaabaaaagaaaaaaabeaaaaaaaaaiadp
bpaaaeadakaabaaaaaaaaaaadcaaaaakhcaabaaaaaaaaaaaegacbaiaebaaaaaa
adaaaaaapgapbaaaaeaaaaaaegacbaaaagaaaaaaaoaaaaahbcaabaaaabaaaaaa
dkaabaaaabaaaaaadkaabaaaadaaaaaadiaaaaahocaabaaaabaaaaaaagaabaaa
abaaaaaaagajbaaaadaaaaaablaaaaagicaabaaaadaaaaaadkiacaaaaaaaaaaa
aoaaaaaadcaaaaajhcaabaaaadaaaaaaegacbaaaadaaaaaaagaabaaaabaaaaaa
egacbaaaaaaaaaaadgaaaaafecaabaaaafaaaaaaabeaaaaaaaaaiadpdgaaaaaf
hcaabaaaaiaaaaaaegacbaaaaaaaaaaadgaaaaaihcaabaaaajaaaaaaaceaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaadgaaaaafhcaabaaaakaaaaaajgahbaaa
abaaaaaadgaaaaafbcaabaaaabaaaaaaabeaaaaaaaaaaaaadgaaaaaficaabaaa
aeaaaaaaabeaaaaaaaaaaaaadgaaaaafhcaabaaaalaaaaaaegacbaaaadaaaaaa
dgaaaaaficaabaaaagaaaaaaabeaaaaaaaaaaaaadaaaaaabcbaaaaahicaabaaa
aiaaaaaadkaabaaaagaaaaaaabeaaaaabeaaaaaaadaaaeaddkaabaaaaiaaaaaa
cbaaaaahicaabaaaaiaaaaaaakaabaaaabaaaaaadkaabaaaadaaaaaabpaaaead
dkaabaaaaiaaaaaaacaaaaabbfaaaaabeiaaaaalpcaabaaaamaaaaaaegaabaaa
alaaaaaaeghobaaaabaaaaaaaagabaaaaaaaaaaaabeaaaaaaaaaaaaadcaaaaal
icaabaaaaiaaaaaaakiacaaaabaaaaaaahaaaaaaakaabaaaamaaaaaabkiacaaa
abaaaaaaahaaaaaaaoaaaaakicaabaaaaiaaaaaaaceaaaaaaaaaiadpaaaaiadp
aaaaiadpaaaaiadpdkaabaaaaiaaaaaadcaaaaalicaabaaaajaaaaaaakiacaaa
abaaaaaaahaaaaaackaabaaaalaaaaaabkiacaaaabaaaaaaahaaaaaaaoaaaaak
icaabaaaajaaaaaaaceaaaaaaaaaiadpaaaaiadpaaaaiadpaaaaiadpdkaabaaa
ajaaaaaadbaaaaahicaabaaaakaaaaaadkaabaaaaiaaaaaadkaabaaaajaaaaaa
bpaaaeaddkaabaaaakaaaaaaaaaaaaaiicaabaaaaiaaaaaadkaabaiaebaaaaaa
aiaaaaaadkaabaaaajaaaaaadbaaaaaiicaabaaaaiaaaaaadkaabaaaaiaaaaaa
akiacaaaaaaaaaaaaoaaaaaabpaaaeaddkaabaaaaiaaaaaadgaaaaafdcaabaaa
afaaaaaaegaabaaaalaaaaaadgaaaaafhcaabaaaajaaaaaaegacbaaaafaaaaaa
dgaaaaaficaabaaaaeaaaaaaabeaaaaappppppppacaaaaabbfaaaaabdiaaaaak
hcaabaaaamaaaaaaegacbaaaakaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaadp
aaaaaaaadcaaaaamhcaabaaaalaaaaaaegacbaaaakaaaaaaaceaaaaaaaaaaadp
aaaaaadpaaaaaadpaaaaaaaaegacbaaaaiaaaaaadgaaaaafhcaabaaaakaaaaaa
egacbaaaamaaaaaabcaaaaabaaaaaaahhcaabaaaamaaaaaaegacbaaaakaaaaaa
egacbaaaalaaaaaadgaaaaafhcaabaaaaiaaaaaaegacbaaaalaaaaaadgaaaaaf
hcaabaaaalaaaaaaegacbaaaamaaaaaabfaaaaabboaaaaahbcaabaaaabaaaaaa
akaabaaaabaaaaaaabeaaaaaabaaaaaaboaaaaahicaabaaaagaaaaaadkaabaaa
agaaaaaaabeaaaaaabaaaaaadgaaaaaihcaabaaaajaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaadgaaaaaficaabaaaaeaaaaaaabeaaaaaaaaaaaaa
bgaaaaabdgaaaaafecaabaaaalaaaaaaabeaaaaaaaaaaaaadhaaaaajhcaabaaa
agaaaaaapgapbaaaaeaaaaaaegacbaaaajaaaaaaegacbaaaalaaaaaabcaaaaab
dgaaaaafecaabaaaagaaaaaaabeaaaaaaaaaaaaabfaaaaabdbaaaaahbcaabaaa
aaaaaaaackaabaaaagaaaaaaabeaaaaaaknhcddmbpaaaeadakaabaaaaaaaaaaa
dgaaaaafpccabaaaaaaaaaaaegaobaaaahaaaaaabcaaaaabeiaaaaalpcaabaaa
abaaaaaaegaabaaaagaaaaaaeghobaaaaaaaaaaaaagabaaaacaaaaaaabeaaaaa
aaaaaaaaaoaaaaaibcaabaaaaaaaaaaadkaabaaaaaaaaaaaakiacaaaaaaaaaaa
apaaaaaaaaaaaaaibcaabaaaaaaaaaaaakaabaiaebaaaaaaaaaaaaaaabeaaaaa
aaaaiadpdiaaaaahbcaabaaaaaaaaaaaakaabaaaaaaaaaaackaabaaaagaaaaaa
edaaaaagccaabaaaaaaaaaaackiacaaaaaaaaaaaaoaaaaaaaoaaaaahccaabaaa
aaaaaaaadkaabaaaafaaaaaabkaabaaaaaaaaaaacpaaaaafccaabaaaaaaaaaaa
bkaabaaaaaaaaaaadiaaaaaiccaabaaaaaaaaaaabkaabaaaaaaaaaaabkiacaaa
aaaaaaaaapaaaaaabjaaaaafccaabaaaaaaaaaaabkaabaaaaaaaaaaaaaaaaaai
ccaabaaaaaaaaaaabkaabaiaebaaaaaaaaaaaaaaabeaaaaaaaaaiadpdiaaaaah
bcaabaaaaaaaaaaabkaabaaaaaaaaaaaakaabaaaaaaaaaaabbaaaaahccaabaaa
aaaaaaaaegaobaaaacaaaaaaegaobaaaacaaaaaaeeaaaaafccaabaaaaaaaaaaa
bkaabaaaaaaaaaaadiaaaaahocaabaaaaaaaaaaafgafbaaaaaaaaaaaagajbaaa
acaaaaaabaaaaaahccaabaaaaaaaaaaaegacbaaaaeaaaaaajgahbaaaaaaaaaaa
aaaaaaahccaabaaaaaaaaaaabkaabaaaaaaaaaaaabeaaaaaaaaaiadpdccaaaak
ccaabaaaaaaaaaaabkiacaaaaaaaaaaaapaaaaaaabeaaaaamnmmmmdnbkaabaaa
aaaaaaaacpaaaaafccaabaaaaaaaaaaabkaabaaaaaaaaaaadiaaaaaiccaabaaa
aaaaaaaabkaabaaaaaaaaaaabkiacaaaaaaaaaaaapaaaaaabjaaaaafccaabaaa
aaaaaaaabkaabaaaaaaaaaaadiaaaaahiccabaaaaaaaaaaabkaabaaaaaaaaaaa
akaabaaaaaaaaaaadgaaaaafhccabaaaaaaaaaaaegacbaaaabaaaaaabfaaaaab
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

#LINE 271


	}	//	Pass
}	//SubShader

Fallback off
}	// Shader