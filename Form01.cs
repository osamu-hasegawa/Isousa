using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace uSCOPE
{
    public partial class Form01 : Form
    {
        public Form01()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
			//C:\Users\araya320\AppData\Roaming\KOP\uSCOPE (<-セットアップにてコピーされる)
			//から
			//C:\Users\araya320\Documents\KOP\uSCOPE
			//へコピーし、元ファイルを削除する
			G.COPY_SETTINGS("settings.xml");
 			//---
			G.AS.load(ref G.AS);
			G.SS.load(ref G.SS);
			//---
			G.SS.AUT_BEF_PATH = G.AS.AUT_BEF_PATH;
			G.SS.BEFORE_PATH  = G.AS.BEFORE_PATH;
			G.SS.PLM_AUT_FOLD = G.AS.PLM_AUT_FOLD;
			G.SS.MOZ_CND_FOLD = G.AS.MOZ_CND_FOLD;
			//---

			if (G.SS.ETC_SPE_CD01 == 0) {
				G.SS.ETC_SPE_CD01 = 1;
				G.SS.PLM_POSWT[0] = G.SS.PLM_POSFT[0] = G.SS.PLM_POSZT[0] = "メモ１";
				G.SS.PLM_POSWT[1] = G.SS.PLM_POSFT[1] = G.SS.PLM_POSZT[1] = "メモ２";
				G.SS.PLM_POSWT[2] = G.SS.PLM_POSFT[2] = G.SS.PLM_POSZT[2] = "メモ３";
			}
			//---
			this.Left = G.AS.APP_F01_LFT;
			this.Top = G.AS.APP_F01_TOP;
			G.FORM01 = this;
			//---
			G.FORM10 = new Form10();
			G.FORM10.TopLevel = false;
			G.FORM11 = new Form11();
			G.FORM11.TopLevel = false;
			G.FORM12 = new Form12();
			G.FORM12.TopLevel = false;
			G.FORM13 = new Form13();
			G.FORM13.TopLevel = false;
			if (true) {
				//プログラム起動時のUIFレベルとして記憶
				G.UIF_LEVL = G.SS.ETC_UIF_LEVL;
			}
			/*
				0:ユーザ用(暫定版)
				1:ユーザ用
				2:開発者用(一度)
				3:開発者用(常に)
			 */
			if (G.UIF_LEVL == 0 || G.UIF_LEVL == 1) {
				this.button3.Visible = false;//設定
			}
			if (G.UIF_LEVL == 0) {
				G.FORM11.SET_UIF_USER();
				G.FORM12.SET_UIF_USER();
			}
			//---
			this.groupBox1.Controls.Add(G.FORM10);
			G.FORM10.Location = new Point(3, 12);
			G.FORM10.Show();
			G.FORM10.BringToFront();
			//---
			this.groupBox2.Controls.Add(G.FORM11);
			G.FORM11.Location = new Point(3, 12);
			G.FORM11.Show();
			G.FORM11.BringToFront();
			//---
			this.groupBox3.Controls.Add(G.FORM12);
			G.FORM12.Location = new Point(3, 12);
			G.FORM12.Show();
			G.FORM12.BringToFront();
			//---
			G.FORM10.UPDSTS();
			G.FORM11.UPDSTS();
			G.FORM12.UPDSTS();
			//---
			UPDSTS();
			//---
			if (G.UIF_LEVL == 1/*ユーザ用*/) {
				this.Text = "uSCOPE Application";
				BeginInvoke(new G.DLG_VOID_VOID(this.UIF_LEVL1_INIT));
			}
			else {
				this.Text = this.Text + Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
			if (G.UIF_LEVL == 2/*開発者用(一度)*/) {
				G.SS.ETC_UIF_LEVL = G.SS.ETC_UIF_BACK;
			}
		}
		private void UIF_LEVL1_INIT()
		{
			//---
			this.button1.Visible = false;
			this.button2.Visible = false;
			this.button3.Visible = false;
			this.groupBox1.Visible = false;
			this.groupBox2.Visible = false;
			this.groupBox3.Visible = false;
			this.Controls.Add(G.FORM13);
			int w0 = this.Width;
			int w1 = G.FORM13.Width;
			this.Height = G.FORM13.Height+(w0-w1)+12;
			G.FORM13.Location = new Point(3, 3);
			G.FORM13.Show();
			G.FORM13.BringToFront();
			G.FORM13.Dock = DockStyle.Fill;
			//CONNECT
			OnClicks(this.button1, null);
			if (!D.isCONNECTED()) {
				BeginInvoke(new G.DLG_VOID_VOID(this.Close));
				return;
			}
			//OPEN(CAMERA)
			G.FORM02 = new Form02();
			G.FORM02.Show();
			if (!G.FORM02.isCONNECTED()) {
				BeginInvoke(new G.DLG_VOID_VOID(this.Close));
				return;
			}
			G.FORM13.START_TIMER();
		}
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
			if (G.FORM02 != null) {
				G.FORM02.cam_close();
				//G.FORM02.m_bcontinuous = false;
				//G.FORM02.Stop();
				//G.FORM02.DestroyCamera();
				G.FORM02.Close();
				//G.FORM02.Dispose();
				G.FORM02 = null;
			}
			if (true) {
				G.FORM10.LED_SET(0, false);
				G.FORM10.LED_SET(1, false);
				G.FORM10.LED_SET(2, false);
			}
			//---
			if (this.Left <= -32000 || this.Top <= -32000) {
				//最小化時は更新しない
			}
			else {
			G.AS.APP_F01_LFT = this.Left;
			G.AS.APP_F01_TOP = this.Top; 
			}
			//---
			G.AS.AUT_BEF_PATH = G.SS.AUT_BEF_PATH;
			G.AS.BEFORE_PATH  = G.SS.BEFORE_PATH ;
			G.AS.PLM_AUT_FOLD = G.SS.PLM_AUT_FOLD;
			G.AS.MOZ_CND_FOLD = G.SS.MOZ_CND_FOLD;
			//---
			if (G.SS.ETC_UIF_LEVL == 0 || G.SS.ETC_UIF_LEVL == 1) {
				G.SS.ETC_UIF_BACK = G.SS.ETC_UIF_LEVL;
			}
			//---
			G.AS.save(G.AS);
			G.SS.save(G.SS);
			//---
			if (G.AS.DEBUG_MODE == 1) {
				DBGMODE.TERM();
			}
		}
		private void UPDSTS()
		{
			if (!D.isCONNECTED())
			{
				this.button1.Enabled = true;
				this.button2.Enabled = false;
				//this.groupBox1.Enabled = false;
				//this.groupBox2.Enabled = false;
				//this.tabControl1.Enabled = false;
				//this.groupBox6.Enabled = false;
				return;
			}
			this.button1.Enabled = false;
			this.button2.Enabled = true;
//			this.groupBox1.Enabled = true;
//			this.groupBox2.Enabled = true;
//			this.groupBox3.Enabled = true;
			//---
			//---
		}
        private void OnClicks(object sender, EventArgs e)
        {
			if (false) {
			}
			else if (sender == this.button1) {
				if (D.INIT()) {
					G.FORM11.INIT();
					G.FORM10.UPDSTS();
					G.FORM11.UPDSTS();
					G.FORM12.UPDSTS();
				}
			}
			else if (sender == this.button2) {
				G.FORM10.LED_SET(0, false);
				G.FORM10.LED_SET(1, false);
				G.FORM10.LED_SET(2, false);
				D.TERM();
				G.FORM10.UPDSTS();
				G.FORM11.UPDSTS();
				G.FORM12.UPDSTS();
			}
			else if (sender == this.button3) {
				frmSettings frm = new frmSettings();
				frm.m_ss = (G.SYSSET)G.SS.Clone();
				if (frm.ShowDialog(this) == System.Windows.Forms.DialogResult.OK) {
					G.SS = (G.SYSSET)frm.m_ss.Clone();
					G.SS.save(G.SS);
				}

				//G.LOAD();
				//G.mlog("#iINIファイルを再読み込みしました.");
			}
			//---------------------------
			UPDSTS();
		}

		private void Form01_Resize(object sender, EventArgs e)
		{
			if (G.FORM02 != null) {
				if (this.WindowState == FormWindowState.Minimized) {
					G.FORM02.WindowState = FormWindowState.Minimized;
				}
				else if (this.WindowState == FormWindowState.Normal) {
					G.FORM02.WindowState = FormWindowState.Normal;
				}
			}
		}

		static bool ctrlKeyFlg = false;
		static bool AKeyFlg = false;
		static bool BKeyFlg = false;
		static bool CKeyFlg = false;

		private void Form01_KeyDown(object sender, KeyEventArgs e)
		{
			bool flag = true;

#if false
			if (false) {
			}
			else if ((System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.LeftCtrl) & System.Windows.Input.KeyStates.Down)== 0) {
				flag = false;
			}
			else if ((System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.A) & System.Windows.Input.KeyStates.Down)== 0) {
				flag = false;
			}
			else if ((System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.B) & System.Windows.Input.KeyStates.Down)== 0) {
				flag = false;
			}
			else if ((System.Windows.Input.Keyboard.GetKeyStates(System.Windows.Input.Key.C) & System.Windows.Input.KeyStates.Down)== 0) {
				flag = false;
			}
#else
			flag = false;
			if(ctrlKeyFlg == false && e.KeyCode == Keys.ControlKey)
			{
				ctrlKeyFlg = true;
			}
			if(AKeyFlg == false && e.KeyCode == Keys.A)
			{
				AKeyFlg = true;
			}
			if(BKeyFlg == false && e.KeyCode == Keys.B)
			{
				BKeyFlg = true;
			}
			if(CKeyFlg == false && e.KeyCode == Keys.C)
			{
				CKeyFlg = true;
			}

            if(ctrlKeyFlg && AKeyFlg && BKeyFlg && CKeyFlg)
            {
                flag = true;
                ctrlKeyFlg = false;
                AKeyFlg = false;
                BKeyFlg = false;
                CKeyFlg = false;
            }
#endif
            if (flag) {
			//this.KeyPreview = false;
			//G.mlog("#iソフトウェアは次回起動時にユーザモードで起動します。");
			var frm = new frmMessage();
			frm.ShowDialog(this);
            }
        }
    }
}
