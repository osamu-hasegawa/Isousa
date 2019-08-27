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
//using LIST_U8 = System.Collections.Generic.List<byte>;

namespace uSCOPE
{
	public partial class Form04 : Form
	{
#if true//2019.05.22(再測定判定(キューティクル枚数))
		private const int C_CLM_ZP_DROP = 2;
		private const int C_CLM_CT_CONT = 3;
		private const int C_CLM_CT_RATE = 4;

		private const int C_CLM_KP_CONT = 3+2;
		private const int C_CLM_KP_DROP = 4+2;
		private const int C_CLM_HR_RATE= 7+2;
		private const int C_CLM_RM_SHOT = 8+2;
		private const int C_CLM_RM_MAKE = 9+2;
#if true//2019.07.27(保存形式変更)
		private const int C_CLM_DL_FLAG = 10+2;
#if true//2019.08.08(保存内容変更)
		private const int C_CLM_CH_FLAG = 11+2;
		private const int C_CLM_UN_FLAG = 12+2;
		private const int C_CLM_GM_FLAG = 13+2;
		private const int C_CLM_RM_CUNT = 14+2;
		private const int C_CLM_CHOKKIN = 15+2;
#else
		private const int C_CLM_RM_CUNT = 11+2;
		private const int C_CLM_CHOKKIN = 12+2;
#endif
		private const int C_CLM_FILNAME = 0;
		private const int C_CLM_ZP_CONT = 1;
#else
		private const int C_CLM_RM_CUNT = 10+2;
		private const int C_CLM_CHOKKIN = 11+2;
#endif
		DIGITI	m_digi = new DIGITI();
#endif
		private int m_i = 0;
		private int m_isel = 0;
#if true//2019.07.27(保存形式変更)
//		private List<string> m_del_flag = null;
#endif
//%%		private string MOZ_CND_FOLD;
#if true//2019.05.08(再測定・深度合成)
//%%		private string m_fold_of_dept;
#endif
		//---
//%%	private List<hair> m_hair = new List<hair>();
		//---
//%%		private Bitmap	m_bmp_dm1;
//%%		private Bitmap	m_bmp_ir1;
//%%		private Bitmap	m_bmp_pd1;
//%%		private Point[] m_dia_top;
//%%		private Point[]	m_dia_btm;
//%%		private int		m_dia_cnt;
		private int		m_chk1, m_chk2;
		//---
		//private ArrayList m_zpos_org = new ArrayList();
		//private ArrayList m_zpos_val = new ArrayList();

		public int MAKELONG(int lowPart, int highPart)
		{
		   return((int)(((ushort)lowPart) | (uint)(highPart << 16)));
		}

