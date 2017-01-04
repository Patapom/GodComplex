using System;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Nuaj.Cirrus.Utility
{
	partial class ColorPickerForm
	{
		protected System.Windows.Forms.Label m_lbl_SelectColor;
		protected System.Windows.Forms.PictureBox m_pbx_BlankBox;
		protected System.Windows.Forms.Button m_cmd_OK;
		protected System.Windows.Forms.Button m_cmd_Cancel;
		protected System.Windows.Forms.TextBox textBoxHexa;
		protected RadioButton buttonHue;
		protected RadioButton buttonSaturation;
		protected RadioButton buttonBrightness;
		protected RadioButton buttonRed;
		protected RadioButton buttonGreen;
		protected RadioButton buttonBlue;
		protected System.Windows.Forms.Label m_lbl_HexPound;
		protected System.Windows.Forms.Label labelPrimaryColor;
		protected System.Windows.Forms.Label labelSecondaryColor;
		protected VerticalColorSliderControl sliderControlHSL;
		protected ColorBoxControl colorBoxControl;
		private GroupBox groupBox1;
		private PaletteButton radioButtonPalette35;
		private PaletteButton radioButtonPalette23;
		private PaletteButton radioButtonPalette11;
		private PaletteButton radioButtonPalette34;
		private PaletteButton radioButtonPalette22;
		private PaletteButton radioButtonPalette10;
		private PaletteButton radioButtonPalette33;
		private PaletteButton radioButtonPalette21;
		private PaletteButton radioButtonPalette9;
		private PaletteButton radioButtonPalette32;
		private PaletteButton radioButtonPalette20;
		private PaletteButton radioButtonPalette8;
		private PaletteButton radioButtonPalette31;
		private PaletteButton radioButtonPalette19;
		private PaletteButton radioButtonPalette7;
		private PaletteButton radioButtonPalette30;
		private PaletteButton radioButtonPalette18;
		private PaletteButton radioButtonPalette6;
		private PaletteButton radioButtonPalette29;
		private PaletteButton radioButtonPalette17;
		private PaletteButton radioButtonPalette5;
		private PaletteButton radioButtonPalette28;
		private PaletteButton radioButtonPalette16;
		private PaletteButton radioButtonPalette4;
		private PaletteButton radioButtonPalette27;
		private PaletteButton radioButtonPalette15;
		private PaletteButton radioButtonPalette3;
		private PaletteButton radioButtonPalette26;
		private PaletteButton radioButtonPalette14;
		private PaletteButton radioButtonPalette2;
		private PaletteButton radioButtonPalette25;
		private PaletteButton radioButtonPalette13;
		private PaletteButton radioButtonPalette1;
		private PaletteButton radioButtonPalette24;
		private PaletteButton radioButtonPalette12;
		private PaletteButton radioButtonPalette0;
		private Button buttonAssignColor;
		private FloatTrackbarControl floatTrackbarControlHue;
		private FloatTrackbarControl floatTrackbarControlSaturation;
		private FloatTrackbarControl floatTrackbarControlLuminance;
		private GradientFloatTrackbarControl floatTrackbarControlRed;
		private GradientFloatTrackbarControl floatTrackbarControlGreen;
		private GradientFloatTrackbarControl floatTrackbarControlBlue;
		private ToolTip toolTip1;

		private System.ComponentModel.IContainer components;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Nuaj.Cirrus.Utility.AdobeColors.HSL hsl1 = new Nuaj.Cirrus.Utility.AdobeColors.HSL();
			Nuaj.Cirrus.Utility.AdobeColors.HSL hsl2 = new Nuaj.Cirrus.Utility.AdobeColors.HSL();
			this.m_lbl_SelectColor = new System.Windows.Forms.Label();
			this.m_pbx_BlankBox = new System.Windows.Forms.PictureBox();
			this.m_cmd_OK = new System.Windows.Forms.Button();
			this.m_cmd_Cancel = new System.Windows.Forms.Button();
			this.textBoxHexa = new System.Windows.Forms.TextBox();
			this.buttonHue = new System.Windows.Forms.RadioButton();
			this.buttonSaturation = new System.Windows.Forms.RadioButton();
			this.buttonBrightness = new System.Windows.Forms.RadioButton();
			this.buttonRed = new System.Windows.Forms.RadioButton();
			this.buttonGreen = new System.Windows.Forms.RadioButton();
			this.buttonBlue = new System.Windows.Forms.RadioButton();
			this.m_lbl_HexPound = new System.Windows.Forms.Label();
			this.labelPrimaryColor = new System.Windows.Forms.Label();
			this.labelSecondaryColor = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.radioButtonPalette35 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette23 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette11 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette34 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette22 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette10 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette33 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette21 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette9 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette32 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette20 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette8 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette31 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette19 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette7 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette30 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette18 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette6 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette29 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette17 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette5 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette28 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette16 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette4 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette27 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette15 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette3 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette26 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette14 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette2 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette25 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette13 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette1 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette24 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette12 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.radioButtonPalette0 = new Nuaj.Cirrus.Utility.PaletteButton();
			this.buttonAssignColor = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip( this.components );
			this.floatTrackbarControlLuminance = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlSaturation = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlBlue = new Nuaj.Cirrus.Utility.GradientFloatTrackbarControl();
			this.floatTrackbarControlGreen = new Nuaj.Cirrus.Utility.GradientFloatTrackbarControl();
			this.floatTrackbarControlRed = new Nuaj.Cirrus.Utility.GradientFloatTrackbarControl();
			this.floatTrackbarControlHue = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlAlpha = new Nuaj.Cirrus.Utility.GradientFloatTrackbarControl();
			this.label2 = new System.Windows.Forms.Label();
			this.colorBoxControl = new Nuaj.Cirrus.Utility.ColorBoxControl();
			this.sliderControlHSL = new Nuaj.Cirrus.Utility.VerticalColorSliderControl();
			((System.ComponentModel.ISupportInitialize) (this.m_pbx_BlankBox)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lbl_SelectColor
			// 
			this.m_lbl_SelectColor.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.m_lbl_SelectColor.Location = new System.Drawing.Point( 12, 113 );
			this.m_lbl_SelectColor.Name = "m_lbl_SelectColor";
			this.m_lbl_SelectColor.Size = new System.Drawing.Size( 260, 20 );
			this.m_lbl_SelectColor.TabIndex = 0;
			this.m_lbl_SelectColor.Text = "Select Color :";
			// 
			// m_pbx_BlankBox
			// 
			this.m_pbx_BlankBox.BackColor = System.Drawing.Color.Black;
			this.m_pbx_BlankBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_pbx_BlankBox.Location = new System.Drawing.Point( 348, 20 );
			this.m_pbx_BlankBox.Name = "m_pbx_BlankBox";
			this.m_pbx_BlankBox.Size = new System.Drawing.Size( 62, 70 );
			this.m_pbx_BlankBox.TabIndex = 3;
			this.m_pbx_BlankBox.TabStop = false;
			// 
			// m_cmd_OK
			// 
			this.m_cmd_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_cmd_OK.Location = new System.Drawing.Point( 428, 346 );
			this.m_cmd_OK.Name = "m_cmd_OK";
			this.m_cmd_OK.Size = new System.Drawing.Size( 94, 21 );
			this.m_cmd_OK.TabIndex = 4;
			this.m_cmd_OK.Text = "OK";
//			this.m_cmd_OK.Click += new System.EventHandler( this.m_cmd_OK_Click );
			// 
			// m_cmd_Cancel
			// 
			this.m_cmd_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_cmd_Cancel.Location = new System.Drawing.Point( 428, 374 );
			this.m_cmd_Cancel.Name = "m_cmd_Cancel";
			this.m_cmd_Cancel.Size = new System.Drawing.Size( 94, 21 );
			this.m_cmd_Cancel.TabIndex = 5;
			this.m_cmd_Cancel.Text = "Cancel";
//			this.m_cmd_Cancel.Click += new System.EventHandler( this.m_cmd_Cancel_Click );
			// 
			// textBoxHexa
			// 
			this.textBoxHexa.Font = new System.Drawing.Font( "Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)) );
			this.textBoxHexa.Location = new System.Drawing.Point( 449, 20 );
			this.textBoxHexa.Name = "textBoxHexa";
			this.textBoxHexa.Size = new System.Drawing.Size( 73, 21 );
			this.textBoxHexa.TabIndex = 19;
			this.textBoxHexa.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textBoxHexa.KeyDown += new System.Windows.Forms.KeyEventHandler( this.textBoxHexa_KeyDown );
			this.textBoxHexa.Validating += new System.ComponentModel.CancelEventHandler( this.textBoxHexa_Validating );
			// 
			// buttonHue
			// 
			this.buttonHue.Checked = true;
			this.buttonHue.Location = new System.Drawing.Point( 315, 131 );
			this.buttonHue.Name = "buttonHue";
			this.buttonHue.Size = new System.Drawing.Size( 36, 24 );
			this.buttonHue.TabIndex = 20;
			this.buttonHue.TabStop = true;
			this.buttonHue.Text = "H:";
			this.buttonHue.CheckedChanged += new System.EventHandler( this.buttonHue_CheckedChanged );
			// 
			// buttonSaturation
			// 
			this.buttonSaturation.Location = new System.Drawing.Point( 315, 156 );
			this.buttonSaturation.Name = "buttonSaturation";
			this.buttonSaturation.Size = new System.Drawing.Size( 35, 24 );
			this.buttonSaturation.TabIndex = 21;
			this.buttonSaturation.Text = "S:";
			this.buttonSaturation.CheckedChanged += new System.EventHandler( this.buttonSaturation_CheckedChanged );
			// 
			// buttonBrightness
			// 
			this.buttonBrightness.Location = new System.Drawing.Point( 315, 181 );
			this.buttonBrightness.Name = "buttonBrightness";
			this.buttonBrightness.Size = new System.Drawing.Size( 35, 24 );
			this.buttonBrightness.TabIndex = 22;
			this.buttonBrightness.Text = "L:";
			this.buttonBrightness.CheckedChanged += new System.EventHandler( this.buttonBrightness_CheckedChanged );
			// 
			// buttonRed
			// 
			this.buttonRed.Location = new System.Drawing.Point( 315, 228 );
			this.buttonRed.Name = "buttonRed";
			this.buttonRed.Size = new System.Drawing.Size( 36, 24 );
			this.buttonRed.TabIndex = 23;
			this.buttonRed.Text = "R:";
			this.buttonRed.CheckedChanged += new System.EventHandler( this.buttonRed_CheckedChanged );
			// 
			// buttonGreen
			// 
			this.buttonGreen.Location = new System.Drawing.Point( 315, 253 );
			this.buttonGreen.Name = "buttonGreen";
			this.buttonGreen.Size = new System.Drawing.Size( 36, 24 );
			this.buttonGreen.TabIndex = 24;
			this.buttonGreen.Text = "G:";
			this.buttonGreen.CheckedChanged += new System.EventHandler( this.buttonGreen_CheckedChanged );
			// 
			// buttonBlue
			// 
			this.buttonBlue.Location = new System.Drawing.Point( 315, 278 );
			this.buttonBlue.Name = "buttonBlue";
			this.buttonBlue.Size = new System.Drawing.Size( 35, 24 );
			this.buttonBlue.TabIndex = 25;
			this.buttonBlue.Text = "B:";
			this.buttonBlue.CheckedChanged += new System.EventHandler( this.buttonBlue_CheckedChanged );
			// 
			// m_lbl_HexPound
			// 
			this.m_lbl_HexPound.Location = new System.Drawing.Point( 433, 24 );
			this.m_lbl_HexPound.Name = "m_lbl_HexPound";
			this.m_lbl_HexPound.Size = new System.Drawing.Size( 16, 14 );
			this.m_lbl_HexPound.TabIndex = 27;
			this.m_lbl_HexPound.Text = "#";
			// 
			// labelPrimaryColor
			// 
			this.labelPrimaryColor.Location = new System.Drawing.Point( 349, 21 );
			this.labelPrimaryColor.Name = "labelPrimaryColor";
			this.labelPrimaryColor.Size = new System.Drawing.Size( 60, 34 );
			this.labelPrimaryColor.TabIndex = 36;
			this.labelPrimaryColor.Click += new System.EventHandler( this.labelPrimaryColor_Click );
			// 
			// labelSecondaryColor
			// 
			this.labelSecondaryColor.Location = new System.Drawing.Point( 349, 55 );
			this.labelSecondaryColor.Name = "labelSecondaryColor";
			this.labelSecondaryColor.Size = new System.Drawing.Size( 60, 34 );
			this.labelSecondaryColor.TabIndex = 37;
			this.labelSecondaryColor.Click += new System.EventHandler( this.labelSecondaryColor_Click );
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add( this.radioButtonPalette35 );
			this.groupBox1.Controls.Add( this.radioButtonPalette23 );
			this.groupBox1.Controls.Add( this.radioButtonPalette11 );
			this.groupBox1.Controls.Add( this.radioButtonPalette34 );
			this.groupBox1.Controls.Add( this.radioButtonPalette22 );
			this.groupBox1.Controls.Add( this.radioButtonPalette10 );
			this.groupBox1.Controls.Add( this.radioButtonPalette33 );
			this.groupBox1.Controls.Add( this.radioButtonPalette21 );
			this.groupBox1.Controls.Add( this.radioButtonPalette9 );
			this.groupBox1.Controls.Add( this.radioButtonPalette32 );
			this.groupBox1.Controls.Add( this.radioButtonPalette20 );
			this.groupBox1.Controls.Add( this.radioButtonPalette8 );
			this.groupBox1.Controls.Add( this.radioButtonPalette31 );
			this.groupBox1.Controls.Add( this.radioButtonPalette19 );
			this.groupBox1.Controls.Add( this.radioButtonPalette7 );
			this.groupBox1.Controls.Add( this.radioButtonPalette30 );
			this.groupBox1.Controls.Add( this.radioButtonPalette18 );
			this.groupBox1.Controls.Add( this.radioButtonPalette6 );
			this.groupBox1.Controls.Add( this.radioButtonPalette29 );
			this.groupBox1.Controls.Add( this.radioButtonPalette17 );
			this.groupBox1.Controls.Add( this.radioButtonPalette5 );
			this.groupBox1.Controls.Add( this.radioButtonPalette28 );
			this.groupBox1.Controls.Add( this.radioButtonPalette16 );
			this.groupBox1.Controls.Add( this.radioButtonPalette4 );
			this.groupBox1.Controls.Add( this.radioButtonPalette27 );
			this.groupBox1.Controls.Add( this.radioButtonPalette15 );
			this.groupBox1.Controls.Add( this.radioButtonPalette3 );
			this.groupBox1.Controls.Add( this.radioButtonPalette26 );
			this.groupBox1.Controls.Add( this.radioButtonPalette14 );
			this.groupBox1.Controls.Add( this.radioButtonPalette2 );
			this.groupBox1.Controls.Add( this.radioButtonPalette25 );
			this.groupBox1.Controls.Add( this.radioButtonPalette13 );
			this.groupBox1.Controls.Add( this.radioButtonPalette1 );
			this.groupBox1.Controls.Add( this.radioButtonPalette24 );
			this.groupBox1.Controls.Add( this.radioButtonPalette12 );
			this.groupBox1.Controls.Add( this.radioButtonPalette0 );
			this.groupBox1.Location = new System.Drawing.Point( 10, 12 );
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size( 296, 95 );
			this.groupBox1.TabIndex = 42;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Palette";
			// 
			// radioButtonPalette35
			// 
			this.radioButtonPalette35.AutoSize = true;
			this.radioButtonPalette35.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette35.Location = new System.Drawing.Point( 270, 67 );
			this.radioButtonPalette35.Name = "radioButtonPalette35";
			this.radioButtonPalette35.Selected = false;
			this.radioButtonPalette35.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette35.TabIndex = 0;
			this.radioButtonPalette35.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette23
			// 
			this.radioButtonPalette23.AutoSize = true;
			this.radioButtonPalette23.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette23.Location = new System.Drawing.Point( 270, 43 );
			this.radioButtonPalette23.Name = "radioButtonPalette23";
			this.radioButtonPalette23.Selected = false;
			this.radioButtonPalette23.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette23.TabIndex = 0;
			this.radioButtonPalette23.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette11
			// 
			this.radioButtonPalette11.AutoSize = true;
			this.radioButtonPalette11.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette11.Location = new System.Drawing.Point( 270, 19 );
			this.radioButtonPalette11.Name = "radioButtonPalette11";
			this.radioButtonPalette11.Selected = false;
			this.radioButtonPalette11.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette11.TabIndex = 0;
			this.radioButtonPalette11.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette34
			// 
			this.radioButtonPalette34.AutoSize = true;
			this.radioButtonPalette34.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette34.Location = new System.Drawing.Point( 246, 67 );
			this.radioButtonPalette34.Name = "radioButtonPalette34";
			this.radioButtonPalette34.Selected = false;
			this.radioButtonPalette34.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette34.TabIndex = 0;
			this.radioButtonPalette34.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette22
			// 
			this.radioButtonPalette22.AutoSize = true;
			this.radioButtonPalette22.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette22.Location = new System.Drawing.Point( 246, 43 );
			this.radioButtonPalette22.Name = "radioButtonPalette22";
			this.radioButtonPalette22.Selected = false;
			this.radioButtonPalette22.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette22.TabIndex = 0;
			this.radioButtonPalette22.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette10
			// 
			this.radioButtonPalette10.AutoSize = true;
			this.radioButtonPalette10.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette10.Location = new System.Drawing.Point( 246, 19 );
			this.radioButtonPalette10.Name = "radioButtonPalette10";
			this.radioButtonPalette10.Selected = false;
			this.radioButtonPalette10.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette10.TabIndex = 0;
			this.radioButtonPalette10.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette33
			// 
			this.radioButtonPalette33.AutoSize = true;
			this.radioButtonPalette33.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette33.Location = new System.Drawing.Point( 222, 67 );
			this.radioButtonPalette33.Name = "radioButtonPalette33";
			this.radioButtonPalette33.Selected = false;
			this.radioButtonPalette33.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette33.TabIndex = 0;
			this.radioButtonPalette33.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette21
			// 
			this.radioButtonPalette21.AutoSize = true;
			this.radioButtonPalette21.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette21.Location = new System.Drawing.Point( 222, 43 );
			this.radioButtonPalette21.Name = "radioButtonPalette21";
			this.radioButtonPalette21.Selected = false;
			this.radioButtonPalette21.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette21.TabIndex = 0;
			this.radioButtonPalette21.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette9
			// 
			this.radioButtonPalette9.AutoSize = true;
			this.radioButtonPalette9.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette9.Location = new System.Drawing.Point( 222, 19 );
			this.radioButtonPalette9.Name = "radioButtonPalette9";
			this.radioButtonPalette9.Selected = false;
			this.radioButtonPalette9.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette9.TabIndex = 0;
			this.radioButtonPalette9.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette32
			// 
			this.radioButtonPalette32.AutoSize = true;
			this.radioButtonPalette32.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette32.Location = new System.Drawing.Point( 198, 67 );
			this.radioButtonPalette32.Name = "radioButtonPalette32";
			this.radioButtonPalette32.Selected = false;
			this.radioButtonPalette32.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette32.TabIndex = 0;
			this.radioButtonPalette32.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette20
			// 
			this.radioButtonPalette20.AutoSize = true;
			this.radioButtonPalette20.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette20.Location = new System.Drawing.Point( 198, 43 );
			this.radioButtonPalette20.Name = "radioButtonPalette20";
			this.radioButtonPalette20.Selected = false;
			this.radioButtonPalette20.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette20.TabIndex = 0;
			this.radioButtonPalette20.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette8
			// 
			this.radioButtonPalette8.AutoSize = true;
			this.radioButtonPalette8.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette8.Location = new System.Drawing.Point( 198, 19 );
			this.radioButtonPalette8.Name = "radioButtonPalette8";
			this.radioButtonPalette8.Selected = false;
			this.radioButtonPalette8.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette8.TabIndex = 0;
			this.radioButtonPalette8.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette31
			// 
			this.radioButtonPalette31.AutoSize = true;
			this.radioButtonPalette31.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette31.Location = new System.Drawing.Point( 174, 67 );
			this.radioButtonPalette31.Name = "radioButtonPalette31";
			this.radioButtonPalette31.Selected = false;
			this.radioButtonPalette31.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette31.TabIndex = 0;
			this.radioButtonPalette31.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette19
			// 
			this.radioButtonPalette19.AutoSize = true;
			this.radioButtonPalette19.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette19.Location = new System.Drawing.Point( 174, 43 );
			this.radioButtonPalette19.Name = "radioButtonPalette19";
			this.radioButtonPalette19.Selected = false;
			this.radioButtonPalette19.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette19.TabIndex = 0;
			this.radioButtonPalette19.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette7
			// 
			this.radioButtonPalette7.AutoSize = true;
			this.radioButtonPalette7.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette7.Location = new System.Drawing.Point( 174, 19 );
			this.radioButtonPalette7.Name = "radioButtonPalette7";
			this.radioButtonPalette7.Selected = false;
			this.radioButtonPalette7.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette7.TabIndex = 0;
			this.radioButtonPalette7.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette30
			// 
			this.radioButtonPalette30.AutoSize = true;
			this.radioButtonPalette30.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette30.Location = new System.Drawing.Point( 150, 67 );
			this.radioButtonPalette30.Name = "radioButtonPalette30";
			this.radioButtonPalette30.Selected = false;
			this.radioButtonPalette30.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette30.TabIndex = 0;
			this.radioButtonPalette30.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette18
			// 
			this.radioButtonPalette18.AutoSize = true;
			this.radioButtonPalette18.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette18.Location = new System.Drawing.Point( 150, 43 );
			this.radioButtonPalette18.Name = "radioButtonPalette18";
			this.radioButtonPalette18.Selected = false;
			this.radioButtonPalette18.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette18.TabIndex = 0;
			this.radioButtonPalette18.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette6
			// 
			this.radioButtonPalette6.AutoSize = true;
			this.radioButtonPalette6.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette6.Location = new System.Drawing.Point( 150, 19 );
			this.radioButtonPalette6.Name = "radioButtonPalette6";
			this.radioButtonPalette6.Selected = false;
			this.radioButtonPalette6.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette6.TabIndex = 0;
			this.radioButtonPalette6.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette29
			// 
			this.radioButtonPalette29.AutoSize = true;
			this.radioButtonPalette29.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette29.Location = new System.Drawing.Point( 126, 67 );
			this.radioButtonPalette29.Name = "radioButtonPalette29";
			this.radioButtonPalette29.Selected = false;
			this.radioButtonPalette29.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette29.TabIndex = 0;
			this.radioButtonPalette29.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette17
			// 
			this.radioButtonPalette17.AutoSize = true;
			this.radioButtonPalette17.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette17.Location = new System.Drawing.Point( 126, 43 );
			this.radioButtonPalette17.Name = "radioButtonPalette17";
			this.radioButtonPalette17.Selected = false;
			this.radioButtonPalette17.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette17.TabIndex = 0;
			this.radioButtonPalette17.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette5
			// 
			this.radioButtonPalette5.AutoSize = true;
			this.radioButtonPalette5.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette5.Location = new System.Drawing.Point( 126, 19 );
			this.radioButtonPalette5.Name = "radioButtonPalette5";
			this.radioButtonPalette5.Selected = false;
			this.radioButtonPalette5.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette5.TabIndex = 0;
			this.radioButtonPalette5.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette28
			// 
			this.radioButtonPalette28.AutoSize = true;
			this.radioButtonPalette28.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette28.Location = new System.Drawing.Point( 102, 67 );
			this.radioButtonPalette28.Name = "radioButtonPalette28";
			this.radioButtonPalette28.Selected = false;
			this.radioButtonPalette28.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette28.TabIndex = 0;
			this.radioButtonPalette28.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette16
			// 
			this.radioButtonPalette16.AutoSize = true;
			this.radioButtonPalette16.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette16.Location = new System.Drawing.Point( 102, 43 );
			this.radioButtonPalette16.Name = "radioButtonPalette16";
			this.radioButtonPalette16.Selected = false;
			this.radioButtonPalette16.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette16.TabIndex = 0;
			this.radioButtonPalette16.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette4
			// 
			this.radioButtonPalette4.AutoSize = true;
			this.radioButtonPalette4.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette4.Location = new System.Drawing.Point( 102, 19 );
			this.radioButtonPalette4.Name = "radioButtonPalette4";
			this.radioButtonPalette4.Selected = false;
			this.radioButtonPalette4.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette4.TabIndex = 0;
			this.radioButtonPalette4.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette27
			// 
			this.radioButtonPalette27.AutoSize = true;
			this.radioButtonPalette27.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette27.Location = new System.Drawing.Point( 78, 67 );
			this.radioButtonPalette27.Name = "radioButtonPalette27";
			this.radioButtonPalette27.Selected = false;
			this.radioButtonPalette27.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette27.TabIndex = 0;
			this.radioButtonPalette27.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette15
			// 
			this.radioButtonPalette15.AutoSize = true;
			this.radioButtonPalette15.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette15.Location = new System.Drawing.Point( 78, 43 );
			this.radioButtonPalette15.Name = "radioButtonPalette15";
			this.radioButtonPalette15.Selected = false;
			this.radioButtonPalette15.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette15.TabIndex = 0;
			this.radioButtonPalette15.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette3
			// 
			this.radioButtonPalette3.AutoSize = true;
			this.radioButtonPalette3.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette3.Location = new System.Drawing.Point( 78, 19 );
			this.radioButtonPalette3.Name = "radioButtonPalette3";
			this.radioButtonPalette3.Selected = false;
			this.radioButtonPalette3.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette3.TabIndex = 0;
			this.radioButtonPalette3.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette26
			// 
			this.radioButtonPalette26.AutoSize = true;
			this.radioButtonPalette26.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette26.Location = new System.Drawing.Point( 54, 67 );
			this.radioButtonPalette26.Name = "radioButtonPalette26";
			this.radioButtonPalette26.Selected = false;
			this.radioButtonPalette26.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette26.TabIndex = 0;
			this.radioButtonPalette26.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette14
			// 
			this.radioButtonPalette14.AutoSize = true;
			this.radioButtonPalette14.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette14.Location = new System.Drawing.Point( 54, 43 );
			this.radioButtonPalette14.Name = "radioButtonPalette14";
			this.radioButtonPalette14.Selected = false;
			this.radioButtonPalette14.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette14.TabIndex = 0;
			this.radioButtonPalette14.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette2
			// 
			this.radioButtonPalette2.AutoSize = true;
			this.radioButtonPalette2.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette2.Location = new System.Drawing.Point( 54, 19 );
			this.radioButtonPalette2.Name = "radioButtonPalette2";
			this.radioButtonPalette2.Selected = false;
			this.radioButtonPalette2.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette2.TabIndex = 0;
			this.radioButtonPalette2.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette25
			// 
			this.radioButtonPalette25.AutoSize = true;
			this.radioButtonPalette25.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette25.Location = new System.Drawing.Point( 30, 67 );
			this.radioButtonPalette25.Name = "radioButtonPalette25";
			this.radioButtonPalette25.Selected = false;
			this.radioButtonPalette25.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette25.TabIndex = 0;
			this.radioButtonPalette25.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette13
			// 
			this.radioButtonPalette13.AutoSize = true;
			this.radioButtonPalette13.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette13.Location = new System.Drawing.Point( 30, 43 );
			this.radioButtonPalette13.Name = "radioButtonPalette13";
			this.radioButtonPalette13.Selected = false;
			this.radioButtonPalette13.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette13.TabIndex = 0;
			this.radioButtonPalette13.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette1
			// 
			this.radioButtonPalette1.AutoSize = true;
			this.radioButtonPalette1.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette1.Location = new System.Drawing.Point( 30, 19 );
			this.radioButtonPalette1.Name = "radioButtonPalette1";
			this.radioButtonPalette1.Selected = false;
			this.radioButtonPalette1.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette1.TabIndex = 0;
			this.radioButtonPalette1.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette24
			// 
			this.radioButtonPalette24.AutoSize = true;
			this.radioButtonPalette24.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette24.Location = new System.Drawing.Point( 6, 67 );
			this.radioButtonPalette24.Name = "radioButtonPalette24";
			this.radioButtonPalette24.Selected = false;
			this.radioButtonPalette24.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette24.TabIndex = 0;
			this.radioButtonPalette24.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette12
			// 
			this.radioButtonPalette12.AutoSize = true;
			this.radioButtonPalette12.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette12.Location = new System.Drawing.Point( 6, 43 );
			this.radioButtonPalette12.Name = "radioButtonPalette12";
			this.radioButtonPalette12.Selected = false;
			this.radioButtonPalette12.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette12.TabIndex = 0;
			this.radioButtonPalette12.Vector = SharpMath.float4.Zero;
			// 
			// radioButtonPalette0
			// 
			this.radioButtonPalette0.AutoSize = true;
			this.radioButtonPalette0.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette0.Location = new System.Drawing.Point( 6, 19 );
			this.radioButtonPalette0.Name = "radioButtonPalette0";
			this.radioButtonPalette0.Selected = true;
			this.radioButtonPalette0.Size = new System.Drawing.Size( 18, 18 );
			this.radioButtonPalette0.TabIndex = 0;
			this.radioButtonPalette0.TabStop = true;
			this.radioButtonPalette0.Vector = SharpMath.float4.Zero;
			// 
			// buttonAssignColor
			// 
			this.buttonAssignColor.Image = global::Nuaj.Cirrus.Utility.Properties.Resources.eyedropper;
			this.buttonAssignColor.Location = new System.Drawing.Point( 314, 23 );
			this.buttonAssignColor.Name = "buttonAssignColor";
			this.buttonAssignColor.Size = new System.Drawing.Size( 26, 26 );
			this.buttonAssignColor.TabIndex = 44;
			this.buttonAssignColor.UseVisualStyleBackColor = true;
			this.buttonAssignColor.Click += new System.EventHandler( this.buttonAssignColor_Click );
			// 
			// floatTrackbarControlLuminance
			// 
			this.floatTrackbarControlLuminance.Location = new System.Drawing.Point( 352, 183 );
			this.floatTrackbarControlLuminance.MaximumSize = new System.Drawing.Size( 100000, 20 );
			this.floatTrackbarControlLuminance.MinimumSize = new System.Drawing.Size( 0, 20 );
			this.floatTrackbarControlLuminance.Name = "floatTrackbarControlLuminance";
			this.floatTrackbarControlLuminance.Size = new System.Drawing.Size( 170, 20 );
			this.floatTrackbarControlLuminance.TabIndex = 45;
			this.toolTip1.SetToolTip( this.floatTrackbarControlLuminance, "Luminance" );
			this.floatTrackbarControlLuminance.Value = 0F;
			this.floatTrackbarControlLuminance.VisibleRangeMax = 1F;
			this.floatTrackbarControlLuminance.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlLuminance_ValueChanged );
			// 
			// floatTrackbarControlSaturation
			// 
			this.floatTrackbarControlSaturation.Location = new System.Drawing.Point( 352, 158 );
			this.floatTrackbarControlSaturation.MaximumSize = new System.Drawing.Size( 100000, 20 );
			this.floatTrackbarControlSaturation.MinimumSize = new System.Drawing.Size( 0, 20 );
			this.floatTrackbarControlSaturation.Name = "floatTrackbarControlSaturation";
			this.floatTrackbarControlSaturation.RangeMax = 1F;
			this.floatTrackbarControlSaturation.RangeMin = 0F;
			this.floatTrackbarControlSaturation.Size = new System.Drawing.Size( 170, 20 );
			this.floatTrackbarControlSaturation.TabIndex = 45;
			this.toolTip1.SetToolTip( this.floatTrackbarControlSaturation, "Saturation (in [0,1])" );
			this.floatTrackbarControlSaturation.Value = 0F;
			this.floatTrackbarControlSaturation.VisibleRangeMax = 1F;
			this.floatTrackbarControlSaturation.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlSaturation_ValueChanged );
			// 
			// floatTrackbarControlBlue
			// 
			this.floatTrackbarControlBlue.ColorMax = System.Drawing.Color.FromArgb( ((int) (((byte) (200)))), ((int) (((byte) (200)))), ((int) (((byte) (200)))) );
			this.floatTrackbarControlBlue.ColorMin = System.Drawing.Color.FromArgb( ((int) (((byte) (200)))), ((int) (((byte) (200)))), ((int) (((byte) (200)))) );
			this.floatTrackbarControlBlue.Location = new System.Drawing.Point( 352, 280 );
			this.floatTrackbarControlBlue.MaximumSize = new System.Drawing.Size( 100000, 20 );
			this.floatTrackbarControlBlue.MinimumSize = new System.Drawing.Size( 0, 20 );
			this.floatTrackbarControlBlue.Name = "floatTrackbarControlBlue";
			this.floatTrackbarControlBlue.RangeMin = 0F;
			this.floatTrackbarControlBlue.Size = new System.Drawing.Size( 170, 20 );
			this.floatTrackbarControlBlue.TabIndex = 45;
			this.toolTip1.SetToolTip( this.floatTrackbarControlBlue, "Blue Component" );
			this.floatTrackbarControlBlue.Value = 0F;
			this.floatTrackbarControlBlue.VisibleRangeMax = 1F;
			this.floatTrackbarControlBlue.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlBlue_ValueChanged );
			// 
			// floatTrackbarControlGreen
			// 
			this.floatTrackbarControlGreen.ColorMax = System.Drawing.Color.FromArgb( ((int) (((byte) (200)))), ((int) (((byte) (200)))), ((int) (((byte) (200)))) );
			this.floatTrackbarControlGreen.ColorMin = System.Drawing.Color.FromArgb( ((int) (((byte) (200)))), ((int) (((byte) (200)))), ((int) (((byte) (200)))) );
			this.floatTrackbarControlGreen.Location = new System.Drawing.Point( 352, 255 );
			this.floatTrackbarControlGreen.MaximumSize = new System.Drawing.Size( 100000, 20 );
			this.floatTrackbarControlGreen.MinimumSize = new System.Drawing.Size( 0, 20 );
			this.floatTrackbarControlGreen.Name = "floatTrackbarControlGreen";
			this.floatTrackbarControlGreen.RangeMin = 0F;
			this.floatTrackbarControlGreen.Size = new System.Drawing.Size( 170, 20 );
			this.floatTrackbarControlGreen.TabIndex = 45;
			this.toolTip1.SetToolTip( this.floatTrackbarControlGreen, "Green Component" );
			this.floatTrackbarControlGreen.Value = 0F;
			this.floatTrackbarControlGreen.VisibleRangeMax = 1F;
			this.floatTrackbarControlGreen.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlGreen_ValueChanged );
			// 
			// floatTrackbarControlRed
			// 
			this.floatTrackbarControlRed.ColorMax = System.Drawing.Color.FromArgb( ((int) (((byte) (200)))), ((int) (((byte) (200)))), ((int) (((byte) (200)))) );
			this.floatTrackbarControlRed.ColorMin = System.Drawing.Color.FromArgb( ((int) (((byte) (200)))), ((int) (((byte) (200)))), ((int) (((byte) (200)))) );
			this.floatTrackbarControlRed.Location = new System.Drawing.Point( 352, 230 );
			this.floatTrackbarControlRed.MaximumSize = new System.Drawing.Size( 100000, 20 );
			this.floatTrackbarControlRed.MinimumSize = new System.Drawing.Size( 0, 20 );
			this.floatTrackbarControlRed.Name = "floatTrackbarControlRed";
			this.floatTrackbarControlRed.RangeMin = 0F;
			this.floatTrackbarControlRed.Size = new System.Drawing.Size( 170, 20 );
			this.floatTrackbarControlRed.TabIndex = 45;
			this.toolTip1.SetToolTip( this.floatTrackbarControlRed, "Red Component" );
			this.floatTrackbarControlRed.Value = 0F;
			this.floatTrackbarControlRed.VisibleRangeMax = 1F;
			this.floatTrackbarControlRed.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlRed_ValueChanged );
			// 
			// floatTrackbarControlHue
			// 
			this.floatTrackbarControlHue.Location = new System.Drawing.Point( 352, 133 );
			this.floatTrackbarControlHue.MaximumSize = new System.Drawing.Size( 100000, 20 );
			this.floatTrackbarControlHue.MinimumSize = new System.Drawing.Size( 0, 20 );
			this.floatTrackbarControlHue.Name = "floatTrackbarControlHue";
			this.floatTrackbarControlHue.RangeMax = 360F;
			this.floatTrackbarControlHue.RangeMin = 0F;
			this.floatTrackbarControlHue.Size = new System.Drawing.Size( 170, 20 );
			this.floatTrackbarControlHue.TabIndex = 45;
			this.toolTip1.SetToolTip( this.floatTrackbarControlHue, "Hue (in degress)" );
			this.floatTrackbarControlHue.Value = 0F;
			this.floatTrackbarControlHue.VisibleRangeMax = 360F;
			this.floatTrackbarControlHue.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlHue_ValueChanged );
			// 
			// floatTrackbarControlAlpha
			// 
			this.floatTrackbarControlAlpha.ColorMax = System.Drawing.Color.FromArgb( ((int) (((byte) (200)))), ((int) (((byte) (200)))), ((int) (((byte) (200)))) );
			this.floatTrackbarControlAlpha.ColorMin = System.Drawing.Color.FromArgb( ((int) (((byte) (200)))), ((int) (((byte) (200)))), ((int) (((byte) (200)))) );
			this.floatTrackbarControlAlpha.Location = new System.Drawing.Point( 352, 305 );
			this.floatTrackbarControlAlpha.MaximumSize = new System.Drawing.Size( 100000, 20 );
			this.floatTrackbarControlAlpha.MinimumSize = new System.Drawing.Size( 0, 20 );
			this.floatTrackbarControlAlpha.Name = "floatTrackbarControlAlpha";
			this.floatTrackbarControlAlpha.RangeMin = 0F;
			this.floatTrackbarControlAlpha.Size = new System.Drawing.Size( 170, 20 );
			this.floatTrackbarControlAlpha.TabIndex = 45;
			this.toolTip1.SetToolTip( this.floatTrackbarControlAlpha, "Alpha component" );
			this.floatTrackbarControlAlpha.Value = 0F;
			this.floatTrackbarControlAlpha.VisibleRangeMax = 1F;
			this.floatTrackbarControlAlpha.ValueChanged += new Nuaj.Cirrus.Utility.FloatTrackbarControl.ValueChangedEventHandler( this.floatTrackbarControlAlpha_ValueChanged );
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point( 331, 308 );
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size( 17, 13 );
			this.label2.TabIndex = 46;
			this.label2.Text = "A:";
			// 
			// colorBoxControl
			// 
			this.colorBoxControl.DrawStyle = Nuaj.Cirrus.Utility.ColorPickerForm.DRAW_STYLE.Hue;
			hsl1.H = 1;
			hsl1.L = 1;
			hsl1.S = 1;
			this.colorBoxControl.HSL = hsl1;
			this.colorBoxControl.Location = new System.Drawing.Point( 12, 133 );
			this.colorBoxControl.Name = "colorBoxControl";
			this.colorBoxControl.RGB = new SharpMath.float3( 1.0f, 0.0f, 0.0f );
			this.colorBoxControl.Size = new System.Drawing.Size( 260, 260 );
			this.colorBoxControl.TabIndex = 39;
			this.colorBoxControl.Scroll += new System.EventHandler( this.colorBoxControl_Scroll );
			// 
			// sliderControlHSL
			// 
			this.sliderControlHSL.DrawStyle = Nuaj.Cirrus.Utility.ColorPickerForm.DRAW_STYLE.Hue;
			hsl2.H = 0;
			hsl2.L = 1;
			hsl2.S = 0.99900001287460327;
			this.sliderControlHSL.HSL = hsl2;
			this.sliderControlHSL.Location = new System.Drawing.Point( 273, 131 );
			this.sliderControlHSL.Name = "sliderControlHSL";
			this.sliderControlHSL.RGB = new SharpMath.float3( 1.0f, 0.0f, 0.0f );
			this.sliderControlHSL.Size = new System.Drawing.Size( 40, 264 );
			this.sliderControlHSL.TabIndex = 38;
			this.sliderControlHSL.Scroll += new System.EventHandler( this.sliderControlHSL_Scroll );
			// 
			// ColorPickerForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size( 5, 13 );
			this.CancelButton = this.m_cmd_Cancel;
			this.ClientSize = new System.Drawing.Size( 534, 407 );
			this.Controls.Add( this.label2 );
			this.Controls.Add( this.floatTrackbarControlLuminance );
			this.Controls.Add( this.floatTrackbarControlSaturation );
			this.Controls.Add( this.floatTrackbarControlAlpha );
			this.Controls.Add( this.floatTrackbarControlBlue );
			this.Controls.Add( this.floatTrackbarControlGreen );
			this.Controls.Add( this.floatTrackbarControlRed );
			this.Controls.Add( this.floatTrackbarControlHue );
			this.Controls.Add( this.buttonAssignColor );
			this.Controls.Add( this.groupBox1 );
			this.Controls.Add( this.colorBoxControl );
			this.Controls.Add( this.sliderControlHSL );
			this.Controls.Add( this.labelSecondaryColor );
			this.Controls.Add( this.labelPrimaryColor );
			this.Controls.Add( this.m_lbl_HexPound );
			this.Controls.Add( this.buttonBlue );
			this.Controls.Add( this.buttonGreen );
			this.Controls.Add( this.buttonRed );
			this.Controls.Add( this.buttonBrightness );
			this.Controls.Add( this.buttonSaturation );
			this.Controls.Add( this.buttonHue );
			this.Controls.Add( this.textBoxHexa );
			this.Controls.Add( this.m_cmd_Cancel );
			this.Controls.Add( this.m_cmd_OK );
			this.Controls.Add( this.m_pbx_BlankBox );
			this.Controls.Add( this.m_lbl_SelectColor );
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.CancelButton = this.m_cmd_Cancel;
			this.AcceptButton = this.m_cmd_OK;
			this.Name = "ColorPickerForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Color Picker";
			this.Load += new System.EventHandler( this.ColorPickerForm_Load );
			((System.ComponentModel.ISupportInitialize) (this.m_pbx_BlankBox)).EndInit();
			this.groupBox1.ResumeLayout( false );
			this.groupBox1.PerformLayout();
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private GradientFloatTrackbarControl floatTrackbarControlAlpha;
		private Label label2;

	}
}
