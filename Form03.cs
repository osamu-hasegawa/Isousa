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
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;
#if true//2018.11.28(メモリリーク)
using LIST_U8 = System.Collections.Generic.List<byte>;
#else
using VectorD = System.Collections.Generic.List<double>;
#endif

namespace uSCOPE
{
	public partial class Form03 : Form
	{
#if true//2019.05.22(再測定判定(キューティクル枚数))
		DIGITI	m_digi = new DIGITI();
#endif
#if false//2019.05.22(再測定判定(キューティクル枚数))
		private int[] C_FILT_COFS = new int[] { 0, 3, 5, 7, 9, 11 };
		private int[] C_FILT_CNTS = new int[] { 1, 5,10, 15, 20};
		private int[] C_SMTH_COFS = new int[] { 0,5,7,9,11,13,15,17,19,21,23, 25};
#endif
		private int m_i = 0;
		private int m_isel = 0;
#if true//2018.10.10(毛髪径算出・改造)
		private int m_imou = 0;
#endif
//%%		private int MOZ_CND_FTCF;//5:11x11
//%%		private int MOZ_CND_FTCT;//0:1回
//%%		private int MOZ_CND_SMCF;//5:重み係数=11
//%%		private string MOZ_CND_FOLD;
		//---
#if false//2019.05.22(再測定判定(キューティクル枚数))
		private ArrayList m_hair = new ArrayList();
#endif
		//---
//%%		private Bitmap	m_bmp_dm1, m_bmp_dm0, m_bmp_dm2;
//%%		private Bitmap	m_bmp_ir1, m_bmp_ir0, m_bmp_ir2;
//%%		private Bitmap	m_bmp_pd1;
#if true//2018.08.21
//%%	private Bitmap	m_bmp_pd0, m_bmp_pd2;
#endif
//%%		private Point[] m_dia_top;
//%%		private Point[]	m_dia_btm;
//%%		private int		m_dia_cnt;
		private int		m_chk1;
//%%			private int		m_chk2;
		//---
//%%		public Dictionary<string, ImageList> m_map_of_dml;
//%%		public Dictionary<string, ImageList> m_map_of_irl;
#if true//2018.08.21
//%%		public Dictionary<string, ImageList> m_map_of_pdl;//毛髪径:位置検出
#endif
//%%		private int		m_thm_wid, m_thm_hei;
//%%		private ArrayList m_zpos_org = new ArrayList();
//%%		private ArrayList m_zpos_val = new ArrayList();
//%%		private string m_fold_of_dept;
#if true//2018.10.10(毛髪径算出・改造)
//%%		public string m_errstr;
#endif
		//---
		public Form03()
		{
			InitializeComponent();
		}
#if true//2019.05.22(再測定判定(キューティクル枚数))
		//---
		private void enable_forms(bool b)
		{
			//this.Enabled = false;
			this.radioButton1.Enabled = this.radioButton2.Enabled = 
			this.radioButton3.Enabled = this.radioButton4.Enabled = 
			this.radioButton7.Enabled = this.radioButton8.Enabled = b;
#if true//2018.08.21
			this.button3.Enabled = b;
#else
			this.button1.Enabled = this.button3.Enabled = b;
#endif
			this.checkBox1.Enabled = 
			this.checkBox2.Enabled = 
			this.checkBox3.Enabled = 
			this.checkBox4.Enabled = 
			this.checkBox5.Enabled = 
			this.checkBox6.Enabled = 
			this.checkBox7.Enabled = 
			this.checkBox8.Enabled = 
			this.checkBox9.Enabled = b;
		}
#if true//2019.05.22(再測定判定(キューティクル枚数))
		private void call_back01()
		{
			this.textBox1.Text = m_digi.MOZ_CND_FOLD;
			//文字列の後方を表示させる
			this.textBox1.SelectionStart = this.textBox1.Text.Length;
			this.comboBox8.Items.Clear();
#if true//2018.08.21
			this.comboBox10.Items.Clear();
			this.comboBox12.Items.Clear();
#endif
			if (G.SS.MOZ_CND_ZCNT <= 0) {
				this.comboBox8.Enabled = false;
#if true//2018.08.21
				this.comboBox10.Enabled = false;
				this.comboBox12.Enabled = false;
#endif
			}
			else {
				this.comboBox8.Tag = true;
				if (G.SS.MOZ_FST_CK00) {
					this.comboBox8.Items.Add("深度合成");
#if true//2018.08.21
					this.comboBox10.Items.Add("深度合成");
					this.comboBox12.Items.Add("深度合成");
#endif
				}
				if (true) {
					string path = m_digi.MOZ_CND_FOLD;
					string[] zary = null;
#if true//2018.10.10(毛髪径算出・改造)
					string NS;
					for (int i = 0; i < 24; i++) {
						NS = i.ToString();
						zary = System.IO.Directory.GetFiles(path, NS + "CR_00_*.*");
						if (zary.Length > 0) {
							break;
						}
						zary = System.IO.Directory.GetFiles(path, NS + "CT_00_*.*");
						if (zary.Length > 0) {
							break;
						}
					}
#else
					zary = System.IO.Directory.GetFiles(path, "0CR_00_*.*");
					if (zary.Length <= 0) {
					zary = System.IO.Directory.GetFiles(path, "0CT_00_*.*");
					}
#endif
					for (int i = 0; i < zary.Length; i++) {
						string tmp = System.IO.Path.GetFileNameWithoutExtension(zary[i]);
						string sgn;
#if true//2018.11.13(毛髪中心AF)
						string k_z;
#endif
#if true//2019.03.16(NODATA対応)
						// 012345678901
						// 0CR_00_ZP00D
						tmp = tmp.Substring(tmp.Length-5);
#else
						tmp = tmp.Substring(7);
#endif
						m_digi.m_zpos_org.Add(tmp);
#if true//2018.11.13(毛髪中心AF)
						k_z = tmp.Substring(0, 1);
#endif
						if (tmp.Substring(1, 1) == "P") {
							sgn = "+";
						}
						else {
							sgn = "-";
						}
						tmp = tmp.Substring(2, 2);
#if true//2018.11.13(毛髪中心AF)
						m_digi.m_zpos_val.Add(k_z+sgn+tmp);
#else
						m_digi.m_zpos_val.Add(sgn+tmp);
#endif
					}
					m_digi.sort_zpos();
					this.comboBox8.Items.AddRange(m_digi.m_zpos_val.ToArray());
					this.comboBox10.Items.AddRange(m_digi.m_zpos_val.ToArray());
					this.comboBox12.Items.AddRange(m_digi.m_zpos_val.ToArray());
				}
#if true//2018.08.21
				this.comboBox10.SelectedIndex = this.comboBox10.FindString(m_digi.ZORG2VAL(G.SS.MOZ_CND_ZPCT));
				this.comboBox8 .SelectedIndex = this.comboBox8 .FindString(m_digi.ZORG2VAL(G.SS.MOZ_CND_ZPHL));
				this.comboBox12.SelectedIndex = this.comboBox12.FindString(m_digi.ZORG2VAL(G.SS.MOZ_CND_ZPML));
				//
				object obj;
				obj = this.comboBox10.Items[this.comboBox10.SelectedIndex];
				this.comboBox10.Items[this.comboBox10.SelectedIndex] = obj.ToString() + "(*)";
				//
				obj = this.comboBox8.Items[this.comboBox8.SelectedIndex];
				this.comboBox8.Items[this.comboBox8.SelectedIndex] = obj.ToString() + "(*)";
				//
				obj = this.comboBox12.Items[this.comboBox12.SelectedIndex];
				this.comboBox12.Items[this.comboBox12.SelectedIndex] = obj.ToString() + "(*)";
#else
				this.comboBox8.SelectedIndex = this.comboBox8.FindString(ZORG2VAL(G.SS.MOZ_CND_ZPOS));
#endif
			}
		}
		private void call_back02(Bitmap bmp1, Bitmap bmp2, Bitmap bmp3)
		{
			Image tmp1 = this.pictureBox1.Image;
			Image tmp2 = this.pictureBox2.Image;
#if true//2018.08.21
			Image tmp3 = this.pictureBox3.Image;
			this.pictureBox3.Image = (Bitmap)bmp3.Clone();
#endif
			this.pictureBox1.Image = (Bitmap)bmp1.Clone();
			if (m_digi.m_bmp_ir1 != null) {
			this.pictureBox2.Image = bmp2;
			}
			this.pictureBox1.Update();
			this.pictureBox2.Update();
#if true//2018.08.21
			this.pictureBox3.Update();
#endif
			//;
//%%			this.listView1.Items.Add(name_dm1, i);
//%%			this.listView2.Items.Add(name_ir1, i);
//%%			this.listView1.Items[i].EnsureVisible();
//%%			this.listView1.Update();
			//
			if (tmp1 != null) {
				tmp1.Dispose();
				tmp1 = null;
			}
			if (tmp2 != null) {
				tmp2.Dispose();
				tmp2 = null;
			}
#if true//2018.08.21
			if (tmp3 != null) {
				tmp3.Dispose();
				tmp3 = null;
			}
#endif
		}
		private void call_back03(string name_dm1, string name_ir1, int i)
		{
			this.listView1.Items.Add(name_dm1, i);
			this.listView2.Items.Add(name_ir1, i);
			this.listView1.Items[i].EnsureVisible();
			this.listView1.Update();
		}
		private void call_back04()
		{
			this.comboBox1.Items.Add(m_digi.m_hair.Count.ToString());
			this.label2.Text = "/ " + m_digi.m_hair.Count.ToString();
		}
		private void call_back05()
		{
			//this.Enabled = true;
			this.comboBox1.Enabled = false;
			this.comboBox1.SelectedIndex = 0;
			this.comboBox1.Enabled = true;
			//---
			if (this.radioButton7.Checked) {
			int isel = m_digi.m_hair[0].cnt_of_seg / 2;
			this.listView1.Items[isel].Selected = true;
			this.listView1.Items[isel].EnsureVisible();
			}
			enable_forms(true);
		}
		private void call_back06()
		{
			this.label6.Text = string.Format("x {0:F1}", m_digi.m_log_info.zoom);
		}
		private void call_back07(object obj)
		{
			DIGITI.hair hr = (DIGITI.hair)obj;
			this.listView1.LargeImageList = hr.il_dm;
			this.listView2.LargeImageList = hr.il_ir;
		}
		//---
		private void load()
		{
			var dlg = new DlgProgress();
			try {
				int cnt_of_hair = 0;
				dlg.Show("毛髄径算出", G.FORM01);
				G.bCANCEL = false;

				enable_forms(false);
				m_digi.load(
						dlg.SetStatus,
						call_back01, call_back02, call_back03,
						call_back04, call_back05, call_back06,
						call_back07,
						null,null,null
				);
				//---
				if (true) {
					if (m_digi.m_hair.Count <= 0) {
						//this.button1.Enabled = this.button3.Enabled = false;
					}
					else {
						if (m_digi.m_hair[0].seg[0].name_of_ir.Length <= 0) {
							this.radioButton8.Enabled = false;
							this.radioButton8.BackColor = Color.FromArgb(64,64,64);
						}
					}
				}
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}

			if (dlg != null) {
				dlg.Hide();
			    dlg.Dispose();
			    dlg = null;
			}
			this.comboBox8.Tag = null;
			this.Enabled = true;
		}
#endif
#if false//2019.05.22(再測定判定(キューティクル枚数))
		//---
		private void load()
		{
			var dlg = new DlgProgress();
			try {
			int cnt_of_hair = 0;
#if true//2019.01.11(混在対応)
			int mode_of_cl;//0:透過, 1:反射
#endif

#if true//2018.08.21
			string zpos = "ZP00D";
#else
			string zpos = G.SS.MOZ_CND_ZPOS;
#endif
			string pext = "";

			dlg.Show("毛髄径算出", G.FORM01);
			G.bCANCEL = false;
#if true
			if (G.SS.MOZ_FST_CK00) {
				dlg.SetStatus("深度合成中");
				fst_make();
			}
#endif
			if (string.IsNullOrEmpty(zpos)) {
				zpos = "";
			}
			else if (string.Compare(zpos, "深度合成") == 0) {
				zpos = "_ZDEPT";
				pext = m_fold_of_dept;
			}
			else {
				zpos = "_" + zpos;
			}

			enable_forms(false);

			if (true) {
				this.textBox1.Text = this.MOZ_CND_FOLD;
				//文字列の後方を表示させる
				this.textBox1.SelectionStart = this.textBox1.Text.Length;
				G.CAM_PRC = G.CAM_STS.STS_HAIR;
				this.comboBox8.Items.Clear();
#if true//2018.08.21
				this.comboBox10.Items.Clear();
				this.comboBox12.Items.Clear();
#endif
				if (G.SS.MOZ_CND_ZCNT <= 0) {
					this.comboBox8.Enabled = false;
#if true//2018.08.21
					this.comboBox10.Enabled = false;
					this.comboBox12.Enabled = false;
#endif
				}
				else {
					this.comboBox8.Tag = true;
					if (G.SS.MOZ_FST_CK00) {
						this.comboBox8.Items.Add("深度合成");
#if true//2018.08.21
						this.comboBox10.Items.Add("深度合成");
						this.comboBox12.Items.Add("深度合成");
#endif
					}
					if (true) {
						string path = this.MOZ_CND_FOLD;
						string[] zary = null;
#if true//2018.10.10(毛髪径算出・改造)
						string NS;
						for (int i = 0; i < 24; i++) {
							NS = i.ToString();
							zary = System.IO.Directory.GetFiles(path, NS + "CR_00_*.*");
							if (zary.Length > 0) {
								break;
							}
							zary = System.IO.Directory.GetFiles(path, NS + "CT_00_*.*");
							if (zary.Length > 0) {
								break;
							}
						}
#else
						zary = System.IO.Directory.GetFiles(path, "0CR_00_*.*");
						if (zary.Length <= 0) {
						zary = System.IO.Directory.GetFiles(path, "0CT_00_*.*");
						}
#endif
						for (int i = 0; i < zary.Length; i++) {
							string tmp = System.IO.Path.GetFileNameWithoutExtension(zary[i]);
							string sgn;
#if true//2018.11.13(毛髪中心AF)
							string k_z;
#endif
#if true//2019.03.16(NODATA対応)
							// 012345678901
							// 0CR_00_ZP00D
							tmp = tmp.Substring(tmp.Length-5);
#else
							tmp = tmp.Substring(7);
#endif
							m_zpos_org.Add(tmp);
#if true//2018.11.13(毛髪中心AF)
							k_z = tmp.Substring(0, 1);
#endif
							if (tmp.Substring(1, 1) == "P") {
								sgn = "+";
							}
							else {
								sgn = "-";
							}
							tmp = tmp.Substring(2, 2);
#if true//2018.11.13(毛髪中心AF)
							m_zpos_val.Add(k_z+sgn+tmp);
#else
							m_zpos_val.Add(sgn+tmp);
#endif
						}
						sort_zpos();
						this.comboBox8.Items.AddRange(m_zpos_val.ToArray());
#if true//2018.08.21
						this.comboBox10.Items.AddRange(m_zpos_val.ToArray());
						this.comboBox12.Items.AddRange(m_zpos_val.ToArray());
#endif
					}
#if true//2018.08.21
					this.comboBox10.SelectedIndex = this.comboBox10.FindString(ZORG2VAL(G.SS.MOZ_CND_ZPCT));
					this.comboBox8 .SelectedIndex = this.comboBox8 .FindString(ZORG2VAL(G.SS.MOZ_CND_ZPHL));
					this.comboBox12.SelectedIndex = this.comboBox12.FindString(ZORG2VAL(G.SS.MOZ_CND_ZPML));
					//
					object obj;
					obj = this.comboBox10.Items[this.comboBox10.SelectedIndex];
					this.comboBox10.Items[this.comboBox10.SelectedIndex] = obj.ToString() + "(*)";
					//
					obj = this.comboBox8.Items[this.comboBox8.SelectedIndex];
					this.comboBox8.Items[this.comboBox8.SelectedIndex] = obj.ToString() + "(*)";
					//
					obj = this.comboBox12.Items[this.comboBox12.SelectedIndex];
					this.comboBox12.Items[this.comboBox12.SelectedIndex] = obj.ToString() + "(*)";
#else
					this.comboBox8.SelectedIndex = this.comboBox8.FindString(ZORG2VAL(G.SS.MOZ_CND_ZPOS));
#endif
				}
			}
			test_log();
			//G.mlog("倍率表示と倍率評価");
			//G.mlog("ピッチ評価、保存と読込と");
			//G.mlog("キャンセル押下時の挙動確認、二本中１本でスルーとかができるようになっていれば");
			//G.mlog("1本目解析終了時に画面がイネーブルになるように...");
			//G.mlog("1本目解析終了時にセンターitemが選ばれるように...");
			//G.mlog("判定範囲40%の有効を確認(dis->enaに倒す)...");
			//G.mlog("毛髪２本で赤外有り→赤外無しのパターン...");
			//G.mlog(".毛髪選択時にstackoverflow..");
			//G.mlog("m_back_ofが毛髪切り替わり時にリセットされてない？...");
			//G.mlog("コントラ計算範囲がhistと自動測定でごっちゃになっている県...");
			//G.mlog("コントラ計算範囲が画面全体だとerror発生...");
			//G.mlog("検出パラメータを元に戻す");
			//---
			cnt_of_hair = get_hair_cnt(pext, zpos);
			//---
			for (int q = 0; q <
#if true//2018.09.27(20本対応と解析用パラメータ追加)
				24
#else
				10
#endif
				; q++) {
				int width = 0;//(int)(2592/8);//2592/8=324
				int height =0;//(int)(1944/8);//1944/8=243
				//string path;
				string[] files_ct, files_cr, files_cl, files_ir;
				string[] files_pd, files_dm;

				string buf = q.ToString();
				int cnt_of_seg;
#if true//2018.09.29(キューティクルライン検出)
				if (q == 0) {
					if (!calc_filter_coeff()) {
						G.mlog("フィルタ係数の計算ができませんでした.パラメータを確認してください.\r"+ F_REMEZ_SCIPY.ERRMSG);
						break;
					}
				}
#endif
				files_ct = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD+pext,  buf +  "CT_??"+zpos+".*");
				files_cr = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD+pext,  buf +  "CR_??"+zpos+".*");
				files_ir = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD+pext,  buf +  "IR_??"+zpos+".*");
				if (files_ct.Length <= 0 && files_cr.Length <= 0) {
#if true//2018.10.10(毛髪径算出・改造)
					continue;
#else
					break;//終了
#endif
				}
				if (files_ct.Length > 0 && files_cr.Length > 0) {
					break;//終了(反射と透過が混在！)
				}
				if (files_ct.Length > 0) {
					files_cl = files_ct;//透過
#if true//2018.09.27(20本対応と解析用パラメータ追加)
					G.set_imp_param(/*透過*/3, -1);
#else
					G.set_imp_param(/*透過*/0, -1);
#endif
#if true//2019.01.11(混在対応)
					SWAP_ANL_CND(mode_of_cl = 0);//0:透過, 1:反射
#endif
				}
				else {
					files_cl = files_cr;//反射
#if true//2018.09.27(20本対応と解析用パラメータ追加)
					G.set_imp_param(/*反射*/4, -1);
#else
					G.set_imp_param(/*反射*/1, -1);
#endif
#if true//2019.01.11(混在対応)
					SWAP_ANL_CND(mode_of_cl = 1);//0:透過, 1:反射
#endif
				}
				cnt_of_seg = files_cl.Length;
				//---
				if (/*位置検出*/G.SS.MOZ_CND_PDFL == 1/*赤外*/ && files_ir.Length <= 0) {
					break;
				}
				switch (/*位置検出*/G.SS.MOZ_CND_PDFL) {
				case  0:/*カラー*/files_pd = files_cl; break;
				default:/*赤外  */files_pd = files_ir; break;
				}
				//カラー断面
#if true//2018.08.21
				files_dm = files_cl;
#else
				files_dm = files_cl;
#endif
				//---
				m_back_of_x = 0;
				//---
				var hr = new hair();
				var ar_seg = new ArrayList();
				seg_of_hair[] segs = null;
#if true//2018.09.29(キューティクルライン検出)
				if (q == 10) {
					q = q;
				}
#endif
#if true//2018.11.22(数値化エラー対応)
				bool bFileExist = true;
				if (q == 5) {
					q = q;
				}
#endif
				for (int i = 0; i < cnt_of_seg; i++) {
#if true//2018.08.21
					string path_dm1 = to_xx_file(0, files_dm[i]);
					string path_ir1 = to_xx_file(2, files_dm[i]);
					string path_pd1 = to_xx_file(1, files_dm[i]);
#else
					string path_dm1 = files_dm[i];
					string path_ir1 = to_ir_file(path_dm1);
					string path_pd1 = files_pd[i];
#endif
					string name_dm1 = get_name_of_path(path_dm1);
					string name_ir1 = get_name_of_path(path_ir1);
					string name_pd1 = get_name_of_path(path_pd1);
#if true//2018.11.22(数値化エラー対応)
					if (string.IsNullOrEmpty(path_dm1) || string.IsNullOrEmpty(path_ir1) || string.IsNullOrEmpty(path_pd1)) {
						bFileExist = false; break;
					}
					if (string.IsNullOrEmpty(name_dm1) || string.IsNullOrEmpty(name_ir1) || string.IsNullOrEmpty(name_pd1)) {
						bFileExist = false; break;
					}
#endif
					seg_of_hair seg = new seg_of_hair();
					seg.path_of_dm = path_dm1;
					seg.path_of_ir = path_ir1;
					seg.name_of_dm = name_dm1;
					seg.name_of_ir = name_ir1;
					//---
					seg.path_of_pd = path_pd1;
					seg.name_of_pd = name_pd1;
					//---
					test_pr0(seg, /*b1st=*/(i==0));
					ar_seg.Add(seg);
#if true//2019.03.16(NODATA対応)
					string tmp;
					G.CAM_STS bak = G.CAM_PRC;
					tmp = to_xx_path(seg.path_of_dm, "ZP00D");
					Bitmap bmp = new Bitmap(tmp);
#if true//2019.03.22(再測定表)
					//mode_of_cl=0:透過, 1:反射
					G.CNT_MOD = G.AFMD2N(G.SS.MOZ_BOK_AFMD[mode_of_cl]);//表面:コントスラト計算範囲
					G.CNT_OFS = G.SS.MOZ_BOK_SOFS[mode_of_cl];			//表面:上下オフセット
					G.CNT_MET = G.SS.MOZ_BOK_CMET[mode_of_cl];			//表面:計算方法
//					G.CNT_USSD= G.SS.MOZ_BOK_USSD[mode_of_cl];			//表面:標準偏差
#else
					if (G.SS.MOZ_BOK_AFMD[mode_of_cl] == 0) {
						G.CNT_MOD = 0;
					}
					else {
						G.CNT_MOD = G.SS.MOZ_BOK_AFMD[mode_of_cl]+1;
					}
					G.CNT_OFS = 0;
#endif
					G.CAM_PRC = G.CAM_STS.STS_HIST;
					G.FORM02.load_file(bmp, false);
					seg.contr = G.IR.CONTRAST;
					seg.contr_drop = double.NaN;
					seg.contr_avg = double.NaN;
					G.CAM_PRC = bak;
					bmp.Dispose();
#endif
				}

#if true//2018.11.22(数値化エラー対応)
				if (!bFileExist) {
					G.mlog(string.Format("毛髪画像が不完全のため{0}本目の毛髪画像の読み込みをスキップします。", m_hair.Count+1));
					continue;
				}
#endif
				segs = (seg_of_hair[])ar_seg.ToArray(typeof(seg_of_hair));
				System.Diagnostics.Debug.WriteLine("image-listのsizeをどこかで調整しないと…");
				//---
				if (m_hair.Count == 0) {
					this.listView1.LargeImageList = hr.il_dm;
					this.listView2.LargeImageList = hr.il_ir;
				}
#if true//2019.03.16(NODATA対応)
				if (true) {
					double contr_avg = 0;
					for (int i = 0; i < segs.Length; i++) {
						contr_avg += segs[i].contr;
					}
					contr_avg /= segs.Length;
					for (int i = 0; i < segs.Length; i++) {
						segs[i].contr_drop = -(segs[i].contr - contr_avg) / contr_avg * 100;
						segs[i].contr_avg = contr_avg;
						segs[i].bNODATA = (segs[i].contr_drop >= G.SS.MOZ_BOK_CTHD);
					}
				}
#endif
				for (int i = 0; i < segs.Length; i++) {
#if true//2018.11.28(メモリリーク)
					//GC.Collect();
					if (i == 10) {
						//G.bCANCEL = true;
						//break;
					}
#endif
					dlg.SetStatus(string.Format("計算中 {0}/{1}\r{2}/{3}本", i+1, segs.Length, m_hair.Count+1, cnt_of_hair));
					Application.DoEvents();
					if (G.bCANCEL) {
						break;
					}
					//---
					string path_dm1 = segs[i].path_of_dm;
					string path_dm2 = (i != (segs.Length-1)) ? (segs[i+1].path_of_dm): null;
					string path_pd1 = segs[i].path_of_pd;
					string name_dm1 = segs[i].name_of_dm;
					string name_ir1 = segs[i].name_of_ir;
					string name_pd1 = segs[i].name_of_pd;
#if true//2018.08.21
					string path_ir1 = segs[i].path_of_ir;
					string path_ir2 = (i != (segs.Length-1)) ? (segs[i+1].path_of_ir): null;

					load_bmp(segs, i,
						path_dm1, path_dm2,
						path_ir1, path_ir2,
						ref m_bmp_dm0, ref m_bmp_dm1, ref m_bmp_dm2,
						ref m_bmp_ir0, ref m_bmp_ir1, ref m_bmp_ir2
					);
#else
					load_bmp(segs, i,
						path_dm1, path_dm2,
						ref m_bmp_dm0, ref m_bmp_dm1, ref m_bmp_dm2,
						ref m_bmp_ir0, ref m_bmp_ir1, ref m_bmp_ir2
					);
#endif
					if (true) {
						dispose_bmp(ref m_bmp_pd1);
						if (name_pd1.Equals(name_dm1)) {
							m_bmp_pd1 = (Bitmap)m_bmp_dm1.Clone();
						}
						else {
							m_bmp_pd1 = new Bitmap(path_pd1);
						}
					}
					if (i == 0) {
						width = m_bmp_dm1.Width;
						height = m_bmp_dm1.Height;
						while (width > 640) {
							width /= 2;		//->324
							height /= 2;	//->243
						}
						if ((i+1) < segs.Length) {
							segs[i+1].width = m_bmp_dm2.Width;
							segs[i+1].height = m_bmp_dm2.Height;
						}
						m_thm_wid = width;
						m_thm_hei = height;
					}
					if (true) {
						segs[i].width = m_bmp_dm1.Width;
						segs[i].height = m_bmp_dm1.Height;
					}
					//---
					Image thm = createThumbnail(m_bmp_dm1, width, height);
					hr.il_dm.Images.Add(thm);
					//---
					if (m_bmp_ir1 != null) {
					thm = createThumbnail(m_bmp_ir1, width, height);
					hr.il_ir.Images.Add(thm);
					}
					//---
					if (m_hair.Count == 0) {
						Image tmp1 = this.pictureBox1.Image;
						Image tmp2 = this.pictureBox2.Image;
#if true//2018.08.21
						Image tmp3 = this.pictureBox3.Image;
						this.pictureBox3.Image = (Bitmap)m_bmp_pd1.Clone();
#endif
						this.pictureBox1.Image = (Bitmap)m_bmp_dm1.Clone();
						if (m_bmp_ir1 != null) {
						this.pictureBox2.Image = (Bitmap)m_bmp_ir1.Clone();
						}
						this.pictureBox1.Update();
						this.pictureBox2.Update();
#if true//2018.08.21
						this.pictureBox3.Update();
#endif
						//;
						this.listView1.Items.Add(name_dm1, i);
						this.listView2.Items.Add(name_ir1, i);
						this.listView1.Items[i].EnsureVisible();
						this.listView1.Update();
						//
						if (tmp1 != null) {
							tmp1.Dispose();
							tmp1 = null;
						}
						if (tmp2 != null) {
							tmp2.Dispose();
							tmp2 = null;
						}
#if true//2018.08.21
						if (tmp3 != null) {
							tmp3.Dispose();
							tmp3 = null;
						}
#endif
					}
					if (false) {
					}
					else if (G.SS.MOZ_CND_PDFL == 0/*カラー*/) {
						//---
						object obj = m_bmp_dm1.Tag;
						m_bmp_dm1.Tag = null;
						G.FORM02.load_file(m_bmp_pd1/*m_bmp_dm1*/, false);
						m_bmp_dm1.Tag = obj;
						//---
					}
					else {/*赤外*/
#if true//2018.09.27(20本対応と解析用パラメータ追加)
						//カラー固定のため(G.SS.MOZ_CND_PDFL == 0)ここは通らない
						throw new Exception("Internal Error");
#else
						//カラー固定
						string path_of_bo = this.textBox1.Text + "\\" + segs[i].name_of_ir;
						Bitmap bo;
						Bitmap bmp_ir = (Bitmap)m_bmp_ir1.Clone();

						G.CAM_PRC = G.CAM_STS.STS_NONE;
						G.FORM02.load_file(bmp_ir, false);
						G.CAM_PRC = G.CAM_STS.STS_HAIR;
						Form02.DO_PROC_IR(bmp_ir, out bo);
#if false//2018.08.21
						if (G.SS.MOZ_IRC_SAVE) {
							save_iz(path_of_bo, ref bo);
						}
#endif
						bmp_ir.Dispose();
						bmp_ir = null;
#endif
					}
					if (G.SS.MOZ_CND_NOMZ) {
						//断面・毛髄径計算は行わない
					}
#if false//2018.08.21
					else if (G.SS.MOZ_CND_PDFL == 1 && G.SS.MOZ_IRC_NOMZ) {
						G.SS.MOZ_IRC_NOMZ = G.SS.MOZ_IRC_NOMZ;//断面・毛髄径計算は行わない
					}
#endif
#if true//2019.03.16(NODATA対応)
					if (segs[i].bNODATA) {
						//処理しない
						m_dia_cnt = 0;
						G.IR.CIR_CNT = 0;
						test_nd(segs, i, segs.Length);
					}
#endif
					else if (G.IR.CIR_CNT > 0) {
						if (m_bmp_ir1 != null && G.SS.MOZ_CND_FTCF > 0) {
							Form02.DO_SMOOTH(m_bmp_ir1, this.MOZ_CND_FTCF, this.MOZ_CND_FTCT);
						}
#if true//2018.11.28(メモリリーク)
						//GC.Collect();
#endif
						test_pr1(segs[i]);
						if (m_dia_cnt > 1) {
							test_dm(segs, i, segs.Length);
						}
					}
#if true//2018.11.02(HSVグラフ)
#if true//2018.11.13(毛髪中心AF)
					if (G.IR.CIR_CNT <= 0) {
						m_dia_cnt = m_dia_cnt;
					}else
#endif
#if true//2018.11.30(ヒストグラム算出エラー)
					if (m_dia_cnt <= 1) {
						m_dia_cnt = m_dia_cnt;
					}else
#endif
					if (true) {
						calc_hist(segs[i]);
					}
#endif
				}
				dispose_bmp(ref m_bmp_dm1);
				dispose_bmp(ref m_bmp_dm2);
				dispose_bmp(ref m_bmp_ir1);
				dispose_bmp(ref m_bmp_ir2);
#if true//2018.11.28(メモリリーク)
				dispose_bmp(ref m_bmp_pd1);
				//GC.Collect();
#endif
				if (G.bCANCEL) {
					break;
				}
#if true//2019.01.11(混在対応)
				hr.mode_of_cl = mode_of_cl;//0:透過, 1:反射
#endif
				//---
				hr.seg = segs;//(seg_of_hair[])ar_seg.ToArray(typeof(seg_of_hair));
				if (true) {
					float ymin = float.MaxValue, ymax = float.MinValue;
					float xmax = float.MinValue;
					for (int j = 0; j < hr.seg.Length; j++) {
						if (hr.seg[j] == null) {
							continue;
						}
						if (ymin > hr.seg[j].pix_pos.Y) {
							ymin = hr.seg[j].pix_pos.Y;
						}
						if (xmax < (hr.seg[j].pix_pos.X + hr.seg[j].width)) {
							xmax = (hr.seg[j].pix_pos.X + hr.seg[j].width);
						}
					}
					for (int j = 0; j < hr.seg.Length; j++) {
						if (hr.seg[j] == null) {
							continue;
						}
						hr.seg[j].pix_pos.Y -= ymin;
						if (ymax < hr.seg[j].pix_pos.Y + hr.seg[j].height) {
							ymax = hr.seg[j].pix_pos.Y + hr.seg[j].height;
						}
					}
					hr.width_of_hair = xmax;
					hr.height_of_hair = ymax;
				}
				hr.cnt_of_seg = hr.seg.Length;
				m_hair.Add(hr);
				this.comboBox1.Items.Add(m_hair.Count.ToString());
				this.label2.Text = "/ " + m_hair.Count.ToString();