		public static short HIWORD(int dword)
		{
			return((short)(dword >> 16));
		}
		public static short LOWORD(int dword)
		{
			return((short)dword);
		}
		//---
		public Form04()
		{
			InitializeComponent();
		}
#if false//2019.05.22(再測定判定(キューティクル枚数))
#endif
		ArrayList m_ah_cl = new ArrayList();
		ArrayList m_ah_ir = new ArrayList();
		ArrayList m_rst = new ArrayList();
#if false//2019.05.22(再測定判定(キューティクル枚数))
#endif
#if false//2019.05.22(再測定判定(キューティクル枚数))
#endif
#if false//2019.05.22(再測定判定(キューティクル枚数))
#endif
		private void enable_forms(bool b)
		{
		}
#if false//2019.05.22(再測定判定(キューティクル枚数))
#endif
		//---
		private void calc_contrast(int mode_of_cl, DIGITI.seg_of_hair seg, Bitmap bmp_dm = null, Bitmap bmp_pd = null)
		{
			string tmp;
			G.CAM_STS bak = G.CAM_PRC;
			Bitmap bmp;

			if (true) {
				if (bmp_pd == null) {
					bmp = new Bitmap(seg.path_of_pd);//中心(KP00D)
				}
				else {
#if true//2019.05.22(再測定判定(キューティクル枚数))
					bmp = (Bitmap)bmp_pd.Clone();
					bmp.Tag = null;
#else
					bmp = bmp_pd;
#endif
				}
				G.CNT_NO_CONTOURS = false;
				G.CNT_MOD = G.AFMD2N(G.SS.MOZ_BOK_AFMD[mode_of_cl+2]);	//中心:コントスラト計算範囲
				G.CNT_OFS = G.SS.MOZ_BOK_COFS[mode_of_cl];				//中心:上下オフセット
#if true//2019.03.22(再測定表)
				G.CNT_MET = G.SS.MOZ_BOK_CMET[mode_of_cl+2];			//中心:計算方法
#endif
				G.CAM_PRC = G.CAM_STS.STS_HIST;
				G.FORM02.load_file(bmp, false);
				seg.kp_contr = G.IR.CONTRAST;
				seg.kp_contr_drop = double.NaN;
				seg.kp_contr_avg = double.NaN;
				seg.msk_of_pd = G.IR.MSK_PLY.Take(G.IR.MSK_PLY_CNT).ToArray();;
				if (G.CNT_MOD < 2) {
					G.CAM_PRC = G.CAM_STS.STS_HAIR;
					G.FORM02.UPDATE_PROC();
				}
				if (G.IR.CIR_CNT > 0 && G.IR.DIA_CNT > 1) {
					int i = 0, h = G.IR.DIA_CNT-1;
					seg.mou_len_l = G.diff(G.IR.DIA_TOP[i], G.IR.DIA_BTM[i]);
					seg.mou_len_l = m_digi.px2um(seg.mou_len_l);
					//---
					seg.mou_len_r = G.diff(G.IR.DIA_TOP[h], G.IR.DIA_BTM[h]);
					seg.mou_len_r = m_digi.px2um(seg.mou_len_r);
				}
				else {
					seg.mou_len_l = double.NaN;
					seg.mou_len_r = double.NaN;
				}
				if (
#if true//2019.05.22(再測定判定(キューティクル枚数))
					true
#else
					bmp_pd == null
#endif
					) {
					bmp.Dispose();
				}
			}

			if (true) {
				if (bmp_dm == null) {
					tmp = m_digi.to_xx_path(seg.path_of_dm, "ZP00D");//表面(ZP00D)
					bmp = new Bitmap(tmp);
				}
				else {
#if true//2019.05.22(再測定判定(キューティクル枚数))
					bmp = (Bitmap)bmp_dm.Clone();
					bmp.Tag = null;
#else
					bmp = bmp_dm
#endif
						;
				}
				//mode_of_cl=0:透過, 1:反射
				G.CNT_NO_CONTOURS = true;
				G.CNT_MOD = G.AFMD2N(G.SS.MOZ_BOK_AFMD[mode_of_cl+0]);	//表面:コントスラト計算範囲
				G.CNT_OFS = G.SS.MOZ_BOK_SOFS[mode_of_cl];				//表面:上下オフセット
#if true//2019.03.22(再測定表)
				G.CNT_MET = G.SS.MOZ_BOK_CMET[mode_of_cl+0];			//表面:計算方法
#endif
				G.CAM_PRC = G.CAM_STS.STS_HIST;
				G.FORM02.load_file(bmp, false);
				seg.zp_contr = G.IR.CONTRAST;
				seg.zp_contr_drop = double.NaN;
				seg.zp_contr_avg = double.NaN;
				seg.msk_of_dm = G.IR.MSK_PLY.Take(G.IR.MSK_PLY_CNT).ToArray();
				//---
				if (
#if true//2019.05.22(再測定判定(キューティクル枚数))
					true
#else
					bmp_dm == null
#endif
					) {
					bmp.Dispose();
					bmp = null;
				}
			}
			//---
			G.CNT_NO_CONTOURS = false;
			G.CAM_PRC = bak;
		}
		//---
		private void calc_contrast_avg(DIGITI.seg_of_hair[] segs)
		{
			double kp_avg = 0, zp_avg = 0;
			double ct_avg = 0;

			for (int i = 0; i < segs.Length; i++) {
#if true//2019.07.27(保存形式変更)
				if (segs[i].pts_cen_cut != null) {
					segs[i].cut_count = segs[i].pts_cen_cut.Count;
				}
				else {
					segs[i].cut_count = 0;
				}
#else
				segs[i].cut_count = segs[i].pts_cen_cut.Count;
#endif
				kp_avg += segs[i].kp_contr;
				zp_avg += segs[i].zp_contr;
				ct_avg += segs[i].cut_count;
			}
			kp_avg /= segs.Length;
			zp_avg /= segs.Length;
			ct_avg /= segs.Length;
			for (int i = 0; i < segs.Length; i++) {
				segs[i].kp_contr_drop = -(segs[i].kp_contr - kp_avg) / kp_avg * 100;
				segs[i].kp_contr_avg = kp_avg;
				segs[i].kp_nodata = (segs[i].kp_contr_drop >= G.SS.MOZ_BOK_CTHD);
				//---
				segs[i].zp_contr_drop = -(segs[i].zp_contr - zp_avg) / zp_avg * 100;
				segs[i].zp_contr_avg = zp_avg;
				segs[i].zp_nodata = (segs[i].zp_contr_drop >= G.SS.MOZ_BOK_CTHD);
				//---
				segs[i].cut_avg = ct_avg;
				segs[i].cut_drop = -(segs[i].cut_count - ct_avg) / ct_avg * 100;
				//---
				double	l_max = segs[i].mou_len_l,
						l_min = segs[i].mou_len_r;
				if (l_min > l_max) {
					double tmp = l_min;
					l_min = l_max;
					l_max = tmp;
				}
				segs[i].mou_len_c = (l_max-l_min)/l_max * 100;
			}
			//---
		}
#if true//2019.05.22(再測定判定(キューティクル枚数))
		private bool check_remes(DIGITI.seg_of_hair seg)
		{
			return(check_remes(seg.cut_count, seg.cut_drop, seg.zp_contr_drop, seg.kp_contr_drop));
		}
		private bool check_remes(double cut_count, double cut_drop, double zp_contr_drop, double kp_contr_drop)
		{
			bool flag;
			if (false) {
			}
			else if (cut_count < G.SS.REM_CUT_CTHD && G.SS.REM_CUT_US_C) {
				flag = true;
			}
			else if (cut_drop >= G.SS.REM_CUT_RTHD && G.SS.REM_CUT_US_R) {
				flag = true;
			}
			else {
				if (G.UIF_LEVL != 0) {
					flag = (zp_contr_drop >= G.SS.REM_BOK_STHD || kp_contr_drop >= G.SS.REM_BOK_CTHD);
				}
				else {
					flag = (zp_contr_drop >= G.SS.REM_BOK_STHD);
				}
			}
			return(flag);
		}
		private bool check_remak(DIGITI.seg_of_hair seg)
		{
			return (check_remak(seg.mou_len_c));
		}
		private bool check_remak(double mou_len_c)
		{
			return (mou_len_c >= G.SS.REM_CHG_DTHD);
		}
#endif
#if true//2019.07.27(保存形式変更)
		private List<string> get_del_list(
#if true//2019.08.08(保存内容変更)
			int I_CLM_INDEX
#endif
			)
		{
			var l = new List<string>();
			for (int i = 0; i < this.dataGridView1.Rows.Count; i++) {
				string	name = (string)this.dataGridView1.Rows[i].Cells[C_CLM_FILNAME].Value;
#if true//2019.08.08(保存内容変更)
				bool	flag = (bool  )this.dataGridView1.Rows[i].Cells[I_CLM_INDEX  ].Value;
#else
				bool	flag = (bool  )this.dataGridView1.Rows[i].Cells[C_CLM_DL_FLAG].Value;
#endif
				if (flag) {
					l.Add(G.get_base_name(name));
				}
			}
			return(l);
		}
#endif
		//private m_idx;
		private void add_grid_row(DIGITI.seg_of_hair seg, int h_idx, int s_idx)
		{
			int idx;
			List<object> objs = new List<object>();

			if (seg.name_of_dm[1] >= '0' && seg.name_of_dm[1] <= '9') {
			objs.Add(seg.name_of_dm);
			}
			else {
			objs.Add(" " + seg.name_of_dm);
			}
			objs.Add(seg.zp_contr);
			objs.Add(seg.zp_contr_drop);
#if true//2019.05.22(再測定判定(キューティクル枚数))
			objs.Add(seg.cut_count);
			objs.Add(seg.cut_drop);
#endif
			objs.Add(seg.kp_contr);
			objs.Add(seg.kp_contr_drop);
			objs.Add(seg.mou_len_l);
			objs.Add(seg.mou_len_r);
			objs.Add(seg.mou_len_c);
#if true
			string buf = G.get_base_name(seg.name_of_dm);
			if (buf != null && m_digi.m_flag_del.Contains(buf)) {
			objs.Add(false);
			objs.Add(false);
			objs.Add(true);
			}
			else {
			objs.Add(check_remes(seg));
			objs.Add(check_remak(seg));
			objs.Add(false);
			}
#if true//2019.08.08(保存内容変更)
			if (buf != null && m_digi.m_flag_hakuri.Contains(buf)) {
				objs.Add(true);
			}
			else {
				objs.Add(false);
			}
			if (buf != null && m_digi.m_flag_uneri.Contains(buf)) {
				objs.Add(true);
			}
			else {
				objs.Add(false);
			}
			if (buf != null && m_digi.m_flag_gomi.Contains(buf)) {
				objs.Add(true);
			}
			else {
				objs.Add(false);
			}
#endif
			objs.Add(seg.bak_cnt);
			objs.Add(seg.bTMR);
#else
#if true//2019.05.22(再測定判定(キューティクル枚数))
			objs.Add(check_remes(seg));
			objs.Add(check_remak(seg));
			objs.Add(seg.bak_cnt);
			objs.Add(seg.bTMR);
#else
#if true//2019.05.08(再測定・深度合成)
			if (G.UIF_LEVL != 0) {
			objs.Add((seg.zp_contr_drop >= G.SS.REM_BOK_STHD || seg.kp_contr_drop >= G.SS.REM_BOK_CTHD) ? true: false);
			}
			else {
			objs.Add((seg.zp_contr_drop >= G.SS.REM_BOK_STHD) ? true: false);
			}
#endif
#endif

#if true//2019.04.02(再測定表ユーザモード)
#if true//2019.05.08(再測定・深度合成)
			objs.Add((seg.mou_len_c >= G.SS.REM_CHG_DTHD) ? true: false);//再作成
#endif
			objs.Add(seg.bak_cnt);
			objs.Add(seg.bTMR);
#endif
#endif
			idx = this.dataGridView1.Rows.Add(objs.ToArray());
			this.dataGridView1.Rows[idx].HeaderCell.Value = (idx+1).ToString();
			this.dataGridView1.Rows[idx].Tag = MAKELONG(s_idx, h_idx);
		}
#if true//2019.04.02(再測定表ユーザモード)
		private List<string> BAK_FOLDS = new List<string>();
		private List<string[]> BAK_FILES = new List<string[]>();

