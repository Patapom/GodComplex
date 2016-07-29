namespace MaterialsOptimizer
{
	partial class TexturesToRescaleForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.textBoxFileNames = new System.Windows.Forms.TextBox();
			this.buttonAbort = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonProcessTextures = new System.Windows.Forms.Button();
			this.buttonCopyToClipboard = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// textBoxFileNames
			// 
			this.textBoxFileNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxFileNames.Location = new System.Drawing.Point(15, 34);
			this.textBoxFileNames.Multiline = true;
			this.textBoxFileNames.Name = "textBoxFileNames";
			this.textBoxFileNames.ReadOnly = true;
			this.textBoxFileNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxFileNames.Size = new System.Drawing.Size(554, 629);
			this.textBoxFileNames.TabIndex = 0;
			this.textBoxFileNames.WordWrap = false;
			// 
			// buttonAbort
			// 
			this.buttonAbort.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonAbort.BackColor = System.Drawing.Color.IndianRed;
			this.buttonAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAbort.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonAbort.Location = new System.Drawing.Point(293, 697);
			this.buttonAbort.Name = "buttonAbort";
			this.buttonAbort.Size = new System.Drawing.Size(117, 50);
			this.buttonAbort.TabIndex = 0;
			this.buttonAbort.Text = "Abort";
			this.buttonAbort.UseVisualStyleBackColor = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(276, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "List of gloss textures not the same size of diffuse textures:";
			// 
			// buttonProcessTextures
			// 
			this.buttonProcessTextures.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.buttonProcessTextures.BackColor = System.Drawing.Color.MediumSeaGreen;
			this.buttonProcessTextures.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonProcessTextures.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonProcessTextures.Location = new System.Drawing.Point(170, 697);
			this.buttonProcessTextures.Name = "buttonProcessTextures";
			this.buttonProcessTextures.Size = new System.Drawing.Size(117, 50);
			this.buttonProcessTextures.TabIndex = 1;
			this.buttonProcessTextures.Text = "Process Valid Textures Anyway";
			this.buttonProcessTextures.UseVisualStyleBackColor = false;
			// 
			// buttonCopyToClipboard
			// 
			this.buttonCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCopyToClipboard.Location = new System.Drawing.Point(441, 669);
			this.buttonCopyToClipboard.Name = "buttonCopyToClipboard";
			this.buttonCopyToClipboard.Size = new System.Drawing.Size(128, 23);
			this.buttonCopyToClipboard.TabIndex = 3;
			this.buttonCopyToClipboard.Text = "Copy to Clipboard";
			this.buttonCopyToClipboard.UseVisualStyleBackColor = true;
			this.buttonCopyToClipboard.Click += new System.EventHandler(this.buttonCopyToClipboard_Click);
			// 
			// TexturesToRescaleForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonAbort;
			this.ClientSize = new System.Drawing.Size(581, 759);
			this.Controls.Add(this.buttonCopyToClipboard);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.buttonProcessTextures);
			this.Controls.Add(this.buttonAbort);
			this.Controls.Add(this.textBoxFileNames);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = new System.Drawing.Size(309, 515);
			this.Name = "TexturesToRescaleForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Textures To Rescale";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxFileNames;
		private System.Windows.Forms.Button buttonAbort;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonProcessTextures;
		private System.Windows.Forms.Button buttonCopyToClipboard;
	}
}