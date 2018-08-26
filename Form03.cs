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

namespace uSCOPE
{
	public partial class Form03 : Form
	{
		private int[] C_FILT_COFS = new int[] { 0, 3, 5, 7, 9, 11 };
		private int[] C_FILT_CNTS = new int[] { 1, 5,10, 15, 20};
		private int[] C_SMTH_COFS = new int[] { 0,5,7,9,11,13,15,17,19,21,23, 25};
		private int m_i = 0;
		private int m_isel = 0;
		private int MOZ_CND_FTCF;//5:11x11
		private int MOZ_CND_FTCT;//0:1回
		private int MOZ_CND_SMCF;//5:重み係数=11
		private string MOZ_CND_FOLD;
		//---
		private ArrayList m_hair = new ArrayList();
		//---
		private Bitmap	m_bmp_dm1, m_bmp_dm0, m_bmp_dm2;
		private Bitmap	m_bmp_ir1, m_bmp_ir0, m_bmp_ir2;
		private Bitmap	m_bmp_pd1;
#if true//2018.08.21
		private Bitmap	m_bmp_pd0, m_bmp_pd2;
#endif
		private Point[] m_dia_top;
		private Point[]	m_dia_btm;
		private int		m_dia_cnt;
		private int		m_chk1, m_chk2;
		//---
		public Dictionary<string, ImageList> m_map_of_dml;
		public Dictionary<string, ImageList> m_map_of_irl;
#if true//2018.08.21
		public Dictionary<string, ImageList> m_map_of_pdl;//毛髪径:位置検出
#endif
		private int		m_thm_wid, m_thm_hei;
		private ArrayList m_zpos_org = new ArrayList();
		private ArrayList m_zpos_val = new ArrayList();
		private string m_fold_of_dept;
		//---
		public Form03()
		{
			InitializeComponent();
		}
		// 幅w、高さhのImageオブジェクトを作成
		private Image createThumbnail(Image image, int w, int h)
		{
			Bitmap canvas = new Bitmap(w, h);

			Graphics g = Graphics.FromImage(canvas);
			g.FillRectangle(new SolidBrush(Color.White), 0, 0, w, h);

			float fw = (float)w / (float)image.Width;
			float fh = (float)h / (float)image.Height;

			float scale = Math.Min(fw, fh);
			fw = image.Width * scale;
			fh = image.Height * scale;
			const
			int mode = 0;
			switch (mode) {
			case 0:
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
			break;
			case 1:
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
			break;
			case 2:
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
			break;
			case 3:
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
			break;
			case 4:
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			break;
			case 5:
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
			break;
			//case 6:
				//g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Invalid;
			//break;
			case 6:
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
			break;
			case 7:
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			break;
			}
			g.DrawImage(image, (w - fw) / 2, (h - fh) / 2, fw, fh);
			g.Dispose();

			return canvas;
		}

		//private PointF scan_pt(Point p1, Point p2, PointF pt, double ds)
		//{
		//    double dx = p2.X - p1.X;
		//    double dy = p2.Y - p1.Y;
		//    double th = Math.Atan2(dy, dx);

		//    pt.X += (float)(ds * Math.Cos(th));
		//    pt.Y += (float)(ds * Math.Sin(th));

		//    return(pt);
		//}
		private PointF scan_pt(ArrayList ap, ref int ip, PointF pt, double ds)
		{
			while (true) {
				PointF p1 = (PointF)ap[ip+0];
				PointF p2 = (PointF)ap[ip+1];
				double dx = p2.X - p1.X;
				double dy = p2.Y - p1.Y;
				double th = Math.Atan2(dy, dx);
				double sx = (float)(ds * Math.Cos(th));
				double sy = (float)(ds * Math.Sin(th));
				if (ds >= 0) {
					if ((pt.X + sx) >= p2.X) {
						if (ip < (ap.Count-2)) {
							double d = Math.Sqrt(Math.Pow(p2.X - pt.X, 2) + Math.Pow(p2.Y - pt.Y, 2));
							ds -= d;
							pt = (PointF)p2;
							ip++;
							continue;
						}
					}
				}
				else {
					if ((pt.X + sx) < p1.X) {
						if (ip > 0) {
							double d = Math.Sqrt(Math.Pow(pt.X - p1.X, 2) + Math.Pow(pt.Y - p1.Y, 2));
							ds -= d;
							pt = (PointF)p1;
							ip--;
							continue;
						}
					}
				}
				pt.X += (float)sx;
				pt.Y += (float)sy;
				break;
			}
			return(pt);
		}
		private PointF scan_pt(FN1D[] af, ref int ip, PointF pt, double ds)
		{
retry:
			FN1D	fn = af[ip];
			PointF p1 = fn.P1;
			PointF p2 = fn.P2;
			PointF pf;

			pf = af[ip].GetScanPt3Ext(p1, p2, pt, ds);
			if (ds >= 0) {
				if (pf.X >= p2.X && ip < (af.Length-1)) {
					double d = G.diff(p2,  pt);
					ds -= d;
					pt = (PointF)p2;
					ip++;
					goto retry;
				}
			}
			else {
				if (pf.X < p1.X && ip > 0) {
					double d = G.diff(pt, p1);
					ds += d;
					pt = (PointF)p1;
					ip--;
					goto retry;
				}
			}
			return(pf);
		}
		private ArrayList test_p1(int idx, int cnt)
		{
			var ar = new ArrayList();
			//Region region = new Region(;
			var gp = new GraphicsPath();
			var pl = new Point[G.IR.PLY_CNT];

			Array.Copy(G.IR.PLY_PTS, pl, G.IR.PLY_CNT);
			gp.AddPolygon(pl);

			//(1)
			for (int i = 0; i < m_dia_cnt; i++) {
				PointF pt = new PointF();
				pt.X = (m_dia_top[i].X + m_dia_btm[i].X)/2;
				pt.Y = (m_dia_top[i].Y + m_dia_btm[i].Y)/2;
				ar.Add(pt);
			}
			if (true) {
				int i1 = 0;
				//---
				PointF pt = (PointF)ar[i1];
				//---
				if (/*idx == 0*/G.IR.PLY_XMIN >= 5) {
					//左端の画像
					while (true) {
						pt = scan_pt(ar, ref i1, pt, -1);
						if (!gp.IsVisible(pt)) {
							break;
						}
					}
					pt = scan_pt(ar, ref i1, pt, +1);
				}
				else {
					//左端以外の画像
					while (true) {
						pt =  scan_pt(ar, ref i1, pt, -1);
						if (pt.X < 0) {
							break;
						}
					}
					pt = scan_pt(ar, ref i1, pt, +1);
				}
				ar.Insert(0, pt);
			}
			if (true) {
				int i1 = m_dia_cnt-1;
				//---
				PointF pt = (PointF)ar[i1];
				//---
				if (/*idx == (cnt-1)*/G.IR.PLY_XMAX <= (2591-5)) {
					//右端の画像
					while (true) {
						pt = scan_pt(ar, ref i1, pt, +1);
						if (!gp.IsVisible(pt)) {
							break;
						}
					}
					pt = scan_pt(ar, ref i1, pt, -1);
				}
				else {
					//右端以外の画像
					while (true) {
						pt =  scan_pt(ar, ref i1, pt, +1);
						if (pt.X >= G.IR.WIDTH) {
							break;
						}
					}
					pt =scan_pt(ar, ref i1, pt, -1);
				}
				ar.Add(pt);
			}
			return(ar);
		}
		private bool IsContainsBmp(Bitmap bmp, Point pt)
		{
			if (pt.X < 0 || pt.Y < 0) {
				return(false);
			}
			if (pt.X >= bmp.Width || pt.Y >= bmp.Height) {
				return(false);
			}
			return(true);
		}
		/*
		 * Pt0(100, 100)
		 *      
		 *              Pt1(200, 200)
		 *                       
		 *                           Pt2(300,300)
		 *                           
		 *  pt@1(-10,100) -> pt@0((Pt1.X-Pt0.X)-10, (Pt1.Y-Pt0.Y)+100)
		 *                -> pt@0(90,200)
		 */
		private object TO_CL(Point pt)
		{
			if (IsContainsBmp(m_bmp_dm1, pt)) {
				return(m_bmp_dm1.GetPixel(pt.X, pt.Y));
			}
			else if (pt.X < 0 && m_bmp_dm0 != null) {
				PointF pt0 = (PointF)m_bmp_dm0.Tag;
				PointF pt1 = (PointF)m_bmp_dm1.Tag;
				PointF pf = new PointF(pt.X+(pt1.X-pt0.X), pt.Y+(pt1.Y-pt0.Y));
				pt = Point.Round(pf);
				if (IsContainsBmp(m_bmp_dm0, pt)) {
					return(m_bmp_dm0.GetPixel(pt.X, pt.Y));
				}
			}
			else if (pt.X >= m_bmp_dm1.Width && m_bmp_dm2 != null) {
				PointF pt1 = (PointF)m_bmp_dm1.Tag;
				PointF pt2 = (PointF)m_bmp_dm2.Tag;
				PointF pf = new PointF( pt.X+(pt1.X-pt2.X),  pt.Y+(pt1.Y-pt2.Y));
				pt = Point.Round(pf);
				if (IsContainsBmp(m_bmp_dm2, pt)) {
					return(m_bmp_dm2.GetPixel(pt.X, pt.Y));
				}
			}
			return(null);
		}
		private object TO_IR(Point pt)
		{
			if (IsContainsBmp(m_bmp_ir1, pt)) {
				return(m_bmp_ir1.GetPixel(pt.X, pt.Y));
			}
			else if (pt.X < 0 && m_bmp_ir0 != null) {
				PointF pt0 = (PointF)m_bmp_ir0.Tag;
				PointF pt1 = (PointF)m_bmp_ir1.Tag;
				PointF pf = new PointF(pt.X+(pt1.X-pt0.X),pt.Y+(pt1.Y-pt0.Y));
				pt = Point.Round(pf);
				if (IsContainsBmp(m_bmp_ir0, pt)) {
					return(m_bmp_ir0.GetPixel(pt.X, pt.Y));
				}
			}
			else if (pt.X >= m_bmp_ir1.Width && m_bmp_ir2 != null) {
				PointF pt1 = (PointF)m_bmp_ir1.Tag;
				PointF pt2 = (PointF)m_bmp_ir2.Tag;
				PointF pf = new PointF(pt.X+(pt1.X-pt2.X),pt.Y+(pt1.Y-pt2.Y));
				pt = Point.Round(pf);
				if (IsContainsBmp(m_bmp_ir2, pt)) {
					return(m_bmp_ir2.GetPixel(pt.X, pt.Y));
				}
			}
			return(null);
		}
		//
		//
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
			//public int 
			//public float start_pix_of_seg;
			//カラー画像と
			//赤外画像による情報
			public int	 cnt_of_val;
			public ArrayList val_xum;//断面:X位置
			public ArrayList val_p5u;//断面:上端+5um
			public ArrayList val_phf;//断面:上側中点
			public ArrayList val_cen;//断面:中心
			public ArrayList val_mph;//断面:下側中点
			public ArrayList val_m5u;//断面:下端-5um
			//---
			public int	 cnt_of_pos;
			//public ArrayList	 pts_x;
			public ArrayList	 pts_p5u;
			public ArrayList	 pts_phf;
			public ArrayList	 pts_cen;
			public ArrayList	 pts_mph;
			public ArrayList	 pts_m5u;
			//---
			public ArrayList	mou_len;//毛髪・径
			//---
			//赤外情報
			public int	 cnt_of_moz;
			public ArrayList moz_zpt;//毛髄:上側点
			public ArrayList moz_zpb;//毛髄:下側点
			public ArrayList moz_zpl;//毛髄:長さ径
			public ArrayList moz_top;
			public ArrayList moz_btm;
			//---
			public ArrayList dia_top;//輪郭・頂点(上側)
			public ArrayList dia_btm;//輪郭・頂点(下側)
			public int       dia_cnt;//輪郭・頂点数
			//---
			public seg_of_hair() {
				this.cnt_of_val = 0;
				this.val_xum = new ArrayList();
				this.val_p5u = new ArrayList();
				this.val_phf = new ArrayList();
				this.val_cen = new ArrayList();
				this.val_mph = new ArrayList();
				this.val_m5u = new ArrayList();
				//--
				this.cnt_of_pos = 0;
				//this.pts_x = null;
				this.pts_p5u =  new ArrayList();
				this.pts_phf =  new ArrayList();
				this.pts_cen =  new ArrayList();
				this.pts_mph =  new ArrayList();
				this.pts_m5u =  new ArrayList();
				//---
				this.mou_len = new ArrayList();
				//---
				//赤外情報
				this.cnt_of_moz = 0;
				this.moz_zpt = new ArrayList();//毛髄:上側点
				this.moz_zpb = new ArrayList();//毛髄:下側点
				this.moz_zpl = new ArrayList();//毛髄:長さ径
				this.moz_top = new ArrayList();
				this.moz_btm = new ArrayList();
			}
		};

