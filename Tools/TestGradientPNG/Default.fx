/////////////////////////////////////////////////////////////////////////////////
// (c) Arkane Studios 2013, a Zenimax company
//
// Standard material using Ward anisotropic shading
//
/////////////////////////////////////////////////////////////////////////////////
// 

/////////////////////////////////////////////////////////////////////////////////
// Parameters:
//	. Anisotropy X (FLOAT), roughness value along the tangent axis
//	. Anisotropy Y (FLOAT), roughness value along the bitangent axis
//	. Isotropic (BOOL), discards secondary anisotropy coefficient along bitangent, using only tangent
//
//
//
//#include <include\\stripe_tex.fx>

/////////////////////////////////////////////////////////////////////////////////
// Textures Parameters:
//
Texture2D DiffuseTexture
<
	string UIGroup = "Diffuse";
	string ResourceName = "";
	string UIWidget = "FilePicker";
	string UIName = "Diffuse Map";
	string ResourceType = "2D";	
	int mipmaplevels = NumberOfMipMaps;
	int UIOrder = 201;
	int UVEditorOrder = 1;
>;

Texture2D EmissiveTexture
<
	string UIGroup = "Ambient and Emissive";
	string ResourceName = "";
	string UIWidget = "FilePicker";
	string UIName = "Emissive Map";
	string ResourceType = "2D";
	int mipmaplevels = NumberOfMipMaps;
	int UIOrder = 101;
	int UVEditorOrder = 2;
>;

Texture2D AmbientOcclusionTexture
<
	string UIGroup = "Ambient and Emissive";
	string ResourceName = "";
	string UIWidget = "FilePicker";
	string UIName = "Ambient Occlusion Map";
	string ResourceType = "2D";	
	int mipmaplevels = NumberOfMipMaps;
	int UIOrder = 106;
	int UVEditorOrder = 2;
>;

TextureCube ReflectionTextureCube : environment
<
	string UIGroup = "Reflection";
	string ResourceName = "";
	string UIWidget = "FilePicker";
	string UIName = "Reflection CubeMap";	// Note: do not rename to 'Reflection Cube Map'. This is named this way for backward compatibilty (resave after compat_maya_2013ff10.mel)
	string ResourceType = "Cube";
	int mipmaplevels = 0; // Use (or load) max number of mip map levels so we can use blurring
	int UIOrder = 602;
	int UVEditorOrder = 6;
>;

//------------------------------------
// Shadow Maps
//------------------------------------
Texture2D light0ShadowMap : SHADOWMAP
<
	string Object = "Light 0";	// UI Group for lights, auto-closed
	string UIWidget = "None";
	int UIOrder = 5010;
>;


//------------------------------------
// Per Frame parameters
//------------------------------------
cbuffer UpdatePerFrame : register(b0)
{
	float4x4 viewInv 		: ViewInverse 			< string UIWidget = "None"; >;   
	float4x4 view			: View					< string UIWidget = "None"; >;
	float4x4 prj			: Projection			< string UIWidget = "None"; >;
	float4x4 World2Proj		: ViewProjection		< string UIWidget = "None"; >;

	// A shader may wish to do different actions when Maya is rendering the preview swatch (e.g. disable displacement)
	// This value will be true if Maya is rendering the swatch
	bool IsSwatchRender     : MayaSwatchRender      < string UIWidget = "None"; > = false;

	// If the user enables viewport gamma correction in Maya's global viewport rendering settings, the shader should not do gamma again
	bool MayaFullScreenGamma : MayaGammaCorrection < string UIWidget = "None"; > = false;
}


