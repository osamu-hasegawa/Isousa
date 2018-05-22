using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
//---
using System.Drawing.Imaging;
using System.Reflection;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;
//---
using Basler.Pylon;

#pragma warning disable 1717//(同じ変数に割り当てられました。他の変数に割り当てますか?)

namespace uSCOPE
{

	public partial class Form02:Form
	{
		private 
		const bool C_SELFCONT = true;
		private 
		const int C_HANKEI = 3;
		//---
		//private int[] m_zoomLevel = { 25, 50, 75, 100, 125, 150, 200 };
		//private int m_zoom = 3;
		//---
		private class CALC_LEN {
			//pt[0],pt[1]:直線
			//pt[2]:直線と直交する線上の点
			//pt[3]:直線上の交点
			Point[] pt_of_bmp;
			bool	flag;		//再計算フラグ
			double	len;		//長さ
			//---
			int		width, height;
			//---
			public CALC_LEN() {
				this.pt_of_bmp = new Point[4];
				this.flag = true;
				this.len = 0.0;
				this.width = this.height = -1;
			}
			public void reset(int width, int height) {
				if (width != this.width || height != this.height) {
					int ox =  width/2, oy =  height/2;
					this.width = width;
					this.height = height;
					this.flag = true;
					this.pt_of_bmp[0].X = ox-100;
					this.pt_of_bmp[0].Y = oy-100;
					this.pt_of_bmp[1].X = ox+100;
					this.pt_of_bmp[1].Y = oy+100;
					this.pt_of_bmp[2].X = ox-100;
					this.pt_of_bmp[2].Y = oy+100;
					get_len();
				}
			}
			public void set_pt(int i, Point pt) {
				this.pt_of_bmp[i] = pt;
				flag = true;
			}
			public Point get_pt(int i) {
				return(this.pt_of_bmp[i]);
			}
			public double get_len() {
				if (flag) {
					FN1D f1 = new FN1D(this.pt_of_bmp[0], this.pt_of_bmp[1]);
					FN1D f2 = f1.GetNormFn(this.pt_of_bmp[2]);
					PointF pt = f1.GetCrossPt(f2);
					if (f1.valid == false || f2.valid == false) {
						len = 0.0;
					}
					//else if (double.IsNaN(f1.A) || double.IsNaN(f2.A) || double.IsNaN(pt.X) || double.IsNaN(pt.Y)) {
					//    len = 0.0;
					//}
					else {
						len = G.diff((PointF)this.pt_of_bmp[2], pt);
					}
					this.pt_of_bmp[3] = Point.Round(pt);
					flag = false;
					//再計算
				}
				return(this.len);
			}
		};
		//---
		private Camera m_camera = null;
		private PixelDataConverter m_converter = new PixelDataConverter();
		private Stopwatch m_stopWatch = new Stopwatch();
		//---
		private ICameraInfo m_caminfo;
		private int m_fid;
		private double m_fps = double.NaN;
		private int m_fps_tk1;
		private int m_fps_tk2;
		private int m_fps_cnt;
		const
		int m_fps_wid = 10;
		//int m_sts_x, m_sts_y;
		string m_sts_xy;
		string m_sts_rgb;
		string m_sts_hsv;
		//string m_sts_rt;
		//---
		private Bitmap m_bmpR;
		//private Bitmap m_bmpG;
		//private Bitmap m_bmpB;
		private Bitmap m_bmpZ;
		private int m_width, m_height;
		//---
		private bool m_bcontinuous = false;
		//---
		//IplImage m_img_a = null;//rgb
		//IplImage m_img_h = null;//hsv
		//IplImage[] m_img_rgb = { null, null, null };//rgb
		//IplImage[] m_img_hsv = { null, null, null };//hsv
		//IplImage m_img_g = null;
		//IplImage m_img_m = null;
		//IplImage m_img_b = null;
		//CvFont fnt = null;
		//AutoResetEvent m_event = new AutoResetEvent(false);
		bool m_bproc = false;
		int	m_tk1, m_tk2, m_tk3;
		string m_filename = null;
		//---
		Chart[] m_cht_dan = null;
		Chart[] m_cht_his = null;
		Color[] m_col_of_hue = new Color[180];
		//---
		private int m_size_mode;
		private int m_size_regi;
		private int m_size_xo, m_size_yo;
		private bool m_bNeedToTakeImgBounds = true;
		private int m_chk1;
		private int m_chk2=0;
		private int m_chk3=0;
		private int HIS_PAR1 = 0;
		private Point[] m_pt_of_marker = new Point[9];//2点間断面(0,1), コントラスト矩形(2,3), 距離(4,5,6)
		private int m_mouse_icap=-1;
		private CALC_LEN m_calc_len = new CALC_LEN();
		//---
		public Form02()
		{
			InitializeComponent();
		}
		public enum CAM_PARAM
		{
			GAMMA,
			CONTR,
			BRIGH,
			SHARP,
			WIDTH,
			HEIGHT,
			EXPOSURE,
			GAIN,
			BAL_SEL,
			BALANCE,
			WHITE,
			OFFSETX,
			OFFSETY
		};
		public bool isCONNECTED()
		{
			if (G.AS.DEBUG_MODE == 1) {
				return(true);
			}
			return (m_camera != null);
		}
		public bool isLOADED()
		{
			return (!string.IsNullOrEmpty(m_filename));
		}
		public void UPDATE_PROC()
		{
			post_proc();
			disp_bmp(true);
		}
		private void cv_init()
		{
#if true
			OCV_RESET(m_width, m_height);
#else
			cv_term();
			m_img_a = Cv.CreateImage(new CvSize(m_width, m_height), BitDepth.U8, 3);
			m_img_h = Cv.CreateImage(new CvSize(m_width, m_height), BitDepth.U8, 3);
			for (int i = 0; i < 3; i++) {
				m_img_rgb[i] = Cv.CreateImage(new CvSize(m_width, m_height), BitDepth.U8, 1);
				m_img_hsv[i] = Cv.CreateImage(new CvSize(m_width, m_height), BitDepth.U8, 1);
			}
			m_img_g = Cv.CreateImage(new CvSize(m_width, m_height), BitDepth.U8, 1);
			m_img_b = Cv.CreateImage(new CvSize(m_width, m_height), BitDepth.U8, 1);
			m_img_m = Cv.CreateImage(new CvSize(m_width, m_height), BitDepth.U8, 1);
			//---
			if (fnt != null) {
				fnt.Dispose();
				fnt = null;
			}
			if (fnt == null) {
				double hscale = 3.2 * m_width / 2592.0;
				double vscale = 3.2 * m_height / 1944.0;
				int thickness = (int)(4 * m_width / 2592.0 + 0.5);
				if (thickness < 1) {
					thickness = 1;
				}
				Cv.InitFont(out fnt, FontFace.HersheyComplex, hscale, vscale, 1.0, thickness);
				//Cv.InitFont(out fnt, FontFace.HersheySimplex, 4.0, 4.0, 1.0, 4);
				//Cv.InitFont(out fnt, FontFace.HersheyDuplex, 4.0, 4.0, 1.0, 4);
				//Cv.InitFont(out fnt, FontFace.HersheyPlain, 3.5, 3.5, 1.0, 4);
				//Cv.InitFont(out fnt, FontFace.HersheyTriplex, 3.5, 3.5, 1.0, 4);
				//Cv.InitFont(out fnt, FontFace.HersheyScriptComplex, 3.5, 3.5, 1.0, 4);
				/*
				CV_FONT_HERSHEY_SIMPLEX 普通サイズの sans-serif フォント
			CV_FONT_HERSHEY_PLAIN 小さいサイズの sans-serif フォント
			CV_FONT_HERSHEY_DUPLEX 普通サイズの sans-serif フォント（ CV_FONT_HERSHEY_SIMPLEX よりも複雑）
			CV_FONT_HERSHEY_COMPLEX 普通サイズの serif フォント
			CV_FONT_HERSHEY_TRIPLEX 普通サイズの serif フォント（ CV_FONT_HERSHEY_COMPLEX よりも複雑）
			CV_FONT_HERSHEY_COMPLEX_SMALL CV_FONT_HERSHEY_COMPLEX の小さいサイズ版
			CV_FONT_HERSHEY_SCRIPT_SIMPLEX 手書きスタイルのフォント
			CV_FONT_HERSHEY_SCRIPT_COMPLEX CV_FONT_HERSHEY_SCRIPT_SIMPLEX 
				 */
			}
#endif
			reset_mask_rect();
		}
		private void cv_term()
		{
#if true
			OCV_TERM();
#else
			if (m_img_a != null) {
				Cv.ReleaseImage(m_img_a);
				m_img_a.Dispose();
				m_img_a = null;
			}
			if (m_img_h != null) {
				Cv.ReleaseImage(m_img_h);
				m_img_h.Dispose();
				m_img_h = null;
			}
			for (int i = 0; i < 3; i++) {
				if (m_img_rgb[i] != null) {
					Cv.ReleaseImage(m_img_rgb[i]);
					m_img_rgb[i].Dispose();
					m_img_rgb[i] = null;
				}
				if (m_img_hsv[i] != null) {
					Cv.ReleaseImage(m_img_hsv[i]);
					m_img_hsv[i].Dispose();
					m_img_hsv[i] = null;
				}
			}
			if (m_img_g != null) {
				Cv.ReleaseImage(m_img_g);
				m_img_g.Dispose();
				m_img_g = null;
			}
			if (m_img_b != null) {
				Cv.ReleaseImage(m_img_b);
				m_img_b.Dispose();
				m_img_b = null;
			}
			if (m_img_m != null) {
				Cv.ReleaseImage(m_img_m);
				m_img_m.Dispose();
				m_img_m = null;
			}
#endif
		}
		//private int m_mode_layout;
		public void set_layout(/*int i*/)
		{
			if (this.checkBox1.Checked) {
				this.groupBox1.Visible = true;
				if (true) {
					if (m_rtDanImg.Width <= 0 && m_rtDanImg.Height <= 0) {
						int GAP = 100;
						m_rtDanBmp.X = m_width/2-GAP/2;
						m_rtDanBmp.Y = m_height/2-GAP/2;
						m_rtDanBmp.Width = m_rtDanBmp.Height = GAP;
						m_rtDanImg = BMPCD_TO_IMGCD(m_rtDanBmp);
					}
					m_pt_of_marker[0].X = m_rtDanImg.X;
					m_pt_of_marker[0].Y = m_rtDanImg.Y;
					m_pt_of_marker[1].X = m_rtDanImg.Right;
					m_pt_of_marker[1].Y = m_rtDanImg.Bottom;
				}
			}
			else {
				this.groupBox1.Visible = false;
				if (true) {
					m_pt_of_marker[0].X = -1;
					m_pt_of_marker[0].Y = -1;
					m_pt_of_marker[1].X = -1;
					m_pt_of_marker[1].Y = -1;
				}
			}
			if (G.CAM_PRC == G.CAM_STS.STS_HIST) {
				if (G.CNT_MOD == 1) {
					m_pt_of_marker[2].X = m_rtCntImg.X;
					m_pt_of_marker[2].Y = m_rtCntImg.Y;
					m_pt_of_marker[3].X = m_rtCntImg.Right;
					m_pt_of_marker[3].Y = m_rtCntImg.Bottom;
				}
				this.groupBox2.Visible = true;
			}
			else {
				this.groupBox2.Visible = false;
				if (true) {
					m_pt_of_marker[2].X = -1;
					m_pt_of_marker[2].Y = -1;
					m_pt_of_marker[3].X = -1;
					m_pt_of_marker[3].Y = -1;
				}
			}
		}
		public void get_param(CAM_PARAM param, out double fVal, out double fMax, out double fMin)
		{
			fVal = double.NaN;
			fMax = double.NaN;
			fMin = double.NaN;
			//---
			if (G.AS.DEBUG_MODE == 1) {
				var r = new Random();
				fVal = 0.5 + r.NextDouble()/10;
				fMin = 0;
				fMax = 1.0;
				return;
			}
			//---
			try {
				FloatName nf = PLCamera.ExposureStartDelayAbs;
				IntegerName ni;
				switch (param) {
				case CAM_PARAM.GAMMA:
					nf = PLCamera.Gamma;
					break;
				case CAM_PARAM.CONTR:
					nf = new FloatName("@CameraDevice/ContrastEnhancement");
					break;
				case CAM_PARAM.BRIGH:
					nf = PLCamera.AutoTargetBrightness;
					break;
				case CAM_PARAM.SHARP:
					nf = PLCamera.SharpnessEnhancement;
					break;
				case CAM_PARAM.WIDTH:
					ni = PLCamera.Width;
					break;
				case CAM_PARAM.HEIGHT:
					ni = PLCamera.Height;
					break;
				case CAM_PARAM.OFFSETX:
					ni = PLCamera.OffsetX;
					break;
				case CAM_PARAM.OFFSETY:
					ni = PLCamera.OffsetY;
					break;
				case CAM_PARAM.EXPOSURE:
					if (m_camera.Parameters.Contains(PLCamera.ExposureTimeAbs)) {
						nf = PLCamera.ExposureTimeAbs;
					}
					else {
						nf = PLCamera.ExposureTime;
					}
					break;
				case CAM_PARAM.GAIN:
					if (m_camera.Parameters.Contains(PLCamera.GainAbs)) {
						nf = PLCamera.GainAbs;
					}
					else {
						nf = PLCamera.Gain;
					}
					break;
				case CAM_PARAM.BAL_SEL:
					{
#if true
						string ret;
						ret = m_camera.Parameters[PLCamera.BalanceRatioSelector].GetValue();
						if (ret == "Red") {
							fVal = 0;
						}
						else if (ret == "Green") {
							fVal = 1;
						}
						else if (ret == "Blue") {
							fVal = 2;
						}
						else {
							ShowException(new Exception(""));
							return;
						}
#else
						for (int i = 0; i < 3; i++) {
							string ret;
							ret = m_camera.Parameters[PLCamera.BalanceRatioSelector].GetValue();
							Debug.WriteLine(string.Format("BalanceRatioSelector:{0}", ret));
							switch (i) {
							case  0:m_camera.Parameters[PLCamera.BalanceRatioSelector].SetValue("Green");break;
							case  1:m_camera.Parameters[PLCamera.BalanceRatioSelector].SetValue("Blue");break;
							default:m_camera.Parameters[PLCamera.BalanceRatioSelector].SetValue("Red");break;
							}
						}
#endif
					}
					break;
				case CAM_PARAM.BALANCE:
					if (m_camera.Parameters.Contains(PLCamera.BalanceRatioAbs)) {
						nf = PLCamera.BalanceRatioAbs;
					}
					else {
						nf = PLCamera.BalanceRatio;
					}
					break;
				default:
					ShowException(new Exception(""));
					break;
				}
				/*
				fn = Basler.Pylon.PLUsbCamera.BslContrast;
				string buf;
				foreach (var f in camera.Parameters) {
					buf = f.FullName + ":"+f.ToString();
					Debug.WriteLine(buf);
				}
				buf = camera.Parameters.ToString();
				fn = new FloatName("@CameraDevice/ContrastEnhancement");*/
				if (nf.Name != PLCamera.ExposureStartDelayAbs.Name) {
					fVal = m_camera.Parameters[nf].GetValue();
					fMax = m_camera.Parameters[nf].GetMaximum();
					fMin = m_camera.Parameters[nf].GetMinimum();
				}
				else {
					fVal = m_camera.Parameters[ni].GetValue();
					fMax = m_camera.Parameters[ni].GetMaximum();
					fMin = m_camera.Parameters[ni].GetMinimum();
				}
			}
			catch (Exception exception) {
				ShowException(exception);
			}
		}

