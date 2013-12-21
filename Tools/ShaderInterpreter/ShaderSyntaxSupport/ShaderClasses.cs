using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using ShaderInterpreter.ShaderMath;

namespace ShaderInterpreter
{
	public class	SemanticAttribute : Attribute
	{
		public string	m_Semantic;
		public SemanticAttribute( string _Semantic )	{ m_Semantic = _Semantic; }
	}
	public class	RegisterAttribute : Attribute
	{
		public string	m_Register;
		public RegisterAttribute( string _Register )	{ m_Register = _Register; }
	}
	public class	cbufferAttribute : Attribute
	{
		public cbufferAttribute()	{}
	}
}
