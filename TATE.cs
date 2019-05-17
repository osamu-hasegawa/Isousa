using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//---
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
//---
using System.Diagnostics;
//using uSCOPE.OCV.*;
//using static uSCOPE.OCV;

namespace uSCOPE
{

	class TATE
	{
		static private int m_chk3;

		static public void post_proc(int m_width, int m_height, ref Bitmap m_bmpR, ref Bitmap m_bmpZ)
		{
			string buf1 = null, buf2 = null;
			int disp;
			G.CAM_STS mode = G.CAM_PRC;
			int tk;
#if true//2019.03.22(再測定表)
			if (G.CNT_NO_CONTOURS) {
				G.CNT_NO_CONTOURS = G.CNT_NO_CONTOURS;
			}
			else
#endif
			G.IR.clear();
			G.IR.WIDTH  = m_width;
			G.IR.HEIGHT = m_height;
			if (G.CAM_PRC == G.CAM_STS.STS_NONE) {
				return;
			}
#if false//////////
			if (G.CAM_PRC == G.CAM_STS.STS_CUTI) {
				post_proc_cuti();
				return;
			}
			//---
			//G.IR.CIR_CNT = 0;g.ir.clear()にて
#endif
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
				ret = OCV.SET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, OCV.PF2BPP(bmpData.PixelFormat));
				m_bmpR.UnlockBits(bmpData);
#else
				m_img_a.CopyFrom(m_bmpR);
#endif
				//グレースケール画像
				//Cv.CvtColor(m_img_a, m_img_g, ColorConversion.RgbaToGray);
				OCV.TO_GRAY((int)OCV.IMG.IMG_A, (int)OCV.IMG.IMG_G);
			}
			else {
				//m_bmpR, m_img_a, m_img_gはセット済
			}
			if (G.SS.CAM_CND_MODH == 1) {
				tk = Environment.TickCount;
				OCV.TO_HSV((int)OCV.IMG.IMG_A, (int)OCV.IMG.IMG_H);//Cv.CvtColor(m_img_a, m_img_h, ColorConversion.BgrToHsv);
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
					OCV.SMOOTH((int)OCV.IMG.IMG_H, cof);//Cv.Smooth(m_img_h, m_img_h, SmoothType.Gaussian, cof, cof, 0, 0);
				}
				else {
					OCV.SMOOTH((int)OCV.IMG.IMG_G, cof);//Cv.Smooth(m_img_g, m_img_g, SmoothType.Gaussian, cof, cof, 0, 0);
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
					OCV.THRESH_BIN((int)OCV.IMG.IMG_G, (int)OCV.IMG.IMG_B, th_val, /*INV=*/1);
					//Cv.Threshold(m_img_g, m_img_b, th_val, 255, ThresholdType.BinaryInv);	//白背景に黒丸の時は反転しておく
				}
			}
			if (G.SS.CAM_CND_MODH == 1) {
				OCV.SPLIT((int)OCV.IMG.IMG_H, (int)OCV.IMG.IMG_HSV_H, (int)OCV.IMG.IMG_HSV_S, (int)OCV.IMG.IMG_HSV_V);
				//Cv.Split(m_img_h, m_img_hsv[0], m_img_hsv[1], m_img_hsv[2], null);
			}
			if (false
			  || (mode == G.CAM_STS.STS_HIST && (G.SS.CAM_CND_MODH == 0 || G.SS.ETC_HIS_MODE == 0))
			  || (mode == G.CAM_STS.STS_HAIR && (G.SS.CAM_CND_MODH == 0))
				) {
				OCV.SPLIT((int)OCV.IMG.IMG_A, (int)OCV.IMG.IMG_RGB_B, (int)OCV.IMG.IMG_RGB_G, (int)OCV.IMG.IMG_RGB_R);// m_img_a:BGRの順
			}
			/*
			 * 二値化(ＨＳＶによる)
			 */
			if (G.SS.CAM_CND_MODH == 1) {
				tk = Environment.TickCount;
				OCV.THRESH_HSV((int)OCV.IMG.IMG_HSV_H, (int)OCV.IMG.IMG_HSV_S, (int)OCV.IMG.IMG_HSV_V, (int)OCV.IMG.IMG_B,
					G.SS.CAM_CND_MINH/2, G.SS.CAM_CND_MAXH/2,
					G.SS.CAM_CND_MINS, G.SS.CAM_CND_MAXS,
					G.SS.CAM_CND_MINV, G.SS.CAM_CND_MAXV
					);

				if ((tk = Environment.TickCount - tk) > 100) {
					tk = tk;
				}
			}
