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
		private int[] C_FILT_COFS = new int[] { 0, 3, 5, 7, 9, 11 };
		private int[] C_FILT_CNTS = new int[] { 1, 5,10, 15, 20};
		private int[] C_SMTH_COFS = new int[] { 0,5,7,9,11,13,15,17,19,21,23, 25};
		private int m_i = 0;
		private int m_isel = 0;
#if true//2018.10.10(毛髪径算出・改造)
		private int m_imou = 0;
#endif
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
#if true//2018.10.10(毛髪径算出・改造)
		public string m_errstr;
#endif
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
#if false//2018.10.10(毛髪径算出・改造)
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
#endif
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
#if true//2018.10.10(毛髪径算出・改造)
		public struct seg_of_mouz {
			//毛髄径方向データ(一列分)
			public Point[]		pbf;//座標バッファ(毛髪下端から上端まで)
			public double[]		ibf;//画素バッファ(毛髪下端から上端まで)
			public double[]		iaf;//画素バッファ(毛髪下端から上端まで):補正後
			public Point		phc;//毛髪中心(バッファの中心)
			public int			ihc;//バッファの中心インデックス
			public int			iml;//バッファの毛髄上端のインデックス
			public int			imr;//バッファの毛髄下端のインデックス
			public int			imc;//バッファの毛髄中心のインデックス
			public Point		pml;//バッファの毛髄上端の座標
			public Point		pmr;//バッファの毛髄下端の座標
			public PointF		pmc;//バッファの毛髄中心の座標
			public double		ddf;//毛髪中心から毛髄中心までの距離
			//---
			public bool			fs2;//S1,S2区分,S1:true, S2:false
			public double		avg;//毛髄:毛髄範囲の画素平均値(生画像による)
#if true//2018.11.06(毛髄4)
			public int			ihl;
			public int			ihr;
#endif
			//---
			public void clear() {
				if (pbf != null) {
					pbf = null;
				}
				if (ibf != null) {
					ibf = null;
				}
				if (iaf != null) {
					iaf = null;
				}
			}
		}
#endif
#if true//2018.10.30(キューティクル長)
		public class seg_of_cuti {
			//毛髄径方向データ(一列分)
			public List<Point>		pbf;//座標バッファ(毛髪左端から右端まで)
			public List<double>		ibf;//画素バッファ(毛髪左端から右端まで)
			public List<double>		iaf;//画素バッファ(毛髪下端から上端まで):フィルター処理後
			public List<Point>		phc;//毛髪中心(バッファの中心)
			public List<int>		ihc;//バッファの中心インデックス
			//public int			iml;//バッファの毛髄上端のインデックス
			//public int			imr;//バッファの毛髄下端のインデックス
			//public int			imc;//バッファの毛髄中心のインデックス
			//public Point		pml;//バッファの毛髄上端の座標
			//public Point		pmr;//バッファの毛髄下端の座標
			//public PointF		pmc;//バッファの毛髄中心の座標
			//public double		ddf;//毛髪中心から毛髄中心までの距離
			//---
			//public bool			fs2;//S1,S2区分,S1:true, S2:false
			//public double		avg;//毛髄:毛髄範囲の画素平均値(生画像による)
			public List<Point>		pct;
#if true//2018.11.28(メモリリーク)
			public LIST_U8			flg;//キューティクル・ライン該当・フラグ
#else
			public List<bool>		flg;//キューティクル・ライン該当・フラグ
#endif
			public List<int>		his;//キューティクル間隔・ヒストグラム
			public List<int>		lbl;//キューティクル・ライン該当・ラベル
#if false//2018.12.10(64ビット化)
			public List<byte>		tst;
#endif
			//---
			public seg_of_cuti() {
				this.pbf = new List<Point> ();
				this.ibf = new List<double>();
				this.iaf = new List<double>();
				this.phc = new List<Point> ();
				this.ihc = new List<int>   ();
#if true//2018.11.28(メモリリーク)
				this.flg = new LIST_U8     ();
#else
				this.flg = new List<bool>  ();
#endif
				this.his = new List<int>   ();
				this.lbl = new List<int>   ();
#if false//2018.12.10(64ビット化)
				this.tst = new List<byte>(1024*128*5);
#endif
			}
			//---
			public void clear() {
				if (pbf != null) {
					pbf = null;
				}
				if (ibf != null) {
					ibf = null;
				}
				if (iaf != null) {
					iaf = null;
				}
				if (phc != null) {
					phc = null;
				}
				if (ihc != null) {
					ihc = null;
				}
#if false//2018.12.10(64ビット化)
				if (tst != null) {
					tst = null;
				}
#endif
			}
		}
#endif
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
#if true //2018.12.17(オーバーラップ範囲)
			public int ow_l_wid;
			public int ow_r_wid;
			public int ow_l_pos;//    0+ow_l_wid
			public int ow_r_pos;//width-ow_r_wid
			public double ow_l_xum;//    0+ow_l_wid
			public double ow_r_xum;//width-ow_r_wid
#endif
#if true//2019.03.16(NODATA対応)
			public double contr;
			public double contr_avg;
			public double contr_drop;
			public bool bNODATA;
#endif
			//public int 
			//public float start_pix_of_seg;
			//カラー画像と
			//赤外画像による情報
			public int	 cnt_of_val;
			public ArrayList val_xum;//断面:X位置
			public ArrayList val_cen;//断面:中心
#if false//2019.02.16(数値化白髪オフセット)
			public ArrayList val_p5u;//断面:上端+5um
			public ArrayList val_phf;//断面:上側中点
			public ArrayList val_mph;//断面:下側中点
			public ArrayList val_m5u;//断面:下端-5um
#endif
#if true//2018.09.29(キューティクルライン検出)
			public ArrayList val_cen_fil;//断面:中心のフィルター処理後
#if false//2019.02.16(数値化白髪オフセット)
			public ArrayList val_phf_fil;
			public ArrayList val_mph_fil;
			public ArrayList val_p5u_fil;
			public ArrayList val_m5u_fil;
#endif
			public ArrayList pts_cen_cut;//中心ライン上のキューティクル・ライン該当・点
#if false//2019.02.16(数値化白髪オフセット)
			public ArrayList pts_phf_cut;
			public ArrayList pts_mph_cut;
			public ArrayList pts_p5u_cut;
			public ArrayList pts_m5u_cut;
#endif
#if true//2018.11.28(メモリリーク)
			public LIST_U8 flg_cen_cut;//中心ライン上のキューティクル・ライン該当・フラグ
			public LIST_U8 flg_phf_cut;
			public LIST_U8 flg_mph_cut;
			public LIST_U8 flg_p5u_cut;
			public LIST_U8 flg_m5u_cut;
#endif
			public List<int> his_cen_cut;
			public List<int> his_phf_cut;
			public List<int> his_mph_cut;
			public List<int> his_p5u_cut;
			public List<int> his_m5u_cut;
#endif
			//---
			public int	 cnt_of_pos;
			public ArrayList	 pts_cen;
#if true//2019.03.22(再測定表)
			public List<Point>	pts_cen_ofs;//毛髪センターの点集合(オフセットされた)
			public List<object> val_cen_ofs;//断面:中心
#endif
#if false//2019.02.16(数値化白髪オフセット)
			public ArrayList	 pts_p5u;
			public ArrayList	 pts_phf;
			public ArrayList	 pts_mph;
			public ArrayList	 pts_m5u;
#endif
#if true//2018.10.30(キューティクル長)
			public List<seg_of_cuti>
								cut_inf;
			public List<List<Point>>
								cut_lsp;
			public List<double>	cut_len;
			public double		cut_ttl;
#endif
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
#if true//2018.10.10(毛髪径算出・改造)
			public List<seg_of_mouz>
								moz_inf;
			public List<bool>	moz_out;//外れ:true, 正常:false
			public List<int>	moz_lbl;//ラベル番号(0:毛髄無し,以外:毛髄領域の連番)
			public List<double> moz_sum;//該当ラベルの積算値を格納([0]にラベル1の積算値)
			public List<Point>	moz_hpt;//毛髄:上側点:補間後
			public List<Point>	moz_hpb;//毛髄:下側点:補間後
			public List<double>	moz_hpl;//毛髄:長さ径:補間後
			public List<seg_of_mouz>
								moz_hnf;//毛髄:補間後
			public double		moz_rsl;//毛髄面積:Sl
			public double		moz_rsd;//毛髄面積:Sd
			public double		moz_hsl;//毛髄面積:Sl(補間後)
			public double		moz_hsd;//毛髄面積:Sd(補間後)
#endif
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
			public Point[]	han_top;
			public Point[]	han_btm;
#else
			public ArrayList dia_top;//輪郭・頂点(上側)
			public ArrayList dia_btm;//輪郭・頂点(下側)
#endif
			public int		dia_cnt;//輪郭・頂点数
#if true//2018.11.02(HSVグラフ)
			public Point[]	his_top;//ヒストグラム算出範囲・頂点(上側)
			public Point[]	his_btm;//ヒストグラム算出範囲・頂点(下側)
			//---
			public double[] HIST_H_DM = new double[256];
			public double[] HIST_S_DM = new double[256];
			public double[] HIST_V_DM = new double[256];
			public double[] HIST_H_PD = new double[256];
			public double[] HIST_S_PD = new double[256];
			public double[] HIST_V_PD = new double[256];
			public double[] HIST_H_IR = new double[256];
			public double[] HIST_S_IR = new double[256];
			public double[] HIST_V_IR = new double[256];
			//public double[] HISTVALD = new double[256];
#endif
			//---
			public seg_of_hair() {
				this.cnt_of_val = 0;
				this.val_xum = new ArrayList();
				this.val_cen = new ArrayList();
#if false//2019.02.16(数値化白髪オフセット)
				this.val_p5u = new ArrayList();
				this.val_phf = new ArrayList();
				this.val_mph = new ArrayList();
				this.val_m5u = new ArrayList();
#endif
				//--
				this.cnt_of_pos = 0;
				//this.pts_x = null;
				this.pts_cen =  new ArrayList();
#if true//2019.03.22(再測定表)
				this.pts_cen_ofs = new List<Point>();
				this.val_cen_ofs = new List<object>();
#endif
#if false//2019.02.16(数値化白髪オフセット)
				this.pts_p5u =  new ArrayList();
				this.pts_phf =  new ArrayList();
				this.pts_mph =  new ArrayList();
				this.pts_m5u =  new ArrayList();
#endif
#if true//2018.10.30(キューティクル長)
				this.cut_inf = new List<seg_of_cuti>();
				this.cut_lsp = new List<List<Point>>();
				this.cut_len = new List<double>();
#endif
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
#if true//2018.10.10(毛髪径算出・改造)
				this.moz_inf = new List<seg_of_mouz>();
				//this.moz_fs2 = new List<bool>();//S1,S2区分,S1:true, S2:false
				//this.moz_avg = new List<double>();//毛髄:毛髄範囲の画素平均値(生画像による)
				this.moz_out = new List<bool>();
				this.moz_hpt = new List<Point>();//毛髄:上側点:補間後
				this.moz_hpb = new List<Point>();//毛髄:下側点:補間後
				this.moz_hpl = new List<double>();//毛髄:長さ径:補間後
#endif
#if true //2018.12.17(オーバーラップ範囲)
				this.ow_l_wid = -1;
				this.ow_r_wid = -1;
				this.ow_l_pos = -1;
				this.ow_r_pos = -1;
#if true//2019.01.09(保存機能修正)
				this.ow_l_xum = -99999;
				this.ow_r_xum = +99999;
#else
				this.ow_l_xum = -1;
				this.ow_r_xum = -1;
#endif
#endif
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
#if true//2018.10.10(毛髪径算出・改造)
					if (i2 >= hr) {
						break;
					}
#endif
				}
				if (i2 >= af.Length) {
					i2 = af.Length-1;
				}
				for (i0 = i1; i0 >= 0; i0--) {
					if (double.IsNaN(af[i0]) || af[i0] > G.SS.MOZ_CND_ZVAL) {
						i0++;
						break;
					}
#if true//2018.10.10(毛髪径算出・改造)
					if (i0 <= hl) {
						break;
					}
#endif
				}
				if (i0 < 0) {
					i0 = 0;
				}
				il = i0;
				ir = i2;
#if true//2018.10.10(毛髪径算出・改造)
				if (il == ir) {
					il = ir;
				}
#endif
			}
			return(true);
		}
