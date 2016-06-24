using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MaterialsOptimizer
{
	public partial class Form1 : Form {

		class Error {
			public FileInfo		m_materialFileName;
			public Exception	m_error;

			public override string ToString() {
				return "ERROR! " + m_materialFileName.FullName + " > " + m_error.Message;
			}
		}

		List< Material >	m_materials = new List< Material >();
		List< Error >		m_errors = new List< Error >();

		public Form1()
		{
			InitializeComponent();

ParseFile( new FileInfo( @"V:\Dishonored2\Dishonored2\base\decls\m2\models\environment\buildings\rich_large_ext_partitions_01.m2" ) );

			RecurseParseMaterials( new DirectoryInfo( @"V:\Dishonored2\Dishonored2\base\decls\m2" ) );
		}

		void	RecurseParseMaterials( DirectoryInfo _directory ) {

			FileInfo[]	materialFileNames = _directory.GetFiles( "*.m2", SearchOption.AllDirectories );
			foreach ( FileInfo materialFileName in materialFileNames ) {

				try {
					ParseFile( materialFileName );
				} catch ( Exception _e ) {
					Error	Err = new Error() { m_materialFileName = materialFileName, m_error = _e };
					m_errors.Add( Err );
					textBoxLog.AppendText( Err + "\n" );
				}
			}
		}

		void	ParseFile( FileInfo _fileName ) {
			string	fileContent = null;
			using ( StreamReader R = _fileName.OpenText() )
				fileContent = R.ReadToEnd();

			Parser	P = new Parser( fileContent );
			while ( P.OK ) {
				string	token = P.ReadString();
				switch ( token.ToLower() ) {
					case "material":
						string		materialName = P.ReadString();
						string		materialContent = P.ReadBlock();
						Material	M = new Material( _fileName, materialName, materialContent );
						m_materials.Add( M );
						break;
				}
			}
		}
	}
}
