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
			//---
			radioButton3_CheckedChanged(null, null);
		}

		private void Form27_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult == DialogResult.Cancel) {
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
				DDV.DDX(bUpdate, this.numericUpDown10 , ref m_ss.TAT_STG_XMIN, G.SS.PLM_MLIM[0], G.SS.PLM_PLIM[0]);
				DDV.DDX(bUpdate, this.numericUpDown11 , ref m_ss.TAT_STG_XMAX, G.SS.PLM_MLIM[0], G.SS.PLM_PLIM[0]);
				DDV.DDX(bUpdate, this.numericUpDown12 , ref m_ss.TAT_STG_XSTP,                1, 100000);
				DDV.DDX(bUpdate, this.numericUpDown13 , ref m_ss.TAT_STG_YMIN, G.SS.PLM_MLIM[1], G.SS.PLM_PLIM[1]);
				DDV.DDX(bUpdate, this.numericUpDown14 , ref m_ss.TAT_STG_YMAX, G.SS.PLM_MLIM[1], G.SS.PLM_PLIM[1]);
				DDV.DDX(bUpdate, this.numericUpDown15 , ref m_ss.TAT_STG_YSTP,                1, 100000);
				DDV.DDX(bUpdate, this.numericUpDown16 , ref m_ss.TAT_STG_ZPOS, G.SS.PLM_MLIM[2], G.SS.PLM_PLIM[2]);
				DDV.DDX(bUpdate, this.checkBox1       , ref m_ss.TAT_STG_XINV);
				DDV.DDX(bUpdate, this.numericUpDown17 , ref m_ss.TAT_STG_SKIP);
				//---
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton1, this.radioButton2}, ref G.SS.TAT_ETC_MODE);
				//---
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton3, this.radioButton4}, ref G.SS.TAT_AFC_MODE);
				DDV.DDX(bUpdate, this.comboBox7       , ref m_ss.TAT_AFC_CMET);//計算方法:当面は画面全体のみ
				DDV.DDX(bUpdate, this.comboBox9       , ref m_ss.TAT_AFC_AFMD);//コントスラト計算範囲
				DDV.DDX(bUpdate, this.numericUpDown18 , ref m_ss.TAT_AFC_HANI);//ステップ範囲
				DDV.DDX(bUpdate, this.numericUpDown19 , ref m_ss.TAT_AFC_DISL);//ステップ大
				DDV.DDX(bUpdate, this.numericUpDown20 , ref m_ss.TAT_AFC_DISM);//ステップ中
				DDV.DDX(bUpdate, this.numericUpDown21 , ref m_ss.TAT_AFC_DISS);//ステップ小
				//---
				DDV.DDX(bUpdate, this.numericUpDown22 , ref m_ss.TAT_SUM_LOWR[0]);
				DDV.DDX(bUpdate, this.numericUpDown23 , ref m_ss.TAT_SUM_UPPR[0]);
				DDV.DDX(bUpdate, this.numericUpDown24 , ref m_ss.TAT_LEN_LOWR[0]);
				DDV.DDX(bUpdate, this.numericUpDown25 , ref m_ss.TAT_LEN_UPPR[0]);
				DDV.DDX(bUpdate, this.numericUpDown26 , ref m_ss.TAT_CIR_LOWR[0]);
				DDV.DDX(bUpdate, this.numericUpDown27 , ref m_ss.TAT_CIR_UPPR[0]);
				//---
				DDV.DDX(bUpdate, this.numericUpDown28 , ref m_ss.TAT_SUM_LOWR[1]);
				DDV.DDX(bUpdate, this.numericUpDown29 , ref m_ss.TAT_SUM_UPPR[1]);
				DDV.DDX(bUpdate, this.numericUpDown30 , ref m_ss.TAT_LEN_LOWR[1]);
				DDV.DDX(bUpdate, this.numericUpDown31 , ref m_ss.TAT_LEN_UPPR[1]);
				DDV.DDX(bUpdate, this.numericUpDown32 , ref m_ss.TAT_CIR_LOWR[1]);
				DDV.DDX(bUpdate, this.numericUpDown33 , ref m_ss.TAT_CIR_UPPR[1]);

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

		private void radioButton3_CheckedChanged(object sender, EventArgs e)
		{
			bool b1 = this.radioButton3.Checked; //F:AF, T:AF2

			this.numericUpDown19.Enabled = b1;
			this.numericUpDown20.Enabled = b1;
			this.numericUpDown21.Enabled = b1;
		}

		private void numericUpDown23_ValueChanged(object sender, EventArgs e)
		{
			this.numericUpDown28.Value = this.numericUpDown23.Value;
		}
    }
}