		private void search_bak_dir()
		{
			this.BAK_FOLDS.AddRange(System.IO.Directory.GetDirectories(m_digi.MOZ_CND_FOLD, "BAK_????????_??????"));
			this.BAK_FOLDS.Sort();
			//順序を逆にして[0]が直近のバックアップ, [N-1]が最古のバックアップにする
			this.BAK_FOLDS.Reverse();
			//---
			int i = 0;
			if (this.BAK_FOLDS.Count <= 0) {
				return;
			}
			while (true) {
				string[] files;
				List<string> lst = new List<string>();

				files = System.IO.Directory.GetFiles(this.BAK_FOLDS[i], "*CT_??_ZP00D.*");
				for (int h = 0; h < files.Length; h++) {
					lst.Add(System.IO.Path.GetFileName(files[h]));
				}
				//---
				files = System.IO.Directory.GetFiles(this.BAK_FOLDS[i], "*CR_??_ZP00D.*");
				for (int h = 0; h < files.Length; h++) {
					lst.Add(System.IO.Path.GetFileName(files[h]));
				}
				//---
				if (lst.Count <= 0) {
					this.BAK_FOLDS.RemoveAt(i);
				}
				else {
					this.BAK_FILES.Add(lst.ToArray());
					i++;
				}
				if (i >= this.BAK_FOLDS.Count) {
					break;
				}
			}
		}
		// nameが直近の再測定データかどうか?
		private bool is_tmr(string name)
		{
#if true//2019.05.08(再測定・深度合成)
			if (name.Contains("_ZDEPT")) {
				name = name.Replace("_ZDEPT", "_ZP00D");
			}
#endif
			if (BAK_FILES.Count <= 0) {
				return(false);//再測定データがない場合
			}
			string[] files = this.BAK_FILES[0];
			for (int i = 0; i <files.Length; i++) {
				if (
#if true//2019.08.20(再撮影バグ修正)
				string.Compare(files[i], name, true) == 0
#else
				files[i].Contains(name)
#endif
				) {
					return(true);
				}
			}
			return(false);
		}
		private string[] get_remes_folds(string name, out int cnt)
		{
			List<string> folds = new List<string>();
#if true//2019.05.08(再測定・深度合成)
			if (name.Contains("_ZDEPT")) {
				name = name.Replace("_ZDEPT", "_ZP00D");
			}
#endif
			cnt = 0;
			if (this.BAK_FOLDS.Count != this.BAK_FILES.Count) {
				throw new Exception("Internal Error");
			}
			for (int i = 0; i < this.BAK_FILES.Count; i++) {
				string[] files = this.BAK_FILES[i];
				for (int h = 0; h < files.Length; h++) {
					if (
#if true//2019.08.20(再撮影バグ修正)
					string.Compare(files[h], name, true) == 0
#else
					files[h].Contains(name)
#endif
					) {
						folds.Add(this.BAK_FOLDS[i]);
						cnt++;
						break;
					}
				}
			}
			return(folds.ToArray());
		}
#endif
#if true//2019.05.22(再測定判定(キューティクル枚数))
		//---
		private void call_back00(){}
		private void call_back01(){}
		private void call_back02(Bitmap bmp1, Bitmap bmp2, Bitmap bmp3)
		{
			DIGITI.dispose_img(this.pictureBox1);
			DIGITI.dispose_img(this.pictureBox2);
			DIGITI.dispose_img(this.pictureBox3);
			this.pictureBox2.Image = (Bitmap)bmp3.Clone();
			this.pictureBox1.Image = (Bitmap)bmp1.Clone();
			if (bmp2 != null) {
			this.pictureBox3.Image = (Bitmap)bmp2.Clone();
			}
			this.pictureBox1.Update();
			this.pictureBox2.Update();
			this.pictureBox3.Update();
		}
		private void call_back03(string name_dm1, string name_ir1, int i){}
		private void call_back04(){}
		private void call_back05(){}
		private void call_back06(){}
		private void call_back07(object obj){}

