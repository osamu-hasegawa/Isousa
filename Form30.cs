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
	public partial class Form30 : Form
	{
		//public string	m_ser1;

		public G.SYSSET	m_ss;

		public Form30()
		{
			InitializeComponent();
		}

		private void Form30_Load(object sender, EventArgs e)
		{
			DDX(true);
		}

		private void Form30_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK) {
				return;
			}
			if (DDX(false) == false) {
				e.Cancel = true;
			}
			else {
				for (int i = 0; i < 4; i++) {
					bool flag = false;
					if (m_ss.PLM_LSPD[i] != G.SS.PLM_LSPD[i]) { flag = true; }
					if (m_ss.PLM_JSPD[i] != G.SS.PLM_JSPD[i]) { flag = true; }
					if (m_ss.PLM_HSPD[i] != G.SS.PLM_HSPD[i]) { flag = true; }
					if (m_ss.PLM_ACCL[i] != G.SS.PLM_ACCL[i]) { flag = true; }
					if (m_ss.PLM_MLIM[i] != G.SS.PLM_MLIM[i]) { flag = true; }
					if (m_ss.PLM_PLIM[i] != G.SS.PLM_PLIM[i]) { flag = true; }
					if (flag) {
						G.mlog("#i速度、加速度、リミットの設定変更は「CONNECT」ボタン押下時に反映されます。");
						break;
					}
				}
			}
		}
		private bool DDX(bool bUpdate)
        {
            bool rc=false;
#if true
			TextBox[] txtLSPD = { this.textBox1, this.textBox8, this.textBox15, this.textBox22 };
			TextBox[] txtJSPD = { this.textBox2, this.textBox9, this.textBox16, this.textBox23 };
			TextBox[] txtHSPD = { this.textBox3, this.textBox10,this.textBox17, this.textBox24 };
			TextBox[] txtACCL = { this.textBox4, this.textBox11,this.textBox18, this.textBox25 };
			TextBox[] txtMLIM = { this.textBox5, this.textBox12,this.textBox19, this.textBox26 };
			TextBox[] txtPLIM = { this.textBox6, this.textBox13,this.textBox20, this.textBox27 };
			TextBox[] txtBSLA = { this.textBox7, this.textBox14, this.textBox21, this.textBox28 };
			TextBox[] txtOKRI = { this.textBox71, this.textBox68, this.textBox69, this.textBox70 };
			CheckBox[]chkPWSV = { this.checkBox7, this.checkBox8, this.checkBox9, this.checkBox10 };
			//
			TextBox[] txtCH1P = { this.textBox29, this.textBox32, this.textBox35, this.textBox73 };
			TextBox[] txtCH2P = { this.textBox30, this.textBox33, this.textBox36, this.textBox72 };
			TextBox[] txtCHWT = { this.textBox31, this.textBox34, this.textBox37 };
			TextBox[] txtCH3P = { this.textBox38, this.textBox40, this.textBox42, this.textBox74 };
			TextBox[] txtCH3T = { this.textBox39, this.textBox41, this.textBox43 };
			TextBox[] txtCH4P = { this.textBox44, this.textBox46, this.textBox48, this.textBox75 };
			TextBox[] txtCH4T = { this.textBox45, this.textBox47, this.textBox49 };
			//
			try {
				for (int i = 0; i < 4; i++) {
					DDV.DDX(bUpdate, txtLSPD[i], ref m_ss.PLM_LSPD[i], 1, 80000);
					DDV.DDX(bUpdate, txtJSPD[i], ref m_ss.PLM_JSPD[i], 1, 80000);
					DDV.DDX(bUpdate, txtHSPD[i], ref m_ss.PLM_HSPD[i], 1, 80000);
					DDV.DDX(bUpdate, txtACCL[i], ref m_ss.PLM_ACCL[i], 10, 1000);
					DDV.DDX(bUpdate, txtMLIM[i], ref m_ss.PLM_MLIM[i], -0x7FFFFF, +0x7FFFFF);
					DDV.DDX(bUpdate, txtPLIM[i], ref m_ss.PLM_PLIM[i], -0x7FFFFF, +0x7FFFFF);
					DDV.DDX(bUpdate, txtBSLA[i], ref m_ss.PLM_BSLA[i], 0, 200 * 8);
					DDV.DDX(bUpdate, txtOKRI[i], ref m_ss.PLM_UMPP[i], -1000, +1000);
					DDV.DDX(bUpdate, chkPWSV[i], ref m_ss.PLM_PWSV[i]);
					//---
					if (bUpdate == false) {
						if (m_ss.PLM_LSPD[i] > m_ss.PLM_JSPD[i] || m_ss.PLM_JSPD[i] > m_ss.PLM_HSPD[i]) {
							G.mlog("速度の大小関係に誤りがあります.", this);
							return(false);
						}
					}
				}
				if (true) {
					DDV.DDX(bUpdate, this.textBox60, ref m_ss.PLM_OFFS[2], -0x7FFFFF, +0x7FFFFF);
				}
				for (int i = 0; i < 4; i++) {
					DDV.DDX(bUpdate, txtCH1P[i], ref m_ss.PLM_POSX[i], -0x7FFFFF, +0x7FFFFF);
					DDV.DDX(bUpdate, txtCH2P[i], ref m_ss.PLM_POSY[i], -0x7FFFFF, +0x7FFFFF);
					DDV.DDX(bUpdate, txtCH3P[i], ref m_ss.PLM_POSF[i], -0x7FFFFF, +0x7FFFFF);
					DDV.DDX(bUpdate, txtCH4P[i], ref m_ss.PLM_POSZ[i], -0x7FFFFF, +0x7FFFFF);
					if (i > 2) {
						continue;
					}
					DDV.DDX(bUpdate, txtCHWT[i], ref m_ss.PLM_POSWT[i]);
					DDV.DDX(bUpdate, txtCH3T[i], ref m_ss.PLM_POSFT[i]);
					DDV.DDX(bUpdate, txtCH4T[i], ref m_ss.PLM_POSZT[i]);
				}
				//---
				if (true) {
					DDV.DDX(bUpdate, this.checkBox1, ref m_ss.LED_PWM_AUTO);
					DDV.DDX(bUpdate, this.textBox50, ref m_ss.LED_PWM_VAL[0]);
					DDV.DDX(bUpdate, this.textBox51, ref m_ss.LED_PWM_VAL[1]);
					DDV.DDX(bUpdate, this.textBox77, ref m_ss.LED_PWM_VAL[2]);
				}
				if (true) {
					DDV.DDX(bUpdate, this.checkBox2, ref m_ss.CAM_PAR_AUTO);
					//---
					//DDV.DDX(bUpdate, this.textBox52, ref m_ss.CAM_PAR_GAMMA[0], 0.0, 2.0);
					//DDV.DDX(bUpdate, this.textBox53, ref m_ss.CAM_PAR_CONTR[0], 0.0, 2.0);
					//DDV.DDX(bUpdate, this.textBox54, ref m_ss.CAM_PAR_BRIGH[0], 0.0, 2.0);
					//DDV.DDX(bUpdate, this.textBox55, ref m_ss.CAM_PAR_SHARP[0], 0.0, 2.0);
					////---
					//DDV.DDX(bUpdate, this.textBox56, ref m_ss.CAM_PAR_GAMMA[1], 0.0, 2.0);
					//DDV.DDX(bUpdate, this.textBox57, ref m_ss.CAM_PAR_CONTR[1], 0.0, 2.0);
					//DDV.DDX(bUpdate, this.textBox58, ref m_ss.CAM_PAR_BRIGH[1], 0.0, 2.0);
					//DDV.DDX(bUpdate, this.textBox59, ref m_ss.CAM_PAR_SHARP[1], 0.0, 2.0);
					//---
					//DDV.DDX(bUpdate, this.comboBox2, ref m_ss.CAM_PAR_EXMOD);
					//DDV.DDX(bUpdate, this.comboBox3, ref m_ss.CAM_PAR_WBMOD);
				}
				if (true) {
					if (bUpdate) {
						this.textBox61.Text = m_ss.ZOM_PLS_A.ToString();
						this.textBox62.Text = m_ss.ZOM_PLS_B.ToString();
					}
					else {
						DDV.DDX(bUpdate, this.textBox61, ref m_ss.ZOM_PLS_A);
						DDV.DDX(bUpdate, this.textBox62, ref m_ss.ZOM_PLS_B);
					}
					DDV.DDX(bUpdate, this.textBox63, ref m_ss.ZOM_TST_Y);
					DDV.DDX(bUpdate, this.textBox66, ref m_ss.ZOM_TST_X);
					DDV.DDX(bUpdate, this.textBox76, ref m_ss.CAM_SPE_UMPPX);
				}
				if (true) {
					DDV.DDX(bUpdate, this.textBox67, ref m_ss.ETC_LED_WAIT);
					DDV.DDX(bUpdate, this.comboBox1, ref m_ss.ETC_UIF_LEVL);
					DDV.DDX(bUpdate, this.checkBox3, ref m_ss.ETC_LED_IRGR);
					DDV.DDX(bUpdate, this.comboBox4, ref m_ss.ETC_CLF_CTCR);
					DDV.DDX(bUpdate, this.checkBox4, ref m_ss.ETC_UIF_CUTI);
					DDV.DDX(bUpdate, this.checkBox5, ref m_ss.PLM_AUT_FINI);
					DDV.DDX(bUpdate, this.checkBox6, ref m_ss.PLM_AUT_ZINI);
				}

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

		private void button3_Click(object sender, EventArgs e)
		{
			if (!DDX(false)) {
				return;
			}
			double X = (m_ss.ZOM_TST_Y - m_ss.ZOM_PLS_B)/m_ss.ZOM_PLS_A;
			this.textBox64.Text = string.Format("{0:F0}", X);
		}

		private void button4_Click(object sender, EventArgs e)
		{
			if (!DDX(false)) {
				return;
			}
			double Y = m_ss.ZOM_PLS_A * m_ss.ZOM_TST_X + m_ss.ZOM_PLS_B;
			this.textBox65.Text = Y.ToString();
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			TextBox[] txts = {
				this.textBox1, this.textBox2, this.textBox3, this.textBox4//, this.textBox7
			};
			TextBox[] txtd = {
				this.textBox8, this.textBox9, this.textBox10, this.textBox11//, this.textBox14
			};
			for (int i = 0; i < txts.Length; i++) {
				if (sender == txts[i]) {
					txtd[i].Text = txts[i].Text;
					break;
				}
			}
		}

		private void Form30_Validated(object sender, EventArgs e)
		{

		}

		private void Form30_Validating(object sender, CancelEventArgs e)
		{
			if (DDX(false) == false) {
				e.Cancel = true;
			}
			else {
				for (int i = 0; i < 4; i++) {
					bool flag = false;
					if (m_ss.PLM_LSPD[i] != G.SS.PLM_LSPD[i]) { flag = true; }
					if (m_ss.PLM_JSPD[i] != G.SS.PLM_JSPD[i]) { flag = true; }
					if (m_ss.PLM_HSPD[i] != G.SS.PLM_HSPD[i]) { flag = true; }
					if (m_ss.PLM_ACCL[i] != G.SS.PLM_ACCL[i]) { flag = true; }
					if (m_ss.PLM_MLIM[i] != G.SS.PLM_MLIM[i]) { flag = true; }
					if (m_ss.PLM_PLIM[i] != G.SS.PLM_PLIM[i]) { flag = true; }
					if (flag) {
						G.mlog("#i速度、加速度、リミットの設定変更は「CONNECT」ボタン押下時に反映されます。");
						break;
					}
				}
			}
		}
	}
}
