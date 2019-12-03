/******************************************************************/
/*****                                                        *****/
/*****     Project:           Adobe Color Picker Clone 1      *****/
/*****     Filename:          VerticalColorSliderControl.cs      *****/
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

using SharpMath;

namespace UIUtility
{
	/// <summary>
	/// A vertical slider control that shows a range for a color property (a.k.a. Hue, Saturation, Brightness,
	/// Red, Green, Blue) and sends an event when the slider is changed.
	/// </summary>
	public class	VerticalColorSliderControl : System.Windows.Forms.UserControl
	{
		#region CONSTANTS

		protected const float	MIN_COMPONENT_VALUE	= 1e-3f;

		#endregion

		#region FIELDS

		protected Bitmap	m_Output = null;

		//	Slider properties
		protected int			m_iMarker_Start_Y = 0;
		protected bool			m_bDragging = false;

		//	These variables keep track of how to fill in the content inside the box;
		protected ColorPickerForm.DRAW_STYLE	m_DrawStyle = ColorPickerForm.DRAW_STYLE.Hue;
		protected float3						m_RGB = AdobeColors.HSB2RGB( float3.One );

		protected System.ComponentModel.Container components = null;

		#endregion

		#region PROPERTIES

		public new event EventHandler Scroll;

		/// <summary>
		/// The drawstyle of the contol (Hue, Saturation, Brightness, Red, Green or Blue)
		/// </summary>
		public ColorPickerForm.DRAW_STYLE	DrawStyle
		{
			get { return m_DrawStyle; }
			set {
				m_DrawStyle = value;

				//	Redraw the control based on the new ColorPickerForm.DRAW_STYLE
				Reset_Slider(true);
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
				Reset_Slider( true );
				DrawContent();
			}
		}

		/// <summary>
		/// The RGB color of the control, changing the RGB will automatically change the HSL color for the control.
		/// </summary>
		public float3		RGB {
			get { return m_RGB; }
			set {
				m_RGB = new float3( Math.Max( value.x, MIN_COMPONENT_VALUE ), Math.Max( value.y, MIN_COMPONENT_VALUE ), Math.Max( value.z, MIN_COMPONENT_VALUE ) );

				//	Redraw the control based on the new color.
				Reset_Slider( true );
				DrawContent();
			}
		}

		#endregion

		#region METHODS

		public VerticalColorSliderControl()
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

				m_pencilSlider.Dispose();
				m_pencilBorderTopLeft.Dispose();

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
			// ctrl1DColorBar
			// 
			this.Name = "ctrl1DColorBar";
			this.Size = new System.Drawing.Size(40, 264);

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

			int y;
			y = e.Y;
			y -= 4;											//	Calculate slider position
			if ( y < 0 ) y = 0;
			if ( y > this.Height - 9 ) y = this.Height - 9;

			if ( y == m_iMarker_Start_Y )					//	If the slider hasn't moved, no need to redraw it.
				return;										//	or send a scroll notification

			DrawSlider(y, false);	//	Redraw the slider
			ResetHSLRGB();			//	Reset the color

			Refresh();

			if ( Scroll != null )	//	Notify anyone who cares that the controls slider(color) has changed
				Scroll(this, e);
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			if ( !m_bDragging )		//	Only respond when the mouse is dragging the marker.
				return;

			int y;
			y = e.Y;
			y -= 4; 										//	Calculate slider position
			if ( y < 0 ) y = 0;
			if ( y > this.Height - 9 ) y = this.Height - 9;

			if ( y == m_iMarker_Start_Y )					//	If the slider hasn't moved, no need to redraw it.
				return;										//	or send a scroll notification

			DrawSlider(y, false);	//	Redraw the slider
			ResetHSLRGB();			//	Reset the color

			Refresh();

			if ( Scroll != null )	//	Notify anyone who cares that the controls slider(color) has changed
				Scroll(this, e);
		}

