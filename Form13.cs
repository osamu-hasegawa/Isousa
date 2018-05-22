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

namespace uSCOPE
{
	public partial class Form13 : Form
	{
		private AutoResetEvent m_event = new AutoResetEvent(false);
		private int m_ijog = -1;
		private int m_cha = -1;
		private int m_dir =  0;
		private int m_cnt =  0;
		private int m_tic;
		private int m_icam = 1;
		private int PLM_STS_BAK;
		private int EUI_STS = 0;
		DlgProgress
					m_prg = null;//new DlgProgress();

		public Form13()
		{
			InitializeComponent();
		}
		private void Form13_Load(object sender, EventArgs e)
		{
			this.radioButton1.Text = G.SS.EUI_XYA_TEXT[0];
			this.radioButton2.Text = G.SS.EUI_XYA_TEXT[1];
			this.radioButton3.Text = G.SS.EUI_XYA_TEXT[2];
			this.radioButton4.Text = G.SS.EUI_ZFC_TEXT[0];
			this.radioButton5.Text = G.SS.EUI_ZFC_TEXT[1];
			this.radioButton6.Text = G.SS.EUI_ZFC_TEXT[2];
			this.radioButton7.Text = G.SS.EUI_ZOM_TEXT[0];
			this.radioButton8.Text = G.SS.EUI_ZOM_TEXT[1];
			this.radioButton9.Text = G.SS.EUI_ZOM_TEXT[2];
			//---
			this.button1.Text = G.SS.EUI_ZOM_LABL[0];
			this.button2.Text = G.SS.EUI_ZOM_LABL[1];
			//---
			this.radioButton1.Checked = true;//連続
			this.radioButton4.Checked = true;//連続
			this.radioButton7.Checked = true;//連続
			//---
			this.radioButton11.Checked = true;
			this.backgroundWorker1.RunWorkerAsync();
			//timer1.Enabled = true;
			this.timer1.Interval = 100;
		}

		private void Form13_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.backgroundWorker1.CancelAsync();
			m_event.Set();
			while (this.backgroundWorker1.IsBusy) {
				Application.DoEvents();
				Thread.Sleep(10);
			}
		}

