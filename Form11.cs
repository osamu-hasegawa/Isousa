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

namespace uSCOPE
{
	public partial class Form11 : Form
	{

		private Label[] m_lblSts1;
		private Label[] m_lblSts2;
		private TextBox[] m_txtPos1;
		private TextBox[] m_txtPos2;
		//private int[] m_bsla = {0,0,0,0 };
		private AutoResetEvent m_event = new AutoResetEvent(false);
		private int m_ijog = -1;
		private bool m_bORGPROC = false;
		//private byte[] m_sts = new byte[16];
		private int REP_STS;

		public Form11()
		{
			InitializeComponent();
		}
		public void SET_UIF_USER()
		{
			//---
			this.tabControl1.TabPages.Remove(this.tabPage3);//詳細
			this.tabControl1.TabPages.Remove(this.tabPage1);//再現性
			//---
			this.button1.Visible = false;//[ZERO]
			//---
		}
		private void init()
		{
			//this.textBox1.Text = G.SS.LED_PWM_VAL[0].ToString();
			//this.textBox2.Text = G.SS.LED_PWM_VAL[0].ToString();
			this.button24.Text = G.SS.PLM_POSWT[0];
			this.button25.Text = G.SS.PLM_POSWT[1];
			this.button26.Text = G.SS.PLM_POSWT[2];
			//---
			this.button27.Text = G.SS.PLM_POSFT[0];
			this.button28.Text = G.SS.PLM_POSFT[1];
			this.button29.Text = G.SS.PLM_POSFT[2];
			//---
			this.button30.Text = G.SS.PLM_POSZT[0];
			this.button31.Text = G.SS.PLM_POSZT[1];
			this.button32.Text = G.SS.PLM_POSZT[2];
			//---
			this.numericUpDown1.Value = G.SS.PLM_DAT_DIST[0];
			this.numericUpDown2.Value = G.SS.PLM_DAT_DIST[1];
			this.numericUpDown3.Value = G.SS.PLM_DAT_DIST[2];
			this.numericUpDown4.Value = G.SS.PLM_DAT_DIST[3];
			//---
			m_lblSts1 = new Label[] { this.label27, this.label18, this.label28, this.label29 };
			m_lblSts2 = new Label[] { this.label30, this.label31, this.label32, this.label33 };
			//---
			m_txtPos1 = new TextBox[] {this.textBox3,this.textBox4,this.textBox5,this.textBox6};
			m_txtPos2 = new TextBox[] {this.textBox7,this.textBox8,this.textBox9,this.textBox10};
			//---
			this.comboBox1.SelectedIndex = 4;
			//---
		}
		public bool isORG_ALL_DONE()
		{
			if (true) {
				//timer1_Tick(null, null);
				D.GET_STG_STS(G.PLM_STS_BIT);
			}
			for (int i = 0; i < 3; i++) {
				if ((G.PLM_STS_BIT[i] & (int)G.PLM_STS_BITS.BIT_ORGOK) == 0) {
					return(false);
				}
			}
			if (this.checkBox1.Checked) {
				if ((G.PLM_STS_BIT[3] & (int)G.PLM_STS_BITS.BIT_ORGOK) == 0) {
					return(false);
				}
			}
			return(true);
		}
		private void Form11_Load(object sender, EventArgs e)
		{
			init();
			for (int i = 0; i < m_txtPos1.Length; i++) {
				m_txtPos1[i].Text = "";
				m_txtPos2[i].Text = "";
			}
			for (int i = 0; i < this.tabControl1.TabCount; i++) {
				this.tabControl1.TabPages[i].BackColor = G.SS.ETC_BAK_COLOR;
			}
			this.backgroundWorker1.RunWorkerAsync();
		}
		private void Form11_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.backgroundWorker1.CancelAsync();
			m_event.Set();
			while (this.backgroundWorker1.IsBusy) {
				Application.DoEvents();
				Thread.Sleep(10);
			}
		}
		private int[] m_dx = { 0, 0, 0, 0 };
		// bUpdate=true:画面更新/false:変数取込
		private bool GETDAT(bool bUpdate)
		{
			bool rc = false;
			if (bUpdate == false) {
				NumericUpDown[] nums = { this.numericUpDown1, this.numericUpDown2, this.numericUpDown3, this.numericUpDown4 };
				CheckBox[] chks = { this.checkBox3, this.checkBox4, this.checkBox5, this.checkBox6 };
				for (int i = 0; i < 4; i++) {
					G.SS.PLM_DAT_DIST[i] = m_dx[i] = (int)nums[i].Value;
					if (chks[i].Checked) {
						m_dx[i] *= 10;
					}
				}
			}
			try {
				//DDV.DDX(bUpdate, this.textBox1, ref G.SS.PLM_AUT_TITL);
				//DDV.DDX(bUpdate, this.comboBox1, ref G.SS.PLM_AUT_SPOS);
				//DDV.DDX(bUpdate, this.numericUpDown5, ref G.SS.PLM_AUT_OVLP);
				//DDV.DDX(bUpdate, this.comboBox2, ref G.SS.PLM_AUT_FLTP);

				//DDV.DDX(bUpdate, this.textBox2, ref G.SS.PLM_AUT_FOLD);
				//DDV.DDX(bUpdate, this.comboBox3, ref G.SS.PLM_AUT_MODE);
				//DDV.DDX(bUpdate, this.numericUpDown6, ref G.SS.PLM_AUT_SKIP);
				//---
				//---
				rc = true;
			}
			catch (Exception e) {
				G.mlog(e.Message);
				rc = false;
			}
			return (rc);
		}
		public void UPDSTS()
		{
			Control[] ctls = {
				this.button37, this.button38,
				this.button20, this.button21,this.button22, this.button23,
				this.button24, this.button25,this.button26,
				this.button27, this.button28,this.button29,
				this.button30, this.button31,this.button32,
				this.button33, this.button34,this.button35,this.button36,
				this.button4, this.button5,this.button6, this.button7,
				this.button8, this.button9,this.button10, this.button11,
				this.button12, this.button13,this.button14, this.button15,
				this.button16, this.button17,this.button18, this.button19,
				this.button1,
				this.button40, this.button41, this.button42,
			};
			if (!D.isCONNECTED()) {
				foreach (Control c in ctls) {
					c.Enabled = false;
				}
				this.comboBox1.Enabled = false;
				this.comboBox1.SelectedIndex = 4;
			}
			else if (!isORG_ALL_DONE()) {
				this.button37.Enabled = true;//原点.allと停止のみenable
				//this.button38.Enabled = true;
#if true//2018.03.13
				this.button4.Enabled = true;
				this.button5.Enabled = true;
				this.button8.Enabled = true;
				this.button9.Enabled = true;
				this.button12.Enabled = true;
				this.button13.Enabled = true;
				this.button16.Enabled = true;
				this.button17.Enabled = true;
#if true//2018.03.28
                this.button6.Enabled = true;
                this.button7.Enabled = true;
                this.button10.Enabled = true;
                this.button11.Enabled = true;
                this.button14.Enabled = true;
                this.button15.Enabled = true;
                this.button18.Enabled = true;
                this.button19.Enabled = true;
#endif
				if ((G.PLM_STS_BIT[0] & (int)G.PLM_STS_BITS.BIT_ORGOK) != 0) {
					this.button6.Enabled = true;
					this.button7.Enabled = true;				
				}
				if ((G.PLM_STS_BIT[1] & (int)G.PLM_STS_BITS.BIT_ORGOK) != 0) {
					this.button10.Enabled = true;
					this.button11.Enabled = true;				
				}
				if ((G.PLM_STS_BIT[2] & (int)G.PLM_STS_BITS.BIT_ORGOK) != 0) {
					this.button14.Enabled = true;
					this.button15.Enabled = true;				
				}
				if ((G.PLM_STS_BIT[3] & (int)G.PLM_STS_BITS.BIT_ORGOK) != 0) {
					this.button18.Enabled = true;
					this.button19.Enabled = true;				
				}
#endif
				return;
			}
			else {
				foreach (Control c in ctls) {
					c.Enabled = true;
				}
				this.comboBox1.Enabled = true;
			}
		}
		private void OnClicks(object sender, EventArgs e)
		{
			GETDAT(false);
			if (false) {
			}
			else if (sender == this.button4 || sender == this.button8 || sender == this.button12 || sender == this.button16) {//原点ch1-ch4
				int q = Convert.ToInt32(((Button)sender).Tag);
				D.SET_STG_ORG(q);
				G.PLM_STS |= (1 << q);
				m_bORGPROC = true;
			}
			else if (sender == this.button5 || sender == this.button9 || sender == this.button13 || sender == this.button17) {//停止ch1-ch4
				int q = Convert.ToInt32(((Button)sender).Tag);
				D.SET_STG_STOP(q);
				G.PLM_STS |= (1 << q);
			}
			else if (sender == this.button7 || sender == this.button10 || sender == this.button14 || sender == this.button18) {//<<ch1-ch4
				int q = Convert.ToInt32(((Button)sender).Tag);
				int c = m_dx[q] * (-1);
				D.SET_STG_REL(q, c);
				G.PLM_STS |= (1 << q);
			}
			else if (sender == this.button6 || sender == this.button11 || sender == this.button15 || sender == this.button19) {//>>ch1-ch4
				int q = Convert.ToInt32(((Button)sender).Tag);
				int c = m_dx[q];
				D.SET_STG_REL(q, c);
				G.PLM_STS |= (1 << q);
			}
			else if (sender == this.button37) {//原点ch.all
				for (int q = 0; q < 3; q++) {
					D.SET_STG_ORG(q);
					G.PLM_STS |= (1 << q);
				}
				if (this.checkBox1.Checked) {//ZOOM軸の原点ALL
					D.SET_STG_ORG(3);
					G.PLM_STS |= (1 << 3);
				}
				else {
					D.SET_STG_POS(3, G.SS.PLM_POSZ[3]);
				}
				this.button37.Tag = 1;
			}
			else if (sender == this.button38) {//停止ch.all
				for (int q = 0; q < 4; q++) {
					D.SET_STG_STOP(q);
					G.PLM_STS |= (1 << q);
				}
				if (this.button37.Tag != null) {
					this.button37.Tag = null;
				}
			}
			else if (sender == this.button24 || sender == this.button25 || sender == this.button26) {//XY.POS
				int q = Convert.ToInt32(((Button)sender).Tag);
				int pos0 = G.PLM_POS[0];
				int pos1 = G.PLM_POS[1];
				int nxt0 = G.SS.PLM_POSX[q];
				int nxt1 = G.SS.PLM_POSY[q];

				if (this.button24.BackColor == Color.LightPink) {
					G.SS.PLM_POSX[q] = pos0;
					G.SS.PLM_POSY[q] = pos1;
					this.button40.PerformClick();
					return;
				}
				if (pos0 > nxt0 && G.SS.PLM_BSLA[0] > 0) {
					G.PLM_BSL[0] = G.SS.PLM_BSLA[0];
				}
				else {
					G.PLM_BSL[0] = 0;
				}
				if (pos1 > nxt1 && G.SS.PLM_BSLA[1] > 0) {
					G.PLM_BSL[1] = G.SS.PLM_BSLA[1];
				}
				else {
					G.PLM_BSL[1] = 0;
				}
				D.SET_STG_ABS(0, nxt0 - G.PLM_BSL[0]);
				D.SET_STG_ABS(1, nxt1 - G.PLM_BSL[1]);
				G.PLM_STS |= (1 | 2);
			}
			else if (sender == this.button27 || sender == this.button28 || sender == this.button29) {//F.POS
				int q = Convert.ToInt32(((Button)sender).Tag);
				int pos = G.PLM_POS[2];
				int nxt = G.SS.PLM_POSF[q] + G.SS.PLM_OFFS[2];
	
				if (this.button27.BackColor == Color.LightPink) {
					G.SS.PLM_POSF[q] = pos - G.SS.PLM_OFFS[2];
					this.button41.PerformClick();
					return;
				}
				if (pos > nxt && G.SS.PLM_BSLA[2] > 0) {
					G.PLM_BSL[2] = G.SS.PLM_BSLA[2];
				}
				else {
					G.PLM_BSL[2] = 0;
				}
				D.SET_STG_ABS(2, nxt - G.PLM_BSL[2]);
				G.PLM_STS |= (4);
			}
			else if (sender == this.button30 || sender == this.button31 || sender == this.button32) {//Z.POS
				int q = Convert.ToInt32(((Button)sender).Tag);
				int pos = G.PLM_POS[3];
				int nxt = G.SS.PLM_POSZ[q];

				if (this.button30.BackColor == Color.LightPink) {
					G.SS.PLM_POSZ[q] = pos;
					this.button42.PerformClick();
					return;
				}
				if (pos > nxt && G.SS.PLM_BSLA[3] > 0) {
					G.PLM_BSL[3] = G.SS.PLM_BSLA[3];
				}
				else {
					G.PLM_BSL[3] = 0;
				}
				D.SET_STG_ABS(3, nxt - G.PLM_BSL[3]);
				G.PLM_STS |= (8);
			}
			else if (sender == this.button1) {
				//Z/FOCUS:ZERO
				G.SS.PLM_OFFS[2] = G.PLM_POS[2];
				G.PLM_STS |= (4);
			}
			else if (sender == this.button40) {
				if (this.button24.BackColor == Color.LightPink) {
					this.button24.BackColor = this.button25.BackColor = this.button26.BackColor = Color.Transparent;
				}
				else {
					this.button24.BackColor = this.button25.BackColor = this.button26.BackColor = Color.LightPink;
				}
			}
			else if (sender == this.button41) {
				if (this.button27.BackColor == Color.LightPink) {
					this.button27.BackColor = this.button28.BackColor = this.button29.BackColor = Color.Transparent;
				}
				else {
					this.button27.BackColor = this.button28.BackColor = this.button29.BackColor = Color.LightPink;
				}
			}
			else if (sender == this.button42) {
				if (this.button30.BackColor == Color.LightPink) {
					this.button30.BackColor = this.button31.BackColor = this.button32.BackColor = Color.Transparent;
				}
				else {
					this.button30.BackColor = this.button31.BackColor = this.button32.BackColor = Color.LightPink;
				}
			}
			//else if (sender == this.button39) {//
			//    if (G.FORMCAM == null) {
			//        G.FORMCAM = new FormCam();
			//        G.FORMCAM.Show();
			//        CAM_INIT();
			//    }
			//    else {
			//        G.FORMCAM.Dispose();
			//        G.FORMCAM = null;
			//    }
			//}
			else if (sender == this.button43) {//再現性:原点
				int q = this.comboBox1.SelectedIndex;
				if (q >= 0 && q <= 3) {
					D.SET_STG_ORG(q);
					G.PLM_STS |= (1 << q);
					m_bORGPROC = true;
				}
			}
			else if (sender == this.button44) {//再現性:停止
				int q = this.comboBox1.SelectedIndex;
				if (q >= 0 && q <= 3) {
					D.SET_STG_STOP(q);
					G.PLM_STS |= (1 << q);
				}
				timer2.Enabled = false;
			}
			else if (sender == this.button47) {//再現性:再現性
				//do_repeatability();
				//再現性テスト
				this.REP_STS = 1;
				this.timer2.Tag = null;
				this.timer2.Enabled = true;
			}
			//---------------------------
			//UPDSTS();
			if (G.PLM_STS != 0) {
				this.timer1.Interval = 100;
				if (this.timer1.Enabled == false) {
					this.timer1.Enabled = true;
				}
			}
		}
		private void disp_zoom_rate()
		{
			double Y;
			Y = G.SS.ZOM_PLS_A * G.PLM_POS[3] + G.SS.ZOM_PLS_B;
			this.label1.Text = string.Format("x{0:F2}", Y);
		}
		//private int aa;
		//private int[] bb = { 1, 2, 3 };
		
