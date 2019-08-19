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
	public partial class Form25 : Form
	{
#if true//2019.07.27(保存形式変更)
		public string m_fullpath;
#else
		public string h_no;
		public string[] i_no;
		public int i_sel;
#endif
#if true//2019.08.18(不具合修正)
		public string MOZ_CND_FOLD;
#endif
		public Form25()
		{
			InitializeComponent();
		}

		private void Form25_Load(object sender, EventArgs e)
		{
#if false//2019.08.18(不具合修正)
			if (string.IsNullOrEmpty(G.SS.MOZ_CND_FOLD)) {
				string path;
				path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += @"\KOP";
				path += @"\" + Application.ProductName;
				G.SS.MOZ_CND_FOLD = path;
			}
#endif
            //---
            DDX(true);
			//---
			radioButton1_Click(null, null);
			//---
		}
		private void Form25_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK) {
				return;
			}
			if (DDX(false) == false) {
                e.Cancel = true;
				return;
            }
			string fold, name;
			int i_s, i_e;
			if (G.SS.MOZ_SAV_DMOD == 0/*毛髪画像と同一のフォルダ*/) {
#if true//2019.08.18(不具合修正)
				fold = this.MOZ_CND_FOLD;
#else
				fold = G.SS.MOZ_CND_FOLD;
#endif
			}
			else {
				fold = this.textBox1.Text;
			}
			name = this.textBox2.Text;

			if (!System.IO.Directory.Exists(fold)) {
				G.mlog("指定されたフォルダは存在しません.\r\r" + fold);
				e.Cancel = true;
				return;
			}
			if (fold[fold.Length-1] != '\\') {
				fold += "\\";
			}
#if false//2019.07.27(保存形式変更)
			if (G.SS.MOZ_SAV_FMOD == 0) {
				i_s = 0;
				i_e = this.i_no.Length-1;
			}
			else {
				i_s = i_e = i_sel;
			}
#endif
#if true//2019.01.09(保存機能修正)
			if (true) {
				string path;

				path = fold;
				path += name;
#if false//2019.07.27(保存形式変更)
				path += "_";
				path += this.h_no;
				if (G.SS.MOZ_SAV_FMOD != 0) {
				path += "_";
				path += i_no[i_sel];
				}
#endif
				path += ".csv";
				if (System.IO.File.Exists(path)) {
					if (G.mlog(string.Format("#q{0}は既に存在します。\r上書きしますか?", path)) != System.Windows.Forms.DialogResult.Yes) {
						e.Cancel = true;
						return;
					}
				}
#if true//2019.07.27(保存形式変更)
				m_fullpath = path;
#endif
			}
#else
			for (int i = i_s; i <= i_e; i++) {
				string path;

				path = fold;
				path += name;
				path += "_";
				path += this.h_no;
				path += "_";
				path += this.i_no[i];
				path += ".csv";
				if (System.IO.File.Exists(path)) {
					if (G.mlog(string.Format("#q{0}は既に存在します。\r上書きしますか?", path)) != System.Windows.Forms.DialogResult.Yes) {
						e.Cancel = true;
						return;
					}
					break;
				}
			}
#endif
		}
		private bool DDX(bool bUpdate)
        {
            bool rc;

            try {
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton1, this.radioButton2}, ref G.SS.MOZ_SAV_DMOD);
				DDV.DDX(bUpdate, this.textBox1       , ref G.SS.MOZ_SAV_FOLD);
#if false//2019.07.27(保存形式変更)
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton3, this.radioButton4}, ref G.SS.MOZ_SAV_FMOD);
#endif
				DDV.DDX(bUpdate, this.textBox2       , ref G.SS.MOZ_SAV_NAME);
				//---
				if (bUpdate == false) {
					if (G.SS.MOZ_SAV_DMOD == 1 && this.textBox1.Text == "") {
						G.mlog("フォルダを指定してください.");
						this.textBox1.Focus();
						return(false);
					}
					if (this.textBox2.Text == "") {
						G.mlog("ファイル名を指定してください.");
						this.textBox2.Focus();
						return(false);
					}
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
				FolderBrowserDialog dlg = new FolderBrowserDialog();
				string path = this.textBox1.Text;
				dlg.RootFolder = Environment.SpecialFolder.Desktop;
				dlg.SelectedPath = path;
				dlg.ShowNewFolderButton = true;
				//ダイアログを表示する
				if (dlg.ShowDialog(this) == DialogResult.OK) {
					this.textBox1.Text = dlg.SelectedPath;
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
		}
	}
}