		private void call_back10(object obj, string name_dm1)
		{
			DIGITI.seg_of_hair seg = (DIGITI.seg_of_hair)obj;
			seg.bak_folds = get_remes_folds(name_dm1, out seg.bak_cnt);
			seg.bTMR = is_tmr(name_dm1);
		}
		private void call_back11(int mode_of_cl, object obj, Bitmap m_bmp_dm1, Bitmap m_bmp_pd1)
		{
			DIGITI.seg_of_hair seg = (DIGITI.seg_of_hair)obj;
			calc_contrast(mode_of_cl, seg, m_bmp_dm1, m_bmp_pd1);
		}
		private void call_back12(object obj)
		{
			DIGITI.seg_of_hair[] segs = (DIGITI.seg_of_hair[])obj;
			calc_contrast_avg(segs);
			for (int i = 0; i < segs.Length; i++) {
				add_grid_row(segs[i], m_digi.m_hair.Count, i);
			}
		}
		private void load()
		{
			var dlg = new DlgProgress();

			try {
				int cnt_of_hair = 0;
				dlg.Show("再撮影", G.FORM01);
				G.bCANCEL = false;

				enable_forms(false);
				m_digi.load(
						dlg.SetStatus,
						call_back01, call_back02, call_back03,
						call_back04, call_back05, call_back06,
						call_back07,
						call_back10, call_back11, call_back12
				);
				//---
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}

			if (dlg != null) {
				dlg.Hide();
			    dlg.Dispose();
			    dlg = null;
			}
			this.dataGridView1.Enabled = true;
			if (this.dataGridView1.Rows.Count > 0) {
				this.dataGridView1.Rows[0].Selected = false;
				this.dataGridView1.Rows[0].Selected = true;
			}
			button4_Click_1(null, null);
		}
#endif
#if false//atode kesukoto
#endif
		private void init()
		{
			if (true) {
			this.tableLayoutPanel1.Dock = DockStyle.Fill;
			this.tableLayoutPanel2.Dock = DockStyle.Fill;
			this.tableLayoutPanel3.Dock = DockStyle.Fill;
//@@@		this.groupBox1.Dock = DockStyle.Fill;
//@@@		this.groupBox2.Dock = DockStyle.Fill;
			this.tabControl1.Dock = DockStyle.Fill;
			this.tabControl2.Dock = DockStyle.Fill;
//			this.tabControl3.Dock = DockStyle.Fill;
			this.tabControl1.Dock = DockStyle.Fill;
			this.tabControl2.Dock = DockStyle.Fill;
			//---
			//---
			this.pictureBox1.Dock = DockStyle.Fill;
			this.pictureBox2.Dock = DockStyle.Fill;
			this.pictureBox3.Dock = DockStyle.Fill;
			this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			this.pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
			this.pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;

			this.dataGridView1.Dock = DockStyle.Fill;
			}
			//---
			//---
//@@@			this.comboBox1.Enabled = false;
#if true//2018.11.10(保存機能)
//@@@			this.tabControl1.SelectedIndex = 2;//キューティクル間隔
#endif
			//---
//%%			this.MOZ_CND_FOLD = (G.SS.NGJ_CND_FMOD == 0) ? G.SS.AUT_BEF_PATH: G.SS.NGJ_CND_FOLD;
			//---
			//---
			//this.checkBox10.Checked = (G.SS.MOZ_CND_USIR == 1);
			//---
//@@@			this.tabControl1.TabPages.Remove(this.tabPage2);
//@@@			this.tabControl1.TabPages.Remove(this.tabPage3);
//@@@			this.tabControl1.TabPages.Remove(this.tabPage4);
			this.dataGridView1.Enabled = false;
//			this.dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.Gray;
			this.dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;//Color.LightGray;
			this.dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240,244,249);//Color.DarkGray;

#if true//2019.05.22(再測定判定(キューティクル枚数))
			for (int i = 0; i < G.AS.TBL_F04_CLM.Length; i++) {
				this.dataGridView1.Columns[i].Width = G.AS.TBL_F04_CLM[i];
			}
#else
#if true//2019.05.08(再測定・深度合成)
			for (int i = 0; i < G.AS.TBL_F04_WID.Length; i++) {
				this.dataGridView1.Columns[i].Width = G.AS.TBL_F04_WID[i];
			}
#endif
#endif
			//
			this.Text = this.Text + "[" + m_digi.MOZ_CND_FOLD + "]";
#if true//2019.04.02(再測定表ユーザモード)
			if (G.UIF_LEVL == 0) {
				this.dataGridView1.Columns[C_CLM_KP_CONT].Visible = false;
				this.dataGridView1.Columns[C_CLM_KP_DROP].Visible = false;
#if false//2019.08.26(その他修正)
				this.dataGridView1.Columns[C_CLM_RM_CUNT].Visible = false;//再測定回数
#endif
				//---
#if true//2019.05.22(再測定判定(キューティクル枚数))
				this.panel2.Visible = false;
#else
				this.label1.Visible = false;//中心
				this.numericUpDown2.Visible = false;//中心
				this.label2.Visible = false;//中心
#endif
			}
			if (true) {
				this.dataGridView1.Columns[C_CLM_CHOKKIN].Visible = false;//直近？
			}
			this.comboBox1.SelectedIndex = 0;
			search_bak_dir();
#endif
		}

