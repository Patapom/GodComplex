/******************************************************************/
/*****                                                        *****/
/*****     Project:           Adobe Color Picker Clone 1      *****/
/*****     Filename:          ColorPickerForm.cs              *****/
/*****     Original Author:   Danny Blanchard                 *****/
/*****                        - scrabcakes@gmail.com          *****/
/*****     Updates:	                                          *****/
/*****      3/28/2005 - Initial Version : Danny Blanchard     *****/
/*****                                                        *****/
/******************************************************************/
//
// And also by Patapom ^_^ :
//
// • an upgrade to support Alpha
// • Full HDR refactor
// • Palette support with storage in the Registry
// • various code factoring and optimization
// • used nice sliders
//

using System;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using SharpMath;

namespace Nuaj.Cirrus.Utility
{
	/// <summary>
	/// An improved, photoshop-like color picker with Alpha and HDR colors support
	/// </summary>
	public partial class ColorPickerForm : System.Windows.Forms.Form {
		#region CONSTANTS

		protected const float	MIN_COMPONENT_VALUE	= 0.0f;//1e-3f;

		protected const int		PALETTE_ENTRIES = 3 * 12;	// 3 rows of 12 buttons

		#endregion

		#region NESTED TYPES

		[Flags()]
		public enum		VECTOR_DAMAGE_ON_EDITION	{	NONE = 0,
														XYZ_WILL_BE_MADE_POSITIVE = 1,
														ALPHA_WILL_BE_MADE_POSITIVE = 2,
														INVALID = -1
													};

		public enum		DONT_REFRESH				{	NONE,
														COLOR_SLIDER,
														ALPHA_SLIDER,
														COLOR_BOX
													};

		public enum	DRAW_STYLE {
			// HSB
			Hue,
			Saturation,
			Brightness,

			// RGB
			Red,
			Green,
			Blue,

			// L*a*b*
			Luminance,
			a,
			b,
		}

		/// <summary>
		/// The delegate to use to subscribe to the ColorChanged event
		/// </summary>
		/// <param name="_Sender">The color picker whose color changed</param>
		public delegate void				ColorChangedEventHandler( ColorPickerForm _sender );

		#endregion

		#region FIELDS

		protected bool					m_initialized = false;
		protected float4				m_initialColor = float4.Zero;

//		protected float3				m_HSL = float3.One;
		protected float4				m_RGB = new float4( AdobeColors.HSB2RGB( float3.One ), 1 );

		protected float4				m_primaryColor = float4.Zero;
		protected float4				m_secondaryColor = float4.Zero;

		protected DONT_REFRESH			m_dontRefresh = DONT_REFRESH.NONE;

		#endregion

		#region PROPERTIES

		public float4	ColorHDR {
			get { return m_RGB; }
			set {
				if ( !m_initialized ) {
					m_initialColor = value;
					m_initialized = true;
				}

				// Setup RGB & HSL colors
				float3	tempValue = new float3( Math.Max( 0.0f, value.x ), Math.Max( 0.0f, value.y ), Math.Max( 0.0f, value.z ) );
				if ( tempValue.LengthSquared < MIN_COMPONENT_VALUE * MIN_COMPONENT_VALUE )
					m_RGB = new float4( Math.Max( value.x, MIN_COMPONENT_VALUE ), Math.Max( value.y, MIN_COMPONENT_VALUE ), Math.Max( value.z, MIN_COMPONENT_VALUE ), value.w );
				else
					m_RGB = new float4( tempValue.x, tempValue.y, tempValue.z, value.w );

				// Setup color & alpha boxes
				if ( m_dontRefresh != DONT_REFRESH.COLOR_BOX )
					colorBoxControl.RGB = (float3) m_RGB;
				if ( m_dontRefresh != DONT_REFRESH.COLOR_SLIDER )
					sliderControlHSL.RGB = (float3) m_RGB;
				m_dontRefresh = DONT_REFRESH.NONE;

				// Setup primary & secondary colors
				m_primaryColor = value;
				labelPrimaryColor.BackColor = AdobeColors.ConvertHDR2LDR( (float3) m_primaryColor );
				if ( m_secondaryColor == float4.Zero ) {
					m_secondaryColor = value;
					labelSecondaryColor.BackColor = AdobeColors.ConvertHDR2LDR( (float3) m_secondaryColor );
				}

				// Update text boxes
				UpdateTextBoxes();

				// Notify of a color change
				if ( ColorChanged != null )
					ColorChanged( this );
			}
		}

