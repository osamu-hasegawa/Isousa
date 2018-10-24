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
#if false//2018.08.21
			this.groupBox6.Enabled = false;
#else
			this.button4.Visible = false;
			this.comboBox6.SelectedIndex = 0;
			this.comboBox6.Enabled = false;
#endif
			if (G.UIF_LEVL == 0) {
#if false
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
#else
                this.groupBox1.Visible = false;
                this.groupBox3.Visible = false;
                this.label32.Visible = false;
                this.label33.Visible = false;
                this.comboBox7.Visible = false;
                this.comboBox9.Visible = false;
#if false//2018.08.21
                this.label26.Visible = false;
#endif
                this.comboBox6.Visible = false;
                this.button4.Visible = false;

                this.label46.Visible = false;
                this.label3.Visible = false;
                this.comboBox1.Visible = false;
                this.comboBox2.Visible = false;
                this.comboBox3.Visible = false;
#if false//2018.10.10(毛髪径算出・改造)
                this.checkBox1.Visible = false;
#endif
#endif
            }
            //---
            DDX(true);
			//---
			radioButton1_Click(null, null);
			//---
		}
		static public string[] cut_IZ(string[] files)
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
		static public void select_default(ComboBox cmb, string zpos)
		{
			int idx = cmb.FindString(zpos);
			if (idx < 0) {
				cmb.SelectedIndex = cmb.FindString("ZP00D");
			}
			else {
				cmb.SelectedIndex = idx;
			}
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
#if true//2018.10.10(毛髪径算出・改造)
					if (zpos.Length <= 0) {
						for (int i = 1; i <= 23; i++) {
							string NS = i.ToString();
							zpos = System.IO.Directory.GetFiles(path, NS + "CR_00_*.*");
							if (zpos.Length > 0) {
								break;
							}
							zpos = System.IO.Directory.GetFiles(path, NS + "CT_00_*.*");
							if (zpos.Length > 0) {
								break;
							}
						}
					}
#endif
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
				files_10 = cut_IZ(files_10);

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
				string[] files_pl, files_mi;
				this.comboBox8.Items.Clear();
#endif
				this.comboBox8.Enabled = true;
#if true //2018.08.21
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
				select_default(this.comboBox10, G.SS.MOZ_CND_ZPCT);
				select_default(this.comboBox8 , G.SS.MOZ_CND_ZPHL);
				select_default(this.comboBox12, G.SS.MOZ_CND_ZPML);
#endif
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
#if false//2018.08.21
				files_10 = System.IO.Directory.GetFiles(path, "*_Z10.*");
#endif
				if (true) {
					files_ct = System.IO.Directory.GetFiles(path, "?CT_??" + zpos + ".*");
					files_cr = System.IO.Directory.GetFiles(path, "?CR_??" + zpos + ".*");
					files_ir = System.IO.Directory.GetFiles(path, "?IR_??" + zpos + ".*");
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
#endif
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
				DDV.DDX(bUpdate, this.numericUpDown1 , ref G.SS.MOZ_CND_CUTE);
				DDV.DDX(bUpdate, this.numericUpDown2 , ref G.SS.MOZ_CND_ZVAL);
				DDV.DDX(bUpdate, this.numericUpDown3 , ref G.SS.MOZ_CND_DSUM);
				//---
				DDV.DDX(bUpdate, this.comboBox1      , ref G.SS.MOZ_CND_FTCF);
				DDV.DDX(bUpdate, this.comboBox2      , ref G.SS.MOZ_CND_FTCT);
				DDV.DDX(bUpdate, this.comboBox3      , ref G.SS.MOZ_CND_SMCF);
#if true//2018.10.10(毛髪径算出・改造)
				DDV.DDX(bUpdate, this.comboBox5      , ref G.SS.MOZ_CND_CNTR);
#else
				DDV.DDX(bUpdate, this.checkBox1      , ref G.SS.MOZ_CND_CTRA);
#endif
				DDV.DDX(bUpdate, this.numericUpDown4 , ref G.SS.MOZ_CND_HANI);
				//---
#if true//2018.08.21
				G.SS.MOZ_CND_PDFL = 0;//カラー固定
				DDV.DDX(bUpdate, this.comboBox10     , ref G.SS.MOZ_CND_ZPCT);
				DDV.DDX(bUpdate, this.comboBox8      , ref G.SS.MOZ_CND_ZPHL);
				DDV.DDX(bUpdate, this.comboBox12     , ref G.SS.MOZ_CND_ZPML);
#else
				DDV.DDX(bUpdate, this.comboBox6      , ref G.SS.MOZ_CND_PDFL);
//				DDV.DDX(bUpdate, this.comboBox7      , ref G.SS.MOZ_CND_DMFL);
				DDV.DDX(bUpdate, this.comboBox8      , ref G.SS.MOZ_CND_ZPOS);
#endif
				DDV.DDX(bUpdate, this.checkBox8, ref G.SS.MOZ_CND_NOMZ);
				//---
#if false//2018.08.21
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
#endif
				//---
				DDV.DDX(bUpdate, this.checkBox9      , ref G.SS.MOZ_FST_CK00);	//深度合成を行う
#if true//2018.07.02
				DDV.DDX(bUpdate, this.checkBox10, ref G.SS.MOZ_FST_CK01);		//合成済時スキップ
#endif
				DDV.DDX(bUpdate, this.numericUpDown5 , ref G.SS.MOZ_FST_RCNT);
				DDV.DDX(bUpdate, this.numericUpDown6 , ref G.SS.MOZ_FST_CCNT);
				DDV.DDX(bUpdate, this.comboBox7      , ref G.SS.MOZ_FST_MODE);
				DDV.DDX(bUpdate, this.comboBox9      , ref G.SS.MOZ_FST_FCOF);
#if true//2018.09.29(キューティクルライン検出)
				DDV.DDX(bUpdate, new RadioButton[] { this.radioButton3, this.radioButton4}, ref G.SS.MOZ_CND_CTYP);
				DDV.DDX(bUpdate, this.numericUpDown7 , ref G.SS.MOZ_CND_BPF1);
				DDV.DDX(bUpdate, this.numericUpDown8 , ref G.SS.MOZ_CND_BPF2);
				DDV.DDX(bUpdate, this.comboBox4      , ref G.SS.MOZ_CND_BPSL);
				DDV.DDX(bUpdate, this.numericUpDown13, ref G.SS.MOZ_CND_NTAP);
				DDV.DDX(bUpdate, this.numericUpDown9 , ref G.SS.MOZ_CND_BPVL);
				DDV.C2V(bUpdate, this.comboBox13     , ref G.SS.MOZ_CND_2DC0);
				DDV.C2V(bUpdate, this.comboBox14     , ref G.SS.MOZ_CND_2DC1);
				DDV.C2V(bUpdate, this.comboBox15     , ref G.SS.MOZ_CND_2DC2);
				DDV.DDX(bUpdate, this.numericUpDown10, ref G.SS.MOZ_CND_2DVL);
				//---
				DDV.DDX(bUpdate, this.numericUpDown11, ref G.SS.MOZ_CND_HMAX);
				DDV.DDX(bUpdate, this.numericUpDown12, ref G.SS.MOZ_CND_HWID);
#endif
#if true//2018.10.10(毛髪径算出・改造)
				DDV.DDX(bUpdate, this.numericUpDown18, ref G.SS.MOZ_CND_SLVL);//面積Sl,Sd判定閾値
				DDV.DDX(bUpdate, this.numericUpDown14, ref G.SS.MOZ_CND_OTW1);//外れ値判定:幅  (毛髄長さ)
				DDV.DDX(bUpdate, this.numericUpDown15, ref G.SS.MOZ_CND_OTV1);//外れ値判定:閾値(毛髄長さ)
				DDV.DDX(bUpdate, this.numericUpDown16, ref G.SS.MOZ_CND_OTW2);//外れ値判定:幅  (毛髄中心)
				DDV.DDX(bUpdate, this.numericUpDown17, ref G.SS.MOZ_CND_OTV2);//外れ値判定:閾値(毛髄中心)
				DDV.DDX(bUpdate, this.comboBox11     , ref G.SS.MOZ_CND_OTMD);//外れ値判定:補間,1:直線補間
				DDV.DDX(bUpdate, this.numericUpDown19, ref G.SS.MOZ_CND_SMVL);//除外判定:面積値
				DDV.DDX(bUpdate, this.checkBox4      , ref G.SS.MOZ_CND_CHK1);//有,無効:除外判定:毛髄面積
				DDV.DDX(bUpdate, this.checkBox5      , ref G.SS.MOZ_CND_CHK2);//有,無効:外れ値判定:毛髄長さ
				DDV.DDX(bUpdate, this.checkBox6      , ref G.SS.MOZ_CND_CHK3);//有,無効:外れ値判定:毛髄中心
#endif
				//---
				if (bUpdate == false) {
					if (G.SS.MOZ_CND_FMOD == 1 && this.textBox1.Text == "") {
						G.mlog("フォルダを指定してください.");
						this.textBox1.Focus();
						return(false);
					}
					G.SS.MOZ_CND_ZCNT = this.comboBox8.Items.Count;
				}
#if true//2018.09.29(キューティクルライン検出)
				if ((G.SS.MOZ_CND_NTAP % 2) == 0) {
					G.mlog("タップ数は奇数を指定してください.");
					this.numericUpDown13.Focus();
					return(false);
				}
				if (G.SS.MOZ_CND_BPF1 >= G.SS.MOZ_CND_BPF2) {
					G.mlog("周波数範囲の指定に誤りがあります.");
					this.numericUpDown7.Focus();
					return(false);
				}
#endif
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
#if false//2018.08.21
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
#endif
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
#if false//2018.08.21
			if (this.comboBox6.SelectedIndex == 2 && this.button4.Text == "<<") {
				this.groupBox6.Enabled = true;
			}
			else {
				this.groupBox6.Enabled = false;
			}
#endif
		}
		static public void check_fst(ComboBox cmb, bool bChecked)
		{
			if (bChecked) {
				if (cmb.FindString("深度合成") < 0) {
					cmb.Items.Insert(0, "深度合成");
//					cmb.SelectedIndex = 0;
				}
			}
			else {
				if (cmb.FindString("深度合成") >= 0) {
					cmb.Items.Remove("深度合成");
					if (cmb.SelectedIndex < 0) {
						cmb.SelectedIndex = cmb.FindString("ZP00D");
					}
				}
			}
		}
		private void checkBox9_CheckedChanged(object sender, EventArgs e)
		{
#if true//2018.08.21
			check_fst(this.comboBox10, this.checkBox9.Checked);
			check_fst(this.comboBox8 , this.checkBox9.Checked);
			check_fst(this.comboBox12, this.checkBox9.Checked);
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
#if true//2018.09.29(キューティクルライン検出)
		private void numericUpDown13_ValueChanged(object sender, EventArgs e)
		{
			if ((this.numericUpDown13.Value % 2) == 0) {
				this.numericUpDown13.Value = this.numericUpDown13.Value+1;
			}
		}
#endif
	}
}
