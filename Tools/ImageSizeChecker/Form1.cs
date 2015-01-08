using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

namespace ImageSizeChecker
{
	public partial class ImageSizeCheckerForm : Form
	{
		private RegistryKey	m_AppKey;

		public ImageSizeCheckerForm()
		{
			InitializeComponent();

			m_AppKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\Arkane\ImageSizeChecker" );
		}

		private void	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			System.Windows.Forms.MessageBox.Show( this, _Text, "Image Size Checker", _Buttons, _Icon );
		}

		bool	m_Cancel;
		List< FileInfo >	m_ImagesTooLarge = new List< FileInfo >();

		private void buttonCheck_Click( object sender, EventArgs e )
		{
			string	PreviousFolder = m_AppKey.GetValue( "PreviousFolder", Application.ExecutablePath ) as string;
			folderBrowserDialog.SelectedPath = PreviousFolder;
			if ( folderBrowserDialog.ShowDialog( this ) != DialogResult.OK ) {
				return;
			}
			DirectoryInfo	Root = new DirectoryInfo( folderBrowserDialog.SelectedPath );
			if ( !Root.Exists ) {
				MessageBox( "The selected folder doesn't exist!", MessageBoxButtons.OK, MessageBoxIcon.Error );
				return;
			}
			m_AppKey.SetValue( "PreviousFolder", folderBrowserDialog.SelectedPath );

			try
			{
				//////////////////////////////////////////////////////////////////////////
				// Gather all images in all subdirectories
				FileInfo[]	ImageFilesTGA = Root.GetFiles( "*.tga", SearchOption.AllDirectories );
				FileInfo[]	ImageFilesPNG = Root.GetFiles( "*.png", SearchOption.AllDirectories );

				int		ImagesCount = ImageFilesPNG.Length + ImageFilesTGA.Length;
				progressBar.Maximum = ImagesCount;
				progressBar.Value = 0;
				progressBar.Visible = true;
				buttonCancel.Visible = true;
				buttonCheck.Enabled = false;
				m_Cancel = false;

				richTextBoxResults.Text = "";
				string	Errors = null;

				int		MaxSize = integerTrackbarControlSize.Value;
				m_ImagesTooLarge.Clear();

				// Check for PNGs
				foreach ( FileInfo ImageFilePNG in ImageFilesPNG ) {
					progressBar.Value++;
					if ( progressBar.Value % 20 == 0 ) {
						richTextBoxResults.Refresh();
						Application.DoEvents();
					}
					if ( m_Cancel )
						break;

					try
					{
						using ( Image B = Bitmap.FromFile( ImageFilePNG.FullName ) ) {
							if ( B.Width > MaxSize || B.Height > MaxSize ) {
								richTextBoxResults.Text += "• File " + ImageFilePNG.FullName + " is " + B.Width + "x" + B.Height + "\r\n";
								m_ImagesTooLarge.Add( ImageFilePNG );
							}
						}
					}
					catch ( Exception _e )
					{
						Errors += "> Failed to open " + ImageFilePNG + ": " + _e.Message + "\r\n";
					}
				}

				// Check for TGAs
				foreach ( FileInfo ImageFileTGA in ImageFilesTGA ) {
					progressBar.Value++;
					if ( progressBar.Value % 20 == 0 ) {
						richTextBoxResults.Refresh();
						Application.DoEvents();
					}
					if ( m_Cancel )
						break;

					try
					{
						using ( ImageUtility.TargaImage B = new ImageUtility.TargaImage( ImageFileTGA.FullName, true ) ) {
							if ( B.Header.Width > MaxSize || B.Header.Height > MaxSize ) {
								richTextBoxResults.Text += "• File " + ImageFileTGA.FullName + " is " + B.Header.Width + "x" + B.Header.Height + "\r\n";
								m_ImagesTooLarge.Add( ImageFileTGA );
							}
						}
					}
					catch ( Exception _e )
					{
						Errors += "> Failed to open " + ImageFileTGA + ": " + _e.Message + "\r\n";
					}
				}

				if ( Errors != null )
					richTextBoxResults.Text += "\r\n\r\nThe following errors were encountered:\r\n" + Errors;
				if ( m_Cancel )
					richTextBoxResults.Text += "\r\n\r\n>>>>>>> CANCELLED <<<<<<<<<<<<<\r\n";


				MessageBox( "Done! " + progressBar.Value + " images were processed and " + m_ImagesTooLarge.Count + " images have been determined as too large...", MessageBoxButtons.OK, MessageBoxIcon.Information );
			}
			catch ( Exception _e )
			{
				MessageBox( "An error occurred while checking images!\r\nReason: " + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
			finally
			{
				progressBar.Visible = false;
				buttonCheck.Enabled = true;
			}
		}

		private void LogText( string _Text ) {

			string	Line = _Text;

			int		StartIndex = richTextBoxResults.TextLength;
			int		EndIndex = StartIndex + Line.Length;

			int		CurrentCaretPosition = richTextBoxResults.SelectionStart;
			bool	IsAtEnd = CurrentCaretPosition == StartIndex;

			AppendText( Line );

			if ( StartIndex == 0 )
			{
				richTextBoxResults.Focus();
				richTextBoxResults.AppendText( "" );
				richTextBoxResults.ScrollToCaret();
			}

// 			// Apply syntax coloring
// 			Color	LineColor = Color.Black;
// 			if ( Line.IndexOf( "buildgame: Successful." ) != -1 )
// 			{	// Yay! Force success by killing the process before it crashes!
// 				LineColor = Color.ForestGreen;
// 				m_ForceSuccess = true;
// 			}
// 			else if ( Line.IndexOf( "error", StringComparison.InvariantCultureIgnoreCase ) != -1 )
// 				LineColor = Color.Red;
// 			else if ( Line.IndexOf( "warning", StringComparison.InvariantCultureIgnoreCase ) != -1 )
// 				LineColor = Color.Coral;
// 
// 			if ( LineColor != Color.Black )
// 			{
// 				int	OldSelectionStart = richTextBoxResults.SelectionStart;
// 				int	OldSelectionLength = richTextBoxResults.SelectionLength;
// 
// 				richTextBoxResults.Select( StartIndex, EndIndex - StartIndex );
// 				richTextBoxResults.SelectionColor = LineColor;
// 
// 				richTextBoxResults.Select( OldSelectionStart, OldSelectionLength );
// 			}
		}

		private void buttonCancel_Click( object sender, EventArgs e )
		{
			m_Cancel = true;
		}

		private void richTextBoxResults_DoubleClick( object sender, EventArgs e )
		{
			if ( m_ImagesTooLarge.Count == 0 )
				return;

			Point		ClientPos = richTextBoxResults.PointToClient( MousePosition );
			int			Index = richTextBoxResults.GetCharIndexFromPosition( ClientPos );
			string		SubString = richTextBoxResults.Text.Substring( 0, Index );
			string[]	Lines = SubString.Split( '\n' );
			int			FileNameIndex = Math.Min( m_ImagesTooLarge.Count-1, Lines.Length-1 );

			FileInfo	ImageFile = m_ImagesTooLarge[FileNameIndex];

			ProcessStartInfo psi = new ProcessStartInfo( ImageFile.FullName );
			psi.UseShellExecute = true;
			Process.Start(psi);
		}

		#region PREVENT RTB SCROLL

		// From http://social.msdn.microsoft.com/Forums/windows/en-US/068b31bd-c659-4b21-a02a-46bf9b9f39f2/richtextbox-controlling-scrolling-when-appending-text?forum=winforms

		void	AppendText( string text )     
		{
			bool focused = richTextBoxResults.Focused; 
	
			//backup initial selection
			int selection = richTextBoxResults.SelectionStart;
			int length = richTextBoxResults.SelectionLength;  

			//allow autoscroll if selection is at end of text
			bool autoscroll = selection == richTextBoxResults.Text.Length;
			if ( !autoscroll )
			{
				//shift focus from RichTextBox to some other control
				if ( focused )
					buttonCancel.Focus();

				//hide selection  
				SendMessage( richTextBoxResults.Handle, EM_HIDESELECTION, 1, 0 );
			}

			richTextBoxResults.AppendText(text);

			if ( !autoscroll )
			{
				//restore initial selection
				richTextBoxResults.SelectionStart = selection;
				richTextBoxResults.SelectionLength = length;

				//unhide selection
				SendMessage( richTextBoxResults.Handle, EM_HIDESELECTION, 0, 0 );

				//restore focus to RichTextBox
				if ( focused )
					richTextBoxResults.Focus();
			}
		}

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);
		const int WM_USER = 0x400;
		const int EM_HIDESELECTION = WM_USER + 63;
		public void HideSelection(bool hide)
		{
			SendMessage(richTextBoxResults.Handle, EM_HIDESELECTION, hide ? 1 : 0, 0);
		}

		#endregion
	}
}
