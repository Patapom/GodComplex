using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FBX.Scene.Materials
{
	/// <summary>
	/// The texture 2D class wraps a standard Nuaj texture and attaches a unique URL and ID to it so it can be serialized
	/// </summary>
	public class	Texture2D
	{
		#region FIELDS

		protected Scene					m_Owner = null;
		protected int					m_ID = -1;
		protected string				m_URL = null;
		protected string				m_OpacityURL = null;

		#endregion

		#region PROPERTIES

		public int				ID				{ get { return m_ID; } }
		public string			URL				{ get { return m_URL; } }

		#endregion

		#region METHODS

		internal Texture2D( Scene _Owner, int _ID, string _URL, string _OpacityURL, bool _bCreateMipMaps )
		{
			m_Owner = _Owner;
			m_ID = _ID;
			m_URL = _URL != null ? _URL : "";
			m_OpacityURL = _OpacityURL != null ? _OpacityURL : "";
		}

		internal Texture2D( Scene _Owner, System.IO.BinaryReader _Reader )
		{
			m_Owner = _Owner;
			m_ID = _Reader.ReadInt32();
			m_URL = _Reader.ReadString();
			m_OpacityURL = _Reader.ReadString();
		}

		public override string ToString()
		{
			return m_URL;
		}

		internal void	Save( System.IO.BinaryWriter _Writer )
		{
			_Writer.Write( m_ID );
			_Writer.Write( m_URL );
			_Writer.Write( m_OpacityURL );
		}

		#endregion
	}
}
