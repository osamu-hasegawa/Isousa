using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//---
using System.Collections;

namespace uSCOPE
{
	public partial class Form26 : Form
	{
		private List<string> m_zpos = new List<string>();
		private List<string> m_kpos = new List<string>();

		public Form26()
		{
			InitializeComponent();
		}

		private void Form26_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(G.SS.NGJ_CND_FOLD)) {
				string path;
				path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += @"\KOP";
				path += @"\" + Application.ProductName;
				G.SS.NGJ_CND_FOLD = path;
			}
			this.comboBox6.SelectedIndex = 0;
			this.comboBox6.Enabled = false;
			if (G.UIF_LEVL == 0) {
            }
            //---
            DDX(true);
			//---
			radioButton1_Click(null, null);
			//---
		}
		private void check_z10(string path)
		{
			//string[] zpos = null;
#if true//2018.11.13(毛髪中心AF)
			//m_zpos.Clear();
			//m_kpos.Clear();
#endif
			//
#if true//2018.08.13
			//zpos = new string[] {};
			//try {
#endif
#if true //2018.08.21
				//this.comboBox8.Items.Clear();
				//this.comboBox8.Enabled = false;
				//this.comboBox10.Items.Clear();
				//this.comboBox12.Items.Clear();
				//this.comboBox10.Enabled = false;
				//this.comboBox12.Enabled = false;
#endif
				//if (true) {
#if true//2018.10.10(毛髪径算出・改造)
					//for (int i = 0; i <= 23; i++) {
					//    string NS = i.ToString();
					//    zpos = System.IO.Directory.GetFiles(path, NS + "CR_00_ZP00D.*");
					//    if (zpos.Length > 0) {
					//        break;
					//    }
					//    zpos = System.IO.Directory.GetFiles(path, NS + "CT_00_ZP00D.*");
					//    if (zpos.Length > 0) {
					//        break;
					//    }
					//}
#endif
				//    if (zpos.Length <= 0) {
				//        //古い形式のファイルもしくはフォルダが空
				//        return;
				//    }
				//}
#if true//2018.08.13
			//}
			//catch (Exception ex) {
			//    G.mlog(ex.Message);
			//}
#endif
			if (true) {
#if true //2018.08.21
				//this.comboBox8.Enabled = true;
				//this.comboBox10.Enabled = true;
				//this.comboBox12.Enabled = true;
#endif
#if true//2018.11.13(毛髪中心AF)
				//for (int i = 0; i < zpos.Length; i++) {
				//    switch (zpos[i][0]) {
				//        case 'Z':
				//        case 'z':
				//            m_zpos.Add(zpos[i]);
				//            break;
				//        case 'K':
				//        case 'k':
				//            m_kpos.Add(zpos[i]);
				//            break;
				//    }
				//}
				//for (int i = 0; i < m_zpos.Count; i++) {
				//    this.comboBox10.Items.Add(m_zpos[i]);
				//    this.comboBox8.Items.Add(m_zpos[i]);
				//    this.comboBox12.Items.Add(m_zpos[i]);
				//}
				//for (int i = 0; i < m_kpos.Count; i++) {
				//    this.comboBox10.Items.Add(m_kpos[i]);
				//    this.comboBox8.Items.Add(m_kpos[i]);
				//    this.comboBox12.Items.Add(m_kpos[i]);
				//}
#endif
			}
		}
		private void Form26_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK) {
				return;
			}
			if (DDX(false) == false) {
                e.Cancel = true;
				return;
            }
			string path = this.textBox1.Text;
			if (G.SS.NGJ_CND_FMOD == 0) {
				path = G.SS.AUT_BEF_PATH;
			}
			else {
				path = this.textBox1.Text;
			}
			if (!System.IO.Directory.Exists(path)) {
				G.mlog("指定されたフォルダは存在しません.\r\r" + path);
				e.Cancel = true;
				return;
			}
			string[] files_ct, files_cr, files_ir;
			string zpos;
			if (true) {
				zpos = "_" + "ZP00D";

				if (true) {
#if true//2019.05.22(再測定判定(キューティクル枚数))
					files_ct = System.IO.Directory.GetFiles(path, "*CT_??" +zpos+ ".*");
					files_cr = System.IO.Directory.GetFiles(path, "*CR_??" +zpos+ ".*");
					files_ir = System.IO.Directory.GetFiles(path, "*IR_??" +zpos+ ".*");
#else
					files_ct = System.IO.Directory.GetFiles(path, "?CT_??" +zpos+ ".*");
					files_cr = System.IO.Directory.GetFiles(path, "?CR_??" +zpos+ ".*");
					files_ir = System.IO.Directory.GetFiles(path, "?IR_??" +zpos+ ".*");
#endif
				}

				if (true) {
					int ttl = files_ct.Length + files_cr.Length;
					if (ttl <= 0) {
						G.mlog("指定されたフォルダには毛髪画像ファイルがありません.\r\r" + path);
						e.Cancel = true;
						return;
					}
				}
				if (!System.IO.File.Exists(path + "\\log.csv")) {
					G.mlog("指定されたフォルダにはログファイル('log.csv')がありません.\r\r" + path + "\\log.csv");
					e.Cancel = true;
					return;
				}
			}
		}
		private bool DDX(bool bUpdate)
        {
            bool rc;

            try {
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton1, this.radioButton2}, ref G.SS.NGJ_CND_FMOD);
				DDV.DDX(bUpdate, this.textBox1       , ref G.SS.NGJ_CND_FOLD);
				//---
				//---
				//---
				if (bUpdate == false) {
					if (G.SS.NGJ_CND_FMOD == 1 && this.textBox1.Text == "") {
						G.mlog("フォルダを指定してください.");
						this.textBox1.Focus();
						return(false);
					}
					//G.SS.MOZ_CND_ZCNT = this.comboBox8.Items.Count;
				}
                rc = true;
            }
            catch (Exception e) {
                G.mlog(e.Message);
                rc = false;
            }
            return (rc);
		}

		private void OnClicks(object sender, EventArgs e)
		{
			if (sender == this.button3) {
				//FolderBrowserDialogクラスのインスタンスを作成
				OpenFileDialog dlg = new OpenFileDialog();
				string path = this.textBox1.Text;
				
				dlg.Title = "指定するフォルダの画像ファイルを選択してください.";
				dlg.Filter = G.filter_string();
				dlg.FilterIndex = 4;
				dlg.InitialDirectory = path;
				dlg.FileName = "*.*";
				//ダイアログを表示する
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					this.textBox1.Text = System.IO.Path.GetDirectoryName(dlg.FileName);
					check_z10(this.textBox1.Text);
				}
			}
		}

		private void radioButton1_Click(object sender, EventArgs e)
		{
			if (this.radioButton1.Checked) {
				this.textBox1.Enabled = this.button3.Enabled = false;
			}
			else {
				this.textBox1.Enabled = this.button3.Enabled = true;
			}
			check_z10(this.radioButton1.Checked ? G.SS.AUT_BEF_PATH: this.textBox1.Text);
		}

		private void checkBox9_CheckedChanged(object sender, EventArgs e)
		{
		}
	}
}