				if (m_hair.Count == 1) {
					//this.Enabled = true;
					this.comboBox1.Enabled = false;
					this.comboBox1.SelectedIndex = 0;
					this.comboBox1.Enabled = true;
					//---
					if (this.radioButton7.Checked) {
					int isel = ((hair)m_hair[0]).cnt_of_seg / 2;
					this.listView1.Items[isel].Selected = true;
					this.listView1.Items[isel].EnsureVisible();
					}
					enable_forms(true);
				}
			}



			G.CAM_PRC = G.CAM_STS.STS_NONE;
			dlg.Hide();
			dlg.Dispose();
			dlg = null;

			if (true) {
				G.FORM02.Close();
				//G.FORM02.Dispose();
				G.FORM02 = null;
				this.Enabled = true;
			}
			if (true) {
				if (m_hair.Count <= 0) {
					//this.button1.Enabled = this.button3.Enabled = false;
				}
				else {
					if (((hair)m_hair[0]).seg[0].name_of_ir.Length <= 0) {
						this.radioButton8.Enabled = false;
						this.radioButton8.BackColor = Color.FromArgb(64,64,64);
					}
				}
			}
			}
			catch (Exception ex) {
#if false//2018.11.22(数値化エラー対応)
#if true//2018.09.29(キューティクルライン検出)
				G.mlog("例外発生時の復帰ができるように修正するコト！！！");
#endif
#endif
				G.mlog(ex.ToString());
				string buf = ex.ToString();
			}
			if (dlg != null) {
#if true//2018.11.22(数値化エラー対応)
				dlg.Hide();
#endif
			    dlg.Dispose();
			    dlg = null;
			}
			this.comboBox8.Tag = null;