//------------------------------------
// Per Object parameters
//------------------------------------
cbuffer UpdatePerObject : register(b1)
{
	float4x4 Local2World 		: World 					< string UIWidget = "None"; >;
	float4x4 World2Local 	: WorldInverseTranspose 	< string UIWidget = "None"; >;
	float4x4 Local2Proj		: WorldViewProjection				< string UIWidget = "None"; >;


	// ---------------------------------------------
	// Lighting GROUP
	// ---------------------------------------------
	bool LinearSpaceLighting
	<
		string UIGroup = "Lighting";
		string UIName = "Linear Space Lighting";
		int UIOrder = 10;
	> = true;

	bool UseShadows
	<
		#ifdef _3DSMAX_
			string UIWidget = "None";
		#else
			string UIGroup = "Lighting";
			string UIName = "Shadows";
			int UIOrder = 11;
		#endif
	> = true;

	float shadowMultiplier
	<
		#ifdef _3DSMAX_
			string UIWidget = "None";
		#else
			string UIGroup = "Lighting";
			string UIWidget = "Slider";
			float UIMin = 0.000;
			float UIMax = 1.000;
			float UIStep = 0.001;
			string UIName = "Shadow Strength";
			int UIOrder = 12;
		#endif
	> = {1.0f};

	// This offset allows you to fix any in-correct self shadowing caused by limited precision.
	// This tends to get affected by scene scale and polygon count of the objects involved.
	float shadowDepthBias : ShadowMapBias
	<
		#ifdef _3DSMAX_
			string UIWidget = "None";
		#else
			string UIGroup = "Lighting";
			string UIWidget = "Slider";
			float UIMin = 0.000;
			float UISoftMax = 10.000;
			float UIStep = 0.001;
			string UIName = "Shadow Bias";
			int UIOrder = 13;
		#endif
	> = {0.01f};

	// flips back facing normals to improve lighting for things like sheets of hair or leaves
	bool flipBackfaceNormals
	<
		string UIGroup = "Lighting";
		string UIName = "Double Sided Lighting";
		int UIOrder = 14;
	> = true;


	// -- light props are inserted here via UIOrder 20 - 49


	float rimFresnelMin
	<
		string UIGroup = "Lighting";
		string UIWidget = "Slider";
		float UIMin = 0.0;
		float UIMax = 1.0;
		float UIStep = 0.001;
		string UIName = "Rim Light Min";
		int UIOrder = 60;
	> = 0.8;

	float rimFresnelMax
	<
		string UIGroup = "Lighting";
		string UIWidget = "Slider";
		float UIMin = 0.0;
		float UIMax = 1.0;
		float UIStep = 0.001;
		string UIName = "Rim Light Max";
		int UIOrder = 61;
	> = 1.0;

	float rimBrightness
	<
		string UIGroup = "Lighting";
		string UIWidget = "Slider";
		float UIMin = 0.0;
		float UISoftMax = 10.0;
		float UIMax = _3DSMAX_SPIN_MAX;
		float UIStep = 0.001;
		string UIName = "Rim Light Brightness";
		int UIOrder = 62;
	> = 0.0;


	// ---------------------------------------------
	// Ambient and Emissive GROUP
	// ---------------------------------------------
	bool UseEmissiveTexture
	<
		string UIGroup = "Ambient and Emissive";
		string UIName = "Emissive Map";
		int UIOrder = 100;
	> = false;

	float EmissiveIntensity
	<
		string UIGroup = "Ambient and Emissive";
		string UIName = "Emissive Intensity";
		int UIOrder = 103;
		float UIMin = 0.0;
		float UISoftMax = 2.0;
		float UIMax = _3DSMAX_SPIN_MAX;
		float UIStep = 0.1;
	> = 1.0f;

	float3 AmbientSkyColor : Ambient
	<
		string UIGroup = "Ambient and Emissive";
		string UIName = "Ambient Sky Color";
		string UIWidget = "ColorPicker";
		int UIOrder = 104;
	> = {0.0f, 0.0f, 0.0f };

	float3 AmbientGroundColor : Ambient
	<
		string UIGroup = "Ambient and Emissive";
		string UIName = "Ambient Ground Color";
		string UIWidget = "ColorPicker";
		int UIOrder = 105;
	> = {0.0f, 0.0f, 0.0f };

	bool UseAmbientOcclusionTexture
	<
		string UIGroup = "Ambient and Emissive";
		string UIName = "Ambient Occlusion Map";
		int UIOrder = 106;
	> = false;




	// ---------------------------------------------
	// Diffuse GROUP
	// ---------------------------------------------
	int DiffuseModel
	<
		string UIGroup = "Diffuse";
		string UIName = "Diffuse Model";
		string UIFieldNames ="Lambert:Blended Normal (Skin):Soften Diffuse (Hair)";
		float UIMin = 0;
		float UIMax = 2;
		float UIStep = 1;
		int UIOrder = 198;
	> = false;

	bool UseDiffuseTexture
	<
		string UIGroup = "Diffuse";
		string UIName = "Diffuse Map";
		int UIOrder = 199;
	> = false;

	bool UseDiffuseTextureAlpha
	<
		string UIGroup = "Diffuse";
		string UIName = "Diffuse Map Alpha";
		int UIOrder = 200;
	> = false;

	float3 DiffuseColor : Diffuse
	<
		string UIGroup = "Diffuse";
		string UIName = "Diffuse Color";
		string UIWidget = "ColorPicker";
		int UIOrder = 203;
	> = {1.0f, 1.0f, 1.0f };



	bool UseLightmapTexture
	<
		string UIGroup = "Diffuse";
		string UIName = "Lightmap Map";
		int UIOrder = 300;
	> = false;


	// blended normal

	// This mask map allows you to control the amount of 'softening' that happens on different areas of the object
	bool UseBlendedNormalTexture
	<
		string UIGroup = "Diffuse";
		string UIName = "Blended Normal Mask";
		int UIOrder = 1100;
	> = false;

	float blendNorm
	<
		string UIGroup = "Diffuse";
		float UIMin = 0.0;
		float UISoftMax = 1.0;
		float UIMax = _3DSMAX_SPIN_MAX;
		float UIStep = 0.1;
		string UIName   = "Blended Normal";
		int UIOrder = 1103;
	> = 0.15;


	bool UseDiffuseIBLMap
	<
		string UIGroup = "Diffuse";
		string UIName = "IBL Map";
		int UIOrder = 1150;
	> = false;

	int DiffuseIBLType
	<		
		string UIGroup = "Diffuse";
		string UIWidget = "Slider";
		string UIFieldNames ="Cube:2D Spherical:2D LatLong:Cube & 2D Spherical:Cube & 2D LatLong";
		string UIName = "IBL Type";
		int UIOrder = 1154;
		float UIMin = 0;
		float UIMax = 4;
		float UIStep = 1;
	> = 0;

	float DiffuseIBLIntensity
	<
		string UIGroup = "Diffuse";
		string UIWidget = "Slider";
		float UIMin = 0.0;
		float UISoftMax = 2.0;
		float UIMax = _3DSMAX_SPIN_MAX;
		float UIStep = 0.001;
		string UIName = "IBL Intensity";
		int UIOrder = 1155;
	> = 0.5;

	float DiffuseIBLBlur
	<
		string UIGroup = "Diffuse";
		string UIWidget = "Slider";
		float UIMin = 0.0;
		float UISoftMax = 10.0;
		float UIMax = _3DSMAX_SPIN_MAX;
		float UIStep = 0.001;
		string UIName = "IBL Blur";
		int UIOrder = 1156;
	> = 5.0;

	float DiffuseIBLRotation
	<
		string UIGroup = "Diffuse";
		string UIName = "IBL Rotation";
		float UIMin = 0.0;
		float UISoftMin = 0;
		float UISoftMax = 360;
		float UIMax = _3DSMAX_SPIN_MAX;
		float UIStep = 1.0;
		int UIOrder = 1157;
		string UIWidget = "Slider";
	> = {0.0f};

	float DiffuseIBLPinching
	<
		string UIGroup = "Diffuse";
		string UIWidget = "Slider";
		float UIMin = 0.0;
		float UISoftMin = 1.0;
		float UISoftMax = 1.5;
		float UIMax = _3DSMAX_SPIN_MAX;
		float UIStep = 0.1;
		string UIName = "IBL Spherical Pinch";
		int UIOrder = 1158;
	> = 1.1;


	// Reflection
	float ReflectionFresnelMax
	<
		string UIGroup = "Reflection";
		string UIWidget = "Slider";
		float UIMin = 0.0;
		float UIMax = 1.0;
		float UIStep = 0.001;
		string UIName = "Reflection Fresnel Max";
		int UIOrder = 609;
	> = 0.0;

	bool UseReflectionMask
	<
		string UIGroup = "Reflection";
		string UIName = "Reflection Mask";
		int UIOrder = 700;
	> = false;


} //end UpdatePerObject cbuffer


