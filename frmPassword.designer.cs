namespace uSCOPE
{
	partial class frmPassword
	{
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナで生成されたコード

		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.button_ok = new System.Windows.Forms.Button();
			this.button_cancel = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// button_ok
			// 
			this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_ok.Location = new System.Drawing.Point(33, 62);
			this.button_ok.Name = "button_ok";
			this.button_ok.Size = new System.Drawing.Size(75, 23);
			this.button_ok.TabIndex = 1;
			this.button_ok.Text = "OK";
			this.button_ok.UseVisualStyleBackColor = true;
			// 
			// button_cancel
			// 
			this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_cancel.Location = new System.Drawing.Point(122, 62);
			this.button_cancel.Name = "button_cancel";
			this.button_cancel.Size = new System.Drawing.Size(75, 23);
			this.button_cancel.TabIndex = 2;
			this.button_cancel.Text = "Cancel";
			this.button_cancel.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(23, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 12);
			this.label1.TabIndex = 2;
			this.label1.Text = "Password:";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(25, 32);
			this.textBox1.Name = "textBox1";
			this.textBox1.PasswordChar = '●';
			this.textBox1.Size = new System.Drawing.Size(180, 19);
			this.textBox1.TabIndex = 0;
			// 
			// frmPassword
			// 
			this.AcceptButton = this.button_ok;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.button_cancel;
			this.ClientSize = new System.Drawing.Size(232, 96);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button_cancel);
			this.Controls.Add(this.button_ok);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmPassword";
			this.Padding = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "パスワードを入力してください";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmPassword_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button_ok;
		private System.Windows.Forms.Button button_cancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox1;


	}
}