#if true//2018.11.22(数値化エラー対応)
			if (G.FORM02 != null) {
				G.FORM02.Close();
				G.FORM02 = null;
			}
			this.Enabled = true;
#endif
		}
#endif
		private void init()
		{
			if (true) {
			this.groupBox1.Dock = DockStyle.Fill;
			this.groupBox2.Dock = DockStyle.Fill;
			//---
			this.chart1.Dock = DockStyle.Fill;
			this.chart2.Dock = DockStyle.Fill;
			//---
			this.pictureBox1.Dock = DockStyle.Fill;
			this.pictureBox2.Dock = DockStyle.Fill;
			this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			this.pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
			}
#if true//2018.08.21
			this.groupBox6.Dock = DockStyle.Fill;
			this.chart3.Dock = DockStyle.Fill;
			this.pictureBox3.Dock = DockStyle.Fill;
			this.pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
#endif
			//---
			this.listView1.Dock = DockStyle.Fill;
			this.listView2.Visible = false;
			this.listView2.Dock = DockStyle.Fill;
			//---
//			this.radioButton1.Enabled = !this.radioButton1.Enabled;
//			this.radioButton1.Enabled = false;
			if (this.radioButton1.Enabled) {
				this.radioButton1.BackColor = Color.Black;
			}
			else {
				this.radioButton1.ForeColor = Color.White;
				this.radioButton1.BackColor = Color.FromArgb(64,64,64);
			}
			this.comboBox1.Enabled = false;
#if true//2019.05.07(毛髄複線面積値対応)
			this.comboBox2.SelectedIndex = 3;//毛髄検出ライン<-全て
#else
#if true//2019.04.17(毛髄検出複線化)
			this.comboBox2.SelectedIndex = 1;//毛髄検出ライン<-中心
#endif
#endif
#if false//2018.11.10(保存機能)
			this.comboBox2.SelectedIndex = 1;
			this.comboBox2.Enabled = true;
#endif
#if true//2018.11.10(保存機能)
			this.tabControl1.SelectedIndex = 2;//キューティクル間隔
			this.tabControl2.SelectedIndex = 1;//毛髪径HSV
			this.tabControl3.SelectedIndex = 0;//毛髄径
#endif
			//---
			//無し, 3x3, 5x5, 7x7, 9x9, 11x11
//%%			this.MOZ_CND_FTCF = m_digi.C_FILT_COFS[G.SS.MOZ_CND_FTCF];
//%%			this.MOZ_CND_FTCT = m_digi.C_FILT_CNTS[G.SS.MOZ_CND_FTCT];
//%%			this.MOZ_CND_SMCF = m_digi.C_SMTH_COFS[G.SS.MOZ_CND_SMCF];//重み係数=11
//%%			this.MOZ_CND_FOLD = (G.SS.MOZ_CND_FMOD == 0) ? G.SS.AUT_BEF_PATH: G.SS.MOZ_CND_FOLD;
			//---
			//---
			//this.checkBox10.Checked = (G.SS.MOZ_CND_USIR == 1);
			//---
//%%			m_map_of_dml = new Dictionary<string,ImageList>();
//%%			m_map_of_irl = new Dictionary<string,ImageList>();
#if true//2018.08.21
//%%			m_map_of_pdl = new Dictionary<string,ImageList>();
#endif
		}

		private void Form03_Load(object sender, EventArgs e)
		{
			if (true) {
#if true//2019.04.17(毛髄検出複線化)
				this.SetDesktopBounds(G.AS.APP_F03_LFT, G.AS.APP_F03_TOP, G.AS.APP_F03_WID, G.AS.APP_F03_HEI);
#else
				this.SetDesktopBounds(G.AS.APP_F02_LFT, G.AS.APP_F02_TOP, G.AS.APP_F02_WID, G.AS.APP_F02_HEI);
#endif
			}
			if (true) {
				if (G.UIF_LEVL == 0) {
#if true//2018.07.02
					/*0:ユーザ用(暫定版)*/
#if false//2018.08.21
					this.checkBox10.Visible = false;//カラー画像の代わりに赤外の毛髪抽出画像を表示する
#endif
					this.label9.Visible = false;//グラフ表示には反映されません
#if false//2018.07.10
					this.panel13.Visible = false;//Z位置とZ選択用コンボ
#endif
#endif
#if true//2019.03.16(NODATA対応)
					this.checkBox21.Visible = false;
					this.panel18.Visible = false;
#endif
				}
				if (G.SS.MOZ_CND_NOMZ) {
					this.groupBox4.Visible = false;
					this.groupBox1.Visible = false;
					this.groupBox2.Visible = false;
#if true//2018.08.21
					this.groupBox6.Visible = false;
#endif
					//---
					this.checkBox1.Visible = false;
					this.checkBox9.Visible = false;
					this.checkBox2.Visible = false;
					this.checkBox8.Visible = false;
#if false//2018.08.21
					this.checkBox10.Visible = false;
#endif
					this.panel5.Visible = false;
					this.panel8.Visible = false;
					this.label7.Visible = false;
					this.label9.Visible = false;
					//---
					this.tableLayoutPanel1.RowCount = 1;
					this.tableLayoutPanel2.RowCount = 1;
				}
			}
//#if false//2018.11.06(毛髄4)
#if true//2018.10.27(画面テキスト)
			this.tabControl3.TabPages.RemoveAt(2);
#endif
//#endif
#if true//2018.11.10(保存機能)
			this.label16.Visible = false;
			this.label17.Visible = false;
			this.numericUpDown2.Visible = false;
			this.panel16.Visible = false;
#endif
			init();
			if (true) {
				G.push_imp_para();
			}
			m_digi.INIT(/*bREMES=*/false);
			this.BeginInvoke(new G.DLG_VOID_VOID(this.load));
#if true//2018.10.10(毛髪径算出・改造)
			this.chart2.Series[1].Enabled = false;
#endif
#if true//2018.11.02(HSVグラフ)
			this.tabControl2.TabPages.RemoveAt(2);//[退避]
			set_hismod();
#endif
		}
		private void Form03_FormClosing(object sender, FormClosingEventArgs e)
		{
			m_digi.TERM();
			//---
			Image tmp1 = this.pictureBox1.Image;
			Image tmp2 = this.pictureBox2.Image;
			this.pictureBox1.Image = null;
			this.pictureBox2.Image = null;
			if (tmp1 != null) {
				tmp1.Dispose();
				tmp1 = null;
			}
			if (tmp2 != null) {
				tmp2.Dispose();
				tmp2 = null;
			}
			if (true) {
				G.pop_imp_para();
			}
#if true//2019.04.17(毛髄検出複線化)
			if (this.Left <= -32000 || this.Top <= -32000) {
				//最小化時は更新しない
			}
			else {
			G.AS.APP_F03_LFT = this.Left;
			G.AS.APP_F03_TOP = this.Top;
			G.AS.APP_F03_WID = this.Width;
			G.AS.APP_F03_HEI = this.Height;
			}
#endif
			//---
			G.FORM03 = null;
			G.FORM12.UPDSTS();
		}
		private void listView2_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.radioButton8.Checked) {
				ListView lv = (ListView)sender;
				if (lv.SelectedItems.Count != 1) {
					return;
				}
				int isel = lv.SelectedItems[0].Index;
				this.listView1.Items[isel].Selected = true;
			}
		}
		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			ListView lv = (ListView)sender;
			if (lv.SelectedItems.Count != 1) {
				return;
			}
			if (m_isel != lv.SelectedItems[0].Index) {
				m_isel = lv.SelectedItems[0].Index;
			}
			if (m_i >= m_digi.m_hair.Count) {
				return;
			}
			DIGITI.hair hr = m_digi.m_hair[m_i];
			if (m_isel >= hr.seg.Count()) {
				return;
			}
			if (this.radioButton2.Checked) {
				draw_graph(hr);
			}
			if (this.radioButton4.Checked) {
				draw_image(hr);
			}
			if (this.radioButton7.Checked) {
				if (m_isel < this.listView2.Items.Count) {
					this.listView2.Items[m_isel].Selected = true;
				}
			}
#if true//2018.09.29(キューティクルライン検出)
			if (this.radioButton4.Checked) {
				draw_cuticle(hr);
			}
#endif
#if true//2018.11.02(HSVグラフ)
			draw_hsv(hr);
#endif

		}
#if true//2019.05.22(再測定判定(キューティクル枚数))
#if true//2018.09.29(キューティクルライン検出)
		private void draw_marker(Graphics gr, Brush brs, Point pt, int LEN)
		{
			gr.FillRectangle(brs, pt.X-LEN, pt.Y-LEN, LEN*2+1, LEN*2+1);
		}
#endif
#if true//2018.10.10(毛髪径算出・改造)
#if true//2018.10.27(画面テキスト)
		private void draw_text(Image img, string txt, float fp=60
#if true//2019.03.16(NODATA対応)
			,StringAlignment v_align=StringAlignment.Far
			,StringAlignment h_align=StringAlignment.Far
			,Brush brs = default(Brush)
#endif
			)
#else
		private void draw_text(Image img, string txt)
#endif
		{
			Graphics gr = Graphics.FromImage(img);
#if true//2018.10.27(画面テキスト)
			Font fnt = new Font("Arial", fp);
#else
			Font fnt = new Font("Arial", 60);
#endif
			RectangleF rt = new RectangleF(0, 0, img.Width, img.Height);
			StringFormat sf  = new StringFormat();
#if true//2019.03.16(NODATA対応)
			if (brs == null) {
				brs = Brushes.LimeGreen;
			}
			sf.Alignment     = h_align;
			sf.LineAlignment = v_align;
			gr.DrawString(txt, fnt, brs, rt, sf);
#else
			sf.Alignment = StringAlignment.Far;
			sf.LineAlignment = StringAlignment.Far;
			gr.DrawString(txt, fnt, Brushes.LimeGreen, rt, sf);
#endif
			gr.Dispose();
		}
		private void draw_moudan(DIGITI.hair hr)
		{
			try {
				//---
				int idx = m_isel;
				DIGITI.seg_of_hair seg = hr.seg[idx];
				double moz_kei_max = -1;
				double mou_kei_max = -1;
				double mou_cen_max = -1;
				//---
				this.chart7.Series[0].Points.Clear();
				this.chart7.Series[1].Points.Clear();
				this.chart7.Series[2].Points.Clear();
				this.chart7.Series[3].Points.Clear();
				this.chart7.Series[4].Points.Clear();
				this.chart7.Series[5].Points.Clear();
				//---
				this.chart7.ChartAreas[0].AxisX.Minimum = double.NaN;
				this.chart7.ChartAreas[0].AxisX.Maximum = double.NaN;
				this.chart7.ChartAreas[0].AxisX.Interval = double.NaN;

				if (seg == null) {
					return;
				}
#if true//2018.11.06(毛髄4)
				m_imou = (int)this.numericUpDown2.Value;
				if (m_imou >= seg.moz_inf.Count) {
					m_imou  = seg.moz_inf.Count-1;
				}
#endif
				DIGITI.seg_of_hair seg_bak = seg;
				double		offs = 0;
				double		xmin = 0;
				double[]	ibuf = seg.moz_inf[m_imou].ibf;
				double[]	hbuf = seg.moz_inf[m_imou].iaf;
				int			ic = seg.moz_inf[m_imou].ihc;
				int			il = seg.moz_inf[m_imou].iml;
				int			ir = seg.moz_inf[m_imou].imr;
				int			xmax;
#if true//2018.11.06(毛髄4)
				int			ihl = seg.moz_inf[m_imou].ihl;
				int			ihr = seg.moz_inf[m_imou].ihr;
#endif
				if (true) {
					const
					int GRID = 50;
					xmax = ibuf.Length/2;
					//xmax = ibuf.Length/25;
					if ((xmax % GRID)!= 0) {
						xmax = xmax/GRID + 1;
					}
					else {
						xmax = xmax/GRID;
					}
					xmax *= GRID;
					this.chart7.ChartAreas[0].AxisX.Minimum = -xmax;
					this.chart7.ChartAreas[0].AxisX.Maximum = +xmax;
					this.chart7.ChartAreas[0].AxisX.Interval = GRID;
#if true//2018.11.06(毛髄4)
					this.chart7.ChartAreas[0].AxisX.Interval = GRID*2;
					this.chart7.ChartAreas[0].AxisX.IntervalOffset = -50;
#endif
				}
				for (int i = 0; i < ibuf.Length; i++) {
					int i0;
					double um = G.PX2UM(seg.width, m_digi.m_log_info.pix_pitch, m_digi.m_log_info.zoom);
					double x0 = i-ic;
					this.chart7.Series[0].Points.AddXY(x0, ibuf[i]);
					this.chart7.Series[1].Points.AddXY(x0, hbuf[i]);
				}
				if (true) {
					((TextAnnotation)this.chart7.Annotations[2]).Text = "X.IDX=" + m_imou.ToString();
					string tmp = this.chart7.Annotations[2].ToString();
					this.chart7.ChartAreas[0].AxisY.Minimum = 0;
					this.chart7.ChartAreas[0].AxisY.Maximum = 256;
					this.chart7.ChartAreas[0].AxisY.Interval = 32;
#if true//2018.11.06(毛髄4)
					this.chart7.ChartAreas[0].AxisY.Interval = 64;
#endif
					//
				}
#if true//2018.11.06(毛髄4)
				if (true) {//閾値ライン
					this.chart7.Series[5].Points.AddXY(this.chart7.ChartAreas[0].AxisX.Minimum, G.SS.MOZ_CND_ZVAL);
					this.chart7.Series[5].Points.AddXY(this.chart7.ChartAreas[0].AxisX.Maximum, G.SS.MOZ_CND_ZVAL);
					this.chart7.Series[5].BorderDashStyle = ChartDashStyle.DashDotDot;
					this.chart7.Series[5].Color = Color.Black;
				}
				if (this.checkBox8.Checked) {//中心ライン
					this.chart7.Series[2].Enabled = true;
					this.chart7.Series[2].Points.AddXY(0, 0);
					this.chart7.Series[2].Points.AddXY(0, this.chart7.ChartAreas[0].AxisY.Maximum);
				}
				else {
					this.chart7.Series[2].Enabled = false;
				}
				if (this.checkBox14.Checked) {//判定ライン
					this.chart7.Series[3].Enabled = true;
					this.chart7.Series[4].Enabled = true;
					this.chart7.Series[3].Points.AddXY(ihl-ic, 0);
					this.chart7.Series[3].Points.AddXY(ihl-ic, this.chart7.ChartAreas[0].AxisY.Maximum);
					this.chart7.Series[4].Points.AddXY(ihr-ic, 0);
					this.chart7.Series[4].Points.AddXY(ihr-ic, this.chart7.ChartAreas[0].AxisY.Maximum);
				}
				else {
					this.chart7.Series[3].Enabled = false;
					this.chart7.Series[4].Enabled = false;
				}
#endif
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}
		}
#endif
#if true //2018.12.17(オーバーラップ範囲)
		private void draw_ow_vert_line(DIGITI.seg_of_hair seg, Graphics gr, float pw)
		{
			Pen pen;
			pen = new Pen(Color.LightGray, pw);
			pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
			if (seg.ow_l_pos >= 0) {
				gr.DrawLine(pen, seg.ow_l_pos, 0, seg.ow_l_pos, seg.height);
			}
			if (seg.ow_r_pos >= 0) {
				gr.DrawLine(pen, seg.ow_r_pos, 0, seg.ow_r_pos, seg.height);
			}
			pen.Dispose();
		}