//------------------------------------
// Light parameters
//------------------------------------
cbuffer UpdateLights : register(b2)
{
	// ---------------------------------------------
	// Light 0 GROUP
	// ---------------------------------------------
	// This value is controlled by Maya to tell us if a light should be calculated
	// For example the artist may disable a light in the scene, or choose to see only the selected light
	// This flag allows Maya to tell our shader not to contribute this light into the lighting
	bool light0Enable : LIGHTENABLE
	<
		string Object = "Light 0";	// UI Group for lights, auto-closed
		string UIName = "Enable Light 0";
		int UIOrder = 20;
	#ifdef _MAYA_
		> = false;	// maya manages lights itself and defaults to no lights
	#else
		> = true;	// in 3dsMax we should have the default light enabled
	#endif

	// follows LightParameterInfo::ELightType
	// spot = 2, point = 3, directional = 4, ambient = 5,
	int light0Type : LIGHTTYPE
	<
		string Object = "Light 0";
		string UIName = "Light 0 Type";
		string UIFieldNames ="None:Default:Spot:Point:Directional:Ambient";
		int UIOrder = 21;
		float UIMin = 0;
		float UIMax = 5;
		float UIStep = 1;
	> = 2;	// default to spot so the cone angle etc work when "Use Shader Settings" option is used

	float3 light0Pos : POSITION 
	< 
		string Object = "Light 0";
		string UIName = "Light 0 Position"; 
		string Space = "World"; 
		int UIOrder = 22;
		int RefID = 0; // 3DSMAX
	> = {100.0f, 100.0f, 100.0f}; 

	float3 light0Color : LIGHTCOLOR 
	<
		string Object = "Light 0";
		#ifdef _3DSMAX_
			int LightRef = 0;
			string UIWidget = "None";
		#else
			string UIName = "Light 0 Color"; 
			string UIWidget = "Color"; 
			int UIOrder = 23;
		#endif
	> = { 1.0f, 1.0f, 1.0f};

	float light0Intensity : LIGHTINTENSITY 
	<
		#ifdef _3DSMAX_
			string UIWidget = "None";
		#else
			string Object = "Light 0";
			string UIName = "Light 0 Intensity"; 
			float UIMin = 0.0;
			float UIMax = _3DSMAX_SPIN_MAX;
			float UIStep = 0.01;
			int UIOrder = 24;
		#endif
	> = { 1.0f };

	float3 light0Dir : DIRECTION 
	< 
		string Object = "Light 0";
		string UIName = "Light 0 Direction"; 
		string Space = "World"; 
		int UIOrder = 25;
		int RefID = 0; // 3DSMAX
	> = {100.0f, 100.0f, 100.0f}; 

	#ifdef _MAYA_
		float light0ConeAngle : HOTSPOT // In radians
	#else
		float light0ConeAngle : LIGHTHOTSPOT
	#endif
	<
		string Object = "Light 0";
		#ifdef _3DSMAX_
			int LightRef = 0;
			string UIWidget = "None";
		#else
			string UIName = "Light 0 Cone Angle"; 
			float UIMin = 0;
			float UIMax = PI/2;
			int UIOrder = 26;
		#endif
	> = { 0.46f };

	#ifdef _MAYA_
		float light0FallOff : FALLOFF // In radians. Sould be HIGHER then cone angle or lighted area will invert
	#else
		float light0FallOff : LIGHTFALLOFF
	#endif
	<
		string Object = "Light 0";
		#ifdef _3DSMAX_
			int LightRef = 0;
			string UIWidget = "None";
		#else
			string UIName = "Light 0 Penumbra Angle"; 
			float UIMin = 0;
			float UIMax = PI/2;
			int UIOrder = 27;
		#endif
	> = { 0.7f };

	float light0AttenScale : DECAYRATE
	<
		string Object = "Light 0";
		string UIName = "Light 0 Decay";
		float UIMin = 0.0;
		float UIMax = _3DSMAX_SPIN_MAX;
		float UIStep = 0.01;
		int UIOrder = 28;
	> = {0.0};

	bool light0ShadowOn : SHADOWFLAG
	<
		#ifdef _3DSMAX_
			string UIWidget = "None";
		#else
			string Object = "Light 0";
			string UIName = "Light 0 Casts Shadow";
			string UIWidget = "None";
			int UIOrder = 29;
		#endif
	> = true;

	float4x4 light0Matrix : SHADOWMAPMATRIX		
	< 
		string Object = "Light 0";
		string UIWidget = "None"; 
	>;

} //end lights cbuffer