		private void Form04_Load(object sender, EventArgs e)
		{
			if (true) {
				this.SetDesktopBounds(G.AS.APP_F04_LFT, G.AS.APP_F04_TOP, G.AS.APP_F04_WID, G.AS.APP_F04_HEI);
			}
			if (true) {
				if (G.UIF_LEVL == 0) {
				}
			}
#if true//2019.05.22(再測定判定(キューティクル枚数))
			m_digi.INIT(/*bREMES=*/true);
#endif
			init();
			DDX(true);
			if (true) {
				G.push_imp_para();
			}
			if (true) {
				if (G.FORM02 != null) {
					G.FORM02.Close();
					G.FORM02 = null;
					Application.DoEvents();
				}
				if (G.FORM02 == null) {
					G.FORM02 = new Form02();
				}
				G.CAM_PRC = G.CAM_STS.STS_HAIR;
				
				this.BeginInvoke(new G.DLG_VOID_VOID(this.load));
			}
		}
		private void Form04_FormClosing(object sender, FormClosingEventArgs e)
		{
#if true//2019.05.22(再測定判定(キューティクル枚数))
			m_digi.TERM();
#else
			if (m_hair != null) {
				m_hair.Clear();
				m_hair = null;
			}
			if (m_bmp_dm1 != null) {
				m_bmp_dm1.Dispose();
				m_bmp_dm1 = null;
			}
			if (m_bmp_ir1 != null) {
				m_bmp_ir1.Dispose();
				m_bmp_ir1 = null;
			}
			//---
			dispose_img(this.pictureBox1);
			dispose_img(this.pictureBox2);
			dispose_img(this.pictureBox3);
#endif
#if true//2019.07.27(保存形式変更)
			button2_Click(null, null);
#endif

			//Image tmp1 = this.pictureBox1.Image;
			//Image tmp2 = this.pictureBox2.Image;
			//this.pictureBox1.Image = null;
			//this.pictureBox2.Image = null;
			//if (tmp1 != null) {
			//    tmp1.Dispose();
			//    tmp1 = null;
			//}
			//if (tmp2 != null) {
			//    tmp2.Dispose();
			//    tmp2 = null;
			//}
			if (true) {
				G.pop_imp_para();
			}
#if true//2019.04.02(再測定実装)
			//---
			if (this.Left <= -32000 || this.Top <= -32000) {
				//最小化時は更新しない
			}
			else {
			G.AS.APP_F04_LFT = this.Left;
			G.AS.APP_F04_TOP = this.Top;
			G.AS.APP_F04_WID = this.Width;
			G.AS.APP_F04_HEI = this.Height;
			}
#endif
#if true//2019.05.22(再測定判定(キューティクル枚数))
			for (int i = 0; i < G.AS.TBL_F04_CLM.Length; i++) {
				G.AS.TBL_F04_CLM[i] = this.dataGridView1.Columns[i].Width;
			}
#else
#if true//2019.05.08(再測定・深度合成)
			for (int i = 0; i < G.AS.TBL_F04_WID.Length; i++) {
				G.AS.TBL_F04_WID[i] = this.dataGridView1.Columns[i].Width;
			}
#endif
#endif
			//---
			G.FORM04 = null;
			G.FORM12.UPDSTS();
		}
		private bool DDX(bool bUpdate)
        {
            bool rc;

            try {
				DDV.DDX(bUpdate, this.numericUpDown1 , ref G.SS.REM_BOK_STHD);
				DDV.DDX(bUpdate, this.numericUpDown2 , ref G.SS.REM_BOK_CTHD);
#if true//2019.05.08(再測定・深度合成)
				DDV.DDX(bUpdate, this.numericUpDown3 , ref G.SS.REM_CHG_DTHD);
#endif
#if true//2019.05.22(再測定判定(キューティクル枚数))
				DDV.DDX(bUpdate, this.numericUpDown4 , ref G.SS.REM_CUT_CTHD);
				DDV.DDX(bUpdate, this.numericUpDown5 , ref G.SS.REM_CUT_RTHD);
				DDV.DDX(bUpdate, this.checkBox4      , ref G.SS.REM_CUT_US_C);
				DDV.DDX(bUpdate, this.checkBox5      , ref G.SS.REM_CUT_US_R);
#endif
                rc = true;
            }
            catch (Exception e) {
                G.mlog(e.Message);
                rc = false;
            }
            return (rc);
		}
#if true//2018.09.29(キューティクルライン検出)
		private void draw_marker(Graphics gr, Brush brs, Point pt, int LEN)
		{
			gr.FillRectangle(brs, pt.X-LEN, pt.Y-LEN, LEN*2+1, LEN*2+1);
		}
#endif
#if true//2018.10.10(毛髪径算出・改造)
		private void draw_text(Image img, string txt, float fp=60
			,StringAlignment v_align=StringAlignment.Far
			,StringAlignment h_align=StringAlignment.Far
			,Brush brs = default(Brush)
			)
		{
			Graphics gr = Graphics.FromImage(img);
			Font fnt = new Font("Arial", fp);
			RectangleF rt = new RectangleF(0, 0, img.Width, img.Height);
			StringFormat sf  = new StringFormat();

			if (brs == null) {
				brs = Brushes.LimeGreen;
			}
			sf.Alignment     = h_align;
			sf.LineAlignment = v_align;
			gr.DrawString(txt, fnt, brs, rt, sf);
			gr.Dispose();
		}
#endif
		private void draw_image(DIGITI.hair hr)
		{
			string buf_dm, buf_ir, buf_pd;
			Image bmp_dm = null, bmp_ir = null, bmp_pd = null;
//			int	Z = 8;
			float pw = 5;
			//---
			int idx = m_isel;
			DIGITI.seg_of_hair seg = hr.seg[idx];

			if (false /*this.radioButton3.Checked*/) {
			}
			//---
			DIGITI.dispose_img(this.pictureBox1);
			DIGITI.dispose_img(this.pictureBox2);
			DIGITI.dispose_img(this.pictureBox3);
			if (true) {
			GC.Collect();
			}
			//---
			if (true) {
				//個別表示
				if (true) {
					this.label4.Text = "表面：" + seg.name_of_dm;
					this.label5.Text = "中心：" + seg.name_of_pd;
					this.label6.Text = "赤外：" + seg.name_of_ir;
					buf_dm = /*to_xx_path(*/seg.path_of_dm;//, null/*ZVAL2ORG(this.comboBox10.Text)*/);
					buf_pd = /*to_xx_path(*/seg.path_of_pd;//, null/*ZVAL2ORG(this.comboBox8.Text)*/);
					buf_ir = /*to_xx_path(*/seg.path_of_ir;//, null/*ZVAL2ORG(this.comboBox12.Text)*/);
#if true//2019.05.08(再測定・深度合成)
					buf_dm =   m_digi.to_xx_path(  seg.path_of_dm   , "ZDEPT");
					buf_pd =   m_digi.to_xx_path(  seg.path_of_pd   , "KP00D");
					buf_ir =   m_digi.to_xx_path(  seg.path_of_ir   , "KP00D");
#endif
				}
#if true//2019.04.02(再測定表ユーザモード)
				if (this.comboBox1.SelectedIndex > 0 && seg.bak_cnt > 0) {
					int i = this.comboBox1.SelectedIndex;
					if (i > seg.bak_cnt) {
						i = seg.bak_cnt;
					}
					i--;
					buf_dm = seg.bak_folds[i] + "\\" + seg.name_of_dm;
					buf_pd = seg.bak_folds[i] + "\\" + seg.name_of_pd;
					buf_ir = seg.bak_folds[i] + "\\" + seg.name_of_ir;
				}
#endif
				bmp_dm = m_digi.to_img_from_file(buf_dm);
				bmp_pd = m_digi.to_img_from_file(buf_pd);
				bmp_ir = m_digi.to_img_from_file(buf_ir);
			}


			if (bmp_dm == null || bmp_pd == null || bmp_ir == null) {
				bmp_dm = bmp_dm;
				return;
			}

			if (true/*bmp_ir != null*/) {//赤外あり？
				//object obj = seg.moz_zpb[0];
				//System.Diagnostics.Debug.WriteLine(obj);
				Graphics gr_ir = Graphics.FromImage(bmp_ir);
				Graphics gr_pd = Graphics.FromImage(bmp_pd);
				Graphics gr_dm = Graphics.FromImage(bmp_dm);
				Pen pen = null;
				if (this.checkBox2.Checked
#if true//2019.07.27(保存形式変更)
					&& seg.dex_top != null && seg.dex_btm != null
#endif
					) {//赤外・輪郭
					pen = new Pen(Color.Blue, pw);
					gr_dm.DrawLines(pen, seg.dex_top);//.dia_top);
					gr_pd.DrawLines(pen, seg.dex_top);
					gr_ir.DrawLines(pen, seg.dex_top);
					//---
					gr_dm.DrawLines(pen, seg.dex_btm);
					gr_pd.DrawLines(pen, seg.dex_btm);
					gr_ir.DrawLines(pen, seg.dex_btm);
				}
				if (this.checkBox8.Checked
#if true//2019.07.27(保存形式変更)
					&& seg.dex_cen != null
#endif
					) {//中心ライン
					pen = new Pen(Color.Cyan/*this.chart1.Series[0].Color*/, pw);
					//---
					gr_dm.DrawLines(pen, seg.dex_cen);
					gr_pd.DrawLines(pen, seg.dex_cen);
					gr_ir.DrawLines(pen, seg.dex_cen);
				}
				if (this.checkBox1.Checked
#if true//2019.07.27(保存形式変更)
					&& seg.msk_of_dm != null && seg.msk_of_pd != null
#endif
					) {//コントスラト計算範囲
					pen = new Pen(Color.Red, pw/2);
					if (seg.msk_of_dm.Length > 0) {
						gr_dm.DrawPolygon(pen, seg.msk_of_dm);
					}
					if (seg.msk_of_pd.Length > 0) {
						gr_pd.DrawPolygon(pen, seg.msk_of_pd);
					}
					for (int i = 0; i < seg.msk_of_dm.Length; i++) {
						draw_marker(gr_dm, Brushes.Yellow, seg.msk_of_dm[i], 4);
					}
					for (int i = 0; i < seg.msk_of_pd.Length; i++) {
						draw_marker(gr_pd, Brushes.Yellow, seg.msk_of_pd[i], 5);
					}
				}
				if (this.checkBox12.Checked) {
					//draw_text(bmp_pd, string.Format("直径={0:F1}um", seg.dia_avg));
#if true//2019.05.08(再測定・深度合成)
					draw_text(bmp_pd, string.Format("直径・左={0:F1}um, 右{1:F1}um, 平均={2:F1}, 変化率={3:F1}%", seg.mou_len_l, seg.mou_len_r, seg.dia_avg, seg.mou_len_c), 60, StringAlignment.Far, StringAlignment.Far);
#else
					draw_text(bmp_pd, string.Format("直径・左={0:F1}um, 右{1:F1}um, AVG={2:F1}, CHANGE={3:F1}%", seg.mou_len_l, seg.mou_len_r, seg.dia_avg, seg.mou_len_c), 60, StringAlignment.Far, StringAlignment.Far);
#endif
				}
				if (this.checkBox21.Checked) {
					draw_text(bmp_dm, string.Format("CONTRAST={0:F3}, AVG={1:F3}, DROP={2:F1}%", seg.zp_contr, seg.zp_contr_avg, seg.zp_contr_drop), 60, StringAlignment.Near, StringAlignment.Near);
					draw_text(bmp_pd, string.Format("CONTRAST={0:F3}, AVG={1:F3}, DROP={2:F1}%", seg.kp_contr, seg.kp_contr_avg, seg.kp_contr_drop), 60, StringAlignment.Near, StringAlignment.Near);
				}
				if (true) {
					draw_text(bmp_dm, string.Format("キューティクル枚数={0:F0}, 平均={1:F1}, DROP={2:F1}%", seg.cut_count, seg.cut_avg, seg.cut_drop), 60, StringAlignment.Far, StringAlignment.Near);
				}
			}

			if (true) {
				this.pictureBox1.Image = bmp_dm;
				this.pictureBox2.Image = bmp_pd;
			}
			if (bmp_ir != null) {
				this.pictureBox3.Image = bmp_ir;
			}
		}
#if true//2019.01.11(混在対応)
		private void SWAP_ANL_CND(int mode)
		{
			return;
			int i = (mode == 0) ? 0/*透過/CT/白髪*/: 1/*反射/CR/黒髪*/;
			G.SS.MOZ_CND_CTYP = G.SS.ANL_CND_CTYP[i];
			G.SS.MOZ_CND_BPF1 = G.SS.ANL_CND_BPF1[i];
			G.SS.MOZ_CND_BPF2 = G.SS.ANL_CND_BPF2[i];
			G.SS.MOZ_CND_BPSL = G.SS.ANL_CND_BPSL[i];
			G.SS.MOZ_CND_NTAP = G.SS.ANL_CND_NTAP[i];
			G.SS.MOZ_CND_BPVL = G.SS.ANL_CND_BPVL[i];
			G.SS.MOZ_CND_2DC0 = G.SS.ANL_CND_2DC0[i];
			G.SS.MOZ_CND_2DC1 = G.SS.ANL_CND_2DC1[i];
			G.SS.MOZ_CND_2DC2 = G.SS.ANL_CND_2DC2[i];
			G.SS.MOZ_CND_2DVL = G.SS.ANL_CND_2DVL[i];

			G.SS.MOZ_CND_FTCF = G.SS.ANL_CND_FTCF[i];
			G.SS.MOZ_CND_FTCT = G.SS.ANL_CND_FTCT[i];
			G.SS.MOZ_CND_SMCF = G.SS.ANL_CND_SMCF[i];
			G.SS.MOZ_CND_CNTR = G.SS.ANL_CND_CNTR[i];
			G.SS.MOZ_CND_ZVAL = G.SS.ANL_CND_ZVAL[i];
			G.SS.MOZ_CND_HANI = G.SS.ANL_CND_HANI[i];
			G.SS.MOZ_CND_SLVL = G.SS.ANL_CND_SLVL[i];
			G.SS.MOZ_CND_OTW1 = G.SS.ANL_CND_OTW1[i];
			G.SS.MOZ_CND_OTV1 = G.SS.ANL_CND_OTV1[i];
			G.SS.MOZ_CND_OTW2 = G.SS.ANL_CND_OTW2[i];
			G.SS.MOZ_CND_OTV2 = G.SS.ANL_CND_OTV2[i];
			G.SS.MOZ_CND_OTMD = G.SS.ANL_CND_OTMD[i];
			G.SS.MOZ_CND_SMVL = G.SS.ANL_CND_SMVL[i];
			G.SS.MOZ_CND_CHK1 = G.SS.ANL_CND_CHK1[i];
			G.SS.MOZ_CND_CHK2 = G.SS.ANL_CND_CHK2[i];
			G.SS.MOZ_CND_CHK2 = G.SS.ANL_CND_CHK2[i];

			G.SS.MOZ_CND_CHAN = G.SS.ANL_CND_CHAN[i];
			G.SS.MOZ_CND_CMIN = G.SS.ANL_CND_CMIN[i];

			G.SS.MOZ_CND_CNEI = G.SS.ANL_CND_CNEI[i];
			G.SS.MOZ_CND_HIST = G.SS.ANL_CND_HIST[i];
		}
#endif
#if true//2018.10.10(毛髪径算出・改造)
		private void dataGridView1_SelectionChanged(object sender, EventArgs e)
		{
			int cnt;
			
			if (!this.dataGridView1.Enabled) {
				return;
			}
			if ((cnt = this.dataGridView1.SelectedRows.Count) <= 0) {
				return;
			}
			try {
#if true
				int idx = (int)this.dataGridView1.SelectedRows[0].Tag;
				int h = HIWORD(idx);
				int i = LOWORD(idx);
#endif
				m_i = h;
				m_isel = i;
				DIGITI.hair hr = m_digi.m_hair[m_i];
				draw_image(hr);
			}
			catch (Exception ex) {
				G.mlog(ex.Message);
			}
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			try {
				if (!this.dataGridView1.Enabled) {
					return;
				}
				if (m_i >= m_digi.m_hair.Count) {
					return;
				}
				DIGITI.hair hr = m_digi.m_hair[m_i];
				if (m_isel >= hr.seg.Count()) {
					return;
				}
				//this.button1.Visible = !this.button1.Visible;
				if (true) {
					draw_image(hr);
				}
			}
			catch (Exception ex) {
			}
		}

