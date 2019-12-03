// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once
#pragma unmanaged

// Generic includes
#include <math.h>
#include <stdio.h>


//////////////////////////////////////////////////////////////////////////
// FBX SDK
// (available from http://usa.autodesk.com)
// NOTE: You must at least install FBX version 2013.3 as their structures changed deeply from that version.
//
// You must define the global environment variable "FBX_SDK" to use the SDK
// This variable must contain the path to the FBXSDK (for example, if you installed
//	the SDK version 2013.3 to "C:\MyPath\FbxSDK" then use 'set FBX_SDK C:\MyPath\FbxSDK\2013.3')
//
// You will also certainly need to change the input FBX library name in the project settings
//	to specify the one of your version...
// Don't forget to also copy the runtime fbx DLLs to the "Runtime" directory!
// I know, it's annoying but you'll need to do that only once! ^_^
// 

#define FBXSDK_NEW_API

#include <fbxsdk.h>
// #include <fbxfilesdk/kfbxio/kfbximporter.h>
// #include <fbxfilesdk/kfbxplugins/kfbxsdkmanager.h>
// #include <fbxfilesdk/kfbxplugins/kfbxscene.h>
// #include <fbxfilesdk/kfbxio/kfbxiosettings.h>
// #include <fbxfilesdk/fbxfilesdk_nsuse.h>

#pragma managed

// #include "../MathSimple/Math.h"
// #include "../MathSimple/Vectors.h"
