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
	public partial class Form32 : Form
	{
		public G.SYSSET	m_ss;
		private NumericUpDown[] numGAMMA;
		private NumericUpDown[] numCONTR;
		private NumericUpDown[] numBRIGT;
		private NumericUpDown[] numSHARP;
		//
		private ComboBox[]      cmbGMODE;
		private ComboBox[]      cmbEMODE;
		private ComboBox[]      cmbWMODE;
		//
		private NumericUpDown[] numGAIDB;
		private NumericUpDown[] numGAIOF;
		//
		private NumericUpDown[] numEXPTM;
		private NumericUpDown[] numEXPOF;
		//
		private NumericUpDown[] numRAT_R;
		private NumericUpDown[] numRAT_G;
		private NumericUpDown[] numRAT_B;
		//
		private Button[]		btnCURVL;
		//
#if true//2019.01.23(GAIN調整&自動測定)
		private NumericUpDown[]	numTAR_G;
#endif
		public Form32()
		{
			InitializeComponent();
		}

		private void Form32_Load(object sender, EventArgs e)
		{
#if true//2019.01.11(混在対応)
			numGAMMA = new NumericUpDown[] { this.numericUpDown1 , this.numericUpDown10, this.numericUpDown19, this.numericUpDown34 };
			numCONTR = new NumericUpDown[] { this.numericUpDown2 , this.numericUpDown11, this.numericUpDown20, this.numericUpDown35 };
			numBRIGT = new NumericUpDown[] { this.numericUpDown3 , this.numericUpDown12, this.numericUpDown21, this.numericUpDown36 };
			numSHARP = new NumericUpDown[] { this.numericUpDown4 , this.numericUpDown13, this.numericUpDown22, this.numericUpDown37 };
			//
			cmbGMODE = new ComboBox[] { this.comboBox1,  this.comboBox4,  this.comboBox7,  this.comboBox10};
			cmbEMODE = new ComboBox[] { this.comboBox2,  this.comboBox5,  this.comboBox8,  this.comboBox11};
			cmbWMODE = new ComboBox[] { this.comboBox3,  this.comboBox6,  this.comboBox9,  this.comboBox12};
			//
			numGAIDB = new NumericUpDown[] { this.numericUpDown5 , this.numericUpDown14, this.numericUpDown23, this.numericUpDown38 };
			numGAIOF = new NumericUpDown[] { this.numericUpDown28, this.numericUpDown29, this.numericUpDown30, this.numericUpDown39 };
			//
			numEXPTM = new NumericUpDown[] { this.numericUpDown6 , this.numericUpDown15, this.numericUpDown24, this.numericUpDown40 };
			numEXPOF = new NumericUpDown[] { this.numericUpDown31, this.numericUpDown32, this.numericUpDown33, this.numericUpDown41 };
			//
			numRAT_R = new NumericUpDown[] { this.numericUpDown7 , this.numericUpDown16, this.numericUpDown25, this.numericUpDown42 };
			numRAT_G = new NumericUpDown[] { this.numericUpDown8 , this.numericUpDown17, this.numericUpDown26, this.numericUpDown43 };
			numRAT_B = new NumericUpDown[] { this.numericUpDown9 , this.numericUpDown18, this.numericUpDown27, this.numericUpDown44 };
			//
			btnCURVL = new Button[] { this.button1, this.button2, this.button3, this.button4 };
#else
			numGAMMA = new NumericUpDown[] { this.numericUpDown1 , this.numericUpDown10, this.numericUpDown19 };
			numCONTR = new NumericUpDown[] { this.numericUpDown2 , this.numericUpDown11, this.numericUpDown20 };
			numBRIGT = new NumericUpDown[] { this.numericUpDown3 , this.numericUpDown12, this.numericUpDown21 };
			numSHARP = new NumericUpDown[] { this.numericUpDown4 , this.numericUpDown13, this.numericUpDown22 };
			//
			cmbGMODE = new ComboBox[] { this.comboBox1,  this.comboBox4,  this.comboBox7 };
			cmbEMODE = new ComboBox[] { this.comboBox2,  this.comboBox5,  this.comboBox8 };
			cmbWMODE = new ComboBox[] { this.comboBox3,  this.comboBox6,  this.comboBox9 };
			//
			numGAIDB = new NumericUpDown[] { this.numericUpDown5 , this.numericUpDown14, this.numericUpDown23 };
			numGAIOF = new NumericUpDown[] { this.numericUpDown28, this.numericUpDown29, this.numericUpDown30 };
			//
			numEXPTM = new NumericUpDown[] { this.numericUpDown6 , this.numericUpDown15, this.numericUpDown24 };
			numEXPOF = new NumericUpDown[] { this.numericUpDown31, this.numericUpDown32, this.numericUpDown33 };
			//
			numRAT_R = new NumericUpDown[] { this.numericUpDown7 , this.numericUpDown16, this.numericUpDown25 };
			numRAT_G = new NumericUpDown[] { this.numericUpDown8 , this.numericUpDown17, this.numericUpDown26 };
			numRAT_B = new NumericUpDown[] { this.numericUpDown9 , this.numericUpDown18, this.numericUpDown27 };
			//
			btnCURVL = new Button[] { this.button1, this.button2, this.button3 };
#endif
#if true//2019.01.23(GAIN調整&自動測定)
			numTAR_G = new NumericUpDown[] { this.numericUpDown45, this.numericUpDown46, this.numericUpDown47, this.numericUpDown48 };
#endif
			//
			//this.comboBox8.Enabled = false;
			//
			DDX(true);
			for (int i = 0; i <
#if true//2019.01.11(混在対応)
				4;
#else
				3;
#endif
				i++) {
				comboBox1_SelectedIndexChanged(cmbGMODE[i], null);
				comboBox2_SelectedIndexChanged(cmbEMODE[i], null);
				comboBox3_SelectedIndexChanged(cmbWMODE[i], null);
			}
		}

		private bool DDX(bool bUpdate)
        {
            bool rc=false;
			//
			try {
				for (int i = 0; i <
#if true//2019.01.11(混在対応)
					4;
#else
					3;
#endif
					i++) {
					//---
					DDV.DDX(bUpdate, numGAMMA[i], ref m_ss.CAM_PAR_GAMMA[i]);
					DDV.DDX(bUpdate, numCONTR[i], ref m_ss.CAM_PAR_CONTR[i]);
					DDV.DDX(bUpdate, numBRIGT[i], ref m_ss.CAM_PAR_BRIGH[i]);
					DDV.DDX(bUpdate, numSHARP[i], ref m_ss.CAM_PAR_SHARP[i]);
					//---
					DDV.DDX(bUpdate, cmbGMODE[i], ref m_ss.CAM_PAR_GAMOD[i]);
					DDV.DDX(bUpdate, cmbEMODE[i], ref m_ss.CAM_PAR_EXMOD[i]);
					DDV.DDX(bUpdate, cmbWMODE[i], ref m_ss.CAM_PAR_WBMOD[i]);
					//---
					DDV.DDX(bUpdate, numGAIDB[i], ref m_ss.CAM_PAR_GA_VL[i]);
					DDV.DDX(bUpdate, numGAIOF[i], ref m_ss.CAM_PAR_GA_OF[i]);
					DDV.DDX(bUpdate, numEXPTM[i], ref m_ss.CAM_PAR_EX_VL[i]);
					DDV.DDX(bUpdate, numEXPOF[i], ref m_ss.CAM_PAR_EX_OF[i]);
					DDV.DDX(bUpdate, numRAT_R[i], ref m_ss.CAM_PAR_WB_RV[i]);
					DDV.DDX(bUpdate, numRAT_G[i], ref m_ss.CAM_PAR_WB_GV[i]);
					DDV.DDX(bUpdate, numRAT_B[i], ref m_ss.CAM_PAR_WB_BV[i]);
					//---
#if true//2019.01.23(GAIN調整&自動測定)
					DDV.DDX(bUpdate, numTAR_G[i], ref m_ss.CAM_PAR_TARVP[i]);
#endif
					//---
					//---
				}
				//-----
                rc = true;
            }
            catch (Exception e)
            {
                G.mlog(e.Message);
                rc = false;
            }
            return (rc);
		}

		private void Form32_Validating(object sender, CancelEventArgs e)
		{
			if (DDX(false) == false) {
				e.Cancel = true;
			}
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox cb = (ComboBox)sender;
			int i;
			for (i = 0; i <
#if true//2019.01.11(混在対応)
				4;
#else
				3;
#endif
				i++) {
				if (object.Equals(cb, this.cmbGMODE[i])) {
					break;
				}
				//if (object.ReferenceEquals(cb, this.cmbGMODE[i])) {
				//    break;
				//}
			}
			this.numGAIDB[i].Enabled = (cb.SelectedIndex == 0);
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox cb = (ComboBox)sender;
			int i;
			for (i = 0; i <
#if true//2019.01.11(混在対応)
				4;
#else
				3;
#endif
				i++) {
				if (object.Equals(cb, this.cmbEMODE[i])) {
					break;
				}
			}
			this.numEXPTM[i].Enabled = (cb.SelectedIndex == 0);
		}

		private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox cb = (ComboBox)sender;
			int i;
			for (i = 0; i <
#if true//2019.01.11(混在対応)
				4;
#else
				3;
#endif
				i++) {
				if (object.Equals(cb, this.cmbWMODE[i])) {
					break;
				}
			}
			this.numRAT_R[i].Enabled = (cb.SelectedIndex == 0);
			this.numRAT_G[i].Enabled = (cb.SelectedIndex == 0);
			this.numRAT_B[i].Enabled = (cb.SelectedIndex == 0);
}

		private void button1_Click(object sender, EventArgs e)
		{
			//Bu cb = (ComboBox)sender;
			int i;
			for (i = 0; i <
#if true//2019.01.11(混在対応)
				4;
#else
				3;
#endif
				i++) {
				if (object.Equals(sender, this.btnCURVL[i])) {
					break;
				}
			}
			if (G.FORM02 == null || G.FORM02.isCONNECTED() == false) {
				return;
			}
			double fval, fmin, fmax;
			//---
			G.FORM02.get_param(Form02.CAM_PARAM.GAIN, out fval, out fmax, out fmin);
			this.numGAIDB[i].Value = (decimal)fval;
			//---
			G.FORM02.get_param(Form02.CAM_PARAM.EXPOSURE, out fval, out fmax, out fmin);
			this.numEXPTM[i].Value = (decimal)fval;
			//---
			G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 0);
			G.FORM02.get_param(Form02.CAM_PARAM.BALANCE, out fval, out fmax, out fmin);
			this.numRAT_R[i].Value = (decimal)fval;
			//---
			G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 1);
			G.FORM02.get_param(Form02.CAM_PARAM.BALANCE, out fval, out fmax, out fmin);
			this.numRAT_G[i].Value = (decimal)fval;
			//---
			G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 2);
			G.FORM02.get_param(Form02.CAM_PARAM.BALANCE, out fval, out fmax, out fmin);
			this.numRAT_B[i].Value = (decimal)fval;
			//---
		}
	}
}
