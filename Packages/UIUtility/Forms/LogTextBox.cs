﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UIUtility
{
	public partial class LogTextBox : RichTextBox
	{
		StringBuilder	m_Log = new StringBuilder();

		public LogTextBox()
		{
			InitializeComponent();
		}

		public LogTextBox( IContainer container )
		{
			container.Add( this );

			InitializeComponent();
		}

		void	FixText( ref string _Text ) {
			_Text = _Text.Replace( "\\", @"\" );
			_Text = _Text.Replace( "\n", @"\line " );
			_Text = _Text.Replace( "\r", "" );
		}

		public void		Log( string _Text ) {
			FixText( ref _Text );
			m_Log.Append( @"\cf1 " + _Text );
			UpdateRTF();
		}

		public void		LogSuccess( string _Text ) {
			FixText( ref _Text );
			m_Log.Append( @"\cf2 " + _Text );
			UpdateRTF();
		}

		public void		LogWarning( string _Text ) {
			FixText( ref _Text );
			m_Log.Append( @"\cf3 " + _Text );
			UpdateRTF();
		}

		public void		LogError( string _Text ) {
			FixText( ref _Text );
			m_Log.Append( @"\cf4 " + _Text );
			UpdateRTF();
		}

		public void		LogDebug( string _Text ) {
			FixText( ref _Text );
			m_Log.Append( @"\cf5 " + _Text );
			UpdateRTF();
		}

		protected void	UpdateRTF() {
			string RTFText = @"{\rtf1\ansi\deff0" +
			@"{\colortbl;\red0\green0\blue0;\red32\green128\blue32;\red192\green128\blue0;\red255\green0\blue0;\red127\green20\blue127;}" +
			m_Log.ToString() + @"\line }";

			this.SuspendLayout();

			bool	wasAutoScrolling = SelectionStart == Rtf.Length;	// Already auto scrolling?

			this.Rtf = RTFText;

			if ( wasAutoScrolling )
				SelectionStart = this.Rtf.Length;	// Continue to auto scroll...

			this.ScrollToCaret();
			this.ResumeLayout();
		}
	}
}
