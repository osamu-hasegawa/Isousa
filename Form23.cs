﻿using System;
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
			if (G.UIF_LEVL == 0) {
            }
            //---
            DDX(true);
			//---
			radioButton1_Click(null, null);
			//---
		}
		private string[] cut_IZ(string[] files)
		{
			ArrayList ar = new ArrayList();

			foreach (string path in files) {
				string name;
				name = System.IO.Path.GetFileName(path);
				if (name.Contains("IZ")) {
					continue;
				}
				ar.Add(path);
			}
			return((string[])ar.ToArray(typeof(string)));
		}
		private void check_z10(string path)
		{
			string[] files_10;
			string[] zpos = null;
			//
			if (true) {
				zpos = System.IO.Directory.GetFiles(path, "0CR_00_*.*");
				if (zpos.Length <= 0) {
				zpos = System.IO.Directory.GetFiles(path, "0CT_00_*.*");
				}
				if (zpos.Length <= 0) {
					//古い形式のファイルもしくはフォルダが空
					this.comboBox8.Items.Clear();
					this.comboBox8.Enabled = false;
					return;
				}
				for (int i = 0; i < zpos.Length; i++) {
					string tmp = System.IO.Path.GetFileNameWithoutExtension(zpos[i]);
					zpos[i] = tmp.Substring(7);
				}
			}
			//files_10 = System.IO.Directory.GetFiles(path, "*_Z10.*");
			files_10 = System.IO.Directory.GetFiles(path, "*_ZP00D.*");
			files_10 = cut_IZ(files_10);

			if (files_10.Length <= 0) {
				//古い形式のファイルもしくはフォルダが空
				this.comboBox8.Items.Clear();
				this.comboBox8.Enabled = false;
				return;
			}
			if (true) {
				this.comboBox8.Items.Clear();
				this.comboBox8.Enabled = true;
				if (true) {
					for (int i = 0; i < zpos.Length; i++) {
						this.comboBox8.Items.Add(zpos[i]);
					}
				}

				if (G.SS.MOZ_FST_CK00) {
					this.comboBox8.Items.Insert(0, "深度合成");
				}
				this.comboBox8.SelectedIndex = this.comboBox8.FindString(G.SS.MOZ_CND_ZPOS);

				int idx = this.comboBox8.FindString(G.SS.MOZ_CND_ZPOS);
				if (idx < 0) {
					this.comboBox8.SelectedIndex = this.comboBox8.FindString("ZP00D");
				}
				else {
					this.comboBox8.SelectedIndex = idx;
				}
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

			if (!System.IO.Directory.Exists(path)) {
				G.mlog("指定されたフォルダは存在しません.\r\r" + path);
				e.Cancel = true;
				return;
			}
			string[] files_ct, files_cr, files_ir, files_10;
			string zpos;

			if (string.Compare(G.SS.MOZ_CND_ZPOS, "深度合成") == 0) {
				zpos = "ZP00D";
			}
			else {
				zpos = G.SS.MOZ_CND_ZPOS;
			}

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
		}
		private bool DDX(bool bUpdate)
        {
            bool rc;

            try {
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton1, this.radioButton2}, ref G.SS.MOZ_CND_FMOD);
				DDV.DDX(bUpdate, this.textBox1       , ref G.SS.MOZ_CND_FOLD);
				//---
				DDV.DDX(bUpdate, this.numericUpDown4 , ref G.SS.MOZ_CND_HANI);
				//---
				DDV.DDX(bUpdate, this.comboBox8      , ref G.SS.MOZ_CND_ZPOS);
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
		}
	}
}
