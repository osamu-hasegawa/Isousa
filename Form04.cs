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
		private int m_i = 0;
		private int m_isel = 0;
		private string MOZ_CND_FOLD;
		//---
		private List<hair> m_hair = new List<hair>();
		//---
		private Bitmap	m_bmp_dm1;
		private Bitmap	m_bmp_ir1;
		private Bitmap	m_bmp_pd1;
		private Point[] m_dia_top;
		private Point[]	m_dia_btm;
		private int		m_dia_cnt;
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
		//
		public class seg_of_hair {
			public string path_of_dm;
			public string path_of_ir;
			public string path_of_pd;// 位置検出用画像ファイルパス
			public string name_of_dm;// 1CL_00.PNG(パスを含まない、拡張子含むファイル名)
			public string name_of_ir;// 1IR_00.PNG
			public string name_of_pd;// 
			public PointF pix_pos;	//画像の座標(ピクセル座標系で毛髪全体を通しての)
			public int	width;		//当該画像のサイズ
			public int	height;		//当該画像のサイズ
			public int total_idx;
#if true//2019.04.09(再測定実装)
			public bool bREMES;
#endif
#if true//2019.03.16(NODATA対応)
			public double zp_contr;
			public double zp_contr_avg;
			public double zp_contr_drop;
			public bool zp_nodata;
#endif
#if true//2019.03.22(再測定表)
			public double kp_contr;
			public double kp_contr_avg;
			public double kp_contr_drop;
			public bool kp_nodata;
			//---
			public double mou_len_l;
			public double mou_len_r;
			public double mou_len_c;

			public Point[]	msk_of_dm;//コントラスト計算多曲線
			public Point[]	msk_of_pd;//コントラスト計算多曲線
#endif
			//---
			//---
#if true//2018.10.10(毛髪径算出・改造)
			public int		IR_PLY_XMIN;
			public int		IR_PLY_XMAX;
			public int		IR_WIDTH;
			//---
			public double	dia_avg;//毛髪直径の平均
			//---
			public Point[]	dia_top;//輪郭・頂点(上側)
			public Point[]	dia_btm;//輪郭・頂点(下側)

			public Point[]	dex_top;//輪郭・頂点(上側)・画像端まで補外
			public Point[]	dex_btm;//輪郭・頂点(下側)
			public Point[]	dex_cen;
#endif
			public int		dia_cnt;//輪郭・頂点数
			//---
			public seg_of_hair() {
			}
		};

		class hair {
			public int	cnt_of_seg;
			public seg_of_hair[]	seg;
#if true//2019.01.11(混在対応)
			public int mode_of_cl;//0:透過, 1:反射
#endif
			public string[] name_of_cl;
			public string[] name_of_ir;
			public double width_of_hair;
			public double height_of_hair;
			public hair() {
				this.cnt_of_seg = 0;
				this.seg = null;
				this.name_of_cl = null;
				this.name_of_ir = null;
				this.width_of_hair = 0;
				this.height_of_hair = 0;
			}
		};


		double px2um(PointF p1, PointF p2)
		{
			double df = G.diff(p1, p2);
			double um = G.PX2UM(df, m_log_info.pix_pitch, m_log_info.zoom);

			return(um);
		}
		double px2um(double df)
		{
			double um = G.PX2UM(df, m_log_info.pix_pitch, m_log_info.zoom);

			return(um);
		}
		//---
		private void test_pr1(seg_of_hair seg)
		{
			List<Point> at = new List<Point>();
			List<Point> ab = new List<Point>();
			Point	at_bak = G.IR.DIA_TOP[0];
			Point	ab_bak = G.IR.DIA_BTM[0];

			for (int i = 0; i < (G.IR.DIA_CNT-1); i++) {
				if (G.IR.DIA_TOP[i].X < at_bak.X || G.IR.DIA_BTM[i].X < ab_bak.X) {
					continue;
				}
				at.Add(at_bak = G.IR.DIA_TOP[i]);
				ab.Add(ab_bak = G.IR.DIA_BTM[i]);
			}
			m_dia_top = at.ToArray();
			m_dia_btm = ab.ToArray();
			m_dia_cnt = m_dia_top.Count();
#if true//2018.10.10(毛髪径算出・改造)
			seg.dia_top = (Point[])m_dia_top.Clone();//輪郭・頂点(上側)
			seg.dia_btm = (Point[])m_dia_btm.Clone();//輪郭・頂点(下側)
			seg.dia_cnt = m_dia_cnt;//輪郭・頂点数
			double avg = 0;
			for (int i = 0; i < seg.dia_cnt; i++) {
			avg+= G.diff(seg.dia_top[i], seg.dia_btm[i]);
			}
			seg.dia_avg = px2um(avg/seg.dia_cnt);
			//---
			seg.IR_PLY_XMIN = G.IR.PLY_XMIN;
			seg.IR_PLY_XMAX = G.IR.PLY_XMAX;
			seg.IR_WIDTH    = G.IR.WIDTH;
#endif
			if (true) {
				List<Point> ac = new List<Point>();
				Point pt = new Point();
				Point pb = new Point();
				FN1D ft, fb;
				int l = m_dia_top.Length;
				//---
				ft = new FN1D(m_dia_top[0], m_dia_top[1]);
				fb = new FN1D(m_dia_btm[0], m_dia_btm[1]);

				pt.X = 0;
				pt.Y = (int)ft.GetYatX(pt.X);
				pb.X = 0;
				pb.Y = (int)fb.GetYatX(pt.X);
				at.Insert(0, pt);
				ab.Insert(0, pb);
				//---
				ft = new FN1D(m_dia_top[l-2], m_dia_top[l-1]);
				fb = new FN1D(m_dia_btm[l-2], m_dia_btm[l-1]);

				pt.X = G.IR.WIDTH-1;
				pt.Y = (int)ft.GetYatX(pt.X);
				pb.X = G.IR.WIDTH-1;
				pb.Y = (int)fb.GetYatX(pt.X);
				at.Add(pt);
				ab.Add(pb);
				//---
				for (int i = 0; i < at.Count; i++) {
					ac.Add(new Point((at[i].X+ab[i].X)/2, (at[i].Y+ab[i].Y)/2));
				}
				//---
				seg.dex_top = at.ToArray();//輪郭・頂点(上側)
				seg.dex_btm = ab.ToArray();//輪郭・頂点(下側)
				seg.dex_cen = ac.ToArray();//輪郭・頂点(中央)
			}
			//---
		}
		private void test_pr0(seg_of_hair seg, bool b1st)
		{
			string key = seg.name_of_dm;
			Point	pnt_of_pls;
			bool ret;
#if true//2018.08.21
			key = key.ToUpper();
#endif
			if (key.Contains("ZDEPT")) {
				key = key.Replace("ZDEPT", "ZP00D");
			}

			if ((ret = m_log_info.map_of_pos.TryGetValue(key, out pnt_of_pls))) {
			}
			else {
				key = key.Substring(0, key.Length-4);//拡張子カット
				ret = m_log_info.map_of_pos.TryGetValue(key, out pnt_of_pls);
			}

			if (ret) {
				pnt_of_pls.X = -pnt_of_pls.X;
				if (b1st) {
					m_log_info.pls_org.X = m_log_info.pls_org.Y = 0;
					m_log_info.pls_org = pnt_of_pls;
				}
				//if (m_log_info.pls_org.X == 0 &&m_log_info.pls_org.Y == 0) {
				//    m_log_info.pls_org = pnt_of_pls;
				//}
				double x, y;
				
				x = (pnt_of_pls.X - m_log_info.pls_org.X);		//[pls]
				x = x * m_log_info.stg_pitch;					//[um ] = [um/pls]*[pls]
				x = x / (m_log_info.pix_pitch/m_log_info.zoom);	//[pix] = [um]/[um/pix]
				//---
				y = (pnt_of_pls.Y - m_log_info.pls_org.Y);		//[pls]
				y = y * m_log_info.stg_pitch;					//[um ] = [um/pls]*[pls]
				y = y / (m_log_info.pix_pitch/m_log_info.zoom);	//[pix] = [um]/[um/pix]
				//---
				seg.pix_pos.X = (float)x;
				seg.pix_pos.Y = (float)y;
			}
		}
		ArrayList m_ah_cl = new ArrayList();
		ArrayList m_ah_ir = new ArrayList();
		ArrayList m_rst = new ArrayList();
