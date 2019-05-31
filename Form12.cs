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
#if true//2019.02.27(ＡＦ２実装)
			public bool		AF2;
			public int		imax;
			public double	cmax;
			public double	c2nd;
			public double	cthr;
			public int		zmax;
			public bool		drop;
			public List<int>	l_zpos;
			public List<double>	l_cont;
#endif
#if true//2019.04.04(微分閾値追加)
			public int		dcnt;//ドロップ回数
#endif
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
#if true//2019.02.27(ＡＦ２実装)
		private int FC2_STS;
#endif
		public  int AUT_STS;
		private int CAL_STS;
#if true//2019.01.19(GAIN調整)
		private int GAI_STS;
#endif
#if true//2019.02.03(WB調整)
		private int WBL_STS;
#endif
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
#if true//2019.02.27(ＡＦ２実装)
			this.tabControl4.TabPages.Remove(this.tabPage11);//ＡＦ２
#endif
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
#if false//2019.04.29(微分バグ修正)
			this.comboBox1.Items.Clear();
			this.comboBox1.Items.Add("矩形範囲");
			this.comboBox1.SelectedIndex = 0;
			G.SS.CAM_HIS_PAR1 = 1;//1:矩形範囲
#endif
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
#if true//2019.01.19(GAIN調整)
			if (!G.SS.ETC_UIF_GAIN) {
				this.tabControl4.TabPages.Remove(this.tabPage9);//GAIN調整ページ
#if true//2019.02.03(WB調整)
				this.tabControl4.TabPages.Remove(this.tabPage10);//WB調整ページ
#endif
			}
#endif
#if true//2019.05.12(縦型対応)
			if (G.bTATE_MODE) {
			this.button26.Visible = false;
			this.button27.Visible = false;
			this.button36.Visible = false;
			this.button12.Visible = false;
			this.button11.Text = "毛包探索";
			}
#endif
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
#if true//2019.02.03(WB調整)
			checkBox14_CheckedChanged(null, null);
#endif
#if true//2019.03.02(直線近似)
			checkBox15_Click(null, null);
#endif
#if true//2019.04.04(微分閾値追加)
			radioButton11_Click(null, null);
#endif
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
#if true//2019.02.03(WB調整)
						if (G.UIF_LEVL == 0) {
							//校正スキップする(ユーザモード時)
						}
						else
#endif
						if (true) {
							//校正実行
							this.CAL_STS = 1;
							timer3.Enabled = true;
						}
					}
				}
			}
			else if (sender == this.button11/*黒*/|| sender == this.button26/*白*/ || sender == this.button27/*全*/) {
#if true//2019.05.12(縦型対応)
				if (G.bTATE_MODE) {
					do_mouhou_search();
				}
				else
#endif
				if (G.UIF_LEVL == 0) {
#if false//2019.01.05(キューティクル検出欠損修正)
					string path;
					path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					path += @"\KOP";
					path += @"\" + Application.ProductName;
					G.SS.PLM_AUT_FOLD = path;
#endif
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
#if false//2019.04.09(再測定実装)
			else if (sender == this.button2) {
				//CLOSE
				if (G.FORM02 != null) {
					G.FORM02.Dispose();
					G.FORM02 = null;
				}
			}
#endif
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
#if true//2019.02.03(WB調整)
				G.CNT_OFS = 0;
#endif
#if true//2019.03.22(再測定表)
				G.CNT_MET = G.SS.CAM_HIS_METH;
#else
#if true//2019.03.18(AF順序)
				G.CNT_USSD = G.SS.CAM_FCS_USSD;
#endif
#endif
#if true//2019.04.04(微分閾値追加)
				G.CNT_DTHD = G.SS.CAM_HIS_DTHD;
#endif
#if true//2019.04.29(微分バグ修正)
				G.CNT_DTH2 = G.SS.CAM_HIS_DTH2;
#endif
				G.CAM_PRC = G.CAM_STS.STS_HIST;
			}
			else if (sender == this.button7) {
				//円形度・実行
				G.CAM_PRC = G.CAM_STS.STS_HAIR;
			}
			else if (sender == this.button9) {
				//フォーカス・実行
				G.CNT_MOD = G.SS.CAM_HIS_PAR1;
#if true//2019.02.03(WB調整)
				G.CNT_OFS = 0;
#endif
#if true//2019.03.22(再測定表)
				G.CNT_MET = G.SS.CAM_HIS_METH;
#else
#if true//2019.03.18(AF順序)
				G.CNT_USSD = G.SS.CAM_FCS_USSD;
#endif
#endif
#if true//2019.04.04(微分閾値追加)
				G.CNT_DTHD = G.SS.CAM_HIS_DTHD;
#endif
#if true//2019.04.29(微分バグ修正)
				G.CNT_DTH2 = G.SS.CAM_HIS_DTH2;
#endif
				G.CAM_PRC =  G.CAM_STS.STS_FCUS;
				this.FCS_STS = 1;
				this.timer1.Tag = null;
				this.timer1.Enabled = true;
			}
#if true//2019.02.27(ＡＦ２実装)
			else if (sender == this.button35) {
				//フォーカス・実行
				G.CNT_MOD = G.SS.CAM_HIS_PAR1;
				G.CNT_OFS = 0;
#if true//2019.03.22(再測定表)
				G.CNT_MET = G.SS.CAM_HIS_METH;
#else
#if true//2019.03.18(AF順序)
				G.CNT_USSD = G.SS.CAM_FCS_USSD;
#endif
#endif
#if true//2019.04.04(微分閾値追加)
				G.CNT_DTHD = G.SS.CAM_HIS_DTHD;
#endif
#if true//2019.04.29(微分バグ修正)
				G.CNT_DTH2 = G.SS.CAM_HIS_DTH2;
#endif
				G.CAM_PRC =  G.CAM_STS.STS_FCUS;
				this.FC2_STS = 1;
				this.timer6.Tag = null;
				this.timer6.Enabled = true;
			}
#endif
			else if (sender == this.button25) {
				//キューティクル・実行
				G.CAM_PRC =  G.CAM_STS.STS_CUTI;
			}
#if true//2019.01.19(GAIN調整)
			else if (sender == this.button30) {
				//GAIN調整・実行
				G.CNT_MOD = G.SS.CAM_GAI_PAR1;
#if true//2019.02.03(WB調整)
				G.CNT_OFS = 0;
#endif
				G.CAM_PRC = G.CAM_STS.STS_HIST;
				G.CHK_VPK = 1;
				this.textBox1.Text = "";
				this.textBox2.Text = "";
				this.GAI_STS = 1;
				this.timer4.Tag = null;
				this.timer4.Enabled = true;
			}
			else if (sender == this.button29) {
				//GAIN調整・再調整
				if (this.GAI_STS >= 40 && this.GAI_STS <= 43) {
					m_gdat.gain_dx = 1.0;
					this.GAI_STS = 30;
				}
			}
#endif
#if true//2019.02.03(WB調整)
			else if (sender == this.button33) {
				//WB調整・実行
				G.CNT_MOD = G.SS.CAM_WBL_PAR1;
#if true//2019.02.03(WB調整)
				G.CNT_OFS = 0;
#endif
#if true//2019.03.22(再測定表)
				G.CNT_MET = G.SS.CAM_HIS_METH;
#else
#if true//2019.03.18(AF順序)
				G.CNT_USSD = G.SS.CAM_FCS_USSD;
#endif
#endif
#if true//2019.04.04(微分閾値追加)
				G.CNT_DTHD = G.SS.CAM_HIS_DTHD;
#endif
#if true//2019.04.29(微分バグ修正)
				G.CNT_DTH2 = G.SS.CAM_HIS_DTH2;
#endif
				G.CAM_PRC = G.CAM_STS.STS_HIST;
				G.CHK_WBL = 1;
				this.WBL_STS = 1;
				this.timer5.Tag = null;
				this.timer5.Enabled = true;
			}
			else if (sender == this.button32) {
				//WB調整・再調整
				if (this.WBL_STS >= 40 && this.WBL_STS <= 43) {
					m_wdat.wbauto_done = false;
					m_wdat.offset_done = false;
					this.WBL_STS = 30;
				}
			}
#endif

			else if (sender == this.button6 || sender == this.button8 || sender == this.button10 || sender == this.button24
#if true//2019.01.19(GAIN調整)
				 || sender == this.button28
#endif
#if true//2019.02.03(WB調整)
				 || sender == this.button31
#endif
#if true//2019.02.27(ＡＦ２実装)
				 || sender == this.button34
#endif
				) {
				//停止
				G.CAM_PRC = 0;
#if true//2019.01.19(GAIN調整)
				G.CHK_VPK = 0;
#endif
#if true//2019.02.03(WB調整)
				G.CHK_WBL = 0;
#endif
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
#if true//2019.02.27(ＡＦ２実装)
			if (this.FC2_STS != 0 && G.CAM_PRC != G.CAM_STS.STS_FCUS) {
				this.FC2_STS = 0;
			}
#endif
#if true//2019.01.23(GAIN調整&自動測定)
			if (this.GAI_STS != 0 && G.CAM_PRC != G.CAM_STS.STS_HIST) {
				this.GAI_STS = 0;
			}
#endif
#if true//2019.02.03(WB調整)
			if (this.WBL_STS != 0 && G.CAM_PRC != G.CAM_STS.STS_HIST) {
				this.WBL_STS = 0;
			}
#endif
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
#if true//2019.04.29(微分バグ修正)
				DDV.DDX(bUpdate, this.comboBox1, ref G.SS.CAM_HIS_PAR1);
#else
				if (G.UIF_LEVL == 0) {
				G.SS.CAM_HIS_PAR1 = 1;//1:矩形範囲
				}
				else {
				DDV.DDX(bUpdate, this.comboBox1, ref G.SS.CAM_HIS_PAR1);
				}
#endif
				DDV.DDX(bUpdate, this.comboBox7, ref G.SS.CAM_HIS_METH);
#if false//2019.03.22(再測定表)
				DDV.DDX(bUpdate, this.comboBox8, ref G.SS.CAM_HIS_OIMG);
#endif
#if true//2019.04.04(微分閾値追加)
				DDV.DDX(bUpdate, this.numericUpDown60, ref G.SS.CAM_HIS_DTHD);
#endif
#if true//2019.04.29(微分バグ修正)
				DDV.DDX(bUpdate, this.numericUpDown62, ref G.SS.CAM_HIS_DTH2);
#endif
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
#if false//2019.02.03(WB調整)
				DDV.DDX(bUpdate, this.numericUpDown27, ref G.SS.CAM_CIR_MAGN);
#endif
#if true//2019.03.02(直線近似)
				DDV.DDX(bUpdate, this.checkBox15     ,ref G.SS.CAM_CIR_LINE);
				DDV.DDX(bUpdate, this.numericUpDown27,ref G.SS.CAM_CIR_LPER);
				DDV.DDX(bUpdate, this.numericUpDown59,ref G.SS.CAM_CIR_LCNT);
				DDV.DDX(bUpdate, this.checkBox7      ,ref G.SS.CAM_CIR_CHK5);
#endif
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
#if false//2019.03.22(再測定表)
				DDV.DDX(bUpdate, this.checkBox7, ref G.SS.CAM_FCS_USSD);
#endif
#if true//2019.02.27(ＡＦ２実装)
				DDV.DDX(bUpdate, this.numericUpDown50, ref G.SS.CAM_FC2_LMIN);
				DDV.DDX(bUpdate, this.numericUpDown51, ref G.SS.CAM_FC2_LMAX);
				DDV.DDX(bUpdate, this.numericUpDown52, ref G.SS.CAM_FC2_FSPD);
				DDV.DDX(bUpdate, this.numericUpDown53, ref G.SS.CAM_FC2_DPLS);
				DDV.DDX(bUpdate, this.numericUpDown54, ref G.SS.CAM_FC2_SKIP);
				DDV.DDX(bUpdate, this.numericUpDown55, ref G.SS.CAM_FC2_FAVG);
				DDV.DDX(bUpdate, this.numericUpDown48, ref G.SS.CAM_FC2_DROP);
				DDV.DDX(bUpdate, this.checkBox16, ref G.SS.CAM_FC2_CHK1);
#endif
#if true//2019.04.04(微分閾値追加)
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton11, this.radioButton12}, ref G.SS.CAM_FC2_DTYP);
				DDV.DDX(bUpdate, this.numericUpDown61, ref G.SS.CAM_FC2_DCNT);
#endif
#if true//2019.03.02(直線近似)
				DDV.DDX(bUpdate, this.numericUpDown56, ref G.SS.CAM_FC2_CNDA);
				DDV.DDX(bUpdate, this.numericUpDown57, ref G.SS.CAM_FC2_CNDB);
				DDV.DDX(bUpdate, this.numericUpDown58, ref G.SS.CAM_FC2_BPLS);
#endif
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
#if true//2019.01.19(GAIN調整)
				DDV.DDX(bUpdate, this.comboBox12     , ref G.SS.CAM_GAI_LEDT);//光源
				DDV.DDX(bUpdate, this.comboBox13     , ref G.SS.CAM_GAI_PAR1);//計算範囲
				DDV.DDX(bUpdate, this.numericUpDown39, ref G.SS.CAM_GAI_SKIP);
				DDV.DDX(bUpdate, this.numericUpDown45, ref G.SS.CAM_GAI_VMIN);
				DDV.DDX(bUpdate, this.numericUpDown44, ref G.SS.CAM_GAI_VMAX);
				DDV.DDX(bUpdate, this.numericUpDown38, ref G.SS.CAM_GAI_VSET);
#endif
#if true//2019.02.03(WB調整)
				DDV.DDX(bUpdate, this.comboBox11     , ref G.SS.CAM_WBL_LEDT);//光源
				DDV.DDX(bUpdate, this.comboBox15     , ref G.SS.CAM_WBL_PAR2);//計算方法
				DDV.DDX(bUpdate, this.numericUpDown46, ref G.SS.CAM_WBL_SKIP);//読み捨て
				DDV.DDX(bUpdate, this.numericUpDown49, ref G.SS.CAM_WBL_TOLE);//許容差
				DDV.DDX(bUpdate, this.checkBox13     , ref G.SS.CAM_WBL_CHK1);
				DDV.DDX(bUpdate, this.checkBox14     , ref G.SS.CAM_WBL_CHK2);
				DDV.DDX(bUpdate, this.numericUpDown47, ref G.SS.CAM_WBL_OFFS);
				G.SS.CAM_WBL_PAR1 = 0;//計算範囲=画面全体で固定
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
#if false//2019.04.09(再測定実装)
				this.button2.Enabled = false;//close
#endif
				//this.button3.Enabled = false;//white.para
				//this.button4.Enabled = false;//ir.para
				this.button5.Enabled = false;//his.exec
				this.button6.Enabled = false;
				this.button7.Enabled = false;//hair.exec
				this.button8.Enabled = false;
				this.button9.Enabled = false;//af.exec
				this.button10.Enabled = false;
#if true//2019.02.27(ＡＦ２実装)
				this.button35.Enabled = false;
				this.button34.Enabled = false;
#endif
				this.button11.Enabled = false;//auto.mes
				this.button26.Enabled = false;
				this.button27.Enabled = false;
#if true//2019.01.19(GAIN調整)
				this.button30.Enabled = false;
				this.button29.Enabled = false;
				this.button28.Enabled = false;
#endif
#if true//2019.02.03(WB調整)
				this.button33.Enabled = false;
				this.button32.Enabled = false;
				this.button31.Enabled = false;
#endif
#if false//2019.03.14(NG画像判定)
				this.checkBox11.Enabled = false;//深度合成
#endif
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
#if false//2019.04.09(再測定実装)
				this.button2.Enabled = true;//close
