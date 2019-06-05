using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//%%using System;
//%%using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//%%using System.Linq;
//%%using System.Text;
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
	//class DIGITI
	//{

	//}
	public class DIGITI
	{
		public class test00 {
			private int dum00;
			public int get_dum() {
				return(dum00);
			}
			public void set_dum(int n) {
				dum00 = n;
			}
		};
		private bool bREMES;
		//---
		static public int[] C_FILT_COFS = new int[] { 0, 3, 5, 7, 9, 11 };
		static public int[] C_FILT_CNTS = new int[] { 1, 5,10, 15, 20};
		static public int[] C_SMTH_COFS = new int[] { 0,5,7,9,11,13,15,17,19,21,23, 25};
		private int m_i = 0;
		private int m_isel = 0;
#if true//2018.10.10(毛髪径算出・改造)
		private int m_imou = 0;
#endif
		public int MOZ_CND_FTCF;//5:11x11
		public int MOZ_CND_FTCT;//0:1回
		public int MOZ_CND_SMCF;//5:重み係数=11
		public string MOZ_CND_FOLD;
		//---
		public List<hair> m_hair = new List<hair>();
		//---
		public Bitmap	m_bmp_dm1, m_bmp_dm0, m_bmp_dm2;
		public Bitmap	m_bmp_ir1, m_bmp_ir0, m_bmp_ir2;
		public Bitmap	m_bmp_pd1;
		public Bitmap	m_bmp_pd0, m_bmp_pd2;
		private Point[] m_dia_top;
		private Point[]	m_dia_btm;
		private int		m_dia_cnt;
		private int		m_chk1, m_chk2;
		//---
		public Dictionary<string, ImageList> m_map_of_dml;
		public Dictionary<string, ImageList> m_map_of_irl;
		public Dictionary<string, ImageList> m_map_of_pdl;//毛髪径:位置検出
		private int		m_thm_wid, m_thm_hei;
		public ArrayList m_zpos_org = new ArrayList();
		public ArrayList m_zpos_val = new ArrayList();
		public string m_fold_of_dept;
		static public string m_errstr;


#if true//2019.04.17(毛髄検出複線化)
		public void chk_pt(seg_of_hair seg, int idx)
		{
#if false
			Point p1;
			Point p2;
			int	i = idx;

			p1 = (Point)seg.moz_zpb[i];
			p2 = (Point)seg.moz_zpt[i];
			if (seg.moz_inf[i].pml != p2 || seg.moz_inf[i].pmr != p1) {
				G.mlog("seg.moz_inf[i].pml != p2 || seg.moz_inf[i].pmr != p1 @ i=" + i.ToString());
			}
			if (seg.moz_hnf == null || seg.moz_hpb.Count <= 0 || seg.moz_hpt.Count <= 0) {
				return;
			}
			//---
			p1 = seg.moz_hpb[i];//補間データ
			p2 = seg.moz_hpt[i];
			if (seg.moz_hnf[i].pml != p2 || seg.moz_hnf[i].pmr != p1) {
				G.mlog("seg.moz_hnf[i].pml != p2 || seg.moz_hnf[i].pmr != p1 @ i=" + i.ToString());
			}
#endif
		}
		public void get_pt(seg_of_hair seg, int idx, bool bATO, out Point p1, out Point p2)
		{
			if (!bATO) {
				p1 = seg.moz_inf[idx].pmr;//moz_zpb
				p2 = seg.moz_inf[idx].pml;//moz_zpt
			}
			else {
				p1 = seg.moz_hnf[idx].pmr;//moz_hpb
				p2 = seg.moz_hnf[idx].pml;//moz_hpt
			}
		}
		public void get_pt(seg_of_hair seg, int idx, bool bATO, out Point p1, out Point p2, out double len)
		{
			if (!bATO) {
				p1  = seg.moz_inf[idx].pmr;
				p2  = seg.moz_inf[idx].pml;
				len = seg.moz_inf[idx].moz_zpl;
			}
			else {
				p1  = seg.moz_hnf[idx].pmr;
				p2  = seg.moz_hnf[idx].pml;
				len = seg.moz_hnf[idx].moz_zpl;
			}
		}
		public void get_pl(seg_of_hair seg, int idx, bool bATO, out double len)
		{
			if (!bATO) {
				len = seg.moz_inf[idx].moz_zpl;
			}
			else {
				len = seg.moz_hnf[idx].moz_zpl;
			}
		}

#endif
		//---
//%%		public Form03()
//%%		{
//%%			InitializeComponent();
//%%		}
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
#if true//2019.04.17(毛髄検出複線化)
			public double		moz_zpl;//毛髄:長さ径
			public bool			moz_out;//外れ:true, 正常:false
			public int			moz_lbl;//ラベル番号(0:毛髄無し,以外:毛髄領域の連番)
			public double		moz_sum;//該当ラベルの積算値を格納([0]にラベル1の積算値)

			//public Point		zpt;//毛髄:上側点
			//public Point		zpb;//毛髄:下側点
			//public double		zpl;//毛髄:長さ径
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
#if true//2019.04.09(再測定実装)
			public bool bREMES;
			public double zp_contr;
			public double zp_contr_avg;
			public double zp_contr_drop;
			public bool zp_nodata;
			public double kp_contr;
			public double kp_contr_avg;
			public double kp_contr_drop;
			public bool kp_nodata;
			//---
			public int	cut_count;
			public double	cut_avg;
			public double	cut_drop;
			//---
			public double mou_len_l;
			public double mou_len_r;
			public double mou_len_c;

			public Point[]	msk_of_dm;//コントラスト計算多曲線
			public Point[]	msk_of_pd;//コントラスト計算多曲線
			public string[] bak_folds;
			public int bak_cnt;
			public bool bTMR;
			public Point[]	dex_top;//輪郭・頂点(上側)・画像端まで補外
			public Point[]	dex_btm;//輪郭・頂点(下側)
			public Point[]	dex_cen;
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
#if false//2019.04.17(毛髄検出複線化)
			public ArrayList moz_zpt;//毛髄:上側点
			public ArrayList moz_zpb;//毛髄:下側点
			public ArrayList moz_zpl;//毛髄:長さ径
#endif
			public ArrayList moz_top;
			public ArrayList moz_btm;
#if true//2018.10.10(毛髪径算出・改造)
			public List<seg_of_mouz>
								moz_inf;
#if false//2019.04.17(毛髄検出複線化)
			public List<bool>	moz_out;//外れ:true, 正常:false
			public List<int>	moz_lbl;//ラベル番号(0:毛髄無し,以外:毛髄領域の連番)
			public List<double> moz_sum;//該当ラベルの積算値を格納([0]にラベル1の積算値)
			public List<Point>	moz_hpt;//毛髄:上側点:補間後
			public List<Point>	moz_hpb;//毛髄:下側点:補間後
			public List<double>	moz_hpl;//毛髄:長さ径:補間後
#endif
			public List<seg_of_mouz>
								moz_hnf;//毛髄:補間後
			public double		moz_rsl;//毛髄面積:Sl
			public double		moz_rsd;//毛髄面積:Sd
			public double		moz_hsl;//毛髄面積:Sl(補間後)
			public double		moz_hsd;//毛髄面積:Sd(補間後)
#endif
#if true//2019.05.07(毛髄複線面積値対応)
			public double		moz_rsl1,moz_rsl2,moz_rsl3, moz_rsl_mul;//毛髄面積:Sl
			public double		moz_rsd1,moz_rsd2,moz_rsd3, moz_rsd_mul;//毛髄面積:Sd
			public double		moz_hsl1,moz_hsl2,moz_hsl3, moz_hsl_mul;//毛髄面積:Sl(補間後)
			public double		moz_hsd1,moz_hsd2,moz_hsd3, moz_hsd_mul;//毛髄面積:Sd(補間後)
#endif
#if true//2019.04.17(毛髄検出複線化)
			public List<seg_of_mouz>
								moz_inf1,moz_inf2,moz_inf3;
			public List<seg_of_mouz>
								moz_hnf1,moz_hnf2,moz_hnf3;//毛髄:補間後
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
#if false//2019.04.17(毛髄検出複線化)
				this.moz_zpt = new ArrayList();//毛髄:上側点
				this.moz_zpb = new ArrayList();//毛髄:下側点
				this.moz_zpl = new ArrayList();//毛髄:長さ径
#endif
				this.moz_top = new ArrayList();
				this.moz_btm = new ArrayList();
#if true//2018.10.10(毛髪径算出・改造)
				this.moz_inf = new List<seg_of_mouz>();
				//this.moz_fs2 = new List<bool>();//S1,S2区分,S1:true, S2:false
				//this.moz_avg = new List<double>();//毛髄:毛髄範囲の画素平均値(生画像による)
#if false//2019.04.17(毛髄検出複線化)
				this.moz_out = new List<bool>();
				this.moz_hpt = new List<Point>();//毛髄:上側点:補間後
				this.moz_hpb = new List<Point>();//毛髄:下側点:補間後
				this.moz_hpl = new List<double>();//毛髄:長さ径:補間後
#endif
#endif
#if true//2019.04.17(毛髄検出複線化)
				this.moz_inf1 = new List<seg_of_mouz>();
				this.moz_inf2 = new List<seg_of_mouz>();
				this.moz_inf3 = new List<seg_of_mouz>();
				this.moz_hnf1 = new List<seg_of_mouz>();
				this.moz_hnf2 = new List<seg_of_mouz>();
				this.moz_hnf3 = new List<seg_of_mouz>();
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

		public class hair {
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
#if true//2019.04.17(毛髄検出複線化)
			int len = seg.moz_inf.Count;
#else
			int len = seg.moz_zpl.Count;
#endif
			List<int> lbl = new List<int>();
			List<double> sum = Enumerable.Repeat<double>(0, len).ToList();
			int n_of_lbl = 0;
			double pl_bak = 0;
#if true//2019.04.17(毛髄検出複線化)
			Point pt_bak, pb_bak;
			get_pt(seg, 0, false, out pb_bak, out pt_bak);
#else
			Point pt_bak = (Point)seg.moz_zpt[0];
			Point pb_bak = (Point)seg.moz_zpb[0];
#endif
			//ラベリングによる領域分割

			for (int i = 0; i < len; i++) {
#if true//2019.04.17(毛髄検出複線化)
				Point pt, pb;
				double pl;
				chk_pt(seg, i);
				get_pt(seg, i, false, out pb, out pt, out pl);
#else
				Point pt = (Point)seg.moz_zpt[i];	//毛髄:上側点
				Point pb = (Point)seg.moz_zpb[i];	//毛髄:下側点
				double pl = (double)seg.moz_zpl[i];	//毛髄:長さ径
#endif
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
#if true//2019.04.17(毛髄検出複線化)
					double tmp;
					get_pl(seg, i, false, out tmp);
					ttl += tmp;
#else
					ttl += (double)seg.moz_zpl[i];
#endif
				}
				sum[no-1] = ttl;
				if (G.SS.MOZ_CND_CHK1 && ttl <= G.SS.MOZ_CND_SMVL) {
					for (; h < i; h++) {
						lbl[h] = -no;
					}
				}
				i--;//一つ戻してから再開
			}
#if true//2019.04.17(毛髄検出複線化)
			for (int i = 0; i < len; i++) {
				seg_of_mouz mz = seg.moz_inf[i];
				mz.moz_lbl = lbl[i];
				mz.moz_sum = sum[i];
				seg.moz_inf[i] = mz;
			}
#else
			seg.moz_lbl = lbl;
			seg.moz_sum = sum;
#endif
		}
		void detect_outliers(seg_of_hair seg)
		{
			int WID_OF_AVG;
			double THR_OF_VAL;
			double avg = 0, div = 0, std = 0, evl;
			int cnt;
#if true//2019.04.17(毛髄検出複線化)
			int len = seg.moz_inf.Count;
#else
			int len = seg.moz_zpl.Count;
#endif
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
						if (seg.moz_inf[i].moz_lbl < 0) {
							buf[i] = 0;
						}
						else {
#if true//2019.04.17(毛髄検出複線化)
							double tmp;
							get_pl(seg, i, false, out tmp);
							get_pl(seg, i, false, out buf[i]);
							if (buf[i] != tmp) {
								G.mlog("kakunin");
							}
#else
							buf[i] = (double)seg.moz_zpl[i];
#endif
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
						if (seg.moz_inf[i].moz_lbl < 0) {
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
#if true//2019.04.17(毛髄検出複線化)
				if (cnt > seg.moz_inf.Count) {
					cnt = seg.moz_inf.Count;
				}
#else
				if (cnt > seg.moz_zpl.Count) {
					cnt = seg.moz_zpl.Count;
				}
#endif
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
#if true//2019.04.17(毛髄検出複線化)
			if (otl.Length != len) {
				len = len;
			}
			for (int i = 0; i < len; i++) {
				seg_of_mouz mz = seg.moz_inf[i];
				mz.moz_out = otl[i];
				seg.moz_inf[i] = mz;
			}
#else
			if (seg.moz_out != null) {
				seg.moz_out.Clear();
				seg.moz_out = null;
			}
			seg.moz_out = new List<bool>(otl);
#endif
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
#if true//2019.04.17(毛髄検出複線化)
			int len = seg.moz_inf.Count;
#else
			int len = seg.moz_zpl.Count;
#endif
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
#if true//2019.04.17(毛髄検出複線化)
				Point pt, pb;
				double pl;
				get_pt(seg, i, false, out pb, out pt, out pl);
#else
				Point pt = (Point)seg.moz_zpt[i];
				Point pb = (Point)seg.moz_zpb[i];
				double pl = (double)seg.moz_zpl[i];
#endif
				double pl_bak = pl;
				int cnt = 0;
				int jcnt;
				int jmax;
				seg_of_mouz mz = seg.moz_inf[i];
#if true//2019.04.17(毛髄検出複線化)
				chk_pt(seg, i);
#endif
				if (!seg.moz_inf[i].moz_out) {
					if (seg.moz_inf[i].moz_lbl < 0) {
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
#if true//2019.04.17(毛髄検出複線化)
					else {
						i = i;
					}
#endif
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
					if (seg.moz_inf[j].moz_lbl < 0) {
						continue;//除外データは補間元データとしない
					}
					if (!seg.moz_inf[j].moz_out) {
#if true//2019.04.17(毛髄検出複線化)
						Point p1, p2;
						get_pt(seg, j, false, out p1, out p2);
						rawtL.Add(p2);
						rawbL.Add(p1);
#else
						rawtL.Add((Point)seg.moz_zpt[j]);
						rawbL.Add((Point)seg.moz_zpb[j]);
#endif
						rawdL.Add(seg.moz_inf[j].ddf);
						cnt++;
						jcnt++;
#if true//2019.04.17(毛髄検出複線化)
						chk_pt(seg, j);
#endif
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
					if (seg.moz_inf[j].moz_lbl < 0) {
						continue;//除外データは補間元データとしない
					}
					if (!seg.moz_inf[j].moz_out) {
#if true//2019.04.17(毛髄検出複線化)
						Point p1, p2;
						get_pt(seg, j, false, out p1, out p2);
						rawtR.Add(p2);
						rawbR.Add(p1);
#else
						rawtR.Add((Point)seg.moz_zpt[j]);
						rawbR.Add((Point)seg.moz_zpb[j]);
#endif
						rawdR.Add(seg.moz_inf[j].ddf);
						cnt++;
						jcnt++;
#if true//2019.04.17(毛髄検出複線化)
						chk_pt(seg, j);
#endif
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
#if true//2019.04.17(毛髄検出複線化)
					mz.iml = mz.ihc;
					mz.imr = mz.ihc;
#endif
				}
				if (pl_bak > 0 && pl <= 0) {
					pl = pl;
				}
				if (true) {
					mz.imc = (mz.iml+mz.imr)/2;
					mz.pml = mz.pbf[mz.iml];
					mz.pmr = mz.pbf[mz.imr];
					mz.pmc = new PointF((mz.pml.X + mz.pmr.X)/2f, (mz.pml.Y+mz.pmr.Y)/2f);
					mz.ddf = px2um(mz.pmc, mz.phc);
					if (mz.pmc.Y > mz.phc.Y) {
						mz.ddf *= -1;
					}
				}
#if true//2019.04.17(毛髄検出複線化)
				if (mz.pml != pt) {
					i = i;
				}
				if (mz.pmr != pb) {
					i = i;
				}
				mz.moz_zpl = pl;
#endif
				hpt.Add(pt);
				hpb.Add(pb);
				hpl.Add(pl);
				hmz.Add(mz);
				if (mz.ddf == 0 && seg.moz_inf[i].ddf != 0) {
					i = i;
				}
#if false//2019.04.17(毛髄検出複線化)
				if (pl == 0 && TO_VAL(seg.moz_zpl[i]) != 0) {
					i = i;
				}
#endif
			}
#if false//2019.04.17(毛髄検出複線化)
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
#endif
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
#if true//2019.04.17(毛髄検出複線化)
				if (moui.fs2) {
					seg.moz_rsl += moui.moz_zpl;
				}
				else {
					seg.moz_rsd += moui.moz_zpl;
				}
				if (mouh.fs2) {
					seg.moz_hsl += mouh.moz_zpl;
				}
				else {
					seg.moz_hsd += mouh.moz_zpl;
				}
#endif
			}
		}
#if true//2019.05.07(毛髄複線面積値対応)
		void sum_avg_mult(seg_of_hair seg)
		{
			Point	pt1, pt2, pt3;
			Point	pb1, pb2, pb3;
			double	ss1, ss2, ss3;
			bool	fs1, fs2, fs3;

			seg.moz_rsl_mul = 0;//毛髄面積:Sl
			seg.moz_rsd_mul = 0;//毛髄面積:Sd
			seg.moz_hsl_mul = 0;//毛髄面積:Sl(補間後)
			seg.moz_hsd_mul = 0;//毛髄面積:Sd(補間後)
			for (int i = 0; i < seg.moz_hnf.Count; i++) {
				if (true) {
					pt1 = seg.moz_inf1[i].pml;//上側
					pb1 = seg.moz_inf1[i].pmr;
					ss1 = seg.moz_inf1[i].moz_zpl;
					fs1 = seg.moz_inf1[i].fs2;

					pt2 = seg.moz_inf2[i].pml;//中心
					pb2 = seg.moz_inf2[i].pmr;
					ss2 = seg.moz_inf2[i].moz_zpl;
					fs2 = seg.moz_inf2[i].fs2;

					pt3 = seg.moz_inf3[i].pml;//下側
					pb3 = seg.moz_inf3[i].pmr;
					ss3 = seg.moz_inf3[i].moz_zpl;
					fs3 = seg.moz_inf3[i].fs2;

					if (ss1 != 0 && ss2 != 0 && pb1.Y >= pt2.Y) {
						ss1 = px2um(pt1, pt2);
					}
					if (ss2 != 0 && ss3 != 0 && pt3.Y <= pb2.Y) {
						ss3 = px2um(pb2, pb3);
					}
					if (fs1) {
						seg.moz_rsl_mul += ss1;
					}
					else {
						seg.moz_rsd_mul += ss1;
					}
					if (fs2) {
						seg.moz_rsl_mul += ss2;
					}
					else {
						seg.moz_rsd_mul += ss2;
					}
					if (fs3) {
						seg.moz_rsl_mul += ss3;
					}
					else {
						seg.moz_rsd_mul += ss3;
					}
				}
				if (true) {
					pt1 = seg.moz_hnf1[i].pml;//上側
					pb1 = seg.moz_hnf1[i].pmr;
					ss1 = seg.moz_hnf1[i].moz_zpl;
					fs1 = seg.moz_hnf1[i].fs2;

					pt2 = seg.moz_hnf2[i].pml;//中心
					pb2 = seg.moz_hnf2[i].pmr;
					ss2 = seg.moz_hnf2[i].moz_zpl;
					fs2 = seg.moz_hnf2[i].fs2;

					pt3 = seg.moz_hnf3[i].pml;//下側
					pb3 = seg.moz_hnf3[i].pmr;
					ss3 = seg.moz_hnf3[i].moz_zpl;
					fs3 = seg.moz_hnf3[i].fs2;

					if (ss1 != 0 && ss2 != 0 && pb1.Y >= pt2.Y) {
						ss1 = px2um(pt1, pt2);
					}
					if (ss2 != 0 && ss3 != 0 && pt3.Y <= pb2.Y) {
						ss3 = px2um(pb2, pb3);
					}
					if (fs1) {
						seg.moz_hsl_mul += ss1;
					}
					else {
						seg.moz_hsd_mul += ss1;
					}
					if (fs2) {
						seg.moz_hsl_mul += ss2;
					}
					else {
						seg.moz_hsd_mul += ss2;
					}
					if (fs3) {
						seg.moz_hsl_mul += ss3;
					}
					else {
						seg.moz_hsd_mul += ss3;
					}
				}
			}
		}
#endif
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
		public double px2um(PointF p1, PointF p2)
		{
			double df = G.diff(p1, p2);
			double um = G.PX2UM(df, m_log_info.pix_pitch, m_log_info.zoom);

			return(um);
		}
		public double px2um(double df)
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
#if true//2019.04.17(毛髄検出複線化)
			int imul = 1;//中心
retry_of_multi:
			if (true) {
				ic = af.Length/2;
				sil = (ic-ll);
				sir = (ic+lr);
				ihl = sil;
				ihr = sir;
			}
			switch (imul) {
			case 0://上側
				ic-= ll/2;
				break;
			case 1://中心
				ic = ic;
				break;
			default://下側
				ic+= ll/2;
				break;
			}
			upl_buf.Clear();
			upr_buf.Clear();
			uil_buf.Clear();
			uir_buf.Clear();
			udf_buf.Clear();
#else
			sil = (ic-ll);
			sir = (ic+lr);
#if true//2018.11.06(毛髄4)
			ihl = sil;
			ihr = sir;
#endif
#endif
retry:
#if true//2019.04.17(毛髄検出複線化)
			//G.mlog("髄検出複線化\ricを書き換え？");
#endif
#if false//2019.04.17(毛髄検出複線化)
			if (sil != (ic-ll)) {
				sil  = (ic-ll);
			}
			if (sir != (ic+lr)) {
				sir  = (ic+lr);
			}
#endif
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
#if false//2019.04.17(毛髄検出複線化)
			seg.moz_zpt.Add(upl);//毛髄:上側点
			seg.moz_zpb.Add(upr);//毛髄:下側点
			seg.moz_zpl.Add(uml);//毛髄:長さ径
#endif
			//G.mlog("上の行あとで確認すること");
			//---
#if true//2019.04.17(毛髄検出複線化)
			if (imul == 0) {
#endif
			seg.moz_top.Add(Point.Round(p2));
			seg.moz_btm.Add(Point.Round(p3));
#if true//2019.04.17(毛髄検出複線化)
			}
#endif
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
#if true//2019.04.17(毛髄検出複線化)
				mouz.moz_zpl = uml;
#endif
				//---
				if (true) {
				mouz.avg = get_avg(bf, uil, uir-uil+1);
				mouz.fs2 =(mouz.avg >= G.SS.MOZ_CND_SLVL);
				}
#if true//2018.11.06(毛髄4)
				mouz.ihl = ihl;
				mouz.ihr = ihr;
#endif
#if true//2019.04.17(毛髄検出複線化)
				mouz.moz_zpl = uml;
				mouz.moz_sum =  0;
				mouz.moz_lbl = -1;
				mouz.moz_out = false;
#endif
				//---
#if false//2019.04.17(毛髄検出複線化)
				seg.moz_inf.Add(mouz);
#endif
			}
#if true//2019.04.17(毛髄検出複線化)
			if (imul == 0) {//上側
				seg.moz_inf1.Add(mouz);
				imul+=2;//→下側
				goto retry_of_multi;
			}
			else if (imul == 1) {//中心
				seg.moz_inf2.Add(mouz);
				imul--;//→上側
				goto retry_of_multi;
			}
			else if (imul == 2) {//下側
				seg.moz_inf3.Add(mouz);
				imul++;//→終了
			}
#endif
		}
#endif
		//private double m_offset_of_hair;
		private double m_back_of_x;
		//---
#if true//2018.10.30(キューティクル長)
		public void test_dm(seg_of_hair[] segs, int idx, int cnt, bool bRECALCIR=false)
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
#if false//2019.04.17(毛髄検出複線化)
				segs[idx].moz_zpt.Clear();
				segs[idx].moz_zpb.Clear();
				segs[idx].moz_zpl.Clear();
#endif
				segs[idx].moz_top.Clear();
				segs[idx].moz_btm.Clear();
				segs[idx].moz_inf.Clear();
				//segs[idx].moz_fs2.Clear();//S1,S2区分,S1:true, S2:false
				//segs[idx].moz_avg.Clear();//毛髄:毛髄範囲の画素平均値(生画像による)
#if false//2019.04.17(毛髄検出複線化)
				segs[idx].moz_out.Clear();
				segs[idx].moz_hpt.Clear();//毛髄:上側点:補間後
				segs[idx].moz_hpb.Clear();//毛髄:下側点:補間後
				segs[idx].moz_hpl.Clear();//毛髄:長さ径:補間後
#endif
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
#if true//2019.04.17(毛髄検出複線化)
			for (int i = 0; i < 3; i++) {
				switch (i) {
					case  0:seg.moz_inf = seg.moz_inf1;break;
					case  1:seg.moz_inf = seg.moz_inf2;break;
					default:seg.moz_inf = seg.moz_inf3;break;
				}
#endif
				detect_area(seg);
				detect_outliers(seg);
				interp_outliers(seg);
				sum_avg(seg);
#if true//2019.05.07(毛髄複線面積値対応)
				switch (i) {
					case  0:
						seg.moz_rsl1 = seg.moz_rsl;
						seg.moz_rsl1 = seg.moz_rsd;
						seg.moz_hsl1 = seg.moz_hsl;
						seg.moz_hsd1 = seg.moz_hsd;
					break;
					case  1:
						seg.moz_rsl2 = seg.moz_rsl;
						seg.moz_rsl2 = seg.moz_rsd;
						seg.moz_hsl2 = seg.moz_hsl;
						seg.moz_hsd2 = seg.moz_hsd;
					break;
					default:
						seg.moz_rsl3 = seg.moz_rsl;
						seg.moz_rsl3 = seg.moz_rsd;
						seg.moz_hsl3 = seg.moz_hsl;
						seg.moz_hsd3 = seg.moz_hsd;
					break;
				}
#endif
#if true//2019.04.17(毛髄検出複線化)
				switch (i) {
					case  0:seg.moz_hnf1 = seg.moz_hnf; break;
					case  1:seg.moz_hnf2 = seg.moz_hnf;break;
					default:seg.moz_hnf3 = seg.moz_hnf;break;
				}
			}
#endif
#if true//2019.05.07(毛髄複線面積値対応)
			sum_avg_mult(seg);
#endif
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
		public struct PLS_XYZ {
			public int X, Y, Z;
			public PLS_XYZ(int X, int Y, int Z) {
				this.X = X;
				this.Y = Y;
				this.Z = Z;
			}
		};
#endif
		public struct log_info {
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
		public log_info m_log_info;
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
#if DEBUG
					if (clms[4].Contains("17CR")) {
						buf = buf;
					}
#endif
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
//%%			label06_text = string.Format("x {0:F1}", m_log_info.zoom);
		}
		//---
		public string to_ir_file(string path)
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
		public Image to_img_from_file(string path)
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
		public string to_xx_path(string path, string zpos)
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
		static public void dispose_bmp(ref Bitmap bmp)
		{
			if (bmp != null) {
				bmp.Dispose();
				bmp = null;
			}
			else {
				bmp = bmp;
			}
		}
		static public void dispose_bmp(ref Image img)
		{
			if (img != null) {
				img.Dispose();
				img = null;
			}
			else {
				img = img;
			}
		}
		static public void dispose_img(PictureBox pic)
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
		public void load_bmp(seg_of_hair[] segs, int i, string path_dm1, string path_dm2, string path_ir1, string path_ir2, ref Bitmap bmp_dm0, ref Bitmap bmp_dm1, ref Bitmap bmp_dm2, ref Bitmap bmp_ir0, ref Bitmap bmp_ir1, ref Bitmap bmp_ir2)
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
		private int get_hair_cnt(string pext, string zpos)
		{
			int cnt = 0;
#if true
			for (int q = 0; q < 24; q++) {
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
#endif
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
		public string ZVAL2ORG(string val)
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
		public string ZORG2VAL(string org)
		{
			int idx = m_zpos_org.IndexOf(org);
			if (idx < 0) {
				return("");
			}
			return((string)m_zpos_val[idx]);
		}
		public void sort_zpos()
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
#if false//2019.05.22(再測定判定(キューティクル枚数))
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
		public bool fst_make()
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
#if false//2019.04.01(表面赤外省略)
					//---
					//if (CL_ZP00D.Length > 0) {
						IR_ZP00D = fst_to_ir_file(path, CL_ZP00D);
					//}
					//---
					//---
#endif
					for (int i = 0; i < CL_ZP00D.Length; i++) {
						ArrayList ar_cl_con = new ArrayList();
#if false//2019.04.01(表面赤外省略)
						ArrayList ar_ir_con = new ArrayList();
#endif
						ArrayList ar_cl_bmp_dep = new ArrayList();
#if false//2019.04.01(表面赤外省略)
						ArrayList ar_ir_bmp_dep = new ArrayList();
#endif
						string tmp;
						string[] CL_ZXXD, IR_ZXXD;
						if (true) {
							tmp = System.IO.Path.GetFileName(CL_ZP00D[i]);
							CL_ZXXD = System.IO.Directory.GetFiles(path, tmp.Replace("ZP00D", "Z???D"));
#if false//2019.04.01(表面赤外省略)
							IR_ZXXD = fst_to_ir_file(path, CL_ZXXD);
#endif
						}

						switch (G.SS.MOZ_FST_MODE) {
						case /*CL*/0:
							if (!fst_calc_contrast(CL_ZXXD, ar_cl_con, ar_cl_bmp_dep)) {
								return(false);
							}
#if false//2019.04.01(表面赤外省略)
							if (!fst_calc_contrast(IR_ZXXD, null     , ar_ir_bmp_dep)) {
								return(false);
							}
							ar_ir_con = ar_cl_con;
#endif
						break;
#if false//2019.04.01(表面赤外省略)
						case /*IR*/1:
							if (!fst_calc_contrast(CL_ZXXD, null     , ar_cl_bmp_dep)) {
								return(false);
							}
							if (!fst_calc_contrast(IR_ZXXD, ar_ir_con, ar_ir_bmp_dep)) {
								return(false);
							}
							ar_cl_con = ar_ir_con;
						break;
#endif
						default/*CL,IR*/:
							if (!fst_calc_contrast(CL_ZXXD, ar_cl_con, ar_cl_bmp_dep)) {
								return(false);
							}
#if false//2019.04.01(表面赤外省略)
							if (!fst_calc_contrast(IR_ZXXD, ar_ir_con, ar_ir_bmp_dep)) {
								return(false);
							}
#endif
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
#if false//2019.04.01(表面赤外省略)
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
#endif
						for (int j = 0; j < ar_cl_bmp_dep.Count; j++) {
							Bitmap bmp;
							bmp = (Bitmap)ar_cl_bmp_dep[j];
							bmp.Dispose();
#if false//2019.04.01(表面赤外省略)
							bmp = (Bitmap)ar_ir_bmp_dep[j];
							bmp.Dispose();
#endif
						}
					}
				}
			}
			catch (Exception ex) {
			}
			return(true);
		}
#endif
		//---
		//---
		public void load(G.DLG_VOID_STR SetStatus,
								G.DLG_VOID_VOID call_back01,
								G.DLG_VOID_BMP_BMP_BMP call_back02,
								G.DLG_VOID_STR_STR_INT call_back03,
								G.DLG_VOID_VOID call_back04,
								G.DLG_VOID_VOID call_back05,
								G.DLG_VOID_VOID call_back06,
								G.DLG_VOID_OBJ call_back07,
								G.DLG_VOID_OBJ_STR call_back10,
								G.DLG_VOID_INT_OBJ_BMP_BMP call_back11,
								G.DLG_VOID_OBJ call_back12
			)
		{
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

#if true
			if (G.SS.MOZ_FST_CK00) {
				SetStatus("深度合成中");
#if true//2019.05.22(再測定判定(キューティクル枚数))
				FCS_STK.fst_make(this.MOZ_CND_FOLD, out m_fold_of_dept);
#else
				fst_make();
#endif
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

//%%			enable_forms(false);

			if (true) {
				call_back01();
				G.CAM_PRC = G.CAM_STS.STS_HAIR;
			}
			test_log();
			call_back06();
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
			for (int q = 0; q < 24; q++) {
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
					if (this.bREMES) {
						call_back10(seg, name_dm1);
					}
					//---
					test_pr0(seg, /*b1st=*/(i==0));
					ar_seg.Add(seg);
					if (!this.bREMES) {
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
//						G.CNT_USSD= G.SS.MOZ_BOK_USSD[mode_of_cl];			//表面:標準偏差
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
					call_back07(hr);
//%%					this.listView1.LargeImageList = hr.il_dm;
//%%					this.listView2.LargeImageList = hr.il_ir;
				}
#if true//2019.03.16(NODATA対応)
				if (!this.bREMES) {
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
					SetStatus(string.Format("計算中 {0}/{1}\r{2}/{3}本", i+1, segs.Length, m_hair.Count+1, cnt_of_hair));
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
					if (m_hair.Count == 0 || this.bREMES) {
						call_back02((Bitmap)m_bmp_dm1.Clone(), (Bitmap)m_bmp_ir1.Clone(), (Bitmap)m_bmp_pd1.Clone());
						call_back03(name_dm1, name_ir1, i);
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
						if (this.bREMES) {
							call_back11(mode_of_cl, segs[i], m_bmp_dm1, m_bmp_pd1);
						}
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
				if (this.bREMES) {
					call_back12(segs);
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

				call_back04();

				if (m_hair.Count == 1) {
					call_back05();
				}
			}



			G.CAM_PRC = G.CAM_STS.STS_NONE;
//%%			dlg.Hide();
//%%			dlg.Dispose();
//%%			dlg = null;

			if (true) {
				G.FORM02.Close();
				//G.FORM02.Dispose();
				G.FORM02 = null;
//%%				this.Enabled = true;
			}
			if (true) {
				if (m_hair.Count <= 0) {
					//this.button1.Enabled = this.button3.Enabled = false;
				}
				else {
					if (m_hair[0].seg[0].name_of_ir.Length <= 0) {
//%%						this.radioButton8.Enabled = false;
//%%						this.radioButton8.BackColor = Color.FromArgb(64,64,64);
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
//%%			if (dlg != null) {
#if true//2018.11.22(数値化エラー対応)
//%%				dlg.Hide();
#endif
//%%			    dlg.Dispose();
//%%			    dlg = null;
//%%			}
//%%			this.comboBox8.Tag = null;
#if true//2018.11.22(数値化エラー対応)
			if (G.FORM02 != null) {
				G.FORM02.Close();
				G.FORM02 = null;
			}
//%%			this.Enabled = true;
#endif
		}
		private void init()
		{
			//無し, 3x3, 5x5, 7x7, 9x9, 11x11
			this.MOZ_CND_FTCF = C_FILT_COFS[G.SS.MOZ_CND_FTCF];
			this.MOZ_CND_FTCT = C_FILT_CNTS[G.SS.MOZ_CND_FTCT];
			this.MOZ_CND_SMCF = C_SMTH_COFS[G.SS.MOZ_CND_SMCF];//重み係数=11
			if (this.bREMES) {
			this.MOZ_CND_FOLD = (G.SS.NGJ_CND_FMOD == 0) ? G.SS.AUT_BEF_PATH: G.SS.NGJ_CND_FOLD;
			}
			else {
			this.MOZ_CND_FOLD = (G.SS.MOZ_CND_FMOD == 0) ? G.SS.AUT_BEF_PATH: G.SS.MOZ_CND_FOLD;
			}
			//---
			//---
			//---
			m_map_of_dml = new Dictionary<string,ImageList>();
			m_map_of_irl = new Dictionary<string,ImageList>();
#if true//2018.08.21
			m_map_of_pdl = new Dictionary<string,ImageList>();
#endif
		}

		public void INIT(bool bREMES)
		{
			if (true) {
				this.bREMES = bREMES;
			}
			if (true) {
			}
			init();
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
//%%				this.BeginInvoke(new G.DLG_VOID_VOID(this.load));
			}
#if true//2018.09.29(キューティクルライン検出)
			G.SS.MOZ_CND_HCNT = (G.SS.MOZ_CND_HMAX/G.SS.MOZ_CND_HWID);
#endif
#if true//2018.11.02(HSVグラフ)
//%%			set_hismod();
#endif
		}
		public void TERM()
		{
			if (m_hair != null) {
				m_hair.Clear();
				m_hair = null;
			}
			dispose_bmp(ref m_bmp_dm0);
			dispose_bmp(ref m_bmp_dm1);
			dispose_bmp(ref m_bmp_dm2);
			dispose_bmp(ref m_bmp_ir0);
			dispose_bmp(ref m_bmp_ir1);
			dispose_bmp(ref m_bmp_ir2);
			dispose_bmp(ref m_bmp_pd0);
			dispose_bmp(ref m_bmp_pd1);
			dispose_bmp(ref m_bmp_pd2);

			if (m_map_of_dml != null) {
				m_map_of_dml.Clear();
				m_map_of_dml = null;
			}
			if (m_map_of_irl != null) {
				m_map_of_irl.Clear();
				m_map_of_irl = null;
			}
			if (m_map_of_pdl != null) {
				m_map_of_pdl.Clear();
				m_map_of_pdl = null;
			}
			if (m_zpos_org != null) {
				m_zpos_org.Clear();
				m_zpos_org = null;
			}
			if (m_zpos_val != null) {
				m_zpos_val.Clear();
				m_zpos_val = null;
			}
			if (m_log_info.map_of_xyz != null) {
				m_log_info.map_of_xyz.Clear();
				m_log_info.map_of_xyz = null;
			}
			if (m_log_info.map_of_pos != null) {
				m_log_info.map_of_pos.Clear();
				m_log_info.map_of_pos = null;
			}
			GC.Collect();
		}

		public double TO_VAL(object obj)
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

		public void prep_image_list(hair hr, int hidx, ref ImageList il_dm, ref ImageList il_pd, ref ImageList il_ir, string zpos_dan, string zpos_kei, string zpos_mou)
		{
			string kh = hidx.ToString();
			Dictionary<string, ImageList>[] maps = { m_map_of_dml, m_map_of_pdl, m_map_of_irl };
			ImageList il;
			ImageList[] ils = {null, null, null};
			string[] zary = { null, null, null};

			zary[0] = ZVAL2ORG(zpos_dan); //danmen
			zary[1] = ZVAL2ORG(zpos_kei); //kei
			zary[2] = ZVAL2ORG(zpos_mou); //mouzui

			for (int i = 0; i < 3; i++) {
				if (!maps[i].TryGetValue(kh + zary[i], out il)) {
					il = null;//ng
				}
				ils[i] = il;
			}
			if (ils[0] == null || ils[1] == null || ils[2] == null) {
				var dlg = new DlgProgress();
				dlg.Show("@", G.FORM01);
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
#if true//2019.06.03(バンドパス・コントラスト値対応)
		static
#endif
		public bool calc_filter_coeff()
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
		private void find_cuticle_line(ArrayList apts, ArrayList afil, out ArrayList acut, out LIST_U8 aflg, out List<int> ahis)
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
#if true//2019.01.11(混在対応)
		public void SWAP_ANL_CND(int mode)
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
#if false
//%%#if true//2018.09.29(キューティクルライン検出)
		public void UPDATE_CUTICLE()
		{//キューティクル・フィルター処理
//%%			if (this.listView1.Items.Count <= 0) {
//%%				return;
//%%			}
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
//%%			draw_image(hr);
//%%			draw_cuticle(hr);
		}
#endif
#if false
#if true//2018.10.10(毛髪径算出・改造)
		private void UPDATE_BY_FILES(int mode)
		{
//%%			if (this.listView1.Items.Count <= 0) {
//%%				return;
//%%			}
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

//%%			draw_graph(hr);
//%%			draw_image(hr);
#if true//2018.11.02(HSVグラフ)
//%%			draw_hsv(hr);
#endif
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
#endif

#endif
#endif

#if true//2018.11.02(HSVグラフ)
//%%		private
//%%		const int ETC_HIS_MODE = 1;
//%%		private
//%%		Chart[] m_cht_his = null;
//%%		private
//%%		Color[] m_col_of_hue = new Color[180];

//%%		private void set_hismod()
//%%		{
//%%		}
		public void calc_hist(seg_of_hair seg)
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
