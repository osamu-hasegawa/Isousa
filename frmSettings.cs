using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uSCOPE
{
	public partial class frmSettings : Form
	{
		public G.SYSSET	m_ss;
		static
		private int m_last_idx = 0;

		public frmSettings()
		{
			InitializeComponent();
		}

		private void frmSettings_Load(object sender, EventArgs e)
		{
			Form30 f1 = new Form30();
#if true//2018.09.27(20本対応と解析用パラメータ追加)
			Form34 f2 = new Form34();
			Form34 f5 = new Form34();
#else
			Form31 f2 = new Form31();
#endif
			Form32 f3 = new Form32();
			Form33 f4 = new Form33();
		//f1.BringToFront();
			f1.m_ss = m_ss;
			f2.m_ss = m_ss;
			f3.m_ss = m_ss;
			f4.m_ss = m_ss;
			f1.TopLevel = false;
			f2.TopLevel = false;
			f3.TopLevel = false;
			f4.TopLevel = false;
			this.tabPage1.Controls.Add(f1);
			this.tabPage2.Controls.Add(f2);
			this.tabPage3.Controls.Add(f3);
			this.tabPage4.Controls.Add(f4);
			//---
			this.tabPage1.Text = f1.Text;
			this.tabPage2.Text = f2.Text;
			this.tabPage3.Text = f3.Text;
			this.tabPage4.Text = f4.Text;
			f1.Visible = true;
			f2.Visible = true;
			f3.Visible = true;
			f4.Visible = true;
#if true//2018.09.27(20本対応と解析用パラメータ追加)
			f5.m_ss = m_ss;
			f5.TopLevel = false;
			this.tabPage5.Controls.Add(f5);
			this.tabPage5.Text = "領域抽出パラメータ(解析)";
			f5.Visible = true;
			f2.Q = 0;
			f5.Q = 3;
#endif
#if true//2018.04.08(ＡＦパラメータ)
			Form35 f6 = new Form35();
			f6.m_ss = m_ss;
			f6.TopLevel = false;
			this.tabPage6.Controls.Add(f6);
			this.tabPage6.Text = f6.Text;
			f6.Visible = true;
#endif
			//---
			DDX(true);
			this.tabControl1.SelectedIndex = m_last_idx;
		}

		private void frmSettings_FormClosing(object sender, FormClosingEventArgs e)
		{
			m_last_idx = this.tabControl1.SelectedIndex;

			if (this.DialogResult != DialogResult.OK) {
				return;
			}
			if (DDX(false) == false) {
				e.Cancel = true;
			}
		}
		private bool DDX(bool bUpdate)
        {
            bool rc=true;
            return (rc);
		}
	}
}
