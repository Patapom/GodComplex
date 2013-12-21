using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WMath;

namespace FBX.Scene.Materials
{
	/// <summary>
	/// The Material Parameters class is a serializable class that encapsulates all the parameters needed by a material for a particular primitive.
	/// Some examples of material parameters are the Local2World transform matrix, colors or specular intensity values associated with a primitive.
	/// 
	/// You must make the distinction between material variables that are the variables exposed by a shader to render a particular material,
	///  and the values of these variables which are contained by an instance of this MaterialParameters class.
	/// </summary>
	public class	MaterialParameters
	{
		#region NESTED TYPES

		/// <summary>
		/// A list of the supported parameter types
		/// </summary>
		public enum	PARAMETER_TYPE
		{
			BOOL,
			INT,
			FLOAT,
			FLOAT2,
			FLOAT3,
			FLOAT4,
			MATRIX4,
			TEXTURE2D,
		};

		/// <summary>
		/// Base parameter class
		/// </summary>
		public abstract class	Parameter
		{
			#region FIELDS

			protected MaterialParameters	m_Owner = null;
			protected string				m_Name = null;			// Parameter name

			#endregion

			#region PROPERTIES

			public string					Name		{ get { return m_Name; } }
			public abstract PARAMETER_TYPE	Type		{ get; }

			// Fast casts
			public ParameterBool			AsBool		{ get { return this as ParameterBool; } }
			public ParameterInt				AsInt		{ get { return this as ParameterInt; } }
			public ParameterFloat			AsFloat		{ get { return this as ParameterFloat; } }
			public ParameterFloat2			AsFloat2	{ get { return this as ParameterFloat2; } }
			public ParameterFloat3			AsFloat3	{ get { return this as ParameterFloat3; } }
			public ParameterFloat4			AsFloat4	{ get { return this as ParameterFloat4; } }
			public ParameterMatrix4			AsMatrix4	{ get { return this as ParameterMatrix4; } }
			public ParameterTexture2D		AsTexture2D	{ get { return this as ParameterTexture2D; } }

			#endregion

			#region METHODS

			public	Parameter( MaterialParameters _Owner, string _Name )
			{
				m_Owner = _Owner;
				m_Name = _Name;
			}

			public override string ToString()
			{
				return m_Name;
			}

			public abstract void	Load( System.IO.BinaryReader _Reader );
			public abstract void	Save( System.IO.BinaryWriter _Writer );

			#endregion
		}

		public class	ParameterBool : Parameter
		{
			#region FIELDS

			protected bool		m_bValue = false;

			#endregion

			#region PROPERTIES

			public override PARAMETER_TYPE	Type	{ get { return PARAMETER_TYPE.BOOL; } }
			public bool						Value	{ get { return m_bValue; } set { m_bValue = value; } }

			#endregion

			#region METHODS

