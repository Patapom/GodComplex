using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Brain2
{
	public class ScrollablePannel : Panel {

		public Panel	m_childPanel = null;

		public ScrollablePannel() {
			InitializeComponent();
			Init();
		}

		public ScrollablePannel( IContainer container ) {
			container.Add( this );
			InitializeComponent();
			Init();
		}

		void		Init() {
			SetStyle( ControlStyles.Selectable, true );
			SetStyle( ControlStyles.ResizeRedraw, true );
			SetStyle( ControlStyles.EnableNotifyMessage, true );
			SetStyle( ControlStyles.ContainerControl, false );
// 			this.DoubleBuffered = true;
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if ( disposing && (components != null) ) {
				components.Dispose();
			}

			base.Dispose( disposing );
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// ScrollablePannel
			// 
			this.AutoScroll = true;
			this.TabStop = true;
			this.ResumeLayout(false);

		}

		#endregion

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);

			if ( m_childPanel == null )
				return;

			Point	scrollPosition = AutoScrollPosition;
					scrollPosition.Y = Math.Abs( scrollPosition.Y );

			switch ( e.KeyCode ) {
				case Keys.PageUp:	scrollPosition.Y -= 100; break;
				case Keys.PageDown:	scrollPosition.Y += 100; break;
				case Keys.Home:		scrollPosition.Y = 0; break;
				case Keys.End:		scrollPosition.Y = m_childPanel.Height - Height; break;
			}

			AutoScrollPosition = scrollPosition;
		}
	}
}