#if true//2018.10.10(毛髪径算出・改造)
		void detect_area(seg_of_hair seg)
		{
			int len = seg.moz_zpl.Count;
			List<int> lbl = new List<int>();
			List<double> sum = Enumerable.Repeat<double>(0, len).ToList();
			int n_of_lbl = 0;
			double pl_bak = 0;
			Point pt_bak = (Point)seg.moz_zpt[0];
			Point pb_bak = (Point)seg.moz_zpb[0];

			//ラベリングによる領域分割

			for (int i = 0; i < len; i++) {
				Point pt = (Point)seg.moz_zpt[i];	//毛髄:上側点
				Point pb = (Point)seg.moz_zpb[i];	//毛髄:下側点
				double pl = (double)seg.moz_zpl[i];	//毛髄:長さ径
				int no;

				if (pl <= 0) {
					no = 0;			//毛髄無し
				}
				else if (pl_bak == 0) {
					no = ++n_of_lbl;//ラベル切り替え
				}
				else if (pt_bak.Y < pb.Y || pb_bak.Y > pt.Y) {
					no = ++n_of_lbl;//ラベル切り替え
				}
				else {
					no = n_of_lbl;	//ラベル継続
				}
				lbl.Add(no);
				pt_bak = pt;
				pb_bak = pb;
				pl_bak = pl;
			}

			//同一ラベルの積算値(面積)を算出
			for (int i = 0; i < len; i++) {
				if (lbl[i] == 0) {
					continue;//毛髄無し
				}
				double ttl = 0;
				int no = lbl[i];
				int h = i;
				for (; i < len; i++) {
					if (lbl[i] != no) {
						break;
					}
					ttl += (double)seg.moz_zpl[i];
				}
				sum[no-1] = ttl;
				if (G.SS.MOZ_CND_CHK1 && ttl <= G.SS.MOZ_CND_SMVL) {
					for (; h < i; h++) {
						lbl[h] = -no;
					}
				}
				i--;//一つ戻してから再開
			}
			seg.moz_lbl = lbl;
			seg.moz_sum = sum;
		}
		void detect_outliers(seg_of_hair seg)
		{
			int WID_OF_AVG;
			double THR_OF_VAL;
			double avg = 0, div = 0, std = 0, evl;
			int cnt;

			int len = seg.moz_zpl.Count;
			double[] buf;
			double[] buf_ddf;
			bool[] otl = Enumerable.Repeat<bool>(false, len).ToArray();

			for (int z = 0; z < 2; z++) {
				if (z == 0) {
					if (!G.SS.MOZ_CND_CHK2) {
						continue;//初期値falseのまま
					}
					// 毛髄径
					buf = new double[len];
					for (int i = 0; i < len; i++) {
						if (seg.moz_lbl[i] < 0) {
							buf[i] = 0;
						}
						else {
							buf[i] = (double)seg.moz_zpl[i];
						}
					}
					WID_OF_AVG = G.SS.MOZ_CND_OTW1;
					THR_OF_VAL = G.SS.MOZ_CND_OTV1;
				}
				else {
					if (!G.SS.MOZ_CND_CHK3) {
						continue;//初期値falseのまま
					}
					//毛髄センター
				//	buf = new double[len];
					buf_ddf = new double[len];
					for (int i = 0; i < len; i++) {
				//		Point u1 = (Point)seg.moz_zpt[i];//毛髄:上側点
				//		Point u2 = (Point)seg.moz_zpb[i];//毛髄:下側点
				//		buf[i] = (u1.Y+u2.Y)/2.0;
						if (seg.moz_lbl[i] < 0) {
							buf_ddf[i] = 0;
						}
						else {
							buf_ddf[i] = seg.moz_inf[i].ddf;
						}
					}
					WID_OF_AVG = G.SS.MOZ_CND_OTW2;
					THR_OF_VAL = G.SS.MOZ_CND_OTV2;
					if (true) {
						buf = buf_ddf;
					}
				}
				cnt = WID_OF_AVG;
				if (cnt > seg.moz_zpl.Count) {
					cnt = seg.moz_zpl.Count;
				}
				for (int i = 0; i < len; i++) {
					int h = i-cnt/2;
					if (h < 0) {
						h = 0;
					}
					if ((h + cnt) >= len) {
						h = len-cnt;
					}
					avg = div = 0;
					for (int j = 0; j < cnt; j++) {
						if (j == (cnt-1)) {
							j = j;
						}
						avg += buf[h+j];
					}
					avg /= cnt;
					//---
					for (int j = 0; j < cnt; j++) {
						div += Math.Pow(buf[h+j] - avg, 2);
					}
					div /= cnt;
					std = Math.Sqrt(div);
					//---
					//外れ度
					evl = Math.Abs((buf[i] - avg)/std);
					if (evl >= THR_OF_VAL) {
						otl[i] = true;
					}
				}
			}
			if (seg.moz_out != null) {
				seg.moz_out.Clear();
				seg.moz_out = null;
			}
			seg.moz_out = new List<bool>(otl);
		}
		int find_nearest_pnt(FN1D fn, Point[]pts)
		{
			double fmin;
			int imin;
			//---
			fmin = double.MaxValue;
			imin = 0;
			for (int k = 0; k < pts.Length; k++) {
				PointF ptm = pts[k];
				PointF ptf = new PointF();
				ptf.X = ptm.X;
				ptf.Y = (float)fn.GetYatX(ptf.X);
				double dif = G.diff(ptf, ptm);
				if (fmin > dif) {
					fmin = dif;
					imin = k;
				}
				else {
					fmin = fmin;
				}
			}
			return(imin);
		}
		int find_nearest_pnt(FNLAGRAN fn, Point[]pts)
		{
			double fmin;
			int imin;
			//---
			fmin = double.MaxValue;
			imin = 0;
			for (int k = 0; k < pts.Length; k++) {
				PointF ptm = pts[k];
				PointF ptf = new PointF();
				ptf.X = ptm.X;
				ptf.Y = (float)fn.GetYatX(ptf.X);
				double dif = G.diff(ptf, ptm);
				if (fmin > dif) {
					fmin = dif;
					imin = k;
				}
				else {
					fmin = fmin;
				}
			}
			return(imin);
		}
		void interp_outliers(seg_of_hair seg)
		{
			int len = seg.moz_zpl.Count;
			List<Point> rawtL = new List<Point>();
			List<Point> rawbL = new List<Point>();
			List<Point> rawtR = new List<Point>();
			List<Point> rawbR = new List<Point>();
			List<double>rawdL = new List<double>();
			List<double>rawdR = new List<double>();
			List<Point> hpt = new List<Point>();
			List<Point> hpb = new List<Point>();
			List<double> hpl = new List<double>();
			List<seg_of_mouz> hmz = new List<seg_of_mouz>();

			for (int i = 0; i < len; i++) {
				Point pt = (Point)seg.moz_zpt[i];
				Point pb = (Point)seg.moz_zpb[i];
				double pl = (double)seg.moz_zpl[i];
				double pl_bak = pl;
				int cnt = 0;
				int jcnt;
				int jmax;
				seg_of_mouz mz = seg.moz_inf[i];
				
				if (!seg.moz_out[i]) {
					if (seg.moz_lbl[i] < 0) {
						pt = mz.phc;
						pb = mz.phc;
						pl = 0;
						//---
						mz.iml = mz.ihc;
						mz.imr = mz.ihc;
						mz.imc = (mz.iml+mz.imr)/2;
						mz.pml = mz.pbf[mz.iml];
						mz.pmr = mz.pbf[mz.imr];
						mz.pmc = cen_of_pt(mz.pml, mz.pmr);
						mz.ddf = 0;
					}
					hpt.Add(pt);
					hpb.Add(pb);
					hpl.Add(pl);
					hmz.Add(mz);
					continue;
				}
				switch (G.SS.MOZ_CND_OTMD) {
					case  1:jmax = 1;break;//直線補間
					case  2:jmax = 2;break;//ラグランジュ補間
					case  0:
					default:jmax = 0;break;//補間しない
				}
				rawtL.Clear();
				rawbL.Clear();
				rawdL.Clear();
				rawtR.Clear();
				rawbR.Clear();
				rawdR.Clear();
				jcnt = 0;

				int max_of_dis = 100;
				int max_of_ddf = 100;
				bool flag = false;

				//手前からjmax点採集
				for (int j = i-1; j >= 0 && jcnt < jmax; j--) {
					if (false/*(i-j) >= max_of_dis*/) {
						break;//離れすぎたデータは補間元データとしない
					}
					if (seg.moz_inf[j].ddf == 0) {
						continue;//毛髄無しのデータは補間元データとしない
					}
					if (seg.moz_lbl[j] < 0) {
						continue;//除外データは補間元データとしない
					}
					if (!seg.moz_out[j]) {
						rawtL.Add((Point)seg.moz_zpt[j]);
						rawbL.Add((Point)seg.moz_zpb[j]);
						rawdL.Add(seg.moz_inf[j].ddf);
						cnt++;
						jcnt++;
					}
				}
				//後方からjmax点採集
				jcnt = 0;
				for (int j = i+1; j < len && jcnt < jmax; j++) {
					if (false/*(j-i) >= max_of_dis*/) {
						break;//離れすぎたデータは補間元データとしない
					}
					if (seg.moz_inf[j].ddf == 0) {
						continue;//毛髄無しのデータは補間元データとしない
					}
					if (seg.moz_lbl[j] < 0) {
						continue;//除外データは補間元データとしない
					}
					if (!seg.moz_out[j]) {
						rawtR.Add((Point)seg.moz_zpt[j]);
						rawbR.Add((Point)seg.moz_zpb[j]);
						rawdR.Add(seg.moz_inf[j].ddf);
						cnt++;
						jcnt++;
					}
				}
				if (rawtL.Count >= 2 && rawtR.Count >= 2) {
					//ラグランジュ補間を行う
					FNLAGRAN fn1 = new FNLAGRAN(rawtL[1], rawtL[0], rawtR[0], rawtR[1]);
					FNLAGRAN fn2 = new FNLAGRAN(rawbL[1], rawbL[0], rawbR[0], rawbR[1]);
					int imin;
					if (true) {
						imin = find_nearest_pnt(fn1, seg.moz_inf[i].pbf);
						mz.iml = imin;
						pt = mz.pbf[imin];
						//---
						imin = find_nearest_pnt(fn2, seg.moz_inf[i].pbf);
						mz.imr = imin;
						pb = mz.pbf[imin];
					}
					else {
						pt.Y = (int)(0.5+T.lagran(
							rawtL[1].X, rawtL[0].X, rawtR[0].X, rawtR[1].X,
							rawtL[1].Y, rawtL[0].Y, rawtR[0].Y, rawtR[1].Y,
							pt.X));
						//---
						pb.Y = (int)(0.5+T.lagran(
							rawbL[1].X, rawbL[0].X, rawbR[0].X, rawbR[1].X,
							rawbL[1].Y, rawbL[0].Y, rawbR[0].Y, rawbR[1].Y,
							pb.X));
					}
					pl = px2um(pt, pb);
					flag = true;
				}
				else if (rawtL.Count >= 1 && rawtR.Count >= 1) {
					if (false/*Math.Abs(rawdL[0]-rawdR[0]) > max_of_ddf*/) {
						flag = flag;//補間できない
					}
					else {
						//直線補間を行う
						FN1D fn1 = new FN1D(rawtL[0], rawtR[0]);
						FN1D fn2 = new FN1D(rawbL[0], rawbR[0]);
						int imin;
						//---
						if (true) {
							imin = find_nearest_pnt(fn1, seg.moz_inf[i].pbf);
							mz.iml = imin;
							pt = mz.pbf[imin];
							//---
							imin = find_nearest_pnt(fn2, seg.moz_inf[i].pbf);
							mz.imr = imin;
							pb = mz.pbf[imin];
						}
						pl = px2um(pt, pb);
						flag = true;

					}
				}
				if (flag == false) {
					//補間できない
					pt = seg.moz_inf[i].phc;
					pb = seg.moz_inf[i].phc;
					pl = 0;
				}
				if (pl_bak > 0 && pl <= 0) {
					pl = pl;
				}
				if (true) {
				//	mz.ibf = bf;
				//	mz.pbf = fp.ToArray();
				//	mz.phc = fp[ic];
				//	mz.ihc = ic;
				//	mz.iml = uil;
				//	mz.imr = uir;
					mz.imc = (mz.iml+mz.imr)/2;
					mz.pml = mz.pbf[mz.iml];
					mz.pmr = mz.pbf[mz.imr];
					mz.pmc = new PointF((mz.pml.X + mz.pmr.X)/2f, (mz.pml.Y+mz.pmr.Y)/2f);
					mz.ddf = px2um(mz.pmc, mz.phc);
					if (mz.pmc.Y > mz.phc.Y) {
						mz.ddf *= -1;
					}
				}
				hpt.Add(pt);
				hpb.Add(pb);
				hpl.Add(pl);
				hmz.Add(mz);
				if (mz.ddf == 0 && seg.moz_inf[i].ddf != 0) {
					i = i;
				}
				if (pl == 0 && TO_VAL(seg.moz_zpl[i]) != 0) {
					i = i;
				}
			}
			if (seg.moz_hpt != null) {
				seg.moz_hpt.Clear();
				seg.moz_hpt = null;
			}
			if (seg.moz_hpb != null) {
				seg.moz_hpb.Clear();
				seg.moz_hpb = null;
			}
			if (seg.moz_hpl != null) {
				seg.moz_hpl.Clear();
				seg.moz_hpl = null;
			}
			seg.moz_hpt = hpt;
			seg.moz_hpb = hpb;
			seg.moz_hpl = hpl;
			seg.moz_hnf = hmz;
			for (int i = 0; i < len; i++) {
				seg_of_mouz mouz = seg.moz_hnf[i];
				mouz.avg = get_avg(mouz.ibf, mouz.iml, mouz.imr-mouz.iml+1);
				mouz.fs2 =(mouz.avg >= G.SS.MOZ_CND_SLVL);
			}
		}
		void sum_avg(seg_of_hair seg)
		{
			seg.moz_rsl = 0;//毛髄面積:Sl
			seg.moz_rsd = 0;//毛髄面積:Sd
			seg.moz_hsl = 0;//毛髄面積:Sl(補間後)
			seg.moz_hsd = 0;//毛髄面積:Sd(補間後)
			for (int i = 0; i < seg.moz_hnf.Count; i++) {
				seg_of_mouz moui = seg.moz_inf[i];
				seg_of_mouz mouh = seg.moz_hnf[i];
				if (moui.fs2) {
					seg.moz_rsl += (double)seg.moz_zpl[i];
				}
				else {
					seg.moz_rsd += (double)seg.moz_zpl[i];
				}
				if (mouh.fs2) {
					seg.moz_hsl += (double)seg.moz_hpl[i];
				}
				else {
					seg.moz_hsd += (double)seg.moz_hpl[i];
				}
			}
		}
		// il, irの範囲をsmin,smaxで正規化する
		void normalize_array(double[]af, double smin=0, double smax=1, int il=0, int ir=0, bool bALL=false)
		{
			double fmax, fmin;

			fmax = double.MinValue;
			fmin = double.MaxValue;
			if (il == 0 && ir == 0) {
				ir = af.Length-1;
			}
			for (int i = il; i <= ir; i++) {
				if (fmin > af[i]) {
					fmin = af[i];
				}
				if (fmax < af[i]) {
					fmax = af[i];
				}
			}
			//コントラスト最大化(0-255範囲にマッピング)
			FN1D fn = new FN1D(new PointF((float)fmin, (float)smin), new PointF((float)fmax, (float)smax));
			if (bALL) {
				for (int i = 0; i < af.Length; i++) {
					af[i] = fn.GetYatX(af[i]);
				}
			}
			else {
				for (int i = il; i <= ir; i++) {
					af[i] = fn.GetYatX(af[i]);
				}
			}
		}
		// il, irの範囲を面積でバランス
		void normalize_balance_s(double[]af, double smin=0, double smax=1, int il=0, int ir=0, bool bALL=false)
		{
			double fmax, fmin, S1 = 0, S2 = 0, R, A,L2;
			int ic, len;
			FN1D fn;
			double[]bf = new double[af.Length];

			fmax = double.MinValue;
			fmin = double.MaxValue;
			if (il == 0 && ir == 0) {
				ir = af.Length-1;
			}

			ic= (il+ir)/2;
			len = (ir-il+1);
			L2 = len/2.0;
			for (int i = il; i <= ic; i++) {
				S1 += af[i];
			}
			for (int i = ic; i <= ir; i++) {
				S2 += af[i];
			}
			R = (S1/S2);
			A = (3-R)/(1+R);
			fn = new FN1D((1-A)/L2, 1);
			if (fn.GetYatX(-L2) < 0 || fn.GetYatX(+L2) < 0) {
				A = A;
			}
			for (int i = il; i <= ir; i++) {
				bf[i] = af[i] * fn.GetYatX(i-ic);
				if (fmin > bf[i]) {
					fmin = bf[i];
				}
				if (fmax < bf[i]) {
					fmax = bf[i];
				}
			}
			//コントラスト最大化(0-255範囲にマッピング)
			fn = new FN1D(new PointF((float)fmin, (float)smin), new PointF((float)fmax, (float)smax));
			if (bALL) {
				for (int i = 0; i < af.Length; i++) {
					af[i] = fn.GetYatX(bf[i]);
				}
			}
			else {
				for (int i = il; i <= ir; i++) {
					af[i] = fn.GetYatX(bf[i]);
				}
			}
		}
		// il, irの範囲を面積でバランス
		void normalize_balance_m(double[]af, double smin=0, double smax=1, int il=0, int ir=0, bool bALL=false)
		{
			double M1, M2, R, A, L1, L2;
			int	I1=0, I2=0;
			int ic, len;
			double fmin, fmax;
			FN1D fn;
			double[]bf = new double[af.Length];

			M1 = double.MinValue;
			M2 = double.MinValue;
			if (il == 0 && ir == 0) {
				ir = af.Length-1;
			}

			ic= (il+ir)/2;
			len = (ir-il+1);
			for (int i = il; i < ic; i++) {
				if (M1 < af[i]) {
					M1 = af[i];
					I1 = i;
				}
			}
			for (int i = ic+1; i <= ir; i++) {
				if (M2 < af[i]) {
					M2 = af[i];
					I2 = i;
				}
			}
			L1 = ic-I1;
			L2 = I2-ic;
			R = (M2-M1)/(M1*(-L1)-M2*L2);
			A = R*(L1+L2)/(1.0+L2/L1);
			fn = new FN1D(A*(1.0+L2/L1)/(L1+L2), 1);
			if (fn.GetYatX(-L2) < 0 || fn.GetYatX(+L2) < 0) {
//				A = A;
			}
			double test3 = fn.GetYatX(-L1) * M1;
			double test4 = fn.GetYatX(+L2) * M2;

			fmin = double.MaxValue;
			fmax = double.MinValue;
			for (int i = il; i <= ir; i++) {
				if (i == I1 || i == I2) {
					i = i;
				}
				bf[i] = af[i] * fn.GetYatX(i-ic);
				if (fmin > bf[i]) {
					fmin = bf[i];
				}
				if (fmax < bf[i]) {
					fmax = bf[i];
				}
			}
			//コントラスト最大化(0-255範囲にマッピング)
			fn = new FN1D(new PointF((float)fmin, (float)smin), new PointF((float)fmax, (float)smax));
			double test1 = fn.GetYatX(fmin);
			double test2 = fn.GetYatX(fmax);
			if (bALL) {
				for (int i = 0; i < af.Length; i++) {
					af[i] = fn.GetYatX(bf[i]);
				}
			}
			else {
				for (int i = il; i <= ir; i++) {
					af[i] = fn.GetYatX(bf[i]);
				}
			}
		}
		PointF cen_of_pt(Point p1, Point p2)
		{
			PointF	pt = new PointF((p1.X+p2.X)/2f, (p1.Y+p2.Y)/2f);
			return(pt);
		}
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
		double[] TO_DBL_ARY(List<object> objs)
		{
			List<double> ls = new List<double>();
			for (int i = 0; i < objs.Count; i++) {
				double f;
				if (objs[i] == null) {
					f = double.NaN;
				}
				else {
					f = ((Color)objs[i]).G;
				}
				ls.Add(f);
			}
			return(ls.ToArray());
		}
		double get_avg(double[]ar, int idx, int len)
		{
			double avg = 0;
			int h = idx;
			if (ar == null || idx < 0 || (idx+len) >= ar.Length) {
				return(double.NaN);
			}
			for (int i = 0; i < len; i++, h++) {
				avg += ar[h];
			}
			return(avg/len);
		}
		//ArrayList m_pt_zpl = new ArrayList();//毛髄:長さ径
		//---
		// p3:毛髪上端の点
		// p2:毛髪下端の点
		//---
		void test_ir(seg_of_hair seg, FN1D f2, PointF p2, PointF p3, int sx)
		{
			Point tx = Point.Truncate(p2);
			Point t3 = Point.Truncate(p3);
			PointF fx = p2;
			List<Point> fp = new List<Point>();
			List<object> fc = new List<object>();
			double df = G.diff(p2, p3);
			double[] af;
			double[] bf;
			int	ic, ll,lr,ir_of_all, il_of_all;
			int  rcnt = 0;

			List<Point > upl_buf = new List<Point>();
			List<Point > upr_buf = new List<Point>();
			List<int   > uil_buf = new List<int>();
			List<int   > uir_buf = new List<int>();
			List<double> udf_buf = new List<double>();
#if true//2018.11.22(数値化エラー対応)
			if (string.Compare(seg.name_of_dm, "2CR_03_ZDEPT.PNG") == 0) {
			if (sx == 32) {
				sx = sx;//for bp
			}
			}
#else
			if (sx == 92) {
				sx = sx;//for bp
			}
#endif
			for (int i = 1;; i++) {
				//毛髪下端から上端に向かって走査する
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
						break;//毛髪上端に達した
					}
					tx = t0;
				}
			}

			af = TO_DBL_ARY(fc);
			bf = (double[])af.Clone();
			if (true) {
				ic = af.Length/2;
				ll = (af.Length *  G.SS.MOZ_CND_HANI)/100/2;
				lr = ll;
				if ((ic+lr) >= af.Length) {
					lr--;
				}
				if ((ic-ll) < 0) {
					ll--;
				}
				il_of_all = ic-ll;
				ir_of_all = ic+lr;
			}
			if (this.MOZ_CND_SMCF > 0) {
				double fmax, fmin;
				T.SG_POL_SMOOTH(af, af, af.Length, this.MOZ_CND_SMCF, out fmax, out fmin);
			}
			if (true) {
				const
				double WID_OF_BALANCE = 0.98;//エッジ部分を含めないようにするため...
				int wg = (int)(af.Length * (1.0-WID_OF_BALANCE));
				int wl = wg/2;
#if true//2018.11.22(数値化エラー対応)
				int wr = af.Length-1-wg/2;
#else
				int wr = af.Length-wg/2;
#endif
				//---
				switch (G.SS.MOZ_CND_CNTR) {
					case 1://上下端全範囲=A
						normalize_array(af, 0.0, 255.0, 0, af.Length-1, false);
					break;
					case 2://毛髄判定範囲=B
						normalize_array(af, 0.0, 255.0, ic-ll, ic+lr, false);
						//normalize_array(af, 0.0, 255.0, ic-ll, ic+ 0, false);
						//normalize_array(af, 0.0, 255.0, ic+ 1, ic+lr, false);
					break;
					case 3://上下で等面積(A)
						normalize_balance_s(af, 0.0, 255.0, wl, wr, false);
					break;
					case 4://上下で等面積(B)
						normalize_balance_s(af, 0.0, 255.0, ic-ll, ic+lr, false);
					break;
					case 5://上下で等最大値(A)
						normalize_balance_m(af, 0.0, 255.0, wl, wr, false);
					break;
					case 6://上下で等最大値(B)
						normalize_balance_m(af, 0.0, 255.0, ic-ll, ic+lr, false);
					break;
					case 0://コントラスト補正無し
					default:
					break;
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
#if true//2018.10.10(毛髪径算出・改造)
					for (int i = ic-ll; i <= ic+lr; i++) {
						wr.Write(string.Format("{0:F0}", af[i]));
						if (i < (fc.Count-1)) {
							wr.Write(",");
						}
					}
#else
					for (int i = 0; i < fc.Count; i++) {
						wr.Write(string.Format("{0:F0}", af[i]));
						if (i < (fc.Count-1)) {
							wr.Write(",");
						}
					}
#endif
					wr.WriteLine("");
					wr.Close();
				}
				catch (Exception ex) {
				}
			}

			Point upl;
			Point upr;
			int	uil, uir, sil, sir;
			double uml;
