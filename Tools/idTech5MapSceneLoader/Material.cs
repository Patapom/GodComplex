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
		public int		m_MaterialIndex = -1;

		public Material( string _SourceFileName ) {

//			string	MaterialFileName = Map.RebaseFileName( _SourceFileName, "T:/generated/m2/", null );

		}

		#region Material Parsing

		private void	Parse( string _Content ) {
			Parser	P = new Parser( _Content );

			// TODO!
		}

		#endregion
	}
}