#endif

		private void draw_image(DIGITI.hair hr)
		{
			string buf_dm, buf_ir, buf_pd;
			Image bmp_dm = null, bmp_ir = null, bmp_pd = null;
			Bitmap bmp_all_dm = null, bmp_all_ir = null, bmp_all_pd = null;
			int	Z = 8;
			float pw = 5;
			//---
			int idx = m_isel;
			DIGITI.seg_of_hair seg = hr.seg[idx];
			string zpos, pext;
			ImageList il_dm = null, il_ir = null, il_pd = null;
#if true//2018.09.29(キューティクルライン検出)
			const int CUT_LEN = 7;
#endif
#if true//2018.10.27(画面テキスト)
			double gi_cut_cnt = 0;
			double gi_mou_dia = 0;
			double gi_moz_rsl = 0;
			double gi_moz_rsd = 0;
			int	gi_cnt = 0;
#endif
#if true//2018.10.30(キューティクル長)
			double gi_cut_len = 0;
#endif
			//if (string.Compare(this.comboBox8.Text, "深度合成") == 0) {
			//    zpos = "ZDEPT";
			//    pext = m_fold_of_dept;
			//}
			//else {
			//    zpos = ZVAL2ORG(this.comboBox8.Text);
			//    pext = "";
			//}
			if (this.radioButton3.Checked) {
				bmp_all_dm = new Bitmap((int)(hr.width_of_hair/Z), (int)(hr.height_of_hair/Z));
				bmp_all_ir = (Bitmap)bmp_all_dm.Clone();
				bmp_all_pd = (Bitmap)bmp_all_dm.Clone();
				pw = 15;
				//---
				m_digi.prep_image_list(hr, m_i, ref il_dm, ref il_pd, ref il_ir, this.comboBox10.Text, this.comboBox8.Text, this.comboBox12.Text);
			}
			//---
			DIGITI.dispose_img(this.pictureBox1);
			DIGITI.dispose_img(this.pictureBox2);
			DIGITI.dispose_img(this.pictureBox3);
			//---
			for (int q = 0;; q++) {
				if (this.radioButton3.Checked) {
					//全体表示
					if (q >= hr.seg.Length) {
						break;
					}
					seg = hr.seg[q];
					if (seg == null) {
						continue;
					}
					if (seg.cnt_of_moz <= 0) {
//						continue;
					}
					if (true) {
						bmp_dm = new Bitmap(seg.width / Z, seg.height / Z);
						bmp_pd = new Bitmap(seg.width / Z, seg.height / Z);
					}
					if (!string.IsNullOrEmpty(seg.name_of_ir)) {
						bmp_ir = new Bitmap(seg.width / Z, seg.height / Z);
					}
					Graphics gr;
					if (true) {
						gr = Graphics.FromImage(bmp_dm);
						gr.DrawImage(/*hr.*/il_dm.Images[q], 0, 0, seg.width / Z, seg.height / Z);
						gr.Dispose();
					}
					if (true) {
						gr = Graphics.FromImage(bmp_pd);
						gr.DrawImage(/*hr.*/il_pd.Images[q], 0, 0, seg.width / Z, seg.height / Z);
						gr.Dispose();
					}
					if (!string.IsNullOrEmpty(seg.name_of_ir)) {
						gr = Graphics.FromImage(bmp_ir);
						gr.DrawImage(/*hr.*/il_ir.Images[q], 0, 0, seg.width/Z, seg.height/Z);
						gr.Dispose();
					}
				}
				else {
					//個別表示
					if (true) {
						buf_dm = m_digi.to_xx_path(seg.path_of_dm, m_digi.ZVAL2ORG(this.comboBox10.Text));
						buf_pd = m_digi.to_xx_path(seg.path_of_pd, m_digi.ZVAL2ORG(this.comboBox8.Text));
						buf_ir = m_digi.to_xx_path(seg.path_of_ir, m_digi.ZVAL2ORG(this.comboBox12.Text));
					}
					if (false/*this.checkBox10.Checked*/) {
						if (System.IO.File.Exists(buf_ir)) {
							string tmp;
							tmp = this.textBox1.Text + "\\" + seg.name_of_ir.Replace("IR", "IZ");
							if (System.IO.File.Exists(tmp)) {
								buf_dm = tmp;
							}
						}
					}

					bmp_dm = m_digi.to_img_from_file(buf_dm);
					bmp_pd = m_digi.to_img_from_file(buf_pd);
					bmp_ir = m_digi.to_img_from_file(buf_ir);
				}

				if (
#if true//2019.03.16(NODATA対応)
					seg.bNODATA == false &&
#endif
#if true//2018.10.30(キューティクル長)
#else
					this.checkBox1.Checked &&
#endif
					seg.val_xum.Count > 0) {//断面・ライン
					Graphics gr = Graphics.FromImage(bmp_dm);
					Pen pen = new Pen(Color.Green, 4);
					if (this.radioButton3.Checked) {
						//全体表示
						gr.ScaleTransform(1f/Z, 1f/Z);
					}
#if true//2018.10.30(キューティクル長)
					if (true) {//新方式?
						int YS, YE;
						if (this.checkBox10.Checked) {//マーカー
							YS = 0;
						}
						if (this.checkBox17.Checked) {
							//全ライン描画
							YS = 0;
							YE = seg.cut_inf.Count-1;
						}
						else {
							//選択ライン描画
							int C = seg.cut_inf.Count/2;
							YS = C + (int)this.numericUpDown1.Value;
							if (YS < 0) {
								YS = 0;
							}
							else if (YS >= seg.cut_inf.Count) {
								YS = seg.cut_inf.Count-1;
							}
							YE = YS;
						}
						for (int Y = YS; Y <= YE; Y++) {
							if (this.checkBox1.Checked) {
								pen = new Pen(this.chart1.Series[0].Color, pw);
								gr.DrawLines(pen, seg.cut_inf[Y].pbf.ToArray());
							}
							if (this.checkBox10.Checked) {
							if (seg.cut_inf[Y].iaf != null) {
								for (int i = 0; i < seg.cut_inf[Y].pbf.Count; i++) {
									if (seg.cut_inf[Y].flg[i]
#if true//2018.11.28(メモリリーク)
									!= 0
#endif
										) {
										draw_marker(gr, Brushes.Yellow, (Point)seg.cut_inf[Y].pbf[i], CUT_LEN);
									}
								}
							}
							}
						}
						if (this.checkBox18.Checked) {//連結キューティクル
							for (int i = 0; i < seg.cut_lsp.Count; i++) {
								pen = new Pen(Color.Red, pw);
								gr.DrawLines(pen, seg.cut_lsp[i].ToArray());
							}
						}
					}
#endif
#if true//2018.11.02(HSVグラフ)
					if (this.checkBox19.Checked) {
						pen = new Pen(Color.White, pw);
						gr.DrawLines(pen, seg.his_top);
						gr.DrawLines(pen, seg.his_btm);
					}
#endif
#if true //2018.12.17(オーバーラップ範囲)
					if (this.checkBox20.Checked) {
						draw_ow_vert_line(seg, gr, pw);
					}
#endif
					pen.Dispose();
					gr.Dispose();
				}
#if true//2019.04.17(毛髄検出複線化)
				int imul;
				int rmul = 0;
retry_of_multi:
				if (this.comboBox2.SelectedIndex > 2 || this.comboBox2.SelectedIndex < 0) {
					imul = rmul;
				}
				else {
					imul = this.comboBox2.SelectedIndex;
				}
				switch (imul) {
				case  0: seg.moz_inf = seg.moz_inf1; seg.moz_hnf = seg.moz_hnf1; break;//上側
				case  1: seg.moz_inf = seg.moz_inf2; seg.moz_hnf = seg.moz_hnf2; break;//中心
				default: seg.moz_inf = seg.moz_inf3; seg.moz_hnf = seg.moz_hnf3; break;//下側
				}
#endif
#if true//2019.05.07(毛髄複線面積値対応)
				if (this.comboBox2.SelectedIndex > 2 || this.comboBox2.SelectedIndex < 0) {
					seg.moz_rsl = seg.moz_rsl_mul;
					seg.moz_rsd = seg.moz_rsd_mul;
					seg.moz_hsl = seg.moz_hsl_mul;
					seg.moz_hsd = seg.moz_hsd_mul;
				}
				else {
					switch(imul) {
					case  0:
						seg.moz_rsl = seg.moz_rsl1;
						seg.moz_rsd = seg.moz_rsd1;
						seg.moz_hsl = seg.moz_hsl1;
						seg.moz_hsd = seg.moz_hsd1;
						break;
					case  1:
						seg.moz_rsl = seg.moz_rsl2;
						seg.moz_rsd = seg.moz_rsd2;
						seg.moz_hsl = seg.moz_hsl2;
						seg.moz_hsd = seg.moz_hsd2;
						break;
					default:
						seg.moz_rsl = seg.moz_rsl3;
						seg.moz_rsd = seg.moz_rsd3;
						seg.moz_hsl = seg.moz_hsl3;
						seg.moz_hsd = seg.moz_hsd3;
						break;
					}
				}
#endif
				if (
#if true//2019.03.16(NODATA対応)
					seg.bNODATA == false &&
#endif
					bmp_ir != null && seg.val_xum.Count > 0) {//赤外あり？
//@@@				object obj = seg.moz_zpb[0];
					//System.Diagnostics.Debug.WriteLine(obj);
					Graphics gr_ir = Graphics.FromImage(bmp_ir);
					Graphics gr_pd = Graphics.FromImage(bmp_pd);
					Pen pen = null;
					Point[] ap;
					if (this.radioButton3.Checked) {
						//全体表示
						gr_ir.ScaleTransform(1f/Z, 1f/Z);
					}
					if (this.checkBox2.Checked) {//赤外・輪郭
						pen = new Pen(Color.Blue, pw);
						ap = (Point[])seg.moz_top.ToArray(typeof(Point));
						gr_ir.DrawLines(pen, ap);
						gr_pd.DrawLines(pen, ap);
						//---
						ap = (Point[])seg.moz_btm.ToArray(typeof(Point));
						gr_ir.DrawLines(pen, ap);
						gr_pd.DrawLines(pen, ap);
					}
					if (this.checkBox8.Checked
#if true//2019.05.07(毛髄複線面積値対応)
						&& rmul == 0
#endif
						) {//赤外・中心ライン
						pen = new Pen(this.chart1.Series[0].Color, pw);
						ap = (Point[])seg.pts_cen.ToArray(typeof(Point));
						gr_ir.DrawLines(pen, ap);
						gr_pd.DrawLines(pen, ap);
					}
					if (this.checkBox9.Checked) {//赤外・毛髄径
#if true//2018.10.10(毛髪径算出・改造)
//						Pen pen_d = new Pen(Color.DarkGreen, pw);
						Pen pen_l = new Pen(Color.LightGreen, pw);
#endif
						pen = new Pen(Color.Green, pw);
						for (int i = 0; i <
#if true//2019.04.17(毛髄検出複線化)
							seg.moz_inf.Count
#else
							seg.moz_zpb.Count
#endif
							; i+=1) {
#if true//2019.04.17(毛髄検出複線化)
							Point p1, p2;
							m_digi.chk_pt(seg, i);
							m_digi.get_pt(seg, i, this.checkBox15.Checked, out p1, out p2);
#else
							Point p1 = (Point)seg.moz_zpb[i];
							Point p2 = (Point)seg.moz_zpt[i];
#if true//2018.10.10(毛髪径算出・改造)
							if (this.checkBox15.Checked) {
								p1 = seg.moz_hpb[i];//補間データ
								p2 = seg.moz_hpt[i];
							}
#endif
#endif
							try {
							//if (p1.X != 0 && p1.Y != 0) {
							//    i = i;
							//}
							//if (true) {
							//    gr.DrawLine(Pens.LightGreen,  (Point)seg.moz_btm[i], (Point)seg.moz_top[i]);
							//}
							if (p1.X == 0 && p2.Y == 0) {
							}
							else if (p1.X == p2.X && p1.Y == p2.Y) {
#if _DEBUG
								i = i;
#endif
								//gr.FillRectangle(Brushes.LightGreen, p1.X-3, p1.Y-3, 7, 7);
							}
							else {
#if true//2018.10.10(毛髪径算出・改造)
								if (this.checkBox15.Checked) {
									if (seg.moz_hnf[i].fs2) {
										gr_ir.DrawLine(pen_l, p1, p2);
									}
									else {
										gr_ir.DrawLine(pen, p1, p2);
									}
								}
								else {
									if (seg.moz_inf[i].fs2) {
										gr_ir.DrawLine(pen_l, p1, p2);
									}
									else {
										gr_ir.DrawLine(pen, p1, p2);
									}
								}
#endif
							}
#if true//2018.10.10(毛髪径算出・改造)
							}
							catch (Exception ex) {
System.Diagnostics.Debug.WriteLine(ex.ToString());
							}
#endif
						}
					}
#if true//2018.10.10(毛髪径算出・改造)
					if (this.checkBox13.Checked) {//赤外・外れ判定
						pen = new Pen(Color.Red, pw);
						for (int i = 0; i <
#if true//2019.04.17(毛髄検出複線化)
							seg.moz_inf.Count
#else
							seg.moz_zpb.Count
#endif
							; i++) {
#if true//2019.04.17(毛髄検出複線化)
							m_digi.chk_pt(seg, i);
#endif
							if (seg.moz_inf[i].moz_out) {
								Point p1, p2;
#if true//2019.04.17(毛髄検出複線化)
								m_digi.get_pt(seg, i, this.checkBox15.Checked, out p1, out p2);
#else
								if (this.checkBox15.Checked) {
									p1 = seg.moz_hpb[i];//補間データ
									p2 = seg.moz_hpt[i];
								}
								else {
									p1 = (Point)seg.moz_zpb[i];
									p2 = (Point)seg.moz_zpt[i];
								}
#endif
								gr_ir.DrawLine(pen, p1, p2);
							}
						}
					}
					if (this.checkBox16.Checked) {//赤外・除外域
						pen = new Pen(Color.DarkRed, pw);
						for (int i = 0; i < seg.moz_inf.Count; i++) {
#if true//2019.04.17(毛髄検出複線化)
							m_digi.chk_pt(seg, i);
#endif
							if (seg.moz_inf[i].moz_lbl<0) {
								Point p1, p2;
#if true//2019.04.17(毛髄検出複線化)
								m_digi.get_pt(seg, i, false, out p1, out p2);
#else
								p1 = (Point)seg.moz_zpb[i];
								p2 = (Point)seg.moz_zpt[i];
#endif
								gr_ir.DrawLine(pen, p1, p2);
							}
						}
					}
					if (this.checkBox14.Checked) {//赤外・判定範囲
						pen = new Pen(Color.Purple, pw);
						gr_ir.DrawLines(pen, seg.han_top);
						gr_ir.DrawLines(pen, seg.han_btm);
					}
#endif
#if true//2018.11.02(HSVグラフ)
					if (this.checkBox19.Checked) {
						pen = new Pen(Color.White, pw);
						gr_pd.DrawLines(pen, seg.his_top);
						gr_pd.DrawLines(pen, seg.his_btm);
						gr_ir.DrawLines(pen, seg.his_top);
						gr_ir.DrawLines(pen, seg.his_btm);
					}
#endif
#if true //2018.12.17(オーバーラップ範囲)
					if (this.checkBox20.Checked) {
						draw_ow_vert_line(seg, gr_ir, pw);
						draw_ow_vert_line(seg, gr_pd, pw);
					}
#endif
					if (pen != null) {
						pen.Dispose();
					}
					gr_pd.Dispose();
					gr_ir.Dispose();
				}

#if true//2019.04.17(毛髄検出複線化)
				if (this.comboBox2.SelectedIndex > 2 || this.comboBox2.SelectedIndex < 0) {
#if true//2019.05.07(毛髄複線面積値対応)
					switch (rmul) {//上側→下側→中心の順に描画
						case 0: rmul = 2; goto retry_of_multi;
						case 2: rmul = 1; goto retry_of_multi;
						default: break;
					}

#else
					if (++rmul < 3) {
						goto retry_of_multi;
					}
#endif
				}
#endif


#if true//2018.10.30(キューティクル長)
				gi_cut_len += seg.cut_ttl;
#endif
#if true//2018.10.27(画面テキスト)
#if true//2018.11.13(毛髪中心AF)
				if (seg.pts_cen_cut != null) {
				gi_cut_cnt += seg.pts_cen_cut.Count;
				}
#else
				gi_cut_cnt += seg.pts_cen_cut.Count;
#endif
				gi_mou_dia += seg.dia_avg;
				if (this.checkBox15.Checked) {//補間データ
#if true//2019.05.07(毛髄複線面積値対応)
				gi_moz_rsl += seg.moz_hsl;
				gi_moz_rsd += seg.moz_hsd;
#else
				gi_moz_rsl += seg.moz_hsl;
				gi_moz_rsd += seg.moz_hsd;
#endif
				}
				else {
				gi_moz_rsl += seg.moz_rsl;
				gi_moz_rsd += seg.moz_rsd;
				}
				gi_cnt++;
#endif
				if (!this.radioButton3.Checked) {
					break;
				}
				if (true) {
					Graphics gr;
					gr = Graphics.FromImage(bmp_all_dm);
					gr.DrawImage(bmp_dm, (seg.pix_pos.X/Z), (seg.pix_pos.Y/Z), seg.width/Z, seg.height/Z);
					gr.Dispose();
				}
				if (bmp_ir != null) {
					Graphics gr;
					gr = Graphics.FromImage(bmp_all_ir);
					gr.DrawImage(bmp_ir, (seg.pix_pos.X/Z), (seg.pix_pos.Y/Z), seg.width/Z, seg.height/Z);
					gr.Dispose();
				}
				if (bmp_pd != null) {
					Graphics gr;
					gr = Graphics.FromImage(bmp_all_pd);
					gr.DrawImage(bmp_pd, (seg.pix_pos.X / Z), (seg.pix_pos.Y / Z), seg.width / Z, seg.height / Z);
					gr.Dispose();
				}
				DIGITI.dispose_bmp(ref bmp_dm);
				DIGITI.dispose_bmp(ref bmp_ir);
				DIGITI.dispose_bmp(ref bmp_pd);
			}
#if true//2018.10.27(画面テキスト)
			if (gi_cnt > 1) {
				gi_mou_dia /= gi_cnt;
			}
#endif
			if (this.radioButton3.Checked) {
#if true//2018.10.27(画面テキスト)
				if (true) {
#if true//2018.10.30(キューティクル長)
					draw_text(bmp_all_dm, string.Format("キューティクル枚数={0:F0}\r\nキューティクル長={1:F0}um", gi_cut_cnt, gi_cut_len), 24);
#else
					draw_text(bmp_all_dm, string.Format("キューティクル枚数={0:F0}", gi_cut_cnt), 24);
#endif
					draw_text(bmp_all_pd, string.Format("直径={0:F1}um", gi_mou_dia), 24);
				}
#endif
				if (true) {
					this.pictureBox1.Image = bmp_all_dm;
					this.pictureBox3.Image = bmp_all_pd;
				}
				if (bmp_all_ir != null) {
#if true//2018.10.27(画面テキスト)
					draw_text(bmp_all_ir, string.Format("Sl={0:F1}, Sd={1:F1} [um\u00b2]", gi_moz_rsl, gi_moz_rsd), 24);
#endif
					this.pictureBox2.Image = bmp_all_ir;
				}
			}
			else {
				if (true) {
#if true//2018.10.27(画面テキスト)
#if true//2018.10.30(キューティクル長)
					draw_text(bmp_dm, string.Format("キューティクル枚数={0:F0}\r\nキューティクル長={1:F0}um", gi_cut_cnt, gi_cut_len));
#else
					draw_text(bmp_dm, string.Format("キューティクル枚数={0:F0}\r\ntest", gi_cut_cnt));
#endif
					draw_text(bmp_pd, string.Format("直径={0:F1}um", gi_mou_dia));
#endif
#if true//2019.03.16(NODATA対応)
					if (this.checkBox21.Checked) {
					draw_text(bmp_dm, string.Format("CONTRAST={0:F3}, AVG={1:F3}, DROP={2:F1}%", seg.contr, seg.contr_avg, seg.contr_drop), 60, StringAlignment.Near, StringAlignment.Near);
					}
					if (seg.bNODATA) {
					draw_text(bmp_dm, "NO DATA", 60, StringAlignment.Far, StringAlignment.Near, Brushes.Red);
					draw_text(bmp_pd, "NO DATA", 60, StringAlignment.Far, StringAlignment.Near, Brushes.Red);
					draw_text(bmp_ir, "NO DATA", 60, StringAlignment.Far, StringAlignment.Near, Brushes.Red);
					}
#endif
					this.pictureBox1.Image = bmp_dm;
					this.pictureBox3.Image = bmp_pd;
				}
				if (bmp_ir != null) {
#if true//2018.10.27(画面テキスト)
					draw_text(bmp_ir, string.Format("Sl={0:F1}, Sd={1:F1} [um\u00b2]", gi_moz_rsl, gi_moz_rsd));
#else
#if true//2018.10.10(毛髪径算出・改造)
					if (true) {
						//Graphics gr = Graphics.FromImage(bmp_ir);
						//Font fnt = new Font("Arial", 60);
						//RectangleF rt = new RectangleF(0, 0, bmp_ir.Width, bmp_ir.Height);
						//StringFormat sf  = new StringFormat();
						string buf;
						if (this.checkBox15.Checked) {//補間データ
							buf = string.Format("Sl={0:F1}, Sd={1:F1} [um\u00b2]", seg.moz_hsl, seg.moz_hsd);
						}
						else {
							buf = string.Format("Sl={0:F1}, Sd={1:F1} [um\u00b2]", seg.moz_rsl, seg.moz_rsd);
						}
						//sf.Alignment = StringAlignment.Far;
						//sf.LineAlignment = StringAlignment.Far;

						//gr.DrawString(buf, fnt, Brushes.LimeGreen, rt, sf);
						//gr.Dispose();
						draw_text(bmp_ir, buf);
					}
#endif
#endif
					this.pictureBox2.Image = bmp_ir;
				}
			}
		}