		private void timer1_Tick(object sender, EventArgs e)
		{
			if (G.PLM_STS == 0) {
				timer1.Interval = 500;
			}
			else {
				timer1.Interval = 100;
			}
			if (!D.isCONNECTED()) {
				return;
			}
			D.GET_STG_STS(G.PLM_STS_BIT);

			for (int i = 0; i < 4; i++) {
				string buf = "";
				if ((G.PLM_STS_BIT[i] & (int)G.PLM_STS_BITS.BIT_LMT_H) != 0) {
					buf += "L.H ";
				}
				if ((G.PLM_STS_BIT[i] & (int)G.PLM_STS_BITS.BIT_LMT_M) != 0) {
					buf += "L.M ";
				}
				if ((G.PLM_STS_BIT[i] & (int)G.PLM_STS_BITS.BIT_LMT_P) != 0) {
					buf += "L.P ";
				}
				if (true) {
					m_lblSts1[i].Text = m_lblSts2[i].Text = buf.Trim();
					if (this.tabControl1.TabPages.Count >= 3) {
						if (i == this.comboBox1.SelectedIndex) {
							this.label4.Text = buf.Trim();
						}
					}
				}
				if ((G.PLM_STS & (1 << i)) != 0) {
					//POS
					G.PLM_POS[i] =  D.GET_STG_POS(i);
					int	pos = G.PLM_POS[i];
					if (i == 2) {
						pos -= G.SS.PLM_OFFS[2];
					}
					m_txtPos1[i].Text = m_txtPos2[i].Text = pos.ToString();
					if (this.tabControl1.TabPages.Count >= 3) {
						if (i == this.comboBox1.SelectedIndex) {
							this.textBox1.Text = pos.ToString();
						}
					}
					if ((G.PLM_STS_BIT[i] & (int)G.PLM_STS_BITS.BIT_ONMOV) == 0) {
						if (G.SS.PLM_PWSV[i] == true) {
							if ((G.STG_TRQ_STS & (1<<i)) != 0) {
								D.SET_STG_TRQ(i, 0);//TRQをLOにする
							}
						}
						//---
						if (G.PLM_BSL[i] > 0) {
							D.SET_STG_REL(i, G.PLM_BSL[i]);
							G.PLM_BSL[i] = 0;
						}
						else {
							G.PLM_STS &= ~(1 << i);
							if (i == 3) {
								disp_zoom_rate();
							}
						}
					}
					if (i == 3 && this.button37.Tag == null) {
						disp_zoom_rate();
					}
				}
			}
			if (this.button37.Tag != null) {
				if ((int)this.button37.Tag == 1 && G.PLM_STS == 0) {
					this.button37.Tag = 2;
					//原点.all後の初期位置へ移動
					D.SET_STG_ABS(0, G.SS.PLM_POSX[3]);
					D.SET_STG_ABS(1, G.SS.PLM_POSY[3]);
					D.SET_STG_ABS(2, G.SS.PLM_POSF[3]);
					if (this.checkBox1.Checked) {
					D.SET_STG_ABS(3, G.SS.PLM_POSZ[3]);
					}
					G.PLM_STS |= (1 | 2 | 4 | 8);
				}
				if ((int)this.button37.Tag == 2 && G.PLM_STS == 0) {
					this.button37.Tag = null;
					this.UPDSTS();
					G.FORM12.UPDSTS();
				}
			}
			if (m_bORGPROC && G.PLM_STS == 0) {//原点ch1-ch4
				m_bORGPROC = false;
				this.UPDSTS();
				G.FORM12.UPDSTS();
			}
		}
		private void buttons_MouseDown(object sender, MouseEventArgs e)
		{
			int q = Convert.ToInt32(((Button)sender).Tag);
			int d = 1;

			if (false) {
			}
			else if (sender == this.button20) {//ch1:←
				d = 1;//CW(+):(カメラとの兼ね合いで方向を逆に)
			}
			else if (sender == this.button21) {//ch1:→
				d = 0;//CCW(-):(カメラとの兼ね合いで方向を逆に)
			}
			else if (sender == this.button22) {//ch2:↑
				d = 0;//CCW(-)
			}
			else if (sender == this.button23) {//ch2:↓
				d = 1;//CW(+)
			}
			else if (sender == this.button33) {//ch3:↑
				d = 0;//CCW(-)
			}
			else if (sender == this.button34) {//ch3:↓
				d = 1;//CW(+)
			}
			else if (sender == this.button35) {//ch4:↑
				d = 0;//CCW(-)
			}
			else if (sender == this.button36) {//ch4:↓
				d = 1;//CW(+)
			}
			else {
				return;
			}
			D.SET_STG_JOG(q, d);
			G.PLM_STS |= (1 << q);
			this.timer1.Interval = 100;
#if true
			m_ijog = q;
			m_event.Set();
#endif
		}