		private void button4_Click_1(object sender, EventArgs e)
		{
			if (!DDX(false)) {
				return;
			}
			for (int i = 0; i < this.dataGridView1.Rows.Count; i++) {
				double val;
				bool flag1 = false, flag2 = false;
#if true//2019.05.22(再測定判定(キューティクル枚数))
				double	cut_count, cut_drop, zp_contr_drop, kp_contr_drop;
				double	mou_len_c;
				cut_count     = (int)this.dataGridView1.Rows[i].Cells[C_CLM_CT_CONT].Value;
				cut_drop      = (double)this.dataGridView1.Rows[i].Cells[C_CLM_CT_RATE].Value;
				zp_contr_drop = (double)this.dataGridView1.Rows[i].Cells[C_CLM_ZP_DROP].Value;
				kp_contr_drop = (double)this.dataGridView1.Rows[i].Cells[C_CLM_KP_DROP].Value;
				mou_len_c     = (double)this.dataGridView1.Rows[i].Cells[C_CLM_HR_RATE].Value;
				flag1 = check_remes(cut_count, cut_drop, zp_contr_drop, kp_contr_drop);
				flag2 = check_remak(mou_len_c);
#if true//2019.07.27(保存形式変更)
				if ((bool)this.dataGridView1.Rows[i].Cells[C_CLM_DL_FLAG].Value) {
					flag1 = false;
					flag2 = false;
				}
#endif
				this.dataGridView1.Rows[i].Cells[C_CLM_RM_SHOT].Value = flag1;
				this.dataGridView1.Rows[i].Cells[C_CLM_RM_MAKE].Value = flag2;
				if (flag1 || flag2) {
					this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 96, 96);//;Color.Red;
				}
				else {
					this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Empty;
				}
#else
#endif
			}
		}
#endif
		private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