#if false//////////
#endif
			if (true) {
				disp = disp;
				switch (disp) {
				case 1:
					if (m_chk3 == 1) {
					OCV.MERGE((int)OCV.IMG.IMG_HSV_H, (int)OCV.IMG.IMG_HSV_H, (int)OCV.IMG.IMG_HSV_H, (int)OCV.IMG.IMG_A);
					m_chk3 = 2;
					}
					else if (m_chk3 == 2) {
					OCV.MERGE((int)OCV.IMG.IMG_HSV_S, (int)OCV.IMG.IMG_HSV_S, (int)OCV.IMG.IMG_HSV_S, (int)OCV.IMG.IMG_A);
					m_chk3 = 3;
					}
					else if (m_chk3 == 3) {
					OCV.MERGE((int)OCV.IMG.IMG_HSV_V, (int)OCV.IMG.IMG_HSV_V, (int)OCV.IMG.IMG_HSV_V, (int)OCV.IMG.IMG_A);
					m_chk3 = 1;
					}
					else {
					OCV.MERGE((int)OCV.IMG.IMG_G, (int)OCV.IMG.IMG_G, (int)OCV.IMG.IMG_G, (int)OCV.IMG.IMG_A);
					}
					//Cv.Merge(m_img_g, m_img_g, m_img_g, null, m_img_a);
					break;
				case 2:
					OCV.MERGE((int)OCV.IMG.IMG_B, (int)OCV.IMG.IMG_B, (int)OCV.IMG.IMG_B, (int)OCV.IMG.IMG_A);
					//Cv.Merge(m_img_b, m_img_b, m_img_b, null, m_img_a);
					break;
				case 3:
					//OCV_MERGE((int)OCV.IMG.IMG_M, (int)OCV.IMG.IMG_M, (int)OCV.IMG.IMG_M, (int)OCV.IMG.IMG_A);
					//OCV_MERGE((int)OCV.IMG.IMG_HSV_H, (int)OCV.IMG.IMG_HSV_H, (int)OCV.IMG.IMG_HSV_H, (int)OCV.IMG.IMG_A);
					//OCV_MERGE((int)OCV.IMG.IMG_HSV_S, (int)OCV.IMG.IMG_HSV_S, (int)OCV.IMG.IMG_HSV_S, (int)OCV.IMG.IMG_A);
					OCV.MERGE((int)OCV.IMG.IMG_HSV_V, (int)OCV.IMG.IMG_HSV_V, (int)OCV.IMG.IMG_HSV_V, (int)OCV.IMG.IMG_A);
				break;
				}
			}
			/*
			 * 毛髪判定
			 */
#if true//2019.03.22(再測定表)
			if (G.CNT_NO_CONTOURS) {
				G.CNT_NO_CONTOURS = G.CNT_NO_CONTOURS;
			}else