#if true//2019.04.09(再測定実装)
		struct PLS_XYZ {
			public int X, Y, Z;
			public PLS_XYZ(int X, int Y, int Z) {
				this.X = X;
				this.Y = Y;
				this.Z = Z;
			}
		};
#endif
		struct log_info {
			public Point pls_org;
			public double stg_pitch;	//[um/pls]
			public double pix_pitch;	//[um/pix]
			public double zoom;
			//---
			public Dictionary<string, Point> map_of_pos;
#if true//2019.04.09(再測定実装)
			public Dictionary<string, PLS_XYZ> map_of_xyz;
#endif
		};
		log_info m_log_info;
		//---
		private void test_log()
		{
			string path = this.MOZ_CND_FOLD + "\\log.csv";
			string buf;
			string[] clms;
			StreamReader sr;
#if true//2019.04.09(再測定実装)
			m_log_info.map_of_xyz = new Dictionary<string,PLS_XYZ>();
#endif
			m_log_info.map_of_pos = new Dictionary<string,Point>();
			m_log_info.zoom = 8;
			m_log_info.stg_pitch = 2.5;		//[um/pls]
			m_log_info.pix_pitch = 2.2;		//[um/pix]
			try {
				sr = new StreamReader(path, Encoding.Default);

				while (!sr.EndOfStream) {
					buf = sr.ReadLine();
					clms = buf.Split(',');
					if (false) {
					}
					else if (clms.Length >= 5 && clms[4].Contains("画像保存:")) {
						string key = clms[4].Substring(5);
						int ptx, pty;
						if (int.TryParse(clms[1], out ptx) && int.TryParse(clms[2], out pty)) {
							Point pt = new Point(ptx, pty);
							m_log_info.map_of_pos.Add(key, pt);
						}
#if true//2019.04.09(再測定実装)
						int ptz;
						if (int.TryParse(clms[1], out ptx) && int.TryParse(clms[2], out pty) &&  int.TryParse(clms[3], out ptz)) {
							PLS_XYZ pt = new PLS_XYZ(ptx, pty, ptz);
							m_log_info.map_of_xyz.Add(key, pt);
						}
#endif
					}
					else if (buf.Contains("ZOOM軸(pls/倍)") && clms.Length >= 2) {
						string tmp = clms[1];
						int	i = tmp.IndexOf("/x");
						double f;
						if (double.TryParse(tmp.Substring(i+2), out f)) {
							m_log_info.zoom = f;
						}
					}
					else if (buf.Contains("ステージピッチ(um/pls)") && clms.Length >= 2) {
						int n;
						if (int.TryParse(clms[1], out n)) {
							m_log_info.stg_pitch = n;
						}
					}
					else if (buf.Contains("画素ピッチ(um/pxl)") && clms.Length >= 2) {
						double f;
						if (double.TryParse(clms[1], out f)) {
							m_log_info.pix_pitch = f;
						}
					}
				}
			}
			catch (Exception ex) {
			}
			//this.label6.Text = string.Format("x {0:F1}", m_log_info.zoom);
		}
		//---
		private string to_ir_file(string path)
		{
			string fold = System.IO.Path.GetDirectoryName(path);
			string name = System.IO.Path.GetFileName(path);
			string buf = null;


			if (string.IsNullOrEmpty(name)) {
				return(null);
			}
			if (name.Contains("CT")) {
				buf = name.Replace("CT", "IR");
			}
			else if (name.Contains("CR")) {
				buf = name.Replace("CR", "IR");
			}
			else {
				buf = name.Replace("CL", "IR");
			}
#if false//2019.04.01(表面赤外省略)
			if (!System.IO.File.Exists(fold+"\\"+buf)) {
				//buf = null;
				return(null);
			}
#endif
			return(fold+"\\"+buf);
		}