#if true//2018.11.06(毛髄4)
			int	ihl, ihr;
#endif

			sil = (ic-ll);
			sir = (ic+lr);
#if true//2018.11.06(毛髄4)
			ihl = sil;
			ihr = sir;
#endif
retry:
			if (sil != (ic-ll)) {
				sil  = (ic-ll);
			}
			if (sir != (ic+lr)) {
				sir  = (ic+lr);
			}

			if (true) {
				bool rc;
				bool flag;

				rc = select_zval_hani(af, sil, sir, G.SS.MOZ_CND_ZVAL, out uil, out uir);
				flag = (uil == il_of_all) || (uir == ir_of_all);//判定範囲に達した？

				if (!rc) {
					uil = uir = ic;
					upl = fp[ic];
					upr = fp[ic];
					uml = 0;
				}
				else {
					int gap = (int)(af.Length *0.05);
					upl = fp[uil];
					upr = fp[uir];
					//uml = Math.Sqrt(Math.Pow(upr.X - upl.X, 2) + Math.Pow(upr.Y - upl.Y, 2));
					uml = G.diff(fp[uir], fp[uil]);
					//G.mlog("kakunin");
					
					if (flag == false && uil  <= (ic) && uir >= (ic)) {
						uil = uil;//選択された毛髄範囲は毛髪センターを含んでいる
					}
					//else if (il  <= (ic+gap) && ir >= (ic-gap)) {
					//    il = il;//選択された毛髄範囲は毛髪センター10%域を含んでいる
					//}
					else {
						//...含んでいない
						if (uil > ic) {
							// ic-ll ~ ic ~ ic+lr
							lr = uil-ic-1;//次の探索範囲
							//--
							sir = uil-1;
						}
						else {
							ll = ic-uir-1;//次の探索範囲
							//--
							sil = uir+1;
						}
						if (ll < 0 || lr < 0) {
							ll = ll;//選択範囲無し
						}
						else if (rcnt < 10) {
							if (flag) {
								//選択範囲は格納しない
								flag = flag;
							}
							else {
								//選択範囲を格納
								upl_buf.Add(upl);
								upr_buf.Add(upr);
								uil_buf.Add(uil);
								uir_buf.Add(uir);

								if (true) {
									PointF uc = cen_of_pt(upl, upr);
									udf_buf.Add(G.diff(uc, fp[ic]));
								}
								else {
									Point uc = new Point((upl.X + upr.X/2), (upl.Y+upr.Y)/2);
									udf_buf.Add(G.diff(uc, fp[ic]));
								}
							}
							//選択範囲を除外してリトライする
							rcnt++;
							goto retry;
						}
						uml = 0;
					}
				}
			}
			if (uml == 0.0) {
				if (udf_buf.Count > 0) {
					//中心ラインに一番近い範囲域を選択
					int i;
					double fmin = double.MaxValue;
					int imin = 0;
					if (udf_buf.Count > 4) {
						i = 0;
					}
					for (i = 0; i < udf_buf.Count; i++) {
						if (fmin > udf_buf[i]) {
							fmin = udf_buf[i];
							imin = i;
						}
					}
					uil = uil_buf[imin];
					uir = uir_buf[imin];
					upl = upl_buf[imin];
					upr = upr_buf[imin];
				}
				else {
					uil = ic;
					uir = ic;
					upl = fp[ic];
					upr = fp[ic];
				}
			}
			//uml = Math.Sqrt(Math.Pow(upr.X - upl.X, 2) + Math.Pow(upr.Y - upl.Y, 2));
			//uml = G.PX2UM(uml, m_log_info.pix_pitch, m_log_info.zoom);
			uml = px2um(upr, upl);
			//if ((++m_chk1 % 20) == 0) {
			//    u1 = Point.Round(p2);
			//    u2 = Point.Round(p3);
			//}
			if (uml >= 25) {
				uml = uml;
			}
			if (uml <= 0.0) {
				uml = uml;
			}
			seg.moz_zpt.Add(upl);//毛髄:上側点
			seg.moz_zpb.Add(upr);//毛髄:下側点
			seg.moz_zpl.Add(uml);//毛髄:長さ径
			//G.mlog("上の行あとで確認すること");
			//---
			seg.moz_top.Add(Point.Round(p2));
			seg.moz_btm.Add(Point.Round(p3));
			seg_of_mouz mouz;
			if (true) {
				mouz.ibf = bf;
#if true//2018.11.28(メモリリーク)
				mouz.iaf = null;
#else
				mouz.iaf = af;
#endif
				mouz.pbf = fp.ToArray();
				mouz.phc = fp[ic];
				mouz.ihc = ic;
				mouz.iml = uil;
				mouz.imr = uir;
				mouz.imc = (uil+uir)/2;
				mouz.pml = upl;
				mouz.pmr = upr;
				mouz.pmc = new PointF((upl.X + upr.X)/2f, (upl.Y+upr.Y)/2f);
				mouz.ddf = px2um(mouz.pmc, mouz.phc);
				if (mouz.pmc.Y > mouz.phc.Y) {
					mouz.ddf *= -1;
				}
				//---
				if (true) {
				mouz.avg = get_avg(bf, uil, uir-uil+1);
				mouz.fs2 =(mouz.avg >= G.SS.MOZ_CND_SLVL);
				}
#if true//2018.11.06(毛髄4)
				mouz.ihl = ihl;
				mouz.ihr = ihr;
#endif
				//---
				seg.moz_inf.Add(mouz);
			}
		}
