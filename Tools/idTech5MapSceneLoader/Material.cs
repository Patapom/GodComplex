using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using RendererManaged;

namespace idTech5Map
{
	public class Material
	{
		public int		m_MaterialIndex = 0;

		public Material( string _SourceFileName, int _ID ) {
			m_MaterialIndex = _ID;

//			string	MaterialFileName = Map.RebaseFileName( _SourceFileName, "T:/generated/m2/", null );

		}

		#region Material Parsing

		private void	Parse( string _Content ) {
			Parser	P = new Parser( _Content );

			// TODO!
		}

		#endregion

		#region Materials Database

		public static List< Material >					ms_Materials = new List< Material >();
		public static Dictionary< string, Material >	ms_Name2Material = new Dictionary< string, Material >();
		public static Material	Find( string _SourceFileName ) {
			if ( _SourceFileName == null )
				return null;

			_SourceFileName = _SourceFileName.ToLower();
			if ( ms_Name2Material.ContainsKey( _SourceFileName ) )
				return ms_Name2Material[_SourceFileName];

			// Create a new material
			Material	M = new Material( _SourceFileName, ms_Name2Material.Count );
			ms_Name2Material.Add( _SourceFileName, M );
			ms_Materials.Add( M );	// In order
			return M;
		}

		#endregion
	}
}
