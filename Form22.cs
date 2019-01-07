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
	public partial class Form22 : Form
	{
		public G.SYSSET m_ss;

		public Form22()
		{
			InitializeComponent();
		}

		private void Form22_Load(object sender, EventArgs e)
		{
			m_ss = G.SS;
#if true//2019.01.05(キューティクル検出欠損修正)
			if (string.IsNullOrEmpty(m_ss.PLM_AUT_FOLD)) {
				string path;
				path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += @"\KOP";
				path += @"\" + Application.ProductName;
				m_ss.PLM_AUT_FOLD = path;
			}
#endif
			DDX(true);

#if true//2018.07.10
			checkBox2_CheckedChanged(null, null);
			checkBox6_CheckedChanged(null, null);
#endif
		}

		private void Form22_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK) {
				return;
			}
			if (DDX(false) == false) {
				e.Cancel = true;
			}
#if true//2019.01.05(キューティクル検出欠損修正)
			else if (!System.IO.Directory.Exists(m_ss.PLM_AUT_FOLD)) {
				G.mlog("指定されたフォルダは存在しません.\r\r" + m_ss.PLM_AUT_FOLD);
				e.Cancel = true;
			}
#endif
			else {
				G.SS = (G.SYSSET)m_ss.Clone();
			}
		}
		private bool DDX(bool bUpdate)
        {
            bool rc;
			try {
#if true//2019.01.05(キューティクル検出欠損修正)
				DDV.DDX(bUpdate, this.comboBox2       , ref m_ss.PLM_AUT_FLTP);
				DDV.DDX(bUpdate, this.textBox2        , ref m_ss.PLM_AUT_FOLD);
				DDV.DDX(bUpdate, this.numericUpDown2  , ref m_ss.PLM_AUT_OVLP);
#endif
				DDV.DDX(bUpdate, this.numericUpDown10 , ref m_ss.PLM_AUT_HP_X, G.SS.PLM_MLIM[0], G.SS.PLM_PLIM[0]);
				DDV.DDX(bUpdate, this.numericUpDown11 , ref m_ss.PLM_AUT_HP_Y, G.SS.PLM_MLIM[1], G.SS.PLM_PLIM[1]);
#if true//2018.07.02
				DDV.DDX(bUpdate, this.numericUpDown12 , ref m_ss.PLM_AUT_HP_Z, G.SS.PLM_MLIM[2], G.SS.PLM_PLIM[2]);
#endif
#if true//2018.07.10
				DDV.DDX(bUpdate, this.checkBox2      , ref m_ss.PLM_AUT_HPOS);
				DDV.DDX(bUpdate, this.checkBox6      , ref m_ss.PLM_AUT_ZDCK);//Ｚ測定:深度合成用
				DDV.DDX(bUpdate, this.textBox3       , ref m_ss.PLM_AUT_ZDEP, 50, -99, +99);
#if true//2018.11.13(毛髪中心AF)
				DDV.DDX(bUpdate, this.checkBox7      , ref m_ss.PLM_AUT_ZKCK);//Ｚ測定:毛髪径判定用
				DDV.DDX(bUpdate, this.textBox4       , ref m_ss.PLM_AUT_ZKEI, 50, -99, +99);
#endif
#if true//2018.07.30(終了位置指定)
				DDV.DDX(bUpdate, this.numericUpDown14 , ref m_ss.PLM_AUT_ED_Y, G.SS.PLM_MLIM[1], G.SS.PLM_PLIM[1]);
                if (bUpdate == false) {
					if (m_ss.PLM_AUT_ED_Y <= m_ss.PLM_AUT_HP_Y) {
						G.mlog("終了ステージ位置:yは開始位置:yより大きい値を指定してください.");
						this.numericUpDown14.Focus();
						return(false);
					}
                }
#endif
				if (bUpdate == false) {
#if true//2019.01.05(キューティクル検出欠損修正)
					if (this.textBox2.Text == "") {
						G.mlog("フォルダを指定してください.");
						this.textBox2.Focus();
						return(false);
					}
#endif
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
					else {
						if (m_ss.PLM_AUT_ZDCK) {
							G.mlog("Z座標を入力してください.");
							this.textBox3.Focus();
							return(false);
						}
					}
#if true//2018.11.13(毛髪中心AF)
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
							if (val == 0) {
								G.mlog("0が指定されています.");
								this.textBox4.Focus();
								return(false);
							}
						}
					}

#endif
				}
#endif
                rc = true;
            }
            catch (Exception e)
            {
                G.mlog(e.Message);
                rc = false;
            }
            return (rc);
		}
#if true//2018.07.10
		private void checkBox2_CheckedChanged(object sender, EventArgs e)
		{
			this.numericUpDown12.Enabled = (this.checkBox2.Checked == false);
		}

		private void checkBox6_CheckedChanged(object sender, EventArgs e)
		{
			textBox3.Enabled = (this.checkBox6.Checked == true);
		}
#endif
#if true//2018.07.30
        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            this.numericUpDown13.Value = this.numericUpDown10.Value;
        }
#endif
#if true//2019.01.05(キューティクル検出欠損修正)
		private void button3_Click(object sender, EventArgs e)
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
#endif
    }
}
