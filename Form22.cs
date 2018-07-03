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
	public partial class Form22 : Form
	{
		public G.SYSSET m_ss;

		public Form22()
		{
			InitializeComponent();
		}

		private void Form22_Load(object sender, EventArgs e)
		{
			m_ss = G.SS;
			DDX(true);
		}

		private void Form22_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK) {
				return;
			}
			if (DDX(false) == false) {
				e.Cancel = true;
			}
			else {
				G.SS = (G.SYSSET)m_ss.Clone();
			}
		}
		private bool DDX(bool bUpdate)
        {
            bool rc;
			try {
				DDV.DDX(bUpdate, this.numericUpDown10 , ref m_ss.PLM_AUT_HP_X, G.SS.PLM_MLIM[0], G.SS.PLM_PLIM[0]);
				DDV.DDX(bUpdate, this.numericUpDown11 , ref m_ss.PLM_AUT_HP_Y, G.SS.PLM_MLIM[1], G.SS.PLM_PLIM[1]);
#if true//2018.07.02
				DDV.DDX(bUpdate, this.numericUpDown12 , ref m_ss.PLM_AUT_HP_Z, G.SS.PLM_MLIM[2], G.SS.PLM_PLIM[2]);
#endif
                rc = true;
            }
            catch (Exception e)
            {
                G.mlog(e.Message);
                rc = false;
            }
            return (rc);
		}
	}
}
