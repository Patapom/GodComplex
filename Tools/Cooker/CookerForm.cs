using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Cooker
{
	public partial class CookerForm : Form
	{
		private class	AbortException : Exception
		{
		}

		private class	MyConsoleWriter : TextWriter
		{
			public RichTextBox	m_Target;

			public override Encoding Encoding
			{
				get { return Encoding.UTF8; }
			}

			public override void Write( string s )
			{
				m_Target.AppendText( s );
			}
		}
		private MyConsoleWriter	m_Writer = new MyConsoleWriter();

		private FileInfo	m_MapFile = null;
		private FileInfo	MapFile
		{
			get { return m_MapFile; }
			set
			{
				if ( value == null )
					return;

				m_MapFile = value;
				buttonCook.Enabled = m_MapFile.Exists;
				textBoxMapName.Text = m_MapFile.FullName;
			}
		}

		public CookerForm()
		{
			InitializeComponent();

			m_MapFile = new FileInfo( "c:" );
			comboBoxPlatform.SelectedIndex = 1;	// ORBIS

			// Setup the process
			processCook.StartInfo.RedirectStandardError = true;
			processCook.StartInfo.RedirectStandardOutput = true;
 			processCook.StartInfo.UseShellExecute = false;

			m_Writer.m_Target = richTextBoxOutput;
			Console.SetOut( m_Writer );
			Console.SetError( m_Writer );
		}

		private void	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			System.Windows.Forms.MessageBox.Show( this, _Text, "Map Cooker", _Buttons, _Icon );
		}

		private void buttonLoadMap_Click(object sender, EventArgs e)
		{
			if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			FileInfo	SelectedMapFile = new FileInfo( openFileDialog.FileName );
			if ( !SelectedMapFile.Exists )
			{
				MessageBox( "Selected map file \"" + SelectedMapFile.FullName + "\" does not exist!", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			MapFile = SelectedMapFile;
		}

		private void comboBoxPlatform_SelectedIndexChanged(object sender, EventArgs e)
		{
		
		}

		private bool	m_Cooking = false;
		private bool	m_Abort = false;
		private void buttonCook_Click(object sender, EventArgs e)
		{
			if ( m_Cooking )
			{	// User pressed abort
				m_Abort = true;
				return;
			}

			m_Cooking = true;	// Prevent re-entrance!

			System.Diagnostics.Process	P = processCook;

			// Invalidate input panel and ready cancel button
			panelInput.Enabled = false;
			buttonCook.Text = "Cancel";

			try
			{
				FileInfo	ApplicationFileName = new FileInfo( @"V:\blacksparrow\idtech5\blacksparrow\Blacksparrowx64.exe" );
				if ( !ApplicationFileName.Exists )
					throw new Exception( "Application Blacksparrow64.exe could not be found while looking at \"" + ApplicationFileName.FullName + "\"! Did you build it?" );

				DirectoryInfo	ApplicationDirectory = ApplicationFileName.Directory;
				DirectoryInfo	MapsDirectory = new DirectoryInfo( Path.Combine( ApplicationDirectory.FullName, "base\\maps" ) );

				// Strip map from base path
				string	MapRelativeFileName = MapFile.FullName;
						MapRelativeFileName = MapRelativeFileName.Replace( MapsDirectory.FullName+"\\", "" );	// Remove absolute

				string	PlatformArg = null;
				switch ( comboBoxPlatform.SelectedIndex )
				{
					case 0:	PlatformArg = "-PC"; break;
					case 1:	PlatformArg = "-ORBIS"; break;
					case 2:	PlatformArg = "-DURANGO"; break;
					default:
						throw new Exception( "Unsupported platform type " + comboBoxPlatform.SelectedIndex + "!" );
				}

				P.StartInfo.FileName = ApplicationFileName.FullName;
				P.StartInfo.Arguments = textBoxCommandLine.Text + " " + PlatformArg + " " + MapRelativeFileName;
				P.StartInfo.WorkingDirectory = ApplicationDirectory.FullName;

				// Let's go !!
				P.Start();

				// Hook to outputs
// 				if ( P.StartInfo.RedirectStandardOutput && !P.StartInfo.UseShellExecute )
// 				{
// 					P.BeginErrorReadLine();
// 					P.BeginOutputReadLine();
// 				}

				DateTime	LastKeepAliveTime = DateTime.Now;
				while ( !P.HasExited )
				{
					if ( m_Abort )
						throw new AbortException();

// 					if ( (DateTime.Now - LastKeepAliveTime).TotalSeconds > KEEP_ALIVE_DELAY )
// 					{	// Send keep alive status
// 						LastKeepAliveTime = DateTime.Now;
// 						m_EndPoint.WriteStream( END_POINT_STATUS.KEEP_ALIVE, "Still alive !" );
// 					}

					Thread.Sleep( 1000 * 1 );	// Poll every 1 seconds...
					Application.DoEvents();

// 					if ( !Focused )
// 						Focus();
				}

// 				StartTime = P.StartTime;
// 				EndTime = P.ExitTime;
// 				ExitCode = (APPLICATION_RESULT_CODE) P.ExitCode;

				MessageBox( "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information );
			}
			catch ( AbortException )
			{
				MessageBox( "Aborted!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
			}
			catch ( Exception _e )
			{
				MessageBox( "The rendering process generated an exception!\r\n\r\n" + _e, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			finally
			{
// 				string	Pouet = P.StandardOutput.ReadToEnd();
// 				string	Pouet2 = P.StandardError.ReadToEnd();
// 
// 				Pouet += "glou";
// 				Pouet2 += "glou";

				if ( !P.HasExited )
					P.Kill();
				P.Dispose();
			}

			// Restore input panel and cook button
			panelInput.Enabled = true;
			buttonCook.Text = "Cook";

			// Restore cooking state
			m_Cooking = m_Abort = false;
		}

		private void processCook_OutputDataReceived( object sender, System.Diagnostics.DataReceivedEventArgs e )
		{
			if ( e.Data == null )
				return;

			richTextBoxOutput.AppendText( e.Data );
		}

		private void processCook_ErrorDataReceived( object sender, System.Diagnostics.DataReceivedEventArgs e )
		{
			if ( e.Data == null )
				return;

			richTextBoxOutput.AppendText( "<ERROR> " + e.Data );
		}
	}
}