		public Color		ColorLDR {
			get { return ConvertHDR2LDR( m_RGB ); }
			set { ColorHDR = AdobeColors.RGB_LDR_to_RGB_HDR( value ); }
		}

		protected DRAW_STYLE	DrawStyle {
			get {
				if ( buttonHue.Checked )
					return DRAW_STYLE.Hue;
				else if ( buttonSaturation.Checked )
					return DRAW_STYLE.Saturation;
				else if ( buttonBrightness.Checked )
					return DRAW_STYLE.Brightness;
				else if ( buttonRed.Checked )
					return DRAW_STYLE.Red;
				else if ( buttonGreen.Checked )
					return DRAW_STYLE.Green;
				else if ( buttonBlue.Checked )
					return DRAW_STYLE.Blue;
				else if ( radioButtonL.Checked )
					return DRAW_STYLE.Luminance;
				else if ( radioButtona.Checked )
					return DRAW_STYLE.a;
				else if ( radioButtonb.Checked )
					return DRAW_STYLE.b;
				else
					return DRAW_STYLE.Hue;
			}
			set {
				switch ( value ) {
					case DRAW_STYLE.Hue :
						buttonHue.Checked = true;
						break;
					case DRAW_STYLE.Saturation :
						buttonSaturation.Checked = true;
						break;
					case DRAW_STYLE.Brightness :
						buttonBrightness.Checked = true;
						break;
					case DRAW_STYLE.Red :
						buttonRed.Checked = true;
						break;
					case DRAW_STYLE.Green :
						buttonGreen.Checked = true;
						break;
					case DRAW_STYLE.Blue :
						buttonBlue.Checked = true;
						break;
					case DRAW_STYLE.Luminance :
						radioButtonL.Checked = true;
						break;
					case DRAW_STYLE.a :
						radioButtona.Checked = true;
						break;
					case DRAW_STYLE.b :
						radioButtonb.Checked = true;
						break;
					default :
						buttonHue.Checked = true;
						break;
				}
			}
		}

		/// <summary>
		/// The event to susbcribe to to be notified the color changed
		/// </summary>
		public event ColorChangedEventHandler		ColorChanged;


		// [PATACODE]
		/// <summary>
		/// This accessor allows to setup the RGB color without erasing the alpha
		/// It should be used by all methods that assign the RGB color from other color spaces so alpha is preserved.
		/// Methods dealing with RGB color space should access m_RGB directly though...
		/// </summary>
		protected float3	RGB {
			get { return (float3) m_RGB; }
			set { ColorHDR = new float4( value.x, value.y, value.z, m_RGB.w ); }
		}
		// [PATACODE]

		#endregion

		#region METHODS

		public ColorPickerForm() {
			InitializeComponent();
			CustomInit();
		}

		public ColorPickerForm( float4 _ColorHDR ) {
			InitializeComponent();
			CustomInit();

			ColorHDR = _ColorHDR;
		}

		public ColorPickerForm( Color _ColorLDR ) {
			InitializeComponent();
			CustomInit();

			ColorLDR = _ColorLDR;
		}

		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			// Restore last location
			string	lastLocationXStr = GetRegistryValue( "LastDialogPositionX", null );
			string	lastLocationYStr = GetRegistryValue( "LastDialogPositionY", null );
			if ( lastLocationXStr != null && lastLocationYStr != null ) {
				int	X, Y;
				if ( int.TryParse( lastLocationXStr, out X ) && int.TryParse( lastLocationYStr, out Y ) )
					this.DesktopLocation = new Point( X, Y );
			}
		}

		protected override void OnClosing( CancelEventArgs e ) {
			base.OnClosing( e );

			SetRegistryValue( "LastDialogPositionX", this.DesktopLocation.X.ToString() );
			SetRegistryValue( "LastDialogPositionY", this.DesktopLocation.Y.ToString() );

			if ( e.Cancel || DialogResult != DialogResult.Cancel )
				return;

			// Cancel any edition and restore initial color!
			ColorHDR = m_initialColor;
		}