/////////////////////////////////////////////////////////////////////////////////
// Uniforms
// float4x4	WorldXf		: World;					// LOCAL => WORLD
// float4x4	WorldITXf	: WorldInverseTranspose;	// WORLD => LOCAL
// float4x4	WvpXf		: WorldViewProjection;		// WORLD => CAMERA => PROJECTION
// float4x4	ViewIXf		: ViewInverse;				// CAMERA => WORLD


/////////////////////////////////////////////////////////////////////////////////
// States
//
RasterizerState WireframeCullFront
{
	CullMode = Front;
	FillMode = WIREFRAME;
};

BlendState PreMultipliedAlphaBlending
{
	AlphaToCoverageEnable = FALSE;
	BlendEnable[0] = TRUE;
	SrcBlend = ONE;
	DestBlend = INV_SRC_ALPHA;
	BlendOp = ADD;
	SrcBlendAlpha = ONE;	// Required for hardware frame render alpha channel
	DestBlendAlpha = INV_SRC_ALPHA;
	BlendOpAlpha = ADD;
	RenderTargetWriteMask[0] = 0x0F;
};


/////////////////////////////////////////////////////////////////////////////////
// Samplers
//
SamplerState CubeMapSampler
{
	Filter = ANISOTROPIC;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;    
};

SamplerState SamplerAnisoWrap
{
	Filter = ANISOTROPIC;
	AddressU = Wrap;
	AddressV = Wrap;
};

SamplerState SamplerShadowDepth
{
	Filter = MIN_MAG_MIP_POINT;
	AddressU = Border;
	AddressV = Border;
	BorderColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
};



struct VS_IN
{
	float3	Position	: POSITION;
	float2	TexCoord0	: TEXCOORD0; 
	float2	TexCoord1	: TEXCOORD1; 
	float2	TexCoord2	: TEXCOORD2;
	float3	Normal		: NORMAL;
	float3	BiTangent	: BINORMAL;
	float3	Tangent		: TANGENT; 
};

struct PS_IN
{
	float4	__Position		: SV_Position;
	float2	TexCoord0		: TEXCOORD0; 
	float2	TexCoord1		: TEXCOORD1;
	float2	TexCoord2		: TEXCOORD2;
	float3	WorldNormal   	: NORMAL;
	float4	WorldTangent 	: TANGENT; 
	float3	WorldPosition	: TEXCOORD3;
};


/////////////////////////////////////////////////////////////////////////////////

#define SHADOW_FILTER_TAPS_CNT 10
float2 SuperFilterTaps[SHADOW_FILTER_TAPS_CNT] 
< 
	string UIWidget = "None"; 
> = 
{ 
    {-0.84052f, -0.073954f}, 
    {-0.326235f, -0.40583f}, 
    {-0.698464f, 0.457259f}, 
    {-0.203356f, 0.6205847f}, 
    {0.96345f, -0.194353f}, 
    {0.473434f, -0.480026f}, 
    {0.519454f, 0.767034f}, 
    {0.185461f, -0.8945231f}, 
    {0.507351f, 0.064963f}, 
    {-0.321932f, 0.5954349f} 
};

float shadowMapTexelSize 
< 
	string UIWidget = "None"; 
> = {0.00195313}; // (1.0f / 512)

// Shadows:
// Percentage-Closer Filtering
float lightShadow(float4x4 LightViewPrj, uniform Texture2D ShadowMapTexture, float3 VertexWorldPosition)
{
	float shadow = 1.0f;

	float4 Pndc = mul( float4(VertexWorldPosition.xyz,1.0) ,  LightViewPrj); 
	Pndc.xyz /= Pndc.w; 
	if ( Pndc.x > -1.0f && Pndc.x < 1.0f && Pndc.y  > -1.0f   
		&& Pndc.y <  1.0f && Pndc.z >  0.0f && Pndc.z <  1.0f ) 
	{ 
		float2 uv = 0.5f * Pndc.xy + 0.5f; 
		uv = float2(uv.x,(1.0-uv.y));	// maya flip Y
		float z = Pndc.z - shadowDepthBias / Pndc.w; 

		// we'll sample a bunch of times to smooth our shadow a little bit around the edges:
		shadow = 0.0f;
		for(int i=0; i<SHADOW_FILTER_TAPS_CNT; ++i) 
		{ 
			float2 suv = uv + (SuperFilterTaps[i] * shadowMapTexelSize);
			float val = z - ShadowMapTexture.SampleLevel(SamplerShadowDepth, suv, 0 ).x;
			shadow += (val >= 0.0f) ? 0.0f : (1.0f / SHADOW_FILTER_TAPS_CNT);
		}

		// a single sample would be:
		// shadow = 1.0f;
		// float val = z - ShadowMapTexture.SampleLevel(SamplerShadowDepth, uv, 0 ).x;
		// shadow = (val >= 0.0f)? 0.0f : 1.0f;
		
		shadow = lerp(1.0f, shadow, shadowMultiplier);  
	} 

	return shadow;
}

// This function is from Nvidia's Human Head demo
float fresnelReflectance( float3 H, float3 V, float F0 )  
{
	float base = 1.0 - dot( V, H );
	float exponential = pow( base, 5.0 );  
	return exponential + F0 * ( 1.0 - exponential );
}

// This function is from Nvidia's Human Head demo
float beckmannBRDF(float ndoth, float m)
{
  float alpha = acos( ndoth );  
  float ta = tan( alpha );  
  float val = 1.0/(m*m*pow(ndoth,4.0)) * exp(-(ta*ta)/(m*m));
  return val;  
}

