using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//---
//using VectorD = System.Collections.Generic.List<double>;
//using VectorI = System.Collections.Generic.List<int>;

namespace uSCOPE
{
	class F_REMEZ_SCIPY
	{
		/********************************************************
		 *
		 *  Code taken from remez.c by Erik Kvaleberg which was 
		 *    converted from an original FORTRAN by
		 *
		 * AUTHORS: JAMES H. MCCLELLAN
		 *
		 *         DEPARTMENT OF ELECTRICAL ENGINEERING AND COMPUTER SCIENCE
		 *         MASSACHUSETTS INSTITUTE OF TECHNOLOGY
		 *         CAMBRIDGE, MASS. 02139
		 *
		 *         THOMAS W. PARKS
		 *         DEPARTMENT OF ELECTRICAL ENGINEERING
		 *         RICE UNIVERSITY
		 *         HOUSTON, TEXAS 77001
		 *
		 *         LAWRENCE R. RABINER
		 *         BELL LABORATORIES
		 *         MURRAY HILL, NEW JERSEY 07974
		 *
		 *  
		 *  Adaptation to C by 
		 *      egil kvaleberg
		 *      husebybakken 14a
		 *      0379 oslo, norway
		 *  Email:
		 *      egil@kvaleberg.no
		 *  Web:
		 *      http://www.kvaleberg.com/
		 * 
		 *
		 *********************************************************/
		public const int BANDPASS       = 1;
		public const int DIFFERENTIATOR = 2;
		public const int HILBERT        = 3;

		//const double M_PI      = (3.141592);
		//const double M_TWOPI   = (2.0*M_PI);

		//#define GOBACK goto
		//#define DOloop(a,from,to) for ( (a) = (from); (a) <= (to); ++(a))
		//#define PI    3.14159265358979323846
		//#define TWOPI (PI+PI)
		//const int GRIDDNS      = 32;
		//const int MAX_ITERAT   = 50;
		//const int MAX_ORDER    = 512;
		//const int MAX_TAPS     = MAX_ORDER + 1;
		//const int MAX_BANDS    = 5;

		//enum SC_TYPE {
		//    SIN = 0,
		//    COS = 1
		//};

		//public VectorD hk;
		/*
		 *-----------------------------------------------------------------------
		 * FUNCTION: lagrange_interp (d)
		 *  FUNCTION TO CALCULATE THE LAGRANGE INTERPOLATION
		 *  COEFFICIENTS FOR USE IN THE FUNCTION gee.
		 *-----------------------------------------------------------------------
		 */
		static double lagrange_interp(int k, int n, int m, double[] x)
		{
			int j, l;
			double q, retval;

			retval = 1.0;
			q = x[k];
			for (l = 1;l <= m; l++) {
				for (j = l; j <= n; j += m) {
					if (j != k) {
						retval *= 2.0 * (q - x[j]);
					}
				}
			}
			return 1.0 / retval;
		}

		/*
		 *-----------------------------------------------------------------------
		 * FUNCTION: freq_eval (gee)
		 *  FUNCTION TO EVALUATE THE FREQUENCY RESPONSE USING THE
		 *  LAGRANGE INTERPOLATION FORMULA IN THE BARYCENTRIC FORM
		 *-----------------------------------------------------------------------
		 */
		static double freq_eval(int k, int n, double[] grid, double[] x, double[] y, double[] ad)
		{
			int j;
			double p,c,d,xf;

			d = 0.0;
			p = 0.0;
			xf = Math.Cos(2*Math.PI * grid[k]);

			for (j = 1; j <= n;j++ ) {
				c = ad[j] / (xf - x[j]);
				d += c;
				p += c * y[j];
			}

			return p/d;
		}