		/// <summary>
		/// Custom Control initialization
		/// </summary>
		protected void	CustomInit() {
			// Subscribe to the palette buttons' events
			FieldInfo[]	Fields = this.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			foreach ( FieldInfo Field in Fields ) {
				if ( Field.FieldType != typeof(PaletteButton) )
					continue;
				if ( Field.Name.IndexOf( "radioButtonPalette" ) == -1 )
					continue;

				PaletteButton	Button = Field.GetValue( this ) as PaletteButton;
								Button.DoubleClick += new EventHandler( RadioButtonPalette_DoubleClick );
								Button.SelectedChanged += new EventHandler( RadioButtonPalette_SelectedChanged );
			}

			// Setup the palette buttons' back colors
			for ( int PaletteIndex=0; PaletteIndex < PALETTE_ENTRIES; PaletteIndex++ )
				UpdatePaletteButtonColor( PaletteIndex );
		}

		protected void WriteHexData( float4 _RGB ) {
			Color	RGB = ConvertHDR2LDR( _RGB );
			textBoxHexa.Text = RGB.R.ToString( "X02" ) + RGB.G.ToString( "X02" ) + RGB.B.ToString( "X02" ) + RGB.A.ToString( "X02" );
			textBoxHexa.Update();
		}

		protected void	UpdateTextBoxes() {
			float3	HSL = AdobeColors.RGB2HSB( (float3) m_RGB );
			floatTrackbarControlHue.Value = HSL.x * 360.0f;
			floatTrackbarControlSaturation.Value = HSL.y;
			floatTrackbarControlLuminance.Value = HSL.z;

			floatTrackbarControlRed.Value = m_RGB.x;
			floatTrackbarControlGreen.Value = m_RGB.y;
			floatTrackbarControlBlue.Value = m_RGB.z;
			floatTrackbarControlAlpha.Value = m_RGB.w;

			// Update RGB gradients
			Color	LDR = ColorLDR;
			floatTrackbarControlRed.ColorMin = Color.FromArgb( 0, LDR.G, LDR.B );
			floatTrackbarControlRed.ColorMax = Color.FromArgb( LDR.R, LDR.G, LDR.B );
			floatTrackbarControlGreen.ColorMin = Color.FromArgb( LDR.R, 0, LDR.B );
			floatTrackbarControlGreen.ColorMax = Color.FromArgb( LDR.R, LDR.G, LDR.B );
			floatTrackbarControlBlue.ColorMin = Color.FromArgb( LDR.R, LDR.G, 0 );
			floatTrackbarControlBlue.ColorMax = Color.FromArgb( LDR.R, LDR.G, LDR.B );
			floatTrackbarControlAlpha.ColorMin = Color.FromArgb( 128, 0, 0, 0 );
			floatTrackbarControlAlpha.ColorMax = Color.FromArgb( 128 + LDR.A / 2, LDR.A, LDR.A, LDR.A );

			// Update hexa LDR value
			WriteHexData( m_RGB );
		}

		#region Registry

		internal static string	GetRegistryValue( string _keyName, string _defaultValue ) {
			Microsoft.Win32.RegistryKey	KeyRoot = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\ColorPicker" );
			string	value = KeyRoot.GetValue( _keyName, _defaultValue ) as string;
			return value;
		}

		internal static void	SetRegistryValue( string _keyName, string _value ) {
			Microsoft.Win32.RegistryKey	KeyRoot = Microsoft.Win32.Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\ColorPicker" );
										KeyRoot.SetValue( _keyName, _value );
		}

		#endregion

		#region Static Palette Access