// This function is from Nvidia's Human Head demo
float3 KelemenSzirmaykalosSpecular(float3 N, float3 L, float3 V, float roughness, float3 specularColorIn)
{
	float3 result = float3(0.0, 0.0, 0.0);
	float ndotl = dot(N, L);
	if (ndotl > 0.0)
	{
		float3 h = L + V;
		float3 H = normalize( h );
		float ndoth = dot(N, H);
		float PH = beckmannBRDF(ndoth, roughness);
		float F = fresnelReflectance( H, V, 0.028 );
		float frSpec = max( PH * F / dot( h, h ), 0 ); 
		result = ndotl * specularColorIn * frSpec;
	}
	return result;
}

// This function is from John Hable's Siggraph talk
float3 blendedNormalDiffuse(float3 L, float3 Ng, float3 Nm, float softenMask, float shadow)
{
	float redBlend = lerp(0, 0.9, softenMask);
	float redSoften = redBlend * blendNorm;
	float blueBlend = lerp(0, 0.35, softenMask);
	float blueSoften = blueBlend * blendNorm;
	
	float DNr = (saturate(dot(Ng, L) * (1 - redSoften) + redSoften) * shadow);//diffuse using geometry normal
	float DNb = (saturate(dot(Nm, L) * (1 - blueSoften) + blueSoften) * shadow);//diffuse using normal map
	float R = lerp(DNb, DNr, redBlend);//final diffuse for red channel using more geometry normal
	float B = lerp(DNb, DNr, blueBlend);//final diffuse for blue using more normal map
	float3 finalDiffuse = float3(R, B, B);
	float cyanReduction = 0.03 + R;
	finalDiffuse.gb = min(cyanReduction, finalDiffuse.gb);
	return finalDiffuse;
}

// Ward anisotropic specular lighting, modified to support anisotropic direction map (aka Comb or Flow map)
float3 WardAniso(float3 N, float3 H, float NdotL, float NdotV, float Roughness1, float Roughness2, float3 anisotropicDir, float3 specColor)
{
	float3 binormalDirection = cross(N, anisotropicDir);

	float HdotN = dot(H, N);
	float dotHDirRough1 = dot(H, anisotropicDir) / Roughness1;
	float dotHBRough2 = dot(H, binormalDirection) / Roughness2;
 
	float attenuation = 1.0;
	float3 spec = attenuation * specColor
		* sqrt(max(0.0, NdotL / NdotV)) 
		* exp(-2.0 * (dotHDirRough1 * dotHDirRough1 
		+ dotHBRough2 * dotHBRough2) / (1.0 + HdotN));

	return spec;
}




//------------------------------------
// vertex shader without tessellation
//------------------------------------
PS_IN	VS( VS_IN _In ) 
{
	PS_IN	Out = PS_IN( _In );
		
	// If we don't use tessellation, pass vertices in clip space:
// #ifdef _SUPPORTTESSELLATION_
// 	Out.__position = mul( float4( _In.Position.xyz, 1.0 ), World2Proj );
// #else
	Out.__position = mul( float4( _In.Position, 1.0 ), Local2Proj );
//#endif

	return Out;
}



