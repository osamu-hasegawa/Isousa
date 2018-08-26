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
	public partial class Form23 : Form
	{
		private
		int GAP = 20;

		public Form23()
		{
			InitializeComponent();
		}

		private void Form23_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(G.SS.MOZ_CND_FOLD)) {
				string path;
				path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += @"\KOP";
				path += @"\" + Application.ProductName;
				G.SS.MOZ_CND_FOLD = path;
			}
#if false//2018.08.21
			this.groupBox6.Enabled = false;
#else
			this.comboBox6.SelectedIndex = 0;
			this.comboBox6.Enabled = false;
#endif
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
			string[] files_10;
			string[] zpos = null;
			//
#if true//2018.08.13
			files_10 = new string[] {};
			zpos = new string[] {};
			try {
#endif
#if true //2018.08.21
				this.comboBox8.Items.Clear();
				this.comboBox8.Enabled = false;
				this.comboBox10.Items.Clear();
				this.comboBox12.Items.Clear();
				this.comboBox10.Enabled = false;
				this.comboBox12.Enabled = false;
#endif
				if (true) {
					zpos = System.IO.Directory.GetFiles(path, "0CR_00_*.*");
					if (zpos.Length <= 0) {
					zpos = System.IO.Directory.GetFiles(path, "0CT_00_*.*");
					}
					if (zpos.Length <= 0) {
						//古い形式のファイルもしくはフォルダが空
#if false//2018.08.21
						this.comboBox8.Items.Clear();
						this.comboBox8.Enabled = false;
#endif
						return;
					}
					for (int i = 0; i < zpos.Length; i++) {
						string tmp = System.IO.Path.GetFileNameWithoutExtension(zpos[i]);
						zpos[i] = tmp.Substring(7);
					}
				}
				//files_10 = System.IO.Directory.GetFiles(path, "*_Z10.*");
				files_10 = System.IO.Directory.GetFiles(path, "*_ZP00D.*");
				files_10 = Form21.cut_IZ(files_10);

				if (files_10.Length <= 0) {
					//古い形式のファイルもしくはフォルダが空
#if false//2018.08.21
					this.comboBox8.Items.Clear();
					this.comboBox8.Enabled = false;
#endif
					return;
				}
#if true//2018.08.13
			}
			catch (Exception ex) {
				G.mlog(ex.Message);
			}
#endif
			if (true) {
#if false//2018.08.21
				this.comboBox8.Items.Clear();
				this.comboBox8.Enabled = true;
#endif
#if true //2018.08.21
				this.comboBox8.Enabled = true;
				this.comboBox10.Enabled = true;
				this.comboBox12.Enabled = true;
#endif
				if (true) {
					for (int i = 0; i < zpos.Length; i++) {
						this.comboBox8.Items.Add(zpos[i]);
#if true //2018.08.21
						this.comboBox10.Items.Add(zpos[i]);
						this.comboBox12.Items.Add(zpos[i]);
#endif
					}
				}

				if (G.SS.MOZ_FST_CK00) {
					this.comboBox8.Items.Insert(0, "深度合成");
#if true //2018.08.21
					this.comboBox10.Items.Insert(0, "深度合成");
					this.comboBox12.Items.Insert(0, "深度合成");
#endif
				}
#if false//2018.08.21
				this.comboBox8.SelectedIndex = this.comboBox8.FindString(G.SS.MOZ_CND_ZPOS);

				int idx = this.comboBox8.FindString(G.SS.MOZ_CND_ZPOS);
				if (idx < 0) {
					this.comboBox8.SelectedIndex = this.comboBox8.FindString("ZP00D");
				}
				else {
					this.comboBox8.SelectedIndex = idx;
				}
#else
				Form21.select_default(this.comboBox10, G.SS.MOZ_CND_ZPCT);
				Form21.select_default(this.comboBox8, G.SS.MOZ_CND_ZPHL);
				Form21.select_default(this.comboBox12, G.SS.MOZ_CND_ZPML);
#endif
			}
		}
		private void Form23_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK) {
				return;
			}
			if (DDX(false) == false) {
                e.Cancel = true;
				return;
            }
			string path = this.textBox1.Text;
			if (G.SS.MOZ_CND_FMOD == 0) {
				path = G.SS.AUT_BEF_PATH;
			}
			else {
				path = this.textBox1.Text;
			}
#if false//2018.08.13
			G.SS.MOZ_IRC_SAVE = false;//常にOFF(IZファイルを保存するとエラー発生のため)
#endif
			if (!System.IO.Directory.Exists(path)) {
				G.mlog("指定されたフォルダは存在しません.\r\r" + path);
				e.Cancel = true;
				return;
			}
			string[] files_ct, files_cr, files_ir, files_10;
			string zpos;
