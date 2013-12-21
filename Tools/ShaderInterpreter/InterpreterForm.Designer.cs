using ShaderInterpreter;

namespace ShaderInterpreter
{
	partial class InterpreterForm
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
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.convertShaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialogShader = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialogShader = new System.Windows.Forms.SaveFileDialog();
			this.button1 = new System.Windows.Forms.Button();
			this.outputPanel1 = new ShaderInterpreter.OutputPanel(this.components);
			this.menuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(1027, 24);
			this.menuStrip.TabIndex = 1;
			this.menuStrip.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.convertShaderToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
			this.toolsToolStripMenuItem.Text = "Tools";
			// 
			// convertShaderToolStripMenuItem
			// 
			this.convertShaderToolStripMenuItem.Name = "convertShaderToolStripMenuItem";
			this.convertShaderToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
			this.convertShaderToolStripMenuItem.Text = "Convert Shader...";
			this.convertShaderToolStripMenuItem.Click += new System.EventHandler(this.convertShaderToolStripMenuItem_Click);
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
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(576, 579);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "button1";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// outputPanel1
			// 
			this.outputPanel1.Location = new System.Drawing.Point(12, 27);
			this.outputPanel1.Name = "outputPanel1";
			this.outputPanel1.Size = new System.Drawing.Size(677, 546);
			this.outputPanel1.TabIndex = 3;
			// 
			// InterpreterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1027, 753);
			this.Controls.Add(this.outputPanel1);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.menuStrip);
			this.MainMenuStrip = this.menuStrip;
			this.Name = "InterpreterForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Shader Interpreter";
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem convertShaderToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openFileDialogShader;
		private System.Windows.Forms.SaveFileDialog saveFileDialogShader;
		private System.Windows.Forms.Button button1;
		private OutputPanel outputPanel1;
	}
}