		public static float4	GetPaletteColor( int _paletteIndex ) {
			string	paletteEntry = GetRegistryValue( "Entry" + _paletteIndex, null );
			if ( paletteEntry != null && paletteEntry != "" ) {
				float4	Result = float4.Zero;
				if ( float4.TryParse( paletteEntry, ref Result ) )
					return Result;
			}

			// If we get here, we have no color for that palette entry so we create a default one...
			switch ( _paletteIndex ) {
				case	0:
					return new float4( 0.0f, 0.0f, 0.0f, 1.0f );
				case	1:
					return new float4( 1.0f, 1.0f, 1.0f, 1.0f );
				case	2:
					return new float4( 1.0f, 0.0f, 0.0f, 1.0f );
				case	3:
					return new float4( 1.0f, 1.0f, 0.0f, 1.0f );
				case	4:
					return new float4( 0.0f, 1.0f, 0.0f, 1.0f );
				case	5:
					return new float4( 0.0f, 1.0f, 1.0f, 1.0f );
				case	6:
					return new float4( 0.0f, 0.0f, 1.0f, 1.0f );
				case	7:
					return new float4( 1.0f, 0.0f, 1.0f, 1.0f );
				case	8:
					return new float4( 0.2f, 0.0f, 0.0f, 1.0f );
				case	9:
					return new float4( 0.4f, 0.0f, 0.0f, 1.0f );
				case	10:
					return new float4( 0.6f, 0.0f, 0.0f, 1.0f );
				case	11:
					return new float4( 0.8f, 0.0f, 0.0f, 1.0f );

				case	12:
					return new float4( 0.113f, 0.113f, 0.113f, 1.0f );
				case	13:									   
					return new float4( 0.225f, 0.225f, 0.225f, 1.0f );
				case	14:									   
					return new float4( 0.338f, 0.338f, 0.338f, 1.0f );
				case	15:									   
					return new float4( 0.450f, 0.450f, 0.450f, 1.0f );
				case	16:									   
					return new float4( 0.562f, 0.562f, 0.562f, 1.0f );
				case	17:									   
					return new float4( 0.675f, 0.675f, 0.675f, 1.0f );
				case	18:									   
					return new float4( 0.788f, 0.788f, 0.788f, 1.0f );
				case	19:									   
					return new float4( 0.900f, 0.900f, 0.900f, 1.0f );
				case	20:
					return new float4( 0.0f, 0.2f, 0.0f, 1.0f );
				case	21:
					return new float4( 0.0f, 0.4f, 0.0f, 1.0f );
				case	22:
					return new float4( 0.0f, 0.6f, 0.0f, 1.0f );
				case	23:
					return new float4( 0.0f, 0.8f, 0.0f, 1.0f );

				case	24:
					return new float4( 0.113f, 0.113f, 0.0f, 1.0f );
				case	25:					   				 
					return new float4( 0.225f, 0.225f, 0.0f, 1.0f );
				case	26:					   				 
					return new float4( 0.338f, 0.338f, 0.0f, 1.0f );
				case	27:					   				 
					return new float4( 0.450f, 0.450f, 0.0f, 1.0f );
				case	28:					   				 
					return new float4( 0.562f, 0.562f, 0.0f, 1.0f );
				case	29:					   				 
					return new float4( 0.675f, 0.675f, 0.0f, 1.0f );
				case	30:					   				 
					return new float4( 0.788f, 0.788f, 0.0f, 1.0f );
				case	31:					   				 
					return new float4( 0.900f, 0.900f, 0.0f, 1.0f );
				case	32:
					return new float4( 0.0f, 0.0f, 0.2f, 1.0f );
				case	33:
					return new float4( 0.0f, 0.0f, 0.4f, 1.0f );
				case	34:
					return new float4( 0.0f, 0.0f, 0.6f, 1.0f );
				case	35:
					return new float4( 0.0f, 0.0f, 0.8f, 1.0f );
			}

			return	float4.Zero;
		}

		public static void		SetPaletteColor( int _paletteIndex, float4 _HDRColor ) {
			SetRegistryValue( "Entry" + _paletteIndex, _HDRColor.ToString() );
		}

		// Retrieves the index of the palette entry given a palette radio button
		//
		protected int	GetPaletteButtonIndex( PaletteButton _Button ) {
			return	int.Parse( _Button.Name.Replace( "radioButtonPalette", "" ) );
		}

		// Retrieves the index of the palette entry given a palette radio button
		//
		protected PaletteButton	GetPaletteButton( int _PaletteIndex ) {
			FieldInfo[]	Fields = this.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			foreach ( FieldInfo Field in Fields ) {
				if ( Field.FieldType != typeof(PaletteButton) )
					continue;
				if ( Field.Name.IndexOf( "radioButtonPalette" ) == -1 )
					continue;

				PaletteButton	Button = Field.GetValue( this ) as PaletteButton;

				if ( GetPaletteButtonIndex( Button ) == _PaletteIndex )
					return	Button;
			}

			return	null;
		}

