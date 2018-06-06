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
	public partial class Form20 : Form
	{
		public G.SYSSET m_ss;

		public Form20()
		{
			InitializeComponent();
		}

		private void Form20_Load(object sender, EventArgs e)
		{
			m_ss = G.SS;
			if (string.IsNullOrEmpty(m_ss.PLM_AUT_FOLD)) {
				string path;
				path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += @"\KOP";
				path += @"\" + Application.ProductName;
				m_ss.PLM_AUT_FOLD = path;
			}
			DDX(true);
			radioButton1_Click(null, null);
			checkBox2_Click(null, null);
			comboBox3_SelectedIndexChanged(null, null);
			//---
			numericUpDown17_ValueChanged(null, null);
			checkBox4_Click(null, null);
			//---
		}

		private void Form20_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK) {
				return;
			}
			if (DDX(false) == false) {
				e.Cancel = true;
			}
			else if (!System.IO.Directory.Exists(m_ss.PLM_AUT_FOLD)) {
				G.mlog("指定されたフォルダは存在しません.\r\r" + m_ss.PLM_AUT_FOLD);
				e.Cancel = true;
			}
			else {
				if (this.comboBox3.Text == "-") {
					G.mlog("有効な測定モードを選択してください.");
					this.comboBox3.Focus();
					e.Cancel = true;
					return;
				}
				G.SS = (G.SYSSET)m_ss.Clone();
			}
		}
		private bool DDX(bool bUpdate)
        {
            bool rc;
			try {
				DDV.DDX(bUpdate, this.textBox1, ref m_ss.PLM_AUT_TITL);
				//DDV.DDX(bUpdate, this.comboBox1, ref m_ss.PLM_AUT_SPOS);
				DDV.DDX(bUpdate, this.comboBox3, ref m_ss.PLM_AUT_MODE);
				DDV.DDX(bUpdate, this.numericUpDown2, ref m_ss.PLM_AUT_OVLP);
				DDV.DDX(bUpdate, this.numericUpDown1, ref m_ss.PLM_AUT_SKIP);
				//---
				DDV.DDX(bUpdate, this.checkBox5      , ref m_ss.PLM_AUT_CNST);
				DDV.DDX(bUpdate, this.checkBox3      , ref m_ss.PLM_AUT_RTRY);
				//---
				DDV.DDX(bUpdate, this.comboBox4      , ref m_ss.PLM_AUT_AFMD);
				DDV.DDX(bUpdate, this.numericUpDown4, ref m_ss.PLM_AUT_HANI);
				DDV.DDX(bUpdate, this.numericUpDown5, ref m_ss.PLM_AUT_DISL);
				DDV.DDX(bUpdate, this.numericUpDown6, ref m_ss.PLM_AUT_DISS);
				//---
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton1, this.radioButton2, this.radioButton3 }, ref G.SS.PLM_AUT_FCMD);
				DDV.DDX(bUpdate, this.numericUpDown3 , ref m_ss.PLM_AUT_CTDR);
				DDV.DDX(bUpdate, this.numericUpDown7 , ref m_ss.PLM_AUT_2HAN);
				DDV.DDX(bUpdate, this.numericUpDown8 , ref m_ss.PLM_AUT_2DSL);
				DDV.DDX(bUpdate, this.numericUpDown9 , ref m_ss.PLM_AUT_2DSS);
				DDV.DDX(bUpdate, this.checkBox1      , ref m_ss.PLM_AUT_2FST);
				//---
				DDV.DDX(bUpdate, this.comboBox2      , ref m_ss.PLM_AUT_FLTP);
				DDV.DDX(bUpdate, this.textBox2       , ref m_ss.PLM_AUT_FOLD);
				//---
				DDV.DDX(bUpdate, this.checkBox2      , ref m_ss.PLM_AUT_HPOS);
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton4, this.radioButton5}, ref G.SS.PLM_AUT_HMOD);
				DDV.DDX(bUpdate, this.numericUpDown10 , ref m_ss.PLM_AUT_HP_X);
				DDV.DDX(bUpdate, this.numericUpDown11 , ref m_ss.PLM_AUT_HP_Y);
				DDV.DDX(bUpdate, this.numericUpDown12, ref m_ss.PLM_AUT_HPRT);
				DDV.DDX(bUpdate, this.numericUpDown13, ref m_ss.PLM_AUT_HPMN);
				DDV.DDX(bUpdate, this.numericUpDown14, ref m_ss.PLM_AUT_HPMX);
				DDV.DDX(bUpdate, this.numericUpDown15, ref m_ss.PLM_AUT_HPSL);
				DDV.DDX(bUpdate, this.numericUpDown16, ref m_ss.PLM_AUT_HPSS);
				//---
				//DDV.DDX(bUpdate, this.checkBox4      , ref m_ss.PLM_AUT_ZMUL);
				//DDV.DDX(bUpdate, this.numericUpDown17, ref m_ss.PLM_AUT_ZHAN);
				//DDV.DDX(bUpdate, this.numericUpDown18, ref m_ss.PLM_AUT_ZSTP);
				//---
				DDV.DDX(bUpdate, this.checkBox6      , ref m_ss.PLM_AUT_ZDCK);//Ｚ測定:深度合成用
				DDV.DDX(bUpdate, this.textBox3       , ref m_ss.PLM_AUT_ZDEP, 20, -99, +99);
				DDV.DDX(bUpdate, this.checkBox7      , ref m_ss.PLM_AUT_ZKCK);//Ｚ測定:毛髪径判定用
				DDV.DDX(bUpdate, this.textBox4       , ref m_ss.PLM_AUT_ZKEI, 20, -99, +99);
				//---
				DDV.DDX(bUpdate, this.checkBox4      , ref m_ss.PLM_AUT_IRCK);//カラーと同時に赤外測定

				if (bUpdate == false) {
					if (this.textBox2.Text == "") {
						G.mlog("フォルダを指定してください.");
						this.textBox2.Focus();
						return(false);
					}
					char[] fc = {
						'\\', '/', ':', '*', '?', '\"', '<', '>', '|'
					};
					foreach (char c in fc) {
						if (this.textBox1.Text.IndexOf(c) >= 0) {
							this.textBox1.Focus();
							G.mlog("次の文字は使えません.\r\\ / : * ? \" < > |");
							return (false);
						}
					}
					//---
					if (m_ss.PLM_AUT_ZDEP != null) {
						for (int i = 0; i < m_ss.PLM_AUT_ZDEP.Length; i++) {
							int val = m_ss.PLM_AUT_ZDEP[i];
							int idxf, idxl;
							idxf = Array.IndexOf(m_ss.PLM_AUT_ZDEP, val);
							idxl = Array.LastIndexOf(m_ss.PLM_AUT_ZDEP, val);
							if (idxf != idxl) {
								G.mlog(string.Format("同じ値({0})が指定されています.", val));
								this.textBox3.Focus();
								return(false);
							}
						}
					}
					if (m_ss.PLM_AUT_ZKEI != null) {
						for (int i = 0; i < m_ss.PLM_AUT_ZKEI.Length; i++) {
							int val = m_ss.PLM_AUT_ZKEI[i];
							int idxf, idxl;
							idxf = Array.IndexOf(m_ss.PLM_AUT_ZKEI, val);
							idxl = Array.LastIndexOf(m_ss.PLM_AUT_ZKEI, val);
							if (idxf != idxl) {
								G.mlog(string.Format("同じ値({0})が指定されています.", val));
								this.textBox4.Focus();
								return(false);
							}
						}
					}
					if (!m_ss.PLM_AUT_ZDCK || !m_ss.PLM_AUT_ZKCK) {
					}
					else if (m_ss.PLM_AUT_ZDEP != null && m_ss.PLM_AUT_ZKEI != null) {
						for (int i = 0; i < m_ss.PLM_AUT_ZDEP.Length; i++) {
							int val = m_ss.PLM_AUT_ZDEP[i];
							int idxf;
							idxf = Array.IndexOf(m_ss.PLM_AUT_ZKEI, val);
							if (idxf >= 0) {
								G.mlog(string.Format("同じ値({0})が指定されています.", val));
								this.textBox3.Focus();
								return(false);
							}
						}
					}
					//---
					//if (m_ss.PLM_AUT_ZMUL) {
					//    if ((m_ss.PLM_AUT_ZHAN % m_ss.PLM_AUT_ZSTP) != 0) {
					//        this.numericUpDown18.Focus();
					//        G.mlog("測定範囲は測定ステップで割り切れる値で指定してください.");
					//        return(false);
					//    }
					//    if ((m_ss.PLM_AUT_ZHAN / m_ss.PLM_AUT_ZSTP) > 10) {
					//        this.numericUpDown18.Focus();
					//        G.mlog("測定ステップが小さすぎます.中心を含めて21位置以下になるように指定してください.");
					//        return(false);
					//    }
					//}
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

		private void OnClicks(object sender, EventArgs e)
		{
			if (sender == this.button3) {
				//FolderBrowserDialogクラスのインスタンスを作成
				FolderBrowserDialog fbd = new FolderBrowserDialog();

				//上部に表示する説明テキストを指定する
				fbd.Description = "フォルダを指定してください。";
				//ルートフォルダを指定する
				//デフォルトでDesktop
				fbd.RootFolder = Environment.SpecialFolder.Desktop;
				//最初に選択するフォルダを指定する
				//RootFolder以下にあるフォルダである必要がある
				fbd.SelectedPath = this.textBox2.Text;
				//ユーザーが新しいフォルダを作成できるようにする
				//デフォルトでTrue
				fbd.ShowNewFolderButton = true;

				//ダイアログを表示する
				if (fbd.ShowDialog(this) == DialogResult.OK)
				{
					this.textBox2.Text = fbd.SelectedPath;
				}
			}
		}

		private void numericUpDown4_ValueChanged(object sender, EventArgs e)
		{
			double f1, f2, f3;
			//---
			f1 = (double)this.numericUpDown4.Value;
			f2 = (double)this.numericUpDown5.Value;
			f3 = (double)this.numericUpDown6.Value;
			f1 *= G.SS.PLM_UMPP[2];
			f2 *= G.SS.PLM_UMPP[2];
			f3 *= G.SS.PLM_UMPP[2];
			this.label10.Text = string.Format("±{0:F1} / {1:F1} / {2:F1} um", f1, f2, f3);
			//---
			f1 = (double)this.numericUpDown7.Value;
			f2 = (double)this.numericUpDown8.Value;
			f3 = (double)this.numericUpDown9.Value;
			f1 *= G.SS.PLM_UMPP[2];
			f2 *= G.SS.PLM_UMPP[2];
			f3 *= G.SS.PLM_UMPP[2];
			this.label16.Text = string.Format("±{0:F1} / {1:F1} / {2:F1} um", f1, f2, f3);
//			this.label16.Text = string.Format("±{0:F1} / {1:F1} um", f1, f2);
		}

		private void radioButton1_Click(object sender, EventArgs e)
		{
			if (this.radioButton1.Checked) {
				this.numericUpDown7.Enabled = false;
				this.numericUpDown8.Enabled = false;
				this.checkBox1.Enabled = false;
			}
			else {
				this.numericUpDown7.Enabled = true;
				this.numericUpDown8.Enabled = true;
				this.checkBox1.Enabled = true;
			}
			if (this.radioButton3.Checked) {
				this.numericUpDown3.Enabled = true;
			}
			else {
				this.numericUpDown3.Enabled = false;
			}
		}

		private void checkBox2_Click(object sender, EventArgs e)
		{
			bool bl = (this.checkBox2.Checked == true);

			//this.numericUpDown10.Enabled = bl;
			//this.numericUpDown11.Enabled = bl;
			this.numericUpDown12.Enabled = bl;
			this.numericUpDown13.Enabled = bl;
			this.numericUpDown14.Enabled = bl;
			this.numericUpDown15.Enabled = bl;
			this.numericUpDown16.Enabled = bl;
		}

		private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.comboBox3.SelectedIndex == 5 || this.comboBox3.SelectedIndex == 8) {
				//5:反射
				//8:反射→赤外
				this.checkBox3.Enabled = true;
			}
			else {
				this.checkBox3.Enabled = false;
			}
		}

		private void numericUpDown17_ValueChanged(object sender, EventArgs e)
		{
			//double f1, f2;
			////---
			//f1 = (double)this.numericUpDown17.Value;
			//f2 = (double)this.numericUpDown18.Value;
			//f1 *= G.SS.PLM_UMPP[2];
			//f2 *= G.SS.PLM_UMPP[2];
			//this.label23.Text = string.Format("±{0:F1} / {1:F1} um", f1, f2);
		}

		private void checkBox4_Click(object sender, EventArgs e)
		{
			//bool bl = (this.checkBox4.Checked == true);

			//this.numericUpDown17.Enabled = bl;
			//this.numericUpDown18.Enabled = bl;
			textBox3.Enabled = (this.checkBox6.Checked == true);
			textBox4.Enabled = (this.checkBox7.Checked == true);
		}
	}
}