		private void buttons_MouseUp(object sender, MouseEventArgs e)
		{
#if false
			int q = Convert.ToInt32(((Button)sender).Tag);
			D.SET_STG_STOP(q);
			G.PLM_STS |= (1 << q);
			this.timer1.Interval = 100;
#endif
		}
		public void INIT()
		{
			D.PRESET_PARAM();
			G.PLM_STS |= (1 | 2 | 4 | 8);
			this.button38.PerformClick();
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
		private void MOVE_REL_PIX(int x, int y)
		{
			double xum = G.FORM02.PX2UM(x);
			double yum = G.FORM02.PX2UM(y);
			int	xpl = (int)(xum / G.SS.PLM_UMPP[0]);
			int ypl = (int)(yum / G.SS.PLM_UMPP[1]);
			MOVE_REL(xpl, ypl);
		}
		private void MOVE_REL(int x, int y)
		{
			MOVE_ABS(G.PLM_POS[0] + x, G.PLM_POS[1] + y);
		}
		private void MOVE_ABS(int x, int y)
		{
			if (G.PLM_POS[0] != x) {
				D.SET_STG_ABS(0, x);
				G.PLM_STS |= (1 << 0);
			}
			if (G.PLM_POS[1] != y) {
				D.SET_STG_ABS(1, y);
				G.PLM_STS |= (1 << 1);
			}
		}
		public void DO_ORG_ALL()
		{
			OnClicks(this.button37, null);
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			int i = this.comboBox1.SelectedIndex;
			if (i < 0 || i > 3 || isORG_ALL_DONE() == false) {
				this.button43.Enabled = false;
				this.button44.Enabled = false;
				this.button45.Enabled = false;
				this.button46.Enabled = false;
				this.button47.Enabled = false;
				this.textBox1.Text = "";
				this.label4.Text = "";
				return;
			}
			this.button43.Enabled = true;
			this.button44.Enabled = true;
			this.button45.Enabled = true;
			this.button46.Enabled = true;
			this.button47.Enabled = true;
			this.textBox1.Text = m_txtPos1[i].Text;
			this.label4.Text = m_lblSts1[i].Text;
			//---
			int mlim = G.SS.PLM_MLIM[i];
			int plim = G.SS.PLM_PLIM[i];
			int wid = (plim-mlim)/1;
			int cen = (plim+mlim)/2;
			wid = (wid/10)*10;
			cen = (cen/10)*10;
			this.numericUpDown6.Value = cen-wid/4;
			this.numericUpDown7.Value = cen+wid/4;
		}
		private void do_repeatability()
		{
			//再現性テスト
		}

		private void button45_MouseDown(object sender, MouseEventArgs e)
		{
			int d = (sender == this.button45) ? 0:1;	//0:CCW(-), 1:CW(+)
			int q = (this.comboBox1.SelectedIndex);
			D.SET_STG_JOG(q, d);
			G.PLM_STS |= (1 << q);
			this.timer1.Interval = 100;
			m_ijog = q;
			m_event.Set();
		}
		private void scroll_to_end(TextBox tb, string txt)
		{
			tb.Text = txt;
			tb.SelectionStart = tb.Text.Length;
			tb.Focus();
			tb.ScrollToCaret();
			tb.Update();
		}
		private int m_rep_cnt;
		private int m_rep_tic;
		private int m_rep_idx;
		private int m_rep_axs;
		private int[] m_rep_pos = null;
		private void timer2_Tick(object sender, EventArgs e)
		{
			int NXT_STS = this.REP_STS+1;
			double fmax;
			int imax;

			this.timer2.Enabled = false;
		

			switch (this.REP_STS) {
			case 0:
				break;
			case 1:
				m_rep_axs = this.comboBox1.SelectedIndex;
				m_rep_idx = m_rep_cnt = 0;
				{
					int min = (int)this.numericUpDown6.Value;
					int max = (int)this.numericUpDown7.Value;
					int wid = max-min;
					ArrayList ar = new ArrayList();
					for (int i = 0; i < 5; i++) {
						ar.Add(min+i*(wid/5));
					}
					ar.Add(max);
					ar.Add(0);
					m_rep_pos = (int[])ar.ToArray(typeof(int));
				}
				this.textBox2.Text = "";
				break;
			case 2:
				if (++m_rep_cnt > this.numericUpDown5.Value) {
					this.REP_STS = 0;
					timer2.Enabled = false;
				}
				if (this.checkBox2.Checked) {
					NXT_STS = 20;
				}
				break;
			case 3:
				{
					int q = this.comboBox1.SelectedIndex;
					int pos = G.PLM_POS[q];
					int nxt = m_rep_pos[m_rep_idx];
	

					if (pos > nxt && G.SS.PLM_BSLA[q] > 0) {
						G.PLM_BSL[q] = G.SS.PLM_BSLA[q];
					}
					else {
						G.PLM_BSL[q] = 0;
					}
					D.SET_STG_ABS(q, nxt - G.PLM_BSL[q]);
					G.PLM_STS |= (1<<q);
				}
			break;
			case 4:
			case 8:
			case 11:
			case 21:
				if (G.PLM_STS != 0) {
					NXT_STS = this.REP_STS;//停止待ち
				}
			break;
			case 5:
				if (++m_rep_idx < m_rep_pos.Length) {
					NXT_STS = 3;
				}
				else {
					m_rep_idx = 0;
				}
				break;
			case 6:
				{
					int q = this.comboBox1.SelectedIndex;
					if ((G.PLM_STS_BIT[q] & (int)G.PLM_STS_BITS.BIT_LMT_H) != 0) {
						//LMT_HがLOになるまで,+1移動
					}
					else {
						//LMT_HがHIになるまで,-1移動
					}
				}
			break;
			case 7:
				{
					int q = this.comboBox1.SelectedIndex;
					if ((G.PLM_STS_BIT[q] & (int)G.PLM_STS_BITS.BIT_LMT_H) != 0) {
						//LMT_HがLOになるまで,+1移動
						D.SET_STG_REL(q, +1);
						G.PLM_STS |= (1<<q);
					}
					else {
						string txt = this.textBox2.Text;
						txt += string.Format("{0}\r\n", G.PLM_POS[q]);
						scroll_to_end(this.textBox2, txt);
						NXT_STS = 13;
					}
				}
			break;
			case 9:
				NXT_STS = 7;
			break;
			case 10:
				{
					int q = this.comboBox1.SelectedIndex;
					if ((G.PLM_STS_BIT[q] & (int)G.PLM_STS_BITS.BIT_LMT_H) == 0) {
						//LMT_HがHIになるまで,-1移動
						D.SET_STG_REL(q, -1);
						G.PLM_STS |= (1<<q);
					}
					else {
						NXT_STS = 7;
					}
				}
			break;
			case 12:
				NXT_STS = 10;
			break;
			case 13:
				m_rep_tic = Environment.TickCount;
			break;
			case 14:
				if ((Environment.TickCount - m_rep_tic) < 3000) {//3秒待ち
					NXT_STS = this.REP_STS;
				}
				break;
			case 15:
				NXT_STS = 2;
			break;
			case 20:
				{
					int q = this.comboBox1.SelectedIndex;
					if (q >= 0 && q <= 3) {
						D.SET_STG_ORG(q);
						G.PLM_STS |= (1 << q);
						m_bORGPROC = true;
					}
				}
			break;
			case 22:
				NXT_STS = 3;
			break;
			default:
				//f軸停止待ち
				break;
			}
			if (NXT_STS == 0) {
				NXT_STS = 0;//for break.point
			}
			if (this.REP_STS != 0) {
				this.REP_STS = NXT_STS;
				this.timer2.Interval = 1;
				this.timer2.Enabled = true;
			}
		}
	}
}
