using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//---
using VectorD = System.Collections.Generic.List<double>;
using VectorI = System.Collections.Generic.List<int>;

namespace uSCOPE
{
	class F_REMEZ
	{
		const int BANDPASS     = 0;
		const int DIFFERENTIAL = 1;
		const int HILBERT      = 2;

		const double M_PI      = (3.141592);
		const double M_PI2     = (2.0*M_PI);
		const int GRIDDNS      = 32;
		const int MAX_ITERAT   = 50;
		const int MAX_ORDER    = 512;
		const int MAX_TAPS     = MAX_ORDER + 1;
		const int MAX_BANDS    = 5;

		enum SC_TYPE {
			SIN = 0,
			COS = 1
		};

		public VectorD hk;

		// Remez のアルゴリズム
		public bool Remez(int ntaps, int nbands, VectorD bands, /*const*/VectorD dg,
				   /*const*/VectorD weight, VectorD deviat, int type, double fs)
		{
			double csx, deviation=0;
			int    i, iter, g_size, r;
			bool   symmetry;

			symmetry = (type == BANDPASS) ? true : false;
			r = ((ntaps % 2)!=0 && symmetry) ? (ntaps/2 + 1) : ntaps/2;    // 極値の数

			g_size = 0;
			if (fs != 1.0)
				for (i=0; i<nbands*2; i++) bands[i] = bands[i]/fs;
			for (i=0; i<nbands; i++) {
				g_size = (int)(g_size + 2*r*GRIDDNS*(bands[2*i+1] - bands[2*i]) + 0.5);
			}
			if (!symmetry) g_size--;

			VectorD grid = new VectorD(new double[g_size+1]);
			VectorD wght = new VectorD(new double[g_size+1]);
			VectorD desired = new VectorD(new double[g_size+1]);
			VectorD w_er = new VectorD(new double[g_size+1]);
			VectorD taps = new VectorD(new double[r+1]);
			VectorD cwk = new VectorD(new double[r+1]);
			VectorD ck = new VectorD(new double[r+1]);
			VectorD bk = new VectorD(new double[r+1]);
			VectorI ext = new VectorI(new int[2*r+1]);

			SetGrid(r, ntaps, nbands, bands, dg, weight, ref g_size, grid,
					desired, wght, symmetry);
		// 極値の初期値
			for (i=0; i<=r; i++) ext[i] = i*(g_size-1)/r;

			if (type == DIFFERENTIAL) {  // 微分器の場合
				for (i=0; i<g_size; i++) {
					desired[i] = desired[i]*grid[i];
					if (desired[i] > 0.0001) wght[i] = wght[i]/grid[i];
				}
			}

			if (symmetry) {
				if (ntaps % 2 == 0) {
					dw(SC_TYPE.COS, g_size, M_PI, grid, desired, wght);
				}
			}
			else {
				if ((ntaps % 2)!= 0) {
					dw(SC_TYPE.SIN, g_size, M_PI2, grid, desired, wght);
				}
				else {
					dw(SC_TYPE.SIN, g_size, M_PI, grid, desired, wght);
				}
			}

		// Remez のアルゴリズムの実行
			for (iter=0; iter<MAX_ITERAT; iter++)
			{
				if (!CalcParms(r, ext, grid, desired, wght, bk, cwk, ck)) {
					return false;
				}
				for (int q=0; q<g_size; q++) {
					w_er[q] = wght[q]*(desired[q] - CalcResp(grid[q], r, bk, cwk, ck));
				}
				SearchExtrms(r, ext, g_size, w_er);
				if (IsConverged(r, ext, w_er, out deviation)) {
					break;
				}
			}
			if (iter == MAX_ITERAT) return false;   // 収束しない

			for (i=0; i<nbands; i++) deviat[i] = deviation/weight[i];

			if (!CalcParms(r, ext, grid, desired, wght, bk, cwk, ck)) {
				return false;                       // 収束しない
			}

			for (i=0; i<=ntaps/2; i++) {
				if (symmetry) {
					csx = (ntaps % 2) != 0 ? 1.0 : Math.Cos(M_PI*i/ntaps);
				}
				else {
					csx = (ntaps % 2) != 0 ? Math.Sin(M_PI2*i/ntaps) : Math.Sin(M_PI*i/ntaps);
				}
				taps[i] = CalcResp((double)i/ntaps, r, bk, cwk, ck)*csx;
			}

			ToCoefs(ntaps, taps, symmetry);
			return true;    // 収束した
		}

