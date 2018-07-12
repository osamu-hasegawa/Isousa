using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//---
using System.Threading;
using System.Collections;
using System.IO;

namespace uSCOPE
{
	public partial class Form12 : Form
	{

		private bool m_bENTER_GETD = false;
		private bool m_bENTER_PARA = false;

		private struct FDATA
		{
			public DateTime dt;
			public int pos;
			public double s, l;
			public double c, p;
			public double contrast;
		};
		private ArrayList m_fdat = new ArrayList();
		private int[] m_pos = null;
		private int m_idx;
		private int[] m_bsla = { 0, 0, 0, 0 };
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
		private bool[] m_pre_set = {false, false, false, false };
		private int[] m_pre_pos = { 0, 0, 0, 0 };
#endif
		private int m_didx;
		private int m_dcur;
		private string m_path;
		private FDATA m_dat = new FDATA();
		private int m_pmin, m_pmax, m_pstp, m_lms;
		private int m_diss, m_dism, m_disl;
		private int m_fcnt;
		public double m_contrast;
		private DlgProgress
					m_prg = null;//new DlgProgress();
		private int	m_tic;
		private int	m_icam;
		private int FCS_STS;
		public  int AUT_STS;
		private int CAL_STS;
		//private int MOK_STS;
		private int SPE_COD=0;
		public Form12()
		{
			InitializeComponent();
		}
		public void SET_UIF_USER()
		{
		#if true//2018.04.26
			//---
			this.tabControl4.TabPages.Remove(this.tabPage5);//毛髪
			this.tabControl4.TabPages.Remove(this.tabPage6);//AF
			this.tabControl4.TabPages.Remove(this.tabPage2);//2値化
			this.tabControl4.TabPages.Remove(this.tabPage8);//CUTI.1
			this.tabControl4.TabPages.Remove(this.tabPage3);//CUTI.2
			//---[メイン]
			var lc1 = this.button11.Location;//左下
			var lc2 = this.button12.Location;//右下
			var lc3 = this.button26.Location;//右上
			var lc4 = this.button27.Location;//左上
			//this.button11.Text = "黒髪";
			//this.button11.Location = lc4;
			this.button26.Visible = true;
			//this.button26.Location = lc3;
			//this.button12.Text = "画像表示";
			//this.button12.Location = lc2;
			this.button27.Visible = true;
			//this.button27.Location = lc1;
#if false//2018.07.10
			this.checkBox11.Visible = true;//深度合成
#endif
			//---[パラメータ]
			this.button3.Visible = false;	//透過用
			this.button16.Visible = false;	//反射用
			this.button4.Visible = false;	//赤外用
			//---[ヒストグラム]
			m_bENTER_GETD = true;//GETDが呼ばれないように
			this.comboBox1.Items.Clear();
			this.comboBox1.Items.Add("矩形範囲");
			this.comboBox1.SelectedIndex = 0;
			G.SS.CAM_HIS_PAR1 = 1;//1:矩形範囲
			this.comboBox2.Items.Clear();
			this.comboBox2.Items.Add("生画像");
			this.comboBox2.SelectedIndex = 0;
			m_bENTER_GETD = false;//元に戻す
			//---		
		#endif
		#if false//2018.04.23
			//---
			this.trackBar2.Visible = false;
			this.numericUpDown2.Visible = false;
			this.label42.Visible = false;
			//---
			this.trackBar4.Visible = false;
			this.numericUpDown4.Visible = false;
			this.label43.Visible = false;
			//---
			this.checkBox1.Visible = false;//コントラスト値
			this.checkBox2.Visible = false;//最大・最小
			//---
			this.numericUpDown18.Visible = false;
			this.numericUpDown7.Visible = false;
			this.label47.Visible = false;
			this.label7.Visible = false;
			//---
			this.numericUpDown19.Visible = false;
			this.numericUpDown8.Visible = false;
			this.label48.Visible = false;
			this.label8.Visible = false;
			//---
			//---
			this.numericUpDown20.Visible = false;
			this.numericUpDown9.Visible = false;
			this.label49.Visible = false;
			this.label9.Visible = false;
			this.checkBox5.Visible = false;//特徴値
			//---
			this.tabControl4.TabPages.Remove(this.tabPage6);//AFページ
			//---
			this.panel1.Visible = false;
			this.panel2.Visible = false;
			this.radioButton1.Visible = false;
			this.radioButton2.Visible = false;
			this.label40.Visible = false;
			this.numericUpDown5.Visible = false;//閾値
			this.numericUpDown21.Visible = false;//H.MAX
			this.numericUpDown22.Visible = false;//H.MIN
			this.numericUpDown23.Visible = false;//S.MAX
			this.numericUpDown24.Visible = false;//S.MIN
			this.numericUpDown25.Visible = false;//V.MAX
			this.numericUpDown26.Visible = false;//V.MIN
			this.label10.Visible = false;
			this.label11.Visible = false;
			this.label12.Visible = false;
			this.label13.Visible = false;
			this.label14.Visible = false;
			this.label15.Visible = false;
			//---
			//---
			//---
		#endif
		}
		private void Form12_Load(object sender, EventArgs e)
		{
			//---
			{
				//this.chart1.Series[0].Points.Clear();
				//this.chart1.Series[0].Color = Color.Red;
				//for (int i = 0; i < 256; i++) {
				//    this.chart1.Series[0].Points.AddY(0);
				//}
				//this.chart1.ChartAreas[0].AxisY.Maximum = double.NaN;
			}
			if (!G.SS.ETC_UIF_CUTI) {
				this.tabControl4.TabPages.Remove(this.tabPage3);//CUTI.2 ページ
				this.tabControl4.TabPages.Remove(this.tabPage8);//CUTI.1 ページ
			}
			//init();
			GETDAT(true);
			UPDSTS();
			//---
			//if (G.SS.CAM_PAR_EXMOD == 0) {
			//    this.radioButton3.Checked = true;
			//}
			//else {
			//    this.radioButton4.Checked = true;
			//}
			//if (G.SS.CAM_PAR_WBMOD == 0) {
			//    this.radioButton5.Checked = true;
			//}
			//else {
			//    this.radioButton6.Checked = true;
			//}
			for (int i = 0; i < this.tabControl4.TabCount; i++) {
				this.tabControl4.TabPages[i].BackColor = G.SS.ETC_BAK_COLOR;
			}
		}
		private void OnClicks(object sender, EventArgs e)
		{
			G.CAM_STS PRC_BAK = G.CAM_PRC;
this.SPE_COD = 0;
			if (!GETDAT(false)) {
				return;
			}
			if (false) {
			}
			else if (sender == this.button1) {
				//OPEN
				if (G.FORM02 == null) {
					G.FORM02 = new Form02();
					G.FORM02.Show();
					CAM_INIT();
					if (G.FORM02.isCONNECTED()) {
						if (this.radioButton3.Checked) {
							G.FORM02.set_auto(Form02.CAM_PARAM.GAIN, 1);
						}
						else {
							G.FORM02.set_auto(Form02.CAM_PARAM.GAIN, 0);
						}
						if (this.radioButton7.Checked) {
							G.FORM02.set_auto(Form02.CAM_PARAM.EXPOSURE, 1);
						}
						else {
							G.FORM02.set_auto(Form02.CAM_PARAM.EXPOSURE, 0);
						}
						if (this.radioButton5.Checked) {
							G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, 1);
						}
						else {
							G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, 0);
						}
						if (true) {
							//校正実行
							this.CAL_STS = 1;
							timer3.Enabled = true;
						}
					}
				}
			}
			else if (sender == this.button11/*黒*/|| sender == this.button26/*白*/ || sender == this.button27/*全*/) {
				if (G.UIF_LEVL == 0) {
					string path;
					path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					path += @"\KOP";
					path += @"\" + Application.ProductName;
					G.SS.PLM_AUT_FOLD = path;
					//G.SS.PLM_AUT_ZDCK = this.checkBox11.Checked;
					//3:透過→赤外, 8:反射→赤外
#if true//2018.06.04 赤外同時測定
					Form22 frm = new Form22();
					if(frm.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) {
						return;
					}
#endif
					if (sender == this.button11/*自動*/) {
						G.SS.PLM_AUT_MODE = 8;
						G.SS.PLM_AUT_RTRY = true;
					}
					else if (sender == this.button26/*白*/) {
						G.SS.PLM_AUT_MODE = 3;
						G.SS.PLM_AUT_RTRY = false;
					}
					else if (sender == this.button27/*黒*/){
						G.SS.PLM_AUT_MODE = 8;
						G.SS.PLM_AUT_RTRY = false;
					}
					//自動測定実行
					do_auto_mes(false);
				}
				else {
					do_auto_mes(true);
				}
			}
			//else if (sender == this.button12) {
			//    do_mouk_mes();
			//}
			else if (sender == this.button2) {
				//CLOSE
				if (G.FORM02 != null) {
					G.FORM02.Dispose();
					G.FORM02 = null;
				}
			}
			else if (sender == this.button3) {
				set_param_auto(0);//白色LED用(透過)
			}
			else if (sender == this.button4) {
				set_param_auto(2);//赤外LED用
			}
			else if (sender == this.button16) {
				set_param_auto(1);//白色LED用(反射)
			}
			else if (sender == this.button19) {
				set_imp_param(0, 1|2);//1=領域抽出,白色LED用(透過)
			}
			else if (sender == this.button18) {
				set_imp_param(1, 1|2);//1=領域抽出,白色LED用(反射)
			}
			else if (sender == this.button17) {
				set_imp_param(2, 1|2);//1=領域抽出,赤外LED用
			}
			else if (sender == this.button22) {
				set_imp_param(0, 1|2);//2=2値化,白色LED用(透過)
			}
			else if (sender == this.button21) {
				set_imp_param(1, 1|2);//2=2値化,白色LED用(反射)
			}
			else if (sender == this.button20) {
				set_imp_param(2, 1|2);//2=2値化,赤外LED用
			}
			else if (sender == this.button5) {
				//ヒストグラム・実行
				G.CNT_MOD = G.SS.CAM_HIS_PAR1;
				G.CAM_PRC = G.CAM_STS.STS_HIST;
			}
			else if (sender == this.button7) {
				//円形度・実行
				G.CAM_PRC = G.CAM_STS.STS_HAIR;
			}
			else if (sender == this.button9) {
				//フォーカス・実行
				G.CNT_MOD = G.SS.CAM_HIS_PAR1;
				G.CAM_PRC =  G.CAM_STS.STS_FCUS;
				this.FCS_STS = 1;
				this.timer1.Tag = null;
				this.timer1.Enabled = true;
			}
			else if (sender == this.button25) {
				//キューティクル・実行
				G.CAM_PRC =  G.CAM_STS.STS_CUTI;
			}
			else if (sender == this.button6 || sender == this.button8 || sender == this.button10 || sender == this.button24) {
				//停止
				G.CAM_PRC = 0;
			}
			else if (sender == this.radioButton3) {
				G.FORM02.set_auto(Form02.CAM_PARAM.GAIN, 1);
			}
			else if (sender == this.radioButton4) {
				G.FORM02.set_auto(Form02.CAM_PARAM.GAIN, 0);
			}
			else if (sender == this.button14) {
				G.FORM02.set_auto(Form02.CAM_PARAM.GAIN, 2);
				this.radioButton4.Checked = true;
			}
			else if (sender == this.radioButton7) {
				G.FORM02.set_auto(Form02.CAM_PARAM.EXPOSURE, 1);
			}
			else if (sender == this.radioButton8) {
				G.FORM02.set_auto(Form02.CAM_PARAM.EXPOSURE, 0);
			}
			else if (sender == this.button23) {
				G.FORM02.set_auto(Form02.CAM_PARAM.EXPOSURE, 2);
				this.radioButton8.Checked = true;
			}
			else if (sender == this.radioButton5) {
				G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, 1);
			}
			else if (sender == this.radioButton6) {
				G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, 0);
			}
			else if (sender == this.button15) {
				G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, 2);
				this.radioButton6.Checked = true;
			}
			if (this.FCS_STS != 0 && G.CAM_PRC != G.CAM_STS.STS_FCUS) {
				this.FCS_STS = 0;
			}
			if (G.CAM_PRC != PRC_BAK) {
				if (G.FORM02 != null) {
					G.FORM02.set_layout();
				}
			}
			//---------------------------
			UPDSTS();
			if (G.FORM02 != null && G.FORM02.isLOADED()) {
				G.FORM02.UPDATE_PROC();
			}
		}
		// bUpdate=true:画面更新/false:変数取込
		private bool GETDAT(bool bUpdate)
		{
			bool rc = false;
			if (m_bENTER_GETD) {
				return(false);
			}
			m_bENTER_GETD = true;
			try {
				if (G.UIF_LEVL == 0) {
				G.SS.CAM_HIS_PAR1 = 1;//1:矩形範囲
				}
				else {
				DDV.DDX(bUpdate, this.comboBox1, ref G.SS.CAM_HIS_PAR1);
				}
				DDV.DDX(bUpdate, this.comboBox7, ref G.SS.CAM_HIS_METH);
				DDV.DDX(bUpdate, this.comboBox8, ref G.SS.CAM_HIS_OIMG);

				DDV.DDX(bUpdate, this.numericUpDown5, ref G.SS.CAM_HIS_BVAL);//, 1, 254);
				if (G.UIF_LEVL == 0) {
				G.SS.CAM_HIS_DISP = 0;//0:生画像
				}
				else {
				DDV.DDX(bUpdate, this.comboBox2, ref G.SS.CAM_HIS_DISP);
				}
				DDV.DDX(bUpdate, this.checkBox1, ref G.SS.CAM_HIS_CHK1);
				DDV.DDX(bUpdate, this.checkBox2, ref G.SS.CAM_HIS_CHK2);
				//---
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton1, this.radioButton2 }, ref G.SS.CAM_CND_MODH);
				DDV.DDX(bUpdate, this.numericUpDown21, ref G.SS.CAM_CND_MINH);
				DDV.DDX(bUpdate, this.numericUpDown22, ref G.SS.CAM_CND_MAXH);
				DDV.DDX(bUpdate, this.numericUpDown23, ref G.SS.CAM_CND_MINS);
				DDV.DDX(bUpdate, this.numericUpDown24, ref G.SS.CAM_CND_MAXS);
				DDV.DDX(bUpdate, this.numericUpDown25, ref G.SS.CAM_CND_MINV);
				DDV.DDX(bUpdate, this.numericUpDown26, ref G.SS.CAM_CND_MAXV);
				//---
				DDV.DDX(bUpdate, this.comboBox3, ref G.SS.CAM_CIR_FILT);
				//DDV.DDX(bUpdate, this.numericUpDown6, ref G.SS.CAM_CIR_BVAL, 1, 254);
				DDV.DDX(bUpdate, this.numericUpDown7, ref G.SS.CAM_CIR_AREA);//, 1000, 2500000);
				DDV.DDX(bUpdate, this.numericUpDown8, ref G.SS.CAM_CIR_LENG);//, 1, 10000);
				DDV.DDX(bUpdate, this.numericUpDown9, ref G.SS.CAM_CIR_CVAL);//, 0.0, 1);
				DDV.DDX(bUpdate, this.numericUpDown15, ref G.SS.CAM_DIR_PREC);//, 5, 100);

				DDV.DDX(bUpdate, this.numericUpDown18, ref G.SS.CAM_CIR_AREA_MAX);//, 1000, 2500000);
				DDV.DDX(bUpdate, this.numericUpDown19, ref G.SS.CAM_CIR_LENG_MAX);//, 1, 100000);
				DDV.DDX(bUpdate, this.numericUpDown20, ref G.SS.CAM_CIR_CVAL_MIN);//, 0.0, 1);
				DDV.DDX(bUpdate, this.numericUpDown27, ref G.SS.CAM_CIR_MAGN);

				DDV.DDX(bUpdate, this.comboBox4, ref G.SS.CAM_CIR_DISP);
				DDV.DDX(bUpdate, this.checkBox3, ref G.SS.CAM_CIR_CHK1);
				DDV.DDX(bUpdate, this.checkBox4, ref G.SS.CAM_CIR_CHK2);
				DDV.DDX(bUpdate, this.checkBox5, ref G.SS.CAM_CIR_CHK3);
				DDV.DDX(bUpdate, this.checkBox6, ref G.SS.CAM_CIR_CHK4);
				//---
				DDV.DDX(bUpdate, this.numericUpDown10, ref G.SS.CAM_FCS_LMIN);
				DDV.DDX(bUpdate, this.numericUpDown11, ref G.SS.CAM_FCS_LMAX);
				DDV.DDX(bUpdate, this.numericUpDown12, ref G.SS.CAM_FCS_DISL);
				DDV.DDX(bUpdate, this.numericUpDown13, ref G.SS.CAM_FCS_DISM);
				DDV.DDX(bUpdate, this.numericUpDown14, ref G.SS.CAM_FCS_DISS);
				DDV.DDX(bUpdate, this.numericUpDown16, ref G.SS.CAM_FCS_SKIP);
				DDV.DDX(bUpdate, this.numericUpDown17, ref G.SS.CAM_FCS_FAVG);
				DDV.DDX(bUpdate, this.comboBox5, ref G.SS.CAM_FCS_PAR1);
				DDV.DDX(bUpdate, this.comboBox6, ref G.SS.CAM_FCS_DISP);
				//DDV.DDX(bUpdate, this.checkBox7, ref G.SS.CAM_FCS_CHK1);
				DDV.DDX(bUpdate, this.checkBox8, ref G.SS.CAM_FCS_CHK2);
				DDV.DDX(bUpdate, this.checkBox7, ref G.SS.CAM_FCS_USSD);
				//---
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton10, this.radioButton9 }, ref G.SS.TST_PAR_GAUS);
				DDV.DDX(bUpdate, this.numericUpDown28, ref G.SS.TST_PAR_VAL1);//半径1(カーネルサイズ)
				DDV.DDX(bUpdate, this.numericUpDown29, ref G.SS.TST_PAR_VAL2);//半径2(カーネルサイズ)
				DDV.DDX(bUpdate, this.numericUpDown30, ref G.SS.TST_PAR_VAL3);//カーネルサイズ
				DDV.DDX(bUpdate, this.numericUpDown31, ref G.SS.TST_PAR_DBL1);//σ1
				DDV.DDX(bUpdate, this.numericUpDown32, ref G.SS.TST_PAR_DBL2);//σ2
				DDV.DDX(bUpdate, this.numericUpDown33, ref G.SS.TST_PAR_VAL4);//二値化閾値
				DDV.DDX(bUpdate, this.numericUpDown37, ref G.SS.TST_PAR_PREC);//二値化閾値
				DDV.DDX(bUpdate, this.comboBox9, ref G.SS.TST_PAR_DISP);		//
				DDV.DDX(bUpdate, this.checkBox9, ref G.SS.TST_PAR_CHK1);		//輪郭
				DDV.DDX(bUpdate, this.checkBox10, ref G.SS.TST_PAR_CHK2);		//多曲線近似・精度
				DDV.DDX(bUpdate, this.checkBox12, ref G.SS.TST_PAR_CHK3);		//特徴値
				DDV.DDX(bUpdate, this.comboBox10, ref G.SS.TST_PAR_ORDR);		//処理手順
				DDV.DDX(bUpdate, this.numericUpDown34, ref G.SS.TST_PAR_EROD);	//収縮
				DDV.DDX(bUpdate, this.numericUpDown35, ref G.SS.TST_PAR_DILA);	//膨張
				DDV.DDX(bUpdate, this.numericUpDown36, ref G.SS.TST_PAR_THIN);	//細線
				DDV.DDX(bUpdate, this.numericUpDown43, ref G.SS.TST_PAR_SMIN);	//面積:MIN
				DDV.DDX(bUpdate, this.numericUpDown41, ref G.SS.TST_PAR_SMAX);	//面積:MAX
				DDV.DDX(bUpdate, this.numericUpDown42, ref G.SS.TST_PAR_LMIN);	//周囲長:MIN
				DDV.DDX(bUpdate, this.numericUpDown40, ref G.SS.TST_PAR_LMAX);	//周囲長:MAX