		// bUpdate=true:画面更新/false:変数取込
		private bool GETDAT(bool bUpdate)
		{
			bool rc = false;
			//if (m_bENTER_GETD) {
			//    return(false);
			//}
			//m_bENTER_GETD = true;
			try {
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton1, this.radioButton2, this.radioButton3 }, ref G.SS.EUI_XYA_MODE);
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton4, this.radioButton5, this.radioButton6 }, ref G.SS.EUI_ZFC_MODE);
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton7, this.radioButton8, this.radioButton9 }, ref G.SS.EUI_ZOM_MODE);
				//DDV.DDX(bUpdate, new RadioButton[] { this.radioButton10, this.radioButton11, this.radioButton12 }, ref G.SS.TST_PAR_GAUS);
				//---
				rc = true;
			}
			catch (Exception e) {
				G.mlog(e.Message);
				rc = false;
			}
			//m_bENTER_GETD = false;
			return (rc);
		}
		public void LED_SET(int il)
		{
			switch (il) {
			case  0:this.radioButton10.Checked = true; break;
			case  1:this.radioButton11.Checked = true; break;
			default:this.radioButton12.Checked = true; break;
			}
		}
		private void OnClicks(object sender, EventArgs e)
		{
			GETDAT(false);
			if (false) {
			}
			else if (sender == this.radioButton10) {
				G.FORM10.LED_SET(0, this.radioButton10.Checked);
			}
			else if (sender == this.radioButton11) {
				G.FORM10.LED_SET(1, this.radioButton11.Checked);
			}
			else if (sender == this.radioButton12) {
				G.FORM10.LED_SET(2, this.radioButton12.Checked);
			}
			else if (sender == this.button20 || sender == this.button21
				  || sender == this.button22 || sender == this.button23
				  || sender == this.button33 || sender == this.button34
				  || sender == this.button35 || sender == this.button36) {
				//---
				if (m_cnt >= 0) {
					G.PLM_STS |= (1 << m_cha);
					D.SET_STG_REL(m_cha, m_dir == 0 ? -m_cnt: +m_cnt);
				}
			}
			//else if (sender == this.button1) {//倍率設定
			//    D.SET_STG_ABS(2, G.SS.EUI_ZOM_PSET[0]);
			//}
			else if (sender == this.button1 || sender == this.button2) {//倍率設定
				int nxt = (sender == this.button1) ? G.SS.EUI_ZOM_PSET[0]: G.SS.EUI_ZOM_PSET[1];
				int pos = G.PLM_POS[3];

				if (pos > nxt && G.SS.PLM_BSLA[3] > 0) {
					G.PLM_BSL[3] = G.SS.PLM_BSLA[3];
				}
				else {
					G.PLM_BSL[3] = 0;
				}
				G.PLM_STS |= (1 << 3);
				D.SET_STG_ABS(3, nxt - G.PLM_BSL[3]);
			}
			else if (sender == this.button9/*黒髪*/ || sender == this.button11/*白髪*/) {
				string path;
				path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += @"\KOP";
				path += @"\" + Application.ProductName;
				G.SS.PLM_AUT_FOLD = path;
				//3:透過→赤外, 8:反射→赤外
				G.SS.PLM_AUT_MODE = (sender == this.button9) ? 8: 3;
				//自動測定実行
				G.FORM12.do_auto_mes(false);
			}
			else if (sender == this.button11) {//白髪
			}
		}
		private void buttons_MouseDown(object sender, MouseEventArgs e)
		{
			m_cha = Convert.ToInt32(((Button)sender).Tag);
			m_dir = 1;

			if (false) {
			}
			else if (sender == this.button20) {//ch1:←
				m_dir = 1;//CW(+):(カメラとの兼ね合いで方向を逆に)
			}
			else if (sender == this.button21) {//ch1:→
				m_dir = 0;//CCW(-):(カメラとの兼ね合いで方向を逆に)
			}
			else if (sender == this.button22) {//ch2:↑
				m_dir = 0;//CCW(-)
			}
			else if (sender == this.button23) {//ch2:↓
				m_dir = 1;//CW(+)
			}
			else if (sender == this.button33) {//ch3:↑
				m_dir = 0;//CCW(-)
			}
			else if (sender == this.button34) {//ch3:↓
				m_dir = 1;//CW(+)
			}
			else if (sender == this.button35) {//ch4:↑
				m_dir = 0;//CCW(-)
			}
			else if (sender == this.button36) {//ch4:↓
				m_dir = 1;//CW(+)
			}
			else {
				return;
			}
			switch (m_cha) {
			case  0:
			case  1:m_cnt = G.SS.EUI_XYA_PCNT[G.SS.EUI_XYA_MODE]; break;
			case  2:m_cnt = G.SS.EUI_ZFC_PCNT[G.SS.EUI_ZFC_MODE]; break;
			default:m_cnt = G.SS.EUI_ZOM_PCNT[G.SS.EUI_ZOM_MODE]; break;
			}
			if (e.Button == System.Windows.Forms.MouseButtons.Right) {
				m_cnt = 1;
				//右クリックなのでOnClickは呼ばれない
				G.PLM_STS |= (1 << m_cha);
				D.SET_STG_REL(m_cha, m_dir == 0 ? -m_cnt: +m_cnt);
			}
			else if (m_cnt >= 0) {
				
			}
			else /*if (p < 0)*/ {
				D.SET_STG_JOG(m_cha, m_dir);
				G.PLM_STS |= (1 << m_cha);
				//---
				m_ijog = m_cha;
				m_event.Set();
			}
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			while (this.backgroundWorker1.CancellationPending == false) {
				while (m_event.WaitOne(250) == false) {
					Thread.Sleep(0);
				}
				if (this.backgroundWorker1.CancellationPending) {
					break;
				}
				while ((Control.MouseButtons & MouseButtons.Left) != 0) {
					Thread.Sleep(1);
				}
				D.SET_STG_STOP(m_ijog);
				G.PLM_STS |= (1 << m_ijog);
				m_ijog = -1;
			}
		}
		public void START_TIMER()
		{
			this.timer1.Enabled = true;
		}
		private void timer1_Tick(object sender, EventArgs e)
		{
			if (true) {
                bool flag1 = (PLM_STS_BAK & (1 << 3)) != (G.PLM_STS & (1 << 3));
                bool flag2 = (G.PLM_STS & (1 << 3)) != 0;
				if (flag1 || flag2) {
					double Y;
					Y = G.SS.ZOM_PLS_A * G.PLM_POS[3] + G.SS.ZOM_PLS_B;
					this.label2.Text = string.Format("{0:F2} 倍", Y);
				}
				PLM_STS_BAK = G.PLM_STS;
			}
			int NXT_STS = this.EUI_STS+1;
			//double fmax;
			//int imax;

			this.timer1.Enabled = false;
			switch (this.EUI_STS) {
			case 0:
				m_prg = new DlgProgress();
				m_prg.Show(Application.ProductName, G.FORM01);
				m_prg.SetStatus("原点検出\r\n\r\n実行中...");
				//LED.反射->ON
				G.FORM10.LED_SET(1, true);
				//原点.ALL
				G.FORM11.DO_ORG_ALL();
				break;
			case 1:
				if (G.PLM_STS == 0
				 && (G.PLM_POS[0] == G.SS.PLM_POSX[3])
				 && (G.PLM_POS[1] == G.SS.PLM_POSY[3])
				 && (G.PLM_POS[2] == G.SS.PLM_POSF[3])
				 && (G.PLM_POS[3] == G.SS.PLM_POSZ[3])) {
					//原点.ALL->終了
				}
				else {
					NXT_STS = this.EUI_STS;
				}
				break;
			case 3:
				if (true) {
					DialogResult ret;
					ret = G.mlog(""
							+ "#qカメラのキョリブレーションを実行します。"
							+ "校正用のプレパラートをセットしてください。\r\n-\r\n"
							+ "「いいえ」を選択するとキャリブレーション処理をスキップします。");
					if (ret != System.Windows.Forms.DialogResult.Yes) {
						NXT_STS = 20;
					}
					else {
						m_prg = new DlgProgress();
						m_prg.Show(Application.ProductName, G.FORM01);
						m_prg.SetStatus("カメラ校正\r\n\r\n実行中...");
						NXT_STS = 11;
					}
				}
				break;
			case 9:
				m_tic = Environment.TickCount;
			break;
			case 10:
				if ((Environment.TickCount-m_tic) < 1000) {
					NXT_STS = this.EUI_STS;
				}
				break;
			case 11:
				G.FORM12.set_expo_mode(1/*1:auto*/);
				m_tic = Environment.TickCount;
			break;
			case 12:
				if ((Environment.TickCount-m_tic) < 5000) {
					NXT_STS = this.EUI_STS;
				}
				else {
					G.FORM12.set_expo_mode(/*const*/0);
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
			case 2:
			case 15:
			case 20:
			if (m_prg != null)
			{
				m_prg.Hide();
				m_prg.Close();
				m_prg.Dispose();
				m_prg = null;
			}
				break;
			case 16:
				G.mlog("#iカメラのキョリブレーションが完了しました。");
			break;
			default:
				NXT_STS = this.EUI_STS;
				break;
			}
			if (NXT_STS == 0) {
				NXT_STS = 0;//for break.point
			}
			if (m_prg != null && G.bCANCEL)
            {
                NXT_STS = 20;
                G.bCANCEL = false;
                for (int q = 0; q < 4; q++)
                {
                    D.SET_STG_STOP(q);
                    G.PLM_STS |= (1 << q);
                }
            }
            if (true)
            {
				//if (this.EUI_STS != 0) {
					this.EUI_STS = NXT_STS;
					this.timer1.Interval = 100;
					this.timer1.Enabled = true;
				//}
			}
			//else {
			//    if (timer1.Enabled) {
			//        this.EUI_STS = NXT_STS;
			//    }
			//}
		}
	}
}