		// Retrieves the index of the selected palette button
		//
		protected int	GetSelectedPaletteButtonIndex() {
			FieldInfo[]	Fields = this.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			foreach ( FieldInfo Field in Fields )
			{
				if ( Field.FieldType != typeof(PaletteButton) )
					continue;
				if ( Field.Name.IndexOf( "radioButtonPalette" ) == -1 )
					continue;

				// Check the button's is checked
				PaletteButton	Button = Field.GetValue( this ) as PaletteButton;
				if ( Button.Selected )
					return	GetPaletteButtonIndex( Button );
			}

			return	-1;
		}

		// Retrieves the index of the selected palette button
		//
		protected void	SetSelectedPaletteButton( PaletteButton _Button ) {
			FieldInfo[]	Fields = this.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			foreach ( FieldInfo Field in Fields )
			{
				if ( Field.FieldType != typeof(PaletteButton) )
					continue;
				if ( Field.Name.IndexOf( "radioButtonPalette" ) == -1 )
					continue;

				// Check the button's is checked
				PaletteButton	Button = Field.GetValue( this ) as PaletteButton;
								Button.Selected = Button == _Button;
			}
		}

		// Updates the palette button's back color based on the associated palette entry
		//
		protected void	UpdatePaletteButtonColor( int _PaletteIndex ) {
			PaletteButton	Button = GetPaletteButton( _PaletteIndex );
							Button.Vector = GetPaletteColor( _PaletteIndex );
		}

		#endregion

		/// <summary>
		/// This helper method will tell if the color picker can safely edit the provided vector without damaging it
		/// Indeed, a vector must have certain characteristics to be used as a HDR color
		/// </summary>
		/// <param name="_Vector">The vector to test</param>
		/// <returns>A combination of damage flags. If none is set, then the vector can be safely edited without damage</returns>
		public static VECTOR_DAMAGE_ON_EDITION	GetVectorDamage( float4 _Vector ) {
			VECTOR_DAMAGE_ON_EDITION	Result = VECTOR_DAMAGE_ON_EDITION.NONE;
			if ( _Vector.x < 0.0f || _Vector.y < 0.0f || _Vector.z < 0.0f )
				Result |= VECTOR_DAMAGE_ON_EDITION.XYZ_WILL_BE_MADE_POSITIVE;
			if ( _Vector.w < 0.0f )
				Result |= VECTOR_DAMAGE_ON_EDITION.ALPHA_WILL_BE_MADE_POSITIVE;

			return	Result;
		}

		/// <summary>
		/// Returns a RGB color from a 3D vector
		/// </summary>
		/// <param name="_Vector">The vector to get the RGB color from</param>
		/// <returns>The color from the vector</returns>
		/// <remarks>You should check if the vector can be cast into a color without damage using the above "GetVectorDamage()" method</remarks>
		public static Color	ConvertHDR2LDR( float3 _Vector ) {
			return AdobeColors.ConvertHDR2LDR( new float3( Math.Max( MIN_COMPONENT_VALUE, _Vector.x ), Math.Max( MIN_COMPONENT_VALUE, _Vector.y ), Math.Max( MIN_COMPONENT_VALUE, _Vector.z ) ) );
		}

		/// <summary>
		/// Returns a RGBA color from a 4D vector
		/// </summary>
		/// <param name="_Vector">The vector to get the RGBA color from</param>
		/// <returns>The color from the vector</returns>
		/// <remarks>You should check if the vector can be cast into a color without damage using the above "GetVectorDamage()" method</remarks>
		public static Color	ConvertHDR2LDR( float4 _Vector ) {
			return Color.FromArgb( Math.Max( 0, Math.Min( 255, (int) (255.0f * _Vector.w) ) ), AdobeColors.ConvertHDR2LDR( new float3( Math.Max( MIN_COMPONENT_VALUE, _Vector.x ), Math.Max( MIN_COMPONENT_VALUE, _Vector.y ), Math.Max( MIN_COMPONENT_VALUE, _Vector.z ) ) ) );
		}

		#endregion

		#region EVENT HANDLERS

		#region General Events

// 		protected void m_cmd_OK_Click(object sender, System.EventArgs e) {
// 			this.DialogResult = DialogResult.OK;
// 			this.Close();
// 		}
// 
// 
// 		protected void m_cmd_Cancel_Click(object sender, System.EventArgs e) {
// 			this.DialogResult = DialogResult.Cancel;
// 			this.Close();
// 		}

		#endregion

		protected void colorBoxControl_Scroll(object sender, System.EventArgs e) {
			m_dontRefresh = DONT_REFRESH.COLOR_BOX;
			RGB = AdobeColors.HSB2RGB( colorBoxControl.HSL );
		}