#if false//2018.07.10
				if (G.UIF_LEVL == 0) {
				DDV.DDX(bUpdate, this.checkBox11, ref G.SS.PLM_AUT_ZDCK);
				}
#endif
				//---
				rc = true;
			}
			catch (Exception e) {
				G.mlog(e.Message);
				rc = false;
			}
			m_bENTER_GETD = false;
			return (rc);
		}
		public void UPDSTS()
		{
			if (G.FORM02 == null || G.FORM03 != null) {// || !G.FORM02.isCONNECTED()) {
				this.button1.Enabled = true;//open
				this.button1.Enabled = (G.FORM03 == null);//open
				this.button2.Enabled = false;//close
				//this.button3.Enabled = false;//white.para
				//this.button4.Enabled = false;//ir.para
				this.button5.Enabled = false;//his.exec
				this.button6.Enabled = false;
				this.button7.Enabled = false;//hair.exec
				this.button8.Enabled = false;
				this.button9.Enabled = false;//af.exec
				this.button10.Enabled = false;
				this.button11.Enabled = false;//auto.mes
				this.button26.Enabled = false;
				this.button27.Enabled = false;
				this.checkBox11.Enabled = false;//深度合成
				//---
				this.radioButton3.Enabled = false;
				this.radioButton4.Enabled = false;
				this.button14.Enabled = false;
				this.radioButton7.Enabled = false;
				this.radioButton8.Enabled = false;
				this.button23.Enabled = false;
				this.radioButton5.Enabled = false;
				this.radioButton6.Enabled = false;
				this.button15.Enabled = false;
			}
			else if (!G.FORM02.isCONNECTED() && !G.FORM02.isLOADED()) {
				this.button1.Enabled = false;//open
				this.button2.Enabled = true;//close
			}
			else {
				this.button1.Enabled = false;//open
				this.button2.Enabled = true;//close
				if (G.CAM_PRC != G.CAM_STS.STS_HIST) {
					this.button5.Enabled = true;
					this.button6.Enabled = false;
				}
				else {
					this.button5.Enabled = false;
					this.button6.Enabled = true;
				}
				if (G.CAM_PRC != G.CAM_STS.STS_HAIR) {
					this.button7.Enabled = true;
					this.button8.Enabled = false;
				}
				else {
					this.button7.Enabled = false;
					this.button8.Enabled = true;
				}
				if (G.CAM_PRC !=  G.CAM_STS.STS_FCUS) {
					this.button9.Enabled = true;
					this.button10.Enabled = false;
				}
				else {
					this.button9.Enabled = false;
					this.button10.Enabled = true;
				}
				//
				if (G.FORM02.isCONNECTED() && D.isCONNECTED() && G.FORM11.isORG_ALL_DONE()) {
					this.button11.Enabled = true;
					this.button26.Enabled = true;
					this.button27.Enabled = true;
					this.checkBox11.Enabled = true;//深度合成
				}
				else {
					this.button11.Enabled = false;
					this.button26.Enabled = false;
					this.button27.Enabled = false;
					this.checkBox11.Enabled = false;//深度合成
				}
				if (G.FORM02.isCONNECTED()) {
					this.radioButton3.Enabled = true;
					this.radioButton4.Enabled = true;
					this.button14.Enabled = true;
					this.radioButton7.Enabled = true;
					this.radioButton8.Enabled = true;
					this.button23.Enabled = true;
					this.radioButton5.Enabled = true;
					this.radioButton6.Enabled = true;
					this.button15.Enabled = true;
				}
			}

		}

		public void CALLBACK()
		{
			if (false) {
			}
			else if (G.CAM_PRC == G.CAM_STS.STS_HIST) {
				//hist
			}
			else if (G.CAM_PRC ==  G.CAM_STS.STS_HAIR) {
				//circle
			}
			else if (G.CAM_PRC ==  G.CAM_STS.STS_FCUS) {
				//focus
			}
			m_didx++;
		}
		private void CAM_INIT()
		{
			if (G.FORM02 == null) {
				return;
			}
			if (G.FORM02.Text.Contains("OFFLINE")) {
				return;
			}
#if false
			double fval, fmax, fmin;
			//---
			G.FORM02.get_param(Form02.CAM_PARAM.GAMMA, out fval, out fmax, out fmin);
			this.numericUpDown1.Value = (decimal)fval;
			//this.trackBar1.Maximum = (int)(fmax * 100);
			//this.trackBar1.Minimum = (int)(fmin * 100);
			this.trackBar1.Value = (int)(fval * 100);
			//---
			G.FORM02.get_param(Form02.CAM_PARAM.CONTR, out fval, out fmax, out fmin);
			this.numericUpDown2.Value = (decimal)fval;
			//this.trackBar2.Maximum = (int)(fmax * 100);
			//this.trackBar2.Minimum = (int)(fmin * 100);
			this.trackBar2.Value = (int)(fval * 100);
			//---
			G.FORM02.get_param(Form02.CAM_PARAM.BRIGH, out fval, out fmax, out fmin);
			this.numericUpDown3.Value = (decimal)fval;
			//this.trackBar3.Maximum = (int)(fmax * 100);
			//this.trackBar3.Minimum = (int)(fmin * 100);
			this.trackBar3.Value = (int)(fval * 100);
			//---
			G.FORM02.get_param(Form02.CAM_PARAM.SHARP, out fval, out fmax, out fmin);
			this.numericUpDown4.Value = (decimal)fval;
			//this.trackBar4.Maximum = (int)(fmax * 100);
			//this.trackBar4.Minimum = (int)(fmin * 100);
			this.trackBar4.Value = (int)(fval * 100);
			//---
#endif
		}
		public void set_param_auto(int ch)
		{
			//ch=0:白色LED用(透過), ch=1:白色LED用(反射), ch=2:赤外LED用
			set_param(G.SS.CAM_PAR_GAMMA[ch], G.SS.CAM_PAR_CONTR[ch], G.SS.CAM_PAR_BRIGH[ch], G.SS.CAM_PAR_SHARP[ch]);
			//
			set_param_gew(ch);
		}
		//Gamma					0.25- 2.0
		//Sharpness Enhancement	0.0 - 1.0
		//Contrast Enhancement	0.0 - 1.0
		//Target Brightness		0.1 - 1.0

		private void set_param(double f1, double f2, double f3, double f4)
		{
			//---
			if (!double.IsNaN(f1)) {
				this.numericUpDown1.Value = (decimal)f1;
				this.trackBar1.Value = (int)(f1 * 100 + 0.5);
				if (G.FORM02 != null && G.FORM02.isCONNECTED()) {
					G.FORM02.set_param(Form02.CAM_PARAM.GAMMA, f1);
				}
			}
			//---
			if (!double.IsNaN(f2)) {
				this.numericUpDown2.Value = (decimal)f2;
				this.trackBar2.Value = (int)(f2 * 100 + 0.5);
				if (G.FORM02 != null && G.FORM02.isCONNECTED()) {
					G.FORM02.set_param(Form02.CAM_PARAM.CONTR, f2);
				}
			}
			//---
			if (!double.IsNaN(f3)) {
				this.numericUpDown3.Value = (decimal)f3;
				this.trackBar3.Value = (int)(f3 * 100 + 0.5);
				if (G.FORM02 != null && G.FORM02.isCONNECTED()) {
					G.FORM02.set_param(Form02.CAM_PARAM.BRIGH, f3);
				}
			}
			//---
			if (!double.IsNaN(f4)) {
				this.numericUpDown4.Value = (decimal)f4;
				this.trackBar4.Value = (int)(f4 * 100 + 0.5);
				if (G.FORM02 != null && G.FORM02.isCONNECTED()) {
					G.FORM02.set_param(Form02.CAM_PARAM.SHARP, f4);
				}
			}
		}
		public void set_param_gew(int ch)
		{
			if (G.FORM02 == null || G.FORM02.isCONNECTED() == false) {
				return;
			}
			if (G.SS.CAM_PAR_GAMOD[ch] == 1) {
				//自動
				G.FORM02.set_auto(Form02.CAM_PARAM.GAIN, 1);
				this.radioButton3.Checked = true;//自動
			}
			else {
				//固定
				G.FORM02.set_auto(Form02.CAM_PARAM.GAIN, 0);
#if true//2018.06.04 赤外同時測定
				G.FORM02.set_param(Form02.CAM_PARAM.GAIN, G.SS.CAM_PAR_GA_VL[ch] + G.SS.CAM_PAR_GA_OF[ch]);
#else
				G.FORM02.set_param(Form02.CAM_PARAM.GAIN, G.SS.CAM_PAR_GA_VL[ch]);
#endif
				this.radioButton4.Checked = true;//固定
			}
			if (G.SS.CAM_PAR_EXMOD[ch] == 1) {
				//自動
				G.FORM02.set_auto(Form02.CAM_PARAM.EXPOSURE, 1);
				this.radioButton7.Checked = true;//自動
			}
			else {
				//固定
				G.FORM02.set_auto(Form02.CAM_PARAM.EXPOSURE, 0);
#if true//2018.06.04 赤外同時測定
				G.FORM02.set_param(Form02.CAM_PARAM.EXPOSURE, G.SS.CAM_PAR_EX_VL[ch] + G.SS.CAM_PAR_EX_OF[ch]);
#else
				G.FORM02.set_param(Form02.CAM_PARAM.EXPOSURE, G.SS.CAM_PAR_EX_VL[ch]);
#endif
				this.radioButton8.Checked = true;//固定
			}
			if (G.SS.CAM_PAR_WBMOD[ch] == 1) {
				//自動
				G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, 1);
				//---
				this.radioButton5.Checked = true;//自動
			}
			else {
				//固定
				G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, 0);
				G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 0);
				G.FORM02.set_param(Form02.CAM_PARAM.BALANCE, G.SS.CAM_PAR_WB_RV[ch]);
				G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 1);
				G.FORM02.set_param(Form02.CAM_PARAM.BALANCE, G.SS.CAM_PAR_WB_GV[ch]);
				G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 2);
				G.FORM02.set_param(Form02.CAM_PARAM.BALANCE, G.SS.CAM_PAR_WB_BV[ch]);
				//---
				this.radioButton6.Checked = true;//固定
			}
		}
		private void trackBar1_ValueChanged(object sender, EventArgs e)
		{
			if (m_bENTER_PARA) {
				return;
			}
			m_bENTER_PARA = true;

			double f1 = double.NaN,
					f2 = double.NaN,
					f3 = double.NaN,
					f4 = double.NaN;
			if (sender == this.trackBar1) {
				f1 = (trackBar1.Value/100.0);
			}
			else if (sender == this.trackBar2) {
				f2 = (trackBar2.Value/100.0);
			}
			else if (sender == this.trackBar3) {
				f3 = (trackBar3.Value/100.0);
			}
			else if (sender == this.trackBar4) {
				f4 = (trackBar4.Value/100.0);
			}
			set_param(f1, f2, f3, f4);

			m_bENTER_PARA = false;
		}
		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			if (m_bENTER_PARA) {
				return;
			}
			m_bENTER_PARA = true;

			double f1 = double.NaN,
					f2 = double.NaN,
					f3 = double.NaN,
					f4 = double.NaN;
			if (false) {
			}
			else if (sender == this.numericUpDown1) {
				f1 = (double)this.numericUpDown1.Value;
			}
			else if (sender == this.numericUpDown2) {
				f2 = (double)this.numericUpDown2.Value;
			}
			else if (sender == this.numericUpDown3) {
				f3 = (double)this.numericUpDown3.Value;
			}
			else if (sender == this.numericUpDown4) {
				f4 = (double)this.numericUpDown4.Value;
			}
			set_param(f1, f2, f3, f4);
			m_bENTER_PARA = false;
		}
		public void set_imp_param(int i, int mask)
		{
			//ch=0:白色LED用(透過), ch=1:白色LED用(反射), ch=2:赤外LED用
			G.set_imp_param(i, mask);
			GETDAT(true);//画面更新
			OnControlStateChanged(null, null);
		}

		private void OnControlStateChanged(object sender, EventArgs e)
		{
			if (m_bENTER_GETD) {
				return;
			}
			GETDAT(false);//変数取込
			if (G.FORM02 != null && (G.FORM02.isLOADED() || G.FORM02.isCONNECTED())) {
if (G.CAM_PRC == G.CAM_STS.STS_HIST) {
	G.CNT_MOD = G.SS.CAM_HIS_PAR1;
}
				G.FORM02.UPDATE_PROC();
			}
		}
		private void f_write(string path)
		{
			StreamWriter wr;
			try {
				/*				rd = new StreamReader(filename, Encoding.GetEncoding("Shift_JIS"));*/
				wr = new StreamWriter(path, true, Encoding.Default);
				wr.WriteLine("DATE,POS,S,L,C,P,CONTRAST");
				wr.Close();
			}
			catch (Exception) {
			}
		}
		private void f_write(string path, FDATA dat)
		{
			string buf;
			StreamWriter wr;
			try {
				/*				rd = new StreamReader(filename, Encoding.GetEncoding("Shift_JIS"));*/
				wr = new StreamWriter(path, true, Encoding.Default);
				buf = string.Format("{0}", dat.dt.ToString());
				buf += string.Format(",{0}", dat.pos);
				buf += string.Format(",{0:F0}", dat.s);
				buf += string.Format(",{0:F0}", dat.l);
				buf += string.Format(",{0:F2}", dat.c);
				buf += string.Format(",{0:F0}", dat.p);
				buf += string.Format(",{0:F3}", dat.contrast);
				wr.WriteLine(buf);
				wr.Close();
			}
			catch (Exception) {
			}
		}
		private void MOVE_ABS(int q, int pos)
		{
			if (G.PLM_POS[q] != pos) {
				if (G.PLM_POS[q] > pos && G.SS.PLM_BSLA[q] > 0) {
					m_bsla[q] = G.SS.PLM_BSLA[q];
				}
#if true//2018.05.22(バックラッシュ方向反転対応)
				else
				if (G.PLM_POS[q] < pos && G.SS.PLM_BSLA[q] < 0) {
					m_bsla[q] = G.SS.PLM_BSLA[q];
				}
#endif
				else {
					m_bsla[q] = 0;
				}
				D.SET_STG_ABS(q, pos - m_bsla[q]);
				G.PLM_STS |= (1 << q);
			}
		}
		private void MOVE_ABS_Z(int pos)
		{
			MOVE_ABS(2, pos + G.SS.PLM_OFFS[2]);//FOCUS/Z軸
		}
		private void MOVE_REL_Z(int dif)
		{
			MOVE_ABS(2, G.PLM_POS[2] + dif);//FOCUS/Z軸
		}
		private int[] calc_pos(int min, int max, int stp)
		{
			var ar = new ArrayList();
			int pos;
#if true//2018.05.22(バックラッシュ方向反転対応)
			bool brev = false;
			if (min > max) {
				int tmp = min;
				min = max;
				max = tmp;
				brev = true;
			}
#endif	
			ar.Add(min);

			if ((min % stp) != 0) {
				pos = (min / stp) * stp;	//(-2500/800)*800= -2400, (2500/800)*800=2400
				if (pos < min) {
					pos += stp;
				}
			}
			else {
				pos = min + stp;
			}
			while (pos < max) {
				ar.Add(pos);
				pos += stp;
			}
			ar.Add(max);
#if true//2018.05.22(バックラッシュ方向反転対応)
			if (brev) {
				ar.Reverse();
			}
#endif
			return ((int[])ar.ToArray(typeof(int)));
		}

		private double get_val(ArrayList ar, int i)
		{
			FDATA fdat = (FDATA)ar[i];
			double	f;
			switch (G.SS.CAM_FCS_PAR1) {
			case 0:
				f =  fdat.contrast;
				break;
			case 1:
				f = fdat.s;
				break;
			case 2:
				f = fdat.l;
				break;
			default:
				f = fdat.p;
				break;
			}
			return(f);
		}
		private void get_max(ArrayList ar, out int imax, out double fmax)
		{
			fmax = get_val(m_fdat, 0);
			imax = 0;
			if (this.SPE_COD != 0) {
				double fmin = fmax;
				int	imin = 0;
				for (int i = 1; i < ar.Count; i++) {
					double f = get_val(ar, i);
					if (fmin > f || double.IsNaN(fmin)) {
						fmin = f;
						imin = i;
					}
				}
				fmax = fmin;
				imax = imin;
			}
			else {
				for (int i = 1; i < ar.Count; i++) {
					double f = get_val(ar, i);
					if (fmax < f || double.IsNaN(fmax)) {
						fmax = f;
						imax = i;
					}
				}
			}
		}
		int m_tic1;
		// オートフォーカス(AFページ / 自動測定)
		private void timer1_Tick(object sender, EventArgs e)
		{
			int NXT_STS = this.FCS_STS+1;
			double fmax;
			int imax;

			this.timer1.Enabled = false;

			switch (this.FCS_STS) {
			case 0:
				break;
			case 1:
				if (G.SS.CAM_FCS_PAR1 == 0/*CONTRAST*/ && G.CNT_MOD >= 2/*毛髪矩形 or 毛髪範囲*/) {
					/*画像全体
					矩形範囲
					毛髪矩形+0%
					毛髪矩形+25%
					毛髪矩形+50%
					毛髪矩形+100%
					毛髪範囲10%
					毛髪範囲25%
					毛髪範囲50%
					毛髪範囲75%
					毛髪範囲100%
					毛髪範囲100%
					毛髪範囲10% (横1/3)
					毛髪範囲10% (横1/4)
					毛髪範囲10% (横1/5)
					 */
					G.CAM_PRC = G.CAM_STS.STS_HIST;
				}
				else {
					NXT_STS = 11;
				}
				break;
			case 2:
				m_dcur = m_didx;
				break;
			case 3:
				if ((m_didx - m_dcur) < G.SS.CAM_FCS_SKIP) {
					NXT_STS = this.FCS_STS;
				}
				else if (G.IR.CIR_CNT <= 0) {
						this.FCS_STS = 0;
						timer1.Enabled = false;
						G.mlog("#e毛髪が検出できませんした.");
				}
				else {
					G.CAM_PRC = G.CAM_STS.STS_FCUS;
					NXT_STS = 11;
				}
			break;
			case 10:
				break;
			case 11://大ステップによる探索範囲
				m_tic1 = Environment.TickCount;
				if (G.SS.CAM_FCS_CHK2) {
					DateTime dt = DateTime.Now;
					m_path = T.GetDocFolder();
					m_path += "\\";
					m_path += string.Format("{0:0000}{1:00}{2:00}-{3:00}{4:00}{5:00}",
							dt.Year, dt.Month, dt.Day,
							dt.Hour, dt.Minute, dt.Second);
					m_path += ".csv";
					f_write(m_path);
				}

				if (this.timer1.Tag == null) {
					//カメラTABより
					m_diss = G.SS.CAM_FCS_DISS;
					m_dism = G.SS.CAM_FCS_DISM;
					m_disl = G.SS.CAM_FCS_DISL;
				}
				else if ((int)this.timer1.Tag == 1) {
					//自動測定より(初回)
					m_diss = G.SS.PLM_AUT_DISS;
					m_dism = G.SS.PLM_AUT_DISS;
					m_disl = G.SS.PLM_AUT_DISL;
				}
				else if ((int)this.timer1.Tag == 2) {
					//自動測定より(２回以降)
					m_diss = G.SS.PLM_AUT_2DSS;
					m_dism = G.SS.PLM_AUT_2DSS;
					m_disl = G.SS.PLM_AUT_2DSL;
				}
				else {
					//自動測定より(フォーカス位置探索用)
					m_diss = G.SS.PLM_AUT_HPSS;
					m_dism = G.SS.PLM_AUT_HPSS;
					m_disl = G.SS.PLM_AUT_HPSL;
				}
				if (this.timer1.Tag == null) {
					m_pmin = G.SS.CAM_FCS_LMIN;
					m_pmax = G.SS.CAM_FCS_LMAX;
				}
				else if ((int)this.timer1.Tag == 1) {
					int tmp = G.PLM_POS[2] - G.SS.PLM_OFFS[2];
					m_pmin = tmp - G.SS.PLM_AUT_HANI;
					m_pmax = tmp + G.SS.PLM_AUT_HANI;
				}
				else if ((int)this.timer1.Tag == 2) {
					int tmp = G.PLM_POS[2] - G.SS.PLM_OFFS[2];
					m_pmin = tmp - G.SS.PLM_AUT_2HAN;
					m_pmax = tmp + G.SS.PLM_AUT_2HAN;
				}
				else {
					m_pmin = G.SS.PLM_AUT_HPMN;
					m_pmax = G.SS.PLM_AUT_HPMX;
				}
#if true//2018.05.22(バックラッシュ方向反転対応)
				if (G.SS.PLM_BSLA[2] < 0) {
					if (m_pmin < m_pmax) {
						int tmp = m_pmin;
						m_pmin = m_pmax;
						m_pmax = tmp;
					}
				}
#endif
				m_pstp = m_disl;
				m_lms = 0;
				break;
			case 12:
				m_pos = calc_pos(m_pmin, m_pmax, m_pstp);
				m_idx = 0;
				m_fdat.Clear();
				break;
			case 13:
				//f軸-> pos
				MOVE_ABS_Z(m_pos[m_idx]);
				NXT_STS = -this.FCS_STS;
				break;
			case 14:
				m_dcur = m_didx;
				break;
			case 15:
				if ((m_didx - m_dcur) < G.SS.CAM_FCS_SKIP) {
					NXT_STS = this.FCS_STS;
				}
				else {
					m_fcnt = 0;
				}
			break;
			case 16:
				//測定
				if (m_fcnt == 0) {
					m_dcur = m_didx;
					m_dat.dt = DateTime.Now;
					m_dat.pos = m_pos[m_idx];
					m_dat.s = G.IR.CIR_S;
					m_dat.l = G.IR.CIR_L;
					m_dat.c = G.IR.CIR_C;
					m_dat.p = G.IR.CIR_P;
					m_dat.contrast = G.IR.CONTRAST;
					m_fcnt++;
				}
				else if (m_didx == m_dcur) {
					//NXT_STS = this.FCS_STS;
				}
				else {
					m_dcur = m_didx;
					m_dat.s += G.IR.CIR_S;
					m_dat.l += G.IR.CIR_L;
					m_dat.c += G.IR.CIR_C;
					m_dat.p += G.IR.CIR_P;
					m_dat.contrast += G.IR.CONTRAST;
					m_fcnt++;
				}
				if (m_fcnt >= G.SS.CAM_FCS_FAVG) {
					m_dat.s /= G.SS.CAM_FCS_FAVG;
					m_dat.l /= G.SS.CAM_FCS_FAVG;
					m_dat.c /= G.SS.CAM_FCS_FAVG;
					m_dat.p /= G.SS.CAM_FCS_FAVG;
					m_dat.contrast /= G.SS.CAM_FCS_FAVG;
					m_fdat.Add(m_dat);
					if (G.SS.CAM_FCS_CHK2) {
						f_write(m_path, m_dat);
					}
				}
				else {
					NXT_STS = this.FCS_STS;
				}
				break;
			case 17:
				if (++m_idx < m_pos.Length) {
					NXT_STS = 13;
				}
				break;
			case 18:
				m_lms++;
				if (m_lms >= 3) {
					NXT_STS = 20;// end
				}
				else if (m_lms == 1) {
				    //中ステップによる探索範囲
					if (m_dism < m_disl) {
						m_pstp = m_dism;
				    }
				    else {
				        NXT_STS = 18;
				    }
				}
				else {
					//小ステップによる探索範囲
					if (m_diss < m_dism) {
						m_pstp = m_diss;
					}
					else {
						NXT_STS = 18;
					}
				}
				break;
			case 19:
				// 最大値の範囲確認
				get_max(m_fdat, out imax, out fmax);
				if (double.IsNaN(fmax)) {
					NXT_STS = 21;
				}
				else if (imax == 0) {
					imax = 1;
					m_pmin = ((FDATA)m_fdat[imax - 1]).pos;
					m_pmax = ((FDATA)m_fdat[imax - 0]).pos;
				}
				else if (imax == (m_fdat.Count-1)) {
					imax = m_fdat.Count - 1;
					m_pmin = ((FDATA)m_fdat[imax - 1]).pos;
					m_pmax = ((FDATA)m_fdat[imax - 0]).pos;
				}
				else if (get_val(m_fdat, imax - 1) > get_val(m_fdat, imax + 1)) {
					m_pmin = ((FDATA)m_fdat[imax - 1]).pos;
					m_pmax = ((FDATA)m_fdat[imax - 0]).pos;
				}
				else {
					m_pmin = ((FDATA)m_fdat[imax - 0]).pos;
					m_pmax = ((FDATA)m_fdat[imax + 1]).pos;
				}
				if (NXT_STS != 21) {
					NXT_STS = 12;
				}
				break;
			case 20:
				//最大値位置へ移動
				get_max(m_fdat, out imax, out fmax);
				MOVE_ABS_Z(((FDATA)m_fdat[imax]).pos);
				NXT_STS = -this.FCS_STS;
				break;
			case 21:
				get_max(m_fdat, out imax, out fmax);
				int ela = Environment.TickCount- m_tic1;
				int	posz = ((FDATA)m_fdat[imax]).pos;

				m_contrast = ((FDATA)m_fdat[imax]).contrast;
				if (((FDATA)m_fdat[imax]).pos != G.PLM_POS[2]) {
					imax = imax;
				}
				if (false) {
					NXT_STS = 1;
				}
				else {
					G.CAM_PRC = G.CAM_STS.STS_NONE;
					this.FCS_STS = 0;
					timer1.Enabled = false;
				}
				if (false) {
					double	f1, f2, f3;
					G.FORM02.get_param(Form02.CAM_PARAM.EXPOSURE, out f1, out f2, out f3);
					StreamWriter wr = new StreamWriter("d:\\temp\\log.txt", true, Encoding.Default);
					wr.WriteLine("CAM,SKIP,AVG,ETM,CONT,POSZ={0}x{1}/{2}ms	{3}	{4}	{5:F1}	{6:F3}	{7}", G.CAM_WID, G.CAM_HEI, f1 / 1000, G.SS.CAM_FCS_SKIP, G.SS.CAM_FCS_FAVG, (double)ela / 1000.0, m_contrast, posz);
					wr.Close();
					if (this.timer1.Tag == null) {
						for (int i = 0; i < 0; i++) {
							Console.Beep(1600, 250);
							Thread.Sleep(250);
						}
						//Thread.Sleep(3000);
					}
				}
				UPDSTS();
				
				break;
			default:
				//f軸停止待ち
				if ((G.PLM_STS & (1 << 2)) == 0) {
					if (m_bsla[2] != 0) {
						Thread.Sleep(1000/G.SS.PLM_LSPD[2]);//2018.05.21
						//バックラッシュ対応
						MOVE_REL_Z(m_bsla[2]);
						m_bsla[2] = 0;
						NXT_STS = this.FCS_STS;
					}
					else {
						NXT_STS = (-this.FCS_STS) + 1;
					}
				}
				else {
					NXT_STS = this.FCS_STS;
				}
				break;
			}
			if (NXT_STS == 0) {
				NXT_STS = 0;//for break.point
			}
			if (true) {
				if (this.FCS_STS != 0) {
					this.FCS_STS = NXT_STS;
					this.timer1.Interval = 1;
					this.timer1.Enabled = true;
				}
			}
			else {
				if (timer1.Enabled) {
					this.FCS_STS = NXT_STS;
				}
			}
		}
		/*
		 * iTag:1(AF初回), 2:(AF2回目以降), 3:(毛髪径によるAF...位置合わせ用)
		 */
		public void start_af(int iTag)
		{
			m_adat.chk3 = 0;
			if (iTag == 2 && G.SS.PLM_AUT_2FST) {
				if (G.FORM02.get_size_mode() <= 1) {
					int	xo = -1, yo = -1;
					if (G.IR.CIR_CNT > 0) {
						xo = G.IR.CIR_RT.Left + G.IR.CIR_RT.Width/2;
						yo = G.IR.CIR_RT.Top + G.IR.CIR_RT.Height/2;
					}
					G.FORM02.set_size_mode(2, xo, yo);
					m_adat.chk3 = 1;
				}
			}
			if (iTag == 3) {
				G.SS.CAM_FCS_PAR1 = 2;//毛髪径最大化によるAF
			}
			else if (G.CNT_MOD == 0 || m_adat.chk3 == 1) {
				//Contrast全体
				G.SS.CAM_FCS_PAR1 = 0;
			}
			else {
				//Contrast矩形
				G.SS.CAM_FCS_PAR1 = 0;
				G.FORM02.set_mask_by_result();
			}
			//---
			this.timer1.Tag = iTag;
			G.CAM_PRC = G.CAM_STS.STS_FCUS;
			this.FCS_STS = 1;
			this.timer1.Enabled = true;
		}
		public void do_auto_mes(bool bShowDialog)
		{
			if (bShowDialog) {
				Form20	frm = new Form20();
				if(frm.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) {
					return;
				}
			}
			DlgProgress
					prg = new DlgProgress();
			int bak_of_mode = G.SS.PLM_AUT_MODE;

			m_adat.trace = false;
			m_adat.retry = false;

			try {
				if (G.FORM02.get_size_mode() > 1) {
					G.FORM02.set_size_mode(1, -1, -1);
				}
				prg.Show("自動撮影", G.FORM01);
				prg.SetStatus("実行中...");
#if false//2018.05.17
				G.CNT_MOD = (G.SS.PLM_AUT_AFMD==0) ? 0: 1+G.SS.PLM_AUT_AFMD;
#endif
				G.CAM_PRC = G.CAM_STS.STS_AUTO;
				this.AUT_STS = 1;
				timer2.Enabled = true;
				while (timer2.Enabled) {
					Application.DoEvents();
					string buf;
					//bool bWAIT = false;
					buf = "";
					if ((this.AUT_STS >= 1 && this.AUT_STS <= 2) || this.AUT_STS == -1) {
						buf += "...\r\r";
						prg.SetStatus(buf);
						continue;
					}
					if ((this.AUT_STS >=  70 && this.AUT_STS <=  72)
					 || (this.AUT_STS >= 100 && this.AUT_STS <= 102)
					 || (this.AUT_STS >= 120 && this.AUT_STS == 122)
					 || (this.AUT_STS >= 140 && this.AUT_STS == 142)) {
						buf += "待機中";
						prg.SetStatus(buf);
						continue;
					}
					if (this.AUT_STS >= 998) {
						buf += "測定終了...\r\r";
						prg.SetStatus(buf);
						continue;
					}
					if ((G.LED_PWR_STS & 1) != 0) {
						buf += "透過:";
					}
					else if ((G.LED_PWR_STS & 2) != 0) {
						buf += "反射:";
					}
					else if ((G.LED_PWR_STS & 4) != 0) {
						buf += "位相差: ";
					}
					//if ((this.AUT_STS >= 140 && this.AUT_STS <= 141)/* || (this.AUT_STS >= 56 && this.AUT_STS <= 58)*/) {
					//    buf += "\r\r";
					//    bWAIT = true;
					//}
					if ((m_adat.h_idx + 1) == 2) {
						buf = buf;
					}
					if (false) {
					}
					else if (this.AUT_STS >= 5 && this.AUT_STS <= 6) {
						buf += "AF位置探索\r\r";
					}
					else if (m_adat.trace == false) {
						buf += string.Format("毛髪 {0}本目\r\r", m_adat.h_idx + 1);
					}
					else {
						buf += string.Format("毛髪 {0}/{1}本目\r\r", m_adat.h_idx + 1, m_adat.h_cnt);
					}
					if (false/*bWAIT*/) {
						buf += "待機中";
					}
					else if (this.FCS_STS != 0) {
						buf += "フォーカス";
					}
					else if (this.AUT_STS < 20) {
						buf += "探索中";
					}
					else if (m_adat.trace == false) {
						buf += (m_adat.f_idx <= 50) ? "左側" : "右側";
					}
					else {
						buf += string.Format("{0}/{1}", 1+m_adat.f_idx, m_adat.f_cnt[m_adat.h_idx]);
					}
					prg.SetStatus(buf);
				}
			}
			catch (Exception ex) {
				G.mlog(ex.Message);
			}
			prg.Hide();
			prg.Dispose();
			prg = null;
			//---
			if (G.FORM02.get_size_mode() > 1) {
				G.FORM02.set_size_mode(1, -1, -1);
			}
			G.SS.PLM_AUT_MODE = bak_of_mode;//リトライ時に書き変わるため元に戻す
		}

		public void set_expo_mode(int n)
		{
			G.FORM02.set_auto(Form02.CAM_PARAM.GAIN, n);
			G.FORM02.set_auto(Form02.CAM_PARAM.EXPOSURE, n);
			G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, n);
			if (n == 1) {
				this.radioButton3.Checked = true;
				this.radioButton7.Checked = true;
				this.radioButton5.Checked = true;
			}
			else {
				//G.mlog("固定時にはオフセット演算を追加する");
				this.radioButton4.Checked = true;
				this.radioButton8.Checked = true;
				this.radioButton6.Checked = true;
			}
		}