#else
		void test_ir(seg_of_hair seg, FN1D f2, PointF p2, PointF p3, int sx)
		{
			Point tx = Point.Truncate(p2);
			Point t3 = Point.Truncate(p3);
			PointF fx = p2;
			ArrayList fp = new ArrayList();
			ArrayList fc = new ArrayList();
			double df = G.diff(p2, p3);
			for (int i = 1;; i++) {
				//毛髪下端から上端に向かって走査する
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
						break;//毛髪上端に達した
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
#if true//2018.10.10(毛髪径算出・改造)
			int	ic, l5;
#endif
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
#if true//2018.10.10(毛髪径算出・改造)
				ic = af.Length/2;
				l5 = af.Length/5;
				if (true) {
					l5 = (af.Length *  G.SS.MOZ_CND_HANI)/100/2;
					if ((ic+l5) >= af.Length) {
						l5--;
					}
					if ((ic-l5) < 0) {
						l5--;
					}
				}
#endif
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
#if true//2018.10.10(毛髪径算出・改造)
				if (G.SS.MOZ_CND_CTRA) {
					if (false) {
						//全範囲で
						normalize_array(af, 0.0, 255.0, 0, af.Length-1, false);
					}
					else if (false) {
						//判定範囲で
						normalize_array(af, 0.0, 255.0, ic-l5, ic+l5, false);
					}
					else {
						//判定範囲の上側、下側で分けて
						normalize_array(af, 0.0, 255.0, ic-l5, ic+ 0, false);
						normalize_array(af, 0.0, 255.0, ic+ 1, ic+l5, false);
					}
				}
#else
				if (G.SS.MOZ_CND_CTRA) {
					//コントラスト最大化(0-255範囲にマッピング)
					FN1D fn = new FN1D(new PointF((float)fmin, 0f), new PointF((float)fmax, 255f));
					for (int i = 0; i < af.Length; i++) {
						af[i] = fn.GetYatX(af[i]);
					}
				}
#endif
			}
			if (true) {
				try {
					StreamWriter wr;
					string path = System.IO.Path.GetFileNameWithoutExtension(seg.name_of_ir);
					path += ".csv";
					path = "c:\\temp\\ir_" + path;
					wr = new StreamWriter(path, true, Encoding.Default);
					wr.Write(string.Format("{0},", sx));
#if true//2018.10.10(毛髪径算出・改造)
					for (int i = ic-l5; i <= ic+l5; i++) {
						wr.Write(string.Format("{0:F0}", af[i]));
						if (i < (fc.Count-1)) {
							wr.Write(",");
						}
					}
#else
					for (int i = 0; i < fc.Count; i++) {
						wr.Write(string.Format("{0:F0}", af[i]));
						if (i < (fc.Count-1)) {
							wr.Write(",");
						}
					}
#endif
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
#if false//2018.10.10(毛髪径算出・改造)
			int	ic = af.Length/2;
			int l5 = af.Length/5;
#endif
			double vmin = 255;
			int imin = 0;
#if false//2018.10.10(毛髪径算出・改造)
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
#endif
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
#if false//2018.10.10(毛髪径算出・改造)
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
#else
				int il, ir;
				bool rc;
				rc = select_zval_hani(af, (ic-l5), (ic+l5), G.SS.MOZ_CND_ZVAL, out il, out ir);
				i0 = il;
				i2 = ir;
				if (!rc) {
					u1 = (Point)fp[ic];
					u2 = (Point)fp[ic];
					l1 = 0;
				}
				else
#endif
				if (i0 < (ic-l5) || i2 > (ic+l5)) {
					//判定された毛髄範囲が範囲外まで連続しているときは
					//毛髄では無いと判定する
					u1 = (Point)fp[ic];
					u2 = (Point)fp[ic];
					l1 = 0;
					if (i0 < (ic-l5) && i2 > (ic+l5)) {
						//両側でNG
						rc = rc;
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
#endif
		//private double m_offset_of_hair;
		private double m_back_of_x;
		//---
#if true//2018.10.30(キューティクル長)
		private void test_dm(seg_of_hair[] segs, int idx, int cnt, bool bRECALCIR=false)
		{
#if true//2019.03.22(再測定表)
			seg_of_hair seg = segs[idx];
			double dia_ofs = 0;
			if (seg.name_of_dm.Contains("CT_")) {
				double dia = seg.dia_avg;
				if (G.SS.MOZ_BOK_SOFS[0] != 0) {
					dia_ofs = (dia * (G.SS.MOZ_BOK_SOFS[0]/100.0));
					dia_ofs = G.UM2PX(dia_ofs, m_log_info.pix_pitch, m_log_info.zoom);
				}
			}
#endif
#if true//2018.10.10(毛髪径算出・改造)
			if (bRECALCIR) {
				m_dia_top = segs[idx].dia_top;
				m_dia_btm = segs[idx].dia_btm;
				m_dia_cnt = segs[idx].dia_cnt;
				//---
				G.IR.PLY_XMIN = segs[idx].IR_PLY_XMIN;
				G.IR.PLY_XMAX = segs[idx].IR_PLY_XMAX;
				G.IR.WIDTH    = segs[idx].IR_WIDTH;
				//---
				segs[idx].moz_zpt.Clear();
				segs[idx].moz_zpb.Clear();
				segs[idx].moz_zpl.Clear();
				segs[idx].moz_top.Clear();
				segs[idx].moz_btm.Clear();
				segs[idx].moz_inf.Clear();
				//segs[idx].moz_fs2.Clear();//S1,S2区分,S1:true, S2:false
				//segs[idx].moz_avg.Clear();//毛髄:毛髄範囲の画素平均値(生画像による)
				segs[idx].moz_out.Clear();
				segs[idx].moz_hpt.Clear();//毛髄:上側点:補間後
				segs[idx].moz_hpb.Clear();//毛髄:下側点:補間後
				segs[idx].moz_hpl.Clear();//毛髄:長さ径:補間後
			}
			double RT = (100-G.SS.MOZ_CND_HANI)/100.0/2.0;
			double RB = (1-RT);
			List<Point> at = new List<Point>();
			List<Point> ab = new List<Point>();
			for (int i = 0; i < m_dia_cnt; i++) {
				Point pt = m_dia_top[i];
				Point pb = m_dia_btm[i];
				Point ht = new Point();
				Point hb = new Point();
				if (i == 0) {
					FN1D ft = new FN1D(m_dia_top[i], m_dia_top[i+1]);
					FN1D fb = new FN1D(m_dia_btm[i], m_dia_btm[i+1]);
					pt.X = 0;
					pt.Y = (int)ft.GetYatX(0.0);
					pb.X = 0;
					pb.Y = (int)fb.GetYatX(0.0);
				}
				else if (i == (m_dia_cnt-1)) {
					FN1D ft = new FN1D(m_dia_top[i-1], m_dia_top[i]);
					FN1D fb = new FN1D(m_dia_btm[i-1], m_dia_btm[i]);
					pt.X = G.IR.PLY_XMAX;
					pb.X = G.IR.PLY_XMAX;
					pt.Y = (int)ft.GetYatX(pt.X);
					pb.Y = (int)fb.GetYatX(pb.X);
				}
				ht.X = (int)(pt.X + RT * (pb.X - pt.X));
				ht.Y = (int)(pt.Y + RT * (pb.Y - pt.Y));
				hb.X = (int)(pt.X + RB * (pb.X - pt.X));
				hb.Y = (int)(pt.Y + RB * (pb.Y - pt.Y));
				at.Add(ht);
				ab.Add(hb);
			}
			segs[idx].han_top = at.ToArray();
			segs[idx].han_btm = ab.ToArray();
#endif
			//(1)中心のラインを求める(両端は画像端まで拡張する)
			//(2)中心ラインに沿って左端から右端まで一定間隔で走査点を進める
			//(3)走査点で垂直方向に上下両側に延ばした時の輪郭線との交点を求める
			//(4)輪郭線との交点と走査点から断面点を求める
			//(5)断面点の画素値を格納する
			//Form02.TO_RR(0.5,  G.IR.DIA_TOP[i],  G.IR.DIA_BTM[i], out top[i], out btm[i]);
			//---(1)
#if false//2019.03.22(再測定表)
			seg_of_hair seg = segs[idx];
#endif
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
				m_ft[i] = new FN1D(pt0, pt1);//毛髪上端の分割エッジ直線
				m_fb[i] = new FN1D(pb0, pb1);//毛髪下端の分割エッジ直線
				m_fc[i] = new FN1D(pc0, pc1);//毛髪中心の分割エッジ直線
			}
			//---(2)
			double	px0 = (idx <= 0) ? 0: segs[idx-1].pix_pos.X;
			double	px1 = seg.pix_pos.X;
			double	dif = (px1-px0);
			int		i0 = 0;
			//double ds = 5;//5dot = 1.375um
			double	ds = G.UM2PX(G.SS.MOZ_CND_DSUM, m_log_info.pix_pitch, m_log_info.zoom);//横方向走査単位[pix]
#if false//2019.01.11(混在対応)
			double	u5 = G.UM2PX(G.SS.MOZ_CND_CUTE, m_log_info.pix_pitch, m_log_info.zoom);//径方向走査単位[pix]
#endif
			PointF	pf;// = (PointF)ac[0];
			//double xend = ((PointF)ac[ac.Count-1]).X;
			double	xmin = (G.IR.PLY_XMIN < C.GAP_OF_IMG_EDGE) ? 0 :G.IR.PLY_XMIN;
			double	xmax = ((G.IR.WIDTH - G.IR.PLY_XMAX) < C.GAP_OF_IMG_EDGE) ? (G.IR.WIDTH-1) : G.IR.PLY_XMAX;
			PointF	sta_of_pf = new PointF();
			int		ii = 0, ss = 0, s = 0;
			int	LMAX = 30;
			if (true) {
				double r = seg.dia_avg/2;/*半径[um]*/
				LMAX = (int)(r * G.SS.MOZ_CND_CHAN/100.0);
				seg.cut_inf.Clear();
			}
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
#if true //2018.12.17(オーバーラップ範囲)
			if (true) {
				//seg.ow_l_wid = seg.ow_r_wid = 0;
			}
			if (idx < (segs.Length-1)) {
				//右重なり有り
				int q1 = idx+0;
				int	q2 = idx+1;
				double right_of_curr_img = segs[q1].pix_pos.X + segs[q1].width-1;
				double left_of_next_img  = segs[q2].pix_pos.X;
				double	wid = right_of_curr_img - left_of_next_img;
#if true//2018.12.22(測定抜け対応)
				wid/=2;
#endif
				segs[q1].ow_r_wid = (int)wid;
				segs[q1].ow_r_pos =-(int)wid+segs[q1].width;
				segs[q2].ow_l_wid = (int)wid;
				segs[q2].ow_l_pos = (int)wid;
			}
			if (idx > 0) {
				//左重なり無し
				int q0 = idx-1;
				int q1 = idx-0;
				double right_of_prev_img = segs[q0].pix_pos.X + segs[q0].width-1;
				double left_of_curr_img  = segs[q1].pix_pos.X;
				double	wid = right_of_prev_img - left_of_curr_img;
#if true//2018.12.22(測定抜け対応)
				wid/=2;
#endif
				segs[q0].ow_r_wid = (int)wid;
				segs[q0].ow_r_pos =-(int)wid+segs[q0].width;
				segs[q1].ow_l_wid = (int)wid;
				segs[q1].ow_l_pos = (int)wid;
			}
#endif
			pf =sta_of_pf;
			//m_back_of_x = pf.X;
			//を
			//現在の画像のＸ値に変換
			//seg.total_idx = m_offset_of_hair;
			//
#if true//2018.11.28(メモリリーク)
			if (true) {
				//GC.Collect();
			}
#endif
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
				//(3) p2:上端, p3:下端
				FN1D f1 = m_fc[ii];			//現在X位置に対応する中心ラインの直線
				FN1D f2 = f1.GetNormFn(pf);	//F1に直交する直線(→径方向)
				PointF p2 = new PointF(), p3 = new PointF(), pt;
				Point p5
#if false//2019.01.11(混在対応)
				, p6, p7, p8, p9;
#else
				;
#endif
				//Color cl;

				//径方向直線と上端ラインの直線の交点を求める
				for (int i = 0; i < m_ft.Length; i++) {
					p2 = f2.GetCrossPt(m_ft[i]);
					if (p2.X < m_dia_top[i+1].X) {
						break;
					}
				}
				//径方向直線と下端ラインの直線の交点を求める
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
#if false//2019.01.11(混在対応)
				pt = new PointF((pf.X + p2.X)/2,  (pf.Y + p2.Y)/2);
				p6 = Point.Round(pt);
				pt = new PointF((pf.X + p3.X)/2,  (pf.Y + p3.Y)/2);
				p7 = Point.Round(pt);
				pt = f2.GetScanPt2Ext(pf, p2, u5);
				p8 = Point.Round(pt);
				pt = f2.GetScanPt2Ext(pf, p3, u5);
				p9 = Point.Round(pt);
#endif
				if (bRECALCIR == false) {
					//(5)
					//格納
					seg.val_cen.Add(TO_CL(p5));
					seg.val_xum.Add(Math.Round(G.PX2UM(s*ds, m_log_info.pix_pitch, m_log_info.zoom), 2));
					//---
					seg.mou_len.Add(Math.Round(G.PX2UM(G.diff(p2,  p3), m_log_info.pix_pitch, m_log_info.zoom), 1));
					//---
					seg.pts_cen.Add(p5);
#if true//2019.03.22(再測定表)
					if (true) {
						PointF p5_ofs = p5;
						p5_ofs.Y -= (float)dia_ofs;
						seg.pts_cen_ofs.Add(Point.Round(p5_ofs));
						seg.val_cen_ofs.Add(TO_CL(Point.Round(p5_ofs)));
					}
#endif
#if false//2018.11.28(メモリリーク)
					seg.pts_phf.Add(p6);
					seg.pts_mph.Add(p7);
					seg.pts_p5u.Add(p8);
					seg.pts_m5u.Add(p9);
#endif
				}
#if true//2018.11.28(メモリリーク)
				//GC.Collect();
				if (false) {
				} else
#endif
				if (true) {
					PointF p0 = pf;//径方向:中心
					PointF pl;
#if true//2019.02.16(数値化白髪オフセット)
					bool flag = false;
					PointF p0_bak = p0;
					PointF p2_bak = p2;
					PointF p3_bak = p3;
					if (seg.name_of_dm.Contains("CT_")) {
						double ofs
#if true//2019.03.22(再測定表)
							= 0;
#endif
							;
						double dia = seg.dia_avg;
						if (G.SS.IMP_AUT_SOFS[0] != 0) {
							ofs = (dia * (G.SS.IMP_AUT_SOFS[0]/100.0));
							ofs = G.UM2PX(ofs, m_log_info.pix_pitch, m_log_info.zoom);

							flag = true;
							p0.Y -= (float)ofs;
							p2.Y -= (float)ofs;
							p3.Y -= (float)ofs;

						}
#if true//2019.03.22(再測定表)
						if (ofs != dia_ofs) {
							ofs = ofs;
						}
#endif
					}
#endif
					double dl = 5;
					//int		ic;
					List<double> ibf = new List<double>();
					List<Point>  ptf = new List<Point>();

					pl = p0;
					//センター含めて上半分
					for (int i = 0; i <= LMAX; i++) {
						Point tmp = Point.Round(pl);
						ibf.Add(TO_VAL(tmp));
						ptf.Add(tmp);
						pl = f2.GetScanPt3Ext(p0, p2, pl, dl);
					}
					ibf.Reverse();
					ptf.Reverse();
					//ic = ibf.Count-1;
					pl = f2.GetScanPt3Ext(p0, p3, p0, dl);
					//センター含めず下半分
					for (int i = 0; i < LMAX; i++) {
						Point tmp = Point.Round(pl);
						ibf.Add(TO_VAL(tmp));
						ptf.Add(tmp);
						pl = f2.GetScanPt3Ext(p0, p3, pl, dl);
					}
					//
					if (seg.cut_inf.Count <= 0) {
						for (int i = 0; i < (LMAX*2+1); i++) {
							seg_of_cuti cut = new seg_of_cuti();
#if false//2018.12.10(64ビット化)
							for (int u = 0; u < cut.tst.Capacity; u++) {
								cut.tst.Add((byte)u);
							}
#endif
							seg.cut_inf.Add(cut);
						}
#if true//2018.11.28(メモリリーク)
						//seg.cut_inf.TrimExcess();
#endif
					}
					for (int i = 0; i < (LMAX*2+1); i++) {
						seg.cut_inf[i].pbf.Add(ptf[i]);
						seg.cut_inf[i].ibf.Add(ibf[i]);
					}
#if true//2019.02.16(数値化白髪オフセット)
					if (flag) {
						p0 = p0_bak;
						p2 = p2_bak;
						p3 = p3_bak;
					}
#endif
				}
#if true//2018.11.28(メモリリーク)
				//GC.Collect();
#endif
				//(6) IR画像より毛髄径検出
				if (m_bmp_ir1 != null) {
					test_ir(seg, f2, p2, p3, s);
					seg.cnt_of_moz = 1;
				}
#if true//2018.11.28(メモリリーク)
				//GC.Collect();
#endif
				// pf(中心ラインの直線式上)をdsだけ進める
				pf = scan_pt(m_fc, ref ii, pf, ds);
			}
			if (true) {
				int ic = seg.cut_inf.Count/2;
				for (int i = 0; i < seg.val_cen.Count; i++) {
					double ff = TO_VAL(seg.val_cen[i]);
					Point pp = (Point)seg.pts_cen[i];
					if (seg.cut_inf[ic].ibf[i] != ff) {
						ff = ff;
					}
					if (seg.cut_inf[ic].pbf[i] != pp) {
						ff = ff;
					}
				}
			}
#if true //2018.12.17(オーバーラップ範囲)
			if (seg.ow_l_pos >= 0) {
				for (int i = 0; i < (seg.pts_cen.Count-1); i++) {
					Point p0 = (Point)seg.pts_cen[i];
					Point p1 = (Point)seg.pts_cen[i+1];
					if (seg.ow_l_pos >= p0.X && seg.ow_l_pos < p1.X) {
						seg.ow_l_xum = (double)seg.val_xum[i];
						break;
					}
				}
			}
			if (seg.ow_r_pos >= 0) {
				for (int i = seg.pts_cen.Count-1; i > 0; i--) {
					Point p0 = (Point)seg.pts_cen[i-1];
					Point p1 = (Point)seg.pts_cen[i];
					if (seg.ow_r_pos >= p0.X && seg.ow_r_pos < p1.X) {
						seg.ow_r_xum = (double)seg.val_xum[i];
						break;
					}
				}
			}
#endif
#if true//2018.11.28(メモリリーク)
			//GC.Collect();
#endif
			detect_area(seg);
			detect_outliers(seg);
			interp_outliers(seg);
			sum_avg(seg);
			if (bRECALCIR == false) {
				//キューティクル断面のフィルター処理
#if true//2019.03.22(再測定表)
				apply_filter(seg.val_cen_ofs, out seg.val_cen_fil);
				find_cuticle_line(seg.pts_cen_ofs, seg.val_cen_fil, out seg.pts_cen_cut, out seg.flg_cen_cut, out seg.his_cen_cut);
#else
				apply_filter(seg.val_cen, out seg.val_cen_fil);
				find_cuticle_line(seg.pts_cen, seg.val_cen_fil, out seg.pts_cen_cut, out seg.flg_cen_cut, out seg.his_cen_cut);
#endif
			}
#if true//2018.11.28(メモリリーク)
			//GC.Collect();
#endif
			for (int i = 0; i < (LMAX*2+1); i++) {
#if true//2018.11.28(メモリリーク)
				seg.cut_inf[i].ibf.TrimExcess();
#endif
				apply_filter     (seg.cut_inf[i].ibf, out seg.cut_inf[i].iaf);
#if true//2018.11.28(メモリリーク)
				seg.cut_inf[i].iaf.TrimExcess();
#endif
				find_cuticle_line(seg.cut_inf[i].pbf, seg.cut_inf[i].iaf, out seg.cut_inf[i].pct, out seg.cut_inf[i].flg, out seg.cut_inf[i].his);
				//フラグに対して接続を探索
				//    接続をライン化け？
#if true//2018.11.28(メモリリーク)
				seg.cut_inf[i].pbf.TrimExcess();
				//seg.cut_inf[i].pct.TrimExcess();
				seg.cut_inf[i].flg.TrimExcess();
				seg.cut_inf[i].his.TrimExcess();
#endif
			}
			label_cuticle_line(seg, 0,0);
#if true//2018.11.28(メモリリーク)
			//GC.Collect();
			for (int i = 0; i < seg.cut_inf.Count; i++) {
				seg.cut_inf[i].lbl = null;//これ以後使用しないため解放
				seg.cut_inf[i].pct = null;
			}
			for (int i = 0; i < seg.moz_inf.Count; i++) {
				seg_of_mouz mouz = seg.moz_inf[i];
				mouz.ibf = null;
				seg.moz_inf[i] = mouz;
				//---
				mouz = seg.moz_hnf[i];
				mouz.iaf = null;
				mouz.ibf = null;
				seg.moz_hnf[i] = mouz;
			}
			//GC.Collect();
#endif
			m_back_of_x = pf.X;
		}
#endif


		private void test_nd(seg_of_hair[] segs, int idx, int cnt, bool bRECALCIR=false)
		{
			seg_of_hair seg = segs[idx];
			//(1)中心のラインを求める(両端は画像端まで拡張する)
			//(2)中心ラインに沿って左端から右端まで一定間隔で走査点を進める
			//(3)走査点で垂直方向に上下両側に延ばした時の輪郭線との交点を求める
			//(4)輪郭線との交点と走査点から断面点を求める
			//(5)断面点の画素値を格納する
			//---(1)
			
			int	cntm1 = 1;//(m_dia_cnt-1);
			FN1D[]	m_ft = new FN1D[cntm1];
			FN1D[]	m_fb = new FN1D[cntm1];
			FN1D[]	m_fc = new FN1D[cntm1];
			for (int i = 0; i < cntm1; i++) {
				m_ft[i] = null;//new FN1D(pt0, pt1);//毛髪上端の分割エッジ直線
				m_fb[i] = null;//new FN1D(pb0, pb1);//毛髪下端の分割エッジ直線
				m_fc[i] = new FN1D(new PointF(0, G.IR.HEIGHT/2), new PointF(G.IR.WIDTH-1, G.IR.HEIGHT/2));//new FN1D(pc0, pc1);//毛髪中心の分割エッジ直線
			}
			//---(2)
			double	px0 = (idx <= 0) ? 0: segs[idx-1].pix_pos.X;
			double	px1 = seg.pix_pos.X;
			double	dif = (px1-px0);
			int		i0 = 0;
			//double ds = 5;//5dot = 1.375um
			double	ds = G.UM2PX(G.SS.MOZ_CND_DSUM, m_log_info.pix_pitch, m_log_info.zoom);//横方向走査単位[pix]
			PointF	pf;
			double	xmin = 0;
			double	xmax = G.IR.WIDTH-1;
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
#if true //2018.12.17(オーバーラップ範囲)
			if (idx < (segs.Length-1)) {
				//右重なり有り
				int q1 = idx+0;
				int	q2 = idx+1;
				double right_of_curr_img = segs[q1].pix_pos.X + segs[q1].width-1;
				double left_of_next_img  = segs[q2].pix_pos.X;
				double	wid = right_of_curr_img - left_of_next_img;
#if true//2018.12.22(測定抜け対応)
				wid/=2;
#endif
				segs[q1].ow_r_wid = (int)wid;
				segs[q1].ow_r_pos =-(int)wid+segs[q1].width;
				segs[q2].ow_l_wid = (int)wid;
				segs[q2].ow_l_pos = (int)wid;
			}
			if (idx > 0) {
				//左重なり無し
				int q0 = idx-1;
				int q1 = idx-0;
				double right_of_prev_img = segs[q0].pix_pos.X + segs[q0].width-1;
				double left_of_curr_img  = segs[q1].pix_pos.X;
				double	wid = right_of_prev_img - left_of_curr_img;
#if true//2018.12.22(測定抜け対応)
				wid/=2;
#endif
				segs[q0].ow_r_wid = (int)wid;
				segs[q0].ow_r_pos =-(int)wid+segs[q0].width;
				segs[q1].ow_l_wid = (int)wid;
				segs[q1].ow_l_pos = (int)wid;
			}
#endif
			pf =sta_of_pf;

			for (s = ss; pf.X <= xmax; s++) {
				//(3) p2:上端, p3:下端
				FN1D f1 = m_fc[ii];			//現在X位置に対応する中心ラインの直線
				Point p5;

				//(4)
				//p5:中心, p6:R50%, p7:R-50%, p8:R+3um, p9:R-3um
				p5 = Point.Round(pf);
				//---
				if (bRECALCIR == false) {
					//(5)
					//格納
					seg.val_cen.Add(TO_CL(p5));
					seg.val_xum.Add(Math.Round(G.PX2UM(s*ds, m_log_info.pix_pitch, m_log_info.zoom), 2));
					//---
					seg.pts_cen.Add(p5);

				}

				// pf(中心ラインの直線式上)をdsだけ進める
				pf = scan_pt(m_fc, ref ii, pf, ds);
			}
#if true //2018.12.17(オーバーラップ範囲)
			if (seg.ow_l_pos >= 0) {
				for (int i = 0; i < (seg.pts_cen.Count-1); i++) {
					Point p0 = (Point)seg.pts_cen[i];
					Point p1 = (Point)seg.pts_cen[i+1];
					if (seg.ow_l_pos >= p0.X && seg.ow_l_pos < p1.X) {
						seg.ow_l_xum = (double)seg.val_xum[i];
						break;
					}
				}
			}
			if (seg.ow_r_pos >= 0) {
				for (int i = seg.pts_cen.Count-1; i > 0; i--) {
					Point p0 = (Point)seg.pts_cen[i-1];
					Point p1 = (Point)seg.pts_cen[i];
					if (seg.ow_r_pos >= p0.X && seg.ow_r_pos < p1.X) {
						seg.ow_r_xum = (double)seg.val_xum[i];
						break;
					}
				}
			}
#endif
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
			for (int q = 0; q <
#if true//2018.09.27(20本対応と解析用パラメータ追加)
				24
#else
				10
#endif
				; q++) {
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
#if true//2018.10.10(毛髪径算出・改造)
					continue;
#else
					break;
#endif
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
#if true//2018.11.13(毛髪中心AF)
				int v0, v1;
				string t0 = (string)m_zpos_val[i+0];
				string t1 = (string)m_zpos_val[i+1];
				string h0, h1;
				h0 = t0.Substring(0, 1).ToUpper();
				h1 = t1.Substring(0, 1).ToUpper();
				v0 = int.Parse(t0.Substring(1));
				v1 = int.Parse(t1.Substring(1));
#else
				int v0 = int.Parse((string)m_zpos_val[i+0]);
				int v1 = int.Parse((string)m_zpos_val[i+1]);
#endif
#if true//2018.11.13(毛髪中心AF)
				if (h1 == "K" && h0 == "Z") {
					//そのまま
				}else
#endif
				if (v1 < v0
#if true//2018.11.13(毛髪中心AF)
					|| (h1 == "Z" && h0 == "K")
#endif
					) {
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
#if true//2018.10.10(毛髪径算出・改造)
					else {
						Form02.DO_PROC_FOCUS(bmp, 0, 0, 0);
					}
#endif
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
#if true//2018.10.10(毛髪径算出・改造)
				bool bInit = false;
#endif
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
#if true//2019.03.16(NODATA対応)
					CL_ZDEPT = System.IO.Directory.GetFiles(path_dep, "*C?_??_ZDEPT.*");
#else
					CL_ZDEPT = System.IO.Directory.GetFiles(path_dep, "?C?_??_ZDEPT.*");
#endif
					if (CL_ZDEPT.Length > 0) {
						return(true);
					}
				}
#endif
				for (int q = 0; q <
#if true//2018.09.29(キューティクルライン検出)
					24
#else
					10
#endif
					; q++) {
					//---
					pat = string.Format("{0}C?_??_ZP00D.*", q);//カラー
					CL_ZP00D = System.IO.Directory.GetFiles(path, pat);
					//---
					if (
#if true//2018.10.10(毛髪径算出・改造)
						!bInit && CL_ZP00D.Length > 0
#else
						q == 0 && CL_ZP00D.Length > 0
#endif
						) {
						//opencvのセットアップのため呼び出し
						Bitmap bmp = new Bitmap(CL_ZP00D[0]);
						G.CAM_PRC = G.CAM_STS.STS_NONE;
						G.FORM02.load_file(bmp, false);
#if true//2018.10.10(毛髪径算出・改造)
						bInit = true;
#endif
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
			this.MOZ_CND_FTCF = C_FILT_COFS[G.SS.MOZ_CND_FTCF];
			this.MOZ_CND_FTCT = C_FILT_CNTS[G.SS.MOZ_CND_FTCT];
			this.MOZ_CND_SMCF = C_SMTH_COFS[G.SS.MOZ_CND_SMCF];//重み係数=11
			this.MOZ_CND_FOLD = (G.SS.MOZ_CND_FMOD == 0) ? G.SS.AUT_BEF_PATH: G.SS.MOZ_CND_FOLD;
			//---
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
#if true//2018.09.29(キューティクルライン検出)
			G.SS.MOZ_CND_HCNT = (G.SS.MOZ_CND_HMAX/G.SS.MOZ_CND_HWID);
#endif
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
#if true//2018.10.30(キューティクル長)
		private double TO_VAL(Point pt)
		{
			object obj = TO_CL(pt);
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
#endif
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
#if true//2018.09.29(キューティクルライン検出)
			if (this.radioButton4.Checked) {
				draw_cuticle(hr);
			}
#endif
#if true//2018.11.02(HSVグラフ)
			draw_hsv(hr);
#endif

		}
#if false//2018.08.21
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
		private void draw_moudan(hair hr)
		{
			try {
				//---
				int idx = m_isel;
				seg_of_hair seg = (seg_of_hair)hr.seg[idx];
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
				seg_of_hair seg_bak = seg;
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
					double um = G.PX2UM(seg.width, m_log_info.pix_pitch, m_log_info.zoom);
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
		private void draw_ow_vert_line(seg_of_hair seg, Graphics gr, float pw)
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
				if (
#if true//2019.03.16(NODATA対応)
					seg.bNODATA == false &&
#endif
					bmp_ir != null && seg.val_xum.Count > 0) {//赤外あり？
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
#if true//2018.10.10(毛髪径算出・改造)
//						Pen pen_d = new Pen(Color.DarkGreen, pw);
						Pen pen_l = new Pen(Color.LightGreen, pw);
#endif
						pen = new Pen(Color.Green, pw);
						for (int i = 0; i < seg.moz_zpb.Count; i+=1) {
							Point p1 = (Point)seg.moz_zpb[i];
							Point p2 = (Point)seg.moz_zpt[i];
#if true//2018.10.10(毛髪径算出・改造)
							if (this.checkBox15.Checked) {
								p1 = seg.moz_hpb[i];//補間データ
								p2 = seg.moz_hpt[i];
							}
							try {
#endif
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

#else
								gr_ir.DrawLine(pen, p1, p2);
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
						for (int i = 0; i < seg.moz_zpb.Count; i++) {
							if (seg.moz_out[i]) {
								Point p1, p2;
								if (this.checkBox15.Checked) {
									p1 = seg.moz_hpb[i];//補間データ
									p2 = seg.moz_hpt[i];
								}
								else {
									p1 = (Point)seg.moz_zpb[i];
									p2 = (Point)seg.moz_zpt[i];
								}
								gr_ir.DrawLine(pen, p1, p2);
							}
						}
					}
					if (this.checkBox16.Checked) {//赤外・除外域
						pen = new Pen(Color.DarkRed, pw);
						for (int i = 0; i < seg.moz_lbl.Count; i++) {
							if (seg.moz_lbl[i]<0) {
								Point p1, p2;
								p1 = (Point)seg.moz_zpb[i];
								p2 = (Point)seg.moz_zpt[i];
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
				gi_moz_rsl += seg.moz_hsl;
				gi_moz_rsd += seg.moz_hsd;
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
				dispose_bmp(ref bmp_dm);
				dispose_bmp(ref bmp_ir);
				dispose_bmp(ref bmp_pd);
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
		private void draw_graph_ow_line(Chart cht, int idx, double offs, seg_of_hair seg)
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
			seg_of_hair seg_bak = seg;
			double		offs = 0;
			double		xmin = 0;

			if (this.radioButton1.Checked) {//グラフ・全体
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
					seg = (seg_of_hair)hr.seg[q];
					if (seg == null) {
						continue;
					}
					if (seg.val_xum.Count <= 0) {
						double um = G.PX2UM(seg.width, m_log_info.pix_pitch, m_log_info.zoom);
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
					double x0 = TO_VAL(seg.val_xum[i]) + offs;
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
						if (this.numericUpDown1.Value == 0 && y2 != TO_VAL(seg.val_cen[i])) {
							double tmp = TO_VAL(seg.val_cen[i]);
#if false//2019.02.16(数値化白髪オフセット)
							throw new Exception("Internal Error");
#endif
						}
#endif
#endif
						this.chart1.Series[0].Points.AddXY(x0, y2);
					}
				}
				for (int i = i0; i < seg.moz_zpl.Count; i++) {
					double x0 = TO_VAL(seg.val_xum[i]) + offs;
					double y5 = TO_VAL(seg.moz_zpl[i]);
					double y6 = TO_VAL(seg.mou_len[i]);
#if true//2018.12.25(オーバーラップ範囲改)
					if ((double)seg.val_xum[i] < seg.ow_l_xum || (double)seg.val_xum[i] > seg.ow_r_xum) {
						continue;
					}
#endif

					if (this.checkBox11.Checked) {
#if true//2018.10.10(毛髪径算出・改造)
						//赤外・外れ判定
#if true//2018.10.10(毛髪径算出・改造)
						if (this.checkBox15.Checked) {
							y5 = seg.moz_hpl[i];//補間データ
						}
#endif
						if (this.checkBox13.Checked/* && seg.moz_out != null*/ && seg.moz_out[i]) {
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
							double df = px2um(mc, pc);
							if (mc.Y > pc.Y) {
								df=-df;
							}
							if (df != seg.moz_inf[i].ddf) {
								df = df;
							}
							if (this.checkBox15.Checked) {
								df = seg.moz_hnf[i].ddf;//補間データ
							}
							if (this.checkBox13.Checked && seg.moz_out[i]) {
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
				double	dx = TO_VAL(seg.val_xum[1])-TO_VAL(seg.val_xum[0]);
				//offs += dx * seg.moz_zpl.Count;
				offs += TO_VAL(seg.val_xum[seg.val_xum.Count-1])+dx;
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
				fmax = TO_VAL(seg.val_xum[seg.val_xum.Count-1])+offs;
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
				hair hr = (hair)m_hair[m_i];
				int i_s, i_e;
				List<string> i_no = new List<string>();
				string h_no = "";
				seg_of_hair seg;
				string fold;
				//0CR_03_ZP02D
				for (int q = 0; q < hr.seg.Length; q++) {
					seg = (seg_of_hair)hr.seg[q];
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
				seg = (seg_of_hair)hr.seg[0/*q*/];
				//offs = -TO_VAL(seg.val_xum[0]);
				dx = TO_VAL(seg.val_xum[1])-TO_VAL(seg.val_xum[0]);
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
					csv.set(c, r, this.MOZ_CND_FOLD);
				}
				for (int q = i_s; q <= i_e; q++, c++) {
					seg = (seg_of_hair)hr.seg[q];
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
					csv.set(c, r+7, F1S(seg.moz_hsl));
					csv.set(c, r+8, F1S(seg.moz_hsd));
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
					seg = (seg_of_hair)hr.seg[q];
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
					seg = (seg_of_hair)hr.seg[q];
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
					seg = (seg_of_hair)hr.seg[q];
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
					seg = (seg_of_hair)hr.seg[q];
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
					seg = (seg_of_hair)hr.seg[q];
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
					seg = (seg_of_hair)hr.seg[q];
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
					seg = (seg_of_hair)hr.seg[q];
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
					seg = (seg_of_hair)hr.seg[q];
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
					seg = (seg_of_hair)hr.seg[q];
					if (q == 11) {
						q  = q;
					}
					//---
					for (int i = 0; i < seg.val_xum.Count; i++) {
						double ff0 = TO_VAL(seg.val_xum[i]);
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
						double ff1 = TO_VAL(seg.val_cen_fil[i]);
						double ff2 = TO_VAL(seg.mou_len[i]);
						double ff3 = seg.moz_hpl[i];
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
#if false//2019.02.16(数値化白髪オフセット)
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
#endif
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
		private void do_fir(double[] fil, double[] src, double[] dst)
		{
			if (fil.Length < 3 || (fil.Length % 2) == 0) {
				MessageBox.Show("filterは３ケ以上、奇数ケで指定してください.");
				return;
			}
			for (int i = 0; i < src.Length; i++) {
				double sum = 0;
				for (int h = 0; h < fil.Length; h++) {
					int j = h - fil.Length/2;
					j = i + j;
					if (j < 0) {
						j = 0;
					}
					else if (j >= src.Length) {
						j = src.Length-1;
					}
					sum += src[j] * fil[h];
				}
				dst[i] = sum;
			}
		}
		private bool calc_filter_coeff()
		{
			//bool ret;
			//---
			//---
			if (G.SS.MOZ_CND_CTYP == 0) {
				//F_REMEZ remez = new F_REMEZ();
				int ntaps = G.SS.MOZ_CND_NTAP;//11;//(int)this.numericUpDown14.Value;
				int nbands = 3;//BPF
				double		fs = 1;//1Hz
				double[]	bands  = new double[3*2];
				double[]	gain   = {0,1,0};
				double[]	weight = {1,1,1};
				//double[]	deviat = new double[3];
				double[]	cof = null;
				bands[0] = 0.00;//BAND#1:下側
				bands[1] = 0.03;//BAND#1:上側
				bands[2] = G.SS.MOZ_CND_BPF1;//0.05;//BAND#2:下側
				bands[3] = G.SS.MOZ_CND_BPF2;// 0.30;//BAND#2:上側
				bands[4] = 0.40;//BAND#3:下側
				bands[5] = 0.50;//BAND#3:上側
				switch (G.SS.MOZ_CND_BPSL) {
					case 0://緩やか
						bands[1] = bands[2]-0.01;
						bands[4] = bands[3]+0.01;
						bands[1] = bands[2]-0.20;
						bands[4] = bands[3]+0.20;
					break;
					case 2://急
						bands[1] = bands[2]-0.001;
						bands[4] = bands[3]+0.001;
						bands[1] = bands[2]-0.05;
						bands[4] = bands[3]+0.05;
					break;
					default://普通
						bands[1] = bands[2]-0.005;
						bands[4] = bands[3]+0.005;
						bands[1] = bands[2]-0.10;
						bands[4] = bands[3]+0.10;
					break;
				}
				if (bands[1] <= bands[0]) {
					bands[1] = (bands[0]+bands[2])/2;
				}
				if (bands[4] >= bands[5]) {
					bands[4] = (bands[3]+bands[5])/2;
				}
				cof = F_REMEZ_SCIPY.sigtools_remez(ntaps, bands, gain, weight, F_REMEZ_SCIPY.BANDPASS, fs);
				//ret = remez.Remez(ntaps, nbands, bands, gain, weight, deviat, /*type*/0, fs);
				if (cof != null) {
					G.SS.MOZ_CND_FCOF = (double[])cof.Clone();
#if true//2018.10.10(毛髪径算出・改造)
					m_errstr = "";
#else
					this.textBox2.Text = "";
#endif
				}
				else {
#if true//2018.10.10(毛髪径算出・改造)
					m_errstr = "フィルタ計算エラー！\r" + F_REMEZ_SCIPY.ERRMSG;
#else
					this.textBox2.Text = "フィルタ計算エラー！\r" + F_REMEZ_SCIPY.ERRMSG;
#endif
					return(false);
				}
			}
			else {
#if true//2018.10.10(毛髪径算出・改造)
				m_errstr = "";
#else
				this.textBox2.Text = "";
#endif
			}
			return(true);
		}
#if true//2019.03.22(再測定表)
		private void apply_filter(List<object> asrc, out ArrayList adst)
		{
			ArrayList als = new ArrayList(asrc);
			apply_filter(als, out adst);
		}
		private void find_cuticle_line(List<Point> apts, ArrayList afil, out ArrayList acut, out LIST_U8 aflg, out List<int> ahis)
		{
			ArrayList alp = new ArrayList(apts);
			find_cuticle_line(alp, afil, out acut, out aflg, out ahis);
		}
#endif
#if true//2018.10.30(キューティクル長)
		private void apply_filter(List<double> asrc, out List<double> adst)
		{
			ArrayList als = new ArrayList(asrc);
			ArrayList ald = null;
			apply_filter(als, out ald);
			adst = new List<double>((double[])ald.ToArray(typeof(double)));
		}
#if true//2018.11.28(メモリリーク)
		private void find_cuticle_line(List<Point> apts, List<double> afil, out List<Point> acut, out LIST_U8 aflg, out List<int> ahis)
#else
		private void find_cuticle_line(List<Point> apts, List<double> afil, out List<Point> acut, out List<bool> aflg, out List<int> ahis)
#endif
		{
			ArrayList alp = new ArrayList(apts);
			ArrayList ald = new ArrayList(afil);
			ArrayList alc = null;
			find_cuticle_line(alp, ald, out alc, out aflg, out ahis);

			acut = new List<Point>((Point[])alc.ToArray(typeof(Point)));
		}
#endif
		private void apply_filter(ArrayList asrc, out ArrayList adst)
		{
			//bool ret;
			double[] src = new double[asrc.Count];
			double[] dst = new double[asrc.Count];
			//---
			for (int i = 0; i < src.Length; i++) {
				src[i] = TO_VAL(asrc[i]);
			}
#if true//2019.01.05(キューティクル検出欠損修正)
			if (double.IsNaN(src[0])) {
				//左側Nanを非Nan値で置き換える
				for (int i = 1; i < src.Length; i++) {
					if (double.IsNaN(src[i])) {
						continue;
					}
					for (int h = 0; h < i; h++) {
						src[h] = src[i];
					}
					break;
				}
			}
			if (double.IsNaN(src[src.Length-1])) {
				//右側Nanを非Nan値で置き換える
				for (int i = src.Length-1; i >= 0; i--) {
					if (double.IsNaN(src[i])) {
						continue;
					}
					for (int h = src.Length-1; h > i; h--) {
						src[h] = src[i];
					}
					break;
				}
			}
#endif

			//---
			if (G.SS.MOZ_CND_CTYP == 0) {
				do_fir(G.SS.MOZ_CND_FCOF, src, dst);
			}
			else {
				double fmax, fmin;
				if (G.SS.MOZ_CND_2DC1 > 0) {
					T.SG_POL_SMOOTH(src, src, src.Length, G.SS.MOZ_CND_2DC1, out fmax, out fmin);
				}
				if (true) {
					T.SG_2ND_DERI(src, dst, src.Length, G.SS.MOZ_CND_2DC0, out fmax);
				}
				if (G.SS.MOZ_CND_2DC2 > 0) {
					T.SG_POL_SMOOTH(dst, dst, dst.Length, G.SS.MOZ_CND_2DC2, out fmax, out fmin);
				}
				for (int i = 0; i < dst.Length; i++) {
					dst[i] *= -1;
				}
			}
			adst = new ArrayList(dst);
		}
#if true//2018.11.28(メモリリーク)
		private void find_cuticle_line(ArrayList apts, ArrayList afil, out ArrayList acut, out LIST_U8 aflg, out List<int> ahis)
#else
		private void find_cuticle_line(ArrayList apts, ArrayList afil, out ArrayList acut, out List<bool> aflg, out List<int> ahis)
#endif
		{
			//bool ret;
			double[] src = new double[apts.Count];
			double[] dst = new double[apts.Count];
#if true//2018.11.28(メモリリーク)
			byte [] flg = new byte[apts.Count];
#else
			bool[] flg = new bool[apts.Count];
#endif
			double	bval = (G.SS.MOZ_CND_CTYP == 0) ? G.SS.MOZ_CND_BPVL: G.SS.MOZ_CND_2DVL;
			acut = new ArrayList();
			//---
			for (int i = 0; i < src.Length; i++) {
				src[i] = TO_VAL(afil[i]);
#if true//2018.11.28(メモリリーク)
				flg[i] = 0;
#else
				flg[i] = false;
#endif
			}
			for (int i = 0; i < src.Length;) {
				if (src[i] < bval) {
					i++;continue;
				}
				//閾値以上の範囲から最大値の位置を求める
				int h = i, imax = i;
				double fmax = src[i];
				for (; h < src.Length; h++) {
					if (src[h] < bval) {
						break;
					}
					if (fmax < src[h]) {
						fmax = src[h];
						imax = h;
					}
				}
#if true//2018.11.28(メモリリーク)
				flg[imax] = 1;
#else
				flg[imax] = true;
#endif
				acut.Add(apts[imax]);
				i = h+1;
			}
			//---
#if true//2018.11.28(メモリリーク)
			aflg = new LIST_U8(flg);
#else
			aflg = new List<bool>(flg);
#endif
			//---
			int[] his = new int[G.SS.MOZ_CND_HCNT];
#if DEBUG//2018.10.27(画面テキスト)
			int tmp = 0;
#endif
			//---
			for (int i = 1; i < acut.Count; i++) {
				double df = G.diff((Point)acut[i-1], (Point)acut[i]);
				double um = G.PX2UM(df, m_log_info.pix_pitch, m_log_info.zoom);
				int k;
				k = (int)(um / G.SS.MOZ_CND_HWID);
				if (k >= 0 && k < G.SS.MOZ_CND_HCNT) {
					his[k]++;
				}
#if DEBUG//2018.10.27(画面テキスト)
				else {
					tmp++;
				}
#endif
			}
#if DEBUG//2018.10.27(画面テキスト)
			if (true) {
				int ttl = 0;
				for (int i = 0; i < his.Length; i++) {
					ttl += his[i];
				}
				if ((ttl+tmp) != acut.Count-1) {
					ttl = ttl;
				}
			}
#endif
			ahis = new List<int>(his);
		}
#if true//2018.10.30(キューティクル長)
		//const
		//double MAX_WID_OF_CUT_PNT = 2;

		private void check_neighborhood(seg_of_hair seg, int ix, int iy, List<Point> lcpt, ref double clen, int LBLNO)
		{
			int XMAX = seg.cut_inf[0].flg.Count;
			int YMAX = seg.cut_inf.Count;
			Point p0 = seg.cut_inf[iy].pbf[ix];
			Point p1;
			int x, y;
			//double um;
#if true//2018.11.02(HSVグラフ)
			int l_ope;
			int[] ope_x = {/*8近傍*/ 0, 1, 1, 1, 0,/*24近傍*/ 0, 1, 2, 2, 2,2,2,1,0/*48近傍*/, 0, 1, 2, 3, 3, 3, 3, 3, 3, 3, 2, 1, 0};
			int[] ope_y = {/*8近傍*/-1,-1, 0, 1, 1,/*24近傍*/-2,-2,-2,-1, 0,1,2,2,2/*48近傍*/,-3,-3,-3,-3,-2,-1, 0, 1, 2, 3, 3, 3, 3};
#else
			int[] ope_x = { 0, 1, 1, 1, 0};
			int[] ope_y = {-1,-1, 0, 1, 1};
#endif		
			if (seg.cut_inf[iy].lbl[ix] != 0) {
				G.mlog("Internal Error");
			}
			if (true) {
				seg.cut_inf[iy].lbl[ix] = LBLNO;
			}
			switch (G.SS.MOZ_CND_CNEI) {
				case  0:l_ope = 5; break;
				case  1:l_ope =14; break;
				default:l_ope =27; break;
			}

			for (int i = 0; i < l_ope; i++) {
				x = ix+ope_x[i];
				y = iy+ope_y[i];
				if (x < 0 || x >= XMAX) {
					continue;//範囲外
				}
				if (y < 0 || y >= YMAX) {
					continue;//範囲外
				}
				if (
#if true//2018.11.28(メモリリーク)
					seg.cut_inf[y].flg[x] == 0
#else
					!seg.cut_inf[y].flg[x]
#endif
					) {
					continue;//キューティクル無し
				}
				if (seg.cut_inf[y].lbl[x] == LBLNO) {
					continue;//ラベル済
				}
				//---
				p1 = seg.cut_inf[y].pbf[x];
				lcpt.Add(p1);				//連結相手
#if true//2018.11.02(HSVグラフ)
				if (true) {
					clen += px2um(p1, p0);
				}
				else
#endif
				if (ope_x[i] == 0 || ope_y[i] == 0) {
					clen += 1.0;			//連結:縦横
				}
				else {
					clen += Math.Sqrt(2);	//連結:斜め
				}
				if (seg.cut_inf[y].lbl[x] != 0) {
					i = i;//ラベル済
				}
				else {
					//ラベル未→再帰呼び出しにてキューティクルを連結させていく
					check_neighborhood(seg, x, y, lcpt, ref clen, LBLNO);
				}
				//
				//um = px2um(p1, p0);
				//if (um <= MAX_WID_OF_CUT_PNT) {
				//    //登録
				//}
				break;//終了→元に戻る
			}
		}
		private void label_cuticle_line(seg_of_hair seg, int ix, int iy)
		{
			int XLEN = seg.cut_inf[0].flg.Count;
			int YLEN = seg.cut_inf.Count;
			int LBLNO = 1;
			double
				LTTL = 0;

			seg.cut_lsp.Clear();
			seg.cut_len.Clear();

			for (int i = 0; i < YLEN; i++) {
				seg.cut_inf[i].lbl = Enumerable.Repeat<int>(0, XLEN).ToList();
			}
			for (int x = 0; x < XLEN; x++) {
				for (int y = 0; y < YLEN; y++) {
					if (
#if true//2018.11.28(メモリリーク)
					seg.cut_inf[y].flg[x] == 0
#else
					!seg.cut_inf[y].flg[x]
#endif
					) {
						continue;//キューティクル無し
					}
					if (seg.cut_inf[y].lbl[x] != 0) {
						continue;//ラベル済
					}
					Point p0 = seg.cut_inf[y].pbf[x];
					List<Point> lcpt = new List<Point>();
					double clen = 0;
					lcpt.Add(p0);
					check_neighborhood(seg, x, y, lcpt, ref clen, LBLNO);
					if (lcpt.Count <= 1) {
						y = y;//孤立したキューティクルポイント
					}
					else if (clen <= G.SS.MOZ_CND_CMIN) {
						y = y;//最短以下のため無視
					}
					else {
#if true//2018.11.28(メモリリーク)
						lcpt.TrimExcess();
#endif
						//登録
						seg.cut_lsp.Add(lcpt);
						seg.cut_len.Add(clen);
						LTTL += clen;
					}
					LBLNO++;
				}
			}
			seg.cut_ttl = LTTL;
#if true//2018.11.28(メモリリーク)
			seg.cut_len.Clear();//使用されてないため
			seg.cut_lsp.TrimExcess();
#endif
		}
#endif
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
		private void draw_cuticle(hair hr)
		{
			try {
				//---
				int idx = m_isel;
				seg_of_hair seg = (seg_of_hair)hr.seg[idx];
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
				seg_of_hair seg_bak = seg;
				double		offs = 0;
				double		xmin = 0;

				if (this.radioButton1.Checked) {//グラフ・毛髪全体
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
						seg = (seg_of_hair)hr.seg[q];
						if (seg == null) {
							continue;
						}
						if (seg.val_xum.Count <= 0) {
							double um = G.PX2UM(seg.width, m_log_info.pix_pitch, m_log_info.zoom);
							double x0 = um+ offs;
							this.chart4.Series[0].Points.AddXY(x0, double.NaN);
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
						double x0 = TO_VAL(seg.val_xum[i]) + offs;
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
							if (this.numericUpDown1.Value == 0 && y2 != TO_VAL(seg.val_cen_fil[i])) {
								double tmp = TO_VAL(seg.val_cen_fil[i]);
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
					double	dx = TO_VAL(seg.val_xum[1])-TO_VAL(seg.val_xum[0]);
					//offs += dx * seg.moz_zpl.Count;
					offs += TO_VAL(seg.val_xum[seg.val_xum.Count-1])+dx;
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
					fmax = TO_VAL(seg.val_xum[seg.val_xum.Count-1])+offs;
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
#if true//2019.01.11(混在対応)
		private void SWAP_ANL_CND(int mode)
		{
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
#if true//2018.09.29(キューティクルライン検出)
		public void UPDATE_CUTICLE()
		{//キューティクル・フィルター処理
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
#if true//2019.01.11(混在対応)
			SWAP_ANL_CND(hr.mode_of_cl);//0:透過, 1:反射
#endif
			if (!calc_filter_coeff()) {
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
			if (m_i >= m_hair.Count) {
				return;
			}
			hair hr = (hair)m_hair[m_i];
			if (m_isel >= hr.seg.Count()) {
				return;
			}
#if true//2019.01.11(混在対応)
			SWAP_ANL_CND(hr.mode_of_cl);//0:透過, 1:反射
#endif
			for (int i = 0; i < hr.seg.Length; i++) {
				seg_of_hair[] segs = hr.seg;
				seg_of_hair seg = (seg_of_hair)hr.seg[i];
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

				load_bmp(segs, i,
					path_dm1, path_dm2,
					path_ir1, path_ir2,
					ref m_bmp_dm0, ref m_bmp_dm1, ref m_bmp_dm2,
					ref m_bmp_ir0, ref m_bmp_ir1, ref m_bmp_ir2
				);
				if (true) {
					dispose_bmp(ref m_bmp_pd1);
					if (name_pd1.Equals(name_dm1)) {
						m_bmp_pd1 = (Bitmap)m_bmp_dm1.Clone();
					}
					else {
						m_bmp_pd1 = new Bitmap(path_pd1);
					}
				}
				if (true) {
					if (m_bmp_ir1 != null && G.SS.MOZ_CND_FTCF > 0) {
						Form02.DO_SMOOTH(m_bmp_ir1, this.MOZ_CND_FTCF, this.MOZ_CND_FTCT);
						//m_bmp_ir1.Save("c:\\temp\\"+name_ir1);
#if false//2018.10.24(毛髪径算出・改造)
						m_bmp_ir1.Save("c:\\temp\\IMG_IR.PNG");
#endif
					}
#if true//2018.11.02(HSVグラフ)
					if (mode == 1) {
						calc_hist(segs[i]);
					}
					else
#endif
					if (segs[i].dia_cnt > 1) {
//Bitmap bmp_msk = (Bitmap)m_bmp_ir1.Clone();
//Form02.DO_SET_FBD_REGION(m_bmp_ir1, bmp_msk, segs[i].dia_top, segs[i].dia_btm);
						test_dm(segs, i, segs.Length, true);
					}
				}
			}
			dispose_bmp(ref m_bmp_dm0);
			dispose_bmp(ref m_bmp_dm1);
			dispose_bmp(ref m_bmp_dm2);
			dispose_bmp(ref m_bmp_ir0);
			dispose_bmp(ref m_bmp_ir1);
			dispose_bmp(ref m_bmp_ir2);

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
				if (m_i >= m_hair.Count) {
					return;
				}
				hair hr = (hair)m_hair[m_i];
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
			if (m_i >= m_hair.Count) {
				return;
			}
			hair hr = (hair)m_hair[m_i];
			SWAP_ANL_CND(hr.mode_of_cl);//0:透過, 1:反射
#endif
			this.MOZ_CND_FTCF = C_FILT_COFS[G.SS.MOZ_CND_FTCF];
			this.MOZ_CND_FTCT = C_FILT_CNTS[G.SS.MOZ_CND_FTCT];
			this.MOZ_CND_SMCF = C_SMTH_COFS[G.SS.MOZ_CND_SMCF];
			//---
			UPDATE_BY_FILES(0);
		}
#if true//2018.11.02(HSVグラフ)
		public void UPDATE_HSV()
		{
#if true//2019.01.11(混在対応)
			if (m_i >= m_hair.Count) {
				return;
			}
			hair hr = (hair)m_hair[m_i];
			SWAP_ANL_CND(hr.mode_of_cl);//0:透過, 1:反射
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
			hair hr = (hair)m_hair[m_i];
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
		private void draw_hsv(hair hr)
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
					seg_of_hair seg = (seg_of_hair)hr.seg[q];
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
		private void calc_hist(seg_of_hair seg)
		{
			Point[]	pts_dia_top = (Point[])seg.dia_top.Clone();
			Point[]	pts_dia_btm = (Point[])seg.dia_btm.Clone();
			int l = pts_dia_top.Length;
			int width = m_bmp_dm1.Width;
			Point[]	pts_msk_top = (Point[])seg.dia_top.Clone();
			Point[]	pts_msk_btm = (Point[])seg.dia_btm.Clone();
//			const
//			double RT = 0.20;
			double RT = (1.0 - G.SS.MOZ_CND_HIST/100.0)/2.0; 
//			const
			double RB = (1-RT);

			if (true) {
				pts_dia_top[0].X = 0;
				pts_dia_btm[0].X = 0;
				pts_dia_top[l-1].X = width-1;
				pts_dia_btm[l-1].X = width-1;
			}
			for (int i = 0; i < l; i++) {
				pts_msk_top[i].X = (int)(pts_dia_top[i].X + RT * (pts_dia_btm[i].X - pts_dia_top[i].X));
				pts_msk_top[i].Y = (int)(pts_dia_top[i].Y + RT * (pts_dia_btm[i].Y - pts_dia_top[i].Y));
				pts_msk_btm[i].X = (int)(pts_dia_top[i].X + RB * (pts_dia_btm[i].X - pts_dia_top[i].X));
				pts_msk_btm[i].Y = (int)(pts_dia_top[i].Y + RB * (pts_dia_btm[i].Y - pts_dia_top[i].Y));
			}
			seg.his_top = pts_msk_top;
			seg.his_btm = pts_msk_btm;
			//---
			Form02.DO_SET_FBD_REGION(m_bmp_dm1, pts_msk_top, pts_msk_btm);
			for (int j = 0; j < 256; j++) {
				seg.HIST_H_DM[j] = G.IR.HISTVALH[j];
				seg.HIST_S_DM[j] = G.IR.HISTVALS[j];
				seg.HIST_V_DM[j] = G.IR.HISTVALV[j];
			}
			Form02.DO_SET_FBD_REGION(m_bmp_pd1, pts_msk_top, pts_msk_btm);
			for (int j = 0; j < 256; j++) {
				seg.HIST_H_PD[j] = G.IR.HISTVALH[j];
				seg.HIST_S_PD[j] = G.IR.HISTVALS[j];
				seg.HIST_V_PD[j] = G.IR.HISTVALV[j];
			}
			Form02.DO_SET_FBD_REGION(m_bmp_ir1, pts_msk_top, pts_msk_btm);
			for (int j = 0; j < 256; j++) {
				seg.HIST_H_IR[j] = G.IR.HISTVALH[j];
				seg.HIST_S_IR[j] = G.IR.HISTVALS[j];
				seg.HIST_V_IR[j] = G.IR.HISTVALV[j];
			}
		}
#endif
	}
}