		public void set_param(CAM_PARAM param, double fVal)
		{
			if (G.AS.DEBUG_MODE == 1) {
				return;
			}
			//---
			try {
				FloatName nf = PLCamera.ExposureStartDelayAbs;
				IntegerName	ni;
				switch (param) {
				case CAM_PARAM.GAMMA:
					nf = PLCamera.Gamma;
					break;
				case CAM_PARAM.CONTR:
					nf = new FloatName("@CameraDevice/ContrastEnhancement");
					break;
				case CAM_PARAM.BRIGH:
					nf = PLCamera.AutoTargetBrightness;
					break;
				case CAM_PARAM.SHARP:
					nf = PLCamera.SharpnessEnhancement;
					break;
				case CAM_PARAM.WIDTH:
					ni = PLCamera.Width;
					break;
				case CAM_PARAM.HEIGHT:
					ni = PLCamera.Height;
					break;
				case CAM_PARAM.OFFSETX:
					ni = PLCamera.OffsetX;
					break;
				case CAM_PARAM.OFFSETY:
					ni = PLCamera.OffsetY;
					break;
				case CAM_PARAM.EXPOSURE:
					if (m_camera.Parameters.Contains(PLCamera.ExposureTimeAbs)) {
						nf = PLCamera.ExposureTimeAbs;
					}
					else {
						nf = PLCamera.ExposureTime;
					}
					break;
				case CAM_PARAM.GAIN:
					if (m_camera.Parameters.Contains(PLCamera.GainAbs)) {
						nf = PLCamera.GainAbs;
					}
					else {
						nf = PLCamera.Gain;
					}
					break;
				case CAM_PARAM.BAL_SEL:
					{
						string ret;
						if (fVal == 0) {
							ret = "Red";
						}
						else if (fVal == 1) {
							ret = "Green";
						}
						else {
							ret = "Blue";
						}
						m_camera.Parameters[PLCamera.BalanceRatioSelector].SetValue(ret);
						return;
					}
					break;
					case CAM_PARAM.BALANCE:
					if (m_camera.Parameters.Contains(PLCamera.BalanceRatioAbs)) {
						nf = PLCamera.BalanceRatioAbs;
					}
					else {
						nf = PLCamera.BalanceRatio;
					}
					break;
				default:
					ShowException(new Exception(""));
					break;
				}
				if (nf.Name != PLCamera.ExposureStartDelayAbs.Name) {
					m_camera.Parameters[nf].SetValue(fVal);
				}
				else {
					m_camera.Parameters[ni].SetValue((int)fVal);
				}
			}
			catch (Exception exception) {
				ShowException(exception);
			}
			//int a = (int)m_camera.Parameters[PLCamera.DeviceLinkThroughputLimit].GetValue();
			//this.toolStripStatusLabel6.Text = "DeviceLinkThroughputLimit:" + a.ToString();
			////---
			//string buf;
			//buf = (string)m_camera.Parameters[PLCamera.DeviceLinkThroughputLimitMode].GetValue();
			//this.toolStripStatusLabel6.Text = "DeviceLinkThroughputLimitMode:" + buf;
		}
		public void set_auto(CAM_PARAM param, int val)
		{
#if false
				//
				object obj1, obj2, obj3;
				obj1 = m_camera.Parameters[PLCamera.ExposureAuto].GetValue();
				obj2 = m_camera.Parameters[PLCamera.GainAuto].GetValue();
				obj3 = m_camera.Parameters[PLCamera.BalanceWhiteAuto].GetValue();
				/*
				m_camera.Parameters[PLCamera.ExposureAuto].SetValue(PLCamera.ExposureAuto.Continuous);
				m_camera.Parameters[PLCamera.ExposureAuto].SetValue(PLCamera.ExposureAuto.Off);
				m_camera.Parameters[PLCamera.ExposureAuto].SetValue(PLCamera.ExposureAuto.Once);
				//
				m_camera.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Continuous);
				m_camera.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Off);
				m_camera.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Once);
				//
				m_camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Continuous);
				m_camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Off);
				m_camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Once);
				*/
#endif
			try {
				if (false) {
				}
				else if (param == CAM_PARAM.GAIN) {
					if (val == 0) {//固定

						if (G.AS.DEBUG_MODE != 1) {
							//m_camera.Parameters[PLCamera.ExposureAuto].SetValue(PLCamera.ExposureAuto.Off);
							m_camera.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Off);
						}
						G.CAM_GAI_STS = 0;
					}
					else if (val == 1) {//自動
						if (G.AS.DEBUG_MODE != 1) {
							//m_camera.Parameters[PLCamera.ExposureAuto].SetValue(PLCamera.ExposureAuto.Continuous);
							m_camera.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Continuous);
						}
						G.CAM_GAI_STS = 1;
					}
					else {//ONCE
						if (G.AS.DEBUG_MODE != 1) {
							//m_camera.Parameters[PLCamera.ExposureAuto].SetValue(PLCamera.ExposureAuto.Once);
							m_camera.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Once);
						}
						G.CAM_GAI_STS = 0;
					}
				}
				else if (param == CAM_PARAM.EXPOSURE) {
					if (val == 0) {
						if (G.AS.DEBUG_MODE != 1) {
							m_camera.Parameters[PLCamera.ExposureAuto].SetValue(PLCamera.ExposureAuto.Off);
							//m_camera.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Off);
						}
						G.CAM_EXP_STS = 0;
					}
					else if (val == 1) {
						if (G.AS.DEBUG_MODE != 1) {
							m_camera.Parameters[PLCamera.ExposureAuto].SetValue(PLCamera.ExposureAuto.Continuous);
							//m_camera.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Continuous);
						}
						G.CAM_EXP_STS = 1;
					}
					else {
						if (G.AS.DEBUG_MODE != 1) {
							m_camera.Parameters[PLCamera.ExposureAuto].SetValue(PLCamera.ExposureAuto.Once);
							//m_camera.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Once);
						}
						G.CAM_EXP_STS = 0;
					}
				}
				else if (param == CAM_PARAM.WHITE) {
					if (val == 0) {
						if (G.AS.DEBUG_MODE != 1) {
							m_camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Off);
						}
						G.CAM_WBL_STS = 0;
					}
					else if (val == 1) {
						if (G.AS.DEBUG_MODE != 1) {
							m_camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Continuous);
						}
						G.CAM_WBL_STS = 1;
					}
					else {
						if (G.AS.DEBUG_MODE != 1) {
							m_camera.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Once);
						}
						G.CAM_WBL_STS = 0;
					}
				}
				else {
					ShowException(new Exception(""));
				}
			}
			catch (Exception exception) {
				ShowException(exception);
			}
		}
		private void set_visible(bool b)
		{
			//this.button1.Visible = b;
			//this.button2.Visible = b;
			//this.button3.Visible = b;
			//this.button4.Visible = b;
			this.button5.Visible = b;
			this.button6.Visible = b;
			this.button7.Visible = b;
			this.button8.Visible = b;
			this.button9.Visible = b;
			this.button10.Visible = b;
			this.button11.Visible = b;
			this.button12.Visible = b;
			this.checkBox1.Visible = b;
			//---
		}
		private void update_sts_txt(int mask)
		{
			if ((mask & 1) != 0) {
				string	buf;
				if (m_width <= 0 || m_height <= 0) {
					buf = "";
				}
				else {
					buf = string.Format("{0}x{1}", m_width, m_height);
				}
				this.toolStripStatusLabel1.Text = buf;
			}
			if ((mask & 2) != 0) {
				this.toolStripStatusLabel2.Text = string.Format("FPS:{0:F2}", m_fps);
			}
			if ((mask & 4) != 0) {
				this.toolStripStatusLabel3.Text = string.Format("FID:{0}", m_fid);
			}
			if ((mask & 8) != 0) {
				this.toolStripStatusLabel4.Text = m_sts_xy;
			}
			if ((mask & 16) != 0) {
				this.toolStripStatusLabel5.Text = m_sts_rgb;
			}
			if ((mask & 32) != 0) {
				this.toolStripStatusLabel6.Text = m_sts_hsv;
			}
		}
		private void Form02_Load(object sender, EventArgs e)
		{
			if (true) {
				this.SetDesktopBounds(G.AS.APP_F02_LFT, G.AS.APP_F02_TOP, G.AS.APP_F02_WID, G.AS.APP_F02_HEI);
			}
			if (true) {
			}
			//this.pictureBox1.Dock = DockStyle.Fill;
			//this.panel1.Dock = DockStyle.Fill;
			set_visible(false);

			if (G.AS.DEBUG_MODE == 1) {
				m_size_mode = m_size_regi = 1;
				set_size_mode(m_size_regi, -1, -1);
			}
			else {
				// Update the list of available camera devices in the upper left area.
				UpdateDeviceList();
				if (m_caminfo != null) {
					cam_open();
				}
				else {
					// Disable all buttons.
					EnableButtons(false, false);
				}
			}
#if true
			if (true) {
				//fit to win.
				this.pictureBox1.Dock = DockStyle.Fill;
				this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			}
#endif
			set_title();
			update_sts_txt(32);
			if (true) {
				//---
				this.chart1.Series[0].Points.Clear();
				this.chart1.Series[0].Points.AddXY(0, 0);
				this.chart1.ChartAreas[0].AxisX.Minimum = 0;
				this.chart1.ChartAreas[0].AxisX.Maximum = 100;
				this.chart1.ChartAreas[0].AxisY.Minimum = 0;
				this.chart1.ChartAreas[0].AxisY.Maximum = 256;
				//---
				this.chart2.Series[0].Points.Clear();
				this.chart2.Series[0].Points.AddXY(0, 0);
				this.chart2.ChartAreas[0].AxisX.Minimum = 0;
				this.chart2.ChartAreas[0].AxisX.Maximum = 100;
				this.chart2.ChartAreas[0].AxisY.Minimum = 0;
				this.chart2.ChartAreas[0].AxisY.Maximum = 256;
				//---
				this.chart3.Series[0].Points.Clear();
				this.chart3.Series[0].Points.AddXY(0, 0);
				this.chart3.ChartAreas[0].AxisX.Minimum = 0;
				this.chart3.ChartAreas[0].AxisX.Maximum = 100;
				this.chart3.ChartAreas[0].AxisY.Minimum = 0;
				this.chart3.ChartAreas[0].AxisY.Maximum = 256;
				//---
				this.chart4.Series[0].Points.Clear();
				this.chart4.Series[0].Points.AddXY(0, 0);
				this.chart4.ChartAreas[0].AxisX.Minimum = 0;
				this.chart4.ChartAreas[0].AxisX.Maximum = 100;
				this.chart4.ChartAreas[0].AxisY.Minimum = 0;
				this.chart4.ChartAreas[0].AxisY.Maximum = 256;
			}
			this.groupBox2.Visible = false;
			//---
			for (int i = 0; i < m_col_of_hue.Length; i++) {
				HSV hsv;
				hsv.h = (byte)i;
				hsv.s = hsv.v = 255;
				m_col_of_hue[i] = hsv2rgb(hsv);
			}
			m_cht_dan = new Chart[] {
				this.chart1, this.chart2, this.chart3, this.chart4
			};
			m_cht_his = new Chart[] {
				this.chart5, this.chart6, this.chart7, this.chart8
			};
			//---
			if (G.SS.ETC_DAN_MODE == 0) {
				this.radioButton1.Checked = true;
				this.radioButton2.Checked = false;
			}
			else {
				this.radioButton1.Checked = false;
				this.radioButton2.Checked = true;
			}
			if (G.SS.ETC_HIS_MODE == 0) {
				this.radioButton3.Checked = true;
				this.radioButton4.Checked = false;
			}
			else {
				this.radioButton3.Checked = false;
				this.radioButton4.Checked = true;
			}
			//---
			set_danmod();
			set_hismod();
			//---
			if (G.AS.DEBUG_MODE == 1) {
				OnCameraOpened(null, null);
			}
			//---
			if (G.SS.ETC_UIF_LEVL == 0) {
				this.toolStripMenuItem20.Visible = false;//ヒストグラム
				this.toolStripMenuItem21.Visible = false;//計算範囲・サブメニュー
				this.toolStripSeparator3.Visible = false;
				this.toolStripSeparator4.Visible = false;
			}
			//---
			for (int i = 0; i < m_pt_of_marker.Length; i++) {
				m_pt_of_marker[i].X = m_pt_of_marker[i].Y = -1;
			}
		}

		private void Form02_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (G.FORM12.AUT_STS != 0) {
				//自動測定中はクローズ禁止
				e.Cancel = true;
				return;
			}
			if (isCONNECTED()) {
				Stop();
				for (int i = 0; i < 3; i++) {
					Thread.Sleep(0);
					Application.DoEvents();
				}
			}
			//---
			if (m_bmpR != null) {
				m_bmpR.Dispose();
				m_bmpR = null;
			}
			if (m_bmpZ != null) {
				m_bmpZ.Dispose();
				m_bmpZ = null;
			}
			//---
			DestroyCamera();
			//---
			if (this.Left <= -32000 || this.Top <= -32000) {
				//最小化時は更新しない
			}
			else {
			G.AS.APP_F02_LFT = this.Left;
			G.AS.APP_F02_TOP = this.Top;
			G.AS.APP_F02_WID = this.Width;
			G.AS.APP_F02_HEI = this.Height;
			}
			//---
			G.FORM02 = null;
			G.FORM12.UPDSTS();
		}

		private void buttons_Click(object sender, EventArgs e)
		{
			if (false) {
			}
			//else if (sender == this.button1) {
			//    //zoom in
			//    if ((m_zoom + 1) < m_zoomLevel.Length) {
			//        m_zoom++;
			//        set_size();
			//    }
			//}
			//else if (sender == this.button2) {
			//    //zoom out
			//    if (m_zoom > 0) {
			//        m_zoom--;
			//    }
			//    set_size();
			//}
			//else if (sender == this.button3) {
			//    //zoom 100%
			//    m_zoom = 3;
			//    set_size();
			//}
			//else if (sender == this.button4) {
			//    //fit to win.
			//    this.pictureBox1.Dock = DockStyle.Fill;
			//    this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			//}
			else if (sender == this.button5) {
				if (m_filename != null) {
					m_filename = null;
					set_title();
				}
				m_bcontinuous = false;
				//// Occurs when the single frame acquisition button is clicked.
				OneShot(); // Start the grabbing of one image.
			}
			else if (sender == this.button6) {
				if (m_filename != null) {
					m_filename = null;
					set_title();
				}
				m_fid = m_fps_cnt = 0;
				m_fps = double.NaN;
				if (C_SELFCONT) {
					m_bcontinuous = true;
					OneShot();//OneShotの手動連続起動に変更
				}
				else {
					m_bcontinuous = false;
					//// Occurs when the continuous frame acquisition button is clicked.
					ContinuousShot(); // Start the grabbing of images until grabbing is stopped.
				}
				EnableButtons(false, true);
			}
			else if (sender == this.button7) {
				m_bcontinuous = false;
				// Occurs when the stop frame acquisition button is clicked.
				Stop(); // Stop the grabbing of images.
			}
			else if (sender == this.button8) {
				load_image();
			}
			else if (sender == this.button9) {
				save_image();
			}
			else if (sender == this.button10) {
				// 画像サイズ→1/1
				if (!(G.CAM_PRC == G.CAM_STS.STS_FCUS || G.CAM_PRC == G.CAM_STS.STS_AUTO)) {
					set_size_mode(1, -1, -1);
				}
			}
			else if (sender == this.button11) {
				// 画像サイズ→1/4(画像サイズによるスピード化の確認用)
				if (!(G.CAM_PRC == G.CAM_STS.STS_FCUS || G.CAM_PRC == G.CAM_STS.STS_AUTO)) {
					set_size_mode(2, -1, -1);
				}
			}
			else if (sender == this.button12) {
				// 画像サイズ→1/16(画像サイズによるスピード化の確認用)
				if (!(G.CAM_PRC == G.CAM_STS.STS_FCUS || G.CAM_PRC == G.CAM_STS.STS_AUTO)) {
					set_size_mode(4, -1, -1);
				}
			}
			else if (sender == this.checkBox1) {
				set_layout(/*this.checkBox1.Checked ? 1: 0*/);
			}
			else if (sender == this.radioButton1 || sender == this.radioButton2) {
				G.SS.ETC_DAN_MODE = (this.radioButton1.Checked ? 0 : 1);
				set_danmod();
				disp_bmp(true);
			}
			else if (sender == this.radioButton3 || sender == this.radioButton4) {
				G.SS.ETC_HIS_MODE = (this.radioButton3.Checked ? 0 : 1);
				set_hismod();
				post_proc();
				disp_bmp(true);
			}
		}
		/*
		private void set_size()
		{
			G.mlog("set_size is called!");
			int wid, hei;
			this.pictureBox1.Dock = DockStyle.None;
			this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			this.pictureBox1.Location = new Point(0, 0);
			wid = (int)(this.pictureBox1.Image.Width * m_zoomLevel[m_zoom] / 100.0);
			hei = (int)(this.pictureBox1.Image.Height * m_zoomLevel[m_zoom] / 100.0);
			this.pictureBox1.Size = new Size(wid, hei);
		}*/


		private void Form02_DragEnter(object sender, DragEventArgs e)
		{
			string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);
			foreach (string d in drags) {
				if (!System.IO.File.Exists(d)) {
					// ファイル以外であればイベント・ハンドラを抜ける
					return;
				}
				string ext = System.IO.Path.GetExtension(d).ToLower();
				if (ext == ".bmp" || ext == ".png" || ext == ".jpg" || ext == ".jpeg") {
					e.Effect = DragDropEffects.Copy;
				}
			}
		}

		private void Form02_DragDrop(object sender, DragEventArgs e)
		{
			// ドラッグ＆ドロップされたファイル
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			load_file(files[0]);
		}
		private void load_file(string filename)
		{
			Bitmap bmp;

			m_filename = filename;

			bmp = new Bitmap(filename);
			load_file(bmp, true);
		}
		public void load_file(Bitmap bmp)
		{
			load_file(bmp, true);
		}
		public void load_file(Bitmap bmp, bool bdispose_old)
		{
#if true
			//G.mlog("必要かどうか？？");
			//if (this.pictureBox1.Image != null) {
			//    this.pictureBox1.Image.Dispose();
			//    this.pictureBox1.Image = null;
			//}
#endif
			Image tmp = this.pictureBox1.Image;
			this.pictureBox1.Image = bmp;
			this.pictureBox1.Location = new Point(0, 0);
			this.pictureBox1.Size = bmp.Size;
			if (bdispose_old) {
				if (tmp != null) {
					if (!object.ReferenceEquals(tmp, m_bmpZ)) {
						tmp.Dispose();
						tmp = null;
					}
					else {
						tmp = tmp;
					}
				}
				if (m_bmpR != null) {
					m_bmpR.Dispose();
					m_bmpR = null;
				}
			}
			m_bmpR = bmp;

			G.CAM_WID = m_width = m_bmpR.Width;
			G.CAM_HEI = m_height = m_bmpR.Height;
			pictureBox1_Resize(null, null);
			cv_init();
			post_proc();
			disp_bmp(true);
			//---
			set_title();
			update_sts_txt(1);
			if (!isCONNECTED()) {
				G.FORM12.UPDSTS();
			}
		}

		private bool m_mouse_cap = false;
		private Rectangle m_rtImgBounds;
		private Rectangle m_rtCntImg;

		private Rectangle m_rtDanImg;
		private Rectangle m_rtDanBmp;
		private ArrayList m_arDan = new ArrayList();
		private double[] m_valDan = null;

		private void set_dan()
		{
			double	dx = m_rtDanBmp.Right - m_rtDanBmp.Left;
			double	dy = m_rtDanBmp.Bottom - m_rtDanBmp.Top;
			double	ds = Math.Sqrt(dx*dx + dy*dy);
			int		xo = m_rtDanBmp.Left;
			int		yo = m_rtDanBmp.Top;
			Point	pt = new Point();
			Point	pb = new Point(-1,-1);

			m_arDan.Clear();

			dx /= ds;
			dy /= ds;
			if (ds > 0.0) {
				for (int i = 0; ; i++) {
					pt.X = (int)(xo + dx*i);
					pt.Y = (int)(yo + dy*i);
					if (pt.X  == pb.X && pt.Y == pb.Y) {
						continue;
					}
					m_arDan.Add(pt);
					if (pt.X == m_rtDanBmp.Right && pt.Y == m_rtDanBmp.Bottom) {
						break;
					}
					if (i >= ds) {
						break;
					}
					pb = pt;
				}
			}
			m_valDan = new double[m_arDan.Count];
		}
		struct HSV
		{
			public byte h;
			public byte s;
			public byte v;
		};
		private Color hsv2rgb(HSV hsv)
		{
			float hh = hsv.h*2f;
			float v = hsv.v/255f;
			float s = hsv.s/255f;

			float r, g, b;
			if (s == 0) {
				r = v;
				g = v;
				b = v;
			}
			else {
				float h = hh / 60f;
				int i = (int)Math.Floor(h);
				float f = h - i;
				float p = v * (1f - s);
				float q;
				if (i % 2 == 0) {
					//t
					q = v * (1f - (1f - f) * s);
				}
				else
				{
					q = v * (1f - f * s);
				}

				switch (i%6) {
					case 0:
						r = v;
						g = q;
						b = p;
						break;
					case 1:
						r = q;
						g = v;
						b = p;
						break;
					case 2:
						r = p;
						g = v;
						b = q;
						break;
					case 3:
						r = p;
						g = q;
						b = v;
						break;
					case 4:
						r = q;
						g = p;
						b = v;
						break;
					case 5:
					default:
						r = v;
						g = p;
						b = q;
						break;
				}
			}
			return Color.FromArgb(
            (int)Math.Round(r * 255f),
            (int)Math.Round(g * 255f),
            (int)Math.Round(b * 255f));
		}
		private HSV rgb2hsv(Color cr)
		{
			float r = (float)cr.R / 255f;
			float g = (float)cr.G / 255f;
			float b = (float)cr.B / 255f;
			float max = Math.Max(r, Math.Max(g, b));
			float min = Math.Min(r, Math.Min(g, b));
			float brightness = max;
			float hue, saturation;
			HSV hsv;	
			if (max == min) {
				//undefined
				hue = 0f;
				saturation = 0f;
			}
			else {
				float c = max - min;

				if (max == r) {
					hue = (g - b) / c;
				}
				else if (max == g) {
					hue = (b - r) / c + 2f;
				}
				else {
					hue = (r - g) / c + 4f;
				}
				hue *= 60f;
				if (hue < 0f) {
					hue += 360f;
				}

				saturation = c / max;
			}
			hsv.h = (byte)(hue / 2);
			hsv.s = (byte)(saturation*255);
			hsv.v = (byte)(brightness*255);
			return (hsv);
		}

		private void set_danmod()
		{
			if (G.SS.ETC_DAN_MODE != 0) {
				this.groupBox1.Text = "２点間断面(H/S/V/GRAY)";
				//---
				this.chart1.Series[0].ChartType = SeriesChartType.Point;
				this.chart1.Series[0].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Square;
				this.chart1.Series[0].MarkerSize = 2;
				this.chart1.ChartAreas[0].AxisY.Minimum = 0;
				this.chart1.ChartAreas[0].AxisY.Maximum = 360;
				this.chart1.ChartAreas[0].AxisY.Interval = 360;
				this.chart1.ChartAreas[0].AxisY.MinorGrid.Interval = 120;
			}
			else {
				this.groupBox1.Text = "２点間断面(R/G/B/GRAY)";
				//---
				this.chart1.Series[0].ChartType = SeriesChartType.Line;
				this.chart1.Series[0].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.None;
				this.chart1.ChartAreas[0].AxisY.Minimum = 0;
				this.chart1.ChartAreas[0].AxisY.Maximum = 255;
				this.chart1.ChartAreas[0].AxisY.Interval = 255;
				this.chart1.ChartAreas[0].AxisY.MinorGrid.Interval = 64;
			}
		}
		private void set_hismod()
		{
			//this.chart5.Series[0].LabelToolTip = "TEST:#VALX{N0},#VALY";
			//this.chart5.Series[0].ToolTip = "(#INDEX, #VAL)";
			if (G.SS.ETC_HIS_MODE != 0) {
				this.groupBox2.Text = "ヒストグラム(H/S/V/GRAY)";
			}
			else {
				this.groupBox2.Text = "ヒストグラム(R/G/B/GRAY)";
			}
			if (G.SS.ETC_HIS_MODE != 0) {
				this.chart5.ChartAreas[0].AxisX.Maximum = 360;// fmax;
				string buf = this.chart5.Series[0].GetCustomProperty("PointWidth");
				this.chart5.Series[0].SetCustomProperty("PointWidth", "2");
			}
			else {
				this.chart5.ChartAreas[0].AxisX.Maximum = 255;// fmax;
			}
			if (G.SS.ETC_HIS_MODE != 0) {
				this.chart5.ChartAreas[0].AxisX.Minimum = 0;
				this.chart5.ChartAreas[0].AxisX.Maximum = 360;// fmax;
				string buf = this.chart5.Series[0].GetCustomProperty("PointWidth");
				this.chart5.Series[0].SetCustomProperty("PointWidth", "2");
			}
			else {
				this.chart5.ChartAreas[0].AxisX.Minimum = 0;
				this.chart5.ChartAreas[0].AxisX.Maximum = 255;// fmax;
				this.chart5.Series[0].SetCustomProperty("PointWidth", "0.8");
			}
			for (int i = 0; i < m_cht_his.Length; i++) {
				m_cht_his[i].Series[0].ToolTip = "(#INDEX, #VAL)";
				if (G.SS.ETC_HIS_MODE != 0) {
					m_cht_his[i].ChartAreas[0].AxisX.Minimum = -1;
					m_cht_his[i].ChartAreas[0].AxisX.Maximum = (i==0) ? 360:256;
					m_cht_his[i].ChartAreas[0].AxisX.IntervalOffset = 1;
					m_cht_his[i].ChartAreas[0].AxisX.Interval = (i == 0) ? 60 : 64;
				}
				else {
					m_cht_his[i].ChartAreas[0].AxisX.Minimum = -1;
					m_cht_his[i].ChartAreas[0].AxisX.Maximum = 256;
					m_cht_his[i].ChartAreas[0].AxisX.IntervalOffset = 1;
					m_cht_his[i].ChartAreas[0].AxisX.Interval = 64;
				}
			}


		}
		private void set_danval()
		{
			int cnt = m_arDan.Count;

			for (int i = 0; i < m_cht_dan.Length; i++) {
				m_cht_dan[i].Series[0].Points.Clear();
			}
			for (int i = 0; i < cnt; i++) {
				Color c = m_bmpR.GetPixel(((Point)m_arDan[i]).X, ((Point)m_arDan[i]).Y);
				// Y=0.299*R + 0.587*G + 0.114*B
				// Y=(4899*R + 9617*G + 1868*B + 8192) >> 14
				byte gr = (byte)((4899 * c.R + 9617 * c.G + 1868 * c.B+8192) >> 14);
				if (G.SS.ETC_DAN_MODE != 0) {
					HSV hsv = rgb2hsv(c);
					c = Color.FromArgb(hsv.h, hsv.s, hsv.v);
				}
				//m_valDan[i] = gr;
				if (G.SS.ETC_DAN_MODE != 0) {
					DataPoint dp = new DataPoint();
					int h = (int)c.R;
					dp.SetValueXY(i, h*2);
					dp.Color = m_col_of_hue[h];
					this.chart1.Series[0].Points.Add(dp);
				}
				else {
					this.chart1.Series[0].Points.AddXY(i, (double)c.R);
				}
				this.chart2.Series[0].Points.AddXY(i, (double)c.G);
				this.chart3.Series[0].Points.AddXY(i, (double)c.B);
				this.chart4.Series[0].Points.AddXY(i, (double)gr);
			}
			if (cnt <= 0) {
				for (int i = 0; i < m_cht_dan.Length; i++) {
					m_cht_dan[i].Series[0].Points.AddXY(0, 0);
				}
			}
			//---
			for (int i = 0; i < m_cht_dan.Length; i++) {
				m_cht_dan[i].ChartAreas[0].AxisX.Minimum = 0;
				m_cht_dan[i].ChartAreas[0].AxisX.Maximum = cnt - 1;
				m_cht_dan[i].ChartAreas[0].AxisX.LabelStyle.Interval = cnt - 1;
			}
			//---
			this.label1.Text = string.Format("({0},{1})",m_rtDanBmp.Left, m_rtDanBmp.Top);
			this.label2.Text = string.Format("({0},{1})", m_rtDanBmp.Right, m_rtDanBmp.Bottom);
			double	dif = 0;
			//dif += Math.Pow((m_rtDanBmp.Right - m_rtDanBmp.Left), 2);
			//dif += Math.Pow((m_rtDanBmp.Bottom - m_rtDanBmp.Top), 2);
			//dif = Math.Sqrt(dif);
			dif = G.diff(m_rtDanBmp.Left, m_rtDanBmp.Top, m_rtDanBmp.Right, m_rtDanBmp.Bottom);
			this.label3.Text = string.Format("{0:F1}", dif);
			this.label4.Text = string.Format("{0:F1}", PX2UM(dif));
		}
		private void set_hisval()
		{
			this.chart5.Series[0].Points.Clear();
			this.chart6.Series[0].Points.Clear();
			this.chart7.Series[0].Points.Clear();
			this.chart8.Series[0].Points.Clear();

			if (G.SS.ETC_HIS_MODE == 0) {
				for (int i = 0; i < 256; i++) {
					this.chart5.Series[0].Points.AddY(G.IR.HISTVALR[i]);
					this.chart6.Series[0].Points.AddY(G.IR.HISTVALG[i]);
					this.chart7.Series[0].Points.AddY(G.IR.HISTVALB[i]);
					this.chart8.Series[0].Points.AddY(G.IR.HISTVALY[i]);
				}
			}
			else {
				for (int i = 0; i < 180; i++) {
					DataPoint dp = new DataPoint();
					dp.SetValueXY(i << 1, G.IR.HISTVALH[i]);
					dp.Color = m_col_of_hue[i];
					this.chart5.Series[0].Points.Add(dp);
				}
				for (int i = 0; i < 256; i++) {
					this.chart6.Series[0].Points.AddY(G.IR.HISTVALS[i]);
					this.chart7.Series[0].Points.AddY(G.IR.HISTVALV[i]);
					this.chart8.Series[0].Points.AddY(G.IR.HISTVALY[i]);
				}
			}
			//---
			//---
			//---
		//	this.chart5.ChartAreas[0].AxisY.Minimum = 0;
			this.chart5.ChartAreas[0].AxisY.Maximum = double.NaN;
		//	this.chart6.ChartAreas[0].AxisY.Minimum = 0;
			this.chart6.ChartAreas[0].AxisY.Maximum = double.NaN;
		//	this.chart7.ChartAreas[0].AxisY.Minimum = 0;
			this.chart7.ChartAreas[0].AxisY.Maximum = double.NaN;
		//	this.chart8.ChartAreas[0].AxisY.Minimum = 0;
			this.chart8.ChartAreas[0].AxisY.Maximum = double.NaN;
			//---
			if (G.IR.HIST_ALL) {
				this.label5.Text = "画像全体";
			}
			else if (!G.IR.HIST_RECT) {
				this.label5.Text = "マスク範囲";
			}
			else {
				Point p1 = new Point(G.SS.CAM_HIS_RT_X, G.SS.CAM_HIS_RT_Y);
				Point p2 = new Point(p1.X + G.SS.CAM_HIS_RT_W, p1.Y + G.SS.CAM_HIS_RT_H);
				this.label5.Text = string.Format(" ({0},{1})\r-({2},{3})", p1.X, p1.Y, p2.X, p2.Y);
			}
		}

		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			//Debug.WriteLine(string.Format("{0}:pictureBox1_MouseDown", Environment.TickCount));

			if (!m_rtImgBounds.Contains(e.Location)) {
				//return;
			}
#if true
			if (e.Button != System.Windows.Forms.MouseButtons.Left) {
				return;
			}
			m_mouse_icap=-1;
			for (int i = 0; i < m_pt_of_marker.Length; i++) {
				if (m_pt_of_marker[i].X < 0) {
					continue;
				}
				if (PtInRect(e.Location, m_pt_of_marker[i], C_HANKEI)) {
					m_mouse_icap = i;
					break;
				}
			}
#else
			if (e.Button == System.Windows.Forms.MouseButtons.Right) {
				if (G.CAM_PRC == G.CAM_STS.STS_HIST && G.CNT_MOD == 1) {
					m_mouse_cap = true;
					m_rtCntImg.X = e.X;
					m_rtCntImg.Y = e.Y;
					m_rtCntImg.Width = 0;
					m_rtCntImg.Height = 0;
				}
			}
			else if (e.Button == System.Windows.Forms.MouseButtons.Left) {
				if (this.chart1.Visible) {
					m_mouse_cap = true;
					m_rtDanImg.X = e.X;
					m_rtDanImg.Y = e.Y;
					m_rtDanImg.Width = 0;
					m_rtDanImg.Height = 0;
				}
			}