#endif
			}
			else {
				this.button1.Enabled = false;//open
#if false//2019.04.09(再測定実装)
				this.button2.Enabled = true;//close
#endif
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
#if true//2019.02.27(ＡＦ２実装)
				if (this.FC2_STS == 0) {
					this.button35.Enabled = true;
					this.button34.Enabled = false;
				}
				else {
					this.button35.Enabled = false;
					this.button34.Enabled = true;
				}
				if (this.FCS_STS == 0) {
					this.button9.Enabled = true;
					this.button10.Enabled = false;
				}
				else {
					this.button9.Enabled = false;
					this.button10.Enabled = true;
				}
#else
				if (G.CAM_PRC !=  G.CAM_STS.STS_FCUS) {
					this.button9.Enabled = true;
					this.button10.Enabled = false;
				}
				else {
					this.button9.Enabled = false;
					this.button10.Enabled = true;
				}
#endif
#if true//2019.01.19(GAIN調整)
				if (G.CHK_VPK != 1) {
					this.button30.Enabled = true;
					this.button29.Enabled = false;
					this.button28.Enabled = false;
				}
				else {
					this.button30.Enabled = false;
					this.button29.Enabled = false;
					this.button28.Enabled = true;
				}
#endif
#if true//2019.02.03(WB調整)
				if (G.CHK_WBL != 1) {
					this.button33.Enabled = true;
					this.button32.Enabled = false;
					this.button31.Enabled = false;
				}
				else {
					this.button33.Enabled = false;
					this.button32.Enabled = false;
					this.button31.Enabled = true;
				}
#endif
				//
				if (G.FORM02.isCONNECTED() && D.isCONNECTED() && G.FORM11.isORG_ALL_DONE()) {
					this.button11.Enabled = true;
					this.button26.Enabled = true;
					this.button27.Enabled = true;
#if false//2019.03.14(NG画像判定)
					this.checkBox11.Enabled = true;//深度合成
#endif
				}
				else {
					this.button11.Enabled = false;
					this.button26.Enabled = false;
					this.button27.Enabled = false;
#if false//2019.03.14(NG画像判定)
					this.checkBox11.Enabled = false;//深度合成
#endif
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
#if true//2019.01.11(混在対応)
			int ch_bak = ch;
			if (ch == 2 && G.LED_PWR_BAK == 1/*反射*/) {
				ch++;
			}
#endif
			//ch=0:白色LED用(透過), ch=1:白色LED用(反射), ch=2:赤外LED用
			set_param(G.SS.CAM_PAR_GAMMA[ch], G.SS.CAM_PAR_CONTR[ch], G.SS.CAM_PAR_BRIGH[ch], G.SS.CAM_PAR_SHARP[ch]);
			//
#if true//2019.01.11(混在対応)
			set_param_gew(ch_bak);
#else
			set_param_gew(ch);
#endif
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
#if true//2019.01.11(混在対応)
			if (ch == 2/*赤外*/) {
				if (G.LED_PWR_BAK == 1/*反射*/) {
					ch++;
				}
				else {
					ch = ch;
				}
			}
#endif
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
#if true//2019.01.23(GAIN調整&自動測定)
		List<double> m_gain_ofs = new List<double>();
		private void push_gain_ofs()
		{
			for (int i = 0; i < G.SS.CAM_PAR_GA_OF.Length; i++) {
				m_gain_ofs.Add(G.SS.CAM_PAR_GA_OF[i]);
			}
		}
		private void pop_gain_ofs(bool bRemove=true)
		{
			for (int i = 0; i < G.SS.CAM_PAR_GA_OF.Length; i++) {
				if (bRemove) {
				G.SS.CAM_PAR_GA_OF[i] = m_gain_ofs[0];
				m_gain_ofs.RemoveAt(0);
				}
				else {
				G.SS.CAM_PAR_GA_OF[i] = m_gain_ofs[i];
				}
			}
		}
#endif
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
#if true//2019.03.02(直線近似)
			checkBox15_Click(null, null);
#endif
		}

		private void OnControlStateChanged(object sender, EventArgs e)
		{
			if (m_bENTER_GETD) {
				return;
			}
			GETDAT(false);//変数取込
#if true//2019.04.29(微分バグ修正)
			this.numericUpDown60.Enabled = (this.comboBox7.SelectedIndex >= 2 && this.comboBox7.SelectedIndex <= 4);
			this.numericUpDown62.Enabled = (this.comboBox7.SelectedIndex >= 5 && this.comboBox7.SelectedIndex <= 7);
#else
#if true//2019.04.04(微分閾値追加)
			this.numericUpDown60.Enabled = (this.comboBox7.SelectedIndex >= 2);
#endif
#endif
			if (G.FORM02 != null && (G.FORM02.isLOADED() || G.FORM02.isCONNECTED())) {
#if true//2019.01.23(GAIN調整&自動測定)
				if (G.CHK_VPK != 0) {
					G.CHK_VPK = G.CHK_VPK;//for bp
				}
				else
#endif
#if true//2019.02.03(WB調整)
				if (G.CHK_WBL != 0) {
					G.CHK_WBL = G.CHK_WBL;//for bp
				}
				else
#endif
				if (G.CAM_PRC == G.CAM_STS.STS_HIST) {
					G.CNT_MOD = G.SS.CAM_HIS_PAR1;
#if true//2019.02.03(WB調整)
					G.CNT_OFS = 0;
#endif
#if true//2019.03.22(再測定表)
					G.CNT_MET = G.SS.CAM_HIS_METH;
#else
#if true//2019.03.18(AF順序)
					G.CNT_USSD = G.SS.CAM_FCS_USSD;
#endif
#endif
#if true//2019.04.04(微分閾値追加)
					G.CNT_DTHD = G.SS.CAM_HIS_DTHD;
#endif
#if true//2019.04.29(微分バグ修正)
					G.CNT_DTH2 = G.SS.CAM_HIS_DTH2;
#endif
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
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
				wr.WriteLine("読み捨て,"    + G.SS.CAM_FCS_SKIP.ToString());
				wr.WriteLine("平均化," + G.SS.CAM_FCS_FAVG.ToString());
				wr.WriteLine("DATE,X(pls),Y(pls),Z(pls),POS,S,L,C,P,CONTRAST");
#else
				wr.WriteLine("DATE,POS,S,L,C,P,CONTRAST");
#endif
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
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
				buf += string.Format(",{0}", DN(G.PLM_POS[0], 5));
				buf += string.Format(",{0}", DN(G.PLM_POS[1], 5));
#if true//2019.02.27(ＡＦ２実装)
				if (dat.AF2) {
				buf += string.Format(",{0}", DN(dat.pos     , 4));
				}
				else {
#endif
				buf += string.Format(",{0}", DN(G.PLM_POS[2], 4));
#if true//2019.02.27(ＡＦ２実装)
				}
#endif
#endif
				buf += string.Format(",{0}", dat.pos);
				buf += string.Format(",{0:F0}", dat.s);
				buf += string.Format(",{0:F0}", dat.l);
				buf += string.Format(",{0:F2}", dat.c);
				buf += string.Format(",{0:F0}", dat.p);
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
				buf += string.Format(",{0:F20}", dat.contrast);
#else
				buf += string.Format(",{0:F3}", dat.contrast);
#endif
				wr.WriteLine(buf);
				wr.Close();
			}
			catch (Exception) {
			}
		}
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
		private void f_write(string path, string buf)
		{
			StreamWriter wr;
			try {
				wr = new StreamWriter(path, true, Encoding.Default);
				wr.WriteLine(buf);
				wr.Close();
			}
			catch (Exception) {
			}
		}
#endif
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
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
					if (this.timer1.Tag == null) {
						//カメラTABより
#endif
						G.mlog("#e毛髪が検出できませんした.");
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
					}
#endif
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
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
					m_path += "AF\\";
					if (System.IO.Directory.Exists(m_path) == false) {
						System.IO.Directory.CreateDirectory(m_path);
					}
#endif
					m_path += string.Format("{0:0000}{1:00}{2:00}-{3:00}{4:00}{5:00}",
							dt.Year, dt.Month, dt.Day,
							dt.Hour, dt.Minute, dt.Second);
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
					m_path += string.Format("(X,Y,Z={0},{1},{2})", G.PLM_POS[0], G.PLM_POS[1], G.PLM_POS[2]);
#endif
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
#if true//2019.03.02(直線近似)
					m_dism = G.SS.PLM_AUT_DISM;
#else
					m_dism = G.SS.PLM_AUT_DISS;
#endif
					m_disl = G.SS.PLM_AUT_DISL;
				}
				else if ((int)this.timer1.Tag == 2) {
					//自動測定より(２回以降)
					m_diss = G.SS.PLM_AUT_2DSS;
#if true//2019.03.02(直線近似)
					m_dism = G.SS.PLM_AUT_2DSM;
#else
					m_dism = G.SS.PLM_AUT_2DSS;
#endif
					m_disl = G.SS.PLM_AUT_2DSL;
				}
#if true//2019.05.12(縦型対応)
				else if ((int)this.timer1.Tag == 9) {
					m_diss = G.SS.TAT_AFC_DISS;
					m_dism = G.SS.TAT_AFC_DISM;
					m_disl = G.SS.TAT_AFC_DISL;
				}
#endif
				else {
					//自動測定より(フォーカス位置探索用)
					m_diss = G.SS.PLM_AUT_HPSS;
#if true//2019.03.02(直線近似)
					m_dism = G.SS.PLM_AUT_HPSM;
#else
					m_dism = G.SS.PLM_AUT_HPSS;
#endif
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
#if true//2019.05.12(縦型対応)
				else if ((int)this.timer1.Tag == 9) {
				    int tmp = G.PLM_POS[2] - G.SS.PLM_OFFS[2];
				    m_pmin = tmp - G.SS.TAT_AFC_HANI;
				    m_pmax = tmp + G.SS.TAT_AFC_HANI;
				}
#endif
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
#if false//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
					if (this.timer1.Tag == null/*カメラTABより*/) {
						if (m_dat.pos == 57 || m_dat.pos == 58) {
							m_dat.pos = m_dat.pos;
						}
						int	lz = (G.SS.PLM_PLIM[2] - G.SS.PLM_MLIM[2]);
						int dz = (m_dat.pos - G.SS.PLM_MLIM[2]);
						double fz = (double)dz / (double)lz;//0～1
						Random rnd = new Random(Environment.TickCount);
						fz = rnd.NextDouble();
						m_dat.contrast = fz;//Math.Sin(fz * Math.PI);
						// pos=57,fz=0.4994350282485876 , ctr=0.99999842484570045
						//           0.4994350282485876 ,     0.99999842484570045
						//     58,   0.50056497175141246,     0.99999842484570045
					}
#endif
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
#if DEBUG//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
				if (true) {
					int nxt = ((FDATA)m_fdat[imax]).pos;
					int cur = G.PLM_POS[2];
					if (nxt < cur) {
						//G.mlog("バックラッシュ制御が必要！");
					}
				}
#endif
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
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
				f_write(m_path, string.Format(",,,MAXPOS,{0},,,,CONTRAST,{1}", posz, m_contrast));
#endif
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
#if DEBUG//2019.01.23(GAIN調整&自動測定)
				else {
					NXT_STS = NXT_STS;//for break.point
				}
#endif
			}
#if false//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
			else {
				if (timer1.Enabled) {
					this.FCS_STS = NXT_STS;
				}
			}
#endif
		}
		/*
		 * iTag:1(AF初回), 2:(AF2回目以降), 3:(毛髪径によるAF...位置合わせ用)
		 */
		public void start_af(int iTag)
		{
#if false//2019.03.18(AF順序)
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
#endif
			if (iTag == 3) {
				G.SS.CAM_FCS_PAR1 = 2;//毛髪径最大化によるAF
			}
			else if (G.CNT_MOD == 0
#if false//2019.03.18(AF順序)
				|| m_adat.chk3 == 1
#endif
				) {
				//Contrast全体
				G.SS.CAM_FCS_PAR1 = 0;
			}
			else {
				//Contrast矩形
				G.SS.CAM_FCS_PAR1 = 0;
				G.FORM02.set_mask_by_result();
			}
#if true//2019.03.18(AF順序)
			//G.mlog("ATODE KAKU NIN");
			//G.CNT_USSD = G.SS.IMP_AUT_USSD[999];
			if ((G.LED_PWR_STS & 1) != 0) {
				//白色(透過)
				//G.CNT_USSD = G.SS.IMP_AUT_USSD[0];
			}
			else {
				//白色(反射)
				//G.CNT_USSD = G.SS.IMP_AUT_USSD[1];
			}
			//G.mlog("ATODE KAKU NIN");
			//G.CNT_USSD = G.SS.IMP_AUT_USSD[999];
#endif
#if true//2019.05.12(縦型対応)
			if (G.bTATE_MODE) {
				if (G.SS.TAT_AFC_MODE == 1) {
				this.FC2_STS = 1;
				this.timer6.Tag = iTag;
				this.timer6.Enabled = true;
				return;
				}
			}
			else
#endif
#if true//2019.03.02(直線近似)
			if (iTag != 3 && G.SS.PLM_AUT_AF_2) {
				this.FC2_STS = 1;
				this.timer6.Tag = iTag;
				this.timer6.Enabled = true;
				return;
			}
#endif
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
#if true//2018.12.22(測定抜け対応)
			m_adat = new ADATA();
#endif
			m_adat.trace = false;
			m_adat.retry = false;
#if true//2018.04.08(ＡＦパラメータ)
			G.push_af2_para();
#endif
			try {
#if false//2019.03.18(AF順序)
				if (G.FORM02.get_size_mode() > 1) {
					G.FORM02.set_size_mode(1, -1, -1);
				}
#endif
				prg.Show("自動撮影", G.FORM01);
				prg.SetStatus("実行中...");
#if false//2018.05.17
				G.CNT_MOD = (G.SS.PLM_AUT_AFMD==0) ? 0: 1+G.SS.PLM_AUT_AFMD;
#endif
#if true//2019.04.29(微分バグ修正)
				G.CNT_DTHD = G.SS.CAM_HIS_DTHD;
				G.CNT_DTH2 = G.SS.CAM_HIS_DTH2;
#endif
				G.CAM_PRC = G.CAM_STS.STS_AUTO;
#if true//2019.01.23(GAIN調整&自動測定)
				if (G.SS.PLM_AUT_V_PK) {
					push_gain_ofs();
				}
#endif
				this.AUT_STS = 1;
				timer2.Enabled = true;
				while (
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
					this.AUT_STS != 0
#else
					timer2.Enabled
#endif
					) {
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
					//if ((m_adat.h_idx + 1) == 2) {
					//    buf = buf;
					//}
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
#if true//2018.11.13(毛髪中心AF)
						if (this.AUT_STS > 600) {
#if true//2019.03.18(AF順序)
							if (G.SS.IMP_AUT_EXAF)
								buf += "フォーカス(表面)";
							else
#endif
						buf += "フォーカス(中心)";
						}
						else if (this.AUT_STS < 10) {
						buf += "フォーカス";
						}
						else {
#if true//2019.03.18(AF順序)
							if (G.SS.IMP_AUT_EXAF)
								buf += "フォーカス(中心)";
							else
#endif
						buf += "フォーカス(表面)";
						}
#else
						buf += "フォーカス";
#endif
					}
					else if (this.AUT_STS < 20) {
						buf += "探索中";
					}
#if true//2019.01.23(GAIN調整&自動測定)
					else if (this.AUT_STS >= 700 && this.AUT_STS <= 701) {
						buf += "GAIN調整中";
					}
#endif
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
#if true//2019.01.23(GAIN調整&自動測定)
			if (G.SS.PLM_AUT_V_PK) {
				pop_gain_ofs();
			}
#endif
#if true//2018.04.08(ＡＦパラメータ)
			G.pop_af2_para();
#endif
			//---
#if false//2019.03.18(AF順序)
			if (G.FORM02.get_size_mode() > 1) {
				G.FORM02.set_size_mode(1, -1, -1);
			}
#endif
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
#if true//2019.01.11(混在対応)
			if (ch == 2/*赤外*/) {
				if (G.LED_PWR_BAK == 1/*反射*/) {
					ch++;
				}
				else {
					ch = ch;
				}
			}
#endif
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
#if true//2019.04.09(再測定実装)
		public
#else
		private
#endif
		class ADATA
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
#if true//2019.03.18(AF順序)
			public List<string> z_nam;
			public List<int> z_pos;
			public bool exaf_done;
#else
			public ArrayList z_nam;
			public ArrayList z_pos;
#endif
#if true//2018.11.13(毛髪中心AF)
			public int k_pre_pos_z;
			public double k_sta_contrast;
			public int k_idx;
			public int k_cnt;
			public int k_cur;
			public bool k_done;
			public List<string> k_nam;
			public List<int> k_pos;
#endif
			//---
			public bool retry;
			public ArrayList y_1st_pos;
#if true//2018.12.22(測定抜け対応)
			public int n_idx;		//抜け測定でのトータルでのインデックス
			public bool nuke;
			public int	nuke_id;
			public int	nuke_cnt;
			public List<int> nuke_st;
			public List<int> nuke_ed;
			public List<int> nuke_pos;
			public int cam_hei_pls;
#endif
#if true//2019.01.11(混在対応)
			public List<string> y_1st_pref;
			public List<string> nuke_pref;
#endif
			//---
			public int ir_next;
			public bool ir_done;
			public int ir_lsbk;
			public int ir_chk1;
#if true//2019.01.23(GAIN調整&自動測定)
			public bool gai_tune_ir_done;
			public bool gai_tune_cl_done;
#endif
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
#if true//2018.09.27(20本対応と解析用パラメータ追加)
				f_cnt = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
#else
				f_cnt = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
#endif
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
#if true//2019.03.18(AF順序)
				z_nam = new List<string>();
				z_pos = new List<int>();
#else
				z_nam = new ArrayList();
				z_pos = new ArrayList();
#endif
#if true//2018.11.13
				k_pre_pos_z = 0;
				k_sta_contrast = double.NaN;
				k_idx =-1;
				k_cnt = 1;
				k_cur = 0;
				k_done = false;
				k_nam = new List<string>();
				k_pos = new List<int>();
#endif
				y_1st_pos = new ArrayList();
#if true//2018.12.22(測定抜け対応)
				n_idx = 0;
				nuke = false;
				nuke_id = 0;
				nuke_cnt = 0;
				nuke_st = new List<int>();
				nuke_ed = new List<int>();
				nuke_pos = new List<int>();
				double tmp;
				tmp = G.CAM_HEI;				//px
				tmp = G.PX2UM(tmp);				//um
				tmp = tmp / G.SS.PLM_UMPP[1];	//pls
				cam_hei_pls = (int)tmp;			//155pls
#endif
#if true//2019.01.11(混在対応)
				y_1st_pref = new List<string>();
				nuke_pref = new List<string>();
#endif
				ir_next = 0;
				ir_done = false;
				ir_lsbk = 0;
#if true//2019.01.23(GAIN調整&自動測定)
				gai_tune_ir_done = false;
				gai_tune_cl_done = false;
#endif
#if true//2019.03.18(AF順序)
				exaf_done = false;
#endif
			}
		};
#if true//2018.12.22(測定抜け対応)
		private ADATA m_adat = null;
#else
		private ADATA m_adat = new ADATA();
#endif
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
#if true//2018.12.22(測定抜け対応)
			if (m_adat.nuke) {
				char seq = (char)('A' + m_adat.n_idx);
				path += seq;
			}
			else {
#endif
			path += string.Format("{0}", m_adat.h_idx);
#if true//2018.12.22(測定抜け対応)
			}
#endif
			path +=  m_adat.pref;
			if (f_idx >= 0) {
			path += string.Format("_{0:00}", m_adat.f_idx);
			}
			else {
			path += "_@@";
			}
			path += "_";
#if true//2018.11.13(毛髪中心AF)
			if (m_adat.k_idx >= 0) {
			path += m_adat.k_nam[m_adat.k_idx];
			}
			else {
#endif
			path += m_adat.z_nam[m_adat.z_idx];
#if true//2018.11.13(毛髪中心AF)
			}
#endif
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
#if true//2019.04.01(表面赤外省略)
					if (System.IO.File.Exists(path_old_ir))
#endif
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
#if true//2019.03.18(AF順序)
							if (G.SS.IMP_AUT_EXAF) {
								name_old = src.Replace("KP00D", tmp);
								name_new = dst.Replace("KP00D", tmp);
							}
#endif
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
#if true//true//2018.11.13(毛髪中心AF)
					if (m_adat.k_cnt > 0) {
						for (int q = 0; q < m_adat.k_cnt; q++) {
							string tmp = (string)m_adat.k_nam[q];
							string name_old = src.Replace("ZP00D", tmp);
							string name_new = dst.Replace("ZP00D", tmp);
#if true//2019.03.18(AF順序)
							if (G.SS.IMP_AUT_EXAF) {
								name_old = src.Replace("KP00D", tmp);
								name_new = dst.Replace("KP00D", tmp);
							}
#endif
							//---
							buf = buf.Replace(name_old, name_new);
							if (G.SS.PLM_AUT_IRCK) {
								string src_ir = to_ir_file(null, name_old);
								string dst_ir = to_ir_file(null, name_new);
								buf = buf.Replace(src_ir, dst_ir);
							}
						}
					}
#endif
				}
				if (true) {
					if (m_adat.z_cnt > 1) {
						for (int q = 1; q < m_adat.z_cnt; q++) {
							string tmp = (string)m_adat.z_nam[q];
							string name_old = path_old.Replace("ZP00D", tmp);
							string name_new = path_new.Replace("ZP00D", tmp);
#if true//2019.03.18(AF順序)
							if (G.SS.IMP_AUT_EXAF) {
								name_old = path_old.Replace("KP00D", tmp);
								name_new = path_new.Replace("KP00D", tmp);
							}
#endif
							//---
							System.IO.File.Move(name_old, name_new);
#if true//2018.06.04 赤外同時測定
							if (G.SS.PLM_AUT_IRCK) {
								string path_old_ir = to_ir_file(m_adat.fold, name_old);
								string path_new_ir = to_ir_file(m_adat.fold, name_new);
#if true//2019.04.01(表面赤外省略)
								if (System.IO.File.Exists(path_old_ir))
#endif
								System.IO.File.Move(path_old_ir, path_new_ir);
							}
#endif
						}
					}
#if true//true//2018.11.13(毛髪中心AF)
					if (m_adat.k_cnt > 0) {
						for (int q = 0; q < m_adat.k_cnt; q++) {
							string tmp = (string)m_adat.k_nam[q];
							string name_old = path_old.Replace("ZP00D", tmp);
							string name_new = path_new.Replace("ZP00D", tmp);
#if true//2019.03.18(AF順序)
							if (G.SS.IMP_AUT_EXAF) {
								name_old = path_old.Replace("KP00D", tmp);
								name_new = path_new.Replace("KP00D", tmp);
							}
#endif
							//---
							System.IO.File.Move(name_old, name_new);
							if (G.SS.PLM_AUT_IRCK) {
								string path_old_ir = to_ir_file(m_adat.fold, name_old);
								string path_new_ir = to_ir_file(m_adat.fold, name_new);
#if true//2019.04.01(表面赤外省略)
								if (System.IO.File.Exists(path_old_ir))
#endif
								System.IO.File.Move(path_old_ir, path_new_ir);
							}
						}
					}
#endif
				}
			}
			System.IO.File.WriteAllText(m_adat.log, buf, Encoding.Default);
			m_adat.f_dum.Clear();
			m_adat.f_nam.Clear();
		}

		private int retry_check(int sts)
		{
			if (G.SS.PLM_AUT_RTRY) {
#if false//2019.01.11(混在対応)
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
#endif
			}
			return(sts);
		}
#if false//2019.01.11(混在対応)
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
#endif
#if true//2019.01.11(混在対応)
		private int get_ypos_min(List<int> ypos)
		{
			double fmin = double.MaxValue;
			for (int i = 0; i < (ypos.Count-1); i++) {
				double fdif = ypos[i+1] - ypos[i];
				if (fmin > fdif) {
					fmin = fdif;
				}
			}
			return((int)fmin);
		}
		private bool check_touka_retry()
		{
			int[] ytmp = (int[])m_adat.y_1st_pos.ToArray(typeof(int));
			List<int> ypos = new List<int>(ytmp);
			double fmin;

			m_adat.n_idx = 0;
			m_adat.nuke_id = 0;
			m_adat.nuke_cnt = 0;
			m_adat.nuke_st.Clear();
			m_adat.nuke_ed.Clear();
			m_adat.nuke_pos.Clear();
			m_adat.nuke_pref.Clear();
			if (m_adat.y_1st_pos.Count < 3) {
				return(false);
			}
			if (ypos.Count <= 0) {
				//全て白髪だったとき
				m_adat.nuke_st.Add(G.SS.PLM_AUT_HP_Y-m_adat.cam_hei_pls);
				m_adat.nuke_ed.Add(G.SS.PLM_AUT_ED_Y+m_adat.cam_hei_pls);
				return(true);
			}
			fmin = get_ypos_min(ypos);
			////以降、黒髪1本以上のとき
			if (G.SS.PLM_AUT_HP_Y <= (ypos[0] - fmin)) {
				m_adat.nuke_st.Add(G.SS.PLM_AUT_HP_Y-m_adat.cam_hei_pls);
				m_adat.nuke_ed.Add(ypos[0]);
			}
			for (int i = 0; i < (ypos.Count-1); i++) {
				double fdif = ypos[i+1] - ypos[i];
				if (fdif >= (fmin*2)) {
					//最小毛髪間隔の範囲を透過測定対象とする
					m_adat.nuke_st.Add(ypos[i]);
					m_adat.nuke_ed.Add(ypos[i+1]);
				}
			}
			if (G.SS.PLM_AUT_ED_Y >= (ypos[ypos.Count-1] + fmin)) {
				m_adat.nuke_st.Add(ypos[ypos.Count-1]);
				m_adat.nuke_ed.Add(G.SS.PLM_AUT_ED_Y+m_adat.cam_hei_pls);
			}
			if (m_adat.nuke_st.Count <= 0) {
				return(false);
			}
			m_adat.nuke_cnt = m_adat.nuke_st.Count;
			return(true);
		}
