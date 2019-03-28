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
#if true//2018.11.13(毛髪中心AF)
		List<string> m_zpos = new List<string>();
		List<string> m_kpos = new List<string>();
#endif
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
            //---
            DDX(true);
			//---
			radioButton1_Click(null, null);
			//---
#if true//2019.03.16(NODATA対応)
			this.comboBox3.SelectedIndex = 0;
#endif
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
#if true//2018.11.13(毛髪中心AF)
			m_zpos.Clear();
			m_kpos.Clear();
#endif
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
						return;
					}
					for (int i = 0; i < zpos.Length; i++) {
						string tmp = System.IO.Path.GetFileNameWithoutExtension(zpos[i]);
#if true//2019.03.16(NODATA対応)
						// 012345678901
						// 0CR_00_ZP00D
						zpos[i] = tmp.Substring(tmp.Length-5);
#else
						zpos[i] = tmp.Substring(7);
#endif
					}
				}
				//files_10 = System.IO.Directory.GetFiles(path, "*_Z10.*");
				files_10 = System.IO.Directory.GetFiles(path, "*_ZP00D.*");
				files_10 = cut_IZ(files_10);

				if (files_10.Length <= 0) {
					//古い形式のファイルもしくはフォルダが空
					return;
				}
#if true//2018.08.13
			}
			catch (Exception ex) {
				G.mlog(ex.Message);
			}
#endif
			if (true) {
				this.comboBox8.Enabled = true;
#if true //2018.08.21
				this.comboBox10.Enabled = true;
				this.comboBox12.Enabled = true;
#endif
#if true//2018.11.13(毛髪中心AF)
				for (int i = 0; i < zpos.Length; i++) {
					switch (zpos[i][0]) {
						case 'Z':
						case 'z':
							m_zpos.Add(zpos[i]);
							break;
						case 'K':
						case 'k':
							m_kpos.Add(zpos[i]);
							break;
					}
				}
				for (int i = 0; i < m_zpos.Count; i++) {
					//if (this.comboBox18.SelectedIndex == 0 || this.comboBox18.SelectedIndex == 2) {
						this.comboBox10.Items.Add(m_zpos[i]);
					//}
					//if (this.comboBox19.SelectedIndex == 0 || this.comboBox19.SelectedIndex == 2) {
						this.comboBox8.Items.Add(m_zpos[i]);
					//}
					//if (this.comboBox20.SelectedIndex == 0 || this.comboBox20.SelectedIndex == 2) {
						this.comboBox12.Items.Add(m_zpos[i]);
					//}
				}
				for (int i = 0; i < m_kpos.Count; i++) {
					//if (this.comboBox18.SelectedIndex == 1 || this.comboBox18.SelectedIndex == 2) {
						this.comboBox10.Items.Add(m_kpos[i]);
					//}
					//if (this.comboBox19.SelectedIndex == 1 || this.comboBox19.SelectedIndex == 2) {
						this.comboBox8.Items.Add(m_kpos[i]);
					//}
					//if (this.comboBox20.SelectedIndex == 1 || this.comboBox20.SelectedIndex == 2) {
						this.comboBox12.Items.Add(m_kpos[i]);
					//}
				}
#else
#endif
				if (G.SS.MOZ_FST_CK00) {
					this.comboBox8.Items.Insert(0, "深度合成");
#if true //2018.08.21
					this.comboBox10.Items.Insert(0, "深度合成");
					this.comboBox12.Items.Insert(0, "深度合成");
#endif
				}
#if false//2018.08.21
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
#if true//2019.03.16(NODATA対応)
					files_ct = System.IO.Directory.GetFiles(path, "*CT_??" + zpos + ".*");
					files_cr = System.IO.Directory.GetFiles(path, "*CR_??" + zpos + ".*");
					files_ir = System.IO.Directory.GetFiles(path, "*IR_??" + zpos + ".*");
#else
					files_ct = System.IO.Directory.GetFiles(path, "?CT_??" + zpos + ".*");
					files_cr = System.IO.Directory.GetFiles(path, "?CR_??" + zpos + ".*");
					files_ir = System.IO.Directory.GetFiles(path, "?IR_??" + zpos + ".*");
#endif
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
#if true//2018.11.13(毛髪中心AF)
					break;
#else
					return;
#endif
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
#if true//2018.11.13(毛髪中心AF)
			if (G.SS.MOZ_FST_CK00) {//深度合成
				if (G.SS.MOZ_FST_IMTP == 0 && m_zpos.Count <= 0) {
					G.mlog("指定されたフォルダには表面画像ファイル('_ZP00D')がありません.\r\r" + path);
					e.Cancel = true;
					return;
				}
				if (G.SS.MOZ_FST_IMTP == 1 && m_kpos.Count <= 0) {
					G.mlog("指定されたフォルダには中心画像ファイル('_KP00D')がありません.\r\r" + path);
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
				DDV.DDX(bUpdate, this.numericUpDown3 , ref G.SS.MOZ_CND_DSUM);
				//---
				//---
#if true//2018.08.21
				G.SS.MOZ_CND_PDFL = 0;//カラー固定
				DDV.DDX(bUpdate, this.comboBox10     , ref G.SS.MOZ_CND_ZPCT);
				DDV.DDX(bUpdate, this.comboBox8      , ref G.SS.MOZ_CND_ZPHL);
				DDV.DDX(bUpdate, this.comboBox12     , ref G.SS.MOZ_CND_ZPML);
#endif
				DDV.DDX(bUpdate, this.checkBox8, ref G.SS.MOZ_CND_NOMZ);
				//---
				//---
				DDV.DDX(bUpdate, this.checkBox9      , ref G.SS.MOZ_FST_CK00);	//深度合成を行う
#if true//2018.07.02
				DDV.DDX(bUpdate, this.checkBox10, ref G.SS.MOZ_FST_CK01);		//合成済時スキップ
#endif
				DDV.DDX(bUpdate, this.numericUpDown5 , ref G.SS.MOZ_FST_RCNT);
				DDV.DDX(bUpdate, this.numericUpDown6 , ref G.SS.MOZ_FST_CCNT);
				DDV.DDX(bUpdate, this.comboBox7      , ref G.SS.MOZ_FST_MODE);
				DDV.DDX(bUpdate, this.comboBox9      , ref G.SS.MOZ_FST_FCOF);
#if true//2018.11.13(毛髪中心AF)
				DDV.DDX(bUpdate, this.comboBox21     , ref G.SS.MOZ_FST_IMTP);
#endif
#if true//2018.09.29(キューティクルライン検出)
				//---
				DDV.DDX(bUpdate, this.numericUpDown11, ref G.SS.MOZ_CND_HMAX);
				DDV.DDX(bUpdate, this.numericUpDown12, ref G.SS.MOZ_CND_HWID);
#endif
#if true//2019.03.16(NODATA対応)
				DDV.DDX(bUpdate, this.comboBox1      , ref G.SS.MOZ_BOK_AFMD[0]);//透過(表面)
				DDV.DDX(bUpdate, this.comboBox2      , ref G.SS.MOZ_BOK_AFMD[1]);//反射(表面)
				DDV.DDX(bUpdate, this.numericUpDown1 , ref G.SS.MOZ_BOK_CTHD);
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
				Form24 frm = new Form24();
				Form24.m_i = -1;
				frm.ShowDialog();
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
#endif
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{

		}
	}
}
