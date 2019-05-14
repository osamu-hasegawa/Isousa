using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//---
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace uSCOPE
{

	public class OCV
	{
		public enum IMG {
			IMG_A = 0,
			IMG_G = 1,
			IMG_B = 2,
			IMG_M = 3,
			IMG_H = 4,
			IMG_RGB_R = 5,
			IMG_RGB_G = 6,
			IMG_RGB_B = 7,
			IMG_HSV_H = 8,
			IMG_HSV_S = 9,
			IMG_HSV_V =10,
			IMG_F = 11,
			IMG_D = 12,
			IMG_T = 13,
		};
		/*
		 * CAM_PRC:   0      1(HIST)      2(HAIR)    4(AF:CNTRA)   4(AF:HAIR)
		 * img_g  :          Y            Y          Y             Y
		 * img_b  :          Y            Y          Y             Y
		 * img_rgb:          Y(CND_H=0)   N          N             N
		 *                   Y(HISMD=0)
		 * img_hsv:          Y(CND_H=1)   Y(CND_H=1) Y(COD_H=1)    Y(COD_H=1)
		 *                   Y(HISMD=1)
		 */
		public struct RECT { public Int32 Left; public Int32 Top; public Int32 Right; public Int32 Bottom; }
		public struct POINT { public Int32 x; public Int32 y;}

#if _X64//2018.12.22(測定抜け対応)
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_TERM")]			public static extern void	TERM();
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_RESET_MASK")]		public static extern void	RESET_MASK(Int32 x, Int32 y, Int32 w, Int32 h);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_RESET")]			public static extern void	RESET(Int32 wid, Int32 hei);//, Int32 mx, Int32 my, Int32 mw, Int32 mh);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_SET_IMG")]			public static extern Int32	SET_IMG(IntPtr ptr, Int32 wid, Int32 hei, Int32 str, Int32 bpp);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_GET_IMG")]			public static extern Int32	GET_IMG(IntPtr ptr, Int32 wid, Int32 hei, Int32 str, Int32 bpp);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_TO_GRAY")]			public static extern void	TO_GRAY(Int32 I, Int32 H);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_TO_HSV")]			public static extern void	TO_HSV(Int32 I, Int32 H);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_MERGE")]			public static extern void	MERGE(Int32 H1, Int32 H2, Int32 H3, Int32 I);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_SPLIT")]			public static extern void	SPLIT(Int32 I, Int32 H1, Int32 H2, Int32 H3);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_SMOOTH")]			public static extern void	SMOOTH(Int32 I, Int32 cof);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_THRESH_BIN")]		public static extern void	THRESH_BIN(Int32 I, Int32 H, Int32 thval, Int32 inv);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_THRESH_HSV")]		public static extern void	THRESH_HSV(Int32 I1, Int32 I2, Int32 I3, Int32 H, Int32 minh, Int32 maxh, Int32 mins, Int32 maxs, Int32 minv, Int32 maxv);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_CAL_HIST")]		public static extern void	CAL_HIST(Int32 I, Int32 bMASK, ref double pval, out double pmin, out double pmax, out double pavg);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_PUTTEXT")]			public static extern void	PUTTEXT(Int32 I, string buf, Int32 x, Int32 y, Int32 c);
		/*
		抽出モード:
				0:CV_RETR_EXTERNAL:最も外側の輪郭のみ抽出
				1:CV_RETR_LIST	:全ての輪郭を抽出し，リストに追加
				2:CV_RETR_CCOMP	:全ての輪郭を抽出し，二つのレベルを持つ階層構造を構成する．
								:1番目のレベルは連結成分の外側の境界線，
								:2番目のレベルは穴（連結成分の内側に存在する）の境界線．
				3:CV_RETR_TREE	:全ての輪郭を抽出し，枝分かれした輪郭を完全に表現する階層構造を構成する．
		 */
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_FIND_FIRST")]		public static extern void	FIND_FIRST(Int32 I, Int32 mode);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_FIND_NEXT")]		public static extern IntPtr	FIND_NEXT(IntPtr pos, Int32 smax, Int32 smin, Int32 lmax, Int32 lmin, double cmax, double cmin, out double ps, out double pl, out double pc);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_FIND_TERM")]		public static extern void	FIND_TERM();
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_DRAW_CONTOURS")]	public static extern void	DRAW_CONTOURS(Int32 I, IntPtr pos, Int32 c1, Int32 c2);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_DRAW_CONTOURS2")]	public static extern void	DRAW_CONTOURS2(Int32 I, IntPtr pos, Int32 c1, Int32 c2, Int32 thickness);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_CONTOURS_CNT")]	public static extern Int32	CONTOURS_CNT(IntPtr pos);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_CONTOURS_PTS")]	public static extern void	CONTOURS_PTS(IntPtr pos, Int32 idx, out POINT p);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_FIT_LINE")]		public static extern void	FIT_LINE(IntPtr pos, out float f);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_APPROX_PTS")]		public static extern Int32	APPROX_PTS(IntPtr pos, Int32 bSIGNE, Int32 PREC);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_GET_PTS")]			public static extern void	GET_PTS(Int32 idx, out POINT p);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_DRAW_LINE")]		public static extern void	DRAW_LINE(Int32 I, ref POINT p1, ref POINT p2, Int32 c, Int32 thick);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_DRAW_RECT")]		public static extern void	DRAW_RECT(Int32 I, ref RECT pr, Int32 c, Int32 thickness);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_BOUNDING_RECT")]	public static extern void	BOUNDING_RECT(IntPtr pos, out RECT pr);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_DRAW_TEXT")]		public static extern void	DRAW_TEXT(Int32 I, Int32 x, Int32 y, string buf, Int32 c);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_MIN_AREA_RECT2")]	public static extern void	MIN_AREA_RECT2(IntPtr pos, out POINT p1, out POINT p2, out POINT p3, out POINT p4);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_FILL_POLY")]		public static extern void	FILL_POLY(Int32 I, ref POINT p, Int32 n, Int32 c);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_ZERO")]			public static extern void	ZERO(Int32 I);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_SOBEL")]			public static extern void	SOBEL(Int32 I, Int32 H, Int32 xorder, Int32 yorder, Int32 apert_size);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_LAPLACE")]			public static extern void	LAPLACE(Int32 I, Int32 H, Int32 apert_size);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_CANNY")]			public static extern void	CANNY(Int32 I, Int32 H, double th1, double th2, Int32 apert_size);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_MINMAX")]			public static extern void	MINMAX(Int32 I, ref double pmin, ref double pmax);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_SCALE")]			public static extern void	SCALE(Int32 I, Int32 H, double scale, double shift);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_SMOOTH2")]			public static extern void	SMOOTH2(Int32 I, Int32 cof, double sig1, double sig2);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_DIFF")]			public static extern void	DIFF(Int32 I, Int32 H, Int32 J);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_TO_01")]			public static extern void	TO_01(Int32 I, Int32 ZERO_VAL, Int32 NONZERO_VAL);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_THINNING")]		public static extern void	THINNING(Int32 I, Int32 H, Int32 cnt);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_COPY")]			public static extern void	COPY(Int32 I, Int32 H);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_NOT")]				public static extern void	NOT(Int32 I, Int32 H);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_ERODE")]			public static extern void	ERODE(Int32 I, Int32 H, Int32 kernel_size, Int32 cnt);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_DILATE")]			public static extern void	DILATE(Int32 I, Int32 H, Int32 kernel_size, Int32 cnt);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_MINMAX_ROI")]		public static extern void	MINMAX_ROI(Int32 I, Int32 x, Int32 y, Int32 w, Int32 h, ref Int32 pmin, ref Int32 pmax);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_AND")]				public static extern void	AND(Int32 I, Int32 H, Int32 J);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_SAVE")]			public static extern void	SAVE(Int32 I, string file);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_FIND_EDGE")]		public static extern void	FIND_EDGE(Int32 I, ref RECT pr, Int32 wid_per, Int32 cnt, ref POINT ptop, ref POINT pbtm);
		[DllImport("IMGSUB64.DLL", EntryPoint = "OCV_FIT_LINE")]		public static extern Int32	FIT_LINE(ref POINT pl, Int32 pcnt, Int32 type, double param, double reps, double aeps, out float pf);
