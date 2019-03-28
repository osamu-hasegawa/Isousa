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
	public partial class Form04 : Form
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
		public Form04()
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
			//カラー画像と
			//赤外画像による情報
			public int	 cnt_of_val;
			//---
			public int	 cnt_of_pos;
			public ArrayList	 pts_cen;
#if false//2019.02.16(数値化白髪オフセット)
			public ArrayList	 pts_p5u;
			public ArrayList	 pts_phf;
			public ArrayList	 pts_mph;
			public ArrayList	 pts_m5u;
#endif
			//---
			public ArrayList	mou_len;//毛髪・径
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
			public Point[]	han_top;
			public Point[]	han_btm;
#endif
			public int		dia_cnt;//輪郭・頂点数
			//---
			public seg_of_hair() {
				this.cnt_of_val = 0;
				//--
				this.cnt_of_pos = 0;
				//this.pts_x = null;
				this.pts_cen =  new ArrayList();
				//---
				this.mou_len = new ArrayList();
				//---
				//赤外情報
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
		}
		//private double m_offset_of_hair;
		private double m_back_of_x;
		//---
#if true//2018.10.30(キューティクル長)
		private void test_dm(seg_of_hair[] segs, int idx, int cnt, bool bRECALCIR=false)
		{
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
						//pf = scan_pt(m_fc, ref ii, sta_of_pf, -ds);
						//if (pf.X < 0) {
							break;
						//}
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


			}

			for (int i = 0; i < (LMAX*2+1); i++) {
			}
			m_back_of_x = pf.X;
		}
#endif
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
			case  0:zpos = "ZP00D"; break;
			case  1:zpos = "KP00D"; break;
			default:zpos = "ZP00D"; path = to_ir_file(path); break;
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

			dlg.Show("画像判定", G.FORM01);
			G.bCANCEL = false;
			if (true) {
				zpos = "_" + zpos;
			}

			enable_forms(false);

			if (true) {
				this.textBox1.Text = this.MOZ_CND_FOLD;
				//文字列の後方を表示させる
				this.textBox1.SelectionStart = this.textBox1.Text.Length;
				G.CAM_PRC = G.CAM_STS.STS_HAIR;
			}
			test_log();
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
				files_dm = files_cl;
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
					//---
					test_pr0(seg, /*b1st=*/(i==0));
					ar_seg.Add(seg);
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
						//this.listView1.Items.Add(name_dm1, i);
						//this.listView2.Items.Add(name_ir1, i);
						//this.listView1.Items[i].EnsureVisible();
						//this.listView1.Update();
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
						this.tabControl1.SelectedIndex = 0;
					}
					if (false) {
					}
					else if (true) {
						//---
						object obj = m_bmp_dm1.Tag;
						m_bmp_dm1.Tag = null;
G.CAM_PRC = G.CAM_STS.STS_HIST;
						G.FORM02.load_file(m_bmp_pd1/*m_bmp_dm1*/, false);
						m_bmp_dm1.Tag = obj;
						//---
					}
					else {/*赤外*/
						//カラー固定のため(G.SS.MOZ_CND_PDFL == 0)ここは通らない
						throw new Exception("Internal Error");
					}
					if (G.IR.CIR_CNT > 0) {
						//if (m_bmp_ir1 != null && G.SS.MOZ_CND_FTCF > 0) {
							//Form02.DO_SMOOTH(m_bmp_ir1, this.MOZ_CND_FTCF, this.MOZ_CND_FTCT);
						//}
						test_pr1(segs[i]);
						if (m_dia_cnt > 1) {
							//test_dm(segs, i, segs.Length);
						}
					}
					else if (true) {
G.CAM_PRC = G.CAM_STS.STS_HIST;
						G.FORM02.load_file(m_bmp_dm1/*m_bmp_dm1*/, false);
					}
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
					//if (this.radioButton7.Checked) {
					//int isel = ((hair)m_hair[0]).cnt_of_seg / 2;
					//this.listView1.Items[isel].Selected = true;
					//this.listView1.Items[isel].EnsureVisible();
					//}
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
						//this.radioButton8.Enabled = false;
						//this.radioButton8.BackColor = Color.FromArgb(64,64,64);
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
			//this.comboBox8.Tag = null;
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
			//---
			this.pictureBox1.Dock = DockStyle.Fill;
			this.pictureBox2.Dock = DockStyle.Fill;
			this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			this.pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
			}
#if true//2018.08.21
			this.pictureBox3.Dock = DockStyle.Fill;
			this.pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
#endif
			//---
			//this.listView1.Dock = DockStyle.Fill;
			//this.listView2.Visible = false;
			//this.listView2.Dock = DockStyle.Fill;
			//---
