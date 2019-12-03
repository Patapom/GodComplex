using System;
using System.Reflection;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace UIUtility
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
			this.buttonAssignColor = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.label2 = new System.Windows.Forms.Label();
			this.radioButtonL = new System.Windows.Forms.RadioButton();
			this.radioButtona = new System.Windows.Forms.RadioButton();
			this.radioButtonb = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlColorTemperature = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlLuminance = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlSaturation = new UIUtility.FloatTrackbarControl();
			this.floatTrackbarControlAlpha = new UIUtility.GradientFloatTrackbarControl();
			this.gradientFloatTrackbarControlb = new UIUtility.GradientFloatTrackbarControl();
			this.floatTrackbarControlBlue = new UIUtility.GradientFloatTrackbarControl();
			this.gradientFloatTrackbarControla = new UIUtility.GradientFloatTrackbarControl();
			this.floatTrackbarControlGreen = new UIUtility.GradientFloatTrackbarControl();
			this.gradientFloatTrackbarControlL = new UIUtility.GradientFloatTrackbarControl();
			this.floatTrackbarControlRed = new UIUtility.GradientFloatTrackbarControl();
			this.floatTrackbarControlHue = new UIUtility.FloatTrackbarControl();
			this.radioButtonPalette35 = new UIUtility.PaletteButton();
			this.radioButtonPalette23 = new UIUtility.PaletteButton();
			this.radioButtonPalette11 = new UIUtility.PaletteButton();
			this.radioButtonPalette34 = new UIUtility.PaletteButton();
			this.radioButtonPalette22 = new UIUtility.PaletteButton();
			this.radioButtonPalette10 = new UIUtility.PaletteButton();
			this.radioButtonPalette33 = new UIUtility.PaletteButton();
			this.radioButtonPalette21 = new UIUtility.PaletteButton();
			this.radioButtonPalette9 = new UIUtility.PaletteButton();
			this.radioButtonPalette32 = new UIUtility.PaletteButton();
			this.radioButtonPalette20 = new UIUtility.PaletteButton();
			this.radioButtonPalette8 = new UIUtility.PaletteButton();
			this.radioButtonPalette31 = new UIUtility.PaletteButton();
			this.radioButtonPalette19 = new UIUtility.PaletteButton();
			this.radioButtonPalette7 = new UIUtility.PaletteButton();
			this.radioButtonPalette30 = new UIUtility.PaletteButton();
			this.radioButtonPalette18 = new UIUtility.PaletteButton();
			this.radioButtonPalette6 = new UIUtility.PaletteButton();
			this.radioButtonPalette29 = new UIUtility.PaletteButton();
			this.radioButtonPalette17 = new UIUtility.PaletteButton();
			this.radioButtonPalette5 = new UIUtility.PaletteButton();
			this.radioButtonPalette28 = new UIUtility.PaletteButton();
			this.radioButtonPalette16 = new UIUtility.PaletteButton();
			this.radioButtonPalette4 = new UIUtility.PaletteButton();
			this.radioButtonPalette27 = new UIUtility.PaletteButton();
			this.radioButtonPalette15 = new UIUtility.PaletteButton();
			this.radioButtonPalette3 = new UIUtility.PaletteButton();
			this.radioButtonPalette26 = new UIUtility.PaletteButton();
			this.radioButtonPalette14 = new UIUtility.PaletteButton();
			this.radioButtonPalette2 = new UIUtility.PaletteButton();
			this.radioButtonPalette25 = new UIUtility.PaletteButton();
			this.radioButtonPalette13 = new UIUtility.PaletteButton();
			this.radioButtonPalette1 = new UIUtility.PaletteButton();
			this.radioButtonPalette24 = new UIUtility.PaletteButton();
			this.radioButtonPalette12 = new UIUtility.PaletteButton();
			this.radioButtonPalette0 = new UIUtility.PaletteButton();
			this.colorBoxControl = new UIUtility.ColorBoxControl();
			this.sliderControlHSL = new UIUtility.VerticalColorSliderControl();
			((System.ComponentModel.ISupportInitialize)(this.m_pbx_BlankBox)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// m_lbl_SelectColor
			// 
			this.m_lbl_SelectColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_lbl_SelectColor.Location = new System.Drawing.Point(12, 113);
			this.m_lbl_SelectColor.Name = "m_lbl_SelectColor";
			this.m_lbl_SelectColor.Size = new System.Drawing.Size(260, 20);
			this.m_lbl_SelectColor.TabIndex = 0;
			this.m_lbl_SelectColor.Text = "Select Color :";
			// 
			// m_pbx_BlankBox
			// 
			this.m_pbx_BlankBox.BackColor = System.Drawing.Color.Black;
			this.m_pbx_BlankBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_pbx_BlankBox.Location = new System.Drawing.Point(348, 20);
			this.m_pbx_BlankBox.Name = "m_pbx_BlankBox";
			this.m_pbx_BlankBox.Size = new System.Drawing.Size(62, 70);
			this.m_pbx_BlankBox.TabIndex = 3;
			this.m_pbx_BlankBox.TabStop = false;
			// 
			// m_cmd_OK
			// 
			this.m_cmd_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cmd_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.m_cmd_OK.Location = new System.Drawing.Point(665, 346);
			this.m_cmd_OK.Name = "m_cmd_OK";
			this.m_cmd_OK.Size = new System.Drawing.Size(94, 21);
			this.m_cmd_OK.TabIndex = 4;
			this.m_cmd_OK.Text = "OK";
			// 
			// m_cmd_Cancel
			// 
			this.m_cmd_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cmd_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_cmd_Cancel.Location = new System.Drawing.Point(665, 374);
			this.m_cmd_Cancel.Name = "m_cmd_Cancel";
			this.m_cmd_Cancel.Size = new System.Drawing.Size(94, 21);
			this.m_cmd_Cancel.TabIndex = 5;
			this.m_cmd_Cancel.Text = "Cancel";
			// 
			// textBoxHexa
			// 
			this.textBoxHexa.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxHexa.Location = new System.Drawing.Point(447, 20);
			this.textBoxHexa.Name = "textBoxHexa";
			this.textBoxHexa.Size = new System.Drawing.Size(73, 21);
			this.textBoxHexa.TabIndex = 19;
			this.textBoxHexa.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textBoxHexa.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxHexa_KeyDown);
			this.textBoxHexa.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxHexa_Validating);
			// 
			// buttonHue
			// 
			this.buttonHue.Checked = true;
			this.buttonHue.Location = new System.Drawing.Point(318, 133);
			this.buttonHue.Name = "buttonHue";
			this.buttonHue.Size = new System.Drawing.Size(36, 24);
			this.buttonHue.TabIndex = 20;
			this.buttonHue.TabStop = true;
			this.buttonHue.Text = "H:";
			this.buttonHue.CheckedChanged += new System.EventHandler(this.buttonHue_CheckedChanged);
			// 
			// buttonSaturation
			// 
			this.buttonSaturation.Location = new System.Drawing.Point(318, 158);
			this.buttonSaturation.Name = "buttonSaturation";
			this.buttonSaturation.Size = new System.Drawing.Size(35, 24);
			this.buttonSaturation.TabIndex = 21;
			this.buttonSaturation.Text = "S:";
			this.buttonSaturation.CheckedChanged += new System.EventHandler(this.buttonSaturation_CheckedChanged);
			// 
			// buttonBrightness
			// 
			this.buttonBrightness.Location = new System.Drawing.Point(318, 183);
			this.buttonBrightness.Name = "buttonBrightness";
			this.buttonBrightness.Size = new System.Drawing.Size(35, 24);
			this.buttonBrightness.TabIndex = 22;
			this.buttonBrightness.Text = "B:";
			this.buttonBrightness.CheckedChanged += new System.EventHandler(this.buttonBrightness_CheckedChanged);
			// 
			// buttonRed
			// 
			this.buttonRed.Location = new System.Drawing.Point(318, 226);
			this.buttonRed.Name = "buttonRed";
			this.buttonRed.Size = new System.Drawing.Size(36, 24);
			this.buttonRed.TabIndex = 23;
			this.buttonRed.Text = "R:";
			this.buttonRed.CheckedChanged += new System.EventHandler(this.buttonRed_CheckedChanged);
			// 
			// buttonGreen
			// 
			this.buttonGreen.Location = new System.Drawing.Point(318, 251);
			this.buttonGreen.Name = "buttonGreen";
			this.buttonGreen.Size = new System.Drawing.Size(36, 24);
			this.buttonGreen.TabIndex = 24;
			this.buttonGreen.Text = "G:";
			this.buttonGreen.CheckedChanged += new System.EventHandler(this.buttonGreen_CheckedChanged);
			// 
			// buttonBlue
			// 
			this.buttonBlue.Location = new System.Drawing.Point(318, 276);
			this.buttonBlue.Name = "buttonBlue";
			this.buttonBlue.Size = new System.Drawing.Size(35, 24);
			this.buttonBlue.TabIndex = 25;
			this.buttonBlue.Text = "B:";
			this.buttonBlue.CheckedChanged += new System.EventHandler(this.buttonBlue_CheckedChanged);
			// 
			// m_lbl_HexPound
			// 
			this.m_lbl_HexPound.Location = new System.Drawing.Point(431, 24);
			this.m_lbl_HexPound.Name = "m_lbl_HexPound";
			this.m_lbl_HexPound.Size = new System.Drawing.Size(16, 14);
			this.m_lbl_HexPound.TabIndex = 27;
			this.m_lbl_HexPound.Text = "#";
			// 
			// labelPrimaryColor
			// 
			this.labelPrimaryColor.Location = new System.Drawing.Point(349, 21);
			this.labelPrimaryColor.Name = "labelPrimaryColor";
			this.labelPrimaryColor.Size = new System.Drawing.Size(60, 34);
			this.labelPrimaryColor.TabIndex = 36;
			this.labelPrimaryColor.Click += new System.EventHandler(this.labelPrimaryColor_Click);
			// 
			// labelSecondaryColor
			// 
			this.labelSecondaryColor.Location = new System.Drawing.Point(349, 55);
			this.labelSecondaryColor.Name = "labelSecondaryColor";
			this.labelSecondaryColor.Size = new System.Drawing.Size(60, 34);
			this.labelSecondaryColor.TabIndex = 37;
			this.labelSecondaryColor.Click += new System.EventHandler(this.labelSecondaryColor_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.radioButtonPalette35);
			this.groupBox1.Controls.Add(this.radioButtonPalette23);
			this.groupBox1.Controls.Add(this.radioButtonPalette11);
			this.groupBox1.Controls.Add(this.radioButtonPalette34);
			this.groupBox1.Controls.Add(this.radioButtonPalette22);
			this.groupBox1.Controls.Add(this.radioButtonPalette10);
			this.groupBox1.Controls.Add(this.radioButtonPalette33);
			this.groupBox1.Controls.Add(this.radioButtonPalette21);
			this.groupBox1.Controls.Add(this.radioButtonPalette9);
			this.groupBox1.Controls.Add(this.radioButtonPalette32);
			this.groupBox1.Controls.Add(this.radioButtonPalette20);
			this.groupBox1.Controls.Add(this.radioButtonPalette8);
			this.groupBox1.Controls.Add(this.radioButtonPalette31);
			this.groupBox1.Controls.Add(this.radioButtonPalette19);
			this.groupBox1.Controls.Add(this.radioButtonPalette7);
			this.groupBox1.Controls.Add(this.radioButtonPalette30);
			this.groupBox1.Controls.Add(this.radioButtonPalette18);
			this.groupBox1.Controls.Add(this.radioButtonPalette6);
			this.groupBox1.Controls.Add(this.radioButtonPalette29);
			this.groupBox1.Controls.Add(this.radioButtonPalette17);
			this.groupBox1.Controls.Add(this.radioButtonPalette5);
			this.groupBox1.Controls.Add(this.radioButtonPalette28);
			this.groupBox1.Controls.Add(this.radioButtonPalette16);
			this.groupBox1.Controls.Add(this.radioButtonPalette4);
			this.groupBox1.Controls.Add(this.radioButtonPalette27);
			this.groupBox1.Controls.Add(this.radioButtonPalette15);
			this.groupBox1.Controls.Add(this.radioButtonPalette3);
			this.groupBox1.Controls.Add(this.radioButtonPalette26);
			this.groupBox1.Controls.Add(this.radioButtonPalette14);
			this.groupBox1.Controls.Add(this.radioButtonPalette2);
			this.groupBox1.Controls.Add(this.radioButtonPalette25);
			this.groupBox1.Controls.Add(this.radioButtonPalette13);
			this.groupBox1.Controls.Add(this.radioButtonPalette1);
			this.groupBox1.Controls.Add(this.radioButtonPalette24);
			this.groupBox1.Controls.Add(this.radioButtonPalette12);
			this.groupBox1.Controls.Add(this.radioButtonPalette0);
			this.groupBox1.Location = new System.Drawing.Point(10, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(296, 95);
			this.groupBox1.TabIndex = 42;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Palette";
			// 
			// buttonAssignColor
			// 
			this.buttonAssignColor.Image = global::UIUtility.Properties.Resources.eyedropper;
			this.buttonAssignColor.Location = new System.Drawing.Point(314, 23);
			this.buttonAssignColor.Name = "buttonAssignColor";
			this.buttonAssignColor.Size = new System.Drawing.Size(26, 26);
			this.buttonAssignColor.TabIndex = 44;
			this.buttonAssignColor.UseVisualStyleBackColor = true;
			this.buttonAssignColor.Click += new System.EventHandler(this.buttonAssignColor_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(334, 306);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(17, 13);
			this.label2.TabIndex = 46;
			this.label2.Text = "A:";
			// 
			// radioButtonL
			// 
			this.radioButtonL.Location = new System.Drawing.Point(552, 133);
			this.radioButtonL.Name = "radioButtonL";
			this.radioButtonL.Size = new System.Drawing.Size(38, 24);
			this.radioButtonL.TabIndex = 23;
			this.radioButtonL.Text = "L*:";
			this.radioButtonL.CheckedChanged += new System.EventHandler(this.radioButtonL_CheckedChanged);
			// 
			// radioButtona
			// 
			this.radioButtona.Location = new System.Drawing.Point(552, 158);
			this.radioButtona.Name = "radioButtona";
			this.radioButtona.Size = new System.Drawing.Size(38, 24);
			this.radioButtona.TabIndex = 24;
			this.radioButtona.Text = "a*:";
			this.radioButtona.CheckedChanged += new System.EventHandler(this.radioButtona_CheckedChanged);
			// 
			// radioButtonb
			// 
			this.radioButtonb.Location = new System.Drawing.Point(552, 183);
			this.radioButtonb.Name = "radioButtonb";
			this.radioButtonb.Size = new System.Drawing.Size(38, 24);
			this.radioButtonb.TabIndex = 25;
			this.radioButtonb.Text = "b*:";
			this.radioButtonb.CheckedChanged += new System.EventHandler(this.radioButtonb_CheckedChanged);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(588, 230);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(171, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "Color Temperature (Kelvin):";
			// 
			// floatTrackbarControlColorTemperature
			// 
			this.floatTrackbarControlColorTemperature.Location = new System.Drawing.Point(588, 251);
			this.floatTrackbarControlColorTemperature.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlColorTemperature.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlColorTemperature.Name = "floatTrackbarControlColorTemperature";
			this.floatTrackbarControlColorTemperature.RangeMax = 20000F;
			this.floatTrackbarControlColorTemperature.RangeMin = 4000F;
			this.floatTrackbarControlColorTemperature.Size = new System.Drawing.Size(170, 20);
			this.floatTrackbarControlColorTemperature.TabIndex = 47;
			this.floatTrackbarControlColorTemperature.Value = 5500F;
			this.floatTrackbarControlColorTemperature.VisibleRangeMax = 10000F;
			this.floatTrackbarControlColorTemperature.VisibleRangeMin = 4000F;
			this.floatTrackbarControlColorTemperature.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlColorTemperature_ValueChanged);
			// 
			// floatTrackbarControlLuminance
			// 
			this.floatTrackbarControlLuminance.Location = new System.Drawing.Point(355, 185);
			this.floatTrackbarControlLuminance.MaximumSize = new System.Drawing.Size(100000, 20);
			this.floatTrackbarControlLuminance.MinimumSize = new System.Drawing.Size(0, 20);
			this.floatTrackbarControlLuminance.Name = "floatTrackbarControlLuminance";
			this.floatTrackbarControlLuminance.Size = new System.Drawing.Size(170, 20);
			this.floatTrackbarControlLuminance.TabIndex = 45;
			this.toolTip1.SetToolTip(this.floatTrackbarControlLuminance, "Luminance");
			this.floatTrackbarControlLuminance.Value = 0F;
			this.floatTrackbarControlLuminance.VisibleRangeMax = 1F;
			this.floatTrackbarControlLuminance.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlLuminance_ValueChanged);
			// 
			// floatTrackbarControlSaturation
			// 
			this.floatTrackbarControlSaturation.Location = new System.Drawing.Point(355, 160);
			this.floatTrackbarControlSaturation.MaximumSize = new System.Drawing.Size(100000, 20);
			this.floatTrackbarControlSaturation.MinimumSize = new System.Drawing.Size(0, 20);
			this.floatTrackbarControlSaturation.Name = "floatTrackbarControlSaturation";
			this.floatTrackbarControlSaturation.RangeMax = 1F;
			this.floatTrackbarControlSaturation.RangeMin = 0F;
			this.floatTrackbarControlSaturation.Size = new System.Drawing.Size(170, 20);
			this.floatTrackbarControlSaturation.TabIndex = 45;
			this.toolTip1.SetToolTip(this.floatTrackbarControlSaturation, "Saturation (in [0,1])");
			this.floatTrackbarControlSaturation.Value = 0F;
			this.floatTrackbarControlSaturation.VisibleRangeMax = 1F;
			this.floatTrackbarControlSaturation.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlSaturation_ValueChanged);
			// 
			// floatTrackbarControlAlpha
			// 
			this.floatTrackbarControlAlpha.ColorMax = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.floatTrackbarControlAlpha.ColorMin = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.floatTrackbarControlAlpha.Location = new System.Drawing.Point(355, 303);
			this.floatTrackbarControlAlpha.MaximumSize = new System.Drawing.Size(100000, 20);
			this.floatTrackbarControlAlpha.MinimumSize = new System.Drawing.Size(0, 20);
			this.floatTrackbarControlAlpha.Name = "floatTrackbarControlAlpha";
			this.floatTrackbarControlAlpha.RangeMin = 0F;
			this.floatTrackbarControlAlpha.Size = new System.Drawing.Size(170, 20);
			this.floatTrackbarControlAlpha.TabIndex = 45;
			this.toolTip1.SetToolTip(this.floatTrackbarControlAlpha, "Alpha component");
			this.floatTrackbarControlAlpha.Value = 0F;
			this.floatTrackbarControlAlpha.VisibleRangeMax = 1F;
			this.floatTrackbarControlAlpha.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlAlpha_ValueChanged);
			// 
			// gradientFloatTrackbarControlb
			// 
			this.gradientFloatTrackbarControlb.ColorMax = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.gradientFloatTrackbarControlb.ColorMin = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.gradientFloatTrackbarControlb.Location = new System.Drawing.Point(588, 185);
			this.gradientFloatTrackbarControlb.MaximumSize = new System.Drawing.Size(100000, 20);
			this.gradientFloatTrackbarControlb.MinimumSize = new System.Drawing.Size(0, 20);
			this.gradientFloatTrackbarControlb.Name = "gradientFloatTrackbarControlb";
			this.gradientFloatTrackbarControlb.RangeMax = 128F;
			this.gradientFloatTrackbarControlb.RangeMin = -128F;
			this.gradientFloatTrackbarControlb.Size = new System.Drawing.Size(170, 20);
			this.gradientFloatTrackbarControlb.TabIndex = 45;
			this.gradientFloatTrackbarControlb.Value = 0F;
			this.gradientFloatTrackbarControlb.VisibleRangeMax = 128F;
			this.gradientFloatTrackbarControlb.VisibleRangeMin = -128F;
			this.gradientFloatTrackbarControlb.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.gradientFloatTrackbarControlb_ValueChanged);
			// 
			// floatTrackbarControlBlue
			// 
			this.floatTrackbarControlBlue.ColorMax = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.floatTrackbarControlBlue.ColorMin = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.floatTrackbarControlBlue.Location = new System.Drawing.Point(355, 278);
			this.floatTrackbarControlBlue.MaximumSize = new System.Drawing.Size(100000, 20);
			this.floatTrackbarControlBlue.MinimumSize = new System.Drawing.Size(0, 20);
			this.floatTrackbarControlBlue.Name = "floatTrackbarControlBlue";
			this.floatTrackbarControlBlue.RangeMin = 0F;
			this.floatTrackbarControlBlue.Size = new System.Drawing.Size(170, 20);
			this.floatTrackbarControlBlue.TabIndex = 45;
			this.toolTip1.SetToolTip(this.floatTrackbarControlBlue, "Blue Component");
			this.floatTrackbarControlBlue.Value = 0F;
			this.floatTrackbarControlBlue.VisibleRangeMax = 1F;
			this.floatTrackbarControlBlue.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlBlue_ValueChanged);
			// 
			// gradientFloatTrackbarControla
			// 
			this.gradientFloatTrackbarControla.ColorMax = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.gradientFloatTrackbarControla.ColorMin = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.gradientFloatTrackbarControla.Location = new System.Drawing.Point(588, 160);
			this.gradientFloatTrackbarControla.MaximumSize = new System.Drawing.Size(100000, 20);
			this.gradientFloatTrackbarControla.MinimumSize = new System.Drawing.Size(0, 20);
			this.gradientFloatTrackbarControla.Name = "gradientFloatTrackbarControla";
			this.gradientFloatTrackbarControla.RangeMax = 128F;
			this.gradientFloatTrackbarControla.RangeMin = -128F;
			this.gradientFloatTrackbarControla.Size = new System.Drawing.Size(170, 20);
			this.gradientFloatTrackbarControla.TabIndex = 45;
			this.gradientFloatTrackbarControla.Value = 0F;
			this.gradientFloatTrackbarControla.VisibleRangeMax = 127F;
			this.gradientFloatTrackbarControla.VisibleRangeMin = -128F;
			this.gradientFloatTrackbarControla.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.gradientFloatTrackbarControla_ValueChanged);
			// 
			// floatTrackbarControlGreen
			// 
			this.floatTrackbarControlGreen.ColorMax = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.floatTrackbarControlGreen.ColorMin = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.floatTrackbarControlGreen.Location = new System.Drawing.Point(355, 253);
			this.floatTrackbarControlGreen.MaximumSize = new System.Drawing.Size(100000, 20);
			this.floatTrackbarControlGreen.MinimumSize = new System.Drawing.Size(0, 20);
			this.floatTrackbarControlGreen.Name = "floatTrackbarControlGreen";
			this.floatTrackbarControlGreen.RangeMin = 0F;
			this.floatTrackbarControlGreen.Size = new System.Drawing.Size(170, 20);
			this.floatTrackbarControlGreen.TabIndex = 45;
			this.toolTip1.SetToolTip(this.floatTrackbarControlGreen, "Green Component");
			this.floatTrackbarControlGreen.Value = 0F;
			this.floatTrackbarControlGreen.VisibleRangeMax = 1F;
			this.floatTrackbarControlGreen.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlGreen_ValueChanged);
			// 
			// gradientFloatTrackbarControlL
			// 
			this.gradientFloatTrackbarControlL.ColorMax = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.gradientFloatTrackbarControlL.ColorMin = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.gradientFloatTrackbarControlL.Location = new System.Drawing.Point(588, 135);
			this.gradientFloatTrackbarControlL.MaximumSize = new System.Drawing.Size(100000, 20);
			this.gradientFloatTrackbarControlL.MinimumSize = new System.Drawing.Size(0, 20);
			this.gradientFloatTrackbarControlL.Name = "gradientFloatTrackbarControlL";
			this.gradientFloatTrackbarControlL.RangeMin = 0F;
			this.gradientFloatTrackbarControlL.Size = new System.Drawing.Size(170, 20);
			this.gradientFloatTrackbarControlL.TabIndex = 45;
			this.gradientFloatTrackbarControlL.Value = 100F;
			this.gradientFloatTrackbarControlL.VisibleRangeMax = 100F;
			this.gradientFloatTrackbarControlL.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.gradientFloatTrackbarControlL_ValueChanged);
			// 
			// floatTrackbarControlRed
			// 
			this.floatTrackbarControlRed.ColorMax = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.floatTrackbarControlRed.ColorMin = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.floatTrackbarControlRed.Location = new System.Drawing.Point(355, 228);
			this.floatTrackbarControlRed.MaximumSize = new System.Drawing.Size(100000, 20);
			this.floatTrackbarControlRed.MinimumSize = new System.Drawing.Size(0, 20);
			this.floatTrackbarControlRed.Name = "floatTrackbarControlRed";
			this.floatTrackbarControlRed.RangeMin = 0F;
			this.floatTrackbarControlRed.Size = new System.Drawing.Size(170, 20);
			this.floatTrackbarControlRed.TabIndex = 45;
			this.toolTip1.SetToolTip(this.floatTrackbarControlRed, "Red Component");
			this.floatTrackbarControlRed.Value = 0F;
			this.floatTrackbarControlRed.VisibleRangeMax = 1F;
			this.floatTrackbarControlRed.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlRed_ValueChanged);
			// 
			// floatTrackbarControlHue
			// 
			this.floatTrackbarControlHue.Location = new System.Drawing.Point(355, 135);
			this.floatTrackbarControlHue.MaximumSize = new System.Drawing.Size(100000, 20);
			this.floatTrackbarControlHue.MinimumSize = new System.Drawing.Size(0, 20);
			this.floatTrackbarControlHue.Name = "floatTrackbarControlHue";
			this.floatTrackbarControlHue.RangeMax = 360F;
			this.floatTrackbarControlHue.RangeMin = 0F;
			this.floatTrackbarControlHue.Size = new System.Drawing.Size(170, 20);
			this.floatTrackbarControlHue.TabIndex = 45;
			this.toolTip1.SetToolTip(this.floatTrackbarControlHue, "Hue (in degress)");
			this.floatTrackbarControlHue.Value = 0F;
			this.floatTrackbarControlHue.VisibleRangeMax = 360F;
			this.floatTrackbarControlHue.ValueChanged += new UIUtility.FloatTrackbarControl.ValueChangedEventHandler(this.floatTrackbarControlHue_ValueChanged);
			// 
			// radioButtonPalette35
			// 
			this.radioButtonPalette35.AutoSize = true;
			this.radioButtonPalette35.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette35.Location = new System.Drawing.Point(270, 67);
			this.radioButtonPalette35.Name = "radioButtonPalette35";
			this.radioButtonPalette35.Selected = false;
			this.radioButtonPalette35.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette35.TabIndex = 0;
			// 
			// radioButtonPalette23
			// 
			this.radioButtonPalette23.AutoSize = true;
			this.radioButtonPalette23.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette23.Location = new System.Drawing.Point(270, 43);
			this.radioButtonPalette23.Name = "radioButtonPalette23";
			this.radioButtonPalette23.Selected = false;
			this.radioButtonPalette23.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette23.TabIndex = 0;
			// 
			// radioButtonPalette11
			// 
			this.radioButtonPalette11.AutoSize = true;
			this.radioButtonPalette11.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette11.Location = new System.Drawing.Point(270, 19);
			this.radioButtonPalette11.Name = "radioButtonPalette11";
			this.radioButtonPalette11.Selected = false;
			this.radioButtonPalette11.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette11.TabIndex = 0;
			// 
			// radioButtonPalette34
			// 
			this.radioButtonPalette34.AutoSize = true;
			this.radioButtonPalette34.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette34.Location = new System.Drawing.Point(246, 67);
			this.radioButtonPalette34.Name = "radioButtonPalette34";
			this.radioButtonPalette34.Selected = false;
			this.radioButtonPalette34.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette34.TabIndex = 0;
			// 
			// radioButtonPalette22
			// 
			this.radioButtonPalette22.AutoSize = true;
			this.radioButtonPalette22.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette22.Location = new System.Drawing.Point(246, 43);
			this.radioButtonPalette22.Name = "radioButtonPalette22";
			this.radioButtonPalette22.Selected = false;
			this.radioButtonPalette22.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette22.TabIndex = 0;
			// 
			// radioButtonPalette10
			// 
			this.radioButtonPalette10.AutoSize = true;
			this.radioButtonPalette10.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette10.Location = new System.Drawing.Point(246, 19);
			this.radioButtonPalette10.Name = "radioButtonPalette10";
			this.radioButtonPalette10.Selected = false;
			this.radioButtonPalette10.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette10.TabIndex = 0;
			// 
			// radioButtonPalette33
			// 
			this.radioButtonPalette33.AutoSize = true;
			this.radioButtonPalette33.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette33.Location = new System.Drawing.Point(222, 67);
			this.radioButtonPalette33.Name = "radioButtonPalette33";
			this.radioButtonPalette33.Selected = false;
			this.radioButtonPalette33.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette33.TabIndex = 0;
			// 
			// radioButtonPalette21
			// 
			this.radioButtonPalette21.AutoSize = true;
			this.radioButtonPalette21.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette21.Location = new System.Drawing.Point(222, 43);
			this.radioButtonPalette21.Name = "radioButtonPalette21";
			this.radioButtonPalette21.Selected = false;
			this.radioButtonPalette21.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette21.TabIndex = 0;
			// 
			// radioButtonPalette9
			// 
			this.radioButtonPalette9.AutoSize = true;
			this.radioButtonPalette9.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette9.Location = new System.Drawing.Point(222, 19);
			this.radioButtonPalette9.Name = "radioButtonPalette9";
			this.radioButtonPalette9.Selected = false;
			this.radioButtonPalette9.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette9.TabIndex = 0;
			// 
			// radioButtonPalette32
			// 
			this.radioButtonPalette32.AutoSize = true;
			this.radioButtonPalette32.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette32.Location = new System.Drawing.Point(198, 67);
			this.radioButtonPalette32.Name = "radioButtonPalette32";
			this.radioButtonPalette32.Selected = false;
			this.radioButtonPalette32.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette32.TabIndex = 0;
			// 
			// radioButtonPalette20
			// 
			this.radioButtonPalette20.AutoSize = true;
			this.radioButtonPalette20.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette20.Location = new System.Drawing.Point(198, 43);
			this.radioButtonPalette20.Name = "radioButtonPalette20";
			this.radioButtonPalette20.Selected = false;
			this.radioButtonPalette20.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette20.TabIndex = 0;
			// 
			// radioButtonPalette8
			// 
			this.radioButtonPalette8.AutoSize = true;
			this.radioButtonPalette8.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette8.Location = new System.Drawing.Point(198, 19);
			this.radioButtonPalette8.Name = "radioButtonPalette8";
			this.radioButtonPalette8.Selected = false;
			this.radioButtonPalette8.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette8.TabIndex = 0;
			// 
			// radioButtonPalette31
			// 
			this.radioButtonPalette31.AutoSize = true;
			this.radioButtonPalette31.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette31.Location = new System.Drawing.Point(174, 67);
			this.radioButtonPalette31.Name = "radioButtonPalette31";
			this.radioButtonPalette31.Selected = false;
			this.radioButtonPalette31.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette31.TabIndex = 0;
			// 
			// radioButtonPalette19
			// 
			this.radioButtonPalette19.AutoSize = true;
			this.radioButtonPalette19.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette19.Location = new System.Drawing.Point(174, 43);
			this.radioButtonPalette19.Name = "radioButtonPalette19";
			this.radioButtonPalette19.Selected = false;
			this.radioButtonPalette19.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette19.TabIndex = 0;
			// 
			// radioButtonPalette7
			// 
			this.radioButtonPalette7.AutoSize = true;
			this.radioButtonPalette7.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette7.Location = new System.Drawing.Point(174, 19);
			this.radioButtonPalette7.Name = "radioButtonPalette7";
			this.radioButtonPalette7.Selected = false;
			this.radioButtonPalette7.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette7.TabIndex = 0;
			// 
			// radioButtonPalette30
			// 
			this.radioButtonPalette30.AutoSize = true;
			this.radioButtonPalette30.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette30.Location = new System.Drawing.Point(150, 67);
			this.radioButtonPalette30.Name = "radioButtonPalette30";
			this.radioButtonPalette30.Selected = false;
			this.radioButtonPalette30.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette30.TabIndex = 0;
			// 
			// radioButtonPalette18
			// 
			this.radioButtonPalette18.AutoSize = true;
			this.radioButtonPalette18.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette18.Location = new System.Drawing.Point(150, 43);
			this.radioButtonPalette18.Name = "radioButtonPalette18";
			this.radioButtonPalette18.Selected = false;
			this.radioButtonPalette18.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette18.TabIndex = 0;
			// 
			// radioButtonPalette6
			// 
			this.radioButtonPalette6.AutoSize = true;
			this.radioButtonPalette6.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette6.Location = new System.Drawing.Point(150, 19);
			this.radioButtonPalette6.Name = "radioButtonPalette6";
			this.radioButtonPalette6.Selected = false;
			this.radioButtonPalette6.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette6.TabIndex = 0;
			// 
			// radioButtonPalette29
			// 
			this.radioButtonPalette29.AutoSize = true;
			this.radioButtonPalette29.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette29.Location = new System.Drawing.Point(126, 67);
			this.radioButtonPalette29.Name = "radioButtonPalette29";
			this.radioButtonPalette29.Selected = false;
			this.radioButtonPalette29.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette29.TabIndex = 0;
			// 
			// radioButtonPalette17
			// 
			this.radioButtonPalette17.AutoSize = true;
			this.radioButtonPalette17.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette17.Location = new System.Drawing.Point(126, 43);
			this.radioButtonPalette17.Name = "radioButtonPalette17";
			this.radioButtonPalette17.Selected = false;
			this.radioButtonPalette17.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette17.TabIndex = 0;
			// 
			// radioButtonPalette5
			// 
			this.radioButtonPalette5.AutoSize = true;
			this.radioButtonPalette5.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette5.Location = new System.Drawing.Point(126, 19);
			this.radioButtonPalette5.Name = "radioButtonPalette5";
			this.radioButtonPalette5.Selected = false;
			this.radioButtonPalette5.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette5.TabIndex = 0;
			// 
			// radioButtonPalette28
			// 
			this.radioButtonPalette28.AutoSize = true;
			this.radioButtonPalette28.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette28.Location = new System.Drawing.Point(102, 67);
			this.radioButtonPalette28.Name = "radioButtonPalette28";
			this.radioButtonPalette28.Selected = false;
			this.radioButtonPalette28.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette28.TabIndex = 0;
			// 
			// radioButtonPalette16
			// 
			this.radioButtonPalette16.AutoSize = true;
			this.radioButtonPalette16.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette16.Location = new System.Drawing.Point(102, 43);
			this.radioButtonPalette16.Name = "radioButtonPalette16";
			this.radioButtonPalette16.Selected = false;
			this.radioButtonPalette16.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette16.TabIndex = 0;
			// 
			// radioButtonPalette4
			// 
			this.radioButtonPalette4.AutoSize = true;
			this.radioButtonPalette4.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette4.Location = new System.Drawing.Point(102, 19);
			this.radioButtonPalette4.Name = "radioButtonPalette4";
			this.radioButtonPalette4.Selected = false;
			this.radioButtonPalette4.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette4.TabIndex = 0;
			// 
			// radioButtonPalette27
			// 
			this.radioButtonPalette27.AutoSize = true;
			this.radioButtonPalette27.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette27.Location = new System.Drawing.Point(78, 67);
			this.radioButtonPalette27.Name = "radioButtonPalette27";
			this.radioButtonPalette27.Selected = false;
			this.radioButtonPalette27.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette27.TabIndex = 0;
			// 
			// radioButtonPalette15
			// 
			this.radioButtonPalette15.AutoSize = true;
			this.radioButtonPalette15.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette15.Location = new System.Drawing.Point(78, 43);
			this.radioButtonPalette15.Name = "radioButtonPalette15";
			this.radioButtonPalette15.Selected = false;
			this.radioButtonPalette15.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette15.TabIndex = 0;
			// 
			// radioButtonPalette3
			// 
			this.radioButtonPalette3.AutoSize = true;
			this.radioButtonPalette3.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette3.Location = new System.Drawing.Point(78, 19);
			this.radioButtonPalette3.Name = "radioButtonPalette3";
			this.radioButtonPalette3.Selected = false;
			this.radioButtonPalette3.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette3.TabIndex = 0;
			// 
			// radioButtonPalette26
			// 
			this.radioButtonPalette26.AutoSize = true;
			this.radioButtonPalette26.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette26.Location = new System.Drawing.Point(54, 67);
			this.radioButtonPalette26.Name = "radioButtonPalette26";
			this.radioButtonPalette26.Selected = false;
			this.radioButtonPalette26.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette26.TabIndex = 0;
			// 
			// radioButtonPalette14
			// 
			this.radioButtonPalette14.AutoSize = true;
			this.radioButtonPalette14.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette14.Location = new System.Drawing.Point(54, 43);
			this.radioButtonPalette14.Name = "radioButtonPalette14";
			this.radioButtonPalette14.Selected = false;
			this.radioButtonPalette14.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette14.TabIndex = 0;
			// 
			// radioButtonPalette2
			// 
			this.radioButtonPalette2.AutoSize = true;
			this.radioButtonPalette2.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette2.Location = new System.Drawing.Point(54, 19);
			this.radioButtonPalette2.Name = "radioButtonPalette2";
			this.radioButtonPalette2.Selected = false;
			this.radioButtonPalette2.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette2.TabIndex = 0;
			// 
			// radioButtonPalette25
			// 
			this.radioButtonPalette25.AutoSize = true;
			this.radioButtonPalette25.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette25.Location = new System.Drawing.Point(30, 67);
			this.radioButtonPalette25.Name = "radioButtonPalette25";
			this.radioButtonPalette25.Selected = false;
			this.radioButtonPalette25.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette25.TabIndex = 0;
			// 
			// radioButtonPalette13
			// 
			this.radioButtonPalette13.AutoSize = true;
			this.radioButtonPalette13.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette13.Location = new System.Drawing.Point(30, 43);
			this.radioButtonPalette13.Name = "radioButtonPalette13";
			this.radioButtonPalette13.Selected = false;
			this.radioButtonPalette13.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette13.TabIndex = 0;
			// 
			// radioButtonPalette1
			// 
			this.radioButtonPalette1.AutoSize = true;
			this.radioButtonPalette1.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette1.Location = new System.Drawing.Point(30, 19);
			this.radioButtonPalette1.Name = "radioButtonPalette1";
			this.radioButtonPalette1.Selected = false;
			this.radioButtonPalette1.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette1.TabIndex = 0;
			// 
			// radioButtonPalette24
			// 
			this.radioButtonPalette24.AutoSize = true;
			this.radioButtonPalette24.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette24.Location = new System.Drawing.Point(6, 67);
			this.radioButtonPalette24.Name = "radioButtonPalette24";
			this.radioButtonPalette24.Selected = false;
			this.radioButtonPalette24.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette24.TabIndex = 0;
			// 
			// radioButtonPalette12
			// 
			this.radioButtonPalette12.AutoSize = true;
			this.radioButtonPalette12.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette12.Location = new System.Drawing.Point(6, 43);
			this.radioButtonPalette12.Name = "radioButtonPalette12";
			this.radioButtonPalette12.Selected = false;
			this.radioButtonPalette12.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette12.TabIndex = 0;
			// 
			// radioButtonPalette0
			// 
			this.radioButtonPalette0.AutoSize = true;
			this.radioButtonPalette0.BackColor = System.Drawing.Color.White;
			this.radioButtonPalette0.Location = new System.Drawing.Point(6, 19);
			this.radioButtonPalette0.Name = "radioButtonPalette0";
			this.radioButtonPalette0.Selected = true;
			this.radioButtonPalette0.Size = new System.Drawing.Size(18, 18);
			this.radioButtonPalette0.TabIndex = 0;
			this.radioButtonPalette0.TabStop = true;
			// 
			// colorBoxControl
			// 
			this.colorBoxControl.DrawStyle = UIUtility.ColorPickerForm.DRAW_STYLE.Hue;
			this.colorBoxControl.Location = new System.Drawing.Point(12, 133);
			this.colorBoxControl.Name = "colorBoxControl";
			this.colorBoxControl.Size = new System.Drawing.Size(260, 260);
			this.colorBoxControl.TabIndex = 39;
			this.colorBoxControl.Scroll += new System.EventHandler(this.colorBoxControl_Scroll);
			// 
			// sliderControlHSL
			// 
			this.sliderControlHSL.DrawStyle = UIUtility.ColorPickerForm.DRAW_STYLE.Hue;
			this.sliderControlHSL.Location = new System.Drawing.Point(272, 131);
			this.sliderControlHSL.Name = "sliderControlHSL";
			this.sliderControlHSL.Size = new System.Drawing.Size(40, 264);
			this.sliderControlHSL.TabIndex = 38;
			this.sliderControlHSL.Scroll += new System.EventHandler(this.sliderControlHSL_Scroll);
			// 
			// ColorPickerForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.m_cmd_Cancel;
			this.ClientSize = new System.Drawing.Size(768, 407);
			this.Controls.Add(this.floatTrackbarControlColorTemperature);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.floatTrackbarControlLuminance);
			this.Controls.Add(this.floatTrackbarControlSaturation);
			this.Controls.Add(this.floatTrackbarControlAlpha);
			this.Controls.Add(this.gradientFloatTrackbarControlb);
			this.Controls.Add(this.floatTrackbarControlBlue);
			this.Controls.Add(this.gradientFloatTrackbarControla);
			this.Controls.Add(this.floatTrackbarControlGreen);
			this.Controls.Add(this.gradientFloatTrackbarControlL);
			this.Controls.Add(this.floatTrackbarControlRed);
			this.Controls.Add(this.floatTrackbarControlHue);
			this.Controls.Add(this.buttonAssignColor);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.colorBoxControl);
			this.Controls.Add(this.sliderControlHSL);
			this.Controls.Add(this.labelSecondaryColor);
			this.Controls.Add(this.labelPrimaryColor);
			this.Controls.Add(this.radioButtonb);
			this.Controls.Add(this.m_lbl_HexPound);
			this.Controls.Add(this.radioButtona);
			this.Controls.Add(this.buttonBlue);
			this.Controls.Add(this.radioButtonL);
			this.Controls.Add(this.buttonGreen);
			this.Controls.Add(this.buttonRed);
			this.Controls.Add(this.buttonBrightness);
			this.Controls.Add(this.buttonSaturation);
			this.Controls.Add(this.buttonHue);
			this.Controls.Add(this.textBoxHexa);
			this.Controls.Add(this.m_cmd_Cancel);
			this.Controls.Add(this.m_cmd_OK);
			this.Controls.Add(this.m_pbx_BlankBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.m_lbl_SelectColor);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ColorPickerForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Color Picker";
			((System.ComponentModel.ISupportInitialize)(this.m_pbx_BlankBox)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private GradientFloatTrackbarControl floatTrackbarControlAlpha;
		private Label label2;
		protected RadioButton radioButtonL;
		protected RadioButton radioButtona;
		protected RadioButton radioButtonb;
		private GradientFloatTrackbarControl gradientFloatTrackbarControlL;
		private GradientFloatTrackbarControl gradientFloatTrackbarControla;
		private GradientFloatTrackbarControl gradientFloatTrackbarControlb;
		private FloatTrackbarControl floatTrackbarControlColorTemperature;
		protected Label label1;

	}
}