#else
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_TERM")]			public static extern void	TERM();
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_RESET_MASK")]		public static extern void	RESET_MASK(Int32 x, Int32 y, Int32 w, Int32 h);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_RESET")]			public static extern void	RESET(Int32 wid, Int32 hei);//, Int32 mx, Int32 my, Int32 mw, Int32 mh);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_SET_IMG")]			public static extern Int32	SET_IMG(IntPtr ptr, Int32 wid, Int32 hei, Int32 str, Int32 bpp);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_GET_IMG")]			public static extern Int32	GET_IMG(IntPtr ptr, Int32 wid, Int32 hei, Int32 str, Int32 bpp);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_TO_GRAY")]			public static extern void	TO_GRAY(Int32 I, Int32 H);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_TO_HSV")]			public static extern void	TO_HSV(Int32 I, Int32 H);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_MERGE")]			public static extern void	MERGE(Int32 H1, Int32 H2, Int32 H3, Int32 I);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_SPLIT")]			public static extern void	SPLIT(Int32 I, Int32 H1, Int32 H2, Int32 H3);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_SMOOTH")]			public static extern void	SMOOTH(Int32 I, Int32 cof);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_THRESH_BIN")]		public static extern void	THRESH_BIN(Int32 I, Int32 H, Int32 thval, Int32 inv);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_THRESH_HSV")]		public static extern void	THRESH_HSV(Int32 I1, Int32 I2, Int32 I3, Int32 H, Int32 minh, Int32 maxh, Int32 mins, Int32 maxs, Int32 minv, Int32 maxv);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_CAL_HIST")]		public static extern void	CAL_HIST(Int32 I, Int32 bMASK, ref double pval, out double pmin, out double pmax, out double pavg);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_PUTTEXT")]			public static extern void	PUTTEXT(Int32 I, string buf, Int32 x, Int32 y, Int32 c);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_FIND_FIRST")]		public static extern void	FIND_FIRST(Int32 I, Int32 mode);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_FIND_NEXT")]		public static extern IntPtr	FIND_NEXT(IntPtr pos, Int32 smax, Int32 smin, Int32 lmax, Int32 lmin, double cmax, double cmin, out double ps, out double pl, out double pc);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_FIND_TERM")]		public static extern void	FIND_TERM();
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_DRAW_CONTOURS")]	public static extern void	DRAW_CONTOURS(Int32 I, IntPtr pos, Int32 c1, Int32 c2);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_DRAW_CONTOURS2")]	public static extern void	DRAW_CONTOURS2(Int32 I, IntPtr pos, Int32 c1, Int32 c2, Int32 thickness);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_CONTOURS_CNT")]	public static extern Int32	CONTOURS_CNT(IntPtr pos);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_CONTOURS_PTS")]	public static extern void	CONTOURS_PTS(IntPtr pos, Int32 idx, out POINT p);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_APPROX_PTS")]		public static extern Int32	APPROX_PTS(IntPtr pos, Int32 bSIGNE, Int32 PREC);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_GET_PTS")]			public static extern void	GET_PTS(Int32 idx, out POINT p);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_DRAW_LINE")]		public static extern void	DRAW_LINE(Int32 I, ref POINT p1, ref POINT p2, Int32 c, Int32 thick);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_DRAW_RECT")]		public static extern void	DRAW_RECT(Int32 I, ref RECT pr, Int32 c, Int32 thickness);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_BOUNDING_RECT")]	public static extern void	BOUNDING_RECT(IntPtr pos, out RECT pr);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_DRAW_TEXT")]		public static extern void	DRAW_TEXT(Int32 I, Int32 x, Int32 y, string buf, Int32 c);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_MIN_AREA_RECT2")]	public static extern void	MIN_AREA_RECT2(IntPtr pos, out POINT p1, out POINT p2, out POINT p3, out POINT p4);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_FILL_POLY")]		public static extern void	FILL_POLY(Int32 I, ref POINT p, Int32 n, Int32 c);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_ZERO")]			public static extern void	ZERO(Int32 I);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_SOBEL")]			public static extern void	SOBEL(Int32 I, Int32 H, Int32 xorder, Int32 yorder, Int32 apert_size);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_LAPLACE")]			public static extern void	LAPLACE(Int32 I, Int32 H, Int32 apert_size);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_CANNY")]			public static extern void	CANNY(Int32 I, Int32 H, double th1, double th2, Int32 apert_size);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_MINMAX")]			public static extern void	MINMAX(Int32 I, ref double pmin, ref double pmax);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_SCALE")]			public static extern void	SCALE(Int32 I, Int32 H, double scale, double shift);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_SMOOTH2")]			public static extern void	SMOOTH2(Int32 I, Int32 cof, double sig1, double sig2);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_DIFF")]			public static extern void	DIFF(Int32 I, Int32 H, Int32 J);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_TO_01")]			public static extern void	TO_01(Int32 I, Int32 ZERO_VAL, Int32 NONZERO_VAL);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_THINNING")]		public static extern void	THINNING(Int32 I, Int32 H, Int32 cnt);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_COPY")]			public static extern void	COPY(Int32 I, Int32 H);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_NOT")]				public static extern void	NOT(Int32 I, Int32 H);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_ERODE")]			public static extern void	ERODE(Int32 I, Int32 H, Int32 kernel_size, Int32 cnt);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_DILATE")]			public static extern void	DILATE(Int32 I, Int32 H, Int32 kernel_size, Int32 cnt);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_MINMAX_ROI")]		public static extern void	MINMAX_ROI(Int32 I, Int32 x, Int32 y, Int32 w, Int32 h, ref Int32 pmin, ref Int32 pmax);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_AND")]				public static extern void	AND(Int32 I, Int32 H, Int32 J);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_SAVE")]			public static extern void	SAVE(Int32 I, string file);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_FIND_EDGE")]		public static extern void	FIND_EDGE(Int32 I, ref RECT pr, Int32 wid_per, Int32 cnt, ref POINT ptop, ref POINT pbtm);
		[DllImport("IMGSUB32.DLL", EntryPoint = "OCV_FIT_LINE")]		public static extern Int32	FIT_LINE(ref POINT pl, Int32 pcnt, Int32 type, double param, double reps, double aeps, out float pf);
#endif
		public static int PF2BPP(PixelFormat pf)
		{
			int bpp;
			switch (pf) {
			case PixelFormat.Indexed:
			case PixelFormat.Format8bppIndexed:
				bpp = 8;
				break;
			case PixelFormat.Format24bppRgb:
				bpp = 24;
				break;
			case PixelFormat.Format32bppRgb:
			case PixelFormat.Format32bppPArgb:
			case PixelFormat.Format32bppArgb:
				bpp = 32;
				break;
			default:
				bpp = 8;
				break;
			}
			return(bpp);
		}
	}
}