		protected void sliderControlHSL_Scroll(object sender, System.EventArgs e) {
			m_dontRefresh = DONT_REFRESH.COLOR_SLIDER;
			RGB = AdobeColors.HSB2RGB( sliderControlHSL.HSL );

			// Handle special cases where saturation is 0 (shades of gray) and it's not possible to devise a hue
			// Simply use the hue dictated by the color slider...
			if ( sliderControlHSL.HSL.y < 1e-4f ) {
				float3	TempHSL = AdobeColors.RGB2HSB( (float3) m_RGB );
						TempHSL.x = sliderControlHSL.HSL.x;

				colorBoxControl.HSL = TempHSL;
			}
		}

		#region Color Boxes

		protected void labelPrimaryColor_Click(object sender, System.EventArgs e)
		{
			ColorHDR = m_primaryColor;
		}

		protected void labelSecondaryColor_Click(object sender, System.EventArgs e)
		{
			ColorHDR = m_secondaryColor;
		}

		#endregion

		#region Radio Buttons

		protected void buttonHue_CheckedChanged(object sender, System.EventArgs e) {
			if ( buttonHue.Checked ) {
				sliderControlHSL.DrawStyle = DRAW_STYLE.Hue;
				colorBoxControl.DrawStyle = DRAW_STYLE.Hue;
			}
		}

		protected void buttonSaturation_CheckedChanged(object sender, System.EventArgs e) {
			if ( buttonSaturation.Checked ) {
				sliderControlHSL.DrawStyle = DRAW_STYLE.Saturation;
				colorBoxControl.DrawStyle = DRAW_STYLE.Saturation;
			}
		}


		protected void buttonBrightness_CheckedChanged(object sender, System.EventArgs e) {
			if ( buttonBrightness.Checked ) {
				sliderControlHSL.DrawStyle = DRAW_STYLE.Brightness;
				colorBoxControl.DrawStyle = DRAW_STYLE.Brightness;
			}
		}


		protected void buttonRed_CheckedChanged(object sender, System.EventArgs e) {
			if ( buttonRed.Checked ) {
				sliderControlHSL.DrawStyle = DRAW_STYLE.Red;
				colorBoxControl.DrawStyle = DRAW_STYLE.Red;
			}
		}

		protected void buttonGreen_CheckedChanged(object sender, System.EventArgs e) {
			if ( buttonGreen.Checked ) {
				sliderControlHSL.DrawStyle = DRAW_STYLE.Green;
				colorBoxControl.DrawStyle = DRAW_STYLE.Green;
			}
		}


		protected void buttonBlue_CheckedChanged(object sender, System.EventArgs e) {
			if ( buttonBlue.Checked ) {
				sliderControlHSL.DrawStyle = DRAW_STYLE.Blue;
				colorBoxControl.DrawStyle = DRAW_STYLE.Blue;
			}
		}

		private void radioButtonL_CheckedChanged( object sender, EventArgs e ) {
			if ( radioButtonL.Checked ) {
				sliderControlHSL.DrawStyle = DRAW_STYLE.Luminance;
				colorBoxControl.DrawStyle = DRAW_STYLE.Luminance;
			}
		}

		private void radioButtona_CheckedChanged( object sender, EventArgs e ) {
			if ( radioButtona.Checked ) {
				sliderControlHSL.DrawStyle = DRAW_STYLE.a;
				colorBoxControl.DrawStyle = DRAW_STYLE.a;
			}
		}

		private void radioButtonb_CheckedChanged( object sender, EventArgs e ) {
			if ( radioButtonb.Checked ) {
				sliderControlHSL.DrawStyle = DRAW_STYLE.b;
				colorBoxControl.DrawStyle = DRAW_STYLE.b;
			}
		}

		#endregion

		#region Trackbars