		/*
		 *-----------------------------------------------------------------------
		 * SUBROUTINE: remez
		 *  THIS SUBROUTINE IMPLEMENTS THE REMEZ EXCHANGE ALGORITHM
		 *  FOR THE WEIGHTED CHEBYSHEV APPROXIMATION OF A CONTINUOUS
		 *  FUNCTION WITH A SUM OF COSINES.  INPUTS TO THE SUBROUTINE
		 *  ARE A DENSE GRID WHICH REPLACES THE FREQUENCY AXIS, THE
		 *  DESIRED FUNCTION ON THIS GRID, THE WEIGHT FUNCTION ON THE
		 *  GRID, THE NUMBER OF COSINES, AND AN INITIAL GUESS OF THE
		 *  EXTREMAL FREQUENCIES.  THE PROGRAM MINIMIZES THE CHEBYSHEV
		 *  ERROR BY DETERMINING THE BSMINEST LOCATION OF THE EXTREMAL
		 *  FREQUENCIES (POINTS OF MAXIMUM ERROR) AND THEN CALCULATES
		 *  THE COEFFICIENTS OF THE BEST APPROXIMATION.
		 *-----------------------------------------------------------------------
		 */
		static int remez(out double dev, double[] des, double[] grid, double[] edge,  
			   double[] wt, int ngrid, int nbands, int[] iext, double[] alpha,
			   int nfcns, int itrmax, double[] work, int dimsize, out int niter_out)
				/* dev, iext, alpha                         are output types */
				/* des, grid, edge, wt, ngrid, nbands, nfcns are input types */
		{
			int k, k1, kkk, kn, knz, klow, kup, nz, nzz, nm1;
			int cn;
			int j, jchnge, jet, jm1, jp1;
			int l, luck=0, nu, nut, nut1=0, niter;

			double ynz=0.0, comp=0.0, devl, gtemp, fsh, y1=0.0, err, dtemp, delf, dnum, dden;
			double aa=0.0, bb=0.0, ft, xe, xt;
#if true
			double[] a, p, q;
			double[] ad, x, y;
			//ara
			a  = new double[dimsize + 1];
			p  = new double[dimsize + 1];
			q  = new double[dimsize + 1];
			ad = new double[dimsize + 1];
			x  = new double[dimsize + 1];
			y  = new double[dimsize + 1];

			dev = 0;
			niter_out =0;
#else
			static double *a, *p, *q;
			static double *ad, *x, *y;
			a  = work;
			p  = a  + dimsize+1;
			q  = p  + dimsize+1; 
			ad = q  + dimsize+1;
			x  = ad + dimsize+1;
			y  = x  + dimsize+1;
#endif

devl = -1.0;
			nz  = nfcns+1;
			nzz = nfcns+2;
			niter = 0;

			do {
		L100:
				iext[nzz] = ngrid + 1;
				++niter;

				if (niter > itrmax) break;

				/* printf("ITERATION %2d: ",niter); */

				for (j=1;j<=nz;j++) {
					x[j] = Math.Cos(grid[iext[j]]*2*Math.PI);
				}
				jet = (nfcns-1) / 15 + 1;

				for (j=1;j<=nz;j++) {
					ad[j] = lagrange_interp(j,nz,jet,x);
				}

				dnum = 0.0;
				dden = 0.0;
				k = 1;

				for (j=1;j<=nz;j++) {
					l = iext[j];
					dnum += ad[j] * des[l];
					dden += (double)k * ad[j] / wt[l];
					k = -k;
				}
				dev = dnum / dden;

				/* printf("DEVIATION = %lg\n",*dev); */

				nu = 1;
				if ( (dev) > 0.0 ) nu = -1;
				dev = -(double)nu * dev;
				k = nu;
				for (j=1;j<=nz;j++) {
					l = iext[j];
					y[j] = des[l] + (double)k * dev / wt[l];
					k = -k;
				}
				if ( dev <= devl ) {
					/* finished */
					niter_out = niter;
					return -1;
				}
				devl = dev;
				jchnge = 0;
				k1 = iext[1];
				knz = iext[nz];
				klow = 0;
				nut = -nu;
				j = 1;

			/*
			 * SEARCH FOR THE EXTREMAL FREQUENCIES OF THE BEST APPROXIMATION
			 */

			L200:
				if (j == nzz) ynz = comp;
				if (j >= nzz) goto L300;
				kup = iext[j+1];
				l = iext[j]+1;
				nut = -nut;
				if (j == 2) y1 = comp;
				comp = dev;
				if (l >= kup) goto L220;
				err = (freq_eval(l,nz,grid,x,y,ad)-des[l]) * wt[l];
				if (((double)nut*err-comp) <= 0.0) goto L220;
				comp = (double)nut * err;
			L210:
				if (++l >= kup) goto L215;
				err = (freq_eval(l,nz,grid,x,y,ad)-des[l]) * wt[l];
				if (((double)nut*err-comp) <= 0.0) goto L215;
				comp = (double)nut * err;
				goto L210;

			L215:
				iext[j++] = l - 1;
				klow = l - 1;
				++jchnge;
				goto L200;

			L220:
				--l;
			L225:
				if (--l <= klow) goto L250;
				err = (freq_eval(l,nz,grid,x,y,ad)-des[l]) * wt[l];
				if (((double)nut*err-comp) > 0.0) goto L230;
				if (jchnge <= 0) goto L225;
				goto L260;

			L230:
				comp = (double)nut * err;
			L235:
				if (--l <= klow) goto L240;
				err = (freq_eval(l,nz,grid,x,y,ad)-des[l]) * wt[l];
				if (((double)nut*err-comp) <= 0.0) goto L240;
				comp = (double)nut * err;
				goto L235;
			L240:
				klow = iext[j];
				iext[j] = l+1;
				++j;
				++jchnge;
				goto L200;

			L250:
				l = iext[j]+1;
				if (jchnge > 0) goto L215;

			L255:
				if (++l >= kup) goto L260;
				err = (freq_eval(l,nz,grid,x,y,ad)-des[l]) * wt[l];
				if (((double)nut*err-comp) <= 0.0) goto L255;
				comp = (double)nut * err;

				goto L210;
			L260:
				klow = iext[j++];
				goto L200;

			L300:
				if (j > nzz) goto L320;
				if (k1 > iext[1] ) k1 = iext[1];
				if (knz < iext[nz]) knz = iext[nz];
				nut1 = nut;
				nut = -nu;
				l = 0;
				kup = k1;
				comp = ynz*(1.00001);
				luck = 1;
			L310:
				if (++l >= kup) goto L315;
				err = (freq_eval(l,nz,grid,x,y,ad)-des[l]) * wt[l];
				if (((double)nut*err-comp) <= 0.0) goto L310;
				comp = (double) nut * err;
				j = nzz;
				goto L210;

			L315:
				luck = 6;
				goto L325;

			L320:
				if (luck > 9) goto L350;
				if (comp > y1) y1 = comp;
				k1 = iext[nzz];
			L325:
				l = ngrid+1;
				klow = knz;
				nut = -nut1;
				comp = y1*(1.00001);
			L330:
				if (--l <= klow) goto L340;
				err = (freq_eval(l,nz,grid,x,y,ad)-des[l]) * wt[l];
				if (((double)nut*err-comp) <= 0.0) goto L330;
				j = nzz;
				comp = (double) nut * err;
				luck = luck + 10;
				goto L235;
			L340:
				if (luck == 6) goto L370;
				for (j=1;j<=nfcns;j++) {
					iext[nzz-j] = iext[nz-j];
				}
				iext[1] = k1;
				goto L100;
			L350:
				kn = iext[nzz];
				for (j=1;j<=nfcns;j++) {
					iext[j] = iext[j+1];
				}
				iext[nz] = kn;

				goto L100;
			L370:
				continue;
			} while (jchnge > 0);

		/*
		 *    CALCULATION OF THE COEFFICIENTS OF THE BEST APPROXIMATION
		 *    USING THE INVERSE DISCRETE FOURIER TRANSFORM
		 */
			nm1 = nfcns - 1;
			fsh = 1.0e-06;
			gtemp = grid[1];
			x[nzz] = -2.0;
			cn  = 2*nfcns - 1;
			delf = 1.0/cn;
			l = 1;
			kkk = 0;

			if (edge[1] == 0.0 && edge[2*nbands] == 0.5) kkk = 1;

			if (nfcns <= 3) kkk = 1;
			if (kkk !=     1) {
			dtemp = Math.Cos(2*Math.PI*grid[1]);
			dnum  = Math.Cos(2*Math.PI*grid[ngrid]);
			aa    = 2.0/(dtemp-dnum);
			bb    = -(dtemp+dnum)/(dtemp-dnum);
			}

			for (j=1;j<=nfcns;j++) {
				ft = (j - 1) * delf;
				xt = Math.Cos(2*Math.PI*ft);
				if (kkk != 1) {
					xt = (xt-bb)/aa;
		#if false
				/*XX* ckeck up !! */
				xt1 = sqrt(1.0-xt*xt);
				ft = atan2(xt1,xt)/(2*Math.PI);
		#else
					ft = Math.Acos(xt)/(2*Math.PI);
		#endif
				}
		L410:
				xe = x[l];
				if (xt > xe) goto L420;
				if ((xe-xt) < fsh) goto L415;
				++l;
				goto L410;
		L415:
				a[j] = y[l];
				goto L425;
		L420:
				if ((xt-xe) < fsh) goto L415;
				grid[1] = ft;
				a[j] = freq_eval(1,nz,grid,x,y,ad);
		L425:
				if (l > 1) l = l-1;
			}

			grid[1] = gtemp;
			dden = (2*Math.PI) / cn;
			for (j=1;j<=nfcns;j++) {
				dtemp = 0.0;
				dnum = (j-1) * dden;
				if (nm1 >= 1) {
					for (k=1;k<=nm1;k++) {
						dtemp += a[k+1] * Math.Cos(dnum*k);
					}
				}
				alpha[j] = 2.0 * dtemp + a[1];
			}

			for (j=2;j<=nfcns;j++) {
				alpha[j] *= 2.0 / cn;
			}
			alpha[1] /= cn;

			if (kkk != 1) {
				p[1] = 2.0*alpha[nfcns]*bb+alpha[nm1];
				p[2] = 2.0*aa*alpha[nfcns];
				q[1] = alpha[nfcns-2]-alpha[nfcns];
				for (j=2;j<=nm1;j++) {
					if (j >= nm1) {
						aa *= 0.5;
						bb *= 0.5;
					}
					p[j+1] = 0.0;
					for (k=1;k<=j;k++) {
						a[k] = p[k];
						p[k] = 2.0 * bb * a[k];
					}
					p[2] += a[1] * 2.0 *aa;
					jm1 = j - 1;
					for (k=1;k<=jm1;k++) p[k] += q[k] + aa * a[k+1];
					jp1 = j + 1;
					for (k=3;k<=jp1;k++) p[k] += aa * a[k-1];

					if (j != nm1) {
						for (k=1;k<=j;k++) q[k] = -a[k];
						q[1] += alpha[nfcns - 1 - j];
					}
				}
				for (j = 1; j <= nfcns; j++) alpha[j] = p[j];
			}

			if (nfcns <= 3) {
				alpha[nfcns+1] = alpha[nfcns+2] = 0.0;
			}
			return 0;
		}