		class hair {
			public int	cnt_of_seg;
			public seg_of_hair[]	seg;
			public ImageList il_dm;
			public ImageList il_ir;
#if true//2018.08.21
			public ImageList il_pd;
#endif
			public string[] name_of_cl;
			public string[] name_of_ir;
			public double width_of_hair;
			public double height_of_hair;
			public hair() {
				this.cnt_of_seg = 0;
				this.seg = null;
				this.il_dm = new ImageList();
				this.il_dm.ColorDepth = ColorDepth.Depth24Bit;
				this.il_dm.ImageSize = new Size((int)(0.8*100), (int)(0.8*80));
				this.il_ir = new ImageList();
				this.il_ir.ColorDepth = ColorDepth.Depth24Bit;
				this.il_ir.ImageSize = new Size((int)(0.8*100), (int)(0.8*80));
				this.name_of_cl = null;
				this.name_of_ir = null;
				this.width_of_hair = 0;
				this.height_of_hair = 0;
			}
		};
		// 配列afのhlからhrの範囲での最小値位置からの連続したzval以下範囲を求める
		bool select_zval_hani(double[] af, int hl, int hr, int zval, out int il, out int ir)
		{
			il = ir = 0;

			int	i1;
			int	ic = (hl+hr)/2;
			double vmin = 255;
			int imin = 0;

			//最小値の位置を探索
			//中心から＋、中心から－の二段階探索に分ける、
			//同値の場合はセンター寄りにするため
			for (i1 = ic; i1 <= hr; i1++) {
				if (double.IsNaN(af[i1])) {
					continue;
				}
				if (vmin > af[i1]) {
					vmin = af[i1];
					imin = i1;
				}
			}
			for (i1 = ic; i1 >= hl; i1--) {
				if (double.IsNaN(af[i1])) {
					continue;
				}
				if (vmin > af[i1]) {
					vmin = af[i1];
					imin = i1;
				}
			}
			i1 = imin;
			if (af.Length < 3 || double.IsNaN(af[i1]) || af[i1] > G.SS.MOZ_CND_ZVAL) {
				//最小値位置の値が範囲外なら毛髄無しと判定
				return(false);
			}
			else {
				//最小値位置から＋側と－側へ閾値外になる位置を探索
				int i0, i2;
				for (i2 = i1; i2 < af.Length; i2++) {
					if (double.IsNaN(af[i2]) || af[i2] > G.SS.MOZ_CND_ZVAL) {
						i2--;
						break;
					}
				}
				if (i2 >= af.Length) {
					i2 = af.Length-1;
				}
				for (i0 = i1; i0 >= 0; i0--) {
					if (double.IsNaN(af[i0]) || af[i0] > G.SS.MOZ_CND_ZVAL) {
						i0++;
						break;
					}
				}
				if (i0 < 0) {
					i0 = 0;
				}
				il = i0;
				ir = i2;
			}
			return(true);
		}
		//ArrayList m_pt_zpl = new ArrayList();//毛髄:長さ径
		//---
		void test_ir(seg_of_hair seg, FN1D f2, PointF p2, PointF p3, int sx)
		{
			Point tx = Point.Truncate(p2);
			Point t3 = Point.Truncate(p3);
			PointF fx = p2;
			ArrayList fp = new ArrayList();
			ArrayList fc = new ArrayList();
			double df = G.diff(p2, p3);
			for (int i = 1;; i++) {
				PointF f0 = f2.GetScanPt1Ext(p2, p3, i);
				//Point  t0 = Point.Round(f0);
				Point  t0 = Point.Truncate(f0);
				
				double	ff;
				if ((ff = G.diff(f0, p3)) <= 0.4) {
					f0 = f0;
				}
				if (t0.Equals(tx)) {
					t0 = t0;//continue;
				}
				else {
					fp.Add(t0);
					fc.Add(TO_IR(t0));
					if ((ff = G.diff(p2, f0)) > df /*t0.Equals(t3)*/) {
						break;
					}
					tx = t0;
				}
				//fx = f0;
				//Point.Round(tm).Equals(k0)
				//if (ff > df) {
				//    ff = ff;
				//}
				//if (true) {
				//    //t3 = Point.Truncate(p3);
				//}
			}

			double[] af;
			if (true) {
				ArrayList ar = new ArrayList();
				for (int i = 0; i < fc.Count; i++) {
					double f;
					if (fc[i] == null) {
						f = double.NaN;
					}
					else {
						f = ((Color)fc[i]).G;
					}
					ar.Add(f);
				}
				af = (double[])ar.ToArray(typeof(double));
//				double[] ao = new double[af.Length];
				double fmax, fmin;
				if (this.MOZ_CND_SMCF > 0) {
					T.SG_POL_SMOOTH(af, af, af.Length, this.MOZ_CND_SMCF, out fmax, out fmin);
//					T.SG_2ND_DERI(af, ao, af.Length, 21, out fmax);
				}
				else {
					fmax = double.MinValue;
					fmin = double.MaxValue;
					for (int i = 0; i < af.Length; i++) {
						if (fmin > af[i]) {
							fmin = af[i];
						}
						if (fmax < af[i]) {
							fmax = af[i];
						}
					}
				}
				if (G.SS.MOZ_CND_CTRA) {
					//コントラスト最大化(0-255範囲にマッピング)
					FN1D fn = new FN1D(new PointF((float)fmin, 0f), new PointF((float)fmax, 255f));
					for (int i = 0; i < af.Length; i++) {
						af[i] = fn.GetYatX(af[i]);
					}
				}
			}
			if (false) {
				try {
					StreamWriter wr;
					string path = System.IO.Path.GetFileNameWithoutExtension(seg.name_of_ir);
					path += ".csv";
					path = "c:\\temp\\ir_" + path;
					wr = new StreamWriter(path, true, Encoding.Default);
					wr.Write(string.Format("{0},", sx));
					for (int i = 0; i < fc.Count; i++) {
						wr.Write(string.Format("{0:F0}", af[i]));
						if (i < (fc.Count-1)) {
							wr.Write(",");
						}
					}
					wr.WriteLine("");
					wr.Close();
				}
				catch (Exception ex) {
				}
			}
			//Color cl;
			Point u1 = new Point(0,0), u2=new Point(0,0);
			double l1 = 0;
			int	i1;
			int	ic = af.Length/2;
			int l5 = af.Length/5;
			double vmin = 255;
			int imin = 0;
			if (true) {
				l5 = (af.Length *  G.SS.MOZ_CND_HANI)/100/2;
				if ((ic+l5) >= af.Length) {
					l5--;
				}
				if ((ic-l5) < 0) {
					l5--;
				}
			}
			else {
				l5 = af.Length/5; // G.SS.MOZ_CND_HANI=40%と同じ
			}
			//最小値の位置を探索
			//中心から＋、中心から－の二段階探索に分ける、
			//同値の場合はセンター寄りにするため
			if (true) {
				for (i1 = ic; i1 <= (ic+l5); i1++) {
					if (double.IsNaN(af[i1])) {
						continue;
					}
					if (vmin > af[i1]) {
						vmin = af[i1];
						imin = i1;
					}
				}
				for (i1 = ic; i1 >= (ic-l5); i1--) {
					if (double.IsNaN(af[i1])) {
						continue;
					}
					if (vmin > af[i1]) {
						vmin = af[i1];
						imin = i1;
					}
				}
			}
			else {
				for (i1 = ic-l5; i1 <= (ic+l5); i1++) {
					if (double.IsNaN(af[i1])) {
						continue;
					}
					if (vmin > af[i1]) {
						vmin = af[i1];
						imin = i1;
					}
				}
			}
			i1 = imin;
			if (af.Length < 3 || double.IsNaN(af[i1]) || af[i1] > G.SS.MOZ_CND_ZVAL) {
				//最小値位置の値が範囲外なら毛髄無しと判定
				u1 = (Point)fp[ic];
				u2 = (Point)fp[ic];
				l1 = 0;
			}
			else {
				//最小値位置から＋側と－側へ閾値外になる位置を探索
				int i0, i2;
				for (i2 = i1; i2 < fc.Count; i2++) {
					if (double.IsNaN(af[i2]) || af[i2] > G.SS.MOZ_CND_ZVAL) {
						i2--;
						break;
					}
				}
				if (i2 >= af.Length) {
					i2 = af.Length-1;
				}
				for (i0 = i1; i0 >= 0; i0--) {
					if (double.IsNaN(af[i0]) || af[i0] > G.SS.MOZ_CND_ZVAL) {
						i0++;
						break;
					}
				}
				if (i0 < 0) {
					i0 = 0;
				}
				int il, ir;
				bool rc;
				rc = select_zval_hani(af, (ic-l5), (ic+l5), G.SS.MOZ_CND_ZVAL, out il, out ir);
				if (i0 != il || i2 != ir) {
					rc = rc;
				}
				if (i0 < (ic-l5) || i2 > (ic+l5)) {
					//判定された毛髄範囲が範囲外まで連続しているときは
					//毛髄では無いと判定する
					u1 = (Point)fp[ic];
					u2 = (Point)fp[ic];
					l1 = 0;
					if (i0 < (ic-l5) && i2 > (ic+l5)) {
						//両側でNG
					}
					else if (i0 < (ic-l5)) {
						//－側でNG
						rc = select_zval_hani(af, i2+1, (ic+l5), G.SS.MOZ_CND_ZVAL, out il, out ir);
						if ((il >= i0 && il <= i2) || (ir >= i0 && ir <= i2)) {
							rc = rc;
							if (rc) {
								rc = rc;
							}
						}
						if (rc && ir <= (ic+l5)) {
							u1 = (Point)fp[il];
							u2 = (Point)fp[ir];
						}
					}
					else {
						//＋側でNG
						rc = select_zval_hani(af, (ic-l5), i0-1, G.SS.MOZ_CND_ZVAL, out il, out ir);
						if ((il >= i0 && il <= i2) || (ir >= i0 && ir <= i2)) {
							rc = rc;
						}
						if (rc && il >= (ic-l5)) {
							u1 = (Point)fp[il];
							u2 = (Point)fp[ir];
						}
					}
				}
				else {
					u1 = (Point)fp[i0];
					u2 = (Point)fp[i2];
					l1 = Math.Sqrt(Math.Pow(u2.X - u1.X, 2) + Math.Pow(u2.Y - u1.Y, 2));
				}
			}
			l1 = Math.Sqrt(Math.Pow(u2.X - u1.X, 2) + Math.Pow(u2.Y - u1.Y, 2));
			l1 = G.PX2UM(l1, m_log_info.pix_pitch, m_log_info.zoom);
			//if ((++m_chk1 % 20) == 0) {
			//    u1 = Point.Round(p2);
			//    u2 = Point.Round(p3);
			//}
			seg.moz_zpt.Add(u1);//毛髄:上側点
			seg.moz_zpb.Add(u2);//毛髄:下側点
			seg.moz_zpl.Add(l1);//毛髄:長さ径
			//---
			seg.moz_top.Add(Point.Round(p2));
			seg.moz_btm.Add(Point.Round(p3));
			if (l1 <= 0.0) {
				l1 = l1;
			}
		}
		//private double m_offset_of_hair;
		private double m_back_of_x;
		//---
		private void test_dm(seg_of_hair[] segs, int idx, int cnt)
		{
			//(1)中心のラインを求める(両端は画像端まで拡張する)
			//(2)中心ラインに沿って左端から右端まで一定間隔で走査点を進める
			//(3)走査点で垂直方向に上下両側に延ばした時の輪郭線との交点を求める
			//(4)輪郭線との交点と走査点から断面点を求める
			//(5)断面点の画素値を格納する
			//Form02.TO_RR(0.5,  G.IR.DIA_TOP[i],  G.IR.DIA_BTM[i], out top[i], out btm[i]);
			//---(1)
			seg_of_hair seg = segs[idx];
			int	cntm1 = (m_dia_cnt-1);
			FN1D[]	m_ft = new FN1D[cntm1];
			FN1D[]	m_fb = new FN1D[cntm1];
			FN1D[]	m_fc = new FN1D[cntm1];
			//ArrayList ac, at = new ArrayList(), ab = new ArrayList();
			//ac = test_p1(idx, cnt);
			for (int i = 0; i < cntm1; i++) {
				PointF pt0 = m_dia_top[i+0];
				PointF pt1 = m_dia_top[i+1];
				PointF pb0 = m_dia_btm[i+0];
				PointF pb1 = m_dia_btm[i+1];
				PointF pc0 = new PointF((pt0.X+pb0.X)/2f, (pt0.Y+pb0.Y)/2f);
				PointF pc1 = new PointF((pt1.X+pb1.X)/2f, (pt1.Y+pb1.Y)/2f);
				m_ft[i] = new FN1D(pt0, pt1);
				m_fb[i] = new FN1D(pb0, pb1);
				m_fc[i] = new FN1D(pc0, pc1);
			}
			//m_ft = (FN1D[])at.ToArray(typeof(FN1D));
			//m_fb = (FN1D[])ab.ToArray(typeof(FN1D));
			//---(2)
			double	px0 = (idx <= 0) ? 0: segs[idx-1].pix_pos.X;
			double	px1 = seg.pix_pos.X;
			double	dif = (px1-px0);
			int		i0 = 0;
			//double ds = 5;//5dot = 1.375um
			double	ds = G.UM2PX(G.SS.MOZ_CND_DSUM, m_log_info.pix_pitch, m_log_info.zoom);
			double	u5 = G.UM2PX(G.SS.MOZ_CND_CUTE, m_log_info.pix_pitch, m_log_info.zoom);//5;
			PointF	pf;// = (PointF)ac[0];
			//double xend = ((PointF)ac[ac.Count-1]).X;
			double	xmin = (G.IR.PLY_XMIN < C.GAP_OF_IMG_EDGE) ? 0 :G.IR.PLY_XMIN;
			double	xmax = ((G.IR.WIDTH - G.IR.PLY_XMAX) < C.GAP_OF_IMG_EDGE) ? (G.IR.WIDTH-1) : G.IR.PLY_XMAX;
			PointF	sta_of_pf = new PointF();
			int		ii = 0, ss = 0, s = 0;
			if (idx <= 0) {
				idx = 0;
			}
			if (m_back_of_x <= 0 || dif == 0) {
				sta_of_pf.X = (float)xmin;
				sta_of_pf.Y = (float)m_fc[0].GetYatX(sta_of_pf.X);
				ss = 0;
			}
			else {
				sta_of_pf.X = (float)(m_back_of_x-dif);
				sta_of_pf.Y = (float)m_fc[0].GetYatX(sta_of_pf.X);
				if (sta_of_pf.X < 0 || sta_of_pf.X > xmax) {
					//画像が抜けてるか、ステージ座標が不正か...
					sta_of_pf.X = (float)xmin;
					sta_of_pf.Y = (float)m_fc[0].GetYatX(sta_of_pf.X);
					ss = 0;
				}
				else {
					for (;;ss--, sta_of_pf = pf) {
						pf = scan_pt(m_fc, ref ii, sta_of_pf, -ds);
						if (pf.X < 0) {
							break;
						}
					}
				}
			}
			pf =sta_of_pf;
			//m_back_of_x = pf.X;
			//を
			//現在の画像のＸ値に変換
			//seg.total_idx = m_offset_of_hair;
			//
			for (s = ss; pf.X <= xmax; s++) {
				//double y0, y1,y2, y3;
				//y0 = m_fc[0].GetYatX(776);
				//y1 = m_fc[0].GetYatX(791.068359);
				//y2 = m_fc[0].GetYatX(861.5);
				//y3 = m_fc[0].GetYatX(1042);
				//pf = scan_pt(ac, ref i0, pf, ds);
				//if (s > 0) {sta_of_pf = pf;}
				//if (pf.X > xend/*G.IR.WIDTH*/) {
				//    break;
				//}
				//(3) p2, p3
				FN1D f1 = m_fc[ii];//FN1D((PointF)ac[i0], (PointF)ac[i0+1]);
				FN1D f2 = f1.GetNormFn(pf);
				PointF p2 = new PointF(), p3 = new PointF(), pt;
				Point p5, p6, p7, p8, p9;
				//Color cl;
				for (int i = 0; i < m_ft.Length; i++) {
					p2 = f2.GetCrossPt(m_ft[i]);
					if (p2.X < m_dia_top[i+1].X) {
						break;
					}
				}
				for (int i = 0; i < m_fb.Length; i++) {
					p3 = f2.GetCrossPt(m_fb[i]);
					if (p3.X < m_dia_btm[i+1].X) {
						break;
					}
				}
				//(4)
				//p5:中心, p6:R50%, p7:R-50%, p8:R+3um, p9:R-3um
				p5 = Point.Round(pf);
				//---
				p5 = Point.Round(pf);
				pt = new PointF((pf.X + p2.X)/2,  (pf.Y + p2.Y)/2);
				p6 = Point.Round(pt);
				pt = new PointF((pf.X + p3.X)/2,  (pf.Y + p3.Y)/2);
				p7 = Point.Round(pt);
				pt = f2.GetScanPt2Ext(pf, p2, u5);
				p8 = Point.Round(pt);
				pt = f2.GetScanPt2Ext(pf, p3, u5);
				p9 = Point.Round(pt);
				//(5)
				//格納
				seg.val_cen.Add(TO_CL(p5));
				seg.val_phf.Add(TO_CL(p6));
				seg.val_mph.Add(TO_CL(p7));
				seg.val_p5u.Add(TO_CL(p8));
				seg.val_m5u.Add(TO_CL(p9));
				seg.val_xum.Add(Math.Round(G.PX2UM(s*ds, m_log_info.pix_pitch, m_log_info.zoom), 2));
				//---
				seg.mou_len.Add(Math.Round(G.PX2UM(G.diff(p2,  p3), m_log_info.pix_pitch, m_log_info.zoom), 1));
				//---
				seg.pts_cen.Add(p5);
				seg.pts_phf.Add(p6);
				seg.pts_mph.Add(p7);
				seg.pts_p5u.Add(p8);
				seg.pts_m5u.Add(p9);
				//(6) IR画像より毛髄径検出
				if (m_bmp_ir1 != null) {
					test_ir(seg, f2, p2, p3, s);
					seg.cnt_of_moz = 1;
				}
				pf = scan_pt(m_fc, ref ii, pf, ds);
				//if (pf.X > xmax/*G.IR.WIDTH*/) {
				//    break;
				//}
			}
			//m_offset_of_hair = s;
			m_back_of_x = pf.X;
		}
		private void test_pr1(seg_of_hair seg)
		{
			ArrayList at = new ArrayList();
			ArrayList ab = new ArrayList();
			Point	at_bak = G.IR.DIA_TOP[0];
			Point	ab_bak = G.IR.DIA_BTM[0];

			for (int i = 0; i < (G.IR.DIA_CNT-1); i++) {
				if (G.IR.DIA_TOP[i].X < at_bak.X || G.IR.DIA_BTM[i].X < ab_bak.X) {
					continue;
				}
				at.Add(at_bak = G.IR.DIA_TOP[i]);
				ab.Add(ab_bak = G.IR.DIA_BTM[i]);
			}
			m_dia_top = (Point[])at.ToArray(typeof(Point));
			m_dia_btm = (Point[])ab.ToArray(typeof(Point));
			m_dia_cnt = m_dia_top.Count();
			//---
/*			string key = seg.name_of_cl;
			Point	pnt_of_pls;
			key = key.Substring(0, key.Length-4);//拡張子カット

			if (m_log_info.map_of_pos.TryGetValue(key, out pnt_of_pls)) {
				
				pnt_of_pls.X = -pnt_of_pls.X;
				if (m_log_info.pls_org.X == 0 &&m_log_info.pls_org.Y == 0) {
					m_log_info.pls_org = pnt_of_pls;
				}
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
			}*/
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
		struct log_info {
			public Point pls_org;
			public double stg_pitch;	//[um/pls]
			public double pix_pitch;	//[um/pix]
			public double zoom;
			//---
			public Dictionary<string, Point> map_of_pos;
		};
		log_info m_log_info;
		//---
		private void test_log()
		{
			string path = this.MOZ_CND_FOLD + "\\log.csv";
			string buf;
			string[] clms;
			StreamReader sr;

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
			this.label6.Text = string.Format("x {0:F1}", m_log_info.zoom);
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
			if (!System.IO.File.Exists(fold+"\\"+buf)) {
				//buf = null;
				return(null);
			}
			return(fold+"\\"+buf);
		}
#if true//2018.08.21
		private string[] to_name_arr(int k, seg_of_hair[] segs)
		{
			seg_of_hair seg;
			ArrayList ar = new ArrayList();

			for (int q = 0; ; q++) {
				if (q >= segs.Length) {
					break;
				}
				seg = (seg_of_hair)segs[q];
				if (seg == null) {
					continue;
				}
				string str;
				switch (k) {
					case  0: str = seg.name_of_dm; break;
					case  1: str = seg.name_of_pd; break;
					default: str = seg.name_of_ir; break;
				}
				ar.Add(str);
			}
			return ((string[])ar.ToArray(typeof(string)));
		}
		private Image to_img_from_file(string path)
		{
			Image img = null;
			if (System.IO.File.Exists(path)) {
				try {
					img = Bitmap.FromFile(path);
				}
				catch (Exception ex) {
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
				zpos = "ZDEPT";
				pext = m_fold_of_dept;
				pext = pext.Replace("\\", "");
			}
			// '_ZP99D', '_ZM99D', '_ZDEPT'
			if (name.Contains("_ZDEPT")) {
				//G.mlog("pathをひとつ上に戻す必要が…");
				buf = name.Replace("_ZDEPT", "_" + zpos);
			}
			else {
				buf = Regex.Replace(name, "_Z.[0-9][0-9].", "_" + zpos);
			}
			//
			file = System.IO.Path.Combine(fold, pext, buf);//fold + "\\" + buf
			//file = fold + pext + buf;//fold + "\\" + buf
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
			case  0:zpos = G.SS.MOZ_CND_ZPCT; break;
			case  1:zpos = G.SS.MOZ_CND_ZPHL; break;
			default:zpos = G.SS.MOZ_CND_ZPML; path = to_ir_file(path); break;
			}
			fold = System.IO.Path.GetDirectoryName(path);
			name = System.IO.Path.GetFileName(path);

			if (string.IsNullOrEmpty(name)) {
				return (null);
			}
			if (zpos == "深度合成") {
				zpos = "ZDEPT";
				pext = m_fold_of_dept;
			}
			// '_ZP99D', '_ZM99D', '_ZDEPT'
			if (name.Contains("_ZDEPT")) {
				//G.mlog("pathをひとつ上に戻す必要が…");
				buf = name.Replace("_ZDEPT", "_"+zpos);
			}
			else {
				buf = Regex.Replace(name, "_Z.[0-9][0-9].", "_"+zpos);
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
#if true//2018.08.21
		private void load_bmp(seg_of_hair[] segs, int i, string path_dm1, string path_dm2, string path_ir1, string path_ir2, ref Bitmap bmp_dm0, ref Bitmap bmp_dm1, ref Bitmap bmp_dm2, ref Bitmap bmp_ir0, ref Bitmap bmp_ir1, ref Bitmap bmp_ir2)
		{
			dispose_bmp(ref bmp_dm0);
			dispose_bmp(ref bmp_ir0);
			//---
			bmp_dm0 = bmp_dm1;
			bmp_ir0 = bmp_ir1;
			//---
			bmp_dm1 = bmp_dm2;
			bmp_ir1 = bmp_ir2;
			//---
			if (i == 0) {
				bmp_dm1 = new Bitmap(path_dm1);
				bmp_dm1.Tag = segs[i].pix_pos;
				//--
				if (path_ir1 != null) {
					bmp_ir1 = new Bitmap(path_ir1);
					bmp_ir1.Tag = bmp_dm1.Tag;
				}
				else {
					bmp_ir1 = null;
				}
			}
			if (!string.IsNullOrEmpty(path_dm2)) {
				bmp_dm2 = new Bitmap(path_dm2);
				bmp_dm2.Tag = segs[i+1].pix_pos;
				//--
				if (path_ir2 != null) {
					bmp_ir2 = new Bitmap(path_ir2);
					bmp_ir2.Tag = bmp_dm2.Tag;
				}
				else {
					bmp_ir2 = null;
				}
			}
			else {
				bmp_dm2 = null;
				bmp_ir2 = null;
			}
		}
#else
		private void load_bmp(seg_of_hair[] segs, int i, string path_dm1, string path_dm2, ref Bitmap bmp_dm0, ref Bitmap bmp_dm1, ref Bitmap bmp_dm2, ref Bitmap bmp_ir0, ref Bitmap bmp_ir1, ref Bitmap bmp_ir2)
		{
			string path_ir1 = to_ir_file(path_dm1);
			string path_ir2 = to_ir_file(path_dm2);

			dispose_bmp(ref bmp_dm0);
			dispose_bmp(ref bmp_ir0);
			//---
			bmp_dm0 = bmp_dm1;
			bmp_ir0 = bmp_ir1;
			//---
			bmp_dm1 = bmp_dm2;
			bmp_ir1 = bmp_ir2;
			//---
			if (i == 0) {
				bmp_dm1 = new Bitmap(path_dm1);
				bmp_dm1.Tag = segs[i].pix_pos;
				//--
				if (path_ir1 != null) {
					bmp_ir1 = new Bitmap(path_ir1);
					bmp_ir1.Tag = bmp_dm1.Tag;
				}
				else {
					bmp_ir1 = null;
				}
			}
			if (!string.IsNullOrEmpty(path_dm2)) {
				bmp_dm2 = new Bitmap(path_dm2);
				bmp_dm2.Tag = segs[i+1].pix_pos;
				//--
				if (path_ir2 != null) {
					bmp_ir2 = new Bitmap(path_ir2);
					bmp_ir2.Tag = bmp_dm2.Tag;
				}
				else {
					bmp_ir2 = null;
				}
			}
			else {
				bmp_dm2 = null;
				bmp_ir2 = null;
			}
		}
#endif
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
		private int get_hair_cnt(string pext, string zpos)
		{
			int cnt = 0;
#if true
			for (int q = 0; q < 10; q++) {
				string buf = q.ToString();
				string[] files_cl =
					System.IO.Directory.GetFiles(this.MOZ_CND_FOLD + pext, buf + "CL_??"+zpos+".*");
				string[] files_cr =
					System.IO.Directory.GetFiles(this.MOZ_CND_FOLD + pext, buf + "CR_??"+zpos+".*");
				string[] files_ct =
					System.IO.Directory.GetFiles(this.MOZ_CND_FOLD + pext, buf + "CT_??"+zpos+".*");
				string[] files_ir =
					System.IO.Directory.GetFiles(this.MOZ_CND_FOLD + pext, buf + "IR_??"+zpos+".*");
				if (files_ct.Length <= 0 && files_cr.Length <= 0 && files_cl.Length <= 0) {
					break;
				}
				cnt++;
			}
#else
			for (int q = 0; q < 10; q++) {
				string path = q.ToString() +  "CL_??.*";
				string[] files_cl =
					System.IO.Directory.GetFiles(this.MOZ_CND_FOLD, path);
				if (files_cl.Length <= 0) {
					continue;
				}
				cnt++;
			}
#endif
			return(cnt);
		}
		private void save_iz(string path_of_bo, ref Bitmap bo)
		{
			path_of_bo = path_of_bo.Replace("IR_", "IZ_");
			while (true) {
				try {
					System.IO.File.Delete(path_of_bo);
					System.Threading.Thread.Sleep(10);
					bo.Save(path_of_bo);
					break;
				}
				catch (Exception ex) {
					G.mlog(ex.ToString());
					string tmp = ex.ToString();
				}
			}
			bo.Dispose();
			bo = null;

		}
#if true//2018.08.21
		private string ZPOS(string pos)
		{
			pos = pos.Replace("(*)", "");
			pos = pos.Replace("*", "");
			return (pos);
		}
#endif
		private string ZVAL2ORG(string val)
		{
#if true//2018.08.21
			val = ZPOS(val);
#endif
			int idx = m_zpos_val.IndexOf(val);
			if (idx < 0) {
#if true//2018.08.21
				if (val == "深度合成") {
					return ("ZDEPT");
				}
#endif
				return ("");
			}
			return((string)m_zpos_org[idx]);
		}
		private string ZORG2VAL(string org)
		{
			int idx = m_zpos_org.IndexOf(org);
			if (idx < 0) {
				return("");
			}
			return((string)m_zpos_val[idx]);
		}
		private void sort_zpos()
		{
			for (int q = 0; q < m_zpos_val.Count; q++) {
			for (int i = 0; i < m_zpos_val.Count-1; i++) {
				int v0 = int.Parse((string)m_zpos_val[i+0]);
				int v1 = int.Parse((string)m_zpos_val[i+1]);
				if (v1 < v0) {
					string tmp;
					tmp = (string)m_zpos_val[i+0];
					m_zpos_val[i+0] = m_zpos_val[i+1];
					m_zpos_val[i+1] = tmp;
					//---
					tmp = (string)m_zpos_org[i+0];
					m_zpos_org[i+0] = m_zpos_org[i+1];
					m_zpos_org[i+1] = tmp;
				}
			}
			}
		}
		//---
		private bool make_focus_stack3(ArrayList ar_con, ArrayList ar_bmp_dep, out Bitmap bmp_dep)
		{
			bool ret = false;

			bmp_dep = null;

			try {
				//string pat;
				//string tmp = System.IO.Path.GetFileName(file);
				//string[] ZXXXD;

				//pat = tmp.Replace("ZP00D", "Z???D");
				//ZXXXD = System.IO.Directory.GetFiles(path, pat);
				bmp_dep = (Bitmap)((Bitmap)ar_bmp_dep[0]).Clone();

				int wid = bmp_dep.Width / G.SS.MOZ_FST_CCNT;
				int hei = bmp_dep.Height/ G.SS.MOZ_FST_RCNT;
				int k = 0;
				Graphics gr = Graphics.FromImage(bmp_dep);

				for (int r = 0; r < G.SS.MOZ_FST_RCNT; r++) {
					for (int c = 0; c < G.SS.MOZ_FST_CCNT; c++, k++) {
						double fmax = -1;
						int imax = 0;
						//最大コントラストの画像を検索
						for (int j = 0; j < ar_bmp_dep.Count; j++) {
							double[] fcnt = (double[])ar_con[j];
							if (fmax < fcnt[k]) {
								fmax = fcnt[k];
								imax = j;
							}
						}
						int x = c*wid;
						int y = r*hei;
						int w = (c == (G.SS.MOZ_FST_CCNT-1) ? (bmp_dep.Width -x): wid);
						int h = (r == (G.SS.MOZ_FST_RCNT-1) ? (bmp_dep.Height-y): hei);							
							
						Bitmap bmp = (Bitmap)ar_bmp_dep[imax];
						Rectangle srt = new Rectangle(x, y, w, h);
						gr.DrawImage(bmp, x, y, srt, GraphicsUnit.Pixel);
					}
				}
				gr.Dispose();
				ret = true;
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}
			return(ret);
		}
		//---
		private string[] fst_to_ir_file(string path, string[] cl_files)
		{
			ArrayList ar = new ArrayList();
			for (int i = 0; i < cl_files.Length; i++) {
				string tmp = System.IO.Path.GetFileName(cl_files[i]);
				tmp = tmp.Replace("CT_", "IR_");
				tmp = tmp.Replace("CR_", "IR_");
				ar.Add(path + "\\" + tmp);
			}
			return((string[])ar.ToArray(typeof(string)));
		}
		//---
		private bool fst_calc_contrast(string[] ZXXXD, ArrayList ar_con, ArrayList ar_bmp_dep)
		{
			bool ret = false;
			try {
				if (ar_con != null) {
					ar_con.Clear();
				}
				if (ar_bmp_dep != null) {
					ar_bmp_dep.Clear();
				}

				for (int j = 0; j < ZXXXD.Length; j++) {
					Bitmap bmp = new Bitmap(ZXXXD[j]);
					if (true) {
						ar_bmp_dep.Add(bmp);
					}
					if (ar_con != null) {
						double[] fctr;
						fctr = Form02.DO_PROC_FOCUS(bmp, G.SS.MOZ_FST_FCOF, G.SS.MOZ_FST_RCNT, G.SS.MOZ_FST_CCNT);
						ar_con.Add(fctr);
					}
				}
				ret = true;
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}
			return(ret);
		}
		//---
		// 深度合成処理
		//---
		private bool fst_make()
		{
			try {
				string path = this.MOZ_CND_FOLD;
				string[] CL_ZP00D = null, IR_ZP00D = null;
				string pat;
				Bitmap bmp_dep;
				string path_dep;
				m_fold_of_dept  = string.Format("\\{0}x{1}", G.SS.MOZ_FST_RCNT, G.SS.MOZ_FST_CCNT);
				switch (G.SS.MOZ_FST_MODE) {
				case   /*CL*/  0:m_fold_of_dept += "_CL"; break;
				case   /*IR*/  1:m_fold_of_dept += "_IR"; break;
				default/*CL,IR*/:m_fold_of_dept += "_CL_IR"; break;
				}
				path_dep = path + m_fold_of_dept;
				System.IO.Directory.CreateDirectory(path_dep);
#if true//2018.07.02
				if (G.SS.MOZ_FST_CK01) {
					//既に合成済みの場合は合成処理をスキップする
					string[] CL_ZDEPT = null;
					CL_ZDEPT = System.IO.Directory.GetFiles(path_dep, "?C?_??_ZDEPT.*");
					if (CL_ZDEPT.Length > 0) {
						return(true);
					}
				}
#endif
				for (int q = 0; q < 10; q++) {
					//---
					pat = string.Format("{0}C?_??_ZP00D.*", q);//カラー
					CL_ZP00D = System.IO.Directory.GetFiles(path, pat);
					//---
					if (q == 0 && CL_ZP00D.Length > 0) {
						//opencvのセットアップのため呼び出し
						Bitmap bmp = new Bitmap(CL_ZP00D[0]);
						G.CAM_PRC = G.CAM_STS.STS_NONE;
						G.FORM02.load_file(bmp, false);
					}
					//---
					//if (CL_ZP00D.Length > 0) {
						IR_ZP00D = fst_to_ir_file(path, CL_ZP00D);
					//}
					//---
					//---
					for (int i = 0; i < CL_ZP00D.Length; i++) {
						ArrayList ar_cl_con = new ArrayList();
						ArrayList ar_ir_con = new ArrayList();
						ArrayList ar_cl_bmp_dep = new ArrayList();
						ArrayList ar_ir_bmp_dep = new ArrayList();
						string tmp;
						string[] CL_ZXXD, IR_ZXXD;
						if (true) {
							tmp = System.IO.Path.GetFileName(CL_ZP00D[i]);
							CL_ZXXD = System.IO.Directory.GetFiles(path, tmp.Replace("ZP00D", "Z???D"));
							IR_ZXXD = fst_to_ir_file(path, CL_ZXXD);
						}

						switch (G.SS.MOZ_FST_MODE) {
						case /*CL*/0:
							if (!fst_calc_contrast(CL_ZXXD, ar_cl_con, ar_cl_bmp_dep)) {
								return(false);
							}
							if (!fst_calc_contrast(IR_ZXXD, null     , ar_ir_bmp_dep)) {
								return(false);
							}
							ar_ir_con = ar_cl_con;
						break;
						case /*IR*/1:
							if (!fst_calc_contrast(CL_ZXXD, null     , ar_cl_bmp_dep)) {
								return(false);
							}
							if (!fst_calc_contrast(IR_ZXXD, ar_ir_con, ar_ir_bmp_dep)) {
								return(false);
							}
							ar_cl_con = ar_ir_con;
						break;
						default/*CL,IR*/:
							if (!fst_calc_contrast(CL_ZXXD, ar_cl_con, ar_cl_bmp_dep)) {
								return(false);
							}
							if (!fst_calc_contrast(IR_ZXXD, ar_ir_con, ar_ir_bmp_dep)) {
								return(false);
							}
						break;
						}

						if (true) {
							string name = System.IO.Path.GetFileName(CL_ZP00D[i]);// name: xCx_xx_ZP00D.xxx

							if (!make_focus_stack3(ar_cl_con, ar_cl_bmp_dep, out bmp_dep)) {
								return(false);
							}
							name = name.Replace("ZP00D", "ZDEPT");
							bmp_dep.Save(path_dep + "\\" + name);
							bmp_dep.Dispose();
							bmp_dep = null;
						}
						if (true) {
							string name = System.IO.Path.GetFileName(IR_ZP00D[i]);// name: xIR_xx_ZP00D.xxx

							if (!make_focus_stack3(ar_ir_con, ar_ir_bmp_dep, out bmp_dep)) {
								return(false);
							}
							name = name.Replace("ZP00D", "ZDEPT");
							bmp_dep.Save(path_dep + "\\" + name);
							bmp_dep.Dispose();
							bmp_dep = null;
						}

						for (int j = 0; j < ar_cl_bmp_dep.Count; j++) {
							Bitmap bmp;
							bmp = (Bitmap)ar_cl_bmp_dep[j];
							bmp.Dispose();
							bmp = (Bitmap)ar_ir_bmp_dep[j];
							bmp.Dispose();
						}
					}
				}
			}
			catch (Exception ex) {
			}
			return(true);
		}
		//---
		private void load()
		{
			var dlg = new DlgProgress();
			try {
			int cnt_of_hair = 0;
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
						zary = System.IO.Directory.GetFiles(path, "0CR_00_*.*");
						if (zary.Length <= 0) {
						zary = System.IO.Directory.GetFiles(path, "0CT_00_*.*");
						}
						for (int i = 0; i < zary.Length; i++) {
							string tmp = System.IO.Path.GetFileNameWithoutExtension(zary[i]);
							string sgn;
							tmp = tmp.Substring(7);
							m_zpos_org.Add(tmp);
							if (tmp.Substring(1, 1) == "P") {
								sgn = "+";
							}
							else {
								sgn = "-";
							}
							tmp = tmp.Substring(2, 2);
							m_zpos_val.Add(sgn+tmp);
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
			for (int q = 0; q < 10; q++) {
				int width = 0;//(int)(2592/8);//2592/8=324
				int height =0;//(int)(1944/8);//1944/8=243
				//string path;
				string[] files_ct, files_cr, files_cl, files_ir;
				string[] files_pd, files_dm;

				string buf = q.ToString();
				int cnt_of_seg;

				files_ct = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD+pext,  buf +  "CT_??"+zpos+".*");
				files_cr = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD+pext,  buf +  "CR_??"+zpos+".*");
				files_ir = System.IO.Directory.GetFiles(this.MOZ_CND_FOLD+pext,  buf +  "IR_??"+zpos+".*");
				if (files_ct.Length <= 0 && files_cr.Length <= 0) {
					break;//終了
				}
				if (files_ct.Length > 0 && files_cr.Length > 0) {
					break;//終了(反射と透過が混在！)
				}
				if (files_ct.Length > 0) {
					files_cl = files_ct;//透過
					G.set_imp_param(/*透過*/0, -1);
				}
				else {
					files_cl = files_cr;//反射
					G.set_imp_param(/*反射*/1, -1);
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
				}
				segs = (seg_of_hair[])ar_seg.ToArray(typeof(seg_of_hair));
				System.Diagnostics.Debug.WriteLine("image-listのsizeをどこかで調整しないと…");
				//---
				if (m_hair.Count == 0) {
					this.listView1.LargeImageList = hr.il_dm;
					this.listView2.LargeImageList = hr.il_ir;
				}
				for (int i = 0; i < segs.Length; i++) {
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
					}
					if (G.SS.MOZ_CND_NOMZ) {
						//断面・毛髄径計算は行わない
					}
#if false//2018.08.21
					else if (G.SS.MOZ_CND_PDFL == 1 && G.SS.MOZ_IRC_NOMZ) {
						G.SS.MOZ_IRC_NOMZ = G.SS.MOZ_IRC_NOMZ;//断面・毛髄径計算は行わない
					}
#endif
					else if (G.IR.CIR_CNT > 0) {
						if (m_bmp_ir1 != null && G.SS.MOZ_CND_FTCF > 0) {
							Form02.DO_SMOOTH(m_bmp_ir1, this.MOZ_CND_FTCF, this.MOZ_CND_FTCT);
						}
						test_pr1(segs[i]);
						if (m_dia_cnt > 1) {
							test_dm(segs, i, segs.Length);
						}
					}
				}
				dispose_bmp(ref m_bmp_dm1);
				dispose_bmp(ref m_bmp_dm2);
				dispose_bmp(ref m_bmp_ir1);
				dispose_bmp(ref m_bmp_ir2);
				if (G.bCANCEL) {
					break;
				}
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
				G.mlog(ex.ToString());
				string buf = ex.ToString();
			}
			if (dlg != null) {
			    dlg.Dispose();
			    dlg = null;
			}
			this.comboBox8.Tag = null;
		}
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
			this.checkBox6.Text = string.Format("R+{0}um", G.SS.MOZ_CND_CUTE);//R+3um
			this.checkBox7.Text = string.Format("R-{0}um", G.SS.MOZ_CND_CUTE);//R-3um
			//---
			this.chart1.Series[3].LegendText = this.checkBox6.Text;//R+3um
			this.chart1.Series[4].LegendText = this.checkBox7.Text;//R-3um
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
			this.comboBox2.SelectedIndex = 1;
			this.comboBox2.Enabled = true;
			//---
			//無し, 3x3, 5x5, 7x7, 9x9, 11x11
			this.MOZ_CND_FTCF = C_FILT_COFS[G.SS.MOZ_CND_FTCF];
			this.MOZ_CND_FTCT = C_FILT_CNTS[G.SS.MOZ_CND_FTCT];
			this.MOZ_CND_SMCF = C_SMTH_COFS[G.SS.MOZ_CND_SMCF];//重み係数=11
			this.MOZ_CND_FOLD = (G.SS.MOZ_CND_FMOD == 0) ? G.SS.AUT_BEF_PATH: G.SS.MOZ_CND_FOLD;
			//---
#if false//2018.08.21
			this.button1.Visible = false;
#endif
			//---
			//this.checkBox10.Checked = (G.SS.MOZ_CND_USIR == 1);
			//---
			m_map_of_dml = new Dictionary<string,ImageList>();
			m_map_of_irl = new Dictionary<string,ImageList>();
#if true//2018.08.21
			m_map_of_pdl = new Dictionary<string,ImageList>();
#endif
		}

		private void Form03_Load(object sender, EventArgs e)
		{
			if (true) {
				this.SetDesktopBounds(G.AS.APP_F02_LFT, G.AS.APP_F02_TOP, G.AS.APP_F02_WID, G.AS.APP_F02_HEI);
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
			init();
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
				//else {
				//    G.FORM02.Visible = false;
				//}
				G.CAM_PRC = G.CAM_STS.STS_HAIR;
				
				//this.timer1.Enabled = true;
				this.BeginInvoke(new G.DLG_VOID_VOID(this.load));
			}
		}
		private void Form03_FormClosing(object sender, FormClosingEventArgs e)
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
			//---
			G.FORM03 = null;
			G.FORM12.UPDSTS();
		}

