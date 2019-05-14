using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//---
using System.Drawing;
using System.Collections;

namespace uSCOPE
{
	class FCS_STK
	{
		/****************************************************************************/
		static private bool make_focus_stack3(List<double[]> ar_con, List<Bitmap> ar_bmp_dep, out Bitmap bmp_dep)
		{
			bool ret = false;

			bmp_dep = null;

			try {
				//string pat;
				//string tmp = System.IO.Path.GetFileName(file);
				//string[] ZXXXD;

				//pat = tmp.Replace("ZP00D", "Z???D");
				//ZXXXD = System.IO.Directory.GetFiles(path, pat);
				bmp_dep = (Bitmap)((Bitmap)ar_bmp_dep[0]).Clone();

				int wid = bmp_dep.Width / G.SS.MOZ_FST_CCNT;
				int hei = bmp_dep.Height/ G.SS.MOZ_FST_RCNT;
				int k = 0;
				Graphics gr = Graphics.FromImage(bmp_dep);

				for (int r = 0; r < G.SS.MOZ_FST_RCNT; r++) {
					for (int c = 0; c < G.SS.MOZ_FST_CCNT; c++, k++) {
						double fmax = -1;
						int imax = 0;
						//最大コントラストの画像を検索
						for (int j = 0; j < ar_bmp_dep.Count; j++) {
							double[] fcnt = (double[])ar_con[j];
							if (fmax < fcnt[k]) {
								fmax = fcnt[k];
								imax = j;
							}
						}
						int x = c*wid;
						int y = r*hei;
						int w = (c == (G.SS.MOZ_FST_CCNT-1) ? (bmp_dep.Width -x): wid);
						int h = (r == (G.SS.MOZ_FST_RCNT-1) ? (bmp_dep.Height-y): hei);							
							
						Bitmap bmp = (Bitmap)ar_bmp_dep[imax];
						Rectangle srt = new Rectangle(x, y, w, h);
						gr.DrawImage(bmp, x, y, srt, GraphicsUnit.Pixel);
					}
				}
				gr.Dispose();
				ret = true;
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}
			return(ret);
		}
		/*
		//---
		static private string[] fst_to_ir_file(string path, string[] cl_files)
		{
			ArrayList ar = new ArrayList();
			for (int i = 0; i < cl_files.Length; i++) {
				string tmp = System.IO.Path.GetFileName(cl_files[i]);
				tmp = tmp.Replace("CT_", "IR_");
				tmp = tmp.Replace("CR_", "IR_");
				ar.Add(path + "\\" + tmp);
			}
			return((string[])ar.ToArray(typeof(string)));
		}*/
		//---
		static private bool fst_calc_contrast(string[] ZXXXD, List<double[]> ar_con, List<Bitmap> ar_bmp_dep)
		{
			bool ret = false;
			try {
				if (ar_con != null) {
					ar_con.Clear();
				}
				if (ar_bmp_dep != null) {
					ar_bmp_dep.Clear();
				}

				for (int j = 0; j < ZXXXD.Length; j++) {
					Bitmap bmp = new Bitmap(ZXXXD[j]);
					if (true) {
						ar_bmp_dep.Add(bmp);
					}
					if (ar_con != null) {
						double[] fctr;
						fctr = Form02.DO_PROC_FOCUS(bmp, G.SS.MOZ_FST_FCOF, G.SS.MOZ_FST_RCNT, G.SS.MOZ_FST_CCNT);
						ar_con.Add(fctr);
					}
#if true//2018.10.10(毛髪径算出・改造)
					else {
						Form02.DO_PROC_FOCUS(bmp, 0, 0, 0);
					}
#endif
				}
				ret = true;
			}
			catch (Exception ex) {
				G.mlog(ex.ToString());
			}
			return(ret);
		}
		//---
		// 深度合成処理
		//---
		static public bool fst_make(string path_fold, out string m_fold_of_dept)
		{
#if true//2019.05.08(再測定・深度合成)
			m_fold_of_dept = null;
#endif
			try {
				string path = path_fold;
				string[] CL_ZP00D = null/*, IR_ZP00D = null*/;
				string pat;
				Bitmap bmp_dep;
				string path_dep;
#if true//2018.10.10(毛髪径算出・改造)
				bool bInit = false;
#endif
				m_fold_of_dept  = string.Format("\\{0}x{1}", G.SS.MOZ_FST_RCNT, G.SS.MOZ_FST_CCNT);
				switch (G.SS.MOZ_FST_MODE) {
				case   /*CL*/  0:m_fold_of_dept += "_CL"; break;
				case   /*IR*/  1:m_fold_of_dept += "_IR"; break;
				default/*CL,IR*/:m_fold_of_dept += "_CL_IR"; break;
				}
				path_dep = path + m_fold_of_dept;
				System.IO.Directory.CreateDirectory(path_dep);
#if false//2019.05.08(再測定・深度合成)
#if true//2018.07.02
				if (G.SS.MOZ_FST_CK01) {
					//既に合成済みの場合は合成処理をスキップする
					string[] CL_ZDEPT = null;
#if true//2019.03.16(NODATA対応)
					CL_ZDEPT = System.IO.Directory.GetFiles(path_dep, "*C?_??_ZDEPT.*");
#endif
					if (CL_ZDEPT.Length > 0) {
						return(true);
					}
				}
#endif
#endif
				for (int q = 0; q < 24; q++) {
					//---
					pat = string.Format("{0}C?_??_ZP00D.*", q);//カラー
					CL_ZP00D = System.IO.Directory.GetFiles(path, pat);
					//---
					if (!bInit && CL_ZP00D.Length > 0) {
						//opencvのセットアップのため呼び出し
						Bitmap bmp = new Bitmap(CL_ZP00D[0]);
						G.CAM_PRC = G.CAM_STS.STS_NONE;
						G.FORM02.load_file(bmp, false);
						bInit = true;
					}

					for (int i = 0; i < CL_ZP00D.Length; i++) {
						List<double[]> ar_cl_con = new List<double[]>();
						List<Bitmap> ar_cl_bmp_dep = new List<Bitmap>();

						string tmp;
						string[] CL_ZXXD, IR_ZXXD;
						if (true) {
							tmp = System.IO.Path.GetFileName(CL_ZP00D[i]);
							CL_ZXXD = System.IO.Directory.GetFiles(path, tmp.Replace("ZP00D", "Z???D"));
						}
#if true//2019.05.08(再測定・深度合成)
						if (G.SS.MOZ_FST_CK01) {
							//既に合成済みの場合は合成処理をスキップする(各ファイル単位で行う)
							string name = tmp.Replace("ZP00D", "ZDEPT");
							if (System.IO.File.Exists(path_dep + "\\" + name)) {
								continue;
							}
						}
#endif
						switch (G.SS.MOZ_FST_MODE) {
						case /*CL*/0:
							if (!fst_calc_contrast(CL_ZXXD, ar_cl_con, ar_cl_bmp_dep)) {
								return(false);
							}
						break;
						default/*CL,IR*/:
							if (!fst_calc_contrast(CL_ZXXD, ar_cl_con, ar_cl_bmp_dep)) {
								return(false);
							}
#if false//2019.04.01(表面赤外省略)
#endif
						break;
						}

						if (true) {
							string name = System.IO.Path.GetFileName(CL_ZP00D[i]);// name: xCx_xx_ZP00D.xxx

							if (!make_focus_stack3(ar_cl_con, ar_cl_bmp_dep, out bmp_dep)) {
								return(false);
							}
							name = name.Replace("ZP00D", "ZDEPT");
							bmp_dep.Save(path_dep + "\\" + name);
							bmp_dep.Dispose();
							bmp_dep = null;
						}
#if false//2019.04.01(表面赤外省略)
#endif
						for (int j = 0; j < ar_cl_bmp_dep.Count; j++) {
							Bitmap bmp;
							bmp = (Bitmap)ar_cl_bmp_dep[j];
							bmp.Dispose();
						}
					}
				}
			}
			catch (Exception ex) {
			}
			return(true);
		}
	
	
	
	
	
	
	
	
	}
}