		protected override void OnMouseUp( MouseEventArgs e )
		{
			base.OnMouseUp( e );

			if ( e.Button != MouseButtons.Left )	//	Only respond to left mouse button events
				return;

			m_bDragging = false;

			int y;
			y = e.Y;
			y -= 4; 										//	Calculate slider position
			if ( y < 0 ) y = 0;
			if ( y > this.Height - 9 ) y = this.Height - 9;

			if ( y == m_iMarker_Start_Y )					//	If the slider hasn't moved, no need to redraw it.
				return;										//	or send a scroll notification

			DrawSlider(y, false);	//	Redraw the slider
			ResetHSLRGB();			//	Reset the color

			Refresh();

			if ( Scroll != null )	//	Notify anyone who cares that the controls slider(color) has changed
				Scroll(this, e);
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

		#region Drawing Methods

		/// <summary>
		/// Redraws the background over the slider area on both sides of the control
		/// </summary>
		protected void ClearSlider() {
			Graphics g = Graphics.FromImage( m_Output );
			Brush brush = System.Drawing.SystemBrushes.Control;
			g.FillRectangle(brush, 0, 0, 9, this.Height);					//	clear left hand slider
			g.FillRectangle(brush, this.Width - 10, 0, 10, this.Height);	//	clear right hand slider
			g.FillRectangle(brush, 0, 0, Width, 2);							//	clear top
			g.FillRectangle(brush, 0, Height-2, Width, 2);					//	clear bottom
			g.Dispose();
		}

		/// <summary>
		/// Draws the slider arrows on both sides of the control.
		/// </summary>
		/// <param name="position">position value of the slider, lowest being at the bottom.  The range
		/// is between 0 and the controls height-9.  The values will be adjusted if too large/small</param>
		/// <param name="Unconditional">If Unconditional is true, the slider is drawn, otherwise some logic 
		/// is performed to determine is drawing is really neccessary.</param>
		Pen m_pencilSlider = new Pen(Color.FromArgb(116,114,106));	//	Same gray color Photoshop uses
		protected void DrawSlider(int position, bool Unconditional) {
			if ( position < 0 ) position = 0;
			if ( position > this.Height - 9 ) position = this.Height - 9;

			if ( m_iMarker_Start_Y == position && !Unconditional )	//	If the marker position hasn't changed
				return;												//	since the last time it was drawn and we don't HAVE to redraw
			//	then exit procedure

			m_iMarker_Start_Y = position;	//	Update the controls marker position

			this.ClearSlider();		//	Remove old slider

			Graphics g = Graphics.FromImage( m_Output );

			Brush brush = Brushes.White;
			
			Point[] arrow = new Point[7];				//	 GGG
			arrow[0] = new Point(1,position);			//	G   G
			arrow[1] = new Point(3,position);			//	G    G
			arrow[2] = new Point(7,position + 4);		//	G     G
			arrow[3] = new Point(3,position + 8);		//	G      G
			arrow[4] = new Point(1,position + 8);		//	G     G
			arrow[5] = new Point(0,position + 7);		//	G    G
			arrow[6] = new Point(0,position + 1);		//	G   G
														//	 GGG

			g.FillPolygon(brush, arrow);			//	Fill left arrow with white
			g.DrawPolygon(m_pencilSlider, arrow);	//	Draw left arrow border with gray

																//	    GGG
			arrow[0] = new Point(this.Width - 2,position);		//	   G   G
			arrow[1] = new Point(this.Width - 4,position);		//	  G    G
			arrow[2] = new Point(this.Width - 8,position + 4);	//	 G     G
			arrow[3] = new Point(this.Width - 4,position + 8);	//	G      G
			arrow[4] = new Point(this.Width - 2,position + 8);	//	 G     G
			arrow[5] = new Point(this.Width - 1,position + 7);	//	  G    G
			arrow[6] = new Point(this.Width - 1,position + 1);	//	   G   G
																//	    GGG

			g.FillPolygon(brush, arrow);			//	Fill right arrow with white
			g.DrawPolygon(m_pencilSlider, arrow);	//	Draw right arrow border with gray

			g.Dispose();
		}

		/// <summary>
		/// Draws the border around the control, in this case the border around the content area between
		/// the slider arrows.
		/// </summary>
		Pen	m_pencilBorderTopLeft = new Pen(Color.FromArgb(172,168,153));	//	The same gray color used by Photoshop
		protected void DrawBorder() {
			Graphics g = Graphics.FromImage( m_Output );

			//	To make the control look like Adobe Photoshop's the border around the control will be a gray line
			//	on the top and left side, a white line on the bottom and right side, and a black rectangle (line) 
			//	inside the gray/white rectangle
			g.DrawLine(m_pencilBorderTopLeft, this.Width - 10, 2, 9, 2);	//	Draw top line
			g.DrawLine(m_pencilBorderTopLeft, 9, 2, 9, this.Height - 4);	//	Draw left hand line

			g.DrawLine( Pens.White, this.Width - 9, 2, this.Width - 9,this.Height - 3);	//	Draw right hand line
			g.DrawLine( Pens.White, this.Width - 9,this.Height - 3, 9,this.Height - 3);	//	Draw bottome line

			g.DrawRectangle( Pens.Black, 10, 3, this.Width - 20, this.Height - 7);	//	Draw inner black rectangle

			g.Dispose();
		}

		/// <summary>
		/// Evaluates the DrawStyle of the control and calls the appropriate
		/// drawing function for content
		/// </summary>
		protected void DrawContent() {
			switch (m_DrawStyle) {
				case ColorPickerForm.DRAW_STYLE.Hue :
					Draw_Style_Hue();
					break;
				case ColorPickerForm.DRAW_STYLE.Saturation :
					Draw_Style_Saturation();
					break;
				case ColorPickerForm.DRAW_STYLE.Brightness :
					Draw_Style_Luminance();
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
			}
		}

		#region Draw_Style_X - Content drawing functions

		//	The following functions do the real work of the control, drawing the primary content (the area between the slider)
		//	

		/// <summary>
		/// Fills in the content of the control showing all values of Hue (from 0 to 360)
		/// </summary>
		protected void Draw_Style_Hue() {
			Graphics g = Graphics.FromImage( m_Output );

			float3	HSL = new float3();
					HSL.y = 1.0f;	//	S and L will both be at 100% for this DrawStyle
					HSL.z = 1.0f;

			for ( int i = 0; i < this.Height - 8; i++ )	{ //	i represents the current line of pixels we want to draw horizontally
				HSL.x = 1.0f - (float) i / (Height - 8);				//	H (hue) is based on the current vertical position
				Pen pen = new Pen( AdobeColors.HSL_to_RGB_LDR( HSL ) );	//	Get the Color for this line

				g.DrawLine(pen, 11, i + 4, this.Width - 11, i + 4);	//	Draw the line and loop back for next line

				pen.Dispose();
			}

			g.Dispose();
		}


		/// <summary>
		/// Fills in the content of the control showing all values of Saturation (0 to 100%) for the given
		/// Hue and Luminance.
		/// </summary>
		protected void Draw_Style_Saturation() {
			Graphics g = Graphics.FromImage( m_Output );

			float3	HSL = this.HSL;	//	Use the H and L values of the current color (m_HSL)
			for ( int i = 0; i < this.Height - 8; i++ ) { //	i represents the current line of pixels we want to draw horizontally
				HSL.y = 1.0f - (float) i / (this.Height - 8);				//	S (Saturation) is based on the current vertical position
				Pen pen = new Pen( AdobeColors.HSL_to_RGB_LDR( HSL ) );	//	Get the Color for this line

				g.DrawLine(pen, 11, i + 4, this.Width - 11, i + 4);	//	Draw the line and loop back for next line

				pen.Dispose();
			}
			g.Dispose();
		}


		/// <summary>
		/// Fills in the content of the control showing all values of Luminance (0 to 100%) for the given
		/// Hue and Saturation.
		/// </summary>
		protected void Draw_Style_Luminance() {
			Graphics g = Graphics.FromImage( m_Output );

			float3	HSL = this.HSL;	//	Use the H and S values of the current color (m_HSL)
			for ( int i = 0; i < this.Height - 8; i++ ) {	//	i represents the current line of pixels we want to draw horizontally
				HSL.z = 1.0f - (float) i / (Height - 8);				//	L (Luminance) is based on the current vertical position
				Pen pen = new Pen( AdobeColors.HSL_to_RGB_LDR( HSL ) );	//	Get the Color for this line

				g.DrawLine(pen, 11, i + 4, this.Width - 11, i + 4);	//	Draw the line and loop back for next line

				pen.Dispose();
			}
			g.Dispose();
		}


		/// <summary>
		/// Fills in the content of the control showing all values of Red (0 to 1) for the given Green and Blue.
		/// </summary>
		protected void Draw_Style_Red()
		{
			Graphics g = Graphics.FromImage( m_Output );

			for ( int i = 0; i < this.Height - 8; i++ ) //	i represents the current line of pixels we want to draw horizontally
			{
				float	red = 1.0f - (float) i / (Height - 8);									//	red is based on the current vertical position
				Pen		pen = new Pen( AdobeColors.ConvertHDR2LDR( new float3( red, m_RGB.y, m_RGB.z ) ) );	//	Get the Color for this line

				g.DrawLine(pen, 11, i + 4, this.Width - 11, i + 4);			//	Draw the line and loop back for next line

				pen.Dispose();
			}
			g.Dispose();
		}


		/// <summary>
		/// Fills in the content of the control showing all values of Green (0 to 1) for the given Red and Blue.
		/// </summary>
		protected void Draw_Style_Green()
		{
			Graphics g = Graphics.FromImage( m_Output );

			for ( int i = 0; i < this.Height - 8; i++ ) //	i represents the current line of pixels we want to draw horizontally
			{
				float	green = 1.0f - (float) i/ (Height - 8);										//	green is based on the current vertical position
				Pen		pen = new Pen( AdobeColors.ConvertHDR2LDR( new float3( m_RGB.x, green, m_RGB.z ) ) );	//	Get the Color for this line

				g.DrawLine(pen, 11, i + 4, this.Width - 11, i + 4);			//	Draw the line and loop back for next line

				pen.Dispose();
			}
			g.Dispose();
		}


		/// <summary>
		/// Fills in the content of the control showing all values of Blue (0 to 1) for the given Red and Green.
		/// </summary>
		protected void Draw_Style_Blue()
		{
			Graphics g = Graphics.FromImage( m_Output );

			for ( int i = 0; i < this.Height - 8; i++ ) //	i represents the current line of pixels we want to draw horizontally
			{
				float	blue = 1.0f - (float) i/ (Height - 8);										//	blue is based on the current vertical position
				Pen		pen = new Pen( AdobeColors.ConvertHDR2LDR( new float3( m_RGB.x, m_RGB.y, blue ) ) );	//	Get the Color for this line

				g.DrawLine(pen, 11, i + 4, this.Width - 11, i + 4);			//	Draw the line and loop back for next line

				pen.Dispose();
			}
			g.Dispose();
		}


		#endregion

		#endregion

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

			DrawSlider(m_iMarker_Start_Y, true);
			DrawBorder();
			switch (m_DrawStyle) {
				case ColorPickerForm.DRAW_STYLE.Hue :
					Draw_Style_Hue();
					break;
				case ColorPickerForm.DRAW_STYLE.Saturation :
					Draw_Style_Saturation();
					break;
				case ColorPickerForm.DRAW_STYLE.Brightness :
					Draw_Style_Luminance();
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
			}

			Invalidate();
		}