		private double TO_VAL(object obj)
		{
			if (obj == null) {
				return(double.NaN);
			}
			if (obj is double) {
				return((double)obj);
			}
			else {
				Color c = (Color)obj;
				byte b = (byte)((4899 * c.R + 9617 * c.G + 1868 * c.B+8192) >> 14);
				return(b);
			}
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
			if (m_i >= m_hair.Count) {
				return;
			}
			hair hr = (hair)m_hair[m_i];
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
		}
#if false//2018.08.21
		private void CreateImageList(int hidx, string zpos)
		{
			hair hr = (hair)m_hair[hidx];
			ImageList il_dm, il_ir;

			il_dm = new ImageList();
			il_dm.ColorDepth = ColorDepth.Depth24Bit;
			il_dm.ImageSize = new Size((int)(0.8*100), (int)(0.8*80));
			il_ir = new ImageList();
			il_ir.ColorDepth = ColorDepth.Depth24Bit;
			il_ir.ImageSize = new Size((int)(0.8*100), (int)(0.8*80));
			//---
			string buf_cl, buf_ir;
			Image bmp_cl = null, bmp_ir = null;
			//---
			seg_of_hair seg;
			Image thm;
			//---
			string pext;
			if (string.Compare(zpos, "ZDEPT") == 0) {
				pext = m_fold_of_dept;
			}
			else {
				pext = "";
			}
			//---
			for (int q = 0;; q++) {
				if (q >= hr.seg.Length) {
					break;
				}
				seg = (seg_of_hair)hr.seg[q];
				if (seg == null) {
					continue;
				}

				buf_cl = this.textBox1.Text + pext + "\\" + seg.name_of_dm;
				buf_ir = this.textBox1.Text + pext + "\\" + seg.name_of_ir;
				if (seg.name_of_dm.Contains("_ZDEPT")) {
					buf_cl = Regex.Replace(buf_cl, "_ZDEPT", "_"+zpos);
					buf_ir = Regex.Replace(buf_ir, "_ZDEPT", "_"+zpos);
				}
				else {
					buf_cl = Regex.Replace(buf_cl, "_Z.[0-9][0-9].", "_"+zpos);
					buf_ir = Regex.Replace(buf_ir, "_Z.[0-9][0-9].", "_"+zpos);
				}
				bmp_cl = Bitmap.FromFile(buf_cl);
				if (System.IO.File.Exists(buf_ir)) {
				bmp_ir = Bitmap.FromFile(buf_ir);
				}
				else {
				bmp_ir = null;
				}
				thm = createThumbnail(bmp_cl, m_thm_wid, m_thm_hei);
				il_dm.Images.Add(thm);
				//
				if (bmp_ir != null) {
				thm = createThumbnail(bmp_ir, m_thm_wid, m_thm_hei);
				il_ir.Images.Add(thm);
				}
				dispose_bmp(ref bmp_cl);
				dispose_bmp(ref bmp_ir);
			}
			m_map_of_dml.Add(hidx.ToString() + zpos, il_dm);
			m_map_of_irl.Add(hidx.ToString() + zpos, il_ir);
		}
		private void draw_image(hair hr)
		{
			string buf_cl, buf_ir, buf_pd;
			Image bmp_cl = null, bmp_ir = null, bmp_pd = null;
			Bitmap bmp_all_cl = null, bmp_all_ir = null, bmp_all_pd = null;
			int Z = 8;
			float pw = 5;
			//---
			int idx = m_isel;
			seg_of_hair seg = (seg_of_hair)hr.seg[idx];
			string zpos, pext;
			ImageList il_dm = null, il_ir = null, il_pd = null;
			PictureBox[] pbox = { this.pictureBox1, this.pictureBox2, this.pictureBox3};
			for (int u = 0; u < 3; u++) {
				if (string.Compare(this.comboBox8.Text, "深度合成") == 0) {
					zpos = "ZDEPT";
					pext = m_fold_of_dept;
				}
				else {
					zpos = ZVAL2ORG(this.comboBox8.Text);
					pext = "";
				}
				if (this.radioButton3.Checked) {
					bmp_all_cl = new Bitmap((int)(hr.width_of_hair / Z), (int)(hr.height_of_hair / Z));
					bmp_all_ir = new Bitmap((int)(hr.width_of_hair / Z), (int)(hr.height_of_hair / Z));
					pw = 15;
					//---
					if (string.IsNullOrEmpty(zpos) || zpos.Contains("*")) {
						//条件設定値を選択中
						il_dm = hr.il_dm;
						il_ir = hr.il_ir;
					}
					else {
						string key = m_i.ToString() + zpos;
						if (m_map_of_dml.TryGetValue(key, out il_dm)) {
							m_map_of_irl.TryGetValue(key, out il_ir);
						}
						else {
							var dlg = new DlgProgress();
							dlg.Show("@", this);
							dlg.SetStatus("画像読込中...");
							CreateImageList(m_i, zpos);
							m_map_of_dml.TryGetValue(key, out il_dm);
							m_map_of_irl.TryGetValue(key, out il_ir);
							dlg.Hide();
							dlg.Dispose();
							dlg = null;
						}
					}
				}
				//---
				dispose_img(this.pictureBox1);
				dispose_img(this.pictureBox2);
				//---
				for (int q = 0; ; q++) {
					if (this.radioButton3.Checked) {
						//全体表示
						if (q >= hr.seg.Length) {
							break;
						}
						seg = (seg_of_hair)hr.seg[q];
						if (seg == null) {
							continue;
						}
						if (seg.cnt_of_moz <= 0) {
							//						continue;
						}
						bmp_cl = new Bitmap(seg.width / Z, seg.height / Z);
						if (!string.IsNullOrEmpty(seg.name_of_ir)) {
							bmp_ir = new Bitmap(seg.width / Z, seg.height / Z);
						}
						Graphics gr;
						gr = Graphics.FromImage(bmp_cl);
						gr.DrawImage(/*hr.*/il_dm.Images[q], 0, 0, seg.width / Z, seg.height / Z);
						gr.Dispose();
						if (!string.IsNullOrEmpty(seg.name_of_ir)) {
							gr = Graphics.FromImage(bmp_ir);
							gr.DrawImage(/*hr.*/il_ir.Images[q], 0, 0, seg.width / Z, seg.height / Z);
							gr.Dispose();
						}
					}
					else {
						//個別表示
						if (true) {
							buf_cl = this.textBox1.Text + pext + "\\" + seg.name_of_dm;
							buf_ir = this.textBox1.Text + pext + "\\" + seg.name_of_ir;
							zpos = zpos.Replace("*", "");

							if (!string.IsNullOrEmpty(zpos)) {
								zpos = "_" + zpos;

								if (seg.name_of_dm.Contains("_ZDEPT")) {
									buf_cl = Regex.Replace(buf_cl, "_ZDEPT", zpos);
									buf_ir = Regex.Replace(buf_ir, "_ZDEPT", zpos);
								}
								else {
									buf_cl = Regex.Replace(buf_cl, "_Z.[0-9][0-9].", zpos);
									buf_ir = Regex.Replace(buf_ir, "_Z.[0-9][0-9].", zpos);
								}
							}
						}
						if (false/*this.checkBox10.Checked*/) {
							if (System.IO.File.Exists(buf_ir)) {
								string tmp;
								tmp = this.textBox1.Text + "\\" + seg.name_of_ir.Replace("IR", "IZ");
								if (System.IO.File.Exists(tmp)) {
									buf_cl = tmp;
								}
							}
						}

						bmp_cl = Bitmap.FromFile(buf_cl);
						if (System.IO.File.Exists(buf_ir)) {
							bmp_ir = Bitmap.FromFile(buf_ir);
						}
						else {
							bmp_ir = null;
						}
					}

					if (this.checkBox1.Checked && seg.val_xum.Count > 0) {//断面・ライン
						Graphics gr = Graphics.FromImage(bmp_cl);
						Pen pen = new Pen(Color.Green, 4);
						if (this.radioButton3.Checked) {
							//全体表示
							gr.ScaleTransform(1f / Z, 1f / Z);
						}
						if (this.checkBox3.Checked) {//R*0
							pen = new Pen(this.chart1.Series[0].Color, pw);
							Point[] ap = (Point[])seg.pts_cen.ToArray(typeof(Point));
							gr.DrawLines(pen, ap);
						}
						if (this.checkBox4.Checked) {//R*+50%
							pen = new Pen(this.chart1.Series[1].Color, pw);
							Point[] ap = (Point[])seg.pts_phf.ToArray(typeof(Point));
							gr.DrawLines(pen, ap);
						}
						if (this.checkBox5.Checked) {//R*-50%
							pen = new Pen(this.chart1.Series[2].Color, pw);
							Point[] ap = (Point[])seg.pts_mph.ToArray(typeof(Point));
							gr.DrawLines(pen, ap);
						}
						if (this.checkBox6.Checked) {//R+5u
							pen = new Pen(this.chart1.Series[3].Color, pw);
							Point[] ap = (Point[])seg.pts_p5u.ToArray(typeof(Point));
							gr.DrawLines(pen, ap);
						}
						if (this.checkBox7.Checked) {//R-5u
							pen = new Pen(this.chart1.Series[4].Color, pw);
							Point[] ap = (Point[])seg.pts_m5u.ToArray(typeof(Point));
							gr.DrawLines(pen, ap);
						}
						pen.Dispose();
						gr.Dispose();
					}

					if (bmp_ir != null && seg.val_xum.Count > 0) {//赤外あり？
						object obj = seg.moz_zpb[0];
						//System.Diagnostics.Debug.WriteLine(obj);
						Graphics gr = Graphics.FromImage(bmp_ir);
						Pen pen = null;
						Point[] ap;
						if (this.radioButton3.Checked) {
							//全体表示
							gr.ScaleTransform(1f / Z, 1f / Z);
						}
						if (this.checkBox2.Checked) {//赤外・輪郭
							pen = new Pen(Color.Blue, pw);
							ap = (Point[])seg.moz_top.ToArray(typeof(Point));
							gr.DrawLines(pen, ap);
							//---
							ap = (Point[])seg.moz_btm.ToArray(typeof(Point));
							gr.DrawLines(pen, ap);
						}
						if (this.checkBox8.Checked) {//赤外・中心ライン
							pen = new Pen(this.chart1.Series[0].Color, pw);
							ap = (Point[])seg.pts_cen.ToArray(typeof(Point));
							gr.DrawLines(pen, ap);
						}
						if (this.checkBox9.Checked) {//赤外・毛髄径
							pen = new Pen(Color.Green, pw);
							for (int i = 0; i < seg.moz_zpb.Count; i += 1) {
								Point p1 = (Point)seg.moz_zpb[i];
								Point p2 = (Point)seg.moz_zpt[i];

								//if (p1.X != 0 && p1.Y != 0) {
								//    i = i;
								//}
								//if (true) {
								//    gr.DrawLine(Pens.LightGreen,  (Point)seg.moz_btm[i], (Point)seg.moz_top[i]);
								//}
								if (p1.X == 0 && p2.Y == 0) {
								}
								else if (p1.X == p2.X && p1.Y == p2.Y) {
									//i = i;
									//gr.FillRectangle(Brushes.LightGreen, p1.X-3, p1.Y-3, 7, 7);
								}
								else {
									//gr.DrawLine(pen, p1.X, p1.Y, p1.X+1, p1.Y+2);
									//gr.DrawLine(pen, p2.X, p2.Y, p2.X+1, p2.Y+2);
									gr.DrawLine(pen, p1, p2);
								}
							}
						}
						if (pen != null) {
							pen.Dispose();
						}
						gr.Dispose();
					}
					if (!this.radioButton3.Checked) {
						break;
					}
					if (true) {
						Graphics gr;
						gr = Graphics.FromImage(bmp_all_cl);
						gr.DrawImage(bmp_cl, (seg.pix_pos.X / Z), (seg.pix_pos.Y / Z), seg.width / Z, seg.height / Z);
						gr.Dispose();
					}
					if (bmp_ir != null) {
						Graphics gr;
						gr = Graphics.FromImage(bmp_all_ir);
						gr.DrawImage(bmp_ir, (seg.pix_pos.X / Z), (seg.pix_pos.Y / Z), seg.width / Z, seg.height / Z);
						gr.Dispose();
					}
					dispose_bmp(ref bmp_cl);
					dispose_bmp(ref bmp_ir);
				}
				if (this.radioButton3.Checked) {
					this.pictureBox1.Image = bmp_all_cl;
					if (bmp_all_ir != null) {
						this.pictureBox2.Image = bmp_all_ir;
					}
				}
				else {
					this.pictureBox1.Image = bmp_cl;
					if (bmp_ir != null) {
						this.pictureBox2.Image = bmp_ir;
					}
				}
			}
		}
#else
		private void CreateImageList(int hidx, string[] names, Dictionary<string, ImageList> map, string zpos)
		{
			string path;
			Image bmp = null;
			ImageList il;
			Image thm;

			il = new ImageList();
			il.ColorDepth = ColorDepth.Depth24Bit;
			il.ImageSize = new Size((int)(0.8 * 100), (int)(0.8 * 80));
			//---
			for (int q = 0; q < names.Length; q++) {				
				path = to_xx_path(names[q], zpos);
				bmp = to_img_from_file(path);

				if (bmp != null) {
					thm = createThumbnail(bmp, m_thm_wid, m_thm_hei);
					il.Images.Add(thm);
				}
				dispose_bmp(ref bmp);
			}
			map.Add(hidx.ToString() + zpos, il);
		}
		private void prep_image_list(hair hr, int hidx, ref ImageList il_dm, ref ImageList il_pd, ref ImageList il_ir)
		{
			string kh = hidx.ToString();
			Dictionary<string, ImageList>[] maps = { m_map_of_dml, m_map_of_pdl, m_map_of_irl };
			ImageList il;
			ImageList[] ils = {null, null, null};
			string[] zary = { null, null, null};

			zary[0] = ZVAL2ORG(this.comboBox10.Text); //danmen
			zary[1] = ZVAL2ORG(this.comboBox8.Text); //kei
			zary[2] = ZVAL2ORG(this.comboBox12.Text); //mouzui

			for (int i = 0; i < 3; i++) {
				if (!maps[i].TryGetValue(kh + zary[i], out il)) {
					il = null;//ng
				}
				ils[i] = il;
			}
			if (ils[0] == null || ils[1] == null || ils[2] == null) {
				var dlg = new DlgProgress();
				dlg.Show("@", this);
				dlg.SetStatus("画像読込中...");
				for (int i = 0; i < 3; i++) {
					if (ils[i] != null) {
						continue;
					}
					string[] names = to_name_arr(i, hr.seg);
					CreateImageList(m_i, names, maps[i], zary[i]);

					maps[i].TryGetValue(kh + zary[i], out il);
					ils[i] = il;
				}
				dlg.Hide();
				dlg.Dispose();
				dlg = null;
			}
			il_dm = ils[0];
			il_pd = ils[1];
			il_ir = ils[2];
		}
		private void draw_image(hair hr)
		{
			string buf_dm, buf_ir, buf_pd;
			Image bmp_dm = null, bmp_ir = null, bmp_pd = null;
			Bitmap bmp_all_dm = null, bmp_all_ir = null, bmp_all_pd = null;
			int	Z = 8;
			float pw = 5;
			//---
			int idx = m_isel;
			seg_of_hair seg = (seg_of_hair)hr.seg[idx];
			string zpos, pext;
			ImageList il_dm = null, il_ir = null, il_pd = null;

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
				prep_image_list(hr, m_i, ref il_dm, ref il_pd, ref il_ir);
			}
			//---
			dispose_img(this.pictureBox1);
			dispose_img(this.pictureBox2);
			dispose_img(this.pictureBox3);
			//---
			for (int q = 0;; q++) {
				if (this.radioButton3.Checked) {
					//全体表示
					if (q >= hr.seg.Length) {
						break;
					}
					seg = (seg_of_hair)hr.seg[q];
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
						buf_dm = to_xx_path(seg.path_of_dm, ZVAL2ORG(this.comboBox10.Text));
						buf_pd = to_xx_path(seg.path_of_pd, ZVAL2ORG(this.comboBox8.Text));
						buf_ir = to_xx_path(seg.path_of_ir, ZVAL2ORG(this.comboBox12.Text));
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

					bmp_dm = to_img_from_file(buf_dm);
					bmp_pd = to_img_from_file(buf_pd);
					bmp_ir = to_img_from_file(buf_ir);
				}

				if (this.checkBox1.Checked && seg.val_xum.Count > 0) {//断面・ライン
					Graphics gr = Graphics.FromImage(bmp_dm);
					Pen pen = new Pen(Color.Green, 4);
					if (this.radioButton3.Checked) {
						//全体表示
						gr.ScaleTransform(1f/Z, 1f/Z);
					}
					if (this.checkBox3.Checked) {//R*0
						pen = new Pen(this.chart1.Series[0].Color, pw);
						Point[] ap = (Point[])seg.pts_cen.ToArray(typeof(Point));
						gr.DrawLines(pen, ap);
					}
					if (this.checkBox4.Checked) {//R*+50%
						pen = new Pen(this.chart1.Series[1].Color, pw);
						Point[] ap = (Point[])seg.pts_phf.ToArray(typeof(Point));
						gr.DrawLines(pen, ap);
					}
					if (this.checkBox5.Checked) {//R*-50%
						pen = new Pen(this.chart1.Series[2].Color, pw);
						Point[] ap = (Point[])seg.pts_mph.ToArray(typeof(Point));
						gr.DrawLines(pen, ap);
					}
					if (this.checkBox6.Checked) {//R+5u
						pen = new Pen(this.chart1.Series[3].Color, pw);
						Point[] ap = (Point[])seg.pts_p5u.ToArray(typeof(Point));
						gr.DrawLines(pen, ap);
					}
					if (this.checkBox7.Checked) {//R-5u
						pen = new Pen(this.chart1.Series[4].Color, pw);
						Point[] ap = (Point[])seg.pts_m5u.ToArray(typeof(Point));
						gr.DrawLines(pen, ap);
					}
					pen.Dispose();
					gr.Dispose();
				}

				if (bmp_ir != null && seg.val_xum.Count > 0) {//赤外あり？
					object obj = seg.moz_zpb[0];
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
					if (this.checkBox8.Checked) {//赤外・中心ライン
						pen = new Pen(this.chart1.Series[0].Color, pw);
						ap = (Point[])seg.pts_cen.ToArray(typeof(Point));
						gr_ir.DrawLines(pen, ap);
						gr_pd.DrawLines(pen, ap);
					}
					if (this.checkBox9.Checked) {//赤外・毛髄径
						pen = new Pen(Color.Green, pw);
						for (int i = 0; i < seg.moz_zpb.Count; i+=1) {
							Point p1 = (Point)seg.moz_zpb[i];
							Point p2 = (Point)seg.moz_zpt[i];

							//if (p1.X != 0 && p1.Y != 0) {
							//    i = i;
							//}
							//if (true) {
							//    gr.DrawLine(Pens.LightGreen,  (Point)seg.moz_btm[i], (Point)seg.moz_top[i]);
							//}
							if (p1.X == 0 && p2.Y == 0) {
							}
							else if (p1.X == p2.X && p1.Y == p2.Y) {
								//i = i;
								//gr.FillRectangle(Brushes.LightGreen, p1.X-3, p1.Y-3, 7, 7);
							}
							else {
							//gr.DrawLine(pen, p1.X, p1.Y, p1.X+1, p1.Y+2);
							//gr.DrawLine(pen, p2.X, p2.Y, p2.X+1, p2.Y+2);
								gr_ir.DrawLine(pen, p1, p2);
							}
						}
					}
					if (pen != null) {
						pen.Dispose();
					}
					gr_pd.Dispose();
					gr_ir.Dispose();
				}
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
				dispose_bmp(ref bmp_dm);
				dispose_bmp(ref bmp_ir);
				dispose_bmp(ref bmp_pd);
			}
			if (this.radioButton3.Checked) {
				if (true) {
					this.pictureBox1.Image = bmp_all_dm;
					this.pictureBox3.Image = bmp_all_pd;
				}
				if (bmp_all_ir != null) {
					this.pictureBox2.Image = bmp_all_ir;
				}
			}
			else {
				if (true) {
					this.pictureBox1.Image = bmp_dm;
					this.pictureBox3.Image = bmp_pd;
				}
				if (bmp_ir != null) {
					this.pictureBox2.Image = bmp_ir;
				}
			}
		}
