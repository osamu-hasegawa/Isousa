using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//---
using System.Drawing;

namespace uSCOPE
{
	class FN1D
	{
		public double A;//傾き
		public double B;//切片Y
		public double C;//切片X(垂線の場合)
		public PointF P1;
		public PointF P2;
		FN1D()
		{
			this.A = this.B = this.C = double.NaN;
			this.P1.X = this.P1.Y = float.NaN;
		}
		public FN1D(double A, double B):this(A, B, double.NaN)
		{
		}
		public FN1D(double A, double B, double C)
		{
			this.A = A;
			this.B = B;
			this.C = C;
			if (double.IsNaN(this.A)) {
				//垂直
				this.C = C;
			}
			else if (this.A == 0.0) {
				//水平
				this.C = double.NaN;
			}
			else {
				this.C = (-this.B) / this.A;
			}
			this.P1.X = this.P1.Y = float.NaN;
		}
		public FN1D(Point p1, Point p2):this((PointF)p1, (PointF)p2)
		{
		}
		public FN1D(PointF p1, PointF p2)
		{
			double dx = p2.X - p1.X;
			double dy = p2.Y - p1.Y;
			this.A = dy/dx;
			this.B = p1.Y - A * p1.X;
			if (dx == 0.0) {
			this.A = double.NaN;
			this.B = double.NaN;
			this.C = p1.X;
			}
			else {
			this.C = (p1.Y - this.B) / this.A;
			}
			this.P1 = p1;
			this.P2 = p2;
		}
		//FN1D(double A, Point p0)
		//{//傾きAで点p0を通る直線
		//}
		//FN1D(double A, PointF p0)
		//{//傾きAで点p0を通る直線
		//}
		//---
		public double GetYatX(double x)
		{
			if (double.IsNaN(this.A)) {
				//垂直
				return(double.NaN);
			}
			else {
				return(this.A * x + this.B);
			}
		}
		//---
		public double GetXatY(double y)
		{
			if (double.IsNaN(this.A)) {
				//垂直
				return(this.C);
			}
			else if (this.A == 0.0) {
				//水平
				return(double.NaN);
			}
			else {
				return((y - this.B)/this.A);
			}
		}
		//---
		// Y = A1*X + B1
		// と
		// Y = A2*X + B2
		// 交点を求める
		// A1*X + B1 = A2*X + B2
		// A1*X - A2*X = B2 - B1
		// X(A1-A2) = B2 - B1
		// X = (B2 - B1) / (A1 - A2)
		public PointF GetCrossPt(FN1D fn)
		{
			PointF pt = new PointF();
			if (this.A == fn.A) {
				pt.X = pt.Y = float.NaN;
			}
			else if (this.A == fn.A) {
				//両直線は平行
				if (double.IsNaN(this.A)) {
					G.mlog("here");
					this.A = this.A;
				}
				pt.X = pt.Y = float.NaN;
			}
			else if (double.IsNaN(this.A)) {
				//当該直線は垂直
				if (double.IsNaN(fn.A)) {
					//対象直線は垂直
					pt.X = pt.Y = float.NaN;
				}
				else if (fn.A == 0.0) {
					//対象直線は水平
					pt.X = (float)this.C;
					pt.Y = (float)fn.B;
				}
				else {
					pt.X = (float)this.C;
					pt.Y = (float)(fn.A * pt.X + fn.B);
				}
			}
			else if (this.A == 0.0) {
				//当該直線は水平
				if (double.IsNaN(fn.A)) {
					//対象直線は垂直
					pt.X = (float)fn.C;
					pt.Y = (float)this.B;
				}
				else if (fn.A == 0.0) {
					//対象直線は水平
					pt.X = pt.Y = float.NaN;
				}
				else {
					pt.Y = (float)this.B;
					pt.X = (float)((pt.Y - fn.B)/fn.A);
				}
			}
			else {
				pt.X = (float)((fn.B - this.B) / (this.A - fn.A));
				pt.Y = (float)(this.A * pt.X + this.B);
			}
			return (pt);
		}
		//
		//ptを通る直交する直線
		// A*A' = -1
		//
		public FN1D GetNormFn(PointF pt)
		{
			if (this.A == 0.0) {
				//当該直線は水平
				FN1D fn = new FN1D();
				fn.A = double.NaN;
				fn.B = double.NaN;
				fn.C = pt.X;
				return(fn);
			}
			else if (double.IsNaN(this.A)) {
				//当該直線は垂直
				FN1D fn = new FN1D();
				fn.A = 0;
				fn.B = pt.Y;
				fn.C = double.NaN;
				return(fn);
			}
			else {
//				double A = this.A * (-1);
				double A = (-1)/this.A;
				double B = pt.Y - A * pt.X;
				return(new FN1D(A, B));
			}
		}
		//
		//直線上の点p1から点p2方向へ、p1からdsだけ線上を進めた点
		//
		public PointF GetScanPt1Ext(PointF p1, PointF p2, double ds)
		{
			// 斜辺R, X, Y, R^2 = X^2 + Y^2
			// A = DY/DX =>  Y=A*X
			// R^2 = X^2 + (A*X)^2
			// R^2 = (1+A^2)*X^2
			// X^2 = R^2/(1+A^2)
			double sx, sy;
			if (double.IsNaN(this.A)) {
				sx = 0;
				sy = Math.Abs(ds);
			}
			else {
				sx = Math.Sqrt((ds*ds)/(1+this.A*this.A));
				sy = Math.Sqrt((ds*ds)-(sx*sx));
			}
			sx *= (p2.X > p1.X) ? +1: -1;
			sy *= (p2.Y > p1.Y) ? +1: -1;
			if (ds < 0) {
			sx *= -1;
			sy *= -1;
			}
			p1.X += (float)sx;
			p1.Y += (float)sy;
			return(p1);
		}
		//
		//直線上の点p1から点p2方向へ、p2からdsだけ線上を進めた点
		//
		public PointF GetScanPt2Ext(PointF p1, PointF p2, double ds)
		{
			// 斜辺R, X, Y, R^2 = X^2 + Y^2
			// A = DY/DX =>  Y=A*X
			// R^2 = X^2 + (A*X)^2
			// R^2 = (1+A^2)*X^2
			// X^2 = R^2/(1+A^2)
			double sx, sy;
			if (double.IsNaN(this.A)) {
				sx = 0;
				sy = Math.Abs(ds);
			}
			else {
				sx = Math.Sqrt((ds*ds)/(1+this.A*this.A));
				sy = Math.Sqrt((ds*ds)-(sx*sx));
			}
			sx *= (p2.X > p1.X) ? +1: -1;
			sy *= (p2.Y > p1.Y) ? +1: -1;
			if (ds < 0) {
			sx *= -1;
			sy *= -1;
			}
			p2.X += (float)sx;
			p2.Y += (float)sy;
			return(p2);
		}
		//
		//直線上の点p1から点p2方向へ、直線上のp3からdsだけ線上を進めた点
		//
		public PointF GetScanPt3Ext(PointF p1, PointF p2, PointF p3, double ds)
		{
			// 斜辺R, X, Y, R^2 = X^2 + Y^2
			// A = DY/DX =>  Y=A*X
			// R^2 = X^2 + (A*X)^2
			// R^2 = (1+A^2)*X^2
			// X^2 = R^2/(1+A^2)
			double sx, sy;
			if (double.IsNaN(this.A)) {
				sx = 0;
				sy = Math.Abs(ds);
			}
			else {
				sx = Math.Sqrt((ds*ds)/(1+this.A*this.A));
				sy = Math.Sqrt((ds*ds)-(sx*sx));
			}
			sx *= (p2.X > p1.X) ? +1: -1;
			sy *= (p2.Y > p1.Y) ? +1: -1;
			if (ds < 0) {
			sx *= -1;
			sy *= -1;
			}
			p3.X += (float)sx;
			p3.Y += (float)sy;
			return(p3);
		}
		public bool IsPointInP1P2(PointF pt)
		{
			if (float.IsNaN(this.P1.X)) {
				return(false);
			}
			if (pt.X < this.P1.X || pt.X > this.P2.X) {
				return(false);
			}
			if (pt.Y < this.P1.Y || pt.Y > this.P2.Y) {
				return(false);
			}
			return(true);
		}
	}
}