#endif
#if true //2018.12.17(オーバーラップ範囲)
		private void draw_graph_ow_line(Chart cht, int idx, double offs, DIGITI.seg_of_hair seg)
		{
			int	q0 = idx;
			int q1 = idx+1;
			if (this.checkBox20.Checked == false || this.radioButton1.Checked) {
				cht.Series[q0].Enabled = false;
				cht.Series[q1].Enabled = false;
				return;
			}
			if (seg.ow_l_pos < 0) {
				cht.Series[q0].Enabled = false;
			}
			else {
				cht.Series[q0].Enabled = true;
				cht.Series[q0].Points.Clear();
				cht.Series[q0].Points.AddXY(offs+seg.ow_l_xum,-999);
				cht.Series[q0].Points.AddXY(offs+seg.ow_l_xum,+999);
			}
			if (seg.ow_r_pos < 0) {
				cht.Series[q1].Enabled = false;
			}
			else {
				cht.Series[q1].Enabled = true;
				cht.Series[q1].Points.Clear();
				cht.Series[q1].Points.AddXY(offs+seg.ow_r_xum,-999);
				cht.Series[q1].Points.AddXY(offs+seg.ow_r_xum,+999);
			}
		}
#endif
		private void draw_graph(DIGITI.hair hr)
		{
			try {
			//---
			int idx = m_isel;
			DIGITI.seg_of_hair seg = (DIGITI.seg_of_hair)hr.seg[idx];
			double moz_kei_max = -1;
#if true//2018.08.21
			double mou_kei_max = -1;
#endif
#if true//2018.10.10(毛髪径算出・改造)
			double mou_cen_max = -1;
#endif
#if true//2019.01.05(キューティクル検出欠損修正)
			int YC;
#endif
			//---
			this.chart1.Series[0].Points.Clear();
			this.chart2.Series[0].Points.Clear();
#if false//2018.08.21
			this.chart2.Series[1].Points.Clear();
#else
			this.chart3.Series[0].Points.Clear();
#endif
#if true//2018.10.10(毛髪径算出・改造)
			this.chart2.Series[1].Points.Clear();
			this.chart6.Series[0].Points.Clear();
#endif
			this.chart1.ChartAreas[0].AxisX.Minimum = 0;
			this.chart1.ChartAreas[0].AxisX.Maximum = double.NaN;
			this.chart1.ChartAreas[0].AxisX.Interval = double.NaN;
			this.chart2.ChartAreas[0].AxisX.Minimum = 0;
			this.chart2.ChartAreas[0].AxisX.Maximum = double.NaN;
			this.chart2.ChartAreas[0].AxisX.Interval = double.NaN;
#if true//2018.08.21
			this.chart3.ChartAreas[0].AxisX.Minimum = 0;
			this.chart3.ChartAreas[0].AxisX.Maximum = double.NaN;
			this.chart3.ChartAreas[0].AxisX.Interval = double.NaN;
#endif
			if (seg == null) {
				return;
			}
			DIGITI.seg_of_hair seg_bak = seg;
			double		offs = 0;
			double		xmin = 0;

			if (this.radioButton1.Checked) {//グラフ・全体
				double um_of_width = G.PX2UM(seg.width, m_digi.m_log_info.pix_pitch, m_digi.m_log_info.zoom);
				for (int q = 0;; q++) {
					if (q >= hr.seg.Length) {
						return;
					}
					seg = hr.seg[q];
					if (seg != null && seg.val_xum.Count > 0) {
						break;
					}
					offs -= um_of_width;
				}
				xmin = offs;
			}
			else {
				if (
#if true//2019.03.16(NODATA対応)
					seg.bNODATA == true ||
#endif
					seg.val_xum.Count <= 0) {
					return;
				}
			}

			for (int q = 0;; q++) {
				int i0;
				if (this.radioButton1.Checked) {
					if (q >= hr.seg.Length) {
						break;
					}
					seg = hr.seg[q];
					if (seg == null) {
						continue;
					}
					if (seg.val_xum.Count <= 0) {
						double um = G.PX2UM(seg.width, m_digi.m_log_info.pix_pitch, m_digi.m_log_info.zoom);
						double x0 = um+ offs;
						this.chart1.Series[0].Points.AddXY(x0, double.NaN);
						this.chart2.Series[0].Points.AddXY(x0, double.NaN);
#if false//2018.08.21
						this.chart2.Series[1].Points.AddXY(x0, double.NaN);
#else
						this.chart3.Series[0].Points.AddXY(x0, double.NaN);
#endif
						offs += um;
						continue;
					}
					for (i0 = 0; m_digi.TO_VAL(seg.val_xum[i0]) < 0; i0++) {
					}
					if (m_chk1!=0) {
						i0 = 0;
					}
				}
				else {
					i0 = 0;
					offs = -m_digi.TO_VAL(seg.val_xum[0]);
				}
#if true//2019.03.16(NODATA対応)
				if (seg.bNODATA) {
					this.chart1.Series[0].Points.AddXY(double.NaN, double.NaN);
					this.chart2.Series[0].Points.AddXY(double.NaN, double.NaN);
					this.chart3.Series[0].Points.AddXY(double.NaN, double.NaN);
					goto skip;
				}
#endif
#if true//2019.01.05(キューティクル検出欠損修正)
				if (true) {
					YC = seg.cut_inf.Count/2 + (int)this.numericUpDown1.Value;
					if (YC < 0) {
						YC = 0;
					}
					else if (YC >= seg.cut_inf.Count) {
						YC = seg.cut_inf.Count-1;
					}
				}
#endif
				for (int i = i0; i < seg.val_xum.Count; i++) {
					double x0 = m_digi.TO_VAL(seg.val_xum[i]) + offs;
#if true//2018.11.28(メモリリーク)
					double y0 = double.NaN;
					double y1 = double.NaN;
#else
					double y0 = TO_VAL(seg.val_p5u[i]);
					double y1 = TO_VAL(seg.val_phf[i]);
#endif
#if true//2019.01.05(キューティクル検出欠損修正)
					double y2;
#else
					double y2 = TO_VAL(seg.val_cen[i]);
#endif
#if true//2018.11.28(メモリリーク)
					double y3 = double.NaN;
					double y4 = double.NaN;
#else
					double y3 = TO_VAL(seg.val_mph[i]);
					double y4 = TO_VAL(seg.val_m5u[i]);
#endif
#if true//2018.12.25(オーバーラップ範囲改)
					if ((double)seg.val_xum[i] < seg.ow_l_xum || (double)seg.val_xum[i] > seg.ow_r_xum) {
						continue;
					}
#endif
					//double y5 = TO_VAL(seg.moz_zpl[i]);
					if (this.checkBox3.Checked) {//R*0
#if true//2019.01.05(キューティクル検出欠損修正)
						y2 = seg.cut_inf[YC].ibf[i];
#if DEBUG
						if (this.numericUpDown1.Value == 0 && y2 != m_digi.TO_VAL(seg.val_cen[i])) {
							double tmp = m_digi.TO_VAL(seg.val_cen[i]);
#if false//2019.02.16(数値化白髪オフセット)
							throw new Exception("Internal Error");
#endif
						}
#endif
#endif
						this.chart1.Series[0].Points.AddXY(x0, y2);
					}
				}
				for (int i = i0; i <
#if true//2019.04.17(毛髄検出複線化)
					seg.moz_inf.Count
#else
					seg.moz_zpl.Count
#endif
					; i++) {
					double x0 = m_digi.TO_VAL(seg.val_xum[i]) + offs;
#if true//2019.04.17(毛髄検出複線化)
					double y5 = seg.moz_inf[i].moz_zpl;
#else
					double y5 = TO_VAL(seg.moz_zpl[i]);
#endif
					double y6 = m_digi.TO_VAL(seg.mou_len[i]);
#if true//2018.12.25(オーバーラップ範囲改)
					if ((double)seg.val_xum[i] < seg.ow_l_xum || (double)seg.val_xum[i] > seg.ow_r_xum) {
						continue;
					}
#endif

					if (this.checkBox11.Checked) {
#if true//2018.10.10(毛髪径算出・改造)
						//赤外・外れ判定
#if true//2018.10.10(毛髪径算出・改造)
#if true//2019.04.17(毛髄検出複線化)
						m_digi.get_pl(seg, i, this.checkBox15.Checked, out y5);
#else
						if (this.checkBox15.Checked) {
							y5 = seg.moz_hpl[i];//補間データ
						}
#endif
#endif
						if (this.checkBox13.Checked/* && seg.moz_out != null*/ && seg.moz_inf[i].moz_out) {
							this.chart2.Series[0].Points.Add(dp_marker(x0, y5, Color.Red));
						}
						else {
							this.chart2.Series[0].Points.AddXY(x0, y5);
						}
#else
						this.chart2.Series[0].Points.AddXY(x0, y5);
#endif
						if (moz_kei_max < y5) {
							moz_kei_max = y5;
						}
					}
#if true//2018.10.10(毛髪径算出・改造)
					if (true) {
						if (i == 513 || i == 41) {
							i = i;
						}
						try {
						if (y5 > 0) {
							int ic = seg.moz_inf[i].ihc;
							int il = seg.moz_inf[i].iml;
							int ir = seg.moz_inf[i].imr;
							Point[] pf = seg.moz_inf[i].pbf;
							//if (pf == null || ic >= pf.Length || il >= pf.Length || ir >= pf.Length) {
							//    ic = ic;
							//}
							Point pc = pf[ic];//毛髪中心
							Point ml = pf[il];//毛髄左端
							Point mr = pf[ir];//毛髄右端
							//毛髄中心
							PointF mc = new PointF((ml.X+mr.X)/2f, (ml.Y+mr.Y)/2f);
							double df = m_digi.px2um(mc, pc);
							if (mc.Y > pc.Y) {
								df=-df;
							}
							if (df != seg.moz_inf[i].ddf) {
								df = df;
							}
							if (this.checkBox15.Checked) {
								df = seg.moz_hnf[i].ddf;//補間データ
							}
							if (this.checkBox13.Checked && seg.moz_inf[i].moz_out) {
								this.chart6.Series[0].Points.Add(dp_marker(x0, df, Color.Red));
							}
							else {
								this.chart6.Series[0].Points.AddXY(x0, df);
							}
							if (mou_cen_max < Math.Abs(df)) {
								mou_cen_max = Math.Abs(df);
							}
						}
						}
						catch (Exception ex) {
							G.mlog(ex.ToString());
						}
					}
#endif
					if (this.checkBox12.Checked) {
#if false//2018.08.21
						this.chart2.Series[1].Points.AddXY(x0, y6);
						if (moz_kei_max < y6) {
							moz_kei_max = y6;
						}
#else
						this.chart3.Series[0].Points.AddXY(x0, y6);
						if (mou_kei_max < y6) {
							mou_kei_max = y6;
						}
#endif
					}
				}
				if (!this.radioButton1.Checked) {
					break;
				}
#if true//2019.03.16(NODATA対応)
skip:
#endif
				double	dx = m_digi.TO_VAL(seg.val_xum[1])-m_digi.TO_VAL(seg.val_xum[0]);
				//offs += dx * seg.moz_zpl.Count;
				offs += m_digi.TO_VAL(seg.val_xum[seg.val_xum.Count-1])+dx;
			}
			if (this.radioButton1.Checked) {
				seg = seg_bak;
			}
#if true //2018.12.17(オーバーラップ範囲)
			if (true) {
				draw_graph_ow_line(this.chart1, 1, offs, seg);//キューティクル断面
				draw_graph_ow_line(this.chart4, 2, offs, seg);//キューティクルライン
				draw_graph_ow_line(this.chart3, 1, offs, seg);//毛髪径
				draw_graph_ow_line(this.chart2, 2, offs, seg);//毛髄径
				draw_graph_ow_line(this.chart6, 1, offs, seg);//毛髄中心
			}
#endif
			if (true) {
				this.chart1.Series[0].Color = Color.Cyan;		//R*0
				this.chart2.Series[0].Color = Color.Green;	//毛髄径
				//---
				this.chart1.Series[0].Enabled = this.checkBox3.Checked;
				this.chart2.Series[0].Enabled = this.checkBox11.Checked;
				this.chart3.Series[0].Enabled = this.checkBox12.Checked;
				//---
			}


			if (true) {
				//---
				this.chart1.ChartAreas[0].RecalculateAxesScale();
				//---
				double fmin = this.chart1.ChartAreas[0].AxisX.Minimum;
				double fmax = this.chart1.ChartAreas[0].AxisX.Maximum;//TO_VAL(seg.val_xum[seg.val_xum.Count-1]);

				this.chart1.ChartAreas[0].AxisY.Minimum = 0;
				this.chart1.ChartAreas[0].AxisY.Maximum = 256;
				this.chart1.ChartAreas[0].AxisY.Interval = 32;
				//
				double tmp;
				//tmp = this.chart1.ChartAreas[0].AxisX.Minimum;
				this.chart1.ChartAreas[0].AxisX.Minimum = xmin;
				this.chart1.ChartAreas[0].AxisX.IntervalOffset = -xmin;
				this.chart2.ChartAreas[0].AxisX.Minimum = xmin;
				this.chart2.ChartAreas[0].AxisX.IntervalOffset = -xmin;
				//---
				if (moz_kei_max < 50) {
				this.chart2.ChartAreas[0].AxisY.Maximum = 50;
				this.chart2.ChartAreas[0].AxisY.Interval =10;
				}
				else if (moz_kei_max < 100) {
				this.chart2.ChartAreas[0].AxisY.Maximum = 100;
				this.chart2.ChartAreas[0].AxisY.Interval =25;
				}
				else if (moz_kei_max < 125) {
				this.chart2.ChartAreas[0].AxisY.Maximum = 125;
				this.chart2.ChartAreas[0].AxisY.Interval =25;
				}
				else if (moz_kei_max < 150) {
				this.chart2.ChartAreas[0].AxisY.Maximum = 150;
				this.chart2.ChartAreas[0].AxisY.Interval =25;
				}
				else {
				this.chart2.ChartAreas[0].AxisY.Maximum = Math.Ceiling(moz_kei_max);
				this.chart2.ChartAreas[0].AxisY.Interval =25;
				}
				this.chart2.ChartAreas[0].AxisY.Minimum = 0;
				//this.chart2.ChartAreas[0].AxisY.Interval = 25;
				//
				//this.chart2.ChartAreas[0].AxisX.Minimum = 0;
//				this.chart2.ChartAreas[0].AxisX.Maximum = Math.Ceiling(fmax);
//				this.chart2.ChartAreas[0].AxisX.Interval = 100;
#if true//2018.08.21
				this.chart3.ChartAreas[0].AxisX.Minimum = xmin;
				this.chart3.ChartAreas[0].AxisX.IntervalOffset = -xmin;
				//---
				if (mou_kei_max < 50) {
					this.chart3.ChartAreas[0].AxisY.Maximum = 50;
					this.chart3.ChartAreas[0].AxisY.Interval = 10;
				}
				else if (mou_kei_max < 100) {
					this.chart3.ChartAreas[0].AxisY.Maximum = 100;
					this.chart3.ChartAreas[0].AxisY.Interval = 25;
				}
				else if (mou_kei_max < 125) {
					this.chart3.ChartAreas[0].AxisY.Maximum = 125;
					this.chart3.ChartAreas[0].AxisY.Interval = 25;
				}
				else if (mou_kei_max < 150) {
					this.chart3.ChartAreas[0].AxisY.Maximum = 150;
					this.chart3.ChartAreas[0].AxisY.Interval = 25;
				}
				else {
					this.chart3.ChartAreas[0].AxisY.Maximum = Math.Ceiling(mou_kei_max);
					this.chart3.ChartAreas[0].AxisY.Interval = 25;
				}
				this.chart3.ChartAreas[0].AxisY.Minimum = 0;
#endif
				//this.chart1.ChartAreas[0].AxisX.IsMarginVisible = false;
				int interval;
				fmax = this.chart1.ChartAreas[0].AxisX.Maximum;
				if (fmax < 300) {
					interval = 50;
				}
				else if (fmax < 500) {
					interval = 100;
				}
				else if (fmax < 1000) {
					interval = 200;
				}
				else if (fmax < 3000) {
					interval = 500;
				}
				else {
					interval = 1000;
				}
				this.chart1.ChartAreas[0].AxisX.Interval = interval;
				this.chart2.ChartAreas[0].AxisX.Interval = interval;
#if true//2018.08.21
				this.chart3.ChartAreas[0].AxisX.Interval = interval;
#endif
#if true//2018.10.10(毛髪径算出・改造)
				this.chart6.ChartAreas[0].AxisX.Minimum = xmin;
				this.chart6.ChartAreas[0].AxisX.IntervalOffset = -xmin;
				this.chart6.ChartAreas[0].AxisX.Interval = interval;
				this.chart6.ChartAreas[0].AxisX.Maximum = fmax;
				if (mou_cen_max < 50) {
					this.chart6.ChartAreas[0].AxisY.Maximum = 50;
					this.chart6.ChartAreas[0].AxisY.Minimum =-50;
					this.chart6.ChartAreas[0].AxisY.Interval = 10;
				}
				else if (mou_cen_max < 100) {
					this.chart6.ChartAreas[0].AxisY.Maximum = 100;
					this.chart6.ChartAreas[0].AxisY.Minimum =-100;
					this.chart6.ChartAreas[0].AxisY.Interval = 25;
				}
				else if (mou_cen_max < 125) {
					this.chart6.ChartAreas[0].AxisY.Maximum = 125;
					this.chart6.ChartAreas[0].AxisY.Maximum = 125;
					this.chart6.ChartAreas[0].AxisY.Interval = 25;
				}
				else if (mou_cen_max < 150) {
					this.chart6.ChartAreas[0].AxisY.Maximum = 150;
					this.chart6.ChartAreas[0].AxisY.Maximum = 150;
					this.chart6.ChartAreas[0].AxisY.Interval = 25;
				}
				else {
					this.chart6.ChartAreas[0].AxisY.Maximum = Math.Ceiling(mou_cen_max);
					this.chart6.ChartAreas[0].AxisY.Maximum = Math.Ceiling(mou_cen_max);
					this.chart6.ChartAreas[0].AxisY.Interval = 25;
				}
#endif
#if true//2018.12.25(オーバーラップ範囲改)
				fmax = m_digi.TO_VAL(seg.val_xum[seg.val_xum.Count-1])+offs;
				this.chart1.ChartAreas[0].AxisX.Maximum = fmax;//キューティクル断面
				this.chart2.ChartAreas[0].AxisX.Maximum = fmax;//毛髄径
				this.chart3.ChartAreas[0].AxisX.Maximum = fmax;//毛髪径
				this.chart6.ChartAreas[0].AxisX.Maximum = fmax;//毛髄中心
#endif
			}
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}
		}

		public int firstVisible(ListView lv)
		{
			int i = 0;
			try {
				while (i < lv.Items.Count) {
					Rectangle rt = lv.GetItemRect(i);
					if (rt.X >= 0) {
						if (rt.X > 0) {
							return(i-1);
						}
						return(i);
					}
					i++;
				}
			}
			catch
			{
				return 0;
			}
			return(0);
		}
		private void radioButton7_Click(object sender, EventArgs e)
		{
			try {
				if (this.radioButton7.Checked && this.listView1.Visible == false && this.radioButton7.Enabled) {
					// 下部リストビューにカラー画像一覧を表示
					this.listView1.Visible = true;
					int idx = firstVisible(this.listView2);
					this.listView1.Items[this.listView1.Items.Count-1].EnsureVisible();
					this.listView1.Items[idx].EnsureVisible();
					this.listView2.Visible = false;
				}
				if (this.radioButton8.Checked && this.listView2.Visible == false && this.radioButton8.Enabled) {
					// 下部リストビューに赤外画像一覧を表示
					this.listView2.Visible = true;
					int idx = firstVisible(this.listView1);
					this.listView2.Items[this.listView2.Items.Count-1].EnsureVisible();
					this.listView2.Items[idx].EnsureVisible();
					this.listView1.Visible = false;
				}
			}
			catch (Exception ex) {
			}
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			if (this.listView1.Items.Count <= 0) {
				return;
			}
			if (m_i >= m_digi.m_hair.Count) {
				return;
			}
			DIGITI.hair hr = m_digi.m_hair[m_i];
			if (m_isel >= hr.seg.Count()) {
				return;
			}
			int q = 0;
			//ここでグラフ更新
			if (sender == this.radioButton3) {
				q |= 1;//画像ファイル
			}
			if (sender == this.radioButton1) {
				q |= 2;//グラフ
			}
			if (sender == this.checkBox1) {//カラー断面ライン
				q |= 1;
			}
			if (sender == this.checkBox2 || sender == this.checkBox8 || sender == this.checkBox9) {
				//赤外・輪郭, 中心ライン, 毛髄径
				q |= 1;
			}
#if true//2018.10.10(毛髪径算出・改造)
			if (sender == this.checkBox13 || sender == this.checkBox16) {
				//赤外・外れ値, 判定範囲
				q |= 1|2;//画像ファイル と グラフ
			}
			if (sender == this.checkBox14) {
				//判定範囲
				q |= 1;//画像ファイル
			}
			if (sender == this.checkBox15) {
				//生データ
				q |= 1|2;//画像ファイル と グラフ
			}
#endif
			if (sender == this.checkBox3 || sender == this.checkBox4
			 || sender == this.checkBox5 || sender == this.checkBox6 || sender == this.checkBox7) {
				//R*0,R*+50%,,R-?um
				q |= 2;
				if (this.checkBox1.Checked) {
				q |= 1;
				}
			}
			if (sender == this.checkBox11 || sender == this.checkBox12) {
				q |= 2;
			}
#if true//2018.10.30(キューティクル長)
			if (sender == this.checkBox10 || sender == this.numericUpDown1 || sender == this.checkBox17) {
				q |= 1|2;//画像ファイル と グラフ
			}
			if (sender == this.checkBox18) {//連結キューティクル
				q |= 1;//画像ファイル
			}
#endif
#if true//2018.11.02(HSVグラフ)
			if (sender == this.checkBox19) {
				q |= 1;
			}
#endif
#if true //2018.12.17(オーバーラップ範囲)
			if (sender == this.checkBox20) {
				q |= 1|2;//画像ファイル と グラフ
			}
#endif
#if true//2019.03.16(NODATA対応)
			if (sender == this.checkBox21) {
				q |= 1;//画像ファイル
			}
#endif
#if true//2019.04.17(毛髄検出複線化)
			if (sender == this.comboBox2) {
				q |= 1|2;//画像ファイル と グラフ
			}
#endif
			//this.button1.Visible = !this.button1.Visible;
			if ((q & 1) != 0) {
				draw_image(hr);
			}
			if ((q & 2) != 0) {
				draw_graph(hr);
			}
#if true//2018.09.29(キューティクルライン検出)
			if ((q & 2) != 0) {
				draw_cuticle(hr);
			}
#endif
#if true//2018.11.02(HSVグラフ)
			if ((q & 2) != 0) {
				draw_hsv(hr);
			}
#endif
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{//対象毛髪の変更
			if (this.comboBox1.Enabled == false) {
				return;
			}
			if (m_i == this.comboBox1.SelectedIndex) {
				return;
			}
			//毛髪選択の変更
			this.listView1.Enabled = this.listView2.Enabled = false;
			this.listView1.Items.Clear();
			this.listView2.Items.Clear();
			//---
			m_i = this.comboBox1.SelectedIndex;
			this.radioButton2.Checked = true;//画像ファイル毎
			this.radioButton4.Checked = true;//画像ファイル毎
			if (true/*m_hair.Count > 0*/) {
				//---
				DIGITI.hair hr = m_digi.m_hair[m_i];
				this.listView1.LargeImageList = hr.il_dm;
				try {
				this.listView2.LargeImageList = hr.il_ir;
				}
				catch (Exception ex) {
					G.mlog(ex.ToString());
				}
				for (int i = 0; i < hr.cnt_of_seg; i++) {
					DIGITI.seg_of_hair seg = hr.seg[i];
					this.listView1.Items.Add(seg.name_of_dm, i);
					if (!string.IsNullOrEmpty(seg.name_of_ir)) {
					this.listView2.Items.Add(seg.name_of_ir, i);
					}
				}

				if (hr.seg[0].name_of_ir.Length <= 0) {
					this.radioButton7.Enabled = false;
					this.radioButton7.Checked = true;
					this.radioButton7.Enabled = true;
					this.radioButton8.Enabled = false;
					this.radioButton8.BackColor = Color.FromArgb(64,64,64);
				}
				else {
					this.radioButton8.Enabled = true;
					this.radioButton8.BackColor = Color.Black;
				}
				int isel = hr.cnt_of_seg / 2;
				if (this.radioButton7.Checked) {
				this.listView1.Items[isel].Selected = true;
				this.listView1.Items[isel].EnsureVisible();
				}
				else {
				this.listView2.Items[isel].Selected = true;
				this.listView2.Items[isel].EnsureVisible();
				}
			}
			this.listView1.Enabled = this.listView2.Enabled = true;
		}
#if true//2019.01.09(保存機能修正)
		private string I2S(int i)
		{
			return(i.ToString());
		}
		private string F2S(double f)
		{
			return(string.Format("{0:F2}", f));
		}
		private string F1S(double f)
		{
			return(string.Format("{0:F1}", f));
		}
		private string F0S(double f)
		{
			return(string.Format("{0:F0}", f));
		}
		private void button3_Click(object sender, EventArgs e)
		{
			try {
				Form25 frm = new Form25();
				DIGITI.hair hr = m_digi.m_hair[m_i];
				int i_s, i_e;
				List<string> i_no = new List<string>();
				string h_no = "";
				DIGITI.seg_of_hair seg;
				string fold;
				//0CR_03_ZP02D
				for (int q = 0; q < hr.seg.Length; q++) {
					seg = hr.seg[q];
					string name = seg.name_of_dm;
					if (q == 0) {
						if (name[1] >= '0' && name[1] <= '9') {
							h_no = name.Substring(0, 2);
						}
						else {
							h_no = name.Substring(0, 1);
						}
					}
					int p = name.IndexOf('_');
					if (p < 0) {
						return;
					}
					i_no.Add(name.Substring(p+1, 2));
				}
				frm.i_no = i_no.ToArray();
				frm.h_no = h_no;
				frm.i_sel = m_isel;
				if (frm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
					return;
				}
				if (G.SS.MOZ_SAV_DMOD == 0) {
					fold = G.SS.MOZ_CND_FOLD;
				}
				else {
					fold = G.SS.MOZ_SAV_FOLD;
				}
				if (fold[fold.Length-1] != '\\') {
					fold += "\\";
				}
				if (G.SS.MOZ_SAV_FMOD == 0) {
					//現在の毛髪
					i_s = 0;
					i_e = hr.cnt_of_seg-1;
				}
				else {
					//現在のグラフ
					i_s = i_e = m_isel;
				}
				CSV csv = new CSV();
				int r = 0, c;
				double dx;
				//List<string> lbuf = new List<string>();
				string path;
				//double offs;
				//int q = 0;
				seg = hr.seg[0/*q*/];
				//offs = -TO_VAL(seg.val_xum[0]);
				dx = m_digi.TO_VAL(seg.val_xum[1])-m_digi.TO_VAL(seg.val_xum[0]);
				path = fold;
				path += G.SS.MOZ_SAV_NAME;
				path += "_";
				path += h_no;
				if (G.SS.MOZ_SAV_FMOD != 0) {
				path += "_";
				path += i_no[m_isel];
				}
				path += ".csv";
				//StreamWriter wr;
				//wr = new StreamWriter(path, false, Encoding.Default);
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r+0, "**********基本情報**********");
					csv.set(0, r+1, "フォルダ");
					csv.set(0, r+2, "ファイル/キューティクル");
					csv.set(0, r+3, "ファイル/径");
					csv.set(0, r+4, "ファイル/赤外");
					csv.set(0, r+5, "キューティクル枚数");
					csv.set(0, r+6, "キューティクル長[um]");
					csv.set(0, r+7, "直径[um]");
					csv.set(0, r+8, "毛髄面積・明[um^2]");
					csv.set(0, r+9, "毛髄面積・暗[um^2]");
				}
				c = 1;
				r++;
				if (true) {
					csv.set(c, r, m_digi.MOZ_CND_FOLD);
				}
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = hr.seg[q];
					//---
					csv.set(c, r+1, seg.name_of_dm);
					csv.set(c, r+2, seg.name_of_pd);
					csv.set(c, r+3, seg.name_of_ir);
#if true//2019.03.21(NODATA-1化)
					if (seg.bNODATA) {
					csv.set(c, r+4, "-1");
					csv.set(c, r+5, "-1");
					csv.set(c, r+6, "-1");
					csv.set(c, r+7, "-1");
					csv.set(c, r+8, "-1");
					}
					else {
#endif
					csv.set(c, r+4, I2S(seg.pts_cen_cut.Count));
					csv.set(c, r+5, F1S(seg.cut_ttl));
					csv.set(c, r+6, F1S(seg.dia_avg));
#if true//2019.05.07(毛髄複線面積値対応)
					csv.set(c, r+7, F1S(seg.moz_hsl_mul));
					csv.set(c, r+8, F1S(seg.moz_hsd_mul));
#else
					csv.set(c, r+7, F1S(seg.moz_hsl));
					csv.set(c, r+8, F1S(seg.moz_hsd));
#endif
#if true//2019.03.16(NODATA対応)
					}
#endif
				}
				r+=9;
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r, "**********キューティクル間隔・ヒストグラム**********");
					r++;
				}
				//キューティクル間隔のヒストグラム
				if (true) {
					csv.set(0, r, "キューティクル間隔[um]");
				}
				for (int i = 0; i < G.SS.MOZ_CND_HCNT; i++) {
					//double x = (G.SS.MOZ_CND_HWID/2.0) + i * G.SS.MOZ_CND_HWID;
					csv.set(0, r+1+i, string.Format("{0}~{1}", i * G.SS.MOZ_CND_HWID, (i+1) * G.SS.MOZ_CND_HWID));
				}
				c = 1;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = hr.seg[q];
					//---
					csv.set(c, r, "度数");
					for (int i = 0; i < G.SS.MOZ_CND_HCNT; i++) {
#if true//2019.03.21(NODATA-1化)
						if (seg.bNODATA) {
						csv.set(c, r+1+i, "-1");
						}
						else {
#endif
						csv.set(c, r+1+i, string.Format("{0}", seg.his_cen_cut[i]));
#if true//2019.03.16(NODATA対応)
						}
#endif
					}
				}
				r += 1+G.SS.MOZ_CND_HCNT;
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r++, "**********HSV・ヒストグラム**********");
				}
				if (true) {
					csv.set(0, r, "キューティクル/H色相[deg.]");
				}
				for (int i = 0; i < 180; i++) {
					csv.set(0, r+1+i, I2S(i*2));
				}
				c = 1;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = (DIGITI.seg_of_hair)hr.seg[q];
					//---
					csv.set(c, r, "H度数");
					for (int i = 0; i < 180; i++) {
#if true//2019.03.21(NODATA-1化)
						if (seg.bNODATA) {
						csv.set(c, r+1+i, "-1");
						}
						else {
#endif
						csv.set(c, r+1+i, F0S(seg.HIST_H_DM[i]));
#if true//2019.03.16(NODATA対応)
						}
#endif
					}
				}
				r += 180+1;
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r, "キューティクル/S画素値");
				}
				for (int i = 0; i < 256; i++) {
					csv.set(0, r+1+i, I2S(i));
				}
				c = 1;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = hr.seg[q];
					//---
					csv.set(c, r, "S度数");
					for (int i = 0; i < 256; i++) {
#if true//2019.03.21(NODATA-1化)
						if (seg.bNODATA) {
						csv.set(c, r+1+i, "-1");
						}
						else {
#endif
						csv.set(c, r+1+i, F0S(seg.HIST_S_DM[i]));
#if true//2019.03.16(NODATA対応)
						}
#endif
					}
				}
				r += 256+1;
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r, "キューティクル/V画素値");
				}
				for (int i = 0; i < 256; i++) {
					csv.set(0, r+1+i, I2S(i));
				}
				c = 1;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = hr.seg[q];
					//---
					csv.set(c, r, "V度数");
					for (int i = 0; i < 256; i++) {
#if true//2019.03.21(NODATA-1化)
						if (seg.bNODATA) {
						csv.set(c, r+1+i, "-1");
						}
						else {
#endif
						csv.set(c, r+1+i, F0S(seg.HIST_V_DM[i]));
#if true//2019.03.16(NODATA対応)
						}
#endif
					}
				}
				r += 256+1;
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r, "毛髪径/H色相[deg.]");
				}
				for (int i = 0; i < 180; i++) {
					csv.set(0, r+1+i, I2S(i*2));
				}
				c = 1;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = hr.seg[q];
					//---
					csv.set(c, r, "H度数");
					for (int i = 0; i < 180; i++) {
#if true//2019.03.21(NODATA-1化)
						if (seg.bNODATA) {
						csv.set(c, r+1+i, "-1");
						}
						else {
#endif
						csv.set(c, r+1+i, F0S(seg.HIST_H_PD[i]));
#if true//2019.03.16(NODATA対応)
						}
#endif
					}
				}
				r += 180+1;
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r, "毛髪径/S画素値");
				}
				for (int i = 0; i < 256; i++) {
					csv.set(0, r+1+i, I2S(i));
				}
				c = 1;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = hr.seg[q];
					//---
					csv.set(c, r, "S度数");
					for (int i = 0; i < 256; i++) {
#if true//2019.03.21(NODATA-1化)
						if (seg.bNODATA) {
						csv.set(c, r+1+i, "-1");
						}
						else {
#endif
						csv.set(c, r+1+i, F0S(seg.HIST_S_PD[i]));
#if true//2019.03.16(NODATA対応)
						}
#endif
					}
				}
				r += 256+1;
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r, "毛髪径/V画素値");
				}
				for (int i = 0; i < 256; i++) {
					csv.set(0, r+1+i, I2S(i));
				}
				c = 1;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = hr.seg[q];
					//---
					csv.set(c, r, "V度数");
					for (int i = 0; i < 256; i++) {
#if true//2019.03.21(NODATA-1化)
						if (seg.bNODATA) {
						csv.set(c, r+1+i, "-1");
						}
						else {
#endif
						csv.set(c, r+1+i, F0S(seg.HIST_V_PD[i]));
#if true//2019.03.16(NODATA対応)
						}
#endif
					}
				}
				r += 256+1;