#if true//2018.06.04 赤外同時測定
		public void set_expo_const()
		{
			double fval, fmin, fmax;
			int		ch;
			if ((G.LED_PWR_STS & 1) != 0) {
				ch = 0;		//透過
			}
			else if ((G.LED_PWR_STS & 2) != 0) {
				ch = 1;		//反射
			}
			else {
				ch = 2;		//赤外
			}
			if (G.CAM_GAI_STS == 1) {
				G.FORM02.set_auto(Form02.CAM_PARAM.GAIN, /*固定*/0);
				if (G.SS.CAM_PAR_GA_OF[ch] != 0) {
				G.FORM02.get_param(Form02.CAM_PARAM.GAIN, out fval, out fmax, out fmin);
				G.FORM02.set_param(Form02.CAM_PARAM.GAIN, fval + G.SS.CAM_PAR_GA_OF[ch]);
				}
			}
			if (G.CAM_EXP_STS == 1) {
				G.FORM02.set_auto(Form02.CAM_PARAM.EXPOSURE, /*固定*/0);
				if (G.SS.CAM_PAR_EX_OF[ch] != 0) {
				G.FORM02.get_param(Form02.CAM_PARAM.EXPOSURE, out fval, out fmax, out fmin);
				G.FORM02.set_param(Form02.CAM_PARAM.EXPOSURE, fval + G.SS.CAM_PAR_EX_OF[ch]);
				}
			}
			if (G.CAM_WBL_STS == 1) {
				G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, /*固定*/0);
			}
			if (true) {
				//G.mlog("固定時にはオフセット演算を追加する");
				this.radioButton4.Checked = true;
				this.radioButton8.Checked = true;
				this.radioButton6.Checked = true;
				this.radioButton4.Update();
				this.radioButton8.Update();
				this.radioButton6.Update();
			}
		}
