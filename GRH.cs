using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//---
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace uSCOPE
{

	class GRH
	{
		struct RECT {
			public 
			int	Top, Bottom, Left, Right;
			public RECT(Rectangle rt)
			{
				this.Top = rt.Top;
				this.Bottom = rt.Bottom;
				this.Left = rt.Left;
				this.Right = rt.Right;
			}
			public void Offset(int dx, int dy)
			{
				this.Left   += dx;
				this.Right  += dx;
				this.Top    += dy;
				this.Bottom += dy;
			}
			public int Width()
			{
				return(this.Right-this.Left);
			}
			public int Height()
			{
				return(this.Bottom-this.Top);
			}
		}
#if true
		void DUMMY() {
		}
#else
		struct GDEF {
			public RECT	rt_cl;		//クライアント領域全体
			public RECT	rt_gr;		//グラフエリア
			public int		rt_wid;		//グラフエリア幅
			public int		rt_hei;		//グラフエリア高
			//---
			public int		xmin;
			public int		xmax;
			public int		xtic;
			public int		ymin;
			public int		ymax;
			public int		ytic;
			//---
			public int		bitLBL;		//1:X軸ラベル, 2:Y軸ラベル
			public int		bGRID_X;
			public int		bGRID_Y;
			public int		bTIC_X;
			public int		bTIC_Y;
			public int		bNUM_X;
			public int		bNUM_Y;
			//---
			public int		xwid;
			public int		yhei;
			//---
		};
		/****************************************************************************/
		int GetPeriod(double f) 
		{
			string tmp;
			int		h, i, l;

			tmp = string.Format("{0:F6}", f);
			l = tmp.Length;
			for (i = l - 1; tmp[i] != '.' && tmp[i] == '0'; i--) {
			}
			if (tmp[i] == '.') {
				return(0);		/* 少数点以下なし */
			}

			h = i;				/* 小数点以下の有効桁数検索 */
			for (; tmp[i] != '.'; i--) {
			}
			l = (h-i);
			return(l > 5 ? 5: l);
		}
		//***********************************************************
		double	m_d;
		double	m_dpgm;
		double	m_gmax;
		double	m_gmin;
		double	m_gtic;
		double	m_dots;
		int		m_nums;
		int		m_bs10;
		string	m_fmt;
		//***********************************************************
		void EnumGridFirst(double gmax, double gmin, double gtic, int dots, ref string fmt, bool bALIGN)
		{
			int	tmp;
			m_gmax = gmax;
			m_gmin = gmin;
			m_gtic = gtic;
			m_dpgm = dots / (gmax-gmin);
			if (bALIGN) {
				m_d = gtic * (double)((int)(gmin/gtic));
				if (m_d < gmin) {
					m_d += gtic;
				}
			}
			else {
				m_d = gmin;
			}
			tmp = GetPeriod(gtic);
			fmt = "{0:F" + tmp.ToString() + "}";

			m_fmt = fmt;
		}
		//***********************************************************
		bool EnumGridNext(ref int pd, ref string buf, ref bool pbZERO, double ep, ref bool pbMAXMIN)
		{
			int		tmp;

			if (m_d > (m_gmax+0.001) || m_gtic <= 0.0) {
				return(false);
			}

			pbMAXMIN = false;

			while (true) {
				if (m_d <= 1E-15 && m_d >= -1E-15) {
					m_d = 0;
				}
				buf = string.Format(m_fmt, m_d);

				tmp = (int)(m_dpgm*(m_d-m_gmin));

				if (m_d >= m_gmax) {
					pbMAXMIN = true;
				}
				if (m_d <= m_gmin) {
					pbMAXMIN = true;
				}
				if (m_d <= ep && m_d >= -ep) {
					pbZERO = true;
				}
				else {
					pbZERO = false;
				}
				break;
			}
			pd = tmp;
			if (m_d < m_gmax) {
				if ((m_d += m_gtic) > m_gmax) {
#if false//2015.06.18
					m_d = m_gmax;
#endif
				}
			}
			else {
				m_d += m_gtic;
			}
			return(true);
		}
		void GDEF_INIT(GDEF pdef, int bitLBL, Control pWnd, ref Rectangle poff)
		{
			RECT rt = new RECT(pWnd.Bounds);

			//pWnd.GetWindowRect(&rt);

			rt.Offset(-rt.Left, -rt.Top);//(0,0)基準にする
			pdef.rt_cl = rt;
			//---
			rt.Top    += 5;
			rt.Bottom -= 7;
			rt.Left   += 5;
			rt.Right  -= 5;	
			if ((bitLBL & 1) != 0) {
				//X軸にラベル表示
				rt.Bottom -= 8;
				rt.Right  -= 7;
			}
			if ((bitLBL & 2) != 0) {
				//Y軸にラベル表示
				rt.Left += 16;//+7:符号があるとき
				rt.Top  += 1;
			}
			if (poff != null) {
				rt.Top    += poff.Top;
				rt.Bottom += poff.Bottom;
				rt.Left   += poff.Left;
				rt.Right  += poff.Right;
			}
			//---
			pdef.rt_gr = rt;
			//---
			pdef.rt_wid = rt.Width();
			pdef.rt_hei = rt.Height();
			//---
			pdef.bitLBL = bitLBL;
		}
		void GDEF_PSET(GDEF pdef, int xmin, int xmax, int xtic, int ymin, int ymax, int ytic)
		{
			pdef.xmin = xmin;
			pdef.xmax = xmax;
			pdef.ymin = ymin;
			pdef.ymax = ymax;
			pdef.xtic = xtic;
			pdef.ytic = ytic;
			//---
			pdef.xwid = xmax-xmin;
			pdef.yhei = ymax-ymin;
		}
		int GDEF_XPOS(GDEF pdef, double f)
		{
			int	x;
			x = (int)(pdef.rt_gr.Left + pdef.rt_wid * (f-pdef.xmin)/(pdef.xwid));
			return(x);
		}
		int GDEF_YPOS(GDEF pdef, double f)
		{
			int	y;
			y = (int)(pdef.rt_gr.Bottom - pdef.rt_hei * (f-pdef.ymin)/(pdef.yhei));
			return(y);
		}
		void Draw4Edge(Graphics pDC, Pen pen, RECT rt)
		{
			const
			int	GAP = 0;
			Point[]	pts = {
				new Point(rt.Left     , rt.Top),
				new Point(rt.Right+GAP, rt.Top ),
				new Point(rt.Right+GAP, rt.Bottom+GAP),
				new Point(rt.Left     , rt.Bottom+GAP),
				new Point(rt.Left     , rt.Top-GAP)
			};
			pDC.DrawLines(pen, pts);
		}
#if  false
		void GDEF_GRID(GDEF pdef, Graphics pDC)
		{
			//CPen*	old_pen = (CPen*)pDC.SelectStockObject(BLACK_PEN);
			//CFont*	old_fnt = (CFont*)pDC.SelectObject(GET_FONT(6));
			Font	fnt = new Font(
			RECT	rt;
			string	fmt, buf;
			bool	bZERO;
			bool	bMAXMIN;
		//	double	ep=0.1;
			int		dot, tmp;
			const
			int		SLEN=3;	
		#if true//2015.07.24
			Brush brs = new SolidBrush(Color.FromArgb(224,224,224));
			pDC.FillRectangle(brs, 0, 0, pdef.rt_cl.Right-2, pdef.rt_cl.Bottom-2);//, RGB(224,224,224));
		#else
			pDC.FillSolidRect(&pdef.rt_cl, RGB(224,224,224));
		#endif
			pDC.FillRectangle(Brushes.White, pdef.rt_gr.Left, pdef.rt_gr.Top, pdef.rt_gr.Width(), pdef.rt_gr.Height());
			Draw4Edge(pDC, Pens.Black, pdef.rt_gr);
			//--------------------------------
			rt = pdef.rt_gr;
			rt.Right = rt.Left-SLEN+1;
			rt.Left  = rt.Right-100;
			//Y軸のめもり描画
			EnumGridFirst(pdef.ymax, pdef.ymin, pdef.ytic, pdef.rt_hei, ref fmt, /*bALIGN=*/false);
			while (EnumGridNext(ref dot, ref buf, ref bZERO, 0.1, ref bMAXMIN)) {
				tmp = pdef.rt_gr.Bottom - dot;
				rt.Top    = tmp - 300;
				rt.Bottom = tmp + 300;

		//		pDC.SelectObject(&rpen);//太
				pDC.DrawLine(Pens.Black, pdef.rt_gr.Left     , tmp, pdef.rt_gr.Left-SLEN, tmp);
				if ((pdef.bitLBL & 2) != 0) {
				pDC.DrawString(buf, 
				pDC.DrawText(buf, -1, &rt, DT_SINGLELINE|DT_RIGHT|DT_VCENTER);
				}
				if (bMAXMIN) {
				continue;
				}
				if (false) {
				pDC.DrawLine(Pens.Black, pdef.rt_gr.Left , tmp, pdef.rt_gr.Right, tmp);
				}
			}
			//Ｘ軸のめもり描画
			//--------------------------------
			rt = pdef.rt_gr;
			rt.Top    = rt.Bottom+SLEN;
			rt.Bottom = rt.Top + 100;
			//--------------------------------
			if (true) {
		//		int		d;
			//@@@	bool	bZERO, bMAXMIN;

				EnumGridFirst(pdef.xmax, pdef.xmin, pdef.xtic, pdef.rt_wid, ref fmt, /*bALIGN=*/false);
				while (EnumGridNext(ref dot, ref buf, ref bZERO, 0.1, ref bMAXMIN)) {
					tmp = pdef.rt_gr.Left + dot;
					rt.Left  = tmp - 500;
					rt.Right = tmp + 500;

		//			pDC.SelectObject(&rpen);//太
					pDC.DrawLine(Pens.Black, tmp, pdef.rt_gr.Bottom, tmp, pdef.rt_gr.Bottom+SLEN);
					if ((pdef.bitLBL & 1) != 0) {
					pDC.DrawText(buf, -1, &rt, DT_SINGLELINE|DT_CENTER|DT_TOP);
					}
					if (bMAXMIN) {
					continue;
					}
					//if (!bZERO) {
					//pDC.SelectObject(&gpen);//細
					//}
					if (false) {
					pDC.DrawLine(Pens.Black, tmp, pdef.rt_gr.Top, tmp, pdef.rt_gr.Bottom);
					}
				}
			}
			//pDC.SelectObject(old_fnt);
			//pDC.SelectObject(old_pen);
		}
#endif
#endif
	}
}