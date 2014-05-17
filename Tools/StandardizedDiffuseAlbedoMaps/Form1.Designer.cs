namespace StandardizedDiffuseAlbedoMaps
{
	partial class Form1
	{
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
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.openFileDialogSourceImage = new System.Windows.Forms.OpenFileDialog();
			this.buttonLoadImage = new System.Windows.Forms.Button();
			this.labelLuminance = new System.Windows.Forms.Label();
			this.checkBoxsRGB = new System.Windows.Forms.CheckBox();
			this.checkBoxLuminance = new System.Windows.Forms.CheckBox();
			this.checkBoxCalibrate02 = new System.Windows.Forms.CheckBox();
			this.groupBoxCameraShotInfos = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.floatTrackbarControlISOSpeed = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlFocalLength = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlAperture = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.floatTrackbarControlShutterSpeed = new Nuaj.Cirrus.Utility.FloatTrackbarControl();
			this.label15 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.labelProbeRelative99 = new System.Windows.Forms.Label();
			this.labelProbeNormalized99 = new System.Windows.Forms.Label();
			this.labelProbeValue99 = new System.Windows.Forms.Label();
			this.labelProbeRelative75 = new System.Windows.Forms.Label();
			this.labelProbeNormalized75 = new System.Windows.Forms.Label();
			this.labelProbeValue75 = new System.Windows.Forms.Label();
			this.labelProbeRelative50 = new System.Windows.Forms.Label();
			this.labelProbeNormalized50 = new System.Windows.Forms.Label();
			this.labelProbeValue50 = new System.Windows.Forms.Label();
			this.labelProbeRelative20 = new System.Windows.Forms.Label();
			this.labelProbeNormalized20 = new System.Windows.Forms.Label();
			this.labelProbeValue20 = new System.Windows.Forms.Label();
			this.labelProbeRelative10 = new System.Windows.Forms.Label();
			this.labelProbeNormalized10 = new System.Windows.Forms.Label();
			this.labelProbeValue10 = new System.Windows.Forms.Label();
			this.labelProbeRelative02 = new System.Windows.Forms.Label();
			this.labelProbeNormalized02 = new System.Windows.Forms.Label();
			this.labelProbeValue02 = new System.Windows.Forms.Label();
			this.buttonCalibrate99 = new System.Windows.Forms.Button();
			this.buttonCalibrate75 = new System.Windows.Forms.Button();
			this.buttonCalibrate50 = new System.Windows.Forms.Button();
			this.buttonCalibrate20 = new System.Windows.Forms.Button();
			this.buttonCalibrate10 = new System.Windows.Forms.Button();
			this.buttonCalibrate02 = new System.Windows.Forms.Button();
			this.checkBoxCalibrate99 = new System.Windows.Forms.CheckBox();
			this.checkBoxCalibrate75 = new System.Windows.Forms.CheckBox();
			this.checkBoxCalibrate50 = new System.Windows.Forms.CheckBox();
			this.checkBoxCalibrate20 = new System.Windows.Forms.CheckBox();
			this.checkBoxCalibrate10 = new System.Windows.Forms.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabPageCreation = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.labelCalbrationImageName = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.panelProbeLuminances = new System.Windows.Forms.Panel();
			this.buttonSaveCalibration = new System.Windows.Forms.Button();
			this.buttonLoadCalibration = new System.Windows.Forms.Button();
			this.checkBoxGraphLagrange = new System.Windows.Forms.CheckBox();
			this.graphPanel = new StandardizedDiffuseAlbedoMaps.GraphPanel(this.components);
			this.splitContainerMain = new System.Windows.Forms.SplitContainer();
			this.outputPanel = new StandardizedDiffuseAlbedoMaps.OutputPanel(this.components);
			this.openFileDialogCalibration = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogCalibration = new System.Windows.Forms.SaveFileDialog();
			this.buttonSetupDatabaseFolder = new System.Windows.Forms.Button();
			this.buttonReCalibrate = new System.Windows.Forms.Button();
			this.folderBrowserDialogDatabaseLocation = new System.Windows.Forms.FolderBrowserDialog();
			this.groupBoxCameraShotInfos.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.panelProbeLuminances.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
			this.splitContainerMain.Panel1.SuspendLayout();
			this.splitContainerMain.Panel2.SuspendLayout();
			this.splitContainerMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// openFileDialogSourceImage
			// 
			this.openFileDialogSourceImage.DefaultExt = "*.png";
			this.openFileDialogSourceImage.Filter = "All supported formats|*.PNG;*.GIF;*.BMP;*.HDR;*.CRW;*.CR2;*.DNG;*.TIFF;*.TGA;*.JP" +
    "G|All Files (*.*)|*.*";
			this.openFileDialogSourceImage.RestoreDirectory = true;
			this.openFileDialogSourceImage.Title = "Choose a source image of a diffuse albedo map...";
			// 
			// buttonLoadImage
			// 
			this.buttonLoadImage.Location = new System.Drawing.Point(362, 59);
			this.buttonLoadImage.Name = "buttonLoadImage";
			this.buttonLoadImage.Size = new System.Drawing.Size(97, 37);
			this.buttonLoadImage.TabIndex = 1;
			this.buttonLoadImage.Text = "Load Image";
			this.buttonLoadImage.UseVisualStyleBackColor = true;
			this.buttonLoadImage.Click += new System.EventHandler(this.buttonLoadImage_Click);
			// 
			// labelLuminance
			// 
			this.labelLuminance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelLuminance.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelLuminance.Location = new System.Drawing.Point(276, 718);
			this.labelLuminance.Name = "labelLuminance";
			this.labelLuminance.Size = new System.Drawing.Size(116, 23);
			this.labelLuminance.TabIndex = 2;
			this.labelLuminance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkBoxsRGB
			// 
			this.checkBoxsRGB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxsRGB.AutoSize = true;
			this.checkBoxsRGB.Checked = true;
			this.checkBoxsRGB.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxsRGB.Location = new System.Drawing.Point(12, 722);
			this.checkBoxsRGB.Name = "checkBoxsRGB";
			this.checkBoxsRGB.Size = new System.Drawing.Size(54, 17);
			this.checkBoxsRGB.TabIndex = 3;
			this.checkBoxsRGB.Text = "sRGB";
			this.checkBoxsRGB.UseVisualStyleBackColor = true;
			this.checkBoxsRGB.CheckedChanged += new System.EventHandler(this.checkBoxsRGB_CheckedChanged);
			// 
			// checkBoxLuminance
			// 
			this.checkBoxLuminance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxLuminance.AutoSize = true;
			this.checkBoxLuminance.Checked = true;
			this.checkBoxLuminance.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxLuminance.Location = new System.Drawing.Point(89, 722);
			this.checkBoxLuminance.Name = "checkBoxLuminance";
			this.checkBoxLuminance.Size = new System.Drawing.Size(78, 17);
			this.checkBoxLuminance.TabIndex = 3;
			this.checkBoxLuminance.Text = "Luminance";
			this.checkBoxLuminance.UseVisualStyleBackColor = true;
			this.checkBoxLuminance.CheckedChanged += new System.EventHandler(this.checkBoxLuminance_CheckedChanged);
			// 
			// checkBoxCalibrate02
			// 
			this.checkBoxCalibrate02.AutoSize = true;
			this.checkBoxCalibrate02.Location = new System.Drawing.Point(3, 18);
			this.checkBoxCalibrate02.Name = "checkBoxCalibrate02";
			this.checkBoxCalibrate02.Size = new System.Drawing.Size(84, 17);
			this.checkBoxCalibrate02.TabIndex = 4;
			this.checkBoxCalibrate02.Text = "SRS-02-010";
			this.checkBoxCalibrate02.UseVisualStyleBackColor = true;
			this.checkBoxCalibrate02.CheckedChanged += new System.EventHandler(this.checkBoxCalibrate02_CheckedChanged);
			// 
			// groupBoxCameraShotInfos
			// 
			this.groupBoxCameraShotInfos.Controls.Add(this.label4);
			this.groupBoxCameraShotInfos.Controls.Add(this.label3);
			this.groupBoxCameraShotInfos.Controls.Add(this.label2);
			this.groupBoxCameraShotInfos.Controls.Add(this.label1);
			this.groupBoxCameraShotInfos.Controls.Add(this.floatTrackbarControlISOSpeed);
			this.groupBoxCameraShotInfos.Controls.Add(this.floatTrackbarControlFocalLength);
			this.groupBoxCameraShotInfos.Controls.Add(this.floatTrackbarControlAperture);
			this.groupBoxCameraShotInfos.Controls.Add(this.floatTrackbarControlShutterSpeed);
			this.groupBoxCameraShotInfos.Enabled = false;
			this.groupBoxCameraShotInfos.Location = new System.Drawing.Point(4, 9);
			this.groupBoxCameraShotInfos.Name = "groupBoxCameraShotInfos";
			this.groupBoxCameraShotInfos.Size = new System.Drawing.Size(310, 137);
			this.groupBoxCameraShotInfos.TabIndex = 10;
			this.groupBoxCameraShotInfos.TabStop = false;
			this.groupBoxCameraShotInfos.Text = "Camera Shot Info   (can\'t edit if provided by image)";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 105);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(69, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Focal Length";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(47, 13);
			this.label3.TabIndex = 9;
			this.label3.Text = "Aperture";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 54);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(75, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Shutter Speed";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 27);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 13);
			this.label1.TabIndex = 9;
			this.label1.Text = "ISO Speed";
			// 
			// floatTrackbarControlISOSpeed
			// 
			this.floatTrackbarControlISOSpeed.Location = new System.Drawing.Point(87, 24);
			this.floatTrackbarControlISOSpeed.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlISOSpeed.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlISOSpeed.Name = "floatTrackbarControlISOSpeed";
			this.floatTrackbarControlISOSpeed.RangeMax = 1000000F;
			this.floatTrackbarControlISOSpeed.RangeMin = 25F;
			this.floatTrackbarControlISOSpeed.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlISOSpeed.TabIndex = 7;
			this.floatTrackbarControlISOSpeed.Value = 100F;
			this.floatTrackbarControlISOSpeed.VisibleRangeMax = 200F;
			this.floatTrackbarControlISOSpeed.VisibleRangeMin = 25F;
			// 
			// floatTrackbarControlFocalLength
			// 
			this.floatTrackbarControlFocalLength.Location = new System.Drawing.Point(87, 102);
			this.floatTrackbarControlFocalLength.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlFocalLength.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlFocalLength.Name = "floatTrackbarControlFocalLength";
			this.floatTrackbarControlFocalLength.RangeMax = 10000F;
			this.floatTrackbarControlFocalLength.RangeMin = 0.01F;
			this.floatTrackbarControlFocalLength.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlFocalLength.TabIndex = 7;
			this.floatTrackbarControlFocalLength.Value = 55F;
			this.floatTrackbarControlFocalLength.VisibleRangeMax = 100F;
			this.floatTrackbarControlFocalLength.VisibleRangeMin = 0.01F;
			// 
			// floatTrackbarControlAperture
			// 
			this.floatTrackbarControlAperture.Location = new System.Drawing.Point(87, 76);
			this.floatTrackbarControlAperture.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlAperture.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlAperture.Name = "floatTrackbarControlAperture";
			this.floatTrackbarControlAperture.RangeMax = 100F;
			this.floatTrackbarControlAperture.RangeMin = 1F;
			this.floatTrackbarControlAperture.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlAperture.TabIndex = 7;
			this.floatTrackbarControlAperture.Value = 8F;
			this.floatTrackbarControlAperture.VisibleRangeMax = 16F;
			this.floatTrackbarControlAperture.VisibleRangeMin = 1F;
			// 
			// floatTrackbarControlShutterSpeed
			// 
			this.floatTrackbarControlShutterSpeed.Location = new System.Drawing.Point(87, 50);
			this.floatTrackbarControlShutterSpeed.MaximumSize = new System.Drawing.Size(10000, 20);
			this.floatTrackbarControlShutterSpeed.MinimumSize = new System.Drawing.Size(70, 20);
			this.floatTrackbarControlShutterSpeed.Name = "floatTrackbarControlShutterSpeed";
			this.floatTrackbarControlShutterSpeed.RangeMax = 1000F;
			this.floatTrackbarControlShutterSpeed.RangeMin = 0.0001F;
			this.floatTrackbarControlShutterSpeed.Size = new System.Drawing.Size(200, 20);
			this.floatTrackbarControlShutterSpeed.TabIndex = 7;
			this.floatTrackbarControlShutterSpeed.Value = 1F;
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label15.Location = new System.Drawing.Point(355, 1);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(54, 13);
			this.label15.TabIndex = 8;
			this.label15.Text = "Relative";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label14.Location = new System.Drawing.Point(265, 1);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(69, 13);
			this.label14.TabIndex = 8;
			this.label14.Text = "Normalized";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label13.Location = new System.Drawing.Point(190, 1);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(62, 13);
			this.label13.TabIndex = 8;
			this.label13.Text = "Measured";
			// 
			// labelProbeRelative99
			// 
			this.labelProbeRelative99.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeRelative99.Enabled = false;
			this.labelProbeRelative99.Location = new System.Drawing.Point(347, 133);
			this.labelProbeRelative99.Name = "labelProbeRelative99";
			this.labelProbeRelative99.Size = new System.Drawing.Size(75, 17);
			this.labelProbeRelative99.TabIndex = 6;
			this.labelProbeRelative99.Text = "label1";
			this.labelProbeRelative99.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeNormalized99
			// 
			this.labelProbeNormalized99.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeNormalized99.Enabled = false;
			this.labelProbeNormalized99.Location = new System.Drawing.Point(266, 133);
			this.labelProbeNormalized99.Name = "labelProbeNormalized99";
			this.labelProbeNormalized99.Size = new System.Drawing.Size(75, 17);
			this.labelProbeNormalized99.TabIndex = 6;
			this.labelProbeNormalized99.Text = "label1";
			this.labelProbeNormalized99.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeValue99
			// 
			this.labelProbeValue99.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeValue99.Enabled = false;
			this.labelProbeValue99.Location = new System.Drawing.Point(185, 133);
			this.labelProbeValue99.Name = "labelProbeValue99";
			this.labelProbeValue99.Size = new System.Drawing.Size(75, 17);
			this.labelProbeValue99.TabIndex = 6;
			this.labelProbeValue99.Text = "label1";
			this.labelProbeValue99.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeRelative75
			// 
			this.labelProbeRelative75.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeRelative75.Enabled = false;
			this.labelProbeRelative75.Location = new System.Drawing.Point(347, 110);
			this.labelProbeRelative75.Name = "labelProbeRelative75";
			this.labelProbeRelative75.Size = new System.Drawing.Size(75, 17);
			this.labelProbeRelative75.TabIndex = 6;
			this.labelProbeRelative75.Text = "label1";
			this.labelProbeRelative75.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeNormalized75
			// 
			this.labelProbeNormalized75.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeNormalized75.Enabled = false;
			this.labelProbeNormalized75.Location = new System.Drawing.Point(266, 110);
			this.labelProbeNormalized75.Name = "labelProbeNormalized75";
			this.labelProbeNormalized75.Size = new System.Drawing.Size(75, 17);
			this.labelProbeNormalized75.TabIndex = 6;
			this.labelProbeNormalized75.Text = "label1";
			this.labelProbeNormalized75.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeValue75
			// 
			this.labelProbeValue75.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeValue75.Enabled = false;
			this.labelProbeValue75.Location = new System.Drawing.Point(185, 110);
			this.labelProbeValue75.Name = "labelProbeValue75";
			this.labelProbeValue75.Size = new System.Drawing.Size(75, 17);
			this.labelProbeValue75.TabIndex = 6;
			this.labelProbeValue75.Text = "label1";
			this.labelProbeValue75.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeRelative50
			// 
			this.labelProbeRelative50.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeRelative50.Enabled = false;
			this.labelProbeRelative50.Location = new System.Drawing.Point(347, 87);
			this.labelProbeRelative50.Name = "labelProbeRelative50";
			this.labelProbeRelative50.Size = new System.Drawing.Size(75, 17);
			this.labelProbeRelative50.TabIndex = 6;
			this.labelProbeRelative50.Text = "label1";
			this.labelProbeRelative50.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeNormalized50
			// 
			this.labelProbeNormalized50.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeNormalized50.Enabled = false;
			this.labelProbeNormalized50.Location = new System.Drawing.Point(266, 87);
			this.labelProbeNormalized50.Name = "labelProbeNormalized50";
			this.labelProbeNormalized50.Size = new System.Drawing.Size(75, 17);
			this.labelProbeNormalized50.TabIndex = 6;
			this.labelProbeNormalized50.Text = "label1";
			this.labelProbeNormalized50.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeValue50
			// 
			this.labelProbeValue50.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeValue50.Enabled = false;
			this.labelProbeValue50.Location = new System.Drawing.Point(185, 87);
			this.labelProbeValue50.Name = "labelProbeValue50";
			this.labelProbeValue50.Size = new System.Drawing.Size(75, 17);
			this.labelProbeValue50.TabIndex = 6;
			this.labelProbeValue50.Text = "label1";
			this.labelProbeValue50.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeRelative20
			// 
			this.labelProbeRelative20.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeRelative20.Enabled = false;
			this.labelProbeRelative20.Location = new System.Drawing.Point(347, 64);
			this.labelProbeRelative20.Name = "labelProbeRelative20";
			this.labelProbeRelative20.Size = new System.Drawing.Size(75, 17);
			this.labelProbeRelative20.TabIndex = 6;
			this.labelProbeRelative20.Text = "label1";
			this.labelProbeRelative20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeNormalized20
			// 
			this.labelProbeNormalized20.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeNormalized20.Enabled = false;
			this.labelProbeNormalized20.Location = new System.Drawing.Point(266, 64);
			this.labelProbeNormalized20.Name = "labelProbeNormalized20";
			this.labelProbeNormalized20.Size = new System.Drawing.Size(75, 17);
			this.labelProbeNormalized20.TabIndex = 6;
			this.labelProbeNormalized20.Text = "label1";
			this.labelProbeNormalized20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeValue20
			// 
			this.labelProbeValue20.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeValue20.Enabled = false;
			this.labelProbeValue20.Location = new System.Drawing.Point(185, 64);
			this.labelProbeValue20.Name = "labelProbeValue20";
			this.labelProbeValue20.Size = new System.Drawing.Size(75, 17);
			this.labelProbeValue20.TabIndex = 6;
			this.labelProbeValue20.Text = "label1";
			this.labelProbeValue20.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeRelative10
			// 
			this.labelProbeRelative10.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeRelative10.Enabled = false;
			this.labelProbeRelative10.Location = new System.Drawing.Point(347, 41);
			this.labelProbeRelative10.Name = "labelProbeRelative10";
			this.labelProbeRelative10.Size = new System.Drawing.Size(75, 17);
			this.labelProbeRelative10.TabIndex = 6;
			this.labelProbeRelative10.Text = "label1";
			this.labelProbeRelative10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeNormalized10
			// 
			this.labelProbeNormalized10.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeNormalized10.Enabled = false;
			this.labelProbeNormalized10.Location = new System.Drawing.Point(266, 41);
			this.labelProbeNormalized10.Name = "labelProbeNormalized10";
			this.labelProbeNormalized10.Size = new System.Drawing.Size(75, 17);
			this.labelProbeNormalized10.TabIndex = 6;
			this.labelProbeNormalized10.Text = "label1";
			this.labelProbeNormalized10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeValue10
			// 
			this.labelProbeValue10.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeValue10.Enabled = false;
			this.labelProbeValue10.Location = new System.Drawing.Point(185, 41);
			this.labelProbeValue10.Name = "labelProbeValue10";
			this.labelProbeValue10.Size = new System.Drawing.Size(75, 17);
			this.labelProbeValue10.TabIndex = 6;
			this.labelProbeValue10.Text = "label1";
			this.labelProbeValue10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeRelative02
			// 
			this.labelProbeRelative02.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeRelative02.Enabled = false;
			this.labelProbeRelative02.Location = new System.Drawing.Point(347, 18);
			this.labelProbeRelative02.Name = "labelProbeRelative02";
			this.labelProbeRelative02.Size = new System.Drawing.Size(75, 17);
			this.labelProbeRelative02.TabIndex = 6;
			this.labelProbeRelative02.Text = "label1";
			this.labelProbeRelative02.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeNormalized02
			// 
			this.labelProbeNormalized02.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeNormalized02.Enabled = false;
			this.labelProbeNormalized02.Location = new System.Drawing.Point(266, 18);
			this.labelProbeNormalized02.Name = "labelProbeNormalized02";
			this.labelProbeNormalized02.Size = new System.Drawing.Size(75, 17);
			this.labelProbeNormalized02.TabIndex = 6;
			this.labelProbeNormalized02.Text = "label1";
			this.labelProbeNormalized02.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProbeValue02
			// 
			this.labelProbeValue02.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelProbeValue02.Enabled = false;
			this.labelProbeValue02.Location = new System.Drawing.Point(185, 18);
			this.labelProbeValue02.Name = "labelProbeValue02";
			this.labelProbeValue02.Size = new System.Drawing.Size(75, 17);
			this.labelProbeValue02.TabIndex = 6;
			this.labelProbeValue02.Text = "label1";
			this.labelProbeValue02.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// buttonCalibrate99
			// 
			this.buttonCalibrate99.Enabled = false;
			this.buttonCalibrate99.Location = new System.Drawing.Point(93, 129);
			this.buttonCalibrate99.Name = "buttonCalibrate99";
			this.buttonCalibrate99.Size = new System.Drawing.Size(75, 23);
			this.buttonCalibrate99.TabIndex = 5;
			this.buttonCalibrate99.Text = "Calibrate";
			this.buttonCalibrate99.UseVisualStyleBackColor = true;
			this.buttonCalibrate99.Click += new System.EventHandler(this.buttonCalibrate99_Click);
			// 
			// buttonCalibrate75
			// 
			this.buttonCalibrate75.Enabled = false;
			this.buttonCalibrate75.Location = new System.Drawing.Point(93, 106);
			this.buttonCalibrate75.Name = "buttonCalibrate75";
			this.buttonCalibrate75.Size = new System.Drawing.Size(75, 23);
			this.buttonCalibrate75.TabIndex = 5;
			this.buttonCalibrate75.Text = "Calibrate";
			this.buttonCalibrate75.UseVisualStyleBackColor = true;
			this.buttonCalibrate75.Click += new System.EventHandler(this.buttonCalibrate75_Click);
			// 
			// buttonCalibrate50
			// 
			this.buttonCalibrate50.Enabled = false;
			this.buttonCalibrate50.Location = new System.Drawing.Point(93, 83);
			this.buttonCalibrate50.Name = "buttonCalibrate50";
			this.buttonCalibrate50.Size = new System.Drawing.Size(75, 23);
			this.buttonCalibrate50.TabIndex = 5;
			this.buttonCalibrate50.Text = "Calibrate";
			this.buttonCalibrate50.UseVisualStyleBackColor = true;
			this.buttonCalibrate50.Click += new System.EventHandler(this.buttonCalibrate50_Click);
			// 
			// buttonCalibrate20
			// 
			this.buttonCalibrate20.Enabled = false;
			this.buttonCalibrate20.Location = new System.Drawing.Point(93, 60);
			this.buttonCalibrate20.Name = "buttonCalibrate20";
			this.buttonCalibrate20.Size = new System.Drawing.Size(75, 23);
			this.buttonCalibrate20.TabIndex = 5;
			this.buttonCalibrate20.Text = "Calibrate";
			this.buttonCalibrate20.UseVisualStyleBackColor = true;
			this.buttonCalibrate20.Click += new System.EventHandler(this.buttonCalibrate20_Click);
			// 
			// buttonCalibrate10
			// 
			this.buttonCalibrate10.Enabled = false;
			this.buttonCalibrate10.Location = new System.Drawing.Point(93, 37);
			this.buttonCalibrate10.Name = "buttonCalibrate10";
			this.buttonCalibrate10.Size = new System.Drawing.Size(75, 23);
			this.buttonCalibrate10.TabIndex = 5;
			this.buttonCalibrate10.Text = "Calibrate";
			this.buttonCalibrate10.UseVisualStyleBackColor = true;
			this.buttonCalibrate10.Click += new System.EventHandler(this.buttonCalibrate10_Click);
			// 
			// buttonCalibrate02
			// 
			this.buttonCalibrate02.Enabled = false;
			this.buttonCalibrate02.Location = new System.Drawing.Point(93, 14);
			this.buttonCalibrate02.Name = "buttonCalibrate02";
			this.buttonCalibrate02.Size = new System.Drawing.Size(75, 23);
			this.buttonCalibrate02.TabIndex = 5;
			this.buttonCalibrate02.Text = "Calibrate";
			this.buttonCalibrate02.UseVisualStyleBackColor = true;
			this.buttonCalibrate02.Click += new System.EventHandler(this.buttonCalibrate02_Click);
			// 
			// checkBoxCalibrate99
			// 
			this.checkBoxCalibrate99.AutoSize = true;
			this.checkBoxCalibrate99.Location = new System.Drawing.Point(3, 133);
			this.checkBoxCalibrate99.Name = "checkBoxCalibrate99";
			this.checkBoxCalibrate99.Size = new System.Drawing.Size(84, 17);
			this.checkBoxCalibrate99.TabIndex = 4;
			this.checkBoxCalibrate99.Text = "SRS-99-010";
			this.checkBoxCalibrate99.UseVisualStyleBackColor = true;
			this.checkBoxCalibrate99.CheckedChanged += new System.EventHandler(this.checkBoxCalibrate99_CheckedChanged);
			// 
			// checkBoxCalibrate75
			// 
			this.checkBoxCalibrate75.AutoSize = true;
			this.checkBoxCalibrate75.Location = new System.Drawing.Point(3, 110);
			this.checkBoxCalibrate75.Name = "checkBoxCalibrate75";
			this.checkBoxCalibrate75.Size = new System.Drawing.Size(84, 17);
			this.checkBoxCalibrate75.TabIndex = 4;
			this.checkBoxCalibrate75.Text = "SRS-75-010";
			this.checkBoxCalibrate75.UseVisualStyleBackColor = true;
			this.checkBoxCalibrate75.CheckedChanged += new System.EventHandler(this.checkBoxCalibrate75_CheckedChanged);
			// 
			// checkBoxCalibrate50
			// 
			this.checkBoxCalibrate50.AutoSize = true;
			this.checkBoxCalibrate50.Location = new System.Drawing.Point(3, 87);
			this.checkBoxCalibrate50.Name = "checkBoxCalibrate50";
			this.checkBoxCalibrate50.Size = new System.Drawing.Size(84, 17);
			this.checkBoxCalibrate50.TabIndex = 4;
			this.checkBoxCalibrate50.Text = "SRS-50-010";
			this.checkBoxCalibrate50.UseVisualStyleBackColor = true;
			this.checkBoxCalibrate50.CheckedChanged += new System.EventHandler(this.checkBoxCalibrate50_CheckedChanged);
			// 
			// checkBoxCalibrate20
			// 
			this.checkBoxCalibrate20.AutoSize = true;
			this.checkBoxCalibrate20.Location = new System.Drawing.Point(3, 64);
			this.checkBoxCalibrate20.Name = "checkBoxCalibrate20";
			this.checkBoxCalibrate20.Size = new System.Drawing.Size(84, 17);
			this.checkBoxCalibrate20.TabIndex = 4;
			this.checkBoxCalibrate20.Text = "SRS-20-010";
			this.checkBoxCalibrate20.UseVisualStyleBackColor = true;
			this.checkBoxCalibrate20.CheckedChanged += new System.EventHandler(this.checkBoxCalibrate20_CheckedChanged);
			// 
			// checkBoxCalibrate10
			// 
			this.checkBoxCalibrate10.AutoSize = true;
			this.checkBoxCalibrate10.Location = new System.Drawing.Point(3, 41);
			this.checkBoxCalibrate10.Name = "checkBoxCalibrate10";
			this.checkBoxCalibrate10.Size = new System.Drawing.Size(84, 17);
			this.checkBoxCalibrate10.TabIndex = 4;
			this.checkBoxCalibrate10.Text = "SRS-10-010";
			this.checkBoxCalibrate10.UseVisualStyleBackColor = true;
			this.checkBoxCalibrate10.CheckedChanged += new System.EventHandler(this.checkBoxCalibrate10_CheckedChanged);
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(211, 723);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(59, 13);
			this.label5.TabIndex = 9;
			this.label5.Text = "Luminance";
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.tabPageCreation);
			this.tabControl.Controls.Add(this.tabPage2);
			this.tabControl.Location = new System.Drawing.Point(3, 152);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(491, 592);
			this.tabControl.TabIndex = 10;
			// 
			// tabPageCreation
			// 
			this.tabPageCreation.Location = new System.Drawing.Point(4, 22);
			this.tabPageCreation.Name = "tabPageCreation";
			this.tabPageCreation.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageCreation.Size = new System.Drawing.Size(483, 566);
			this.tabPageCreation.TabIndex = 0;
			this.tabPageCreation.Text = "Texure Creation";
			this.tabPageCreation.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.labelCalbrationImageName);
			this.tabPage2.Controls.Add(this.label6);
			this.tabPage2.Controls.Add(this.panelProbeLuminances);
			this.tabPage2.Controls.Add(this.buttonSaveCalibration);
			this.tabPage2.Controls.Add(this.buttonSetupDatabaseFolder);
			this.tabPage2.Controls.Add(this.buttonReCalibrate);
			this.tabPage2.Controls.Add(this.buttonLoadCalibration);
			this.tabPage2.Controls.Add(this.checkBoxGraphLagrange);
			this.tabPage2.Controls.Add(this.graphPanel);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(483, 566);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Camera Calibration";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// labelCalbrationImageName
			// 
			this.labelCalbrationImageName.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labelCalbrationImageName.Location = new System.Drawing.Point(191, 12);
			this.labelCalbrationImageName.Name = "labelCalbrationImageName";
			this.labelCalbrationImageName.Size = new System.Drawing.Size(246, 16);
			this.labelCalbrationImageName.TabIndex = 14;
			this.labelCalbrationImageName.Text = "<NO IMAGE>";
			this.labelCalbrationImageName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 14);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(185, 13);
			this.label6.TabIndex = 14;
			this.label6.Text = "Reference Image used for Calibration:";
			// 
			// panelProbeLuminances
			// 
			this.panelProbeLuminances.Controls.Add(this.checkBoxCalibrate02);
			this.panelProbeLuminances.Controls.Add(this.checkBoxCalibrate10);
			this.panelProbeLuminances.Controls.Add(this.checkBoxCalibrate20);
			this.panelProbeLuminances.Controls.Add(this.checkBoxCalibrate50);
			this.panelProbeLuminances.Controls.Add(this.label15);
			this.panelProbeLuminances.Controls.Add(this.checkBoxCalibrate75);
			this.panelProbeLuminances.Controls.Add(this.label14);
			this.panelProbeLuminances.Controls.Add(this.checkBoxCalibrate99);
			this.panelProbeLuminances.Controls.Add(this.label13);
			this.panelProbeLuminances.Controls.Add(this.buttonCalibrate02);
			this.panelProbeLuminances.Controls.Add(this.labelProbeRelative99);
			this.panelProbeLuminances.Controls.Add(this.buttonCalibrate10);
			this.panelProbeLuminances.Controls.Add(this.labelProbeNormalized99);
			this.panelProbeLuminances.Controls.Add(this.buttonCalibrate20);
			this.panelProbeLuminances.Controls.Add(this.labelProbeValue99);
			this.panelProbeLuminances.Controls.Add(this.buttonCalibrate50);
			this.panelProbeLuminances.Controls.Add(this.labelProbeRelative75);
			this.panelProbeLuminances.Controls.Add(this.buttonCalibrate75);
			this.panelProbeLuminances.Controls.Add(this.labelProbeNormalized75);
			this.panelProbeLuminances.Controls.Add(this.buttonCalibrate99);
			this.panelProbeLuminances.Controls.Add(this.labelProbeValue75);
			this.panelProbeLuminances.Controls.Add(this.labelProbeValue02);
			this.panelProbeLuminances.Controls.Add(this.labelProbeRelative50);
			this.panelProbeLuminances.Controls.Add(this.labelProbeNormalized02);
			this.panelProbeLuminances.Controls.Add(this.labelProbeNormalized50);
			this.panelProbeLuminances.Controls.Add(this.labelProbeRelative02);
			this.panelProbeLuminances.Controls.Add(this.labelProbeValue50);
			this.panelProbeLuminances.Controls.Add(this.labelProbeValue10);
			this.panelProbeLuminances.Controls.Add(this.labelProbeRelative20);
			this.panelProbeLuminances.Controls.Add(this.labelProbeNormalized10);
			this.panelProbeLuminances.Controls.Add(this.labelProbeNormalized20);
			this.panelProbeLuminances.Controls.Add(this.labelProbeRelative10);
			this.panelProbeLuminances.Controls.Add(this.labelProbeValue20);
			this.panelProbeLuminances.Location = new System.Drawing.Point(8, 41);
			this.panelProbeLuminances.Name = "panelProbeLuminances";
			this.panelProbeLuminances.Size = new System.Drawing.Size(429, 160);
			this.panelProbeLuminances.TabIndex = 13;
			// 
			// buttonSaveCalibration
			// 
			this.buttonSaveCalibration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSaveCalibration.Location = new System.Drawing.Point(402, 537);
			this.buttonSaveCalibration.Name = "buttonSaveCalibration";
			this.buttonSaveCalibration.Size = new System.Drawing.Size(75, 23);
			this.buttonSaveCalibration.TabIndex = 12;
			this.buttonSaveCalibration.Text = "Save";
			this.buttonSaveCalibration.UseVisualStyleBackColor = true;
			this.buttonSaveCalibration.Click += new System.EventHandler(this.buttonSaveCalibration_Click);
			// 
			// buttonLoadCalibration
			// 
			this.buttonLoadCalibration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLoadCalibration.Location = new System.Drawing.Point(321, 537);
			this.buttonLoadCalibration.Name = "buttonLoadCalibration";
			this.buttonLoadCalibration.Size = new System.Drawing.Size(75, 23);
			this.buttonLoadCalibration.TabIndex = 12;
			this.buttonLoadCalibration.Text = "Load";
			this.buttonLoadCalibration.UseVisualStyleBackColor = true;
			this.buttonLoadCalibration.Click += new System.EventHandler(this.buttonLoadCalibration_Click);
			// 
			// checkBoxGraphLagrange
			// 
			this.checkBoxGraphLagrange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxGraphLagrange.AutoSize = true;
			this.checkBoxGraphLagrange.Location = new System.Drawing.Point(5, 516);
			this.checkBoxGraphLagrange.Name = "checkBoxGraphLagrange";
			this.checkBoxGraphLagrange.Size = new System.Drawing.Size(150, 17);
			this.checkBoxGraphLagrange.TabIndex = 3;
			this.checkBoxGraphLagrange.Text = "Use Lagrange polynomials";
			this.checkBoxGraphLagrange.UseVisualStyleBackColor = true;
			this.checkBoxGraphLagrange.CheckedChanged += new System.EventHandler(this.checkBoxGraphLagrange_CheckedChanged);
			// 
			// graphPanel
			// 
			this.graphPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.graphPanel.BackColor = System.Drawing.Color.Ivory;
			this.graphPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.graphPanel.Calibration = null;
			this.graphPanel.Location = new System.Drawing.Point(6, 207);
			this.graphPanel.Name = "graphPanel";
			this.graphPanel.Size = new System.Drawing.Size(471, 303);
			this.graphPanel.TabIndex = 11;
			this.graphPanel.UseLagrange = false;
			// 
			// splitContainerMain
			// 
			this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
			this.splitContainerMain.Name = "splitContainerMain";
			// 
			// splitContainerMain.Panel1
			// 
			this.splitContainerMain.Panel1.Controls.Add(this.outputPanel);
			this.splitContainerMain.Panel1.Controls.Add(this.checkBoxsRGB);
			this.splitContainerMain.Panel1.Controls.Add(this.labelLuminance);
			this.splitContainerMain.Panel1.Controls.Add(this.label5);
			this.splitContainerMain.Panel1.Controls.Add(this.checkBoxLuminance);
			// 
			// splitContainerMain.Panel2
			// 
			this.splitContainerMain.Panel2.Controls.Add(this.tabControl);
			this.splitContainerMain.Panel2.Controls.Add(this.groupBoxCameraShotInfos);
			this.splitContainerMain.Panel2.Controls.Add(this.buttonLoadImage);
			this.splitContainerMain.Panel2MinSize = 490;
			this.splitContainerMain.Size = new System.Drawing.Size(1499, 747);
			this.splitContainerMain.SplitterDistance = 998;
			this.splitContainerMain.TabIndex = 11;
			// 
			// outputPanel
			// 
			this.outputPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.outputPanel.Image = null;
			this.outputPanel.Location = new System.Drawing.Point(3, 0);
			this.outputPanel.Name = "outputPanel";
			this.outputPanel.Size = new System.Drawing.Size(996, 716);
			this.outputPanel.TabIndex = 0;
			this.outputPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.outputPanel_MouseMove);
			// 
			// openFileDialogCalibration
			// 
			this.openFileDialogCalibration.DefaultExt = "*.xml";
			this.openFileDialogCalibration.Filter = "Calibration Files (*.xml)|*.xml|All Files (*.*)|*.*";
			this.openFileDialogCalibration.Title = "Choose the calibration file to load";
			// 
			// saveFileDialogCalibration
			// 
			this.saveFileDialogCalibration.DefaultExt = "*.xml";
			this.saveFileDialogCalibration.Filter = "Calibration Files (*.xml)|*.xml|All Files (*.*)|*.*";
			this.saveFileDialogCalibration.Title = "Choose the calbration file to save";
			// 
			// buttonSetupDatabaseFolder
			// 
			this.buttonSetupDatabaseFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonSetupDatabaseFolder.Location = new System.Drawing.Point(6, 537);
			this.buttonSetupDatabaseFolder.Name = "buttonSetupDatabaseFolder";
			this.buttonSetupDatabaseFolder.Size = new System.Drawing.Size(185, 23);
			this.buttonSetupDatabaseFolder.TabIndex = 12;
			this.buttonSetupDatabaseFolder.Text = "Set Calibration Database Location";
			this.buttonSetupDatabaseFolder.UseVisualStyleBackColor = true;
			this.buttonSetupDatabaseFolder.Click += new System.EventHandler(this.buttonSetupDatabaseFolder_Click);
			// 
			// buttonReCalibrate
			// 
			this.buttonReCalibrate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonReCalibrate.Location = new System.Drawing.Point(209, 537);
			this.buttonReCalibrate.Name = "buttonReCalibrate";
			this.buttonReCalibrate.Size = new System.Drawing.Size(75, 23);
			this.buttonReCalibrate.TabIndex = 12;
			this.buttonReCalibrate.Text = "Re-Calibrate";
			this.buttonReCalibrate.UseVisualStyleBackColor = true;
			this.buttonReCalibrate.Click += new System.EventHandler(this.buttonReCalibrate_Click);
			// 
			// folderBrowserDialogDatabaseLocation
			// 
			this.folderBrowserDialogDatabaseLocation.Description = "Select the base folder containing the calibration files to use as a database...";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1499, 747);
			this.Controls.Add(this.splitContainerMain);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Standardized Diffuse Albedo Maps Creator";
			this.groupBoxCameraShotInfos.ResumeLayout(false);
			this.groupBoxCameraShotInfos.PerformLayout();
			this.tabControl.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.panelProbeLuminances.ResumeLayout(false);
			this.panelProbeLuminances.PerformLayout();
			this.splitContainerMain.Panel1.ResumeLayout(false);
			this.splitContainerMain.Panel1.PerformLayout();
			this.splitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
			this.splitContainerMain.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private OutputPanel outputPanel;
		private System.Windows.Forms.OpenFileDialog openFileDialogSourceImage;
		private System.Windows.Forms.Button buttonLoadImage;
		private System.Windows.Forms.Label labelLuminance;
		private System.Windows.Forms.CheckBox checkBoxsRGB;
		private System.Windows.Forms.CheckBox checkBoxLuminance;
		private System.Windows.Forms.CheckBox checkBoxCalibrate02;
		private System.Windows.Forms.Button buttonCalibrate02;
		private System.Windows.Forms.Label labelProbeValue02;
		private System.Windows.Forms.Label labelProbeValue99;
		private System.Windows.Forms.Label labelProbeValue75;
		private System.Windows.Forms.Label labelProbeValue50;
		private System.Windows.Forms.Label labelProbeValue20;
		private System.Windows.Forms.Label labelProbeValue10;
		private System.Windows.Forms.Button buttonCalibrate99;
		private System.Windows.Forms.Button buttonCalibrate75;
		private System.Windows.Forms.Button buttonCalibrate50;
		private System.Windows.Forms.Button buttonCalibrate20;
		private System.Windows.Forms.Button buttonCalibrate10;
		private System.Windows.Forms.CheckBox checkBoxCalibrate99;
		private System.Windows.Forms.CheckBox checkBoxCalibrate75;
		private System.Windows.Forms.CheckBox checkBoxCalibrate50;
		private System.Windows.Forms.CheckBox checkBoxCalibrate20;
		private System.Windows.Forms.CheckBox checkBoxCalibrate10;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlShutterSpeed;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlISOSpeed;
		private System.Windows.Forms.Label labelProbeRelative99;
		private System.Windows.Forms.Label labelProbeNormalized99;
		private System.Windows.Forms.Label labelProbeRelative75;
		private System.Windows.Forms.Label labelProbeNormalized75;
		private System.Windows.Forms.Label labelProbeRelative50;
		private System.Windows.Forms.Label labelProbeNormalized50;
		private System.Windows.Forms.Label labelProbeRelative20;
		private System.Windows.Forms.Label labelProbeNormalized20;
		private System.Windows.Forms.Label labelProbeRelative10;
		private System.Windows.Forms.Label labelProbeNormalized10;
		private System.Windows.Forms.Label labelProbeRelative02;
		private System.Windows.Forms.Label labelProbeNormalized02;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.GroupBox groupBoxCameraShotInfos;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlFocalLength;
		private Nuaj.Cirrus.Utility.FloatTrackbarControl floatTrackbarControlAperture;
		private System.Windows.Forms.Label label5;
		private GraphPanel graphPanel;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabPageCreation;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Button buttonLoadCalibration;
		private System.Windows.Forms.Button buttonSaveCalibration;
		private System.Windows.Forms.CheckBox checkBoxGraphLagrange;
		private System.Windows.Forms.SplitContainer splitContainerMain;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Panel panelProbeLuminances;
		private System.Windows.Forms.Label labelCalbrationImageName;
		private System.Windows.Forms.OpenFileDialog openFileDialogCalibration;
		private System.Windows.Forms.SaveFileDialog saveFileDialogCalibration;
		private System.Windows.Forms.Button buttonSetupDatabaseFolder;
		private System.Windows.Forms.Button buttonReCalibrate;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogDatabaseLocation;
	}
}

