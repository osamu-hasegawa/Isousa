using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//---
using System.Drawing;
namespace uSCOPE
{
	class FNLAGRAN
	{
		private PointF P1;
		private PointF P2;
		private PointF P3;
		private PointF P4;
		public bool valid;

		FNLAGRAN()
		{
			this.P1.X = this.P1.Y = float.NaN;
			this.P2.X = this.P2.Y = float.NaN;
			this.P3.X = this.P3.Y = float.NaN;
			this.P4.X = this.P4.Y = float.NaN;
			this.valid = false;
		}
		public FNLAGRAN(PointF p1, PointF p2, PointF p3, PointF p4)
		{
			this.P1 = p1;
			this.P2 = p2;
			this.P3 = p3;
			this.P4 = p4;
			this.valid = true;
		}
		public FNLAGRAN(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
		{
			this.P1 = new PointF((float)x1, (float)y1);
			this.P2 = new PointF((float)x2, (float)y2);
			this.P3 = new PointF((float)x3, (float)y3);
			this.P4 = new PointF((float)x4, (float)y4);
			this.valid = true;
		}
		/****************************************************************************/
		/* ラグランジュ補間
		/* x1, x2 ... x ... x3, x4
		/* y1, y2 ... y ... y3, y4
		/****************************************************************************/
		public double GetYatX(double x)
		{
			double	Y1, Y2, Y3, Y4, YY;

		//	if (x <= x3) {
				Y1 = P1.Y * (x-P2.X)*(x-P3.X)*(x-P4.X) / ((P1.X-P2.X)*(P1.X-P3.X)*(P1.X-P4.X));
				Y2 = P2.Y * (x-P1.X)*(x-P3.X)*(x-P4.X) / ((P2.X-P1.X)*(P2.X-P3.X)*(P2.X-P4.X));
				Y3 = P3.Y * (x-P1.X)*(x-P2.X)*(x-P4.X) / ((P3.X-P1.X)*(P3.X-P2.X)*(P3.X-P4.X));
				Y4 = P4.Y * (x-P1.X)*(x-P2.X)*(x-P3.X) / ((P4.X-P1.X)*(P4.X-P2.X)*(P4.X-P3.X));
				YY = Y1 + Y2 + Y3 + Y4;
		//	}
			return(YY);
		}
	}
}