#endif
		private void MOVE_PIX_XY(int x, int y)
		{
			double xum = G.FORM02.PX2UM(x);
			double yum = G.FORM02.PX2UM(y);
			if (G.PX2UM(x) != xum || G.PX2UM(y) != yum) {
				G.mlog("internal error");
			}
			int	xpl = (int)(xum / G.SS.PLM_UMPP[0]);
			int ypl = (int)(yum / G.SS.PLM_UMPP[1]);
			MOVE_REL_XY(xpl, ypl);
		}
		private void MOVE_REL_XY(int x, int y)
		{
			MOVE_ABS_XY(G.PLM_POS[0] + x, G.PLM_POS[1] + y);
		}
		private void MOVE_ABS_XY(int x, int y)
		{
			MOVE_ABS(0, x);
			MOVE_ABS(1, y);
		}
		private class ADATA
		{
//			public DateTime dt;
			public string log;
			public string fold;
			public string pref;
			public string ext;
			public bool trace;
			public int org_pos_x;
			public int org_pos_y;
			public int org_pos_z;
			public int sta_pos_x;
			public int sta_pos_y;
			public int sta_pos_z;
			public double sta_contrast;
			public ArrayList pos_x;
			public ArrayList pos_y;
			public ArrayList pos_z;
			public ArrayList f_nam;
			public ArrayList f_dum;
			public int[] f_cnt;
			//public int[] z_pls;
			public int f_ttl;
			public int h_idx;
			public int h_cnt;
			public int f_idx;
			public int r_idx;
			public int chk1, chk2;
			public int sts_bak;
			public int chk3;
			public int led_sts;
			//---
			public int z_idx;
			public int z_cnt;
			public int z_cur;
			public ArrayList z_nam;
			public ArrayList z_pos;
			//---
			public bool retry;
			public ArrayList y_1st_pos;
			//---
			public int ir_nxst;
			public bool ir_done;
			public int ir_lsbk;
			public int ir_chk1;
			//---
			public ADATA()
			{
//				dt = DateTime.Now;
				fold = "";
				ext = "";
				pref = "";
				retry = false;
				org_pos_x = 0;
				org_pos_y = 0;
				org_pos_z = 0;
				sta_pos_x = 0;
				sta_pos_y = 0;
				sta_pos_z = 0;
				sta_contrast = 0;
				pos_x = new ArrayList();
				pos_y = new ArrayList();
				pos_z = new ArrayList();
				f_nam = new ArrayList();
				f_dum = new ArrayList();
				f_cnt = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				//z_pls = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				f_ttl = 0;
				h_idx = 0;
				h_cnt = 0;
				f_idx = 50;
				r_idx = 0;
				chk1 = 0;
				chk2 = 0;
				sts_bak = 0;
				//---
				z_idx = 0;
				z_cnt = 1;
				z_cur = 0;
				z_nam = new ArrayList();
				z_pos = new ArrayList();
				y_1st_pos = new ArrayList();
				ir_nxst = 0;
				ir_done = false;
				ir_lsbk = 0;
			}
		};
		private ADATA m_adat = new ADATA();
		private string FLTP2STR(int n)
		{
			string buf;
			switch (n) {
			case 0:
				buf= "BMP";
				break;
			case 1:
				buf= "PNG";
				break;
			default:
				buf= "JPG";
				break;
			}
			return(buf);
		}
		private void a_write()
		{
			string path = m_adat.log;
			CSV csv = new CSV();
			string[] hd = {
				"DATE",
				"TITLE",
				"MODE",
				"読み捨てフレーム",
				"計算範囲",
				"探索範囲(±pls)",
				"探索ステップ",
				"探索ステップ大小",
				"ファイル形式",
				"フォルダ",
				"-",
				"FOCUS軸(pls)",
				"ZOOM軸(pls/倍)",
				"ステージピッチ(um/pls)",
				"画素ピッチ(um/pxl)"
			};

			for (int i = 0; i < hd.Length; i++) {
				string buf;
				switch (i) {
				case 0:
					buf = DateTime.Now.ToString();
					break;
				case 1:
					buf = G.SS.PLM_AUT_TITL;
					break;
				case 2:
					/*
						0:白色(透過)
						1:白色(反射)
						2:白色(透過)→赤外
						3:白色(反射)→赤外
					 */
					switch (G.SS.PLM_AUT_MODE) {
					case  0:buf = "透過"; break;
					case  1:buf = "透過→反射"; break;
					case  2:buf = "透過→反射→赤外"; break;
					case  3:buf = "透過→赤外"; break;
					case  4:buf = "透過→赤外→反射"; break;
					case  5:buf = "反射"; break;
					case  6:buf = "反射→透過"; break;
					case  7:buf = "反射→透過→赤外"; break;
					case  8:buf = "反射→赤外"; break;
					case  9:buf = "反射→赤外→透過"; break;
					default:buf = ""; break;
					}
					//buf = G.SS.PLM_AUT_MODE == 0 ? "白色のみ":"白色+赤外";
					break;
				case 3:
					buf = G.SS.PLM_AUT_SKIP.ToString();
					break;
				case 4:
#if false//2018.05.17
					switch (G.SS.PLM_AUT_AFMD) {
					case  0:buf = "画像全体"; break;
					case  1:buf = "毛髪範囲"; break;
					case  2:buf = "毛髪範囲+25%"; break;
					case  3:buf = "毛髪範囲+50%"; break;
					default:buf = "毛髪範囲+100%"; break;
					}
#else
					buf = "";
#endif
				break;
				case 5:
					buf = G.SS.PLM_AUT_HANI.ToString();
				break;
				case 6:
					buf = G.SS.PLM_AUT_DISL.ToString();
				break;
				case 7:
					buf = G.SS.PLM_AUT_DISS.ToString();
				break;
				case 8:
					buf = FLTP2STR(G.SS.PLM_AUT_FLTP);
				break;
				case 9:
					buf = G.SS.PLM_AUT_FOLD;
				break;
				case 11:
					buf = G.PLM_POS[2].ToString();
				break;
				case 12:
					buf = G.PLM_POS[3].ToString();
					buf+="/";
					buf+= string.Format("x{0:F2}", G.SS.ZOM_PLS_A * G.PLM_POS[3] + G.SS.ZOM_PLS_B);
				break;
				case 13:
					buf = G.SS.PLM_UMPP[0].ToString();
				break;
				case 14:
					buf = G.SS.CAM_SPE_UMPPX.ToString();
				break;
				default:
					buf = "";
					break;
				}
				csv.set(0, i, hd[i]);
				csv.set(1, i, buf);
			}
			csv.save(path);
			try {
				StreamWriter wr;
				/*				rd = new StreamReader(filename, Encoding.GetEncoding("Shift_JIS"));*/
				wr = new StreamWriter(path, true, Encoding.Default);
				wr.WriteLine("-");
				wr.WriteLine("TIME,X(pls),Y(pls),Z(pls),STATUS,,CONTRAST,S,L,C,P,,X(px),Y(px),W(px),H(px)");
				wr.Close();
			}
			catch (Exception) {
			}
		}
		private string DN(int n, int d)
		{
			string buf = n.ToString();
			int	tar = d-buf.Length;
			if (tar > 0) {
				for (int i = 0; i < tar; i++) {
					buf = " " + buf;
				}
			}
			return (buf);
		}
		private void a_write(string sts)
		{
			string path = m_adat.log;
			string buf;
			StreamWriter wr;
			try {
				wr = new StreamWriter(path, true, Encoding.Default);
				buf = string.Format("{0}", DateTime.Now.ToLongTimeString());
				buf += string.Format(",{0}", DN(G.PLM_POS[0], 5));
				buf += string.Format(",{0}", DN(G.PLM_POS[1], 5));
				buf += string.Format(",{0}", DN(G.PLM_POS[2], 4));
				buf += ",";
				buf += sts;
				buf += ",";
				if (G.IR.CIR_CNT <= 0 || sts.Contains("移動")) {
					buf += ",,,,,";
				}
				else {
					buf += string.Format(",{0:F3}", G.IR.CONTRAST);
					buf += string.Format(",{0:F0}", G.IR.CIR_S);
					buf += string.Format(",{0:F0}", G.IR.CIR_L);
					buf += string.Format(",{0:F2}", G.IR.CIR_C);
					buf += string.Format(",{0:F0}", G.IR.CIR_P);
				}
				buf += ",";
				if (G.IR.CIR_CNT <= 0 || sts.Contains("移動")) {
					buf += ",,,,";
				}
				else {
					buf += string.Format(",{0:F0}", G.IR.CIR_RT.Left);
					buf += string.Format(",{0:F0}", G.IR.CIR_RT.Top);
					buf += string.Format(",{0:F0}", G.IR.CIR_RT.Width);
					buf += string.Format(",{0:F0}", G.IR.CIR_RT.Height);
				}
				wr.WriteLine(buf);
				wr.Close();
			}
			catch (Exception) {
			}
		}
		private string get_aut_path(int f_idx)
		{
			//FOLDER_PATH\\Z10_0CR_00.PNG
			string path = m_adat.fold;
			path += "\\";
			path = "";
			path += string.Format("{0}", m_adat.h_idx);
			path +=  m_adat.pref;
			if (f_idx >= 0) {
			path += string.Format("_{0:00}", m_adat.f_idx);
			}
			else {
			path += "_@@";
			}
			path += "_";
			path += m_adat.z_nam[m_adat.z_idx];
			path += ".";
			path += m_adat.ext;

			return(path);
		}
		//---
		private string to_ir_file(string fold, string path)
		{
			string name = System.IO.Path.GetFileName(path);

			if (name.Contains("CT")) {
				name = name.Replace("CT", "IR");
			}
			else {
				name = name.Replace("CR", "IR");
			}
			if (string.IsNullOrEmpty(fold)) {
				return(name);
			}
			return(fold+"\\"+name);
		}

		private void rename_aut_files()
		{
			string buf = System.IO.File.ReadAllText(m_adat.log, Encoding.Default);
					
			//rename処理
			int cnt = m_adat.f_cnt[m_adat.h_idx];
			for (int i = 0; i < cnt; i++) {
				string path_old = (string)m_adat.f_dum[i];
				string path_new = (string)m_adat.f_nam[i];
				path_new = path_new.Replace("@@", string.Format("{0:00}", i));
				System.IO.File.Move(path_old, path_new);
#if true//2018.06.04 赤外同時測定
				if (G.SS.PLM_AUT_IRCK) {
					string path_old_ir = to_ir_file(m_adat.fold, path_old);
					string path_new_ir = to_ir_file(m_adat.fold, path_new);
					System.IO.File.Move(path_old_ir, path_new_ir);
				}
#endif
				if (true) {
					int	idx = path_old.LastIndexOf('\\');
					string src = path_old.Substring(idx+1);
					string dst = path_new.Substring(idx+1);
					//---
					idx = src.LastIndexOf('.');
					src = src.Substring(0, idx);
					dst = dst.Substring(0, idx);
					buf = buf.Replace(src, dst);
#if true//2018.06.04 赤外同時測定
					if (G.SS.PLM_AUT_IRCK) {
						string src_ir = to_ir_file(null, src);
						string dst_ir = to_ir_file(null, dst);
						buf = buf.Replace(src_ir, dst_ir);
					}
#endif
					if (m_adat.z_cnt > 1) {
						for (int q = 1; q < m_adat.z_cnt; q++) {
							string tmp = (string)m_adat.z_nam[q];
							string name_old = src.Replace("ZP00D", tmp);
							string name_new = dst.Replace("ZP00D", tmp);
							//---
							buf = buf.Replace(name_old, name_new);
#if true//2018.06.04 赤外同時測定
							if (G.SS.PLM_AUT_IRCK) {
								string src_ir = to_ir_file(null, name_old);
								string dst_ir = to_ir_file(null, name_new);
								buf = buf.Replace(src_ir, dst_ir);
							}
#endif
						}
					}
				}
				if (true) {
					if (m_adat.z_cnt > 1) {
						for (int q = 1; q < m_adat.z_cnt; q++) {
							string tmp = (string)m_adat.z_nam[q];
							string name_old = path_old.Replace("ZP00D", tmp);
							string name_new = path_new.Replace("ZP00D", tmp);
							//---
							System.IO.File.Move(name_old, name_new);
#if true//2018.06.04 赤外同時測定
							if (G.SS.PLM_AUT_IRCK) {
								string path_old_ir = to_ir_file(m_adat.fold, name_old);
								string path_new_ir = to_ir_file(m_adat.fold, name_new);
								System.IO.File.Move(path_old_ir, path_new_ir);
							}
#endif
						}
					}
				}
			}
			System.IO.File.WriteAllText(m_adat.log, buf, Encoding.Default);
			m_adat.f_dum.Clear();
			m_adat.f_nam.Clear();
		}

		private int retry_check(int sts)
		{
			if (G.SS.PLM_AUT_RTRY) {
				if (G.SS.PLM_AUT_MODE == 5 || G.SS.PLM_AUT_MODE == 8) {
					//5:反射
					//8:反射→赤外
					//反射の未検出域に対して透過にてリトライする
					G.SS.PLM_AUT_MODE -= 5;
					//0:透過
					//3:透過→赤外
					sts = 1;
					m_adat.retry = true;
				}
			}
			return(sts);
		}
		private bool retry_ypos_check(int ycur, out int ynxt)
		{
			ynxt = ycur;
			double hei;
			
			if (m_adat.retry == false) {
				return(false);
			}
			hei = G.CAM_HEI;				//px
			hei = G.PX2UM(hei);				//um
			hei = hei / G.SS.PLM_UMPP[1];	//pls
			for (int i = 0; i < m_adat.y_1st_pos.Count; i++) {
				int ypos = (int)m_adat.y_1st_pos[i];
				int ydif = ycur-ypos;
				if (Math.Abs(ydif) < hei) {
					ynxt = (int)(ypos+hei+0.5);
					return(true);
				}
			}
			//int ret;
			////ret = G.CAM_HEI * G.SS.CAM_SPE_UMPPX
			//MOVE_PIX_XY(0, (int)(G.CAM_HEI * (1 - G.SS.PLM_AUT_OVLP / 100.0)));

			return(false);
		}
		private int m_retry_cnt_of_hpos;
		// 自動測定
		private void timer2_Tick(object sender, EventArgs e)
		{
			int NXT_STS = this.AUT_STS + 1;
			int yy, y0, ypos;

			if (G.bCANCEL) {
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				this.AUT_STS = 0;
				G.bCANCEL = false;
			}
			switch (this.AUT_STS) {
			case 0:
				this.timer2.Enabled = false;
				break;
			case 1://中上へ
				/*
					0:透過
					1:透過→反射
					2:透過→反射→赤外
					3:透過→赤外
					4:透過→赤外→反射
					5:反射
					6:反射→透過
					7:反射→透過→赤外
					8:反射→赤外
					9:反射→赤外→透過*/
				//---
				if (false) {
				}
				else if ((G.SS.PLM_AUT_MODE >= 0 && G.SS.PLM_AUT_MODE <= 4) && (G.LED_PWR_STS & 1) == 0) {
					//光源=>白色(透過)
					NXT_STS = 70;//70->71->1として白色点灯->安定待機後に戻ってくる
				}
				else if ((G.SS.PLM_AUT_MODE >= 5 && G.SS.PLM_AUT_MODE <= 9) && (G.LED_PWR_STS & 2) == 0) {
					//光源=>白色(反射)
					NXT_STS = 70;//70->71->1として白色点灯->安定待機後に戻ってくる
				}/*
				if ((G.LED_PWR_STS & 1) == 0 || (G.LED_PWR_STS & 2) != 0) {
					//光源=>白色
					NXT_STS = 70;//70->71->1として白色点灯->安定待機後に戻ってくる
				}*/
				else {
#if true//2018.05.17
					if ((G.LED_PWR_STS & 1) != 0) {
						//白色(透過)
						G.CNT_MOD = (G.SS.IMP_AUT_AFMD[0]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[0];
					}
					else {
						//白色(反射)
						G.CNT_MOD = (G.SS.IMP_AUT_AFMD[1]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[1];
					}
#endif
					if (m_adat.retry == false) {
						DateTime dt = DateTime.Now;
						string buf = "";
						buf = string.Format("{0:0000}{1:00}{2:00}_{3:00}{4:00}{5:00}",
										dt.Year,
										dt.Month,
										dt.Day,
										dt.Hour,
										dt.Minute,
										dt.Second);
						if (!string.IsNullOrEmpty(G.SS.PLM_AUT_TITL)) {
							buf = G.SS.PLM_AUT_TITL + "_" + buf;
						}
						m_adat.fold = G.SS.PLM_AUT_FOLD;
						if (G.SS.PLM_AUT_FOLD.Last() != '\\') {
							m_adat.fold += "\\";
						}
						m_adat.fold += buf;
						m_adat.ext = FLTP2STR(G.SS.PLM_AUT_FLTP);
					}
					if (G.SS.PLM_AUT_MODE >= 0 && G.SS.PLM_AUT_MODE <= 4) {
						m_adat.pref = "CT";//白色(透過)
					}
					else {
						m_adat.pref = "CR";//白色(反射)
					}
					if (m_adat.retry == false) {
						try {
							System.IO.Directory.CreateDirectory(m_adat.fold);
							G.SS.AUT_BEF_PATH = m_adat.fold;
							m_adat.log = m_adat.fold + "\\log.csv";
							a_write();
						}
						catch (Exception ex) {
							G.mlog(ex.Message);
							G.CAM_PRC = G.CAM_STS.STS_NONE;
							this.AUT_STS = 0;
							break;
						}
					}
				}
#if true//2018.06.04 赤外同時測定
				MOVE_ABS_XY(G.SS.PLM_AUT_HP_X, G.SS.PLM_AUT_HP_Y);
#endif
				//中上
				if (G.SS.PLM_AUT_HPOS) {
#if false//2018.06.04 赤外同時測定
					MOVE_ABS_XY(G.SS.PLM_AUT_HP_X, G.SS.PLM_AUT_HP_Y);
#endif
					if (NXT_STS != 70) {
						m_retry_cnt_of_hpos = 0;
						NXT_STS = -(5 - 1);//->5
						if (G.SS.PLM_AUT_HMOD == 0) {
							this.SPE_COD = 1;
						}
						else {
							this.SPE_COD = 0;
						}
					}
				}
#if false//2018.06.04 赤外同時測定
				else if (G.bJITAN) {
					//for debug
					MOVE_ABS_XY((G.SS.PLM_MLIM[0] + G.SS.PLM_PLIM[0]) / 2, 0);
				}
				else {
					//中上
					MOVE_ABS_XY((G.SS.PLM_MLIM[0] + G.SS.PLM_PLIM[0]) / 2, G.SS.PLM_MLIM[1]);
				}
#endif
#if true//2018.07.10
				if (G.SS.PLM_AUT_HPOS) {
					//AF位置探索
				}
				else
#endif
#if true//2018.07.02
				if (G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/) {
					if (NXT_STS < 0) {
						m_pre_set[2] = true;
						m_pre_pos[2] = G.SS.PLM_AUT_HP_Z;
					}
					else {
						MOVE_ABS_Z(G.SS.PLM_AUT_HP_Z);//FOCUS/Z軸
					}
				}
				else
#endif
				if (G.SS.PLM_AUT_FINI) {
					if (NXT_STS < 0) {
						m_pre_set[2] = true;
						m_pre_pos[2] = G.SS.PLM_POSF[3];
					}
					else {
						MOVE_ABS_Z(G.SS.PLM_POSF[3]);//FOCUS/Z軸
					}
				}
				if (G.SS.PLM_AUT_ZINI) {
					MOVE_ABS(3, G.SS.PLM_POSZ[3]);//ZOOM軸
				}
				if (NXT_STS != 70 && NXT_STS != -4) {
					NXT_STS = -this.AUT_STS;
				}
				break;
			case 2:
				if (m_adat.retry == false) {
					m_adat.h_idx = 0;//毛髪１本目
					m_adat.h_cnt = 0;
					m_adat.org_pos_x = m_adat.org_pos_y = m_adat.org_pos_z = -0x1000000;
					for (int i = 0; i < m_adat.f_cnt.Length; i++) {
						m_adat.f_cnt[i] = 0;
					}
					m_adat.trace = false;
					m_adat.f_ttl = 0;
					m_adat.f_dum.Clear();
					m_adat.f_nam.Clear();
					m_adat.chk1 = 0;
					m_adat.pos_x.Clear();
					m_adat.pos_y.Clear();
					m_adat.pos_z.Clear();
					//---
					m_adat.z_nam.Clear();
					m_adat.z_pos.Clear();
#if true//2018.06.04 赤外同時測定
					m_adat.y_1st_pos.Clear();
#endif
					//---
					if (true) {
						m_adat.z_cnt = 1;
						m_adat.z_idx = 0;
						m_adat.z_nam.Add("ZP00D");
						m_adat.z_pos.Add(0);
					}
					if (G.SS.PLM_AUT_ZDCK && G.SS.PLM_AUT_ZDEP != null && G.SS.PLM_AUT_ZDEP.Length > 0) {
						for (int i = 0; i < G.SS.PLM_AUT_ZDEP.Length; i++) {
							int pos = G.SS.PLM_AUT_ZDEP[i];
					        m_adat.z_cnt++;
							if (pos >= 0) {
					        m_adat.z_nam.Add(string.Format("ZP{0:00}D", +pos));
							}
							else {
					        m_adat.z_nam.Add(string.Format("ZM{0:00}D", -pos));
							}
					        m_adat.z_pos.Add(pos);
					    }
					}
					if (G.SS.PLM_AUT_ZKCK && G.SS.PLM_AUT_ZKEI != null && G.SS.PLM_AUT_ZKEI.Length > 0) {
						for (int i = 0; i < G.SS.PLM_AUT_ZKEI.Length; i++) {
							int pos = G.SS.PLM_AUT_ZKEI[i];
					        m_adat.z_cnt++;
							if (pos >= 0) {
					        m_adat.z_nam.Add(string.Format("ZP{0:00}K", +pos));
							}
							else {
					        m_adat.z_nam.Add(string.Format("ZM{0:00}K", -pos));
							}
					        m_adat.z_pos.Add(pos);
					    }
					}
				}
				NXT_STS = 12;
				break;
			case 5:
				if ((G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					//SOFT.LIMIT(+)
#if true
					if ((NXT_STS = retry_check(NXT_STS)) == 1) {
						break;
					}
#endif
					NXT_STS = 999;
				}
				else if (retry_ypos_check(G.PLM_POS[1], out ypos)) {
					MOVE_REL_XY(0, ypos-G.PLM_POS[1]);
					NXT_STS = -(5 - 1);//->5
				}
				else {
a_write("AF:開始");
					start_af(3);
				}
			break;
			case 6:
				//AF処理(終了待ち)
				if (this.FCS_STS != 0) {
					NXT_STS = this.AUT_STS;
					//m_adat.chk2 = 1;
					//G.mlog("m_adat.chk2参照箇所のチェック");
				}
				else {
a_write("AF:終了");
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					if (G.IR.CIR_CNT <= 0) {
a_write("毛髪判定(AF位置探索):NG");
						m_retry_cnt_of_hpos++;
						if (m_retry_cnt_of_hpos > G.SS.PLM_AUT_HPRT) {
#if true
							if ((NXT_STS = retry_check(NXT_STS)) == 1) {
								break;
							}
#endif
							NXT_STS = 999;
						}
						else {
							MOVE_PIX_XY(0, (int)(G.CAM_HEI * (1 - G.SS.PLM_AUT_OVLP / 100.0)));
							NXT_STS = -(5 - 1);//->5
a_write("移動:下へ");
						}
					}
					else {
a_write("毛髪判定(AF位置探索):OK");
						this.SPE_COD = 0;
						NXT_STS = 2;//OK
					}
				}
			break;
			case 10:
				//a_write("毛髪探索中:LIMIT.CHECK");
				if ((G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					//SOFT.LIMIT(+)
					NXT_STS = 40;
				}
				else if (retry_ypos_check(G.PLM_POS[1], out ypos)) {
					MOVE_REL_XY(0, ypos-G.PLM_POS[1]);
					NXT_STS = -(10 - 1);//->10
				}
				else if ((G.PLM_POS[1]+(G.FORM02.PX2UM(G.CAM_HEI)/ G.SS.PLM_UMPP[1])) >=  G.SS.PLM_PLIM[1]) {
					if (m_adat.sts_bak == 14) {
						MOVE_REL_XY(0, (G.SS.PLM_PLIM[1] - G.PLM_POS[1]+10));
						NXT_STS = -(12 - 1);//->12
					}
					else {
						NXT_STS = 40;
					}
				}
				if (NXT_STS == 40) {
					m_adat.h_cnt = m_adat.h_idx;
#if true
					if ((NXT_STS = retry_check(NXT_STS)) == 1) {
						//反射の未検出域に対して透過にてリトライする
						break;
					}
#endif
#if true//2018.06.04 赤外同時測定
					if (G.SS.PLM_AUT_IRCK) {
						NXT_STS = 998;//開始位置へ移動後に終了
					}
					else
#endif
					//m_adat.trace = true;
					if (m_adat.f_ttl <= 0 || (G.SS.PLM_AUT_MODE == 0 || G.SS.PLM_AUT_MODE == 5)) {
						NXT_STS = 998;//開始位置へ移動後に終了
					}
					else if (G.SS.PLM_AUT_MODE == 1 || G.SS.PLM_AUT_MODE == 2) {
						NXT_STS = 120;//->反射
						m_adat.trace = true;
					}
					else if (G.SS.PLM_AUT_MODE == 6 || G.SS.PLM_AUT_MODE == 7) {
						NXT_STS = 100;//->透過
						m_adat.trace = true;
					}
					else if (G.SS.PLM_AUT_MODE == 3 || G.SS.PLM_AUT_MODE == 4|| G.SS.PLM_AUT_MODE == 8 || G.SS.PLM_AUT_MODE == 9) {
						NXT_STS = 140;//->赤外
						m_adat.trace = true;
					}
				}
				break;
			case 11:
a_write("移動:下へ");
				//画面サイズ分↓へ
				MOVE_PIX_XY(0, (int)(G.CAM_HEI * (1 - G.SS.PLM_AUT_OVLP / 100.0)));

				NXT_STS = -this.AUT_STS;
				break;
			case 12:
			case 22:
			case 32:
			case 112:
			case 132:
			case 152:
				m_dcur = m_didx;
				break;
			case 13:
			case 23:
			case 33:
			case 113:
			case 133:
			case 153:
				if ((m_didx - m_dcur) < G.SS.PLM_AUT_SKIP) {
					NXT_STS = this.AUT_STS;//画面が更新されるまで
				}
				break;
			case 14:
				//測定
				if (G.IR.CIR_CNT <= 0) {
					//毛髪判定NG
a_write("毛髪判定(中心):NG");
					NXT_STS = 10;
				}
				else {
a_write("毛髪判定(中心):OK");
					NXT_STS = NXT_STS;
				}
				break;
			case 15:
			case 25:
			case 35:
				//毛髪エリアの垂直方向センタリング
				bool flag = true;
				yy = G.IR.CIR_RT.Top + G.IR.CIR_RT.Height/2;
				y0 = G.CAM_HEI/2;
				if (m_adat.chk1 != 0) {
					//OK
				}
				else if (Math.Abs(yy-y0) < (G.CAM_HEI/5)) {
					//OK
					double	TR = 0.03 * G.IR.CIR_RT.Height;
					bool bHitT = (G.IR.CIR_RT.Top  - 0) < TR;
					bool bHitB = (G.CAM_HEI - G.IR.CIR_RT.Bottom) <= TR;
					if (bHitT && bHitB) {
						//画像の上端と下端の両方に接している => 毛髪が縦方向?
					}
					else if (bHitT || bHitB) {
						flag = false;
					}
				}
				else if ((yy - y0) > 0 && (G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					yy = yy;//SOFT.LIMIT(+)
				}
				else if ((yy - y0) < 0 && (G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_LMT_M) != 0) {
					yy = yy;//SOFT.LIMIT(-)
				}
				else {
					flag = false;
				}
				if (!flag) {
					int dif = (yy - y0);
					int dst = (int)(G.PLM_POS[1] + (G.FORM02.PX2UM(dif) / G.SS.PLM_UMPP[1]));

					if (dif < 0 && dst <=  G.SS.PLM_MLIM[1]) {
						dif = dif;
					}
					else if (dif > 0 && dst >= G.SS.PLM_PLIM[1]) {
						dif = dif;
					}
					else {
						a_write("センタリング");
						MOVE_PIX_XY(0, dif);
						NXT_STS = -(this.AUT_STS - 3 - 1);
					}
				}
				else {
					flag = flag;
				}
				if (this.AUT_STS == 15 && NXT_STS == 16) {
					for (int i = 0; i < 2; i++) {
						Console.Beep(1600, 250);
						Thread.Sleep(250);
					}
a_write("AF:開始");
					start_af(1/*1:1st*/);
				}
				else if (NXT_STS == (this.AUT_STS + 1)) {
					if (m_adat.chk1 != 0) {
						NXT_STS++;//AF処理をSKIP
					}
					else if (false
					 || (G.SS.PLM_AUT_FCMD == 1)
					 || (G.SS.PLM_AUT_FCMD == 2 && G.IR.CONTRAST <= (m_adat.sta_contrast * (1 - G.SS.PLM_AUT_CTDR / 100.0)))) {
a_write("AF:開始");
						start_af(2/*2:next*/);
					}
					else {
						NXT_STS++;//AF処理をSKIP
					}
				}
				break;
			case 16:
			case 26:
			case 36:
				//AF処理(終了待ち)
				if (this.FCS_STS != 0) {
					NXT_STS = this.AUT_STS;
					m_adat.chk2 = 1;
				}
				else if (m_adat.chk2 == 1) {
					NXT_STS = this.AUT_STS;
					m_adat.chk2 = 0;
					if (m_adat.chk3 == 1) {
						m_adat.chk3 = 0;
						G.FORM02.set_size_mode(1, -1, -1);
					}
					m_dcur = m_didx;
a_write("AF:終了");
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
				}
				else if ((m_didx - m_dcur) < (G.SS.PLM_AUT_SKIP+3)) {
					NXT_STS = this.AUT_STS;
				}
				else {
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					//m_adat.chk1 = Environment.TickCount;
					//m_adat.z_pls[m_adat.h_idx] = G.PLM_POS[2];
				}
				break;
			case 17://初回AF後
			case 27://左側探索
			case 37://右側探索
#if true//2018.06.04 赤外同時測定
				if (G.SS.PLM_AUT_IRCK && m_adat.ir_done) {
					//赤外同時測定の赤外測定後
				}
				else {
#endif
				if (m_adat.z_idx == 0) {
					if (this.AUT_STS == 17) {
						//if ((Environment.TickCount - m_adat.chk1) < 2000) {
						//    //フォーカス軸移動直後のため少し待機
						//    NXT_STS = this.AUT_STS;
						//    break;
						//}
						m_adat.sta_contrast = m_contrast;
						m_adat.sta_pos_x = G.PLM_POS[0];
						m_adat.sta_pos_y = G.PLM_POS[1];
						m_adat.sta_pos_z = G.PLM_POS[2];
						if (m_adat.org_pos_x == -0x1000000) {
							m_adat.org_pos_x = m_adat.sta_pos_x;
							m_adat.org_pos_y = m_adat.sta_pos_y;
							m_adat.org_pos_z = m_adat.sta_pos_z;
						}
						m_adat.f_idx = 50;
						//---
						if (m_adat.retry == false) {
							//反射での毛髪Ｙ位置を保存して、
							//透過のときはこのＹ座標をスキップするようにする
							m_adat.y_1st_pos.Add(G.PLM_POS[1]);
						}
						//--- ONCE
						if (G.SS.PLM_AUT_CNST) {
							if (G.CAM_GAI_STS == 1 || G.CAM_EXP_STS == 1 || G.CAM_WBL_STS == 1) {/*1:自動*/
	#if true//2018.06.04 赤外同時測定
								set_expo_const();
	#else
								set_expo_mode(/*const*/0);
	#endif
							}
						}
					}
					if (true) {
						m_adat.pos_x.Add(G.PLM_POS[0]);
						m_adat.pos_y.Add(G.PLM_POS[1]);
						m_adat.pos_z.Add(G.PLM_POS[2]);
					}
					//
					System.IO.Directory.CreateDirectory(m_adat.fold);
					//
					m_adat.z_cur = G.PLM_POS[2];
				}
				if (true) {
					string path0, path1, path2, path3;
					path0 = get_aut_path(-1);
					path1 = path0.Replace("@@", m_adat.f_idx.ToString());
					//path1 = get_aut_path(m_adat.f_idx);
					path2 = m_adat.fold + "\\" + path1;
					G.FORM02.save_image(path2);
					if (m_adat.z_idx == 0) {
						m_adat.f_dum.Add(path2);
						path3 = m_adat.fold + "\\" + path0;
						m_adat.f_nam.Add(path3);
					}
					a_write(string.Format("画像保存:{0}", path1));
				}
				//画像保存
				Console.Beep(800, 250);
#if true//2018.06.04 赤外同時測定
				}
				if (G.SS.PLM_AUT_IRCK) {
					if (m_adat.ir_done == false) {
						m_adat.ir_nxst = this.AUT_STS;
						m_adat.ir_lsbk = G.LED_PWR_STS;
						m_adat.ir_chk1 = m_adat.chk1;
						NXT_STS = 440;//赤外に切替
						break;
					}
					else {
						//毛髪判定ステータスを元に戻す
						m_adat.chk1 = m_adat.ir_chk1;
					}
				}
#endif
				if (m_adat.z_cnt > 1) {
					if (++m_adat.z_idx >= m_adat.z_cnt) {
						m_adat.z_idx = 0;
						MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
						NXT_STS = -this.AUT_STS;
					}
					else {
						NXT_STS = 200+this.AUT_STS;
						break;
					}
				}
				//---
				m_adat.f_cnt[m_adat.h_idx]++;
				m_adat.f_ttl++;
				break;
			case 18:
				m_adat.f_idx--;
				m_adat.chk1 = 0;
				NXT_STS = 20;
				break;
			case 19:
				break;
			case 20:
				if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_M) != 0) {
					//SOFT.LIMIT(-)
					NXT_STS = 29;
				}
				if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					//SOFT.LIMIT(-)
					NXT_STS = 29;//こっちを通る
				}
				break;
			case 21:
				//画面サイズ分←へ
				//MOVE_PIX_XY((int)(+G.CAM_WID * 0.9), 0);
				MOVE_PIX_XY((int)(+G.CAM_WID * (1 - G.SS.PLM_AUT_OVLP / 100.0)), 0);

				NXT_STS = -this.AUT_STS;
				a_write("移動:左へ");
				break;
			case 24:
				if (G.IR.CIR_CNT <= 0) {
					//毛髪判定NG
					m_adat.chk1 = 1;
					a_write("毛髪判定(左側):NG");
				}
				else {
					a_write("毛髪判定(左側):OK");
				}
				break;
			//case 26:
			//    break;
			case 28:
				m_adat.f_idx--;
				if (m_adat.chk1 != 0) {
					m_adat.chk1 = 0;
					NXT_STS = 29;
				}
				else {
					NXT_STS = 20;
				}
				break;
			case 29:
				if (true) {
					//毛髪左側の位置順序の入れ替え
					int cnt = m_adat.f_cnt[m_adat.h_idx];
					m_adat.pos_x.Reverse(m_adat.pos_x.Count - cnt, cnt);
					m_adat.pos_y.Reverse(m_adat.pos_y.Count - cnt, cnt);
					m_adat.pos_z.Reverse(m_adat.pos_z.Count - cnt, cnt);
					m_adat.f_dum.Reverse(m_adat.f_dum.Count - cnt, cnt);
				}
				//開始位置へ移動後,右側処理
				MOVE_ABS_XY(m_adat.sta_pos_x, m_adat.sta_pos_y);
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
				m_pre_set[2] = true;
				m_pre_pos[2] = m_adat.sta_pos_z;
#else
				MOVE_ABS_Z(m_adat.sta_pos_z);
#endif
				m_adat.f_idx = 51;
				m_adat.chk1 = 0;
				NXT_STS = -(30 - 1);//->30
				break;
			case 30:
				if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					//SOFT.LIMIT(+)
					NXT_STS = 39;
				}
				if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_M) != 0) {
					//SOFT.LIMIT(-)
					NXT_STS = 39;
				}
				break;
			case 31:
				//画面サイズ分→へ
				//MOVE_PIX_XY((int)(-G.CAM_WID * 0.9), 0);
				MOVE_PIX_XY((int)(-G.CAM_WID * (1-G.SS.PLM_AUT_OVLP/100.0)), 0);
				NXT_STS = -this.AUT_STS;
				a_write("移動:右へ");
				break;
			case 34:
				if (G.IR.CIR_CNT <= 0) {
					//毛髪判定NG時
					m_adat.chk1 = 1;
					a_write("毛髪判定(右側):NG");
				}
				else {
					a_write("毛髪判定(右側):OK");
				}
				break;
			case 38:
				m_adat.f_idx++;
				if (m_adat.chk1 != 0) {
					m_adat.chk1 = 0;
					NXT_STS = 39;
				}
				else {
					NXT_STS = 30;
				}
				break;
			case 39:
				//開始位置へ移動後,次の毛髪処理
				MOVE_ABS_XY(m_adat.sta_pos_x, m_adat.sta_pos_y);
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
				m_pre_set[2] = true;
				m_pre_pos[2] = m_adat.sta_pos_z;
#else
				MOVE_ABS_Z(m_adat.sta_pos_z);
#endif
				NXT_STS = -(10 - 1);//->10
				//---
				rename_aut_files();
				//---
				m_adat.h_idx++;
				break;
			case 100:
			case 400://赤外同時測定
				//光源切り替え(->透過)
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, false);//赤外
				G.FORM10.LED_SET(0, true );//透過
				m_adat.pref = "CT";//白色(透過)
				m_adat.chk1 = Environment.TickCount;