#if false
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r, "毛髄径/H色相[deg.]");
				}
				for (int i = 0; i < 180; i++) {
					csv.set(0, r+1+i, I2S(i*2));
				}
				c = 1;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = (seg_of_hair)hr.seg[q];
					//---
					csv.set(c, r, "H度数");
					for (int i = 0; i < 180; i++) {
						csv.set(c, r+1+i, F0S(seg.HIST_H_IR[i]));
					}
				}
				r += 180+1;
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r, "毛髄径/S画素値");
				}
				for (int i = 0; i < 256; i++) {
					csv.set(0, r+1+i, I2S(i));
				}
				c = 1;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = (seg_of_hair)hr.seg[q];
					//---
					csv.set(c, r, "S度数");
					for (int i = 0; i < 256; i++) {
						csv.set(c, r+1+i, F0S(seg.HIST_S_IR[i]));
					}
				}
				r += 256+1;
#endif
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r, "毛髄径/V画素値");
				}
				for (int i = 0; i < 256; i++) {
					csv.set(0, r+1+i, I2S(i));
				}
				c = 1;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = hr.seg[q];
					//---
					csv.set(c, r, "V度数");
					for (int i = 0; i < 256; i++) {
#if true//2019.03.21(NODATA-1化)
						if (seg.bNODATA) {
						csv.set(c, r+1+i, "-1");
						}
						else {
#endif
						csv.set(c, r+1+i, F0S(seg.HIST_V_IR[i]));
#if true//2019.03.16(NODATA対応)
						}
#endif
					}
				}
				r += 256+1;
				/*-------------------------------------------------------------------*/
				if (true) {
					csv.set(0, r++, "**********毛髪情報**********");
				}
				if (true) {
					csv.set(0, r, "毛髪位置[um]");
					csv.set(1, r, "キューティクルライン画素値");
					csv.set(2, r, "毛髪径[um]");
					csv.set(3, r, "毛髄径[um]");
					csv.set(4, r, "ファイル番号");
					r++;
				}
				double xum = 0;
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = hr.seg[q];
					if (q == 11) {
						q  = q;
					}
					//---
					for (int i = 0; i < seg.val_xum.Count; i++) {
						double ff0 = m_digi.TO_VAL(seg.val_xum[i]);
#if false//2019.03.16(NODATA対応)
						double ff1 = TO_VAL(seg.val_cen_fil[i]);
						double ff2 = TO_VAL(seg.mou_len[i]);
						double ff3 = seg.moz_hpl[i];
#endif
						if (ff0 < seg.ow_l_xum) {
							continue;
						}
						if (ff0 >= seg.ow_r_xum) {
							break;
						}
						csv.set(0, r, F1S(xum));
#if true//2019.03.21(NODATA-1化)
						if (seg.bNODATA) {
						csv.set(1, r, "-1");
						csv.set(2, r, "-1");
						csv.set(3, r, "-1");
						}
						else {
						double ff1 = m_digi.TO_VAL(seg.val_cen_fil[i]);
						double ff2 = m_digi.TO_VAL(seg.mou_len[i]);
#if true//2019.04.17(毛髄検出複線化)
						double ff3;
						m_digi.get_pl(seg, i, true, out ff3);
#else
						double ff3 = seg.moz_hpl[i];
#endif
#endif
						csv.set(1, r, F0S(ff1));
						csv.set(2, r, F1S(ff2));
						csv.set(3, r, F1S(ff3));
#if true//2019.03.16(NODATA対応)
						}
#endif
						csv.set(4, r, i_no[q]);
						r++;
						xum += dx;
					}
				}
				/*-------------------------------------------------------------------*/
				csv.save(path);
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}
		}
