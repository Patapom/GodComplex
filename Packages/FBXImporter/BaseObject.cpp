// This is the main DLL file.

#include "stdafx.h"

#include "BaseObject.h"

using namespace	FBXImporter;
using namespace System::Reflection;

String^	BaseObject::ListProperties()
{
	String^	Result = "";
	Type^	T = GetType();
	cli::array<PropertyInfo^>^	Properties = T->GetProperties( BindingFlags::Instance | BindingFlags::Public );
	for ( int PropertyIndex=0; PropertyIndex < Properties->Length; PropertyIndex++ )
	{
		PropertyInfo^			PropInfo = Properties[PropertyIndex];

		BrowsableAttribute^		Browsable = nullptr;
		DescriptionAttribute^	Description = nullptr;
		cli::array<Object^>^	CustomAttributes = PropInfo->GetCustomAttributes( true );
		for ( int AttribIndex=0; AttribIndex < CustomAttributes->Length; AttribIndex++ )
		{
			Attribute^	Att = dynamic_cast<Attribute^>( CustomAttributes[AttribIndex] );

			if ( Browsable == nullptr )
				Browsable = dynamic_cast<BrowsableAttribute^>( Att );
			if ( Description == nullptr )
				Description = dynamic_cast<DescriptionAttribute^>( Att );
		}

		if ( Browsable!= nullptr &&!Browsable->Browsable )
			continue;

		// Format property
		String^		PropText = PropInfo->Name;
		PropText += " (" + PropInfo->PropertyType->Name + ") ";
		if ( PropInfo->CanRead )
			PropText += "R";
		if ( PropInfo->CanWrite )
			PropText += "W";
		if ( Description!= nullptr )
			PropText += "	Desc: " + Description->Description;

		Result += PropText + "\r\n";
	}

	return	Result;
}

String^	BaseObject::ListMethods()
{
	String^	Result = "";
	Type^	T = GetType();
	cli::array<MethodInfo^>^	Methods = T->GetMethods( BindingFlags::Instance | BindingFlags::Public );
	for ( int MethodIndex=0; MethodIndex < Methods->Length; MethodIndex++ )
	{
		MethodInfo^		MethInfo = Methods[MethodIndex];
		if ( MethInfo->Name->StartsWith( "get_" ) || MethInfo->Name->StartsWith( "set_" ) )
			continue;	// Certainly a property accessor method...

		BrowsableAttribute^		Browsable = nullptr;
		DescriptionAttribute^	Description = nullptr;
		cli::array<Object^>^	CustomAttributes = MethInfo->GetCustomAttributes( true );
		for ( int AttribIndex=0; AttribIndex < CustomAttributes->Length; AttribIndex++ )
		{
			Attribute^	Att = dynamic_cast<Attribute^>( CustomAttributes[AttribIndex] );

			if ( Browsable == nullptr )
				Browsable = dynamic_cast<BrowsableAttribute^>( Att );
			if ( Description == nullptr )
				Description = dynamic_cast<DescriptionAttribute^>( Att );
		}

		if ( Browsable!= nullptr &&!Browsable->Browsable )
			continue;

		// Format property
		String^		PropText = MethInfo->ReturnType->Name + " " + MethInfo->Name;
		PropText += "(";
		cli::array<ParameterInfo^>^	Params = MethInfo->GetParameters();
		for ( int ParamIndex=0; ParamIndex < Params->Length; ParamIndex++ )
		{
			ParameterInfo^	Param = Params[ParamIndex];
			PropText += (ParamIndex!= 0 ? ", " : "") + Param->ParameterType->Name + " " + Param->Name;
		}
		PropText += " )";
		if ( Description!= nullptr )
			PropText += "	Desc: " + Description->Description;

		Result += PropText + "\r\n";
	}

	return	Result;
}