//------------------------------------
// pixel shader
//------------------------------------
float4 PS( PS_IN _In, bool FrontFace : SV_IsFrontFace ) : SV_Target
{
	return float4( 1, 0, 0, 1 );

// #ifdef _3DSMAX_
// 	FrontFace = !FrontFace;
// #endif
// 
// 	// clip are early as possible
// 	float2 opacityMaskUV = pickTexcoord(OpacityMaskTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 	OpacityMaskClip(opacityMaskUV);
// 
// 	float gammaCorrection = lerp(1.0, 2.2, LinearSpaceLighting);
// 
// 	float3 N = normalize(_In.worldNormal.xyz);
// 	if (flipBackfaceNormals)
// 	{
// 		N = lerp (-N, N, FrontFace);
// 	}
// 	float3 Nw = N;
// 
// 	// Tangent and BiNormal:
// 	float3 T = normalize(_In.worldTangent.xyz);
// 	float3 Bn = cross(N, T); 
// 	Bn *= _In.worldTangent.w; 
// 
// 	if (UseNormalTexture)
// 	{
// 		float3x3 toWorld = float3x3(T, Bn, N);
// 
// 		float2 normalUV = pickTexcoord(NormalTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 		float3 NormalMap = NormalTexture.Sample(SamplerAnisoWrap, normalUV).xyz * 2 - 1;
// 
// 		if (NormalCoordsysX > 0)
// 			NormalMap.x = -NormalMap.x;
// 		if (NormalCoordsysY > 0)
// 			NormalMap.y = -NormalMap.y;
// 
// 		NormalMap.xy *= NormalHeight; 
// 		NormalMap = mul(NormalMap.xyz, toWorld);
// 
// 		N = normalize(NormalMap.rgb);
// 	}
// 	
// 	float3 V = normalize( viewInv[3].xyz - _In.worldPosition.xyz );
// 
// 	float glossiness =  max(1.0, SpecPower);
// 	float specularAlpha = 1.0;
// 	float3 specularColor = SpecularColor;
// 	if (UseSpecularTexture)
// 	{
// 		float2 opacityUV = pickTexcoord(SpecularTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 		float4 SpecularTextureSample = SpecularTexture.Sample(SamplerAnisoWrap, opacityUV);
// 
// 		specularColor *= pow(SpecularTextureSample.rgb, gammaCorrection);
// 		specularAlpha = SpecularTextureSample.a;
// 		glossiness *= (SpecularTextureSample.a + 1);
// 	}
// 
// 	float4 anisotropicDir = float4(T, 1);	// alpha is the blinn-aniso mask
// 	if (UseAnisotropicDirectionMap)
// 	{
// 		float2 anisoDirUV = pickTexcoord(AnisotropicTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 
// 		if (AnisotropicDirectionType == 0)	// use tangent map for direction
// 		{
// 			anisotropicDir = AnisotropicTexture.Sample(SamplerAnisoWrap, anisoDirUV);
// 			anisotropicDir.xyz = anisotropicDir.xyz * 2 - 1;	// unpack
// 		}
// 	}
// 
// 	float roughness = min( SpecPower/100.0f, 1) * specularAlpha;		// divide by 100 so we get more user friendly values when switching from Phong based on slider range.
// 	roughness = 1.0f-roughness;											// flip so it is more user friendly when switching from Phong
// 
// 	float reflectFresnel = saturate((saturate(1.0f - dot(N, V))-ReflectionFresnelMin)/(ReflectionFresnelMax - ReflectionFresnelMin));	
// 
// 	bool reflectMapUsed = UseReflectionMap;
// 	float3 reflectionColor = lerp(float3(1,1,1), specularColor, UseSpecColorToTintReflection) * (ReflectionIntensity*reflectMapUsed) * reflectFresnel;	
// 	if (UseReflectionMask)
// 	{
// 		float2 reflectionMaskUV = pickTexcoord(ReflectionMaskTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 		float4 ReflectionMaskSample = ReflectionMask.Sample(SamplerAnisoWrap, reflectionMaskUV);
// 
// 		reflectionColor *=  ReflectionMaskSample.r;
// 	}
// 
// 	float3 diffuseColor = DiffuseColor;
// 	diffuseColor *= (1 - saturate(reflectionColor));
// 	float diffuseAlpha = 1.0f;
// 	if (UseDiffuseTexture)
// 	{
// 		float2 diffuseUV = pickTexcoord(DiffuseTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 		float4 diffuseTextureSample = DiffuseTexture.Sample(SamplerAnisoWrap, diffuseUV);
// 
// 		if (UseDiffuseTextureAlpha)
// 		{
// 			diffuseAlpha = diffuseTextureSample.a;
// 		}
// 
// 		diffuseColor *= pow(diffuseTextureSample.rgb, gammaCorrection);
// 	}
// 
// 	// Opacity:
// 	float opacity = saturate(diffuseAlpha * Opacity);
// 
// 	// allow opacity to changed based on angle from camera:
// 	// This will only work well for polygons that are facing the camera
// 	if (OpacityFresnelMin > 0 || OpacityFresnelMax > 0)
// 	{
// 		float opacityFresnel = saturate( (saturate(1.0f - dot(N, V))-OpacityFresnelMin)/(OpacityFresnelMax - OpacityFresnelMin) );
// 		opacityFresnel *= FrontFace;
// 		opacity *= opacityFresnel;
// 	}
// 
// 	float3 reflectColorTotal = reflectionColor;
// 	if (reflectMapUsed)
// 	{
// 		// below "8" should really be the number of mip maps levels in the cubemap, but since we don't know this (Maya is not passing this to us) we hard code it.
// 		float ReflectionMipLevel = (ReflectionBlur + (8.0 * (UseSpecAlphaForReflectionBlur * (1 - specularAlpha))));
// 
// 		float3 reflectMapColor = float3(0,0,0);
// 
// 		if (ReflectionType == 0 || ReflectionType == 3 || ReflectionType == 4)	// CUBE	
// 		{
// 			float3 reflectionVector = reflect(-V, N);
// 			#ifdef _ZUP_
// 				reflectionVector = reflectionVector.xzy;
// 			#endif
// 			reflectionVector = RotateVectorYaw(reflectionVector, ReflectionRotation);
// 			reflectionVector = normalize(reflectionVector);
// 			reflectMapColor += pow(ReflectionTextureCube.SampleLevel(CubeMapSampler, reflectionVector, ReflectionMipLevel).rgb, gammaCorrection);
// 		}
// 
// 		if (ReflectionType == 1 || ReflectionType == 3)	// 2D SPHERICAL
// 		{
// 			float3 reflectionVector = reflect(V, N);
// 			#ifdef _ZUP_
// 				reflectionVector = reflectionVector.xzy;
// 			#endif
// 			reflectionVector = RotateVectorYaw(reflectionVector, ReflectionRotation);
// 			reflectionVector = normalize(reflectionVector);
// 			float2 sphericalUVs = SphericalReflectionUVFunction(reflectionVector, ReflectionPinching);
// 			reflectMapColor += pow(ReflectionTexture2D.SampleLevel(SamplerAnisoWrap, sphericalUVs, ReflectionMipLevel).rgb, gammaCorrection);
// 		}
// 		else if (ReflectionType == 2 || ReflectionType == 4)	// 2D LATLONG
// 		{
// 			float3 reflectionVector = reflect(-V, N);
// 			#ifdef _ZUP_
// 				reflectionVector = reflectionVector.xzy;
// 			#endif
// 			reflectionVector = RotateVectorYaw(reflectionVector, ReflectionRotation);
// 			reflectionVector = normalize(reflectionVector);
// 			float2 latLongUVs = Latlong(reflectionVector);
// 			reflectMapColor += pow(ReflectionTexture2D.SampleLevel(SamplerAnisoWrap, latLongUVs, ReflectionMipLevel).rgb, gammaCorrection);
// 		}
// 
// 		reflectColorTotal *= reflectMapColor;
// 
// 		if (!ReflectionAffectOpacity)	// multiply reflection with opacity for pre-mul alpha only when reflections do not make object opaque in those areas
// 			reflectColorTotal *= opacity;
// 	}
// 
// 	#ifndef _ZUP_
// 		float ambientUpAxis = N.y;
// 	#else
// 		float ambientUpAxis = N.z;
// 	#endif
// 
// 	float3 ambientColor = (lerp(AmbientGroundColor, AmbientSkyColor, ((ambientUpAxis * 0.5) + 0.5)) * diffuseColor);
// 
// 	float3 ambientOcclusion = float3(1,1,1);
// 	if (UseAmbientOcclusionTexture)
// 	{
// 		float2 aomapUV = pickTexcoord(AmbientOcclusionTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 		float3 aomapTextureSample = AmbientOcclusionTexture.Sample(SamplerAnisoWrap, aomapUV).rgb;
// 		ambientOcclusion *= aomapTextureSample.rgb;
// 		ambientColor *= ambientOcclusion;
// 	}
// 
// 	// emissive after AO to make sure AO does not block glow
// 	if (UseEmissiveTexture)
// 	{
// 		float2 emissiveUV = pickTexcoord(EmissiveTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 		float4 EmissiveColor = EmissiveTexture.Sample(SamplerAnisoWrap, emissiveUV);
// 
// 		ambientColor += EmissiveColor.rgb * EmissiveIntensity;
// 	}
// 
// 	if (UseLightmapTexture)
// 	{
// 		// We assume this texture does not need to be converted to linear space
// 		float2 lightmapUV = pickTexcoord(LightmapTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 		float3 lightmapTextureSample = LightmapTexture.Sample(SamplerAnisoWrap, lightmapUV).rgb;
// 		diffuseColor *= lightmapTextureSample.rgb;
// 	}
// 
// 	float3 thickness = float3(1,1,1);
// 	if (UseThicknessTexture)
// 	{
// 		float2 thicknessUV = pickTexcoord(ThicknessTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 		thickness = TranslucencyThicknessMask.Sample(SamplerAnisoWrap, thicknessUV).xyz;
// 	}
// 
// 	float softenMask = 1.0f;
// 	if (UseBlendedNormalTexture)
// 	{
// 		float2 softenUV = pickTexcoord(BlendedNormalMaskTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
// 		softenMask = BlendedNormalMask.Sample(SamplerAnisoWrap, softenUV).r;
// 	}
// 
// 	// Rim light:
// 	// This will only work well for polygons that are facing the camera
// 	float rim = saturate((saturate(1.0f - dot(N, V))-rimFresnelMin)/(max(rimFresnelMax, rimFresnelMin)  - rimFresnelMin));
// 	rim *= FrontFace;
// 	rim *= rimBrightness * max(specularAlpha, 0.2);	
// 
// 
// 
// 	// --------
// 	// LIGHTS:
// 	// --------
// 	// future todo: Maya could pass light info in array so we can loop any number of lights.
// 
// 	// light 0:
// 	lightOut light0 = CalculateLight(	light0Enable, light0Type, light0AttenScale, light0Pos, _In.worldPosition.xyz, 
// 										light0Color, light0Intensity, light0Dir, light0ConeAngle, light0FallOff, light0Matrix, 
// 										light0ShadowMap, light0ShadowOn, Nw, N, diffuseColor, V, roughness, specularColor,
// 										thickness, softenMask, gammaCorrection, rim, glossiness, opacity, ambientOcclusion, anisotropicDir );
// 
// 	// light 1:
// 	lightOut light1 = CalculateLight(	light1Enable, light1Type, light1AttenScale, light1Pos, _In.worldPosition.xyz, 
// 										light1Color, light1Intensity, light1Dir, light1ConeAngle, light1FallOff, light1Matrix, 
// 										light1ShadowMap, light1ShadowOn, Nw, N, diffuseColor, V, roughness, specularColor,
// 										thickness, softenMask, gammaCorrection, rim, glossiness, opacity, ambientOcclusion, anisotropicDir );
// 
// 	// light 2:
// 	lightOut light2 = CalculateLight(	light2Enable, light2Type, light2AttenScale, light2Pos, _In.worldPosition.xyz, 
// 										light2Color, light2Intensity, light2Dir, light2ConeAngle, light2FallOff, light2Matrix, 
// 										light2ShadowMap, light2ShadowOn, Nw, N, diffuseColor, V, roughness, specularColor,
// 										thickness, softenMask, gammaCorrection, rim, glossiness, opacity, ambientOcclusion, anisotropicDir );
// 
// 	float3 lightTotal =  light0.Color + light1.Color + light2.Color;
// 
// 
// 	// ----------------------
// 	// IMAGE BASED LIGHTING
// 	// ----------------------
// 	// Diffuse IBL
// 	bool useDiffuseIBL = UseDiffuseIBLMap;
// 	if (useDiffuseIBL)
// 	{
// 		float diffuseIBLMipLevel = DiffuseIBLBlur;
// 
// 		// We use the world normal to sample the lighting texture
// 		float3 diffuseIBLVec = N;
// 		#ifdef _ZUP_
// 			diffuseIBLVec = diffuseIBLVec.xzy;
// 		#endif
// 
// 		diffuseIBLVec = RotateVectorYaw(diffuseIBLVec, DiffuseIBLRotation);
// 		diffuseIBLVec = normalize(diffuseIBLVec);
// 
// 		float3 diffuseIBLcolor = float3(0,0,0);
// 		if (DiffuseIBLType == 0 || DiffuseIBLType == 3 || DiffuseIBLType == 4)	// CUBE
// 		{
// 			diffuseIBLcolor = pow(DiffuseIBLTextureCube.SampleLevel(CubeMapSampler, diffuseIBLVec, diffuseIBLMipLevel).rgb, gammaCorrection);
// 		}
// 
// 		if (DiffuseIBLType == 1 || DiffuseIBLType == 3)	// 2D SPHERICAL
// 		{
// 			float2 sphericalUVs = SphericalReflectionUVFunction(-diffuseIBLVec, DiffuseIBLPinching);
// 			float3 preDiffuseIBL = diffuseIBLcolor;
// 			diffuseIBLcolor = pow(DiffuseIBLTexture2D.SampleLevel(SamplerAnisoWrap, sphericalUVs, diffuseIBLMipLevel).rgb, gammaCorrection);
// 
// 			if (DiffuseIBLType == 3)	// combine Cube and Spherical
// 				diffuseIBLcolor += preDiffuseIBL;
// 		}
// 		else if (DiffuseIBLType == 2 || DiffuseIBLType == 4)	// 2D LATLONG
// 		{
// 			float2 latLongUVs = Latlong(diffuseIBLVec);
// 			float3 preDiffuseIBL = diffuseIBLcolor;
// 			diffuseIBLcolor = pow(DiffuseIBLTexture2D.SampleLevel(SamplerAnisoWrap, latLongUVs, diffuseIBLMipLevel).rgb, gammaCorrection);
// 
// 			if (DiffuseIBLType == 4)	// combine Cube and Latlong
// 				diffuseIBLcolor += preDiffuseIBL;
// 		}
// 
// 		// The Diffuse IBL gets added to what the dynamic lights have already illuminated
// 		// The Diffuse IBL texture should hold diffuse lighting information, so we multiply the diffuseColor (diffuseTexture) by the IBL
// 		// IBL intensity allows the user to specify how much the IBL contributes on top of the dynamic lights
// 		// Also compensate for pre-multiplied alpha
// 		lightTotal += diffuseColor * diffuseIBLcolor * DiffuseIBLIntensity * opacity;
// 	}
// 
// 
// 
// 	// ----------------------
// 	// FINAL COLOR AND ALPHA:
// 	// ----------------------
// 	// ambient must also compensate for pre-multiplied alpha
// 	float3 result = (ambientColor * opacity) + reflectColorTotal;
// 	result += lightTotal;
// 
// 	// do gamma correction in shader:
// 	if (!MayaFullScreenGamma)
// 		result = pow(result, 1/gammaCorrection);
// 
// 	// final alpha:
// 	float transparency = opacity;
// 	if (ReflectionAffectOpacity)
// 	{
// 		float cubeTransparency = dot(saturate(reflectColorTotal), float3(0.3, 0.6, 0.1));
// 		float specTotal = light0.Specular + light1.Specular + light2.Specular;
// 		transparency += (cubeTransparency + specTotal);
// 	}
// 	transparency = saturate(transparency);	// keep 0-1 range
// 
// 	return float4(result, transparency);
}

