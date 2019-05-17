using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace uSCOPE
{
    class T
    {
		static public string GetDocFolder()
		{
			return(GetDocFolder(null));
		}
		static public string GetDocFolder(string file)
		{
			string	path;
//			path = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
//			path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#if true//2019.05.12(縦型対応)
			path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#else
			path = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#endif
			path += "\\KOP";
			if (!System.IO.Directory.Exists(path)) {
				System.IO.Directory.CreateDirectory(path);
			}
			path += "\\" + Application.ProductName;
#if true//2019.05.12(縦型対応)
			if (G.bTATE_MODE) {
				path += ".TATE";
			}
#endif
			if (!System.IO.Directory.Exists(path)) {
				System.IO.Directory.CreateDirectory(path);
			}
			if (file != null && file.Length > 0) {
				path += "\\" + file;
			}
			return(path);
		}		//
		static public double DT2DBL(DateTime tm)
		{
			DateTime	tmBase = new DateTime(2007,1,1,0,0,0);
			const
			double		dbBase = 63303206400.0;
			TimeSpan	span;

			span = tm-tmBase;
			return(dbBase + span.TotalSeconds);
		}
		/**/
		static public DateTime DBL2DT(double f)
		{
			DateTime tm;
			DateTime	tmBase = new DateTime(2007,1,1,0,0,0);
			const
			double		dbBase = 63303206400.0;

			tm = tmBase.AddSeconds(f-dbBase);
			return(tm);
		}
		static public bool IsNumeric(string str)
        {
            double dNullable;

            return double.TryParse(
                str,
                System.Globalization.NumberStyles.Any,
                null,
                out dNullable
            );
        }
		/**/
		static public double AXpB(double x1, double x2, double y1, double y2, double x)
		{
			double	A, B;
			double	Y;
			if ((x2 - x1) == 0.0) {
				return(-99999);
			}
			else if (x == x1) {
				Y = y1;
			}
			else if (x == x2) {
				Y = y2;
			}
			else {
				A = (y2 - y1) / (x2 - x1);
				B = y1 - A * x1;
				Y = A * x + B;
			}
			return(Y);
		}
		/****************************************************************************/
		/* ラグランジュ補間
		/* x1, x2 ... x ... x3, x4
		/* y1, y2 ... y ... y3, y4
		/****************************************************************************/
		static public double lagran(	double x1, double x2, double x3, double x4,
						double y1, double y2, double y3, double y4,
						double x)
		{
			double	Y1, Y2, Y3, Y4, YY;

		//	if (x <= x3) {
				Y1 = y1 * (x-x2)*(x-x3)*(x-x4) / ((x1-x2)*(x1-x3)*(x1-x4));
				Y2 = y2 * (x-x1)*(x-x3)*(x-x4) / ((x2-x1)*(x2-x3)*(x2-x4));
				Y3 = y3 * (x-x1)*(x-x2)*(x-x4) / ((x3-x1)*(x3-x2)*(x3-x4));
				Y4 = y4 * (x-x1)*(x-x2)*(x-x3) / ((x4-x1)*(x4-x2)*(x4-x3));
				YY = Y1 + Y2 + Y3 + Y4;
		//	}
			return(YY);
		}
		/**/
		static public void MakeLaglan(double[] fWav, double[] fVal, double[] xx, double[] yy)
		{
			double	w1, w2, w3, w4, ww;
			double	v1, v2, v3, v4, vv;
			int		i, h = 0;

			xx[h] = fWav[0];
			yy[h] = fVal[0];
			h++;
			/*
			 Ex. m_cnt: 100
				loop 0-98
			*/
			for (i = 0; i < (fWav.Length-1); i++) {
				if (i == 0) {
					w1 = fWav[0] - 1;
					v1 = fVal[0];
				}
				else {
					w1 = fWav[i-1];
					v1 = fVal[i-1];
				}
				/*if (i == (m_cnt-2)) {
					i = i;
				}*/

				w2 = fWav[i];
				v2 = fVal[i];

				if (i < (fWav.Length-2)) {
					w3 = fWav[i+1];
					v3 = fVal[i+1];
					w4 = fWav[i+2];
					v4 = fVal[i+2];
				}
				else {
					w3 = fWav[i+1];
					v3 = fVal[i+1];
					w4 = fWav[i+1] + 1;
					v4 = fVal[i+1];
				}
				for (ww = w2+1; ww <= w3; ww+=1.0) {	/* w2 ～ w3 の波長間を1nm毎に補間する */
					vv = lagran( w1, w2, w3, w4,
								v1, v2, v3, v4,
								ww);
					xx[h] = ww;
					yy[h] = vv;
					h++;
				}
			}
		}
		/**/
		static public void MakeLinear(double[] fWav, double[] fVal, ref double[] xx, ref double[] yy)
		{
#if true
//			double		v1, v2, v3, v4;
			int			w;
			double		v;
			int			i;
			int			i0, i1/*, i2, i3*/;
			
			bool		bLinearInterpolation = true;
			int			LMIN = (int)fWav[0];
			int			LMAX = (int)fWav[fWav.Length-1];

			/**/
			int		len = (int)(LMAX-LMIN+1);
			xx = new double[len];
			yy = new double[len];
			/**/
			if (bLinearInterpolation) {
				i0 = i1 = 0;
				//for (w = LMIN; w < m_tbl[0].fWaveLength; w++) {
				//	m_dat[w-LMIN] = 0;
				//}
				for (w = LMIN, i = 0; w <= LMAX; i++, w++) {
					//w = LMIN; w <= LMAX; w++) {
					while (fWav[i1] <= w) {
						if ((i1+1) < fWav.Length) {
							i1++;
						}
						else {
							break;
						}
					}
					if (i1 > 0) {
						i0 = i1 - 1;
					}
					else {
						i0 = 0;
					}
					v = AXpB(
							fWav[i0], fWav[i1],
							fVal[i0], fVal[i1],
							w);
//					System.Diagnostics.Debug.Write(String.Format("{0} [{1}-{2}]: {3}\n",
//								w, fWav[i0], fWav[i1], v));
					xx[i] = w;
					yy[i] = v;
				}
			}
#else
			double	w1, w2, ww,vv;
			int		i, h = 0;

			xx[h] = fWav[0];
			yy[h] = fVal[0];
			h++;
			for (i = 0; i < (fWav.Length-1); i++) {
				w1 = fWav[i];
				w2 = fWav[i+1];

				for (ww = w1+1.0; ww <= w2; ww+=1.0) {
					vv = AXpB(
							fWav[i], fWav[i+1],
							fVal[i], fVal[i+1],
							ww);
					xx[h] = ww;
					yy[h] = vv;
					h++;
				}
			}
#endif
		}
		static public double GetAVG(double[] p, int n)
		{
			double	avg = 0;
			int		i;

			for (i = 0; i < n; i++) {
				avg += p[i];
			}
			avg /= n;
			return(avg);
		}
		static public double GetSTD(double[] p, int n)
		{
			double	avg = GetAVG(p, n);
			int		i;
			double	s=0;
			double	f;

			for (i = 0; i < n; i++) {
				f = (p[i]-avg);
				s += f*f;
			}
			s /= n;			// <- 分散
			s = Math.Sqrt(s);	// <- 標準偏差
			return(s);
		}
		//static public DateTime HM2DT(DateTime dt, double secs1, double secs2)
		//{
		//}
		//static public double GetTicByTime(DateTime tm1, DateTime tm2)
		//{
		//}
		static public Color GetColor(int idx)
		{
			Color	col;
#if false
			switch (idx) {
				case 0: col = G.SS.gcol[11]; break;	/*G*/
				case 1: col = G.SS.gcol[12]; break;	/*B*/
				case 2: col = G.SS.gcol[13]; break;	/*R*/
				case 3: col = G.SS.gcol[14]; break;
				case 4: col = G.SS.gcol[15]; break;
				case 5: col = G.SS.gcol[16]; break;
				case 6: col = G.SS.gcol[17]; break;
				default:col = G.SS.gcol[18]; break;
			}
#else
			switch (idx%8) {
				case 0: col = Color.Lime; break;	/*G*/
				case 1: col = Color.Blue; break;	/*B*/
				case 2: col = Color.Red; break;		/*R*/
				case 3: col = Color.Yellow; break;
				case 4: col = Color.Violet; break;
				case 5: col = Color.Cyan; break;
				case 6: col = Color.Orange; break;
				default:col = Color.LightGray; break;
			}
#endif
			return(col);
		}
		///************************************************************/
		//static public String GRID2CSV(DataGridView dg)
		//{
		//}
		static public string DT2STR(DateTime dt)
		{
			return(String.Format(
				"{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}",
					dt.Year,
					dt.Month,
					dt.Day,
					dt.Hour,
					dt.Minute,
					dt.Second)
			);
		}
 		/************************************************************/
		static private Color GetHoshoku(Color col)
		{
			int	r = col.R,
				g = col.G,
				b = col.B;
			r = 255-r;
			g = 255-g;
			b = 255-b;
			return(Color.FromArgb(r, g, b));
		}
		///************************************************************/
		//static private int M2D(float mm)
		//{
		//}
		///************************************************************/
		//static public void PrintGridPairStr(Graphics g, Rectangle rt, ArrayList ary, int c_max, int r_max)
		//{
		//}
		///************************************************************/
		//static public void PrintDataGrid(DataGridView grid, Graphics g, Rectangle rt, int c_max, int r_max)
		//{
		//}
		///************************************************************/
		//static public void PrintDataGrid2(DataGridView grid, Graphics g, Rectangle rt, int c_max, int r_max)
		//{
		//}
		///************************************************************/
		//static public void PrintDataGrid3(DataGridView grid, Graphics g, Rectangle rt, int C_WID, int C_HEI, float[] C_WID_TIMES, String title)
		//{
		//}
		///************************************************************/
		//static public void PrintText(Graphics g, Rectangle rt, int rows, int clms, ArrayList strs)
		//{
		//}
		///************************************************************/
		//static public void PrintDate(Graphics g, Rectangle rt, DateTime dt)
		//{
		//}
		///************************************************************/
		//static public void GetPairStrGrid(DataGridView grid, ArrayList ary)
		//{
		//}
		///************************************************************/
		//static public Rectangle GetYohakuRect(Rectangle rtBounds)
		//{
		//}
		///************************************************************/
		//static public void GetCellSize(out int C_WID, out int C_HEI)
		//{
		//    C_WID = M2D(20);
		//    C_HEI = M2D( 7);
		//}
		/************************************************************/
		static public double GetMax(double[] f)
		{
			double	max = f[0];

			for (int i = 1; i < f.Length; i++) {
				if (max < f[i]) {
					max = f[i];
				}
			}
			return(max);
		}
		/************************************************************/
		static public void GetLstMst(double[] ary, out int lst, out int mst)
		{
			lst = (int)ary[0];
			mst = (int)ary[ary.Length-1];
		}
		/************************************************************/
		/*
		static public double GetNearWavVal(double[] wav, double[] val, int wget)
		{
			int		idx = 0;
			double	min = double.MaxValue,
					dif;

			if (wav.Length != val.Length) {
				throw new Exception("logical error");
			}
			for (int i = 0; i < wav.Length; i++) {
				dif = Math.Abs(wav[i]-wget);
				if (dif < min) {
					min = dif;
					idx = i;
					if (min == 0) {
						break;
					}
				}
			}
			if (min != 0) {
				Debug.WriteLine(String.Format("MIN={0}", min));
			}
			return(val[idx]);
		}*/
		/************************************************************************/
		/* Smoothing by Savitzky-Golay 平滑化 */
		/************************************************************************/
		static public double SG_POL_SMOOTH(double[] fin, double[] fout2, int cnt, int weight, out double ofmax, out double ofmin)
		{
			int[] w05 = { -3,   12, 17										  };
			int[] w07 = { -2,    3,  6,  7									  };
			int[] w09 = { -21,  14, 39, 54, 59								  };
			int[] w11 = { -36,   9, 44, 69, 84, 89							  };
			int[] w13 = { -11,   0,  9, 16, 21, 24, 25						  };
			int[] w15 = { -78, -13, 42, 87,122,147,162,167					  };
			int[] w17 = { -21,  -6,  7, 18, 27, 34, 39, 42, 43				  };
			int[] w19 = {-136, -51, 24, 89,144,189,224,249,264,269			  };
			int[] w21 = {-171, -76,  9, 84,149,204,249,284,309,324,329		  };
			int[] w23 = { -42, -21, -2, 15, 30, 43, 54, 63, 70, 75, 78, 79	  };
			int[] w25 = {-253,-138,-33, 62,147,222,287,343,387,422,447,462,467};
			int[] wp;
			int n, s;
			double	sum, f;
			int		i, h;
			double	fmax = -1E10, fmin = +1e10;
			double[]
					fout;

			ofmax = double.NaN;
			ofmin = double.NaN;
			try {
				/**/ if (weight == 5) { wp = w05; n = 2; s = 35; }
				else if (weight == 7) { wp = w07; n = 3; s = 21; }
				else if (weight == 9) { wp = w09; n = 4; s = 231; }
				else if (weight == 11) { wp = w11; n = 5; s = 429; }
				else if (weight == 13) { wp = w13; n = 6; s = 143; }
				else if (weight == 15) { wp = w15; n = 7; s = 1105; }
				else if (weight == 17) { wp = w17; n = 8; s = 323; }
				else if (weight == 19) { wp = w19; n = 9; s = 2261; }
				else if (weight == 21) { wp = w21; n = 10; s = 3059; }
				else if (weight == 23) { wp = w23; n = 11; s = 805; }
				else if (weight == 25) { wp = w25; n = 12; s = 5175; }
				else
				{
					throw new Exception("Internal Error");
				}

				if (fout2 == fin)
				{
					fout = new double[cnt];
				}
				else
				{
					fout = fout2;
				}
				for (i = 0; i < cnt; i++)
				{
					sum = 0.0;
					for (h = -n; h <= n; h++) {
						if ((h+i) < 0) {
							f = fin[0];
						}
						else if ((h+i) >= cnt) {
							f = fin[cnt-1];
						}
						else {
							f = fin[h+i];
						}
						if (h > 0) {
							sum += f * wp[n-h];
						}
						else {
							sum += f * wp[h+n];
						}
					}
					fout[i] = sum / s;
					if (fout[i] > fmax) {
						fmax = fout[i];
					}
					if (fout[i] < fmin) {
						fmin = fout[i];
					}
				}
				if (fout != fout2)
				{
					Array.Copy(fout, fout2, cnt);
					//free(fout);
				}
				if (true /*ofmax != null*/)
				{
					ofmax = fmax;
					ofmin = fmin;
				}
			}
			catch (Exception ex)
			{
				fmax = 1.0;
				G.mlog(ex.ToString());
			}
			return (fmax);
		}
		/************************************************************************/
		/* Smoothing by Savitzky-Golay 平滑化 */
		/************************************************************************/
		static public double SG_1ST_DERI(double[] fin, double[] fout2, int cnt, int weight, out double ofmax)
		{
			int[] w05 = {  2,    1,  0										};
			int[] w07 = {  3,    2,  1,  0									};
			int[] w09 = {  4,    3,  2,  1,  0								};
			int[] w11 = {  5,    4,  3,  2,  1,  0							};
			int[] w13 = {  6,    5,  4,  3,  2,  1, 0						};
			int[] w15 = {  7,    6,  5,  4,  3,  2, 1, 0 					};
			int[] w17 = {  8,    7,  6,  5,  4,  3, 2, 1,  0				};
			int[] w19 = {  9,    8,  7,  6,  5,  4, 3, 2,  1,  0			};
			int[] w21 = { 10,    9,  8,  7,  6,  5, 4, 3,  2,  1,  0		};
			int[] w23 = { 11,   10,  9,  8,  7,  6, 5, 4,  3,  2,  1,  0	};
			int[] w25 = { 12,   11, 10,  9,  8,  7, 6, 5,  4,  3,  2,  1,  0};
			int[] wp;
			int n, s;
			double	sum, f;
			int		i, h;
			double	fmax = -1E10;
			double[]
					fout;

			ofmax = double.NaN;
			try {
				/**/ if (weight == 5)  { wp = w05; n =  2; s =  10; }
				else if (weight == 7)  { wp = w07; n =  3; s =  28; }
				else if (weight == 9)  { wp = w09; n =  4; s =  60; }
				else if (weight == 11) { wp = w11; n =  5; s = 110; }
				else if (weight == 13) { wp = w13; n =  6; s = 182; }
				else if (weight == 15) { wp = w15; n =  7; s = 280; }
				else if (weight == 17) { wp = w17; n =  8; s = 408; }
				else if (weight == 19) { wp = w19; n =  9; s = 570; }
				else if (weight == 21) { wp = w21; n = 10; s = 770; }
				else if (weight == 23) { wp = w23; n = 11; s =1012; }
				else if (weight == 25) { wp = w25; n = 12; s =1300; }
				else
				{
					throw new Exception("Internal Error");
				}

				if (fout2 == fin)
				{
					fout = new double[cnt];
				}
				else
				{
					fout = fout2;
				}
				for (i = 0; i < cnt; i++)
				{
					sum = 0.0;
					for (h = -n; h <= n; h++) {
						if ((h+i) < 0) {
							f = fin[0];
						}
						else if ((h+i) >= cnt) {
							f = fin[cnt-1];
						}
						else {
							f = fin[h+i];
						}
						if (h > 0) {
							sum += f * wp[n-h];
						}
						else {
							sum += f *(-wp[h+n]);
						}
					}
					fout[i] = sum / s;
					if (fout[i] > fmax) {
						fmax = fout[i];
					}
				}
				if (fout != fout2)
				{
					Array.Copy(fout, fout2, cnt);
					//free(fout);
				}
				if (true /*ofmax != null*/)
				{
					ofmax = fmax;
				}
			}
			catch (Exception ex)
			{
				fmax = 1.0;
				G.mlog(ex.ToString());
			}
			return (fmax);
		}
		/************************************************************************/
		/* Smoothing by Savitzky-Golay 2次微分　http://www.statistics4u.info/fundstat_eng/cc_savgol_coeff.html */
		/************************************************************************/
		static public double SG_2ND_DERI(double[] fin, double[] fout2, int cnt, int weight, out double ofmax)
		{
			int[] w05 = {   2,  -1, -2 };
			int[] w07 = {   5,  0,  -3, -4 };
			int[] w09 = {  28,  7,  -8, -17, -20 };
			int[] w11 = {  15,  6,  -1, -6,   -9, -10 };
			int[] w13 = {  22,  11,  2, -5,  -10, -13, -14 };
			int[] w15 = {  91,  52, 19, -8,  -29, -44, -53, -56 };
			int[] w17 = {  40,  25, 12,  1,   -8, -15, -20, -23, -24 };
			int[] w19 = {  51,  34, 19,  6,   -5, -14, -21, -26, -29,  -30 };
			int[] w21 = { 190, 133, 82, 37,   -2, -35, -62, -83, -98, -107, -110 };
			int[] w23 = {  77,  56, 37, 20,    5,  -8, -19, -28, -35,  -40,  -43, -44 };
			int[] w25 = {  92,  69, 48, 29,   12,  -3, -16, -27, -36,  -43,  -48, -51, -52 };
			int[]	wp;
			int		n, s;
			double	sum, f;
			int		i, h;
			double	fmax = -1E10;
			double[]
					fout;

			ofmax = double.NaN;

			try {

				/**/ if (weight ==  5) { wp = w05; n = 2; s = 7; }
				else if (weight ==  7) { wp = w07; n =  3; s =    42;}
				else if (weight ==  9) { wp = w09; n =  4; s =   462;}
				else if (weight == 11) { wp = w11; n =  5; s =   429;}
				else if (weight == 13) { wp = w13; n =  6; s =  1001;}
				else if (weight == 15) { wp = w15; n =  7; s =  6188;}
				else if (weight == 17) { wp = w17; n =  8; s =  3876;}
				else if (weight == 19) { wp = w19; n =  9; s =  6783;}
				else if (weight == 21) { wp = w21; n = 10; s = 33649;}
				else if (weight == 23) { wp = w23; n = 11; s = 17710;}
				else if (weight == 25) { wp = w25; n = 12; s = 26910;}
				else {
					throw new Exception("Internal Error");
				}

				if (fout2 == fin) {
					fout = new double[cnt];
				}
				else {
					fout = fout2;
				}
				for (i = 0; i < cnt; i++) {
					sum = 0.0;
					for (h = -n; h <= n; h++) {
						if ((h+i) < 0) {
							f = fin[0];
						}
						else if ((h+i) >= cnt) {
							f = fin[cnt-1];
						}
						else {
							f = fin[h+i];
						}
						if (h > 0) {
							sum += f * wp[n-h];
						}
						else {
							sum += f * wp[h+n];
						}
					}
					fout[i] = sum / s;
					if (fout[i] > fmax) {
						fmax = fout[i];
					}
				}
	#if true
				for (i = 0; i < n; i++) {
					fout[i] = fout[n];
				}
				for (i = cnt-n; i < cnt; i++) {
					fout[i] = fout[cnt-n-1];
				}
	#endif
				if (fout != fout2) {
					Array.Copy(fout, fout2, cnt);
					//free(fout);
				}
				if (true /*ofmax != null*/)
				{
					ofmax = fmax;
				}
			}
			catch (Exception /*ex*/)
			{
				fmax = 1.0;
//				MessageBox.Show(ex.ToString(), "例外発生", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			return(fmax);
		}
		/************************************************************************/
		/* Smoothing by Savitzky-Golay */
		/************************************************************************/
		static public double SmoothMA(double[] fin, double[] fout2, int cnt, int weight, out double ofmax)
		{
			int n = weight/*, s*/;
			double	sum, f;
			int		i, h;
			double	fmax = -1E10;
			double[]
					fout;

			ofmax = double.NaN;
			try {
				if (fout2 == fin)
				{
					fout = new double[cnt];
				}
				else
				{
					fout = fout2;
				}
				for (i = 0; i < cnt; i++)
				{
					int	l = n/2;
					sum = 0.0;
					for (h = -l; h <= l; h++) {
						if ((h+i) < 0) {
							f = fin[0];
						}
						else if ((h+i) >= cnt) {
							f = fin[cnt-1];
						}
						else {
							f = fin[h+i];
						}
						sum += f;
					}
					fout[i] = sum / (2*l+1);
					if (fout[i] > fmax) {
						fmax = fout[i];
					}
				}
				if (fout != fout2)
				{
					Array.Copy(fout, fout2, cnt);
					//free(fout);
				}
				if (true /*ofmax != null*/)
				{
					ofmax = fmax;
				}
			}
			catch (Exception ex)
			{
				fmax = 1.0;
				MessageBox.Show(ex.ToString());
			}
			return (fmax);
		}

#if false
		//***************************************************************************
		/* 重心ピクセル位置の算出
		 * dat :ピクセルデータ
		 * fnm :ピクセルに対応する波長値の配列(datに一対一)
		 * femi:輝線の波長
		 * pwid:ピクセル範囲(最大値検索用,±で評価, -pwid ~ +pwidで検索)
		 * fPx :輝線の波長に対応する重心ピクセル位置
		 */
		//****************************************************************************
		static public bool FindCalPeak(double[] dat, double[] fnm, double fEmi, int pwid, ref double fPx)
		{
		//	double fLeftNm, fRightNm;
		//	BOOL	rc = FALSE;
			int		i, h, iL, iR;
			double	fmax = -9999.9f, fmin, f, fhaf;
			int		imax, imin = 0;
			double	fsum1 = 0,
					fsum2 = 0;

			// 輝線に一番近いピクセル位置を検索
#if false
			imin = fnm.Min((double ff) => Math.Abs(ff-fEmi));
			//imin = fnm.Min((double ff) => Math.Abs(ff-fEmi));
#else
			fmin = double.MaxValue;
			for (i = 0; i < fnm.Length; i++) {
				f = Math.Abs(fnm[i] - fEmi);
				if (fmin > f) {
					fmin = f;
					imin = i;
				}
			}
#endif

			// 最大ピクセル値を検索
			fmax = -1e99;
			for (h = -pwid; h <= pwid; h++) {
				i = imin + h;
				if (i < 0) {
					i = 0;
				}
				if (i >= C.PIXEL_MAX) {
					i = C.PIXEL_MAX-1;
				}
				if (fmax < dat[i]) {
					fmax = dat[i];
					imax = i;
				}
			}
			//---左側の半値位置を検索
			fhaf = fmax/2;
			for (i = imin; i > 0; i--) {
				if (dat[i] <= fhaf) {
					break;
				}
			}
			iL = i;
			//---右側の半値位置を検索
			fhaf = fmax/2;
			for (i = imin; i < C.PIXEL_MAX-1; i++) {
				if (dat[i] <= fhaf) {
					break;
				}
			}
			iR = i;
			// 最大値位置の重心ピクセルを(±9ピクセル範囲で)算出
#if false
			iL = imax - 9, iR = imax + 9;
			if (iL < 0 || iR >= PIXELS) {
				return(FALSE);	/* こうはならないハズ */
			}
#endif
			for (i = iL; i <= iR; i++) {
				fsum1 += i * dat[i];
				fsum2 +=     dat[i];
			}
			fPx  = fsum1 / fsum2;
			/****************************************************************************/
			return(true);
		}
		static public double RoundK(double f, int k)
		{
			int	e = (int)Math.Pow(10, k);

			f*= e;
			f = (f >= 0) ?  Math.Floor(f): Math.Ceiling(f);
			f/= e;
			return(e);
		}
#endif
		static private void SET_TAB_SEL_ENTER(object sender, EventArgs e)
		{
			TextBox t;
			if (false) {
			}
			else if (sender.GetType() == typeof(TextBox)) {
				t = (TextBox)sender;
			}
			else if (sender.GetType() == typeof(NumericUpDown)) {
				t = (TextBox)((NumericUpDown)sender).Controls[1];
			}
			else {
				return;
			}
			if (t.Text.Length > 0) {
				t.SelectAll();
			}
		}
		/*
		 * TextBoxフォーカス移動時の全選択処理
		 */
		static public int SET_TAB_SEL(Control ctl)
		{
			int	cnt = 0;
			foreach (Control c in ctl.Controls) {
				if (c.Controls.Count > 0) {
					if (c.HasChildren == false) {
						//int j = 0;
					}
					cnt += SET_TAB_SEL(c);
				}
				if (c.GetType() == typeof(TextBox)
				 || c.GetType() == typeof(NumericUpDown)
					) {
					c.Enter += new EventHandler(SET_TAB_SEL_ENTER);
					cnt++;
				}
			}
			return(cnt);
		}
	}
}
