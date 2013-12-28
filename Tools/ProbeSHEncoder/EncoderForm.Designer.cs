using ProbeSHEncoder;

namespace ProbeSHEncoder
{
	partial class EncoderForm
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
			WMath.Vector vector1 = new WMath.Vector();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.convertShaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialogShader = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogShader = new System.Windows.Forms.SaveFileDialog();
			this.buttonCompute = new System.Windows.Forms.Button();
			this.outputPanel1 = new ProbeSHEncoder.OutputPanel( this.components );
			this.menuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem} );
			this.menuStrip.Location = new System.Drawing.Point( 0, 0 );
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size( 1027, 24 );
			this.menuStrip.TabIndex = 1;
			this.menuStrip.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size( 37, 20 );
			this.fileToolStripMenuItem.Text = "File";
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange( new System.Windows.Forms.ToolStripItem[] {
            this.convertShaderToolStripMenuItem} );
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size( 48, 20 );
			this.toolsToolStripMenuItem.Text = "Tools";
			// 
			// convertShaderToolStripMenuItem
			// 
			this.convertShaderToolStripMenuItem.Name = "convertShaderToolStripMenuItem";
			this.convertShaderToolStripMenuItem.Size = new System.Drawing.Size( 164, 22 );
			this.convertShaderToolStripMenuItem.Text = "Convert Shader...";
			// 
			// openFileDialogShader
			// 
			this.openFileDialogShader.DefaultExt = "hlsl";
			this.openFileDialogShader.Filter = "HLSL Shader File (*.hlsl)|*.hlsl|All Files (*.*)|*.*";
			this.openFileDialogShader.Title = "Choose a shader file to convert...";
			// 
			// saveFileDialogShader
			// 
			this.saveFileDialogShader.DefaultExt = "cs";
			this.saveFileDialogShader.Filter = "C# Source File (*.cs)|*.cs|All Files (*.*)|*.*";
			this.saveFileDialogShader.Title = "Choose a target C# file to save the converted shader to...";
			// 
			// buttonCompute
			// 
			this.buttonCompute.Location = new System.Drawing.Point( 722, 57 );
			this.buttonCompute.Name = "buttonCompute";
			this.buttonCompute.Size = new System.Drawing.Size( 87, 66 );
			this.buttonCompute.TabIndex = 2;
			this.buttonCompute.Text = "Compute k-Means";
			this.buttonCompute.UseVisualStyleBackColor = true;
			this.buttonCompute.Click += new System.EventHandler( this.buttonCompute_Click );
			// 
			// outputPanel1
			// 
			vector1.X = 0F;
			vector1.Y = 0F;
			vector1.Z = 1F;
			this.outputPanel1.At = vector1;
			this.outputPanel1.Location = new System.Drawing.Point( 12, 27 );
			this.outputPanel1.Name = "outputPanel1";
			this.outputPanel1.Size = new System.Drawing.Size( 677, 546 );
			this.outputPanel1.TabIndex = 3;
			// 
			// EncoderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 13F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size( 1027, 753 );
			this.Controls.Add( this.outputPanel1 );
			this.Controls.Add( this.buttonCompute );
			this.Controls.Add( this.menuStrip );
			this.MainMenuStrip = this.menuStrip;
			this.Name = "EncoderForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Probe SH Encoder";
			this.menuStrip.ResumeLayout( false );
			this.menuStrip.PerformLayout();
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem convertShaderToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openFileDialogShader;
		private System.Windows.Forms.SaveFileDialog saveFileDialogShader;
		private System.Windows.Forms.Button buttonCompute;
		private OutputPanel outputPanel1;
	}
}

