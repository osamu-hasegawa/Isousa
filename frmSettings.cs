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
			Form31 f2 = new Form31();
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