#endif
		private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.comboBox8.Tag != null) {
				return;
			}
			DIGITI.hair hr = m_digi.m_hair[m_i];
			if (m_isel >= hr.seg.Count()) {
				return;
			}
			if (true/*this.radioButton4.Checked*/) {
				draw_image(hr);
			}
		}
		private void set_max_min(double f, ref double fmin, ref double fmax)
		{
			if (fmax < f) {
				fmax = f;
			}
			if (fmin > f) {
				fmin = f;
			}
		}
		private DataPoint dp_marker(double x, double y, Color c)
		{
			DataPoint dp = new DataPoint();
			dp.SetValueXY(x, y);
			dp.MarkerStyle = MarkerStyle.Square;
			dp.MarkerColor = c;
			return(dp);
		}
		private void draw_cuticle(DIGITI.hair hr)
		{
			try {
				//---
				int idx = m_isel;
				DIGITI.seg_of_hair seg = hr.seg[idx];
				double cut_max = double.MinValue;
				double cut_min = double.MaxValue;
				int[] his_p5u = new int[G.SS.MOZ_CND_HCNT];
				int[] his_phf = new int[G.SS.MOZ_CND_HCNT];
				int[] his_cen = new int[G.SS.MOZ_CND_HCNT];
				int[] his_mph = new int[G.SS.MOZ_CND_HCNT];
				int[] his_m5u = new int[G.SS.MOZ_CND_HCNT];
#if true//2019.01.05(キューティクル検出欠損修正)
				int YC;
#endif
				//---
				this.chart4.Series[0].Points.Clear();
//@@@				this.chart4.Series[1].Points.Clear();
//@@@				this.chart4.Series[2].Points.Clear();
//@@@				this.chart4.Series[3].Points.Clear();
//@@@				this.chart4.Series[4].Points.Clear();
#if true//2018.10.10(毛髪径算出・改造)
				this.chart4.Series[5-4].Points.Clear();
#endif
				this.chart4.ChartAreas[0].AxisX.Minimum = 0;
				this.chart4.ChartAreas[0].AxisX.Maximum = double.NaN;
				this.chart4.ChartAreas[0].AxisX.Interval = double.NaN;
				//
				this.chart5.Series[0].Points.Clear();
//@@@				this.chart5.Series[1].Points.Clear();
//@@@				this.chart5.Series[2].Points.Clear();
//@@@				this.chart5.Series[3].Points.Clear();
//@@@				this.chart5.Series[4].Points.Clear();
				this.chart5.ChartAreas[0].AxisX.Minimum = 0;
				this.chart5.ChartAreas[0].AxisX.Maximum = G.SS.MOZ_CND_HMAX;
				this.chart5.ChartAreas[0].AxisX.Interval = double.NaN;

				//
				if (seg == null) {
					return;
				}
				DIGITI.seg_of_hair seg_bak = seg;
				double		offs = 0;
				double		xmin = 0;

				if (this.radioButton1.Checked) {//グラフ・毛髪全体
					double um_of_width = G.PX2UM(seg.width, m_digi.m_log_info.pix_pitch, m_digi.m_log_info.zoom);
					for (int q = 0;; q++) {
						if (q >= hr.seg.Length) {
							return;
						}
						seg = hr.seg[q];
						if (seg != null && seg.val_xum.Count > 0) {
							break;
						}
						offs -= um_of_width;
					}
					xmin = offs;
				}
				else {//画像ファイル毎
					if (seg.val_xum.Count <= 0
#if true//2019.03.16(NODATA対応)
						|| seg.bNODATA == true
#endif
						) {
						return;
					}
				}

				for (int q = 0;; q++) {
					int i0;
					if (this.radioButton1.Checked) {//グラフ・毛髪全体
						if (q >= hr.seg.Length) {
							break;
						}
						seg = hr.seg[q];
						if (seg == null) {
							continue;
						}
						if (seg.val_xum.Count <= 0) {
							double um = G.PX2UM(seg.width, m_digi.m_log_info.pix_pitch, m_digi.m_log_info.zoom);
							double x0 = um+ offs;
							this.chart4.Series[0].Points.AddXY(x0, double.NaN);
							offs += um;
							continue;
						}
						for (i0 = 0; m_digi.TO_VAL(seg.val_xum[i0]) < 0; i0++) {
						}
						if (m_chk1!=0) {
							i0 = 0;
						}
					}
					else {
						i0 = 0;
						offs = -m_digi.TO_VAL(seg.val_xum[0]);
					}
#if true//2019.03.16(NODATA対応)
					if (seg.bNODATA) {
						this.chart4.Series[0].Points.AddXY(double.NaN, double.NaN);
						goto skip;
					}
#endif
#if true//2019.01.05(キューティクル検出欠損修正)
					if (true) {
						YC = seg.cut_inf.Count/2 + (int)this.numericUpDown1.Value;
						if (YC < 0) {
							YC = 0;
						}
						else if (YC >= seg.cut_inf.Count) {
							YC = seg.cut_inf.Count-1;
						}
					}
#endif
					for (int i = i0; i < seg.val_xum.Count; i++) {
						double x0 = m_digi.TO_VAL(seg.val_xum[i]) + offs;
#if true//2018.11.28(メモリリーク)
						double y0 = double.NaN;
						double y1 = double.NaN;
#else
						double y0 = TO_VAL(seg.val_p5u_fil[i]);
						double y1 = TO_VAL(seg.val_phf_fil[i]);
#endif
#if true//2019.01.05(キューティクル検出欠損修正)
						double y2;
#else
						double y2 = TO_VAL(seg.val_cen_fil[i]);
#endif
#if true//2018.11.28(メモリリーク)
						double y3 = double.NaN;
						double y4 = double.NaN;
#else
						double y3 = TO_VAL(seg.val_mph_fil[i]);
						double y4 = TO_VAL(seg.val_m5u_fil[i]);
#endif
#if true//2018.12.25(オーバーラップ範囲改)
						if ((double)seg.val_xum[i] < seg.ow_l_xum || (double)seg.val_xum[i] > seg.ow_r_xum) {
							continue;
						}
#endif

						if (this.checkBox3.Checked) {//R*0
#if true//2019.01.05(キューティクル検出欠損修正)
							y2 = seg.cut_inf[YC].iaf[i];
#if DEBUG
							if (this.numericUpDown1.Value == 0 && y2 != m_digi.TO_VAL(seg.val_cen_fil[i])) {
								double tmp = m_digi.TO_VAL(seg.val_cen_fil[i]);
#if false//2019.02.16(数値化白髪オフセット)
								throw new Exception("Internal Error");
#endif
							}
#endif
#endif
							if (seg.flg_cen_cut[i]
#if true//2018.11.28(メモリリーク)
								!= 0
#endif
								) {
								//キューティクル位置はマーカー表示
								/*DataPoint dp = new DataPoint();
								dp.SetValueXY(x0, y2);
								dp.MarkerStyle = MarkerStyle.Square;
								dp.MarkerColor = Color.Yellow;
								dp.MarkerColor = Color.DarkBlue;*/
								this.chart4.Series[0].Points.Add(dp_marker(x0, y2, Color.DarkBlue));
							}
							else {
								this.chart4.Series[0].Points.AddXY(x0, y2);
							}
							set_max_min(y2, ref cut_min, ref cut_max);
							//if (cut_max < y2) {
							//    cut_max = y2;
							//}
							//if (cut_min > y2) {
							//    cut_min = y2;
							//}
						}
					}
					if (seg.his_cen_cut.Count != G.SS.MOZ_CND_HCNT) {
						G.mlog("Internal Error");
					}
					if (this.radioButton1.Checked) {//グラフ・毛髪全体
						for (int i = 0; i < G.SS.MOZ_CND_HCNT; i++) {
							his_cen[i] += seg.his_cen_cut[i];//集計する
#if false//2018.11.28(メモリリーク)
							his_phf[i] += seg.his_phf_cut[i];//集計する
							his_mph[i] += seg.his_mph_cut[i];//集計する
							his_p5u[i] += seg.his_p5u_cut[i];//集計する
							his_m5u[i] += seg.his_m5u_cut[i];//集計する
#endif
						}
					}
					else {
						//キューティクル間隔のヒストグラム
						for (int i = 0; i < G.SS.MOZ_CND_HCNT; i++) {
							double x = (G.SS.MOZ_CND_HWID/2.0) + i * G.SS.MOZ_CND_HWID;
							this.chart5.Series[0].Points.AddXY(x, seg.his_cen_cut[i]);
						}
					}

					if (!this.radioButton1.Checked) {
						break;
					}
#if true//2019.03.16(NODATA対応)
skip:
#endif
					double	dx = m_digi.TO_VAL(seg.val_xum[1])-m_digi.TO_VAL(seg.val_xum[0]);
					//offs += dx * seg.moz_zpl.Count;
					offs += m_digi.TO_VAL(seg.val_xum[seg.val_xum.Count-1])+dx;
				}
				if (this.radioButton1.Checked) {//毛髪全体
					//キューティクル間隔のヒストグラム
					for (int i = 0; i < G.SS.MOZ_CND_HCNT; i++) {
						double x = (G.SS.MOZ_CND_HWID/2.0) + i * G.SS.MOZ_CND_HWID;
						this.chart5.Series[0].Points.AddXY(x, his_cen[i]);
//@@@						this.chart5.Series[1].Points.AddXY(x, his_phf[i]);
//@@@						this.chart5.Series[2].Points.AddXY(x, his_mph[i]);
//@@@						this.chart5.Series[3].Points.AddXY(x, his_p5u[i]);
//@@@						this.chart5.Series[4].Points.AddXY(x, his_m5u[i]);
					}
				}

				if (this.radioButton1.Checked) {
					seg = seg_bak;
				}
				if (true) {
					this.chart4.Series[0].Color = Color.Cyan;	//R*0
//@@@					this.chart4.Series[1].Color = Color.Green;	//R*+50%
//@@@					this.chart4.Series[2].Color = Color.Magenta;//R*-50%
//@@@					this.chart4.Series[3].Color = Color.Blue;	//R+3um
//@@@					this.chart4.Series[4].Color = Color.Red;	//R-3um
					//---
					this.chart5.Series[0].Color = Color.Cyan;	//R*0
//@@@					this.chart5.Series[1].Color = Color.Green;	//R*+50%
//@@@					this.chart5.Series[2].Color = Color.Magenta;//R*-50%
//@@@					this.chart5.Series[3].Color = Color.Blue;	//R+3um
//@@@					this.chart5.Series[4].Color = Color.Red;	//R-3um
					//---
					this.chart4.Series[0].Enabled = this.checkBox3.Checked;//R*0
//@@@					this.chart4.Series[1].Enabled = this.checkBox4.Checked;//R*+50%
//@@@					this.chart4.Series[2].Enabled = this.checkBox5.Checked;//R*-50%
//@@@					this.chart4.Series[3].Enabled = this.checkBox6.Checked;//R+3um
//@@@					this.chart4.Series[4].Enabled = this.checkBox7.Checked;//R-3um
					//---
					this.chart5.Series[0].Enabled = this.checkBox3.Checked;//R*0
//@@@					this.chart5.Series[1].Enabled = this.checkBox4.Checked;//R*+50%
//@@@					this.chart5.Series[2].Enabled = this.checkBox5.Checked;//R*-50%
//@@@					this.chart5.Series[3].Enabled = this.checkBox6.Checked;//R+3um
//@@@					this.chart5.Series[4].Enabled = this.checkBox7.Checked;//R-3um
				}
				//---
				if (true) {
					//---
					this.chart4.ChartAreas[0].RecalculateAxesScale();
					//---
					double fmin = this.chart4.ChartAreas[0].AxisX.Minimum;
					double fmax = this.chart4.ChartAreas[0].AxisX.Maximum;
					if (G.SS.MOZ_CND_CTYP == 1) {
						cut_min = Math.Abs(cut_min);
						if (cut_max < cut_min) {
							cut_max = cut_min;
						}
						if (cut_max <= 1.0) {
							cut_max = 1;
						}
						else {
							cut_max /= 10;
							if (cut_max <= 1.0) {
								cut_max = 1;
							}
							else {
								cut_max = Math.Ceiling(cut_max);
							}
							cut_max *= 10;
						}
						this.chart4.ChartAreas[0].AxisY.Minimum  = -cut_max;
						this.chart4.ChartAreas[0].AxisY.Maximum  = +cut_max;//double.NaN;//256;
						if (cut_max >= 40) {
							this.chart4.ChartAreas[0].AxisY.Interval =  10;//double.NaN;//32;
						}
						if (cut_max > 1) {
							this.chart4.ChartAreas[0].AxisY.Interval =  5;//double.NaN;//32;
						}
						else {
							this.chart4.ChartAreas[0].AxisY.Interval =  0.2;
						}
					}
					else {
						this.chart4.ChartAreas[0].AxisY.Minimum  = -30;
						this.chart4.ChartAreas[0].AxisY.Maximum  = +80;//double.NaN;//256;
						this.chart4.ChartAreas[0].AxisY.Interval =  10;//32;
					}
					//
					this.chart4.ChartAreas[0].AxisX.Minimum = xmin;
					this.chart4.ChartAreas[0].AxisX.IntervalOffset = -xmin;
					//---
					int interval;
					fmax = this.chart4.ChartAreas[0].AxisX.Maximum;
					if (fmax < 300) {
						interval = 50;
					}
					else if (fmax < 500) {
						interval = 100;
					}
					else if (fmax < 1000) {
						interval = 200;
					}
					else if (fmax < 3000) {
						interval = 500;
					}
					else {
						interval = 1000;
					}
					this.chart4.ChartAreas[0].AxisX.Interval = interval;
#if true//2018.12.25(オーバーラップ範囲改)
					fmax = m_digi.TO_VAL(seg.val_xum[seg.val_xum.Count-1])+offs;
					this.chart4.ChartAreas[0].AxisX.Maximum = fmax;//キューティクルライン
#endif
				}
				if (true) {
					double	bval = (G.SS.MOZ_CND_CTYP == 0) ? G.SS.MOZ_CND_BPVL: G.SS.MOZ_CND_2DVL;
#if true//2018.10.10(毛髪径算出・改造)
					this.chart4.Series[5-4].Points.AddXY(this.chart4.ChartAreas[0].AxisX.Minimum, bval);
					this.chart4.Series[5-4].Points.AddXY(this.chart4.ChartAreas[0].AxisX.Maximum, bval);
					this.chart4.Series[5-4].BorderDashStyle = ChartDashStyle.DashDotDot;
					this.chart4.Series[5-4].Color = Color.Black;
#endif
				}
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}
		}
