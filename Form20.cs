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
#if true//2019.03.02(直線近似)
				DDV.DDX(bUpdate, this.numericUpDown20,ref m_ss.PLM_AUT_DISM);
#endif
				//---
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton1, this.radioButton2, this.radioButton3 }, ref G.SS.PLM_AUT_FCMD);
				DDV.DDX(bUpdate, this.numericUpDown3 , ref m_ss.PLM_AUT_CTDR);
				DDV.DDX(bUpdate, this.numericUpDown7 , ref m_ss.PLM_AUT_2HAN);
				DDV.DDX(bUpdate, this.numericUpDown8 , ref m_ss.PLM_AUT_2DSL);
				DDV.DDX(bUpdate, this.numericUpDown9 , ref m_ss.PLM_AUT_2DSS);
#if true//2019.03.02(直線近似)
				DDV.DDX(bUpdate, this.numericUpDown21, ref m_ss.PLM_AUT_2DSM);
#endif
#if false//2019.03.18(AF順序)
				DDV.DDX(bUpdate, this.checkBox1      , ref m_ss.PLM_AUT_2FST);
#endif
				//---
				DDV.DDX(bUpdate, this.comboBox2      , ref m_ss.PLM_AUT_FLTP);
				DDV.DDX(bUpdate, this.textBox2       , ref m_ss.PLM_AUT_FOLD);
				//---
				DDV.DDX(bUpdate, this.checkBox2      , ref m_ss.PLM_AUT_HPOS);
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton4, this.radioButton5}, ref G.SS.PLM_AUT_HMOD);
				DDV.DDX(bUpdate, this.numericUpDown10 , ref m_ss.PLM_AUT_HP_X, G.SS.PLM_MLIM[0], G.SS.PLM_PLIM[0]);
				DDV.DDX(bUpdate, this.numericUpDown11 , ref m_ss.PLM_AUT_HP_Y, G.SS.PLM_MLIM[1], G.SS.PLM_PLIM[1]);
#if true//2018.12.22(測定抜け対応)
				DDV.DDX(bUpdate, this.numericUpDown12 , ref m_ss.PLM_AUT_HP_Z, G.SS.PLM_MLIM[2], G.SS.PLM_PLIM[2]);
				DDV.DDX(bUpdate, this.numericUpDown14 , ref m_ss.PLM_AUT_ED_Y, G.SS.PLM_MLIM[1], G.SS.PLM_PLIM[1]);
                if (bUpdate == false) {
					if (m_ss.PLM_AUT_ED_Y <= m_ss.PLM_AUT_HP_Y) {
						G.mlog("終了ステージ位置:yは開始位置:yより大きい値を指定してください.");
						this.numericUpDown14.Focus();
						return(false);
					}
                }
				DDV.DDX(bUpdate, this.checkBox10     , ref m_ss.PLM_AUT_NUKE);
#endif
#if true//2019.01.23(GAIN調整&自動測定)
				DDV.DDX(bUpdate, this.checkBox11     , ref m_ss.PLM_AUT_V_PK);
#endif
				DDV.DDX(bUpdate, this.numericUpDown17, ref m_ss.PLM_AUT_HPRT);
				DDV.DDX(bUpdate, this.numericUpDown18, ref m_ss.PLM_AUT_HPMN);
				DDV.DDX(bUpdate, this.numericUpDown19, ref m_ss.PLM_AUT_HPMX);
				DDV.DDX(bUpdate, this.numericUpDown15, ref m_ss.PLM_AUT_HPSL);
				DDV.DDX(bUpdate, this.numericUpDown16, ref m_ss.PLM_AUT_HPSS);
#if true//2019.03.02(直線近似)
				DDV.DDX(bUpdate, this.numericUpDown22, ref m_ss.PLM_AUT_HPSM);
#endif
				//---
				//DDV.DDX(bUpdate, this.checkBox4      , ref m_ss.PLM_AUT_ZMUL);
				//DDV.DDX(bUpdate, this.numericUpDown17, ref m_ss.PLM_AUT_ZHAN);
				//DDV.DDX(bUpdate, this.numericUpDown18, ref m_ss.PLM_AUT_ZSTP);
				//---
				DDV.DDX(bUpdate, this.checkBox6      , ref m_ss.PLM_AUT_ZDCK);//Ｚ測定:深度合成用
#if true//2018.07.02
				DDV.DDX(bUpdate, this.textBox3       , ref m_ss.PLM_AUT_ZDEP, 50, -99, +99);
#else
				DDV.DDX(bUpdate, this.textBox3       , ref m_ss.PLM_AUT_ZDEP, 20, -99, +99);
#endif
				DDV.DDX(bUpdate, this.checkBox7      , ref m_ss.PLM_AUT_ZKCK);//Ｚ測定:毛髪径判定用
#if true//2018.07.02
				DDV.DDX(bUpdate, this.textBox4       , ref m_ss.PLM_AUT_ZKEI, 50, -99, +99);
#else
				DDV.DDX(bUpdate, this.textBox4       , ref m_ss.PLM_AUT_ZKEI, 20, -99, +99);