#if true//2018.08.21
			for (int i = 0; i < 3; i++) {
				switch (i) {
					case  0: zpos = G.SS.MOZ_CND_ZPCT; break;//CL
					case  1: zpos = G.SS.MOZ_CND_ZPHL; break;//CL
					default: zpos = G.SS.MOZ_CND_ZPML; break;//IR
				}
				if (zpos == "深度合成") {
					zpos = "ZP00D";
				}
#else
				if (string.Compare(G.SS.MOZ_CND_ZPOS, "深度合成") == 0) {
					zpos = "ZP00D";
				}
				else {
					zpos = G.SS.MOZ_CND_ZPOS;
				}
#endif
				if (string.IsNullOrEmpty(zpos)) {
					zpos = "";
				}
				else {
					zpos = "_" + zpos;
				}

				files_10 = System.IO.Directory.GetFiles(path, "*_Z10.*");

				if (true) {
					files_ct = System.IO.Directory.GetFiles(path, "?CT_??" +zpos+ ".*");
					files_cr = System.IO.Directory.GetFiles(path, "?CR_??" +zpos+ ".*");
					files_ir = System.IO.Directory.GetFiles(path, "?IR_??" +zpos+ ".*");
				}

				if (true) {
					int ttl = files_ct.Length + files_cr.Length;
					if (ttl <= 0) {
						G.mlog("指定されたフォルダには毛髪画像ファイルがありません.\r\r" + path);
						e.Cancel = true;
						return;
					}
				}
				//画像表示のみの場合
				if (G.SS.MOZ_CND_NOMZ) {
					return;
				}
				//赤外画像
				if (G.SS.MOZ_CND_PDFL == 1) {
					if (files_ir.Length <= 0) {
						G.mlog("指定されたフォルダには赤外画像ファイル('IR')がありません.\r\r" + path);
						e.Cancel = true;
						return;
					}
				}
#if true//2018.08.21
			}
#endif
		}
		private bool DDX(bool bUpdate)
        {
            bool rc;

            try {
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton1, this.radioButton2}, ref G.SS.MOZ_CND_FMOD);
				DDV.DDX(bUpdate, this.textBox1       , ref G.SS.MOZ_CND_FOLD);
				//---
#if true//2018.08.13
				DDV.DDX(bUpdate, this.numericUpDown2 , ref G.SS.MOZ_CND_ZVAL);
#endif
				DDV.DDX(bUpdate, this.numericUpDown4 , ref G.SS.MOZ_CND_HANI);
				//---
#if true//2018.08.21
				G.SS.MOZ_CND_PDFL = 0;//カラー固定
				DDV.DDX(bUpdate, this.comboBox10     , ref G.SS.MOZ_CND_ZPCT);
				DDV.DDX(bUpdate, this.comboBox8      , ref G.SS.MOZ_CND_ZPHL);
				DDV.DDX(bUpdate, this.comboBox12     , ref G.SS.MOZ_CND_ZPML);
#else
				DDV.DDX(bUpdate, this.comboBox8      , ref G.SS.MOZ_CND_ZPOS);
#endif
				DDV.DDX(bUpdate, this.checkBox8      , ref G.SS.MOZ_CND_NOMZ);
				//---
				//---
				DDV.DDX(bUpdate, this.checkBox9      , ref G.SS.MOZ_FST_CK00);
				DDV.DDX(bUpdate, this.checkBox10     , ref G.SS.MOZ_FST_CK01);
				DDV.DDX(bUpdate, this.numericUpDown5 , ref G.SS.MOZ_FST_RCNT);
				DDV.DDX(bUpdate, this.numericUpDown6 , ref G.SS.MOZ_FST_CCNT);
				//---
				if (bUpdate == false) {
					if (G.SS.MOZ_CND_FMOD == 1 && this.textBox1.Text == "") {
						G.mlog("フォルダを指定してください.");
						this.textBox1.Focus();
						return(false);
					}
					G.SS.MOZ_CND_ZCNT = this.comboBox8.Items.Count;
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
#if true//2018.08.21
			Form21.check_fst(this.comboBox10, this.checkBox9.Checked);
			Form21.check_fst(this.comboBox8 , this.checkBox9.Checked);
			Form21.check_fst(this.comboBox12, this.checkBox9.Checked);
#else

			if (this.checkBox9.Checked) {
				if (this.comboBox8.FindString("深度合成") < 0) {
					this.comboBox8.Items.Insert(0, "深度合成");
					this.comboBox8.SelectedIndex = 0;
				}
			}
			else {
				if (this.comboBox8.FindString("深度合成") >= 0) {
					this.comboBox8.Items.Remove("深度合成");
					if (this.comboBox8.SelectedIndex < 0) {
						this.comboBox8.SelectedIndex = this.comboBox8.FindString("ZP00D");
					}
				}
			}
#endif
		}
	}
}
