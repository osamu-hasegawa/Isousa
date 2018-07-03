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
	public partial class Form21 : Form
	{
		private
		int GAP = 20;

		public Form21()
		{
			InitializeComponent();
		}

		private void Form21_Load(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(G.SS.MOZ_CND_FOLD)) {
				string path;
				path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				path += @"\KOP";
				path += @"\" + Application.ProductName;
				G.SS.MOZ_CND_FOLD = path;
			}
			//G.SS.MOZ_CND_FMOD = 1;
			//G.SS.ETC_NOZ_FOLD = "C:\\temp\\test_20171102_225530";
			//G.SS.ETC_NOZ_FOLD = "C:\\temp\\test_20171108_024440";
			this.groupBox6.Enabled = false;
			if (G.UIF_LEVL == 0) {
				this.groupBox1.Visible = false;
				this.groupBox3.Visible = false;
				this.groupBox4.Visible = false;
				this.comboBox6.Enabled = false;
				//this.comboBox7.Enabled = false;
				this.comboBox8.Enabled = false;
				this.checkBox8.Enabled = false;
				this.button4.Enabled = false;
				this.groupBox2.Height = this.comboBox6.Location.Y;
				this.button1.Top = this.groupBox2.Bottom + GAP/2;
				this.button2.Top = this.groupBox2.Bottom + GAP/2;
				this.Height = this.button1.Bottom + GAP/2 + (this.Size.Height-this.ClientSize.Height);
				this.Text = "画像表示";
			}
			//---
			DDX(true);
			//---
			radioButton1_Click(null, null);
			//check_z10(G.SS.MOZ_CND_FMOD == 0 ? G.SS.AUT_BEF_PATH: this.textBox1.Text);
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
				string[] files_pl, files_mi;
				this.comboBox8.Items.Clear();
				this.comboBox8.Enabled = true;
				if (true) {
					for (int i = 0; i < zpos.Length; i++) {
						this.comboBox8.Items.Add(zpos[i]);
					}
				}
				else {
					this.comboBox8.Items.Add("ZP00D");
					for (int i = 1; i < 10; i++) {
						files_mi = System.IO.Directory.GetFiles(path, string.Format("*_Z{0:00}.*", 10-i));
						files_pl = System.IO.Directory.GetFiles(path, string.Format("*_Z{0:00}.*", 10+i));
						files_mi = cut_IZ(files_mi);
						files_pl = cut_IZ(files_pl);
						if (files_pl.Length != files_10.Length || files_mi.Length != files_10.Length) {
							break;
						}
						this.comboBox8.Items.Insert(0, string.Format("Z{0:00}", 10-i));
						this.comboBox8.Items.Add(string.Format("Z{0:00}", 10+i));
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
		private void Form21_FormClosing(object sender, FormClosingEventArgs e)
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
	#if true
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
	#else
			//画像表示のみの場合
			if (G.SS.MOZ_CND_NOMZ) {
				int ttl = 0;
				ttl += files_cl.Length + files_ct.Length;
				ttl += files_cr.Length + files_ir.Length;
				if (ttl <= 0) {
					G.mlog("指定されたフォルダには毛髪画像ファイルがありません.\r\r" + path);
					e.Cancel = true;
				}
				return;
			}
			//透過画像
			if (/*位置検出*/G.SS.MOZ_CND_PDFL == 0 || /*カラー断面*/G.SS.MOZ_CND_DMFL == 0) {
				if (files_ct.Length > 0 || (G.SS.ETC_CLF_CTCR == 0 && files_cl.Length > 0)) {
					// ok
				}
				else {
					G.mlog("指定されたフォルダには透過画像ファイル('CT')がありません.\r\r" + path);
					e.Cancel = true;
					return;
				}
			}
			
			//反射画像
			if (/*位置検出*/G.SS.MOZ_CND_PDFL == 1 || /*カラー断面*/G.SS.MOZ_CND_DMFL == 1) {
				if (files_cr.Length > 0 || (G.SS.ETC_CLF_CTCR == 1 && files_cl.Length > 0)) {
					// ok
				}
				else {
					G.mlog("指定されたフォルダには反射画像ファイル('CR')がありません.\r\r" + path);
					e.Cancel = true;
					return;
				}
			}
			//赤外画像
			if (G.SS.MOZ_CND_PDFL == 2) {
				if (files_ir.Length <= 0) {
					G.mlog("指定されたフォルダには赤外画像ファイル('IR')がありません.\r\r" + path);
					e.Cancel = true;
					return;
				}
			}
	#endif
		}
		private bool DDX(bool bUpdate)
        {
            bool rc;

            try {
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton1, this.radioButton2}, ref G.SS.MOZ_CND_FMOD);
				DDV.DDX(bUpdate, this.textBox1       , ref G.SS.MOZ_CND_FOLD);
				DDV.DDX(bUpdate, this.numericUpDown1 , ref G.SS.MOZ_CND_CUTE);
				DDV.DDX(bUpdate, this.numericUpDown2 , ref G.SS.MOZ_CND_ZVAL);
				DDV.DDX(bUpdate, this.numericUpDown3 , ref G.SS.MOZ_CND_DSUM);
				//---
				DDV.DDX(bUpdate, this.comboBox1      , ref G.SS.MOZ_CND_FTCF);
				DDV.DDX(bUpdate, this.comboBox2      , ref G.SS.MOZ_CND_FTCT);
				DDV.DDX(bUpdate, this.comboBox3      , ref G.SS.MOZ_CND_SMCF);
				DDV.DDX(bUpdate, this.checkBox1      , ref G.SS.MOZ_CND_CTRA);
				DDV.DDX(bUpdate, this.numericUpDown4 , ref G.SS.MOZ_CND_HANI);
				//---
				DDV.DDX(bUpdate, this.comboBox6      , ref G.SS.MOZ_CND_PDFL);
//				DDV.DDX(bUpdate, this.comboBox7      , ref G.SS.MOZ_CND_DMFL);
				DDV.DDX(bUpdate, this.comboBox8      , ref G.SS.MOZ_CND_ZPOS);
				DDV.DDX(bUpdate, this.checkBox8      , ref G.SS.MOZ_CND_NOMZ);
				//---
				DDV.DDX(bUpdate, this.comboBox5      , ref  G.SS.IMP_FLT_COEF[3]);
				G.SS.IMP_BIN_MODE[3] = 1;//GRAY-SCALE
				DDV.DDX(bUpdate, this.numericUpDown30, ref G.SS.IMP_BIN_BVAL[3]);
				DDV.DDX(bUpdate, this.numericUpDown31, ref G.SS.IMP_SUM_UPPR[3]);
				DDV.DDX(bUpdate, this.numericUpDown32, ref G.SS.IMP_SUM_LOWR[3]);
				DDV.DDX(bUpdate, this.numericUpDown33, ref G.SS.IMP_LEN_UPPR[3]);
				DDV.DDX(bUpdate, this.numericUpDown34, ref G.SS.IMP_LEN_LOWR[3]);
				DDV.DDX(bUpdate, this.numericUpDown35, ref G.SS.IMP_CIR_UPPR[3]);
				DDV.DDX(bUpdate, this.numericUpDown36, ref G.SS.IMP_CIR_LOWR[3]);
				DDV.DDX(bUpdate, this.numericUpDown37, ref G.SS.IMP_CUV_UPPR[3]);
				DDV.DDX(bUpdate, this.numericUpDown38, ref G.SS.IMP_CUV_LOWR[3]);
				DDV.DDX(bUpdate, this.numericUpDown39, ref G.SS.IMP_GIZ_UPPR[3]);
				DDV.DDX(bUpdate, this.numericUpDown40, ref G.SS.IMP_GIZ_LOWR[3]);
				DDV.DDX(bUpdate, this.numericUpDown19, ref G.SS.IMP_OPT_MAGN[3]);
				//---
				DDV.DDX(bUpdate, this.checkBox7      , ref G.SS.MOZ_IRC_NOMZ);
				DDV.DDX(bUpdate, this.checkBox2      , ref G.SS.MOZ_IRC_SAVE);
				DDV.DDX(bUpdate, this.checkBox3      , ref G.SS.MOZ_IRC_CK00);
				DDV.DDX(bUpdate, this.checkBox4      , ref G.SS.MOZ_IRC_CK01);
				DDV.DDX(bUpdate, this.checkBox5      , ref G.SS.MOZ_IRC_CK02);
				DDV.DDX(bUpdate, this.checkBox6      , ref G.SS.MOZ_IRC_CK03);
				DDV.DDX(bUpdate, this.comboBox4      , ref G.SS.MOZ_IRC_DISP);
				//---
				DDV.DDX(bUpdate, this.checkBox9      , ref G.SS.MOZ_FST_CK00);
#if true//2018.07.02
				DDV.DDX(bUpdate, this.checkBox10     , ref G.SS.MOZ_FST_CK01);
#endif
				DDV.DDX(bUpdate, this.numericUpDown5 , ref G.SS.MOZ_FST_RCNT);
				DDV.DDX(bUpdate, this.numericUpDown6 , ref G.SS.MOZ_FST_CCNT);
				DDV.DDX(bUpdate, this.comboBox7      , ref G.SS.MOZ_FST_MODE);
				DDV.DDX(bUpdate, this.comboBox9      , ref G.SS.MOZ_FST_FCOF);
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
			else if (sender == this.button4) {
				// [>>]
				if (this.button4.Text == ">>") {
					this.Width = this.groupBox6.Location.X + this.groupBox6.Width+GAP;
					this.groupBox6.Enabled = true;
					if ((this.Location.X + this.Width) > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width) {
						this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - this.Width - GAP;
					}
					this.button4.Text = "<<";
					comboBox6_SelectedIndexChanged(null, null);
				}
				else {
					// [<<]
					this.Width = this.groupBox1.Location.X + this.groupBox1.Width+GAP;
					this.groupBox6.Enabled = false;
					this.button4.Enabled = true;
					this.button4.Text = ">>";
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

		private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.comboBox6.SelectedIndex == 2 && this.button4.Text == "<<") {
				this.groupBox6.Enabled = true;
			}
			else {
				this.groupBox6.Enabled = false;
			}
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
