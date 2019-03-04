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
	public partial class Form34 : Form
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
#if true//2019.02.03(WB調整)
		private NumericUpDown[] numSOFFS;
		private NumericUpDown[] numCOFFS;
#endif
#if true//2019.03.02(直線近似)
		private CheckBox[]      chkRLINE;
#endif
		public int Q;

		public Form34()
		{
			InitializeComponent();
		}

		private void Form34_Load(object sender, EventArgs e)
		{
			//
			cmbFILTR = new ComboBox[] { this.comboBox1,  this.comboBox3,  this.comboBox5};
			cmbBINAR = new ComboBox[] { this.comboBox2,  this.comboBox4,  this.comboBox6};
			//
			numBINVL = new NumericUpDown[] { this.numericUpDown5 , this.numericUpDown24, this.numericUpDown43};
			numH_LOW = new NumericUpDown[] { this.numericUpDown6 , this.numericUpDown25, this.numericUpDown44};
			numH_UPR = new NumericUpDown[] { this.numericUpDown7 , this.numericUpDown26, this.numericUpDown45};
			numS_LOW = new NumericUpDown[] { this.numericUpDown8 , this.numericUpDown27, this.numericUpDown46};
			numS_UPR = new NumericUpDown[] { this.numericUpDown9 , this.numericUpDown28, this.numericUpDown47};
			numV_LOW = new NumericUpDown[] { this.numericUpDown10, this.numericUpDown29, this.numericUpDown48};
			numV_UPR = new NumericUpDown[] { this.numericUpDown11, this.numericUpDown30, this.numericUpDown49};
			//
			numA_LOW = new NumericUpDown[] { this.numericUpDown12, this.numericUpDown31, this.numericUpDown50};
			numA_UPR = new NumericUpDown[] { this.numericUpDown13, this.numericUpDown32, this.numericUpDown51};
			numL_LOW = new NumericUpDown[] { this.numericUpDown14, this.numericUpDown33, this.numericUpDown52};
			numL_UPR = new NumericUpDown[] { this.numericUpDown15, this.numericUpDown34, this.numericUpDown53};
			numC_LOW = new NumericUpDown[] { this.numericUpDown16, this.numericUpDown35, this.numericUpDown54};
			numC_UPR = new NumericUpDown[] { this.numericUpDown17, this.numericUpDown36, this.numericUpDown55};
			//
			numPRECI = new NumericUpDown[] { this.numericUpDown18, this.numericUpDown37, this.numericUpDown56};
			numMAGNI = new NumericUpDown[] { this.numericUpDown19, this.numericUpDown38, this.numericUpDown57};
			//
#if true//2019.03.02(直線近似)
			chkRLINE = new CheckBox[] { this.checkBox1, this.checkBox2, this.checkBox3};
#endif

			DDX(true);
			for (int i = 0; i < cmbBINAR.Length; i++) {
				comboBox2_SelectedIndexChanged(cmbBINAR[i], null);
			}
			if (Q == 3) {
				this.Text = "領域抽出パラメータ(解析)";
				this.label30.Text = "白色LED(透過)画像時";
				this.label40.Text = "白色LED(反射)画像時";
				this.label31.Text = "赤外LED画像時";
				this.comboBox9.Enabled = false;
				this.comboBox10.Enabled = false;
#if true//2018.11.13(毛髪中心AF)
				this.comboBox11.Enabled = false;//透過(中心)
				this.comboBox12.Enabled = false;//反射(中心)
#endif
#if true//2019.02.03(WB調整)
				this.numericUpDown1.Enabled = false;
				this.numericUpDown2.Enabled = false;
				this.numericUpDown3.Enabled = false;
				this.numericUpDown4.Enabled = false;
#endif
				this.label4.Visible = true;
			}
#if true//2019.03.02(直線近似)
			checkBox1_CheckedChanged(null, null);
#endif
		}

		//private void Form34_FormClosing(object sender, FormClosingEventArgs e)
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
				for (int i = 0; i < 3; i++) {
					//---
					DDV.DDX(bUpdate, cmbFILTR[i], ref m_ss.IMP_FLT_COEF[Q+i]);
					DDV.DDX(bUpdate, cmbBINAR[i], ref m_ss.IMP_BIN_MODE[Q+i]);
					//---
					DDV.DDX(bUpdate, numBINVL[i], ref m_ss.IMP_BIN_BVAL[Q+i]);
					DDV.DDX(bUpdate, numH_LOW[i], ref m_ss.IMP_HUE_LOWR[Q+i]);
					DDV.DDX(bUpdate, numH_UPR[i], ref m_ss.IMP_HUE_UPPR[Q+i]);
					DDV.DDX(bUpdate, numS_LOW[i], ref m_ss.IMP_SAT_LOWR[Q+i]);
					DDV.DDX(bUpdate, numS_UPR[i], ref m_ss.IMP_SAT_UPPR[Q+i]);
					DDV.DDX(bUpdate, numV_LOW[i], ref m_ss.IMP_VAL_LOWR[Q+i]);
					DDV.DDX(bUpdate, numV_UPR[i], ref m_ss.IMP_VAL_UPPR[Q+i]);
					//---
					DDV.DDX(bUpdate, numA_LOW[i], ref m_ss.IMP_SUM_LOWR[Q+i]);
					DDV.DDX(bUpdate, numA_UPR[i], ref m_ss.IMP_SUM_UPPR[Q+i]);
					DDV.DDX(bUpdate, numL_LOW[i], ref m_ss.IMP_LEN_LOWR[Q+i]);
					DDV.DDX(bUpdate, numL_UPR[i], ref m_ss.IMP_LEN_UPPR[Q+i]);
					DDV.DDX(bUpdate, numC_LOW[i], ref m_ss.IMP_CIR_LOWR[Q+i]);
					DDV.DDX(bUpdate, numC_UPR[i], ref m_ss.IMP_CIR_UPPR[Q+i]);
					//---
					if (i < 3) {
					DDV.DDX(bUpdate, numPRECI[i], ref m_ss.IMP_POL_PREC[Q+i]);
					}
#if false//2019.02.03(WB調整)
					DDV.DDX(bUpdate, numMAGNI[i], ref m_ss.IMP_OPT_MAGN[Q+i]);
#endif
#if true//2019.03.02(直線近似)
					DDV.DDX(bUpdate, chkRLINE[i], ref m_ss.IMP_REG_LINE[Q+i]);
#endif
					//---
					if (i == 3) {
					//DDV.DDX(bUpdate, this.numericUpDown71, ref m_ss.IMP_CUV_LOWR[i]);
					//DDV.DDX(bUpdate, this.numericUpDown72, ref m_ss.IMP_CUV_UPPR[i]);
					//DDV.DDX(bUpdate, this.numericUpDown73, ref m_ss.IMP_GIZ_LOWR[i]);
					//DDV.DDX(bUpdate, this.numericUpDown74, ref m_ss.IMP_GIZ_UPPR[i]);
					}
				}
				if (Q == 0) {
				DDV.DDX(bUpdate, this.comboBox9, ref m_ss.IMP_AUT_AFMD[0]);//透過(表面)
				DDV.DDX(bUpdate, this.comboBox10, ref m_ss.IMP_AUT_AFMD[1]);//反射(表面)
#if true//2018.11.13(毛髪中心AF)
				DDV.DDX(bUpdate, this.comboBox11, ref m_ss.IMP_AUT_AFMD[2]);//透過(中心)
				DDV.DDX(bUpdate, this.comboBox12, ref m_ss.IMP_AUT_AFMD[3]);//反射(中心)
#endif
#if true//2019.02.03(WB調整)
				DDV.DDX(bUpdate, this.numericUpDown1, ref m_ss.IMP_AUT_SOFS[0]);//透過(表面)
				DDV.DDX(bUpdate, this.numericUpDown2, ref m_ss.IMP_AUT_SOFS[1]);//反射(表面)
				DDV.DDX(bUpdate, this.numericUpDown3, ref m_ss.IMP_AUT_COFS[0]);//透過(中心)
				DDV.DDX(bUpdate, this.numericUpDown4, ref m_ss.IMP_AUT_COFS[1]);//反射(中心)
#endif
				}
#if true//2018.08.21
				//DDV.DDX(bUpdate, this.checkBox3, ref G.SS.MOZ_IRC_CK00);
				//DDV.DDX(bUpdate, this.checkBox4, ref G.SS.MOZ_IRC_CK01);
				//DDV.DDX(bUpdate, this.checkBox5, ref G.SS.MOZ_IRC_CK02);
				//DDV.DDX(bUpdate, this.checkBox6, ref G.SS.MOZ_IRC_CK03);
				//DDV.DDX(bUpdate, this.comboBox11, ref G.SS.MOZ_IRC_DISP);
#endif
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

		private void Form34_Validating(object sender, CancelEventArgs e)
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
#if true//2019.03.02(直線近似)
		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			this.numericUpDown18.Enabled = !this.checkBox1.Checked;
			this.numericUpDown37.Enabled = !this.checkBox2.Checked;
			this.numericUpDown56.Enabled = !this.checkBox3.Checked;
		}
#endif
	}
}