a_write("光源切替:->透過");
#if true//2018.06.04 赤外同時測定
				if (this.AUT_STS == 400) {
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					break;
				}
#endif
				G.CAM_PRC = G.CAM_STS.STS_ATIR;
				break;
			case 120:
			case 420://赤外同時測定
				//光源切り替え(->反射)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(2, false);//赤外
				G.FORM10.LED_SET(1, true );//反射
				m_adat.pref = "CR";//白色(反射)
				m_adat.chk1 = Environment.TickCount;
a_write("光源切替:->反射");
#if true//2018.06.04 赤外同時測定
				if (this.AUT_STS == 420) {
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					break;
				}
#endif
				G.CAM_PRC = G.CAM_STS.STS_ATIR;
				break;
			case 140:
			case 440://赤外同時測定
				//光源切り替え(->赤外)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, true );//赤外
				m_adat.pref = "IR";//赤外
				m_adat.chk1 = Environment.TickCount;
a_write("光源切替:->赤外");
				G.CAM_PRC = G.CAM_STS.STS_ATIR;
				break;
			case 71:
			case 101://透過:トレース
			case 121://反射:トレース
			case 141://赤外:トレース
#if true//2018.06.04 赤外同時測定
			case 401://赤外同時測定
			case 421://赤外同時測定
			case 441://赤外同時測定