		/*
		 *-----------------------------------------------------------------------
		 * FUNCTION: eff
		 *  FUNCTION TO CALCULATE THE DESIRED MAGNITUDE RESPONSE
		 *  AS A FUNCTION OF FREQUENCY.
		 *  AN ARBITRARY FUNCTION OF FREQUENCY CAN BE
		 *  APPROXIMATED IF THE USER REPLACES THIS FUNCTION
		 *  WITH THE APPROPRIATE CODE TO EVALUATE THE IDEAL
		 *  MAGNITUDE.  NOTE THAT THE PARAMETER FREQ IS THE
		 *  VALUE OF NORMALIZED FREQUENCY NEEDED FOR EVALUATION.
		 *-----------------------------------------------------------------------
		 */
		static double eff(double freq, double[] fx, int lband, int jtype)
		{
			  if (jtype != 2) return fx[lband];
			  else            return fx[lband] * freq;
		}

		/*
		 *-----------------------------------------------------------------------
		 * FUNCTION: wate
		 *  FUNCTION TO CALCULATE THE WEIGHT FUNCTION AS A FUNCTION
		 *  OF FREQUENCY.  SIMILAR TO THE FUNCTION eff, THIS FUNCTION CAN
		 *  BE REPLACED BY A USER-WRITTEN ROUTINE TO CALCULATE ANY
		 *  DESIRED WEIGHTING FUNCTION.
		 *-----------------------------------------------------------------------
		 */
		static double wate(double freq, double[] fx, double[] wtx, int lband, int jtype)
		{
			  if (jtype != 2)          return wtx[lband];
			  if (fx[lband] >= 0.0001) return wtx[lband] / freq;
			  return                          wtx[lband];
		}
		static double[] ins0(double[] a)
		{
			List<double> l = new List<double>(a);
			l.Insert(0, -1);
			return (l.ToArray());
		}
		/*********************************************************/