#if true//2018.08.21
		private Image to_img_from_file(string path)
		{
			Image img = null;
			if (System.IO.File.Exists(path)) {
				try {
					img = Bitmap.FromFile(path);
				}
				catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine(ex.Message);
				}
			}
			return (img);
		}
		private string to_xx_path(string path, string zpos)
		{
			string fold, name, buf, pext = "";
			string file;

			fold = System.IO.Path.GetDirectoryName(path);
			fold = this.MOZ_CND_FOLD;
			name = System.IO.Path.GetFileName(path);

			if (string.IsNullOrEmpty(name)) {
				return (null);
			}
			if (zpos == "深度合成" || zpos == "ZDEPT") {
				throw new Exception("Internal Error");
			}
			// '_ZP99D', '_ZM99D', '_ZDEPT'
			if (name.Contains("_ZDEPT")) {
				throw new Exception("Internal Error");
			}
#if true//2018.11.13(毛髪中心AF)
			else if (name.Contains("_K")) {
				buf = Regex.Replace(name, "_K.[0-9][0-9].", "_" + zpos);
			}
#endif
			else {
				buf = Regex.Replace(name, "_Z.[0-9][0-9].", "_" + zpos);
			}
			//
			file = System.IO.Path.Combine(fold, pext, buf);//fold + "\\" + buf
			//
			if (!System.IO.File.Exists(file)) {
				return (null);
			}
			return (file);
		}
		// k=0(キューティクル/断面用), 1(毛髪検出/毛髪径用), 2(毛髄用) 
		private string to_xx_file(int k, string path)
		{
			string fold, name, buf, zpos, pext = "";
			string file;
			switch (k) {
			case  0:zpos = "ZP00D"; break;
			case  1:zpos = "KP00D"; break;
			default:zpos = "KP00D"; path = to_ir_file(path); break;
			}
			fold = System.IO.Path.GetDirectoryName(path);
			name = System.IO.Path.GetFileName(path);

			if (string.IsNullOrEmpty(name)) {
				return (null);
			}
			// '_ZP99D', '_ZM99D', '_ZDEPT'
			if (false) {
			}
#if true//2018.11.13(毛髪中心AF)
			else if (name.Contains("_K")) {
				buf = Regex.Replace(name, "_K.[0-9][0-9].", "_" + zpos);
			}
#endif
			else {
				buf = Regex.Replace(name, "_Z.[0-9][0-9].", "_" + zpos);
			}
			//
			file = System.IO.Path.Combine(fold, pext, buf);//fold + "\\" + buf
			file = fold + pext + "\\" + buf;
			//
			if (!System.IO.File.Exists(file)) {
				return (null);
			}
			return (file);
		}
