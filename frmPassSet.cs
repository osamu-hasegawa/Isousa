using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace uSCOPE
{
	partial class frmPassSet : Form
	{
		public string m_pass_str;
		public frmPassSet()
		{
			InitializeComponent();
		}

		private void frmPassSet_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != System.Windows.Forms.DialogResult.OK) {
				return;
			}
			if (this.textBox1.Text != this.textBox2.Text) {
				G.mlog("確認用パスワードが一致しません.");
				e.Cancel = true;
				return;
			}
			if (string.IsNullOrEmpty(this.textBox1.Text)) {
				if (G.mlog("#qデフォルトのパスワードを設定しますがよろしいですか?") != System.Windows.Forms.DialogResult.Yes) {
					e.Cancel = true;
					return;
				}
				m_pass_str = "miyazaki";
			}
			else {
				m_pass_str = this.textBox1.Text;
			}
			G.SS.save(G.SS);
		}
	}
}