		// 誤差極値の初期値の設定
		void SetGrid(int r, int ntaps, int nbands, /*const*/VectorD bands, /*const*/VectorD dg,
					 /*const*/VectorD weight, ref int g_size, VectorD grid,
					 VectorD desired, VectorD wght, bool symmetry)
		{
			int i, j, k, nb;
			double dF, fL, fH;
			VectorD bandsX = bands;

			dF = 0.5/(GRIDDNS*r);
			if (!symmetry && (dF > bandsX[0])) bandsX[0] = dF;

			j=0;
			for (nb=0; nb<nbands; nb++)
			{
				grid[j] = bandsX[2*nb];
				fL = bandsX[2*nb];
				fH = bandsX[2*nb+1];
				k = (int)((fH - fL)/dF + 0.5);

				for (i=0; i<k; i++)
				{
					desired[j] = dg[nb];
					wght[j] = weight[nb];
					grid[j] = fL;
					fL = fL + dF;
					j++;
				}
				grid[j-1] = fH;
			}

			if (!symmetry && (grid[g_size-1] > (0.5 - dF)) && (ntaps % 2) != 0) {
				grid[g_size-1] = 0.5 - dF;
			}
		}

		// desired[], wght[] の設定
		void dw(SC_TYPE SC, int g_size, double pi, /*const*/VectorD grid,
						VectorD desired, VectorD wght)
		{
			double csx;
			for (int i=0; i<g_size; i++)
			{
				if (SC == SC_TYPE.SIN) {
					csx = Math.Sin(pi*grid[i]);
				}
				else {
					csx = Math.Cos(pi*grid[i]);
				}
				desired[i] = desired[i]/csx;
				wght[i] = wght[i]*csx;
			}
		}

		// パラメータの計算
		bool CalcParms(int r, /*const*/VectorI ext, /*const*/VectorD grid,
					   /*const*/VectorD desired, /*const*/VectorD wght, VectorD bk,
					   VectorD cwk, VectorD ck)
		{
			int i, j, k, jmax;
			double sign, xi, delta, denom, numer;

			for (i=0; i<=r; i++) cwk[i] = Math.Cos(M_PI2*grid[ext[i]]);

			jmax = (r - 1)/15 + 1;
			for (i=0; i<=r; i++)
			{
				denom = 1.0;
				xi = cwk[i];
				for (j=0; j<jmax; j++)
					for (k=j; k<=r; k+=jmax)
						if (k != i) denom =  2.0*denom*(xi - cwk[k]);
				if (Math.Abs(denom)<0.00001) denom = 0.00001;
				bk[i] = 1.0/denom;
		   }

			numer = 0.0;
			denom = 0.0;
			sign = 1;
			for (i=0; i<=r; i++)
			{
				numer = numer + bk[i]*desired[ext[i]];
				denom = denom + sign*bk[i]/wght[ext[i]];
				sign = -sign;
			}
			if (denom == 0.0) return false;
			delta = numer/denom;
			sign = 1;

			for (i=0; i<=r; i++)
			{
				ck[i] = desired[ext[i]] - sign*delta/wght[ext[i]];
				sign = -sign;
			}
			return true;
		}

		// フィルタの応答の計算
		double CalcResp(double freq, int r, /*const*/VectorD bk, /*const*/VectorD cwk,
						/*const*/VectorD ck)
		{
			double cos_f, cfx, denom, numer;

			denom = 0.0;
			numer = 0.0;
			cos_f = Math.Cos(M_PI2*freq);
			for (int i=0; i<=r; i++)
			{
				cfx = cos_f - cwk[i];
				if (Math.Abs(cfx) < 1.0e-7)
				{
					numer = ck[i];
					denom = 1.0;
					break;
				}
				cfx = bk[i]/cfx;
				denom = denom + cfx;
				numer = numer + cfx*ck[i];
			}
			return (numer/denom);
		}