#endif
		private void dispose_bmp(ref Bitmap bmp)
		{
			if (bmp != null) {
				bmp.Dispose();
				bmp = null;
			}
			else {
				bmp = bmp;
			}
		}
		private void dispose_bmp(ref Image img)
		{
			if (img != null) {
				img.Dispose();
				img = null;
			}
			else {
				img = img;
			}
		}
		private void dispose_img(PictureBox pic)
		{
			Image img = pic.Image;
			pic.Image = null;
			if (img != null) {
				img.Dispose();
				img = null;
			}
			else {
				img = img;
			}
		}
		private string get_name_of_path(string path)
		{
			string name = "";
			if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path)) {
				name = System.IO.Path.GetFileName(path);	
			}
			return(name);
		}
		private void enable_forms(bool b)
		{
		}
		private int get_hair_cnt(string pext, string zpos)
		{
			int cnt = 0;
#if true
			for (int q = 0; q < 24; q++) {
				string buf = q.ToString();
				string[] files_cr =
					System.IO.Directory.GetFiles(this.MOZ_CND_FOLD + pext, buf + "CR_??"+zpos+".*");
				string[] files_ct =
					System.IO.Directory.GetFiles(this.MOZ_CND_FOLD + pext, buf + "CT_??"+zpos+".*");
				string[] files_ir =
					System.IO.Directory.GetFiles(this.MOZ_CND_FOLD + pext, buf + "IR_??"+zpos+".*");

				if (files_ct.Length <= 0 && files_cr.Length <= 0) {
					continue;
				}
				cnt++;
			}
#endif
			return(cnt);
		}
		//---
		private void calc_contrast(int mode_of_cl, seg_of_hair seg, Bitmap bmp_dm = null, Bitmap bmp_pd = null)
		{
			string tmp;
			G.CAM_STS bak = G.CAM_PRC;
			Bitmap bmp;

			if (true) {
				if (bmp_pd == null) {
					bmp = new Bitmap(seg.path_of_pd);//中心(KP00D)
				}
				else {
					bmp = bmp_pd;
				}
				G.CNT_NO_CONTOURS = false;
				G.CNT_MOD = G.AFMD2N(G.SS.MOZ_BOK_AFMD[mode_of_cl+2]);	//中心:コントスラト計算範囲
				G.CNT_OFS = G.SS.MOZ_BOK_COFS[mode_of_cl];				//中心:上下オフセット
#if true//2019.03.22(再測定表)
				G.CNT_MET = G.SS.MOZ_BOK_CMET[mode_of_cl+2];			//中心:計算方法
#else
				G.CNT_USSD= G.SS.MOZ_BOK_USSD[mode_of_cl+2];			//中心:標準偏差
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
					seg.mou_len_l = px2um(seg.mou_len_l);
					//---
					seg.mou_len_r = G.diff(G.IR.DIA_TOP[h], G.IR.DIA_BTM[h]);
					seg.mou_len_r = px2um(seg.mou_len_r);
				}
				else {
					seg.mou_len_l = double.NaN;
					seg.mou_len_r = double.NaN;
				}
				if (bmp_pd == null) {
					bmp.Dispose();
				}
			}

			if (true) {
				if (bmp_dm == null) {
					tmp = to_xx_path(seg.path_of_dm, "ZP00D");//表面(ZP00D)
					bmp = new Bitmap(tmp);
				}
				else {
					bmp = bmp_dm;
				}
				//mode_of_cl=0:透過, 1:反射
				G.CNT_NO_CONTOURS = true;
				G.CNT_MOD = G.AFMD2N(G.SS.MOZ_BOK_AFMD[mode_of_cl+0]);	//表面:コントスラト計算範囲
				G.CNT_OFS = G.SS.MOZ_BOK_SOFS[mode_of_cl];				//表面:上下オフセット
#if true//2019.03.22(再測定表)
				G.CNT_MET = G.SS.MOZ_BOK_CMET[mode_of_cl+0];			//表面:計算方法
#else
				G.CNT_USSD= G.SS.MOZ_BOK_USSD[mode_of_cl+0];			//表面:標準偏差
#endif
				G.CAM_PRC = G.CAM_STS.STS_HIST;
				G.FORM02.load_file(bmp, false);
				seg.zp_contr = G.IR.CONTRAST;
				seg.zp_contr_drop = double.NaN;
				seg.zp_contr_avg = double.NaN;
				seg.msk_of_dm = G.IR.MSK_PLY.Take(G.IR.MSK_PLY_CNT).ToArray();
				//---
				if (bmp_dm == null) {
					bmp.Dispose();
					bmp = null;
				}
			}
			//---
			G.CNT_NO_CONTOURS = false;
			G.CAM_PRC = bak;
		}
		//---
		private void calc_contrast_avg(seg_of_hair[] segs)
		{
			double kp_avg = 0, zp_avg = 0;

			for (int i = 0; i < segs.Length; i++) {
				kp_avg += segs[i].kp_contr;
				zp_avg += segs[i].zp_contr;
			}
			kp_avg /= segs.Length;
			zp_avg /= segs.Length;
			for (int i = 0; i < segs.Length; i++) {
				segs[i].kp_contr_drop = -(segs[i].kp_contr - kp_avg) / kp_avg * 100;
				segs[i].kp_contr_avg = kp_avg;
				segs[i].kp_nodata = (segs[i].kp_contr_drop >= G.SS.MOZ_BOK_CTHD);
				//---
				segs[i].zp_contr_drop = -(segs[i].zp_contr - zp_avg) / zp_avg * 100;
				segs[i].zp_contr_avg = zp_avg;
				segs[i].zp_nodata = (segs[i].zp_contr_drop >= G.SS.MOZ_BOK_CTHD);
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
		//private m_idx;
		private void add_grid_row(seg_of_hair seg, int h_idx, int s_idx)
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
			objs.Add(seg.kp_contr);
			objs.Add(seg.kp_contr_drop);
			objs.Add(seg.mou_len_l);
			objs.Add(seg.mou_len_r);
			objs.Add(seg.mou_len_c);
#if true//2019.04.09(再測定実装)
			objs.Add((seg.zp_contr_drop >= G.SS.REM_BOK_STHD) ? true: false);
#else
			objs.Add(false);
#endif
			idx = this.dataGridView1.Rows.Add(objs.ToArray());
			this.dataGridView1.Rows[idx].HeaderCell.Value = (idx+1).ToString();
			this.dataGridView1.Rows[idx].Tag = MAKELONG(s_idx, h_idx);
		}
		//---
		private void load()
		{
			var dlg = new DlgProgress();
			try {
				int cnt_of_hair = 0;
				int mode_of_cl;//0:透過, 1:反射
				string zpos = "ZP00D";
				string pext = "";

				dlg.Show("再測定", G.FORM01);
				G.bCANCEL = false;
				if (true) {
					zpos = "_" + zpos;
				}

				enable_forms(false);

				if (true) {
					G.CAM_PRC = G.CAM_STS.STS_HAIR;
				}
				test_log();
				//---
				cnt_of_hair = get_hair_cnt(pext, zpos);
				//---
				for (int q = 0; q < 24; q++) {
					string[] files_ct, files_cr, files_cl, files_ir;
					string[] files_pd, files_dm;

					string buf = q.ToString();
					int cnt_of_seg;

					files_ct = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD+pext,  buf +  "CT_??"+zpos+".*");
					files_cr = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD+pext,  buf +  "CR_??"+zpos+".*");
					files_ir = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD+pext,  buf +  "IR_??"+zpos+".*");
					if (files_ct.Length <= 0 && files_cr.Length <= 0) {
						continue;
					}
					if (files_ct.Length > 0 && files_cr.Length > 0) {
						break;//終了(反射と透過が混在！)
					}
					if (files_ct.Length > 0) {
						files_cl = files_ct;//透過
						G.set_imp_param(/*透過*/3, -1);
						SWAP_ANL_CND(mode_of_cl = 0);//0:透過, 1:反射
					}
					else {
						files_cl = files_cr;//反射
						G.set_imp_param(/*反射*/4, -1);
						SWAP_ANL_CND(mode_of_cl = 1);//0:透過, 1:反射
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
					files_dm = files_cl;
					//---
					//---
					var hr = new hair();
					var ar_seg = new ArrayList();
					seg_of_hair[] segs = null;
					bool bFileExist = true;

					if (q == 10) {
						q = q;
					}
					for (int i = 0; i < cnt_of_seg; i++) {
	#if true//2018.08.21
						string path_dm1 = to_xx_file(0, files_dm[i]);
						string path_ir1 = to_xx_file(2, files_dm[i]);
						string path_pd1 = to_xx_file(1, files_dm[i]);
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
						seg.path_of_pd = path_pd1;
						seg.path_of_ir = path_ir1;
						//---
						seg.name_of_dm = name_dm1;
						seg.name_of_pd = name_pd1;
						seg.name_of_ir = name_ir1;
						//---
	dlg.SetStatus(string.Format("計算中 {0}/{1}\r{2}/{3}本", i+1, cnt_of_seg, m_hair.Count+1, cnt_of_hair));
						//---
						test_pr0(seg, /*b1st=*/(i==0));
	//test_pr1(segs[i]);
						ar_seg.Add(seg);
						//---
	#if false
						calc_contrast(mode_of_cl, seg);
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
						//this.listView1.LargeImageList = hr.il_dm;
						//this.listView2.LargeImageList = hr.il_ir;
					}
	#if false
					calc_contrast_avg(segs);
	#endif
					for (int i = 0; i < segs.Length; i++) {
	#if true//2018.11.28(メモリリーク)
						//GC.Collect();
	#endif
						dlg.SetStatus(string.Format("計算中 {0}/{1}\r{2}/{3}本", i+1, segs.Length, m_hair.Count+1, cnt_of_hair));
						Application.DoEvents();
						if (G.bCANCEL) {
							break;
						}
						//---
						string path_dm1 = segs[i].path_of_dm;
						string path_pd1 = segs[i].path_of_pd;
						string path_ir1 = segs[i].path_of_ir;
						string name_dm1 = segs[i].name_of_dm;
						string name_ir1 = segs[i].name_of_ir;
						string name_pd1 = segs[i].name_of_pd;
	#if true//2018.08.21
						m_bmp_dm1 = new Bitmap(segs[i].path_of_dm);//表面(ZP00D)
						m_bmp_pd1 = new Bitmap(segs[i].path_of_pd);//中心(KP00D)
						m_bmp_ir1 = new Bitmap(segs[i].path_of_ir);//赤外(KP00D)
	#endif
						if (true) {
							segs[i].width = m_bmp_dm1.Width;
							segs[i].height = m_bmp_dm1.Height;
							test_pr0(segs[i], /*b1st=*/(i==0));
						}
						//---
						//---
						//---
						if (true/*m_hair.Count == 0*/) {
							dispose_img(this.pictureBox1);
							dispose_img(this.pictureBox2);
							dispose_img(this.pictureBox3);
							this.pictureBox2.Image = (Bitmap)m_bmp_pd1.Clone();
							this.pictureBox1.Image = (Bitmap)m_bmp_dm1.Clone();
							if (m_bmp_ir1 != null) {
							this.pictureBox3.Image = (Bitmap)m_bmp_ir1.Clone();
							}
							this.pictureBox1.Update();
							this.pictureBox2.Update();
							this.pictureBox3.Update();
						}
						if (true) {
							calc_contrast(mode_of_cl, segs[i], m_bmp_dm1, m_bmp_pd1);
						}
						if (G.IR.CIR_CNT > 0) {
							test_pr1(segs[i]);
						}
						dispose_bmp(ref m_bmp_dm1);
						dispose_bmp(ref m_bmp_pd1);
						dispose_bmp(ref m_bmp_ir1);
					}
	#if true//2018.11.28(メモリリーク)
					dispose_bmp(ref m_bmp_pd1);
					dispose_bmp(ref m_bmp_dm1);
					dispose_bmp(ref m_bmp_ir1);
					//GC.Collect();
	#endif
					if (G.bCANCEL) {
						break;
					}
	#if true//2019.01.11(混在対応)
					hr.mode_of_cl = mode_of_cl;//0:透過, 1:反射
	#endif
					//---
					calc_contrast_avg(segs);
					for (int i = 0; i < segs.Length; i++) {
						add_grid_row(segs[i], m_hair.Count, i);
					}
					//---
					hr.seg = segs;//(seg_of_hair[])ar_seg.ToArray(typeof(seg_of_hair));
					hr.cnt_of_seg = hr.seg.Length;
					m_hair.Add(hr);
				}
				enable_forms(true);

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
			}
			catch (Exception ex) {
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
			//this.comboBox8.Tag = null;
#if true//2018.11.22(数値化エラー対応)
			if (G.FORM02 != null) {
				G.FORM02.Close();
				G.FORM02 = null;
			}
			this.Enabled = true;
#endif
			this.dataGridView1.Enabled = true;
			if (this.dataGridView1.Rows.Count > 0) {
				this.dataGridView1.Rows[0].Selected = false;
				this.dataGridView1.Rows[0].Selected = true;
			}
			button4_Click_1(null, null);
		}
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
			this.MOZ_CND_FOLD = (G.SS.NGJ_CND_FMOD == 0) ? G.SS.AUT_BEF_PATH: G.SS.NGJ_CND_FOLD;
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
			//
			this.Text = this.Text + "[" + this.MOZ_CND_FOLD + "]";
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
		private void draw_image(hair hr)
		{
			string buf_dm, buf_ir, buf_pd;
			Image bmp_dm = null, bmp_ir = null, bmp_pd = null;
//			int	Z = 8;
			float pw = 5;
			//---
			int idx = m_isel;
			seg_of_hair seg = (seg_of_hair)hr.seg[idx];

			if (false /*this.radioButton3.Checked*/) {
			}
			//---
			dispose_img(this.pictureBox1);
			dispose_img(this.pictureBox2);
			dispose_img(this.pictureBox3);
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
				}

				bmp_dm = to_img_from_file(buf_dm);
				bmp_pd = to_img_from_file(buf_pd);
				bmp_ir = to_img_from_file(buf_ir);
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
				if (this.checkBox2.Checked) {//赤外・輪郭
					pen = new Pen(Color.Blue, pw);
					gr_dm.DrawLines(pen, seg.dex_top);//.dia_top);
					gr_pd.DrawLines(pen, seg.dex_top);
					gr_ir.DrawLines(pen, seg.dex_top);
					//---
					gr_dm.DrawLines(pen, seg.dex_btm);
					gr_pd.DrawLines(pen, seg.dex_btm);
					gr_ir.DrawLines(pen, seg.dex_btm);
				}
				if (this.checkBox8.Checked) {//中心ライン
					pen = new Pen(Color.Cyan/*this.chart1.Series[0].Color*/, pw);
					//---
					gr_dm.DrawLines(pen, seg.dex_cen);
					gr_pd.DrawLines(pen, seg.dex_cen);
					gr_ir.DrawLines(pen, seg.dex_cen);
				}
				if (this.checkBox1.Checked) {//コントスラト計算範囲
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
					draw_text(bmp_pd, string.Format("直径・左={0:F1}um, 右{1:F1}um, AVG={2:F1}, CHANGE={3:F1}%", seg.mou_len_l, seg.mou_len_r, seg.dia_avg, seg.mou_len_c), 60, StringAlignment.Far, StringAlignment.Far);
				}
				if (this.checkBox21.Checked) {
					draw_text(bmp_dm, string.Format("CONTRAST={0:F3}, AVG={1:F3}, DROP={2:F1}%", seg.zp_contr, seg.zp_contr_avg, seg.zp_contr_drop), 60, StringAlignment.Near, StringAlignment.Near);
					draw_text(bmp_pd, string.Format("CONTRAST={0:F3}, AVG={1:F3}, DROP={2:F1}%", seg.kp_contr, seg.kp_contr_avg, seg.kp_contr_drop), 60, StringAlignment.Near, StringAlignment.Near);
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
				hair hr = (hair)m_hair[m_i];
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
				if (m_i >= m_hair.Count) {
					return;
				}
				hair hr = (hair)m_hair[m_i];
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
				bool flag = false;
				val = (double)this.dataGridView1.Rows[i].Cells[2].Value;
				if (val >= G.SS.REM_BOK_STHD) {
					flag = true;
				}
				val = (double)this.dataGridView1.Rows[i].Cells[4].Value;
				if (val >= G.SS.REM_BOK_CTHD) {
					flag = true;
				}
				if (flag) {
					this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(255, 96, 96);//;Color.Red;
				}
				else {
					this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Empty;
				}
#if true//2019.04.09(再測定実装)
				this.dataGridView1.Rows[i].Cells[8].Value = flag;
#endif
			}
		}