#endif
				//---
				DDV.DDX(bUpdate, this.checkBox4      , ref m_ss.PLM_AUT_IRCK);//カラーと同時に赤外測定
#if true//2018.08.16
				DDV.DDX(bUpdate, this.checkBox8, ref m_ss.PLM_AUT_ZORG);//Z軸原点
				DDV.DDX(bUpdate, this.checkBox9, ref m_ss.PLM_AUT_ZNOR);//右側カット
#endif
#if true//2019.02.14(Z軸初期位置戻し)
				DDV.DDX(bUpdate, this.checkBox12,ref m_ss.PLM_AUT_ZRET);//Z軸初期位置戻し
#endif
#if true//2019.03.02(直線近似)
				DDV.DDX(bUpdate, this.checkBox13,ref m_ss.PLM_AUT_AF_2);//AF2使用
#endif
#if true//2019.03.18(AF順序)
				DDV.DDX(bUpdate, this.checkBox14,ref m_ss.IMP_AUT_EXAF);//測定順序を中心→表面
#endif
#if true//2019.04.01(表面赤外省略)
				DDV.DDX(bUpdate, this.checkBox15,ref m_ss.PLM_AUT_NOSF);//表面赤外省略
#endif
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
#if true//2018.09.29(キューティクルライン検出)
							if (val == 0) {
								G.mlog("0が指定されています.");
								this.textBox3.Focus();
								return(false);
							}
#endif
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
#if true//2018.09.29(キューティクルライン検出)
							if (val == 0) {
								G.mlog("0が指定されています.");
#if true//2018.11.13(毛髪中心AF)
								this.textBox4.Focus();
#else
								this.textBox3.Focus();
#endif
								return(false);
							}
#endif
						}
					}
					if (!m_ss.PLM_AUT_ZDCK || !m_ss.PLM_AUT_ZKCK) {
					}
					else if (m_ss.PLM_AUT_ZDEP != null && m_ss.PLM_AUT_ZKEI != null) {
#if false//2018.11.13(毛髪中心AF)
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
#endif
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
#if true//2019.03.18(AF順序)
					if (m_ss.IMP_AUT_EXAF && m_ss.PLM_AUT_ZDCK == false) {
						G.mlog("「毛髪径判定」が選択されていません.");
						this.checkBox14.Focus();
					}
#endif
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
#if true//2019.03.02(直線近似)
			if (!this.checkBox13.Checked) {
				check_for_af2();
			}
#endif
			if (this.radioButton1.Checked) {//初回のみ
				this.numericUpDown7.Enabled = false;
				this.numericUpDown8.Enabled = false;
#if true//2019.03.02(直線近似)
				this.numericUpDown9.Enabled = false;
				this.numericUpDown21.Enabled = false;
#endif
				this.checkBox1.Enabled = false;
			}
			else {
				this.numericUpDown7.Enabled = true;
				this.numericUpDown8.Enabled = true;
#if true//2019.03.02(直線近似)
				this.numericUpDown9.Enabled = true;
				this.numericUpDown21.Enabled = true;
#endif
				this.checkBox1.Enabled = true;
			}
			if (this.radioButton3.Checked) {
				this.numericUpDown3.Enabled = true;
			}
			else {
				this.numericUpDown3.Enabled = false;
			}
#if true//2019.03.02(直線近似)
			if (this.checkBox13.Checked) {
				check_for_af2();
			}
#endif
		}

		private void checkBox2_Click(object sender, EventArgs e)
		{
			bool bl = (this.checkBox2.Checked == true);
#if true//2018.12.22(測定抜け対応)
			if (this.checkBox10.Checked) {
				bl = true;
			}
#endif
			//this.numericUpDown10.Enabled = bl;
			//this.numericUpDown11.Enabled = bl;
			this.numericUpDown17.Enabled = bl;
			this.numericUpDown18.Enabled = bl;
			this.numericUpDown19.Enabled = bl;
			this.numericUpDown15.Enabled = bl;
			this.numericUpDown16.Enabled = bl;
#if true//2019.03.02(直線近似)
			this.numericUpDown22.Enabled = bl;
#endif
#if true//2018.12.22(測定抜け対応)
			//this.checkBox10.Enabled = bl;
			this.numericUpDown12.Enabled = (this.checkBox2.Checked == false);
#endif
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
#if true//2018.12.22(測定抜け対応)
		private void numericUpDown10_ValueChanged(object sender, EventArgs e)
		{
			 this.numericUpDown13.Value = this.numericUpDown10.Value;
		}

		private void checkBox10_Click(object sender, EventArgs e)
		{
			checkBox2_Click(null, null);
		}
#endif
#if true//2019.03.02(直線近似)
		private void check_for_af2()
		{
			this.numericUpDown5.Enabled = !this.checkBox13.Checked;
			this.numericUpDown6.Enabled = !this.checkBox13.Checked;
			this.numericUpDown8.Enabled = !this.checkBox13.Checked;
			this.numericUpDown9.Enabled = !this.checkBox13.Checked;
			this.numericUpDown20.Enabled = !this.checkBox13.Checked;
			this.numericUpDown21.Enabled = !this.checkBox13.Checked;
		}
#endif
	}
}
