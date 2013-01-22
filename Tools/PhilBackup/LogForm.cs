using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace PhilBackup
{
	public partial class LogForm : Form
	{
		Dictionary<FileInfo,string>		m_Errors = null;
		public Dictionary<FileInfo,string>		Errors
		{
			get { return m_Errors; }
			set
			{
				if ( value == m_Errors )
					return;
				if ( value == null )
					return;

				m_Errors = value;

				listBoxErrors.BeginUpdate();
				listBoxErrors.Items.Clear();
				foreach ( FileInfo File in m_Errors.Keys )
				{
					string	ErrorLine = File.FullName + " => " + m_Errors[File]; 
					listBoxErrors.Items.Add( ErrorLine );
				}
				listBoxErrors.EndUpdate();
			}
		}

		public LogForm()
		{
			InitializeComponent();
		}
	}
}