#endif
//■■■■■■■■■■if (this.AUT_STS == 71 || G.SS.PLM_AUT_EXAT == 1) {
//■■■■■■■■■■		set_expo_mode(/*auto*/1);
//■■■■■■■■■■}
			break;
			case 72:
			case 102://透過:トレース
			case 122://反射:トレース
			case 142://赤外:トレース
#if true//2018.06.04 赤外同時測定
			case 402://赤外同時測定
			case 422://赤外同時測定
			case 442://赤外同時測定
#endif
				//カメラ安定待機
				if ((Environment.TickCount - m_adat.chk1) < (G.SS.ETC_LED_WAIT*1000)) {
					NXT_STS = this.AUT_STS;
				}
				else if (G.SS.PLM_AUT_CNST && this.AUT_STS != 72) {
					if (G.CAM_GAI_STS == 1 || G.CAM_EXP_STS == 1 || G.CAM_WBL_STS == 1) {/*1:自動*/
#if true//2018.06.04 赤外同時測定
							set_expo_const();
#else
							set_expo_mode(/*const*/0);
#endif
					}
				}
#if true//2018.06.04 赤外同時測定
				if (this.AUT_STS == 402 || this.AUT_STS == 422) {
					NXT_STS = m_adat.ir_nxst;
					if (NXT_STS != 17 && NXT_STS != 27 && NXT_STS != 37) {
						NXT_STS = NXT_STS;
					}
				}