		/*  This routine accepts basic input information and puts it in 
		 *  the form expected by remez.

		 *  Adpated from main() by Travis Oliphant
		 */

		static int pre_remez(double[] h2, int numtaps, int numbands, double[] bands,
							 double[] response, double[] weight, int type, int maxiter,
							 int grid_density, out int niter_out)
		{
			int jtype, nbands, nfilt, lgrid, nz;
			int neg, nodd, nm1;
			int j, k, l, lband, dimsize;
			double delf, change, fup, temp;
			double[]tempstor,edge,h,fx,wtx;
			double[]des, grid, wt, alpha, work;
			double dev;
			int ngrid;
			int[] iext;
			int nfcns, wrksize, total_dsize, total_isize;

			lgrid = grid_density;
			dimsize = (int) Math.Ceiling(numtaps/2.0 + 2);
			wrksize = grid_density * dimsize;
			nfilt = numtaps;
			jtype = type; nbands = numbands;
			/* Note:  code assumes these arrays start at 1 */
#if true
			niter_out = 0;
			edge = ins0(bands);
			h    = ins0(h2);
			fx   = ins0(response);
			wtx  = ins0(weight);
#else
			edge = bands-1; 
			h = h2 - 1;
			fx = response - 1;
			wtx = weight - 1;
#endif
			total_dsize = (dimsize+1)*7 + 3*(wrksize+1);
			total_isize = (dimsize+1);
			/* Need space for:  (all arrays ignore the first element).

				des  (wrksize+1)
				grid (wrksize+1)
				wt   (wrksize+1)
				iext (dimsize+1)   (integer)
				alpha (dimsize+1)
				work  (dimsize+1)*6 

			*/
#if true
			des   = new double[wrksize + 1];
			grid  = new double[wrksize + 1];
			wt    = new double[wrksize + 1];
			iext  = new int   [dimsize + 1];
			alpha = new double[dimsize + 1];
			work = null;// new double[(dimsize + 1)*6];
#else
			tempstor = malloc((total_dsize)*sizeof(double)+(total_isize)*sizeof(int));
			if (tempstor == NULL) return -2;

			des = tempstor;
			grid = des + wrksize+1;
			wt = grid + wrksize+1;
			alpha = wt + wrksize+1;
			work = alpha + dimsize+1; iext = (int *)(work + (dimsize+1)*6);
#endif
			/* Set up problem on dense_grid */

			neg = 1;
			if (jtype == 1) neg = 0;
			nodd = nfilt % 2;
			nfcns = nfilt / 2;
			if (nodd == 1 && neg == 0) nfcns = nfcns + 1;

			/*
			 * SET UP THE DENSE GRID. THE NUMBER OF POINTS IN THE GRID
			 * IS (FILTER LENGTH + 1)*GRID DENSITY/2
			 */
			grid[1] = edge[1];
			delf = lgrid * nfcns;
			delf = 0.5 / delf;
			if (neg != 0) {
			if (edge[1] < delf) grid[1] = delf;
			}
			j = 1;
			l = 1;
			lband = 1;

			/*
			 * CALCULATE THE DESIRED MAGNITUDE RESPONSE AND THE WEIGHT
			 * FUNCTION ON THE GRID
			 */
			for (;;) {
			fup = edge[l + 1];
			do {
				temp = grid[j];
				des[j] = eff(temp,fx,lband,jtype);
				wt[j] = wate(temp,fx,wtx,lband,jtype);
				if (++j > wrksize) {
						/* too many points, or too dense grid */
#if false
						free(tempstor);
#endif
						return -1;
					} 
				grid[j] = temp + delf;
			} while (grid[j] <= fup);

			grid[j-1] = fup;
			des[j-1] = eff(fup,fx,lband,jtype);
			wt[j-1] = wate(fup,fx,wtx,lband,jtype);
			++lband;
			l += 2;
			if (lband > nbands) break;
			grid[j] = edge[l];
			}

			ngrid = j - 1;
			if (neg == nodd) {
			if (grid[ngrid] > (0.5-delf)) --ngrid;
			}

			/*
			 * SET UP A NEW APPROXIMATION PROBLEM WHICH IS EQUIVALENT
			 * TO THE ORIGINAL PROBLEM
			 */
			if (neg <= 0) {
				if (nodd != 1) {
					for (j=1;j<=ngrid;j++) {
						change = Math.Cos(Math.PI*grid[j]);
						des[j] = des[j] / change;
						wt[j]  = wt[j] * change;
					}
				}
			}
			else {
				if (nodd != 1) {
					for (j=1;j<=ngrid;j++) {
						change = Math.Sin(Math.PI * grid[j]);
						des[j] = des[j] / change;
						wt[j]  = wt[j]  * change;
					}
				}
				else {
					for (j=1;j<=ngrid;j++) {
						change = Math.Sin(Math.PI*2 * grid[j]);
						des[j] = des[j] / change;
						wt[j]  = wt[j]  * change;
					}
				}
			}

			/*XX*/
			temp = (double)(ngrid-1) / (double)nfcns;
			for (j=1;j<=nfcns;j++) {
				iext[j] = (int)((j-1)*temp) + 1; /* round? !! */
			}
			iext[nfcns+1] = ngrid;
			nm1 = nfcns - 1;
			nz  = nfcns + 1;

			if (remez(out dev, des, grid, edge, wt, ngrid, numbands, iext, alpha, nfcns,
					  maxiter, work, dimsize, out niter_out) < 0) {
#if false
				free(tempstor);
#endif
				return -1;
			}

			/*
			 * CALCULATE THE IMPULSE RESPONSE.
			 */
			if (neg <= 0) {

			if (nodd != 0) {
				for (j=1;j<=nm1;j++) {
				h[j] = 0.5 * alpha[nz-j];
				}
				h[nfcns] = alpha[1];
			} else {
				h[1] = 0.25 * alpha[nfcns];
				for (j=2;j<=nm1;j++) {
				h[j] = 0.25 * (alpha[nz-j] + alpha[nfcns+2-j]);
				}
				h[nfcns] = 0.5*alpha[1] + 0.25*alpha[2];
			}
			} else {
			if (nodd != 0) {
				h[1] = 0.25 * alpha[nfcns];
				h[2] = 0.25 * alpha[nm1];
				for (j=3;j<=nm1;j++) {
				h[j] = 0.25 * (alpha[nz-j] - alpha[nfcns+3-j]);
				}
				h[nfcns] = 0.5 * alpha[1] - 0.25 * alpha[3];
				h[nz] = 0.0;
			} else {
				h[1] = 0.25 * alpha[nfcns];
				for (j=2;j<=nm1;j++) {
				h[j] = 0.25 * (alpha[nz-j] - alpha[nfcns+2-j]);
				}
				h[nfcns] = 0.5 * alpha[1] - 0.25 * alpha[2];
			}
			}

			for (j=1;j<=nfcns;j++) {
				k = nfilt + 1 - j;
				if (neg == 0)
				   h[k] = h[j];
				else
				   h[k] = -h[j];
			}
			if (neg == 1 && nodd == 1) h[nz] = 0.0;
#if false
		  free(tempstor);
#else
			for (int i = 0; i < h2.Length; i++) {
				h2[i] = h[i+1];
			}
#endif
		  return 0;
		}

