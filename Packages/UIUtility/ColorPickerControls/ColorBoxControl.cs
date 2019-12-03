/******************************************************************/
/*****                                                        *****/
/*****     Project:           Adobe Color Picker Clone 1      *****/
/*****     Filename:          ColorBoxControl.cs               *****/
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data;
using System.Windows.Forms;

using SharpMath;

namespace Nuaj.Cirrus.Utility
{
	/// <summary>
	/// Summary description for ColorBoxControl.
	/// </summary>
	public class ColorBoxControl : System.Windows.Forms.UserControl
	{
		#region FIELDS

		protected Bitmap	m_Output = null;

		protected int		m_iMarker_X = 0;
		protected int		m_iMarker_Y = 0;
		protected bool		m_bDragging = false;

		//	These variables keep track of how to fill in the content inside the box;
		protected ColorPickerForm.DRAW_STYLE	m_DrawStyle = ColorPickerForm.DRAW_STYLE.Hue;
//		protected float3						m_HSL = float3.One;
		protected float3						m_RGB = float3.One;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		protected System.ComponentModel.Container components = null;

		#endregion
		
		#region PROPERTIES

		/// <summary>
		/// The drawstyle of the contol (Hue, Saturation, Brightness, Red, Green or Blue)
		/// </summary>
		public ColorPickerForm.DRAW_STYLE DrawStyle {
			get { return m_DrawStyle; }
			set {
				m_DrawStyle = value;

				//	Redraw the control based on the new ColorPickerForm.DRAW_STYLE
				Reset_Marker( true );
				Redraw_Control();
			}
		}


		/// <summary>
		/// The HSL color of the control, changing the HSL will automatically change the RGB color for the control.
		/// </summary>
		public float3	HSL {
			get { return AdobeColors.RGB2HSB( m_RGB ); }
			set {
				m_RGB = AdobeColors.HSB2RGB( value );

				//	Redraw the control based on the new color.
				Reset_Marker( true );
				Redraw_Control();
			}
		}


		/// <summary>
		/// The RGB color of the control, changing the RGB will automatically change the HSL color for the control.
		/// </summary>
		public float3	RGB {
			get { return m_RGB; }
			set {
				m_RGB = value;

				//	Redraw the control based on the new color.
				Reset_Marker( true );
				Redraw_Control();
			}
		}

		public new event EventHandler Scroll;

		#endregion

		#region METHODS

		public ColorBoxControl()
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
			// 
			// ColorBoxControl
			// 
			this.Name = "ColorBoxControl";
			this.Size = new System.Drawing.Size(260, 260);

		}
		#endregion

