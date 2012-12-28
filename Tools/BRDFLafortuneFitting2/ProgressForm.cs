using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BRDFLafortuneFitting
{
	public partial class ProgressForm : Form
	{
		protected int		m_BRDFComponentIndex = 0;
		public int			BRDFComponentIndex	{ get {return m_BRDFComponentIndex; } set { m_BRDFComponentIndex = value; } }

		protected double	m_Progress = 0;
		public double		Progress	{ get { return m_Progress; } set { m_Progress = value; UpdateProgress(); } }

		public ProgressForm()
		{
			InitializeComponent();
		}

		protected void	UpdateProgress()
		{
			double	Progress = (m_BRDFComponentIndex + m_Progress) / 3.0;
			int		nProgress = (int) (Progress * progressBar.Maximum);
			progressBar.Value = nProgress;

			// Let the application process...
			Application.DoEvents();
		}
	}
}