		private void floatTrackbarControlHue_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			float3	HSL = AdobeColors.RGB2HSB( (float3) m_RGB );
			HSL.x = _Sender.Value / 360.0f;
			RGB = AdobeColors.HSB2RGB( HSL );
		}

		private void floatTrackbarControlSaturation_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			float3	HSL = AdobeColors.RGB2HSB( (float3) m_RGB );
			HSL.y = _Sender.Value;
			RGB = AdobeColors.HSB2RGB( HSL );
		}

		private void floatTrackbarControlLuminance_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			float3	HSL = AdobeColors.RGB2HSB( (float3) m_RGB );
			HSL.z = _Sender.Value;
			RGB = AdobeColors.HSB2RGB( HSL );
		}

		private void floatTrackbarControlRed_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			m_RGB.x = _Sender.Value;
			ColorHDR = m_RGB;
		}

		private void floatTrackbarControlGreen_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			m_RGB.y = _Sender.Value;
			ColorHDR = m_RGB;
		}

		private void floatTrackbarControlBlue_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			m_RGB.z = _Sender.Value;
			ColorHDR = m_RGB;
		}

		private void floatTrackbarControlAlpha_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			m_RGB.w = _Sender.Value;
			ColorHDR = m_RGB;
		}

		private void gradientFloatTrackbarControlL_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			float3	Lab = AdobeColors.RGB2Lab_D65( (float3) m_RGB );
			Lab.x = _Sender.Value;
			RGB = AdobeColors.HSB2RGB( Lab );
		}

		private void gradientFloatTrackbarControla_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			float3	Lab = AdobeColors.RGB2Lab_D65( (float3) m_RGB );
			Lab.y = _Sender.Value;
			RGB = AdobeColors.HSB2RGB( Lab );
		}

		private void gradientFloatTrackbarControlb_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {
			float3	Lab = AdobeColors.RGB2Lab_D65( (float3) m_RGB );
			Lab.z = _Sender.Value;
			RGB = AdobeColors.HSB2RGB( Lab );
		}

		private void floatTrackbarControlColorTemperature_ValueChanged( FloatTrackbarControl _Sender, float _fFormerValue ) {

		}

		#endregion

		#region Hexa Text Box

		protected void	textBoxHexa_Validating(object sender, CancelEventArgs e)
		{
			string text = textBoxHexa.Text.ToUpper();
			bool has_illegal_chars = false;

			if ( textBoxHexa.Text.Length != 8 )
				has_illegal_chars = true;

			foreach ( char letter in text )
			{
				if ( !char.IsNumber(letter) )
				{
					if ( letter >= 'A' && letter <= 'F' )
						continue;
					has_illegal_chars = true;
					break;
				}
			}

			if ( has_illegal_chars )
			{
				MessageBox.Show( "Hex must be a hex value between 0x00000000 and 0xFFFFFFFF" );
				WriteHexData( m_RGB );
				return;
			}

			// Parse value
			string a_text, r_text, g_text, b_text;
			int a, r, g, b;

			r_text = textBoxHexa.Text.Substring(0, 2);
			g_text = textBoxHexa.Text.Substring(2, 2);
			b_text = textBoxHexa.Text.Substring(4, 2);
			a_text = textBoxHexa.Text.Substring(6, 2);

			a = int.Parse(a_text, System.Globalization.NumberStyles.HexNumber);
			r = int.Parse(r_text, System.Globalization.NumberStyles.HexNumber);
			g = int.Parse(g_text, System.Globalization.NumberStyles.HexNumber);
			b = int.Parse(b_text, System.Globalization.NumberStyles.HexNumber);

			ColorHDR = AdobeColors.RGB_LDR_to_RGB_HDR( r, g, b, a );
		}

		private void textBoxHexa_KeyDown( object sender, KeyEventArgs e )
		{
			if ( e.KeyCode != Keys.Return )
				return;

			e.Handled = true;
			textBoxHexa_Validating( sender, new CancelEventArgs() );
		}

		#endregion

		#region Palette Handling

		protected void	RadioButtonPalette_SelectedChanged( object sender, EventArgs e )
		{
			if ( (sender as PaletteButton).Selected )
				SetSelectedPaletteButton( sender as PaletteButton );
		}

		protected void	RadioButtonPalette_DoubleClick( object sender, EventArgs e )
		{
			int	PaletteIndex = GetPaletteButtonIndex( sender as PaletteButton );
			ColorHDR = GetPaletteColor( PaletteIndex );
		}

		protected void	buttonAssignColor_Click( object sender, EventArgs e )
		{
			int	PaletteIndex = GetSelectedPaletteButtonIndex();
			SetPaletteColor( PaletteIndex, ColorHDR );
			UpdatePaletteButtonColor( PaletteIndex );
		}

		#endregion

		#endregion
	}
}
