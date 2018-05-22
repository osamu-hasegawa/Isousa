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
	public partial class Form33 : Form
	{
		//public string	m_ser1;

		public G.SYSSET	m_ss;

		public Form33()
		{
			InitializeComponent();
		}

		private void Form33_Load(object sender, EventArgs e)
		{
			DDX(true);
		}

		private void Form33_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK) {
				return;
			}
			if (DDX(false) == false) {
				e.Cancel = true;
			}
			else {
			}
		}
		private bool DDX(bool bUpdate)
        {
            bool rc=false;
#if true
			//
			try {
				DDV.DDX(bUpdate, this.textBox1, ref m_ss.EUI_XYA_TEXT[0]);
				DDV.DDX(bUpdate, this.textBox2, ref m_ss.EUI_XYA_TEXT[1]);
				DDV.DDX(bUpdate, this.textBox3, ref m_ss.EUI_XYA_TEXT[2]);
				DDV.DDX(bUpdate, this.textBox4, ref m_ss.EUI_ZFC_TEXT[0]);
				DDV.DDX(bUpdate, this.textBox5, ref m_ss.EUI_ZFC_TEXT[1]);
				DDV.DDX(bUpdate, this.textBox6, ref m_ss.EUI_ZFC_TEXT[2]);
				DDV.DDX(bUpdate, this.textBox7, ref m_ss.EUI_ZOM_TEXT[0]);
				DDV.DDX(bUpdate, this.textBox8, ref m_ss.EUI_ZOM_TEXT[1]);
				DDV.DDX(bUpdate, this.textBox9, ref m_ss.EUI_ZOM_TEXT[2]);
				//---
				DDV.DDX(bUpdate, this.numericUpDown1, ref m_ss.EUI_XYA_PCNT[0]);
				DDV.DDX(bUpdate, this.numericUpDown2, ref m_ss.EUI_XYA_PCNT[1]);
				DDV.DDX(bUpdate, this.numericUpDown3, ref m_ss.EUI_XYA_PCNT[2]);
				DDV.DDX(bUpdate, this.numericUpDown4, ref m_ss.EUI_ZFC_PCNT[0]);
				DDV.DDX(bUpdate, this.numericUpDown5, ref m_ss.EUI_ZFC_PCNT[1]);
				DDV.DDX(bUpdate, this.numericUpDown6, ref m_ss.EUI_ZFC_PCNT[2]);
				DDV.DDX(bUpdate, this.numericUpDown7, ref m_ss.EUI_ZOM_PCNT[0]);
				DDV.DDX(bUpdate, this.numericUpDown8, ref m_ss.EUI_ZOM_PCNT[1]);
				DDV.DDX(bUpdate, this.numericUpDown9, ref m_ss.EUI_ZOM_PCNT[2]);
				//-----
				DDV.DDX(bUpdate, this.textBox10, ref m_ss.EUI_ZOM_LABL[0]);
				DDV.DDX(bUpdate, this.textBox11, ref m_ss.EUI_ZOM_LABL[1]);
				DDV.DDX(bUpdate, this.numericUpDown10, ref m_ss.EUI_ZOM_PSET[0]);
				DDV.DDX(bUpdate, this.numericUpDown11, ref m_ss.EUI_ZOM_PSET[1]);
				//-----
                rc = true;
            }
            catch (Exception e)
            {
                G.mlog(e.Message);
                rc = false;
            }
#endif
            return (rc);
		}

		private void Form33_Validating(object sender, CancelEventArgs e)
		{
			if (DDX(false) == false) {
				e.Cancel = true;
			}
			else {
			}
		}
	}
}