		#region Control Events

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );
			Redraw_Control();
		}

		protected override void OnMouseDown( MouseEventArgs e ) {
			base.OnMouseDown( e );

			if ( e.Button != MouseButtons.Left )	//	Only respond to left mouse button events
				return;

			m_bDragging = true;		//	Begin dragging which notifies MouseMove function that it needs to update the marker

			int x = e.X - 2, y = e.Y - 2;
			if ( x < 0 ) x = 0;
			if ( x > this.Width - 4 ) x = this.Width - 4;	//	Calculate marker position
			if ( y < 0 ) y = 0;
			if ( y > this.Height - 4 ) y = this.Height - 4;

			if ( x == m_iMarker_X && y == m_iMarker_Y )		//	If the marker hasn't moved, no need to redraw it.
				return;										//	or send a scroll notification

			DrawMarker( x, y, true );	//	Redraw the marker
			ResetHSLRGB();				//	Reset the color

			if ( Scroll != null )		//	Notify anyone who cares that the controls marker (selected color) has changed
				Scroll(this, e);

			Refresh();
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			if ( !m_bDragging )		//	Only respond when the mouse is dragging the marker.
				return;

			int x = e.X - 2, y = e.Y - 2;
			if ( x < 0 ) x = 0;
			if ( x > this.Width - 4 ) x = this.Width - 4;	//	Calculate marker position
			if ( y < 0 ) y = 0;
			if ( y > this.Height - 4 ) y = this.Height - 4;

			if ( x == m_iMarker_X && y == m_iMarker_Y )		//	If the marker hasn't moved, no need to redraw it.
				return;										//	or send a scroll notification

			DrawMarker(x, y, true);	//	Redraw the marker
			ResetHSLRGB();			//	Reset the color

			if ( Scroll != null )	//	Notify anyone who cares that the controls marker (selected color) has changed
				Scroll(this, e);

			Refresh();
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );

			if ( e.Button != MouseButtons.Left )	//	Only respond to left mouse button events
				return;

			if ( !m_bDragging )
				return;

			m_bDragging = false;

			int x = e.X - 2, y = e.Y - 2;
			if ( x < 0 ) x = 0;
			if ( x > this.Width - 4 ) x = this.Width - 4;	//	Calculate marker position
			if ( y < 0 ) y = 0;
			if ( y > this.Height - 4 ) y = this.Height - 4;

			if ( x == m_iMarker_X && y == m_iMarker_Y )		//	If the marker hasn't moved, no need to redraw it.
				return;										//	or send a scroll notification

			DrawMarker(x, y, true);	//	Redraw the marker
			ResetHSLRGB();			//	Reset the color

			if ( Scroll != null )	//	Notify anyone who cares that the controls marker (selected color) has changed
				Scroll(this, e);

			Refresh();
		}

		protected override void OnResize( EventArgs e ) {
			base.OnResize( e );

			// Dispose of previous bitmap
			if ( m_Output != null ) {
				m_Output.Dispose();
				m_Output = null;
			}

			Redraw_Control();
		}

		protected override void OnPaintBackground( PaintEventArgs e ) {
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e ) {
//			base.OnPaint( e );

			// Simply output the bitmap...
			if ( m_Output != null )
				e.Graphics.DrawImage( m_Output, ClientRectangle );
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Redraws only the content over the marker
		/// </summary>
		protected void ClearMarker()
		{
			Graphics g = Graphics.FromImage( m_Output );
			
			//	Determine the area that needs to be redrawn
			int start_x, start_y, end_x, end_y;
			int red = 0; int green = 0; int blue = 0;
			float3	hsl_start = float3.Zero;
			float3	hsl_end = float3.Zero;

			//	Find the markers corners
			start_x = m_iMarker_X - 5;
			start_y = m_iMarker_Y - 5;
			end_x = m_iMarker_X + 5;
			end_y = m_iMarker_Y + 5;
			//	Adjust the area if part of it hangs outside the content area
			if ( start_x < 0 ) start_x = 0;
			if ( start_y < 0 ) start_y = 0;
			if ( end_x > this.Width - 4 ) end_x = this.Width - 4;
			if ( end_y > this.Height - 4 ) end_y = this.Height - 4;

			float3	HSL = this.HSL;

			//	Redraw the content based on the current draw style:
			//	The code is getting a little messy from here
			switch ( m_DrawStyle ) {
					//		  S=0,S=1,S=2,S=3.....S=100
					//	L=100
					//	L=99
					//	L=98		Drawstyle
					//	L=97		   Hue
					//	...
					//	L=0
				case ColorPickerForm.DRAW_STYLE.Hue :	

					hsl_start.x = HSL.x;	hsl_end.x = HSL.x;			//	Hue is constant
					hsl_start.y = (float) start_x / (Width - 4);		//	Because we're drawing horizontal lines, s will not change
					hsl_end.y = (float) end_x / (Width - 4);			//	from line to line

					for ( int i = start_y; i <= end_y; i++ ) {			//	For each horizontal line:
						hsl_start.z = 1.0f - (float) i / (Height - 4);	//	Brightness (L) WILL change for each horizontal
						hsl_end.z = hsl_start.z;						//	line drawn
				
						LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(start_x + 1,i + 2, end_x - start_x + 1, 1), AdobeColors.HSL_to_RGB_LDR(hsl_start), AdobeColors.HSL_to_RGB_LDR(hsl_end), 0, false); 
						g.FillRectangle(br,new System.Drawing.Rectangle(start_x + 2,i + 2, end_x - start_x + 1 , 1)); 
					}
					
					break;
					//		  H=0,H=1,H=2,H=3.....H=360
					//	L=100
					//	L=99
					//	L=98		Drawstyle
					//	L=97		Saturation
					//	...
					//	L=0
				case ColorPickerForm.DRAW_STYLE.Saturation :

					hsl_start.y = HSL.y;	hsl_end.y = HSL.y;			//	Saturation is constant
					hsl_start.z = 1.0f - (float) start_y/(Height - 4);	//	Because we're drawing vertical lines, L will 
					hsl_end.z = 1.0f - (float) end_y/(Height - 4);		//	not change from line to line

					for ( int i = start_x; i <= end_x; i++ ) {			//	For each vertical line:
						hsl_start.x = (float) i / (Width - 4);			//	Hue (H) WILL change for each vertical
						hsl_end.x = hsl_start.x;						//	line drawn
				
						LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(i + 2,start_y + 1, 1, end_y - start_y + 2), AdobeColors.HSL_to_RGB_LDR(hsl_start), AdobeColors.HSL_to_RGB_LDR(hsl_end), 90, false); 
						g.FillRectangle(br,new System.Drawing.Rectangle(i + 2, start_y + 2, 1, end_y - start_y + 1)); 
					}
					break;
					//		  H=0,H=1,H=2,H=3.....H=360
					//	S=100
					//	S=99
					//	S=98		Drawstyle
					//	S=97		Brightness
					//	...
					//	S=0
				case ColorPickerForm.DRAW_STYLE.Brightness :
					
					hsl_start.z = HSL.z;	hsl_end.z = HSL.z;				//	Luminance is constant
					hsl_start.y = 1.0f - (float) start_y / (Height - 4);	//	Because we're drawing vertical lines, S will 
					hsl_end.y = 1.0f - (float) end_y / (Height - 4);		//	not change from line to line

					for ( int i = start_x; i <= end_x; i++ ) {				//	For each vertical line:
						hsl_start.x = (float) i / (Width - 4);				//	Hue (H) WILL change for each vertical
						hsl_end.x = hsl_start.x;							//	line drawn
				
						LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(i + 2,start_y + 1, 1, end_y - start_y + 2), AdobeColors.HSL_to_RGB_LDR(hsl_start), AdobeColors.HSL_to_RGB_LDR(hsl_end), 90, false); 
						g.FillRectangle(br,new System.Drawing.Rectangle(i + 2, start_y + 2, 1, end_y - start_y + 1)); 
					}

					break;
					//		  B=0,B=1,B=2,B=3.....B=100
					//	G=100
					//	G=99
					//	G=98		Drawstyle
					//	G=97		   Red
					//	...
					//	G=0
				case ColorPickerForm.DRAW_STYLE.Red :
					
					red = AdobeColors.ConvertHDR2LDR( m_RGB ).R;				//	Red is constant
					int start_b = Round(255 * (double)start_x/(this.Width - 4));	//	Because we're drawing horizontal lines, B
					int end_b = Round(255 * (double)end_x/(this.Width - 4));		//	will not change from line to line

					for ( int i = start_y; i <= end_y; i++ )						//	For each horizontal line:
					{
						green = Round(255 - (255 * (double)i/(this.Height - 4)));	//	green WILL change for each horizontal line drawn

						LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(start_x + 1,i + 2, end_x - start_x + 1, 1), Color.FromArgb(red, green, start_b), Color.FromArgb(red, green, end_b), 0, false); 
						g.FillRectangle(br,new System.Drawing.Rectangle(start_x + 2,i + 2, end_x - start_x + 1 , 1));  
					}

					break;
					//		  B=0,B=1,B=2,B=3.....B=100
					//	R=100
					//	R=99
					//	R=98		Drawstyle
					//	R=97		  Green
					//	...
					//	R=0
				case ColorPickerForm.DRAW_STYLE.Green :
					
					green = AdobeColors.ConvertHDR2LDR( m_RGB ).G;				//	Green is constant
					int start_b2 = Round(255 * (double)start_x/(this.Width - 4));	//	Because we're drawing horizontal lines, B
					int end_b2 = Round(255 * (double)end_x/(this.Width - 4));		//	will not change from line to line

					for ( int i = start_y; i <= end_y; i++ )						//	For each horizontal line:
					{
						red = Round(255 - (255 * (double)i/(this.Height - 4)));		//	red WILL change for each horizontal line drawn

						LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(start_x + 1,i + 2, end_x - start_x + 1, 1), Color.FromArgb(red, green, start_b2), Color.FromArgb(red, green, end_b2), 0, false); 
						g.FillRectangle(br,new System.Drawing.Rectangle(start_x + 2,i + 2, end_x - start_x + 1 , 1)); 
					}

					break;
					//		  R=0,R=1,R=2,R=3.....R=100
					//	G=100
					//	G=99
					//	G=98		Drawstyle
					//	G=97		   Blue
					//	...
					//	G=0
				case ColorPickerForm.DRAW_STYLE.Blue :
					
					blue = AdobeColors.ConvertHDR2LDR( m_RGB ).B;				//	Blue is constant
					int start_r = Round(255 * (double)start_x/(this.Width - 4));	//	Because we're drawing horizontal lines, R
					int end_r = Round(255 * (double)end_x/(this.Width - 4));		//	will not change from line to line

					for ( int i = start_y; i <= end_y; i++ )						//	For each horizontal line:
					{
						green = Round(255 - (255 * (double)i/(this.Height - 4)));	//	green WILL change for each horizontal line drawn

						LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(start_x + 1,i + 2, end_x - start_x + 1, 1), Color.FromArgb(start_r, green, blue), Color.FromArgb(end_r, green, blue), 0, false); 
						g.FillRectangle(br,new System.Drawing.Rectangle(start_x + 2,i + 2, end_x - start_x + 1 , 1)); 
					}

					break;
			}
		}


		/// <summary>
		/// Draws the marker (circle) inside the box
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="Unconditional"></param>
		protected void DrawMarker(int x, int y, bool Unconditional)			//	   *****
		{																	//	  *  |  *
			if ( x < 0 ) x = 0;												//	 *   |   *
			if ( x > this.Width - 4 ) x = this.Width - 4;					//	*    |    *
			if ( y < 0 ) y = 0;												//	*    |    *
			if ( y > this.Height - 4 ) y = this.Height - 4;					//	*----X----*
			//	*    |    *
			if ( m_iMarker_Y == y && m_iMarker_X == x && !Unconditional )	//	*    |    *
				return;														//	 *   |   *
			//	  *  |  *
			ClearMarker();													//	   *****

			m_iMarker_X = x;
			m_iMarker_Y = y;

			Graphics g = Graphics.FromImage( m_Output );

			Pen pen;
			float3	_hsl = GetColor(x,y);	//	The selected color determines the color of the marker drawn over
			//	it (black or white)
			if ( _hsl.z < 200.0f / 255 )
				pen = new Pen(Color.White);									//	White marker if selected color is dark
			else if ( _hsl.x < 26.0f / 360 || _hsl.x > 200.0f / 360 )
				if ( _hsl.y > 70.0f / 255 )
					pen = new Pen(Color.White);
				else
					pen = new Pen(Color.Black);								//	Else use a black marker for lighter colors
			else
				pen = new Pen(Color.Black);

			g.DrawEllipse(pen, x - 3, y - 3, 10, 10);						//	Draw the marker : 11 x 11 circle

			DrawBorder();		//	Force the border to be redrawn, just in case the marker has been drawn over it.
		}


		/// <summary>
		/// Draws the border around the control.
		/// </summary>
		protected void DrawBorder()
		{
			Graphics g = Graphics.FromImage( m_Output );

			Pen pencil;
			
			//	To make the control look like Adobe Photoshop's the border around the control will be a gray line
			//	on the top and left side, a white line on the bottom and right side, and a black rectangle (line) 
			//	inside the gray/white rectangle

			pencil = new Pen(Color.FromArgb(172,168,153));	//	The same gray color used by Photoshop
			g.DrawLine(pencil, this.Width - 2, 0, 0, 0);	//	Draw top line
			g.DrawLine(pencil, 0, 0, 0, this.Height - 2);	//	Draw left hand line

			pencil = new Pen(Color.White);
			g.DrawLine(pencil, this.Width - 1, 0, this.Width - 1,this.Height - 1);	//	Draw right hand line
			g.DrawLine(pencil, this.Width - 1,this.Height - 1, 0,this.Height - 1);	//	Draw bottome line

			pencil = new Pen(Color.Black);
			g.DrawRectangle(pencil, 1, 1, this.Width - 3, this.Height - 3);	//	Draw inner black rectangle
		}


		/// <summary>
		/// Evaluates the DrawStyle of the control and calls the appropriate
		/// drawing function for content
		/// </summary>
		protected void DrawContent() {
			switch ( m_DrawStyle ) {
				case ColorPickerForm.DRAW_STYLE.Hue :
					Draw_Style_Hue();
					break;
				case ColorPickerForm.DRAW_STYLE.Saturation :
					Draw_Style_Saturation();
					break;
				case ColorPickerForm.DRAW_STYLE.Brightness :
					Draw_Style_Brightness();
					break;
				case ColorPickerForm.DRAW_STYLE.Red :
					Draw_Style_Red();
					break;
				case ColorPickerForm.DRAW_STYLE.Green :
					Draw_Style_Green();
					break;
				case ColorPickerForm.DRAW_STYLE.Blue :
					Draw_Style_Blue();
					break;
				case ColorPickerForm.DRAW_STYLE.Luminance:
					Draw_Style_Luminance();
					break;
				case ColorPickerForm.DRAW_STYLE.a:
					Draw_Style_a();
					break;
				case ColorPickerForm.DRAW_STYLE.b:
					Draw_Style_b();
					break;
			}
		}


		/// <summary>
		/// Draws the content of the control filling in all color values with the provided Hue value.
		/// </summary>
		protected void Draw_Style_Hue() {
			Graphics g = Graphics.FromImage( m_Output );

			float3	HSL = this.HSL;
			float3	hsl_start = float3.Zero;
			float3	hsl_end = float3.Zero;
			hsl_start.x = HSL.x;
			hsl_end.x = HSL.x;
			hsl_start.y = 0.0f;
			hsl_end.y = 1.0f;

			for ( int i = 0; i < this.Height - 4; i++ ) {		//	For each horizontal line in the control:
				hsl_start.z = 1.0f - (float) i / (Height - 4);	//	Calculate luminance at this line (Hue and Saturation are constant)
				hsl_end.z = hsl_start.z;
				
				LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(2,2, this.Width - 4, 1), AdobeColors.HSL_to_RGB_LDR(hsl_start), AdobeColors.HSL_to_RGB_LDR(hsl_end), 0, false); 
				g.FillRectangle(br,new System.Drawing.Rectangle(2,i + 2, this.Width - 4, 1)); 
			}
		}


		/// <summary>
		/// Draws the content of the control filling in all color values with the provided Saturation value.
		/// </summary>
		protected void Draw_Style_Saturation()
		{
			Graphics g = Graphics.FromImage( m_Output );

			float3	HSL = this.HSL;
			float3	hsl_start = float3.Zero;
			float3	hsl_end = float3.Zero;
			hsl_start.y = HSL.y;
			hsl_end.y = HSL.y;
			hsl_start.z = 1.0f;
			hsl_end.z = 0.0f;

			for ( int i = 0; i < this.Width - 4; i++ ) {	//	For each vertical line in the control:
				hsl_start.x = (float) i / (Width - 4);		//	Calculate Hue at this line (Saturation and Luminance are constant)
				hsl_end.x = hsl_start.x;
				
				LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(2,2, 1, this.Height - 4), AdobeColors.HSL_to_RGB_LDR(hsl_start), AdobeColors.HSL_to_RGB_LDR(hsl_end), 90, false); 
				g.FillRectangle(br,new System.Drawing.Rectangle(i + 2, 2, 1, this.Height - 4)); 
			}
		}


		/// <summary>
		/// Draws the content of the control filling in all color values with the provided Luminance or Brightness value.
		/// </summary>
		protected void Draw_Style_Brightness()
		{
			Graphics g = Graphics.FromImage( m_Output );

			float3	HSL = this.HSL;
			float3	hsl_start = float3.Zero;
			float3	hsl_end = float3.Zero;
			hsl_start.z = HSL.z;
			hsl_end.z = HSL.z;
			hsl_start.y = 1.0f;
			hsl_end.y = 0.0f;

			for ( int i = 0; i < Width - 4; i++ ) {		//	For each vertical line in the control:
				hsl_start.x = (float) i / (Width - 4);	//	Calculate Hue at this line (Saturation and Luminance are constant)
				hsl_end.x = hsl_start.x;
				
				LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(2,2, 1, this.Height - 4), AdobeColors.HSL_to_RGB_LDR(hsl_start), AdobeColors.HSL_to_RGB_LDR(hsl_end), 90, false); 
				g.FillRectangle(br,new System.Drawing.Rectangle(i + 2, 2, 1, this.Height - 4)); 
			}
		}


		/// <summary>
		/// Draws the content of the control filling in all color values with the provided Red value.
		/// </summary>
		protected void Draw_Style_Red()
		{
			Graphics g = Graphics.FromImage( m_Output );

			int red = AdobeColors.ConvertHDR2LDR( m_RGB ).R;

			for ( int i = 0; i < this.Height - 4; i++ )				//	For each horizontal line in the control:
			{
				//	Calculate Green at this line (Red and Blue are constant)
				int green = Round(255 - (255 * (double)i/(this.Height - 4)));

				LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(2,2, this.Width - 4, 1), Color.FromArgb(red, green, 0), Color.FromArgb(red, green, 255), 0, false); 
				g.FillRectangle(br,new System.Drawing.Rectangle(2,i + 2, this.Width - 4, 1)); 
			}
		}


		/// <summary>
		/// Draws the content of the control filling in all color values with the provided Green value.
		/// </summary>
		protected void Draw_Style_Green() {
			Graphics g = Graphics.FromImage( m_Output );

			int green = AdobeColors.ConvertHDR2LDR( m_RGB ).G;

			for ( int i = 0; i < this.Height - 4; i++ )	//	For each horizontal line in the control:
			{
				//	Calculate Red at this line (Green and Blue are constant)
				int red = Round(255 - (255 * (double)i/(this.Height - 4)));

				LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(2,2, this.Width - 4, 1), Color.FromArgb(red, green, 0), Color.FromArgb(red, green, 255), 0, false); 
				g.FillRectangle(br,new System.Drawing.Rectangle(2,i + 2, this.Width - 4, 1)); 
			}
		}


		/// <summary>
		/// Draws the content of the control filling in all color values with the provided Blue value.
		/// </summary>
		protected void Draw_Style_Blue() {
			Graphics g = Graphics.FromImage( m_Output );

			int blue = AdobeColors.ConvertHDR2LDR( m_RGB ).B;

			for ( int i = 0; i < this.Height - 4; i++ )	//	For each horizontal line in the control:
			{
				//	Calculate Green at this line (Red and Blue are constant)
				int green = Round(255 - (255 * (double)i/(this.Height - 4)));

				LinearGradientBrush br = new LinearGradientBrush(new System.Drawing.Rectangle(2,2, this.Width - 4, 1), Color.FromArgb(0, green, blue), Color.FromArgb(255, green, blue), 0, false); 
				g.FillRectangle(br,new System.Drawing.Rectangle(2,i + 2, this.Width - 4, 1)); 
			}
		}

		/// <summary>
		/// Draws the content of the control filling in all color values with the provided Luminance value.
		/// </summary>
		protected unsafe void	Draw_Style_Luminance() {
			BitmapData	lockedBitmap = m_Output.LockBits( new Rectangle( 0, 0, Width, Height ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );

			float3	Lab = AdobeColors.RGB2Lab_D65( m_RGB );
			float3	RGB;

			int		W = Width - 4;
			int		H = Height - 4;
			for ( int Y=0; Y < H; Y++ ) {
				byte*	scanlinePtr = (byte*) lockedBitmap.Scan0.ToPointer() + (2+Y) * lockedBitmap.Stride + (2*4);	// Add a margin of 2
				Lab.z = -(256.0f * Y / H - 128.0f);
				for ( int X=0; X < W; X++ ) {
					Lab.y = 256.0f * X / W - 128.0f;
					RGB = AdobeColors.Lab2RGB_D65( Lab );
					*scanlinePtr++ = (byte) Math.Max( 0, Math.Min( 255, 255 * RGB.z ));
					*scanlinePtr++ = (byte) Math.Max( 0, Math.Min( 255, 255 * RGB.y ));
					*scanlinePtr++ = (byte) Math.Max( 0, Math.Min( 255, 255 * RGB.x ));
					*scanlinePtr++ = 0xFF;
				}
			}

			m_Output.UnlockBits( lockedBitmap );
		}

		/// <summary>
		/// Draws the content of the control filling in all color values with the provided a* value.
		/// </summary>
		protected unsafe void	Draw_Style_a() {
		}

		/// <summary>
		/// Draws the content of the control filling in all color values with the provided b* value.
		/// </summary>
		protected unsafe void	Draw_Style_b() {
		}

		/// <summary>
		/// Calls all the functions neccessary to redraw the entire control.
		/// </summary>
		protected void Redraw_Control() {
			// Check we have a valid bitmap to paint into
			if ( m_Output == null ) {
				Graphics	Reference = this.CreateGraphics();
				m_Output = new Bitmap( Width, Height, Reference );
				Reference.Dispose();
			}

			DrawBorder();

			DrawContent();
// 			switch ( m_DrawStyle ) {
// 				case ColorPickerForm.DRAW_STYLE.Hue :
// 					Draw_Style_Hue();
// 					break;
// 				case ColorPickerForm.DRAW_STYLE.Saturation :
// 					Draw_Style_Saturation();
// 					break;
// 				case ColorPickerForm.DRAW_STYLE.Brightness :
// 					Draw_Style_Brightness();
// 					break;
// 				case ColorPickerForm.DRAW_STYLE.Red :
// 					Draw_Style_Red();
// 					break;
// 				case ColorPickerForm.DRAW_STYLE.Green :
// 					Draw_Style_Green();
// 					break;
// 				case ColorPickerForm.DRAW_STYLE.Blue :
// 					Draw_Style_Blue();
// 					break;
// 				case ColorPickerForm.DRAW_STYLE.Luminance:
// 					Draw_Style_Luminance();
// 					break;
// 			} 

			DrawMarker(m_iMarker_X, m_iMarker_Y, true);
			Invalidate();
		}


		/// <summary>
		/// Resets the marker position of the slider to match the controls color.  Gives the option of redrawing the slider.
		/// </summary>
		/// <param name="Redraw">Set to true if you want the function to redraw the slider after determining the best position</param>
		protected void Reset_Marker( bool Redraw ) {
			float3	HSL = this.HSL;
			switch ( m_DrawStyle ) {
				case ColorPickerForm.DRAW_STYLE.Hue :
					m_iMarker_X = Round( (this.Width - 4) * HSL.y );
					m_iMarker_Y = Round( (this.Height - 4) * (1.0 - Clamp( HSL.z )) );
					break;
				case ColorPickerForm.DRAW_STYLE.Saturation :
					m_iMarker_X = Round( (this.Width - 4) * HSL.x );
					m_iMarker_Y = Round( (this.Height - 4) * (1.0 - Clamp( HSL.z )) );
					break;
				case ColorPickerForm.DRAW_STYLE.Brightness :
					m_iMarker_X = Round( (this.Width - 4) * HSL.x );
					m_iMarker_Y = Round( (this.Height - 4) * (1.0 - HSL.y) );
					break;
				case ColorPickerForm.DRAW_STYLE.Red :
					m_iMarker_X = Round( (this.Width - 4) * (double) AdobeColors.ConvertHDR2LDR( m_RGB ).B );
					m_iMarker_Y = Round( (this.Height - 4) * (1.0 - (double) AdobeColors.ConvertHDR2LDR( m_RGB ).G ) );
					break;
				case ColorPickerForm.DRAW_STYLE.Green :
					m_iMarker_X = Round( (this.Width - 4) * (double) AdobeColors.ConvertHDR2LDR( m_RGB ).B );
					m_iMarker_Y = Round( (this.Height - 4) * (1.0 - (double) AdobeColors.ConvertHDR2LDR( m_RGB ).R) );
					break;
				case ColorPickerForm.DRAW_STYLE.Blue :
					m_iMarker_X = Round( (this.Width - 4) * (double) AdobeColors.ConvertHDR2LDR( m_RGB ).R );
					m_iMarker_Y = Round( (this.Height - 4) * (1.0 - (double) AdobeColors.ConvertHDR2LDR( m_RGB ).G) );
					break;
			}

			if ( Redraw )
				DrawMarker( m_iMarker_X, m_iMarker_Y, true );
		}


		/// <summary>
		/// Resets the controls color (both HSL and RGB variables) based on the current marker position
		/// </summary>
		protected void ResetHSLRGB() {
			float3	HSL = GetColor( m_iMarker_X, m_iMarker_Y );
			m_RGB = AdobeColors.HSB2RGB( HSL );
		}


		/// <summary>
		/// Kindof self explanitory, I really need to look up the .NET function that does this.
		/// </summary>
		/// <param name="val">double value to be rounded to an integer</param>
		/// <returns></returns>
		protected int Round(double val)
		{
			int ret_val = (int)val;
			
			int temp = (int)(val * 100);

			if ( (temp % 100) >= 50 )
				ret_val += 1;

			return ret_val;
			
		}

		protected double	Clamp( double _fValue )
		{
			return	Math.Max( 0.0, Math.Min( 1.0, _fValue ) );
		}

		/// <summary>
		/// Returns the graphed color at the x,y position on the control
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected float3	GetColor(int x, int y) {
			float	InvWidth = 1.0f / (Width - 4);
			float	InvHeight = 1.0f / (Height - 4);

			float3	Result = float3.One;

			float3	HSL = this.HSL;
			switch ( m_DrawStyle ) {
				case ColorPickerForm.DRAW_STYLE.Hue :
					Result.x = HSL.x;
					Result.y = x * InvWidth;
					Result.z = 1.0f - y * InvHeight;
					break;
				case ColorPickerForm.DRAW_STYLE.Saturation :
					Result.y = HSL.y;
					Result.x = x * InvWidth;
					Result.z = 1.0f - y * InvHeight;
					break;
				case ColorPickerForm.DRAW_STYLE.Brightness :
					Result.z = HSL.z;
					Result.x = x * InvWidth;
					Result.y = 1.0f - y * InvHeight;
					break;
				case ColorPickerForm.DRAW_STYLE.Red :
					Result = AdobeColors.RGB2HSB( new float3( m_RGB.x, 1.0f - y * InvHeight, x * InvWidth ) );
					break;
				case ColorPickerForm.DRAW_STYLE.Green :
					Result = AdobeColors.RGB2HSB( new float3( 1.0f - y * InvHeight, m_RGB.y, x * InvWidth ) );
					break;
				case ColorPickerForm.DRAW_STYLE.Blue :
					Result = AdobeColors.RGB2HSB( new float3( x * InvWidth, 1.0f - y * InvHeight, m_RGB.z ) );
					break;
			}

			return	Result;
		}

		#endregion

		#endregion
	}
}