#endif
		private void draw_graph(hair hr)
		{
			try {
			//---
			int idx = m_isel;
			seg_of_hair seg = (seg_of_hair)hr.seg[idx];
			double moz_kei_max = -1;
#if true//2018.08.21
			double mou_kei_max = -1;
#endif
			//---
			this.chart1.Series[0].Points.Clear();
			this.chart1.Series[1].Points.Clear();
			this.chart1.Series[2].Points.Clear();
			this.chart1.Series[3].Points.Clear();
			this.chart1.Series[4].Points.Clear();
			this.chart2.Series[0].Points.Clear();
#if false//2018.08.21
			this.chart2.Series[1].Points.Clear();
#else
			this.chart3.Series[0].Points.Clear();
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
			seg_of_hair seg_bak = seg;
			double		offs = 0;
			double		xmin = 0;

			if (this.radioButton1.Checked) {
				double um_of_width = G.PX2UM(seg.width, m_log_info.pix_pitch, m_log_info.zoom);
				for (int q = 0;; q++) {
					if (q >= hr.seg.Length) {
						return;
					}
					seg = (seg_of_hair)hr.seg[q];
					if (seg != null && seg.val_xum.Count > 0) {
						break;
					}
					offs -= um_of_width;
				}
				xmin = offs;
			}
			else {
				if (seg.val_xum.Count <= 0) {
					return;
				}
			}

			for (int q = 0;; q++) {
				int i0;
				if (this.radioButton1.Checked) {
					if (q >= hr.seg.Length) {
						break;
					}
					seg = (seg_of_hair)hr.seg[q];
					if (seg == null) {
						continue;
					}
					if (seg.val_xum.Count <= 0) {
						double um = G.PX2UM(seg.width, m_log_info.pix_pitch, m_log_info.zoom);
						double x0 = um+ offs;
						this.chart1.Series[0].Points.AddXY(x0, double.NaN);
						this.chart1.Series[1].Points.AddXY(x0, double.NaN);
						this.chart1.Series[2].Points.AddXY(x0, double.NaN);
						this.chart1.Series[3].Points.AddXY(x0, double.NaN);
						this.chart1.Series[4].Points.AddXY(x0, double.NaN);
						this.chart2.Series[0].Points.AddXY(x0, double.NaN);
#if false//2018.08.21
						this.chart2.Series[1].Points.AddXY(x0, double.NaN);
#else
						this.chart3.Series[0].Points.AddXY(x0, double.NaN);
#endif
						offs += um;
						continue;
					}
					for (i0 = 0; TO_VAL(seg.val_xum[i0]) < 0; i0++) {
					}
					if (m_chk1!=0) {
						i0 = 0;
					}
				}
				else {
					i0 = 0;
					offs = -TO_VAL(seg.val_xum[0]);
				}
				for (int i = i0; i < seg.val_xum.Count; i++) {
					double x0 = TO_VAL(seg.val_xum[i]) + offs;
					double y0 = TO_VAL(seg.val_p5u[i]);
					double y1 = TO_VAL(seg.val_phf[i]);
					double y2 = TO_VAL(seg.val_cen[i]);
					double y3 = TO_VAL(seg.val_mph[i]);
					double y4 = TO_VAL(seg.val_m5u[i]);
					//double y5 = TO_VAL(seg.moz_zpl[i]);
					if (this.checkBox3.Checked) {//R*0
						this.chart1.Series[0].Points.AddXY(x0, y2);
					}
					if (this.checkBox4.Checked) {//R*+50%
						this.chart1.Series[1].Points.AddXY(x0, y1);
					}
					if (this.checkBox5.Checked) {//R*-50%
						this.chart1.Series[2].Points.AddXY(x0, y3);
					}
					if (this.checkBox6.Checked) {//R+5u
						this.chart1.Series[3].Points.AddXY(x0, y0);
					}
					if (this.checkBox7.Checked) {//R-5u
						this.chart1.Series[4].Points.AddXY(x0, y4);
					}
				}
				for (int i = i0; i < seg.moz_zpl.Count; i++) {
					double x0 = TO_VAL(seg.val_xum[i]) + offs;
					double y5 = TO_VAL(seg.moz_zpl[i]);
					double y6 = TO_VAL(seg.mou_len[i]);
					if (this.checkBox11.Checked) {
						this.chart2.Series[0].Points.AddXY(x0, y5);
						if (moz_kei_max < y5) {
							moz_kei_max = y5;
						}
					}
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
				double	dx = TO_VAL(seg.val_xum[1])-TO_VAL(seg.val_xum[0]);
				//offs += dx * seg.moz_zpl.Count;
				offs += TO_VAL(seg.val_xum[seg.val_xum.Count-1])+dx;
			}
			if (this.radioButton1.Checked) {
				seg = seg_bak;
			}
			if (true) {
				this.chart1.Series[0].Color = Color.Cyan;		//R*0
				this.chart1.Series[1].Color = Color.Green;		//R*+50%
				this.chart1.Series[2].Color = Color.Magenta;	//R*-50%
				this.chart1.Series[3].Color = Color.Blue;//R+3um
				this.chart1.Series[4].Color = Color.Red;	//R-3um
				this.chart2.Series[0].Color = Color.Green;	//毛髄径
				//---
				this.chart1.Series[0].Enabled = this.checkBox3.Checked;
				this.chart1.Series[1].Enabled = this.checkBox4.Checked;
				this.chart1.Series[2].Enabled = this.checkBox5.Checked;
				this.chart1.Series[3].Enabled = this.checkBox6.Checked;
				this.chart1.Series[4].Enabled = this.checkBox7.Checked;
				this.chart2.Series[0].Enabled = this.checkBox11.Checked;
#if false//2018.08.21
				this.chart2.Series[1].Enabled = this.checkBox12.Checked;
#else
				this.chart3.Series[0].Enabled = this.checkBox12.Checked;
#endif
				//---
				//this.chart1.ChartAreas[0].AxisX.Title = "[um]";
				//this.chart1.ChartAreas[0].AxisY.Title = "PIXEL VALUE";
				//this.chart2.ChartAreas[0].AxisX.Title = "[um]";
				//this.chart2.ChartAreas[0].AxisY.Title = "[um]";
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
				//if (this.radioButton1.Checked) {
				////this.chart1.ChartAreas[0].AxisX.Crossing = this.chart1.ChartAreas[0].AxisX.Maximum/10;
				//}
				//else {
				//this.chart1.ChartAreas[0].AxisX.Crossing = double.NaN;
				//}
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
			if (m_i >= m_hair.Count) {
				return;
			}
			hair hr = (hair)m_hair[m_i];
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
			//this.button1.Visible = !this.button1.Visible;
			if ((q & 1) != 0) {
				draw_image(hr);
			}
			if ((q & 2) != 0) {
				draw_graph(hr);
			}
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
				hair hr = (hair)m_hair[m_i];
				this.listView1.LargeImageList = hr.il_dm;
				try {
				this.listView2.LargeImageList = hr.il_ir;
				}
				catch (Exception ex) {
					G.mlog(ex.ToString());
				}
				for (int i = 0; i < hr.cnt_of_seg; i++) {
					seg_of_hair seg = hr.seg[i];
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

		private void button3_Click(object sender, EventArgs e)
		{
			//保存
/*
フォルダ			C:\temp\test_20171108_024440					
ファイル・カラー	1CL_10.JPG	-	1CL_20.JPG			
ファイル・赤外		1IR_10.JPG	-	1IR_20.JPG			
毛髄閾値			50					
							
毛髪位置(um)		断面・R*0		断面・R+3um		断面・R-3um		毛髄径(um)		ファイル・カラー	ファイル・赤外
	1				255				255				255				1				1CL_10.JPG			1IR_10.JPG
	2				255				255				255				2			
	3				100				100				100				3			
 */

			SaveFileDialog	dlg = new SaveFileDialog();
			string path = this.MOZ_CND_FOLD;

			dlg.Filter = "csv(*.csv)|*.csv|All files (*.*)|*.*";
			dlg.FilterIndex = 1;
			dlg.DefaultExt = "csv";
			dlg.InitialDirectory = this.MOZ_CND_FOLD;
			switch (this.comboBox2.SelectedIndex) {
			case 0://現在の毛髪
				dlg.FileName = string.Format("{0}_ALL.csv", m_i);
			break;
			case 1://現在のグラフ
				dlg.FileName = string.Format("{0}_{1:00}.csv", m_i, m_isel);
			break;
			default:
				return;
				//dlg.FileName = string.Format("{0}_ALL.csv", m_i);
			//break;
			}
			if (dlg.ShowDialog() != DialogResult.OK) {
				return;
			}
			bool[] chk = new bool[] {
				true,
				this.checkBox3.Checked,
				this.checkBox4.Checked,
				this.checkBox5.Checked,
				this.checkBox6.Checked,
				this.checkBox7.Checked,
				this.radioButton8.Enabled,
				true,
				true
			};
			string[] header = new string[] {
				"毛髪位置(um)",
				"断面・R*0",
				"断面・R*+50%",
				"断面・R*-50%",
				"断面・R+3um",
				"断面・R-3um",
				"毛髄径(um)",
				"毛髪径(um)",
				"ファイル・カラー",
				"ファイル・赤外"
			};
			StreamWriter wr;
			StringBuilder buf = new StringBuilder();
			
			int	i_s, i_e;
			double	offs=0;

			header[4] = string.Format("断面・R+{0}um", G.SS.MOZ_CND_CUTE);//R+3um
			header[5] = string.Format("断面・R-{0}um", G.SS.MOZ_CND_CUTE);//R-3um

			try {
				hair hr = (hair)m_hair[m_i];
				/*				rd = new StreamReader(filename, Encoding.GetEncoding("Shift_JIS"));*/
				wr = new StreamWriter(dlg.FileName, false, Encoding.Default);
				wr.WriteLine(string.Format("フォルダ,{0}", this.MOZ_CND_FOLD));
				wr.WriteLine(string.Format("走査単位[um],{0}", G.SS.MOZ_CND_DSUM));
				wr.WriteLine(string.Format("平滑化フィルタ,{0}x{0}/{1}回", C_FILT_COFS[G.SS.MOZ_CND_FTCF], C_FILT_CNTS[G.SS.MOZ_CND_FTCT]));
				wr.WriteLine(string.Format("スムージング係数,{0}", C_SMTH_COFS[G.SS.MOZ_CND_SMCF], C_FILT_CNTS[G.SS.MOZ_CND_FTCT]));
				wr.WriteLine(string.Format("コントラスト補正,{0}", G.SS.MOZ_CND_CTRA.ToString()));
				wr.WriteLine(string.Format("毛髄閾値,{0}", G.SS.MOZ_CND_ZVAL));
				wr.WriteLine(string.Format("径方向・毛髄判定範囲[%],{0}", G.SS.MOZ_CND_HANI));
				wr.WriteLine("");
				for (int i = 0; i < chk.Length; i++) {
					if (chk[i]) {
						buf.Append(header[i]);
						buf.Append(",");
					}
				}
				buf.Remove(buf.Length-1, 1);
				wr.WriteLine(buf);
				if (this.comboBox2.SelectedIndex == 0) {
					//現在の毛髪
					i_s = 0;
					i_e = hr.cnt_of_seg-1;
				}
				else {
					//現在のグラフ
					i_s = i_e = m_isel;
				}
				for (int q = i_s; q <= i_e; q++) {
					int i0;
					seg_of_hair seg = (seg_of_hair)hr.seg[q];

					if (this.comboBox2.SelectedIndex == 0) {
						//現在の毛髪
						if (q >= hr.seg.Length) {
							break;
						}
						seg = (seg_of_hair)hr.seg[q];
						if (seg == null || seg.val_xum.Count <= 0) {
							continue;
						}
						for (i0 = 0; TO_VAL(seg.val_xum[i0]) < 0; i0++) {
						}
						if (m_chk1!=0) {
							i0 = 0;
						}
					}
					else {
						//現在のグラフ
						i0 = 0;
						offs = -TO_VAL(seg.val_xum[0]);
					}

					for (int i = i0; i < seg.val_xum.Count; i++) {
						buf.Clear();
						if (true) {
							buf.Append(string.Format("{0:F1}", offs + TO_VAL(seg.val_xum[i])));
							buf.Append(",");
						}
						if (chk[1]) {
							buf.Append(string.Format("{0:F0}", TO_VAL(seg.val_cen[i])));
							buf.Append(",");
						}
						if (chk[2]) {
							buf.Append(string.Format("{0:F0}", TO_VAL(seg.val_p5u[i])));
							buf.Append(",");
						}
						if (chk[3]) {
							buf.Append(string.Format("{0:F0}", TO_VAL(seg.val_mph[i])));
							buf.Append(",");
						}
						if (chk[4]) {
							buf.Append(string.Format("{0:F0}", TO_VAL(seg.val_p5u[i])));
							buf.Append(",");
						}
						if (chk[5]) {
							buf.Append(string.Format("{0:F0}", TO_VAL(seg.val_m5u[i])));
							buf.Append(",");
						}
						if (chk[6]) {
							buf.Append(string.Format("{0:F1}", TO_VAL(seg.moz_zpl[i])));
							buf.Append(",");
						}
						if (chk[7]) {
							buf.Append(string.Format("{0:F1}", TO_VAL(seg.mou_len[i])));
							buf.Append(",");
						}
						if (i == i0) {
							buf.Append(seg.name_of_dm);
							buf.Append(",");
							if (seg.name_of_ir.Length > 0) {
							buf.Append(seg.name_of_ir);
							buf.Append(",");
							}
						}
						buf.Remove(buf.Length-1, 1);
						wr.WriteLine(buf);
					}
					double dx = TO_VAL(seg.val_xum[1])-TO_VAL(seg.val_xum[0]);
				
					offs += TO_VAL(seg.val_xum[seg.val_xum.Count-1])+dx;
				}
				wr.Close();
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			m_chk1 = 1-m_chk1;
			//radioButton1_CheckedChanged(null, null);
			double x1 = this.chart1.ChartAreas[0].AxisX.ValueToPixelPosition(this.chart1.ChartAreas[0].AxisX.Minimum);
			double x2 = this.chart1.ChartAreas[0].AxisX.ValueToPixelPosition(this.chart2.ChartAreas[0].AxisX.Minimum);

			if (this.chart2.Visible) {
				this.chart2.Visible = false;
				this.propertyGrid1.Visible = true;
				this.propertyGrid1.SelectedObject = this.tableLayoutPanel1;
				//this.propertyGrid1.SelectedObject = this.chart1;
				this.propertyGrid1.SelectedObject = this.pictureBox1;
				this.propertyGrid1.Dock = DockStyle.Fill;
			}
			else {
				this.chart2.Visible = true;
				this.propertyGrid1.Visible = false;
			}
		}

		private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.comboBox8.Tag != null) {
				return;
			}
			hair hr = (hair)m_hair[m_i];
			if (m_isel >= hr.seg.Count()) {
				return;
			}
			if (true/*this.radioButton4.Checked*/) {
				draw_image(hr);
			}
		}
	}
}