#endif
#if true//2018.12.22(測定抜け対応)
		private bool check_nuke()
		{
			m_adat.n_idx = 0;
			m_adat.nuke_id = 0;
			m_adat.nuke_cnt = 0;
			m_adat.nuke_st.Clear();
			m_adat.nuke_ed.Clear();
#if true//2019.01.11(混在対応)
			m_adat.nuke_pos.Clear();
			m_adat.nuke_pref.Clear();
#endif
			if (m_adat.y_1st_pos.Count < 3) {
				return(false);
			}
			int[] ytmp = (int[])m_adat.y_1st_pos.ToArray(typeof(int));
			List<int> ypos = new List<int>(ytmp);
#if true//2019.01.11(混在対応)
			double fmin = get_ypos_min(ypos);
#else
			double fmin = double.MaxValue;
			int	imin = 0;

			for (int i = 0; i < (ypos.Count-1); i++) {
				double fdif = ypos[i+1] - ypos[i];
				if (fmin > fdif) {
					fmin = fdif;
					imin = i;
				}
			}
#endif
			for (int i = 0; i < (ypos.Count-1); i++) {
				double fdif = ypos[i+1] - ypos[i];
				if (fdif >= (fmin*2)) {
					//最小毛髪間隔の二倍以上の範囲を検索抜け対象とする
					m_adat.nuke_st.Add(ypos[i]);
					m_adat.nuke_ed.Add(ypos[i+1]);
				}
			}
			if (G.SS.PLM_AUT_ED_Y >= (ypos[ypos.Count-1] + fmin)) {
				m_adat.nuke_st.Add(ypos[ypos.Count-1]);
				m_adat.nuke_ed.Add(G.SS.PLM_AUT_ED_Y+m_adat.cam_hei_pls);
			}
			if (m_adat.nuke_st.Count <= 0) {
				return(false);
			}
			DialogResult ret;
			timer2.Enabled = false;
#if true//2019.02.23(毛髪スキップ時文言変更)
			ret = G.mlog(string.Format("#q未検出の毛髪が存在する可能性があります。検出条件を変えて再検出しますか？（測定済み:{0}本）", m_adat.h_cnt));
#else
			ret = G.mlog(string.Format("#q測定済み({0}本)の毛髪間隔が一定ではありません。測定抜けのチェックを行いますか？", m_adat.h_cnt));
#endif
#if false//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
			timer2.Enabled = true;
#endif
			if (ret != System.Windows.Forms.DialogResult.Yes) {
				return(false);
			}
			m_adat.nuke_cnt = m_adat.nuke_st.Count;
			return(true);
		}
		/*
		  y_1st_pos: -849,-555,  33, 274, 568, 862
		              0CR, 1CR, 2CR, 3CR, 4CR, 5CR
		  nuke__pos:          -255
		                       ACR
		 

		 
		 */
		private void rename_nuke_files()
		 {
			List<int>	old_im_posi = new List<int>();
			List<string>old_cl_name = new List<string>();
			List<string>old_ir_name = new List<string>();
			//List<int>	upd_posi = new List<int>();
			List<string>upd_cl_name = new List<string>();
			List<string>upd_ir_name = new List<string>();

			for (int i = 0; i < m_adat.y_1st_pos.Count; i++) {
				old_im_posi.Add((int)m_adat.y_1st_pos[i]);
#if true//2019.01.11(混在対応)
				old_cl_name.Add(i.ToString() + m_adat.y_1st_pref[i]);
#else
				old_cl_name.Add(i.ToString() + m_adat.pref);
#endif
				old_ir_name.Add(i.ToString() + "IR");
			}
			//G.mlog("IRについても処理を追加する");
			//@@
			for (int h = 0; h < m_adat.nuke_pos.Count; h++) {
				int i;
				for (i = 0; i < old_im_posi.Count; i++) {
					if (m_adat.nuke_pos[h] < old_im_posi[i]) {
						break;
					}
				}
				old_im_posi.Insert(i, m_adat.nuke_pos[h]);
				char seq = (char)('A'+h);
#if true//2019.01.11(混在対応)
				old_cl_name.Insert(i, seq + m_adat.nuke_pref[h]);
#else
				old_cl_name.Insert(i, seq + m_adat.pref);
#endif
				old_ir_name.Insert(i, seq + "IR");
			}
			for (int i = 0; i < old_cl_name.Count; i++) {
#if true//2019.01.11(混在対応)
				upd_cl_name.Add(i.ToString() + old_cl_name[i].Substring(old_cl_name[i].Length-2, 2));
#else
				upd_cl_name.Add(i.ToString() + m_adat.pref);
#endif
				upd_ir_name.Add(i.ToString() + "IR");
			}
			//---
			for (int i = old_cl_name.Count-1; i >= 0; i--) {
				string[] files;
				files = System.IO.Directory.GetFiles(m_adat.fold, old_cl_name[i]+"*");
				for (int h = 0; h < files.Length; h++) {
					string old_path = files[h];
					string upd_path = old_path.Replace(old_cl_name[i], upd_cl_name[i]);
					System.IO.File.Move(old_path, upd_path);
				}
			}
			//---
			for (int i = old_ir_name.Count-1; i >= 0; i--) {
				string[] files;
				files = System.IO.Directory.GetFiles(m_adat.fold, old_ir_name[i]+"*");
				for (int h = 0; h < files.Length; h++) {
					string old_path = files[h];
					string upd_path = old_path.Replace(old_ir_name[i], upd_ir_name[i]);
#if true//2019.04.01(表面赤外省略)
					if (System.IO.File.Exists(old_path))
#endif
					System.IO.File.Move(old_path, upd_path);
				}
			}
			//---
			string buf = System.IO.File.ReadAllText(m_adat.log, Encoding.Default);
			for (int i = old_cl_name.Count-1; i >= 0; i--) {
#if true//2019.05.22(再測定判定(キューティクル枚数))
				
				buf = buf.Replace("画像保存:" + old_cl_name[i], "画像保存:" + upd_cl_name[i]);
				buf = buf.Replace("画像保存:" + old_ir_name[i], "画像保存:" + upd_ir_name[i]);
#else
				buf = buf.Replace(old_cl_name[i], upd_cl_name[i]);
				buf = buf.Replace(old_ir_name[i], upd_ir_name[i]);
#endif
			}
			System.IO.File.WriteAllText(m_adat.log, buf, Encoding.Default);
#if true//2019.01.11(混在対応)
			m_adat.y_1st_pos.Clear();
			m_adat.y_1st_pref.Clear();

			for (int i = 0; i < old_cl_name.Count; i++) {
				int		pos = old_im_posi[i];
				string	tmp = old_cl_name[i];
				m_adat.y_1st_pos.Add(pos);
				m_adat.y_1st_pref.Add(tmp.Substring(tmp.Length-2, 2));
			}
#endif
		}
#endif
#if true//2019.03.18(AF順序)
		private void set_af_mode(int AUT_STS)
		{
			int AFMD = -1;
			switch (AUT_STS) {
				case 1:
				case 15:
				case 25:
				case 35:
#if true//2019.04.09(再測定実装)
				case 1000:
#endif
					if ((G.LED_PWR_STS & 1) != 0) {
						//白色(透過)
						if (!G.SS.IMP_AUT_EXAF) {
							AFMD = 0;//透過(表面)
						}
						else {
							AFMD = 2;//透過(中心)
						}
					}
					else {
						//白色(反射)
						if (!G.SS.IMP_AUT_EXAF) {
							AFMD = 1;//反射(表面)
						}
						else {
							AFMD = 3;//反射(中心)
						}
					}
				break;
				case 615:
				case 625:
				case 635:
#if true//2019.04.09(再測定実装)
				case 2000:
#endif
					if ((G.LED_PWR_STS & 1) != 0) {
						//白色(透過):中心用
						if (!G.SS.IMP_AUT_EXAF) {
							AFMD = 2;//透過(中心)
						}
						else {
							AFMD = 0;//透過(表面)
						}
					}
					else {
						//白色(反射):中心用
						if (!G.SS.IMP_AUT_EXAF) {
							AFMD = 3;//反射(中心)
						}
						else {
							AFMD = 1;//反射(表面)
						}
					}
				break;
			}
			//---
			switch (AFMD) {
				case 0://透過(表面)
					G.CNT_MOD  = G.AFMD2N(G.SS.IMP_AUT_AFMD[0]);
					G.CNT_OFS  = G.SS.IMP_AUT_SOFS[0];//透過(表面)
#if true//2019.03.22(再測定表)
					G.CNT_MET  = G.SS.IMP_AUT_CMET[0];//透過(表面)
#else
					G.CNT_USSD = G.SS.IMP_AUT_USSD[0];//透過(表面)
#endif
#if true//2018.04.08(ＡＦパラメータ)
					G.SS.CAM_FC2_FSPD = G.SS.IMP_FC2_FSPD[0];
					G.SS.CAM_FC2_DPLS = G.SS.IMP_FC2_DPLS[0];
					G.SS.CAM_FC2_CNDA = G.SS.IMP_FC2_CNDA[0];
					G.SS.CAM_FC2_CNDB = G.SS.IMP_FC2_CNDB[0];
					G.SS.CAM_FC2_SKIP = G.SS.IMP_FC2_SKIP[0];
					G.SS.CAM_FC2_FAVG = G.SS.IMP_FC2_FAVG[0];
					G.SS.CAM_FC2_BPLS = G.SS.IMP_FC2_BPLS[0];
					G.SS.CAM_FC2_DTYP = G.SS.IMP_FC2_DTYP[0];
					G.SS.CAM_FC2_DROP = G.SS.IMP_FC2_DROP[0];
					G.SS.CAM_FC2_DCNT = G.SS.IMP_FC2_DCNT[0];
#endif
					break;
				case 1:
					G.CNT_MOD  = G.AFMD2N(G.SS.IMP_AUT_AFMD[1]);
					G.CNT_OFS  = G.SS.IMP_AUT_SOFS[1];//反射(表面)
#if true//2019.03.22(再測定表)
					G.CNT_MET  = G.SS.IMP_AUT_CMET[1];//反射(表面)
#else
					G.CNT_USSD = G.SS.IMP_AUT_USSD[1];//反射(表面)
#endif
#if true//2018.04.08(ＡＦパラメータ)
					G.SS.CAM_FC2_FSPD = G.SS.IMP_FC2_FSPD[1];
					G.SS.CAM_FC2_DPLS = G.SS.IMP_FC2_DPLS[1];
					G.SS.CAM_FC2_CNDA = G.SS.IMP_FC2_CNDA[1];
					G.SS.CAM_FC2_CNDB = G.SS.IMP_FC2_CNDB[1];
					G.SS.CAM_FC2_SKIP = G.SS.IMP_FC2_SKIP[1];
					G.SS.CAM_FC2_FAVG = G.SS.IMP_FC2_FAVG[1];
					G.SS.CAM_FC2_BPLS = G.SS.IMP_FC2_BPLS[1];
					G.SS.CAM_FC2_DTYP = G.SS.IMP_FC2_DTYP[1];
					G.SS.CAM_FC2_DROP = G.SS.IMP_FC2_DROP[1];
					G.SS.CAM_FC2_DCNT = G.SS.IMP_FC2_DCNT[1];
#endif
					break;
				case 2:
					G.CNT_MOD  = G.AFMD2N(G.SS.IMP_AUT_AFMD[2]);
					G.CNT_OFS  = G.SS.IMP_AUT_COFS[0];//透過(中心)
#if true//2019.03.22(再測定表)
					G.CNT_MET  = G.SS.IMP_AUT_CMET[2];//透過(中心)
#else
					G.CNT_USSD = G.SS.IMP_AUT_USSD[2];//透過(中心)
#endif
#if true//2018.04.08(ＡＦパラメータ)
					G.SS.CAM_FC2_FSPD = G.SS.IMP_FC2_FSPD[2];
					G.SS.CAM_FC2_DPLS = G.SS.IMP_FC2_DPLS[2];
					G.SS.CAM_FC2_CNDA = G.SS.IMP_FC2_CNDA[2];
					G.SS.CAM_FC2_CNDB = G.SS.IMP_FC2_CNDB[2];
					G.SS.CAM_FC2_SKIP = G.SS.IMP_FC2_SKIP[2];
					G.SS.CAM_FC2_FAVG = G.SS.IMP_FC2_FAVG[2];
					G.SS.CAM_FC2_BPLS = G.SS.IMP_FC2_BPLS[2];
					G.SS.CAM_FC2_DTYP = G.SS.IMP_FC2_DTYP[2];
					G.SS.CAM_FC2_DROP = G.SS.IMP_FC2_DROP[2];
					G.SS.CAM_FC2_DCNT = G.SS.IMP_FC2_DCNT[2];
#endif
					break;
				case 3:
					G.CNT_MOD  = G.AFMD2N(G.SS.IMP_AUT_AFMD[3]);
					G.CNT_OFS  = G.SS.IMP_AUT_COFS[1];//反射(中心)
#if true//2019.03.22(再測定表)
					G.CNT_MET  = G.SS.IMP_AUT_CMET[3];//反射(中心)
#else
					G.CNT_USSD = G.SS.IMP_AUT_USSD[3];//反射(中心)
#endif
#if true//2018.04.08(ＡＦパラメータ)
					G.SS.CAM_FC2_FSPD = G.SS.IMP_FC2_FSPD[3];
					G.SS.CAM_FC2_DPLS = G.SS.IMP_FC2_DPLS[3];
					G.SS.CAM_FC2_CNDA = G.SS.IMP_FC2_CNDA[3];
					G.SS.CAM_FC2_CNDB = G.SS.IMP_FC2_CNDB[3];
					G.SS.CAM_FC2_SKIP = G.SS.IMP_FC2_SKIP[3];
					G.SS.CAM_FC2_FAVG = G.SS.IMP_FC2_FAVG[3];
					G.SS.CAM_FC2_BPLS = G.SS.IMP_FC2_BPLS[3];
					G.SS.CAM_FC2_DTYP = G.SS.IMP_FC2_DTYP[3];
					G.SS.CAM_FC2_DROP = G.SS.IMP_FC2_DROP[3];
					G.SS.CAM_FC2_DCNT = G.SS.IMP_FC2_DCNT[3];
#endif
					break;
			}
		}
#endif
#if true//2019.04.01(表面赤外省略)
		private bool is_sf()
		{
			bool ret;
			if (m_adat.k_idx >= 0) {
				ret = false;//中心
			}
			else {
				ret = true;//表面
			}
			if (G.SS.IMP_AUT_EXAF) {
				ret = !ret;
			}
			return(ret);
		}