#endif
		private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 8) {
				if ((bool)this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) {
					this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = false;
				}
				else {
					this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = true;
				}
			}
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
		private void add_remes(List<G.RE_MES> remes, string name)
		{
			G.RE_MES mes = new G.RE_MES();
			string nam;
			string tmp;
			string[] files;

			if (true) {
				mes.fold = this.MOZ_CND_FOLD;
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
				files = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD, mes.h_name() + "*.*");
				tmp = "";
				for (int i = 0; i < files.Length; i++) {
					tmp += System.IO.Path.GetFileName(files[i]);
					tmp += "\r";
				}
				G.mlog(tmp);
			}
			files = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD, mes.h_name() + "ZP00D.*");
			if (files.Length == 1) {
				tmp = files[0];
				if (true) {
					//表面ＡＦ画像
					nam = System.IO.Path.GetFileName(tmp);
					mes.name_of_zp.Add(nam);
					mes.offs_of_zp.Add(0);
					//---
					PLS_XYZ pos;
					if (m_log_info.map_of_xyz.TryGetValue(nam, out pos)) {
						mes.pls_x = pos.X;
						mes.pls_y = pos.Y;
						mes.pls_z_of_zp = pos.Z;
					}
				}
				files = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD, mes.h_name() + "Z???D.*");
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
			files = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD, mes.h_name() + "KP00D.*");
			if (files.Length == 1) {
				tmp = files[0];
				if (true) {
					//中心ＡＦ画像
					nam = System.IO.Path.GetFileName(tmp);
					mes.name_of_kp.Add(nam);
					mes.offs_of_kp.Add(0);
					//---
					PLS_XYZ pos;
					if (m_log_info.map_of_xyz.TryGetValue(nam, out pos)) {
					//	mes.pls_x = pos.X; //XとYはZPとKPで同じ,Zのみ異なる
					//	mes.pls_y = pos.Y;
						mes.pls_z_of_kp = pos.Z;
					}
				}
				files = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD, mes.h_name() + "K???D.*");
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
					tmp = this.MOZ_CND_FOLD + "\\" + mes.name_of_zp[i];
					tmp = to_ir_file(tmp);
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
					tmp = this.MOZ_CND_FOLD + "\\" + mes.name_of_kp[i];
					tmp = to_ir_file(tmp);
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
			for (int q = 0; q < this.dataGridView1.Rows.Count; q++) {
				bool flag = false;
				flag = (bool)this.dataGridView1.Rows[q].Cells[8].Value;
				if (true) {
					int idx = (int)this.dataGridView1.Rows[q].Tag;
					int h = HIWORD(idx);
					int i = LOWORD(idx);
					m_i = h;
					m_isel = i;
					hair hr = m_hair[m_i];
					seg_of_hair seg = (seg_of_hair)hr.seg[m_isel];
					seg.bREMES = flag;
				}
			}
			//---
			G.REMES.Clear();
			//---
			for (int q = 0; q < m_hair.Count; q++) {
				for (int i = 0; i < m_hair[q].seg.Count(); i++) {
					//G.mlog("seg.name_of_dm:\r\r" + seg.name_of_dm);
					if (m_hair[q].seg[i].bREMES) {
						add_remes(G.REMES, m_hair[q].seg[i].name_of_dm);
					}
				}
			}
			if (true) {
				G.FORM12.BeginInvoke(new G.DLG_VOID_VOID(G.FORM12.do_re_mes));
			}
		}
#endif
	}
}
