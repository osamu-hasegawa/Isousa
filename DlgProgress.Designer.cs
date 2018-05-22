namespace uSCOPE
{
	partial class DlgProgress
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
			if (disposing && (components != null)) {
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
			this.Label1 = new System.Windows.Forms.Label();
			this.Cancel_Button = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// Label1
			// 
			this.Label1.Location = new System.Drawing.Point(23, 9);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(136, 40);
			this.Label1.TabIndex = 4;
			this.Label1.Text = "000000000000000000000ZZZZZZZZZZZZZZZZZZZZZ333333333333333333333";
			this.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = new System.Drawing.Point(60, 52);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = new System.Drawing.Size(67, 21);
			this.Cancel_Button.TabIndex = 3;
			this.Cancel_Button.Text = "キャンセル";
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// DlgProgress
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(189, 82);
			this.ControlBox = false;
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.Cancel_Button);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DlgProgress";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "xxx";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DlgProgress_FormClosed);
			this.VisibleChanged += new System.EventHandler(this.DlgProgress_VisibleChanged);
			this.ResumeLayout(false);

		}

		#endregion

		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.Button Cancel_Button;
	}
}