#endif
		private int m_retry_cnt_of_hpos;
		// 自動測定
		private void timer2_Tick(object sender, EventArgs e)
		{
			int NXT_STS = this.AUT_STS + 1;
			int yy, y0, ypos;

#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
			this.timer2.Enabled = false;
#endif
			if (G.bCANCEL) {
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				this.AUT_STS = 0;
				G.bCANCEL = false;
			}
#if true//2018.11.13(毛髪中心AF)
			if (this.AUT_STS == 16 && this.FCS_STS != 0) {
			}
			else {
				this.AUT_STS = this.AUT_STS;//FOR BP
			}
#endif
#if DEBUG//2019.01.23(GAIN調整&自動測定)
System.Diagnostics.Debug.WriteLine("{0}:STS={1},DIDX={2}", Environment.TickCount, this.AUT_STS, m_didx);
#endif
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
#if true//2019.03.18(AF順序)
					set_af_mode(this.AUT_STS);
#else
#if true//2018.05.17
					if ((G.LED_PWR_STS & 1) != 0) {
						//白色(透過)
						G.CNT_MOD = (G.SS.IMP_AUT_AFMD[0]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[0];
#if true//2019.02.03(WB調整)
						G.CNT_OFS = G.SS.IMP_AUT_SOFS[0];//透過(表面)
#endif
					}
					else {
						//白色(反射)
						G.CNT_MOD = (G.SS.IMP_AUT_AFMD[1]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[1];
#if true//2019.02.03(WB調整)
						G.CNT_OFS = G.SS.IMP_AUT_SOFS[1];//反射(表面)
#endif
					}
#endif
#endif
#if true//2018.12.22(測定抜け対応)
					if (m_adat.nuke) {
					}
					else
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
#if true//2018.12.22(測定抜け対応)
					if (m_adat.nuke) {
					}
					else
#endif
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
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke) {
				MOVE_ABS_XY(G.SS.PLM_AUT_HP_X, m_adat.nuke_st[0] + m_adat.cam_hei_pls);
				}
				else {
#endif
#if true//2018.06.04 赤外同時測定
				MOVE_ABS_XY(G.SS.PLM_AUT_HP_X, G.SS.PLM_AUT_HP_Y);
#endif
#if true//2018.12.22(測定抜け対応)
				}
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
				if (
#if true//2018.12.22(測定抜け対応)
					true
#else
					G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/
#endif
					) {
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
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke) {
				}
				else
#endif
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
#if true//2019.01.11(混在対応)
//					m_adat.y_1st_pref.Clear();
#endif
					//---
					if (true) {
						m_adat.z_cnt = 1;
						m_adat.z_idx = 0;
						m_adat.z_nam.Add("ZP00D");
						m_adat.z_pos.Add(0);
#if true//2018.11.13(毛髪中心AF)
						m_adat.k_cnt = 0;
						m_adat.k_idx =-1;
						m_adat.k_nam.Clear();
						m_adat.k_pos.Clear();
#endif
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
#if true//2018.11.13(毛髪中心AF)
					if (G.SS.PLM_AUT_ZKCK) {
						m_adat.k_cnt = 1;
						m_adat.k_nam.Add("KP00D");
						m_adat.k_pos.Add(0);
					}
#endif
					if (G.SS.PLM_AUT_ZKCK && G.SS.PLM_AUT_ZKEI != null && G.SS.PLM_AUT_ZKEI.Length > 0) {
						for (int i = 0; i < G.SS.PLM_AUT_ZKEI.Length; i++) {
							int pos = G.SS.PLM_AUT_ZKEI[i];
#if true//2018.11.13(毛髪中心AF)
					        m_adat.k_cnt++;
							if (pos >= 0) {
					        m_adat.k_nam.Add(string.Format("KP{0:00}D", +pos));
							}
							else {
					        m_adat.k_nam.Add(string.Format("KM{0:00}D", -pos));
							}
					        m_adat.k_pos.Add(pos);
#else
					        m_adat.z_cnt++;
							if (pos >= 0) {
					        m_adat.z_nam.Add(string.Format("ZP{0:00}K", +pos));
							}
							else {
					        m_adat.z_nam.Add(string.Format("ZM{0:00}K", -pos));
							}
					        m_adat.z_pos.Add(pos);
#endif
					    }
					}
#if true//2019.03.18(AF順序)
					if (m_adat.exaf_done == false && G.SS.IMP_AUT_EXAF) {
						m_adat.exaf_done = true;
						if (m_adat.k_cnt <= 0) {
							G.mlog("internal error");
						}
						int				tmp_cnt = m_adat.k_cnt;
						List<string>	tmp_nam = m_adat.k_nam;
						List<int>		tmp_pos = m_adat.k_pos;
						m_adat.k_cnt = m_adat.z_cnt;
						m_adat.k_nam = m_adat.z_nam;
						m_adat.k_pos = m_adat.z_pos;
						m_adat.z_cnt = tmp_cnt;
						m_adat.z_nam = tmp_nam;
						m_adat.z_pos = tmp_pos;
					}
#endif
				}
				NXT_STS = 12;
				break;
			case 5:
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke && G.PLM_POS[1] >= (m_adat.nuke_ed[m_adat.nuke_id]-m_adat.cam_hei_pls)) {
					m_adat.nuke_id++;
					if (m_adat.nuke_id >= m_adat.nuke_cnt) {
						NXT_STS = 999;//->終了
					}
					else {
						int nxt_ypos = m_adat.nuke_st[m_adat.nuke_id] + m_adat.cam_hei_pls;
						MOVE_REL_XY(0,nxt_ypos-G.PLM_POS[1]);
						NXT_STS = -(5 - 1);//->5
					}
				}
				else
#endif
#if true//2018.07.30(終了位置指定)
				if (
#if false//2018.12.22(測定抜け対応)
					G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/ &&
#endif
					G.PLM_POS[1] >= G.SS.PLM_AUT_ED_Y) {
					NXT_STS = 999;
				}
				else
#endif
				if ((G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					//SOFT.LIMIT(+)
#if true
					if ((NXT_STS = retry_check(NXT_STS)) == 1) {
						break;
					}
#endif
					NXT_STS = 999;
				}
#if false//2019.01.11(混在対応)
				else if (retry_ypos_check(G.PLM_POS[1], out ypos)) {
					MOVE_REL_XY(0, ypos-G.PLM_POS[1]);
					NXT_STS = -(5 - 1);//->5
				}
#endif
				else {
a_write("AF:開始");
					start_af(3);
				}
			break;
			case 6:
				//AF処理(終了待ち)
				if (this.FCS_STS != 0
#if true//2019.03.02(直線近似)
				 || this.FC2_STS != 0
#endif
					) {
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
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke) {
					//画面サイズ分↓へ
					MOVE_PIX_XY(0, (int)(G.CAM_HEI * (1 - G.SS.PLM_AUT_OVLP / 100.0)));
					NXT_STS = -(5 - 1);//->5
					m_adat.h_cnt = m_adat.h_idx;
					break;
				}
#endif
				//a_write("毛髪探索中:LIMIT.CHECK");
#if true//2018.07.30(終了位置指定)
				if (
#if false//2018.12.22(測定抜け対応)
					G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/ &&
#endif
					G.PLM_POS[1] >= G.SS.PLM_AUT_ED_Y) {
					//NXT_STS = 999;
					NXT_STS = 40;
				}
				else
#endif

				if ((G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					//SOFT.LIMIT(+)
					NXT_STS = 40;
				}
#if false//2019.01.11(混在対応)
				else if (retry_ypos_check(G.PLM_POS[1], out ypos)) {
					MOVE_REL_XY(0, ypos-G.PLM_POS[1]);
					NXT_STS = -(10 - 1);//->10
				}
#endif
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
#if true//2018.07.30(終了位置指定)
				if (this.AUT_STS == 12 &&
#if false//2018.12.22(測定抜け対応)
					G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/ &&
#endif
					G.PLM_POS[1] >= G.SS.PLM_AUT_ED_Y) {
					NXT_STS = 10;
					break;
				}
#endif
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
#if DEBUG//2018.12.22(測定抜け対応)
				// -849,-555,-263, +31,+274,+568,+862,(7本OFFLINE画像,XYリミットを共に±1000に設定)
				if (NXT_STS == 15 && !m_adat.nuke) {
					if (false
					 ||Math.Abs(G.PLM_POS[1]-( -729)) < 20/*06CR / 7th*/
					 ||Math.Abs(G.PLM_POS[1]-( -576)) < 20/*07CR / 8th*/
					 ||Math.Abs(G.PLM_POS[1]-( +923)) < 20/*14CR /15th*/
					 ||Math.Abs(G.PLM_POS[1]-(+1335)) < 20/*16CR /17th*/
					 ||Math.Abs(G.PLM_POS[1]-(+1641)) < 20/*17CR /18th*/
					) {
						DialogResult ret;
						this.timer2.Enabled = false;
						ret = DialogResult.Yes;
					//	ret = G.mlog("#q[デバッグ用]\r毛髪抜けデバッグのため当該毛髪をスキップさせますか?");
						if (ret == DialogResult.Yes) {
						NXT_STS = 10;//抜けデバッグのため毛髪をスキップさせる
						}
#if false//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
						this.timer2.Enabled = true;
#endif
					}
				}
#endif
				break;
			case 15:
			case 25:
			case 35:
				//毛髪エリアの垂直方向センタリング
				bool flag = true;
				yy = G.IR.CIR_RT.Top + G.IR.CIR_RT.Height/2;
				y0 = G.CAM_HEI/2;
				if (m_adat.chk1 != 0) {
					//OK(左/右移動後毛髪判定にNGのため最後の画像)
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
#if true//2019.03.18(AF順序)
				set_af_mode(this.AUT_STS);
#else
#if true//2018.11.13(毛髪中心AF)
				if ((G.LED_PWR_STS & 1) != 0) {
					//白色(透過)
					G.CNT_MOD = (G.SS.IMP_AUT_AFMD[0]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[0];
#if true//2019.02.03(WB調整)
					G.CNT_OFS = G.SS.IMP_AUT_SOFS[0];//透過(表面)
#endif
				}
				else {
					//白色(反射)
					G.CNT_MOD = (G.SS.IMP_AUT_AFMD[1]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[1];
#if true//2019.02.03(WB調整)
					G.CNT_OFS = G.SS.IMP_AUT_SOFS[1];//反射(表面)
#endif
				}
#endif
#endif
				if (this.AUT_STS == 15 && NXT_STS == 16) {
					for (int i = 0; i < 2; i++) {
						Console.Beep(1600, 250);
						Thread.Sleep(250);
					}
a_write("AF:開始");
					start_af(1/*1:1st*/);
#if true//2019.01.23(GAIN調整&自動測定)
					m_adat.gai_tune_cl_done = false;
					m_adat.gai_tune_ir_done = false;
#endif
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
#if true//2018.11.13(毛髪中心AF)
			case 616:
			case 626:
			case 636:
#endif
				//AF処理(終了待ち)
				if (this.FCS_STS != 0
#if true//2019.03.02(直線近似)
				 || this.FC2_STS != 0
#endif
					) {
					NXT_STS = this.AUT_STS;
					m_adat.chk2 = 1;
				}
				else if (m_adat.chk2 == 1) {
					NXT_STS = this.AUT_STS;
					m_adat.chk2 = 0;
#if false//2019.03.18(AF順序)
					if (m_adat.chk3 == 1) {
						m_adat.chk3 = 0;
						G.FORM02.set_size_mode(1, -1, -1);
					}
#endif
					m_dcur = m_didx;
#if true//2018.11.13(毛髪中心AF)
					if (this.AUT_STS > 600) {
a_write("AF:終了(中心)");
					} else {
#endif
a_write("AF:終了");
#if true//2018.11.13(毛髪中心AF)
					}
#endif
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
#if true//2019.01.23(GAIN調整&自動測定)
					if (this.AUT_STS == 17 && G.SS.PLM_AUT_V_PK && m_adat.gai_tune_cl_done == false) {
						NXT_STS = 700;//GAIN調整
						break;
					}
#endif
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
#if true//2018.12.22(測定抜け対応)
						if (m_adat.nuke) {
							m_adat.nuke_pos.Add(G.PLM_POS[1]);
#if true//2019.01.11(混在対応)
							m_adat.nuke_pref.Add(m_adat.pref);
#endif
						}
						else
#endif
						if (m_adat.retry == false) {
							//反射での毛髪Ｙ位置を保存して、
							//透過のときはこのＹ座標をスキップするようにする
							m_adat.y_1st_pos.Add(G.PLM_POS[1]);
#if true//2019.01.11(混在対応)
							m_adat.y_1st_pref.Add(m_adat.pref);
#endif
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
#if true//2019.04.01(表面赤外省略)
					if (G.SS.PLM_AUT_NOSF && is_sf()) {
						if (m_adat.ir_done == false) {
							m_adat.ir_done = true;
							m_adat.ir_chk1 = m_adat.chk1;
						}
					}
#endif
					if (m_adat.ir_done == false) {
						m_adat.ir_next = this.AUT_STS;
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
#if true//2018.11.13(毛髪中心AF)
				//@if (m_adat.k_cnt > 0 && m_adat.k_done) {
				//@}
				//@else
#endif
				if (m_adat.z_cnt > 1) {
					if (++m_adat.z_idx >= m_adat.z_cnt) {
						m_adat.z_idx = 0;
#if false//true//2018.11.13(毛髪中心AF)
						//@if (m_adat.k_cnt <= 0) {
//@#endif
						MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
						NXT_STS = -this.AUT_STS;
//@#if true//2018.11.13(毛髪中心AF)
						//@}
#endif
					}
					else {
						NXT_STS = (200+this.AUT_STS);
						break;
					}
				}
#if true//2018.11.13(毛髪中心AF)
				if (m_adat.k_cnt > 0 && m_adat.k_done == false) {
					m_adat.k_idx = 0;
					if (this.AUT_STS == 17/*初回AF後*/) {
						MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
					}
					else {
						MOVE_ABS_Z(m_adat.k_pre_pos_z);//Z軸を前位置のAF(中心)位置に戻す
					}
					NXT_STS =-(600-2-1+this.AUT_STS);//移動完了後に615,625,635へ
					break;
				}
				MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
				NXT_STS = -this.AUT_STS;
#endif
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
#if true//2018.08.16(右側カット)
				if (G.SS.PLM_AUT_ZNOR) {
					NXT_STS = 39;
				}
#endif

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
#if true//2019.03.18(AF順序)
				set_af_mode(1);//初期状態に戻す
#endif

				//---
				m_adat.h_idx++;
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke) {
				m_adat.n_idx++;
				}
#endif
#if true//2019.01.23(GAIN調整&自動測定)
				if (G.SS.PLM_AUT_V_PK) {
					pop_gain_ofs(false);
					if ((G.LED_PWR_STS & 1) != 0) {
						//白色(透過)
						G.FORM02.set_param(Form02.CAM_PARAM.GAIN, G.SS.CAM_PAR_GA_VL[0] + G.SS.CAM_PAR_GA_OF[0]);
					}
					else {
						//白色(反射)
						G.FORM02.set_param(Form02.CAM_PARAM.GAIN, G.SS.CAM_PAR_GA_VL[1] + G.SS.CAM_PAR_GA_OF[1]);
					}
				}
#endif
#if true//2018.08.16(Z軸再原点)
				if (G.SS.PLM_AUT_ZORG) {
					m_pre_set[2] = false;
					NXT_STS = -(500-1);	//500を経由して10へ遷移
				}
#endif
#if true//2019.02.14(Z軸初期位置戻し)
				else if (G.SS.PLM_AUT_ZRET) {//Z軸初期位置戻し
					m_pre_set[2] = true;
					m_pre_pos[2] = G.SS.PLM_POSF[3];
				}
#endif
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
#if true//2019.01.23(GAIN調整&自動測定)
				if (NXT_STS == 443 && G.SS.PLM_AUT_V_PK && m_adat.gai_tune_ir_done == false) {
					NXT_STS = 700;
				}
				else
#endif
#if true//2018.06.04 赤外同時測定
				if (this.AUT_STS == 402 || this.AUT_STS == 422) {
					NXT_STS = m_adat.ir_next;
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
#if true//2018.11.13(毛髪中心AF)
				if (m_adat.k_idx >= 0) {
					int zpos = (int)(m_adat.k_pos[m_adat.k_idx]);
					MOVE_ABS_Z(m_adat.k_pre_pos_z + zpos);
					NXT_STS = -this.AUT_STS;
				} else
#endif
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
#if true//2018.11.13(毛髪中心AF)
				if (m_adat.k_idx >= 0) {
				NXT_STS = -3-200+this.AUT_STS+600;
				break;
				}
#endif
				NXT_STS = -3-200+this.AUT_STS;
				break;
#if true//2018.08.16(Z軸再原点)
			case 500:
				D.SET_STG_ORG(2);
				G.PLM_STS |= (1 << 2);
				NXT_STS = -this.AUT_STS;
			break;
			case 501:
#if true//2019.02.14(Z軸初期位置戻し)
				if (G.SS.PLM_AUT_ZRET) {//Z軸初期位置戻し
					MOVE_ABS_Z(G.SS.PLM_POSF[3]);//FOCUS/Z軸
					NXT_STS = -(10 - 1);//->10
					break;
				}
#endif
				MOVE_ABS_Z(m_adat.sta_pos_z);
				NXT_STS = -(10 - 1);//->10
			break;
#endif
#if true//2018.11.13(毛髪中心AF)
			case 615:
			case 625:
			case 635:
#if true//2019.03.18(AF順序)
				set_af_mode(this.AUT_STS);
#else
#if true//2018.05.17
				if ((G.LED_PWR_STS & 1) != 0) {
					//白色(透過):中心用
					G.CNT_MOD = (G.SS.IMP_AUT_AFMD[2]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[2];
#if true//2019.02.03(WB調整)
					G.CNT_OFS = G.SS.IMP_AUT_COFS[0];//透過(中心)
#endif
				}
				else {
					//白色(反射):中心用
					G.CNT_MOD = (G.SS.IMP_AUT_AFMD[3]==0) ? 0: 1+G.SS.IMP_AUT_AFMD[3];
#if true//2019.02.03(WB調整)
					G.CNT_OFS = G.SS.IMP_AUT_COFS[1];//反射(中心)
#endif
				}
#endif
#endif
				if (this.AUT_STS == 615 && NXT_STS == 616) {
a_write("AF:開始(中心)");
					start_af(1/*1:1st*/);
				}
				else if (NXT_STS == (this.AUT_STS + 1)) {
					if (m_adat.chk1 != 0) {
						NXT_STS++;//AF処理をSKIP
					}
					else if (false
					 || (G.SS.PLM_AUT_FCMD == 1)
					 || (G.SS.PLM_AUT_FCMD == 2 && G.IR.CONTRAST <= (m_adat.k_sta_contrast * (1 - G.SS.PLM_AUT_CTDR / 100.0)))) {
a_write("AF:開始(中心)");
						start_af(2/*2:next*/);
					}
					else {
						NXT_STS++;//AF処理をSKIP
					}
				}
			break;
			case 617://初回AF後
			case 627://左側探索
			case 637://右側探索
#if true//2018.06.04 赤外同時測定
				if (G.SS.PLM_AUT_IRCK && m_adat.ir_done) {
					//赤外同時測定の赤外測定後
				}
				else {
#endif
				if (m_adat.k_idx == 0) {
					if (this.AUT_STS == 617) {
						m_adat.k_sta_contrast = m_contrast;
						//---
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
					//@if (true) {
					//@    m_adat.pos_x.Add(G.PLM_POS[0]);
					//@    m_adat.pos_y.Add(G.PLM_POS[1]);
					//@    m_adat.pos_z.Add(G.PLM_POS[2]);
					//@}
					//
					//@System.IO.Directory.CreateDirectory(m_adat.fold);
					//
					//@m_adat.z_cur = G.PLM_POS[2];
					m_adat.k_pre_pos_z = G.PLM_POS[2];
				}
				if (true) {
					string path0, path1, path2, path3;
					path0 = get_aut_path(-1);
					path1 = path0.Replace("@@", m_adat.f_idx.ToString());
					//path1 = get_aut_path(m_adat.f_idx);
					path2 = m_adat.fold + "\\" + path1;
					G.FORM02.save_image(path2);
					//@if (m_adat.z_idx == 0) {
					//@    m_adat.f_dum.Add(path2);
					//@    path3 = m_adat.fold + "\\" + path0;
					//@    m_adat.f_nam.Add(path3);
					//@}
					a_write(string.Format("画像保存:{0}", path1));
				}
				//画像保存
				Console.Beep(800, 250);
#if true//2018.06.04 赤外同時測定
				}
				if (G.SS.PLM_AUT_IRCK) {
#if true//2019.04.01(表面赤外省略)
					if (G.SS.PLM_AUT_NOSF && is_sf()) {
						if (m_adat.ir_done == false) {
							m_adat.ir_done = true;
							m_adat.ir_chk1 = m_adat.chk1;
						}
					}
#endif
					if (m_adat.ir_done == false) {
						m_adat.ir_next = this.AUT_STS;
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
				if (true/*m_adat.k_cnt > 0*/) {
					if (++m_adat.k_idx >= m_adat.k_cnt) {
						m_adat.k_idx =-1;
					}
					else {
						NXT_STS = 200+this.AUT_STS-600;
						break;
					}
				}
				if (true) {
					MOVE_ABS_Z(m_adat.z_cur);//Z軸を元に戻す
					NXT_STS = -(this.AUT_STS-600);
				}
				//---
				m_adat.f_cnt[m_adat.h_idx]++;
				m_adat.f_ttl++;
				break;
#endif
#if true//2019.01.23(GAIN調整&自動測定)
			case 700:
				if (G.LED_PWR_STS == 1) {
					this.timer4.Tag = 0;//透過
				}
				else if (G.LED_PWR_STS == 2) {
					this.timer4.Tag = 1;//反射
				}
				else if (G.LED_PWR_BAK == 1/*反射*/) {
					this.timer4.Tag = 3;//赤外(<-反射)
				}
				else {
					this.timer4.Tag = 2;//赤外(<-透過)
				}
				G.CNT_MOD = 0;//0:画面全体
#if true//2019.02.03(WB調整)
				G.CNT_OFS = 0;
#endif
				G.CAM_PRC = G.CAM_STS.STS_HIST;
				G.CHK_VPK = 1;
				this.GAI_STS = 1;
				this.timer4.Enabled = true;
a_write("GAIN調整:開始");
				break;
			case 701:
				//GAIN調整-終了待ち
				if (this.GAI_STS != 0) {
					NXT_STS = this.AUT_STS;
				}
				else {
a_write(string.Format("GAIN調整:終了(OFFSET={0})", G.SS.CAM_PAR_GA_OF[(int)this.timer4.Tag]));
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					if (m_adat.gai_tune_cl_done == false) {
						m_adat.gai_tune_cl_done = true;
						NXT_STS = 17;//初回AF後
					}
					else {
						m_adat.gai_tune_ir_done = true;
						NXT_STS = 443;//IRの保存へ
					}
				}
				break;
			case 702:
				break;
#endif
			case 999:
#if true//2018.12.22(測定抜け対応)
				if (m_adat.nuke) {
					//抜けチェック測定後
					rename_nuke_files();
					m_adat.nuke = false;
#if false//2019.01.11(混在対応) -> rename内でソートしてコピーするように変更
					for (int i = 0; i < m_adat.nuke_pos.Count; i++) {
						//透過リトライ用にコピーしておく
						m_adat.y_1st_pos.Add(m_adat.nuke_pos[i]);
					}
#endif
				}
				else if (G.SS.PLM_AUT_NUKE/* && G.SS.PLM_AUT_HPOS*/) {
					if (check_nuke()) {
						m_adat.nuke = true;
						NXT_STS = 1;
						break;
					}
				}
#endif
#if true//2019.01.11(混在対応)
				if (G.SS.PLM_AUT_RTRY && (G.SS.PLM_AUT_MODE == 5 || G.SS.PLM_AUT_MODE == 8)) {
					if (check_touka_retry()) {
						m_adat.nuke = true;
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
#else
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
#endif
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
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
				this.timer2.Enabled = true;
#endif
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
#if true//2019.03.14(NG画像判定)
		private void button36_Click(object sender, EventArgs e)
		{
			Form frm;
			if (G.UIF_LEVL == 0/*0:ユーザ用(暫定版)*/) {
				frm = new Form26();
			}
			else {
				frm = new Form26();
			}
#if true//2019.05.22(再測定判定(キューティクル枚数))
			G.CNT_DTHD = G.SS.CAM_HIS_DTHD;
			G.CNT_DTH2 = G.SS.CAM_HIS_DTH2;
#endif
			if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				if (G.FORM02 != null) {
					if (G.FORM02.isCONNECTED()) {
						G.FORM02.Stop();
					}
				}
				if (G.FORM04 != null) {
					G.FORM04.Close();
					G.FORM04 = null;
					Application.DoEvents();
				}
				G.FORM04 = new Form04();
				G.FORM04.Show();
			}
		}
#endif
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
#if true//2019.01.11(混在対応)
					if (m_icam == 2) {
					G.SS.CAM_PAR_GA_VL[3] = G.SS.CAM_PAR_GA_VL[2];
					G.SS.CAM_PAR_EX_VL[3] = G.SS.CAM_PAR_EX_VL[2];
					G.SS.CAM_PAR_WB_RV[3] = G.SS.CAM_PAR_WB_RV[2];
					G.SS.CAM_PAR_WB_GV[3] = G.SS.CAM_PAR_WB_GV[2];
					G.SS.CAM_PAR_WB_BV[3] = G.SS.CAM_PAR_WB_BV[2];
					}
#endif
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
#if true//2019.01.19(GAIN調整)
		private class GDATA
		{
			public double	gain_dx;
			public double	gain_val;
			public double	gain_bas;
//			public double	gain_bak;
			public double	vpk_bak;
			public double	vpk_set;
			public int		ch;
			public int chk1, chk2;
//			public int sts_bak;
//			public int chk3;
//			public int led_sts;
			//---
			//---
			public GDATA()
			{
				chk1 = 0;
				chk2 = 0;
				gain_dx = 1.0;
			}
		};
		private const double C_GAIN_MAX  = 24;
		private const double C_GAIN_MIN  = 0;
		private GDATA m_gdat = null;

		private void timer4_Tick(object sender, EventArgs e)
		{
			int NXT_STS = this.GAI_STS+1;

			this.timer4.Enabled = false;

			switch (this.GAI_STS) {
			case 0:
				break;
			case 1:
				m_gdat = new GDATA();
#if true//2019.01.23(GAIN調整&自動測定)
				if (this.timer4.Tag != null) {
					//自動測定より
					m_gdat.ch = (int)this.timer4.Tag;
					m_gdat.vpk_set = G.SS.CAM_PAR_TARVP[m_gdat.ch];
				}
				else {
					//カメラTABより
					m_gdat.ch = G.SS.CAM_GAI_LEDT;
					m_gdat.vpk_set = G.SS.CAM_GAI_VSET;
				}
				if (this.timer4.Tag != null) {
					//自動測定より
					NXT_STS = 30;//点灯制御へ
				}
				else
#endif
				if (G.CNT_MOD >= 2/*毛髪矩形 or 毛髪範囲*/) {
					NXT_STS = 0;
					G.mlog("internal error");
				}
				else {
					if (G.SS.CAM_GAI_LEDT == 0) {
						NXT_STS = 2;//点灯制御へ
					}
					if (G.SS.CAM_GAI_LEDT == 1) {
						NXT_STS = 3;//点灯制御へ
					}
					if (G.SS.CAM_GAI_LEDT >= 2) {
						NXT_STS = 20;//点灯制御へ
					}
				}
				break;
			case 2://透過点灯
				if (G.LED_PWR_STS == 1) {
					NXT_STS = 10;
				}
				else {
					G.FORM10.LED_SET(0, false);//透過
					G.FORM10.LED_SET(1, false);//反射
					G.FORM10.LED_SET(2, false);//赤外
					G.FORM10.LED_SET(0, true);//透過
					m_gdat.chk1 = Environment.TickCount;
					NXT_STS = 5;
				}
				break;
			case 3://反射点灯
				if (G.LED_PWR_STS == 2) {
					NXT_STS = 10;
				}
				else {
					G.FORM10.LED_SET(0, false);//透過
					G.FORM10.LED_SET(1, false);//反射
					G.FORM10.LED_SET(2, false);//赤外
					G.FORM10.LED_SET(1, true);//反射
					m_gdat.chk1 = Environment.TickCount;
					NXT_STS = 5;
				}
				break;
			case 5://カメラ安定待機
				if ((Environment.TickCount - m_gdat.chk1) < (G.SS.ETC_LED_WAIT*1000)) {
					NXT_STS = this.AUT_STS;
				}
				NXT_STS = 10;
				break;
			case 10:
				NXT_STS = 20;
				break;
			case 20://赤外点灯
				if (G.SS.CAM_GAI_LEDT < 2) {
					NXT_STS = 30;
				}
				else if (G.LED_PWR_STS == 4) {
					NXT_STS = 30;
				}
				else {
					G.FORM10.LED_SET(0, false);//透過
					G.FORM10.LED_SET(1, false);//反射
					G.FORM10.LED_SET(2, false);//赤外
					G.FORM10.LED_SET(2, true);//赤外
					m_gdat.chk1 = Environment.TickCount;
				}
				break;
			case 21://カメラ安定待機
				if ((Environment.TickCount - m_gdat.chk1) < (G.SS.ETC_LED_WAIT*1000)) {
					NXT_STS = this.AUT_STS;
				}
				NXT_STS = 30;
				break;
			case 30:
			case 40:
				if (true) {
					int ch = m_gdat.ch;
					m_gdat.gain_bas = G.SS.CAM_PAR_GA_VL[ch];
					m_gdat.gain_val = m_gdat.gain_bas + G.SS.CAM_PAR_GA_OF[ch];
					m_gdat.vpk_bak = double.NaN;
					if (this.GAI_STS != 40) {
					this.button29.Enabled = false;//再調整
					}
				}
				break;
			case 31:
			case 41:
				m_dcur = m_didx;
				break;
			case 32:
			case 42:
				if ((m_didx - m_dcur) < G.SS.CAM_GAI_SKIP) {
					NXT_STS = this.GAI_STS;
				}
				break;
			case 33:
				//測定
				if (true) {
					this.textBox1.Text = string.Format("{0}", G.IR.HIST_VPK);
					this.textBox2.Text = string.Format("{0:F6}", m_gdat.gain_val - m_gdat.gain_bas);
				}
				if (G.IR.HIST_VPK == m_gdat.vpk_set) {
					NXT_STS = 99;//end
				}
				else {
					if (double.IsNaN(m_gdat.vpk_bak)) {
					}
					else {
						if ((m_gdat.vpk_bak < m_gdat.vpk_set && G.IR.HIST_VPK > m_gdat.vpk_set)
						 || (m_gdat.vpk_bak > m_gdat.vpk_set && G.IR.HIST_VPK < m_gdat.vpk_set)
							) {
							if ((m_gdat.gain_dx /= 10) < 0.01) {
								NXT_STS = 99;//end
							}
						}
					}
					if (NXT_STS == 99) {
					}
					else if (G.IR.HIST_VPK < m_gdat.vpk_set) {
						if (m_gdat.gain_val < C_GAIN_MAX) {
							if ((m_gdat.gain_val += m_gdat.gain_dx) > C_GAIN_MAX) {
								m_gdat.gain_val = C_GAIN_MAX;
							}
						}
						else {
							NXT_STS = 99;//end
						}
					}
					else {
						if (m_gdat.gain_val > C_GAIN_MIN) {
							if ((m_gdat.gain_val -= m_gdat.gain_dx) < C_GAIN_MIN) {
								m_gdat.gain_val = C_GAIN_MIN;
							}
						}
						else {
							NXT_STS = 99;//end
						}
					}
				}
				m_gdat.vpk_bak = G.IR.HIST_VPK;
				break;
			case 34:
				G.FORM02.set_param(Form02.CAM_PARAM.GAIN, m_gdat.gain_val);
				NXT_STS = 31;
				break;
			case 43:
				this.textBox1.Text = string.Format("{0}", G.IR.HIST_VPK);
				NXT_STS = 41;
			break;
			case 99:
				if (this.timer4.Tag != null) {
					//自動測定より
					NXT_STS = 0;
				}
				else {
					NXT_STS = 40;
					this.button29.Enabled = true;//再調整
				}
				if (true) {
					int ch = m_gdat.ch;
					G.SS.CAM_PAR_GA_OF[ch] = m_gdat.gain_val - m_gdat.gain_bas;
				}
				if (true) {
					if (this.timer4.Tag == null) {
						for (int i = 0; i < 1; i++) {
							Console.Beep(1600, 250);
							Thread.Sleep(250);
						}
						//Thread.Sleep(3000);
					}
				}
				break;
			default:
				break;
			}
			if (NXT_STS == 0) {
				NXT_STS = 0;//for break.point
			}
			if (this.GAI_STS != 0) {
				this.GAI_STS = NXT_STS;
			}
			if (this.GAI_STS != 0) {
				this.timer4.Enabled = true;
			}
			else {
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				G.CHK_VPK = 0;
				G.FORM02.set_layout();
				UPDSTS();
			}
		}
#endif
#if true//2019.02.03(WB調整)
		private class WDATA
		{
			public double	gain_dx;
			public double	gain_val;
			//public double	gain_bas;
			public double	wbl_dif;
			public double	wbl_tol;
			public double	wbl_bak;
			public int		lch;//光源
			public int		cch;//最大値カラーR/G/B
		//	public int		mch;//最小値カラーR/G/B
			public int		ich;
		//	public List<int>idone;
			public int		chk1, chk2;
			public bool		wbauto_done;
			public bool		offset_done;
			//---
			//---
			public WDATA()
			{
				chk1 = 0;
				chk2 = 0;
			//	idone = new List<int>();
				gain_dx =-1.0;
				wbauto_done = false;
				offset_done = false;
			}
		};
		private const double C_WBL_MAX  = 7.9;//7.984375
		private const double C_WBL_MIN  = 1;
		private WDATA m_wdat = null;
		private double get_max(double f1, double f2, double f3, out int idx)
		{
			if (f1 >= f2 && f1 >= f3) {
				idx = 0;
				return(f1);
			}
			if (f2 >= f1 && f2 >= f3) {
				idx = 1;
				return(f2);
			}
			idx = 2;
			return(f3);
		}
		private double get_min(double f1, double f2, double f3, out int idx)
		{
			if (f1 <= f2 && f1 <= f3) {
				idx = 0;
				return(f1);
			}
			if (f2 <= f1 && f2 <= f3) {
				idx = 1;
				return(f2);
			}
			idx = 2;
			return(f3);
		}
		private double get_dif(double f1, double f2, double f3)
		{
			int		imin, imax;
			double	fmin = get_min(f1, f2, f3, out imin);
			double	fmax = get_max(f1, f2, f3, out imax);
			return((fmax-fmin)/fmax*100.0);
		}
		private void timer5_Tick(object sender, EventArgs e)
		{
			int NXT_STS = this.WBL_STS+1;

			this.timer5.Enabled = false;

			switch (this.WBL_STS) {
			case 0:
				break;
			case 1:
				//---
				this.textBox3.Text = "";
				this.textBox4.Text = "";
				this.textBox5.Text = "";
				this.textBox6.Text = "";
				this.textBox7.Text = "";
				this.textBox8.Text = "";
				this.textBox9.Text = "";
				//---
				m_wdat = new WDATA();
				if (this.timer5.Tag != null) {
					//自動測定より
					m_wdat.lch = (int)this.timer5.Tag;
					m_wdat.wbl_tol = G.SS.CAM_WBL_TOLE;
				}
				else {
					//カメラTABより
					m_wdat.lch = G.SS.CAM_WBL_LEDT;
					m_wdat.wbl_tol = G.SS.CAM_WBL_TOLE;
				}
				if (this.timer5.Tag != null) {
					//自動測定より
					NXT_STS = 30;//点灯制御へ
				}
				else if (G.CNT_MOD >= 2/*毛髪矩形 or 毛髪範囲*/) {
					NXT_STS = 0;
					G.mlog("internal error");
				}
				else {
					if (G.SS.CAM_WBL_LEDT == 0) {
						NXT_STS = 2;//点灯制御へ
					}
					if (G.SS.CAM_WBL_LEDT == 1) {
						NXT_STS = 3;//点灯制御へ
					}
					if (G.SS.CAM_WBL_LEDT >= 2) {
						NXT_STS = 20;//点灯制御へ
					}
				}
				break;
			case 2://透過点灯
				if (G.LED_PWR_STS == 1) {
					NXT_STS = 10;
				}
				else {
					G.FORM10.LED_SET(0, false);//透過
					G.FORM10.LED_SET(1, false);//反射
					G.FORM10.LED_SET(2, false);//赤外
					G.FORM10.LED_SET(0, true);//透過
					m_wdat.chk1 = Environment.TickCount;
					NXT_STS = 5;
				}
				break;
			case 3://反射点灯
				if (G.LED_PWR_STS == 2) {
					NXT_STS = 10;
				}
				else {
					G.FORM10.LED_SET(0, false);//透過
					G.FORM10.LED_SET(1, false);//反射
					G.FORM10.LED_SET(2, false);//赤外
					G.FORM10.LED_SET(1, true);//反射
					m_wdat.chk1 = Environment.TickCount;
					NXT_STS = 5;
				}
				break;
			case 5://カメラ安定待機
				if ((Environment.TickCount - m_wdat.chk1) < (G.SS.ETC_LED_WAIT*1000)) {
					NXT_STS = this.AUT_STS;
				}
				NXT_STS = 10;
				break;
			case 10:
				NXT_STS = 20;
				break;
			case 20://赤外点灯
				if (G.SS.CAM_WBL_LEDT < 2) {
					NXT_STS = 30;
				}
				else if (G.LED_PWR_STS == 4) {
					NXT_STS = 30;
				}
				else {
					G.FORM10.LED_SET(0, false);//透過
					G.FORM10.LED_SET(1, false);//反射
					G.FORM10.LED_SET(2, false);//赤外
					G.FORM10.LED_SET(2, true);//赤外
					m_wdat.chk1 = Environment.TickCount;
				}
				break;
			case 21://カメラ安定待機
				if ((Environment.TickCount - m_wdat.chk1) < (G.SS.ETC_LED_WAIT*1000)) {
					NXT_STS = this.AUT_STS;
				}
				NXT_STS = 30;
				break;
			case 30:
			case 40:
				if (G.SS.CAM_WBL_CHK1 && m_wdat.wbauto_done == false) {
					NXT_STS = 50;
					break;
				}
				if (G.SS.CAM_WBL_CHK2 && m_wdat.offset_done == false) {
					NXT_STS = 60;
					break;
				}
				if (true) {
					m_wdat.wbl_bak = double.NaN;
					m_wdat.gain_dx = -0.1;
					//---
					get_max(G.IR.HIST_RPK, G.IR.HIST_GPK, G.IR.HIST_BPK, out m_wdat.cch);
					m_wdat.ich = m_wdat.cch;
					//---
					int lch = m_wdat.lch;
					switch (m_wdat.cch) {
					case  0:
						m_wdat.gain_val = G.SS.CAM_PAR_WB_RV[lch];
						this.textBox3.BackColor = Color.LightGreen;
						this.textBox6.BackColor = Color.LightGreen;
					break;
					case  1:
						m_wdat.gain_val = G.SS.CAM_PAR_WB_GV[lch];
						this.textBox4.BackColor = Color.LightGreen;
						this.textBox7.BackColor = Color.LightGreen;
						break;
					default:
						m_wdat.gain_val = G.SS.CAM_PAR_WB_BV[lch];
						this.textBox5.BackColor = Color.LightGreen;
						this.textBox8.BackColor = Color.LightGreen;
						break;
					}
					G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, m_wdat.cch);
					//m_wdat.vpk_bak = double.NaN;
					if (this.WBL_STS != 40) {
					this.button29.Enabled = false;//再調整
					}
				}
				break;
			case 31:
			case 41:
				m_dcur = m_didx;
				break;
			case 32:
			case 42:
				if ((m_didx - m_dcur) < G.SS.CAM_WBL_SKIP) {
					NXT_STS = this.WBL_STS;
				}
				break;
			case 33:
			case 43:
				//測定
				if (true) {
					m_wdat.wbl_dif = get_dif(G.IR.HIST_RPK, G.IR.HIST_GPK, G.IR.HIST_BPK);
					this.textBox3.Text = string.Format("{0}", G.IR.HIST_RPK);
					this.textBox4.Text = string.Format("{0}", G.IR.HIST_GPK);
					this.textBox5.Text = string.Format("{0}", G.IR.HIST_BPK);
					this.textBox9.Text = string.Format("{0:F0}", m_wdat.wbl_dif);
				}
				if (true) {
					int lch = m_wdat.lch;
					this.textBox6.Text = string.Format("{0:F3}", m_wdat.cch == 0 ? m_wdat.gain_val: G.SS.CAM_PAR_WB_RV[lch]);
					this.textBox7.Text = string.Format("{0:F3}", m_wdat.cch == 1 ? m_wdat.gain_val: G.SS.CAM_PAR_WB_GV[lch]);
					this.textBox8.Text = string.Format("{0:F3}", m_wdat.cch == 2 ? m_wdat.gain_val: G.SS.CAM_PAR_WB_BV[lch]);
				}
				if (NXT_STS == 44) {
					NXT_STS = 41;
				}
				break;
			case 34:
				if (m_wdat.wbl_dif <= m_wdat.wbl_tol) {
					NXT_STS = 99;//end
				}
				else {
					if (double.IsNaN(m_wdat.wbl_bak)) {
					}
					else {
						int itmp;
						get_max(G.IR.HIST_RPK, G.IR.HIST_GPK, G.IR.HIST_BPK, out itmp);
						if (m_wdat.ich != itmp) {
							m_wdat.ich = itmp;
							m_wdat.gain_dx /= 10;
							if (Math.Abs(m_wdat.gain_dx) < 0.001) {
								NXT_STS = 99;//end
							}
							else {
								m_wdat.gain_dx *= -1;//方向逆転
							}
						}
					}

					if (NXT_STS == 99) {
					}
					else if (m_wdat.gain_dx > 0) {
						if (m_wdat.gain_val < C_WBL_MAX) {
							if ((m_wdat.gain_val += m_wdat.gain_dx) > C_WBL_MAX) {
								m_wdat.gain_val = C_WBL_MAX;
							}
						}
						else {
							NXT_STS = 99;//end
						}
					}
					else {
						if (m_wdat.gain_val > C_WBL_MIN) {
							if ((m_wdat.gain_val += m_wdat.gain_dx) < C_WBL_MIN) {
								m_wdat.gain_val = C_WBL_MIN;
							}
						}
						else {
							NXT_STS = 99;//end
						}
					}
				}
				m_wdat.wbl_bak = m_wdat.wbl_dif;
				break;
			case 35:
				G.FORM02.set_param(Form02.CAM_PARAM.BALANCE, m_wdat.gain_val);
				NXT_STS = 31;
				break;
			case 50:
				//WB自動実行
				G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, /*auto*/1);
				m_wdat.chk1 = Environment.TickCount;
				break;
			case 51:
			case 54:
				if (true) {
					double fval, fmax, fmin;
					int lch = m_wdat.lch;
					//---
					G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 0);
					G.FORM02.get_param(Form02.CAM_PARAM.BALANCE, out fval, out fmax, out fmin);
					G.SS.CAM_PAR_WB_RV[lch] = fval;
					//---
					G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 1);
					G.FORM02.get_param(Form02.CAM_PARAM.BALANCE, out fval, out fmax, out fmin);
					G.SS.CAM_PAR_WB_GV[lch] = fval;
					//---
					G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 2);
					G.FORM02.get_param(Form02.CAM_PARAM.BALANCE, out fval, out fmax, out fmin);
					G.SS.CAM_PAR_WB_BV[lch] = fval;
					//---
					this.textBox6.Text = string.Format("{0:F3}", G.SS.CAM_PAR_WB_RV[lch]);
					this.textBox7.Text = string.Format("{0:F3}", G.SS.CAM_PAR_WB_GV[lch]);
					this.textBox8.Text = string.Format("{0:F3}", G.SS.CAM_PAR_WB_BV[lch]);
				}
				break;
			case 52://カメラ安定待機
				if ((Environment.TickCount - m_wdat.chk1) < (G.SS.ETC_LED_WAIT*1000)) {
					NXT_STS = 51;
				}
				break;
			case 53:
				G.FORM02.set_auto(Form02.CAM_PARAM.WHITE, /*const*/0);
				break;
			case 55:
				m_wdat.wbauto_done = true;
				NXT_STS = 30;
				break;
			case 60:
				//OFFSET自動実行
				if (true) {
					int lch = m_wdat.lch;
					int itmp;
					bool flag;
					get_max(G.IR.HIST_RPK, G.IR.HIST_GPK, G.IR.HIST_BPK, out itmp);
					switch (itmp) {
					case  0:flag = (G.SS.CAM_PAR_WB_RV[lch] <= C_WBL_MIN); break;
					case  1:flag = (G.SS.CAM_PAR_WB_GV[lch] <= C_WBL_MIN); break;
					default:flag = (G.SS.CAM_PAR_WB_BV[lch] <= C_WBL_MIN); break;
					}
					if (!flag) {
						m_wdat.offset_done = true;
						NXT_STS = 30;
					}
				}
				break;
			case 61:
				if (true) {
					int lch = m_wdat.lch;
					double ofs = G.SS.CAM_WBL_OFFS;
					//---
					if ((G.SS.CAM_PAR_WB_RV[lch] += ofs) >= C_WBL_MAX) {
						G.SS.CAM_PAR_WB_RV[lch] = C_WBL_MAX;
					}
					if ((G.SS.CAM_PAR_WB_GV[lch] += ofs) >= C_WBL_MAX) {
						G.SS.CAM_PAR_WB_GV[lch] = C_WBL_MAX;
					}
					if ((G.SS.CAM_PAR_WB_BV[lch] += ofs) >= C_WBL_MAX) {
						G.SS.CAM_PAR_WB_BV[lch] = C_WBL_MAX;
					}
					G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 0);
					G.FORM02.set_param(Form02.CAM_PARAM.BALANCE, G.SS.CAM_PAR_WB_RV[lch]);
					G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 1);
					G.FORM02.set_param(Form02.CAM_PARAM.BALANCE, G.SS.CAM_PAR_WB_GV[lch]);
					G.FORM02.set_param(Form02.CAM_PARAM.BAL_SEL, 2);
					G.FORM02.set_param(Form02.CAM_PARAM.BALANCE, G.SS.CAM_PAR_WB_BV[lch]);
				}
				m_wdat.chk1 = Environment.TickCount;
				break;
			case 62:
				if (true) {
					int lch = m_wdat.lch;
					//---
					this.textBox6.Text = string.Format("{0:F3}", G.SS.CAM_PAR_WB_RV[lch]);
					this.textBox7.Text = string.Format("{0:F3}", G.SS.CAM_PAR_WB_GV[lch]);
					this.textBox8.Text = string.Format("{0:F3}", G.SS.CAM_PAR_WB_BV[lch]);
				}
				break;
			case 63://カメラ安定待機
				if ((Environment.TickCount - m_wdat.chk1) < (G.SS.ETC_LED_WAIT*1000)) {
					NXT_STS = this.WBL_STS;
				}
				else {
					m_wdat.offset_done = true;
					NXT_STS = 30;
				}
				break;
			case 99:
				//if (m_wdat.wbl_dif > m_wdat.wbl_tol && m_wdat.idone.Count() < 3) {
				//    NXT_STS = 30;
				//}
				this.textBox3.BackColor = SystemColors.Control;
				this.textBox6.BackColor = SystemColors.Control;
				this.textBox4.BackColor = SystemColors.Control;
				this.textBox7.BackColor = SystemColors.Control;
				this.textBox5.BackColor = SystemColors.Control;
				this.textBox8.BackColor = SystemColors.Control;
				break;
			case 100:
				//if (this.timer5.Tag != null) {
				//    //自動測定より
				//    NXT_STS = 0;
				//}
				//else {
				//    NXT_STS = 41;
				//    m_wdat.cch = -1;
				//    this.button32.Enabled = true;//再調整
				//}
				if (true) {
					int lch = m_wdat.lch;
					switch (m_wdat.cch) {
					case  0:G.SS.CAM_PAR_WB_RV[lch] = m_wdat.gain_val; break;
					case  1:G.SS.CAM_PAR_WB_GV[lch] = m_wdat.gain_val; break;
					default:G.SS.CAM_PAR_WB_BV[lch] = m_wdat.gain_val; break;
					}
				}
				if (true) {
					if (this.timer5.Tag == null) {
						for (int i = 0; i < 1; i++) {
							Console.Beep(1600, 250);
							Thread.Sleep(250);
						}
						//Thread.Sleep(3000);
					}
				}
				if (this.timer5.Tag != null) {
					//自動測定より
					NXT_STS = 0;
				}
				else {
					NXT_STS = 41;
					m_wdat.cch = -1;
					this.button32.Enabled = true;//再調整
				}
				break;
			default:
				break;
			}
			if (NXT_STS == 0) {
				NXT_STS = 0;//for break.point
			}
			if (this.WBL_STS != 0) {
				this.WBL_STS = NXT_STS;
			}
			if (this.WBL_STS != 0) {
				this.timer5.Enabled = true;
			}
			else {
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				G.CHK_WBL = 0;
				G.FORM02.set_layout();
				UPDSTS();
			}
		}

		private void checkBox14_CheckedChanged(object sender, EventArgs e)
		{
			this.numericUpDown47.Enabled = this.checkBox14.Checked;
		}
#endif
#if true//2019.02.27(ＡＦ２実装)
		private void get_max_2nd(List<double>cont,/* List<int>zpos,*/ out int imax, out double cmax, out double c2nd)
		{
			imax = 0;
			cmax = cont[0];

			for (int i = 1; i < cont.Count; i++) {
				if (cmax < cont[i]) {
					cmax = cont[i];
					imax = i;
				}
			}

			//m_dat.imax = imax;
			//m_dat.cmax = cmax;
			//m_dat.zmax = zpos[imax];
			if (imax >= cont.Count-1) {//右端
    			c2nd = cont[imax-1];
			}
			else if (imax == 0) {//左端
    			c2nd = cont[imax+1];
			}
			else if (cont[imax-1] > cont[imax+1]) {
				c2nd = cont[imax-1];
			}
			else {
				c2nd = cont[imax+1];
			}
			return;
		}
#if true//2019.04.04(微分閾値追加)
		private bool is_dropped()
		{
			bool ret;
			if (G.SS.CAM_FC2_DTYP == 0) {
				ret = ((m_dat.cmax - m_contrast) >= G.SS.CAM_FC2_DROP);
			}
			else {
				ret = (m_dat.dcnt >= G.SS.CAM_FC2_DCNT);//ドロップ回数
#endif
			}
			return(ret);
		}
#endif
		private void timer6_Tick(object sender, EventArgs e)
		{
			int NXT_STS = this.FC2_STS+1;
			//double fmax;
			//int imax;

			this.timer6.Enabled = false;

			switch (this.FC2_STS) {
			case 0:
				break;
			case 1:
				if (true/*CONTRAST*/ && G.CNT_MOD >= 2/*毛髪矩形 or 毛髪範囲*/) {
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
				if ((m_didx - m_dcur) < G.SS.CAM_FC2_SKIP) {
					NXT_STS = this.FC2_STS;
				}
				else if (G.IR.CIR_CNT <= 0) {
					this.FC2_STS = 0;
					timer6.Enabled = false;
					if (this.timer6.Tag == null) {
						//カメラTABより
						G.mlog("#e毛髪が検出できませんした.");
					}
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
				if (G.SS.CAM_FC2_CHK1) {
					DateTime dt = DateTime.Now;
					m_path = T.GetDocFolder();
					m_path += "\\";
					m_path += "AF\\";
					if (System.IO.Directory.Exists(m_path) == false) {
						System.IO.Directory.CreateDirectory(m_path);
					}
					m_path += string.Format("{0:0000}{1:00}{2:00}-{3:00}{4:00}{5:00}",
							dt.Year, dt.Month, dt.Day,
							dt.Hour, dt.Minute, dt.Second);
					m_path += string.Format("(X,Y,Z={0},{1},{2})", G.PLM_POS[0], G.PLM_POS[1], G.PLM_POS[2]);
					m_path += ".csv";
					f_write(m_path);
				}

				if (this.timer6.Tag == null) {
					//カメラTABより
					m_pmin = G.SS.CAM_FC2_LMIN;
					m_pmax = G.SS.CAM_FC2_LMAX;
				}
				else if ((int)this.timer6.Tag == 1) {
				    int tmp = G.PLM_POS[2] - G.SS.PLM_OFFS[2];
				    m_pmin = tmp - G.SS.PLM_AUT_HANI;
				    m_pmax = tmp + G.SS.PLM_AUT_HANI;
				}
				else if ((int)this.timer6.Tag == 2) {
				    int tmp = G.PLM_POS[2] - G.SS.PLM_OFFS[2];
				    m_pmin = tmp - G.SS.PLM_AUT_2HAN;
				    m_pmax = tmp + G.SS.PLM_AUT_2HAN;
				}
#if true//2019.05.12(縦型対応)
				else if ((int)this.timer6.Tag == 9) {
				    int tmp = G.PLM_POS[2] - G.SS.PLM_OFFS[2];
				    m_pmin = tmp - G.SS.TAT_AFC_HANI;
				    m_pmax = tmp + G.SS.TAT_AFC_HANI;
				}
#endif
				else {
					m_pmin = G.SS.PLM_AUT_HPMN;
					m_pmax = G.SS.PLM_AUT_HPMX;
				}
				m_pmin -= G.SS.CAM_FC2_DPLS;
				m_pmax += G.SS.CAM_FC2_DPLS;
#if true//2018.05.22(バックラッシュ方向反転対応)
				if (G.SS.PLM_BSLA[2] < 0) {
					if (m_pmin < m_pmax) {
						int tmp = m_pmin;
						m_pmin = m_pmax;
						m_pmax = tmp;
					}
				}
#endif
				break;
			case 12:
				//f軸-> min
				MOVE_ABS_Z(m_pmin);
				NXT_STS = -this.FC2_STS;
				break;
			case 13:
				D.SET_FCS_SPD(G.SS.CAM_FC2_FSPD);
				break;
			case 14:
			case 20:
				m_fcnt = 0;
				m_contrast = 0;
				m_dcur = m_didx;
				break;
			case 15:
			case 21:
				if ((m_didx - m_dcur) < G.SS.CAM_FC2_SKIP) {
					NXT_STS = this.FC2_STS;
				}
				else {
					m_fcnt++;
				}
			break;
			case 16:
				G.IR.FC2_POS = new List<int>();
				G.IR.FC2_CTR = new List<double>();
				G.FC2_DONE = 0;
				G.FC2_FLG = true;
				m_dat.dt = DateTime.Now;
				MOVE_ABS_Z(m_pmax);
			break;
			case 17:
				//f軸停止待ち
				if ((G.PLM_STS & (1 << 2)) != 0) {
					NXT_STS = this.FC2_STS;
					if (G.FC2_DONE == 2) {
						G.FC2_DONE++;
						D.SET_STG_STOP(2);
					}
				}
				else {
					D.RESET_FCS_SPD();
					G.FC2_FLG = false;
				}
			break;
			case 18:
				m_dat.s = double.NaN;
				m_dat.l = double.NaN;
				m_dat.c = double.NaN;
				m_dat.p = double.NaN;
				m_dat.AF2 = true;
				if (G.IR.FC2_POS.Count <= 3 || G.IR.FC2_CTR.Count != G.IR.FC2_POS.Count) {
					G.mlog("探索スピードが速すぎるか探索範囲が狭すぎます.");
					this.FC2_STS = 0;
					break;
				}
				if (true) {
					//全データを保存する
					DateTime sta = m_dat.dt;
					TimeSpan ela = (DateTime.Now-m_dat.dt);
					double itv = ela.Seconds/(G.IR.FC2_CTR.Count-1);
					for (int i = 0; i < G.IR.FC2_CTR.Count; i++) {
						m_dat.dt       = sta.AddSeconds((int)(itv*i));
						m_dat.pos      = G.IR.FC2_POS[i];
						m_dat.contrast = G.IR.FC2_CTR[i];
						if (G.SS.CAM_FC2_CHK1) {
							f_write(m_path, m_dat);
						}
					}
					if (G.SS.CAM_FC2_CHK1) {
						f_write(m_path, "***精密探索***");
					}
				}
				break;
			case 19:
				if (true) {
					List<int> zpos = new List<int>();
					List<double>cont = new List<double>();
					zpos.Add(G.IR.FC2_POS[0]);
					cont.Add(G.IR.FC2_CTR[0]);
					for (int i = 1; i < G.IR.FC2_CTR.Count; i++) {
						if (G.IR.FC2_POS[i] == zpos[zpos.Count-1]) {
							if (G.IR.FC2_CTR[i] > cont[cont.Count-1]) {
								//同一のZPOSでコントスラトが高ければ書き換え
								cont.RemoveAt(cont.Count-1);
								cont.Add(G.IR.FC2_CTR[i]);
							}
							else {
								//当該データはスキップ
							}
						}
						else {
							zpos.Add(G.IR.FC2_POS[i]);
							cont.Add(G.IR.FC2_CTR[i]);
						}
					}
#if true//2019.03.02(直線近似)
					//---
					if (zpos.Count <= 3) {
						G.mlog("探索スピードが速すぎるか探索範囲が狭すぎます.");
						this.FC2_STS = 0;
						break;
					}
#endif
					//---
					get_max_2nd(cont, out m_dat.imax, out m_dat.cmax, out m_dat.c2nd);
					m_dat.zmax = zpos[m_dat.imax];
					m_dat.l_cont = new List<double>();
					m_dat.l_zpos = new List<int>();
					m_dat.drop = false;
					m_dat.cmax = 0;
				}
                MOVE_ABS_Z(m_dat.zmax-G.SS.CAM_FC2_DPLS);
				NXT_STS = -this.FC2_STS;
				break;
			case 22:
				m_contrast += G.IR.CONTRAST;
				if (m_fcnt < G.SS.CAM_FC2_FAVG) {
					m_dcur++;
					NXT_STS = 21;
				}
				else {
					m_contrast /= m_fcnt;
				}
				break;
			case 23:
				if (true) {
					m_dat.l_cont.Add(m_contrast);
					m_dat.l_zpos.Add(G.PLM_POS[2]);
				}
				if (G.SS.CAM_FC2_CHK1) {
					m_dat.dt       = DateTime.Now;
					m_dat.pos      = G.PLM_POS[2];
					m_dat.contrast = m_contrast;
					f_write(m_path, m_dat);
				}
				break;
			case 24:
				if (m_dat.cmax < m_contrast) {
					m_dat.cmax = m_contrast;
#if true//2019.04.04(微分閾値追加)
					m_dat.dcnt = 0;//ドロップ回数
#endif
				}
#if true//2019.04.04(微分閾値追加)
				else {
					m_dat.dcnt++;//ドロップ回数
				}
#endif
				if (m_dat.drop == true && m_contrast >= m_dat.cthr) {
					//検索終了(最大と次点の中間値以上の検知で終了とする)
					m_dat.cmax = m_contrast;
				}
				else if (
#if true//2019.04.04(微分閾値追加)
					is_dropped()
#else
					(m_dat.cmax - m_contrast) >= G.SS.CAM_FC2_DROP
#endif
					) {
					//精密探索をやり直し
					int imax, zpos;
					get_max_2nd(m_dat.l_cont, out imax, out m_dat.cmax, out m_dat.c2nd);
					zpos = m_dat.l_zpos[imax];
					m_dat.l_cont.Clear();
					m_dat.l_zpos.Clear();
					m_dat.cthr = (m_dat.cmax+m_dat.c2nd)/2;
					m_dat.drop = true;
					m_dat.cmax = 0;
#if true//2019.04.04(微分閾値追加)
					m_dat.dcnt = 0;//ドロップ回数
#endif
					m_contrast = 0;
#if true//2019.03.02(直線近似)
					MOVE_ABS_Z(zpos-G.SS.CAM_FC2_BPLS);
#else
					MOVE_ABS_Z(zpos);
#endif
					NXT_STS = -(20 - 1);//->20
					if (G.SS.CAM_FC2_CHK1) {
						f_write(m_path, "***コントラストドロップ検知***");
					}
				}
				else {
					//1PLS進める
					MOVE_REL_Z(1);
					NXT_STS = -(20 - 1);//->20
				}
				break;
			case 25:
				if (true) {
					G.CAM_PRC = G.CAM_STS.STS_NONE;
					this.FC2_STS = 0;
				}
				f_write(m_path, string.Format(",,,MAXPOS,{0},,,,CONTRAST,{1}", G.PLM_POS[2], m_dat.cmax));
				if (this.timer6.Tag == null) {
					for (int i = 0; i < 1; i++) {
						Console.Beep(1600, 250);
						Thread.Sleep(250);
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
						NXT_STS = this.FC2_STS;
					}
					else {
						NXT_STS = (-this.FC2_STS) + 1;
					}
				}
				else {
					NXT_STS = this.FC2_STS;
				}
				break;
			}
			//---
			if (this.FC2_STS != 0) {
				this.FC2_STS = NXT_STS;
				this.timer6.Enabled = true;
			}
			else {
				D.RESET_FCS_SPD();
				NXT_STS = NXT_STS;//for break.point
			}
		}
#if true//2019.03.02(直線近似)
		private void checkBox15_Click(object sender, EventArgs e)
		{
			this.numericUpDown27.Enabled = this.checkBox15.Checked;
			this.numericUpDown59.Enabled = this.checkBox15.Checked;
			this.checkBox7.Enabled       = this.checkBox15.Checked;
			this.numericUpDown15.Enabled =!this.checkBox15.Checked;
		}
#endif
#if true//2019.04.04(微分閾値追加)
		private void radioButton11_Click(object sender, EventArgs e)
		{
			if (this.radioButton11.Checked) {
				this.numericUpDown48.Enabled = true;
				this.numericUpDown61.Enabled = false;
			}
			else {
				this.numericUpDown48.Enabled = false;
				this.numericUpDown61.Enabled = true;
			}
		}
#endif
#if true//2019.04.09(再測定実装)
		private void button37_Click(object sender, EventArgs e)
		{
			do_re_mes();
		}
		public void do_re_mes()
		{
			//G.mlog("FORM12::do_re_mes()");

			//再測定画面をクローズ
			if (G.FORM04 != null) {
				G.FORM04.Close();
				G.FORM04 = null;
				Application.DoEvents();
			}
			//カメラ画面をオープン
			if (G.FORM02 == null) {
				G.FORM02 = new Form02();
				G.FORM02.Show();
				CAM_INIT();
				if (!G.FORM02.isCONNECTED()) {
					G.mlog("カメラのオープンに失敗しました.");
					return;
					//カメラ画面をオープンできず
				}
			}
			DlgProgress
					prg = new DlgProgress();
			int bak_of_mode = G.SS.PLM_AUT_MODE;
#if true//2018.12.22(測定抜け対応)
			m_adat = new ADATA();
#endif
			m_adat.trace = false;
			m_adat.retry = false;
			//---
			m_rdat = new RDATA();
#if true//2018.04.08(ＡＦパラメータ)
			G.push_af2_para();
#endif
			try {
				prg.Show("再測定", G.FORM01);
				prg.SetStatus("実行中...");
				G.CAM_PRC = G.CAM_STS.STS_AUTO;
#if true//2019.01.23(GAIN調整&自動測定)
				if (G.SS.PLM_AUT_V_PK) {
					push_gain_ofs();
				}
#endif
				this.AUT_STS = 1;
				timer7.Enabled = true;
				while (this.AUT_STS != 0) {
					try {
					Application.DoEvents();
					}
					catch (Exception ex) {
						G.mlog(ex.Message);
					}
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
					if (false) {
					}
					else if (this.AUT_STS >= 5 && this.AUT_STS <= 6) {
						buf += "AF位置探索\r\r";
					}
					else {
						buf += string.Format("画像 {0}/{1}枚目\r\r", 1+m_rdat.r_idx, G.REMES.Count);
					}
					if (false/*bWAIT*/) {
					//	buf += "待機中";
					}
					else if (this.FCS_STS != 0) {
						if (this.AUT_STS > 600) {
							buf += "フォーカス(中心)";
						}
						else {
							buf += "フォーカス(表面)";
						}
					}
					else if (this.AUT_STS < 20) {
						buf += "探索中";
					}
#if true//2019.01.23(GAIN調整&自動測定)
					else if (this.AUT_STS >= 700 && this.AUT_STS <= 701) {
						buf += "GAIN調整中";
					}
#endif
					else if (true) {
					//	buf += (m_adat.f_idx <= 50) ? "左側" : "右側";
						buf += "測定中";
					}
					else {
						//buf += string.Format("{0}/{1}", 1+m_adat.f_idx, m_adat.f_cnt[m_adat.h_idx]);
						buf += "CUR/TTL";
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
#if true//2019.01.23(GAIN調整&自動測定)
			if (G.SS.PLM_AUT_V_PK) {
				pop_gain_ofs();
			}
#endif
#if true//2018.04.08(ＡＦパラメータ)
			G.pop_af2_para();
#endif
			//---
			G.SS.PLM_AUT_MODE = bak_of_mode;//リトライ時に書き変わるため元に戻す
		}


		class RDATA
		{
			public string bup_fold;
//			public int cnt;
			public int r_idx;
			//---
			public bool bONKP;//true:中心処理中, false:表面処理中
			public bool bEXHR;//毛髪切り替わり
			//---
			public int k_idx;
			public int z_idx;
			public int g_nxt;
			//---
			public RDATA()
			{
//				dt = DateTime.Now;
				bup_fold = "";
				bEXHR = false;
//				cnt = 0;
				r_idx = 0;
				k_idx = 0;
				z_idx = 0;
			}
		};
#if true//2019.05.08(再測定・深度合成)
		private void move_zdept_bakup(G.RE_MES mes)
		{
			if (System.IO.File.Exists(mes.path_of_zp)) {
				string src = mes.path_of_zp;
				string dst = m_rdat.bup_fold + "\\" + System.IO.Path.GetFileName(mes.path_of_zp);
				System.IO.File.Move(src,  dst);
			}
		}
#endif

		private RDATA m_rdat = null;

		private void timer7_Tick(object sender, EventArgs e)
		{
			int NXT_STS = this.AUT_STS + 1;
			int yy, y0;
			G.RE_MES mes = null;
			
			if (m_rdat.r_idx < G.REMES.Count) {
				mes = G.REMES[m_rdat.r_idx];
			}
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
			this.timer7.Enabled = false;
#endif
			if (G.bCANCEL) {
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				this.AUT_STS = 0;
				G.bCANCEL = false;
			}
#if true
			if (this.AUT_STS >= 700 && this.AUT_STS <= 702) {
			}
			else if (this.AUT_STS == 116 || this.AUT_STS == 616) {
			}
			else {
				this.AUT_STS = this.AUT_STS;//FOR BP
			}
#endif
#if DEBUG//2019.01.23(GAIN調整&自動測定)
//System.Diagnostics.Debug.WriteLine("{0}:STS={1},DIDX={2}", Environment.TickCount, this.AUT_STS, m_didx);
#endif
			switch (this.AUT_STS) {
			case 0:
				this.timer7.Enabled = false;
				break;
			case 1:
				if (true) {
					DateTime dt = DateTime.Now;
					string buf = "";
					buf = string.Format("{0:0000}{1:00}{2:00}_{3:00}{4:00}{5:00}",
									dt.Year,
									dt.Month,
									dt.Day,
									dt.Hour,
									dt.Minute,
									dt.Second);
					m_adat.fold     = G.REMES[0].fold;
					m_adat.log      = m_adat.fold + "\\log.csv";
					m_rdat.bup_fold = G.REMES[0].fold + "\\BAK_" + buf;
					m_rdat.bEXHR    = true;
					//if (m_adat.fold.Last() != '\\') {
					//	m_adat.fold += "\\";
					//}
					//m_adat.fold += buf;
					//m_adat.ext = FLTP2STR(G.SS.PLM_AUT_FLTP);
					if (System.IO.Directory.Exists(m_rdat.bup_fold) == false) {
						System.IO.Directory.CreateDirectory(m_rdat.bup_fold);
					}
					a_write("再測定(" + buf + ")");
				}
				if (true) {
					m_adat.gai_tune_cl_done = false;
					m_adat.gai_tune_ir_done = false;
				}
				break;
			case 2:
				//---
				if (mes.crt == "CT" && (G.LED_PWR_STS & 1) == 0) {
					//光源=>白色(透過)
					NXT_STS = 70;//70->71->72->73->3として白色点灯->安定待機後に戻ってくる
				}
				if (mes.crt == "CR" && (G.LED_PWR_STS & 2) == 0) {
					//光源=>白色(反射)
					NXT_STS = 70;//70->71->72->73->3として白色点灯->安定待機後に戻ってくる
				}
				if (NXT_STS == 70) {
					MOVE_ABS_XY(mes.pls_x, mes.pls_y);
				}
				if (true) {
					m_rdat.k_idx = 0;
					m_rdat.z_idx = 0;
				}
				break;
			case 3:
				MOVE_ABS_XY(mes.pls_x, mes.pls_y);
				NXT_STS = -this.AUT_STS;
				break;
			case 4:
				//Z原点
				if (m_rdat.bEXHR && G.SS.PLM_AUT_ZORG) {
					m_rdat.bEXHR = false;
					//---
					m_pre_set[2] = false;
					m_rdat.g_nxt = NXT_STS;
					NXT_STS = 500;	//500を経由して5へ遷移
				}
				break;
			case 5:
				if (!G.SS.IMP_AUT_EXAF) {
					//表面ＡＦから実行
					MOVE_ABS_Z(mes.pls_z_of_zp);
				}
				else {
					//中心ＡＦから実行
					MOVE_ABS_Z(mes.pls_z_of_kp);
				}
				if (true) {
					set_af_mode(1000);
				}
				NXT_STS = -this.AUT_STS;
				break;
			case 6:
				m_adat.chk1 = 0;
				NXT_STS = 12;
				break;
			case 7:
			break;
			case 10:
				//次の画像測定へ
				if (++m_rdat.r_idx >= G.REMES.Count) {
					NXT_STS = 998;//開始位置へ移動後に終了
				}
				else {
					if (G.REMES[m_rdat.r_idx].hno != mes.hno) {
						//次の画像が現在と異なる毛髪ならゲイン調整済フラグをクリア
						m_adat.gai_tune_cl_done = false;
						m_adat.gai_tune_ir_done = false;
						m_rdat.bEXHR = true;
					}
					NXT_STS = 2;
				}
				break;
			case 11:
				break;
			case 12:
				m_dcur = m_didx;
				break;
			case 13:
				if ((m_didx - m_dcur) < G.SS.PLM_AUT_SKIP) {
					NXT_STS = this.AUT_STS;//画面が更新されるまで
				}
				break;
			case 14:
				//測定
				if (G.IR.CIR_CNT <= 0) {
					//毛髪判定NG
a_write("毛髪判定(中心):NG");
					G.mlog("毛髪の検出ができませんでした.\r画像\r\t" + mes.name_of_kp[0] + "\rの測定をスキップします.");
					NXT_STS = 10;
				}
				else {
a_write("毛髪判定(中心):OK");
					NXT_STS = NXT_STS;
				}
				break;
			case 15:
				//毛髪エリアの垂直方向センタリング
				bool flag = true;
				yy = G.IR.CIR_RT.Top + G.IR.CIR_RT.Height/2;
				y0 = G.CAM_HEI/2;
				if (m_adat.chk1 != 0) {
					//OK(左/右移動後毛髪判定にNGのため最後の画像)
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
						NXT_STS = -(this.AUT_STS - 3 - 1);		// -> 12
					}
				}
				else {
					flag = flag;
				}
#if true//2019.03.18(AF順序)
				set_af_mode(1000);
#endif
//@@			if (this.AUT_STS == 15 && NXT_STS == 16) {
//@@				for (int i = 0; i < 2; i++) {
//@@					Console.Beep(1600, 250);
//@@					Thread.Sleep(250);
//@@				}
//@@			}
				break;
			case  16:
//@@			set_af_mode(this.AUT_STS);
				if (!G.SS.IMP_AUT_EXAF) {
					//表面ＡＦから実行
					NXT_STS = 116;
a_write("AF:開始(表面)");
				}
				else {
					//中心ＡＦから実行
					NXT_STS = 616;
a_write("AF:開始(中心)");
				}
				for (int i = 0; i < 2; i++) {
					Console.Beep(1600, 250);
					Thread.Sleep(250);
				}
				start_af(1/*1:1st*/);
				break;
			case 116://表面
			case 616://中心
				//AF処理(終了待ち)
				if (this.FCS_STS != 0 || this.FC2_STS != 0) {
					NXT_STS = this.AUT_STS;
					m_adat.chk2 = 1;
				}
				else if (m_adat.chk2 == 1) {
					NXT_STS = this.AUT_STS;
					m_adat.chk2 = 0;
					m_dcur = m_didx;
#if true//2018.11.13(毛髪中心AF)
					if (this.AUT_STS > 600) {
a_write("AF:終了(中心)");
					} else {
a_write("AF:終了(表面)");
					}
#endif
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
				}
				else if ((m_didx - m_dcur) < (G.SS.PLM_AUT_SKIP+3)) {
					NXT_STS = this.AUT_STS;
				}
				else {
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
				}
				break;
			case 117://表面
			case 617://中心
				//初回AF後,
				if (true) {
					m_adat.z_cur = G.PLM_POS[2];
				}
				if (G.SS.PLM_AUT_V_PK && m_adat.gai_tune_cl_done == false) {
					m_rdat.g_nxt = NXT_STS;
					NXT_STS = 700;//GAIN調整
				}
				break;
			case 118://表面
			case 618://中心
				if (true/*@@ this.AUT_STS == 17*/) {
					//--- ONCE
					if (G.SS.PLM_AUT_CNST) {
						if (G.CAM_GAI_STS == 1 || G.CAM_EXP_STS == 1 || G.CAM_WBL_STS == 1) {/*1:自動*/
							set_expo_const();
						}
					}
				}
				break;
			case 119://表面
#if true//2019.05.08(再測定・深度合成)
				if (mes.path_of_zp.Contains("ZDEPT")) {
					move_zdept_bakup(mes);
				}
#endif
				if (true) {
					string name = mes.name_of_zp[m_rdat.z_idx];
					string path = mes.fold + "\\" + name;
					System.IO.File.Copy(path, m_rdat.bup_fold +  "\\" + name);
					G.FORM02.save_image(path);
					a_write(string.Format("画像保存:{0}", path));
				}
				//画像保存
				Console.Beep(800, 250);
				break;
			case 619://中心
#if true//2019.05.08(再測定・深度合成)
				if (mes.path_of_zp.Contains("ZDEPT")) {
					move_zdept_bakup(mes);
				}
#endif
				if (true) {
					string name = mes.name_of_kp[m_rdat.k_idx];
					string path = mes.fold + "\\" + name;
					System.IO.File.Copy(path, m_rdat.bup_fold +  "\\" + name);
					G.FORM02.save_image(path);
					a_write(string.Format("画像保存:{0}", path));
				}
				//画像保存
				Console.Beep(800, 250);
				break;
			case 120://表面
				if (!string.IsNullOrEmpty(mes.name_of_zr[m_rdat.z_idx])) {
					//赤外測定：有り
					if (m_adat.ir_done == false) {
						m_rdat.bONKP   = false;
						m_adat.ir_next = NXT_STS;
						m_adat.ir_lsbk = G.LED_PWR_STS;
						m_adat.ir_chk1 = m_adat.chk1;
						NXT_STS = 440;//赤外に切替
					}
				}
				break;
			case 620://中心
				if (!string.IsNullOrEmpty(mes.name_of_kr[m_rdat.z_idx])) {
					//赤外測定：有り
					if (m_adat.ir_done == false) {
						m_rdat.bONKP   = true;
						m_adat.ir_next = NXT_STS;
						m_adat.ir_lsbk = G.LED_PWR_STS;
						m_adat.ir_chk1 = m_adat.chk1;
						NXT_STS = 440;//赤外に切替
					}
				}
				break;
			case 121://表面:赤外測定後
				if (++m_rdat.z_idx < mes.name_of_zr.Count) {
					//表面画像:次のオフセット位置
					MOVE_ABS_Z(m_adat.z_cur + mes.offs_of_zp[m_rdat.z_idx]);
					NXT_STS = -this.AUT_STS;
				}
				else {
					NXT_STS = 125;//表面画像：終了
				}
				break;
			case 621://中心:赤外測定後
				if (++m_rdat.k_idx < mes.name_of_kr.Count) {
					//中心画像:次のオフセット位置
					MOVE_ABS_Z(m_adat.z_cur + mes.offs_of_kp[m_rdat.k_idx]);
					NXT_STS = -this.AUT_STS;
				}
				else {
					NXT_STS = 625;//中心画像：終了
				}
				break;
			case 122://表面
			case 622://中心
				m_dcur = m_didx;
				break;
			case 123://表面
			case 623://中心
				if ((m_didx - m_dcur) < G.SS.PLM_AUT_SKIP) {
					NXT_STS = this.AUT_STS;//画面が更新されるまで
				}
				break;
			case 124://表面
			case 624://中心
				NXT_STS = (this.AUT_STS-5);//->保存
				break;
			case 125://表面
				if (!G.SS.IMP_AUT_EXAF) {
					//表面ＡＦ→中心ＡＦ
					set_af_mode(2000);
a_write("AF:開始(表面)");
					start_af(1/*1:1st*/);
					NXT_STS = 616;
				}
				else {
					//現在位置の画像は測定終了：次の位置の画像へ
					NXT_STS = 10;
				}
				break;
			case 625://中心
				if (G.SS.IMP_AUT_EXAF) {
					//中心ＡＦ→表面ＡＦ
					set_af_mode(2000);
a_write("AF:開始(中心)");
					start_af(1/*1:1st*/);
					NXT_STS = 116;
				}
				else {
					//現在位置の画像は測定終了：次の位置の画像へ
					NXT_STS = 10;
				}
				break;
			case 126:
			case 626:
				NXT_STS = 10;//->10
				//---
#if true//2019.03.18(AF順序)
				set_af_mode(1);//初期状態に戻す
#endif
#if true//2019.01.23(GAIN調整&自動測定)
				if (G.SS.PLM_AUT_V_PK) {
					pop_gain_ofs(false);
					if ((G.LED_PWR_STS & 1) != 0) {
						//白色(透過)
						G.FORM02.set_param(Form02.CAM_PARAM.GAIN, G.SS.CAM_PAR_GA_VL[0] + G.SS.CAM_PAR_GA_OF[0]);
					}
					else {
						//白色(反射)
						G.FORM02.set_param(Form02.CAM_PARAM.GAIN, G.SS.CAM_PAR_GA_VL[1] + G.SS.CAM_PAR_GA_OF[1]);
					}
				}
#endif
				break;
			case 400://赤外同時測定
				//光源切り替え(->透過)
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, false);//赤外
				G.FORM10.LED_SET(0, true );//透過
				m_adat.pref = "CT";//白色(透過)
				m_adat.chk1 = Environment.TickCount;
a_write("光源切替:->透過");
				G.CAM_PRC = G.CAM_STS.STS_AUTO;
				break;
			case 420://赤外同時測定
				//光源切り替え(->反射)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(2, false);//赤外
				G.FORM10.LED_SET(1, true );//反射
				m_adat.pref = "CR";//白色(反射)
				m_adat.chk1 = Environment.TickCount;
a_write("光源切替:->反射");
				G.CAM_PRC = G.CAM_STS.STS_AUTO;
				break;
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
			case 401://'透過'切替後
			case 421://'反射'切替後
			case 441://'赤外'切替後
			break;
			case 72:
			case 402://'透過'切替後
			case 422://'反射'切替後
			case 442://'赤外'切替後
				//カメラ安定待機
				if ((Environment.TickCount - m_adat.chk1) < (G.SS.ETC_LED_WAIT*1000)) {
					NXT_STS = this.AUT_STS;
				}
				break;
			case 403://'透過'切替後
			case 423://'反射'切替後
			case 443://'赤外'切替後
				if (G.SS.PLM_AUT_CNST) {
					if (G.CAM_GAI_STS == 1 || G.CAM_EXP_STS == 1 || G.CAM_WBL_STS == 1) {/*1:自動*/
						set_expo_const();
					}
				}
				break;
			case 73:
				NXT_STS = 3;
				break;
			case 404://'透過'切替後
			case 424://'反射'切替後
				NXT_STS = m_adat.ir_next;
				if (NXT_STS != 17 && NXT_STS != 27 && NXT_STS != 37) {
					//G.mlog("後で確認すること");
				}
				break;
			case 444://'赤外'切替後
				if (G.SS.PLM_AUT_V_PK && m_adat.gai_tune_ir_done == false) {
					m_rdat.g_nxt = NXT_STS;
					NXT_STS = 700;
				}
				break;
			case 445://赤外同時測定
				if (true) {
					string name, path;
					if (!m_rdat.bONKP) {
						//表面
						name = mes.name_of_zr[m_rdat.z_idx];
						path = mes.fold + "\\" + name;
					}
					else {
						//中心
						name = mes.name_of_kr[m_rdat.k_idx];
						path = mes.fold + "\\" + name;
					}
					System.IO.File.Copy(path, m_rdat.bup_fold +  "\\" + name);
					G.FORM02.save_image(path);
					a_write(string.Format("画像保存:{0}", path));
				}
				break;
			case 446:
				m_adat.ir_done = true;
				if ((m_adat.ir_lsbk & 1)!=0) {
					NXT_STS = 400;//透過に戻す
				}
				else {
					NXT_STS = 420;//反射に戻す
				}
				break;
			case 998:
				//開始位置へ移動
//a_write("開始位置へ移動");
				break;
			case 70:
				//光源切り替え(開始時)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, false);//赤外

				if (mes.crt == "CT") {
					G.FORM10.LED_SET(0, true);//透過
a_write("光源切替:->透過");
				}
				else {
					G.FORM10.LED_SET(1, true);//反射
a_write("光源切替:->反射");
				}
				m_adat.chk1 = Environment.TickCount;
				break;

#if true//2018.08.16(Z軸再原点)
			case 500:
				D.SET_STG_ORG(2);
				G.PLM_STS |= (1 << 2);
				NXT_STS = -this.AUT_STS;
			break;
			case 501:
//				MOVE_ABS_Z(m_adat.sta_pos_z);
				NXT_STS = m_rdat.g_nxt;
			break;
#endif
			case 700:
				if (G.LED_PWR_STS == 1) {
					this.timer4.Tag = 0;//透過
				}
				else if (G.LED_PWR_STS == 2) {
					this.timer4.Tag = 1;//反射
				}
				else if (G.LED_PWR_BAK == 1/*反射*/) {
					this.timer4.Tag = 3;//赤外(<-反射)
				}
				else {
					this.timer4.Tag = 2;//赤外(<-透過)
				}
				G.CNT_MOD = 0;//0:画面全体
#if true//2019.02.03(WB調整)
				G.CNT_OFS = 0;
#endif
				G.CAM_PRC = G.CAM_STS.STS_HIST;
				G.CHK_VPK = 1;
				this.GAI_STS = 1;
				this.timer4.Enabled = true;
a_write("GAIN調整:開始");
				break;
			case 701:
				//GAIN調整-終了待ち
				if (this.GAI_STS != 0) {
					NXT_STS = this.AUT_STS;
				}
				else {
a_write(string.Format("GAIN調整:終了(OFFSET={0})", G.SS.CAM_PAR_GA_OF[(int)this.timer4.Tag]));
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					NXT_STS = m_rdat.g_nxt;
					if (m_adat.gai_tune_cl_done == false) {
						m_adat.gai_tune_cl_done = true;
//@@					NXT_STS = 17;//初回AF後
					}
					else {
						m_adat.gai_tune_ir_done = true;
//@@					NXT_STS = 443;//IRの保存へ
					}
				}
				break;
			case 999:
//■■■■■■■set_expo_mode(/*auto*/1);
				a_write(string.Format("再測定:終了"));
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				this.AUT_STS = 0;
				timer7.Enabled = false;
				UPDSTS();
				for (int i = 0; i < 3; i++) {
					Console.Beep(1600, 250);
					Thread.Sleep(250);
				}
			//	G.mlog(string.Format("#i再測定が終了しました.\r毛髪:{0}本", m_adat.h_cnt));
				G.mlog(string.Format("#i再測定が終了しました."));
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
								MOVE_REL_XY(m_bsla[0], m_bsla[1]);
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
				this.timer7.Enabled = true;
			}
		}
#endif
#if true//2019.05.12(縦型対応)
		public void do_mouhou_search()
		{
			if (true) {
				Form27	frm = new Form27();
				if(frm.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) {
					return;
				}
			}
			DlgProgress
					prg = new DlgProgress();
			int bak_of_mode = G.SS.PLM_AUT_MODE;
			int xcnt = (int)Math.Floor((double)(G.SS.TAT_STG_XMAX-G.SS.TAT_STG_XMIN)/G.SS.TAT_STG_XSTP);
			int ycnt = (int)Math.Floor((double)(G.SS.TAT_STG_YMAX-G.SS.TAT_STG_YMIN)/G.SS.TAT_STG_YSTP);
			int sttl = xcnt*ycnt;
#if true//2018.12.22(測定抜け対応)
			m_adat = new ADATA();
			m_rdat = new RDATA();
#endif
			m_adat.trace = false;
			m_adat.retry = false;
#if true//2018.04.08(ＡＦパラメータ)
			G.push_af2_para();
#endif
			try {
				prg.Show("毛包探索", G.FORM01);
				prg.SetStatus("実行中...");
#if true//2019.04.29(微分バグ修正)
				G.CNT_DTHD = G.SS.CAM_HIS_DTHD;
				G.CNT_DTH2 = G.SS.CAM_HIS_DTH2;
#endif
				G.CAM_PRC = G.CAM_STS.STS_AUTO;
#if true//2019.01.23(GAIN調整&自動測定)
				if (G.SS.PLM_AUT_V_PK) {
					push_gain_ofs();
				}
#endif
				this.AUT_STS = 1;
				timer8.Enabled = true;
				while (this.AUT_STS != 0) {
					Application.DoEvents();
					string buf;
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
						buf += "探索終了...\r\r";
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
					//if ((m_adat.h_idx + 1) == 2) {
					//    buf = buf;
					//}
					if (false) {
					}
					else if (G.SS.TAT_ETC_MODE == 0) {
						buf += "枝追跡モード\r\r";
					}
					else  {
						buf += "毛包直接モード\r\r";
					}
					if (false/*bWAIT*/) {
						buf += "待機中";
					}
					else if (this.FCS_STS != 0 || this.FC2_STS != 0) {
						if (this.AUT_STS == 116) {
							buf += "フォーカス(枝)";
						}
						else {
							buf += "フォーカス(毛包)";
						}
					}
					else if (false/*this.AUT_STS < 20*/) {
						buf += "探索中";
					}
#if true//2019.01.23(GAIN調整&自動測定)
					else if (this.AUT_STS >= 700 && this.AUT_STS <= 701) {
						buf += "GAIN調整中";
					}
#endif
					else if (this.AUT_STS == 500 || this.AUT_STS == -500) {
						buf += "原点復帰(Z)";
					}
					else {
						buf += string.Format("探索中 {0}/{1}", 1+m_adat.f_idx, sttl);
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
#if true//2019.01.23(GAIN調整&自動測定)
			if (G.SS.PLM_AUT_V_PK) {
				pop_gain_ofs();
			}
#endif
#if true//2018.04.08(ＡＦパラメータ)
			G.pop_af2_para();
#endif
			//---
			G.SS.PLM_AUT_MODE = bak_of_mode;//リトライ時に書き変わるため元に戻す
		}
		private double next_deg(double deg, int dx, int dy)
		{
			double	nxt;
			double	cur = Math.Atan2(-dy, dx) / Math.PI * 180;
			if (double.IsNaN(deg)) {
				if ((cur += 180) >= 360) {
					cur -= 360;
				}
				nxt = cur;
			}
			else if (deg >= 90 || deg <= -90) {
				nxt = cur + 180;
			}
			else {
				nxt = cur;
			}
			return(nxt);
		}
		private double	m_dx, m_dy;
		private double	m_deg;
		private int		m_bak_x, m_bak_y;
		private int		m_cen_x, m_cen_y;
		private double	m_bak_d;
		private bool check_eob(Point p1, Point p2, Point p3, Point p4)
		{
			int			gap = G.CAM_HEI/10;
			Rectangle	rt = new Rectangle(gap, gap, G.CAM_WID-gap*2, G.CAM_HEI-gap*2);
			if (rt.Contains(p1) && rt.Contains(p2)) {
				return(true);
			}
			if (rt.Contains(p2) && rt.Contains(p3)) {
				return(true);
			}
			if (rt.Contains(p3) && rt.Contains(p4)) {
				return(true);
			}
			if (rt.Contains(p4) && rt.Contains(p1)) {
				return(true);
			}
			return(false);
		}
		private void timer8_Tick(object sender, EventArgs e)
		{
			int NXT_STS = this.AUT_STS + 1;
			int yy, y0, xx, x0;
			Rectangle RT;
#if true//2019.02.23(自動測定中の(不要な)MSGBOX表示のBTN押下で測定で終了してしまう現象)
			this.timer8.Enabled = false;
#endif
			if (G.bCANCEL) {
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				this.AUT_STS = 0;
				G.bCANCEL = false;
			}
#if true
			if (this.AUT_STS >= 700 && this.AUT_STS <= 702) {
			}
			else if (this.AUT_STS == 116 || this.AUT_STS == 616) {
			}
			else {
				this.AUT_STS = this.AUT_STS;//FOR BP
			}
#endif
#if DEBUG//2019.01.23(GAIN調整&自動測定)
//System.Diagnostics.Debug.WriteLine("{0}:STS={1},DIDX={2}", Environment.TickCount, this.AUT_STS, m_didx);
#endif
			switch (this.AUT_STS) {
			case 0:
				this.timer8.Enabled = false;
				break;
			case 1:
				if (true) {
					m_adat.gai_tune_cl_done = false;
					m_adat.gai_tune_ir_done = false;
					//
				}
				if (true/*G.SS.CAM_FCS_CHK2*/) {
					DateTime dt = DateTime.Now;
					m_adat.log = T.GetDocFolder();
					m_adat.log += "\\ログ(毛包探索)\\";
					if (System.IO.Directory.Exists(m_adat.log) == false) {
						System.IO.Directory.CreateDirectory(m_adat.log);
					}
					m_adat.log += string.Format("{0:0000}{1:00}{2:00}-{3:00}{4:00}{5:00}",
							dt.Year, dt.Month, dt.Day,
							dt.Hour, dt.Minute, dt.Second);
					m_adat.log += ".csv";
					m_adat.h_cnt = 0;
					m_adat.f_idx = 0;
					a_write("開始");
				}
				if (true) {
					G.CNT_MOD  = G.SS.TAT_AFC_AFMD;
					G.CNT_OFS  = 0;
					G.CNT_MET  = G.SS.TAT_AFC_CMET;
					//---
					G.SS.CAM_FC2_FSPD = G.SS.IMP_FC2_FSPD[0];
					G.SS.CAM_FC2_DPLS = G.SS.IMP_FC2_DPLS[0];
					G.SS.CAM_FC2_CNDA = G.SS.IMP_FC2_CNDA[0];
					G.SS.CAM_FC2_CNDB = G.SS.IMP_FC2_CNDB[0];
					G.SS.CAM_FC2_SKIP = G.SS.IMP_FC2_SKIP[0];
					G.SS.CAM_FC2_FAVG = G.SS.IMP_FC2_FAVG[0];
					G.SS.CAM_FC2_BPLS = G.SS.IMP_FC2_BPLS[0];
					G.SS.CAM_FC2_DTYP = G.SS.IMP_FC2_DTYP[0];
					G.SS.CAM_FC2_DROP = G.SS.IMP_FC2_DROP[0];
					G.SS.CAM_FC2_DCNT = G.SS.IMP_FC2_DCNT[0];
				}
				break;
			case 2:
				//---
				if (true /*mes.crt == "CT"*/ && (G.LED_PWR_STS & 1) == 0) {
					//光源=>白色(透過)
					NXT_STS = 70;//70->71->72->73->3として白色点灯->安定待機後に戻ってくる
				}
				if (false/*mes.crt == "CR"*/ && (G.LED_PWR_STS & 2) == 0) {
					//光源=>白色(反射)
					NXT_STS = 70;//70->71->72->73->3として白色点灯->安定待機後に戻ってくる
				}
				if (NXT_STS == 70) {
					MOVE_ABS_XY(G.SS.TAT_STG_XMIN, G.SS.TAT_STG_YMIN);
				}
				if (true) {
					//m_rdat.k_idx = 0;
					//m_rdat.z_idx = 0;
				}
				if (true) {
					m_deg = double.NaN;
				}
				break;
			case 3:
				MOVE_ABS_XY(G.SS.TAT_STG_XMIN, G.SS.TAT_STG_YMIN);
				NXT_STS = -this.AUT_STS;
				break;
			case 4:
				//Z原点
				if (true/*m_rdat.bEXHR && G.SS.PLM_AUT_ZORG*/) {
					m_rdat.bEXHR = false;
					//---
					m_pre_set[2] = false;
					m_rdat.g_nxt = NXT_STS;
					NXT_STS = 500;	//500を経由して5へ遷移
				}
				break;
			case 5:
				if (true/*!G.SS.IMP_AUT_EXAF*/) {
					//表面ＡＦから実行
					MOVE_ABS_Z(G.SS.TAT_STG_ZPOS);
				}
				else {
					//中心ＡＦから実行
					MOVE_ABS_Z(G.SS.TAT_STG_ZPOS);
				}
				NXT_STS = -this.AUT_STS;
				break;
			case 6:
				m_adat.chk1 = 0;
				NXT_STS = 12;
				break;
			case 7:
			break;
			case 10:
				//次の画像測定へ
				if (true) {
					int nxt_x = G.PLM_POS[0] + G.SS.TAT_STG_XSTP;
					int nxt_y = G.PLM_POS[1];
					if (nxt_x > G.SS.TAT_STG_XMAX) {
						nxt_x = G.SS.TAT_STG_XMIN;
						nxt_y = G.PLM_POS[1] + G.SS.TAT_STG_YSTP;
					}
					if (nxt_y > G.SS.TAT_STG_YMAX) {
						NXT_STS = 998;//開始位置へ移動後に終了
					}
					else {
						m_adat.f_idx++;
						MOVE_ABS_XY(nxt_x, nxt_y);
						NXT_STS = -this.AUT_STS;
					}
				}
				break;
			case 11:
				m_adat.chk3 = 0;
				m_deg   = double.NaN;
				m_bak_x = G.PLM_POS[0];
				m_bak_y = G.PLM_POS[1];
				break;
			case 12:
			case 212:
				m_dcur = m_didx;
				break;
			case  13:
			case 213:
				if ((m_didx - m_dcur) < G.SS.TAT_STG_SKIP) {
					NXT_STS = this.AUT_STS;//画面が更新されるまで
				}
				break;
			case  14:
			case 214:
				//測定
				if (G.SS.TAT_ETC_MODE == 1/*毛包直接*/) {
					if (G.IR.TAT_CNT <= 0) {
a_write("画像判定:NG");
						NXT_STS = 10;
					}
					else {
a_write("画像判定:毛包");
					}
				}
				else {
					if (G.IR.CIR_CNT <= 0 && G.IR.TAT_CNT <= 0) {
a_write("画像判定:NG");
						NXT_STS = 10;
					}
					else if (G.IR.CIR_CNT > 0) {
a_write("画像判定:枝");
					}
					else {
a_write("画像判定:毛包");
					}
				}
				break;
			case  15:
			case 215:
				//毛髪エリアの水平/垂直方向センタリング
				bool flag = true;
				int	TOL;
				if (G.IR.CIR_CNT > 0) {
					yy = G.IR.TAT_OY; xx = G.IR.TAT_OX; TOL = 5;
				}
				else {
					yy = G.IR.TAT_GY; xx = G.IR.TAT_GX; TOL = 10;
				}
				y0 = G.CAM_HEI/2;
				x0 = G.CAM_WID/2;
				if (m_adat.chk1 != 0) {
					//OK(左/右移動後毛髪判定にNGのため最後の画像)
				}
				else if (Math.Abs(yy-y0) < (G.CAM_HEI/TOL) && Math.Abs(xx-x0) < (G.CAM_WID/TOL)) {
					//OK
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
					int dif_y = (yy - y0);
					int dst_y = (int)(G.PLM_POS[1] + (G.FORM02.PX2UM(dif_y) / G.SS.PLM_UMPP[1]));
					int dif_x = (xx - x0);
					int dst_x = (int)(G.PLM_POS[0] + (G.FORM02.PX2UM(dif_x) / G.SS.PLM_UMPP[0]));

					if (dif_y < 0 && dst_y <= G.SS.PLM_MLIM[1]) {
						dif_y = 0;
					}
					if (dif_y > 0 && dst_y >= G.SS.PLM_PLIM[1]) {
						dif_y = 0;
					}
					if (dif_x < 0 && dst_x <= G.SS.PLM_MLIM[0]) {
						dif_x = 0;
					}
					if (dif_x > 0 && dst_x >= G.SS.PLM_PLIM[0]) {
						dif_x = 0;
					}
					if (dif_x != 0 || dif_y != 0) {
						if (G.IR.CIR_CNT > 0) {
a_write(string.Format("センタリング(枝/X+={0}/Y+={1})", dif_x, dif_y));
						}
						else {
a_write(string.Format("センタリング(毛包/X+={0}/Y+={1})", dif_x, dif_y));
						}
						MOVE_PIX_XY(dif_x, dif_y);
						NXT_STS = -(this.AUT_STS - 3 - 1);		// -> 12
					}
				}
				else {
					flag = flag;
				}
				m_cen_x = G.PLM_POS[0];
				m_cen_y = G.PLM_POS[1];
				break;
			case  16:
			case 216:
				if (G.IR.CIR_CNT <= 0 && G.IR.TAT_CNT <= 0) {
a_write("枝/毛包判定:NG");
					NXT_STS = 10;
				}
				else if (G.IR.CIR_CNT > 0) {
					NXT_STS = 17;
a_write("AF:開始(枝)");
				}
				else {
					NXT_STS = 217;
					if (m_adat.h_cnt == 0) {
a_write("毛包発見");
						m_adat.h_cnt++;
					}
a_write("AF:開始(毛包)");
				}
				for (int i = 0; i < 2; i++) {
					Console.Beep(1600, 250);
					Thread.Sleep(250);
				}
				start_af(9/*縦型用*/);
				break;
			case  17://枝
			case 217://毛包
				//AF処理(終了待ち)
				if (this.FCS_STS != 0 || this.FC2_STS != 0) {
					NXT_STS = this.AUT_STS;
				}
				else {
a_write(string.Format("AF:終了({0})", this.AUT_STS == 17 ? "枝": "毛包"));
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					m_dcur = m_didx;
				}
				break;
			case 18:
				if ((m_didx - m_dcur) < (G.SS.TAT_STG_SKIP+3)) {
					NXT_STS = this.AUT_STS;
				}
				break;
			case 19://表面
				//初回AF後
				if (G.IR.CIR_CNT <= 0 && G.IR.TAT_CNT <= 0) {
a_write("AF後の枝/毛包の再検出に失敗");
					MOVE_ABS_XY(m_bak_x, m_bak_y);
					NXT_STS = -(10 - 1);//->10
				}
				if (true) {
					m_adat.z_cur = G.PLM_POS[2];
				}
				NXT_STS = 118;
				/*
				if (G.SS.PLM_AUT_V_PK && m_adat.gai_tune_cl_done == false) {
					m_rdat.g_nxt = NXT_STS;
					NXT_STS = 700;//GAIN調整
				}*/
				break;
			case 118://表面
				if (false/*@@ this.AUT_STS == 17*/) {
					//--- ONCE
					if (G.SS.PLM_AUT_CNST) {
						if (G.CAM_GAI_STS == 1 || G.CAM_EXP_STS == 1 || G.CAM_WBL_STS == 1) {/*1:自動*/
							set_expo_const();
						}
					}
				}
				break;
			case 119://表面
				if (G.IR.TAT_CNT > 0) {
a_write("毛包発見");
					G.IR.TAT_CNT = G.IR.TAT_CNT;
					NXT_STS = 210;
				}
				else if (G.IR.CIR_CNT <= 0) {
a_write("枝:終了");
					NXT_STS = 130;
				}
				/*else if (check_eob(G.IR.TAT_P1, G.IR.TAT_P2, G.IR.TAT_P3, G.IR.TAT_P4)) {
a_write("枝:終了");
					NXT_STS = 130;
				}*/
				else {
					//次の位置へ
					int	nxt_x, nxt_y, tmp = (int)((/*G.CAM_WID+*/G.CAM_HEI)*0.8);
					bool ret = check_eob(G.IR.TAT_P1, G.IR.TAT_P2, G.IR.TAT_P3, G.IR.TAT_P4);

					if (double.IsNaN(m_deg)) {
						m_deg = next_deg(m_deg, G.IR.TAT_DX, G.IR.TAT_DY);
						m_bak_d = m_deg;
					}
					else {
						m_deg = next_deg(m_deg, G.IR.TAT_DX, G.IR.TAT_DY);
					}
					nxt_x =+(int)(tmp * Math.Cos(m_deg/180*Math.PI));
					nxt_y =-(int)(tmp * Math.Sin(m_deg/180*Math.PI));
					if (!ret) {
						yy = G.IR.TAT_OY;
						xx = G.IR.TAT_OX;
						y0 = G.CAM_HEI/2;
						x0 = G.CAM_WID/2;

						nxt_y += (yy-y0);
						nxt_x += (xx-x0);
					}
a_write(string.Format("枝:探索({0})", m_adat.chk3 == 0 ? "逆方向": "順方向"));
					MOVE_PIX_XY(nxt_x, nxt_y);
					NXT_STS = -this.AUT_STS;
				}
				//画像保存
				Console.Beep(800, 250);
				break;
			case 120:
				m_dcur = m_didx;
				break;
			case 121:
				if ((m_didx - m_dcur) < G.SS.TAT_STG_SKIP) {
					NXT_STS = this.AUT_STS;//画面が更新されるまで
				}
				else {
					NXT_STS = 119;
				}
				break;
			case 130://発見地点に戻って、逆方向へ
				if (m_adat.chk3 == 1) {
a_write("枝:毛包発見できず");
					NXT_STS = 999;
				}
				else {
a_write("枝:検出位置へ");
					m_adat.chk3 = 1;
					MOVE_ABS_XY(m_cen_x, m_cen_y);
					NXT_STS = -this.AUT_STS;
				}
				break;
			case 131://表面
				m_dcur = m_didx;
				break;
			case 132://表面
				if ((m_didx - m_dcur) < G.SS.TAT_STG_SKIP) {
					NXT_STS = this.AUT_STS;//画面が更新されるまで
				}
				break;
			case 133:
				if (G.IR.CIR_CNT <= 0) {
a_write("枝:検出位置で枝の再検出に失敗");
					MOVE_ABS_XY(m_bak_x, m_bak_y);
					NXT_STS = -(10 - 1);//->10
				}
				else {
a_write("枝:方向逆転");
					m_deg = m_bak_d+180;
					if (m_deg >= 270) {
						m_deg -= 360;
					}
					NXT_STS = 119;
				}
				break;
			case 210://毛包発見
				a_write("毛包発見");
				m_adat.h_cnt++;
				NXT_STS = 215;
				break;
			case 218://AF終了後にここへ
				break;
			case 219:
				break;
			case 220:
				//光源切り替え(->赤外)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, true );//赤外
				NXT_STS = 999;
				break;
			case 400://赤外同時測定
				//光源切り替え(->透過)
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, false);//赤外
				G.FORM10.LED_SET(0, true );//透過
				m_adat.pref = "CT";//白色(透過)
				m_adat.chk1 = Environment.TickCount;
a_write("光源切替:->透過");
				G.CAM_PRC = G.CAM_STS.STS_AUTO;
				break;
			case 420://赤外同時測定
				//光源切り替え(->反射)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(2, false);//赤外
				G.FORM10.LED_SET(1, true );//反射
				m_adat.pref = "CR";//白色(反射)
				m_adat.chk1 = Environment.TickCount;
a_write("光源切替:->反射");
				G.CAM_PRC = G.CAM_STS.STS_AUTO;
				break;
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
			case 401://'透過'切替後
			case 421://'反射'切替後
			case 441://'赤外'切替後
			break;
			case 72:
			case 402://'透過'切替後
			case 422://'反射'切替後
			case 442://'赤外'切替後
				//カメラ安定待機
				if ((Environment.TickCount - m_adat.chk1) < (G.SS.ETC_LED_WAIT*1000)) {
					NXT_STS = this.AUT_STS;
				}
				break;
			case 403://'透過'切替後
			case 423://'反射'切替後
			case 443://'赤外'切替後
				if (G.SS.PLM_AUT_CNST) {
					if (G.CAM_GAI_STS == 1 || G.CAM_EXP_STS == 1 || G.CAM_WBL_STS == 1) {/*1:自動*/
						set_expo_const();
					}
				}
				break;
			case 73:
				NXT_STS = 3;
				break;
			case 404://'透過'切替後
			case 424://'反射'切替後
				NXT_STS = m_adat.ir_next;
				if (NXT_STS != 17 && NXT_STS != 27 && NXT_STS != 37) {
					//G.mlog("後で確認すること");
				}
				break;
			case 444://'赤外'切替後
				if (G.SS.PLM_AUT_V_PK && m_adat.gai_tune_ir_done == false) {
					m_rdat.g_nxt = NXT_STS;
					NXT_STS = 700;
				}
				break;
			case 445://赤外同時測定
				if (true) {
				}
				break;
			case 446:
				m_adat.ir_done = true;
				if ((m_adat.ir_lsbk & 1)!=0) {
					NXT_STS = 400;//透過に戻す
				}
				else {
					NXT_STS = 420;//反射に戻す
				}
				break;
			case 998:
				//開始位置へ移動
//a_write("開始位置へ移動");
				break;
			case 70:
				//光源切り替え(開始時)
				G.FORM10.LED_SET(0, false);//透過
				G.FORM10.LED_SET(1, false);//反射
				G.FORM10.LED_SET(2, false);//赤外

				if (true/*mes.crt == "CT"*/) {
					G.FORM10.LED_SET(0, true);//透過
a_write("光源切替:->透過");
				}
				else {
					G.FORM10.LED_SET(1, true);//反射
a_write("光源切替:->反射");
				}
				m_adat.chk1 = Environment.TickCount;
				break;

#if true//2018.08.16(Z軸再原点)
			case 500:
a_write("原点復帰:開始(Z軸)");
				D.SET_STG_ORG(2);
				G.PLM_STS |= (1 << 2);
				NXT_STS = -this.AUT_STS;
			break;
			case 501:
a_write("原点復帰:終了(Z軸)");
//				MOVE_ABS_Z(m_adat.sta_pos_z);
				NXT_STS = m_rdat.g_nxt;
			break;
#endif
			case 700:
				if (G.LED_PWR_STS == 1) {
					this.timer4.Tag = 0;//透過
				}
				else if (G.LED_PWR_STS == 2) {
					this.timer4.Tag = 1;//反射
				}
				else if (G.LED_PWR_BAK == 1/*反射*/) {
					this.timer4.Tag = 3;//赤外(<-反射)
				}
				else {
					this.timer4.Tag = 2;//赤外(<-透過)
				}
				G.CNT_MOD = 0;//0:画面全体
#if true//2019.02.03(WB調整)
				G.CNT_OFS = 0;
#endif
				G.CAM_PRC = G.CAM_STS.STS_HIST;
				G.CHK_VPK = 1;
				this.GAI_STS = 1;
				this.timer4.Enabled = true;
//a_write("GAIN調整:開始");
				break;
			case 701:
				//GAIN調整-終了待ち
				if (this.GAI_STS != 0) {
					NXT_STS = this.AUT_STS;
				}
				else {
//a_write(string.Format("GAIN調整:終了(OFFSET={0})", G.SS.CAM_PAR_GA_OF[(int)this.timer4.Tag]));
					G.CAM_PRC = G.CAM_STS.STS_AUTO;
					NXT_STS = m_rdat.g_nxt;
					if (m_adat.gai_tune_cl_done == false) {
						m_adat.gai_tune_cl_done = true;
//@@					NXT_STS = 17;//初回AF後
					}
					else {
						m_adat.gai_tune_ir_done = true;
//@@					NXT_STS = 443;//IRの保存へ
					}
				}
				break;
			case 999:
//■■■■■■■set_expo_mode(/*auto*/1);
a_write(string.Format("毛包探索:終了"));
				G.CAM_PRC = G.CAM_STS.STS_NONE;
				this.AUT_STS = 0;
				timer8.Enabled = false;
				UPDSTS();
				for (int i = 0; i < 3; i++) {
					Console.Beep(1600, 250);
					Thread.Sleep(250);
				}
			//	G.mlog(string.Format("#i再測定が終了しました.\r毛髪:{0}本", m_adat.h_cnt));
				if (m_adat.h_cnt > 0) {
				G.mlog(string.Format("#i毛包探索が終了しました."));
				}
				else {
				G.mlog(string.Format("#i毛包を検出できませんでした."));
				}
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
								MOVE_REL_XY(m_bsla[0], m_bsla[1]);
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
				this.timer8.Enabled = true;
			}
		}
#endif
	}
}