#endif
			if (mode == G.CAM_STS.STS_HAIR || G.CAM_PRC == G.CAM_STS.STS_FCUS || (G.CAM_PRC == G.CAM_STS.STS_HIST && G.CNT_MOD >= 2)) {
				OCV.FIND_FIRST((Int32)OCV.IMG.IMG_B, /*0:CV_RETR_EXTERNAL*/0);

				if (true) {
					IntPtr pos = (IntPtr)0;//, bak = (IntPtr)(-1);
					double s, l, c;
#if true//2019.03.02(直線近似)
					IntPtr pos_max = (IntPtr)0;
					IntPtr pos_mou = (IntPtr)0;
					//double s_max, l_max, c_max;
					int	bSIGNE_max = 0;
#endif
					for (pos = (IntPtr)0;;) {
						pos = OCV.FIND_NEXT(pos,
								G.SS.TAT_SUM_UPPR[0], G.SS.TAT_SUM_LOWR[0],
								G.SS.TAT_LEN_UPPR[0], G.SS.TAT_LEN_LOWR[0],
								G.SS.TAT_CIR_UPPR[0], G.SS.TAT_CIR_LOWR[0],
								out s, out l, out c);
						if (pos == (IntPtr)0) {
							break;
						}
						int bSIGNE = (s < 0) ? 1 : 0;
						double p = double.NaN;
						OCV.RECT	rc;
						s = Math.Abs(s);

						if (G.IR.CIR_CNT == 0 || s >= G.IR.CIR_S) {
							pos_max = pos;
							bSIGNE_max = bSIGNE;
							G.IR.CIR_S = s;
							G.IR.CIR_L = l;
							G.IR.CIR_C = c;
							OCV.BOUNDING_RECT(pos, out rc);
							G.IR.CIR_RT = new Rectangle(rc.Left, rc.Top, (rc.Right-rc.Left), (rc.Bottom-rc.Top));
						}
						//輪郭の描画
						if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK1) {
							OCV.DRAW_CONTOURS((Int32)OCV.IMG.IMG_A, pos, 0x0000FF, 0xFF0000);
						}
						G.IR.CIR_CNT++;
					}
					for (pos = (IntPtr)0;;) {
						pos = OCV.FIND_NEXT(pos,
								G.SS.TAT_SUM_UPPR[1], G.SS.TAT_SUM_LOWR[1],
								G.SS.TAT_LEN_UPPR[1], G.SS.TAT_LEN_LOWR[1],
								G.SS.TAT_CIR_UPPR[1], G.SS.TAT_CIR_LOWR[1],
								out s, out l, out c);
						if (pos == (IntPtr)0) {
							break;
						}
						int bSIGNE = (s < 0) ? 1 : 0;
						double p = double.NaN;
						OCV.RECT	rc;
						s = Math.Abs(s);

						if (G.IR.CIR_CNT == 0 || s >= G.IR.CIR_S) {
							pos_mou = pos;
							bSIGNE_max = bSIGNE;
							G.IR.TAT_S = s;
							G.IR.TAT_L = l;
							G.IR.TAT_C = c;
							OCV.BOUNDING_RECT(pos, out rc);
							G.IR.TAT_RT = new Rectangle(rc.Left, rc.Top, (rc.Right-rc.Left), (rc.Bottom-rc.Top));
						}
						//輪郭の描画
						if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK1) {
							OCV.DRAW_CONTOURS((Int32)OCV.IMG.IMG_A, pos, 0x0000FF, 0xFF0000);
						}
						G.IR.TAT_CNT++;
					}

					if (G.IR.CIR_CNT > 0 || G.IR.TAT_CNT > 0) {
						int bSIGNE = bSIGNE_max;
						double p = double.NaN;
						OCV.RECT	rc;
						if (G.IR.TAT_CNT != 0) {
						pos = pos_mou;
						s = G.IR.TAT_S;
						l = G.IR.TAT_L;
						c = G.IR.TAT_C;
						rc.Left   = G.IR.TAT_RT.Left;
						rc.Top    = G.IR.TAT_RT.Top;
						rc.Right  = G.IR.TAT_RT.Right;
						rc.Bottom = G.IR.TAT_RT.Bottom;
						}
						else {
						pos = pos_max;
						s = G.IR.CIR_S;
						l = G.IR.CIR_L;
						c = G.IR.CIR_C;
						rc.Left   = G.IR.CIR_RT.Left;
						rc.Top    = G.IR.CIR_RT.Top;
						rc.Right  = G.IR.CIR_RT.Right;
						rc.Bottom = G.IR.CIR_RT.Bottom;
						}
						if (true) {
							double m00, m01, m10;
							OCV.POINT p0;
							OCV.CONTOURS_MOMENTS(pos, out m00, out m01, out m10);
							G.IR.TAT_GX = (int)(m10/m00);
							G.IR.TAT_GY = (int)(m01/m00);
							p0.x = G.IR.TAT_GX;
							p0.y = G.IR.TAT_GY;
							Form02.draw_marker(p0, 0xFF0000);
						}
						//CHK1:輪郭, CHK2:多曲線, CHK3:特徴値, CHK4:毛髪径
#if true//2019.03.02(直線近似)
						if (true) {//外接矩形
							OCV.POINT	p1, p2, p3, p4, p5, p6, p7;
							OCV.MIN_AREA_RECT2(pos, out p1, out p2, out p3, out p4);

							OCV.DRAW_LINE((int)OCV.IMG.IMG_A, ref p1, ref p2, 0xc08000, 4);
							OCV.DRAW_LINE((int)OCV.IMG.IMG_A, ref p2, ref p3, 0xc08000, 4);
							OCV.DRAW_LINE((int)OCV.IMG.IMG_A, ref p3, ref p4, 0xc08000, 4);
							OCV.DRAW_LINE((int)OCV.IMG.IMG_A, ref p4, ref p1, 0xc08000, 4);

							double l12 = G.diff(p1.x, p1.y, p2.x, p2.y);
							double l23 = G.diff(p2.x, p2.y, p3.x, p3.y);
							if (l12 <= l23) {
								//l12とl34のセンターが毛髪の方向
								p5.x = (p1.x + p2.x)/2;
								p5.y = (p1.y + p2.y)/2;
								p6.x = (p3.x + p4.x)/2;
								p6.y = (p3.y + p4.y)/2;
							}
							else {
								//l23とl41のセンターが毛髪の方向
								p5.x = (p2.x + p3.x)/2;
								p5.y = (p2.y + p3.y)/2;
								p6.x = (p4.x + p1.x)/2;
								p6.y = (p4.y + p1.y)/2;
							}
							//外接矩形のセンターライン
							OCV.DRAW_LINE((int)OCV.IMG.IMG_A, ref p5, ref p6, 0xc08000, 4);
							//重心(外接矩形の中心)
							p7.x = (p5.x + p6.x)/2;
							p7.y = (p5.y + p6.y)/2;
							Form02.draw_marker(p7);
							//
							G.IR.TAT_OX = p7.x;
							G.IR.TAT_OY = p7.y;
							G.IR.TAT_DX = (p6.x - p5.x);
							G.IR.TAT_DY = (p6.y - p5.y);
							G.IR.TAT_P1 = new Point(p1.x, p1.y);
							G.IR.TAT_P2 = new Point(p2.x, p2.y);
							G.IR.TAT_P3 = new Point(p3.x, p3.y);
							G.IR.TAT_P4 = new Point(p4.x, p4.y);
							//
						}
#endif
						//多曲線と毛髪径
						if (true) {
							int n;
							n = OCV.APPROX_PTS(pos, bSIGNE, G.SS.CAM_DIR_PREC);
							if (n >= 4) {
#if false//2019.03.02(直線近似)
#endif
								//毛髪径
								try {
									p = 12345.6789;//calc_diam2(n/*m_img_a, pts*/);
								}
								catch (Exception ex) {
									Trace.WriteLineIf((G.AS.TRACE_LEVEL & 4)!=0, ex.Message);
								}
#if true//2019.02.03(WB調整)
								G.IR.CIR_PX = p;
#endif
								//p = PX2UM(p);
								//多曲線の接続点の描画
								if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK2) {
									for (int i = 0; i < n; i++) {
										OCV.POINT	pt;
										OCV.GET_PTS(i, out pt);
										Form02.draw_marker(pt);
									}
								}
							}
						}
						//特徴値
						if (true) {
							string buf="";
							if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK3) {
								//	string buf = string.Format("S={0:F0},L={1:F0},C={2:F2},P={3:F0}p", s, l, c, p);
								buf += string.Format("S={0:F0},L={1:F0},C={2:F2}", s, l, c);
							}
							if (mode == G.CAM_STS.STS_HAIR && G.SS.CAM_CIR_CHK4) {
								if (G.IR.CIR_CNT > 0) {
									if (buf.Length > 0) { buf += ",";}
									buf += "[BR]";
								}
								if (G.IR.TAT_CNT > 0) {
									if (buf.Length > 0) { buf += ",";}
									buf += "[HF]";
								}
							}
							if (buf.Length > 0) {
								//draw_text(m_img_a, rc.Left + (rc.Right-rc.Left) / 2, rc.Top + (rc.Bottom-rc.Top) / 2, buf);
								//OCV_DRAW_TEXT((int)OCV.IMG.IMG_A, rc.Left + (rc.Right - rc.Left) / 2, rc.Top + (rc.Bottom - rc.Top) / 2, buf, 0x00FF00);
								buf2 = buf;
								//OCV_PUTTEXT((int)OCV.IMG.IMG_A, buf, 50, 100, 0x00FF00);
							}
						}
					}
				}
				OCV.FIND_TERM();
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
						G.FORM02.set_mask_by_result();
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
						G.FORM02.set_mask_by_result();
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
#if true//2019.03.22(再測定表)
				if (G.IR.HIST_ALL) {
					G.FORM02.reset_mask_poly(0, 0, m_width, m_height, true);
				}