//------------------------------------
// pixel shader for shadow map generation
//------------------------------------
//float4 PS_ShadowMap( float3 Pw, float4x4 shadowViewProj ) 
float4	PS_ShadowMap( PS_IN _In ) : SV_Target
{ 
	// clip as early as possible
	float2 opacityMaskUV = pickTexcoord(OpacityMaskTexcoord, _In.texCoord0, _In.texCoord1, _In.texCoord2);
	OpacityMaskClip( opacityMaskUV );

	float4	Pndc = mul( float4( _In.worldPosition, 1.0 ), World2Proj ); 

	// divide Z and W component from clip space vertex position to get final depth per pixel
	float retZ = Pndc.z / Pndc.w; 

//	retZ += fwidth(retZ); ??
	return retZ;
} 



//-----------------------------------
// Objects without tessellation
//------------------------------------
technique11 TessellationOFF
<
	bool	overridesDrawState = false;	// we do not supply our own render state settings
	int		isTransparent = 3;

	// objects with clipped pixels need to be flagged as isTransparent to avoid the occluding underlying geometry since Maya renders the object with flat shading when computing depth
	string	transparencyTest = "Opacity < 1.0 || (UseDiffuseTexture && UseDiffuseTextureAlpha) || UseOpacityMaskTexture || OpacityFresnelMax > 0 || OpacityFresnelMin > 0";

	// 'VariableNameAsAttributeName = false' can be used to tell Maya's DX11ShaderNode to use the UIName annotation string for the Maya attribute name instead of the shader variable name.
	// When changing this option, the attribute names generated for the shader inside Maya will change and this can have the side effect that older scenes have their shader attributes reset to default.
//	bool	VariableNameAsAttributeName = false;
>
{  
	pass p0
	< 
		string	drawContext = "colorPass";	// tell maya during what draw context this shader should be active, in this case 'Color'
	>
	{
		// even though overrideDrawState is false, we still set the pre-multiplied alpha state here in case Maya is using 'Depth Peeling' transparency algorithm
		// This unfortunately won't solve sorting issues, but at least our object can draw transparent.
		// If we don't set this, the object will always be opaque.
		// In the future, hopefully ShaderOverride nodes can participate properly in Maya's Depth Peeling setup
		SetBlendState( PreMultipliedAlphaBlending, float4(0.0f, 0.0f, 0.0f, 0.0f), 0xFFFFFFFF );

		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetHullShader( NULL );
		SetDomainShader( NULL );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_5_0, PS() ) );
	}

	pass pShadow
	< 
		string	drawContext = "shadowPass";	// shadow pass
	>
	{
		SetVertexShader( CompileShader( vs_5_0, VS() ) );
		SetHullShader( NULL );
		SetDomainShader( NULL );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_5_0, PS_ShadowMap() ) );
	}
}