#endif
		}

		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			//Debug.WriteLine(string.Format("{0}:pictureBox1_MouseUp", Environment.TickCount));

			m_mouse_cap = false;
			m_mouse_icap = -1;
		}
		private Point BMPCD_TO_IMGCD(Point pi)
		{
			Point po = new Point();

			po.X = (int)(m_rtImgBounds.X + (double)m_rtImgBounds.Width * (pi.X) / m_width + 0.5);
			po.Y = (int)(m_rtImgBounds.Y + (double)m_rtImgBounds.Height * (pi.Y) / m_height + 0.5);
			return (po);
		}
		private Point IMGCD_TO_BMPCD(Point pi)
		{
			Point po = new Point();

			po.X = (int)( (double)m_width * (pi.X - m_rtImgBounds.X) / m_rtImgBounds.Width+0.5);
			po.Y = (int)( (double)m_height * (pi.Y - m_rtImgBounds.Y) / m_rtImgBounds.Height+0.5);
			return (po);
		}
		private Rectangle BMPCD_TO_IMGCD(Rectangle ri)
		{
			Point p1 = ri.Location;
			Point p2 = new Point(ri.Right, ri.Bottom);
			Rectangle ro;
			p1 = BMPCD_TO_IMGCD(p1);
			p2 = BMPCD_TO_IMGCD(p2);
			ro = new Rectangle(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
			return (ro);
		}
		private Rectangle IMGCD_TO_BMPCD(Rectangle ri)
		{
			Point p1 = ri.Location;
			Point p2 = new Point(ri.Right, ri.Bottom);
			Rectangle ro;
			p1 = IMGCD_TO_BMPCD(p1);
			p2 = IMGCD_TO_BMPCD(p2);
			ro = new Rectangle(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
			return (ro);
		}
		private Rectangle NORMALIZE_RT(Rectangle rt)
		{
			Rectangle ret = rt;
			if (ret.Width < 0) {
				ret.X = ret.X + ret.Width;
				ret.Width = -ret.Width;
			}
			if (ret.Height < 0) {
				ret.Y = ret.Y + ret.Height;
				ret.Height = -ret.Height;
			}
			return(ret);
		}
		private Point CHK_PT(Point p/*, int width, int height*/)
		{
			Point	pt = p;
			if (pt.X < m_rtImgBounds.Left) {
				pt.X = m_rtImgBounds.Left;
			}
			else if (pt.X > (m_rtImgBounds.Right-1)) {
				pt.X = m_rtImgBounds.Right-1;
			}
			if (pt.Y < m_rtImgBounds.Top) {
				pt.Y = m_rtImgBounds.Top;
			}
			else if (pt.Y > (m_rtImgBounds.Bottom-1)) {
				pt.Y = m_rtImgBounds.Bottom-1;
			}
			return(pt);
		}
		public Color GetPixel(Point pt)
		{
			Color c = m_bmpR.GetPixel(pt.X, pt.Y);
			return(c);
		}
		private bool PtInRect(Point pt, int center_x, int center_y, int radius)
		{
			return(PtInRect(pt, new Point(center_x, center_y), radius));
		}
		private bool PtInRect(Point pt, Point center, int radius)
		{
			Rectangle rt = new Rectangle(center.X-radius, center.Y-radius, radius*2, radius*2);
			return(rt.Contains(pt));
		}
		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			if (m_rtImgBounds.Contains(e.Location)) {
//				m_fps = 1;
				Point pt = IMGCD_TO_BMPCD(e.Location);
#if true
				if (m_bmpR != null && pt.X >= 0 && pt.X < m_bmpR.Width && pt.Y >= 0 && pt.Y < m_bmpR.Height) {
					Color c = m_bmpR.GetPixel(pt.X, pt.Y);
					m_sts_rgb = string.Format("RGB={0},{1},{2}", c.R, c.G, c.B);
					//---
					HSV hsv = rgb2hsv(c);
					m_sts_hsv = string.Format("HSV={0},{1},{2}", hsv.h*2, hsv.s, hsv.v);
				}
#else
				if (m_img_a != null && pt.X >= 0 && pt.X < m_img_a.Width && pt.Y >= 0 && pt.Y < m_img_a.Height) {
					CvScalar sc;
					//sc = m_img_a[pt.X, pt.Y];
					//sc = m_img_a.Get2D(pt.X, pt.Y);
					sc = m_img_a[pt.X + pt.Y*m_width];
					m_sts_cl = string.Format("RGB={0},{1},{2}", sc.Val2, sc.Val1, sc.Val0);
					//Color c = m_bmpR.GetPixel(pt.X, pt.Y);
					//m_sts_cl = string.Format("RGB={0},{1},{2}", c.R, c.G, c.B);
				}
#endif
				else {
					m_sts_rgb = string.Format("RGB=---,---,---");
					m_sts_hsv = string.Format("HSV=---,---,---");
				}
				m_sts_xy = string.Format("X,Y={0},{1}", pt.X, pt.Y);
				//m_img_a.get
				//sc = Cv.Get1D(m_img_a, pt.X + pt.Y * m_width);
			}
			else {
//				m_fps = 0;
				m_sts_xy = string.Format("X,Y=---,---");
				m_sts_rgb = string.Format("RGB=---,---,---");
				m_sts_hsv = string.Format("HSV=---,---,---");
			}

			//GraphicsUnit units = GraphicsUnit.Point;

			//RectangleF imgRectangleF = pictureBox1.Image.GetBounds(ref units);
			//Rectangle imgRectangle = Rectangle.Round(imgRectangleF);
			//m_sts_x = e.X;
			//m_sts_y = e.Y;
			update_sts_txt(2 | 8| 16 | 32);
			bool flag = false;
#if false
			if (m_mouse_cap && e.Button == System.Windows.Forms.MouseButtons.Right) {
				m_rtCntImg.Width = e.X - m_rtCntImg.X;
				m_rtCntImg.Height = e.Y - m_rtCntImg.Y;

				flag = true;
//				disp_bmp(true);

				Point p1, p2;
				p1 = IMGCD_TO_BMPCD(new Point(m_rtCntImg.Left, m_rtCntImg.Top));
				p2 = IMGCD_TO_BMPCD(new Point(m_rtCntImg.Right, m_rtCntImg.Bottom));
#if DEBUG
				if ((m_rtCntImg.X + m_rtCntImg.Width) != m_rtCntImg.Right) {
					G.mlog("(m_rtImg.X + m_rtImg.Width) != m_rtImg.Right");
				}
				if ((m_rtCntImg.Y + m_rtCntImg.Height) != m_rtCntImg.Bottom) {
					G.mlog("(m_rtImg.X + m_rtImg.Width) != m_rtImg.Right");
				}
#endif
				//m_sts_rt = string.Format("RECT=({0},{1})-({2},{3})", p1.X, p1.Y, p2.X, p2.Y);
				G.SS.CAM_HIS_RT_X = p1.X;
				G.SS.CAM_HIS_RT_Y = p1.Y;
				G.SS.CAM_HIS_RT_W = (p2.X - p1.X);
				G.SS.CAM_HIS_RT_H = (p2.Y - p1.Y);
				update_sts_txt(32);
				reset_mask_rect();
			}
			if (m_mouse_cap && e.Button == System.Windows.Forms.MouseButtons.Left) {
				Point	pt = e.Location;
				pt = CHK_PT(pt/*, 0, 0*/);
				m_rtDanImg.Width = pt.X - m_rtDanImg.X;
				m_rtDanImg.Height = pt.Y - m_rtDanImg.Y;
				Point p1, p2;
				p1 = IMGCD_TO_BMPCD(new Point(m_rtDanImg.Left, m_rtDanImg.Top));
				p2 = IMGCD_TO_BMPCD(new Point(m_rtDanImg.Right, m_rtDanImg.Bottom));
				//if ((p1.X > p2.X) || (p1.X == p2.X && p1.Y > p2.Y)) {
				//    Point p3 = p1;
				//    p1 = p2;
				//    p2 = p3;
				//}
				m_rtDanBmp.X = p1.X;
				m_rtDanBmp.Y = p1.Y;
				m_rtDanBmp.Width = (p2.X-p1.X);
				m_rtDanBmp.Height= (p2.Y-p1.Y);
				set_dan();
				set_danval();
				flag = true;
			}
#else
			if (m_mouse_icap >= 0) {
				//kokode pt wo sousa
				m_pt_of_marker[m_mouse_icap] = CHK_PT(e.Location);
				switch (m_mouse_icap) {
				case 0:
				case 1://2点間断面
					m_rtDanImg.X = m_pt_of_marker[0].X;
					m_rtDanImg.Y = m_pt_of_marker[0].Y;
					m_rtDanImg.Width = (m_pt_of_marker[1].X-m_pt_of_marker[0].X);
					m_rtDanImg.Height = (m_pt_of_marker[1].Y-m_pt_of_marker[0].Y);
					m_rtDanBmp = IMGCD_TO_BMPCD(m_rtDanImg);
					set_dan();
					set_danval();
					flag = true;
				break;
				case 2:
				case 3://コントラスト矩形
					m_rtCntImg.X = m_pt_of_marker[2].X;
					m_rtCntImg.Y = m_pt_of_marker[2].Y;
					m_rtCntImg.Width = (m_pt_of_marker[3].X-m_pt_of_marker[2].X);
					m_rtCntImg.Height = (m_pt_of_marker[3].Y-m_pt_of_marker[2].Y);
					m_rtCntImg = NORMALIZE_RT(m_rtCntImg);
					Rectangle rt = IMGCD_TO_BMPCD(m_rtCntImg);
					G.SS.CAM_HIS_RT_X = rt.X;
					G.SS.CAM_HIS_RT_Y = rt.Y;
					G.SS.CAM_HIS_RT_W = rt.Width;
					G.SS.CAM_HIS_RT_H = rt.Height;
					update_sts_txt(32);
					reset_mask_rect();
					flag = true;
				break;
				case 4:
				case 5:
				case 6://距離
					m_calc_len.set_pt(0, IMGCD_TO_BMPCD(m_pt_of_marker[4]));
					m_calc_len.set_pt(1, IMGCD_TO_BMPCD(m_pt_of_marker[5]));
					m_calc_len.set_pt(2, IMGCD_TO_BMPCD(m_pt_of_marker[6]));
					flag = true;
				break;
				}
			}
			else
#endif
			if (!m_mouse_cap) {
				Point pt = e.Location;
				bool bl = false;
				for (int i = 0; i < m_pt_of_marker.Length; i++) {
					if (m_pt_of_marker[i].X < 0) {
						continue;
					}
					if (PtInRect(e.Location, m_pt_of_marker[i], C_HANKEI)) {
						bl = true;
					}
				}
				if (bl) {
					this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Hand;
				}
				else {
					this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Default;
				}
			}
			if (flag) {
				disp_bmp(true);
			}
		}


		public void reset_mask_rect()
		{
#if true
			if (true) {
			RECT	rt;
			rt.Left   = G.SS.CAM_HIS_RT_X;
			rt.Top    = G.SS.CAM_HIS_RT_Y;
			rt.Right  = G.SS.CAM_HIS_RT_X + G.SS.CAM_HIS_RT_W;
			rt.Bottom = G.SS.CAM_HIS_RT_Y + G.SS.CAM_HIS_RT_H;
			OCV_ZERO((int)IMG.IMG_M);
			OCV_DRAW_RECT((int)IMG.IMG_M, ref rt, 0xFFFFFF, -1);
			}
			else {
			OCV_RESET_MASK(G.SS.CAM_HIS_RT_X, G.SS.CAM_HIS_RT_Y, G.SS.CAM_HIS_RT_W, G.SS.CAM_HIS_RT_H);
			}
#else
			CvRect rt = new CvRect(G.SS.CAM_HIS_RT_X, G.SS.CAM_HIS_RT_Y, G.SS.CAM_HIS_RT_W, G.SS.CAM_HIS_RT_H);
			Cv.Zero(m_img_m);
			Cv.Rectangle(m_img_m, rt, Cv.RGB(255, 255, 255));
#endif
			//---/969
			if (true) {
				//G.mlog("#iあとで修正するコト");
				m_rtCntImg = BMPCD_TO_IMGCD(new Rectangle(G.SS.CAM_HIS_RT_X, G.SS.CAM_HIS_RT_Y, G.SS.CAM_HIS_RT_W, G.SS.CAM_HIS_RT_H));
			}
		}
		//
		// P1, P2 (毛髪径の上側:P1, 毛髪径の下側:P2)の中点(毛髪中心)から径方向へ
		// rr倍した点を上側:P3, 下側:P4として算出
		// rr=1のときは、P3=P1, P4=P2となる
		//
		public void TO_RR(double rr, Point p1, Point p2, out Point p3, out Point p4)
		{
			int	xo = (p1.X + p2.X)/2;
			int yo = (p1.Y + p2.Y)/2;
			p3 = new Point();
			p4 = new Point();
			p3.X = (int)((p1.X-xo)*rr) + xo;
			p3.Y = (int)((p1.Y-yo)*rr) + yo;
			p4.X = (int)((p2.X-xo)*rr) + xo;
			p4.Y = (int)((p2.Y-yo)*rr) + yo;
		}
		// n=0:毛髪範囲10%, 1:毛髪範囲25%, 2:毛髪範囲50%, 3:毛髪範囲75%, 4:毛髪範囲100%
		//   5:毛髪範囲10% (横1/3), 6:毛髪範囲10% (横1/4), 6:毛髪範囲10% (横1/5)
		public void reset_mask_poly(int n)
		{
			try {
				POINT[] pts = null;
				int q = 0;
				if (G.IR.DIA_CNT <= 1) {
					pts = new POINT[4];
					q = 4;
					G.IR.MSK_PLY[0].X = 0;
					G.IR.MSK_PLY[0].Y = 0;
					//---
					G.IR.MSK_PLY[1].X = 0;
					G.IR.MSK_PLY[1].Y = this.m_height-1;
					//---
					G.IR.MSK_PLY[2].X = this.m_width -1;
					G.IR.MSK_PLY[2].Y = this.m_height-1;
					//---
					G.IR.MSK_PLY[3].X = this.m_width -1;
					G.IR.MSK_PLY[3].Y = 0;
					G.IR.MSK_PLY_CNT = 4;
				}
				else {
					double []rate = {0.10, 0.25, 0.50, 0.75, 1.00, 0.10, 0.10, 0.10};
					Point[] top = new Point[G.IR.DIA_CNT];
					Point[] btm = new Point[G.IR.DIA_CNT];
					int xmin = 99999, ymin = 99999;
					int xmax = 0, ymax = 0;
					int xcen, xwid, ycen, ywid;
					double	xrat;
					if (G.IR.DIA_CNT <= 2) {
					G.IR.DIA_CNT = G.IR.DIA_CNT;
					}
					pts = new POINT[G.IR.DIA_CNT*2];
					for (int i = 0; i < G.IR.DIA_CNT; i++) {
						TO_RR(rate[n],  G.IR.DIA_TOP[i],  G.IR.DIA_BTM[i], out top[i], out btm[i]);
						if (xmin > top[i].X) {
							xmin = top[i].X;
						}
						if (xmin > btm[i].X) {
							xmin = btm[i].X;
						}
						if (xmax < top[i].X) {
							xmax = top[i].X;
						}
						if (xmax < btm[i].X) {
							xmax = btm[i].X;
						}
						if (ymin > top[i].Y) {
							ymin = top[i].Y;
						}
						if (ymin > btm[i].Y) {
							ymin = btm[i].Y;
						}
						if (ymax < top[i].Y) {
							ymax = top[i].Y;
						}
						if (ymax < btm[i].Y) {
							ymax = btm[i].Y;
						}
					}
					try {
					if (n >= 5) {
						PointF cen = new Point((xmin+xmax)/2, (ymin+ymax)/2);
						xwid = (xmax-xmin)+1;
						ywid = (xmax-xmin)+1;
						switch (n) {
						case 5:
							xrat = 1.0/3.0;
						break;
						case 6:
							xrat = 1.0/4.0;
						break;
						default:
							xrat = 1.0/5.0;
						break;
						}
						
						PointF rat_lt, rat_lb, rat_rt, rat_rb;
						int	imin = 0, imax = G.IR.DIA_CNT-1;
						int Z = G.IR.DIA_CNT-1;
						FN1D f_lt = new FN1D(top[0], cen);
						FN1D f_lb = new FN1D(btm[0], cen);
						FN1D f_rt = new FN1D(top[Z], cen);
						FN1D f_rb = new FN1D(btm[Z], cen);
						double dif;

						rat_lt = f_lt.GetScanPt1Ext(cen, top[0], (dif=G.diff(top[0], cen))*xrat);
						rat_lb = f_lb.GetScanPt1Ext(cen, btm[0], (dif=G.diff(btm[0], cen))*xrat);
						rat_rt = f_rt.GetScanPt1Ext(cen, top[Z], (dif=G.diff(top[Z], cen))*xrat);
						rat_rb = f_rb.GetScanPt1Ext(cen, btm[Z], (dif=G.diff(btm[Z], cen))*xrat);
						if (G.IR.DIA_CNT <= 3) {
							imin = 0;
							imax = G.IR.DIA_CNT-1;
						}
						else {
							for (int i = 0; i < G.IR.DIA_CNT; i++) {
								if (top[i].X < rat_lt.X && btm[i].X < rat_lb.X) {
									continue;
								}
								imin = i;break;
							}
							for (int i = G.IR.DIA_CNT-1; i >= 0; i--) {
								if (top[i].X > rat_rt.X && btm[i].X > rat_rb.X) {
									continue;
								}
								imax = i;break;
							}
						}
						if (true) {
							if ((imin+1) >= top.Length) {
								imin--;
							}
							if ((imax-1) < 0) {
								imax++;
							}
						}
						f_lt = new FN1D(top[imin], top[imin+1]);
						f_lb = new FN1D(btm[imin], btm[imin+1]);
						f_rt = new FN1D(top[imax], top[imax-1]);
						f_rb = new FN1D(btm[imax], btm[imax-1]);
						try {
						top[imin].X = (int)rat_lt.X;
						top[imin].Y = (int)f_lt.GetYatX(rat_lt.X);
						//---
						btm[imin].X = (int)rat_lb.X;
						btm[imin].Y = (int)f_lb.GetYatX(rat_lb.X);
						//---
						top[imax].X = (int)rat_rt.X;
						top[imax].Y = (int)f_rt.GetYatX(rat_rt.X);
						//---
						btm[imax].X = (int)rat_rb.X;
						btm[imax].Y = (int)f_rb.GetYatX(rat_rb.X);
						}
						catch (Exception ex) {
						ShowException(ex);
						}
						//---
						for (int i = imin; i <= imax; i++, q++) {
							G.IR.MSK_PLY[q] = top[i];
						}
						for (int i = imax; i >= imin; i--, q++) {
							G.IR.MSK_PLY[q] = btm[i];
						}
						G.IR.MSK_PLY_CNT = q;
					}
					else {
						for (int i = 0; i < G.IR.DIA_CNT; i++, q++) {
							G.IR.MSK_PLY[q] = top[i];
						}
						for (int i = G.IR.DIA_CNT-1; i >= 0; i--, q++) {
							G.IR.MSK_PLY[q] = btm[i];
						}
						G.IR.MSK_PLY_CNT = q;
					}
					}
					catch (Exception ex) {
					ShowException(ex);
					}
				}
				for (int i = 0; i < G.IR.MSK_PLY_CNT; i++) {
					pts[i].x = G.IR.MSK_PLY[i].X;
					pts[i].y = G.IR.MSK_PLY[i].Y;

					G.IR.MSK_PLY_IMG[i] = BMPCD_TO_IMGCD(G.IR.MSK_PLY[i]);
				}
				OCV_ZERO((int)IMG.IMG_M);
				OCV_FILL_POLY((int)IMG.IMG_M, ref pts[0], G.IR.MSK_PLY_CNT, 0xFFFFFF);
			}
			catch (Exception exception) {
				ShowException(exception);
			}
		}
		// Occurs when a device with an opened connection is removed.
		private void OnConnectionLost(Object sender, EventArgs e)
		{
			if (InvokeRequired) {
				// If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
				BeginInvoke(new EventHandler<EventArgs>(OnConnectionLost), sender, e);
				return;
			}

			// Close the camera object.
			DestroyCamera();
			// Because one device is gone, the list needs to be updated.
			UpdateDeviceList();
		}


		// Occurs when the connection to a camera device is opened.
		private void OnCameraOpened(Object sender, EventArgs e)
		{
			if (InvokeRequired) {
				// If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
				BeginInvoke(new EventHandler<EventArgs>(OnCameraOpened), sender, e);
				return;
			}

			// The image provider is ready to grab. Enable the grab buttons.
			EnableButtons(true, false);
#if true
			//buttons_Click(this.button4, null);//zoom fit
			buttons_Click(this.button6, null);//continuous
#endif
		}


		// Occurs when the connection to a camera device is closed.
		private void OnCameraClosed(Object sender, EventArgs e)
		{
			if (InvokeRequired) {
				// If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
				BeginInvoke(new EventHandler<EventArgs>(OnCameraClosed), sender, e);
				return;
			}

			// The camera connection is closed. Disable all buttons.
			EnableButtons(false, false);
		}


		// Occurs when a camera starts grabbing.
		private void OnGrabStarted(Object sender, EventArgs e)
		{
		//	m_tk1 = Environment.TickCount;
			if (InvokeRequired) {
				// If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
				BeginInvoke(new EventHandler<EventArgs>(OnGrabStarted), sender, e);
				return;
			}

			// Reset the stopwatch used to reduce the amount of displayed images. The camera may acquire images faster than the images can be displayed.

			m_stopWatch.Reset();

			// The camera is grabbing. Disable the grab buttons. Enable the stop button.
			if (!m_bcontinuous) {
				EnableButtons(false, true);
			}
#if false//@@
			m_fid = m_fps_cnt = 0;
			m_fps = double.NaN;
#endif
		//	m_tk1 = Environment.TickCount - m_tk1;
		}


		// Occurs when an image has been acquired and is ready to be processed.
		private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
		{
			if (this.InvokeRequired) {
				// If called from a different thread, we must use the Invoke method to marshal the call to the proper GUI thread.
				// The grab result will be disposed after the event call. Clone the event arguments for marshaling to the GUI thread.
				BeginInvoke(new EventHandler<ImageGrabbedEventArgs>(OnImageGrabbed), sender, e.Clone());
				return;
			}
Trace.WriteLineIf((G.AS.TRACE_LEVEL & 1)!=0, "1:OnImageGrabbed()::" + Environment.TickCount.ToString());

			m_tk1 = Environment.TickCount;

			try {
				// Acquire the image from the camera. Only show the latest image. The camera may acquire images faster than the images can be displayed.

				// Get the grab result.
				IGrabResult grabResult = null;
				int mask = 0;
				if (G.AS.DEBUG_MODE != 1) {
					grabResult = e.GrabResult;
				}
				// Check if the image can be displayed.
				if (grabResult != null && !grabResult.IsValid) {
					m_tk1 = m_tk1;
				}
				else {
					m_fid++;
					m_fps_cnt++;
					if (m_fps_cnt == 1) {
						m_fps_tk1 = Environment.TickCount;
					}
					else if (m_fps_cnt == (m_fps_wid + 1)) {
						m_fps_tk2 = Environment.TickCount;
						m_fps = (double)m_fps_wid / ((m_fps_tk2 - m_fps_tk1) / 1000.0);
						m_fps_cnt = 0;
						mask = 2;
					}
					mask |= 4;

					// Reduce the number of displayed images to a reasonable amount if the camera is acquiring images very fast.
					if (!(m_stopWatch.IsRunning == false|| m_stopWatch.ElapsedMilliseconds > /*33*/500)) {
						m_tk1 = m_tk1;
						//this.BeginInvoke(new G.DLG_VOID_INT(this.update_sts_txt), new object[] { mask });
					}
					else {
						//       !m_stopWatch ||  m_stopWatch.ElapsedMilliseconds > /*33*/500     
						//             F      ||                F         =                  F => T上へ
						//             T      ||                F         =                  T => F下へ  
						//             F      ||                T         =                  T => F下へ  
						//             T      ||                T         =                  T => F下へ  
						bool flag = false;
						m_stopWatch.Restart();
#if true//@@
#endif
						if (m_bmpR != null) {
							m_bmpR.Dispose();// Dispose the bitmap.
							m_bmpR = null;
						}
						if (grabResult == null) {
							// DEBUG_MODE
						}
						else if (m_width != grabResult.Width || m_height != grabResult.Height) {
							G.CAM_WID = m_width = grabResult.Width;
							G.CAM_HEI = m_height = grabResult.Height;
Trace.WriteLineIf((G.AS.TRACE_LEVEL & 1)!=0, "1:OnImageGrabbed()::WxH" + m_width.ToString() + "x" + m_height.ToString());
							//pictureBox1_Resize(null, null);
							flag = true;
							cv_term();
							cv_init();
							mask |= 1;
						}
						this.BeginInvoke(new G.DLG_VOID_INT(this.update_sts_txt), new object[] { mask });
						m_tk2 = Environment.TickCount;
						m_bmpR = new Bitmap(m_width, m_height, PixelFormat.Format32bppRgb);
						Rectangle rtBmp = new Rectangle(0, 0, m_bmpR.Width, m_bmpR.Height);
						BitmapData bmpData;
						if (G.AS.DEBUG_MODE == 1)
						{
							Image img = DBGMODE.GET_IMAGE();
							Graphics g = Graphics.FromImage(m_bmpR);
							g.DrawImage(img, 0, 0);
							g.Dispose();
						}
						else
						{
							// Lock the bits of the bitmap.
							bmpData = m_bmpR.LockBits(rtBmp, ImageLockMode.ReadWrite, m_bmpR.PixelFormat);
							// Place the pointer to the buffer of the bitmap.
							m_converter.OutputPixelFormat = PixelType.BGRA8packed;
							IntPtr ptrBmp = bmpData.Scan0;
							m_converter.Convert(ptrBmp, bmpData.Stride * m_bmpR.Height, grabResult); //Exception handling TODO
							m_bmpR.UnlockBits(bmpData);
						}
						if (true) {
							// 32bitRGB -> 24bitRGB
							//false:78ms,63ms,63ms,47ms(->30ms程度)
							Bitmap bmp = m_bmpR.Clone(rtBmp, PixelFormat.Format24bppRgb);
							m_bmpR.Dispose();
							m_bmpR = bmp;
						}
#if false
						else {
							// 32bitRGB -> 24bitRGB
							int ret;
							Bitmap bmp = new Bitmap(m_width, m_height, PixelFormat.Format24bppRgb);
							bmpData = m_bmpR.LockBits(rtBmp, ImageLockMode.ReadWrite, m_bmpR.PixelFormat);
							ret = OCV_SET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
							m_bmpR.UnlockBits(bmpData);

							bmpData = bmp.LockBits(rtBmp, ImageLockMode.ReadWrite, bmp.PixelFormat);
							ret = OCV_GET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
							bmp.UnlockBits(bmpData);
							m_bmpR.Dispose();
							m_bmpR = bmp;
						}
#endif
						m_tk2 = Environment.TickCount-m_tk2;//94ms,109ms,109ms,109ms
						m_tk2 = Environment.TickCount;
						if (G.SS.ETC_LED_IRGR && ((G.LED_PWR_STS & 4) != 0)) {
#if true
							int ret;
							bmpData = m_bmpR.LockBits(rtBmp, ImageLockMode.ReadWrite, m_bmpR.PixelFormat);
							ret = OCV_SET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
							OCV_TO_GRAY((int)IMG.IMG_A, (int)IMG.IMG_G);
							OCV_MERGE((int)IMG.IMG_G, (int)IMG.IMG_G, (int)IMG.IMG_G, (int)IMG.IMG_A);
							ret = OCV_GET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
							m_bmpR.UnlockBits(bmpData);
#else
							//グレースケールに即時変換
							m_img_a.CopyFrom(m_bmpR);//生画像
							Cv.CvtColor(m_img_a, m_img_g, ColorConversion.RgbaToGray);
							Cv.Merge(m_img_g, m_img_g, m_img_g, null, m_img_a);
							if (m_bmpR != null) {
								m_bmpR.Dispose();// Dispose the bitmap.
								m_bmpR = null;
							}
							m_bmpR = m_img_a.ToBitmap();
#endif
							m_bmpR.Tag = true;//true:gray-scaled
						}
						else {
							m_bmpR.Tag = null;
						}
						m_tk2 = Environment.TickCount - m_tk2;//47ms,62ms,63ms,47ms
						// Assign a temporary variable to dispose the bitmap after assigning the new bitmap to the display control.
						//Bitmap bitmapOld = pictureBox1.Image as Bitmap;
						// Provide the display control with the new bitmap. This action automatically updates the display.
						if (G.CAM_PRC == G.CAM_STS.STS_NONE) {
							//disp_bmp(true);
							//this.BeginInvoke(new DLG_VOID_BOOL(this.disp_bmp), new object[] {true});
						}
						else {
							post_proc();
						}
						disp_bmp(true);
						if (flag) {
							pictureBox1_Resize(null, null);
						}
						//if (bitmapOld != null) {
						//    // Dispose the bitmap.
						//    bitmapOld.Dispose();
						//}
					}
				}
			}
			catch (Exception exception) {
				ShowException(exception);
			}
			finally {
				// Dispose the grab result if needed for returning it to the grab loop.
				if (e != null)
				{
					e.DisposeGrabResultIfClone();
				}
			}
			m_tk1 = Environment.TickCount-m_tk1;
			//m_tk3 = m_tk2 - m_tk1;
			if (m_tk3 > 150) {
				m_tk3 = m_tk3;
			}
		}


		// Occurs when a camera has stopped grabbing.
		private void OnGrabStopped(Object sender, GrabStopEventArgs e)
		{
			if (InvokeRequired) {
				// If called from a different thread, we must use the Invoke method to marshal the call to the proper thread.
				BeginInvoke(new EventHandler<GrabStopEventArgs>(OnGrabStopped), sender, e);
				return;
			}
Trace.WriteLineIf((G.AS.TRACE_LEVEL & 1)!=0, "1:OnGrabStopped()::" + Environment.TickCount.ToString());
			// Reset the stopwatch.
			m_stopWatch.Reset();
			if (!m_bcontinuous) {
				// The camera stopped grabbing. Enable the grab buttons. Disable the stop button.
				EnableButtons(true, false);
			}
			// If the grabbed stop due to an error, display the error message.
			if (G.AS.DEBUG_MODE == 1)
			{
			}
			else if (e.Reason != GrabStopReason.UserRequest) {
				MessageBox.Show("A grab error occured:\n" + e.ErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			if (C_SELFCONT) {
				if (m_bcontinuous) {
					this.BeginInvoke(new G.DLG_VOID_VOID(this.FireOneShot));
					//Thread.Sleep(0);
					//OneShot();
				}
			}
		}
		private void FireOneShot()
		{
			if (!m_bcontinuous) {
				return;
			}
			if (m_bproc) {
				this.BeginInvoke(new G.DLG_VOID_VOID(this.FireOneShot));
				//Thread.Sleep(0);
				return;
			}
			m_tk1 = Environment.TickCount;
			OneShot();
			m_tk1 = Environment.TickCount - m_tk1;
			if (m_tk1 > 100) {
				m_tk1 = m_tk1;
			}
		}
		//private void FireCmd(int n)
		//{
		//    switch (n) {
		//    case 0:
		//        G.FORM12.BeginInvoke(new G.DLG_VOID_VOID(G.FORM12.CALLBACK));
		//        break;
		//    default:
		//        break;
		//    }
		//    disp_bmp(true);
		//}
		// Helps to set the states of all buttons.
		private void EnableButtons(bool canGrab, bool canStop)
		{
			this.button6.Enabled = canGrab;//continuous
			this.button5.Enabled = canGrab;//one shot
			this.button7.Enabled = canStop;//stop
			//---
			this.toolStripMenuItem1.Checked = false;//one shot
			this.toolStripMenuItem2.Checked =  m_bcontinuous;//continuous
			this.toolStripMenuItem3.Checked = !m_bcontinuous;//stop
			//this.button10.Enabled = (G.CAM_PRC == 0);//canGrab;
			//this.button11.Enabled = (G.CAM_PRC == 0);//canGrab;
			//this.button12.Enabled = (G.CAM_PRC == 0);//canGrab;
		}


		// Stops the grabbing of images and handles exceptions.
		public void Stop()
		{
#if true
			if (m_camera == null) {
				return;
			}
#endif
			// Stop the grabbing.
			try {
				m_camera.StreamGrabber.Stop();
			}
			catch (Exception exception) {
				ShowException(exception);
			}
		}


		// Closes the camera object and handles exceptions.
		private void DestroyCamera()
		{
			cv_term();
			// Disable all parameter controls.
			try {
				if (m_camera != null) {
#if false//@@
					testImageControl.Parameter = null;
					pixelFormatControl.Parameter = null;
#if true
					floatSliderUserControl1.Parameter = null;
					floatSliderUserControl2.Parameter = null;
					floatSliderUserControl3.Parameter = null;
					floatSliderUserControl4.Parameter = null;
#else
                    widthSliderControl.Parameter = null;
                    heightSliderControl.Parameter = null;
#endif
					gainSliderControl.Parameter = null;
					exposureTimeSliderControl.Parameter = null;
#endif
				}
			}
			catch (Exception exception) {
				ShowException(exception);
			}

			// Destroy the camera object.
			try {
				if (m_camera != null) {
					m_camera.Close();
					m_camera.Dispose();
					m_camera = null;
				}
			}
			catch (Exception exception) {
				ShowException(exception);
			}
		}
		public int get_size_mode()
		{
			return(m_size_mode);
		}
		public void set_size_mode(int size_mode, int xo, int yo)
		{
			if (size_mode != m_size_regi) {
				m_size_regi = size_mode;
				m_size_xo = xo;
				m_size_yo = yo;
			}
			switch (m_size_mode) {
			case 1:
				this.button10.BackColor = Color.LightCyan;
				this.button11.BackColor = Color.Transparent;
				this.button12.BackColor = Color.Transparent;
			break;
			case 2:
				this.button10.BackColor = Color.Transparent;
				this.button11.BackColor = Color.LightCyan;
				this.button12.BackColor = Color.Transparent;
			break;
			case 4:
				this.button10.BackColor = Color.Transparent;
				this.button11.BackColor = Color.Transparent;
				this.button12.BackColor = Color.LightCyan;
			break;
			}
		}

		// Starts the grabbing of a single image and handles exceptions.
		private void OneShot()
		{
Trace.WriteLineIf((G.AS.TRACE_LEVEL & 1)!=0, "1:OneShot()::" + Environment.TickCount.ToString());
			try {
				if (m_size_regi != m_size_mode) {
					double val, max, min;
					int	hei, hmax, hmin, wid, wmax, wmin;
					int		offx, offy;
					get_param(CAM_PARAM.WIDTH, out val, out max, out min);
					wid  = (int)(val+0.5);
					wmax = (int)(max+0.5);
					wmin = (int)(min+0.5);
					//---
					get_param(CAM_PARAM.HEIGHT, out val, out max, out min);
					hei  = (int)(val+0.5);
					hmax = (int)(max+0.5);
					hmin = (int)(min+0.5);
					//---
					wid = wmax / m_size_regi;
					hei = hmax / m_size_regi;
					set_param(CAM_PARAM.WIDTH, wid);
					Thread.Sleep(5);
					set_param(CAM_PARAM.HEIGHT, hei);
					Thread.Sleep(5);
					if (m_size_xo < 0 || m_size_yo < 0) {
						offx = (wmax-wmax/m_size_regi)/2;
						offy = (hmax-hmax/m_size_regi)/2;
					}
					else {
						//m_size_xo /= m_size_regi;
						//m_size_yo /= m_size_regi;
						if ((offx = m_size_xo - (wid/2)) < 0) {
							offx = 0;
						}
						else if ((offx + wid) >= wmax) {
							offx = wmax-wid;
						}
						if ((offy = m_size_yo - (hei/2)) < 0) {
							offy = 0;
						}
						else if ((offy + hei) >= hmax) {
							offy = hmax-hei;
						}
					}
					offx &= (~1);
					offy &= (~1);
					set_param(CAM_PARAM.OFFSETX, offx);
					Thread.Sleep(5);
					set_param(CAM_PARAM.OFFSETY, offy);
					//---
					Thread.Sleep(5);
					set_size_mode(m_size_mode = m_size_regi, -1, -1);
					m_bNeedToTakeImgBounds = true;
				}
				if (G.AS.DEBUG_MODE == 1) {
					DBGMODE.REG_CALLBACK(this, OnImageGrabbed, OnGrabStopped);
					DBGMODE.ONE_SHOT();
				}
				else {
					// Starts the grabbing of one image.
					m_camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
					m_camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
				}
			}
			catch (Exception exception) {
				ShowException(exception);
			}
		}


		// Starts the continuous grabbing of images and handles exceptions.
		private void ContinuousShot()
		{
			try {
				// Start the grabbing of images until grabbing is stopped.
				m_camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
				m_camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
			}
			catch (Exception exception) {
				ShowException(exception);
			}
		}

		private void set_title()
		{
			string buf;
			int wid, hei;
			if (G.AS.DEBUG_MODE == 1) {
				buf = "CAM:DEBUG";
				//---
				G.CAM_WID = m_width = 2592;
				G.CAM_HEI = m_height = 1944;
				pictureBox1_Resize(null, null);
				cv_term();
				cv_init();
				update_sts_txt(1);
			}
			else if (m_caminfo == null) {
				buf = "CAM:OFFLINE";
				if (!string.IsNullOrEmpty(m_filename)) {
					buf += ":";
					buf += System.IO.Path.GetFileName(m_filename);
				}
				//m_width = m_height = 0;
			}
			else {
				wid = (int)m_camera.Parameters[PLCamera.Width].GetValue();
				hei = (int)m_camera.Parameters[PLCamera.Height].GetValue();
#if true
				buf = "";
#else
				buf = string.Format("CAM:{0}", m_caminfo[CameraInfoKey.FriendlyName]);
#endif
				//---
				G.CAM_WID = m_width = wid;
				G.CAM_HEI = m_height = hei;
				pictureBox1_Resize(null, null);
				cv_term();
				cv_init();
				update_sts_txt(1);
			}
			this.Text = buf;
		}
		// Updates the list of available camera devices.
		private void UpdateDeviceList()
		{
			try {
				// Ask the camera finder for a list of camera devices.
				List<ICameraInfo> allCameras = CameraFinder.Enumerate();
				if (allCameras.Count <= 0) {
					m_caminfo = null;
					//this.Text = "CAM:" + "OFFLINE";
					return;
				}
				m_caminfo = allCameras[0];
				//this.Text = "CAM:" + m_caminfo[CameraInfoKey.FullName];
				//this.Text = "CAM:" + m_caminfo[CameraInfoKey.FriendlyName];

				//Trace.WriteLineIf("DefaultGateway:" + m_caminfo[CameraInfoKey.DefaultGateway]);
				//Trace.WriteLineIf("DeviceCurrentIpConfiguration:" + m_caminfo[CameraInfoKey.DeviceCurrentIpConfiguration]);
				Trace.WriteLineIf((G.AS.TRACE_LEVEL & 4)!=0, "DeviceFactory:" + m_caminfo[CameraInfoKey.DeviceFactory]);
				Trace.WriteLineIf((G.AS.TRACE_LEVEL & 4)!=0, "DeviceGUID:" + m_caminfo[CameraInfoKey.DeviceGUID]);
				//Trace.WriteLineIf("FriendlyName:" + m_caminfo[CameraInfoKey.DeviceID]);
				Trace.WriteLineIf((G.AS.TRACE_LEVEL & 4)!=0, "DeviceIdx:" + m_caminfo[CameraInfoKey.DeviceIdx]);
				//Trace.WriteLineIf("FriendlyName:" + m_caminfo[CameraInfoKey.DeviceIpAddress]);
				//Trace.WriteLineIf("FriendlyName:" + m_caminfo[CameraInfoKey.DeviceIpConfigurationOptions]);
				//Trace.WriteLineIf("FriendlyName:" + m_caminfo[CameraInfoKey.DeviceMacAddress]);
				//Trace.WriteLineIf("FriendlyName:" + m_caminfo[CameraInfoKey.DeviceSocketAddress]);
				Trace.WriteLineIf((G.AS.TRACE_LEVEL & 4)!=0, "DeviceType:" + m_caminfo[CameraInfoKey.DeviceType]);
				//Trace.WriteLineIf("FriendlyName:" + m_caminfo[CameraInfoKey.DeviceVersion]);
				//Trace.WriteLineIf("FriendlyName:" + m_caminfo[CameraInfoKey.DriverKeyName]);
				Trace.WriteLineIf((G.AS.TRACE_LEVEL & 4)!=0, "FriendlyName:" + m_caminfo[CameraInfoKey.FriendlyName]);
				Trace.WriteLineIf((G.AS.TRACE_LEVEL & 4)!=0, "FullName:" + m_caminfo[CameraInfoKey.FullName]);
				Trace.WriteLineIf((G.AS.TRACE_LEVEL & 4)!=0, "VendorName:" + m_caminfo[CameraInfoKey.VendorName]);
			}
			catch (Exception exception) {
				ShowException(exception);
			}
		}


		// Shows exceptions in a message box.
		private void ShowException(Exception exception)
		{
			MessageBox.Show("Exception caught:\n" + exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		// Occurs when a new camera has been selected in the list. Destroys the object of the currently opened camera device and
		// creates a new object for the selected camera device. After that, the connection to the selected camera device is opened.
		private void cam_open()
		{
			// Destroy the old camera object.
			if (m_camera != null) {
				DestroyCamera();
			}


			// Open the connection to the selected camera device.
			if (true) {
				// Get the attached device data.
				ICameraInfo selectedCamera = m_caminfo;
				try {
					// Create a new camera object.
					m_camera = new Camera(selectedCamera);

					m_camera.CameraOpened += Configuration.AcquireContinuous;

					// Register for the events of the image provider needed for proper operation.
					m_camera.ConnectionLost += OnConnectionLost;
					m_camera.CameraOpened += OnCameraOpened;
					m_camera.CameraClosed += OnCameraClosed;
					m_camera.StreamGrabber.GrabStarted += OnGrabStarted;
					m_camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
					m_camera.StreamGrabber.GrabStopped += OnGrabStopped;

					// Open the connection to the camera device.
					m_camera.Open();
					if (true) {
						double	fwid, fmax, fmin;
						int		iwid, imax, imin;
						get_param(CAM_PARAM.WIDTH, out fwid, out fmax, out fmin);
						iwid = (int)(fwid+0.5);
						imax = (int)(fmax+0.5);
						imin = (int)(fmin+0.5);
						if (iwid == (imax/2)) {
							m_size_mode = m_size_regi= 2;
						}
						else if (iwid == (imax/4)) {
							m_size_mode = m_size_regi = 4;
						}
						else {
							m_size_mode = m_size_regi = 1;
						}
						set_size_mode(m_size_regi, -1, -1);
					}
#if false//@@
					// Set the parameter for the controls.
					testImageControl.Parameter = camera.Parameters[PLCamera.TestImageSelector];
					pixelFormatControl.Parameter = camera.Parameters[PLCamera.PixelFormat];
#if true
					floatSliderUserControl1.Parameter = camera.Parameters[PLCamera.Gamma];
					floatSliderUserControl2.Parameter = camera.Parameters[PLCamera.BslContrast];
					floatSliderUserControl3.Parameter = camera.Parameters[PLCamera.BslBrightness];
					floatSliderUserControl4.Parameter = camera.Parameters[PLCamera.SharpnessEnhancement];
					camera.Parameters[PLCamera.Width].SetValue(640);
					camera.Parameters[PLCamera.Height].SetValue(480);
#else
                    widthSliderControl.Parameter = camera.Parameters[PLCamera.Width];
                    heightSliderControl.Parameter = camera.Parameters[PLCamera.Height];
#endif
					if (camera.Parameters.Contains(PLCamera.GainAbs)) {
						gainSliderControl.Parameter = camera.Parameters[PLCamera.GainAbs];
					}
					else {
						gainSliderControl.Parameter = camera.Parameters[PLCamera.Gain];
					}
					if (camera.Parameters.Contains(PLCamera.ExposureTimeAbs)) {
						exposureTimeSliderControl.Parameter = camera.Parameters[PLCamera.ExposureTimeAbs];
					}
					else {
						exposureTimeSliderControl.Parameter = camera.Parameters[PLCamera.ExposureTime];
					}
#endif
				}
				catch (Exception exception) {
					ShowException(exception);
				}
			}
		}
		public void cam_close()
		{
			m_bcontinuous = false;
			Stop();
			DestroyCamera();
		}
		//private void draw_rect(IplImage img, Point[] pts, CvScalar col, int thickness)
		//{
		//    //Cv.Line(img_a, pts[0], pts[1], Cv.RGB(0, 0, 255), 2);
		//    //Cv.Line(img_a, pts[1], pts[2], Cv.RGB(0, 0, 255), 2);
		//    //Cv.Line(img_a, pts[2], pts[3], Cv.RGB(0, 0, 255), 2);
		//    //Cv.Line(img_a, pts[3], pts[0], Cv.RGB(0, 0, 255), 2);
		//    CvPoint pt1 = new CvPoint(pts[0].X, pts[0].Y);
		//    CvPoint pt2 = new CvPoint(pts[1].X, pts[1].Y);
		//    CvPoint pt3 = new CvPoint(pts[2].X, pts[2].Y);
		//    CvPoint pt4 = new CvPoint(pts[3].X, pts[3].Y);
		//    Cv.Line(m_img_a, pt1, pt2, col, thickness);
		//    Cv.Line(m_img_a, pt2, pt3, col, thickness);
		//    Cv.Line(m_img_a, pt3, pt4, col, thickness);
		//    Cv.Line(m_img_a, pt4, pt1, col, thickness);
		//}
		//private void calc_circum(CvSeq<CvPoint> pos)
		//{
		//    CvBox2D box = Cv.MinAreaRect2(pos);
		//    CvPoint2D32f[] ptf = new CvPoint2D32f[4];
		//    CvPoint[] pts = new CvPoint[4];

		//    Cv.BoxPoints(box, out ptf);
		//    for (int i = 0; i < ptf.Length; i++) {
		//        G.IR.CIRCUM_PTF[i].X = ptf[i].X;
		//        G.IR.CIRCUM_PTF[i].Y = ptf[i].Y;
		//        //---
		//        G.IR.CIRCUM_PTS[i].X = (int)ptf[i].X;
		//        G.IR.CIRCUM_PTS[i].Y = (int)ptf[i].Y;
		//    }
		//}
		//static public double AXpB(double x1, double x2, double y1, double y2, double x)
		//{
		//    double A, B;
		//    double Y;
		//    if ((x2 - x1) == 0.0) {
		//        return (-99999);
		//    }
		//    else if (x == x1) {
		//        Y = y1;
		//    }
		//    else if (x == x2) {
		//        Y = y2;
		//    }
		//    else {
		//        A = (y2 - y1) / (x2 - x1);
		//        B = y1 - A * x1;
		//        Y = A * x + B;
		//    }
		//    return (Y);
		//}
		// Y = A1*X + B1
		// と
		// Y = A2*X + B2
		// 交点を求める
		static private PointF get_cross_p(double A1, double B1, double A2, double B2)
		{
			PointF P = new PointF();
			P.X = (float)((B2 - B1) / (A1 - A2));
			P.Y = (float)(A1 * P.X + B1);
			return (P);
		}
		// 傾きAで点Cを通る直線の
		//private double get_y_at_x(double A, PointF C, double x)
		//{
		//    double B, y;
		//    if (A == 0.0) {
		//        y = C.Y;
		//    }
		//    else {
		//        B = C.Y - A * C.X;
		//        y = A * x + B;
		//    }
		//    return (y);
		//}
		private struct DIAMR
		{
			public Point p1, p2;
			public double dx, dy;
			public double len;
			public bool ng;
			public int b_touch;
			//public int flag1;
			//public int flag2;
		};
		private static POINT P2P(Point pt)
		{
			POINT	p;
			p.x = pt.X;
			p.y = pt.Y;
			return(p);
		}
		// l1:上辺, l2:下辺, x:l1上のx座標
		// return
		//	l1,l2を両辺とする二等辺三角形の底辺を返す
		//  l3:底辺,p1:l1とl3の交点, p2:l2とl3の交点
		static private DIAMR get_diam(DIAMR l1, DIAMR l2, int x)
		{
			DIAMR l3 = new DIAMR();
			double r1 = Math.Atan2(l1.dy, l1.dx);
			double r2 = Math.Atan2(l2.dy, l2.dx);
			double r3, a3, b3, a2, b2, a1, b1;
			PointF p1 = new PointF(), p2 = new PointF();

			//l1辺のa1, b1
			a1 = l1.dy / l1.dx;
			b1 = l1.p1.Y - a1 * l1.p1.X;
			//l2辺のa2, b2
			a2 = l2.dy / l2.dx;
			b2 = l2.p1.Y - a2 * l2.p1.X;
			//r1,r2の正規化(-90~+90deg)
			if (r1 >= (Math.PI / 2)) {
				r1 -= (Math.PI);
			}
			if (r1 <= -(Math.PI / 2)) {
				r1 += +(Math.PI);
			}
			if (r2 >= (Math.PI / 2)) {
				r2 -= (Math.PI);
			}
			if (r2 <= -(Math.PI / 2)) {
				r2 += +(Math.PI);
			}
			//底辺の角度と傾き
			r3 = ((r1 + r2) / 2);
			a3 = (-1) / Math.Tan(r3);
			if (double.IsInfinity(a3)) {//底辺が90°?
				//l1辺上の点p1
				p1.X = x;
				p1.Y = (float)(a1 * x + b1);
				//l2辺上の点p2
				p2.X = x;
				p2.Y = (float)(a2 * x + b2);
			}
			else {
				//l1辺上の点p1
				p1.X = x;
				p1.Y = (float)(a1 * x + b1);
				//p1を通るl3のb3
				b3 = p1.Y - a3 * p1.X;
				//p2:l2, l3の交点
				p2 = get_cross_p(a2, b2, a3, b3);
			}
			l3.len = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
			l3.p1 = Point.Round(p1);
			l3.p2 = Point.Round(p2);
			return (l3);
		}
		// xがx1とx2の間にあるか
		private bool is_inter(double x1, double x2, double x)
		{
			if (x1 > x2) {
				double f = x1;
				x1 = x2;
				x2 = f;
			}
			return (x >= x1 && x <= x2);
		}
#if true
		private int next_i(int i, int cnt)
		{
			if (++i >= cnt) {
				i = 0;
			}
			return(i);
		}
		private int prev_i(int i, int cnt)
		{
			if (--i < 0) {
				i = cnt-1;
			}
			return(i);
		}
		static private void draw_marker(POINT pt)
		{
			RECT rt;
			rt.Left = pt.x - 4;
			rt.Top = pt.y-4;
			rt.Right = rt.Left + 9;
			rt.Bottom = rt.Top + 9;
			OCV_DRAW_RECT((Int32)IMG.IMG_A, ref rt, 0x00FFFF, -1);
		}
#if true
		private struct VERTEX
		{
			public int x, y;
			public bool ng;
			public int b_touch;
			//public int flag1;
			//public int flag2;
		};
		private void extend_edge(ArrayList ap, int i1, out int i_s, out int i_e)
		{
			int		cnt = ap.Count;
			int		i0 = prev_i(i1, cnt);
//			int		i1 = i1;
			int		i2 = next_i(i1, cnt);
			VERTEX v0 = (VERTEX)ap[i0];
			VERTEX v1 = (VERTEX)ap[i1];
			VERTEX v2 = (VERTEX)ap[i2];
			int		dx01 = (v0.x-v1.x);
			int		dy01 = (v0.y-v1.y);
			int		dx21 = (v2.x-v1.x);
			int		dy21 = (v2.y-v1.y);
			bool	b01 = true;
			if (false) {
			}
			else if (v0.ng && !v2.ng) {
				b01 = true;
			}
			else if (!v0.ng && v2.ng) {
				b01 = false;
			}
			else if ((dx01*dx01+dy01*dy01) < (dx21*dx21+dy21*dy21)) {
				b01 = true;
			}
			else {
				b01 = false;
			}
			if (b01) {
				i_s = i0;
				i_e = i1;
				v0.b_touch = v1.b_touch;
				v0.ng = false;
				ap[i0] = v0;
			}
			else {
				i_s = i1;
				i_e = i2;
				v2.b_touch = v1.b_touch;
				v2.ng = false;
				ap[i2] = v2;
			}
		}
		private double calc_diam2(int cnt/*IplImage img, Point[] pts*/)
		{
			//int cnt = pts.Length;
			ArrayList
					ar = new ArrayList();
			DIAMR[] dr = null;
			int xmin = int.MaxValue,
				xmax = int.MinValue,
				ymin = int.MaxValue,
				ymax = int.MinValue;
			int xlen, ylen;
			int MIN_OF_LINE = 175;
			int GAP_OF_EDGE =  50;
			int	i_ls = -1, i_le = -1, i_rs = -1, i_re = -1;
			int	x_bak;
			ArrayList	ap = new ArrayList();

			for (int i = 0; i < cnt; i++) {
				//頂点は左回りに格納されている
				POINT pt;
				VERTEX	vt = new VERTEX();
				OCV_GET_PTS(i, out pt);
				if (xmin > pt.x) {
					xmin = pt.x;
				}
				if (xmax < pt.x) {
					xmax = pt.x;
				}
				if (ymin > pt.y) {
					ymin = pt.y;
				}
				if (ymax < pt.y) {
					ymax = pt.y;
				}
				vt.x = pt.x;
				vt.y = pt.y;
				ap.Add(vt);
			}
			//右端(？)を見つける
			for (int i = 0; i < cnt; i++) {
				int x = ((VERTEX)ap[i]).x;
				if ((xmax - x) < GAP_OF_EDGE && (xmax - x) < GAP_OF_EDGE) {
					i_re = i;break;
				}
			}
			//左端(始)を見つける
			for (int i = i_re;; i = next_i(i, cnt)) {
				int x = ((VERTEX)ap[i]).x;
				if ((x - xmin) < GAP_OF_EDGE && (x - xmin) < GAP_OF_EDGE) {
					i_ls = i;
					break;
				}
			}
			//左端(終)を見つける
			for (int i = i_re;; i = prev_i(i, cnt)) {
				int x = ((VERTEX)ap[i]).x;
				if ((x - xmin) < GAP_OF_EDGE && (x - xmin) < GAP_OF_EDGE) {
					i_le = i;
					break;
				}
			}
			//右端(始)を見つける
			for (int i = i_le;; i = next_i(i, cnt)) {
				int x = ((VERTEX)ap[i]).x;
				if ((xmax - x) < GAP_OF_EDGE && (xmax - x) < GAP_OF_EDGE) {
					i_rs = i;
					break;
				}
			}
			//右端(終)を見つける
			for (int i = i_ls;; i = prev_i(i, cnt)) {
				int x = ((VERTEX)ap[i]).x;
				if ((xmax - x) < GAP_OF_EDGE && (xmax - x) < GAP_OF_EDGE) {
					i_re = i;
					break;
				}
			}
			//--- エッジ中の異常頂点を除去
			for (int i = i_ls; ; i = next_i(i, cnt)) {
				VERTEX vt = (VERTEX)ap[i];
				if ((vt.x - xmin) < GAP_OF_EDGE && (vt.x - xmin) < GAP_OF_EDGE) {
					vt.b_touch = 1;
				}
				else {
					vt.ng = true;
				}
				ap[i] = vt;
				if (i == i_le) {
					break;
				}
			}
			for (int i = i_rs; ; i = next_i(i, cnt)) {
				VERTEX vt = (VERTEX)ap[i];
				if ((xmax - vt.x) < GAP_OF_EDGE && (xmax - vt.x) < GAP_OF_EDGE) {
					vt.b_touch = 2;
				}
				else {
					vt.ng = true;
				}
				ap[i] = vt;
				if (i == i_re) {
					break;
				}
			}
			x_bak = ((VERTEX)ap[i_le]).x;
			//--- 左端エッジから右端エッジへ
			for (int i = next_i(i_le, cnt); i != i_rs; i = next_i(i, cnt)) {
				VERTEX vt = (VERTEX)ap[i];
				if (vt.x < x_bak) {
					vt.ng = true;
					ap[i] = vt;
				}
				else {
					x_bak = vt.x;
				}
			}
			x_bak = ((VERTEX)ap[i_re]).x;
			//--- 右端エッジから左端エッジへ
			for (int i = next_i(i_re, cnt); i != i_ls; i = next_i(i, cnt)) {
				VERTEX vt = (VERTEX)ap[i];
				if (vt.x > x_bak) {
					vt.ng = true;
					ap[i] = vt;
				}
				else {
					x_bak = vt.x;
				}
			}
			//--- エッジ判定の頂点が１つだけのとき
			if (i_le == i_ls) {
				extend_edge(ap, i_ls, out i_ls, out i_le);
			}
			if (i_re == i_rs) {
				extend_edge(ap, i_rs, out i_rs, out i_re);
			}

			if (true) {
				int i = i_ls;
				VERTEX vi = (VERTEX)ap[i];
				DIAMR d0 = new DIAMR();
				d0.ng = false;
				d0.p1 = new Point(vi.x, vi.y);

				for (i = next_i(i, cnt);; ) {
					//頂点は左回りに格納されている
					VERTEX vh = (VERTEX)ap[i];
					if (vh.ng == false) {
						d0.dx = vh.x - vi.x;
						d0.dy = vh.y - vi.y;
						d0.len = Math.Sqrt(d0.dx*d0.dx + d0.dy*d0.dy);
						//--- 短い線分をカットする
						if (/*(m_chk == 0) ||*/ d0.len > MIN_OF_LINE || vh.b_touch != 0) {
							d0.p1 = new Point(vi.x, vi.y);
							d0.p2 = new Point(vh.x, vh.y);
							if (vi.b_touch == 1 && vh.b_touch == 1) {
								d0.b_touch = 1;
							}
							else if (vi.b_touch == 2 && vh.b_touch == 2) {
								d0.b_touch = 2;
							}
							else {
								d0.b_touch = 0;
							}
							ar.Add(d0);
							vi = vh;
						}
					}
					if (i == i_ls) {
						break;
					}
					i = next_i(i, cnt);
				}
			}
			cnt = ar.Count;
			i_ls = i_le = i_rs = i_re = -1;
			for (int i = 0; i < cnt; i++) {
				DIAMR d0;
				d0 = (DIAMR)ar[i];
				if (d0.b_touch == 1) {
					if (i_ls < 0) {
						i_ls = i;
					}
					i_le = i;
				}
				if (d0.b_touch == 2) {
					if (i_rs < 0) {
						i_rs = i;
					}
					i_re = i;
				}
				/*
				if ((d0.p1.X - xmin) < GAP_OF_EDGE && (d0.p2.X - xmin) < GAP_OF_EDGE) {
					if (i_ls < 0) {
						i_ls = i;
					}
					i_le = i;
				}
				if ((xmax - d0.p1.X) < GAP_OF_EDGE && (xmax - d0.p2.X) < GAP_OF_EDGE) {
					if (i_rs < 0) {
						i_rs = i;
					}
					i_re = i;
				}*/
				if (i < G.IR.PLY_PTS.Length) {
					G.IR.PLY_PTS[i].X = d0.p1.X;
					G.IR.PLY_PTS[i].Y = d0.p1.Y;
				}
			}
			G.IR.PLY_CNT = cnt;
			//---
			xlen = xmax - xmin;
			ylen = ymax - ymin;
			//---
			G.IR.PLY_XMAX = xmax;
			G.IR.PLY_XMIN = xmin;
			G.IR.PLY_YMAX = ymax;
			G.IR.PLY_YMIN = ymin;
			//---
			cnt = ar.Count;
			//---
			for (int i = 0; i < cnt; i++) {
				DIAMR d = (DIAMR)ar[i];
				/*
				 * mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK2
				 */
				//if (G.SS.CAM_CIR_CHK4 && (G.CAM_PRC == G.CAM_STS.STS_HAIR || G.CAM_PRC == G.CAM_STS.STS_AUTO || (G.CAM_PRC == G.CAM_STS.STS_FCUS && G.SS.CAM_FCS_PAR1 > 1))) {
				if (G.SS.CAM_CIR_CHK2 && (G.CAM_PRC == G.CAM_STS.STS_HAIR || G.CAM_PRC == G.CAM_STS.STS_AUTO || (G.CAM_PRC == G.CAM_STS.STS_FCUS && G.SS.CAM_FCS_PAR1 > 1))) {
					//Cv.DrawLine(m_img_a, P2P(d.p1), P2P(d.p2), Cv.RGB(0, 255, 0), 4);
					POINT p1, p2;
					p1 = P2P(d.p1);
					p2 = P2P(d.p2);
					if (false) {
					p1.y -= 500;
					p2.y -= 500;
					}
					if (true) {
					OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p1, ref p2, 0x008000, 4);
					}
					else if (d.b_touch != 0) {
						OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p1, ref p2, 0xff0000, 4);
					}
					else {
						OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p1, ref p2, 0xC0C000, 4);
					}
					//draw_marker(p1);
				}
			}
			cnt = ar.Count;
			dr = (DIAMR[])ar.ToArray(typeof(DIAMR));
			if (cnt <= 1) {
				cnt = 0;//for break.point
				return (double.NaN);
			}
			G.IR.DIA_CNT = 0;
			//--- 
			int dcnt = 0;
			double dlen = 0;
			for (int i = 0; i < 10; i++) {
//				int x = xmin + (i + 1) * xlen / 11;
				int gap = xlen/20;
				int x = xmin + gap + i * (xlen-2*gap) / 9;
				int h, j;
				bool b1 = false, b2 = false;

				for (h = i_le; h != i_rs; h = next_i(h, cnt)) {
					if (is_inter(dr[h].p1.X, dr[h].p2.X, x)) {
						b1 = true;
						break;// HIT
					}
				}
				for (j = i_re; j != i_ls; j = next_i(j, cnt)) {
					if (is_inter(dr[j].p1.X, dr[j].p2.X, x)) {
						b2 = true;
						break;// HIT
					}
				}
				if (!b1 || !b2) {
					continue;
				}
				DIAMR dm = get_diam(dr[h], dr[j], x);
				if (true) {
					G.IR.DIA_TOP[G.IR.DIA_CNT] = dm.p1;
					G.IR.DIA_BTM[G.IR.DIA_CNT] = dm.p2;
					if (G.IR.DIA_CNT > 0 && (G.IR.DIA_TOP[G.IR.DIA_CNT-1].X >= dm.p1.X || G.IR.DIA_BTM[G.IR.DIA_CNT-1].X >= dm.p2.X)) {
					G.IR.DIA_CNT=G.IR.DIA_CNT;
					}
					else {
					G.IR.DIA_CNT++;
					}
				}
				if (G.SS.CAM_CIR_CHK4 && (G.CAM_PRC == G.CAM_STS.STS_HAIR || G.CAM_PRC == G.CAM_STS.STS_AUTO || (G.CAM_PRC == G.CAM_STS.STS_FCUS && G.SS.CAM_FCS_PAR1 > 1))) {
					//img.DrawLine(P2P(dm.p1), P2P(dm.p2), Cv.RGB(0, 255, 255), 3);
					POINT p1, p2;
					p1 = P2P(dm.p1);
					p2 = P2P(dm.p2);
					//p1.x = dm.p1.X;
					//p1.y = dm.p1.Y;
					//p2.x = dm.p2.X;
					//p2.y = dm.p2.Y;

					OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p1, ref p2, 0xFFFF00, 4);
				}
				dlen += dm.len;
				dcnt++;
			}
			return(dlen / dcnt);
		}
#endif
#endif

		public double PX2UM(double px)
		{
			//const
			//double pitch = 2.2;//um/px
			double pitch = G.SS.CAM_SPE_UMPPX;
			double	Y, P;
			Y = G.SS.ZOM_PLS_A * G.PLM_POS[3] + G.SS.ZOM_PLS_B;
			P = px* pitch/Y;	//[um]
			return (P);
		}
#if false
		private void calc_hist(IplImage img_g, IplImage img_m, double[] fval, out double fmin, out double fmax, out double favg)
		{
			CvHistogram hist;
			int[] hist_size = { 256 };
			float[] range_0 = { 0, 256 };
			float[][] ranges = { range_0 };

			
			hist = Cv.CreateHist(hist_size, HistogramFormat.Array, ranges, true);
			if (img_m != null) {
				Cv.CalcHist(img_g, hist, false, img_m);
			}
			else {
				Cv.CalcHist(img_g, hist);
			}
			int cnt = 0;
			double f, sum = 0;
			
			fmin = fmax = favg = double.NaN;
			
			for (int i = 0; i < 256; i++) {
				f = Cv.QueryHistValue_1D(hist, i);
				fval[i] = f;
				cnt += (int)f;
				sum += (i * f);
				if (f > 0) {
					//contrast計算用,画素値の最小と最大
					if (double.IsNaN(fmin)) {
						fmin = i;
					}
					if (true) {
						fmax = i;
					}
				}
			}
			f = m_width * m_height;
			if (cnt != f) {
				f = 0;//検算:FOR BREAK POINT
			}
			favg = sum / cnt;
			Cv.ReleaseHist(hist);
		}
		private void calc_hist(IplImage img_g, IplImage img_m, double[] fval)
		{
			double	fmin, fmax, favg;
			calc_hist(img_g, img_m, fval, out fmin, out fmax, out favg);
		}
#endif
		enum IMG {
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
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_TERM();

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_RESET_MASK(Int32 x, Int32 y, Int32 w, Int32 h);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_RESET(Int32 wid, Int32 hei);//, Int32 mx, Int32 my, Int32 mw, Int32 mh);

		[DllImport("IMGSUB.DLL")]
		private static extern Int32 OCV_SET_IMG(IntPtr ptr, int wid, int hei, int str, int bpp);

		[DllImport("IMGSUB.DLL")]
		private static extern Int32 OCV_GET_IMG(IntPtr ptr, int wid, int hei, int str, int bpp);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_TO_GRAY(Int32 I, Int32 H);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_TO_HSV(Int32 I, Int32 H);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_MERGE(Int32 H1, Int32 H2, Int32 H3, Int32 I);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_SPLIT(Int32 I, Int32 H1, Int32 H2, Int32 H3);

		[DllImport("IMGSUB.DLL")]
		static extern void OCV_SMOOTH(Int32 I, Int32 cof);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_THRESH_BIN(Int32 I, Int32 H, Int32 thval, Int32 inv);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_THRESH_HSV(Int32 I1, Int32 I2, Int32 I3, Int32 H, Int32 minh, Int32 maxh, Int32 mins, Int32 maxs, Int32 minv, Int32 maxv);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_CAL_HIST(Int32 I, Int32 bMASK, ref double pval, out double pmin, out double pmax, out double pavg);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_PUTTEXT(Int32 I, string buf, Int32 x, Int32 y, Int32 c);

		/*
		抽出モード:
				0:CV_RETR_EXTERNAL:最も外側の輪郭のみ抽出
				1:CV_RETR_LIST	:全ての輪郭を抽出し，リストに追加
				2:CV_RETR_CCOMP	:全ての輪郭を抽出し，二つのレベルを持つ階層構造を構成する．
								:1番目のレベルは連結成分の外側の境界線，
								:2番目のレベルは穴（連結成分の内側に存在する）の境界線．
				3:CV_RETR_TREE	:全ての輪郭を抽出し，枝分かれした輪郭を完全に表現する階層構造を構成する．
		 */
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_FIND_FIRST(Int32 I, Int32 mode);

		[DllImport("IMGSUB.DLL")]
		private static extern IntPtr OCV_FIND_NEXT(IntPtr pos, Int32 smax, Int32 smin, Int32 lmax, Int32 lmin, double cmax, double cmin, out double ps, out double pl, out double pc);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_FIND_TERM();

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_DRAW_CONTOURS(Int32 I, IntPtr pos, Int32 c1, Int32 c2);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_DRAW_CONTOURS2(Int32 I, IntPtr pos, Int32 c1, Int32 c2, Int32 thickness);

		[DllImport("IMGSUB.DLL")]
		private static extern Int32 OCV_CONTOURS_CNT(IntPtr pos);
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_CONTOURS_PTS(IntPtr pos, Int32 idx, out POINT p);
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_FIT_LINE(IntPtr pos, out float f);

		[DllImport("IMGSUB.DLL")]
		private static extern Int32 OCV_APPROX_PTS(IntPtr pos, Int32 bSIGNE, Int32 PREC);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_GET_PTS(int idx, out POINT p);
			//private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);
		private struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
		private struct POINT { public int x; public int y;}

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_DRAW_LINE(int I, ref POINT p1, ref POINT p2, Int32 c, Int32 thick);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_DRAW_RECT(int I, ref RECT pr, Int32 c, Int32 thickness);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_BOUNDING_RECT(IntPtr pos, out RECT pr);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_DRAW_TEXT(int I, int x, int y, string buf, Int32 c);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_MIN_AREA_RECT2(IntPtr pos, out POINT p1, out POINT p2, out POINT p3, out POINT p4);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_FILL_POLY(int I, ref POINT p, Int32 n, Int32 c);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_ZERO(int I);
		
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_SOBEL(Int32 I, Int32 H, Int32 xorder, Int32 yorder, Int32 apert_size);
		
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_LAPLACE(Int32 I, Int32 H, Int32 apert_size);
		
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_CANNY(Int32 I, Int32 H, double th1, double th2, Int32 apert_size);
		
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_MINMAX(Int32 I, ref double pmin, ref double pmax);
		
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_SCALE(Int32 I, Int32 H, double scale, double shift);
		
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_SMOOTH2(Int32 I, Int32 cof, double sig1, double sig2);
		
		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_DIFF(Int32 I, Int32 H, Int32 J);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_TO_01(Int32 I, Int32 ZERO_VAL, Int32 NONZERO_VAL);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_THINNING(Int32 I, Int32 H, Int32 cnt);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_COPY(Int32 I, Int32 H);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_NOT(Int32 I, Int32 H);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_ERODE(Int32 I, Int32 H, Int32 kernel_size, Int32 cnt);

		[DllImport("IMGSUB.DLL")]
		private static extern void OCV_DILATE(Int32 I, Int32 H, Int32 kernel_size, Int32 cnt);

		
		private static int PF2BPP(PixelFormat pf)
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
		//自動測定:計算範囲:
		//0:画像全体
		//1:毛髪矩形+0%
		//2:毛髪矩形+25%
		//3:毛髪矩形+50%
		//4:毛髪矩形+100%
		//---------------------
		// ヒストグラム
		//画像全体
		//矩形範囲
		//毛髪矩形+0%
		//毛髪矩形+25%
		//毛髪矩形+50%
		//毛髪矩形+100%
		//毛髪範囲10%
		//毛髪範囲25%
		//毛髪範囲50%
		//毛髪範囲75%
		//毛髪範囲100%
		//---------------------
		//AF:最大化・変数
		//Contrast(全体)
		//Contrast(矩形)
		//抽出領域・S
		//抽出領域・L
		//抽出領域・径
		//---------------------
		//---------------------

		public void set_mask_by_result()
		{
			G.SS.CAM_HIS_RT_X = G.IR.CIR_RT.Left;
			G.SS.CAM_HIS_RT_Y = G.IR.CIR_RT.Top;
			G.SS.CAM_HIS_RT_W = G.IR.CIR_RT.Width;
			G.SS.CAM_HIS_RT_H = G.IR.CIR_RT.Height;
			//---
			if (G.CNT_MOD < 2) {
				if (m_chk2 == 0) {
					m_chk2 = 1;
					G.mlog("internal error#1");
				}
				return;
			}
			//---
			//int AFMD = 0;
			//if (G.CAM_PRC == G.CAM_STS.STS_AUTO) {
			//    AFMD = G.CNT_MOD-1;
			//}
			//else /*if (G.CAM_PRC == G.CAM_STS.STS_HIST)*/ {
			//    AFMD = G.CNT_MOD-2;
			//}

			if (G.CNT_MOD == 2) {
				//矩形範囲このまま
				reset_mask_rect();
			}
			else if (G.CNT_MOD >= 3 && G.CNT_MOD <= 5) {
				//矩形範囲拡大
				double p;
				switch (G.CNT_MOD) {
				case 3:
					p = 0.25;
					break;
				case 4:
					p = 0.5;
					break;
				default:
					p = 1.0;
					break;
				}
				int d = (int)(G.SS.CAM_HIS_RT_H * p);

				G.SS.CAM_HIS_RT_Y -= d / 2;
				G.SS.CAM_HIS_RT_H += d;
				if (G.SS.CAM_HIS_RT_Y < 0) {
					G.SS.CAM_HIS_RT_Y = 0;
				}
				if ((G.SS.CAM_HIS_RT_Y + G.SS.CAM_HIS_RT_H) > G.CAM_HEI) {
					G.SS.CAM_HIS_RT_H = G.CAM_HEI - G.SS.CAM_HIS_RT_Y;
				}
				reset_mask_rect();
			}
			else {
				//多曲線マスクの作成
				reset_mask_poly(G.CNT_MOD-6);
			}
		}
		private void post_proc()
		{
			string buf1 = null, buf2 = null;
			int disp;
			G.CAM_STS mode = G.CAM_PRC;
			int tk;
			G.IR.clear();
			G.IR.WIDTH = this.m_width;
			G.IR.HEIGHT = this.m_height;
			if (G.CAM_PRC == G.CAM_STS.STS_NONE) {
				return;
			}
			if (G.CAM_PRC == G.CAM_STS.STS_CUTI) {
				post_proc_cuti();
				return;
			}
			//---
			//G.IR.CIR_CNT = 0;g.ir.clear()にて
			//---
			switch (G.CAM_PRC) {
			case G.CAM_STS.STS_HIST://ヒストグラム表示実行中
				disp = G.SS.CAM_HIS_DISP;
				break;
			case G.CAM_STS.STS_HAIR://毛髪判定実行中
				disp = G.SS.CAM_CIR_DISP;
				break;
			case G.CAM_STS.STS_FCUS://オートフォーカス実行中
				disp = G.SS.CAM_FCS_DISP;
				switch (G.SS.CAM_FCS_PAR1) {
				case 0:
				//case 1:
					mode = G.CAM_STS.STS_HIST;//ヒストグラムに
					break;
				default:
					mode = G.CAM_STS.STS_HAIR;//円形度に
					break;
				}
				break;
			case G.CAM_STS.STS_AUTO://自動測定
				disp = 0;
				mode = G.CAM_STS.STS_HAIR;//円形度
				break;
			case G.CAM_STS.STS_ATIR:
				disp = 0;
				mode = G.CAM_STS.STS_NONE;
				break;
			default:
				disp = 0;
				mode = G.CAM_STS.STS_NONE;
				//G.mlog("(TT;");
				break;
			}
			if (m_bmpR.Tag == null) {
				//生画像
#if true
				int ret;
				BitmapData bmpData = m_bmpR.LockBits(new Rectangle(0, 0, m_bmpR.Width, m_bmpR.Height), ImageLockMode.ReadWrite, m_bmpR.PixelFormat);
				ret = OCV_SET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
				m_bmpR.UnlockBits(bmpData);
#else
				m_img_a.CopyFrom(m_bmpR);
#endif
				//グレースケール画像
				//Cv.CvtColor(m_img_a, m_img_g, ColorConversion.RgbaToGray);
				OCV_TO_GRAY((int)IMG.IMG_A, (int)IMG.IMG_G);
			}
			else {
				//m_bmpR, m_img_a, m_img_gはセット済
			}
			if (G.SS.CAM_CND_MODH == 1) {
				tk = Environment.TickCount;
				OCV_TO_HSV((int)IMG.IMG_A, (int)IMG.IMG_H);//Cv.CvtColor(m_img_a, m_img_h, ColorConversion.BgrToHsv);
				if ((tk = Environment.TickCount - tk) > 150) {
					tk = tk;
				}
			}
			if ((mode == G.CAM_STS.STS_HIST || mode == G.CAM_STS.STS_HAIR) && G.SS.CAM_CIR_FILT > 0) {
				//フィルタ適用
				int[] cofs = { 3, 5, 7, 9, 11 };
				int cof = cofs[G.SS.CAM_CIR_FILT - 1];
				
				tk = Environment.TickCount;
				if (G.SS.CAM_CND_MODH == 1) {
					OCV_SMOOTH((int)IMG.IMG_H, cof);//Cv.Smooth(m_img_h, m_img_h, SmoothType.Gaussian, cof, cof, 0, 0);
				}
				else {
					OCV_SMOOTH((int)IMG.IMG_G, cof);//Cv.Smooth(m_img_g, m_img_g, SmoothType.Gaussian, cof, cof, 0, 0);
				}
				if ((tk = Environment.TickCount - tk) > 150) {
					tk = tk;
				}
			}
			/*
			 * 二値化(ＲＧＢによる)
			 */
			if (G.SS.CAM_CND_MODH == 0) {
				//int th_val = (mode == 1) ? G.SS.CAM_HIS_BVAL : G.SS.CAM_CIR_BVAL;
				int th_val = G.SS.CAM_HIS_BVAL;
				if (true) {
					OCV_THRESH_BIN((int)IMG.IMG_G, (int)IMG.IMG_B, th_val, /*INV=*/1);
					//Cv.Threshold(m_img_g, m_img_b, th_val, 255, ThresholdType.BinaryInv);	//白背景に黒丸の時は反転しておく
				}
			}
			if (G.SS.CAM_CND_MODH == 1) {
				OCV_SPLIT((int)IMG.IMG_H, (int)IMG.IMG_HSV_H, (int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_V);
				//Cv.Split(m_img_h, m_img_hsv[0], m_img_hsv[1], m_img_hsv[2], null);
			}
			if (false
			  || (mode == G.CAM_STS.STS_HIST && (G.SS.CAM_CND_MODH == 0 || G.SS.ETC_HIS_MODE == 0))
			  || (mode == G.CAM_STS.STS_HAIR && (G.SS.CAM_CND_MODH == 0))
				) {
				OCV_SPLIT((int)IMG.IMG_A, (int)IMG.IMG_RGB_B, (int)IMG.IMG_RGB_G, (int)IMG.IMG_RGB_R);// m_img_a:BGRの順
			}
			/*
			 * 二値化(ＨＳＶによる)
			 */
			if (G.SS.CAM_CND_MODH == 1) {
				tk = Environment.TickCount;
				OCV_THRESH_HSV((int)IMG.IMG_HSV_H, (int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_V, (int)IMG.IMG_B,
					G.SS.CAM_CND_MINH/2, G.SS.CAM_CND_MAXH/2,
					G.SS.CAM_CND_MINS, G.SS.CAM_CND_MAXS,
					G.SS.CAM_CND_MINV, G.SS.CAM_CND_MAXV
					);

				if ((tk = Environment.TickCount - tk) > 100) {
					tk = tk;
				}
			}
			if (false /*G.SS.TST_PAR_CHK1*/) {
				int tmp = G.SS.TST_PAR_VAL1;
				if ((tmp%2) == 0) {
					tmp--;
				}
				if (tmp < 1) {
					tmp = 1;
				}
				OCV_SOBEL((int)IMG.IMG_G, (int)IMG.IMG_D, 1, 1, tmp);
				OCV_MERGE((int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_A);
			}
			if (false) {
				double f1 = 0, f2 = 0;
				//double sig1=G.SS.TST_PAR_DBL1, sig2=G.SS.TST_PAR_DBL2;
				int tmp1 = G.SS.TST_PAR_VAL1;
				int tmp2 = G.SS.TST_PAR_VAL2;
				tmp1 = 3+tmp1*2;
				tmp2 = 3+tmp2*2;
				OCV_TO_GRAY((int)IMG.IMG_A, (int)IMG.IMG_G);
				OCV_TO_GRAY((int)IMG.IMG_A, (int)IMG.IMG_T);
				OCV_SMOOTH2((int)IMG.IMG_G, tmp1, 0, 0);
				OCV_SMOOTH2((int)IMG.IMG_T, tmp2, 0, 0);
				OCV_DIFF((int)IMG.IMG_G, (int)IMG.IMG_T, (int)IMG.IMG_D);
				OCV_MINMAX((int)IMG.IMG_D, ref f1, ref f2);
				OCV_SCALE((int)IMG.IMG_D,(int)IMG.IMG_T, (255.0/(f2-f1)), -f1);
				OCV_MINMAX((int)IMG.IMG_T, ref f1, ref f2);
				f1 = f1;
				OCV_THRESH_BIN((int)IMG.IMG_T, (int)IMG.IMG_D, G.SS.TST_PAR_VAL3, /*INV=*/1);
				//---
				OCV_MERGE((int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_A);
			}
			if (false) {
				int tmp = G.SS.TST_PAR_VAL4;
				if ((tmp%2) == 0) {
					tmp--;
				}
				if (tmp < 1) {
					tmp = 1;
				}
				OCV_CANNY((int)IMG.IMG_G, (int)IMG.IMG_D, G.SS.TST_PAR_DBL1, G.SS.TST_PAR_DBL2, tmp);
				OCV_MERGE((int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_A);
			}
			/*if (G.SS.TST_PAR_CHK2) {
				int tmp = G.SS.TST_PAR_VAL2;
				if ((tmp%2) == 0) {
					tmp--;
				}
				if (tmp < 1) {
					tmp = 1;
				}
				OCV_LAPLACE((int)IMG.IMG_G, (int)IMG.IMG_D, tmp);
				OCV_MERGE((int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_A);
			}*/
			if (true) {
				disp = disp;
				switch (disp) {
				case 1:
					if (m_chk3 == 1) {
					OCV_MERGE((int)IMG.IMG_HSV_H, (int)IMG.IMG_HSV_H, (int)IMG.IMG_HSV_H, (int)IMG.IMG_A);
					m_chk3 = 2;
					}
					else if (m_chk3 == 2) {
					OCV_MERGE((int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_S, (int)IMG.IMG_A);
					m_chk3 = 3;
					}
					else if (m_chk3 == 3) {
					OCV_MERGE((int)IMG.IMG_HSV_V, (int)IMG.IMG_HSV_V, (int)IMG.IMG_HSV_V, (int)IMG.IMG_A);
					m_chk3 = 1;
					}
					else {
					OCV_MERGE((int)IMG.IMG_G, (int)IMG.IMG_G, (int)IMG.IMG_G, (int)IMG.IMG_A);
					}
					//Cv.Merge(m_img_g, m_img_g, m_img_g, null, m_img_a);
					break;
				case 2:
					OCV_MERGE((int)IMG.IMG_B, (int)IMG.IMG_B, (int)IMG.IMG_B, (int)IMG.IMG_A);
					//Cv.Merge(m_img_b, m_img_b, m_img_b, null, m_img_a);
					break;
				case 3:
					//OCV_MERGE((int)IMG.IMG_M, (int)IMG.IMG_M, (int)IMG.IMG_M, (int)IMG.IMG_A);
					//OCV_MERGE((int)IMG.IMG_HSV_H, (int)IMG.IMG_HSV_H, (int)IMG.IMG_HSV_H, (int)IMG.IMG_A);
					//OCV_MERGE((int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_S, (int)IMG.IMG_A);
					OCV_MERGE((int)IMG.IMG_HSV_V, (int)IMG.IMG_HSV_V, (int)IMG.IMG_HSV_V, (int)IMG.IMG_A);
				break;
				}
			}
			/*
			 * 毛髪判定
			 */
			if (mode == G.CAM_STS.STS_HAIR || G.CAM_PRC == G.CAM_STS.STS_FCUS || (G.CAM_PRC == G.CAM_STS.STS_HIST && G.CNT_MOD >= 2)) {
				OCV_FIND_FIRST((Int32)IMG.IMG_B, /*0:CV_RETR_EXTERNAL*/0);

				if (true) {
					IntPtr pos = (IntPtr)0;//, bak = (IntPtr)(-1);
					double s, l, c;

					for (;;) {
						//for (; pos != null; pos = pos.HNext) {
						pos = OCV_FIND_NEXT(pos,
								G.SS.CAM_CIR_AREA_MAX, G.SS.CAM_CIR_AREA,
								G.SS.CAM_CIR_LENG_MAX, G.SS.CAM_CIR_LENG,
								G.SS.CAM_CIR_CVAL, G.SS.CAM_CIR_CVAL_MIN,
								out s, out l, out c);
						if (pos == (IntPtr)0) {
							break;
						}
						//double s = Cv.ContourArea(pos);
						//double l = Cv.ArcLength(pos);
						int bSIGNE = (s < 0) ? 1 : 0;
						//円形度＝4π×（面積）÷（周囲長）^2(1:真円,正方形:0.785,正三角形:0.604)
						//double c = 4 * Math.PI * Math.Abs(s) / Math.Pow(l, 2);
						double p = double.NaN;
						//CvRect rc;
						RECT	rc;
						s = Math.Abs(s);
						//c = Math.Abs(c);

						//if (fmax < s) {
						//    fmax = s;
						//    pmax = pos;
						//}
						//if (bSIGNE) {
						//    bSIGNE = true;//左回り
						//}
						//else {
						//    bSIGNE = false;//右回り
						//}
						if (true) {
							//CHK1:輪郭, CHK2:多曲線, CHK3:特徴値, CHK4:毛髪径
							//輪郭の描画
							if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK1) {
								//Cv.DrawContours(m_img_a, pos, Cv.RGB(255, 0, 0), Cv.RGB(0, 0, 255), 0, 2);//Cv.FILLED);
								OCV_DRAW_CONTOURS((Int32)IMG.IMG_A, pos, 0x0000FF, 0xFF0000);
							}
							if (G.IR.CIR_CNT > 0 && s < G.IR.CIR_S) {
								G.IR.CIR_CNT++;
								continue;
							}
							if (false) {
								POINT	p1, p2, p3, p4;
								OCV_MIN_AREA_RECT2(pos, out p1, out p2, out p3, out p4);

								OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p1, ref p2, 0xc08000, 4);
								OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p2, ref p3, 0xc08000, 4);
								OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p3, ref p4, 0xc08000, 4);
								OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p4, ref p1, 0xc08000, 4);
							}
							//多曲線と毛髪径
							if (true) {
								//CvSeq<CvPoint> tmp;
								int n;
								//tmp = Cv.ApproxPoly(pos, CvContour.SizeOf, null, ApproxPolyMethod.DP, G.SS.CAM_DIR_PREC);
								//n = tmp.Count();
								n = OCV_APPROX_PTS(pos, bSIGNE, G.SS.CAM_DIR_PREC);
								if (n >= 4) {
									//Point[] pts = new Point[n];
									//for (int i = 0; i < n; i++) {
									//    if (bSIGNE) {//左回り時は順番の入れ替え
									//        pts[i] = P2P((CvPoint)tmp[n - 1 - i]);
									//    }
									//    else {
									//        pts[i] = P2P((CvPoint)tmp[i]);
									//    }
									//}
									//多曲線の描画
									if (false /*mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK2*/) {
										for (int i = 0; i < n; i++) {
											int h = (i == (n - 1)) ? 0 : i + 1;
											POINT p1, p2;
											OCV_GET_PTS(i, out p1);
											OCV_GET_PTS(h, out p2);
											OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p1, ref p2, 0x008000, 4);
											//Cv.DrawLine(m_img_a, P2P(pts[i]), P2P(pts[h]), Cv.RGB(0, 128, 0), 4);
										}
									}
									//毛髪径
									try {
										p = calc_diam2(n/*m_img_a, pts*/);
									}
									catch (Exception ex) {
										Trace.WriteLineIf((G.AS.TRACE_LEVEL & 4)!=0, ex.Message);
									}
									p = PX2UM(p);
									//多曲線の接続点の描画
									if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK2) {
										for (int i = 0; i < n; i++) {
											//CvPoint p1 = (CvPoint)tmp[i];
											//CvRect rt = new CvRect(p1.X - 4, p1.Y - 4, 9, 9);
											//Cv.DrawRect(m_img_a, rt, Cv.RGB(255, 255, 0), -1);
											POINT	pt;
											RECT	rt;
											OCV_GET_PTS(i, out pt);
											#if true
											draw_marker(pt);
											#else
											rt.Left = pt.x - 4;
											rt.Top = pt.y-4;
											rt.Right = rt.Left + 9;
											rt.Bottom = rt.Top + 9;
											OCV_DRAW_RECT((Int32)IMG.IMG_A, ref rt, 0x00FFFF);
											#endif
										}
									}
								}
							}
							if (true) {
								//RECT	rt;
								OCV_BOUNDING_RECT(pos, out rc);
								//rc = Cv.BoundingRect(pos);
							}
							//特徴値
							if (true) {
								string buf="";
								if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK3) {
									//	string buf = string.Format("S={0:F0},L={1:F0},C={2:F2},P={3:F0}p", s, l, c, p);
									buf += string.Format("S={0:F0},L={1:F0},C={2:F2}", s, l, c);
								}
								if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK4) {
									if (buf.Length > 0) {
										buf += ",";
									}
									if (double.IsNaN(p)) {
										buf += string.Format("P=NaN");
									}
									else {
										buf += string.Format("P={0:F0}u", p);
									}
								}
								if (buf.Length > 0) {
									//draw_text(m_img_a, rc.Left + (rc.Right-rc.Left) / 2, rc.Top + (rc.Bottom-rc.Top) / 2, buf);
									//OCV_DRAW_TEXT((Int32)IMG.IMG_A, rc.Left + (rc.Right - rc.Left) / 2, rc.Top + (rc.Bottom - rc.Top) / 2, buf, 0x00FF00);
									buf2 = buf;
									//OCV_PUTTEXT((Int32)IMG.IMG_A, buf, 50, 100, 0x00FF00);
								}
							}
							if (G.IR.CIR_CNT <= 0 || G.IR.CIR_S < s) {
								G.IR.CIR_S = s;
								G.IR.CIR_L = l;
								G.IR.CIR_C = c;
								G.IR.CIR_P = p;
								if (G.IR.CIR_RT != null) {
								}
								G.IR.CIR_RT = new Rectangle(rc.Left, rc.Top, (rc.Right-rc.Left), (rc.Bottom-rc.Top));
							}
							G.IR.CIR_CNT++;
						}
					}
					//輪郭に隣接する矩形の取得
					//CvRect		rt = Cv.BoundingRect(pmax);
					//fmax = fmax;
					//contours.Dispose();
					//contours = null;
				}
				//m_bmpZ = img_b.ToBitmap();
				//メモリストレージの解放
				//Cv.ReleaseMemStorage(storage);
				//storage = null;
				OCV_FIND_TERM();
			}
			//
			//ヒストグラム/コントラスト計算
			//
			G.IR.HIST_ALL = G.IR.HIST_RECT = false;
			//
			if (mode == G.CAM_STS.STS_HIST || G.CAM_PRC == G.CAM_STS.STS_FCUS || G.CAM_PRC == G.CAM_STS.STS_AUTO || G.CAM_PRC == G.CAM_STS.STS_ATIR) {
				int bMASK;
				if (G.CAM_PRC == G.CAM_STS.STS_AUTO) {
					if (G.IR.CIR_CNT <= 0 || G.CNT_MOD == 0) {
						G.IR.HIST_ALL = true;
					}
					else {
						G.IR.HIST_ALL = false;
						set_mask_by_result();
					}
				}
				else if (G.CAM_PRC == G.CAM_STS.STS_HIST && G.CNT_MOD == 1) {
					G.IR.HIST_RECT = true;
				}
				else if (G.CAM_PRC == G.CAM_STS.STS_HIST && G.CNT_MOD >= 2) {
					if (G.IR.CIR_CNT <= 0) {
						G.IR.HIST_ALL = true;
					}
					else {
						G.IR.HIST_ALL = false;
						set_mask_by_result();
					}
				}
				else if ((G.CAM_PRC == G.CAM_STS.STS_HIST && G.CNT_MOD == 1/*矩形範囲*/)
					  || (G.CAM_PRC == G.CAM_STS.STS_FCUS && G.SS.CAM_FCS_PAR1 == 0/*CONTRAST*/ && G.CNT_MOD >= 1/*矩形範囲 or 毛髪矩形 or 毛髪範囲*/)) {
					//現在のマスクを継続して使用
					G.IR.HIST_ALL = false;
				}
				else {
					G.IR.HIST_ALL = true;
				}
				bMASK = (G.IR.HIST_ALL) ? 0 : 1;
				//calc_hist(m_img_g, mask, G.IR.HISTVALY, out G.IR.HIST_MIN, out G.IR.HIST_MAX, out G.IR.HIST_AVG);
				OCV_CAL_HIST((int)IMG.IMG_G, bMASK, ref G.IR.HISTVALY[0], out G.IR.HIST_MIN, out G.IR.HIST_MAX, out G.IR.HIST_AVG);

				if (G.CAM_PRC == G.CAM_STS.STS_HIST || this.groupBox2.Visible) {
					double tmp;
					if (G.SS.ETC_HIS_MODE == 0) {
						//calc_hist(m_img_rgb[0], mask, G.IR.HISTVALR);
						//calc_hist(m_img_rgb[1], mask, G.IR.HISTVALG);
						//calc_hist(m_img_rgb[2], mask, G.IR.HISTVALB);
						OCV_CAL_HIST((int)IMG.IMG_RGB_R, bMASK, ref G.IR.HISTVALR[0], out tmp, out tmp, out tmp);
						OCV_CAL_HIST((int)IMG.IMG_RGB_G, bMASK, ref G.IR.HISTVALG[0], out tmp, out tmp, out tmp);
						OCV_CAL_HIST((int)IMG.IMG_RGB_B, bMASK, ref G.IR.HISTVALB[0], out tmp, out tmp, out tmp);
					}
					else {
						//calc_hist(m_img_hsv[0], mask, G.IR.HISTVALH);
						//calc_hist(m_img_hsv[1], mask, G.IR.HISTVALS);
						//calc_hist(m_img_hsv[2], mask, G.IR.HISTVALV);
						OCV_CAL_HIST((int)IMG.IMG_HSV_H, bMASK, ref G.IR.HISTVALH[0], out tmp, out tmp, out tmp);
						OCV_CAL_HIST((int)IMG.IMG_HSV_S, bMASK, ref G.IR.HISTVALS[0], out tmp, out tmp, out tmp);
						OCV_CAL_HIST((int)IMG.IMG_HSV_V, bMASK, ref G.IR.HISTVALV[0], out tmp, out tmp, out tmp);
					}
				}
				if (false) {
					double tmp, fsum = 0, fttl = 0;
					OCV_SOBEL((int)IMG.IMG_G, (int)IMG.IMG_D, 1, 1, 3);
					OCV_CAL_HIST((int)IMG.IMG_D, bMASK, ref G.IR.HISTVALD[0], out tmp, out tmp, out tmp);
					for (int i = 0; i < 256; i++) {
						fsum += (i * G.IR.HISTVALD[i]);
						fttl += G.IR.HISTVALD[i];
					}
					//G.IR.CONTRAST = fsum / (255*fttl/2);
					G.IR.CONTRAST = fsum / (127.5*fttl/2);
					G.IR.CONTRAST*= 10;//小さすぎるため根拠なく10倍
					//---
					//OCV_MERGE((int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_A);
				}
				else
				if (G.SS.CAM_FCS_USSD) {
					//標準偏差(最大値127.5^2で正規化)
					// 255, 0 = avg=127.5
					// 
					int imin = (int)(G.IR.HIST_MIN+0.5);
					int imax = (int)(G.IR.HIST_MAX+0.5);
					int icnt = 0;
					double fsum = 0;
					for (int i = imin; i <= imax; i++) {
						fsum += G.IR.HISTVALY[i] * Math.Pow(i-G.IR.HIST_AVG, 2);
						icnt += (int)(G.IR.HISTVALY[i]+0.5);
					}
					G.IR.CONTRAST = fsum / icnt / (127.5*127.5);
					G.IR.CONTRAST = Math.Sqrt(G.IR.CONTRAST);
					//G.IR.CONTRAST = Math.Log10(G.IR.CONTRAST);
				}
				else {
					//double fttl = 0;
					//for (int i = 0; i < 256; i++) {
					//    fttl += G.IR.HISTVALY[i];
					//}
					G.IR.CONTRAST = (G.IR.HIST_MAX - G.IR.HIST_MIN) / (G.IR.HIST_MAX + G.IR.HIST_MIN);
				}
				//---
				//---
				string buf = "";
				if ((G.CAM_PRC == G.CAM_STS.STS_HIST && G.SS.CAM_HIS_CHK1) || (G.CAM_PRC == G.CAM_STS.STS_FCUS && G.SS.CAM_FCS_PAR1 <= 1) || (G.CAM_PRC == G.CAM_STS.STS_AUTO)) {
					buf += string.Format("CONTRAST={0:F3}", G.IR.CONTRAST);
				}
				if (G.CAM_PRC == G.CAM_STS.STS_HIST && G.SS.CAM_HIS_CHK2) {
					if (!string.IsNullOrEmpty(buf)) {
						buf += ",";
					}
					buf += string.Format("MIN,MAX,AVG={0:F0},{1:F0},{2:F0}", G.IR.HIST_MIN, G.IR.HIST_MAX, G.IR.HIST_AVG);
				}
				if (!string.IsNullOrEmpty(buf)) {
					buf1 = buf;
					//OCV_PUTTEXT((Int32)IMG.IMG_A, buf, 50, 100, 0x00FF00);
					//CvPoint pnt = new CvPoint(50, 100);
					//Cv.PutText(m_img_a, buf, pnt, fnt, Cv.RGB(0, 255, 0));
				}
			}
			if (!string.IsNullOrEmpty(buf1) || !string.IsNullOrEmpty(buf2)) {
				if (string.IsNullOrEmpty(buf1)) {
					buf1 = buf2;
					buf2 = null;
				}
				if (false) {
					if (!string.IsNullOrEmpty(buf2)) {
						buf1 += ",";
						buf1 += buf2;
					}
					OCV_PUTTEXT((Int32)IMG.IMG_A, buf1, 50, 100, 0x00FF00);
				}
				else {
					if (!string.IsNullOrEmpty(buf1)) {
						OCV_PUTTEXT((Int32)IMG.IMG_A, buf1, 50, 100, 0x00FF00);
					}
					if (!string.IsNullOrEmpty(buf2)) {
						OCV_PUTTEXT((Int32)IMG.IMG_A, buf2, 50, 200, 0x00FF00);
					}
				}
			}
			if (true) {
#if true
				if (m_bmpZ != null && (m_bmpZ.Width != m_width || m_bmpZ.Height != m_height)) {
				    m_bmpZ.Dispose();
				    m_bmpZ = null;
				}
				//if (m_bmpZ.GetHbitmap() == IntPtr.Zero) {
				//    m_bmpZ = null;
				//}
				if (m_bmpZ == null) {
					m_bmpZ = new Bitmap(m_width, m_height, PixelFormat.Format24bppRgb);
				}
				BitmapData bmpData = m_bmpZ.LockBits(new Rectangle(0, 0, m_bmpZ.Width, m_bmpZ.Height), ImageLockMode.ReadWrite, m_bmpZ.PixelFormat);
				int ret;
				ret = OCV_GET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
				m_bmpZ.UnlockBits(bmpData);
#else

				if (m_bmpZ != null) {
					m_bmpZ.Dispose();
					m_bmpZ = null;
				}
				m_bmpZ = m_img_a.ToBitmap();
#endif
				//m_bmpB = img_b.ToBitmap();
				//m_bmpG = img_g.ToBitmap();
			}
			//---
