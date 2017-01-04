/******************************************************************/
/*****                                                        *****/
/*****     Project:           Adobe Color Picker Clone 1      *****/
/*****     Filename:          HorizontalAlphaSliderControl.cs    *****/
/*****     Original Author:   Danny Blanchard                 *****/
/*****                        - scrabcakes@gmail.com          *****/
/*****     Updates:	                                          *****/
/*****      3/28/2005 - Initial Version : Danny Blanchard     *****/
/*****                                                        *****/
/******************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Nuaj.Cirrus.Utility
{
	/// <summary>
	/// An horizontal slider control that shows a range for an alpha property and sends an event when the slider is changed.
	/// </summary>
	public class HorizontalAlphaSliderControl : System.Windows.Forms.UserControl
	{
		#region FIELDS

		protected Bitmap	m_Output = null;

		//	Slider properties
		protected int			m_iMarker_Start_X = 0;
		protected bool		m_bDragging = false;

		//	These variables keep track of how to fill in the content inside the box;
		protected float		m_Alpha = 1.0f;

		protected System.ComponentModel.Container components = null;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// The Alpha of the control
		/// </summary>
		public float	Alpha
		{
			get { return m_Alpha; }
			set
			{
				m_Alpha = value;

				//	Redraw the control based on the new color.
				Reset_Slider( true );
				DrawContent();
			}
		}

		public new event EventHandler	Scroll;

		#endregion

		#region METHODS

		public HorizontalAlphaSliderControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

// 			SetStyle( ControlStyles.DoubleBuffer, true );
// 			SetStyle( ControlStyles.AllPaintingInWmPaint, true );
// 			SetStyle( ControlStyles.UserPaint, true );
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if ( m_Output != null )
					m_Output.Dispose();

				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// HorizontalAlphaSliderControl
			// 
			this.Name = "HorizontalAlphaSliderControl";
			this.Size = new System.Drawing.Size( 264, 40 );
			this.ResumeLayout( false );

		}

		#endregion

		#region Control Events

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			Redraw_Control();
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );

			if ( e.Button != MouseButtons.Left )	//	Only respond to left mouse button events
				return;

			m_bDragging = true;		//	Begin dragging which notifies MouseMove function that it needs to update the marker

			int x;
			x = e.X;
			x -= 4;											//	Calculate slider position
			if ( x < 0 ) x = 0;
			if ( x > this.Width - 9 ) x = this.Width - 9;

			if ( x == m_iMarker_Start_X )					//	If the slider hasn't moved, no need to redraw it.
				return;										//	or send a scroll notification

			DrawSlider(x, false);	//	Redraw the slider
			Refresh();

			UpdateAlpha( e );
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			if ( !m_bDragging )		//	Only respond when the mouse is dragging the marker.
				return;

			int x;
			x = e.X;
			x -= 4; 										//	Calculate slider position
			if ( x < 0 ) x = 0;
			if ( x > this.Width - 9 ) x = this.Width - 9;

			if ( x == m_iMarker_Start_X )					//	If the slider hasn't moved, no need to redraw it.
				return;										//	or send a scroll notification

			DrawSlider(x, false);	//	Redraw the slider
			Refresh();

			UpdateAlpha( e );
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );

			if ( e.Button != MouseButtons.Left )	//	Only respond to left mouse button events
				return;

			m_bDragging = false;

			int x;
			x = e.X;
			x -= 4; 										//	Calculate slider position
			if ( x < 0 ) x = 0;
			if ( x > this.Width - 9 ) x = this.Width - 9;

			if ( x == m_iMarker_Start_X )					//	If the slider hasn't moved, no need to redraw it.
				return;										//	or send a scroll notification

			DrawSlider(x, false);	//	Redraw the slider
			Refresh();

			UpdateAlpha( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
//			base.OnPaint( e );

			// Simply output the bitmap...
			if ( m_Output != null )
				e.Graphics.DrawImage( m_Output, ClientRectangle );
		}

		protected override void OnResize( EventArgs e )
		{
			base.OnResize( e );

			// Dispose of previous bitmap
			if ( m_Output != null )
			{
				m_Output.Dispose();
				m_Output = null;
			}

			Redraw_Control();
		}

		#endregion

		protected void	UpdateAlpha( EventArgs _e )
		{
			m_Alpha  = (float) m_iMarker_Start_X / (Width - 9);

			if ( Scroll != null )
				Scroll( this, _e );
		}

		/// <summary>
		/// Redraws the background over the slider area on both sides of the control
		/// </summary>
		protected void ClearSlider()
		{
			Graphics g = Graphics.FromImage( m_Output );
			Brush brush = System.Drawing.SystemBrushes.Control;
			g.FillRectangle(brush, 0, 0, this.Width, 9 );				//	clear top hand slider
			g.FillRectangle(brush, 0, this.Height - 8, this.Width, 8 );	//	clear bottom hand slider
			g.FillRectangle(brush, 0, 0, 2, Height);					//	clear left
			g.FillRectangle(brush, Width-2, 0, 2, Height);				//	clear right
			g.Dispose();
		}


		/// <summary>
		/// Draws the slider arrows on both sides of the control.
		/// </summary>
		/// <param name="position">position value of the slider, lowest being at the left.  The range
		/// is between 0 and the controls width-9.  The values will be adjusted if too large/small</param>
		/// <param name="Unconditional">If Unconditional is true, the slider is drawn, otherwise some logic 
		/// is performed to determine is drawing is really neccessary.</param>
		protected void DrawSlider(int position, bool Unconditional)
		{
			if ( position < 0 ) position = 0;
			if ( position > this.Width - 9 ) position = this.Width - 9;

			if ( m_iMarker_Start_X == position && !Unconditional )	//	If the marker position hasn't changed
				return;												//	since the last time it was drawn and we don't HAVE to redraw
			//	then exit procedure

			m_iMarker_Start_X = position;	//	Update the controls marker position


			this.ClearSlider();		//	Remove old slider

			Graphics g = Graphics.FromImage( m_Output );

			Pen pencil = new Pen(Color.FromArgb(116,114,106));	//	Same gray color Photoshop uses
			Brush brush = Brushes.White;
			
			Point[] arrow = new Point[7];				//	 GGG
			arrow[0] = new Point(position,1);			//	G   G
			arrow[1] = new Point(position,3);			//	G    G
			arrow[2] = new Point(position + 4,7);		//	G     G
			arrow[3] = new Point(position + 8,3);		//	G      G
			arrow[4] = new Point(position + 8,1);		//	G     G
			arrow[5] = new Point(position + 7,0);		//	G    G
			arrow[6] = new Point(position + 1,0);		//	G   G
			//	 GGG

			g.FillPolygon(brush, arrow);	//	Fill left arrow with white
			g.DrawPolygon(pencil, arrow);	//	Draw left arrow border with gray

			//	    GGG
			arrow[0] = new Point(position, this.Height - 2);		//	   G   G
			arrow[1] = new Point(position, this.Height - 4);		//	  G    G
			arrow[2] = new Point(position + 4, this.Height - 8);	//	 G     G
			arrow[3] = new Point(position + 8, this.Height - 4);	//	G      G
			arrow[4] = new Point(position + 8, this.Height - 2);	//	 G     G
			arrow[5] = new Point(position + 7, this.Height - 1);	//	  G    G
			arrow[6] = new Point(position + 1, this.Height - 1);	//	   G   G
			//	    GGG

			g.FillPolygon(brush, arrow);	//	Fill right arrow with white
			g.DrawPolygon(pencil, arrow);	//	Draw right arrow border with gray

			pencil.Dispose();
			g.Dispose();
		}


		/// <summary>
		/// Draws the border around the control, in this case the border around the content area between
		/// the slider arrows.
		/// </summary>
		protected void DrawBorder()
		{
			Graphics g = Graphics.FromImage( m_Output );

			Pen pencil;
			
			//	To make the control look like Adobe Photoshop's the border around the control will be a gray line
			//	on the top and left side, a white line on the bottom and right side, and a black rectangle (line) 
			//	inside the gray/white rectangle

			pencil = new Pen(Color.FromArgb(172,168,153));	//	The same gray color used by Photoshop
			g.DrawLine(pencil, 2, this.Height - 10, 2, 9);	//	Draw top line
			g.DrawLine(pencil, 2, 9, this.Width - 4, 9);	//	Draw left hand line
			pencil.Dispose();

			pencil = new Pen(Color.White);
			g.DrawLine(pencil, 2, this.Height - 9, this.Width - 3,this.Height - 9);	//	Draw right hand line
			g.DrawLine(pencil, this.Width - 3,this.Height - 9, this.Width - 3, 9);	//	Draw bottome line
			pencil.Dispose();

			pencil = new Pen(Color.Black);
			g.DrawRectangle(pencil, 3, 10, this.Width - 7, this.Height - 20);	//	Draw inner black rectangle
			pencil.Dispose();

			g.Dispose();
		}


		/// <summary>
		/// Evaluates the DrawStyle of the control and calls the appropriate
		/// drawing function for content
		/// </summary>
		protected void DrawContent()
		{
			Graphics g = Graphics.FromImage( m_Output );

			Rectangle	Rect = new Rectangle( 4, 11, Width-8, Height-21 );

			// Draw a nice checker box
			System.Drawing.Drawing2D.HatchBrush	Checker = new System.Drawing.Drawing2D.HatchBrush( System.Drawing.Drawing2D.HatchStyle.LargeCheckerBoard, Color.Gray, Color.DarkGray );
			g.FillRectangle( Checker, Rect );
			Checker.Dispose();

			// Draw a gradient box
			System.Drawing.Drawing2D.GraphicsPath		Path = new System.Drawing.Drawing2D.GraphicsPath();
														Path.AddRectangle( Rect );

			System.Drawing.Drawing2D.PathGradientBrush	Gradient = new System.Drawing.Drawing2D.PathGradientBrush( Path );
														Gradient.SurroundColors = new Color[] { Color.FromArgb( 0, Color.White ), Color.FromArgb( 255, Color.White ), Color.FromArgb( 255, Color.White ), Color.FromArgb( 0, Color.White ) };
 														Gradient.CenterPoint = new PointF( 0.5f * (Rect.Left + Rect.Right), .5f * (Rect.Bottom + Rect.Top) );
 														Gradient.CenterColor = Color.FromArgb( 128, Color.White );

			g.FillRectangle( Gradient, Rect );

			Gradient.Dispose();
		}

		/// <summary>
		/// Calls all the functions neccessary to redraw the entire control.
		/// </summary>
		protected void Redraw_Control()
		{
			// Check we have a valid bitmap to paint into
			if ( m_Output == null )
			{
				Graphics	Reference = this.CreateGraphics();
				m_Output = new Bitmap( Width, Height, Reference );
				Reference.Dispose();
			}

			DrawSlider(m_iMarker_Start_X, true);
			DrawBorder();
			DrawContent();
			Invalidate();
		}

		/// <summary>
		/// Resets the vertical position of the slider to match the controls color.  Gives the option of redrawing the slider.
		/// </summary>
		/// <param name="Redraw">Set to true if you want the function to redraw the slider after determining the best position</param>
		protected void Reset_Slider(bool Redraw)
		{
			//	The position of the marker (slider) changes based on the current drawstyle:
			m_iMarker_Start_X = (int) Math.Max( 0.0f, Math.Min( Width-8, (Width - 8) * m_Alpha ) );

			if ( Redraw )
				DrawSlider( m_iMarker_Start_X, true );
		}

		#endregion
	}
}