#endif
				bMASK = (G.IR.HIST_ALL) ? 0 : 1;
				//calc_hist(m_img_g, mask, G.IR.HISTVALY, out G.IR.HIST_MIN, out G.IR.HIST_MAX, out G.IR.HIST_AVG);
				OCV.CAL_HIST((int)OCV.IMG.IMG_G, bMASK, ref G.IR.HISTVALY[0], out G.IR.HIST_MIN, out G.IR.HIST_MAX, out G.IR.HIST_AVG);

				if (G.CAM_PRC == G.CAM_STS.STS_HIST/* || this.groupBox2.Visible*/) {
					double tmp;
					if (G.SS.ETC_HIS_MODE == 0) {
						OCV.CAL_HIST((int)OCV.IMG.IMG_RGB_R, bMASK, ref G.IR.HISTVALR[0], out tmp, out tmp, out tmp);
						OCV.CAL_HIST((int)OCV.IMG.IMG_RGB_G, bMASK, ref G.IR.HISTVALG[0], out tmp, out tmp, out tmp);
						OCV.CAL_HIST((int)OCV.IMG.IMG_RGB_B, bMASK, ref G.IR.HISTVALB[0], out tmp, out tmp, out tmp);
					}
					else {
						OCV.CAL_HIST((int)OCV.IMG.IMG_HSV_H, bMASK, ref G.IR.HISTVALH[0], out tmp, out tmp, out tmp);
						OCV.CAL_HIST((int)OCV.IMG.IMG_HSV_S, bMASK, ref G.IR.HISTVALS[0], out tmp, out tmp, out tmp);
						OCV.CAL_HIST((int)OCV.IMG.IMG_HSV_V, bMASK, ref G.IR.HISTVALV[0], out tmp, out tmp, out tmp);
					}
				}
				if (G.CNT_MET >= 2) {
					double tmp, fsum = 0, fttl = 0, fgra;
					int dx, dy, ap;
					switch (G.CNT_MET) {
						case  2: dx = 1;dy = 0;ap = 0; break;
						case  3: dx = 0;dy = 1;ap = 0; break;
#if true//2019.04.29(微分バグ修正)
						case  4: dx = 1;dy = 1;ap = 0; break;
						case  5: dx = 2;dy = 0;ap = 0; break;//2次微分 X
						case  6: dx = 0;dy = 2;ap = 0; break;//2次微分 Y
						default: dx = 2;dy = 2;ap = 0; break;//2次微分 XY
#else
						default: dx = 1;dy = 1;ap = 0; break;
#endif
					}
					OCV.SOBEL((int)OCV.IMG.IMG_G, (int)OCV.IMG.IMG_D, dx, dy, 3+ap*2);
					OCV.CAL_HIST((int)OCV.IMG.IMG_D, bMASK, ref G.IR.HISTVALD[0], out tmp, out tmp, out tmp);
#if true//2019.04.29(微分バグ修正)
					if (dx == 1 || dy == 1) {
						for (int i = 0; i < 256; i++) {
							if (i >= G.CNT_DTHD) {
								fsum += (i * G.IR.HISTVALD[i]);
							}
							fttl += G.IR.HISTVALD[i];
						}
					}
					else {
						for (int i = 0; i < 256; i++) {
							if (i >= G.CNT_DTH2) {
								fsum += (i * G.IR.HISTVALD[i]);
							}
							fttl += G.IR.HISTVALD[i];
						}
					}
#endif
					fgra = fsum/fttl;
					//G.IR.CONTRAST = fsum / (255*fttl/2);
					G.IR.CONTRAST = fsum / (127.5*fttl/2);
					G.IR.CONTRAST*= 10;//小さすぎるため根拠なく10倍
#if true//2019.03.22(再測定表)
					if (disp == 1) {
						OCV.SCALE((int)OCV.IMG.IMG_D, (int)OCV.IMG.IMG_D, 10, 0);
						OCV.MERGE((int)OCV.IMG.IMG_D, (int)OCV.IMG.IMG_D, (int)OCV.IMG.IMG_D, (int)OCV.IMG.IMG_A);
					}
#endif
				}
				else if (G.CNT_MET == 1) {
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
#if true//2019.02.27(ＡＦ２実装)
				if (G.FC2_FLG && G.IR.FC2_POS != null) {
					G.IR.FC2_POS.Add(G.IR.FC2_TMP);
					G.IR.FC2_CTR.Add(G.IR.CONTRAST);
#if true//2019.03.02(直線近似)
					if (G.FC2_DONE == 0 && G.IR.CONTRAST >= G.SS.CAM_FC2_CNDA) {
						G.FC2_DONE++;
					}
					if (G.FC2_DONE == 1 && G.IR.CONTRAST <= G.SS.CAM_FC2_CNDB) {
						G.FC2_DONE++;
					}
#endif
				}
#endif
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
					//OCV_PUTTEXT((int)OCV.IMG.IMG_A, buf, 50, 100, 0x00FF00);
					//CvPoint pnt = new CvPoint(50, 100);
					//Cv.PutText(m_img_a, buf, pnt, fnt, Cv.RGB(0, 255, 0));
				}
			}
			if (!string.IsNullOrEmpty(buf1) || !string.IsNullOrEmpty(buf2)) {
				if (string.IsNullOrEmpty(buf1)) {
					buf1 = buf2;
					buf2 = null;
				}
				if (true) {
					if (!string.IsNullOrEmpty(buf1)) {
						OCV.PUTTEXT((int)OCV.IMG.IMG_A, buf1, 50, 100, 0x00FF00);
					}
					if (!string.IsNullOrEmpty(buf2)) {
						OCV.PUTTEXT((int)OCV.IMG.IMG_A, buf2, 50, 200, 0x00FF00);
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
				ret = OCV.GET_IMG(bmpData.Scan0, bmpData.Width, bmpData.Height, bmpData.Stride, OCV.PF2BPP(bmpData.PixelFormat));
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
#if false////////////////////////////////
#endif
		}
	}
}