//			this.radioButton1.Enabled = !this.radioButton1.Enabled;
//			this.radioButton1.Enabled = false;
			//if (this.radioButton1.Enabled) {
			//    this.radioButton1.BackColor = Color.Black;
			//}
			//else {
			//    this.radioButton1.ForeColor = Color.White;
			//    this.radioButton1.BackColor = Color.FromArgb(64,64,64);
			//}
			this.comboBox1.Enabled = false;
#if false//2018.11.10(保存機能)
			this.comboBox2.SelectedIndex = 1;
			this.comboBox2.Enabled = true;
#endif
#if true//2018.11.10(保存機能)
			this.tabControl1.SelectedIndex = 2;//キューティクル間隔
#endif
			//---
			//無し, 3x3, 5x5, 7x7, 9x9, 11x11
			this.MOZ_CND_FTCF = C_FILT_COFS[G.SS.MOZ_CND_FTCF];
			this.MOZ_CND_FTCT = C_FILT_CNTS[G.SS.MOZ_CND_FTCT];
			this.MOZ_CND_SMCF = C_SMTH_COFS[G.SS.MOZ_CND_SMCF];//重み係数=11
			this.MOZ_CND_FOLD = (G.SS.NGJ_CND_FMOD == 0) ? G.SS.AUT_BEF_PATH: G.SS.NGJ_CND_FOLD;
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

		private void Form04_Load(object sender, EventArgs e)
		{
			if (true) {
				this.SetDesktopBounds(G.AS.APP_F02_LFT, G.AS.APP_F02_TOP, G.AS.APP_F02_WID, G.AS.APP_F02_HEI);
			}
			if (true) {
				if (G.UIF_LEVL == 0) {
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
			G.FORM04 = null;
			G.FORM12.UPDSTS();
		}
		private void listView2_SelectedIndexChanged(object sender, EventArgs e)
		{
			//if (this.radioButton8.Checked) {
			//    ListView lv = (ListView)sender;
			//    if (lv.SelectedItems.Count != 1) {
			//        return;
			//    }
			//    int isel = lv.SelectedItems[0].Index;
			//    this.listView1.Items[isel].Selected = true;
			//}
		}
		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			//ListView lv = (ListView)sender;
			//if (lv.SelectedItems.Count != 1) {
			//    return;
			//}
			//if (m_isel != lv.SelectedItems[0].Index) {
			//    m_isel = lv.SelectedItems[0].Index;
			//}
			//if (m_i >= m_hair.Count) {
			//    return;
			//}
			//hair hr = (hair)m_hair[m_i];
			//if (m_isel >= hr.seg.Count()) {
			//    return;
			//}
			//if (this.radioButton4.Checked) {
			//    draw_image(hr);
			//}
			//if (this.radioButton7.Checked) {
			//    if (m_isel < this.listView2.Items.Count) {
			//        this.listView2.Items[m_isel].Selected = true;
			//    }
			//}
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
#if true
			return;
#else
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
#endif
		}
#if true//2018.09.29(キューティクルライン検出)
		private void draw_marker(Graphics gr, Brush brs, Point pt, int LEN)
		{
			gr.FillRectangle(brs, pt.X-LEN, pt.Y-LEN, LEN*2+1, LEN*2+1);
		}
#endif
#if true//2018.10.10(毛髪径算出・改造)
#if true//2018.10.27(画面テキスト)
		private void draw_text(Image img, string txt, float fp=60)
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
			sf.Alignment = StringAlignment.Far;
			sf.LineAlignment = StringAlignment.Far;

			gr.DrawString(txt, fnt, Brushes.LimeGreen, rt, sf);
			gr.Dispose();
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
			if (false /*this.radioButton3.Checked*/) {
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
				if (false/*this.radioButton3.Checked*/) {
					//全体表示
					if (q >= hr.seg.Length) {
						break;
					}
					seg = (seg_of_hair)hr.seg[q];
					if (seg == null) {
						continue;
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
						buf_dm = to_xx_path(seg.path_of_dm, null/*ZVAL2ORG(this.comboBox10.Text)*/);
						buf_pd = to_xx_path(seg.path_of_pd, null/*ZVAL2ORG(this.comboBox8.Text)*/);
						buf_ir = to_xx_path(seg.path_of_ir, null/*ZVAL2ORG(this.comboBox12.Text)*/);
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

#if true//2018.10.27(画面テキスト)
				gi_cnt++;
#endif
				if (true/*!this.radioButton3.Checked*/) {
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
			if (false/*this.radioButton3.Checked*/) {
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
		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{//対象毛髪の変更
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
		}
#endif
#if true//2018.10.10(毛髪径算出・改造)
		private void UPDATE_BY_FILES(int mode)
		{
			//if (this.listView1.Items.Count <= 0) {
			//    return;
			//}
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
						//calc_hist(segs[i]);
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

			draw_image(hr);
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
#endif
	}
}