#endif
				break;
			case 73:
				NXT_STS = 1;
				break;
			case 103://透過:トレース
			case 123://反射:トレース
			case 143://赤外:トレース
				//m_adat.h_cnt = m_adat.h_idx;
				m_adat.h_idx = 0;
				m_adat.r_idx = 0;
				m_adat.f_idx = 0;
				//MOVE_ABS(2, m_adat.z_pls[0]);
				//NXT_STS = -this.AUT_STS;
				break;
			case 104://透過:トレース
				NXT_STS = 110;
				break;
			case 124://反射:トレース
				NXT_STS = 130;
				break;
			case 144://赤外:トレース
				NXT_STS = 150;
				break;
			case 110://透過:トレース
			case 130://反射:トレース
			case 150://赤外:トレース
				//位置トレース
				if (true) {
					int i = m_adat.r_idx++;
					int x = (int)m_adat.pos_x[i];
					int y = (int)m_adat.pos_y[i];
					int z = (int)m_adat.pos_z[i];
					MOVE_ABS_XY(x, y);
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
					m_pre_set[2] = true;
					m_pre_pos[2] = z;
#else
					MOVE_ABS_Z(z);
#endif
				}
				NXT_STS = -this.AUT_STS;
a_write("次へ移動");
				break;
			case 111://透過:トレース
			case 131://反射:トレース
			case 151://赤外:トレース
				break;
			case 114://透過:トレース
			case 134://反射:トレース
			case 154://赤外:トレース
#if true//2018.06.04 赤外同時測定
			case 443://赤外同時測定
#endif
				if (true) {
					string path0, path1;
					path0 = get_aut_path(m_adat.f_idx);
					path1 = m_adat.fold + "\\" + path0;
//System.Diagnostics.Debug.WriteLine("path0:" + path0);
//System.Diagnostics.Debug.WriteLine("path1:" + path1);
					G.FORM02.save_image(path1);
a_write(string.Format("画像保存:{0}", path0));
				}
#if true//2018.06.04 赤外同時測定
				if (this.AUT_STS == 443) {
					m_adat.ir_done = true;
					if ((m_adat.ir_lsbk & 1)!=0) {
						NXT_STS = 400;//透過に戻す
					}
					else {
						NXT_STS = 420;//反射に戻す
					}
					break;
				}