#if true//2019.07.27(保存形式変更)
			bool flag;
			if (e.ColumnIndex == C_CLM_RM_SHOT || e.ColumnIndex == C_CLM_RM_MAKE || e.ColumnIndex == C_CLM_DL_FLAG
#if true//2019.08.08(保存内容変更)
			 || e.ColumnIndex == C_CLM_CH_FLAG
			 || e.ColumnIndex == C_CLM_UN_FLAG
			 || e.ColumnIndex == C_CLM_GM_FLAG
#endif
			) {
				if ((bool)this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) {
					flag = false;
				}
				else {
					flag  = true;
				}
				if (e.ColumnIndex == C_CLM_DL_FLAG) {
					if (flag) {
					this.dataGridView1.Rows[e.RowIndex].Cells[C_CLM_RM_SHOT].Value = false;
					this.dataGridView1.Rows[e.RowIndex].Cells[C_CLM_RM_MAKE].Value = false;
					}
					this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = flag;
#if true//2019.08.08(保存内容変更)
					button2_Click(e.ColumnIndex, null);
#else
					button2_Click(null, null);
#endif
				}
#if true//2019.08.08(保存内容変更)
				else if (e.ColumnIndex == C_CLM_CH_FLAG || e.ColumnIndex == C_CLM_UN_FLAG || e.ColumnIndex == C_CLM_GM_FLAG) {
					this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = flag;
					button2_Click(e.ColumnIndex, null);
				}
#endif
				else {
					bool dflag = (bool)this.dataGridView1.Rows[e.RowIndex].Cells[C_CLM_DL_FLAG].Value;
					if (!dflag) {
					this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = flag;
					}
				}
				
			}
#else
			if (e.ColumnIndex == C_CLM_RM_SHOT || e.ColumnIndex == C_CLM_RM_MAKE) {
				if ((bool)this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) {
					this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = false;
				}
				else {
					this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = true;
				}
#endif
		}
#if true//2019.04.09(再測定実装)
		private int get_offs(string name)
		{
			int		offs;
			string	tmp = name.Substring(name.Length-5);
			string	k_z = tmp.Substring(0, 1);
			int		sgn;
			if (tmp.Substring(1, 1) == "P") {
				sgn = +1;
			}
			else {
				sgn = -1;
			}
			tmp = tmp.Substring(2, 2);
			offs = int.Parse(tmp);
			return(sgn*offs);
		}
		//?CR_??_????D.???
		//?CT_??_????D.???
		//?IR_??_????D.???
		private void add_remes(List<G.RE_MES> remes, string name
#if true//2019.05.08(再測定・深度合成)
													,string path
#endif
		)
		{
			G.RE_MES mes = new G.RE_MES();
			string nam;
			string tmp;
			string[] files;

			if (true) {
				mes.fold = m_digi.MOZ_CND_FOLD;
#if true//2019.05.08(再測定・深度合成)
				mes.path_of_zp = path;
#endif
			}
			if (true) {
				int	p;
				name = name.ToUpper();
				if ((p = name.IndexOf("CR")) >= 1) {
				}
				else if ((p = name.IndexOf("CT")) >= 1) {
				}
				else {
					throw new Exception("Internal Error");
				}
				mes.hno = name.Substring(0, p);
				mes.crt = name.Substring(p, 2);
				p+=2;
				mes.sno = name.Substring(p, 4);
			}
			if (false) {
				files = System.IO.Directory.GetFiles(m_digi.MOZ_CND_FOLD, mes.h_name() + "*.*");
				tmp = "";
				for (int i = 0; i < files.Length; i++) {
					tmp += System.IO.Path.GetFileName(files[i]);
					tmp += "\r";
				}
				G.mlog(tmp);
			}
			files = System.IO.Directory.GetFiles(m_digi.MOZ_CND_FOLD, mes.h_name() + "ZP00D.*");
			if (files.Length == 1) {
				tmp = files[0];
				if (true) {
					//表面ＡＦ画像
					nam = System.IO.Path.GetFileName(tmp);
					mes.name_of_zp.Add(nam);
					mes.offs_of_zp.Add(0);
					//---
					DIGITI.PLS_XYZ pos;
					if (m_digi.m_log_info.map_of_xyz.TryGetValue(nam, out pos)) {
						mes.pls_x = pos.X;
						mes.pls_y = pos.Y;
						mes.pls_z_of_zp = pos.Z;
					}
				}
				files = System.IO.Directory.GetFiles(m_digi.MOZ_CND_FOLD, mes.h_name() + "Z???D.*");
				for (int i = 0; i < files.Length; i++) {
					nam = System.IO.Path.GetFileName(files[i]);
					tmp = System.IO.Path.GetFileNameWithoutExtension(files[i]);
					tmp = tmp.ToUpper();
					if (tmp.IndexOf("ZP00D") >= 0) {
						continue;
					}
					mes.name_of_zp.Add(nam);
					mes.offs_of_zp.Add(get_offs(tmp));					
				}
			}
			files = System.IO.Directory.GetFiles(m_digi.MOZ_CND_FOLD, mes.h_name() + "KP00D.*");
			if (files.Length == 1) {
				tmp = files[0];
				if (true) {
					//中心ＡＦ画像
					nam = System.IO.Path.GetFileName(tmp);
					mes.name_of_kp.Add(nam);
					mes.offs_of_kp.Add(0);
					//---
					DIGITI.PLS_XYZ pos;
					if (m_digi.m_log_info.map_of_xyz.TryGetValue(nam, out pos)) {
					//	mes.pls_x = pos.X; //XとYはZPとKPで同じ,Zのみ異なる
					//	mes.pls_y = pos.Y;
						mes.pls_z_of_kp = pos.Z;
					}
				}
				files = System.IO.Directory.GetFiles(m_digi.MOZ_CND_FOLD, mes.h_name() + "K???D.*");
				for (int i = 0; i < files.Length; i++) {
					nam = System.IO.Path.GetFileName(files[i]);
					tmp = System.IO.Path.GetFileNameWithoutExtension(files[i]);
					tmp = tmp.ToUpper();
					if (tmp.IndexOf("KP00D") >= 0) {
						continue;
					}
					mes.name_of_kp.Add(nam);
					mes.offs_of_kp.Add(get_offs(tmp));					
				}
			}
			//該当のＩＲファイルの有無をチェック
			if (true) {
				for (int i = 0; i < mes.name_of_zp.Count; i++) {
					tmp = m_digi.MOZ_CND_FOLD + "\\" + mes.name_of_zp[i];
					tmp = m_digi.to_ir_file(tmp);
					if (System.IO.File.Exists(tmp)) {
						mes.name_of_zr.Add(System.IO.Path.GetFileName(tmp));
					}
					else {
						mes.name_of_zr.Add(null);
					}
				}
			}
			if (true) {
				for (int i = 0; i < mes.name_of_kp.Count; i++) {
					tmp = m_digi.MOZ_CND_FOLD + "\\" + mes.name_of_kp[i];
					tmp = m_digi.to_ir_file(tmp);
					if (System.IO.File.Exists(tmp)) {
						mes.name_of_kr.Add(System.IO.Path.GetFileName(tmp));
					}
					else {
						mes.name_of_kr.Add(null);
					}
				}
			}
			remes.Add(mes);
		}
		private void button1_Click(object sender, EventArgs e)
		{//再測定
#if true//2019.05.08(再測定・深度合成)
			List<int> la = new List<int>();
			List<string> ls = new List<string>();

			for (int q = 0; q < this.dataGridView1.Rows.Count; q++) {
				bool flag = false;
				flag = (bool)this.dataGridView1.Rows[q].Cells[C_CLM_RM_MAKE].Value;
				if (flag) {
					int idx = (int)this.dataGridView1.Rows[q].Tag;
					int h = HIWORD(idx);
					int i = LOWORD(idx);
					la.Add(h+1);
					/*
					m_i = h;
					m_isel = i;
					hair hr = m_hair[m_i];
					seg_of_hair seg = (seg_of_hair)hr.seg[m_isel];
					seg.bREMES = flag;*/
				}
			}
#else
			if (!D.isCONNECTED() || !G.FORM11.isORG_ALL_DONE()) {
				G.mlog("CONNECT及び原点復帰をしてください.");
				return;
			}
#endif
			for (int q = 0; q < this.dataGridView1.Rows.Count; q++) {
				bool flag = false;
				flag = (bool)this.dataGridView1.Rows[q].Cells[C_CLM_RM_SHOT].Value;
				if (true) {
					int idx = (int)this.dataGridView1.Rows[q].Tag;
					int h = HIWORD(idx);
					int i = LOWORD(idx);

					m_i = h;
					m_isel = i;
					DIGITI.hair hr = m_digi.m_hair[m_i];
					DIGITI.seg_of_hair seg = hr.seg[m_isel];
#if true//2019.05.08(再測定・深度合成)
					if (flag && la.Contains(h+1)) {
						ls.Add(seg.name_of_dm);
						flag = false;
					}
#endif
					seg.bREMES = flag;
				}
			}
#if true//2019.05.08(再測定・深度合成)
			if (la.Count > 0) {
				string buf;
				buf = "#i次の毛髪サンプルを再作成してください.\r\r";
				for (int i = 0; i < la.Count; i++) {
					if (i > 0) {
					buf += ", ";
					}
					buf += string.Format("{0}本目", la[i]);
				}
				G.mlog(buf);
			}
			if (ls.Count > 0) {
				string buf;
				buf = "#i再作成対象の毛髪サンプルに含まれている再撮影対象の次の画像は再撮影かスキップされます。\r";
				for (int i = 0; i < ls.Count; i++) {
					buf += "\r";
					buf += ls[i];
				}
				G.mlog(buf);
			}
#endif
			//---
			G.REMES.Clear();
			//---
			for (int q = 0; q < m_digi.m_hair.Count; q++) {
				for (int i = 0; i < m_digi.m_hair[q].seg.Count(); i++) {
					//G.mlog("seg.name_of_dm:\r\r" + seg.name_of_dm);
					if (m_digi.m_hair[q].seg[i].bREMES) {
#if true//2019.05.08(再測定・深度合成)
						add_remes(G.REMES, m_digi.m_hair[q].seg[i].name_of_dm, m_digi.m_hair[q].seg[i].path_of_dm);
#else
						add_remes(G.REMES, m_hair[q].seg[i].name_of_dm);
#endif
					}
				}
			}
#if true//2019.05.08(再測定・深度合成)
			string tmp = m_digi.m_hair[0].seg[0].path_of_dm;
			if (ls.Count > 0 && G.REMES.Count <= 0) {
				return;//メッセージ表示済み
			}
#endif
			if (G.REMES.Count <= 0) {
				G.mlog("再撮影対象のチェック項目をＯＮしてください.");
				return;
			}
#if true//2019.05.08(再測定・深度合成)
			if (!D.isCONNECTED() || !G.FORM11.isORG_ALL_DONE()) {
				G.mlog("CONNECT及び原点復帰をしてください.");
				return;
			}
#endif
			if (true) {
				G.FORM12.BeginInvoke(new G.DLG_VOID_VOID(G.FORM12.do_re_mes));
			}
		}
