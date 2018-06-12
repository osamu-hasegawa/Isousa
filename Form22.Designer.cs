namespace uSCOPE
{
	partial class Form22
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
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.numericUpDown11 = new System.Windows.Forms.NumericUpDown();
			this.label11 = new System.Windows.Forms.Label();
			this.numericUpDown10 = new System.Windows.Forms.NumericUpDown();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown11)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown10)).BeginInit();
			this.SuspendLayout();
			// 
			// button2
			// 
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.Location = new System.Drawing.Point(189, 82);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 26);
			this.button2.TabIndex = 1;
			this.button2.Tag = "1";
			this.button2.Text = "キャンセル";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Location = new System.Drawing.Point(94, 82);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 26);
			this.button1.TabIndex = 0;
			this.button1.Tag = "0";
			this.button1.Text = "実行";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.numericUpDown11);
			this.groupBox3.Controls.Add(this.label11);
			this.groupBox3.Controls.Add(this.numericUpDown10);
			this.groupBox3.Location = new System.Drawing.Point(19, 12);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(323, 59);
			this.groupBox3.TabIndex = 0;
			this.groupBox3.TabStop = false;
			// 
			// numericUpDown11
			// 
			this.numericUpDown11.BackColor = System.Drawing.SystemColors.Window;
			this.numericUpDown11.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDown11.Location = new System.Drawing.Point(229, 22);
			this.numericUpDown11.Maximum = new decimal(new int[] {
            8388607,
            0,
            0,
            0});
			this.numericUpDown11.Minimum = new decimal(new int[] {
            8388607,
            0,
            0,
            -2147483648});
			this.numericUpDown11.Name = "numericUpDown11";
			this.numericUpDown11.Size = new System.Drawing.Size(51, 19);
			this.numericUpDown11.TabIndex = 1;
			this.numericUpDown11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.BackColor = System.Drawing.SystemColors.Control;
			this.label11.Location = new System.Drawing.Point(11, 26);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(128, 12);
			this.label11.TabIndex = 57;
			this.label11.Text = "開始ステージ位置x,y(pls)";
			// 
			// numericUpDown10
			// 
			this.numericUpDown10.BackColor = System.Drawing.SystemColors.Window;
			this.numericUpDown10.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDown10.Location = new System.Drawing.Point(166, 22);
			this.numericUpDown10.Maximum = new decimal(new int[] {
            8388607,
            0,
            0,
            0});
			this.numericUpDown10.Minimum = new decimal(new int[] {
            8388607,
            0,
            0,
            -2147483648});
			this.numericUpDown10.Name = "numericUpDown10";
			this.numericUpDown10.Size = new System.Drawing.Size(51, 19);
			this.numericUpDown10.TabIndex = 0;
			this.numericUpDown10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Form22
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.button2;
			this.ClientSize = new System.Drawing.Size(354, 114);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form22";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "自動撮影";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form22_FormClosing);
			this.Load += new System.EventHandler(this.Form22_Load);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown11)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown10)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.NumericUpDown numericUpDown11;
		private System.Windows.Forms.NumericUpDown numericUpDown10;
		private System.Windows.Forms.Label label11;
	}
}