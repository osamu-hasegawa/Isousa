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
	public partial class Form31 : Form
	{
		//public string	m_ser1;

		public G.SYSSET	m_ss;
		//private NumericUpDown[] numGAMMA;
		//private NumericUpDown[] numCONTR;
		//private NumericUpDown[] numBRIGT;
		//private NumericUpDown[] numSHARP;
		//
		private ComboBox[]      cmbFILTR;
		private ComboBox[]      cmbBINAR;
		//
		private NumericUpDown[] numBINVL;
		private NumericUpDown[] numH_LOW;
		private NumericUpDown[] numH_UPR;
		private NumericUpDown[] numS_LOW;
		private NumericUpDown[] numS_UPR;
		private NumericUpDown[] numV_LOW;
		private NumericUpDown[] numV_UPR;
		//
		private NumericUpDown[] numA_LOW;
		private NumericUpDown[] numA_UPR;
		private NumericUpDown[] numL_LOW;
		private NumericUpDown[] numL_UPR;
		private NumericUpDown[] numC_LOW;
		private NumericUpDown[] numC_UPR;
		//
		private NumericUpDown[] numPRECI;
		private NumericUpDown[] numMAGNI;

		public Form31()
		{
			InitializeComponent();
		}

		private void Form31_Load(object sender, EventArgs e)
		{
			//numGAMMA = new NumericUpDown[] { this.numericUpDown1 , this.numericUpDown20, this.numericUpDown39 };
			//numCONTR = new NumericUpDown[] { this.numericUpDown2 , this.numericUpDown21, this.numericUpDown40 };
			//numBRIGT = new NumericUpDown[] { this.numericUpDown3 , this.numericUpDown22, this.numericUpDown41 };
			//numSHARP = new NumericUpDown[] { this.numericUpDown4 , this.numericUpDown23, this.numericUpDown42 };
			//
			cmbFILTR = new ComboBox[] { this.comboBox1,  this.comboBox3,  this.comboBox5,  this.comboBox7 };
			cmbBINAR = new ComboBox[] { this.comboBox2,  this.comboBox4,  this.comboBox6,  this.comboBox8 };
			//
			numBINVL = new NumericUpDown[] { this.numericUpDown5 , this.numericUpDown24, this.numericUpDown43, this.numericUpDown58 };
			numH_LOW = new NumericUpDown[] { this.numericUpDown6 , this.numericUpDown25, this.numericUpDown44, this.numericUpDown59 };
			numH_UPR = new NumericUpDown[] { this.numericUpDown7 , this.numericUpDown26, this.numericUpDown45, this.numericUpDown60 };
			numS_LOW = new NumericUpDown[] { this.numericUpDown8 , this.numericUpDown27, this.numericUpDown46, this.numericUpDown61 };
			numS_UPR = new NumericUpDown[] { this.numericUpDown9 , this.numericUpDown28, this.numericUpDown47, this.numericUpDown62 };
			numV_LOW = new NumericUpDown[] { this.numericUpDown10, this.numericUpDown29, this.numericUpDown48, this.numericUpDown63 };
			numV_UPR = new NumericUpDown[] { this.numericUpDown11, this.numericUpDown30, this.numericUpDown49, this.numericUpDown64 };
			//
			numA_LOW = new NumericUpDown[] { this.numericUpDown12, this.numericUpDown31, this.numericUpDown50, this.numericUpDown65 };
			numA_UPR = new NumericUpDown[] { this.numericUpDown13, this.numericUpDown32, this.numericUpDown51, this.numericUpDown66 };
			numL_LOW = new NumericUpDown[] { this.numericUpDown14, this.numericUpDown33, this.numericUpDown52, this.numericUpDown67 };
			numL_UPR = new NumericUpDown[] { this.numericUpDown15, this.numericUpDown34, this.numericUpDown53, this.numericUpDown68 };
			numC_LOW = new NumericUpDown[] { this.numericUpDown16, this.numericUpDown35, this.numericUpDown54, this.numericUpDown69 };
			numC_UPR = new NumericUpDown[] { this.numericUpDown17, this.numericUpDown36, this.numericUpDown55, this.numericUpDown70 };
			//
			numPRECI = new NumericUpDown[] { this.numericUpDown18, this.numericUpDown37, this.numericUpDown56, null                 };
			numMAGNI = new NumericUpDown[] { this.numericUpDown19, this.numericUpDown38, this.numericUpDown57, this.numericUpDown75 };
			//
			this.comboBox8.Enabled = false;
			//
			DDX(true);
			for (int i = 0; i < 4; i++) {
				comboBox2_SelectedIndexChanged(cmbBINAR[i], null);
			}
		}

		//private void Form31_FormClosing(object sender, FormClosingEventArgs e)
		//{
		//    if (this.DialogResult != DialogResult.OK) {
		//        return;
		//    }
		//    if (DDX(false) == false) {
		//        e.Cancel = true;
		//    }
		//    else {
		//        //for (int i = 0; i < 4; i++) {
		//        //    bool flag = false;
		//        //    if (m_ss.PLM_LSPD[i] != G.SS.PLM_LSPD[i]) { flag = true; }
		//        //    if (m_ss.PLM_JSPD[i] != G.SS.PLM_JSPD[i]) { flag = true; }
		//        //    if (m_ss.PLM_HSPD[i] != G.SS.PLM_HSPD[i]) { flag = true; }
		//        //    if (m_ss.PLM_ACCL[i] != G.SS.PLM_ACCL[i]) { flag = true; }
		//        //    if (m_ss.PLM_MLIM[i] != G.SS.PLM_MLIM[i]) { flag = true; }
		//        //    if (m_ss.PLM_PLIM[i] != G.SS.PLM_PLIM[i]) { flag = true; }
		//        //    if (flag) {
		//        //        G.mlog("#i速度、加速度、リミットの設定変更は「CONNECT」ボタン押下時に反映されます。");
		//        //        break;
		//        //    }
		//        //}
		//    }
		//}
		private bool DDX(bool bUpdate)
        {
            bool rc=false;
			//
			try {
				for (int i = 0; i < 4; i++) {
					//---
					//if (i < 3) {
					//DDV.DDX(bUpdate, numGAMMA[i], ref m_ss.CAM_PAR_GAMMA[i]);
					//DDV.DDX(bUpdate, numCONTR[i], ref m_ss.CAM_PAR_CONTR[i]);
					//DDV.DDX(bUpdate, numBRIGT[i], ref m_ss.CAM_PAR_BRIGH[i]);
					//DDV.DDX(bUpdate, numSHARP[i], ref m_ss.CAM_PAR_SHARP[i]);
					//}
					//---
					DDV.DDX(bUpdate, cmbFILTR[i], ref m_ss.IMP_FLT_COEF[i]);
					DDV.DDX(bUpdate, cmbBINAR[i], ref m_ss.IMP_BIN_MODE[i]);
					//---
					DDV.DDX(bUpdate, numBINVL[i], ref m_ss.IMP_BIN_BVAL[i]);
					DDV.DDX(bUpdate, numH_LOW[i], ref m_ss.IMP_HUE_LOWR[i]);
					DDV.DDX(bUpdate, numH_UPR[i], ref m_ss.IMP_HUE_UPPR[i]);
					DDV.DDX(bUpdate, numS_LOW[i], ref m_ss.IMP_SAT_LOWR[i]);
					DDV.DDX(bUpdate, numS_UPR[i], ref m_ss.IMP_SAT_UPPR[i]);
					DDV.DDX(bUpdate, numV_LOW[i], ref m_ss.IMP_VAL_LOWR[i]);
					DDV.DDX(bUpdate, numV_UPR[i], ref m_ss.IMP_VAL_UPPR[i]);
					//---
					DDV.DDX(bUpdate, numA_LOW[i], ref m_ss.IMP_SUM_LOWR[i]);
					DDV.DDX(bUpdate, numA_UPR[i], ref m_ss.IMP_SUM_UPPR[i]);
					DDV.DDX(bUpdate, numL_LOW[i], ref m_ss.IMP_LEN_LOWR[i]);
					DDV.DDX(bUpdate, numL_UPR[i], ref m_ss.IMP_LEN_UPPR[i]);
					DDV.DDX(bUpdate, numC_LOW[i], ref m_ss.IMP_CIR_LOWR[i]);
					DDV.DDX(bUpdate, numC_UPR[i], ref m_ss.IMP_CIR_UPPR[i]);
					//---
					if (i < 3) {
					DDV.DDX(bUpdate, numPRECI[i], ref m_ss.IMP_POL_PREC[i]);
					}
					DDV.DDX(bUpdate, numMAGNI[i], ref m_ss.IMP_OPT_MAGN[i]);
					//---
					if (i == 3) {
					DDV.DDX(bUpdate, this.numericUpDown71, ref m_ss.IMP_CUV_LOWR[i]);
					DDV.DDX(bUpdate, this.numericUpDown72, ref m_ss.IMP_CUV_UPPR[i]);
					DDV.DDX(bUpdate, this.numericUpDown73, ref m_ss.IMP_GIZ_LOWR[i]);
					DDV.DDX(bUpdate, this.numericUpDown74, ref m_ss.IMP_GIZ_UPPR[i]);
					}
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

		private void Form31_Validating(object sender, CancelEventArgs e)
		{
			if (DDX(false) == false) {
				e.Cancel = true;
			}
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox cb = (ComboBox)sender;
			int i = int.Parse(cb.Tag.ToString());
			bool b = (cb.SelectedIndex != 0);
			numBINVL[i].Enabled = !b;
			numH_LOW[i].Enabled = b;
			numH_UPR[i].Enabled = b;
			numS_LOW[i].Enabled = b;
			numS_UPR[i].Enabled = b;
			numV_LOW[i].Enabled = b;
			numV_UPR[i].Enabled = b;
		}
	}
}