		// 誤差の極大，極小の探索
		void SearchExtrms(int r, VectorI ext, int g_size, /*const*/VectorD w_er)
		{
			int i, k, m, extra, up, alt;

			k = 0;

			if (((w_er[0]>0.0) && (w_er[0]>w_er[1])) ||
				((w_er[0]<0.0) && (w_er[0]<w_er[1]))) ext[k++] = 0;

			for (i=1; i<g_size-1; i++)
				if (((w_er[i]>=w_er[i-1]) && (w_er[i]>w_er[i+1]) && (w_er[i]>0.0)) ||
					((w_er[i]<=w_er[i-1]) && (w_er[i]<w_er[i+1]) && (w_er[i]<0.0)))
					ext[k++] = i;

			i = g_size-1;
			if (((w_er[i]>0.0) && (w_er[i]>w_er[i-1])) ||
				((w_er[i]<0.0) && (w_er[i]<w_er[i-1]))) ext[k++] = i;

			extra = k - (r + 1);

			while (extra > 0)
			{
				if (w_er[ext[0]] > 0.0) up = 1;   // 最初が極大の場合
				else                    up = 0;   // 最初が極小の場合

				m=0;
				alt = 1;
				for (i=1; i<k; i++)
				{
					if (Math.Abs(w_er[ext[i]]) < Math.Abs(w_er[ext[m]]))
						m = i;
					if (up!=0 && (w_er[ext[i]] < 0.0))
						up = 0;
					else if (up==0 && (w_er[ext[i]] > 0.0))
						up = 1;
					else
					{
						alt = 0;
						break;
					}
				}

				if (alt!=0 && (extra == 1))
				{
					if (Math.Abs(w_er[ext[k-1]]) < Math.Abs(w_er[ext[0]]))
						m = ext[k-1];   // 最後の極小を除去
					else
						m = ext[0];     // 最初の極小を除去
				}

				for (i=m; i<k; i++) ext[i] = ext[i+1];
				k--;
				extra--;
			}
		}

		// 最適近似関数からフィルタ係数を計算
		void ToCoefs(int ntaps, /*const*/VectorD taps, bool symmetry)
		{
			double sum, nh, fntp;
			//
			hk = new VectorD(new double[ntaps]);
			//
			fntp = (double)ntaps;
			nh = (ntaps - 1.0)/2.0;
			if (symmetry)
				for (int n=0; n<ntaps; n++)
				{
					sum = taps[0];
					for (int k=1; k<=(ntaps-1)/2; k++)
						sum = sum + 2.0*taps[k]*Math.Cos(k*M_PI2*(n - nh)/fntp);
					hk[n] = sum/fntp;
				}
			else
				for (int n=0; n<ntaps; n++)
				{
					sum = (ntaps % 2)!=0 ? 0.0 : taps[ntaps/2]*Math.Sin(M_PI*(n - nh));
					for (int k=1; k<=(ntaps-1)/2; k++)
						sum = sum + 2.0*taps[k]*Math.Sin(k*M_PI2*(n - nh)/fntp);
					hk[n] = sum/fntp;
				}
		}

		// 収束の判定
		bool IsConverged(int r, /*const*/VectorI ext, /*const*/VectorD w_er, out double deviation)
		{
			double extrm, min, max;

			min = Math.Abs(w_er[ext[0]]);
			max = min;
			for (int i=1; i<=r; i++)
			{
				extrm = Math.Abs(w_er[ext[i]]);
				min = extrm < min ? extrm : min;
				max = extrm > max ? extrm : max;
			}
			deviation = max;
			if (((max-min)/max) < 0.0001) return (true);    // 収束
			return (false);                                 // 未収束
		}







	}
}