		/**************************************************************
		 * End of remez routines 
		 **************************************************************/
		private const int PyExc_ValueError = 0;
		static public string ERRMSG;
		static private void PyErr_SetString(int type, string msg)
		{
			ERRMSG = msg;
		}
		static string doc_remez =
		"h = _remez(numtaps, bands, des, weight, type, fs, maxiter, grid_density)\n"+
		"  returns the optimal (in the Chebyshev/minimax sense) FIR filter impulse\n"+
		"  response given a set of band edges, the desired response on those bands,\n"+
		"  and the weight given to the error in those bands.  Bands is a monotonic\n"+
		"  vector with band edges given in frequency domain where fs is the sampling\n"+
		"  frequency.";

		static
#if false
			PyObject *sigtools_remez(PyObject *NPY_UNUSED(dummy), PyObject *args)
#else
			public 
 double[] sigtools_remez(int numtaps, double[]bands, double[]des, double[]weight,
									int type = BANDPASS, double fs = 1.0, int maxiter = 25, int grid_density = 16
			)
#endif
		{
#if false
			PyObject *bands, *des, *weight;
			int k, numtaps, numbands, type = BANDPASS, err; 
			PyArrayObject *a_bands=NULL, *a_des=NULL, *a_weight=NULL;
			PyArrayObject *h=NULL;
			intp ret_dimens; int maxiter = 25, grid_density = 16;
			double oldvalue, *dptr, fs = 1.0;
			char mystr[255];
			int niter = -1;

			if (!PyArg_ParseTuple(args, "iOOO|idii", &numtaps, &bands, &des, &weight, 
								  &type, &fs, &maxiter, &grid_density)) {
				return NULL;
			}
#else
			int k, numbands, err;
			double[] a_bands=null, a_des=null, a_weight=null;
			double[] h=null;
			int ret_dimens;
			double oldvalue;
			double[] dptr;
			//char mystr[255];
			int niter = -1;

#endif
			if (type != BANDPASS && type != DIFFERENTIATOR && type != HILBERT) {
				PyErr_SetString(PyExc_ValueError,
								"The type must be BANDPASS, DIFFERENTIATOR, or HILBERT.");
				return null;
			}

			if (numtaps < 2) {
				PyErr_SetString(PyExc_ValueError,
								"The number of taps must be greater than 1.");
				return null;
			}
#if false
			a_bands = (PyArrayObject *)PyArray_ContiguousFromObject(bands, NPY_DOUBLE,1,1);
			if (a_bands == NULL) goto fail;
			a_des = (PyArrayObject *)PyArray_ContiguousFromObject(des, NPY_DOUBLE,1,1);
			if (a_des == NULL) goto fail;
			a_weight = (PyArrayObject *)PyArray_ContiguousFromObject(weight, NPY_DOUBLE,1,1);
			if (a_weight == NULL) goto fail;

			numbands = PyArray_DIMS(a_des)[0];
			if ((PyArray_DIMS(a_bands)[0] != 2*numbands) || 
				(PyArray_DIMS(a_weight)[0] != numbands)) {
				PyErr_SetString(PyExc_ValueError,
								"The inputs desired and weight must have same length.\n  "
								"The input bands must have twice this length.");
				goto fail;
			}
#else
			a_bands  = bands;
			a_des    = des;
			a_weight = weight;
			numbands = a_des.Length;
			if (a_bands.Length != 2*numbands || a_weight.Length != 1*numbands) {
				PyErr_SetString(PyExc_ValueError,
								"The inputs desired and weight must have same length.\n  "+
								"The input bands must have twice this length.");
				goto fail;
			}
#endif
			/* 
			 * Check the bands input to see if it is monotonic, divide by 
			 * fs to take from range 0 to 0.5 and check to see if in that range
			 */
#if false
			dptr = (double *)PyArray_DATA(a_bands);
			oldvalue = 0;
			for (k=0; k < 2*numbands; k++) {
				if (*dptr < oldvalue) {
					PyErr_SetString(PyExc_ValueError,
									"Bands must be monotonic starting at zero.");
					goto fail;
				}
				if (*dptr * 2 > fs) {
					PyErr_SetString(PyExc_ValueError,
									"Band edges should be less than 1/2 the sampling frequency");
					goto fail;
				}
				oldvalue = *dptr;
				*dptr = oldvalue / fs;  /* Change so that sampling frequency is 1.0 */
				dptr++;
			}
#else
			dptr = a_bands;
			oldvalue = 0;
			for (k = 0; k < 2 * numbands; k++) {
				if (dptr[k] < oldvalue) {
					PyErr_SetString(PyExc_ValueError,
									"Bands must be monotonic starting at zero.");
					goto fail;
				}
				if (dptr[k] * 2 > fs) {
					PyErr_SetString(PyExc_ValueError,
									"Band edges should be less than 1/2 the sampling frequency");
					goto fail;
				}
				oldvalue = dptr[k];
				dptr[k] = oldvalue / fs;  /* Change so that sampling frequency is 1.0 */
			}
#endif
			ret_dimens = numtaps;
#if true
			h = new double[ret_dimens];

			err = pre_remez(h, numtaps, numbands, 
							a_bands,
							a_des,
							a_weight,
							type, maxiter, grid_density, out niter);
#else
			h = (PyArrayObject *)PyArray_SimpleNew(1, &ret_dimens, NPY_DOUBLE);
			if (h == NULL) goto fail;

			err = pre_remez((double *)PyArray_DATA(h), numtaps, numbands, 
							(double *)PyArray_DATA(a_bands),
							(double *)PyArray_DATA(a_des),
							(double *)PyArray_DATA(a_weight),
							type, maxiter, grid_density, &niter);
#endif
			if (err < 0) {
				if (err == -1) {
					PyErr_SetString(PyExc_ValueError, string.Format("Failure to converge at iteration {0}, try reducing transition band width.\n", niter));
					goto fail;
				}
				else if (err == -2) {
					//PyErr_NoMemory();
					goto fail;
				}
			}

			//Py_DECREF(a_bands);
			//Py_DECREF(a_des);
			//Py_DECREF(a_weight);

			//return PyArray_Return(h);
			return(h);
		fail:
			//Py_XDECREF(a_bands);
			//Py_XDECREF(a_des);
			//Py_XDECREF(a_weight);
			//Py_XDECREF(h);
			return null;
		}
	}
}
