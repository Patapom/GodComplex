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
using Microsoft.Win32;

// +fs_basepath V:\blacksparrow\idtech5\blacksparrow +buildgame +ark_useStdOut 1

namespace Cooker
{
	public partial class CookerForm : Form
	{
		private RegistryKey	m_AppKey;

		private class	AbortException : Exception
		{
		}

		private FileInfo	m_MapFile = new FileInfo( @"V:\Dishonored2\Dishonored2\base\maps\dummy.map" );
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

				// Update registry key
				m_AppKey.SetValue( "LastSelectedmap", m_MapFile.FullName );
			}
		}

		private string		m_ExecutablePath = @"V:\Dishonored2\Dishonored2\Dishonored2_x64.exe";
		private string		m_BasePath = @"V:\Dishonored2\Dishonored2";

		private string		m_CustomExecutablePath = @"V:\Dishonored2\Dishonored2\Dishonored2_x64.exe";

		public CookerForm()
		{
			InitializeComponent();

			m_AppKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\Arkane\MapCooker" );

			// Retrieve previous map
			string	MapName = m_AppKey.GetValue( "LastSelectedmap", m_MapFile.FullName ) as string;
			MapFile = new FileInfo( MapName );

			// Retrieve previous platform selection
			int		PlatformIndex = 0;
			string	PlatformIndexAsString = m_AppKey.GetValue( "Platform" ) as string;
			int.TryParse( PlatformIndexAsString, out PlatformIndex );

			comboBoxPlatform.SelectedIndex = PlatformIndex;

			// Retrieve previous executable selection
			int		ExecutableIndex = 2;
			string	ExecutableIndexAsString = m_AppKey.GetValue( "Executable" ) as string;
			int.TryParse( ExecutableIndexAsString, out ExecutableIndex );

			m_CustomExecutablePath = m_AppKey.GetValue( "CustomExecutablepath", m_CustomExecutablePath ) as string;

			comboBoxExecutable.SelectedIndex = ExecutableIndex;
			comboBoxExecutable_SelectedIndexChanged( comboBoxExecutable, EventArgs.Empty );

			// Retrieve previous base path
			m_BasePath = m_AppKey.GetValue( "BasePath", m_BasePath ) as string;
			textBoxBasePath.Text = m_BasePath;

			// Retrieve previous command line
			string	CommandLine = m_AppKey.GetValue( "CommandLine" ) as string;
			if ( CommandLine != null )
				textBoxCommandLine.Text = CommandLine;
			else
			{
				string	Backup = textBoxCommandLine.Text;
				textBoxCommandLine.Text = "";
				textBoxCommandLine.Text = Backup;
			}

			// Setup the process
			processCook.StartInfo.RedirectStandardError = true;
			processCook.StartInfo.RedirectStandardOutput = true;
 			processCook.StartInfo.UseShellExecute = false;
		}

		private void	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			System.Windows.Forms.MessageBox.Show( this, _Text, "Map Cooker", _Buttons, _Icon );
		}

		private void buttonLoadMap_Click(object sender, EventArgs e)
		{
			string	MapName = m_AppKey.GetValue( "LastSelectedmap", m_MapFile.FullName ) as string;
			openFileDialogMap.FileName = Path.GetFileName( MapName );
			openFileDialogMap.InitialDirectory = Path.GetDirectoryName( MapName );
			if ( openFileDialogMap.ShowDialog( this ) != DialogResult.OK )
				return;

			FileInfo	SelectedMapFile = new FileInfo( openFileDialogMap.FileName );
			if ( !SelectedMapFile.Exists )
			{
				MessageBox( "Selected map file \"" + SelectedMapFile.FullName + "\" does not exist!", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}

			MapFile = SelectedMapFile;
		}

		private void comboBoxPlatform_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Update registry
			m_AppKey.SetValue( "Platform", comboBoxPlatform.SelectedIndex.ToString() );
			textBoxFinalCommandLine.Text = GenerateCommandLine();
		}

		private void textBoxCommandLine_TextChanged( object sender, EventArgs e )
		{
			// Update registry
			m_AppKey.SetValue( "CommandLine", textBoxCommandLine.Text );
			textBoxFinalCommandLine.Text = GenerateCommandLine();
		}

		/// <summary>
		/// Generates the final command line used for cooking
		/// </summary>
		/// <returns></returns>
		private string	GenerateCommandLine()
		{
			// ORBIS COMMAND LINE (as of 12/08/2014):
			// Running : "V:\blacksparrow\idtech5\blacksparrow\Blacksparrowx64.exe" +win_silentCrash 1 +com_assertOutOfDebugger  1 +r_fullscreen 0 +ark_usestdout 1 +win_crashDmp_enable 1
			//			+com_crashProcessOnError 1 +fs_basepath "V:\blacksparrow\idtech5\blacksparrow" +r_imagesMaxMipmapsNbr 4 +com_production 1
			//			+buildgame -orbis -fast    LIST OF MAPS (e.g.: checkmap/dojo_routine/dojo_routine.map)
			//
			DirectoryInfo	WorkingDirectory = new DirectoryInfo( m_BasePath );
			DirectoryInfo	MapsDirectory = new DirectoryInfo( Path.Combine( WorkingDirectory.FullName, "base\\maps" ) );

			// Strip map from base path
			string	MapRelativeFileName = MapFile.FullName;
					MapRelativeFileName = MapRelativeFileName.Replace( MapsDirectory.FullName + "\\", "" );	// Remove absolute

			string	PlatformArg = null;
			switch ( comboBoxPlatform.SelectedIndex )
			{
				case 0:	PlatformArg = "-pc"; break;
				case 1:	PlatformArg = "-orbis"; break;
				case 2:	PlatformArg = "-durango"; break;
				default:
					throw new Exception( "Unsupported platform type " + comboBoxPlatform.SelectedIndex + "!" );
			}

			string	CommandLine = textBoxCommandLine.Text + " +com_production 1 +ark_useStdOut 1 +fs_basepath \"" + m_BasePath + "\" +buildgame -fast " + PlatformArg + " " + MapRelativeFileName;
			return CommandLine;
		}

		private bool	m_Cooking = false;
		private bool	m_Abort = false;
		private bool	m_ForceSuccess = false;
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
			richTextBoxOutput.Clear();

			try
			{
				FileInfo	ApplicationFileName = new FileInfo( m_ExecutablePath );
				if ( !ApplicationFileName.Exists )
					throw new Exception( "Application Blacksparrow64.exe could not be found while looking at \"" + ApplicationFileName.FullName + "\"! Did you build it?" );

				P.StartInfo.FileName = ApplicationFileName.FullName;
				P.StartInfo.Arguments = GenerateCommandLine();
				P.StartInfo.WorkingDirectory = m_BasePath;
				P.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;	// Start minimized

				// Let's go !!
				P.Start();

				// Hook to outputs
				if ( P.StartInfo.RedirectStandardOutput && !P.StartInfo.UseShellExecute )
				{
					P.BeginErrorReadLine();
					P.BeginOutputReadLine();
				}

				DateTime	LastKeepAliveTime = DateTime.Now;
				while ( !P.HasExited )
				{
					if ( m_Abort )
						throw new AbortException();

					// We met success!
					if ( m_ForceSuccess )
						P.Kill();

// 					if ( (DateTime.Now - LastKeepAliveTime).TotalSeconds > KEEP_ALIVE_DELAY )
// 					{	// Send keep alive status
// 						LastKeepAliveTime = DateTime.Now;
// 						m_EndPoint.WriteStream( END_POINT_STATUS.KEEP_ALIVE, "Still alive !" );
// 					}

//					Thread.Sleep( 10 );	// Poll every 0.01 seconds...
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
				P.Kill();
				MessageBox( "Aborted!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
			}
			catch ( Exception _e )
			{
				MessageBox( "The rendering process generated an exception!\r\n\r\n" + _e, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			finally
			{
				if ( !P.HasExited )
					P.Kill();

				// Unhook from outputs
				if ( P.StartInfo.RedirectStandardOutput && !P.StartInfo.UseShellExecute )
				{
					P.CancelErrorRead();
					P.CancelOutputRead();
				}

// 				P.Dispose();
			}

			// Restore input panel and cook button
			panelInput.Enabled = true;
			buttonCook.Text = "Cook";

			// Restore cooking states
			m_Cooking = m_Abort = m_ForceSuccess = false;
		}

		private void processCook_OutputDataReceived( object sender, System.Diagnostics.DataReceivedEventArgs e )
		{
			if ( e.Data == null || m_ForceSuccess )
				return;

			string	Line = e.Data + "\r\n";

			int		StartIndex = richTextBoxOutput.TextLength;
			int		EndIndex = StartIndex + Line.Length;

			int		CurrentCaretPosition = richTextBoxOutput.SelectionStart;
			bool	IsAtEnd = CurrentCaretPosition == StartIndex;

			AppendText( Line );

			if ( StartIndex == 0 )
			{
//				richTextBoxOutput.SelectionStart = richTextBoxOutput.TextLength;
				richTextBoxOutput.Focus();
				richTextBoxOutput.AppendText( "" );
				richTextBoxOutput.ScrollToCaret();
			}
// 			if ( !IsAtEnd )
// 				richTextBoxOutput.SelectionStart = CurrentCaretPosition;	// Restore caret position
//			richTextBoxOutput.ScrollToCaret();

			// Apply syntax coloring
			Color	LineColor = Color.Black;
			if ( Line.IndexOf( "buildgame: Successful." ) != -1 )
			{	// Yay! Force success by killing the process before it crashes!
				LineColor = Color.ForestGreen;
				m_ForceSuccess = true;
			}
			else if ( Line.IndexOf( "error", StringComparison.InvariantCultureIgnoreCase ) != -1 )
				LineColor = Color.Red;
			else if ( Line.IndexOf( "warning", StringComparison.InvariantCultureIgnoreCase ) != -1 )
				LineColor = Color.Coral;

			if ( LineColor != Color.Black )
			{
				int	OldSelectionStart = richTextBoxOutput.SelectionStart;
				int	OldSelectionLength = richTextBoxOutput.SelectionLength;

				richTextBoxOutput.Select( StartIndex, EndIndex - StartIndex );
				richTextBoxOutput.SelectionColor = LineColor;

				richTextBoxOutput.Select( OldSelectionStart, OldSelectionLength );
			}
		}

		private void processCook_ErrorDataReceived( object sender, System.Diagnostics.DataReceivedEventArgs e )
		{
			if ( e.Data == null )
				return;

			richTextBoxOutput.AppendText( "<ERROR> " + e.Data + "\r\n" );
		}

		#region PREVENT RTB SCROLL

		// From http://social.msdn.microsoft.com/Forums/windows/en-US/068b31bd-c659-4b21-a02a-46bf9b9f39f2/richtextbox-controlling-scrolling-when-appending-text?forum=winforms

		void	AppendText( string text )     
		{
			bool focused = richTextBoxOutput.Focused; 
	
			//backup initial selection
			int selection = richTextBoxOutput.SelectionStart;
			int length = richTextBoxOutput.SelectionLength;  

			//allow autoscroll if selection is at end of text
			bool autoscroll = selection == richTextBoxOutput.Text.Length;
			if ( !autoscroll )
			{
				//shift focus from RichTextBox to some other control
				if ( focused )
					buttonCook.Focus();

				//hide selection  
				SendMessage( richTextBoxOutput.Handle, EM_HIDESELECTION, 1, 0 );
			}

			richTextBoxOutput.AppendText(text);

			if ( !autoscroll )
			{
				//restore initial selection
				richTextBoxOutput.SelectionStart = selection;
				richTextBoxOutput.SelectionLength = length;

				//unhide selection
				SendMessage( richTextBoxOutput.Handle, EM_HIDESELECTION, 0, 0 );

				//restore focus to RichTextBox
				if ( focused )
					richTextBoxOutput.Focus();
			}
		}

// 		bool	m_ScrollLock = false;	//toggled by the key of the same name.
// 		private void	AppendText( string text )
// 		{
// 			if ( richTextBoxOutput.Focused )
// 			{
// 				if ( !m_ScrollLock )
// 				{
// 					HideSelection(false);
// 					richTextBoxOutput.AppendText(text);
// 				}
// 				else
// 				{	// This is the problem case ....
// //					HideCaret();
// 					richTextBoxOutput.Focus();
// 					HideSelection(true);
// 					richTextBoxOutput.HideSelection = true;
// 
// 					int pos = richTextBoxOutput.SelectionStart;
// 					int len = richTextBoxOutput.SelectionLength;
// 					richTextBoxOutput.AppendText(text);
// 					richTextBoxOutput.SelectionStart = pos;
// 					richTextBoxOutput.SelectionLength = len;
// 
// 					richTextBoxOutput.HideSelection = false;
// 					HideSelection(false);
// 					richTextBoxOutput.Focus();
// 				}
// 			}
// 			else
// 			{
// 				if ( !m_ScrollLock )
// 				{
// 					HideSelection(false);
// 					richTextBoxOutput.HideSelection = false;
// 					richTextBoxOutput.AppendText(text);
// 				}
// 				else
// 				{
// 					HideSelection(true);
// 					int pos = richTextBoxOutput.SelectionStart;
// 					richTextBoxOutput.AppendText(text);
// 					richTextBoxOutput.SelectionStart = pos;
// 				}
// 			}
// 		}

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);
		const int WM_USER = 0x400;
		const int EM_HIDESELECTION = WM_USER + 63;
		public void HideSelection(bool hide)
		{
			SendMessage(richTextBoxOutput.Handle, EM_HIDESELECTION, hide ? 1 : 0, 0);
		}

		#endregion

		private void comboBoxExecutable_SelectedIndexChanged( object sender, EventArgs e )
		{
			// Update registry
			m_AppKey.SetValue( "Executable", comboBoxExecutable.SelectedIndex.ToString() );

			switch ( comboBoxExecutable.SelectedIndex )
			{
				case 0:
					m_ExecutablePath = @"V:\Dishonored2\VoidEngine\build\output\x64\Debug\Dishonored2_x64.exe";
					break;
				case 1:
					m_ExecutablePath = @"V:\Dishonored2\VoidEngine\build\output\x64\Release\Dishonored2_x64.exe";
					break;
				case 2:
					m_ExecutablePath = @"V:\Dishonored2\Dishonored2\Dishonored2_x64.exe";
					break;
				case 3:
					m_ExecutablePath = m_CustomExecutablePath;
					break;
			}

			textBoxExecutablePath.Text = m_ExecutablePath;
			buttonCustomExecutable.Enabled = comboBoxExecutable.SelectedIndex == 3;
		}

		private void buttonCustom_Click( object sender, EventArgs e )
		{
			string	customPath = m_AppKey.GetValue( "CustomExecutablepath", m_CustomExecutablePath ) as string;
			openFileDialogExecutable.FileName = Path.GetFileName( customPath );
			openFileDialogExecutable.InitialDirectory = Path.GetDirectoryName( customPath );
			if ( openFileDialogExecutable.ShowDialog( this ) != DialogResult.OK ) {
				return;
			}
			m_AppKey.SetValue( "CustomExecutablepath", openFileDialogExecutable.FileName );

			m_CustomExecutablePath = openFileDialogExecutable.FileName;
			textBoxExecutablePath.Text = m_CustomExecutablePath;
		}

		private void buttonBasePath_Click( object sender, EventArgs e )
		{
			folderBrowserDialog.SelectedPath =	m_AppKey.GetValue( "BasePath", m_BasePath ) as string;
			if ( folderBrowserDialog.ShowDialog( this ) != DialogResult.OK ) {
				return;
			}
			string	selectedFolder = folderBrowserDialog.SelectedPath.TrimEnd( '\\' );
			m_AppKey.SetValue( "BasePath", selectedFolder );

			m_BasePath = selectedFolder;
			textBoxBasePath.Text = m_BasePath;
		}
	}
}