#if true//2018.09.29(キューティクルライン検出)
		public void UPDATE_CUTICLE()
		{//キューティクル・フィルター処理
			if (this.listView1.Items.Count <= 0) {
				return;
			}
			if (m_i >= m_digi.m_hair.Count) {
				return;
			}
			DIGITI.hair hr = m_digi.m_hair[m_i];
			if (m_isel >= hr.seg.Count()) {
				return;
			}
#if true//2019.01.11(混在対応)
			m_digi.SWAP_ANL_CND(hr.mode_of_cl);//0:透過, 1:反射
#endif
			if (!m_digi.calc_filter_coeff()) {
				return;
			}
#if true//2018.10.30(キューティクル長)
			UPDATE_BY_FILES(0);
#endif
			draw_image(hr);
			draw_cuticle(hr);
		}
#endif
#if true//2018.10.10(毛髪径算出・改造)
		private void UPDATE_BY_FILES(int mode)
		{
			if (this.listView1.Items.Count <= 0) {
				return;
			}
			if (m_i >= m_digi.m_hair.Count) {
				return;
			}
			DIGITI.hair hr = m_digi.m_hair[m_i];
			if (m_isel >= hr.seg.Count()) {
				return;
			}
#if true//2019.01.11(混在対応)
			m_digi.SWAP_ANL_CND(hr.mode_of_cl);//0:透過, 1:反射
#endif
			for (int i = 0; i < hr.seg.Length; i++) {
				DIGITI.seg_of_hair[] segs = hr.seg;
				DIGITI.seg_of_hair seg = hr.seg[i];
				if (seg == null) {
					continue;
				}
				string path_dm1 = segs[i].path_of_dm;
				string path_dm2 = (i != (segs.Length-1)) ? (segs[i+1].path_of_dm): null;
				string path_pd1 = segs[i].path_of_pd;
				string name_dm1 = segs[i].name_of_dm;
				string name_ir1 = segs[i].name_of_ir;
				string name_pd1 = segs[i].name_of_pd;
				string path_ir1 = segs[i].path_of_ir;
				string path_ir2 = (i != (segs.Length-1)) ? (segs[i+1].path_of_ir): null;

				m_digi.load_bmp(segs, i,
					path_dm1, path_dm2,
					path_ir1, path_ir2,
					ref m_digi.m_bmp_dm0, ref m_digi.m_bmp_dm1, ref m_digi.m_bmp_dm2,
					ref m_digi.m_bmp_ir0, ref m_digi.m_bmp_ir1, ref m_digi.m_bmp_ir2
				);
				if (true) {
					DIGITI.dispose_bmp(ref m_digi.m_bmp_pd1);
					if (name_pd1.Equals(name_dm1)) {
						m_digi.m_bmp_pd1 = (Bitmap)m_digi.m_bmp_dm1.Clone();
					}
					else {
						m_digi.m_bmp_pd1 = new Bitmap(path_pd1);
					}
				}
				if (true) {
					if (m_digi.m_bmp_ir1 != null && G.SS.MOZ_CND_FTCF > 0) {
						Form02.DO_SMOOTH(m_digi.m_bmp_ir1, m_digi.MOZ_CND_FTCF, m_digi.MOZ_CND_FTCT);
						//m_bmp_ir1.Save("c:\\temp\\"+name_ir1);
#if false//2018.10.24(毛髪径算出・改造)
						m_bmp_ir1.Save("c:\\temp\\IMG_IR.PNG");
#endif
					}
#if true//2018.11.02(HSVグラフ)
					if (mode == 1) {
						m_digi.calc_hist(segs[i]);
					}
					else
#endif
					if (segs[i].dia_cnt > 1) {
//Bitmap bmp_msk = (Bitmap)m_bmp_ir1.Clone();
//Form02.DO_SET_FBD_REGION(m_bmp_ir1, bmp_msk, segs[i].dia_top, segs[i].dia_btm);
						m_digi.test_dm(segs, i, segs.Length, true);
					}
				}
			}
			DIGITI.dispose_bmp(ref m_digi.m_bmp_dm0);
			DIGITI.dispose_bmp(ref m_digi.m_bmp_dm1);
			DIGITI.dispose_bmp(ref m_digi.m_bmp_dm2);
			DIGITI.dispose_bmp(ref m_digi.m_bmp_ir0);
			DIGITI.dispose_bmp(ref m_digi.m_bmp_ir1);
			DIGITI.dispose_bmp(ref m_digi.m_bmp_ir2);

			draw_graph(hr);
			draw_image(hr);
#if true//2018.11.02(HSVグラフ)
			draw_hsv(hr);
#endif
		}

		private void button4_Click(object sender, EventArgs e)
		{
			if (G.FORM24 == null) {
				Form24 frm = new Form24();
#if true//2019.01.11(混在対応)
				if (m_i >= m_digi.m_hair.Count) {
					return;
				}
				DIGITI.hair hr = m_digi.m_hair[m_i];
				Form24.m_i = hr.mode_of_cl;//0:透過, 1:反射
#endif
				frm.Show(this);
			}
			else {
				G.FORM24.Close();
			}
		}
		public void UPDATE_MOUZUI()
		{
#if true//2019.01.11(混在対応)
			if (m_i >= m_digi.m_hair.Count) {
				return;
			}
			DIGITI.hair hr = m_digi.m_hair[m_i];
			m_digi.SWAP_ANL_CND(hr.mode_of_cl);//0:透過, 1:反射
#endif
			m_digi.MOZ_CND_FTCF = DIGITI.C_FILT_COFS[G.SS.MOZ_CND_FTCF];
			m_digi.MOZ_CND_FTCT = DIGITI.C_FILT_CNTS[G.SS.MOZ_CND_FTCT];
			m_digi.MOZ_CND_SMCF = DIGITI.C_SMTH_COFS[G.SS.MOZ_CND_SMCF];
			//---
			UPDATE_BY_FILES(0);
		}
#if true//2018.11.02(HSVグラフ)
		public void UPDATE_HSV()
		{
#if true//2019.01.11(混在対応)
			if (m_i >= m_digi.m_hair.Count) {
				return;
			}
			DIGITI.hair hr = m_digi.m_hair[m_i];
			m_digi.SWAP_ANL_CND(hr.mode_of_cl);//0:透過, 1:反射
#endif
			//---
			UPDATE_BY_FILES(1);
		}
#endif
		private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
		{
#if true//2018.11.02(HSVグラフ)
			set_hismod();
			this.propertyGrid1.Visible = true;
#endif
		}

		private void Form03_KeyDown(object sender, KeyEventArgs e)
		{
			if (this.tabControl3.SelectedIndex != 2) {
				return;
			}
			int imou = m_imou;
			switch (e.KeyCode) {
				case Keys.Right:
					imou++;
					break;
				case Keys.Left:
					imou--;
					break;
				default:
					return;
			}
			e.Handled = true;
			DIGITI.hair hr = m_digi.m_hair[m_i];
			if (m_isel >= hr.seg.Count()) {
				return;
			}
			if (imou < 0 || imou >= hr.seg[m_isel].moz_inf.Count) {
				return;
			}
			m_imou = imou;
			draw_moudan(hr);
		}
#endif




#if true//2018.11.02(HSVグラフ)
		private
		const int ETC_HIS_MODE = 1;
		private
		Chart[] m_cht_his = null;
		private
		Color[] m_col_of_hue = new Color[180];

		private void set_hismod()
		{
			if (true) {
				m_cht_his = new Chart[] {
					this.chart10, this.chart11, this.chart12,
					this.chart13, this.chart14, this.chart15,
					this.chart16, this.chart17, this.chart18
				};
				for (int i = 0; i < m_col_of_hue.Length; i++) {
					//m_col_of_hue[i] = G.FORM02.m_col_of_hue[i];
					Form02.HSV hsv;
					hsv.h = (byte)i;
					hsv.s = hsv.v = 255;
					m_col_of_hue[i] = Form02.hsv2rgb(hsv);
				}
			}

			for (int i = 0; i < m_cht_his.Length; i++) {
				m_cht_his[i].Series[0].ToolTip = "(#INDEX, #VAL)";
				if (ETC_HIS_MODE != 0) {
					m_cht_his[i].ChartAreas[0].AxisX.Minimum = 0;
					m_cht_his[i].ChartAreas[0].AxisX.Maximum = ((i%3)==0) ? 360:256;
					m_cht_his[i].ChartAreas[0].AxisX.IntervalOffset = 0;
					m_cht_his[i].ChartAreas[0].AxisX.Interval =((i%3)==0) ? 60 : 64;
					m_cht_his[i].Series[0].SetCustomProperty("PointWidth", "1.0");
				}
				else {
					m_cht_his[i].ChartAreas[0].AxisX.Minimum = 0;
					m_cht_his[i].ChartAreas[0].AxisX.Maximum = 256;
					m_cht_his[i].ChartAreas[0].AxisX.IntervalOffset = 0;
					m_cht_his[i].ChartAreas[0].AxisX.Interval = 64;
					//this.chart10.Series[0].SetCustomProperty("PointWidth", "0.8");
				}
			}
		}
		private void draw_hsv(DIGITI.hair hr)
		{
			try {
				int idx = m_isel;
				//seg_of_hair seg = (seg_of_hair)hr.seg[idx];
				double []HIST_H_DM = new double[256];
				double []HIST_S_DM = new double[256];
				double []HIST_V_DM = new double[256];
				double []HIST_H_PD = new double[256];
				double []HIST_S_PD = new double[256];
				double []HIST_V_PD = new double[256];
				double []HIST_H_IR = new double[256];
				double []HIST_S_IR = new double[256];
				double []HIST_V_IR = new double[256];

				for (int i = 0; i < m_cht_his.Length; i++) {
					m_cht_his[i].Series[0].Points.Clear();
				}

				if (ETC_HIS_MODE == 0) {
					return;
				}
				int i_s, i_e;
				if (this.radioButton1.Checked) {//グラフ・全体
					i_s = 0;
					i_e = hr.seg.Length-1;
				}
				else {
					i_s = m_isel;
					i_e = m_isel;
				}
				for (int q = i_s; q <= i_e; q++) {
					DIGITI.seg_of_hair seg = hr.seg[q];
					for (int i = 0; i < 256; i++) {
						HIST_H_DM[i] += seg.HIST_H_DM[i];
						HIST_S_DM[i] += seg.HIST_S_DM[i];
						HIST_V_DM[i] += seg.HIST_V_DM[i];
						HIST_H_PD[i] += seg.HIST_H_PD[i];
						HIST_S_PD[i] += seg.HIST_S_PD[i];
						HIST_V_PD[i] += seg.HIST_V_PD[i];
						HIST_H_IR[i] += seg.HIST_H_IR[i];
						HIST_S_IR[i] += seg.HIST_S_IR[i];
						HIST_V_IR[i] += seg.HIST_V_IR[i];
					}
				}
				for (int i = 0; i < 180; i++) {
					DataPoint dp = new DataPoint();
					dp = new DataPoint();
					dp.Color = m_col_of_hue[i];
					//---
					dp.SetValueXY(i << 1, HIST_H_DM[i]);
					this.chart10.Series[0].Points.Add(dp);
					//---
					dp = new DataPoint();
					dp.Color = m_col_of_hue[i];
					dp.SetValueXY(i << 1, HIST_H_PD[i]);
					this.chart13.Series[0].Points.Add(dp);
					//---
					dp = new DataPoint();
					dp.Color = m_col_of_hue[i];
					dp.SetValueXY(i << 1, HIST_H_IR[i]);
					this.chart16.Series[0].Points.Add(dp);
				}
				for (int i = 0; i < 256; i++) {
					this.chart11.Series[0].Points.AddXY(i, HIST_S_DM[i]);
					this.chart12.Series[0].Points.AddXY(i, HIST_V_DM[i]);
					this.chart14.Series[0].Points.AddXY(i, HIST_S_PD[i]);
					this.chart15.Series[0].Points.AddXY(i, HIST_V_PD[i]);
					this.chart17.Series[0].Points.AddXY(i, HIST_S_IR[i]);
					this.chart18.Series[0].Points.AddXY(i, HIST_V_IR[i]);
				}
				//---
				for (int i = 0; i < m_cht_his.Length; i++) {
					m_cht_his[i].ChartAreas[0].AxisY.Maximum = double.NaN;
				}
				//---
				//if (G.IR.HIST_ALL) {
				//    this.label5.Text = "画像全体";
				//}
				//else if (!G.IR.HIST_RECT) {
				//    this.label5.Text = "マスク範囲";
				//}
				//else {
				//    Point p1 = new Point(G.SS.CAM_HIS_RT_X, G.SS.CAM_HIS_RT_Y);
				//    Point p2 = new Point(p1.X + G.SS.CAM_HIS_RT_W, p1.Y + G.SS.CAM_HIS_RT_H);
				//    this.label5.Text = string.Format(" ({0},{1})\r-({2},{3})", p1.X, p1.Y, p2.X, p2.Y);
				//}
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}
		}
#endif
	}
}
#endif