//			if (G.CAM_PRC == G.CAM_STS.STS_AUTO) {
//				G.FORM11.BeginInvoke(new G.DLG_VOID_VOID(G.FORM11.CALLBACK));
//			}
//			else {
				G.FORM12.BeginInvoke(new G.DLG_VOID_VOID(G.FORM12.CALLBACK));
//			}
		}
		void make_curve (double  sigma, int length)
		{
			int[]   curve;
			double  sigma2;
			double  l;
			int     temp;
			int     i, n;
			int		o;

			sigma2 = 2 * sigma * sigma;
			l = Math.Sqrt (-sigma2 * Math.Log (1.0 / 255.0));

			n = (int)(Math.Ceiling(l) * 2);
			if ((n % 2) == 0)
			n += 1;

			curve = new int[n];

			length = n / 2;
			o = length;
			curve[o+0] = 255;

			for (i = 1; i <= length; i++)
			{
				temp = (int) (Math.Exp(- (i * i) / sigma2) * 255);
				curve[o-i] = temp;
				curve[o+i] = temp;
			}

			//return curve;
		}
		private void post_proc_cuti()
		{
			string buf1 = null, buf2 = null;
			int disp;
			G.CAM_STS mode = G.CAM_PRC;
			int tk;
			//---
			if (true) {
				double radius = 1, std_dev;
				double log2_255=(2 * Math.Log(1.0 / 255.0));
				radius = Math.Abs(radius) + 1.0;
				std_dev = Math.Sqrt (-(radius * radius) / log2_255);
				make_curve (std_dev, 0);
			}
			if (true) {
				double radius = 5, std_dev;
				double log2_255=(2 * Math.Log(1.0 / 255.0));
				radius = Math.Abs(radius) + 1.0;
				std_dev = Math.Sqrt (-(radius * radius) / log2_255);
				make_curve (std_dev, 0);
			}
			//return;
			//G.IR.CIR_CNT = 0;g.ir.clear()にて
			//---
			if (m_bmpR.Tag == null) {
				//生画像
				int ret;
				BitmapData bmpData = m_bmpR.LockBits(new Rectangle(0, 0, m_bmpR.Width, m_bmpR.Height), ImageLockMode.ReadWrite, m_bmpR.PixelFormat);
				ret = OCV_SET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
				m_bmpR.UnlockBits(bmpData);
				//グレースケール画像
				//Cv.CvtColor(m_img_a, m_img_g, ColorConversion.RgbaToGray);
//				OCV_TO_GRAY((int)IMG.IMG_A, (int)IMG.IMG_G);
			}
			else {
				//m_bmpR, m_img_a, m_img_gはセット済
			}
			//if (false
			//  || (mode == G.CAM_STS.STS_HIST && (G.SS.CAM_CND_MODH == 0 || G.SS.ETC_HIS_MODE == 0))
			//  || (mode == G.CAM_STS.STS_HAIR && (G.SS.CAM_CND_MODH == 0))
			//    ) {
			//    OCV_SPLIT((int)IMG.IMG_A, (int)IMG.IMG_RGB_B, (int)IMG.IMG_RGB_G, (int)IMG.IMG_RGB_R);// m_img_a:BGRの順
			//}
			if (true) {
				OCV_TO_GRAY((int)IMG.IMG_A, (int)IMG.IMG_G);
				OCV_TO_GRAY((int)IMG.IMG_A, (int)IMG.IMG_T);
			}
			else {
				OCV_TO_HSV((int)IMG.IMG_A, (int)IMG.IMG_H);
				OCV_SPLIT((int)IMG.IMG_H, (int)IMG.IMG_HSV_H, (int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_V);
				OCV_COPY((int)IMG.IMG_HSV_V, (int)IMG.IMG_G);
				OCV_COPY((int)IMG.IMG_HSV_V, (int)IMG.IMG_T);
			}
			if (G.SS.TST_PAR_GAUS == 0) {
				//カーネル可変
				if (G.SS.TST_PAR_VAL1 > 0) {
					OCV_SMOOTH2((int)IMG.IMG_G, 1+G.SS.TST_PAR_VAL1*2, 0, 0);
				}
				if (G.SS.TST_PAR_VAL2 > 0) {
					OCV_SMOOTH2((int)IMG.IMG_T, 1+G.SS.TST_PAR_VAL2*2, 0, 0);
				}
			}
			else /*if (G.SS.TST_PAR_GAUS == 1)*/ {
				//カーネル固定
				if (G.SS.TST_PAR_VAL3 > 0) {
					OCV_SMOOTH2((int)IMG.IMG_G, 1+G.SS.TST_PAR_VAL3*2, G.SS.TST_PAR_DBL1, 0);
					OCV_SMOOTH2((int)IMG.IMG_T, 1+G.SS.TST_PAR_VAL3*2, G.SS.TST_PAR_DBL2, 0);
				}
			}
			//else {
			//    if (G.SS.TST_PAR_VAL7 > 4) {
			//        G.SS.TST_PAR_VAL7 = 4;
			//    }
			//    if (G.SS.TST_PAR_VAL7 < 1) {
			//        G.SS.TST_PAR_VAL7 = 1;
			//    }
			//    OCV_CANNY((int)IMG.IMG_G, (int)IMG.IMG_G, G.SS.TST_PAR_VAL5, G.SS.TST_PAR_VAL6, G.SS.TST_PAR_VAL7*2+1);
			//    OCV_ZERO((int)IMG.IMG_T);
			//}
			if (true) {
				double fmin = 0, fmax = 0;
				//---
				OCV_DIFF((int)IMG.IMG_G, (int)IMG.IMG_T, (int)IMG.IMG_D);
				//---
				OCV_MINMAX((int)IMG.IMG_D, ref fmin, ref fmax);
				OCV_SCALE((int)IMG.IMG_D,(int)IMG.IMG_HSV_H, (255.0/(fmax-fmin)), -fmin);
				OCV_MINMAX((int)IMG.IMG_HSV_H, ref fmin, ref fmax);//確認用
				fmin = fmin;
				OCV_THRESH_BIN((int)IMG.IMG_HSV_H, (int)IMG.IMG_HSV_S, G.SS.TST_PAR_VAL4, /*INV=*/0);

				OCV_COPY((int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_V);
			}
			for (int i = 0; i < 3; i++) {
				/*
				 0:膨張→収縮→細線
				 1:膨張→細線→収縮
				 2:収縮→膨張→細線
				 3:収縮→細線→膨張
				 4:細線→収縮→膨張
				 5:細線→膨張→収縮
				 */
				if ((i == 0 && (G.SS.TST_PAR_ORDR == 0 || G.SS.TST_PAR_ORDR == 1))
				 || (i == 1 && (G.SS.TST_PAR_ORDR == 2 || G.SS.TST_PAR_ORDR == 5))
				 || (i == 2 && (G.SS.TST_PAR_ORDR == 3 || G.SS.TST_PAR_ORDR == 4))) {
					//膨張
					if (G.SS.TST_PAR_DILA > 0) {
						OCV_DILATE((int)IMG.IMG_HSV_V, (int)IMG.IMG_HSV_V, 3, G.SS.TST_PAR_DILA);
					}
				}
				if ((i == 0 && (G.SS.TST_PAR_ORDR == 2 || G.SS.TST_PAR_ORDR == 3))
				 || (i == 1 && (G.SS.TST_PAR_ORDR == 0 || G.SS.TST_PAR_ORDR == 4))
				 || (i == 2 && (G.SS.TST_PAR_ORDR == 1 || G.SS.TST_PAR_ORDR == 5))) {
					//収縮
					if (G.SS.TST_PAR_EROD > 0) {
						OCV_ERODE((int)IMG.IMG_HSV_V, (int)IMG.IMG_HSV_V, 3, G.SS.TST_PAR_EROD);
					}
				}
				if ((i == 0 && (G.SS.TST_PAR_ORDR == 4 || G.SS.TST_PAR_ORDR == 5))
				 || (i == 1 && (G.SS.TST_PAR_ORDR == 1 || G.SS.TST_PAR_ORDR == 3))
				 || (i == 2 && (G.SS.TST_PAR_ORDR == 0 || G.SS.TST_PAR_ORDR == 2))) {
					////細線
					if (G.SS.TST_PAR_THIN > 0) {
						OCV_THINNING((int)IMG.IMG_HSV_V, (int)IMG.IMG_HSV_V, G.SS.TST_PAR_THIN);
					}
				}
			}
			/*
生画像
ガウス１
ガウス２
ガウス差分
ガウス差分正規化
２値化
何か…			 */
			if (true) {
				switch (G.SS.TST_PAR_DISP) {
				case 1://ガウス1
					OCV_MERGE((int)IMG.IMG_G, (int)IMG.IMG_G, (int)IMG.IMG_G, (int)IMG.IMG_A);
					break;
				case 2://ガウス2
					OCV_MERGE((int)IMG.IMG_T, (int)IMG.IMG_T, (int)IMG.IMG_T, (int)IMG.IMG_A);
					break;
				case 3://ガウス差分
					OCV_MERGE((int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_D, (int)IMG.IMG_A);
					break;
				case 4://ガウス差分正規化
					OCV_MERGE((int)IMG.IMG_HSV_H, (int)IMG.IMG_HSV_H, (int)IMG.IMG_HSV_H, (int)IMG.IMG_A);
					break;
				case 5://2値化
					OCV_MERGE((int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_S, (int)IMG.IMG_A);
					break;
				case 6://何か…
					OCV_MERGE((int)IMG.IMG_HSV_V, (int)IMG.IMG_HSV_V, (int)IMG.IMG_HSV_V, (int)IMG.IMG_A);
					break;
				}
			}
			if (true) {
				OCV_NOT((Int32)IMG.IMG_HSV_V, (Int32)IMG.IMG_B);
			}
			else {
				OCV_COPY((Int32)IMG.IMG_HSV_V, (Int32)IMG.IMG_B);
			}
			/*
			 * 毛髪判定
			 */
			if (true) {
				const
				int RETR_MODE = 2;
				switch (RETR_MODE) {
				case 0:
					OCV_FIND_FIRST((Int32)IMG.IMG_B, /*0:CV_RETR_EXTERNAL*/0);
				break;
				case 1:
					OCV_FIND_FIRST((Int32)IMG.IMG_B, /*1:CV_RETR_LIST*/1);
				break;
				case 2:
					OCV_FIND_FIRST((Int32)IMG.IMG_B, /*2:CV_RETR_CCOMP*/2);
				break;
				case 3:
					OCV_FIND_FIRST((Int32)IMG.IMG_B, /*3:CV_RETR_TREE*/3);
				break;
				}

				if (true) {
					IntPtr pos = (IntPtr)0;//, bak = (IntPtr)(-1);
					double s, l, c;

					for (;;) {
						//for (; pos != null; pos = pos.HNext) {
						pos = OCV_FIND_NEXT(pos,
								G.SS.TST_PAR_SMAX, G.SS.TST_PAR_SMIN,
								G.SS.TST_PAR_LMAX, G.SS.TST_PAR_LMIN,
								1, 0,
								out s, out l, out c);
						if (pos == (IntPtr)0) {
							break;
						}
						//if (s < 250 || s > 10000) {
						//    if (G.SS.TST_PAR_CHK2) {
						//        OCV_DRAW_CONTOURS2((Int32)IMG.IMG_A, pos, 0x000000, 0x000000, -1);
						//    }
						//    continue;
						//}
						//double s = Cv.ContourArea(pos);
						//double l = Cv.ArcLength(pos);
						int bSIGNE = (s < 0) ? 1 : 0;
						//円形度＝4π×（面積）÷（周囲長）^2(1:真円,正方形:0.785,正三角形:0.604)
						//double c = 4 * Math.PI * Math.Abs(s) / Math.Pow(l, 2);
						double p = double.NaN;
						//CvRect rc;
						RECT	rc;
						s = Math.Abs(s);
						//c = Math.Abs(c);

						//if (fmax < s) {
						//    fmax = s;
						//    pmax = pos;
						//}
						//if (bSIGNE) {
						//    bSIGNE = true;//左回り
						//}
						//else {
						//    bSIGNE = false;//右回り
						//}
						if (G.SS.TST_PAR_CHK1) {
							//輪郭の描画
							OCV_DRAW_CONTOURS((Int32)IMG.IMG_A, pos, 0x0000FF, 0xFF0000);
						}
						if (true) {
							//CHK1:輪郭, CHK2:多曲線, CHK3:特徴値, CHK4:毛髪径
							//if (G.IR.CIR_CNT > 0 && s < G.IR.CIR_S) {
							//    G.IR.CIR_CNT++;
							//    continue;
							//}
							if (false) {
								POINT	p1, p2, p3, p4;
								OCV_MIN_AREA_RECT2(pos, out p1, out p2, out p3, out p4);

								OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p1, ref p2, 0xc08000, 4);
								OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p2, ref p3, 0xc08000, 4);
								OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p3, ref p4, 0xc08000, 4);
								OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p4, ref p1, 0xc08000, 4);
							}
							//多曲線と毛髪径
							if (false) {
								//CvSeq<CvPoint> tmp;
								int n;
								//tmp = Cv.ApproxPoly(pos, CvContour.SizeOf, null, ApproxPolyMethod.DP, G.SS.CAM_DIR_PREC);
								//n = tmp.Count();
								n = OCV_APPROX_PTS(pos, bSIGNE, G.SS.CAM_DIR_PREC);
								if (n >= 4) {
									//Point[] pts = new Point[n];
									//for (int i = 0; i < n; i++) {
									//    if (bSIGNE) {//左回り時は順番の入れ替え
									//        pts[i] = P2P((CvPoint)tmp[n - 1 - i]);
									//    }
									//    else {
									//        pts[i] = P2P((CvPoint)tmp[i]);
									//    }
									//}
									//多曲線の描画
									if (false /*mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK2*/) {
										for (int i = 0; i < n; i++) {
											int h = (i == (n - 1)) ? 0 : i + 1;
											POINT p1, p2;
											OCV_GET_PTS(i, out p1);
											OCV_GET_PTS(h, out p2);
											OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p1, ref p2, 0x008000, 4);
											//Cv.DrawLine(m_img_a, P2P(pts[i]), P2P(pts[h]), Cv.RGB(0, 128, 0), 4);
										}
									}
									//毛髪径
									try {
										p = calc_diam2(n/*m_img_a, pts*/);
									}
									catch (Exception ex) {
										Trace.WriteLineIf((G.AS.TRACE_LEVEL & 4)!=0, ex.Message);
									}
									p = PX2UM(p);
									//多曲線の接続点の描画
									if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK2) {
										for (int i = 0; i < n; i++) {
											//CvPoint p1 = (CvPoint)tmp[i];
											//CvRect rt = new CvRect(p1.X - 4, p1.Y - 4, 9, 9);
											//Cv.DrawRect(m_img_a, rt, Cv.RGB(255, 255, 0), -1);
											POINT	pt;
											RECT	rt;
											OCV_GET_PTS(i, out pt);
											#if true
											draw_marker(pt);
											#else
											rt.Left = pt.x - 4;
											rt.Top = pt.y-4;
											rt.Right = rt.Left + 9;
											rt.Bottom = rt.Top + 9;
											OCV_DRAW_RECT((Int32)IMG.IMG_A, ref rt, 0x00FFFF);
											#endif
										}
									}
								}
							}
							if (true) {
								//RECT	rt;
								OCV_BOUNDING_RECT(pos, out rc);
								//rc = Cv.BoundingRect(pos);
							}
							//特徴値
							if (G.SS.TST_PAR_CHK3) {
								string buf="";
								if (true) {
									//	string buf = string.Format("S={0:F0},L={1:F0},C={2:F2},P={3:F0}p", s, l, c, p);
									//buf += string.Format("S={0:F0},L={1:F0},C={2:F2}", s, l, c);
									buf += string.Format("S={0:F0},L={1:F0}", s, l);
								}
								//if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK4) {
								//    if (buf.Length > 0) {
								//        buf += ",";
								//    }
								//    if (double.IsNaN(p)) {
								//        buf += string.Format("P=NaN");
								//    }
								//    else {
								//        buf += string.Format("P={0:F0}u", p);
								//    }
								//}
								if (buf.Length > 0) {
									//draw_text(m_img_a, rc.Left + (rc.Right-rc.Left) / 2, rc.Top + (rc.Bottom-rc.Top) / 2, buf);
									OCV_DRAW_TEXT((Int32)IMG.IMG_A, rc.Left + (rc.Right - rc.Left) / 2, rc.Top + (rc.Bottom - rc.Top) / 2, buf, 0x00FF00);
									buf2 = buf;
									//OCV_PUTTEXT((Int32)IMG.IMG_A, buf, 50, 100, 0x00FF00);
								}
							}
						}
					}
					//輪郭に隣接する矩形の取得
					//CvRect		rt = Cv.BoundingRect(pmax);
					//fmax = fmax;
					//contours.Dispose();
					//contours = null;
				}
				//m_bmpZ = img_b.ToBitmap();
				//メモリストレージの解放
				//Cv.ReleaseMemStorage(storage);
				//storage = null;
				OCV_FIND_TERM();
			}
			//if (mode == G.CAM_STS.STS_HIST || G.CAM_PRC == G.CAM_STS.STS_FCUS || G.CAM_PRC == G.CAM_STS.STS_AUTO || G.CAM_PRC == G.CAM_STS.STS_ATIR) {
			//    if ((G.CAM_PRC == G.CAM_STS.STS_HIST && G.SS.CAM_HIS_CHK1) || (G.CAM_PRC == G.CAM_STS.STS_FCUS && G.SS.CAM_FCS_PAR1 <= 1) || (G.CAM_PRC == G.CAM_STS.STS_AUTO)) {
			//        buf += string.Format("CONTRAST={0:F3}", G.IR.CONTRAST);
			//    }
			//    if (G.CAM_PRC == G.CAM_STS.STS_HIST && G.SS.CAM_HIS_CHK2) {
			//        if (!string.IsNullOrEmpty(buf)) {
			//            buf += ",";
			//        }
			//        buf += string.Format("MIN,MAX,AVG={0:F0},{1:F0},{2:F0}", G.IR.HIST_MIN, G.IR.HIST_MAX, G.IR.HIST_AVG);
			//    }
			//    if (!string.IsNullOrEmpty(buf)) {
			//        buf1 = buf;
			//        //OCV_PUTTEXT((Int32)IMG.IMG_A, buf, 50, 100, 0x00FF00);
			//        //CvPoint pnt = new CvPoint(50, 100);
			//        //Cv.PutText(m_img_a, buf, pnt, fnt, Cv.RGB(0, 255, 0));
			//    }
			//}
			if (false) {
				if (!string.IsNullOrEmpty(buf1) || !string.IsNullOrEmpty(buf2)) {
					if (string.IsNullOrEmpty(buf1)) {
						buf1 = buf2;
						buf2 = null;
					}
					if (false) {
						if (!string.IsNullOrEmpty(buf2)) {
							buf1 += ",";
							buf1 += buf2;
						}
						OCV_PUTTEXT((Int32)IMG.IMG_A, buf1, 50, 100, 0x00FF00);
					}
					else {
						if (!string.IsNullOrEmpty(buf1)) {
							OCV_PUTTEXT((Int32)IMG.IMG_A, buf1, 50, 100, 0x00FF00);
						}
						if (!string.IsNullOrEmpty(buf2)) {
							OCV_PUTTEXT((Int32)IMG.IMG_A, buf2, 50, 200, 0x00FF00);
						}
					}
				}
			}
			if (true) {
#if true
				if (m_bmpZ != null && (m_bmpZ.Width != m_width || m_bmpZ.Height != m_height)) {
				    m_bmpZ.Dispose();
				    m_bmpZ = null;
				}
				//if (m_bmpZ.GetHbitmap() == IntPtr.Zero) {
				//    m_bmpZ = null;
				//}
				if (m_bmpZ == null) {
					m_bmpZ = new Bitmap(m_width, m_height, PixelFormat.Format24bppRgb);
				}
				BitmapData bmpData = m_bmpZ.LockBits(new Rectangle(0, 0, m_bmpZ.Width, m_bmpZ.Height), ImageLockMode.ReadWrite, m_bmpZ.PixelFormat);
				int ret;
				ret = OCV_GET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
				m_bmpZ.UnlockBits(bmpData);
#endif
			}
			G.FORM12.BeginInvoke(new G.DLG_VOID_VOID(G.FORM12.CALLBACK));
		}

		static public void DO_SMOOTH(Bitmap bmp, int cof, int n)
		{
			int ret;
			BitmapData bmpData;
			bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
			ret = OCV_SET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
			bmp.UnlockBits(bmpData);
			//---
			for (int i = 0; i < n; i++) {
			OCV_SMOOTH((int)IMG.IMG_A, cof);//Cv.Smooth(m_img_h, m_img_h, SmoothType.Gaussian, cof, cof, 0, 0);
			}
			//---
			bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
			ret = OCV_GET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
			bmp.UnlockBits(bmpData);
		}

		private static double GET_KYOKURITSU(POINT p1, POINT p2, POINT p3)
		{
			//b2:p1.x, c2:p1.y
			//b3:p2.x, c3:p2.y
			//b4:p3.x, c4:p3.y
			double ox, oy, r;
			double p1x2 = p1.x * p1.x;
			double p2x2 = p2.x * p2.x;
			double p3x2 = p3.x * p3.x;
			double p1y2 = p1.y * p1.y;
			double p2y2 = p2.y * p2.y;
			double p3y2 = p3.y * p3.y;

			ox = (
			        ((p1x2 + p1y2)-(p2x2+p2y2))*(p2.y-p3.y)
			       -((p2x2 + p2y2)-(p3x2+p3y2))*(p1.y-p2.y)
			     )
			     /
			     (2*
			        ((p1.x - p2.x)*(p2.y-p3.y)-(p2.x-p3.x)*(p1.y-p2.y))
			     );
			//ox = (
			//        ((p1x2 + p1y2)-(p3x2+p2y2))*(p2.y-p3.y)
			//       -((p2x2 + p2y2)-(p3x2+p3y2))*(p1.y-p2.y)
			//     )
			//     /
			//     (2.0*
			//        ((p1.x-p2.x)*(p2.y-p3.y)-(p2.x-p3.x)*(p1.y-p2.y))
			//     );

			oy = (
					((p1y2+p1x2)-(p2y2+p2x2))*(p2.x-p3.x)
				   -((p2y2+p2x2)-(p3y2+p3x2))*(p1.x-p2.x)
				 )
				 /
				 (2.0*
					((p1.y-p2.y)*(p2.x-p3.x)-(p2.y-p3.y)*(p1.x-p2.x))
				 );
			r = Math.Sqrt(Math.Pow(p1.x - ox, 2) + Math.Pow(p1.y - oy, 2));

			return(1/r);
		}

		static public void DO_PROC_IR(Bitmap bi, out Bitmap bo)
		{
			int ret;
			BitmapData bmpData;

			G.IR.clear();
			//---
			bmpData = bi.LockBits(new Rectangle(0, 0, bi.Width, bi.Height), ImageLockMode.ReadWrite, bi.PixelFormat);
			ret = OCV_SET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
			bi.UnlockBits(bmpData);
			//---
			if (true) {
				OCV_SPLIT((int)IMG.IMG_A, (int)IMG.IMG_G, (int)IMG.IMG_HSV_S, (int)IMG.IMG_HSV_V);
			}
			//---

			//---
			if (G.SS.IMP_FLT_COEF[3] > 0) {
				//フィルタ適用
				int[] cofs = { 3, 5, 7, 9, 11 };
				int cof = cofs[G.SS.IMP_FLT_COEF[3]-1];
				
				OCV_SMOOTH((int)IMG.IMG_G, cof);
			}
			if (true) {
				int th_val = G.SS.IMP_BIN_BVAL[3];//135-5-5;
//				OCV_THRESH_BIN((int)IMG.IMG_G, (int)IMG.IMG_B, th_val, /*INV=*/1);	//白背景に黒丸の時は反転しておく
				OCV_THRESH_BIN((int)IMG.IMG_G, (int)IMG.IMG_B, th_val, /*INV=*/0);
			}
			if (G.SS.MOZ_IRC_DISP == 1) {
				OCV_MERGE((int)IMG.IMG_B, (int)IMG.IMG_B, (int)IMG.IMG_B, (int)IMG.IMG_A);
			}
			if (true) {
				OCV_FIND_FIRST((Int32)IMG.IMG_B, /*0:CV_RETR_EXTERNAL*/0);

				IntPtr pos = (IntPtr)0;//, bak = (IntPtr)(-1);
				double s, l, c, e, k=0;

				for (;;) {
					pos = OCV_FIND_NEXT(pos,
							G.SS.IMP_SUM_UPPR[3], G.SS.IMP_SUM_LOWR[3],
							G.SS.IMP_LEN_UPPR[3], G.SS.IMP_LEN_LOWR[3],
							G.SS.IMP_CIR_UPPR[3], G.SS.IMP_CIR_LOWR[3],
							out s, out l, out c);
					if (pos == (IntPtr)0) {
						break;
					}
					int bSIGNE = (s < 0) ? 1 : 0;
					//円形度＝4π×（面積）÷（周囲長）^2(1:真円,正方形:0.785,正三角形:0.604)
					//double p = double.NaN;
					RECT	rc;

					s = Math.Abs(s);
					//CHK1:輪郭, CHK2:多曲線, CHK3:特徴値, CHK4:毛髪径
					//輪郭の描画
					if (G.SS.MOZ_IRC_CK00) {
						OCV_DRAW_CONTOURS((Int32)IMG.IMG_A, pos, 0x0000FF, 0xFF0000);
					}
					//if (G.IR.CIR_CNT > 0 && s < G.IR.CIR_S) {
					//    G.IR.CIR_CNT++;
					//    continue;
					//}
					if (true) {
						int		cnt = OCV_CONTOURS_CNT(pos);
						POINT	p1, p2, p3;
						double	ttl = 0, f;

						p1.x = 1; p1.y = 2;
						p2.x = 3; p2.y = 7;
						p3.x = 5; p3.y = 4;
						f = GET_KYOKURITSU(p1, p2, p3);

						OCV_CONTOURS_PTS(pos, 0, out p1);
						OCV_CONTOURS_PTS(pos, 1, out p2);
						for (int i = 0+2; i < cnt; i++) {
							OCV_CONTOURS_PTS(pos, i, out p3);
							f = GET_KYOKURITSU(p1, p2, p3);
							if (!double.IsNaN(f)) {
								ttl += f;
							}
							p1 = p2;
							p2 = p3;
						}
						k = ttl;
					}
					if (true) {
						OCV_BOUNDING_RECT(pos, out rc);
						
					}
					if (true) {
						//Extentは外接矩形の面積に対する輪郭が占める面積の比です．
						//Extent = \frac{Object \; Area}{Bounding \; Rectangle \; Area}
						e = s/((rc.Right-rc.Left)*(rc.Bottom-rc.Top));
					}
					//多曲線と毛髪径
					if (k >= G.SS.IMP_CUV_LOWR[3] && k <= G.SS.IMP_CUV_UPPR[3]) {
						double u = k/l;
						if (u >= G.SS.IMP_GIZ_LOWR[3] && u <= G.SS.IMP_GIZ_UPPR[3]) {
							float[] fl = new float[4];
							OCV_FIT_LINE(pos, out fl[0]);
							//(vx, vy, x0, y0)の配列
							PointF pf1 = new PointF(fl[2], fl[3]);
							PointF pf2 = new PointF(pf1.X + fl[0]*100, pf1.Y+fl[1]*100);
							FN1D fn = new FN1D(pf1, pf2);
							POINT p1;
							POINT p2;
							int	xmin = (rc.Left < C.GAP_OF_IMG_EDGE) ? 0 : rc.Left;
							int	xmax = ((bi.Width - rc.Right) < C.GAP_OF_IMG_EDGE) ? (bi.Width-1) : rc.Right;

							pf1.X = xmin;
							pf1.Y = (float)fn.GetYatX(xmin);
							pf2.X = xmax;
							pf2.Y = (float)fn.GetYatX(xmax);
							//---
							p1 = P2P(Point.Round(pf1));
							p2 = P2P(Point.Round(pf2));
							//---
							OCV_DRAW_LINE((Int32)IMG.IMG_A, ref p1, ref p2, 0xFFFF00, 7);
							if (G.IR.EDG_CNT < G.IR.EDG_LFT.Length) {
								G.IR.EDG_LFT[G.IR.EDG_CNT] = pf1;
								G.IR.EDG_RGT[G.IR.EDG_CNT] = pf2;
								G.IR.EDG_CNT++;
							}
						}
					}

					//特徴値
					if (true) {
						string buf="";
						if (G.SS.MOZ_IRC_CK01) {
							buf += string.Format("S={0:F0},L={1:F0}", s, l);
						}
						if (G.SS.MOZ_IRC_CK02) {
							if (buf.Length > 0) {
								buf += ",";
							}
							buf += string.Format("C={0:F2}", c);
						}
						if (G.SS.MOZ_IRC_CK03) {
							if (buf.Length > 0) {
								buf += ",";
							}
							buf += string.Format("K={0:F0},K/L={1:F2}", k, k/l);
						}
						if (buf.Length > 0) {
							OCV_DRAW_TEXT((Int32)IMG.IMG_A, rc.Left + (rc.Right - rc.Left) / 2, rc.Top + (rc.Bottom - rc.Top) / 2, buf, 0x00FF00);
							//OCV_PUTTEXT((Int32)IMG.IMG_A, buf, 50, 100, 0x00FF00);
						}
					}
				}
				OCV_FIND_TERM();
			}
			//
			if (true) {
				bo = new Bitmap(bi.Width, bi.Height, PixelFormat.Format24bppRgb);
				bmpData = bo.LockBits(new Rectangle(0, 0, bo.Width, bo.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
				ret = OCV_GET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, PF2BPP(bmpData.PixelFormat));
				bo.UnlockBits(bmpData);
			}
			if (G.IR.EDG_CNT == 2) {
				FN1D ft, fb;
				double fmin, fmax, flen;
				int xmin, xmax, xlen;

				if (G.IR.EDG_LFT[0].Y < G.IR.EDG_LFT[1].Y) {
					ft = new FN1D(G.IR.EDG_LFT[0], G.IR.EDG_RGT[0]);
					fb = new FN1D(G.IR.EDG_LFT[1], G.IR.EDG_RGT[1]);
				}
				else {
					ft = new FN1D(G.IR.EDG_LFT[1], G.IR.EDG_RGT[1]);
					fb = new FN1D(G.IR.EDG_LFT[0], G.IR.EDG_RGT[0]);
				}
				fmin = (G.IR.EDG_LFT[0].X < G.IR.EDG_LFT[1].X) ? G.IR.EDG_LFT[0].X : G.IR.EDG_LFT[1].X;
				fmax = (G.IR.EDG_RGT[0].X > G.IR.EDG_RGT[1].X) ? G.IR.EDG_RGT[0].X : G.IR.EDG_RGT[1].X;
				flen = (fmax-fmin);
				xmin = (int)fmin;
				xmax = (int)fmax;
				xlen = xmax-xmin;
				//---
				G.IR.PLY_XMIN = xmin;
				G.IR.PLY_XMAX = xmax;

				G.IR.PLY_YMIN = (ft.P1.Y < ft.P2.Y) ?  (int)ft.P1.Y: (int)ft.P2.Y;
				G.IR.PLY_YMAX = (fb.P1.Y > fb.P2.Y) ?  (int)fb.P1.Y: (int)fb.P2.Y;
				//--- 
				//int dcnt = 0;
				//double dlen = 0;
				DIAMR dt = new DIAMR();
				DIAMR db = new DIAMR();
				//---
				dt.dx = ft.P2.X-ft.P1.X;
				dt.dy = ft.P2.Y-ft.P1.Y;
				dt.p1 = Point.Round(ft.P1);
				dt.p2 = Point.Round(ft.P2);
				//---
				db.dx = fb.P2.X-fb.P1.X;
				db.dy = fb.P2.Y-fb.P1.Y;
				db.p1 = Point.Round(fb.P1);
				db.p2 = Point.Round(fb.P2);
				//---
				for (int i = 0; i < 10; i++) {
	//				int x = xmin + (i + 1) * xlen / 11;
					int gap = xlen/20;
					int x = xmin + gap + i * (xlen-2*gap) / 9;
					//int h, j;
					//bool b1 = false, b2 = false;

					DIAMR dm = get_diam(dt, db, x);
					if (true) {
						G.IR.DIA_TOP[G.IR.DIA_CNT] = dm.p1;
						G.IR.DIA_BTM[G.IR.DIA_CNT] = dm.p2;
						if (G.IR.DIA_CNT > 0 && (G.IR.DIA_TOP[G.IR.DIA_CNT-1].X >= dm.p1.X || G.IR.DIA_BTM[G.IR.DIA_CNT-1].X >= dm.p2.X)) {
						G.IR.DIA_CNT=G.IR.DIA_CNT;
						}
						else {
						G.IR.DIA_CNT++;
						}
					}
				}
				G.IR.CIR_CNT = 1;
			}
		}

		private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			//Debug.WriteLine(string.Format("{0}:pictureBox1_MouseDoubleClick", Environment.TickCount));

			//if (e.Button == System.Windows.Forms.MouseButtons.Right) {
				//if (++m_mode_disp > 3) {
				//    m_mode_disp = 0;
				//}
				//disp_bmp(true);
			//}
			if (e.Button == System.Windows.Forms.MouseButtons.Left) {
				if (e.X < 10 && e.Y < 10) {
					DO_SMOOTH(m_bmpR, 11, 1);
					//---
					for (int i = 0; i < 1; i++) {
						Console.Beep(1600, 250);
						Thread.Sleep(250);
					}
				}
				post_proc();
				disp_bmp(true);
			}
			else if (e.Button == System.Windows.Forms.MouseButtons.Right) {
				m_chk1 = 1-m_chk1;
				post_proc();
				disp_bmp(true);
			}
		}
		private void disp_bmp(bool bUpdate)
		{
#if true
			Bitmap bmp = m_bmpR;
			int disp = 0;
#else
			Bitmap bmp;
			int disp;

			//if ((bmp = (Bitmap)this.pictureBox1.Image) != null) {
			//    this.pictureBox1.Image = null;
			//    bmp.Dispose();
			//    bmp = null;
			//}
#endif
			switch (G.CAM_PRC) {
			case G.CAM_STS.STS_HIST:
				disp = G.SS.CAM_HIS_DISP;
				break;
			case G.CAM_STS.STS_HAIR:
				disp = G.SS.CAM_CIR_DISP;
				break;
			case G.CAM_STS.STS_FCUS:
				disp = G.SS.CAM_FCS_DISP;
				break;
			case G.CAM_STS.STS_AUTO:
				disp = 0;
				break;
			case G.CAM_STS.STS_CUTI:
				disp = 0;
				break;
			default:
				disp = -1;
				break;
			}

			switch (disp) {
			case 1://グレースケール画像
				bmp = m_bmpZ;//m_bmpG;
				break;
			case 2://２値化画像
				bmp = m_bmpZ;//m_bmpB;
				break;
			case 3://コントラスト計算対象画像
				bmp = m_bmpZ;
			break;
			case 0://生画像
				bmp = m_bmpZ;
				break;
			default://生画像
				bmp = m_bmpR;//m_bmpR;
				break;
			}
#if true
			//if (this.pictureBox1.Image != null) {
			//    this.pictureBox1.Image.Dispose();
			//    this.pictureBox1.Image = null;
			//}
			this.pictureBox1.Image = bmp;
#else
			if (bmp != null && bmp.GetHbitmap() != null) {
				this.pictureBox1.Image = bmp;
			}
#endif
			if (bUpdate) {
				//this.pictureBox1.Update();
			}
			if (m_bNeedToTakeImgBounds) {
				m_bNeedToTakeImgBounds = false;
				pictureBox1_Resize(null, null);
			}
			else {
				disp_extr(null);
			}
		}
		private void draw_mask_line(Graphics gr)
		{
			for (int i = 0; i < G.IR.MSK_PLY_CNT; i++) {
				int	h = (i == G.IR.MSK_PLY_CNT-1) ? 0: i+1;
				gr.DrawLine(Pens.Blue, G.IR.MSK_PLY_IMG[i], G.IR.MSK_PLY_IMG[h]);
				gr.DrawRectangle(Pens.Yellow, G.IR.MSK_PLY_IMG[i].X-1, G.IR.MSK_PLY_IMG[i].Y-1, 3, 3);
			}
		}
		private void draw_marker(Graphics gr, Pen pen, int x, int y)
		{
			gr.DrawEllipse(pen, x-C_HANKEI, y-C_HANKEI, C_HANKEI*2, C_HANKEI*2);
		}
		private void disp_extr(Graphics gr)
		{
			bool flag = false;
			if (gr == null) {
				flag = true;
				gr = this.pictureBox1.CreateGraphics();
			}
			if ((G.CAM_PRC == G.CAM_STS.STS_HIST || (G.CAM_PRC == G.CAM_STS.STS_FCUS && G.SS.CAM_FCS_PAR1 == 0))
			   && (G.CNT_MOD >= 1 && G.CNT_MOD <= 5)) {
				gr.DrawRectangle(Pens.Blue, m_rtCntImg);
				if (G.CNT_MOD == 1) {
//				draw_marker(gr, Pens.LightSkyBlue, m_rtCntImg.Left, m_rtCntImg.Top);
//				draw_marker(gr, Pens.LightSkyBlue, m_rtCntImg.Right, m_rtCntImg.Top);
//				draw_marker(gr, Pens.LightSkyBlue, m_rtCntImg.Right, m_rtCntImg.Bottom);
//				draw_marker(gr, Pens.LightSkyBlue, m_rtCntImg.Left, m_rtCntImg.Bottom);
				draw_marker(gr, Pens.LightSkyBlue, m_pt_of_marker[2].X, m_pt_of_marker[2].Y);
				draw_marker(gr, Pens.LightSkyBlue, m_pt_of_marker[3].X, m_pt_of_marker[3].Y);
				}
			}
			if ((G.CAM_PRC == G.CAM_STS.STS_AUTO) && (G.CNT_MOD >= 6)) {
				if (G.IR.HIST_ALL == false) {
					//draw_mask_line(gr); //デバック用
				}
			}
			else if ((G.CAM_PRC == G.CAM_STS.STS_HIST || (G.CAM_PRC == G.CAM_STS.STS_FCUS && G.SS.CAM_FCS_PAR1 == 0))
			  && (G.CNT_MOD >= 6)) {
				draw_mask_line(gr);
			}

			if (this.chart1.Visible) {
				gr.DrawLine(Pens.Red, m_rtDanImg.Left, m_rtDanImg.Top, m_rtDanImg.Right, m_rtDanImg.Bottom);
				draw_marker(gr, Pens.LightGreen, m_rtDanImg.Left, m_rtDanImg.Top);
				draw_marker(gr, Pens.LightGreen, m_rtDanImg.Right, m_rtDanImg.Bottom);
				set_danval();
			}
			if (this.toolStripMenuItem40.Checked) {
				Pen pen1 = new Pen(Color.DarkGreen);
				Pen pen2 = new Pen(Color.DarkGreen);

				pen2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
				pen2.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

				gr.DrawLine(pen2, m_pt_of_marker[4], m_pt_of_marker[5]);
				draw_marker(gr, Pens.LightGreen, m_pt_of_marker[4].X,m_pt_of_marker[4].Y);
				draw_marker(gr, Pens.LightGreen, m_pt_of_marker[5].X,m_pt_of_marker[5].Y);
				draw_marker(gr, Pens.LightGreen, m_pt_of_marker[6].X,m_pt_of_marker[6].Y);
				Font fnt = new Font("Arial", 20);
				RectangleF rt = m_rtImgBounds;
				StringFormat sf  = new StringFormat();
				string buf = string.Format("L={0:F1}um", PX2UM(m_calc_len.get_len()));
				Point pt = BMPCD_TO_IMGCD(m_calc_len.get_pt(3));
				
				sf.Alignment = StringAlignment.Far;
				sf.LineAlignment = StringAlignment.Far;
				//gr.DrawRectangle(Pens.Indigo, Rectangle.Round(rt));
				gr.DrawString(buf, fnt, Brushes.LimeGreen, rt, sf);
				gr.DrawLine(pen1/*Pens.Green*/, m_pt_of_marker[6], pt);
			}
			if (this.chart5.Visible) {
				set_hisval();
			}
			if (flag) {
				gr.Dispose();
				gr = null;
			}
		}
		private ImageFormat to_imf(string ext)
		{
			ImageFormat imf = null;
			if (false) {
			}
			else if (ext.Contains("bmp")) {
				imf = ImageFormat.Bmp;
			}
			else if (ext.Contains("png")) {
				imf = ImageFormat.Png;
			}
			else if (ext.Contains("jpg") || ext.Contains("jpeg")) {
				imf = ImageFormat.Jpeg;
			}
			else {
				imf = ImageFormat.Bmp;
			}
			return (imf);
		}
		private void load_image()
		{
			Image img = this.pictureBox1.Image;
			OpenFileDialog dlg = new OpenFileDialog();
			string path = G.SS.BEFORE_PATH;
			string ext;

			if (m_bcontinuous) {
				m_bcontinuous = false;
				Stop();
			}
			if (string.IsNullOrEmpty(path)) {
				ext = "bmp";
			}
			else {
				ext = System.IO.Path.GetExtension(path).ToLower();
				if (ext.Substring(0, 1) == ".") {
					ext = ext.Substring(1);
				}
			}
			dlg.Filter = G.filter_string();
			dlg.FilterIndex = G.to_fidx(ext);
			dlg.DefaultExt = ext;
			dlg.InitialDirectory = G.SS.BEFORE_PATH;
			if (string.IsNullOrEmpty(path)) {
			}
			else {
				dlg.FileName = System.IO.Path.GetFileName(path);
			}
			if (dlg.ShowDialog() != DialogResult.OK) {
				return;
			}
			try {
				load_file(dlg.FileName);
			}
			catch (Exception ex) {
				G.mlog("#e" + ex.Message);
			}
			G.SS.BEFORE_PATH = dlg.FileName;
		}
		private void save_image()
		{
			Image img = this.pictureBox1.Image;
			SaveFileDialog dlg = new SaveFileDialog();
			string path = G.SS.BEFORE_PATH;
			string ext;
			ImageFormat imf;
			if (m_bcontinuous) {
				m_bcontinuous = false;
				Stop();
			}
			if (string.IsNullOrEmpty(path)) {
				ext = "bmp";
			}
			else {
				ext = System.IO.Path.GetExtension(path).ToLower();
				if (ext.Substring(0, 1) == ".") {
					ext = ext.Substring(1);
				}
			}
			dlg.Filter = G.filter_string();
			dlg.FilterIndex = G.to_fidx(ext);
			dlg.DefaultExt = ext;
			dlg.InitialDirectory = G.SS.BEFORE_PATH;
			if (string.IsNullOrEmpty(path)) {
			}
			else {
				dlg.FileName = System.IO.Path.GetFileName(path);
			}
			if (dlg.ShowDialog() != DialogResult.OK) {
				return;
			}
			try {
				path = dlg.FileName;
				ext = System.IO.Path.GetExtension(path).ToLower();
				imf = to_imf(ext);
				img.Save(path, imf);
			}
			catch (Exception ex) {
				G.mlog("#e" + ex.Message);
			}
			G.SS.BEFORE_PATH = dlg.FileName;
		}
		
		public void save_image(string path)
		{
			Bitmap bmp = (Bitmap)m_bmpR.Clone();
			string ext;
			ImageFormat imf;

			try {
				ext = System.IO.Path.GetExtension(path).ToLower();
				imf = to_imf(ext);
				bmp.Save(path, imf);
			}
			catch (Exception ex) {
				G.mlog("#e" + ex.Message);
			}
			bmp.Dispose();
			bmp = null;
		}
		private void pictureBox1_Resize(object sender, EventArgs e)
		{
			try {
				//Debug.WriteLine(string.Format("{0}:pictureBox1_Resize", Environment.TickCount));
				PropertyInfo pi = typeof(PictureBox).GetProperty("ImageRectangle", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance);
				object obj = pi.GetValue(pictureBox1, null);
				m_rtImgBounds = (Rectangle)obj;
				//---
				if (m_width > 0 && m_height > 0) {
					m_rtCntImg = BMPCD_TO_IMGCD(new Rectangle(G.SS.CAM_HIS_RT_X, G.SS.CAM_HIS_RT_Y, G.SS.CAM_HIS_RT_W, G.SS.CAM_HIS_RT_H));
					if (G.CNT_MOD == 1) {
						m_pt_of_marker[2].X = m_rtCntImg.X;
						m_pt_of_marker[2].Y = m_rtCntImg.Y;
						m_pt_of_marker[3].X = m_rtCntImg.Right;
						m_pt_of_marker[3].Y = m_rtCntImg.Bottom;
					}
				}
				if (true) {
				}
				if (m_width > 0 && m_height > 0) {
					m_rtDanImg = BMPCD_TO_IMGCD(m_rtDanBmp);

					m_pt_of_marker[0].X = m_rtDanImg.X;
					m_pt_of_marker[0].Y = m_rtDanImg.Y;
					m_pt_of_marker[1].X = m_rtDanImg.Right;
					m_pt_of_marker[1].Y = m_rtDanImg.Bottom;

					if (this.toolStripMenuItem40.Checked) {
						for (int i=0; i < 3; i++) {
							m_pt_of_marker[4+i] = BMPCD_TO_IMGCD(m_calc_len.get_pt(i));
						}
					}
				}
				disp_extr(null);
			}
			catch (Exception ex) {
				G.mlog(ex.Message);
			}
		}
		//private void draw_line()
		//{
		//    if (true) {
		//        Graphics gr = this.pictureBox1.CreateGraphics();
		//        gr.DrawLine(Pens.Red, m_rtDanImg.Left, m_rtDanImg.Top, m_rtDanImg.Right, m_rtDanImg.Bottom);
		//        gr.Dispose();
		//        gr = null;
		//    }
		//}
		private void pictureBox1_Paint(object sender, PaintEventArgs e)
		{
			//Debug.WriteLine(string.Format("{0}:pictureBox1_Paint", Environment.TickCount));
			//if ((G.CAM_PRC == 1 && G.SS.CAM_HIS_PAR1 == 1)
			// || (G.CAM_PRC == 4 && G.SS.CAM_FCS_PAR1 == 1)) {
			//    Graphics gr = this.pictureBox1.CreateGraphics();
			//    gr.DrawRectangle(Pens.Blue, m_rtImg);
			//}
			//draw_line();
			disp_extr(e.Graphics);
		}


		private void toolStripMenuItems_Click(object sender, EventArgs e)
		{
			if (false) {
			}
			else if (sender == this.toolStripMenuItem1) {
				buttons_Click(this.button5, null);//one short
				this.toolStripMenuItem2.Checked = false;
				this.toolStripMenuItem3.Checked = true;
			}
			else if (sender == this.toolStripMenuItem2) {
				buttons_Click(this.button6, null);//continuous
				this.toolStripMenuItem2.Checked = true;
				this.toolStripMenuItem3.Checked = false;
			}
			else if (sender == this.toolStripMenuItem3) {
				buttons_Click(this.button7, null);//stop
				this.toolStripMenuItem2.Checked = false;
				this.toolStripMenuItem3.Checked = true;
			}
			else if (sender == this.toolStripMenuItem4) {
				buttons_Click(this.button8, null);//open
			}
			else if (sender == this.toolStripMenuItem5) {
				buttons_Click(this.button9, null);//save
			}
			else if (sender == this.toolStripMenuItem6) {
				//断面
				this.toolStripMenuItem6.Checked = !this.toolStripMenuItem6.Checked;
				this.checkBox1.Checked = this.toolStripMenuItem6.Checked;
				set_layout();
				if (this.checkBox1.Checked) {
					if (m_rtDanImg.Width <= 0 && m_rtDanImg.Height <= 0) {
						m_rtDanImg.X = m_rtDanImg.Y = 100;
						m_rtDanImg.Width = m_rtDanImg.Height = 100;
					}
					m_pt_of_marker[0].X = m_rtDanImg.X;
					m_pt_of_marker[0].Y = m_rtDanImg.Y;
					m_pt_of_marker[1].X = m_rtDanImg.Right;
					m_pt_of_marker[1].Y = m_rtDanImg.Bottom;
				}
				else {
					m_pt_of_marker[0].X = -1;
					m_pt_of_marker[0].Y = -1;
					m_pt_of_marker[1].X = -1;
					m_pt_of_marker[1].Y = -1;
				}
			}
			else if (sender == this.toolStripMenuItem20) {
				//ヒストグラム
				if (this.toolStripMenuItem20.Checked) {
					this.toolStripMenuItem20.Checked = false;
					G.CAM_PRC = 0;
				}
				else {
					this.toolStripMenuItem20.Checked = true;
					G.CNT_MOD = this.HIS_PAR1;
					G.CAM_PRC = G.CAM_STS.STS_HIST;
				}
				set_layout();
			}
			else if (sender == this.toolStripMenuItem22) {
				if (this.toolStripMenuItem22.Checked == false) {
					this.toolStripMenuItem22.Checked = true;
					this.toolStripMenuItem23.Checked = false;
					G.CNT_MOD = this.HIS_PAR1 = 0;
				}
			}
			else if (sender == this.toolStripMenuItem23) {
				if (this.toolStripMenuItem23.Checked == false) {
					this.toolStripMenuItem22.Checked = false;
					this.toolStripMenuItem23.Checked = true;
					G.CNT_MOD = this.HIS_PAR1 = 1;
				}
			}
			else if (sender == this.toolStripMenuItem31) {
				buttons_Click(this.button10, null);//(1/1)
				this.toolStripMenuItem31.Checked = true;
				this.toolStripMenuItem32.Checked = false;
				this.toolStripMenuItem33.Checked = false;
			}
			else if (sender == this.toolStripMenuItem32) {
				buttons_Click(this.button11, null);//(1/1)
				this.toolStripMenuItem31.Checked = false;
				this.toolStripMenuItem32.Checked = true;
				this.toolStripMenuItem33.Checked = false;
			}
			else if (sender == this.toolStripMenuItem33) {
				buttons_Click(this.button12, null);//(1/1)
				this.toolStripMenuItem31.Checked = false;
				this.toolStripMenuItem32.Checked = false;
				this.toolStripMenuItem33.Checked = true;
			}
			else if (sender == this.toolStripMenuItem40) {
				//距離計算
				this.toolStripMenuItem40.Checked = !this.toolStripMenuItem40.Checked;
				//set_layout();
				if (this.toolStripMenuItem40.Checked) {
					m_calc_len.reset(m_width, m_height);
					for (int i=0; i < 3; i++) {
						m_pt_of_marker[4+i] = BMPCD_TO_IMGCD(m_calc_len.get_pt(i));
					}
				}
				else {
					m_pt_of_marker[4].X = m_pt_of_marker[4].Y = -1;
					m_pt_of_marker[5].X = m_pt_of_marker[5].Y = -1;
					m_pt_of_marker[6].X = m_pt_of_marker[6].Y = -1;
				}
			}
		}

		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
			if (G.FORM12.AUT_STS != 0) {
				//自動測定中はコンテキストメニューは表示しない
				e.Cancel = true;
			}
		}
	};
}