#endif
#if true//2019.04.02(再測定表ユーザモード)
		private void checkBox3_CheckedChanged(object sender, EventArgs e)
		{
			for (int q = 0; q < this.dataGridView1.Rows.Count; q++) {
				bool flag = (bool)this.dataGridView1.Rows[q].Cells[C_CLM_CHOKKIN].Value;
				if (this.checkBox3.Checked) {
					this.dataGridView1.Rows[q].Visible = flag;
				}
				else {
					this.dataGridView1.Rows[q].Visible = true;
				}
				if (this.checkBox6.Checked && flag) {
					this.dataGridView1.Rows[q].DefaultCellStyle.BackColor = Color.LimeGreen;
				}
				else {
					this.dataGridView1.Rows[q].DefaultCellStyle.BackColor = Color.Empty;
				}
			}
		}
#endif
#if true//2019.05.22(再測定判定(キューティクル枚数))
		private void checkBox4_CheckedChanged(object sender, EventArgs e)
		{
			this.numericUpDown4.Enabled = (this.checkBox4.Checked);
			this.numericUpDown5.Enabled = (this.checkBox5.Checked);
		}
#endif
#if true//2019.08.08(保存内容変更)
		private void button2_Click(object sender, EventArgs e)
		{
			int CLM_IDX = -1;
			List<int> clms = new List<int>() {C_CLM_DL_FLAG, C_CLM_CH_FLAG, C_CLM_UN_FLAG, C_CLM_GM_FLAG};
		//	List<string> file = new List<string>() {"flag_del", "flag_hakuri", "flag_uneri", "flag_gomi"};
			List<string> flgs;
			if (sender != null && sender.GetType() == typeof(int)) {
				CLM_IDX = (int)sender;
			}
			for (int i = 0; i < clms.Count; i++) {
				if (CLM_IDX != -1 && clms[i] != CLM_IDX) {
					continue;
				}
				int iclm = clms[i];
				string file;
				switch (iclm) {
				case C_CLM_DL_FLAG:	flgs = m_digi.m_flag_del   ; file = "flag_del.txt"; break;
				case C_CLM_CH_FLAG:	flgs = m_digi.m_flag_hakuri; file = "flag_hakuri.txt"; break;
				case C_CLM_UN_FLAG:	flgs = m_digi.m_flag_uneri ; file = "flag_uneri.txt"; break;
				case C_CLM_GM_FLAG:	flgs = m_digi.m_flag_gomi  ; file = "flag_gomi.txt"; break;
				default:throw new Exception("Internal Error");
				}
				var l = get_del_list(iclm);
				if (!G.is_equ(l, flgs)) {
					G.save_txt(m_digi.MOZ_CND_FOLD + "\\" + file, l);
					flgs.Clear();
					flgs.AddRange(l);
				}
			}
		}
#else
#if true//2019.07.27(保存形式変更)
		private void button2_Click(object sender, EventArgs e)
		{
			var l = get_del_list();
			if (!G.is_equ(l, m_digi.m_del_flag)) {
				//if (G.mlog("#q削除指定が変更されました.保存しますか?") == System.Windows.Forms.DialogResult.Yes) {
					G.save_txt(m_digi.MOZ_CND_FOLD + "\\delflag.txt", l);
					m_digi.m_del_flag.Clear();
					m_digi.m_del_flag.AddRange(l);
				//}
			}
		}
#endif
#endif
	}
}