		/// <summary>
		/// Resets the vertical position of the slider to match the controls color.  Gives the option of redrawing the slider.
		/// </summary>
		/// <param name="Redraw">Set to true if you want the function to redraw the slider after determining the best position</param>
		protected void Reset_Slider(bool Redraw) {
			float3	HSL = this.HSL;

			//	The position of the marker (slider) changes based on the current drawstyle:
			switch (m_DrawStyle) {
				case ColorPickerForm.DRAW_STYLE.Hue :
					m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * HSL.x );
					break;
				case ColorPickerForm.DRAW_STYLE.Saturation :
					m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * HSL.y );
					break;
				case ColorPickerForm.DRAW_STYLE.Brightness :
					m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * Clamp( HSL.z ) );
					break;
				case ColorPickerForm.DRAW_STYLE.Red :
					m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * Clamp( m_RGB.x ) );
					break;
				case ColorPickerForm.DRAW_STYLE.Green :
					m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * Clamp( m_RGB.y ) );
					break;
				case ColorPickerForm.DRAW_STYLE.Blue :
					m_iMarker_Start_Y = (Height - 8) - Round( (Height - 8) * Clamp( m_RGB.z ) );
					break;
			}

			if ( Redraw )
				DrawSlider(m_iMarker_Start_Y, true);
		}

		/// <summary>
		/// Resets the controls color (both HSL and RGB variables) based on the current slider position
		/// </summary>
		protected void ResetHSLRGB() {
			float3	HSL = this.HSL;

			float	InvHeight = 1.0f / (Height - 9);
			switch ( m_DrawStyle ) {
				case ColorPickerForm.DRAW_STYLE.Hue :
					HSL.x = 1.0f - m_iMarker_Start_Y * InvHeight;
					m_RGB = AdobeColors.HSB2RGB(HSL);
					break;
				case ColorPickerForm.DRAW_STYLE.Saturation :
					HSL.y = 1.0f - m_iMarker_Start_Y * InvHeight;
					m_RGB = AdobeColors.HSB2RGB(HSL);
					break;
				case ColorPickerForm.DRAW_STYLE.Brightness :
					HSL.z = 1.0f - m_iMarker_Start_Y * InvHeight;
					m_RGB = AdobeColors.HSB2RGB( HSL );
					break;
				case ColorPickerForm.DRAW_STYLE.Red :
					m_RGB = new float3( 1.0f - (float) (m_iMarker_Start_Y * InvHeight), m_RGB.y, m_RGB.z );
					break;
				case ColorPickerForm.DRAW_STYLE.Green :
					m_RGB = new float3( m_RGB.x, 1.0f - (float) (m_iMarker_Start_Y * InvHeight), m_RGB.z );
					break;
				case ColorPickerForm.DRAW_STYLE.Blue :
					m_RGB = new float3( m_RGB.x, m_RGB.y, 1.0f - (float) (m_iMarker_Start_Y * InvHeight) );
					break;
			}
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

// 		protected float		Clamp( float _fValue )
// 		{
// 			return	Math.Max( 0.0f, Math.Min( 1.0f, _fValue ) );
// 		}

		protected double	Clamp( double _fValue )
		{
			return	Math.Max( 0.0, Math.Min( 1.0, _fValue ) );
		}

		#endregion
	}
}
