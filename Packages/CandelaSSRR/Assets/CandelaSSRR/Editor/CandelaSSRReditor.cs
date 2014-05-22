// CANDELA-SSRR SCREEN SPACE RAYTRACED REFLECTIONS
// Copyright 2014 Livenda

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(CandelaSSRR))]
public class CandelaSSRReditor : Editor
{
	
	private CandelaSSRR cdla;
	private Texture2D candelalogo;
	private GUIStyle back1;
	private GUIStyle back2;
	private GUIStyle back3;
	
	string[] mixOptions = new string[]{"Additive","Physically Accurate" };
	
	private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width*height];
     
        for(int i = 0; i < pix.Length; i++)
                pix[i] = col;
     
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix); 
        result.Apply();
        result.hideFlags = HideFlags.HideAndDontSave;
        return result;
    }
	
	void OnEnable()
	{
	back1 = new GUIStyle();
	back1.normal.background = MakeTex(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.1f));
	back2 = new GUIStyle();
	back2.normal.background = MakeTex(600, 1, new Color(0.5f, 0.0f, 0.0f, 0.1f));
	back3 = new GUIStyle();
	back3.normal.background = MakeTex(600, 1, new Color(0.2f, 0.0f, 0.0f, 0.3f));	
	}
	
	
	
	void Awake()
	{
	cdla = (CandelaSSRR)target;
	candelalogo = Resources.Load("CandelaLogo", typeof(Texture2D)) as Texture2D;
	}
	
	public override void OnInspectorGUI() 
	{
		
		GUI.backgroundColor = new Color(0,0,1,1);
		GUILayout.Box(candelalogo, GUILayout.ExpandWidth(true));
		GUI.backgroundColor = Color.white;
		GUILayout.BeginVertical(back1);
		GUILayout.Space(5);
		GUI.contentColor = new Color(0.7f,0.7f,1.0f,1.0f);
		GUILayout.Label("Global Settings For Reflections",EditorStyles.boldLabel);
		GUILayout.Space(3);
		GUILayout.EndVertical();
		GUI.contentColor = Color.white;
		
		cdla.GlobalScale 		= EditorGUILayout.Slider("Global Step Scale", cdla.GlobalScale, 1.0f, 50.0f);
		cdla.maxGlobalStep      = EditorGUILayout.IntSlider("Global Step Count", cdla.maxGlobalStep, 1, 100);
		cdla.maxFineStep        = EditorGUILayout.IntSlider("Fine Step Count", cdla.maxFineStep, 1, 40);
		GUILayout.Space(5);
		
		GUILayout.BeginVertical(back1);
		cdla.bias		 		= EditorGUILayout.Slider("Global Bias", cdla.bias, 0.0f, 0.001f);
		cdla.fadePower	 		= EditorGUILayout.Slider("Global Fade", cdla.fadePower, 0.02f, 12.0f);
		cdla.maxDepthCull	 	= EditorGUILayout.Slider("Depth Cull", cdla.maxDepthCull, 0.0f, 1.0f);
		cdla.reflectionMultiply	= EditorGUILayout.Slider("Reflection Multiply", cdla.reflectionMultiply, 0.0f, 1.0f);
		GUILayout.EndVertical();
		GUILayout.Space(3);
		
		GUI.contentColor = new Color(0.7f,0.7f,1.0f,1.0f);
		GUILayout.Label("Blur Settings For Glossy Reflections",EditorStyles.boldLabel);
		GUILayout.Space(3);
		GUI.contentColor = Color.white;
		GUILayout.BeginVertical(back2);
		cdla.GlobalBlurRadius	= EditorGUILayout.Slider("Global Blur Radius", cdla.GlobalBlurRadius, 0.0f, 10.0f);
		cdla.HQ_BlurIterations     = EditorGUILayout.IntSlider("Blur Iterations", cdla.HQ_BlurIterations, 1, 5);
		cdla.DistanceBlurRadius	= EditorGUILayout.Slider("Distance Blur Radius", cdla.DistanceBlurRadius, 0.0f, 8.0f);
		cdla.DistanceBlurStart	= EditorGUILayout.Slider("Distance Blur Start", cdla.DistanceBlurStart, 0.0f, 10.0f);
		cdla.GrazeBlurPower   	= EditorGUILayout.Slider("Graze Blur Power", cdla.GrazeBlurPower, 0.0f, 1.0f);
		GUILayout.Space(5);
		GUILayout.EndVertical();
		GUILayout.BeginVertical(back3);
		cdla.BlurQualityHigh       = EditorGUILayout.Toggle("High Quality Blur",cdla.BlurQualityHigh);

		cdla.HQ_DepthSensetivity   = EditorGUILayout.Slider("HQ Depth Sensetivity", cdla.HQ_DepthSensetivity, 0.0f, 5.0f);
		cdla.HQ_NormalsSensetivity = EditorGUILayout.Slider("HQ Normal Sensetivity", cdla.HQ_NormalsSensetivity, 0.0f, 5.0f);
		GUILayout.Space(3);
		GUILayout.EndVertical();
		
		GUI.contentColor = new Color(0.7f,0.7f,1.0f,1.0f);
		GUILayout.Label("Screen Edge Fade Controls",EditorStyles.boldLabel);
		GUILayout.Space(3);
		GUI.contentColor = Color.white;
		
		//SCREEN EDGE FADE CONTROLS
		GUILayout.BeginVertical(back2);

		GUILayout.BeginHorizontal();
		
		bool screenfadeTmp = false;
		if(cdla.DebugScreenFade > 0.0f) screenfadeTmp = true;
		if(EditorGUILayout.Toggle("Show Screen Fade", screenfadeTmp))
			cdla.DebugScreenFade = 1.0f;
		else
			cdla.DebugScreenFade = 0.0f;

		GUILayout.Label("(Use For Debug)");
		
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();

//DEBUG POM
cdla.DEBUG_ShowReflectionTexture = EditorGUILayout.Toggle( "(DEBUG) Show Reflection Texture", cdla.DEBUG_ShowReflectionTexture );
		
		GUILayout.EndHorizontal();
		
		cdla.ScreenFadePower  = EditorGUILayout.Slider("ScreenFadePower", cdla.ScreenFadePower, 0.0f, 10.0f);
		cdla.ScreenFadeSpread = EditorGUILayout.Slider("ScreenFadeSpread", cdla.ScreenFadeSpread, 0.0f, 3.0f);
		cdla.ScreenFadeEdge   = EditorGUILayout.Slider("ScreenFadeEdge", cdla.ScreenFadeEdge, 0.0f, 4.0f);
		
		
		GUI.contentColor = Color.yellow;
		bool useEdgeTextureTmp = false;
		if(cdla.UseEdgeTexture > 0.0f) useEdgeTextureTmp = true;
		if(EditorGUILayout.Toggle("Use Edge Texture", useEdgeTextureTmp))
			cdla.UseEdgeTexture = 1.0f;
		else
			cdla.UseEdgeTexture = 0.0f;
		
		
		//EditorGUIUtility.LookLikeInspector();
		cdla.EdgeFadeTexture = (Texture2D) EditorGUILayout.ObjectField("Edge Fade Texture", cdla.EdgeFadeTexture, typeof (Texture2D), false);
		//EditorGUIUtility.LookLikeControls();

		GUI.contentColor = Color.white;
		GUILayout.EndVertical();
		
		GUILayout.Space(3);
		GUI.contentColor = new Color(0.7f,0.7f,1.0f,1.0f);
		GUILayout.Label("Optimizations For Faster Rendering",EditorStyles.boldLabel);
		GUILayout.Space(3);
		GUI.contentColor = Color.white;
		GUILayout.BeginVertical(back1);
		cdla.ResolutionOptimized   = EditorGUILayout.Toggle("Resolution Optimized",cdla.ResolutionOptimized);
		cdla.UseCustomDepth   	   = EditorGUILayout.Toggle("High Precision Depth",cdla.UseCustomDepth);
		
		GUILayout.EndVertical();
		GUILayout.Space(3);
		
		GUI.contentColor = new Color(0.7f,0.7f,1.0f,1.0f);
		GUILayout.Label("Final Composition Mode",EditorStyles.boldLabel);
		GUILayout.Space(3);
		GUI.contentColor = Color.white;
		
		GUILayout.BeginVertical(back1);
		int mixoption = EditorGUILayout.Popup("Compose Mode:", (int)cdla.SSRRcomposeMode, mixOptions, EditorStyles.popup);
		cdla.SSRRcomposeMode = (float)mixoption;
		cdla.HDRreflections   = EditorGUILayout.Toggle("HDR Reflections",cdla.HDRreflections);
		GUILayout.EndVertical();
		GUILayout.Space(3);
		
		GUI.contentColor = new Color(0.7f,0.7f,1.0f,1.0f);
		GUILayout.Label("Invert Roughness Map For Compatibility",EditorStyles.boldLabel);
		GUILayout.Space(3);
		GUI.contentColor = Color.white;
		GUILayout.BeginVertical(back1);
		cdla.InvertRoughness   	   = EditorGUILayout.Toggle("Invert Roughness",cdla.InvertRoughness);
		GUILayout.EndVertical();
			
		if(GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
		
		//DrawDefaultInspector();
	}
}
 