#endif
				if (m_adat.z_idx == 0) {
					m_adat.z_cur = G.PLM_POS[2];
				}
				if (m_adat.z_cnt > 1) {
					if (++m_adat.z_idx >= m_adat.z_cnt) {
						m_adat.z_idx = 0;
						MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
						NXT_STS = -this.AUT_STS;
					}
					else {
						NXT_STS = 200+this.AUT_STS;
						break;
					}
				}
				Console.Beep(800, 250);
				break;
			case 115://透過:トレース
			case 135://反射:トレース
			case 155://赤外:トレース
				if (true) {
					int cnt = m_adat.f_cnt[m_adat.h_idx];
					if ((m_adat.f_idx+1) < cnt) {
						//次の画像へ
						m_adat.f_idx++;
						NXT_STS = (this.AUT_STS/10)*10;//->110,130,150
					}
					else {
						//次の毛髪へ
						if (m_adat.f_cnt[m_adat.h_idx+1] <= 0) {//最後の毛髪？
							//次のLEDでトレースを継続
						}
						else {
							m_adat.h_idx++;
							m_adat.f_idx = 0;
							//MOVE_ABS(2, m_adat.z_pls[m_adat.h_idx]);
							NXT_STS = (this.AUT_STS/10)*10;//->110,130,150
						}
					}
				}
				break;
			case 116://透過:トレース
			case 136://反射:トレース
			case 156://赤外:トレース
			case 998:
				//開始位置へ移動
				NXT_STS = -this.AUT_STS;
				if (m_adat.org_pos_x != -0x1000000) {
					//最初の1本目探索位置へ
					MOVE_ABS_XY(m_adat.org_pos_x, m_adat.org_pos_y);
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
					m_pre_set[2] = true;
					m_pre_pos[2] = m_adat.org_pos_z;
#else
					MOVE_ABS_Z(m_adat.org_pos_z);
#endif
				}
				else {
#if true//2018.06.04 赤外同時測定
					MOVE_ABS_XY(G.SS.PLM_AUT_HP_X, G.SS.PLM_AUT_HP_Y);
#else
					//中上
					MOVE_ABS_XY((G.SS.PLM_MLIM[0] + G.SS.PLM_PLIM[0]) / 2, G.SS.PLM_MLIM[1]);
#endif
				}
a_write("開始位置へ移動");
				break;
			case 117://透過:トレース
				if (true) {
					G.FORM10.LED_SET(0, false);//透過OFF
				}
				if (G.SS.PLM_AUT_MODE == 6 || G.SS.PLM_AUT_MODE == 9) {
					//6:反射→透過
					//9:反射→赤外→透過
					G.FORM10.LED_SET(1, true );//反射に戻して終了
					NXT_STS = 999;
				}
				else if (G.SS.PLM_AUT_MODE == 7) {
					//7:反射→透過→赤外
					NXT_STS = 140;//赤外に切り替えて継続
				}
				else {
					G.mlog("kokoni ha konai hazu!!");
				}
				m_adat.chk1 = Environment.TickCount;
			break;
			case 137://反射:トレース
				if (true) {
					G.FORM10.LED_SET(1, false);//反射OFF
				}
				if (G.SS.PLM_AUT_MODE == 1 || G.SS.PLM_AUT_MODE == 4) {
					//1:透過→反射
					//4:透過→赤外→反射
					G.FORM10.LED_SET(0, true );//透過に戻して終了
					NXT_STS = 999;
				}
				else if (G.SS.PLM_AUT_MODE == 2) {
					//2:透過→反射→赤外
					NXT_STS = 140;//赤外に切り替えて継続
				}
				else {
					G.mlog("kokoni ha konai hazu!!");
				}
			break;
			case 157://赤外:トレース
				//光源切り替え
				if (true) {
					G.FORM10.LED_SET(2, false);//赤外OFF
				}
				if (G.SS.PLM_AUT_MODE == 2 || G.SS.PLM_AUT_MODE == 3) {
					//2:透過→反射→赤外
					//3:透過→赤外
					G.FORM10.LED_SET(0, true );//透過に戻して終了
					NXT_STS = 999;
				}
				else if (G.SS.PLM_AUT_MODE == 7 || G.SS.PLM_AUT_MODE == 8) {
					//7:反射→透過→赤外
					//8:反射→赤外
					G.FORM10.LED_SET(1, true );//反射に戻して終了
					NXT_STS = 999;
				}
				else if (G.SS.PLM_AUT_MODE == 4) {
					//4:透過→赤外→反射
					NXT_STS = 120;//反射に切り替えて継続
				}
				else if (G.SS.PLM_AUT_MODE == 9) {
					//9:反射→赤外→透過
					NXT_STS = 100;//透過に切り替えて継続
				}
				else {
					G.mlog("kokoni ha konai hazu!!");
				}
			break;
			case 70:
				//光源切り替え(開始時)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, false);//赤外

				if ((G.SS.PLM_AUT_MODE >= 0 && G.SS.PLM_AUT_MODE <= 4)) {
					G.FORM10.LED_SET(0, true);//透過
a_write("光源切替:->透過");
				}
				else {
					G.FORM10.LED_SET(1, true);//反射
a_write("光源切替:->反射");
				}
				m_adat.chk1 = Environment.TickCount;
				break;
			case 118://透過:トレース
			case 138://反射:トレース
			case 158://赤外:トレース
				break;
			case 119://透過:トレース
			case 139://反射:トレース
			case 159://赤外:トレース
				break;
			case 61:
				NXT_STS = 999;//自動測定:終了
				break;
			case 217:
			case 227:
			case 237:
			case 314:
			case 334:
			case 354:
				//Z軸移動
				if (true) {
					int zpos = (int)(m_adat.z_pos[m_adat.z_idx]);
					MOVE_ABS_Z(m_adat.z_cur + zpos);
					NXT_STS = -this.AUT_STS;
				}
				break;
			case 218:
			case 228:
			case 238:
			case 315:
			case 335:
			case 355:
				m_dcur = m_didx;
				break;
			case 219:
			case 229:
			case 239:
			case 316:
			case 336:
			case 356:
				if ((m_didx - m_dcur) < G.SS.PLM_AUT_SKIP) {
					NXT_STS = this.AUT_STS;//画面が更新されるまで
				}
				break;
			case 220:
			case 230:
			case 240:
			case 317:
			case 337:
			case 357:
				NXT_STS = -3-200+this.AUT_STS;
				break;
			case 999:
				if (m_adat.h_cnt == 0 && G.SS.PLM_AUT_RTRY) {
					if (G.SS.PLM_AUT_MODE == 5 || G.SS.PLM_AUT_MODE == 8) {
						//5:反射
						//8:反射→赤外
						//反射で毛髪検出できないときは透過にてリトライする
						G.SS.PLM_AUT_MODE -= 5;
						//0:透過
						//3:透過→赤外
						NXT_STS = 1;
						break;
					}
				}
//■■■■■■■set_expo_mode(/*auto*/1);
				a_write(string.Format("終了:毛髪{0}本", m_adat.h_cnt));
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				this.AUT_STS = 0;
				timer2.Enabled = false;
				UPDSTS();
				for (int i = 0; i < 3; i++) {
					Console.Beep(1600, 250);
					Thread.Sleep(250);
				}
				G.mlog(string.Format("#i測定が終了しました.\r毛髪:{0}本", m_adat.h_cnt));
				break;
			default:
				if (!(this.AUT_STS < 0)) {
					G.mlog("kakunin suru koto!!!");
				}
				else {
					//f軸停止待ち
#if true//2018.06.04 赤外同時測定
					m_adat.ir_done = false;
#endif
					if ((G.PLM_STS & (1|2|4)) == 0) {
						if (m_bsla[0] != 0 || m_bsla[1] != 0) {
#if true//2018.05.23(毛髪右端での繰り返し発生対応)
							if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_M) != 0) {
								NXT_STS = NXT_STS;//リミットステータスが消えてしまうのでバックラッシュ制御はスキップする
							}
							else if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
								NXT_STS = NXT_STS;//リミットステータスが消えてしまうのでバックラッシュ制御はスキップする
							}
							else {
#endif
							MOVE_REL_XY(m_bsla[0], m_bsla[1]);
#if true//2018.05.23(毛髪右端での繰り返し発生対応)
							}
#endif
							m_bsla[0] = m_bsla[1] = 0;
							NXT_STS = this.AUT_STS;
						}
#if true//2018.05.21(Z軸制御をXY移動後に行うようにする)
						else if (m_pre_set[2]) {
							m_pre_set[2] = false;
							MOVE_ABS_Z(m_pre_pos[2]);
							NXT_STS = this.AUT_STS;
						}
#endif
						else if (m_bsla[2] != 0) {
							Thread.Sleep(1000/G.SS.PLM_LSPD[2]);//2018.05.21
							MOVE_REL_Z(m_bsla[2]);
							m_bsla[2] = 0;
							NXT_STS = this.AUT_STS;
						}
						else {
							NXT_STS = (-this.AUT_STS) + 1;
						}
					}
					else {
						NXT_STS = this.AUT_STS;
					}
				}
				break;
			}
			if (NXT_STS == 0) {
				NXT_STS = 0;//for break.point
			}
			if (this.AUT_STS > 0) {
				m_adat.sts_bak = this.AUT_STS;
			}
			if (this.AUT_STS != 0) {
				this.AUT_STS = NXT_STS;
			}
		}

		private void button12_Click(object sender, EventArgs e)
		{
#if true//2018.07.11(解析画面ユーザ用条件画面の追加)
			Form frm;
			if (G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/) {
				frm = new Form23();
			}
			else {
				frm = new Form21();
			}
#else
			Form21 frm = new Form21();
#endif
			if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				if (G.FORM02 != null) {
					if (G.FORM02.isCONNECTED()) {
						G.FORM02.Stop();
					}
				}
				if (G.FORM03 != null) {
					G.FORM03.Close();
					G.FORM03 = null;
					Application.DoEvents();
				}
				G.FORM03 = new Form03();
				G.FORM03.Show();
			}
		}

		private void timer3_Tick(object sender, EventArgs e)
		{
			int NXT_STS = this.CAL_STS+1;

			this.timer3.Enabled = false;

			switch (this.CAL_STS) {
			case 0:
				this.timer3.Enabled = false;
				break;
			case 2:
				if ((Environment.TickCount-m_tic) < 250) {
					NXT_STS = this.CAL_STS;
				}
				break;
			case 3:
				if (true) {
					DialogResult ret;
					ret = G.mlog(""
							+ "#qカメラのキョリブレーションを実行します。"
							+ "校正用のプレパラートをセットしてください。\r\n-\r\n"
							+ "「いいえ」を選択するとキャリブレーション処理をスキップします。", G.FORM01);
					if (ret != System.Windows.Forms.DialogResult.Yes) {
						NXT_STS = -1;
					}
					else {
						m_prg = new DlgProgress();
						m_prg.Show(Application.ProductName, G.FORM01);
						m_prg.SetStatus("カメラ校正\r\n\r\n実行中...");
						G.FORM10.LED_SET(m_icam = 1, true);//LED.反射->ON
						NXT_STS = 9;
					}
				}
				break;
			case 1:
			case 9:
				m_tic = Environment.TickCount;
			break;
			case 10:
				if ((Environment.TickCount-m_tic) < 1000) {
					NXT_STS = this.CAL_STS;
				}
				break;
			case 11:
				set_expo_mode(1/*1:auto*/);
				m_tic = Environment.TickCount;
			break;
			case 12:
				if ((Environment.TickCount-m_tic) < 5000) {
					NXT_STS = this.CAL_STS;
				}
				else {
					set_expo_mode(/*const*/0);
				}
			break;
			case 13:
				//設定を保存
				if (true) {
					double fval, fmin, fmax;
					//---
					G.FORM02.get_param(Form02.CAM_PARAM.GAIN, out fval, out fmax, out fmin);
					G.SS.CAM_PAR_GA_VL[m_icam] = fval;
					//---
					G.FORM02.get_param(Form02.CAM_PARAM.EXPOSURE, out fval, out fmax, out fmin);
					G.SS.CAM_PAR_EX_VL[m_icam] = fval;
					//---
					G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 0);
					G.FORM02.get_param(Form02.CAM_PARAM.BALANCE, out fval, out fmax, out fmin);
					G.SS.CAM_PAR_WB_RV[m_icam] = fval;
					//---
					G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 1);
					G.FORM02.get_param(Form02.CAM_PARAM.BALANCE, out fval, out fmax, out fmin);
					G.SS.CAM_PAR_WB_GV[m_icam] = fval;
					//---
					G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 2);
					G.FORM02.get_param(Form02.CAM_PARAM.BALANCE, out fval, out fmax, out fmin);
					G.SS.CAM_PAR_WB_BV[m_icam] = fval;
				}
			break;
			case 14:
				if (m_icam == 1) {
					G.FORM10.LED_SET(m_icam = 0, true);//LED.透過->ON
					NXT_STS = 9;
				}
				else if (m_icam == 0) {
					G.FORM10.LED_SET(m_icam = 2, true);//LED.赤外->ON
					NXT_STS = 9;
				}
				else {
					G.FORM10.LED_SET(m_icam = 1, true);//LED.反射->ON
					NXT_STS = NXT_STS;
				}
			break;
			case 15:
			case 20:
				if (m_prg != null) {
					m_prg.Hide();
					m_prg.Close();
					m_prg.Dispose();
					m_prg = null;
				}
				break;
			case 16:
				G.mlog("#iカメラのキョリブレーションが完了しました。");
			break;
			case 17:
			case 21:
				NXT_STS = 0;
			break;
			default:
				NXT_STS = 0;
				break;
			}
			if (NXT_STS == 0) {
				NXT_STS = 0;//for break.point
			}
			if (m_prg != null && G.bCANCEL) {
				NXT_STS = 20;
                G.bCANCEL = false;
            }
			this.CAL_STS = NXT_STS;
			if (NXT_STS != 0) {
				this.timer3.Enabled = true;
			}
		}
	}
}