			public	ParameterBool( MaterialParameters _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			public override string ToString()
			{
				return base.ToString() + " " + m_bValue;
			}

			public override void	Load( System.IO.BinaryReader _Reader )
			{
				m_bValue = _Reader.ReadBoolean();
			}

			public override void	Save( System.IO.BinaryWriter _Writer )
			{
				_Writer.Write( m_bValue );
			}

			public override bool	Equals( object _Other )
			{
				ParameterBool	Other = _Other as ParameterBool;
				if ( Other == null )
					throw new Exception( "Other parameter is not of type FLOAT !" );

				return Other.m_bValue == m_bValue;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			#endregion
		}

		public class	ParameterInt : Parameter
		{
			#region FIELDS

			protected int			m_Value = 0;

			#endregion

			#region PROPERTIES

			public override PARAMETER_TYPE	Type	{ get { return PARAMETER_TYPE.INT; } }
			public int						Value	{ get { return m_Value; } set { m_Value = value; } }

			#endregion

			#region METHODS

			public	ParameterInt( MaterialParameters _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			public override string ToString()
			{
				return base.ToString() + " " + m_Value;
			}

			public override void	Load( System.IO.BinaryReader _Reader )
			{
				m_Value = _Reader.ReadInt32();
			}

			public override void	Save( System.IO.BinaryWriter _Writer )
			{
				_Writer.Write( m_Value );
			}

			public override bool	Equals( object _Other )
			{
				ParameterInt	Other = _Other as ParameterInt;
				if ( Other == null )
					throw new Exception( "Other parameter is not of type FLOAT !" );

				return Other.m_Value == m_Value;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			#endregion
		}

		public class	ParameterFloat : Parameter
		{
			#region FIELDS

			protected float			m_Value = 0.0f;

			#endregion

			#region PROPERTIES

			public override PARAMETER_TYPE	Type	{ get { return PARAMETER_TYPE.FLOAT; } }
			public float					Value	{ get { return m_Value; } set { m_Value = value; } }

			#endregion

			#region METHODS

			public	ParameterFloat( MaterialParameters _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			public override string ToString()
			{
				return base.ToString() + " " + m_Value;
			}

			public override void	Load( System.IO.BinaryReader _Reader )
			{
				m_Value = _Reader.ReadSingle();
			}

			public override void	Save( System.IO.BinaryWriter _Writer )
			{
				_Writer.Write( m_Value );
			}

			public override bool	Equals( object _Other )
			{
				ParameterFloat	Other = _Other as ParameterFloat;
				if ( Other == null )
					throw new Exception( "Other parameter is not of type FLOAT !" );

				return Other.m_Value == m_Value;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			#endregion
		}

		public class	ParameterFloat2 : Parameter
		{
			#region FIELDS

			protected Vector2D		m_Value = Vector2D.Zero;

			#endregion

			#region PROPERTIES

			public override PARAMETER_TYPE	Type	{ get { return PARAMETER_TYPE.FLOAT2; } }
			public Vector2D					Value	{ get { return m_Value; } set { m_Value = value; } }

			#endregion

			#region METHODS

			public	ParameterFloat2( MaterialParameters _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			public override string ToString()
			{
				return base.ToString() + " " + m_Value;
			}

			public override void	Load( System.IO.BinaryReader _Reader )
			{
				m_Value.X = _Reader.ReadSingle();
				m_Value.Y = _Reader.ReadSingle();
			}

			public override void	Save( System.IO.BinaryWriter _Writer )
			{
				_Writer.Write( m_Value.X );
				_Writer.Write( m_Value.Y );
			}

			public override bool	Equals( object _Other )
			{
				ParameterFloat2	Other = _Other as ParameterFloat2;
				if ( Other == null )
					throw new Exception( "Other parameter is not of type FLOAT2 !" );

				return Other.m_Value == m_Value;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			#endregion
		}

		public class	ParameterFloat3 : Parameter
		{
			#region FIELDS

			protected Vector		m_Value = Vector.Zero;

			#endregion

			#region PROPERTIES

			public override PARAMETER_TYPE	Type	{ get { return PARAMETER_TYPE.FLOAT3; } }
			public Vector					Value	{ get { return m_Value; } set { m_Value = value; } }

			#endregion

			#region METHODS

			public	ParameterFloat3( MaterialParameters _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			public override string ToString()
			{
				return base.ToString() + " " + m_Value;
			}

			public override void	Load( System.IO.BinaryReader _Reader )
			{
				m_Value.X = _Reader.ReadSingle();
				m_Value.Y = _Reader.ReadSingle();
				m_Value.Z = _Reader.ReadSingle();
			}

			public override void	Save( System.IO.BinaryWriter _Writer )
			{
				_Writer.Write( m_Value.X );
				_Writer.Write( m_Value.Y );
				_Writer.Write( m_Value.Z );
			}

			public override bool	Equals( object _Other )
			{
				ParameterFloat3	Other = _Other as ParameterFloat3;
				if ( Other == null )
					throw new Exception( "Other parameter is not of type FLOAT3 !" );

				return Other.m_Value == m_Value;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			#endregion
		}

		public class	ParameterFloat4 : Parameter
		{
			#region FIELDS

			protected Vector4D		m_Value = Vector4D.Zero;

			#endregion

			#region PROPERTIES

			public override PARAMETER_TYPE	Type	{ get { return PARAMETER_TYPE.FLOAT4; } }
			public Vector4D					Value	{ get { return m_Value; } set { m_Value = value; } }

			#endregion

			#region METHODS

			public	ParameterFloat4( MaterialParameters _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			public override string ToString()
			{
				return base.ToString() + " " + m_Value;
			}

			public override void	Load( System.IO.BinaryReader _Reader )
			{
				m_Value.X = _Reader.ReadSingle();
				m_Value.Y = _Reader.ReadSingle();
				m_Value.Z = _Reader.ReadSingle();
				m_Value.W = _Reader.ReadSingle();
			}

			public override void	Save( System.IO.BinaryWriter _Writer )
			{
				_Writer.Write( m_Value.X );
				_Writer.Write( m_Value.Y );
				_Writer.Write( m_Value.Z );
				_Writer.Write( m_Value.W );
			}

			public override bool	Equals( object _Other )
			{
				ParameterFloat4	Other = _Other as ParameterFloat4;
				if ( Other == null )
					throw new Exception( "Other parameter is not of type FLOAT4 !" );

				return Other.m_Value == m_Value;
			}
			
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			#endregion
		}

		public class	ParameterMatrix4 : Parameter
		{
			#region FIELDS

			protected Matrix4x4		m_Value = Matrix4x4.Identity;

			#endregion

			#region PROPERTIES

			public override PARAMETER_TYPE	Type	{ get { return PARAMETER_TYPE.MATRIX4; } }
			public Matrix4x4				Value	{ get { return m_Value; } set { m_Value = value; } }

			#endregion

			#region METHODS

			public	ParameterMatrix4( MaterialParameters _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			public override string ToString()
			{
				return base.ToString() + " " + m_Value;
			}

			public override void	Load( System.IO.BinaryReader _Reader )
			{
				m_Value.m[0,0] = _Reader.ReadSingle();
				m_Value.m[0,1] = _Reader.ReadSingle();
				m_Value.m[0,2] = _Reader.ReadSingle();
				m_Value.m[0,3] = _Reader.ReadSingle();
				m_Value.m[1,0] = _Reader.ReadSingle();
				m_Value.m[1,1] = _Reader.ReadSingle();
				m_Value.m[1,2] = _Reader.ReadSingle();
				m_Value.m[1,3] = _Reader.ReadSingle();
				m_Value.m[2,0] = _Reader.ReadSingle();
				m_Value.m[2,1] = _Reader.ReadSingle();
				m_Value.m[2,2] = _Reader.ReadSingle();
				m_Value.m[2,3] = _Reader.ReadSingle();
				m_Value.m[3,0] = _Reader.ReadSingle();
				m_Value.m[3,1] = _Reader.ReadSingle();
				m_Value.m[3,2] = _Reader.ReadSingle();
				m_Value.m[3,3] = _Reader.ReadSingle();
			}

			public override void	Save( System.IO.BinaryWriter _Writer )
			{
				_Writer.Write( m_Value.m[0,0] );
				_Writer.Write( m_Value.m[0,1] );
				_Writer.Write( m_Value.m[0,2] );
				_Writer.Write( m_Value.m[0,3] );
				_Writer.Write( m_Value.m[1,0] );
				_Writer.Write( m_Value.m[1,1] );
				_Writer.Write( m_Value.m[1,2] );
				_Writer.Write( m_Value.m[1,3] );
				_Writer.Write( m_Value.m[2,0] );
				_Writer.Write( m_Value.m[2,1] );
				_Writer.Write( m_Value.m[2,2] );
				_Writer.Write( m_Value.m[2,3] );
				_Writer.Write( m_Value.m[3,0] );
				_Writer.Write( m_Value.m[3,1] );
				_Writer.Write( m_Value.m[3,2] );
				_Writer.Write( m_Value.m[3,3] );
			}

			public override bool	Equals( object _Other )
			{
				ParameterMatrix4	Other = _Other as ParameterMatrix4;
				if ( Other == null )
					throw new Exception( "Other parameter is not of type MATRIX4 !" );

				return Other.m_Value == m_Value;
			}
			
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			#endregion
		}

		public class	ParameterTexture2D : Parameter
		{
			#region FIELDS

			protected Texture2D	m_Value = null;

			#endregion

			#region PROPERTIES

			public override PARAMETER_TYPE	Type	{ get { return PARAMETER_TYPE.TEXTURE2D; } }
			public Texture2D		Value
			{
				get { return m_Value; }
				set
				{
					if ( value == m_Value )
						return;	// No change...

					if ( m_Value != null )
						m_Owner.RemoveTextureParameter( m_Value );

					m_Value = value;

					if ( m_Value != null )
						m_Owner.AddTextureParameter( m_Value );
				}
			}

			#endregion

			#region METHODS

			public	ParameterTexture2D( MaterialParameters _Owner, string _Name ) : base( _Owner, _Name )
			{
			}

			public override string ToString()
			{
				return base.ToString() + " " + m_Value;
			}

			public override void	Load( System.IO.BinaryReader _Reader )
			{
				int	TextureID = _Reader.ReadInt32();
				Value = m_Owner.m_Owner.FindTexture( TextureID );
			}

			public override void	Save( System.IO.BinaryWriter _Writer )
			{
				_Writer.Write( (int) (m_Value != null ? m_Value.ID : -1) );
			}

			public override bool	Equals( object _Other )
			{
				ParameterTexture2D	Other = _Other as ParameterTexture2D;
				if ( Other == null )
					throw new Exception( "Other parameter is not of type TEXTURE2D !" );

				return Other.m_Value == m_Value;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			#endregion
		}

		#endregion

		#region FIELDS

		protected Scene							m_Owner = null;
		protected int							m_ID = -1;
		protected string						m_Name = "";
		protected string						m_ShaderURL = "";
		protected List<Parameter>				m_Parameters = new List<Parameter>();
		protected Dictionary<string,Parameter>	m_Name2Parameter = new Dictionary<string,Parameter>();

		// Cached data
		protected List<Texture2D>				m_TextureParameters = new List<Texture2D>();

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Gets the ID
		/// </summary>
		public int				ID					{ get { return m_ID; } }

		/// <summary>
		/// Gets its name
		/// </summary>
		public string			Name				{ get { return m_Name; } }

		/// <summary>
		/// Gets the shader URL
		/// </summary>
		public string			ShaderURL			{ get { return m_ShaderURL; } }

		/// <summary>
		/// Gets the list of parameters
		/// </summary>
		public Parameter[]		Parameters			{ get { return m_Parameters.ToArray(); } }

		/// <summary>
		/// Gets the list of textures from Texture Parameters
		/// </summary>
		public Texture2D[]		TexturesFromParameters	{ get { return m_TextureParameters.ToArray(); } }

		#endregion

		#region METHODS

		internal	MaterialParameters( Scene _Owner, int _ID, string _Name, string _ShaderURL )
		{
			m_Owner = _Owner;
			m_ID = _ID;
			m_Name = _Name != null ? _Name : "";
			m_ShaderURL = _ShaderURL != null ? _ShaderURL : "";
		}

		/// <summary>
		/// Loads parameters from a stream
		/// </summary>
		/// <param name="_Owner"></param>
		/// <param name="_Reader"></param>
		internal	MaterialParameters( Scene _Owner, System.IO.BinaryReader _Reader )
		{
			m_Owner = _Owner;

			m_ID = _Reader.ReadInt32();
			m_Name = _Reader.ReadString();
			m_ShaderURL = _Reader.ReadString();

			int	ParametersCount = _Reader.ReadInt32();
			for ( int ParameterIndex=0; ParameterIndex < ParametersCount; ParameterIndex++ )
			{
				string			Name = _Reader.ReadString();
				PARAMETER_TYPE	Type = (PARAMETER_TYPE) _Reader.ReadByte();

				Parameter		NewParam = CreateParameter( Name, Type );

				// De-serialize the parameter
				NewParam.Load( _Reader );
			}
		}

		public override string ToString()
		{
			return m_Name + " (" + m_Parameters.Count + " Parameters)";
		}

		/// <summary>
		/// Creates a new parameter
		/// </summary>
		/// <param name="_Name"></param>
		/// <param name="_Type"></param>
		/// <returns></returns>
		public Parameter	CreateParameter( string _Name, PARAMETER_TYPE _Type )
		{
			Parameter	Result = null;
			switch ( _Type )
			{
				case PARAMETER_TYPE.BOOL:
					Result = new ParameterBool( this, _Name );
					break;
				case PARAMETER_TYPE.INT:
					Result = new ParameterInt( this, _Name );
					break;
				case PARAMETER_TYPE.FLOAT:
					Result = new ParameterFloat( this, _Name );
					break;
				case PARAMETER_TYPE.FLOAT2:
					Result = new ParameterFloat2( this, _Name );
					break;
				case PARAMETER_TYPE.FLOAT3:
					Result = new ParameterFloat3( this, _Name );
					break;
				case PARAMETER_TYPE.FLOAT4:
					Result = new ParameterFloat4( this, _Name );
					break;
				case PARAMETER_TYPE.MATRIX4:
					Result = new ParameterMatrix4( this, _Name );
					break;
				case PARAMETER_TYPE.TEXTURE2D:
					Result = new ParameterTexture2D( this, _Name );
					break;

				default:
					throw new Exception( "Unsupported parameter type !" );
			}

			// Add it...
			m_Parameters.Add( Result );
			m_Name2Parameter.Add( Result.Name, Result );

			return Result;
		}

		/// <summary>
		/// Finds a parameter by name
		/// </summary>
		/// <param name="_ParameterName"></param>
		/// <returns></returns>
		public Parameter	Find( string _ParameterName )
		{
			return m_Name2Parameter.ContainsKey( _ParameterName ) ? m_Name2Parameter[_ParameterName] : null;
		}

		/// <summary>
		/// Clears all registered parameters
		/// </summary>
		public void			ClearParameters()
		{
			m_Parameters.Clear();
			m_Name2Parameter.Clear();
		}

		/// <summary>
		/// Saves parameters to a stream
		/// </summary>
		/// <param name="_Stream"></param>
		internal void		Save( System.IO.BinaryWriter _Writer )
		{
			_Writer.Write( m_ID );
			_Writer.Write( m_Name );
			_Writer.Write( m_ShaderURL );
			_Writer.Write( m_Parameters.Count );
			foreach ( Parameter Param in m_Parameters )
			{
				_Writer.Write( Param.Name );
				_Writer.Write( (byte) Param.Type );
				Param.Save( _Writer );
			}
		}

		/// <summary>
		///  Adds a texture parameter (called by one of our TextureParameter which was assigned a texture)
		/// </summary>
		/// <param name="_Texture"></param>
		protected void	AddTextureParameter( Texture2D _Texture )
		{
			if ( _Texture == null )
				return;

			m_TextureParameters.Add( _Texture );
		}

		/// <summary>
		///  Removes a texture parameter (called by one of our TextureParameter which was assigned a texture)
		/// </summary>
		/// <param name="_Texture"></param>
		protected void	RemoveTextureParameter( Texture2D _Texture )
		{
			if ( _Texture == null || !m_TextureParameters.Contains( _Texture ) )
				return;

			m_TextureParameters.Remove( _Texture );
		}

		#endregion
	}

}
