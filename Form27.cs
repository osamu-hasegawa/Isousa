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
	public partial class Form27 : Form
	{
		public G.SYSSET m_ss;

		public Form27()
		{
			InitializeComponent();
		}

		private void Form27_Load(object sender, EventArgs e)
		{
			m_ss = G.SS;
			DDX(true);

		}

		private void Form27_FormClosing(object sender, FormClosingEventArgs e)
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
#if true//2019.01.05(キューティクル検出欠損修正)
				DDV.DDX(bUpdate, this.numericUpDown10 , ref m_ss.TAT_STG_XMIN, G.SS.PLM_MLIM[0], G.SS.PLM_PLIM[0]);
				DDV.DDX(bUpdate, this.numericUpDown11 , ref m_ss.TAT_STG_XMAX, G.SS.PLM_MLIM[0], G.SS.PLM_PLIM[0]);
				DDV.DDX(bUpdate, this.numericUpDown12 , ref m_ss.TAT_STG_XSTP,                1, 100000);
#endif
				DDV.DDX(bUpdate, this.numericUpDown13 , ref m_ss.TAT_STG_YMIN, G.SS.PLM_MLIM[1], G.SS.PLM_PLIM[1]);
				DDV.DDX(bUpdate, this.numericUpDown14 , ref m_ss.TAT_STG_YMAX, G.SS.PLM_MLIM[1], G.SS.PLM_PLIM[1]);
				DDV.DDX(bUpdate, this.numericUpDown15 , ref m_ss.TAT_STG_YSTP,                1, 100000);
				DDV.DDX(bUpdate, this.numericUpDown16 , ref m_ss.TAT_STG_ZPOS, G.SS.PLM_MLIM[2], G.SS.PLM_PLIM[2]);
                if (bUpdate == false) {
/*					if (m_ss.PLM_AUT_ED_Y <= m_ss.PLM_AUT_HP_Y) {
						G.mlog("終了ステージ位置:yは開始位置:yより大きい値を指定してください.");
						this.numericUpDown14.Focus();
						return(false);
					}*/
                }
				if (bUpdate == false) {
				}
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
